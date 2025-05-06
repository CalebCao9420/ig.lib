using System;
using System.Collections.Generic;
using System.Text;

namespace IG{
    public static class GnObjectPool{
        public const   int                             CAPACITY         = 20;
        private static Dictionary<Type, int>           s_capacityDict   = null;
        private static Dictionary<Type, int>           s_objectUsedDict = null;
        private static Dictionary<Type, Queue<Object>> s_objectDict     = null;
        private static Dictionary<Type, List<Object>>  s_objectUsedRef  = null;

        static GnObjectPool(){
            s_objectDict     = new();
            s_objectUsedDict = new();
            s_capacityDict   = new();
            s_objectUsedRef  = new();
        }

        public static int GetIdleCount<T>(){
            Type p = typeof(T);
            if (s_objectDict.TryGetValue(p, out var value)){
                return value.Count;
            }

            return 0;
        }

        public static Object Obtain(Type prototype){
            if (!s_objectDict.ContainsKey(prototype)){
                CreateObjectPool(prototype, CAPACITY);
            }

            Queue<Object> objects  = s_objectDict[prototype];
            Object        instance = null;
            if (objects.Count > 0){
                //返回第一个
                instance = objects.Dequeue();
                //更新使用计数
                s_objectUsedDict[prototype]++;
                //添加到使用中的队列
                s_objectUsedRef[prototype].Add(instance);
            }
            else{
                //创建新的实例
                instance = Activator.CreateInstance(prototype);
                //更新使用计数
                s_objectUsedDict[prototype]++;

                //添加到使用中的队列
                s_objectUsedRef[prototype].Add(instance);
            }

            return instance;
        }

        /// <summary>
        /// 获取对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Obtain<T>(){
            Type prototype = typeof(T);
            T    ins       = (T)Obtain(prototype);
            // ins.Clear();
            return ins;
        }

        public static void Return(System.Object obj){
            Type prototype = obj.GetType();
            if (s_objectDict.TryGetValue(prototype, out var value)){
                Queue<Object> objects  = value;
                List<Object>  used     = s_objectUsedRef[prototype];
                int           capacity = s_capacityDict[prototype];
                if (used.Contains(obj)){
                    //更新使用引用计数
                    s_objectUsedDict[prototype]--;
                    //从使用队列删除,并且将对象加入未使用队列
                    used.Remove(obj);
                    //空闲对象到达阀值则将对象直接丢弃等待垃圾回收
                    // ((IPoolable)obj).Clear();
                    if (objects.Count < capacity){
                        objects.Enqueue(obj);
                    }
                }
            }
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="capacitySize"></param>
        // public static void CreateObjectPool<T>(int capacitySize){
        //     Type prototype = typeof(T);
        //     CreateObjectPool(prototype, capacitySize);
        // }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="capacitySize"></param>
        /// <param name="initCreate"></param>
        // public static void CreateObjectPool<T>(int capacitySize, bool initCreate){
        //     Type prototype = typeof(T);
        //     CreateObjectPool(prototype, capacitySize);
        //     if (initCreate){
        //         Queue<Object> pool = s_objectDict[prototype];
        //         for (int idx = 0; idx < capacitySize; idx++){
        //             object ins = Activator.CreateInstance(prototype);
        //             pool.Enqueue(ins);
        //         }
        //     }
        // }

        public static bool IsPoolExists<T>(){ return s_objectDict.ContainsKey(typeof(T)); }

        public static void DeleteObjectPool<T>(){
            Type prototype = typeof(T);
            DeleteObjectPool(prototype);
        }

        public static void DeleteObjectPool(Type type){
            if (s_objectDict.TryGetValue(type, out var pool)){
                // while (pool.Count > 0){
                //     IPoolable ins = (IPoolable)pool.Dequeue();
                //     ins.Clear();
                // }
                pool.Clear();
                s_objectDict.Remove(type);
            }

            if (s_objectUsedRef.TryGetValue(type, out var refUse)){
                refUse.Clear();
                s_objectUsedRef.Remove(type);
            }

            s_objectUsedDict.Remove(type);
            s_capacityDict.Remove(type);
        }

        /// <summary>
        /// 获得指定类型对象池的状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string DumpPoolStatus<T>(){
            StringBuilder result    = new StringBuilder();
            Type          prototype = typeof(T);
            if (s_objectDict.ContainsKey(prototype)){
                if (s_capacityDict.TryGetValue(prototype, out int capacity)){
                    result.AppendLine("Capacity = " + capacity);
                }

                if (s_objectDict.TryGetValue(prototype, out Queue<Object> pool)){
                    result.AppendLine("Idle Object = " + pool.Count);
                }

                if (s_objectUsedDict.TryGetValue(prototype, out int used)){
                    result.AppendLine("Used=" + used);
                }
            }

            return result.ToString();
        }

    #region Private functions

        /// <summary>
        /// 通过指定的原型创建
        /// </summary>
        /// <param name="prototype">Prototype.</param>
        /// <param name="capacitySize">Capacity size.</param>
        /// <param name="initObjs"></param>
        private static void CreateObjectPool(Type prototype, int capacitySize, params object[] initObjs){
            if (!s_objectDict.ContainsKey(prototype)){
                Queue<Object> pool = new Queue<Object>(CAPACITY);
                if (null != initObjs){
                    foreach (object obj in initObjs){
                        pool.Enqueue(obj);
                    }
                }

                s_objectDict.Add(prototype, pool);
                s_objectUsedRef.Add(prototype, new List<Object>());
                s_objectUsedDict.Add(prototype, 0);
                s_capacityDict.Add(prototype, capacitySize);
            }
        }

    #endregion
    }
}