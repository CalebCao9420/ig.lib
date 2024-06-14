using System.Collections.Generic;
using IG.Runtime.Wrap.Center;
using UnityEngine;

namespace IG.Events{
    public class EventMonoBehaviour : MonoBehaviour, IGBC{
        public string GUID{ get; protected set; }

    #region Events process

        private Dictionary<string, List<GameEventManager.EventCallback>> _eventDic;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake(){
            this.GUID = System.Guid.NewGuid().ToString();
            EventMonoCenter.Instance.RegisterCtrl(this.GUID, this);
            this.AddEvent();
        }

        /// <summary>
        /// Raises the destroy event.
        /// After the override method, must call the base method
        /// </summary>
        protected virtual void OnDestroy(){
            if (EventMonoCenter.IsValid){
                EventMonoCenter.Instance.DeregisterCtrl(this.GUID);
            }

            if (GameEventManager.IsValid){
                RemoveEvent();
            }
        }

        /// <summary>
        /// Adds the event.
        /// </summary>
        protected virtual void AddEvent(){ }

        /// <summary>
        /// Remove listener event
        /// </summary>
        protected void RemoveEvent(){
            if (_eventDic == null){
                return;
            }

            foreach (var item in _eventDic){
                while (item.Value.Count > 0){
                    GameEventManager.RemoveEventListener(item.Key, item.Value[0]);
                    item.Value.RemoveAt(0);
                }
            }

            _eventDic.Clear();
            _eventDic = null;
        }

        /// <summary>
        /// Adds the event.
        /// </summary>
        /// <param name="engineEventType">Engine event type.</param>
        /// <param name="eventCallback">Event callback.</param>
        protected void AddEventListener(string engineEventType, GameEventManager.EventCallback eventCallback){
            if (_eventDic == null){
                _eventDic = new Dictionary<string, List<GameEventManager.EventCallback>>();
            }

            if (!_eventDic.ContainsKey(engineEventType)){
                _eventDic.Add(engineEventType, new List<GameEventManager.EventCallback>());
            }

            if (!_eventDic[engineEventType].Contains(eventCallback)){
                _eventDic[engineEventType].Add(eventCallback);
            }

            GameEventManager.AddEventListener(engineEventType, eventCallback);
        }

        /// <summary>
        /// Removes the event.
        /// </summary>
        /// <param name="engineEventType">Engine event type.</param>
        /// <param name="eventCallback">Event callback.</param>
        protected void RemoveEventListener(string engineEventType, GameEventManager.EventCallback eventCallback){
            if (!_eventDic.ContainsKey(engineEventType)){
                return;
            }

            if (!_eventDic[engineEventType].Contains(eventCallback)){
                _eventDic[engineEventType].Remove(eventCallback);
            }

            GameEventManager.RemoveEventListener(engineEventType, eventCallback);
        }
    }

#endregion
}