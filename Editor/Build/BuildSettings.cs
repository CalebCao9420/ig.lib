#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using IG.AssetBundle;
using UnityEditor.Build.Reporting;
using IG.Editor.Helper;
using IG.Editor.Res;
using IG.Runtime.Utils;
using UnityEditor;
using UnityEngine;

namespace IG.Editor{
    public partial class BuildSettings : EditorWindow{
        private const int              WINDOWS_SPACE_DEFAULT  = 10;
        private const int              WINDOWS_HEIGHT_DEFAULT = 20;
        private const int              WINDOWS_WIDTH_DEFAULT  = 400;
        private const string           FORMAT_PATH            = "{0}/{1}";
        private const string           PATH_BUILD             = "Build/AssetBundle";
        private const string           NAME_OK                = "OK";
        private const string           GUI_PRE_PROCESS_TITLE  = "Build Preprocess [{0}/{1}]";
        private const string           GUI_PRE_PROCESS_INFO   = "Preprocessing...{0}";
        private const string           GUI_POST_PROCESS_TITLE = "Build Postprocess [{0}/{1}]";
        private const string           GUI_POST_PROCESS_INFO  = "Postprocessing...{0}";
        private const string           NAME_BUILD_COMPLETE    = "Build Complete";
        private const string           NAME_BUILD_TIME        = "Build cost time:{0}.";
        public static BuildSettingData Data;

        /// <summary>
        /// 标记是否有打包报错信息
        /// </summary>
        private bool isBuildSuccess = true;

        [MenuItem("AssetManager/SettingWindow")]
        private static void ShowSettingWindow(){
            var window = GetWindow(typeof(BuildSettings));
            window.maxSize = Vector2.one * 700.0f;
            window.minSize = Vector2.one * 200.0f;
        }

        private BuildSettings(){ }

        private void OnGUI(){
            if (!EditorApplication.isCompiling){
                Draw();
            }
            else{
                GUILayout.Label("编译中...");
            }
        }

        partial void OnPreDrawComplete();

        private void OnPreDraw(){
            if (Data == null){
                Data            = BuildSettingData.Instance;
                Data.TargetPlat = EditorUserBuildSettings.activeBuildTarget;
            }

            OnPreDrawComplete();
        }

        partial void OnDraw();

        private void Draw(){
            OnPreDraw();
            GUI.skin.label.fontSize  = 14;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            ///Setting
            EditorHelper.VerticalPair(DrawPlatformSelect);

            //Version
            EditorHelper.VerticalPair(DrawVersion);

            //Code version
            EditorHelper.VerticalPair(DrawCodeVer);

            //Version option
            EditorHelper.HorizontalPair(DrawVersionOption);
            OnDraw();

            //Company
            EditorHelper.VerticalPair(DrawCompany);

            //ProductName
            EditorHelper.VerticalPair(DrawProductName);

            //Identifier
            EditorHelper.VerticalPair(DrawIdentifier);

            //Ab res relative
            EditorHelper.VerticalPair(DrawABResInfo);

            //Pack option
            EditorHelper.VerticalPair(DrawPackOption);

            //Save build setting data
            EditorHelper.VerticalPair(
                                      () => {
                                          if (GUILayout.Button("保存")){
                                              Data.Save();
                                          }
                                      }
                                     );
        }

        /// <summary>
        /// Setting
        /// </summary>
        private void DrawPlatformSelect(){
            DefaultSpace();
            Data.TargetPlat = (BuildTarget)EditorHelper.HasTitleField("Build Setting", "Build Platform", Data.TargetPlat);
            SetParameters();
        }

