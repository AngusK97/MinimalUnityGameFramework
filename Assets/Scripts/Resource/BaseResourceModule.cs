using Core;
using System;
using UnityEngine;
using System.Collections.Generic;

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
        private readonly List<ResourceInfo> _resourceInfos = new();
        private readonly Dictionary<int, object> _resources = new();
        
        private bool _recordedResourceInfos;

        public abstract ResourceModuleName GetName();


        //-------------------------------------------------------------------------------------
        // Get Resource And Info
        //-------------------------------------------------------------------------------------

        protected T GetResource<T>(int id)
        {
            _resources.TryGetValue(id, out var resource);
            return (T)resource;
        }

        private bool IsAllResourcesLoaded()
        {
            return _resourceInfos.Count == _resources.Count;
        }


        //-------------------------------------------------------------------------------------
        // Record Resource Infos
        //-------------------------------------------------------------------------------------

        protected abstract void RecordAllResourceInfos();

        protected void AddResourceInfo(ResourceType type, int resId, string path)
        {
            _resourceInfos.Add(new ResourceInfo { ResNameId = resId, ResPath = path, ResourceType = type });
        }


        //-------------------------------------------------------------------------------------
        // Load Resources
        //-------------------------------------------------------------------------------------

        public void LoadResources(Action loadedCallBack = null)
        {
            if (!_recordedResourceInfos)
            {
                _resourceInfos.Clear();
                RecordAllResourceInfos();
                _recordedResourceInfos = true;
            }
            
            if (IsAllResourcesLoaded())
            {
                loadedCallBack?.Invoke();
                return;
            }

            _resources.Clear();
            LoadResourcesByInfo(loadedCallBack);
        }

        private void LoadResourcesByInfo(Action loadedCallback = null)
        {
            foreach (var resInfo in _resourceInfos)
            {
                var type = resInfo.ResourceType;

                switch (type)
                {
                    case ResourceType.Sprite:
                        LoadResourceOfType<Sprite>(resInfo, loadedCallback);
                        break;
                    case ResourceType.AudioClip:
                        LoadResourceOfType<AudioClip>(resInfo, loadedCallback);
                        break;
                    case ResourceType.GameObject:
                        LoadResourceOfType<GameObject>(resInfo, loadedCallback);
                        break;
                }
            }
        }

        private void LoadResourceOfType<T>(ResourceInfo resourceInfo, Action loadedCallback = null)
        {
            var path = resourceInfo.ResPath;
            var resId = resourceInfo.ResNameId;

            GameCore.Resource.LoadAssetAsync<T>(path).Completed += handle =>
            {
                if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    return;
                }

                _resources.Add(resId, handle.Result);
                if (_resources.Count == _resourceInfos.Count)
                {
                    loadedCallback?.Invoke();
                }
            };
        }


        //-------------------------------------------------------------------------------------
        // Unload Resources
        //-------------------------------------------------------------------------------------

        public virtual void UnloadResources() { }
    }
}