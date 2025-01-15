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
        private readonly Dictionary<ResourceModuleName, BaseResourceModule> _resourceModules = new();

        public void RegisterResourceModule(BaseResourceModule baseResourceModule)
        {
            if (baseResourceModule != null)
            {
                _resourceModules[baseResourceModule.GetName()] = baseResourceModule;
            }
        }

        public T GetResourceModule<T>(ResourceModuleName moduleName) where T : BaseResourceModule
        {
            T t = default(T);

            if (_resourceModules.TryGetValue(moduleName, out var module))
            {
                t = (T)module;
            }

            return t;
        }

        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key)
        {
            return Addressables.LoadAssetAsync<TObject>(key);
        }

        public void Release<TObject>(TObject obj)
        {
            Addressables.Release(obj);
        }
        
        public void ReleaseInstance(GameObject obj)
        {
            Addressables.ReleaseInstance(obj);
        }

        public void InstantiateGameObjectAsync(string path, Transform parent, 
            bool instantiateInWorldSpace, Action<AsyncOperationHandle<GameObject>> callback)
        {
            AsyncOperationHandle<GameObject> handle = default;
            
            try
            {
                var instantiateParameters = new InstantiationParameters(parent, instantiateInWorldSpace);
                handle = Addressables.InstantiateAsync(path, instantiateParameters);
                handle.Completed += operationHandle =>
                {
                    if (callback != null && callback.Target != null && !callback.Target.Equals(null))
                    {
                        callback.Invoke(handle);
                    }
                    else
                    {
                        Release(operationHandle);
                    }
                };
            }
            catch (Exception)
            {
                callback?.Invoke(handle);
            }
        }
    }
}