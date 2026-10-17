using System;
using UnityEngine;
using System.Collections.Generic;

namespace Event
{
    public class DispatchData
    {
        public EventName Type;
        public object Sender;
        public BaseEventArgs EventArgs;
    }

    public abstract class BaseEventArgs
    {
        public abstract void Clear();
    }

    public delegate void HandlerEvent(object sender, EventName type, BaseEventArgs eventArgs = null);

    public class EventManager : MonoBehaviour
    {
        private List<DispatchData> _dispatchDataList = new();
        private List<DispatchData> _processingDataList = new();
        private bool _isUpdating;
        private readonly Dictionary<EventName, List<HandlerEvent>> _handlerEventDict = new();

        public void RegisterEvent(EventName type, HandlerEvent handle)
        {
            var ifContain = _handlerEventDict.TryGetValue(type, out var handles);
            if (ifContain)
            {
                handles.Add(handle);
            }
            else
            {
                handles = new List<HandlerEvent> { handle };
                _handlerEventDict.Add(type, handles);
            }
        }

        public void UnRegisterEvent(EventName type, HandlerEvent handle)
        {
            var ifContain = _handlerEventDict.TryGetValue(type, out var handles);
            if (!ifContain)
            {
                return;
            }

            if (handles.Contains(handle))
            {
                handles.Remove(handle);
            }
        }

        public void DispatchNow(object sender, EventName type, BaseEventArgs eventArgs = null)
        {
            var ifContain = _handlerEventDict.TryGetValue(type, out var handles);
            if (!ifContain)
            {
                return;
            }

            // Each dispatch owns its snapshot, including nested immediate dispatches.
            foreach (var handlerEvent in handles.ToArray())
            {
                try
                {
                    handlerEvent?.Invoke(sender, type, eventArgs);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        public void Dispatch(object sender, EventName type, BaseEventArgs eventArgs = null)
        {
            _dispatchDataList.Add(new DispatchData() { Sender = sender, Type = type, EventArgs = eventArgs });
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            if (_isUpdating || _dispatchDataList.Count == 0)
            {
                return;
            }

            _isUpdating = true;
            // Callbacks enqueue into the other buffer for the next update.
            var pending = _dispatchDataList;
            _dispatchDataList = _processingDataList;
            _processingDataList = pending;

            try
            {
                foreach (var dispatchData in _processingDataList)
                {
                    DispatchNow(dispatchData.Sender, dispatchData.Type, dispatchData.EventArgs);
                }
            }
            finally
            {
                _processingDataList.Clear();
                _isUpdating = false;
            }
        }
    }
}
