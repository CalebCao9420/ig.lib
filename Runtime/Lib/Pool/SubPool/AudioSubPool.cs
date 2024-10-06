using System;
using System.Collections.Generic;
using IG.AssetBundle;
using IG.Runtime.Extensions;
using IG.Runtime.Log;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Pool.SubPool{
    public class AudioSubPool : ISubPool, IDisposable{
        public    Type            SourceType{ get; } = typeof(AudioClip);
        public    string          Name      { get{ return this._Key; } }
        public    string          Path      { get{ return this._Path; } }
        protected string          _Key;
        protected string          _Path; //类似于Bundle
        protected List<AudioClip> _Pool;
        protected AudioSubPool(){ }

        /// <summary>
        ///按Bundle注册子池 
        /// </summary>
        public AudioSubPool(string path){
            this._Path = path;
            AssetsSystem.HookBundleAndAssetName(path, out string notDo, out this._Key);
            this._Pool = new();
            var origin = AssetsSystem.Load<AudioClip>(path);
            this._Pool.Add(origin);
        }
        
        /// <summary>
        /// 按预置注册子池
        /// </summary>
        public AudioSubPool(Object prefab){
            this._Pool = new();
            var origin = prefab as AudioClip;
            this._Pool.Add(origin);
        }

        public void Return(Object obj){ this.Recycle(obj, false); }

        public void Recycle(Object obj, bool createNewPool = false){
            if (obj is AudioClip audioClip){
                this._Pool.Add(audioClip);
            }
        }

        public void Clear(bool cleanMemory = false){ }
        public void Release()                      { throw new NotImplementedException(); }

        public Object Spawn(){
            AudioClip rel;
            if (_Pool.Count > 0){
                rel = _Pool.First(true);
            }
            else{
                rel = AssetsSystem.Load<AudioClip>(this.Path);
            }

            return rel;
        }

        public bool SpawnAsync(AsyncReqParam param){
            if (param.OnCompleted == null){
                LogHelper.Log(this, $"Audio异步回调不能为空:{param.Name}");
                return false;
            }

            if (_Pool.Count > 0){
                param.OnCompleted.Invoke(_Pool.First(true));
            }
            else{
                Action<object, object> callback = (ac, bc) => {
                    var rel = ac as AudioClip;
                    param.OnCompleted.Invoke(rel);
                };
                AssetsSystem.LoadAsync(callback, this.Path);
            }

            return true;
        }

        public void Dispose(){
            if (this._Pool != null){
                this._Pool.Clear();
                this._Pool = null;
            }
        }
    }
}