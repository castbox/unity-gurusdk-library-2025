
namespace Guru
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;
    
    public class EventBus: MonoBehaviour
    {
        private const string Tag = "[EventBus]";
        private static bool _initOnce = false;
        private Dictionary<string, List<Delegate>> _normalEvents;
        private Dictionary<string, List<Delegate>> _mainThreadEvents;
        private Queue<MainThreadEvent> _mainThreadQueue;
        private Queue<MainThreadEvent> _mainThreadEventPool;

        private static EventBus _instance;
        public static EventBus Instance {
            get
            {
                if (_instance == null)
                {
                    if (GuruSDKCallback.Instance != null)
                    {
                        _instance = AttachOnHost(GuruSDKCallback.Instance); // Set to the host object
                    }
                }
                return _instance;
            }
        }



        public static EventBus AttachOnHost(GameObject go)
        {
            var ins = go.AddComponent<EventBus>();
            ins.Init();
            return ins;
        }
        public static EventBus AttachOnHost(Component component)
        {
            return component.gameObject.AddComponent<EventBus>();
        }



        

        #region API

        public static void Bind(string evtName, Action<object> callback)
        {
            if (Instance == null)
            {
                LogE($"{Tag} EventBus is not ready");
                return;
            }
            
            Instance.AddNormalEvent(evtName, callback);
        }


        public static void UnBind(string evtName, Action<object> callback)
        {
            if (Instance == null)
            {
                LogE($"{Tag} EventBus is not ready");
                return;
            }
            
            Instance.RemoveNormalEvent(evtName, callback);
        }


        public static void BindOnMainThread(string evtName, Action<object> callback)
        {
            if (Instance == null)
            {
                LogE($"{Tag} EventBus is not ready");
                return;
            }
            
            Instance.AddNormalEventOnMainThread(evtName, callback);
        }
        
        
        
        
        public static void UnBindOnMainThread(string evtName, Action<object> callback)
        {
            if (Instance == null)
            {
                LogE($"{Tag} EventBus is not ready");
                return;
            }
            
            Instance.RemoveNormalEventOnMainThread(evtName, callback);
        }

        public static void Send(string evtName, object evt)
        {
            if (Instance == null)
            {
                LogE($"{Tag} EventBus is not ready");
                return;
            }
            
            Instance.FireEvent(evtName, evt);
        }
        
        
        


        #endregion





        #region Initialize Functions

        private void Init()
        {
            if (_initOnce) return;
            _initOnce = true;
            
            _normalEvents = new Dictionary<string, List<Delegate>>(20);
            _mainThreadEvents = new Dictionary<string, List<Delegate>>(20);
            _mainThreadQueue = new Queue<MainThreadEvent>(20);
            _mainThreadEventPool = new Queue<MainThreadEvent>(20);
        }

        private void AddEvent(string evtName, Action<object> callback, ref Dictionary<string, List<Delegate>> dict)
        {
            if (dict == null) dict = new Dictionary<string, List<Delegate>>(20);
            
            if (!IsEventExists(evtName, dict))
            {
                dict[evtName] = new List<Delegate>(20);
            }

            if (!dict[evtName].Contains(callback))
            {
                dict[evtName].Add(callback);
            }
            
        }

        
        private void RemoveEvent(string evtName, Action<object> callback, ref Dictionary<string, List<Delegate>> dict)
        {

            if (dict == null)
            {
                dict = new Dictionary<string, List<Delegate>>(20);
                return;
            }
            
            if (IsEventExists(evtName, dict) && dict[evtName].Contains(callback))
            {
                dict[evtName].Remove(callback);
            }
        }
        
        private bool IsEventExists(string eventName, Dictionary<string , List<Delegate>> events)
        {
            if(events == null) return false;
            return events.ContainsKey(eventName);
        }
        
        #endregion

        #region Normal Events

        
        private void AddNormalEvent(string evtName, Action<object> callback)
        {
            AddEvent(evtName, callback, ref _normalEvents);
        }

        private void AddNormalEventOnMainThread(string evtName,  Action<object> callback)
        {
            AddEvent(evtName, callback, ref _mainThreadEvents);
        }

        private void RemoveNormalEvent(string evtName, Action<object> callback)
        {
            RemoveEvent(evtName, callback, ref _normalEvents);
        }
        
        private void RemoveNormalEventOnMainThread(string evtName, Action<object> callback)
        {
            RemoveEvent(evtName, callback, ref _mainThreadEvents);
        }

        private void FireEvent(string evtName, object evtObject)
        {
            // ---------- Normal Events ------------
            if (_normalEvents.TryGetValue(evtName, out var listeners))
            {
                int i = 0;
                while (i < listeners.Count)
                {
                    listeners[i]?.DynamicInvoke(evtObject);
                    i++;
                }
                return;
            }

            // ---------- Main Thread Events ------------
            if (IsEventExists(evtName, _mainThreadEvents))
            {
                AddEventToMainThread(evtName, evtObject);
                return;
            }
            
            // ---------- Send Events Failed ------------
            LogE($"{Tag} EventBus:: Fire event [{evtName}] not found");
        }


        #endregion

        #region Logger


        private static void LogI(object msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        private static void LogE(object msg)
        {
            UnityEngine.Debug.LogError(msg);
        }
        #endregion


        #region Mono Lifecycle

        private void Update()
        {
            ConsumeMainThreadEvent(); // 消费主线程事件
        }

        #endregion




        #region ThreadQueueInfo

        
        internal struct MainThreadEvent
        {

            public string eventName;
            public object eventBody;

            public void Setup(string evtName, object evtBody)
            {
                eventName = evtName;
                eventBody = evtBody;
            }

            public void Clear()
            {
                eventName = null;
                eventBody = null;
            }
        }


        private MainThreadEvent GetMainThreadEvent(string evtName, object evtBody)
        {
            MainThreadEvent evt;
            if (_mainThreadEventPool != null && _mainThreadEventPool.Count > 0)
            {
                evt  = _mainThreadEventPool.Dequeue();
            }
            else
            {
                evt = new MainThreadEvent();
            }
            evt.Setup(evtName, evtBody);
            return evt;
        }


        private void AddEventToMainThread(string evtName, object evtBody)
        {
            if(_mainThreadQueue == null) _mainThreadQueue = new Queue<MainThreadEvent>(20);
            _mainThreadQueue.Enqueue(GetMainThreadEvent(evtName, evtBody));
        }

        private void SaveMainThreadEvent(MainThreadEvent evt)
        {
            if (_mainThreadEventPool == null)
            {
                _mainThreadEventPool = new Queue<MainThreadEvent>(20);
            }
            evt.Clear();
            _mainThreadEventPool.Enqueue(evt);
        }

        
        
        private void ConsumeMainThreadEvent()
        {
            if (_mainThreadQueue != null && _mainThreadQueue.Count > 0)
            {
                while (_mainThreadQueue.Count > 0)
                {
                    var evt  = _mainThreadQueue.Dequeue();
                    FireEvent(evt.eventName, evt.eventBody);
                    SaveMainThreadEvent(evt);
                }
            }
        }



        #endregion

    }
}