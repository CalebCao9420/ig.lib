using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace IG.Runtime.Log{
    public class LogServerMsg{
        public string message   { set; get; }
        public int    code      { set; get; }
        public string data      { set; get; }
        public long   serverTime{ set; get; }
    }

    public class LogServerHelper{
        /// <summary>
        /// 网络链接超时时间
        /// </summary>
        public const int OUT_TIME = 1000;

        public const           string NETWORK_CONTENT_TYPE   = "application/x-www-form-urlencoded";
        public const           string NETWORK_CONTENT_TYPE_2 = "application/octet-stream";
        public const           string NETWORK_CONTENT_KEY    = "ContentType";
        public static readonly string NETWORK_CONTENT_TYPE_3 = $"multipart/form-data; boundary={0}";
        public static          void   UploadFile(MonoBehaviour body, string url, Action<UnityWebRequest> callback, byte[] data)    { RequestUpload(body, url, callback, data); }
        public static          void   UploadFile(MonoBehaviour body, string url, Action<UnityWebRequest> callback, string filePath){ RequestUpload(body, url, callback, filePath); }

    #region Http normal function

        /// <summary>
        /// 基于Gameobject的tmpe http请求方式 
        /// </summary>
        public void RequestHttpPostTemp(MonoBehaviour body, string url, Action<UnityWebRequest> callback, byte[] data){ body.StartCoroutine(HttpCoroutine(url, UnityWebRequest.kHttpVerbPOST, data, callback)); }

        public IEnumerator HttpCoroutine(string url, string method, byte[] data, Action<UnityWebRequest> callback){
            UnityWebRequest request = null;
            request = RequestMethod(method, url, data);
            yield return request.SendWebRequest();
            if (callback != null){
                callback.Invoke(request);
            }

            request.Dispose();
        }

        /// <summary>
        /// 请求方式处理
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static UnityWebRequest RequestMethod(string method, string url, byte[] data){
            UnityWebRequest request;
            switch (method){
                case UnityWebRequest.kHttpVerbPUT:
                    request         = UnityWebRequest.Put(url, data);
                    request.timeout = OUT_TIME;
                    break;
                case UnityWebRequest.kHttpVerbPOST:
                    request                           = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                    request.uploadHandler             = (UploadHandler)new UploadHandlerRaw(data);
                    request.uploadHandler.contentType = NETWORK_CONTENT_TYPE;
                    request.downloadHandler           = (DownloadHandler)new DownloadHandlerBuffer();
                    request.timeout                   = OUT_TIME;
                    break;
                default:
                case UnityWebRequest.kHttpVerbGET:
                    request = UnityWebRequest.Get(url);
                    break;
            }

            return request;
        }

        public static void RequestUpload(MonoBehaviour body, string url, Action<UnityWebRequest> callback, byte[] data){ body.StartCoroutine(Upload(url, callback, data)); }

        private static IEnumerator Upload(string url, Action<UnityWebRequest> callback, byte[] data){
            UnityWebRequest request  = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            byte[]          boundary = UnityWebRequest.GenerateBoundary();
            request.uploadHandler             = (UploadHandler)new UploadHandlerRaw(data);
            request.uploadHandler.contentType = string.Format(NETWORK_CONTENT_TYPE_3, System.Text.Encoding.UTF8.GetString(boundary));
            request.downloadHandler           = new DownloadHandlerBuffer();
            request.downloadHandler           = (DownloadHandler)new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            // 回调
            if (callback != null){
                callback.Invoke(request);
            }

            yield break;
        }

        public static void RequestUpload(MonoBehaviour body, string url, Action<UnityWebRequest> callback, string filePath){ body.StartCoroutine(UploadFile(url, callback, filePath)); }

        private static IEnumerator UploadFile(string url, Action<UnityWebRequest> callback, string filePath){
            UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler   = (UploadHandler)new UploadHandlerFile(filePath);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            string boundary = string.Format("--{0}",                                            DateTime.Now.Ticks.ToString("x"));
            request.SetRequestHeader(NETWORK_CONTENT_KEY, string.Format(NETWORK_CONTENT_TYPE_3, boundary));
            yield return request.SendWebRequest();

            // 回调
            if (callback != null){
                callback.Invoke(request);
            }

            yield break;
        }

        public static void RequestUpload(string url, Dictionary<string, object> dic, Action<string> callback, string filePath, string fileName){ UploadFile(url, dic, callback, filePath, fileName); }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"><服务器地址>
        /// <param name="dic"></param>
        /// <param name="filepath"><文件的本地储存地址>
        /// <param name="filename"><文件的名字>
        /// <returns></returns>
        public static async void UploadFile(string url, Dictionary<string, object> dic, Action<string> callback, string filepath, string filename){
            try{
                using (HttpClient client = new HttpClient()){
                    MultipartFormDataContent postContent = new MultipartFormDataContent();
                    string                   boundary    = System.Text.Encoding.UTF8.GetString(UnityWebRequest.GenerateBoundary());
                    postContent.Headers.Add(NETWORK_CONTENT_KEY, string.Format(NETWORK_CONTENT_TYPE_3, boundary));
                    const string filekeyname = "file";
                    postContent.Add(new ByteArrayContent(File.ReadAllBytes(filepath)), filekeyname, filename);
                    foreach (var key in dic.Keys){
                        postContent.Add(new StringContent(dic[key].ToString()), key);
                    }

                    //异步获取，不要同步拿结果，走task的皆是如此
                    // HttpResponseMessage response = client.PostAsync(url, postContent).Result;
                    HttpResponseMessage response = await client.PostAsync(url, postContent);
                    if (response.IsSuccessStatusCode){
                        var rel = await response.Content.ReadAsStringAsync();
                        if (callback != null){
                            callback.Invoke(rel);
                        }
                    }
                    else{
                        Debug.LogError($"[Error]: server callback error code = {response.StatusCode},Msg : {response}");
                    }
                }
            }
            catch (Exception ex){
                Debug.LogError("[Error]:" + ex.ToString());
            }
        }

    #endregion
    }
}