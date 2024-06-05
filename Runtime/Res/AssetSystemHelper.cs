using UnityEditor;
using UnityEngine;

namespace IG{
    public static class AssetSystemHelper{
        public static string GetPlatformABDirName(BuildTarget buildTarget){
            switch (buildTarget){
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    return "PC";
                case BuildTarget.iOS:     return "IOS";
                case BuildTarget.Android: return "Android";
                default:                  return string.Empty;
            }
        }

        public static string GetPlatformABDirName(){
            RuntimePlatform platform = RuntimePlatform.WindowsPlayer;
            switch (platform){
                case RuntimePlatform.WindowsPlayer: return "PC";
                case RuntimePlatform.IPhonePlayer:  return "IOS";
                case RuntimePlatform.Android:       return "Android";
                default:                            return "PC";
            }
        }
    }
}