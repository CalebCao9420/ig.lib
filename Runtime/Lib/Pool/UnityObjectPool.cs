using System;
using System.Collections;
using System.Collections.Generic;
using IG.Runtime.Log;
using IG.Runtime.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Pool{
    public class UnityObjectPool : SingletonMono<UnityObjectPool>{
        public override void OnDispose(){ }

        public static GameObject PoolRoot{ get; private set; }
        /// <summary>
        /// 清理对象池等待时间
        /// </summary>
        // public WaitForSeconds WaitTime = new WaitForSeconds(GameConfig.CleanObjectPoolCacheTime);

        /// <summary>
        /// 异步对象间隔更新时间
        /// </summary>
        public WaitForSeconds WaitAsyncInterval = new WaitForSeconds(GameLooper.DeltaTime);

        /// <summary>
        /// 总池
        /// </summary>
        private Dictionary<Type, Dictionary<string, ISubPool>> _pool;

        /// <summary>
        /// 异步加载队列
        /// </summary>
        private readonly Queue<AsyncReqParam> _asyncQue = new Queue<AsyncReqParam>();

        IEnumerator AsyncUpdate(){
            while (Application.isPlaying){
                yield return WaitAsyncInterval;
                if (this._asyncQue.Count > 0){
                    this.AsyncLoad(this._asyncQue.Dequeue());
                }
            }
        }

        /// <summary>
        /// 初始化初始所有的预制体，放在load里边
        /// 对象池按时间自动回收，不走事件
        /// </summary>
        /// <returns></returns>
        public override void Init(){
            PoolRoot                    = this.gameObject;
            this._pool                  = new();
            PoolRoot.transform.position = Vector3.one * -1000.0f;

            //开启Async间隔
            StopAllCoroutines();
            StartCoroutine(this.AsyncUpdate());
        }

        public T Spawn<T>(string path) where T : UnityEngine.Object{ return Spawn(typeof(T), path) as T; }

        public Object Spawn(Type resourceType, string path){
            if (string.IsNullOrEmpty(path)){
                return null;
            }

            Object result = null;
            HookSubPool(resourceType, path, out ISubPool subPool);
            result = subPool.Spawn();
            if (result == null){
                this.Log($"Spawn obj error = {result} , sourceType = {resourceType} ,name = {path}", LogType.Error);
                // return this.Spawn(resourceType, name, parent, isActive);
            }

            return result;
        }

        public void SpawnAsync(Type resourceType, string name, SubPoolEvent onCompleted){
            if (string.IsNullOrEmpty(name)){
                return;
            }

            this._asyncQue.Enqueue(AsyncReqParam.CreateReq(resourceType, name, onCompleted));
        }

        private void AsyncLoad(AsyncReqParam param){
            Type   resourceType = param.SourceType;
            string name         = param.Name;
            //传递完就回收了
            AsyncReqParam.RecycleReq(param);
            HookSubPool(resourceType, name, out ISubPool subPool);
            subPool.SpawnAsync(param);
        }

        public void Return<T>(T obj) where T : UnityEngine.Object{
            Type type = typeof(T);
            this.Return(type,obj);
        }

        public void Return(Type type, Object obj){
            if (!Application.isPlaying || obj == null){
                return;
            }

            string objName = obj.name;
            if (_pool.TryGetValue(type, out var pools)){
                if (pools.TryGetValue(objName, out var subPool)){
                    subPool.Return(obj);
                    if (obj is GameObject gObj){
                        gObj.transform.SetParent(PoolRoot.transform);
                    }
                }
            }
            else{
                GameObjUtils.DestroyObj(obj);
            }
        }

        /// <summary>
        /// 按prefab添加子池 
        /// </summary>
        public void PushPrefab(Type type, GameObject prefab){
            if (!Application.isPlaying || prefab == null){
                return;
            }

            string objName = prefab.name;
            if (_pool.TryGetValue(type, out var pools)){
                if (pools.TryGetValue(objName, out var subPool)){
                    subPool.Return(prefab);
                    prefab.transform.SetParent(PoolRoot.transform);
                }
                else{
                    subPool = RegisterSubPool(type,prefab);
                    pools.ReAdd(objName, subPool);
                }
            }
            //无子池则创建子池
            else{
                HookSubPool(type,prefab,out var subPool);
            }
        }

        /// <summary>
        /// 按prefab释放子池 
        /// </summary>
        public void ReleaseSubPool(Type type, GameObject prefab){
            if (!Application.isPlaying || prefab == null){
                return;
            }

            string objName = prefab.name;
            if (_pool.TryGetValue(type, out var pools)){
                if (pools.TryGetValue(objName, out var subPool)){
                    subPool.Release();
                    subPool = null;
                    pools.Remove(objName);
                }
            }
        }

        public void Clear(){
            if (_pool.Count <= 0){
                return;
            }

            PoolRoot.transform.DestroyKids();
            this._pool.Clear();
        }

        private void HookSubPool(Type resourceType, string name, out ISubPool subPool){
            bool     exist    = _pool.TryGetValue(resourceType, out Dictionary<string, ISubPool> pools);
            ISubPool pool     = null;
            bool     existSub = exist && pools.TryGetValue(name, out pool);
            if (existSub){
                subPool = pool;
            }
            else{
                subPool = RegisterSubPool(resourceType, name);
                if (subPool == null){
                    this.Log($"错误GameObject名:{name}", LogType.Error);
                }
            }

            //ReAdd
            if (exist){
                pools.ReAdd(name, subPool);
            }
            else{
                _pool.Add(resourceType, new Dictionary<string, ISubPool>(){ { name, subPool } });
            }
        }
        
        private void HookSubPool(Type resourceType, UnityEngine.Object prefab, out ISubPool subPool){
            string   name     = prefab.name;
            bool     exist    = _pool.TryGetValue(resourceType, out Dictionary<string, ISubPool> pools);
            ISubPool pool     = null;
            bool     existSub = exist && pools.TryGetValue(name, out pool);
            if (existSub){
                subPool = pool;
            }
            else{
                subPool = RegisterSubPool(resourceType, prefab);
                if (subPool == null){
                    this.Log($"错误GameObject名:{name}", LogType.Error);
                }
            }

            //ReAdd
            if (exist){
                pools.ReAdd(name, subPool);
            }
            else{
                _pool.Add(resourceType, new Dictionary<string, ISubPool>(){ { name, subPool } });
            }
        }

        private ISubPool RegisterSubPool(Type type, string             path)  { return PoolHelper.CreateSubPool(type, path); }
        private ISubPool RegisterSubPool(Type type, UnityEngine.Object prefab){ return PoolHelper.CreateSubPool(type, prefab); }
    }
}