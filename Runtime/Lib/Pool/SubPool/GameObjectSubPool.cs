using System;
using System.Collections.Generic;
using IG.AssetBundle;
using IG.Runtime.Extensions;
using IG.Runtime.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Pool.SubPool{
    public class GameObjectSubPool : ISubPool, IDisposable{
        public    string           Name    { get{ return this._Path; } }
        public    string           Path    { get{ return this._Path; } }
        public    PoolResourceType PoolType{ get{ return this._PoolType; } }
        protected PoolResourceType _PoolType;
        protected Object           _Original;
        protected string           _Key;  //类似于Bundle
        protected string           _Path; //类似于Bundle
        protected List<Object>     _Pool;
        protected GameObjectSubPool(){ }

        public GameObjectSubPool(PoolResourceType type, string path, Object original = null){
            this._Path = path;
            AssetsSystem.HookBundleAndAssetName(path, out string notDo, out this._Key);
            this._PoolType = type;
            this._Original = original;
            this._Pool     = new List<Object>();
        }

        public UnityEngine.Object Spawn(){ return Spawn(null, true); }

        public Object Spawn(Transform parent = null, bool isActive = true){
            GameObject rel;
            if (_Pool.Count > 0){
                rel = _Pool.First(true) as GameObject;
                if (rel.activeSelf){
                    this.Recycle(rel);
                    return null;
                }

                OnSpawnSuccess(rel, parent, isActive);
                return rel;
            }

            if (this._Original == null){
                this._Original = this.ReloadPrefab(this.Path);
                if (this._Original == null){
                    Debug.LogError($"ReloadPrefab错误请检查加载或者配置是否正确:cur= {this.Name} , input :{this.Name}");
                    return null;
                }
            }

            rel      = UnityEngine.Object.Instantiate(this._Original) as GameObject;
            rel.name = this.Name;
            OnSpawnSuccess(rel, parent, isActive);
            return rel;
        }

        public bool SpawnAsync(AsyncReqParam param){
            PoolResourceType resourceType = param.SourceType;
            string           name         = param.Name;
            SubPoolEvent     onCompleted  = param.OnCompleted;
            //参数传递完就回收了
            AsyncReqParam.RecycleReq(param);
            if (!name.Equals(this.Name) || resourceType != this._PoolType){
                Debug.LogError($"错误的Prefab名 或者资源类型 : cur= {this.Name} , input :{name} , curType={this._PoolType} , inputType={resourceType}");
                return false;
            }

            //开始加载
            GameObject rel = null;
            if (_Pool.Count > 0){
                rel = _Pool.First(true) as GameObject;
                if (rel.activeSelf){
                    this.Recycle(rel);
                    return false; //return false 则ObjectPool判断完，直接重新load即可
                }

                OnSpawnSuccess(rel);
                onCompleted?.Invoke(rel);
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
            };
            if (this._Original == null){
                this.AsyncReloadPrefab(this.Path, onAsyncLoadSuccess);
            }
            else{
                onAsyncLoadSuccess?.Invoke(this._Original);
            }

            return true;
        }

        public void Recycle(Object obj){
            if (!Application.isPlaying || obj == null){
                return;
            }

            GameObject gObj = obj as GameObject;
            StopEffect(gObj);
            CallOnOnUnSpawn(gObj);
            this.SetDefaultGameObjectAtr(gObj, null, false);
            _Pool.Add(gObj);
        }

        public void Clear(bool cleanMemory = false){
            int len = _Pool.Count;
            for (int i = 0; i < len; ++i){
                GameObject oc = _Pool[i] as GameObject;
                GameObjUtils.DestroyObj(oc);
            }

            this._Pool.Clear();
            if (cleanMemory){
                GameUtils.CleanMemory();
            }
        }

        public bool IsLock(){ return true; }

        public void Dispose(){
            if (this._Pool != null){
                this._Pool.Clear();
                this._Pool = null;
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
            obj.transform.SetParent(parent == null ? ObjectPool.Instance.transform : parent);
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