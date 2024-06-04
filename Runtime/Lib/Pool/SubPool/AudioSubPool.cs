using System;
using System.Collections.Generic;
using IG.AssetBundle;
using IG.Runtime.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Pool.SubPool{
    public class AudioSubPool : ISubPool, IDisposable{
         public string                        Name      { get{ return this._Key; } }
    public      string                        SourcePath{ get{ return this._SourcePath; } }
    public      PoolResourceType              PoolType  { get{ return this._PoolType; } }
    protected   PoolResourceType              _PoolType;
    protected   string                        _Key;
    protected   string                        _SourcePath;
    protected   Dictionary<string, AudioClip> _Pool;
    protected AudioSubPool(){ }

    public AudioSubPool(PoolResourceType type, string sourcePath){
        this._PoolType = type;
        this._Key = sourcePath;
        this._SourcePath = sourcePath;
        this._Pool = new Dictionary<string, AudioClip>();

        //TODO:Editor 才需要区分格式，打出的ab包直接一个LoadAll即可
        // Object[] allAudio_mp3  = AssetSystem.LoadAll(PathConfig.BundleRelated.SOUND_BUNDLE, typeof(AudioClip), PathConst.Suffix.SOUND);
        // Object[] allAudio2_wav = AssetSystem.LoadAll(PathConfig.BundleRelated.SOUND_BUNDLE, typeof(AudioClip), PathConst.Suffix.SOUND2);
        // int      length        = allAudio_mp3 == null ? 0 : allAudio_mp3.Length;
        // for (int i = 0; i < length; ++i){
        //     if (this._Pool.ContainsKey(allAudio_mp3[i].name)){
        //         Debug.Log($"Object pool repeat add the same obj:{allAudio_mp3[i].name}");
        //         continue;
        //     }
        //
        //     _Pool.Add(allAudio_mp3[i].name, allAudio_mp3[i] as AudioClip);
        // }
        //
        // length = allAudio2_wav == null ? 0 : allAudio2_wav.Length;
        // for (int i = 0; i < length; ++i){
        //     if (this._Pool.ContainsKey(allAudio2_wav[i].name)){
        //         Debug.Log($"Object pool repeat add the same obj:{allAudio2_wav[i].name}");
        //         continue;
        //     }
        //
        //     _Pool.Add(allAudio2_wav[i].name, allAudio2_wav[i] as AudioClip);
        // }
    }
    
    public void Recycle(Object obj){ }
    public void Clear(bool cleanMemory = false){ }
    public Object Spawn(string name){ return null; }
    public bool SpawnAsync(AsyncReqParam param){ return true; }
    public bool IsLock(){ return true; }

    public void Dispose(){
        if (this._Pool != null){
            this._Pool.Clear();
            this._Pool = null;
        }
    }
    }
}