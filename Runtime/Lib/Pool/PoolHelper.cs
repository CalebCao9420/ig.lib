using System;
using IG.Pool.SubPool;
using UnityEngine;

namespace IG.Pool{
    public static class PoolHelper{
        public static ISubPool CreateSubPool(Type type, string path){
            if (type == typeof(AudioClip)){
                return new AudioSubPool(path);
            }
            else if (type == typeof(GameObject)){
                return new GameObjectSubPool(path);
            }

            return default;
        }

        public static ISubPool CreateSubPool(Type type, UnityEngine.Object prefab){
            if (type == typeof(AudioClip)){
                return new AudioSubPool(prefab);
            }
            else if (type == typeof(GameObject)){
                return new GameObjectSubPool(prefab);
            }

            return default;
        }
        
        public static void ReturnRoot(Type type, Transform root){
            for (int i = root.childCount - 1; i >= 0; i--){
                ObjectPool.Instance.Return(root.GetChild(i).gameObject);
            }
        }
    }
}