        /// <summary>
        /// Version
        /// </summary>
        private void DrawVersion(){
            Data.Version = EditorHelper.HasTitleField("Version", Data.Version, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
        }

        /// <summary>
        /// Code ver
        /// </summary>
        private void DrawCodeVer(){
            Data.CodeVer = EditorHelper.HasTitleField("脚本 Version(脚本Hotfix用)", Data.CodeVer, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            if (GUILayout.Button("升级脚本Version")){
                Data.CodeVer = int.Parse(DateTime.Now.ToString(StringUtils.DATE_TIME_FORMAT_FULL));
                // Data.CodeVer++;
            }
        }

        /// <summary>
        /// 资源改动操作
        /// </summary>
        private void DrawVersionOption(){
            if (GUILayout.Button("游戏巨大更新")){
                UpdateBuildVersion(0);
            }

            DefaultSpace();
            if (GUILayout.Button("升级大版本号")){
                UpdateBuildVersion(1);
            }

            DefaultSpace();
            if (GUILayout.Button("升级小版本号")){
                UpdateBuildVersion(2);
            }
        }

        /// <summary>
        /// Company
        /// </summary>
        private void DrawCompany(){
            Data.CompanyName = EditorHelper.HasTitleField("Company", Data.CompanyName, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
        }

        /// <summary>
        /// Product
        /// </summary>
        private void DrawProductName(){
            Data.ProductName = EditorHelper.HasTitleField("ProductName", Data.ProductName, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
        }

        /// <summary>
        /// Identifier
        /// </summary>
        private void DrawIdentifier(){
            Data.ApplicationIdentifier = EditorHelper.HasTitleField("ApplicationIdentifier", Data.ApplicationIdentifier, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
        }

        /// <summary>
        /// AB资源相关
        /// </summary>
        private void DrawABResInfo(){
            //设置资源目录
            AssetSystemConfig.Instance.ABDIR_URL = EditorHelper.HasTitleSelectFolderPathHorizontal("AB资源目录", AssetSystemConfig.Instance.ABDIR_URL, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            AssetSystemConfig.Instance.ABInfoFile = EditorHelper.HasTitleField("AB资源表名", AssetSystemConfig.Instance.ABInfoFile, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            AssetSystemConfig.Instance.ABPackResMap = EditorHelper.HasTitleField("AB资源映射表名", AssetSystemConfig.Instance.ABPackResMap, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            AssetSystemConfig.Instance.ABTotalMap = EditorHelper.HasTitleField("AB总资源表名", AssetSystemConfig.Instance.ABTotalMap, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            EditorHelper.HorizontalPair(DrawPackRes);
        }

        /// <summary>
        /// 资源打包操作
        /// </summary>
        private void DrawPackRes(){
            if (GUILayout.Button("自动标记资源地址")){
                AssetBuilder.SetABLabels();
                EditorUtility.DisplayDialog("自动标记", "自动标记成功", "确定");
            }

            DefaultSpace();
            if (GUILayout.Button("打包资源")){
                AssetBuilder.BuildAllAB();
            }
        }

        /// <summary>
        /// 游戏出包操作
        /// </summary>
        private void DrawPackOption(){
            if (GUILayout.Button("直接出包")){
                isBuildSuccess = true;
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                DirecBuildTotalPack();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                if (isBuildSuccess){
                    if (EditorUtility.DisplayDialog("一键打包完成", "一键打包完成", "确定")){
                        EditorUtility.RevealInFinder(BuildSettingData.ProjectOutFile());
                    }
                }
                else{
                    if (EditorUtility.DisplayDialog("打包失败", "请检测报错信息", "确定")){
                        EditorUtility.RevealInFinder(BuildSettingData.ProjectOutFile());
                    }
                }
            }
        }

        private void DefaultSpace(){ GUILayout.Space(WINDOWS_SPACE_DEFAULT); }

        private void SetParameters(){
            if (PlayerSettings.Android.bundleVersionCode != Data.CodeVer){
                PlayerSettings.Android.bundleVersionCode = Data.CodeVer;
            }

            if (PlayerSettings.bundleVersion != Data.Version){ PlayerSettings.bundleVersion = Data.Version; }

            if (PlayerSettings.companyName != Data.CompanyName){ PlayerSettings.companyName = Data.CompanyName; }

            if (PlayerSettings.productName != Data.ProductName){ PlayerSettings.productName = Data.ProductName; }

            if (PlayerSettings.allowUnsafeCode != Data.AllowUnsafeCode){
                PlayerSettings.allowUnsafeCode = Data.AllowUnsafeCode;
            }

            if (PlayerSettings.stripEngineCode != Data.StripEngineCode){
                PlayerSettings.stripEngineCode = Data.StripEngineCode;
            }

            if (PlayerSettings.applicationIdentifier != Data.ApplicationIdentifier){
                PlayerSettings.applicationIdentifier = Data.ApplicationIdentifier;
            }
        }

        public static string ToRelativeAssetPath(string s){ return s.Substring(s.IndexOf("Assets/")); }

        /// <summary>
        /// 直接出包
        /// </summary>
        private void DirecBuildTotalPack(){
            //TODO:全远程包需要删除StreamingAssetPath下边的所有内容，本地包可以不用
            // BuildTool.DeleteFolder(Application.streamingAssetsPath);
            string[]    outScenes    = GetBuildScenes();
            BuildTarget target       = GetActiveBuildTarget();
            var         buildOptions = BuildOptions.None;
            Debug.Log($"Build 平台:{EditorUserBuildSettings.selectedBuildTargetGroup}");
            //TODO:空了来修改平台
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions(){
                                                                                // scenes = new string[] { "Assets/Scenes/main.unity" },
                                                                                scenes           = outScenes,
                                                                                locationPathName = BuildSettingData.ProjectOutFile() + ".apk",
                                                                                options          = buildOptions,
                                                                                target           = target,
                                                                                targetGroup      = EditorUserBuildSettings.selectedBuildTargetGroup,
                                                                            };
            Debug.Log("====> 摘抄Huatuo 第1次 Build App(为了生成补充AOT元数据dll)");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            // Debug.Log("====> Build AssetBundle");
            // AssetBundleBuildHelper.BuildAssetBundleByTarget(target);
            // Debug.Log("====> 第2次打包 导出正确包体");
            // BuildPipeline.BuildPlayer(buildPlayerOptions);
            this.isBuildSuccess = report.summary.result == BuildResult.Succeeded;
            if (this.isBuildSuccess){
                Application.OpenURL($"file:///{BuildSettingData.ProjectOutFile()}");
            }
        }

        private string[] GetBuildScenes(){
            List<string> pathList = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes){
                if (scene.enabled){
                    pathList.Add(scene.path);
                }
            }

            return pathList.ToArray();
        }

        /// <summary>
        /// 更新版本号，需要强制更新
        /// </summary>
        private void UpdateBuildVersion(int verIndex){
            string[] ver = Data.Version.Split('.');
            switch (verIndex){
                case 0:
                    Data.Version = (int.Parse(ver[0]) + 1) + ".0" + ".0";
                    break;
                case 1:
                    Data.Version = ver[0] + "." + (int.Parse(ver[1]) + 1) + ".0";
                    break;
                case 2:
                    Data.Version = ver[0] + "." + ver[1] + "." + (int.Parse(ver[2]) + 1);
                    break;
            }
        }

        /// <summary>
        /// 更新资源号，不用强制更新安装包
        /// </summary>
        private void UpdateAssetVersion(){
            string[] ver = Data.Version.Split('.');
            Data.Version = ver[0] +
                           "."    +
                           ver[1] +
                           "."    +
                           (ver.Length > 2 ? (int.Parse(ver[2]) + 1) : int.Parse(string.Empty));

            // string[] ver = version.Split('.');
            // version = ver[0] + "." + ver[1] + "." + (int.Parse(ver[2]) + 1);
        }

       

        /// <summary>
        ///获取到unity中当前切换的平台 
        /// </summary>
        /// <returns></returns>
        public static BuildTarget GetActiveBuildTarget(){ return EditorUserBuildSettings.activeBuildTarget; }
      

      
    }
}
#endif