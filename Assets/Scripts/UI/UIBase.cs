using Core;
using UnityEngine;

namespace UI
{
    public class UIBase : MonoBehaviour
    {
        protected object Data;
        protected UIName UIName;
        
        public void Init(UIName id, object data)
        {
            Data = data;
            UIName = id;
            OnInit();
        }

        public void Show()
        {
            OnShow();
        }

        public void Close()
        {
            OnClose();
        }

        public void Update()
        {
            OnUpdate();
        }
        
        public void Release()
        {
            Data = null;
        }

        protected virtual void OnInit() { }

        protected virtual void OnShow() { }

        protected virtual void OnClose() { }
        
        protected virtual void OnUpdate() { }
        
        protected void CloseSelf()
        {
            GameCore.UI.CloseUI(UIName);
        }
    }
}