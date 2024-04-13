using UnityEngine;

namespace IG{
    /// <summary>
    /// MonoBehaviour Single class
    /// </summary>
    public abstract class SingletonMono<T> : MonoBehaviour, ISingleton where T : MonoBehaviour, ISingleton{
        private static T s_instance = null;

        private void Awake(){
            if (!Application.isPlaying){
                return;
            }

            s_instance = this as T;
            OnAwake();
        }

        private void Start(){
            if (!Application.isPlaying){
                return;
            }

            this.OnStart();
        }

        /// <summary>
        /// Raises the awake event.
        /// </summary>
        protected virtual void OnAwake(){ }

        /// <summary>
        /// Raises the start event.
        /// </summary>
        protected virtual void OnStart(){ }

        /// <summary>
        /// Get object instance
        /// </summary>
        public static T Instance{
            get{
                if (s_instance == null){
                    s_instance = SingletonManager.Create<T>(typeof(T).ToString());
                }

                return s_instance;
            }
        }

        /// <summary>
        /// Raises the application pause event.
        /// </summary>
        /// <param name="isPause">If set to <c>true</c> is pause.</param>
        protected virtual void OnApplicationPause(bool isPause){ }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        public virtual void OnDestroy(){
            SingletonManager.Remove(this);
        }

        /// <summary>
        /// Gets a value indicating is valid.
        /// </summary>
        /// <value><c>true</c> if is N ull; otherwise, <c>false</c>.</value>
        public static bool IsValid{ get{ return s_instance != null; } }

        public void Dispose(){
            OnDispose();
            s_instance = null;
        }

        public abstract void OnDispose();
        public virtual  void Init(){ }
    }
}