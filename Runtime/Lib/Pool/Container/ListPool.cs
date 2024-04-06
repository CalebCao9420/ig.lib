using System.Collections.Generic;

namespace IG{
    using System.Timers;

    public static class ListPool<T>{
        public class PoolObj{
            private List<T> _list;
            private bool    _state;

            public PoolObj(List<T> list){
                this._list  = list;
                this._state = false;
            }

            public bool    IsUsing()  { return this._state; }
            public List<T> Container(){ return _list; }
            public void    OnGet()    { this._state = true; }
            public void    OnRecycle(){ this._state = false; }
        }

        public const   int                 RECYCLE_LOOP = 60000 * 15;
        public const   int                 CAPACITY     = 10;
        public const   int                 POOLSIZE     = 10;
        private static List<PoolObj>       s_pool;
        private static System.Timers.Timer s_timer;
        private static bool                s_initComplete = false;
        private static object              s_lock         = new();

        private static void RecycleAll(object sender, ElapsedEventArgs e){
            if (s_pool.Count <= POOLSIZE){
                return;
            }

            lock (s_lock){
                s_pool.Clear();
            }
        }

        public static void Initialize(int capacity, int poolSize = 1){
            if (s_pool == null){
                s_pool = new List<PoolObj>(poolSize);
            }

            for (int i = s_pool.Count; i < poolSize; ++i){
                List<T> li  = new List<T>(capacity);
                PoolObj obj = new PoolObj(li);
                s_pool.Add(obj);
            }

            if (!s_initComplete){
                s_timer           =  new System.Timers.Timer(RECYCLE_LOOP);             //周期调用Update
                s_timer.Elapsed   += new System.Timers.ElapsedEventHandler(RecycleAll); //timer定时事件绑定Update方法
                s_timer.AutoReset =  true;                                              //设置一直循环调用；若设置timer.AutoReset = false;只调用一次绑定方法
                s_timer.Start();                                                        //开启定时器事件或者写成timer.Enabled = true;
                s_initComplete = true;
            }
        }

        public static List<T> GetList(int capacity = 1){
            if (s_pool == null){
                Initialize(CAPACITY, POOLSIZE);
            }

            lock (s_pool){
                PoolObj obj;
                List<T> ret;
                if (s_pool.Count == 0){
                    ret = new List<T>(capacity);
                    obj = new PoolObj(ret);
                    obj.OnGet();
                    s_pool.Add(obj);
                    return ret;
                }

                for (int i = 0; i < s_pool.Count; ++i){
                    obj = s_pool[i];
                    if (obj.Container().Capacity >= capacity && !obj.IsUsing()){
                        obj.OnGet();
                        return obj.Container();
                    }
                }

                ret = new List<T>(capacity);
                obj = new PoolObj(ret);
                obj.OnGet();
                s_pool.Add(obj);
                //没有匹配的就直接拿，先不管
                return ret;
            }
        }

        public static void Recycle(List<T> list){
            if (!s_initComplete){
                list?.Clear();
                return;
            }

            lock (s_pool){
                list.Clear();
                // _pool.Enqueue(list);
                for (int i = 0; i < s_pool.Count; ++i){
                    PoolObj obj = s_pool[i];
                    if (obj.Container().Equals(list)){
                        obj.OnRecycle();
                    }
                }
            }
        }
    }
}