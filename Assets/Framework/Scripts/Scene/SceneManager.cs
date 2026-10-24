using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace Scene
{
    public class SceneManager : MonoBehaviour
    {
        private sealed class TransitionRequest
        {
            public object Key;
            public UIManager UI;
            public UnityScene ExistingScene;
            public UnityScene LoadedScene;
            public Func<IEnumerator> Prepare;
            public Func<UnityScene, IEnumerator> EnterScene;
            public Action<bool, string> Completed;
            public Func<bool> Retry;
            public readonly Stack<IEnumerator> Routines = new();
            public Coroutine Runner;
            public bool Executing;
            public bool Settled;
            public bool LoadFinished;
            public string AbortError;
            public string LoadError;
        }

        private readonly Dictionary<string, object> _registeredScenes = new(StringComparer.Ordinal);
        private TransitionRequest _current;
        private TransitionRequest _retry;
        private bool _unavailable;

        public bool IsTransitioning => _current != null;
        public string LastError { get; private set; }

        private void OnEnable()
        {
            _unavailable = false;
        }

        private void OnDisable()
        {
            _unavailable = true;
            var request = _current;
            _retry = null;
            LastError = null;

            if (request == null)
            {
                return;
            }

            request.AbortError = "Scene transition owner was disabled or destroyed.";

            if (request.Executing)
            {
                return;
            }

            if (request.Runner != null)
            {
                StopCoroutine(request.Runner);
            }

            DisposeRoutines(request);
            Finish(request, false, request.AbortError, false);
        }

        public void RegisterScene(string id, object key)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Scene registration requires a non-empty ID.", nameof(id));
            }

            if (key == null || (key is string path && string.IsNullOrWhiteSpace(path)))
            {
                throw new ArgumentException("Scene registration requires a non-empty address.", nameof(key));
            }

            if (_registeredScenes.ContainsKey(id))
            {
                Debug.LogError($"SceneManager.RegisterScene: {id} already registered", this);
                return;
            }

            _registeredScenes.Add(id, key);
        }

        public bool TryTransition(string id, Func<UnityScene, IEnumerator> enterScene = null,
            Action<bool, string> completed = null)
        {
            return TryTransition(id, default, null, enterScene, completed, null);
        }

        public bool TryTransition(string id, UnityScene existingScene,
            Func<IEnumerator> prepare, Func<UnityScene, IEnumerator> enterScene,
            Action<bool, string> completed, Func<bool> retry)
        {
            if (string.IsNullOrWhiteSpace(id) || !_registeredScenes.TryGetValue(id, out var key))
            {
                return RejectTransition($"Scene ID '{id}' is not registered. Register scene configuration before transitioning.", completed);
            }

            return TryTransitionAddress(key, existingScene, prepare, enterScene, completed, retry);
        }

        // Legacy object keys remain supported. Use TryTransitionAddress for raw string addresses.
        public bool TryTransition(object key, Func<UnityScene, IEnumerator> enterScene = null,
            Action<bool, string> completed = null)
        {
            return TryTransitionAddress(key, enterScene, completed);
        }

        public bool TryTransition(object key, UnityScene existingScene,
            Func<IEnumerator> prepare, Func<UnityScene, IEnumerator> enterScene,
            Action<bool, string> completed, Func<bool> retry)
        {
            return TryTransitionAddress(key, existingScene, prepare, enterScene, completed, retry);
        }

        public bool TryTransitionAddress(object key, Func<UnityScene, IEnumerator> enterScene = null,
            Action<bool, string> completed = null)
        {
            return TryTransitionAddress(key, default, null, enterScene, completed, null);
        }

        public bool TryTransitionAddress(object key, UnityScene existingScene,
            Func<IEnumerator> prepare, Func<UnityScene, IEnumerator> enterScene,
            Action<bool, string> completed, Func<bool> retry)
        {
            var ui = GameCore.UI;

            if (IsTransitioning)
            {
                return RejectTransition("Another scene transition is already running.", completed);
            }

            if (_unavailable || !isActiveAndEnabled || GameCore.Scene != this)
            {
                return RejectTransition("Scene transition service is unavailable.", completed);
            }

            if (ui == null || !ui.isActiveAndEnabled || ui.transitionCanvasGroup == null)
            {
                return RejectTransition("Scene transition requires an active UI service with a transition CanvasGroup.", completed);
            }

            if (key == null || (key is string path && string.IsNullOrWhiteSpace(path)))
            {
                return RejectTransition("Scene transition requires a non-empty address.", completed);
            }

            var request = new TransitionRequest
            {
                Key = key,
                UI = ui,
                ExistingScene = existingScene,
                Prepare = prepare,
                EnterScene = enterScene,
                Completed = completed,
                Retry = retry
            };

            _current = request;
            _retry = null;
            LastError = null;
            request.Routines.Push(Transition(request));
            request.Runner = StartCoroutine(RunTransition(request));
            return true;
        }

        public bool RetryTransition()
        {
            var request = _retry;

            if (request == null || IsTransitioning || _unavailable || !isActiveAndEnabled
                || GameCore.Scene != this)
            {
                return false;
            }

            if (request.Retry != null)
            {
                return request.Retry();
            }

            return TryTransitionAddress(request.Key, request.EnterScene, request.Completed);
        }

        private bool RejectTransition(string error, Action<bool, string> completed)
        {
            Debug.LogError($"SceneManager.TryTransition: {error}", this);
            InvokeCompleted(completed, false, error);
            return false;
        }

        private IEnumerator RunTransition(TransitionRequest request)
        {
            try
            {
                while (_current == request && request.Routines.Count > 0)
                {
                    object yielded = null;
                    Exception failure = null;
                    var routine = request.Routines.Peek();
                    var finished = false;

                    try
                    {
                        if (GameCore.Scene != this || GameCore.UI != request.UI || request.UI == null
                            || !request.UI.isActiveAndEnabled)
                        {
                            throw new InvalidOperationException("Scene transition lost its Core or UI owner.");
                        }

                        request.Executing = true;

                        if (routine.MoveNext())
                        {
                            yielded = routine.Current;
                        }
                        else
                        {
                            request.Routines.Pop();
                            (routine as IDisposable)?.Dispose();
                            finished = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                    }
                    finally
                    {
                        request.Executing = false;
                    }

                    if (failure != null || request.AbortError != null)
                    {
                        DisposeRoutines(request);
                        Finish(request, false, request.AbortError ?? failure.Message,
                            request.AbortError == null);
                        yield break;
                    }

                    if (_current != request)
                    {
                        yield break;
                    }

                    if (finished)
                    {
                        continue;
                    }

                    if (yielded is IEnumerator nested)
                    {
                        request.Routines.Push(nested);
                        continue;
                    }

                    yield return yielded;
                }

                Finish(request, true, null);
            }
            finally
            {
                DisposeRoutines(request);
            }
        }

        private IEnumerator Transition(TransitionRequest request)
        {
            yield return Fade(request, true);

            if (request.Prepare != null)
            {
                yield return request.Prepare();
            }

            if (request.ExistingScene.IsValid() && request.ExistingScene.isLoaded)
            {
                request.LoadedScene = request.ExistingScene;
            }
            else
            {
                var handle = request.Key is IResourceLocation location
                    ? LoadSceneAsync(location)
                    : LoadSceneAsync(request.Key);

                // This callback must settle the load even after the owner stops its coroutine.
                handle.Completed += result => RecordSceneResult(request, result);

                while (!request.LoadFinished)
                {
                    yield return null;
                }

                if (request.LoadError != null)
                {
                    throw new InvalidOperationException(request.LoadError);
                }
            }

            if (!request.LoadedScene.IsValid() || !request.LoadedScene.isLoaded)
            {
                throw new InvalidOperationException("Loaded scene is no longer available.");
            }

            request.UI.CloseAll();

            if (request.AbortError != null)
            {
                yield break;
            }

            if (request.EnterScene != null)
            {
                var entry = request.EnterScene(request.LoadedScene);

                if (entry != null)
                {
                    yield return entry;
                }
            }

            if (!request.LoadedScene.IsValid() || !request.LoadedScene.isLoaded)
            {
                throw new InvalidOperationException("Scene was unloaded during initialization.");
            }

            yield return Fade(request, false);
        }

        private static IEnumerator Fade(TransitionRequest request, bool covered)
        {
            var completed = false;
            var tween = request.UI.FadeTransition(covered, () => completed = true);

            while (!completed && tween.IsActive())
            {
                if (request.UI == null || request.UI.transitionCanvasGroup == null)
                {
                    throw new InvalidOperationException("Scene transition overlay was destroyed.");
                }

                yield return null;
            }

            if (!completed)
            {
                throw new InvalidOperationException("Scene transition fade was interrupted.");
            }
        }

        private static void RecordSceneResult(TransitionRequest request,
            AsyncOperationHandle<SceneInstance> handle)
        {
            if (!handle.IsValid())
            {
                request.LoadError = "Scene load returned an invalid handle.";
            }
            else if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                request.LoadError = $"Scene load failed: {handle.OperationException?.Message}";
                Addressables.Release(handle);
            }
            else
            {
                // Successful scenes stay tracked by Addressables until their native unload.
                request.LoadedScene = handle.Result.Scene;
            }

            request.LoadFinished = true;
        }

        private void Finish(TransitionRequest request, bool succeeded, string error, bool allowRetry = true)
        {
            if (_current != request || request.Settled)
            {
                return;
            }

            request.Settled = true;

            try
            {
                if (request.UI != null)
                {
                    request.UI.HideTransition();
                }
            }
            catch (Exception exception)
            {
                succeeded = false;
                error = $"Scene transition cleanup failed: {exception.Message}";
            }

            allowRetry = allowRetry && !_unavailable && GameCore.Scene == this;
            request.Runner = null;

            _current = null;
            LastError = allowRetry ? error : null;
            _retry = !succeeded && allowRetry ? request : null;

            InvokeCompleted(request.Completed, succeeded, error);
        }

        private void InvokeCompleted(Action<bool, string> completed, bool succeeded, string error)
        {
            if (completed == null)
            {
                return;
            }

            foreach (Action<bool, string> callback in completed.GetInvocationList())
            {
                if (callback.Target is UnityEngine.Object target && target == null)
                {
                    continue;
                }

                try
                {
                    callback(succeeded, error);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private static void DisposeRoutines(TransitionRequest request)
        {
            var executing = request.Executing;
            request.Executing = true;

            try
            {
                while (request.Routines.Count > 0)
                {
                    var routine = request.Routines.Pop();

                    try
                    {
                        (routine as IDisposable)?.Dispose();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }
            }
            finally
            {
                request.Executing = executing;
            }
        }

        private void OnGUI()
        {
            if (_retry == null || LastError == null || IsTransitioning)
            {
                return;
            }

            var width = Mathf.Min(620f, Screen.width - 32f);
            var left = (Screen.width - width) * 0.5f;
            var top = Mathf.Max(16f, (Screen.height - 230f) * 0.5f);
            GUI.Box(new Rect(left, top, width, 230f), "Scene transition failed");
            var style = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUI.Label(new Rect(left + 16f, top + 32f, width - 32f, 140f), LastError, style);

            if (GUI.Button(new Rect(left + 16f, top + 184f, width - 32f, 30f), "Retry"))
            {
                RetryTransition();
            }
        }

        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }

        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(IResourceLocation location, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(location, loadMode, activateOnLoad, priority);
        }

        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
        }

        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, autoReleaseHandle);
        }

        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, autoReleaseHandle);
        }
    }
}
