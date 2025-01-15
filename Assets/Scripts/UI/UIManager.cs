using Core;
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private class LoadingData
        {
            public object Arg;
            public UILayer Layer;
            public UIName UIName;
            public Action<UIBase> Callback;
        }

        private readonly Dictionary<UIName, UIData> _registeredUIData = new(); // 所有可用的窗口数据
        private readonly List<LoadingData> _loadingDataList = new(); // 等待打开的窗口
        private readonly List<UIName> _openRecord = new(); // 打开的窗口记录
        private readonly Dictionary<UIName, UIBase> _openedUIBaseDict = new(); // 已经打开的窗口

        [SerializeField] private Camera uiCamera;
        [SerializeField] private GameObject[] layerObjects;

        [Header("Transition")]
        public CanvasGroup transitionCanvasGroup;
        public float fadeOutTime = 0.1f;
        public float fadeInTime = 0.18f;
        public Ease fadeInType = Ease.OutCubic;
        public Ease fadeOutType = Ease.OutQuad;


        //-----------------------------------------------------------------------------------------------
        // Lifecycle
        //-----------------------------------------------------------------------------------------------

        public void RegisterUiData(UIData uiData)
        {
            var viewName = uiData.UIName;
            if (_registeredUIData.ContainsKey(viewName))
            {
                Debug.LogError($"UIManager.RegisterUiData: {viewName} already registered");
                return;
            }

            _registeredUIData.Add(viewName, uiData);
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            foreach (var pair in _openedUIBaseDict)
            {
                var uiBase = pair.Value;
                uiBase.Update();
            }

            while (_loadingDataList.Count > 0)
            {
                var waitOpenData = _loadingDataList[0];
                _loadingDataList.RemoveAt(0);
                LoadUI(waitOpenData);
            }
        }


        //-----------------------------------------------------------------------------------------------
        // Open View
        //-----------------------------------------------------------------------------------------------

        public void OpenUI(UILayer layer, UIName uiName, object arg = null, Action<UIBase> callback = null)
        {
            if (!_registeredUIData.ContainsKey(uiName))
            {
                callback?.Invoke(null);
                return;
            }

            if (_openRecord.Contains(uiName))
            {
                callback?.Invoke(null);
                return;
            }

            _openRecord.Add(uiName);

            var wait = new LoadingData
            {
                Arg = arg,
                Layer = layer,
                UIName = uiName,
                Callback = callback
            };
            _loadingDataList.Add(wait);
        }

        private void LoadUI(LoadingData loadingData)
        {
            var arg = loadingData.Arg;
            var viewName = loadingData.UIName;
            var uiData = _registeredUIData[viewName];
            var layerObj = GetGameObjectByUILayers(loadingData.Layer);
            var layerTransform = layerObj.transform;

            GameCore.Resource.InstantiateGameObjectAsync(uiData.ResourcePath,
                layerTransform, false, loadHandle =>
                {
                    var obj = loadHandle.Result;
                    if (obj == null)
                    {
                        GameCore.Resource.Release(loadHandle);
                        return;
                    }

                    var uiBase = obj.GetComponent<UIBase>();
                    uiBase.Init(viewName, arg);
                    uiBase.Show();

                    _openedUIBaseDict.Add(viewName, uiBase);
                    loadingData.Callback?.Invoke(uiBase);
                });
        }

        private GameObject GetGameObjectByUILayers(UILayer uiLayer)
        {
            GameObject obj = null;

            var index = (int)uiLayer;
            if (index < layerObjects.Length)
            {
                obj = layerObjects[index];
            }

            return obj;
        }


        //-----------------------------------------------------------------------------------------------
        // Close View
        //-----------------------------------------------------------------------------------------------

        public bool CloseUI(UIName uiName)
        {
            // 未注册过
            if (!_registeredUIData.ContainsKey(uiName))
            {
                return false;
            }

            // 未打开过
            if (!_openRecord.Contains(uiName))
            {
                return false;
            }

            var waitOpenData = _loadingDataList.Find(openData => openData.UIName == uiName);
            if (waitOpenData != null)
            {
                _loadingDataList.Remove(waitOpenData);
                _openRecord.Remove(uiName);
                return false;
            }

            var uiData = _registeredUIData[uiName];
            return CloseOpenedUI(uiData);
        }

        private bool CloseOpenedUI(UIData data)
        {
            var viewName = data.UIName;
            if (!_openedUIBaseDict.ContainsKey(viewName))
            {
                return false;
            }

            var uiBase = _openedUIBaseDict[viewName];
            _openedUIBaseDict.Remove(viewName);

            if (uiBase != null)
            {
                uiBase.Close();
                GameCore.Resource.ReleaseInstance(uiBase.gameObject);
            }

            _openRecord.Remove(viewName);

            return true;
        }

        public void CloseAll()
        {
            var closeList = _openedUIBaseDict.Select(pair => pair.Key).ToList();
            foreach (var uiBase in closeList)
            {
                CloseUI(uiBase);
            }
        }


        //-----------------------------------------------------------------------------------------------
        // Transition
        //-----------------------------------------------------------------------------------------------

        public void OpenViewWithTransition(UIName newUIName, UIName curUIName,
            UILayer layer, object data = null, Action blackCallback = null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.gameObject.SetActive(true);
            transitionCanvasGroup.DOFade(1f, fadeInTime).SetEase(fadeInType).onComplete = () =>
            {
                blackCallback?.Invoke();
                OpenUI(layer, newUIName, data, uiBase =>
                {
                    if (IsOpen(curUIName))
                    {
                        CloseUI(curUIName);
                    }

                    transitionCanvasGroup.DOFade(0f, fadeOutTime).SetEase(fadeOutType).onComplete = () =>
                    {
                        transitionCanvasGroup.gameObject.SetActive(false);
                    };
                });
            };
        }

        public bool IsOpen(UIName uiName)
        {
            return _openRecord.Contains(uiName);
        }

        public void TransitionFadeOut(Action completeCallback = null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.gameObject.SetActive(true);
            transitionCanvasGroup.DOFade(1f, fadeOutTime).SetEase(fadeOutType).onComplete = () =>
            {
                completeCallback?.Invoke();
            };
        }

        public void TransitionFadeIn(Action completeCallback = null)
        {
            transitionCanvasGroup.DOFade(0f, fadeInTime).SetEase(fadeInType).onComplete = () =>
            {
                completeCallback?.Invoke();
                transitionCanvasGroup.gameObject.SetActive(false);
            };
        }

        public void ShowTransition()
        {
            transitionCanvasGroup.gameObject.SetActive(true);
            transitionCanvasGroup.alpha = 1f;
        }
    }
}