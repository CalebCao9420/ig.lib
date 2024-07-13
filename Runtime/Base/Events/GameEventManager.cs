using System.Collections;
using System.Collections.Generic;

namespace IG.Events{
    /// <summary>
    /// 游戏事件管理器
    /// TODO:当前只有立即触发,有对应需求时可以增加Sequence列表,触发时打开Sequence列表逐一执行
    /// </summary>
    public class GameEventManager : SingletonAbs<GameEventManager>{
        public delegate void EventCallback(GameEvent engineEvent);

    #region string key events

        private readonly Hashtable _eventHashtable         = null;
        private readonly Hashtable _eventCallbackHashtable = null;

    #endregion

        public GameEventManager(){
            _eventHashtable         = new Hashtable();
            _eventCallbackHashtable = new Hashtable();
        }

        public override void OnDispose(){
            if (_eventHashtable != null){
                _eventHashtable.Clear();
            }

            if (_eventCallbackHashtable != null){
                _eventCallbackHashtable.Clear();
            }
        }

        /// <summary>
        /// Dispatchs the event.
        /// </summary>
        /// <returns><c>true</c>, if event was dispatched, <c>false</c> otherwise.</returns>
        /// <param name="engineEvent">Engine event.</param>
        public static bool DispatchEvent(GameEvent engineEvent){ return Instance.PDispatchEvent(engineEvent); }

        /// <summary>
        /// Dispatchs the event.
        /// </summary>
        /// <returns><c>true</c>, if event was dispatched, <c>false</c> otherwise.</returns>
        /// <param name="eventType">Event type.</param>
        /// <param name="eventParams">Event parameters.</param>
        public static bool DispatchEvent(string eventType, IGameEventParam eventParams = null){ return DispatchEvent(new GameEvent(eventType, eventParams)); }

        /// <summary>
        ///Public adds event listener 
        /// </summary>
        public static void AddEventListener(string eventType, EventCallback eventCallback){ Instance.PAddEventListener(eventType, eventCallback); }

        public static void RemoveEventListener(string eventType, EventCallback eventCallback){
            if (IsValid){
                Instance.PRemoveEventListener(eventType, eventCallback);
            }
        }

        /// <summary>
        /// Clears the event listener.
        /// </summary>
        public static void ClearEventListener(){ Instance.PClearEventListener(); }

        /// <summary>
        /// Dispatch event
        /// </summary>
        /// <returns><c>true</c>, Distribute success, <c>false</c>Distribution failure.</returns>
        /// <param name="engineEvent">Event object.</param>
        private bool PDispatchEvent(GameEvent engineEvent){
            if (_eventHashtable.ContainsKey(engineEvent.EventType)){
                return (_eventHashtable[engineEvent.EventType] as GameEventHandle).DispatchEvent(engineEvent);
            }
            else{
                return false;
            }
        }

        /// <summary>
        /// Dispatch event.
        /// </summary>
        /// <returns><c>true</c>, Distribute success, <c>false</c> Distribution failure.</returns>
        /// <param name="eventType">Event type.</param>
        /// <param name="eventParams">Passed parameters.</param>
        private bool PDispatchEvent(string eventType, IGameEventParam eventParams = null){ return DispatchEvent(new GameEvent(eventType, eventParams)); }

        /// <summary>
        /// Listening event
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="eventCallback">Event callback method.</param>
        private void PAddEventListener(string eventType, EventCallback eventCallback){
            if (!_eventHashtable.ContainsKey(eventType)){
                _eventHashtable.Add(eventType, new GameEventHandle());
            }

            (_eventHashtable[eventType] as GameEventHandle).eventHandle -= eventCallback;
            (_eventHashtable[eventType] as GameEventHandle).eventHandle += eventCallback;
            if (!_eventCallbackHashtable.ContainsKey(eventType)){
                _eventCallbackHashtable.Add(eventType, new List<EventCallback>());
            }

            (_eventCallbackHashtable[eventType] as List<EventCallback>).Add(eventCallback);
        }

        /// <summary>
        /// Remove event listener
        /// </summary>
        /// <param name="eventType">Event type.</param>
        /// <param name="eventCallback">Event callback method.</param>
        private void PRemoveEventListener(string eventType, EventCallback eventCallback){
            if (_eventHashtable.ContainsKey(eventType)){
                //(eventHashtable[eventType] as EngineEvnetHander).Remove(eventCallback);
                (_eventHashtable[eventType] as GameEventHandle).eventHandle -= eventCallback;
                if (!(_eventHashtable[eventType] as GameEventHandle).HasEvent){
                    _eventHashtable.Remove(eventType);
                }

                if (_eventCallbackHashtable.ContainsKey(eventType)){
                    var list = _eventCallbackHashtable[eventType] as List<EventCallback>;
                    if (list.IndexOf(eventCallback) > 0){
                        list.Remove(eventCallback);
                    }
                    else{
                        eventCallback = null;
                    }
                }
            }
        }

        /// <summary>
        /// Clear all events
        /// </summary>
        private void PClearEventListener(){
            foreach (DictionaryEntry item in _eventCallbackHashtable){
                RemoveEventListener(item.Key.ToString(), item.Value as EventCallback);
            }

            _eventCallbackHashtable.Clear();
            _eventHashtable.Clear();
        }

        class GameEventHandle{
            public event EventCallback eventHandle;

            public bool DispatchEvent(GameEvent engineEvent){
                if (eventHandle != null){
                    eventHandle(engineEvent);
                    return true;
                }
                else{
                    return false;
                }
            }

            public bool HasEvent => eventHandle != null && eventHandle.GetInvocationList().Length > 0;
        }
    }
}