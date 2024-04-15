using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace IG{
    using IG.Attribute;

    public class GameLooper : SingletonMono<GameLooper>, IGameLoop{
        /// <summary>
        /// 异步对象间隔更新时间
        /// </summary>
        public WaitForSeconds WaitAsyncInterval = new WaitForSeconds(DeltaTime);

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

        public  GameEvent                OnInit             { get; private set; }
        public  GameEvent                OnFrameTick        { get; private set; }
        public  GameEvent                OnFixedIntervalTick{ get; private set; }
        public  GameEvent                OnTick             { get; private set; }
        public  GameEvent                OnFixedTick        { get; private set; }
        public  GameEvent                OnLateTick         { get; private set; }
        public  GameEvent                OnAsyncTick        { get; private set; }
        public  Dictionary<string, IGBC> Ctrls         = new Dictionary<string, IGBC>();
        private bool                     _initComplete = false;

        private void OnEnable(){
            if (!_initComplete){
                return;
            }

            StopAllCoroutines();
            StartCoroutine(this.AsyncUpdate());
        }

        private void OnDisable(){ StopAllCoroutines(); }

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

            var                type      = igbc.GetType();
            IGBCEventAttribute attribute = type.GetCustomAttribute<IGBCEventAttribute>();
            if ((attribute.EventType & GameEventType.Init) != 0){ this.RegisterEvent(GameEventType.Init, igbc.Init); }

            if ((attribute.EventType & GameEventType.Update) != 0){ this.RegisterEvent(GameEventType.Update, igbc.Tick); }

            if ((attribute.EventType & GameEventType.FixedIntervalUpdate) != 0){ this.RegisterEvent(GameEventType.FixedIntervalUpdate, igbc.FixedIntervalTick); }

            if ((attribute.EventType & GameEventType.FixedUpdate) != 0){ this.RegisterEvent(GameEventType.FixedUpdate, igbc.FixedTick); }

            if ((attribute.EventType & GameEventType.FrameUpdate) != 0){ this.RegisterEvent(GameEventType.FrameUpdate, igbc.FrameTick); }

            if ((attribute.EventType & GameEventType.LateUpdate) != 0){ this.RegisterEvent(GameEventType.LateUpdate, igbc.LateTick); }

            if ((attribute.EventType & GameEventType.AsyncUpdate) != 0){ this.RegisterEvent(GameEventType.AsyncUpdate, igbc.AsyncTick); }

            this.Ctrls.Add(guid, igbc);
            igbc.Init(DeltaTime);
        }

        /// <summary>
        /// 移除控制器
        /// </summary>
        /// <param name="guid"></param>
        public void DeregisterCtrl(string guid){
            if (!this.Ctrls.ContainsKey(guid)){
                Debug.LogError($"[GameLooper]Remove new ctrl error,not has :{guid} ");
                return;
            }

            var                igbc      = this.Ctrls[guid];
            var                type      = igbc.GetType();
            IGBCEventAttribute attribute = type.GetCustomAttribute<IGBCEventAttribute>();
            if ((attribute.EventType & GameEventType.Init) != 0){ this.DeregisterEvent(GameEventType.Init, igbc.Init); }

            if ((attribute.EventType & GameEventType.Update) != 0){ this.DeregisterEvent(GameEventType.Update, igbc.Tick); }

            if ((attribute.EventType & GameEventType.FixedIntervalUpdate) != 0){ this.DeregisterEvent(GameEventType.FixedIntervalUpdate, igbc.FixedIntervalTick); }

            if ((attribute.EventType & GameEventType.FixedUpdate) != 0){ this.DeregisterEvent(GameEventType.FixedUpdate, igbc.FixedTick); }

            if ((attribute.EventType & GameEventType.FrameUpdate) != 0){ this.DeregisterEvent(GameEventType.FrameUpdate, igbc.FrameTick); }

            if ((attribute.EventType & GameEventType.LateUpdate) != 0){ this.DeregisterEvent(GameEventType.LateUpdate, igbc.LateTick); }

            if ((attribute.EventType & GameEventType.AsyncUpdate) != 0){ this.DeregisterEvent(GameEventType.AsyncUpdate, igbc.AsyncTick); }

            this.Ctrls.Remove(guid);
        }

        /// <summary>
        /// Single add event
        /// TODO:事件也可以单独容器管理,方便debug
        /// </summary>
        /// <param name="type"></param>
        /// <param name="event"></param>
        public void RegisterEvent(GameEventType type, GameEvent @event){
            switch (type){
                case GameEventType.Init:
                    this.OnInit -= @event;
                    this.OnInit += @event;
                    @event?.Invoke(DeltaTime);
                    break;
                case GameEventType.FrameUpdate:
                    this.OnFrameTick -= @event;
                    this.OnFrameTick += @event;
                    break;
                case GameEventType.Update:
                    this.OnTick -= @event;
                    this.OnTick += @event;
                    break;
                case GameEventType.FixedIntervalUpdate:
                    this.OnFixedIntervalTick -= @event;
                    this.OnFixedIntervalTick += @event;
                    break;
                case GameEventType.FixedUpdate:
                    this.OnFixedTick -= @event;
                    this.OnFixedTick += @event;
                    break;
                case GameEventType.LateUpdate:
                    this.OnLateTick -= @event;
                    this.OnLateTick += @event;
                    break;
                case GameEventType.AsyncUpdate:
                    this.OnAsyncTick -= @event;
                    this.OnAsyncTick += @event;
                    break;
            }
        }

        /// <summary>
        /// Single remove event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="event"></param>
        public void DeregisterEvent(GameEventType type, GameEvent @event){
            switch (type){
                case GameEventType.Init:
                    this.OnInit -= @event;
                    break;
                case GameEventType.FrameUpdate:
                    this.OnFrameTick -= @event;
                    break;
                case GameEventType.Update:
                    this.OnTick -= @event;
                    break;
                case GameEventType.FixedIntervalUpdate:
                    this.OnFixedIntervalTick -= @event;
                    break;
                case GameEventType.FixedUpdate:
                    this.OnFixedTick -= @event;
                    break;
                case GameEventType.LateUpdate:
                    this.OnLateTick -= @event;
                    break;
                case GameEventType.AsyncUpdate:
                    this.OnAsyncTick -= @event;
                    break;
            }
        }

        public void RecalculateDeltaTime(float targetFrame){ DeltaTime = (float)Math.Round(1.0f / targetFrame * GameSpeed, 3); }

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