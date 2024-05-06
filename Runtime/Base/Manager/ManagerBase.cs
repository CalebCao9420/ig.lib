namespace IG.Manager{
    public abstract class ManagerBase<T> : SingletonAbs<T>, IManager where T : SingletonAbs<T>, new(){
        public override void OnDispose(){ Cleanup(); }
        public abstract bool Reload();
        public abstract void Cleanup();

        public override void Init(){
            base.Init();
            Reload();
        }
    }
}