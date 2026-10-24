using Core;
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private enum UIState { Queued, Loading, Open }

        private sealed class UIRequest
        {
            public string Name;
            public UILayer Layer;
            public object Arg;
            public UIBase View;
            public AsyncOperationHandle<GameObject> Handle;
            public List<Action<UIBase>> Callbacks = new();
            public UIState State;
            public bool InLifecycle;
            public bool Cancelled;
        }

        private readonly Dictionary<string, UIData> _registeredUIData = new();
        private readonly Dictionary<string, UIRequest> _requests = new();
        private readonly Queue<UIRequest> _pendingOpen = new();
        private bool _shuttingDown;
        private int _transitionVersion;

        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameObject[] layerObjects;

        [Header("Transition")]
        public CanvasGroup transitionCanvasGroup;
        public float fadeOutTime = 0.1f;
        public float fadeInTime = 0.18f;
        public Ease fadeInType = Ease.OutCubic;
        public Ease fadeOutType = Ease.OutQuad;

        public void Init()
        {
            if (GetGameObjectByUILayers(UILayer.First) == null)
            {
                throw new InvalidOperationException("UIManager.Init: missing First UI layer.");
            }

            if (transitionCanvasGroup == null)
            {
                throw new InvalidOperationException("UIManager.Init: missing transition CanvasGroup.");
            }
        }

        public void RegisterUiData(UIData uiData)
        {
            if (uiData == null)
            {
                throw new ArgumentNullException(nameof(uiData));
            }

            if (string.IsNullOrWhiteSpace(uiData.UIName))
            {
                throw new ArgumentException("UI registration requires a non-empty ID.", nameof(uiData));
            }

            if (_registeredUIData.ContainsKey(uiData.UIName))
            {
                Debug.LogError($"UIManager.RegisterUiData: {uiData.UIName} already registered");
                return;
            }

            _registeredUIData.Add(uiData.UIName, uiData);
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            // Unity drives UIBase.Update. New requests made by callbacks wait for the next pump.
            if (_shuttingDown)
            {
                return;
            }

            var pending = _pendingOpen.ToArray();
            _pendingOpen.Clear();

            // OnDestroy may be skipped for a never-active object or a derived hook without base.
            List<UIRequest> destroyed = null;

            foreach (var request in _requests.Values)
            {
                if (request.State == UIState.Open && request.View == null)
                {
                    destroyed ??= new List<UIRequest>();
                    destroyed.Add(request);
                }
            }

            if (destroyed != null)
            {
                foreach (var request in destroyed)
                {
                    CloseRequest(request);
                }
            }

            foreach (var request in pending)
            {
                if (IsCurrent(request))
                {
                    LoadUI(request);
                }
            }
        }

        public void OpenUI(UILayer layer, string uiName, object arg = null, Action<UIBase> callback = null)
        {
            if (_shuttingDown || string.IsNullOrWhiteSpace(uiName) || !_registeredUIData.ContainsKey(uiName))
            {
                InvokeCallback(callback, null);
                return;
            }

            if (_requests.TryGetValue(uiName, out var current))
            {
                if (current.State != UIState.Open)
                {
                    if (callback != null)
                    {
                        current.Callbacks.Add(callback);
                    }

                    return;
                }

                if (current.View != null)
                {
                    InvokeCallback(callback, current.View);
                    return;
                }

                // Recover if a derived view destroyed itself without calling base.OnDestroy.
                CloseRequest(current);
                OpenUI(layer, uiName, arg, callback);
                return;
            }

            if (string.IsNullOrWhiteSpace(_registeredUIData[uiName].ResourcePath))
            {
                Debug.LogError($"UIManager.OpenUI: {uiName} has no prefab assigned. Assign a prefab and update configuration.");
                InvokeCallback(callback, null);
                return;
            }

            if (GetGameObjectByUILayers(layer) == null)
            {
                Debug.LogError($"UIManager.OpenUI: invalid layer {layer} for {uiName}");
                InvokeCallback(callback, null);
                return;
            }

            var request = new UIRequest { Name = uiName, Layer = layer, Arg = arg };

            if (callback != null)
            {
                request.Callbacks.Add(callback);
            }

            _requests.Add(uiName, request);
            _pendingOpen.Enqueue(request);
        }

        private void LoadUI(UIRequest request)
        {
            var resourcePath = _registeredUIData[request.Name].ResourcePath;

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                Debug.LogError($"UIManager.OpenUI: {request.Name} has no prefab assigned. Assign a prefab and update configuration.");
                CloseRequest(request);
                return;
            }

            var layer = GetGameObjectByUILayers(request.Layer);

            if (layer == null)
            {
                Debug.LogError($"UIManager.OpenUI: missing layer for {request.Name}");
                CloseRequest(request);
                return;
            }

            request.State = UIState.Loading;

            try
            {
                GameCore.Resource.InstantiateGameObjectAsync(
                    resourcePath, layer.transform, false,
                    handle => CompleteLoad(request, handle));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                CloseRequest(request);
            }
        }

        private void CompleteLoad(UIRequest request, AsyncOperationHandle<GameObject> handle)
        {
            request.Handle = handle;

            if (!IsCurrent(request))
            {
                Cleanup(request);
                return;
            }

            if (!handle.IsValid() || handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"UIManager.OpenUI: failed to instantiate {request.Name}");
                CloseRequest(request);
                return;
            }

            var view = handle.Result.GetComponent<UIBase>();

            if (view == null)
            {
                Debug.LogError($"UIManager.OpenUI: {request.Name} prefab has no UIBase");
                CloseRequest(request);
                return;
            }

            request.View = view;
            view.SetCloseCallback(() => CloseRequest(request));
            request.InLifecycle = true;

            try
            {
                view.Init(request.Name, request.Arg);

                if (IsCurrent(request))
                {
                    view.Show();
                }

                if (IsCurrent(request))
                {
                    request.State = UIState.Open;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                CloseRequest(request);
            }
            finally
            {
                request.InLifecycle = false;

                if (!IsCurrent(request))
                {
                    Cleanup(request);
                }
            }

            CompleteCallbacks(request);
        }

        private bool IsCurrent(UIRequest request)
        {
            return !_shuttingDown && !request.Cancelled
                && _requests.TryGetValue(request.Name, out var current)
                && ReferenceEquals(current, request);
        }

        private GameObject GetGameObjectByUILayers(UILayer uiLayer)
        {
            var index = (int)uiLayer;
            return layerObjects != null && index >= 0 && index < layerObjects.Length
                ? layerObjects[index] : null;
        }

        public bool CloseUI(string uiName)
        {
            if (string.IsNullOrWhiteSpace(uiName) || !_requests.TryGetValue(uiName, out var request))
            {
                return false;
            }

            CloseRequest(request);
            return true;
        }

        private void CloseRequest(UIRequest request)
        {
            if (_requests.TryGetValue(request.Name, out var current) && ReferenceEquals(current, request))
            {
                _requests.Remove(request.Name);
            }

            request.Cancelled = true;

            // A hook can request close, then continue registering listeners. Close only after it returns.
            if (request.InLifecycle)
            {
                return;
            }

            Cleanup(request);
            CompleteCallbacks(request);
        }

        private void Cleanup(UIRequest request)
        {
            var view = request.View;
            var handle = request.Handle;

            request.View = null;
            request.Handle = default;
            request.Arg = null;
            request.InLifecycle = true;

            try
            {
                if (!ReferenceEquals(view, null))
                {
                    view.ReleaseFromManager();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                request.InLifecycle = false;

                if (handle.IsValid())
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Addressables.ReleaseInstance(handle);
                    }
                    else
                    {
                        Addressables.Release(handle);
                    }
                }
            }
        }

        private void CompleteCallbacks(UIRequest request)
        {
            var callbacks = request.Callbacks;
            request.Callbacks = null;

            if (callbacks == null)
            {
                return;
            }

            foreach (var callback in callbacks)
            {
                // An earlier callback may have closed this instance and opened a new request.
                var view = IsCurrent(request) && request.State == UIState.Open ? request.View : null;
                InvokeCallback(callback, view);
            }
        }

        private void InvokeCallback(Action<UIBase> callback, UIBase view)
        {
            if (callback == null || (callback.Target is UnityEngine.Object target && target == null))
            {
                return;
            }

            try
            {
                callback(view);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        public void CloseAll()
        {
            var requests = _requests.Values.ToArray();
            _requests.Clear();
            _pendingOpen.Clear();

            foreach (var request in requests)
            {
                request.Cancelled = true;
            }

            foreach (var request in requests)
            {
                CloseRequest(request);
            }
        }

        private void OnDestroy()
        {
            _shuttingDown = true;
            HideTransition();

            CloseAll();
        }

        //-----------------------------------------------------------------------------------------------
        // Transition
        //-----------------------------------------------------------------------------------------------

        public void OpenViewWithTransition(string newUIName, string curUIName,
            UILayer layer, object data = null, Action blackCallback = null)
        {
            var version = _transitionVersion + 1;
            TransitionFadeOut(() =>
            {
                try
                {
                    blackCallback?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);

                    if (_transitionVersion == version)
                    {
                        TransitionFadeIn();
                    }

                    return;
                }

                if (_transitionVersion != version)
                {
                    return;
                }

                OpenUI(layer, newUIName, data, uiBase =>
                {
                    if (_transitionVersion != version)
                    {
                        return;
                    }

                    if (uiBase != null && newUIName != curUIName && IsOpen(curUIName))
                    {
                        CloseUI(curUIName);
                    }

                    if (_transitionVersion == version)
                    {
                        TransitionFadeIn();
                    }
                });
            });
        }

        public bool IsOpen(string uiName)
        {
            return !string.IsNullOrWhiteSpace(uiName) && _requests.TryGetValue(uiName, out var request) && !request.Cancelled
                && (request.State != UIState.Open || request.View != null);
        }

        public void TransitionFadeOut(Action completeCallback = null)
        {
            FadeTransition(true, completeCallback);
        }

        public void TransitionFadeIn(Action completeCallback = null)
        {
            FadeTransition(false, completeCallback);
        }

        internal Tween FadeTransition(bool covered, Action completeCallback = null)
        {
            if (_shuttingDown || transitionCanvasGroup == null)
            {
                throw new InvalidOperationException("UIManager: transition CanvasGroup is unavailable.");
            }

            var duration = covered ? fadeOutTime : fadeInTime;

            if (float.IsNaN(duration) || float.IsInfinity(duration))
            {
                throw new InvalidOperationException("UIManager: transition duration must be finite.");
            }

            var group = transitionCanvasGroup;
            var version = ++_transitionVersion;
            group.DOKill(false);
            group.blocksRaycasts = true;
            group.gameObject.SetActive(true);

            return group.DOFade(covered ? 1f : 0f, Mathf.Max(0f, duration))
                .SetEase(covered ? fadeOutType : fadeInType)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (group == null || _transitionVersion != version)
                    {
                        return;
                    }

                    if (!covered)
                    {
                        group.alpha = 0f;
                        group.gameObject.SetActive(false);
                    }

                    try
                    {
                        completeCallback?.Invoke();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, this);
                    }
                });
        }

        public void ShowTransition()
        {
            if (_shuttingDown || transitionCanvasGroup == null)
            {
                throw new InvalidOperationException("UIManager.ShowTransition: missing transition CanvasGroup.");
            }

            ++_transitionVersion;
            transitionCanvasGroup.DOKill(false);
            transitionCanvasGroup.blocksRaycasts = true;
            transitionCanvasGroup.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 1f;
        }

        public void HideTransition()
        {
            ++_transitionVersion;

            if (transitionCanvasGroup == null)
            {
                return;
            }

            transitionCanvasGroup.DOKill(false);
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.gameObject.SetActive(false);
        }
    }
}
