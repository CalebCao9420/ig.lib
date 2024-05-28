using IG.Manager;

namespace IG{
    public static class Launch{
        public static void Setup(){
            GameLooper.Instance.Init();
            ManagerLauncher.Launch();
        }
    }
}