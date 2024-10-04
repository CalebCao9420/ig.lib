using System;
using System.Collections.Generic;
using IG.Runtime.Extensions;

namespace IG{
    using System.Timers;

    public static class ListPool<T>{
        public const   int                 RECYCLE_LOOP = 60000 * 15;
        public const   int                 CAPACITY     = 10;
        public const   int                 POOLSIZE     = 10;
        private static List<List<T>>      s_pool;
        private static HashSet<List<T>>    s_using;
        private static System.Timers.Timer s_timer;
        private static bool                s_initComplete = false;
        private static object              s_lock         = new();

        public static void Initialize(int capacity, int poolSize = 1){
            if (null == s_pool){
                s_pool = new List<List<T>>(poolSize);
            }

            if (null == s_using){
                s_using = new HashSet<List<T>>();
            }

            for (int i = s_pool.Count; i < poolSize; ++i){
                List<T> li  = new List<T>(capacity);
                s_pool.Add(li);
            }

            if (false == s_initComplete){
                s_timer           =  new System.Timers.Timer(RECYCLE_LOOP);             //周期调用Update
                s_timer.Elapsed   += new System.Timers.ElapsedEventHandler(ReturnAll); //timer定时事件绑定Update方法
                s_timer.AutoReset =  true;                                              //设置一直循环调用；若设置timer.AutoReset = false;只调用一次绑定方法
                s_timer.Start();                                                        //开启定时器事件或者写成timer.Enabled = true;
                s_initComplete = true;
            }
        }

        public static List<T> Get(int capacity = 1){
            if (s_using == null){
                Initialize(CAPACITY, POOLSIZE);
            }

            lock (s_lock){
                List<T> ret = null;

                bool OnCheck(List<T> checkObj){
                    if (checkObj.Capacity >= capacity){
                        s_pool.Remove(checkObj);
                        ret = checkObj;
                        return true;
                    }

                    return false;
                }

                s_pool.Ergodic(OnCheck);
                if (null == ret){
                    ret = new List<T>(capacity);
                }

                s_using.Add(ret);
                // s_pool.Add(obj);
                //没有匹配的就直接拿，先不管
                return ret;
            }
        }

        public static void Return(List<T> list){
            if (false == s_initComplete){
                list?.Clear(); //没正确初始化，走正常gc,不走池管理
                return;
            }

            lock (s_lock){
                if (false == s_using.Remove(list)){
                    throw new ArgumentException($"[ListPool] : Input list doesn't contain the same list : {list}");
                }
                s_pool.Add(list);
                list.Clear();
            }
        }

    #region Private static function

        private static void ReturnAll(object sender, ElapsedEventArgs e){
            if (s_pool.Count <= POOLSIZE){
                return;
            }

            lock (s_lock){
                s_pool.Clear(); //目前只清理pool内容器
            }
        }

    #endregion
    }
}