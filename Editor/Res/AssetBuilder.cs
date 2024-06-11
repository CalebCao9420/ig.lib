using System.Collections.Generic;
using System.IO;
using IG;
using IG.AssetBundle;
using IG.Editor;
using IG.IO;
using IG.Runtime.Common;
using IG.Runtime.Extensions;
using IG.Runtime.Utils;
using UnityEngine;

namespace ig.lib.Editor.Res{
#if UNITY_EDITOR
    using UnityEditor;

    public static class AssetBuilder{
        private const string FORMAT_PATH            = "{0}/{1}";
        private const string GUI_PRE_PROCESS_TITLE  = "Build Preprocess [{0}/{1}]";
        private const string GUI_PRE_PROCESS_INFO   = "Preprocessing...{0}";
        private const string GUI_POST_PROCESS_TITLE = "Build Postprocess [{0}/{1}]";
        private const string GUI_POST_PROCESS_INFO  = "Postprocessing...{0}";
        private const string NAME_BUILD_COMPLETE    = "Build Complete";
        private const string NAME_BUILD_TIME        = "Build cost time:{0}.";

    #region static dynamic data

        public static Dictionary<string, AssetBundleConfig> S_BundleMap    = null; //存储设置label的ab配置数据
        public static Dictionary<string, string>            S_ResBundleMap = null; //key:资源完整路径 value:bundle名
        public static Dictionary<string, List<string>>      S_BundlePacks  = null;

    #endregion

        /// <summary>
        /// Bundle 信息输出目录
        /// </summary>
        /// <returns></returns>
        public static string GetABBundleInfo(){
            return BuildSettingData.GetPlatformPath() + "/" + AssetSystemHelper.GetPlatformABDirName(BuildSettingData.Instance.TargetPlat) + "/" + AssetSystemConfig.Instance.ABInfoFile;
        }

        /// <summary>
        /// 资源-bundle 映射表目录
        /// </summary>
        /// <returns></returns>
        public static string GetABBundleMapInfo(){
            return BuildSettingData.GetPlatformPath() + "/" + AssetSystemHelper.GetPlatformABDirName(BuildSettingData.Instance.TargetPlat) + "/" + AssetSystemConfig.Instance.ABPackResMap;
        }

        /// <summary>
        /// 总资源表目录
        /// </summary>
        /// <returns></returns>
        public static string GetABTotalBundleInfo(){
            return BuildSettingData.GetPlatformPath() + "/" + AssetSystemHelper.GetPlatformABDirName(BuildSettingData.Instance.TargetPlat) + "/" + AssetSystemConfig.Instance.ABTotalMap;
        }

        /// <summary>
        /// 打包生成所有AssetBundles
        /// </summary>
        [MenuItem("AssetManager/2.BuildAllAssetBundles")]
        public static void BuildAllAB(){
            //(打包)AB的输出路径
            string      strABOutPathDIR = "";
            BuildTarget buildTarget     = BuildSettings.GetActiveBuildTarget();
            strABOutPathDIR = BuildSettingData.GetABOutFile();
            if (!Directory.Exists(strABOutPathDIR)){
                Directory.CreateDirectory(strABOutPathDIR);
            }

            //打包生成
            BuildPipeline.BuildAssetBundles(strABOutPathDIR, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget);
            // BuildPipeline.BuildAssetBundles(strABOutPathDIR, BuildAssetBundleOptions.None, buildTarget);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            OnPostBuild(strABOutPathDIR);
            Debug.Log("AB资源打包完成-文件路径：" + strABOutPathDIR);
        }

        /// <summary>
        /// （自动）给资源文件（预设）添加标记
        /// </summary>
        [MenuItem("AssetManager/1.Set AB Label")]
        public static void SetABLabels(){
            S_BundleMap    = new();
            S_BundlePacks  = new();
            S_ResBundleMap = new();
            //需要给AB做标记的根目录
            string strNeedSetABLableRootDIR = "";
            //目录信息
            DirectoryInfo[] dirScenesDIRArray = null;

            //清空无用AB标记
            AssetDatabase.RemoveUnusedAssetBundleNames();
            //定位需要打包资源的文件夹根目录。
            strNeedSetABLableRootDIR = AssetSystemConfig.Instance.ABDIR_URL;
            DirectoryInfo dirTempInfo = new DirectoryInfo(strNeedSetABLableRootDIR);
            dirScenesDIRArray = dirTempInfo.GetDirectories();

            //on set ab label pre action
            OnPreSetABLabel(strNeedSetABLableRootDIR);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            //遍历常规资源每个目录
            foreach (DirectoryInfo currentDIR in dirScenesDIRArray){
                //遍历目录下的所有的文件,
                //如果是目录，则继续递归访问里面的文件，直到定位到文件。
                // string tmpDIR = strNeedSetABLableRootDIR + "/" + currentDIR.Name; //res/**
                // DirectoryInfo tmpScenesDIRInfo = new DirectoryInfo(tmpDIR);
                // int           tmpIndex         = tmpDIR.LastIndexOf("/");
                // string        tmpName          = tmpDIR.Substring(tmpIndex + 1);
                //递归调用与处理目录或文件系统，如果找到文件，修改AssetBundle 的标签（label）
                JudgeDIROrFileByRecursive(currentDIR, currentDIR.Name);
            } //foreach_end

            //Post execute
            OnPostSetABLabel();
            //刷新
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            //提示
            Debug.Log("AssetBundles 标签设置完成！".CyanColor());
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
            //add in bundle map
            if (!S_BundleMap.TryGetValue(abnameValue, out var value)){
                AssetBundleConfig valueNode = new AssetBundleConfig{ Name = abnameValue, CRC = 0, Size = 0 };
                S_BundleMap.Add(abnameValue, valueNode);
            }
            else{
                value.Size += fileInfo.Length;
            }

            //add in res bundle map
            //res-bundle资源映射需要把bundle一起处理到key
            string remakeFilePath = strAssetFilePath.Replace('\\', '/');
            int    bundleIndex    = remakeFilePath.IndexOf(packName);
            string resBundleKey   = remakeFilePath.Substring(bundleIndex);
            // S_ResBundleMap.Add(abnameKey, abnameValue);
            S_ResBundleMap.Add(resBundleKey, abnameValue);

            //add in bundle pack
            if (!S_BundlePacks.ContainsKey(abnameValue)){
                List<string> valueNode = new List<string>(){ abnameKey };
                S_BundlePacks.Add(abnameValue, valueNode);
            }
            else{
                S_BundlePacks[abnameValue].Add(abnameKey);
            }
        }

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
        /// Set ab label 后处理
        /// </summary>
        private static void OnPostSetABLabel(){
            RecordPreBundleInfo(); //Record bundle
            RecordResBundleMap();  //Record res-bundle 
            RecordTotalMap();      //Record total bundle 
        }

