namespace IG{
    public interface IReusable{
        /// <summary>
        /// 取出时
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 回收时
        /// </summary>
        void OnUnSpawn();
    }
}