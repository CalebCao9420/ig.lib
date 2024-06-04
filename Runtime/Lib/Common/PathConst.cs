using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace IG.Runtime.Common{
    public static class PathConst{
        public static Dictionary<Type, string> S_TypeMap = new Dictionary<Type, string>(){
                                                                                             { typeof(GameObject), Suffix.PREFAB },
                                                                                             { typeof(Material), Suffix.MAT },
                                                                                             { typeof(TextAsset), Suffix.JSON },
                                                                                             { typeof(Texture2D), Suffix.PNG },
                                                                                             { typeof(Texture), Suffix.PNG },
                                                                                             { typeof(Sprite), Suffix.PNG },
                                                                                             { typeof(AudioClip), Suffix.AUDIO_MP3 },
                                                                                             { typeof(ScriptableObject), Suffix.SCRIPTABLE_OBJECT },
                                                                                         };

        public static readonly Dictionary<string, Type> S_FileTypeMap = new Dictionary<string, Type>(){
                                                                                                          { Suffix.PREFAB, typeof(GameObject) },
                                                                                                          { Suffix.MAT, typeof(Material) },
                                                                                                          { Suffix.JSON, typeof(TextAsset) },
                                                                                                          { Suffix.XML, typeof(TextAsset) },
                                                                                                          { Suffix.PNG, typeof(Texture) },
                                                                                                          { Suffix.BUNDLE, typeof(UnityEngine.AssetBundle) },
                                                                                                          { Suffix.AUDIO_MP3, typeof(AudioClip) },
                                                                                                          { Suffix.AUDIO_WAV, typeof(AudioClip) },
                                                                                                          { Suffix.SPRITE_ATLAS, typeof(SpriteAtlas) },
                                                                                                          { Suffix.SCRIPTABLE_OBJECT, typeof(ScriptableObject) },
                                                                                                          { Suffix.RENDER_TEXTURE, typeof(RenderTexture) }
                                                                                                      };

        public static class Suffix{
            public const string PREFAB         = ".prefab";
            public const string UNITY          = ".unity"; //Unity 资源变体
            public const string U3D            = ".u3d";
            public const string AUDIO_MP3      = ".mp3";
            public const string AUDIO_WAV      = ".wav";
            public const string MAT            = ".mat";
            public const string PNG            = ".png";
            public const string RENDER_TEXTURE = ".renderTexture";

            // public const string BUNDLE = ".bundle";
            public const string BUNDLE            = ".ab";
            public const string ASSETS            = ".assets";
            public const string SCRIPTABLE_OBJECT = ".asset";
            public const string JSON              = ".json";
            public const string XML               = ".xml";
            public const string MANIFEST          = ".manifest";
            public const string SPRITE_ATLAS      = ".spriteatlas";
        }
    }
}