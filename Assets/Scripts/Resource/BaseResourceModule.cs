using Core;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Resource
{
    public enum ResourceType
    {
        Sprite,
        AudioClip,
        GameObject,
    }

    public struct ResourceInfo
    {
        public int ResNameId;
        public string ResPath;
        public ResourceType ResourceType;
    }

    public abstract class BaseResourceModule
    {
        private sealed class LoadRequest
        {
            public readonly Dictionary<int, object> Resources = new();
            public readonly List<AsyncOperationHandle> Handles = new();
            public List<(Action Loaded, Action Failed)> Callbacks = new();
            public int Remaining;
            public bool Ready;
        }

        private readonly List<ResourceInfo> _resourceInfos = new();
        private LoadRequest _request;
        private bool _recordedResourceInfos;
        private bool _shuttingDown;

        public abstract ResourceModuleName GetName();

        protected T GetResource<T>(int id)
        {
            return _request != null && _request.Ready
                && _request.Resources.TryGetValue(id, out var resource) && resource is T value
                ? value : default;
        }

        protected abstract void RecordAllResourceInfos();

        protected void AddResourceInfo(ResourceType type, int resId, string path)
        {
            _resourceInfos.Add(new ResourceInfo { ResNameId = resId, ResPath = path, ResourceType = type });
        }

        public void LoadResources(Action loadedCallback = null, Action failedCallback = null)
        {
            if (_shuttingDown)
            {
                InvokeCallback(failedCallback);
                return;
            }

            if (_request != null)
            {
                if (_request.Ready)
                    InvokeCallback(loadedCallback);
                else if (loadedCallback != null || failedCallback != null)
                    _request.Callbacks.Add((loadedCallback, failedCallback));
                return;
            }

            var request = new LoadRequest();
            _request = request;
            if (loadedCallback != null || failedCallback != null)
                request.Callbacks.Add((loadedCallback, failedCallback));

            try
            {
                if (!_recordedResourceInfos)
                {
                    _resourceInfos.Clear();
                    RecordAllResourceInfos();
                    var ids = new HashSet<int>();
                    foreach (var info in _resourceInfos)
                    {
                        if (!ids.Add(info.ResNameId) || string.IsNullOrWhiteSpace(info.ResPath)
                            || !Enum.IsDefined(typeof(ResourceType), info.ResourceType))
                            throw new InvalidOperationException(
                                $"Resource module {GetName()}: invalid definition {info.ResNameId}, {info.ResPath}");
                    }
                    _recordedResourceInfos = true;
                }

                if (!IsCurrent(request))
                    return;

                request.Remaining = _resourceInfos.Count;
                if (request.Remaining == 0)
                {
                    request.Ready = true;
                    CompleteCallbacks(request);
                    return;
                }

                foreach (var info in _resourceInfos)
                {
                    if (!IsCurrent(request))
                        break;

                    switch (info.ResourceType)
                    {
                        case ResourceType.Sprite:
                            LoadResource<Sprite>(request, info);
                            break;
                        case ResourceType.AudioClip:
                            LoadResource<AudioClip>(request, info);
                            break;
                        case ResourceType.GameObject:
                            LoadResource<GameObject>(request, info);
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                CancelRequest(request);
            }
        }

        private void LoadResource<T>(LoadRequest request, ResourceInfo info) where T : UnityEngine.Object
        {
            var handle = GameCore.Resource.LoadAssetAsync<T>(info.ResPath);
            var index = request.Handles.Count;
            request.Handles.Add(handle);
            if (!handle.IsValid())
                throw new InvalidOperationException($"Resource module {GetName()}: invalid handle for {info.ResPath}");

            handle.Completed += completed =>
            {
                if (!IsCurrent(request))
                {
                    ReleaseHandle(request, index);
                    return;
                }

                if (!completed.IsValid() || completed.Status != AsyncOperationStatus.Succeeded || completed.Result == null)
                {
                    Debug.LogError($"Resource module {GetName()}: failed to load {info.ResNameId}, {info.ResPath}. "
                        + (completed.IsValid() ? completed.OperationException?.Message : "Invalid handle"));
                    CancelRequest(request);
                    return;
                }

                request.Resources.Add(info.ResNameId, completed.Result);
                if (--request.Remaining == 0)
                {
                    request.Ready = true;
                    CompleteCallbacks(request);
                }
            };
        }

        private bool IsCurrent(LoadRequest request)
        {
            return !_shuttingDown && ReferenceEquals(_request, request);
        }

        public virtual void UnloadResources()
        {
            CancelRequest(_request);
        }

        internal void Shutdown()
        {
            _shuttingDown = true;
            try
            {
                UnloadResources();
            }
            finally
            {
                CancelRequest(_request);
            }
        }

        private void CancelRequest(LoadRequest request)
        {
            if (request == null)
                return;

            if (ReferenceEquals(_request, request))
                _request = null;
            request.Ready = false;
            request.Resources.Clear();

            // Pending operations retain their callback and release themselves when they finish.
            for (var i = 0; i < request.Handles.Count; i++)
                if (request.Handles[i].IsValid() && request.Handles[i].IsDone)
                    ReleaseHandle(request, i);

            CompleteCallbacks(request);
        }

        private static void ReleaseHandle(LoadRequest request, int index)
        {
            var handle = request.Handles[index];
            request.Handles[index] = default;
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        private void CompleteCallbacks(LoadRequest request)
        {
            var callbacks = request.Callbacks;
            request.Callbacks = null;
            if (callbacks == null)
                return;

            foreach (var callback in callbacks)
                InvokeCallback(IsCurrent(request) && request.Ready ? callback.Loaded : callback.Failed);
        }

        private static void InvokeCallback(Action callback)
        {
            if (callback == null || (callback.Target is UnityEngine.Object target && target == null))
                return;

            try
            {
                callback();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
