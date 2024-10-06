using System;
using System.Collections.Generic;
using IG.AssetBundle;
using IG.Runtime.Extensions;
using IG.Runtime.Log;
using IG.Runtime.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Pool.SubPool{
    public class AudioSubPool : ISubPool, IDisposable{
        public    Type             SourceType{ get; } = typeof(AudioClip);
        public    string           Name      { get{ return this._Key; } }
        public    string           Path      { get{ return this._Path; } }
        protected string           _Key;
        protected string           _Path; //类似于Bundle
        protected Stack<AudioClip> _Pool;
        protected HashSet<AudioClip> _Using;
        protected AudioSubPool(){ }

        /// <summary>
        ///按Bundle注册子池 
        /// </summary>
        public AudioSubPool(string path){
            this._Path = path;
            AssetsSystem.HookBundleAndAssetName(path, out string notDo, out this._Key);
            this._Pool  = new();
            this._Using = new();
            var origin = AssetsSystem.Load<AudioClip>(path);
            this._Pool.Push(origin);
        }
        
        /// <summary>
        /// 按预置注册子池
        /// </summary>
        public AudioSubPool(Object prefab){
            this._Pool  = new();
            this._Using = new();
            var origin = prefab as AudioClip;
            this._Pool.Push(origin);
        }

        public void Return(Object obj){ this.Recycle(obj, false); }

        public void Recycle(Object obj, bool createNewPool = false){
            if (obj is AudioClip audioClip){
                if (false == _Using.Remove(audioClip)){
                    throw new ArgumentException($"[AudioSubPool] Cannot remove audio clip {audioClip.name}]");
                }
                this._Pool.Push(audioClip);
            }
        }

        public void Clear(bool cleanMemory = false){
            int len = _Pool.Count;
            for (int i = 0; i < len; ++i){
                AudioClip oc = _Pool.Pop() as AudioClip;
                GameObjUtils.DestroyObj(oc);
            }

            this._Pool.Clear();
            if (cleanMemory){
                GameUtils.CleanMemory();
            }
        }

        public void Release(){
            foreach (var single in _Using){
                GameObjUtils.DestroyObj(single);
            }

            this._Using.Clear();
            this.Clear(true);
        }

        public Object Spawn(){
            AudioClip rel;
            if (_Pool.Count > 0){
                rel = _Pool.Pop();
            }
            else{
                rel = AssetsSystem.Load<AudioClip>(this.Path);
            }

            _Using.Add(rel);
            return rel;
        }

        public bool SpawnAsync(AsyncReqParam param){
            if (param.OnCompleted == null){
                LogHelper.Log(this, $"Audio异步回调不能为空:{param.Name}");
                return false;
            }

            if (_Pool.Count > 0){
                var rel = _Pool.Pop();
                _Using.Add(rel);
                param.OnCompleted.Invoke(rel);
            }
            else{
                Action<object, object> callback = (ac, bc) => {
                    var rel = ac as AudioClip;
                    _Using.Add(rel);
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

            if (_Using != null){
                this._Using.Clear();
                this._Using = null;
            }
        }
    }
}