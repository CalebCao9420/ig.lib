using System.Collections.Generic;

namespace IG{
    /// <summary>
    /// Singleton manager.
    /// </summary>
    public static class SingletonManager{
        /// <summary>
        /// Must lazy
        /// </summary>
        private static UnityEngine.GameObject s_singletonRoot{
            get{
                lock (s_lock){
                    if (s_singletonRoot_static == null){
                        s_singletonRoot_static = new UnityEngine.GameObject("SingletonMono");
                    }

                    return s_singletonRoot_static;
                }
            }
        }

        private static          UnityEngine.GameObject s_singletonRoot_static;
        private static readonly object                 s_lock       = new object();
        private static          List<ISingleton>       s_objectList = null;
        static SingletonManager(){ s_objectList = new(); }

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
        /// If you want remove singleton or singletonMono ,Must use this function , can't directory remove singleton self
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
                Remove(s_objectList[0]);
            }
        }
    }
}