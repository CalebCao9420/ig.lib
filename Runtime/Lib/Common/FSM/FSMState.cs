using System;

namespace IG.Runtime.Common.FSM{
    /// <summary>
    /// 状态类型
    /// </summary>
    /// <typeparam name="TStatus">[State的状态类型]</typeparam>
    /// <typeparam name="TState">[State的类型]</typeparam>
    public abstract class FSMState<TStatus, TState> : IDisposable{
        public TStatus Status    { get; protected set; }
        public TState  State     { get; private set; }
        
        /// <summary>
        /// Complete Condition
        /// </summary>
        public bool    IsComplete{ get; protected set; }
        public FSMState(TState state){ this.State = state; }

        public bool Enter(IFSMStateParam param = null){
            return this.OnEnter(param);
        }

        public bool Leave(){ return this.OnLeave(); }

        public bool Tick(float deltaTime){
            return this.OnTick(deltaTime);
        }

        public abstract void Dispose();

        protected virtual bool OnEnter(IFSMStateParam param = null){
            IsComplete = false;
            return true;
        }

        protected virtual bool OnLeave()              { return true; }
        protected virtual bool OnTick(float deltaTime){ return true; }
    }
}