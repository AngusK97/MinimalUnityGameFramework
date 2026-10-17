using System;
using UnityEngine;

namespace UI
{
    public class UIBase : MonoBehaviour
    {
        protected object Data;
        protected string UIName;

        private bool _initialized;
        private bool _shown;
        private bool _released;
        private Action _closeRequested;

        internal void SetCloseCallback(Action callback)
        {
            _closeRequested = callback;
        }

        public void Init(string id, object data)
        {
            if (_initialized || _released)
            {
                return;
            }

            _initialized = true;
            Data = data;
            UIName = id;
            OnInit();
        }

        public void Show()
        {
            if (!_initialized || _shown || _released)
            {
                return;
            }

            _shown = true;
            OnShow();
        }

        public void Close()
        {
            if (_closeRequested != null)
            {
                _closeRequested();
            }
            else
            {
                CloseInternal();
            }
        }

        private void CloseInternal()
        {
            if (!_shown)
            {
                return;
            }

            _shown = false;
            OnClose();
        }

        public void Update()
        {
            if (_shown && !_released)
            {
                OnUpdate();
            }
        }

        public void Release()
        {
            if (_closeRequested != null)
            {
                _closeRequested();
            }
            else
            {
                ReleaseFromManager();
            }
        }

        internal void ReleaseFromManager()
        {
            if (_released)
            {
                return;
            }

            _released = true;

            try
            {
                CloseInternal();
            }
            finally
            {
                Data = null;
                _closeRequested = null;
            }
        }

        protected virtual void OnDestroy()
        {
            try
            {
                Release();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        protected virtual void OnInit() { }
        protected virtual void OnShow() { }
        protected virtual void OnClose() { }
        protected virtual void OnUpdate() { }

        protected void CloseSelf()
        {
            Close();
        }
    }
}
