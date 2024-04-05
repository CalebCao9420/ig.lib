using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IG{

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

        public static float GameSpeed{ get; private set; }
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

        public GameEvent OnInit{ get; private set; }
        public GameEvent OnFrameTick{ get; private set; }
        public GameEvent OnFixedIntervalTick{ get; private set; }
        public GameEvent OnTick{ get; private set; }
        public GameEvent OnFixedTick{ get; private set; }
        public GameEvent OnLateTick{ get; private set; }
        public GameEvent OnAsyncTick{ get; private set; }
        public Dictionary<string, IGBC> Ctrls = new Dictionary<string, IGBC>();

        private void OnEnable(){
            StopAllCoroutines();
            StartCoroutine(this.AsyncUpdate());
        }

        private void OnDisable(){ StopAllCoroutines(); }

        public static void SetGameSpeed(float gameSped){
            PreGameSpeed = GameSpeed;
            GameSpeed = gameSped;
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

            // this.OnInit += igbc.OnInit;
            // this.OnFrameUpdate += igbc.OnFrameUpdate;
            // this.OnUpdate += igbc.OnUpdate;
            // this.OnFixedUpdate += igbc.OnFixedUpdate;
            // this.OnLateUpdate += igbc.OnLateUpdate;
            this.RegisterEvent(GameEventType.Init, igbc.Init);
            this.RegisterEvent(GameEventType.Update, igbc.Tick);
            this.RegisterEvent(GameEventType.FixedIntervalUpdate, igbc.FixedIntervalTick);
            this.RegisterEvent(GameEventType.FixedUpdate, igbc.FixedTick);
            this.RegisterEvent(GameEventType.FrameUpdate, igbc.FrameTick);
            this.RegisterEvent(GameEventType.LateUpdate, igbc.LateTick);
            this.RegisterEvent(GameEventType.AsyncUpdate, igbc.AsyncTick);
            this.Ctrls.Add(guid, igbc);
            // //TODO:每次注册事件，都会响应所有Init是不正确的
            // this.OnInit?.Invoke(DeltaTime);
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

            var igbc = this.Ctrls[guid];
            this.OnInit -= igbc.Init;
            this.OnFrameTick -= this.OnFrameTick;
            this.OnTick -= igbc.Tick;
            this.OnFixedTick -= igbc.FixedTick;
            this.OnFixedIntervalTick -= igbc.FixedIntervalTick;
            this.OnLateTick -= igbc.LateTick;
            this.OnAsyncTick -= igbc.AsyncTick;
            this.Ctrls.Remove(guid);
        }

        /// <summary>
        /// Single add event
        /// TODO:事件也要容器单独管理才行
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

            // //TODO:每次注册事件，都会响应所有Init是不正确的。
            // this.OnInit?.Invoke(DeltaTime);
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

        public override void OnDispose(){ }

        /// <summary>
        /// 游戏暂停
        /// 当前仅用于后台中
        /// </summary>
        public void GamePause(){
            // GameUtils.CleanMemory(); //TODO:
            // GameUtils.SetTargetFrame(0);//TODO:
            PreGameSpeed = GameSpeed;
            GameSpeed = 0.0f;
        }

        /// <summary>
        /// 继续游戏
        /// 当前仅用于后台中
        /// </summary>
        public void GameContinue(){
            GameSpeed = PreGameSpeed <= 0 ? 1 : PreGameSpeed;
            PreGameSpeed = 0.0f;
            RecalculateDeltaTime(GameFrame);
            // GameUtils.SetTargetFrame(GameFrame);//TODO:
        }
    }
}