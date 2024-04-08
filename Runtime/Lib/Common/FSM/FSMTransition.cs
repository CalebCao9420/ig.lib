using System.Collections.Generic;

namespace IG.Runtime.Common.FSM{
    /// <summary>
    /// 设置状态转换关系
    /// </summary>
    /// <typeparam name="TStatus">[State的状态类型]</typeparam>
    /// <typeparam name="TState">[State的类型]</typeparam>
    public abstract class FSMTransition<TStatus, TState>{
        private Dictionary<TStatus, TState> _mapping = new Dictionary<TStatus, TState>();
        private TState                      _currentStateId;
        private TState                      _defaultNextStateId;
        public FSMTransition(TState          currentStateId){ this._currentStateId = currentStateId; }
        public bool EqualCurrentState(TState inputState){ return _currentStateId.Equals(inputState); }

        public FSMTransition<TStatus, TState> AddTransform(TStatus status, TState stateId){
            _mapping.Add(status, stateId);
            return this;
        }

        public FSMTransition<TStatus, TState> SetDefaultTransform(TState stateId){
            _defaultNextStateId = stateId;
            return this;
        }

        public virtual TState Translate(TStatus status){
            if (_mapping.TryGetValue(status,out var result)){
                return result;
            }

            return _defaultNextStateId;
        }
    }
}