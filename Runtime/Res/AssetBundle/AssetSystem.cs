using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IG.IO;
using IG.Runtime.Common;
using IG.Runtime.Log;
using IG.Runtime.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace IG.AssetBundle{
    /// <summary>
    /// 资源系统
    /// </summary>
    public sealed class AssetSystem : SingletonAbs<AssetSystem>{
        private static List<SpriteAtlas> SpriteData = new List<SpriteAtlas>();

        /// <summary>
        /// Ab bundle manifest 资源名
        /// </summary>
        private static readonly string ASSET_MANIFEST = "AssetBundleManifest";

        private const string        FORMAT_PATH  = "{0}/{1}";
        private const string        LOG_NULL     = "Load complete callback is null.";
        private const string        LOG_GET      = "Can't find out asset bundle:{0}.";
        private const string        LOG_LOAD     = "Can't load asset:{0}";
        public static MonoBehaviour CoroutineObj = null;

        /// <summary>
        /// 资源总表
        /// TODO:资源总表有修改，不走这里了
        /// </summary>
        private AssetBundleManifest m_Manifest;

        /// <summary>
        /// 资源地址
        /// </summary>
        private string m_AssetURL;

        /// <summary>
        /// 资源包表 [键:资源包名 值:资源包]
        /// </summary>
        private Dictionary<string, UnityEngine.AssetBundle> m_AssetBundleMap = new Dictionary<string, UnityEngine.AssetBundle>();

        /// <summary>
        /// 资源请求表[键:资源包名 值:资源请求]
        /// </summary>
        private Dictionary<string, AssetBundleCreateRequest> m_RequestMap =
            new Dictionary<string, AssetBundleCreateRequest>();

        private Queue<LoadInfo> m_LoadQueue = new Queue<LoadInfo>();

        private static void CheckCoroutineObj(){
            // 提交协程处理
            if (CoroutineObj == null){
                //TODO:
                // GameObject singleMonoBehaviour = GameObject.Find("NetworkObj");
                // if (singleMonoBehaviour != null){
                //     CoroutineObj = singleMonoBehaviour.GetComponent<NotDestroy>();
                // }
                // else{
                //     GameObject NetworkObj = new GameObject("NetworkObj");
                //     CoroutineObj = NetworkObj.GetOrAddComponent<NotDestroy>();
                // }
            }
        }

        /// <summary>
        /// 读取精灵文件
        /// </summary>
        /// <param name="path">Resources下资源路径</param>
        /// <returns></returns>
        // public Sprite LoadSprite(string ImageName){
        //     int k = -1;
        //
        //     if (string.IsNullOrEmpty(ImageName)){
        //         return null;
        //     }
        //
        //     for (int i = 0; i < SpriteData.Count; i++){
        //         if (SpriteData[i].SpriteDir.ContainsKey(ImageName)){
        //             k = i;
        //             break;
        //         }
        //     }
        //
        //     if (k == -1){
        //         Debug.Log(ImageName + " is none");
        //         return null;
        //     }
        //
        //     return SpriteData[k].SpriteDir[ImageName];
        // }

        /// <summary>
        /// 读取精灵文件
        /// </summary>
        /// <param name="path">Resources下资源路径</param>
        /// <returns></returns>
        public Sprite LoadSprite(string ImageName){
            int k = -1;
            if (string.IsNullOrEmpty(ImageName)){
                return null;
            }

            for (int i = 0; i < SpriteData.Count; i++){
                if (SpriteData[i].GetSprite(ImageName) != null){
                    k = i;
                    break;
                }
            }

            if (k == -1){
                Debug.Log(ImageName + " is none");
                return null;
            }

            return SpriteData[k].GetSprite(ImageName);
        }

        public static void OnDestroy(){ SpriteData.Clear(); }

        ///// 
        //AssetBundle 
        /////

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetURL">资源地址</param>
        /// <param name="manifest">资源包</param>
        public static void Init(string assetURL, string manifest){
            Clear();
            Instance.m_AssetURL = assetURL;
            string path = GetPath(manifest);

            //TODO:
            Debug.Log($"初始化 AssetSystem!");
            if (File.Exists(path)){
                //TODO:
                Debug.Log($"AssetSystem! 检测到资源地址! :{path}");
                UnityEngine.AssetBundle assetBundle = UnityEngine.AssetBundle.LoadFromFile(path);
                //TODO:
                Debug.Log($"AssetSystem! 加载assetbundle! :{assetBundle}");
                // Instance.m_Manifest = assetBundle.LoadAsset<AssetBundleManifest>(ASSET_MANIFEST);
                Instance.m_Manifest = assetBundle.LoadAsset<AssetBundleManifest>(ASSET_MANIFEST);
                //TODO:
                Debug.Log($"AssetSystem! Manifest! :{Instance.m_Manifest}");
                Add(manifest, assetBundle);
                //TODO:
                Debug.Log($"AssetSystem! 初始化成功! ");
            }

            //init sprite atlas
            //TODO:测试Hotfix暂时先隐藏，SpriteAtlas 晚点再加回来
            // Instance.InitAtlasSprite();
        }

        /// <summary>
        /// 添加资源包
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="assetBundle">资源包</param>
        public static void Add(string bundleName, UnityEngine.AssetBundle assetBundle){ Instance.m_AssetBundleMap[bundleName] = assetBundle; }

        /// <summary>
        /// 移除资源包
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="unload">是否释放资源</param>
        public static void Remove(string bundleName, bool unload = true){
            UnityEngine.AssetBundle assetBundle = null;
            if (!Instance.m_AssetBundleMap.TryGetValue(bundleName, out assetBundle)){
                return;
            }

            if (unload){
                assetBundle.Unload(true);
            }

            Instance.m_AssetBundleMap.Remove(bundleName);
        }

        /// <summary>
        /// 获取资源路径
        /// TODO: 注意区分Local资源和Server资源
        /// </summary>
        /// <param name="bundleName">资源名</param>
        /// <returns></returns>
        private static string GetPath(string bundleName){
#if UNITY_EDITOR && !LOCAL_AB
            // return string.Format(FORMAT_PATH, Instance.m_AssetURL, bundleName);
            string url = string.Format(FORMAT_PATH, Instance.m_AssetURL, bundleName);
#else
         string url = Instance.m_AssetURL + bundleName;
        string key;
        FileInfo fileInfo;
        DownloadSystem.HasCached(url, out key, out fileInfo);
        return fileInfo.FullName;
        
        // string url = string.Format(FORMAT_PATH, Instance.m_AssetURL, bundleName);
        // return url;
#endif
            return string.Format(FORMAT_PATH, Instance.m_AssetURL, bundleName);
        }

        /// <summary>
        /// 获取资源包
        /// 需要带后缀名
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <returns>返回资源包</returns>
        public static UnityEngine.AssetBundle Get(string bundleName){
            UnityEngine.AssetBundle assetBundle = null;
            string                  path        = null;
            if (Instance.m_AssetBundleMap.TryGetValue(bundleName, out assetBundle)){
                return assetBundle;
            }
            else if (Instance.m_Manifest != null){
                // string[] dependencies = Instance.m_Manifest.GetAllDependencies(bundleName);
                string[] dependencies = Instance.m_Manifest.GetDirectDependencies(bundleName);
                for (int i = 0; i < dependencies.Length; ++i){
                    string dependence = dependencies[i];
                    // Get(dependence);
                    //TODO:依赖已加载的忽略即可，防止重复嵌套加载
                    if (!Instance.m_AssetBundleMap.ContainsKey(dependence)){
                        path = GetPath(dependence);
                        if (!File.Exists(path)){
                            // Debug.LogError(string.Format(LOG_GET, path));
                            return null;
                        }

                        assetBundle = UnityEngine.AssetBundle.LoadFromFile(path);
                        Add(dependence, assetBundle);
                    }
                    // else{
                    //     Get(dependence);
                    // }
                }

                path = GetPath(bundleName);
                if (!File.Exists(path)){
                    // Debug.LogError(string.Format(LOG_GET, path));
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
            if (Instance.m_AssetBundleMap.TryGetValue(bundleName, out assetBundle)){
                if (onLoadComplete != null){
                    onLoadComplete.Invoke(assetBundle);
                }
            }
            else{
                List<string> dependenList = new List<string>();
                GetAllDependencies(bundleName, dependenList);
                //TODO:临时修改的携程开启策略
                CheckCoroutineObj();
                //游戏主控Mono 去开启这个携程
                CoroutineObj.StartCoroutine(
                                            LoadAssetBundleCoroutine(
                                                                     dependenList,
                                                                     () => {
                                                                         try{
                                                                             Instance.m_AssetBundleMap.TryGetValue(bundleName, out assetBundle);
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
            if (Instance.m_Manifest == null){
                return;
            }

            string[] dependencies = Instance.m_Manifest.GetAllDependencies(bundleName);
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
                string                  bundleName  = bundleList[i];
                UnityEngine.AssetBundle assetBundle = null;
                if (Instance.m_AssetBundleMap.TryGetValue(bundleName, out assetBundle)){
                    continue; //已经下载过了
                }

                //检测是否加载中
                AssetBundleCreateRequest request = null;
                if (Instance.m_RequestMap.TryGetValue(bundleName, out request)){
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
                    request                           = UnityEngine.AssetBundle.LoadFromFileAsync(path);
                    Instance.m_RequestMap[bundleName] = request;
                    yield return request;

                    //执行完毕回调
                    assetBundle = request.assetBundle;
                    Add(bundleName, assetBundle);
                    Instance.m_RequestMap.Remove(bundleName);
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
        /// 通过中间名，简介指定Editor文件夹
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="name"></param>
        /// <param name="median"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        //     public static UnityEngine.Object LoadByMedian(string bundleName, string name = null, string median = null, Type type = null){
        // #if UNITY_EDITOR && !LOCAL_AB
        //         string path = string.Empty;
        //         string suffix = type == null ? String.Empty : s_TypeMap[type];
        //         bundleName = bundleName.Replace(PathConfig.Suffix.BUNDLE, "");
        //         path = PathConfig.Path.BaseABPath + bundleName + median + name + suffix;
        //         UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        //         if (asset == null){
        //             Debug.LogError(string.Format(LOG_LOAD, path));
        //         }
        //
        //         return asset;
        // #else
        //     return Load(bundleName,name);
        // #endif
        //     }

        /// <summary>
        /// 加载资源(资源类型明确)
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>(string bundleName, string name = null) where T : Object{
            var strs = StringUtils.SplitBySign('/', bundleName);
            if (strs != null){
                for (int i = 0; i < strs.Length; i++){
                    strs[i] = strs[i].ToLower();
                    UnityEngine.AssetBundle bundle = Get(strs[i]);
                    //正常情况无法加载，连URL一起转一次MD5,查找本地资源
                    if (bundle == null){
                        //TODO:
                        LogHelper.Log($"[无法获取确认一下]:{bundleName}", LogType.Error);
                        // string url = PathConfig.ServerRelated.SERVER_CONFIG.resource +
                        //              PathConfig.Path.URL_RESOURCE_SUB                +
                        //              PathConfig.Path.BUNDLE_PATH                     +
                        //              PathConfig.Path.PLATFORM                        +
                        //              bundleName;
                        // var fileName = FileManage.StringToMD5(url.ToLower());
                        // bundle = Get(fileName);
                    }

                    if (name == null){
                        name = bundleName.Substring(bundleName.LastIndexOf('/') + 1);
                        name = name.Remove(name.LastIndexOf('.'));
                    }

                    if (bundle.Contains(name)){
                        return bundle.LoadAsset<T>(name);
                    }
                }
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
        public static Object Load(string bundleName, string name = null, Type type = null){
            if (type == null){
                type = GetAssetType(ref name);
            }

            if (!bundleName.Contains(PathConst.Suffix.BUNDLE)){
                bundleName += PathConst.Suffix.BUNDLE;
            }

#if UNITY_EDITOR && !LOCAL_AB
            //如果Editor没有Local_AB就只有path
            string path = name;
            //  ShopTools.InitInfo();
            string suffix = type != null ? PathConst.S_TypeMap[type] : String.Empty;
            if (!path.Contains(suffix)){
                path += suffix;
            }

            Object asset = AssetDatabase.LoadAssetAtPath(path, type);
            if (asset == null){
                Debug.LogError(string.Format(LOG_LOAD, path));
            }

            return asset;
#else
        //TODO:测试使用，正是项目修改了依赖try部分就不需要了，只留catch部分
        try{
            var strs = StringUtils.SplitBySign('/', bundleName);
            if (strs != null){
                for (int i = 0; i < strs.Length; i++){
                    strs[i] = strs[i].ToLower();
                    UnityEngine.AssetBundle bundle = Get(strs[i]);
                    //正常情况无法加载，连URL一起转一次MD5,查找本地资源
                    // if (bundle == null){
                    //     string url = PathConfig.ServerRelated.SERVER_CONFIG.resource +
                    //                  PathConfig.Path.URL_RESOURCE_SUB +
                    //                  PathConfig.Path.BUNDLE_PATH +
                    //                  PathConfig.Path.PLATFORM +
                    //                  bundleName;
                    //     var fileName = FileManage.StringToMD5(url.ToLower());
                    //     bundle = Get(fileName);
                    // }

                    if (name == null){
                        name = bundleName.Substring(bundleName.LastIndexOf('/') + 1);
                        name = name.Remove(name.LastIndexOf('.'));
                    }

                    if (bundle.Contains(name)){
                        return bundle.LoadAsset(name, type);
                    }
                }
            }
        }
        catch (Exception e){
            if (bundleName.Contains(PathConst.Suffix.PREFAB)){
                bundleName = StringUtils.CutSuffix(bundleName);
            }

            if (!bundleName.Contains(PathConst.Suffix.BUNDLE)){
                bundleName += PathConst.Suffix.BUNDLE;
            }

            bundleName = bundleName.ToLower();
            UnityEngine.AssetBundle assetBundle = Get(bundleName);
            //正常情况无法加载，连URL一起转一次MD5,查找本地资源
            // if (assetBundle == null){
            //     string url = PathConfig.ServerRelated.SERVER_CONFIG.resource +
            //                  PathConfig.Path.URL_RESOURCE_SUB +
            //                  PathConfig.Path.BUNDLE_PATH +
            //                  PathConfig.Path.PLATFORM +
            //                  bundleName;
            //     var fileName = FileManage.StringToMD5(url.ToLower());
            //     assetBundle = Get(fileName);
            // }

            if (name == null){
                name = bundleName.Substring(bundleName.LastIndexOf('/') + 1);
                name = name.Remove(name.LastIndexOf('.'));
            }

            name = name.ToLower();
            return assetBundle.LoadAsset(name, type);
        }

        return null;
#endif
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
        public static LoadInfo LoadAsync(
            Action<object, object> loadComplete,
            string                 bundleName,
            string                 name = null,
            Type                   type = null,
            object                 arg  = null){
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
#if UNITY_EDITOR && !LOCAL_AB
            //如果Editor没有Local_AB就只有path
            string path  = name;
            Object asset = AssetDatabase.LoadAssetAtPath(path, type);
            if (asset == null){
                Debug.LogError(string.Format(LOG_LOAD, path));
            }
            else{
                loadComplete.Invoke(asset, loadInfo.Arg);
            }
#else
        if (Instance.m_LoadQueue.Count > 0)
        {
            Instance.m_LoadQueue.Enqueue(loadInfo);
        }
        else
        {
            StartLoad(loadInfo);
        }
#endif
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
#if UNITY_EDITOR && !LOCAL_AB
            //如果Editor没有Local_AB就只有path = bundleName
            string path = bundleName.Replace(PathConst.Suffix.BUNDLE, string.Empty);
            if (!suffix.Contains("*")){ suffix = $"*{suffix}"; }

            string[] files  = FileManager.GetFiles(path, suffix, true);
            int      length = files == null ? 0 : files.Length;
            Object[] assets = files == null ? null : new Object[length];
            for (int i = 0; i < length; ++i){
                assets[i] = AssetDatabase.LoadAssetAtPath(files[i], type);
            }

            if (assets == null){
                Debug.LogError(string.Format(LOG_LOAD, path));
            }

            return assets;
#else
                // bundleName += PathConfig.Suffix.BUNDLE;
                // bundleName = bundleName.ToLower();
                // AssetBundle assetBundle = Get(bundleName);
                // return type == null ? assetBundle.LoadAllAssets() : assetBundle.LoadAllAssets(type);

        // bundleName += PathConfig.Suffix.BUNDLE;
        bundleName = bundleName.ToLower();
        UnityEngine.AssetBundle assetBundle = Get(bundleName);
        //正常情况无法加载，连URL一起转一次MD5,查找本地资源
        if (assetBundle == null){
            // string url = PathConfig.ServerRelated.SERVER_CONFIG.resource +
            //              PathConfig.Path.URL_RESOURCE_SUB +
            //              PathConfig.Path.BUNDLE_PATH +
            //              PathConfig.Path.PLATFORM +
            //              bundleName;
            // var fileName = FileManage.StringToMD5(url.ToLower());
            // assetBundle = Get(fileName);
        }

        return type == null ? assetBundle.LoadAllAssets() : assetBundle.LoadAllAssets(type);
#endif
        }

        /// <summary>
        /// 异步加载所有资源
        /// </summary>
        /// <param name="loadCallback">加载完成回调</param>
        /// <param name="bundleName">资源包名</param>
        /// <param name="type">资源类型</param>
        /// <param name="arg">加载参数</param>
        /// <returns>返回加载请求</returns>
        public static LoadInfo LoadAllAsync(
            Action<object, object> loadCallback,
            string                 bundleName,
            Type                   type = null,
            object                 arg  = null){
            if (loadCallback == null){
                Debug.LogError(LOG_NULL);
                return null;
            }

            LoadInfo loadInfo = new LoadInfo(){ LoadCallback = loadCallback, BundleName = bundleName, Type = type, Arg = arg };
#if UNITY_EDITOR && !LOCAL_AB
            //如果Editor没有Local_AB就只有path=bundleName
            string   path   = bundleName.Replace(PathConst.Suffix.BUNDLE, string.Empty);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets == null){
                Debug.LogError(string.Format(LOG_LOAD, path));
            }
            else{
                loadCallback.Invoke(assets, loadInfo.Arg);
            }
#else
        if (Instance.m_LoadQueue.Count > 0)
        {
            Instance.m_LoadQueue.Enqueue(loadInfo);
        }
        else
        {
            StartLoad(loadInfo);
        }
#endif
            return loadInfo;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <param name="unloadAll">是否卸载全部资源</param>
        public static void Unload(string bundleName, bool unloadAll){
            UnityEngine.AssetBundle assetBundle = null;
            if (!Instance.m_AssetBundleMap.TryGetValue(bundleName, out assetBundle)){
                return;
            }

            assetBundle.Unload(unloadAll);
            Instance.m_AssetBundleMap.Remove(bundleName);
        }

        /// <summary>
        /// 清除所有资源
        /// </summary>
        public static void Clear(){
            foreach (UnityEngine.AssetBundle assetBundle in Instance.m_AssetBundleMap.Values){
                assetBundle.Unload(true);
            }

            Instance.m_AssetBundleMap.Clear();
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
            if (Instance.m_LoadQueue.Count > 0){
                loadInfo = Instance.m_LoadQueue.Dequeue();
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

        /// <summary>
        /// 使用于Editor模式下，加载指定目录下所有文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static T[] LoadAllAssetAtDirectory<T>(string path) where T : Object{
#if UNITY_EDITOR && !LOCAL_AB
            //如果Editor没有Local_AB就只有path
            T[] assets = null;
            path += "/";
            if (Directory.Exists(path)){
                List<T>       list      = new List<T>();
                DirectoryInfo direction = new DirectoryInfo(path);
                FileInfo[]    files     = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; ++i){
                    if (files[i].Name.EndsWith(".meta")) continue;
                    list.Add(AssetDatabase.LoadAssetAtPath<T>(path + files[i].Name));
                }

                assets = list.ToArray();
            }

            return assets;
#else
        return null;
#endif
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