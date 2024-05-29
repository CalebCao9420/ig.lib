using System.Collections.Generic;

namespace IG.Runtime.Common.FSM{
    /// <summary>
    /// 设置状态转换关系
    /// </summary>
    /// <typeparam name="TStatus">[State的状态类型]</typeparam>
    /// <typeparam name="TState">[State的类型]</typeparam>
    public abstract class FSMTransition<TStatus, TState>{
        private Dictionary<TStatus, TState> _mapping = new Dictionary<TStatus, TState>();
        private TState                      _actAffectType;
        private TState                      _defaultNextStateId;
        public FSMTransition(TState          actAffectType){ this._actAffectType = actAffectType; }
        public bool EqualCurrentState(TState inputState){ return _actAffectType.Equals(inputState); }

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