using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IG.IO;
using IG.Runtime.Extensions;
using UnityEngine;
using UnityEngine.Networking;

namespace IG.AssetBundle{
    /// <summary>
    /// 下载系统
    /// </summary>
    public sealed class DownloadSystem : SingletonAbs<DownloadSystem>{
        public const string DOWNLOAD_SYSTEM = "DownloadSystem";

        /// <summary>
        /// 下载超时尽可能长即可
        /// -1
        /// </summary>
        public const int DOWNLOAD_TIMEOUT = -1;

        private const string     LOG_NULL     = "Url is null.";
        private const string     FORMAT_PREFS = "Cache_{0}";
        private const string     FORMAT_URL   = "file://{0}";
        private const string     FORMAT_PATH  = "{0}/{1}";
        public static NotDestroy CoroutineObj = null;

        /// <summary>
        /// 缓存路径
        /// </summary>
        public static string CACHE_PATH;

        /// <summary>
        /// 下载表[键:URL 值:下载信息]
        /// </summary>
        private static Dictionary<string, DownloadInfo> s_DownloadMap = new Dictionary<string, DownloadInfo>();

        /// <summary>
        /// 下载队列
        /// </summary>
        private static Queue<DownloadInfo> s_DownloadQueue = new Queue<DownloadInfo>();

