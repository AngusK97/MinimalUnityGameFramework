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
        private sealed class Listener
        {
            public readonly Delegate Handler;
            public readonly Delegate[] InvocationList;

            public Listener(Delegate handler)
            {
                Handler = handler;
                InvocationList = handler.GetInvocationList();
            }
        }

        private List<DispatchData> _dispatchDataList = new();
        private List<DispatchData> _processingDataList = new();
        private bool _isUpdating;
        private bool _disposed;
        private readonly Dictionary<string, List<Listener>> _handlerEventDict = new();

        public void RegisterEvent(string type, Action handle)
        {
            RegisterListener(type, handle);
        }

        public void RegisterEvent(string type, HandlerEvent handle)
        {
            RegisterListener(type, handle);
        }

        private void RegisterListener(string type, Delegate handle)
        {
            ThrowIfDisposed();
            ValidateEventType(type);

            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            if (!_handlerEventDict.TryGetValue(type, out var listeners))
            {
                listeners = new List<Listener>();
                _handlerEventDict.Add(type, listeners);
            }

            listeners.Add(new Listener(handle));
        }

        public void UnRegisterEvent(string type, Action handle)
        {
            UnRegisterListener(type, handle);
        }

        public void UnRegisterEvent(string type, HandlerEvent handle)
        {
            UnRegisterListener(type, handle);
        }

        private void UnRegisterListener(string type, Delegate handle)
        {
            if (_disposed)
            {
                return;
            }

            ValidateEventType(type);

            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            if (!_handlerEventDict.TryGetValue(type, out var listeners))
            {
                return;
            }

            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].Handler.Equals(handle))
                {
                    listeners.RemoveAt(i);
                    RemoveEmptyBucket(type, listeners);
                    return;
                }
            }
        }

        public void DispatchNow(string type)
        {
            DispatchNow(null, type);
        }

        public void DispatchNow(object sender, string type, BaseEventArgs eventArgs = null)
        {
            ThrowIfDisposed();
            ValidateEventType(type);

            if (!_handlerEventDict.TryGetValue(type, out var listeners))
            {
                return;
            }

            // Each dispatch owns its snapshot, including nested immediate dispatches.
            foreach (var listener in listeners.ToArray())
            {
                if (_disposed)
                {
                    break;
                }

                if (!HasLiveTarget(listener))
                {
                    // Remove this exact registration, not another equal subscription.
                    if (_handlerEventDict.TryGetValue(type, out var currentListeners))
                    {
                        currentListeners.Remove(listener);
                        RemoveEmptyBucket(type, currentListeners);
                    }

                    continue;
                }

                try
                {
                    // A multicast delegate remains one registered listener for exception handling.
                    foreach (var callback in listener.InvocationList)
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        if (IsDestroyedTarget(callback))
                        {
                            continue;
                        }

                        if (callback is Action action)
                        {
                            action();
                        }
                        else
                        {
                            ((HandlerEvent)callback)(sender, type, eventArgs);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        public void Dispatch(string type)
        {
            Dispatch(null, type);
        }

        public void Dispatch(object sender, string type, BaseEventArgs eventArgs = null)
        {
            ThrowIfDisposed();
            ValidateEventType(type);

            _dispatchDataList.Add(new DispatchData() { Sender = sender, Type = type, EventArgs = eventArgs });
        }

        private void RemoveEmptyBucket(string type, List<Listener> listeners)
        {
            if (listeners.Count == 0)
            {
                _handlerEventDict.Remove(type);
            }
        }

        private static bool HasLiveTarget(Listener listener)
        {
            foreach (var callback in listener.InvocationList)
            {
                if (!IsDestroyedTarget(callback))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDestroyedTarget(Delegate callback)
        {
            return callback.Target is UnityEngine.Object target && target == null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(EventManager));
            }
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
            if (_disposed || _isUpdating || _dispatchDataList.Count == 0)
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
                    if (_disposed)
                    {
                        break;
                    }

                    DispatchNow(dispatchData.Sender, dispatchData.Type, dispatchData.EventArgs);
                }
            }
            finally
            {
                _processingDataList.Clear();
                _isUpdating = false;
            }
        }

        private void OnDestroy()
        {
            _disposed = true;
            _handlerEventDict.Clear();
            _dispatchDataList.Clear();

            // An active batch owns its list until OnUpdate's finally block.
            if (!_isUpdating)
            {
                _processingDataList.Clear();
            }
        }
    }
}
