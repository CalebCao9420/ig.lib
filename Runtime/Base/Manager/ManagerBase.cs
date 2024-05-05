namespace IG.Manager{
    public abstract class ManagerBase<T> : SingletonAbs<T>, IManager where T : SingletonAbs<T>, new(){
        public override void OnDispose(){ Cleanup(); }
        public abstract bool Init();
        public abstract void Cleanup();
    }
}