        /// <summary>
        /// 初始化下载系统
        /// </summary>
        /// <param name="cachePath">缓存路径</param>
        public static void Setup(string cachePath){ CACHE_PATH = cachePath; }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="downloadCallback">下载回调</param>
        /// <param name="version">版本号</param>
        /// <param name="type">下载类型</param>
        /// <returns>返回下载信息</returns>
        public static DownloadInfo Download(string url, Action<UnityWebRequest> downloadCallback, long version = 0, string secret = "", Type type = null){
            //如果URL为空时报错
            if (string.IsNullOrEmpty(url)){
                Debug.LogError(LOG_NULL);
                return null;
            }

            try{
                //尝试获取已下载好的信息，有则直接返回和调用回调
                DownloadInfo downloadInfo = null;
                if (s_DownloadMap.TryGetValue(url, out downloadInfo)){
                    if (downloadInfo.Request.isDone && downloadCallback != null){
                        downloadCallback.Invoke(downloadInfo.Request);
                    }
                    else{
                        downloadInfo.DownloadCallback += downloadCallback;
                    }
                }
                else{
                    //构建下载信息
                    downloadInfo = new DownloadInfo(){
                                                         Url              = url,
                                                         DownloadCallback = downloadCallback,
                                                         Secret           = secret,
                                                         Version          = version,
                                                         Type             = type
                                                     };
                    s_DownloadMap[url] = downloadInfo;
                    if (s_DownloadQueue.Count > 0){
                        s_DownloadQueue.Enqueue(downloadInfo);
                    }
                    else{
                        StartDownload(downloadInfo);
                    }
                }

                return downloadInfo;
            }
            catch (Exception e){
                Debug.LogError("下载文件处理错误！" + url + " " + e.ToString());
            }

            return null;
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="downloadInfo">下载信息</param>
        private static void StartDownload(DownloadInfo downloadInfo){
            // 提交协程处理
            if (CoroutineObj == null){
                GameObject singleMonoBehaviour = GameObject.Find(DOWNLOAD_SYSTEM);
                if (singleMonoBehaviour != null){
                    CoroutineObj = singleMonoBehaviour.GetComponent<NotDestroy>();
                }
                else{
                    GameObject NetworkObj = new GameObject(DOWNLOAD_SYSTEM);
                    CoroutineObj = NetworkObj.GetOrAddComponent<NotDestroy>();
                }
            }

            CoroutineObj.StartCoroutine(DownloadCoroutine(downloadInfo));
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public static void Clear(){
            foreach (DownloadInfo downloadInfo in s_DownloadMap.Values){
                downloadInfo.Request.Dispose();
            }

            s_DownloadMap.Clear();
        }

        /// <summary>
        /// 是否缓存
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="version">版本</param>
        /// <returns>返回结果</returns>
        public static bool HasCached(string url, long version = 0){
            string   key;
            FileInfo fileInfo;
            return HasCached(url, out key, out fileInfo, version);
        }

        /// <summary>
        /// 是否缓存
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="key">储存键</param>
        /// <param name="fileInfo">文件信息</param>
        /// <param name="version">版本</param>
        /// <returns>返回结果</returns>
        public static bool HasCached(string url, out string key, out FileInfo fileInfo, long version = 0){
            string md5 = FileManager.StringToMD5(url.ToLower());
            //load的资源暂时不用md5,并且只保留文件名
            // string[] strs = url.Split('/');
            // string md5 = strs[strs.Length - 1].ToLower();
            key      = string.Format(FORMAT_PREFS, md5);
            fileInfo = new FileInfo(string.Format(FORMAT_PATH, CACHE_PATH, md5));
            //获取版本信息
            if (version >= 0 && PlayerPrefs.HasKey(key)){
                //对比版本
                long localVersion = long.Parse(PlayerPrefs.GetString(key, "0"));
                if (localVersion == version && fileInfo.Exists){
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        /// <param name="url">地址</param>
        public static void ClearCache(string url){
            string   key;
            FileInfo fileInfo;
            HasCached(url, out key, out fileInfo);
            if (PlayerPrefs.HasKey(key)){
                PlayerPrefs.DeleteKey(key);
            }

            if (fileInfo.Exists){
                fileInfo.Delete();
            }
        }

        /// <summary>
        /// 清理所有缓存
        /// </summary>
        public static void ClearAllCache(){
            DirectoryInfo directoryInfo = new DirectoryInfo(CACHE_PATH);
            FileInfo[]    fileInfos     = directoryInfo.GetFiles();
            for (int i = 0; i < fileInfos.Length; ++i){
                FileInfo fileInfo = fileInfos[i];
                string   key      = string.Format(FORMAT_PREFS, fileInfo.Name);
                if (PlayerPrefs.HasKey(key)){
                    PlayerPrefs.DeleteKey(key);
                }

                fileInfo.Delete();
            }
        }

        /// <summary>
        /// 下载协程
        /// </summary>
        /// <param name="downloadInfo">下载信息</param>
        /// <returns>返回迭代器</returns>
        private static IEnumerator DownloadCoroutine(DownloadInfo downloadInfo){
            FileInfo fileInfo = null;  //文件信息
            string   key      = null;  //资源key
            bool     cached   = false; //是否在缓存
            string   url      = null;
            try{
                //查看缓存信息
                string sec = string.IsNullOrEmpty(downloadInfo.Secret) ? downloadInfo.Url : downloadInfo.Secret;
                cached = HasCached(sec, out key, out fileInfo, downloadInfo.Version);
                url    = cached ? string.Format(FORMAT_URL, fileInfo.FullName) : downloadInfo.Url;
                if (downloadInfo.Type == typeof(Texture)){
                    downloadInfo.Request = UnityWebRequestTexture.GetTexture(url);
                }
                else{
                    downloadInfo.Request = UnityWebRequest.Get(url);
                }

                downloadInfo.Request.timeout = DOWNLOAD_TIMEOUT;
            }
            catch (Exception e){
                Debug.LogError("资源下载错误：" + downloadInfo.Url + " " + e);
                //EventSystem.Broadcast()
            }

            yield return downloadInfo.Request.SendWebRequest();

            //如果发生网络错误
            if (!string.IsNullOrEmpty(downloadInfo.Request.error)                     || //
                downloadInfo.Request.result == UnityWebRequest.Result.ConnectionError || //
                downloadInfo.Request.result == UnityWebRequest.Result.ProtocolError)     //
            {
                Debug.LogError("下载错误：" + downloadInfo.Request.error + " : " + downloadInfo.Url);
                s_DownloadMap.Remove(downloadInfo.Url);
            }

            try{
                if (!cached){
                    //如果非读取缓存则写入缓存
                    if (fileInfo.Exists){
                        fileInfo.Delete();
                    }
                    else if (!fileInfo.Directory.Exists){
                        fileInfo.Directory.Create();
                    }

                    File.WriteAllBytes(fileInfo.FullName, downloadInfo.Request.downloadHandler.data);
                    PlayerPrefs.SetString(key, downloadInfo.Version.ToString());
                }
            }
            catch (Exception e){
                Debug.LogError("写入缓存错误！" + url + " " + e);
            }

            try{
                //调用回调
                if (downloadInfo.DownloadCallback != null){
                    downloadInfo.DownloadCallback.Invoke(downloadInfo.Request);
                    downloadInfo.DownloadCallback = null;
                }
            }
            catch (Exception e){
                Debug.LogError("执行下载完毕回调错误! " + url + " " + e);
            }

            //下载下一个资源
            try{
                if (s_DownloadQueue.Count > 0){
                    downloadInfo = s_DownloadQueue.Dequeue();
                    StartDownload(downloadInfo);
                }
            }
            catch (Exception e){
                Debug.LogError("开始下个任务错误！" + e);
            }
        }

        public override void OnDispose(){ }

        /// <summary>
        /// 下载信息
        /// </summary>
        public sealed class DownloadInfo{
            /// <summary>
            /// 下载地址
            /// </summary>
            public string Url{ get; internal set; }

            /// <summary>
            /// 密钥
            /// </summary>
            public string Secret{ get; internal set; }

            /// <summary>
            /// 版本号
            /// </summary>
            public long Version{ get; internal set; }

            /// <summary>
            /// 下载类型
            /// </summary>
            public Type Type{ get; internal set; }

            /// <summary>
            /// 下载回调
            /// </summary>
            public Action<UnityWebRequest> DownloadCallback{ get; internal set; }

            /// <summary>
            /// 下载请求
            /// </summary>
            public UnityWebRequest Request{ get; internal set; }
        }
    }
}