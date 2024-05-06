namespace IG{
    /// <summary>
    /// Abs single base.
    /// </summary>
    public abstract class SingletonAbs<T> : ISingleton where T : SingletonAbs<T>, new(){
        private static T s_instance = null;

        /// <summary>
        /// GetInstance this instance.
        /// </summary>
        public static T Instance{
            get{
                if (s_instance == null){
                    s_instance = SingletonManager.Create<T>();
                }

                return s_instance;
            }
        }

        public virtual void Init(){ }

        public void Dispose(){
            OnDispose();
            s_instance = null;
        }

        public abstract void OnDispose();

        /// <summary>
        /// Gets a value indicating is Valid.
        /// </summary>
        /// <value><c>true</c> if is N ull; otherwise, <c>false</c>.</value>
        public static bool IsValid{ get{ return s_instance != null; } }
    }
}