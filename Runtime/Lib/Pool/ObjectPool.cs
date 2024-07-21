﻿using System;
using System.Collections;
using System.Collections.Generic;
using IG.Runtime.Log;
using IG.Runtime.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Pool{
    public class ObjectPool : SingletonMono<ObjectPool>{
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
        private Dictionary<PoolResourceType, Dictionary<string, ISubPool>> _pool;

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

        public T Spawn<T>(string path) where T : UnityEngine.Object{
            PoolResourceType resType  = PoolResourceType.GameObject;
            Type             hookType = typeof(T);
            if (hookType == typeof(GameObject)){
                resType = PoolResourceType.GameObject;
            }
            else if (hookType == typeof(AudioClip)){
                resType = PoolResourceType.Audio;
            }
            else if (hookType == typeof(Texture)){
                resType = PoolResourceType.Texture;
            }

            return Spawn(resType, path) as T;
        }

        public Object Spawn(PoolResourceType resourceType, string path){
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

        public void SpawnAsync(PoolResourceType resourceType, string name, SubPoolEvent onCompleted){
            if (string.IsNullOrEmpty(name)){
                return;
            }

            this._asyncQue.Enqueue(AsyncReqParam.CreateReq(resourceType, name, onCompleted));
        }

        private void AsyncLoad(AsyncReqParam param){
            PoolResourceType resourceType = param.SourceType;
            string           name         = param.Name;
            //传递完就回收了
            AsyncReqParam.RecycleReq(param);
            HookSubPool(resourceType, name, out ISubPool subPool);
            subPool.SpawnAsync(param);
        }

        public void Recycle(PoolResourceType type, Transform root){
            for (int i = root.childCount - 1; i >= 0; i--){
                Recycle(type, root.GetChild(i).gameObject);
            }
        }

        public void Recycle(PoolResourceType type, GameObject obj){
            if (!Application.isPlaying || obj == null){
                return;
            }

            string objName = obj.name;
            if (_pool.TryGetValue(type, out var pools)){
                if (pools.TryGetValue(objName, out var subPool)){
                    subPool.Recycle(obj);
                }
            }
            else{
                GameObjUtils.DestroyObj(obj);
            }
        }

        public void Clear(){
            if (_pool.Count <= 0){
                return;
            }

            PoolRoot.transform.DestroyKids();
            this._pool.Clear();
        }

        private void HookSubPool(PoolResourceType resourceType, string name, out ISubPool subPool){
            bool exist = _pool.TryGetValue(resourceType, out Dictionary<string, ISubPool> pools);
            if (exist && pools.TryGetValue(name, out var pool)){
                subPool = pool;
            }
            else{
                subPool = RegisterSubPool(resourceType, name);
                if (subPool == null){
                    this.Log($"错误GameObject名:{name}", LogType.Error);
                }

                pools.Add(name, subPool);
            }

            //ReAdd
            if (!exist){
                _pool.Add(resourceType, new Dictionary<string, ISubPool>(){ { name, subPool } });
            }
        }

        private ISubPool RegisterSubPool(PoolResourceType type, string path){ return PoolHelper.CreateSubPool(type, path); }
    }
}