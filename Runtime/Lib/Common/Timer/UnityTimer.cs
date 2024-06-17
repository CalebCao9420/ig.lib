using System;
using System.Collections.Generic;
using UnityEngine;

namespace IG.Runtime.Common.Timer{
    public sealed class UnityTimer{
        internal class TimerManager : LooperBase<TimerManager>{
            private List<UnityTimer> _timers      = new();
            private List<UnityTimer> _timersToAdd = new();
            private bool             _hasChanged  = false;

            internal void Add(UnityTimer timer){
                _timersToAdd.Add(timer);
                _hasChanged = true;
            }

            internal void StopAll(){
                _timers.ForEach(x => x.Stop());
                _timersToAdd.ForEach(x => x.Stop());
            }

            internal void PauseAll(){
                _timers.ForEach(x => x.Pause());
                _timersToAdd.ForEach(x => x.Pause());
            }

            internal void UnPauseAll(){
                _timers.ForEach(x => x.UnPause());
                _timersToAdd.ForEach(x => x.UnPause());
            }

            protected override bool Tick(float deltaTime){
                Update(deltaTime);
                return base.Tick(deltaTime);
            }

            private void Update(float deltaTime){
                if (_hasChanged){
                    _timers.AddRange(_timersToAdd);
                    _timersToAdd.Clear();
                    _hasChanged = false;
                }

                var dt         = deltaTime;
                var dtUnscaled = Time.unscaledDeltaTime;
                foreach (var timer in _timers){
                    var t = timer.UseScaledTime ? dt : dtUnscaled;
                    timer.Tick(t);
                }

                for (int i = _timers.Count - 1; i >= 0; i--){
                    var timer = _timers[i];
                    if (timer.IsFinished || timer.IsStopped){
                        _timers.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 指定的秒数后调用函数或计算表达式
        /// </summary>
        /// <param name="interval">秒</param>
        /// <param name="onTime"></param>
        /// <param name="useScaleTime"></param>
        /// <returns></returns>
        public static UnityTimer SetTimeout(float interval, Action onTime, bool useScaleTime = false){ return SetInterval(interval, onTime, 1, useScaleTime); }

        /// <summary>
        /// 按照指定的周期（以秒计）来调用函数或计算表达式
        /// </summary>
        /// <param name="interval">秒</param>
        /// <param name="onTime"></param>
        /// <param name="repeatCount"></param>
        /// <param name="useSaledTime"></param>
        /// <returns></returns>
        public static UnityTimer SetInterval(float interval, Action onTime, int repeatCount = -1, bool useSaledTime = false){
            if (s_timerManager == null){
                // timerManager = new GameObject("TimerManager").AddComponent<TimerManager>();
                s_timerManager = TimerManager.Instance;
            }

            var timer = new UnityTimer();
            timer.Interval      = interval;
            timer.RepeatCount   = repeatCount < 0 ? int.MaxValue : repeatCount;
            timer.UseScaledTime = useSaledTime;
            timer.OnTime        = onTime;
            s_timerManager.Add(timer);
            return timer;
        }

        /// <summary>
        /// 停止所有定时器
        /// </summary>
        public static void StopAll(){
            if (s_timerManager != null){
                s_timerManager.StopAll();
            }
        }

        /// <summary>
        /// 暂停所有定时器
        /// </summary>
        public static void PauseAll(){
            if (s_timerManager != null){
                s_timerManager.PauseAll();
            }
        }

        /// <summary>
        /// 恢复所有定时器
        /// </summary>
        public static void UnPauseAll(){
            if (s_timerManager != null){
                s_timerManager.UnPauseAll();
            }
        }

        private static TimerManager s_timerManager;
        private        float        _currentTime;
        public         Action       OnTime       { get; private set; }
        public         bool         IsStopped    { get; private set; }
        public         bool         IsPaused     { get; private set; }
        public         bool         IsFinished   { get; private set; }
        public         bool         UseScaledTime{ get; private set; }
        public         int          RepeatCount  { get; private set; }
        public         int          CurrentCount { get; private set; }
        public         float        Interval     { get; private set; }
        private UnityTimer(){ }
        public void Pause()                       { IsPaused  = true; }
        public void UnPause()                     { IsPaused  = false; }
        public void Stop()                        { IsStopped = true; }
        public void UpdateInterval(float interval){ Interval  = interval; }

        internal void Tick(float deltaTime){
            if (!IsStopped && !IsPaused && !IsFinished){
                _currentTime += deltaTime;
                if (_currentTime > Interval){
                    if (CurrentCount < RepeatCount){
                        try{
                            OnTime.Invoke();
                        }
                        catch (Exception e){
                            Debug.LogException(e);
                        }
                    }

                    var dt = _currentTime % Interval;
                    _currentTime =  dt;
                    CurrentCount += 1;
                    if (CurrentCount >= RepeatCount){
                        IsFinished = true;
                    }
                }
            }
        }
    }
}