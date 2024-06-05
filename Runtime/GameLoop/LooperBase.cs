using System;
using System.Collections.Generic;
using System.Reflection;
using IG.Attribute;
using UnityEngine;

namespace IG{
    /// <summary>
    /// Looper的基本内容
    /// 方便子类继承做单独的容器所以子类T做单例实例
    /// 区别与GameLooper是Mono,LooperBase的继承者只是简单懒汉单例而已
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LooperBase<T> : SingletonAbs<T>, IGBC, IGameLoop where T : SingletonAbs<T>, new(){
        private LoopEvent _onInit;
        private LoopEvent _onDestroy;
        private LoopEvent _onFrameTick;
        private LoopEvent _onFixedIntervalTick;
        private LoopEvent _onTick;
        private LoopEvent _onFixedTick;
        private LoopEvent _onLateTick;
        private LoopEvent _onAsyncTick;

        //TODO:摘抄的GameLooper 优化下，没必要存在这种重复代码，虽然LooperBase主要目的是不走Mono，走GameLooper控制下的 单独的Event转发控制。但是重复代码确实没必要
        public Dictionary<string, IGBC> Ctrls{ get; private set; } = new();

        /// <summary>
        /// //TODO:摘抄的GameLooper 优化下，没必要存在这种重复代码，虽然LooperBase主要目的是不走Mono，走GameLooper控制下的 单独的Event转发控制。但是重复代码确实没必要
        /// 所有控制器保存的GameEvent
        /// </summary>
        public Dictionary<string, List<LoopEvent>> AllEvents{ get; private set; } = new();

        public string GUID{ get; protected set; }

        [IGBCEvent(LoopEventType.FrameUpdate)]
        protected virtual bool FrameTick(float deltaTime){
            this._onFrameTick?.Invoke(deltaTime);
            return true;
        }

        [IGBCEvent(LoopEventType.Update)]
        protected virtual bool Tick(float deltaTime){
            this._onTick?.Invoke(deltaTime);
            return true;
        }

        [IGBCEvent(LoopEventType.FixedIntervalUpdate)]
        protected virtual bool FixedIntervalTick(float deltaTime){
            this._onFixedIntervalTick?.Invoke(deltaTime);
            return true;
        }

        [IGBCEvent(LoopEventType.FixedUpdate)]
        protected virtual bool FixedTick(float deltaTime){
            this._onFixedTick?.Invoke(deltaTime);
            return true;
        }

        [IGBCEvent(LoopEventType.LateUpdate)]
        protected virtual bool LateTick(float deltaTime){
            this._onLateTick?.Invoke(deltaTime);
            return true;
        }

        [IGBCEvent(LoopEventType.AsyncUpdate)]
        protected virtual bool AsyncTick(float deltaTime){
            this._onAsyncTick?.Invoke(deltaTime);
            return true;
        }

        private void AddEvent(string id, LoopEvent @event){
            if (AllEvents.TryGetValue(id, out var list)){
                list.Add(@event);
            }
            else{
                AllEvents.Add(id, new List<LoopEvent>(){ @event });
            }
        }

        private void DelEvent(string id){
            if (AllEvents.TryGetValue(id, out var list)){
                list.Clear();
                list = null;
                AllEvents.Remove(id);
            }
        }

        public void RegisterEvent(LoopEventType type, LoopEvent @event){
            switch (type){
                case LoopEventType.Init:
                    this._onInit -= @event;
                    this._onInit += @event;
                    @event?.Invoke(GameLooper.DeltaTime);
                    break;
                case LoopEventType.FrameUpdate:
                    this._onFrameTick -= @event;
                    this._onFrameTick += @event;
                    break;
                case LoopEventType.Update:
                    this._onTick -= @event;
                    this._onTick += @event;
                    break;
                case LoopEventType.FixedIntervalUpdate:
                    this._onFixedIntervalTick -= @event;
                    this._onFixedIntervalTick += @event;
                    break;
                case LoopEventType.FixedUpdate:
                    this._onFixedTick -= @event;
                    this._onFixedTick += @event;
                    break;
                case LoopEventType.LateUpdate:
                    this._onLateTick -= @event;
                    this._onLateTick += @event;
                    break;
                case LoopEventType.AsyncUpdate:
                    this._onAsyncTick -= @event;
                    this._onAsyncTick += @event;
                    break;
                case LoopEventType.Destroy: //实际上只是为了Ctrl方便使用
                    this._onDestroy -= @event;
                    this._onDestroy += @event;
                    break;
            }
        }

        public void DeregisterEvent(LoopEventType type, LoopEvent @event){
            switch (type){
                case LoopEventType.Init:
                    this._onInit -= @event;
                    break;
                case LoopEventType.FrameUpdate:
                    this._onFrameTick -= @event;
                    break;
                case LoopEventType.Update:
                    this._onTick -= @event;
                    break;
                case LoopEventType.FixedIntervalUpdate:
                    this._onFixedIntervalTick -= @event;
                    break;
                case LoopEventType.FixedUpdate:
                    this._onFixedTick -= @event;
                    break;
                case LoopEventType.LateUpdate:
                    this._onLateTick -= @event;
                    break;
                case LoopEventType.AsyncUpdate:
                    this._onAsyncTick -= @event;
                    break;
                case LoopEventType.Destroy:
                    this._onDestroy -= @event;
                    @event?.Invoke(GameLooper.DeltaTime);
                    break;
            }
        }

        /// <summary>
        /// 注册控制器
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="igbc"></param>
        public void RegisterCtrl(string guid, IGBC igbc){
            if (this.Ctrls.ContainsKey(guid)){
                Debug.LogError($"[GameLooper]Add new ctrl error,already has :{guid} , ctrl:{igbc}");
                return;
            }

            var type = igbc.GetType();
            var methods = type.GetMethods(
                                          BindingFlags.Instance |
                                          BindingFlags.Static |
                                          BindingFlags.Public |
                                          BindingFlags.NonPublic
                                         ); //| BindingFlags.DeclaredOnly
            foreach (var method in methods){
                if (System.Attribute.IsDefined(method, typeof(IGBCEventAttribute))){
                    IGBCEventAttribute igbcAttr = method.GetCustomAttribute<IGBCEventAttribute>();
                    //考虑下创建的delegate要单独管理吗
                    var @delegate = (LoopEvent)Delegate.CreateDelegate(typeof(LoopEvent), igbc, method);
                    AddEvent(guid, @delegate);
                    this.RegisterEvent(igbcAttr.EventType, @delegate);
                }
            }

            this.Ctrls.Add(guid, igbc);
            // igbc.Init(DeltaTime);//RegisterEvent时已经执行
        }

        /// <summary>s
        /// 移除控制器
        /// </summary>
        /// <param name="guid"></param>
        public void DeregisterCtrl(string guid){
            if (!this.Ctrls.ContainsKey(guid)){
                Debug.LogError($"[GameLooper]Remove new ctrl error,not has :{guid} ");
                return;
            }

            var igbc = this.Ctrls[guid];
            var type = igbc.GetType();
            var methods = type.GetMethods(
                                          BindingFlags.Instance |
                                          BindingFlags.Static |
                                          BindingFlags.Public |
                                          BindingFlags.NonPublic
                                         ); //| BindingFlags.DeclaredOnly
            foreach (var method in methods){
                if (System.Attribute.IsDefined(method, typeof(IGBCEventAttribute))){
                    IGBCEventAttribute igbcAttr = method.GetCustomAttribute<IGBCEventAttribute>();
                    if (AllEvents.TryGetValue(guid, out var list)){
                        foreach (var single in list){
                            this.DeregisterEvent(igbcAttr.EventType, single);
                        }
                    }

                    DelEvent(guid);
                }
            }

            this.Ctrls.Remove(guid);
        }

        public override void Init(){
            base.Init();
            this.GUID = Guid.NewGuid().ToString();
            GameLooper.Instance.RegisterCtrl(this.GUID, this);
        }

        public override void OnDispose(){
            if (GameLooper.IsValid){
                GameLooper.Instance.DeregisterCtrl(this.GUID);
            }
        }
    }
}