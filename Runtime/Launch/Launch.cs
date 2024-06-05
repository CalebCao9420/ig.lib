using IG.AssetBundle;
using IG.Manager;

namespace IG{
    public static partial class Launch{
        public static void Setup(){
            LaunchAssetSystem();
            GameLooper.Instance.Init();
            OnPostGameLooperInit();
            ManagerLauncher.Launch();
            OnLaunchComplete();
        }

        private static void LaunchAssetSystem(){
            AssetSystemConfig assetSystemConfig = AssetSystemConfig.Instance;
            DownloadSystem.Setup(assetSystemConfig.CACHE_PATH);
            AssetSystem.Setup(assetSystemConfig);
        }

        static partial void OnPostGameLooperInit();
        static partial void OnLaunchComplete();
    }
}