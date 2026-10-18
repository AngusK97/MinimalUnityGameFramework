using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Resource
{
    public class ResourceManager : MonoBehaviour
    {
        private readonly Dictionary<string, BaseResourceModule> _resourceModules = new();
        private bool _shuttingDown;

        public void Init()
        {
        }

        public void RegisterResourceModule(BaseResourceModule baseResourceModule)
        {
            if (baseResourceModule == null)
            {
                throw new ArgumentNullException(nameof(baseResourceModule));
            }

            if (_shuttingDown)
            {
                throw new InvalidOperationException("ResourceManager is shutting down.");
            }

            var name = baseResourceModule.GetName();

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Resource module ID must not be empty.", nameof(baseResourceModule));
            }

            if (_resourceModules.ContainsKey(name))
            {
                Debug.LogError($"ResourceManager.RegisterResourceModule: {name} already registered");
                return;
            }

            _resourceModules.Add(name, baseResourceModule);
        }

        public T GetResourceModule<T>(string moduleName) where T : BaseResourceModule
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return null;
            }

            T t = default(T);

            if (_resourceModules.TryGetValue(moduleName, out var module))
            {
                t = (T)module;
            }

            return t;
        }

        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key)
        {
            if (_shuttingDown)
            {
                throw new InvalidOperationException("ResourceManager is shutting down.");
            }

            return Addressables.LoadAssetAsync<TObject>(key);
        }

        public void Release<TObject>(AsyncOperationHandle<TObject> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        public void Release(AsyncOperationHandle handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        public void Release<TObject>(TObject obj)
        {
            Addressables.Release(obj);
        }
        
        public void ReleaseInstance(GameObject obj)
        {
            Addressables.ReleaseInstance(obj);
        }

        public void ReleaseInstance(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsValid())
            {
                Addressables.ReleaseInstance(handle);
            }
        }

        public void InstantiateGameObjectAsync(string path, Transform parent, 
            bool instantiateInWorldSpace, Action<AsyncOperationHandle<GameObject>> callback)
        {
            if (_shuttingDown)
            {
                DeliverInstance(default, callback);
                return;
            }

            AsyncOperationHandle<GameObject> handle;
            
            try
            {
                var instantiateParameters = new InstantiationParameters(parent, instantiateInWorldSpace);
                handle = Addressables.InstantiateAsync(path, instantiateParameters);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                DeliverInstance(default, callback);
                return;
            }

            handle.Completed += completed => DeliverInstance(completed, callback);
        }

        private void DeliverInstance(AsyncOperationHandle<GameObject> handle,
            Action<AsyncOperationHandle<GameObject>> callback)
        {
            if (callback == null || (callback.Target is UnityEngine.Object target && target == null))
            {
                ReleaseInstance(handle);
                return;
            }

            // Ownership transfers before invoking: a receiver may retain or release the handle, then throw.
            try
            {
                callback(handle);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void OnDestroy()
        {
            _shuttingDown = true;
            var modules = new List<BaseResourceModule>(_resourceModules.Values);
            _resourceModules.Clear();

            foreach (var module in modules)
            {
                try
                {
                    module.Shutdown();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }
    }
}
