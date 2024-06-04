namespace IG.AssetBundle{
    /// <summary>
    /// AssetBundle配置
    /// </summary>
    public sealed class AssetBundleConfig{
        /// <summary>
        /// 资源包名
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// 资源包CRC
        /// </summary>
        public uint CRC = 0;

        /// <summary>
        /// 资源包大小
        /// </summary>
        public long Size = 0;
    }
}