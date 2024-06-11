using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IG.IO;
using IG.Runtime.Common;
using IG.Runtime.Extensions;
using IG.Runtime.Log;
using IG.Runtime.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace IG.AssetBundle{
    /// <summary>
    /// 资源系统
    ///
    /// 后续计划,
    /// 1.Bundle做额外类型，内部有引用计数，方便计数<=0时卸载
    /// 2.有周期性检查，每个周期检测引用计数<=0的没卸载的包，手动卸载(周期考虑是外部来还是内部自动，主要为防止因为这里卸载影响Main的帧率等)
    /// </summary>
    public sealed partial class AssetSystem : SingletonAbs<AssetSystem>{
        /// <summary>
        /// Ab bundle manifest 资源名
        /// </summary>
        private static readonly string ASSET_MANIFEST = "AssetBundleManifest";

        private const string        FORMAT_PATH  = "{0}/{1}";
        private const string        LOG_NULL     = "Load complete callback is null.";
        private const string        LOG_GET      = "Can't find out asset bundle:{0}.";
        private const string        LOG_LOAD     = "Can't load asset:{0}";
        public static MonoBehaviour CoroutineObj = null;
        private       AssetMode     _mode;

        /// <summary>
        /// 资源总表
        /// </summary>
        private AssetBundleManifest _manifest;

        /// <summary>
        /// 资源地址
        /// </summary>
        private string _assetURL;

        /// <summary>
        /// 资源包表 [键:资源包名 值:资源包]
        /// </summary>
        private readonly Dictionary<string, UnityEngine.AssetBundle> _assetBundleMap = new();

        /// <summary>
        /// 资源请求表[键:资源包名 值:资源请求]
        /// </summary>
        private readonly Dictionary<string, AssetBundleCreateRequest> _requestMap = new();

        private Queue<LoadInfo> _loadQueue = new();

        private static void CheckCoroutineObj(){
            // 提交协程处理
            if (CoroutineObj == null){
                GameObject singleMonoBehaviour = GameObject.Find(SingletonManager.SINGLE_MONO_NAME);
                if (singleMonoBehaviour != null){
                    CoroutineObj = singleMonoBehaviour.GetOrAddComponent<NotDestroy>();
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetURL">资源地址</param>
        /// <param name="mainBundleName">资源包</param>
        public static void Setup(AssetSystemConfig config){
            Instance._mode = config.AssetLoadMode;
            string assetURL       = config.URL;
            string mainBundleName = config.MainBundleName;
            Clear();
            Instance._assetURL = assetURL;
            string mainPkgPath = GetPath(mainBundleName);
            // mainPkgPath = JudgeBundleFixSuffix(mainPkgPath);
            if (File.Exists(mainPkgPath)){
                UnityEngine.AssetBundle assetBundle = UnityEngine.AssetBundle.LoadFromFile(mainPkgPath);
                Instance._manifest = assetBundle.LoadAsset<AssetBundleManifest>(ASSET_MANIFEST);
                Add(mainBundleName, assetBundle);
                LogHelper.Log($"AssetSystem! 初始化成功!".GreenColor());
            }
            else{
                LogHelper.Log($"AssetSystem初始化错误,{mainBundleName} 不存在!!", LogType.Error);
            }
        }

        /// <summary>
        /// 添加资源包
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="assetBundle">资源包</param>
        public static void Add(string bundleName, UnityEngine.AssetBundle assetBundle){ Instance._assetBundleMap[bundleName] = assetBundle; }

        /// <summary>
        /// 移除资源包
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="unload">是否释放资源</param>
        public static void Remove(string bundleName, bool unload = true){
            UnityEngine.AssetBundle assetBundle = null;
            if (!Instance._assetBundleMap.TryGetValue(bundleName, out assetBundle)){
                return;
            }

            if (unload){
                assetBundle.Unload(true);
            }

            Instance._assetBundleMap.Remove(bundleName);
        }

        /// <summary>
        /// 获取资源路径
        /// </summary>
        /// <param name="sourceName">资源名</param>
        private static string GetPath(string sourceName){
            switch (Instance._mode){
                case AssetMode.Editor:
                case AssetMode.EditorPkg:
                    return string.Format(FORMAT_PATH, Instance._assetURL, sourceName);
                case AssetMode.Local:
                case AssetMode.Remote:
                case AssetMode.LocalAndRemote:
                    string url = Instance._assetURL + sourceName;
                    string key;
                    DownloadSystem.HasCached(url, out key, out FileInfo fileInfo);
                    return fileInfo.FullName;
            }

            return string.Empty;
        }

        private static string JudgeBundleFixSuffix(string path){
            if (!path.Contains(PathConst.Suffix.BUNDLE)){
                path += PathConst.Suffix.BUNDLE;
            }

            return path;
        }

        /// <summary>
        /// 获取资源包
        /// 需要带后缀名
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <returns>返回资源包</returns>
        public static UnityEngine.AssetBundle Get(string bundleName){
            if (Instance._assetBundleMap.TryGetValue(bundleName, out var assetBundle)){
                return assetBundle;
            }
            else if (Instance._manifest != null){
                string[] dependencies = Instance._manifest.GetDirectDependencies(bundleName);
                string   path         = null;
                for (int i = 0; i < dependencies.Length; ++i){
                    string dependence = dependencies[i];
                    if (!Instance._assetBundleMap.ContainsKey(dependence)){
                        path = GetPath(dependence);
                        if (!File.Exists(path)){
                            LogHelper.Log(string.Format(LOG_GET, path), LogType.Error);
                            return null;
                        }

                        assetBundle = UnityEngine.AssetBundle.LoadFromFile(path);
                        Add(dependence, assetBundle);
                    }
                }

                path = GetPath(bundleName);
                if (!File.Exists(path)){
                    LogHelper.Log(string.Format(LOG_GET, path), LogType.Error);
                    return null;
                }

                assetBundle = UnityEngine.AssetBundle.LoadFromFile(path);
                Add(bundleName, assetBundle);
            }

            return assetBundle;
        }

        /// <summary>
        /// 异步获取资源包
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="onLoadComplete">加载回调</param>
        public static void GetAsync(string bundleName, Action<UnityEngine.AssetBundle> onLoadComplete){
            UnityEngine.AssetBundle assetBundle = null;
            if (Instance._assetBundleMap.TryGetValue(bundleName, out assetBundle)){
                if (onLoadComplete != null){
                    onLoadComplete.Invoke(assetBundle);
                }
            }
            else{
                List<string> dependenList = new List<string>();
                GetAllDependencies(bundleName, dependenList);
                CheckCoroutineObj();
                //游戏主控Mono 去开启这个携程
                CoroutineObj.StartCoroutine(
                                            LoadAssetBundleCoroutine(
                                                                     dependenList,
                                                                     () => {
                                                                         try{
                                                                             Instance._assetBundleMap.TryGetValue(bundleName, out assetBundle);
                                                                             onLoadComplete.Invoke(assetBundle);
                                                                         }
                                                                         catch (Exception e){
                                                                             Debug.LogError("异步加载资源回调错误!" + bundleName + " " + e);
                                                                         }
                                                                     }
                                                                    )
                                           );
            }
        }

        /// <summary>
        /// 获取所有依赖资源
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="dependenList">依赖列表</param>
        private static void GetAllDependencies(string bundleName, List<string> dependenList){
            if (Instance._manifest == null){
                return;
            }

            string[] dependencies = Instance._manifest.GetAllDependencies(bundleName);
            for (int i = 0; i < dependencies.Length; ++i){
                string dependence = dependencies[i];
                if (dependenList.Contains(dependence)){
                    continue;
                }

                GetAllDependencies(dependence, dependenList);
            }

            dependenList.Add(bundleName);
        }

        /// <summary>
        /// 加载协程
        /// </summary>
        /// <param name="bundleList">资源包链表</param>
        /// <param name="onLoadComplete">加载回调</param>
        /// <returns></returns>
        private static IEnumerator LoadAssetBundleCoroutine(List<string> bundleList, Action onLoadComplete){
            for (int i = 0; i < bundleList.Count; ++i){
                string bundleName = bundleList[i];
                if (Instance._assetBundleMap.TryGetValue(bundleName, out var assetBundle)){
                    continue; //已经下载过了
                }

                //检测是否加载中
                if (Instance._requestMap.TryGetValue(bundleName, out var request)){
                    while (!request.isDone){
                        yield return new WaitForEndOfFrame();
                    }

                    assetBundle = request.assetBundle;
                }
                else{
                    // 检测资源是否存在
                    string path = GetPath(bundleName);
                    if (!File.Exists(path)){
                        Debug.LogError(string.Format(LOG_GET, path));
                        continue;
                    }

                    //创建加载任务
                    request = UnityEngine.AssetBundle.LoadFromFileAsync(path);
                    Instance._requestMap.Readd(bundleName, request);
                    yield return request;

                    //执行完毕回调
                    assetBundle = request.assetBundle;
                    Add(bundleName, assetBundle);
                    Instance._requestMap.Remove(bundleName);
                }
            }

            // 执行加载回调
            if (onLoadComplete != null){
                try{
                    onLoadComplete.Invoke();
                }
                catch (Exception e){
                    Debug.LogError("加载回调执行错误！" + e);
                }
            }
        }

        /// <summary>
        /// 加载资源(资源类型明确)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>(string path) where T : Object{
            HookBundleAndAssetName(path, out string bundleName, out string name);
            UnityEngine.AssetBundle bundle = Get(bundleName);
            //正常情况无法加载，连URL一起转一次MD5,查找本地资源
            if (bundle == null){
                LogHelper.Log($"[无法获取确认一下]:{bundleName}", LogType.Error);
                string url      = Instance._assetURL + AssetSystemHelper.GetPlatformABDirName() + bundleName;
                var    fileName = FileManager.StringToMD5(url.ToLower());
                bundle = Get(fileName);
            }

            if (bundle.Contains(name)){
                return bundle.LoadAsset<T>(name);
            }

            return null;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="name">资源名</param>
        /// <param name="type">资源类型</param>
        /// <returns>返回资源</returns>
        public static Object Load(string path, Type type = null){
            HookBundleAndAssetName(path, out string bundleName, out string name);
            if (type == null){
                type = GetAssetType(ref path);
            }

            // if (!path.Contains(PathConst.Suffix.BUNDLE)){
            //     path += PathConst.Suffix.BUNDLE;
            // }
            switch (Instance._mode){
                case AssetMode.Editor:
#if UNITY_EDITOR

                    //如果Editor没有Local_AB就只有path
                    string suffix = type != null ? PathConst.S_TypeMap[type] : String.Empty;
                    if (!path.Contains(suffix)){
                        path += suffix;
                    }

                    path = $"{AssetSystemConfig.Instance.ABDIR_URL}/{path}";
                    Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
                    if (asset == null){
                        Debug.LogError(string.Format(LOG_LOAD, path));
                    }

                    return asset;
#endif
                    break;
                case AssetMode.EditorPkg:
                case AssetMode.Local:
                case AssetMode.Remote:
                case AssetMode.LocalAndRemote:
                    bundleName = FixBundleName(bundleName);
                    UnityEngine.AssetBundle assetBundle = Get(bundleName);
                    //正常情况无法加载，连URL一起转一次MD5,查找本地资源
                    if (assetBundle == null){
                        string url      = Instance._assetURL + AssetSystemHelper.GetPlatformABDirName() + bundleName;
                        var    fileName = FileManager.StringToMD5(url.ToLower());
                        assetBundle = Get(fileName);
                    }

                    if (name == null){
                        name = bundleName.Substring(bundleName.LastIndexOf('/') + 1);
                        name = name.Remove(name.LastIndexOf('.'));
                    }

                    name = name.ToLower();
                    return assetBundle.LoadAsset(name, type);
            }

            return null;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="loadComplete">加载完成回调</param>
        /// <param name="bundleName">资源包名</param>
        /// <param name="name">资源名</param>
        /// <param name="type">资源类型</param>
        /// <param name="arg">加载参数</param>
        /// <returns>返回加载请求</returns>
        public static LoadInfo LoadAsync(Action<object, object> loadComplete, string path, Type type = null, object arg = null){
            HookBundleAndAssetName(path, out string bundleName, out string name);
            // 检测回调
            if (loadComplete == null){
                Debug.LogError(LOG_NULL);
                return null;
            }

            bundleName += PathConst.Suffix.BUNDLE;
            bundleName =  bundleName.ToLower();
            if (name == null){
                name = bundleName.Substring(bundleName.LastIndexOf('/') + 1);
                name = name.Remove(name.LastIndexOf('.'));
            }

            name = name.ToLower();
            if (type == null) type = GetAssetType(ref bundleName);
            LoadInfo loadInfo = new LoadInfo(){
                                                  LoadCallback = loadComplete,
                                                  BundleName   = bundleName,
                                                  Name         = name,
                                                  Type         = type,
                                                  Arg          = arg
                                              };
            switch (Instance._mode){
                case AssetMode.Editor:
#if UNITY_EDITOR
                    Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
                    if (asset == null){
                        Debug.LogError(string.Format(LOG_LOAD, path));
                    }
                    else{
                        loadComplete.Invoke(asset, loadInfo.Arg);
                    }
#endif
                    break;
                case AssetMode.EditorPkg:
                case AssetMode.Local:
                case AssetMode.Remote:
                case AssetMode.LocalAndRemote:
                    if (Instance._loadQueue.Count > 0){
                        Instance._loadQueue.Enqueue(loadInfo);
                    }
                    else{
                        StartLoad(loadInfo);
                    }

                    break;
            }

            return loadInfo;
        }

        /// <summary>
        /// 加载指定的资源包中包含的所有资源
        /// Editor 加载要添加suffix
        /// Runtime 不管suffix
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="type"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static Object[] LoadAll(string bundleName, Type type = null, string suffix = null){
            switch (Instance._mode){
                case AssetMode.Editor:
#if UNITY_EDITOR

                    //如果Editor没有Local_AB就只有path = bundleName
                    string path = bundleName.Replace(PathConst.Suffix.BUNDLE, string.Empty);
                    if (!suffix.Contains("*")){ suffix = $"*{suffix}"; }

                    string[] files  = FileManager.GetFiles(path, suffix, true);
                    int      length = files?.Length ?? 0;
                    Object[] assets = files == null ? null : new Object[length];
                    for (int i = 0; i < length; ++i){
                        assets[i] = UnityEditor.AssetDatabase.LoadAssetAtPath(files[i], type);
                    }

                    if (assets == null){
                        Debug.LogError(string.Format(LOG_LOAD, path));
                    }

                    return assets;
#endif
                case AssetMode.EditorPkg:
                case AssetMode.Local:
                case AssetMode.Remote:
                case AssetMode.LocalAndRemote:
                    bundleName = FixBundleName(bundleName);
                    UnityEngine.AssetBundle assetBundle = Get(bundleName);
                    //正常情况无法加载，连URL一起转一次MD5,查找本地资源
                    if (assetBundle == null){
                        // string url      = Instance._assetURL + AssetSystemHelper.GetPlatformABDirName() + bundleName;
                        string url      = Instance._assetURL + "/" + bundleName;
                        var    fileName = FileManager.StringToMD5(url.ToLower());
                        assetBundle = Get(fileName);
                    }

                    return type == null ? assetBundle.LoadAllAssets() : assetBundle.LoadAllAssets(type);
            }

            return null;
        }

        /// <summary>
        /// 加载指定的文件夹下的所有资源包中包含的所有资源
        /// Editor 加载要添加suffix
        /// Runtime 不管suffix
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="type"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static Object[] LoadAllByRecursive(string dir, Type type = null, string suffix = null){
            switch (Instance._mode){
                case AssetMode.Editor:
                case AssetMode.EditorPkg:
                case AssetMode.Local:
                case AssetMode.Remote:
                case AssetMode.LocalAndRemote:
                    List<Object> rel = ListPool<Object>.GetList();
                    // dir = FixBundleName(dir);
                    if (!suffix.Contains("*")){ suffix = $"*{suffix}"; }

                    string[] allFiles = FileManager.GetFiles($"{AssetSystemConfig.Instance.URL}/{dir}", suffix, true);
                    int      len      = allFiles?.Length ?? 0;
                    for (int i = 0; i < len; ++i){
                        string single = allFiles[i];
                        single = single.Replace("\\", "/");
                        single = single.Substring(single.LastIndexOf(dir)).ToLower();
                        // single.Log($"加载资源：{single}",LogType.Error);
                        UnityEngine.AssetBundle assetBundle = Get(single);
                        //正常情况无法加载，连URL一起转一次MD5,查找本地资源
                        if (assetBundle == null){
                            // string url      = Instance._assetURL + AssetSystemHelper.GetPlatformABDirName() + bundleName;
                            string url      = Instance._assetURL + "/" + single;
                            var    fileName = FileManager.StringToMD5(url.ToLower());
                            assetBundle = Get(fileName);
                        }

                        var bundleFiles = type == null ? assetBundle.LoadAllAssets() : assetBundle.LoadAllAssets(type);
                        rel.AddRange(bundleFiles);
                    }

                    Object[] relArr = rel.ToArray();
                    rel.Recycle();
                    return relArr;
            }

            return null;
        }

        /// <summary>
        /// 异步加载所有资源
        /// </summary>
        /// <param name="loadCallback">加载完成回调</param>
        /// <param name="bundleName">资源包名</param>
        /// <param name="type">资源类型</param>
        /// <param name="arg">加载参数</param>
        /// <returns>返回加载请求</returns>
        public static LoadInfo LoadAllAsync(Action<object, object> loadCallback, string bundleName, Type type = null, object arg = null){
            if (loadCallback == null){
                Debug.LogError(LOG_NULL);
                return null;
            }

            LoadInfo loadInfo = new LoadInfo(){ LoadCallback = loadCallback, BundleName = bundleName, Type = type, Arg = arg };
            switch (Instance._mode){
                case AssetMode.Editor:
#if UNITY_EDITOR

                    //如果Editor没有Local_AB就只有path=bundleName
                    string   path   = bundleName.Replace(PathConst.Suffix.BUNDLE, string.Empty);
                    Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                    if (assets == null){
                        Debug.LogError(string.Format(LOG_LOAD, path));
                    }
                    else{
                        loadCallback.Invoke(assets, loadInfo.Arg);
                    }
#endif
                    break;
                case AssetMode.EditorPkg:
                case AssetMode.Local:
                case AssetMode.Remote:
                case AssetMode.LocalAndRemote:
                    if (Instance._loadQueue.Count > 0){
                        Instance._loadQueue.Enqueue(loadInfo);
                    }
                    else{
                        StartLoad(loadInfo);
                    }

                    break;
            }

            return loadInfo;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="unloadAll">是否卸载全部资源</param>
        public static void Unload(string bundleName, bool unloadAll){
            if (!Instance._assetBundleMap.TryGetValue(bundleName, out UnityEngine.AssetBundle assetBundle)){
                return;
            }

            assetBundle.Unload(unloadAll);
            Instance._assetBundleMap.Remove(bundleName);
        }

        /// <summary>
        /// 清除所有资源
        /// </summary>
        public static void Clear(){
            foreach (UnityEngine.AssetBundle assetBundle in Instance._assetBundleMap.Values){
                assetBundle.Unload(true);
            }

            Instance._assetBundleMap.Clear();
        }

        private static string FixBundleName(string bundleName){
            int suffixIndex = bundleName.LastIndexOf(StringUtils.PERIOD_SIGN);
            if (suffixIndex >= 0 && suffixIndex < bundleName.Length){
                string suffix = bundleName.Substring(suffixIndex);
                if (!suffix.Equals(PathConst.Suffix.BUNDLE)){
                    bundleName = StringUtils.CutSuffix(bundleName);
                }
            }

            if (!bundleName.Contains(PathConst.Suffix.BUNDLE)){
                bundleName += PathConst.Suffix.BUNDLE;
            }

            return bundleName.ToLower();
        }

        /// <summary>
        /// 开始加载
        /// </summary>
        /// <param name="loadInfo">加载信息</param>
        private static void StartLoad(LoadInfo loadInfo){
            GetAsync(
                     loadInfo.BundleName,
                     (assetBundle) => {
                         CheckCoroutineObj();
                         CoroutineObj.StartCoroutine(LoadCoroutine(loadInfo, assetBundle));
                     }
                    );
        }

        /// <summary>
        /// 加载协程
        /// </summary>
        /// <param name="loadInfo">加载信息</param>
        /// <param name="assetBundle"></param>
        /// <returns>返回迭代器</returns>
        private static IEnumerator LoadCoroutine(LoadInfo loadInfo, UnityEngine.AssetBundle assetBundle){
            AssetBundleRequest request   = null;
            bool               isLoadAll = string.IsNullOrEmpty(loadInfo.Name);
            if (isLoadAll){
                request = loadInfo.Type == null
                    ? assetBundle.LoadAllAssetsAsync()
                    : assetBundle.LoadAllAssetsAsync(loadInfo.Type);
            }
            else{
                request = loadInfo.Type == null
                    ? assetBundle.LoadAssetAsync(loadInfo.Name)
                    : assetBundle.LoadAssetAsync(loadInfo.Name, loadInfo.Type);
            }

            loadInfo.Request = request;
            yield return request;
            loadInfo.LoadCallback.Invoke(isLoadAll ? (object)request.allAssets : request.asset, loadInfo.Arg);
            if (Instance._loadQueue.Count > 0){
                loadInfo = Instance._loadQueue.Dequeue();
                StartLoad(loadInfo);
            }
        }

        /// <summary>
        /// 获得资源类型
        /// </summary>
        /// <param name="resName">bundle名字</param>
        /// <returns></returns>
        private static Type GetAssetType(ref string resName){
            int bundleIdx                = resName.LastIndexOf(PathConst.Suffix.BUNDLE);
            if (bundleIdx != -1) resName = resName.Remove(bundleIdx);
            Type type                    = null;
            int  idx                     = resName.IndexOf(".");
            if (idx == -1){
                Debug.LogError(resName + "\n " + new StackTrace().ToString());
                return null;
            }

            string typestr = resName.Substring(idx);
            type = PathConst.S_FileTypeMap[typestr];
            return type;
        }

        public static void HookBundleAndAssetName(string path, out string bundle, out string name){
            path = path.ToLower();
            int index = path.LastIndexOf('/');
            bundle = path.Substring(0, index);
            bundle = JudgeBundleFixSuffix(bundle);
            name   = path.Substring(index + 1);
        }

        public override void OnDispose(){ }

        /// <summary>
        /// 加载信息
        /// </summary>
        public sealed class LoadInfo{
            /// <summary>
            /// 加载回调
            /// </summary>
            public Action<object, object> LoadCallback{ get; internal set; }

            /// <summary>
            /// 资源包名
            /// </summary>
            public string BundleName{ get; internal set; }

            /// <summary>
            /// 资源名
            /// </summary>
            public string Name{ get; internal set; }

            /// <summary>
            /// 资源类型
            /// </summary>
            public Type Type{ get; internal set; }

            /// <summary>
            /// 加载参数
            /// </summary>
            public object Arg{ get; internal set; }

            /// <summary>
            /// 资源包请求
            /// </summary>
            public AssetBundleRequest Request{ get; internal set; }
        }
    }
}