using System;
using System.Collections.Generic;

namespace IG{
    using System.Timers;

    public static class DictionaryPool<TK, TV>{
        public const   int                         RECYCLE_LOOP = 60000 * 15;
        public const   int                         POOLSIZE     = 10;
        private static Stack<Dictionary<TK, TV>>   s_pool;
        private static HashSet<Dictionary<TK, TV>> s_using;
        private static System.Timers.Timer         s_timer;
        private static bool                        s_initComplete = false;
        private static object                      s_lock         = new();

        public static void Initialize(int poolSize = 1){
            if (null == s_pool){
                s_pool = new Stack<Dictionary<TK, TV>>(poolSize);
            }

            if (null == s_using){
                s_using = new HashSet<Dictionary<TK, TV>>();
            }

            for (int i = s_pool.Count; i < poolSize; ++i){
                Dictionary<TK, TV> li = new Dictionary<TK, TV>();
                s_pool.Push(li);
            }

            if (false == s_initComplete){
                s_timer           =  new System.Timers.Timer(RECYCLE_LOOP);             //周期调用Update
                s_timer.Elapsed   += new System.Timers.ElapsedEventHandler(ReturnAll); //timer定时事件绑定Update方法
                s_timer.AutoReset =  true;                                              //设置一直循环调用；若设置timer.AutoReset = false;只调用一次绑定方法
                s_timer.Start();                                                        //开启定时器事件或者写成timer.Enabled = true;
                s_initComplete = true;
            }
        }

        public static Dictionary<TK, TV> Get(){
            if (s_using == null){
                Initialize(POOLSIZE);
            }

            lock (s_lock){
                Dictionary<TK, TV> ret = s_pool.Count > 0 ? s_pool.Pop() : new Dictionary<TK, TV>();
                s_using.Add(ret);
                return ret;
            }
        }

        public static void Return(Dictionary<TK, TV> dic){
            if (false == s_initComplete){
                dic?.Clear(); //没正确初始化，走正常gc,不走池管理
                return;
            }

            lock (s_lock){
                if (false == s_using.Remove(dic)){
                    throw new ArgumentException($"[ListPool] : Input list doesn't contain the same list : {dic}");
                }

                s_pool.Push(dic);
                dic.Clear();
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