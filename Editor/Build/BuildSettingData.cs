using System;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;

namespace IG.Editor{
    public partial class BuildSettingData : ScriptableObject{
    #region publicMethod

        public static string ProjectOutFile(){
            var root = Directory.GetCurrentDirectory();
            root += "/Output" + "/" + EditorUserBuildSettings.activeBuildTarget + "/" + BuildSettingData.Instance.ProductName;
            return root;
        }

    #endregion

        private void OnEnable(){
            CompanyName           = Application.companyName ?? "xxx";
            ProductName           = Application.productName ?? "xxx";
            ApplicationIdentifier = Application.identifier  ?? "com.xx.xx";
        }

    #region Path

        /// <summary>
        /// 设置数据基本路径
        /// </summary>
        private const string SettingDataBasePath = "Assets/Editor/Res";

        /// <summary>
        /// 储存路径
        /// </summary>
        public const string PATH = SettingDataBasePath + "/BuildSettingData.asset";

        // <summary>
        /// 获取到打包出的ab包路径
        /// </summary>
        /// <returns></returns>
        public static string GetABOutFile(){ return GetPlatformPath() + "/" + AssetSystemHelper.GetPlatformABDirName(Instance.TargetPlat); }
       

        /// <summary>
        /// 获取平台路径
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformPath(){
            string strReturnPlatformPath = "";
            // switch (Instance.TargetPlat){
            //     case BuildTarget.StandaloneWindows64:
            //     case BuildTarget.StandaloneWindows:
            //         strReturnPlatformPath = Application.streamingAssetsPath;
            //         break;
            //     case BuildTarget.Android:
            //     case BuildTarget.iOS:
            //         strReturnPlatformPath = Application.persistentDataPath;
            //         break;
            // }
            strReturnPlatformPath = $"{Application.dataPath}/../Builds";
            return strReturnPlatformPath;
        }

    #endregion

    #region Data

        public string CompanyName;
        public string ProductName;
        public bool   AllowUnsafeCode = true;
        public bool   StripEngineCode = false;
        public string ApplicationIdentifier;
        
        public string AB_MANIFEST => $"{AssetSystemHelper.GetPlatformABDirName(Instance.TargetPlat)}.manifest";

        /// <summary>
        /// 本地AB包
        /// </summary>
        public bool IsLocalAB;

        /// <summary>
        /// GM包
        /// </summary>
        public bool IsGM;

        /// <summary>
        /// 代码版本
        /// </summary>
        public Int32 CodeVer = 0;

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version = "1.0.0";

        /// <summary>
        /// icon路径
        /// </summary>
        public string Icon = SettingDataBasePath + "/xxx_icon/xxx_512.png";

        /// <summary>
        /// logo
        /// </summary>
        public Sprite Logo;

        /// <summary>
        /// 打包目标平台
        /// </summary>
        public BuildTarget TargetPlat;

        /// <summary>
        /// 构建目录
        /// </summary>
        private const string m_BaseBuildPath = "IG_PX";

        /// <summary>
        /// 构建目录变量
        /// </summary>
        private string m_BuildPath;

    #endregion

        private static BuildSettingData m_Instance;

        public static BuildSettingData Instance{
            get{
                if (m_Instance == null){
                    m_Instance = AssetDatabase.LoadAssetAtPath<BuildSettingData>(PATH);
                    if (m_Instance == null){
                        m_Instance = new BuildSettingData();
                        FileInfo fileInfo = new FileInfo(PATH);
                        if (!fileInfo.Directory.Exists){
                            fileInfo.Directory.Create();
                        }

                        AssetDatabase.CreateAsset(m_Instance, PATH);
                    }
                }

                return m_Instance;
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        public void Save(){
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif