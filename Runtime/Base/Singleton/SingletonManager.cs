using System.Collections.Generic;

namespace IG{
    /// <summary>
    /// Singleton manager.
    /// </summary>
    public static class SingletonManager{
        private static readonly UnityEngine.GameObject s_singletonRoot;
        private static readonly object                 s_lock       = new object();
        private static          List<ISingleton>       s_objectList = null;

        static SingletonManager(){
            s_objectList    = new();
            s_singletonRoot = new UnityEngine.GameObject("SingleMonoBehaviour");
        }

        /// <summary>
        /// Adds the single class
        /// </summary>
        /// <param name="singleton"></param>
        public static void Add(ISingleton singleton){ s_objectList.Add(singleton); }

        /// <summary>
        /// Create simple obj
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>() where T : ISingleton{
            lock (s_lock){
                T rel = (T)System.Activator.CreateInstance(typeof(T), true);
                rel.Init();
                Add(rel);
                return rel;
            }
        }

        /// <summary>
        /// Create mono single class
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>(string name) where T : UnityEngine.MonoBehaviour, ISingleton{
            lock (s_lock){
                UnityEngine.GameObject obj = new UnityEngine.GameObject();
                UnityEngine.Object.DontDestroyOnLoad(obj);
                obj.transform.SetParent(s_singletonRoot.transform);
                T singleton = obj.AddComponent<T>();
                obj.name = name;
                obj.SetActive(true);
                Add(singleton);
                return singleton;
            }
        }

        /// <summary>
        /// Removes the single class.
        /// </summary>
        /// <param name="singleton">Single class.</param>
        public static void Remove(ISingleton singleton){
            s_objectList.Remove(singleton);
            singleton.Dispose();
        }

        /// <summary>
        /// Destroies all.
        /// </summary>
        public static void DestroyAll(){
            while (s_objectList.Count > 0){
                s_objectList[0].Dispose();
            }

            s_objectList.Clear();
        }

        public static bool IsValid(){ return s_singletonRoot != null; }
    }
}