        /// <summary>
        /// 记录所有资源表
        /// </summary>
        private static void RecordTotalMap(){
            //将生成的bundle名与资源名添加到json文件
            var map         = S_BundlePacks;
            var jsonContent = JSONUtils.ObjectToJson(map);
            FileManager.WriteString(AssetBuilder.GetABTotalBundleInfo(), jsonContent);
            Debug.Log("Total Res Map数据配置完成！".GreenColor());
        }

        /// <summary>
        /// 记录资源-bundle映射表
        /// </summary>
        private static void RecordResBundleMap(){
            //将生成的bundle名与资源名添加到json文件
            var map         = S_ResBundleMap;
            var jsonContent = JSONUtils.ObjectToJson(map);
            FileManager.WriteString(AssetBuilder.GetABBundleMapInfo(), jsonContent);
            Debug.Log("Res-Bundle Map数据配置完成！".GreenColor());
        }

        /// <summary>
        /// 记录bundle信息
        /// </summary>
        private static void RecordPreBundleInfo(){
            //将生成的bundle名与资源名添加到json文件
            List<AssetBundleConfig> list        = S_BundleMap.Values();
            var                     jsonContent = JSONUtils.ObjectToJson(list);
            FileManager.WriteString(AssetBuilder.GetABBundleInfo(), jsonContent);
            Debug.Log("Bundle数据配置完成！".GreenColor());
        }

        /// <summary>
        /// 完成Ab资源Build后，再次回去修改ab_info.table内容
        /// </summary>
        /// <param name="configMap"></param>
        /// <param name="path"></param>
        /// <param name="buildPath"></param>
        private static void OnPostBuild(string path){
            EditorUtility.ClearProgressBar();
            RecordBundleInfo(path);
            EditorUtility.ClearProgressBar();
        }

        private static void RecordBundleInfo(string outDir){
            var                     configMap    = S_BundleMap;
            List<AssetBundleConfig> configs      = new List<AssetBundleConfig>();
            string                  tempFilePath = string.Empty;
            foreach (AssetBundleConfig config in configMap.Values){
                if (configs.Contains(config)){
                    continue;
                }

                //recalculate size and crc
                if (config.CRC <= 0){
                    tempFilePath = string.Format(FORMAT_PATH, outDir, config.Name);
                    BuildPipeline.GetCRCForAssetBundle(tempFilePath, out config.CRC);
                }

                string title = string.Format(GUI_POST_PROCESS_TITLE, configs.Count + 1, configMap.Count);
                string info  = string.Format(GUI_POST_PROCESS_INFO,  config.Name);
                EditorUtility.DisplayProgressBar(GUI_POST_PROCESS_TITLE, info, (float)(configs.Count + 1) / configMap.Count);
                configs.Add(config);
            }

            string manifestFilePath = string.Format(FORMAT_PATH, outDir, BuildSettingData.Instance.AB_MANIFEST);
            if (!File.Exists(manifestFilePath)){
                // throw new NullReferenceException("manifest不能为空!");
                EditorUtility.DisplayDialog("警告", "manifest不能为空!", "确定");
                EditorUtility.ClearProgressBar();
                return;
            }

            //添加manifest
            InsertManifest(configs, manifestFilePath);
            //添加ab整包
            InsertABPackConfig(configs, outDir);
            string json = JSONUtils.ObjectToJson(configs);
            File.WriteAllText(AssetBuilder.GetABBundleInfo(), json);
        }

        private static void InsertManifest(List<AssetBundleConfig> configs, string manifestFilePath){
            AssetBundleConfig manifestConfig = new AssetBundleConfig();
            manifestConfig.Name = BuildSettingData.Instance.AB_MANIFEST;
            BuildPipeline.GetCRCForAssetBundle(manifestFilePath, out manifestConfig.CRC);
            manifestConfig.Size = new FileInfo(manifestFilePath).Length;
            configs.Insert(0, manifestConfig);
        }

        private static void InsertABPackConfig(List<AssetBundleConfig> configs, string path){
            string            platABDirName = AssetSystemHelper.GetPlatformABDirName(BuildSettingData.Instance.TargetPlat);
            string            filePath      = string.Format(FORMAT_PATH, path, platABDirName);
            AssetBundleConfig abConfig      = new AssetBundleConfig();
            abConfig.Name = platABDirName;
            BuildPipeline.GetCRCForAssetBundle(filePath, out abConfig.CRC);
            abConfig.Size = new FileInfo(filePath).Length;
            configs.Insert(0, abConfig);
        }
    }
#endif
}