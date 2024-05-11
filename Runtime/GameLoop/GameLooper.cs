using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IG{
    using IG.Attribute;

    public sealed class GameLooper : SingletonMono<GameLooper>, IGameLoop{
        /// <summary>
        /// 异步对象间隔更新时间
        /// </summary>
        public WaitForSeconds WaitAsyncInterval;

        /// <summary>
        /// 帧更新固定间隔
        /// </summary>
        public static readonly float FrameUnitTime = 1.0f / 24.0f;

        /// <summary>
        /// 间隔更新固定间隔
        /// </summary>
        public static readonly float FixedIntervalTime = 0.5f;

        public static float GameSpeed   { get; private set; }
        public static float PreGameSpeed{ get; private set; }

        /// <summary>
        /// 游戏当前帧率
        /// </summary>
        public static int GameFrame{ get; private set; }

        /// <summary>
        /// 游戏单位时间
        /// </summary>
        public static float DeltaTime;

        /// <summary>
        /// 游戏运行时间，运行开始时开始累加
        /// </summary>
        public static float GameTime;

        /// <summary>
        /// 帧更新计算时间
        /// </summary>
        public float FrameDeltaCalTime{ get; private set; }

        /// <summary>
        /// 固定间隔更新计算时间
        /// </summary>
        public float IntervalCalTime{ get; private set; }

        public LoopEvent                OnInit             { get; private set; }
        public LoopEvent                OnFrameTick        { get; private set; }
        public LoopEvent                OnFixedIntervalTick{ get; private set; }
        public LoopEvent                OnTick             { get; private set; }
        public LoopEvent                OnFixedTick        { get; private set; }
        public LoopEvent                OnLateTick         { get; private set; }
        public LoopEvent                OnAsyncTick        { get; private set; }
        public LoopEvent                OnDestroy          { get; private set; }
        public Dictionary<string, IGBC> Ctrls              { get; private set; } = new();

        /// <summary>
        /// 所有控制器保存的GameEvent
        /// </summary>
        public Dictionary<string, List<LoopEvent>> AllEvents{ get; private set; } = new();

        private bool _initComplete = false;

        private void OnEnable(){
            if (!_initComplete){
                return;
            }

            WaitAsyncInterval = new WaitForSeconds(DeltaTime);
            StopAllCoroutines();
            StartCoroutine(this.AsyncUpdate());
        }

        private void OnDisable(){ StopAllCoroutines(); }

        protected override void OnAwake(){
            // GameFrame = GameConfig.GameFrame;//TODO:
            GameFrame = 60;
            GameSpeed = 1.0f;
            // GameUtils.SetTargetFrame(GameFrame);//TODO:
            Application.targetFrameRate = (GameFrame);
            this.RecalculateDeltaTime(GameFrame);
            _initComplete = true;
        }

        private void Update(){ this.OnTick?.Invoke(DeltaTime); }

        private void FixedUpdate(){
            this.OnFixedTick?.Invoke(DeltaTime);
            this.FrameDeltaCalTime += FrameUnitTime;
            if (this.FrameDeltaCalTime >= DeltaTime){
                this.FrameDeltaCalTime = 0.0f;
                this.OnFrameTick?.Invoke(DeltaTime);
            }

            this.IntervalCalTime += DeltaTime;
            if (this.IntervalCalTime >= FixedIntervalTime){
                this.IntervalCalTime = 0.0f;
                this.OnFixedIntervalTick?.Invoke(DeltaTime);
            }

            GameTime += DeltaTime;
        }

        private void LateUpdate(){ this.OnLateTick?.Invoke(DeltaTime); }

        IEnumerator AsyncUpdate(){
            while (Application.isPlaying){
                yield return WaitAsyncInterval;
                this.OnAsyncTick?.Invoke(DeltaTime);
            }
        }

        /// <summary>
        /// 游戏切出后台，暂停游戏
        /// </summary>
        /// <param name="isPause"></param>
        protected override void OnApplicationPause(bool isPause){
            if (isPause){
                this.GamePause();
            }
            else{
                this.GameContinue();
            }
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

        public static void SetGameSpeed(float gameSped){
            PreGameSpeed = GameSpeed;
            GameSpeed    = gameSped;
            Instance.RecalculateDeltaTime(GameFrame);
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

            var type    = igbc.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); //| BindingFlags.DeclaredOnly
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

            var igbc    = this.Ctrls[guid];
            var type    = igbc.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); //| BindingFlags.DeclaredOnly
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

        /// <summary>
        /// Single add event
        /// TODO:事件也可以单独容器管理,方便debug
        /// </summary>
        /// <param name="type"></param>
        /// <param name="event"></param>
        public void RegisterEvent(LoopEventType type, LoopEvent @event){
            switch (type){
                case LoopEventType.Init:
                    this.OnInit -= @event;
                    this.OnInit += @event;
                    @event?.Invoke(DeltaTime);
                    break;
                case LoopEventType.FrameUpdate:
                    this.OnFrameTick -= @event;
                    this.OnFrameTick += @event;
                    break;
                case LoopEventType.Update:
                    this.OnTick -= @event;
                    this.OnTick += @event;
                    break;
                case LoopEventType.FixedIntervalUpdate:
                    this.OnFixedIntervalTick -= @event;
                    this.OnFixedIntervalTick += @event;
                    break;
                case LoopEventType.FixedUpdate:
                    this.OnFixedTick -= @event;
                    this.OnFixedTick += @event;
                    break;
                case LoopEventType.LateUpdate:
                    this.OnLateTick -= @event;
                    this.OnLateTick += @event;
                    break;
                case LoopEventType.AsyncUpdate:
                    this.OnAsyncTick -= @event;
                    this.OnAsyncTick += @event;
                    break;
                case LoopEventType.Destroy: //实际上只是为了Ctrl方便使用
                    this.OnDestroy -= @event;
                    this.OnDestroy += @event;
                    break;
            }
        }

        /// <summary>
        /// Single remove event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="event"></param>
        public void DeregisterEvent(LoopEventType type, LoopEvent @event){
            switch (type){
                case LoopEventType.Init:
                    this.OnInit -= @event;
                    break;
                case LoopEventType.FrameUpdate:
                    this.OnFrameTick -= @event;
                    break;
                case LoopEventType.Update:
                    this.OnTick -= @event;
                    break;
                case LoopEventType.FixedIntervalUpdate:
                    this.OnFixedIntervalTick -= @event;
                    break;
                case LoopEventType.FixedUpdate:
                    this.OnFixedTick -= @event;
                    break;
                case LoopEventType.LateUpdate:
                    this.OnLateTick -= @event;
                    break;
                case LoopEventType.AsyncUpdate:
                    this.OnAsyncTick -= @event;
                    break;
                case LoopEventType.Destroy: //实际上只是为了Ctrl方便使用
                    this.OnDestroy -= @event;
                    @event?.Invoke(DeltaTime);
                    break;
            }
        }

        public void RecalculateDeltaTime(float targetFrame){ DeltaTime = (float)Math.Round(1.0f / targetFrame * GameSpeed, 3); }

        public override void OnDispose(){
            GameFrame = 0;
            GameSpeed = 0;
            // GameUtils.SetTargetFrame(GameFrame);//TODO:
            Application.targetFrameRate = (GameFrame);
            this.RecalculateDeltaTime(GameFrame);
            _initComplete = false;
        }

        /// <summary>
        /// 游戏暂停
        /// 当前仅用于后台中
        /// </summary>
        public void GamePause(){
            // GameUtils.CleanMemory(); //TODO:
            // GameUtils.SetTargetFrame(0);//TODO:
            PreGameSpeed = GameSpeed;
            GameSpeed    = 0.0f;
        }

        /// <summary>
        /// 继续游戏
        /// 当前仅用于后台中
        /// </summary>
        public void GameContinue(){
            GameSpeed    = PreGameSpeed <= 0 ? 1 : PreGameSpeed;
            PreGameSpeed = 0.0f;
            RecalculateDeltaTime(GameFrame);
            // GameUtils.SetTargetFrame(GameFrame);//TODO:
        }
    }
}