namespace IG.Data{
    public interface IVisualRenderer<D, R> where D : IVisualData<D, R> where R : IVisualRenderer<D, R>{
        void OnConnect(D data);
        void OnDisconnect();
    }
}