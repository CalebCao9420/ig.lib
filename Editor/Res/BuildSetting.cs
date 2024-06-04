using System;
using System.Collections.Generic;
using System.IO;
using IG.AssetBundle;
using IG.IO;
using IG.Runtime.Common;
using IG.Runtime.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Build.Reporting;
using IG.Editor.Helper;
using UnityEditor;

namespace IG.Editor.Res{
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
            Data.ABResDir = EditorHelper.HasTitleSelectFolderPathHorizontal("AB资源目录", Data.ABResDir, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            Data.ABInfoFile = EditorHelper.HasTitleField("AB资源表名", Data.ABInfoFile, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            Data.ABPackResMap = EditorHelper.HasTitleField("AB资源映射表名", Data.ABPackResMap, WINDOWS_WIDTH_DEFAULT, WINDOWS_HEIGHT_DEFAULT);
            DefaultSpace();
            EditorHelper.HorizontalPair(DrawPackRes);
        }

        /// <summary>
        /// 资源打包操作
        /// </summary>
        private void DrawPackRes(){
            if (GUILayout.Button("自动标记资源地址")){
                SetABLabels();
                EditorUtility.DisplayDialog("自动标记", "自动标记成功", "确定");
            }

            DefaultSpace();
            if (GUILayout.Button("打包资源")){
                BuildAllAB();
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

        public static Dictionary<string, AssetBundleConfig> bundleMap   = null; //存储设置label的ab配置数据
        public static Dictionary<string, List<string>>      bundlePacks = null;

        /// <summary>
        /// 打包生成所有AssetBundles
        /// </summary>
        [MenuItem("AssetManager/2.BuildAllAssetBundles")]
        public static void BuildAllAB(){
            //(打包)AB的输出路径
            string      strABOutPathDIR = "";
            BuildTarget buildTarget     = GetActiveBuildTarget();
            strABOutPathDIR = BuildSettingData.GetABOutFile();
            if (!Directory.Exists(strABOutPathDIR)){
                Directory.CreateDirectory(strABOutPathDIR);
            }

            //打包生成
            BuildPipeline.BuildAssetBundles(strABOutPathDIR, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
            // BuildPipeline.BuildAssetBundles(strABOutPathDIR, BuildAssetBundleOptions.None, buildTarget);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            OnPostBuild(bundleMap, ref strABOutPathDIR, BuildSettingData.Instance.ABInfoFile);
            Debug.Log("AB资源打包完成-文件路径：" + strABOutPathDIR);
        }

        /// <summary>
        /// （自动）给资源文件（预设）添加标记
        /// </summary>
        [MenuItem("AssetManager/1.Set AB Label")]
        public static void SetABLabels(){
            bundleMap   = new Dictionary<string, AssetBundleConfig>();
            bundlePacks = new Dictionary<string, List<string>>();
            //需要给AB做标记的根目录
            string strNeedSetABLableRootDIR = "";
            //目录信息
            DirectoryInfo[] dirScenesDIRArray = null;

            //清空无用AB标记
            AssetDatabase.RemoveUnusedAssetBundleNames();
            //定位需要打包资源的文件夹根目录。
            strNeedSetABLableRootDIR = BuildSettingData.Instance.ABResDir;
            DirectoryInfo dirTempInfo = new DirectoryInfo(strNeedSetABLableRootDIR);
            dirScenesDIRArray = dirTempInfo.GetDirectories();

            //on set ab label pre action
            OnPreSetABLabel(strNeedSetABLableRootDIR);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            //遍历常规资源每个目录
            foreach (DirectoryInfo currentDIR in dirScenesDIRArray){
                //遍历目录下的所有的文件,
                //如果是目录，则继续递归访问里面的文件，直到定位到文件。
                string tmpDIR = strNeedSetABLableRootDIR + "/" + currentDIR.Name; //res/**
                // DirectoryInfo tmpScenesDIRInfo = new DirectoryInfo(tmpDIR);
                // int           tmpIndex         = tmpDIR.LastIndexOf("/");
                // string        tmpName          = tmpDIR.Substring(tmpIndex + 1);
                //递归调用与处理目录或文件系统，如果找到文件，修改AssetBundle 的标签（label）
                JudgeDIROrFileByRecursive(currentDIR, currentDIR.Name);
            } //foreach_end

            //将生成的bundle名与资源名添加到json文件
            List<AssetBundleConfig> list        = bundleMap.Values();
            var                     jsonContent = JSONUtils.ObjectToJson(list);
            FileManager.WriteString(BuildSettingData.GetABBundleInfo(), jsonContent);
            Debug.Log("Json数据配置完成！");
            //刷新
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            //提示
            Debug.Log("AssetBundles 标签设置完成！");
        }

        /// <summary>
        /// 递归调用与处理目录或文件系统
        /// 1：如果是目录，则进行递归调用。
        /// 2：如果是文件，则给文件做“AB标记”
        /// </summary>
        /// <param name="dirInfo">目录信息</param>
        /// <param name="packName">场景名称</param>
        private static void JudgeDIROrFileByRecursive(FileSystemInfo fileSysInfo, string packName){
            if (!fileSysInfo.Exists){
                Debug.LogError("文件或目录名称： " + fileSysInfo.Name + " 不存在，请检查！");
                return;
            }

            //得到当前目录下一级的文件信息集合
            DirectoryInfo    dirInfoObj   = fileSysInfo as DirectoryInfo;
            FileSystemInfo[] fileSysArray = dirInfoObj.GetFileSystemInfos();
            foreach (FileSystemInfo fileInfo in fileSysArray){
                FileInfo fileInfoObj = fileInfo as FileInfo;
                //文件类型
                if (fileInfoObj != null){
                    //修改此文件的AssetBundle的标签
                    //Old 处理是每个File的直系目录作为ab
                    // SetFileABLabel(fileInfoObj, dirInfoObj.Name);
                    //New 处理是更改为每个File的直属于AB的根目录为ab，例如(ABRes/Prefabs/Character/Effect/Slash.prefab, ab就是Prefabs)
                    SetFileABLabel(fileInfoObj, packName);
                }
                //目录类型
                else{
                    if (fileInfo.Name.Equals(packName)){
                        continue;
                    }

                    string nextFolder = packName;
                    //递归下一层
                    if (fileInfo is DirectoryInfo tmpDIR){
                        int    tmpIndex = tmpDIR.FullName.LastIndexOf("\\");
                        string tmpName  = tmpDIR.FullName.Substring(tmpIndex + 1);
                        nextFolder += $"/{tmpName}";
                    }

                    JudgeDIROrFileByRecursive(fileInfo, nextFolder);
                }
            }
        }

        /// <summary>
        /// 修改文件的AssetBundle 标记
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="scenesName">场景名称</param>
        private static void SetFileABLabel(FileInfo fileInfo, string packName){
            //AssetBundle 包名称
            string strABName = string.Empty;
            //(资源)文件路径（相对路径）
            string strAssetFilePath = string.Empty;

            //参数检查
            if (fileInfo.Extension == ".meta"){ return; }

            //得到AB包名
            //  strABName = GetABName(fileInfo, prefabName).ToLower();
            /* 使用AssetImporter 类，修改名称与后缀 */
            //获取资源文件相对路径
            int tmpIndex = fileInfo.FullName.IndexOf("Assets");
            strAssetFilePath = fileInfo.FullName.Substring(tmpIndex);
            //给资源文件设置AB名称与后缀
            AssetImporter tmpAssetImportObj = AssetImporter.GetAtPath(strAssetFilePath);
            tmpAssetImportObj.assetBundleName = packName.ToLower(); //设置AB包名
            if (fileInfo.Extension.Equals(PathConst.Suffix.UNITY))  //设置AB包扩展名称 
            {
                tmpAssetImportObj.assetBundleVariant = StringUtils.GetSuffix(PathConst.Suffix.U3D, '.');
            }
            else{
                tmpAssetImportObj.assetBundleVariant = StringUtils.GetSuffix(PathConst.Suffix.BUNDLE, '.'); //AB资源包
            }

            // //将数据配置存储以备更新json中配置数据
            int    dex         = strAssetFilePath.LastIndexOf("\\");
            string abnameKey   = strAssetFilePath.Substring(dex + 1).Replace(PathConst.Suffix.PREFAB, "");
            string abnameValue = tmpAssetImportObj.assetBundleName + "." + tmpAssetImportObj.assetBundleVariant;
            if (!bundleMap.TryGetValue(abnameValue, out var value)){
                AssetBundleConfig valueNode = new AssetBundleConfig{ Name = abnameValue, CRC = 0, Size = 0 };
                bundleMap.Add(abnameValue, valueNode);
            }
            else{
                value.Size += fileInfo.Length;
            }

            if (!bundlePacks.ContainsKey(abnameValue)){
                List<string> valueNode = new List<string>(){ abnameKey };
                bundlePacks.Add(abnameValue, valueNode);
            }
            else{
                bundlePacks[abnameValue].Add(abnameKey);
            }
        }

        /// <summary>
        ///获取到unity中当前切换的平台 
        /// </summary>
        /// <returns></returns>
        private static BuildTarget GetActiveBuildTarget(){ return EditorUserBuildSettings.activeBuildTarget; }

        /// <summary>
        /// Set AB label前把热更脚本copy过来
        /// </summary>
        /// <param name="path"></param>
        private static void OnPreSetABLabel(string strNeedSetABLableRootDIR){
            //加入华佗热更后，额外加入的将dll文件copy到hotfix文件在set标签
            //TODO:Hybrid 热更接入部分
            // string strABOutPathDIR = $"{strNeedSetABLableRootDIR}HotFix";
            // BuildTarget buildTarget = GetActiveBuildTarget();
            // CompileDllCommand.CompileDllActiveBuildTarget();
            // // string hotfixDllSrcDir = HybridCLR.Editor.BuildConfig.GetHotFixDllsOutputDirByTarget(buildTarget);
            // string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget);
            // foreach (var dll in SettingsUtil.HotUpdateAssemblyFiles){
            //     string dllPath = $"{hotfixDllSrcDir}/{dll}";
            //     string dllBytesPath = $"{strABOutPathDIR}/{dll}.bytes";
            //     File.Copy(dllPath, dllBytesPath, true);
            // }
            //
            // // string aotDllDir = HybridCLR.Editor.BuildConfig.GetAssembliesPostIl2CppStripDir(buildTarget);
            // string aotDllDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            // foreach (var dll in DllLoader.AOTMetaAssemblies){
            //     string dllPath = $"{aotDllDir}/{dll}";
            //     if (!File.Exists(dllPath)){
            //         Debug.LogError($"ab中添加AOT补充元数据dll:{dllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
            //         continue;
            //     }
            //
            //     string dllBytesPath = $"{strABOutPathDIR}/{dll}.bytes";
            //     File.Copy(dllPath, dllBytesPath, true);
            //     Debug.Log($"补充AOT meta {dllPath}");
            // }
        }

        /// <summary>
        /// 完成Ab资源Build后，再次回去修改ab_info.table内容
        /// </summary>
        /// <param name="configMap"></param>
        /// <param name="path"></param>
        /// <param name="buildPath"></param>
        private static void OnPostBuild(Dictionary<string, AssetBundleConfig> configMap, ref string path, string buildPath){
            EditorUtility.ClearProgressBar();
            List<AssetBundleConfig> configs      = new List<AssetBundleConfig>();
            string                  tempFilePath = string.Empty;
            foreach (AssetBundleConfig config in configMap.Values){
                if (configs.Contains(config)){
                    continue;
                }

                //recalculate size and crc
                if (config.CRC <= 0){
                    tempFilePath = string.Format(FORMAT_PATH, path, config.Name);
                    BuildPipeline.GetCRCForAssetBundle(tempFilePath, out config.CRC);
                }

                string title = string.Format(GUI_POST_PROCESS_TITLE, configs.Count + 1, configMap.Count);
                string info  = string.Format(GUI_POST_PROCESS_INFO,  config.Name);
                EditorUtility.DisplayProgressBar(GUI_POST_PROCESS_TITLE, info, (float)(configs.Count + 1) / configMap.Count);
                configs.Add(config);
            }

            string manifestFilePath = string.Format(FORMAT_PATH, path, BuildSettingData.Instance.AB_MANIFEST);
            if (!File.Exists(manifestFilePath)){
                // throw new NullReferenceException("manifest不能为空!");
                EditorUtility.DisplayDialog("警告", "manifest不能为空!", "确定");
                EditorUtility.ClearProgressBar();
                return;
            }

            //添加manifest
            InsertManifest(configs, manifestFilePath);
            //添加ab整包
            InsertABPackConfig(configs, path);
            string json = JSONUtils.ObjectToJson(configs);
            File.WriteAllText(BuildSettingData.GetABBundleInfo(), json);
            EditorUtility.ClearProgressBar();
        }

        private static void InsertManifest(List<AssetBundleConfig> configs, string manifestFilePath){
            AssetBundleConfig manifestConfig = new AssetBundleConfig();
            manifestConfig.Name = BuildSettingData.Instance.AB_MANIFEST;
            BuildPipeline.GetCRCForAssetBundle(manifestFilePath, out manifestConfig.CRC);
            manifestConfig.Size = new FileInfo(manifestFilePath).Length;
            configs.Insert(0, manifestConfig);
        }

        private static void InsertABPackConfig(List<AssetBundleConfig> configs, string path){
            string            bundleName = BuildSettingData.GetPlatformABDirName();
            string            filePath   = string.Format(FORMAT_PATH, path, bundleName);
            AssetBundleConfig abConfig   = new AssetBundleConfig();
            abConfig.Name = bundleName;
            BuildPipeline.GetCRCForAssetBundle(filePath, out abConfig.CRC);
            abConfig.Size = new FileInfo(filePath).Length;
            configs.Insert(0, abConfig);
        }
    }
}
#endif