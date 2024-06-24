using System;
using System.Collections.Generic;
using IG.Runtime.Log;
using UnityEngine;

namespace IG.Runtime.Common.FSM{
    /// <summary>
    /// 状态机
    /// </summary>
    /// <typeparam name="TStatus">[State的状态类型]</typeparam>
    /// <typeparam name="TState">[State的类型]</typeparam>
    public abstract class FSM<TStatus, TState> : IDisposable{
        public            FSMState<TStatus, TState>            CurrentState{ get; private set; }
        protected         List<FSMTransition<TStatus, TState>> _Transitions = new List<FSMTransition<TStatus, TState>>();
        protected         List<FSMState<TStatus, TState>>      _States      = new List<FSMState<TStatus, TState>>();
        protected         FSMTransition<TStatus, TState>       GetTransition(TState stateId){ return _Transitions.Find(x => x.EqualCurrentState(stateId)); }
        protected         FSMState<TStatus, TState>            GetState(TState      state)  { return _States.Find(x => x.State.Equals(state)); }
        protected virtual void                                 OnDispose()                  { }

        protected virtual void NextState(){
            if (CurrentState != null){
                CurrentState.Leave();
                var nextState = GetNextState(CurrentState);
                CurrentState = nextState;
                if (nextState != null){
                    nextState.Enter();
                    if (nextState.IsComplete){
                        NextState();
                    }
                }
            }
        }

        private FSMState<TStatus, TState> GetNextState(FSMState<TStatus, TState> cur){
            var status     = cur.Status;
            var transition = GetTransition(cur.State);
            if (transition == null){
                LogHelper.Log("Transition not found, already last state!", LogType.Error);
                Dispose();
                return null;
            }

            var nextStateId = transition.Translate(status);
            Debug.Assert(!nextStateId.Equals(default(TState)), $"{cur.GetType().Name} status [{status}]转换规则不存在");
            var nextState = GetState(nextStateId);
            Debug.Assert(nextState != null, $"状态[{nextStateId}]实例不存在");
            return nextState;
        }

        public void AddTransition(FSMTransition<TStatus, TState> transition){ _Transitions.Add(transition); }
        public void AddState(FSMState<TStatus, TState>           state)     { _States.Add(state); }

        public void Dispose(){
            _States.ForEach(x => x.Dispose());
            _States.Clear();
            CurrentState = null;
            OnDispose();
        }

        public virtual void Tick(float deltaTime){
            if (CurrentState != null){
                if (CurrentState.IsComplete){
                    NextState();
                }
                else{
                    CurrentState.Tick(deltaTime);
                }
            }
        }

        public virtual void Setup(TState state,IFSMStateParam param = null){
            if (CurrentState != null){
                CurrentState.Leave();
            }

            CurrentState = GetState(state);
            CurrentState.Enter(param);
        }
    }
}