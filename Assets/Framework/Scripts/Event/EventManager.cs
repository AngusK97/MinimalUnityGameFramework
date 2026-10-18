using System;
using UnityEngine;
using System.Collections.Generic;

namespace Event
{
    public class DispatchData
    {
        public string Type;
        public object Sender;
        public BaseEventArgs EventArgs;
    }

    public abstract class BaseEventArgs
    {
        public abstract void Clear();
    }

    public delegate void HandlerEvent(object sender, string type, BaseEventArgs eventArgs = null);

    public class EventManager : MonoBehaviour
    {
        private List<DispatchData> _dispatchDataList = new();
        private List<DispatchData> _processingDataList = new();
        private bool _isUpdating;
        private readonly Dictionary<string, List<HandlerEvent>> _handlerEventDict = new();

        public void RegisterEvent(string type, HandlerEvent handle)
        {
            ValidateEventType(type);

            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

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

        public void UnRegisterEvent(string type, HandlerEvent handle)
        {
            ValidateEventType(type);

            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

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

        public void DispatchNow(object sender, string type, BaseEventArgs eventArgs = null)
        {
            ValidateEventType(type);

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

        public void Dispatch(object sender, string type, BaseEventArgs eventArgs = null)
        {
            ValidateEventType(type);

            _dispatchDataList.Add(new DispatchData() { Sender = sender, Type = type, EventArgs = eventArgs });
        }

        private static void ValidateEventType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Event ID must not be empty.", nameof(type));
            }
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
