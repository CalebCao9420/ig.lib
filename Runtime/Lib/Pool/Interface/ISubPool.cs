using System;
using System.Collections.Generic;
using UnityEngine;

namespace IG{
    public struct AsyncReqParam{
    #region Req pool

        private static Stack<AsyncReqParam>   s_CACHE = new();
        private static HashSet<AsyncReqParam> s_USING = new();

        public static AsyncReqParam CreateReq(Type resourceType, string name, SubPoolEvent onCompleted){
            AsyncReqParam rel;
            if (s_CACHE.Count > 0){
                rel = s_CACHE.Pop();
            }
            else{
                rel = new AsyncReqParam();
            }

            s_USING.Add(rel);
            return rel.Init(resourceType, name, onCompleted);
        }

        public static void RecycleReq(AsyncReqParam req){
            req.Reset();
            if (false == s_USING.Remove(req)){
                throw new ArgumentException($"[ObjectPool][AsycnParamPool] : Input list doesn't contain the same list : {req}");
            }

            s_CACHE.Push(req);
        }

    #endregion

        public Type         SourceType;
        public string       Name;
        public SubPoolEvent OnCompleted;

        public void Reset(){
            this.SourceType  = typeof(GameObject);
            this.Name        = string.Empty;
            this.OnCompleted = null;
        }

        private AsyncReqParam Init(Type resourceType, string name, SubPoolEvent onCompleted){
            this.SourceType  = resourceType;
            this.Name        = name;
            this.OnCompleted = onCompleted;
            return this;
        }

        private AsyncReqParam(Type resourceType, string name, SubPoolEvent onCompleted){
            this.SourceType  = resourceType;
            this.Name        = name;
            this.OnCompleted = onCompleted;
        }
    }

    public delegate void SubPoolEvent(UnityEngine.Object obj);

    public interface ISubPool{
        Type               SourceType{ get; }
        string             Name      { get; }
        string             Path      { get; }
        void               Clear(bool cleanMemory = false);
        void               Release();
        UnityEngine.Object Spawn();
        void               Return(UnityEngine.Object obj);
        bool               SpawnAsync(AsyncReqParam  param);
    }
}