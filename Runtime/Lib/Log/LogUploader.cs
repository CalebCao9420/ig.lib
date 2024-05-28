using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IG.Runtime.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace IG.Runtime.Log{
    public static class LogUploader{
        /// <summary>
        /// Function:POST
        /// Param : 1 file = 文件类型 , 2 filePath = 要保存的文件路径
        /// Callback : 1 string message  , 2 int code , 3 string data = call back google cloud storage server path , 4 long serverTime
        /// </summary>
        private static string s_URL;

        private static string s_TokenKey;
        private static int    s_MaxResendCount;
        private static int    s_ResendInterval;
        private static int    _errorCount = 0;

        /// <summary>
        /// 游戏中途强行上传日志
        /// </summary>
        public static void ManualUpload(){
            LogHelper.Log(LogConst.GAME_FRONT_KEY);
            //将上次日志设置为本次日志
            string prePath = PlayerPrefs.GetString(LogConst.LOG_PATH_KEY);
            PlayerPrefs.SetString(LogConst.PREV_LOG_PATH_KEY, prePath);
            UploadLogFile(s_URL);
        }

        public static void Upload(string url, string token, int maxResendCount = 5, int resendInterval = 60){
            s_URL            = url;
            s_TokenKey       = token;
            s_MaxResendCount = maxResendCount;
            s_ResendInterval = resendInterval;
            UploadLogFile(s_URL);
        }

        /// <summary>
        /// 上传整个日志
        /// </summary>
        public static void UploadLogFile(string url){
            _errorCount = 0;
            string logPath = PlayerPrefs.HasKey(LogConst.PREV_LOG_PATH_KEY) ? PlayerPrefs.GetString(LogConst.PREV_LOG_PATH_KEY) : null;
            string uuid    = PlayerPrefs.GetString(LogConst.DEVICE_ID, null);
            if (string.IsNullOrEmpty(logPath)){
                return;
            }

            if (!File.Exists(logPath)){
                LogHelper.Log("[LogAnalyzer][日志]日志文件丢失！" + logPath, LogType.Error);
                return;
            }

            if (string.IsNullOrEmpty(url)){
                LogHelper.Log("[LogAnalyzer][日志][上传日志错误]没有服务器地址！", LogType.Error);
                return;
            }

            //检查日志是否合格
            if (!CheckFile(logPath)){
                return;
            }

            string logInfoJson = PlayerPrefs.GetString(LogConst.PREV_LOG_INFO_KEY);
            if (string.IsNullOrEmpty(logInfoJson)){
                Debug.LogError($"[LogAnalyzer][日志]LogInfo信息丢失,上次未成功记录,上线前需要保证完整可用!");
            }

            //设备号
            string deviceId       = DeviceInfo.GetDeviceModel();
            string deviceUniqueId = uuid ?? Guid.NewGuid().ToString();
            string curTime        = DeviceInfo.GetDate();
            string subPath        = $"{deviceId}_{deviceUniqueId}_{curTime}.zip";
            //TODO: if need dev
            // if (dev){
            //     subPath = $"(dev_{Application.version}){subPath}";
            // }
            string outputPath = $"{Application.persistentDataPath}/Log/{subPath}";
            string serverPath = $"{DeviceInfo.GetDataForDay()}/{subPath}";

            //压缩成zip文件
            ZipFile(logPath, outputPath, 9);
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("file",     File.ReadAllBytes(outputPath));
            dic.Add("filePath", serverPath);
            dic.Add("sign",     CreateToken(serverPath));
            dic.Add("logInfo",  logInfoJson);

            // 上传日志
            LogServerHelper.RequestUpload(url, dic, OnUploadCallback, outputPath, $"{subPath}");
            LogHelper.Log("日志上传中！" + " ->" + deviceId + " logPath:" + logPath);
        }

        /// <summary>
        ///检查文件是否满足上传要求
        /// 1.不能包含正常退出游戏的======= Game End =======这个key
        /// 2.文件大小在1M内,超出则需要客户端手动切段
        /// </summary>
        private static bool CheckFile(string logPath){
            if (string.IsNullOrEmpty(logPath)){
                return false;
            }

            FileInfo fileInfo = new FileInfo(string.Format(logPath));
            if (!fileInfo.Exists){
                return false;
            }

            using (StreamReader reader = fileInfo.OpenText()){
                string txt       = reader.ReadToEnd();
                int    manualKey = txt.CalcKeyInStr(LogConst.GAME_MANUAL_KEY);
                if (manualKey > 0){
                    LogHelper.Log("手动设置上传!");
                    return true;
                }

                //正常退出 
                if (txt.Contains(LogConst.GAME_END_KEY)){
                    LogHelper.Log("上次正常退出，不上传!");
                    return false;
                }

                //杀进程(如果是app崩溃，1：不会有backKey，2：backKey数量=frontKey)
                int backKeyCount  = txt.CalcKeyInStr(LogConst.GAME_BACK_KEY);
                int frontKeyCount = txt.CalcKeyInStr(LogConst.GAME_FRONT_KEY);
                if (backKeyCount > frontKeyCount){
                    //当前只统计前台崩溃，后台崩溃暂不处理
                    LogHelper.Log($"上次杀进程正常退出，不上传 -> 后台次数 {backKeyCount} : -> 前台次数 {frontKeyCount}!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 简单创建压缩zip文件
        /// 需要注意如果使用ICSharpCode打移动包的话，需要把新版的Unity（如2019）的安装目录Editor\Data\MonoBleedingEdge\lib\mono\unityjit下找到I18N.dll和I18N.West.dll文件
        /// 单独拎出来到Assets目录或者Plugin目录一起打包才行
        /// </summary>
        /// <param name="file">需要压缩的文件</param>
        /// <param name="outputFilePath">输出的路径</param>
        /// <param name="compressLevel">压缩等级0-9</param>
        private static void ZipFile(string file, string outputFilePath, int compressLevel){
            //TODO:Old logic
            // byte[] buffer = new byte[4096];
            // using (ZipOutputStream stream = new ZipOutputStream(File.Create(outputFilePath))){
            //     stream.SetLevel(compressLevel); //设置压缩等级
            //     var entry = new ZipEntry(System.IO.Path.GetFileName(file)){ DateTime = DateTime.Now };
            //     stream.PutNextEntry(entry);
            //     using (FileStream fs = File.OpenRead(file)){
            //         int sourceBytes;
            //         do{
            //             sourceBytes = fs.Read(buffer, 0, buffer.Length);
            //             stream.Write(buffer, 0, sourceBytes);
            //         }
            //         while (sourceBytes > 0);
            //     }
            //
            //     stream.Finish();
            //     stream.Close();
            // }
        }

        private static string CreateToken(string serverFilePath){
            string token = $"{s_TokenKey}{serverFilePath}";
            return StringUtils.String2MD5(token);
        }

        private static void OnUploadCallback(UnityWebRequest callback){
            if (callback.downloadHandler == null || callback.downloadHandler.text == null){
                LogHelper.Log($"[日志]：上传日志，服务器回调错误:{callback.error}", LogType.Error);
                return;
            }

            LogHelper.Log($"[日志] : {callback.downloadHandler.text}", LogType.Error);
            // JsonData convert = new JsonData(callback.downloadHandler.text);
            // LogHelper.Log($"[日志] : convert :{convert}");
        }

        private static void OnUploadCallback(string callback){
            LogHelper.Log($"[日志] : convert :{callback}");
            if (string.IsNullOrEmpty(callback)){
                LogAnalyzer.Instance.StopCoroutine(DelayOption(s_ResendInterval,  Resend));
                LogAnalyzer.Instance.StartCoroutine(DelayOption(s_ResendInterval, Resend));
                return;
            }

            LogServerMsg msg  = JSONUtils.JsonToObject<LogServerMsg>(callback);
            int          code = msg.code;
            if (code == 0){
                LogHelper.Log($"[Log] : 上传日志成功 :{msg}");
            }
            else{
                LogAnalyzer.Instance.StopCoroutine(DelayOption(s_ResendInterval,  Resend));
                LogAnalyzer.Instance.StartCoroutine(DelayOption(s_ResendInterval, Resend));
                LogHelper.Log($"[Log] : 上传日志失败 :{msg} \n 重试:{_errorCount}", LogType.Error);
            }
        }

        private static void Resend(){
            _errorCount++;
            if (_errorCount < s_MaxResendCount){
                //Upload
                try{
                    UploadLogFile(s_URL);
                }
                catch (Exception e){
                    LogHelper.Log($"[日志]:上传日志错误{e}", LogType.Error);
                }
            }
        }

        static IEnumerator DelayOption(float delayTime, Action callback){
            yield return new WaitForSeconds(delayTime);
            callback?.Invoke();
        }
    }
}