using System;
using System.Collections.Generic;
using IG.AssetBundle;
using IG.Runtime.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Pool.SubPool{
    public class GameObjectSubPool : ISubPool, IDisposable{
        public    Type            SourceType{ get; } = typeof(GameObject);
        public    string          Name      { get{ return this._Path; } }
        public    string          Path      { get{ return this._Path; } }
        protected Object          _Original;
        protected string          _Key;  //类似于Bundle
        protected string          _Path; //类似于Bundle
        protected Stack<Object>   _Pool;
        protected HashSet<Object> _Using;
        protected GameObjectSubPool(){ }

        /// <summary>
        ///按Bundle注册子池 
        /// </summary>
        public GameObjectSubPool(string path, Object original = null){
            this._Path = path;
            AssetsSystem.HookBundleAndAssetName(path, out string notDo, out this._Key);
            this._Original = original;
            this._Pool     = new();
            this._Using    = new();
        }

        /// <summary>
        /// 按预置注册子池
        /// </summary>
        public GameObjectSubPool(Object original){
            this._Original = original;
            this._Path     = original.name;
            this._Pool     = new();
            this._Using    = new();
        }

        public UnityEngine.Object Spawn(){ return Spawn(null, true); }

        public Object Spawn(Transform parent = null, bool isActive = true){
            GameObject rel;
            if (_Pool.Count > 0){
                rel = _Pool.Pop() as GameObject;
                OnSpawnSuccess(rel, parent, isActive);
            }
            else{
                if (this._Original == null){
                    this._Original = this.ReloadPrefab(this.Path);
                }

                rel      = UnityEngine.Object.Instantiate(this._Original) as GameObject;
                rel.name = this.Name;
                OnSpawnSuccess(rel, parent, isActive);
            }

            _Using.Add(rel);
            return rel;
        }

        public bool SpawnAsync(AsyncReqParam param){
            Type         resourceType = param.SourceType;
            string       name         = param.Name;
            SubPoolEvent onCompleted  = param.OnCompleted;
            //参数传递完就回收了
            AsyncReqParam.RecycleReq(param);
            if (!name.Equals(this.Name) || resourceType != this.SourceType){
                Debug.LogError($"错误的Prefab名 或者资源类型 : cur= {this.Name} , input :{name} , curType={this.SourceType} , inputType={resourceType}");
                return false;
            }

            //开始加载
            GameObject rel = null;
            if (_Pool.Count > 0){
                rel = _Pool.Pop() as GameObject;
                OnSpawnSuccess(rel);
                onCompleted?.Invoke(rel);
                _Using.Add(rel);
                return true;
            }

            bool resetOrigin = false;
            SubPoolEvent onAsyncLoadSuccess = com => {
                if (resetOrigin){
                    this._Original = com as GameObject;
                }

                rel      = UnityEngine.Object.Instantiate(this._Original) as GameObject;
                rel.name = name;
                OnSpawnSuccess(rel);
                onCompleted?.Invoke(rel);
                _Using.Add(rel);
            };
            if (this._Original == null){
                this.AsyncReloadPrefab(this.Path, onAsyncLoadSuccess);
            }
            else{
                onAsyncLoadSuccess?.Invoke(this._Original);
            }

            return true;
        }

        public void Return(Object obj){
            if (!Application.isPlaying || obj == null){
                return;
            }

            if (false == _Using.Remove(obj)){
                throw new ArgumentException($"[GameObjectSubPool] : Is not pool obj:{obj}");
            }

            GameObject gObj = obj as GameObject;
            StopEffect(gObj);
            CallOnOnUnSpawn(gObj);
            this.SetDefaultGameObjectAtr(gObj, null, false);
            _Pool.Push(gObj);
        }

        public void Release(){
            foreach (var single in _Using){
                GameObjUtils.DestroyObj(single);
            }

            this._Using.Clear();
            this.Clear(true);
        }

        public void Clear(bool cleanMemory = false){
            int len = _Pool.Count;
            for (int i = 0; i < len; ++i){
                GameObject oc = _Pool.Pop() as GameObject;
                GameObjUtils.DestroyObj(oc);
            }

            this._Pool.Clear();
            if (cleanMemory){
                GameUtils.CleanMemory();
            }
        }

        public void Dispose(){
            if (this._Pool != null){
                this._Pool.Clear();
                this._Pool = null;
            }
            
            if (_Using != null){
                this._Using.Clear();
                this._Using = null;
            }
        }

    #region Load fun

        private GameObject ReloadPrefab(string path){
            GameObject rel = AssetsSystem.Load(path, typeof(GameObject)) as GameObject;
            return rel;
        }

        private void AsyncReloadPrefab(string path, SubPoolEvent onComplete){
            GameObject rel;
            Action<object, object> callback = (ac, bc) => {
                rel = ac as GameObject;
                onComplete?.Invoke(rel);
            };
            AssetsSystem.LoadAsync(callback, path);
        }

    #endregion

    #region Set gameObject attribute

        private void OnSpawnSuccess(GameObject rel, Transform parent = null, bool active = true){
            SetDefaultGameObjectAtr(rel, parent, active);
            CallOnSpawn(rel);
        }

        private void SetDefaultGameObjectAtr(GameObject obj, Transform parent = null, bool active = true){
            obj.transform.SetParent(parent == null ? UnityObjectPool.Instance.transform : parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.SetActive(active);
        }

        private void StopEffect(GameObject obj, bool recursion){
            this.StopEffect(obj);
            if (recursion){
                int len = obj.transform.childCount;
                for (int i = 0; i < len; ++i){
                    GameObject child = obj.transform.GetChild(i).gameObject;
                    this.StopEffect(child);
                }
            }
        }

        private void StopEffect(GameObject obj){
            ParticleSystem particle = obj.GetComponent<ParticleSystem>();
            if (particle != null){
                particle.Stop();
            }
        }

        private void CallOnSpawn(GameObject obj){
            IReusable reusable = obj.GetComponent<IReusable>();
            if (reusable != null){
                reusable.OnSpawn();
            }
        }

        private void CallOnOnUnSpawn(GameObject obj){
            IReusable reusable = obj.GetComponent<IReusable>();
            if (reusable != null){
                reusable.OnUnSpawn();
            }
        }

    #endregion
    }
}