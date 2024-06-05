using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace IG{
    /// <summary>
    /// 对象池物体对象类型
    /// 新增特殊对象在这里添加新类型
    /// </summary>
    public enum PoolResourceType{
        /// <summary>
        /// 公共Gameobject对象池
        /// </summary>
        GameObject,

        /// <summary>
        /// 公共音效对象池
        /// </summary>
        Audio,

        /// <summary>
        /// 公共贴图对象池
        /// </summary>
        Texture,
    }

    public struct AsyncReqParam{
    #region Req pool

        private static Stack<AsyncReqParam> s_CACHE = new Stack<AsyncReqParam>();

        public static AsyncReqParam CreateReq(PoolResourceType resourceType, string name, SubPoolEvent onCompleted){
            AsyncReqParam rel;
            if (s_CACHE.Count > 0){
                rel = s_CACHE.Pop();
            }
            else{
                rel = new AsyncReqParam();
            }

            return rel.Init(resourceType, name, onCompleted);
        }

        public static void RecycleReq(AsyncReqParam req){
            req.Reset();
            s_CACHE.Push(req);
        }

    #endregion

        public PoolResourceType SourceType;
        public string           Name;
        public SubPoolEvent     OnCompleted;

        public void Reset(){
            this.SourceType  = PoolResourceType.GameObject;
            this.Name        = string.Empty;
            this.OnCompleted = null;
        }

        private AsyncReqParam Init(PoolResourceType resourceType, string name, SubPoolEvent onCompleted){
            this.SourceType  = resourceType;
            this.Name        = name;
            this.OnCompleted = onCompleted;
            return this;
        }

        private AsyncReqParam(PoolResourceType resourceType, string name, SubPoolEvent onCompleted){
            this.SourceType  = resourceType;
            this.Name        = name;
            this.OnCompleted = onCompleted;
        }
    }

    public delegate void SubPoolEvent(UnityEngine.Object obj);

    public interface ISubPool{
        string             Name    { get; }
        string             Path    { get; }
        PoolResourceType   PoolType{ get; }
        void               Recycle(UnityEngine.Object obj);
        void               Clear(bool                 cleanMemory = false);
        UnityEngine.Object Spawn();
        bool               SpawnAsync(AsyncReqParam param);
    }
}