using System;
using System.IO;
using IG.Runtime.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace IG.AssetBundle{
    public enum AssetMode{
        Editor,         //编辑器
        EditorPkg,      //编辑器包
        Local,          //本地包
        Remote,         //远程包
        LocalAndRemote, //本地远程修复包
    }

    /// <summary>
    /// AssetSystem
    /// 可以修改成json，服务器下发
    /// </summary>
    [CreateAssetMenu(fileName = "AssetSystemConfig", menuName = "Config/AssetSystemConfig")]
    public class AssetSystemConfig : ScriptableObject{
        private static string    s_configPath;
        private static string    s_cachePath;
        public static  string    CONFIG_PATH => s_configPath;
        public         AssetMode AssetLoadMode; //TODO:时间充足补充,现在就用本地包模式

        public string ABInfoFile   = "au_abinfo.table";//所有资源CRC与Size信息
        public string ABPackResMap = "au_map.table"; //key=资源包和value=资源映射表
        public string ABTotalMap   = "au_total.table";//key=资源 value=资源包表
        /// <summary>
        /// 编辑器的资源路径
        /// </summary>
        public string ABDIR_URL;

        /// <summary>
        /// 资源url
        /// </summary>
        public string URL;

        /// <summary>
        /// //DownloadSystem缓存路径,当前暂定Application.persistentDataPath
        /// </summary>
        public string CACHE_PATH => s_cachePath;

        /// <summary>
        /// TODO:没时间做，暂时用BuildingSettingData的PlatformName
        /// </summary>
        public string MainBundleName = AssetSystemHelper.GetPlatformABDirName();

    #region Instance

        private static AssetSystemConfig m_Instance;

        public static AssetSystemConfig Instance{
            get{
                if (m_Instance == null){
                    s_configPath = $"{Application.dataPath}/Resources/AssetSystemConfig.asset";
                    s_cachePath  = $"{Application.persistentDataPath}/{AssetSystemHelper.GetPlatformABDirName()}";
#if UNITY_EDITOR
                    m_Instance = EditorLoadInstance();
#else
                    m_Instance = LoadInstance();
#endif
                }

                return m_Instance;
            }
        }

        private static AssetSystemConfig LoadInstance(){
            string            realPath = StringUtils.CutStrBySign(CONFIG_PATH, "Resources", StringUtils.CutStrType.Front);
            AssetSystemConfig rel      = Resources.Load<AssetSystemConfig>(realPath);
            return rel;
        }

#if UNITY_EDITOR
        private static AssetSystemConfig EditorLoadInstance(){
            string            realPath = StringUtils.CutStrBySign(CONFIG_PATH, "Assets", StringUtils.CutStrType.Front);
            AssetSystemConfig rel      = AssetDatabase.LoadAssetAtPath<AssetSystemConfig>(realPath);
            if (rel == null){
                rel = new AssetSystemConfig();
                FileInfo fileInfo = new FileInfo(CONFIG_PATH);
                if (!fileInfo.Directory.Exists){
                    fileInfo.Directory.Create();
                }

                AssetDatabase.CreateAsset(rel, realPath);
            }

            return rel;
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        public void Save(){
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

    #endregion
    }
}