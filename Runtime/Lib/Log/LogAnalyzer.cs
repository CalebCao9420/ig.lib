using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IG.Runtime.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace IG.Runtime.Log{
    public struct LogConfig{
        public string LogUploadURL;
        public string LogUploadURLDev;
        public string Token;
        public int    MaxResendCount;
        public int    ErrorResendInterval;
        public bool   ReceiveLog;
        public bool   UploadLog;

        public LogConfig(string url, string urlDev, string token, int resendCount, int resendInterval, bool receiveLog, bool upload){
            this.LogUploadURL        = url;
            this.LogUploadURLDev     = urlDev;
            this.Token               = token;
            this.MaxResendCount      = resendCount;
            this.ErrorResendInterval = resendInterval;
            this.ReceiveLog          = receiveLog;
            this.UploadLog           = upload;
        }
    }

    //服务端记录数据，所以命名规则与服务端统一
    public class LogInfo{
        public string logKey  { get; private set; }
        public string logVal  { get; private set; }
        public int    logCount{ get; private set; }
        private LogInfo(){ }

        public LogInfo(string key, string val){
            logKey   = key;
            logVal   = val;
            logCount = 1;
        }

        public void AddCount(string key){
            if (!string.IsNullOrEmpty(key) && logKey.Equals(key)){
                this.logCount++;
            }
        }
    }

    /// <summary>
    /// 日志统计
    /// 当前是走配置做的
    /// 可以考虑单独拆开，记录的只管记录，上传的才管上传
    /// </summary>
    public class LogAnalyzer : SingletonMono<LogAnalyzer>{
    #region cnf

        private static string s_TokenKey;
        private static string s_SavePath;
        private static int    s_MaxResendCount;
        private static int    s_ResendInterval;

        /// <summary>
        /// Function:POST
        /// Param : 1 file = 文件类型 , 2 filePath = 要保存的文件路径
        /// Callback : 1 string message  , 2 int code , 3 string data = call back google cloud storage server path , 4 long serverTime
        /// </summary>
        private static string s_URL;

        private static bool s_Upload;

    #endregion

    #region Const params

        public const string NULL_STR         = "NULL";
        public const string LOG_PRINT_KEY    = "<color=write>[LOG]:{0}</color>";
        public const string WARING_PRINT_KEY = "<color=yellolw>[WARNIING]:{0}</color>";
        public const string ERROR_PRINT_KEY  = "<color=red>[ERROR]:{0}</color>";
        public const string GAME_MANUAL_KEY  = "======= Game Manual Upload =======";

        /// <summary>
        /// 错误日志达到一定数量hook 设备信息
        /// </summary>
        private const int ERROR_COUNT_2_HOOK_DEVICE_STATE = 10;

        private const int    SIZE_LIMIT     = 1024 * 1024;
        private const string FORMAT_PATH    = "{0}/Log_{1}.txt";
        private const string GAME_START_KEY = "======= Game Start =======";
        private const string GAME_END_KEY   = "======= Game End =======";
        private const string GAME_BACK_KEY  = "======= Game Back =======";
        private const string GAME_FRONT_KEY = "======= Game Front =======";

        /// <summary>
        /// 缓存记录日志路径key
        /// </summary>
        private const string LOG_PATH_KEY = "LogPath";

        /// <summary>
        /// 缓存记录之前日志路径key
        /// </summary>
        private const string PREV_LOG_PATH_KEY = "PrevLogPath";

        /// <summary>
        /// 缓存记录日志Info的key(存在PREV_LOG_PATH_KEY,一定存在LogInfo)
        /// </summary>
        private const string PREV_LOG_INFO_KEY = "PrevLogInfo";

        /// <summary>
        /// 当前缓存记录日志Info的key
        /// </summary>
        private const string LOG_INFO_KEY = "LogInfo";

    #endregion

        /// <summary>
        /// 日志路径
        /// </summary>
        public static string Path{ get; private set; }

        /// <summary>
        /// 是否接收日志
        /// </summary>
        public static bool ReceivedLog{
            get{ return s_enabledConsoleLog; }
            set{
                if (value == s_enabledConsoleLog){
                    return;
                }

                s_enabledConsoleLog = value;
                if (value){
                    Application.logMessageReceivedThreaded += Instance.OnReceiveLog;
                }
                else{
                    Application.logMessageReceivedThreaded -= Instance.OnReceiveLog;
                }
            }
        }

    #region Print message with trace

        /// <summary>
        /// Log 堆栈
        /// </summary>
        public bool StackTraceMessages = false;

        /// <summary>
        /// Warning堆栈
        /// </summary>
        public bool StackTraceWarnings = false;

        /// <summary>
        /// Error堆栈
        /// </summary>
        public bool StackTraceErrors = true;

    #endregion

        private static StreamWriter s_streamWriter;

        public static StreamWriter S_StreamWriter{
            get{
                if (s_streamWriter == null){
                    s_streamWriter = new StreamWriter(Path, true);
                }

                return s_streamWriter;
            }
        }

        private static bool s_enabledConsoleLog = false;

        /// <summary>
        /// IP地址信息（防止在非主线程获取)
        /// </summary>
        private static string s_ipAddress = "null";

        private static bool                        s_initComplete = false;
        private static bool                        s_isDestroy    = false;
        private        int                         _errorCount    = 0;
        private        int                         _errorLogCount = 0;
        private        Dictionary<string, LogInfo> _logRecord     = new Dictionary<string, LogInfo>();
        private static List<string>                _filterKeys    = new List<string>(){ "[玩家路径]", "[设备信息]", };

        public override void OnDispose(){
            // OnStandardQuit();
            Release();
        }

        public override void Init(){
            base.Init();
            SetQuitFunc();
            s_ipAddress    = IPAddressHelper.GetIP(ADDRESSFAM.IPv4);
            _errorCount    = 0;
            _errorLogCount = 0;

            //Set console log to unity editor
            //需要处理一下UnityEditor的Redirect后的最后入栈几行，去除UnitySystemConsoleRedirector相关部分，防止Error定位误差
            UnitySystemConsoleRedirector.Redirect();
        }

        /// <summary>
        /// 初始化(Release)
        /// </summary>
        /// <param name="path">日志路径</param>
        /// <param name="onLogCallback">日志回调</param>
        public void Setup(LogConfig logConfig){
            this.Init();
            ReceivedLog      = logConfig.ReceiveLog;
            s_SavePath       = $"{Application.persistentDataPath}/Log";
            s_URL            = logConfig.LogUploadURL;
            s_Upload         = logConfig.UploadLog;
            s_TokenKey       = logConfig.Token;
            s_MaxResendCount = logConfig.MaxResendCount;
            s_ResendInterval = logConfig.ErrorResendInterval;
            //TODO:if need dev
            // if (dev){
            //     s_URL = logConfig.LogUploadURLDev;
            // }
            DontDestroyOnLoad(gameObject);
            string logPath   = string.Format(FORMAT_PATH, s_SavePath, DeviceInfo.GetDate());
            string directory = logPath.Substring(0, logPath.LastIndexOf('/'));
            if (!Directory.Exists(directory)){
                Directory.CreateDirectory(directory);
            }

            //设置日志Info参数
            string logInfoJsonStr = PlayerPrefs.HasKey(LOG_INFO_KEY) ? PlayerPrefs.GetString(LOG_INFO_KEY) : null;
            PlayerPrefs.SetString(PREV_LOG_INFO_KEY, logInfoJsonStr); //设置之前的日志Infokey
            PlayerPrefs.SetString(LOG_INFO_KEY,      String.Empty);

            //设置日志参数
            var prevLogPath = PlayerPrefs.HasKey(LOG_PATH_KEY) ? PlayerPrefs.GetString(LOG_PATH_KEY) : null; //记录之前日志的路径
            PlayerPrefs.SetString(PREV_LOG_PATH_KEY, prevLogPath);                                           //设置之前的日志路径key
            PlayerPrefs.SetString(LOG_PATH_KEY,      logPath);
            LogHelper.Log("[Current][日志路径]:" + logPath);
            LogHelper.Log("[Pre][日志路径]:"     + prevLogPath);
            Path = logPath; //路径地址
            OnStandardStart();
            LogHelper.Log($"[设备信息]:{DeviceInfo.GetAllMemory()}");
            s_initComplete = true;
            s_isDestroy    = false;

            //Upload
            try{
                if (s_Upload){
                    UploadLogFile(s_URL);
                }
            }
            catch (Exception e){
                LogHelper.Log($"[日志]:上传日志错误{e}", LogType.Error);
            }
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message">打印消息</param>
        public static void Log(object message, LogType logType = LogType.Log){
            if (!ReceivedLog){
                return;
            }

            string messageStr = message == null ? NULL_STR : message.ToString();
            string content    = string.Empty;
            switch (logType){
                case LogType.Log:
                    content = string.Format(LOG_PRINT_KEY, messageStr);
                    break;
                case LogType.Warning:
                    content = string.Format(WARING_PRINT_KEY, messageStr);
                    break;
                case LogType.Error:
                    content = string.Format(ERROR_PRINT_KEY, messageStr);
                    break;
            }

            WriteLog(content);
        }

        public void ManualUpload(){
            LogHelper.Log(GAME_FRONT_KEY);
            //将上次日志设置为本次日志
            string prePath = PlayerPrefs.GetString(LOG_PATH_KEY);
            PlayerPrefs.SetString(PREV_LOG_PATH_KEY, prePath);
            UploadLogFile(s_URL);
        }

        /// <summary>
        /// 上传整个日志
        /// </summary>
        public void UploadLogFile(string url){
            string logPath = PlayerPrefs.HasKey(PREV_LOG_PATH_KEY) ? PlayerPrefs.GetString(PREV_LOG_PATH_KEY) : null;
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

            string logInfoJson = PlayerPrefs.GetString(PREV_LOG_INFO_KEY);
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

        private string CreateToken(string serverFilePath){
            string token = $"{s_TokenKey}{serverFilePath}";
            return StringUtils.String2MD5(token);
        }

        private void OnUploadCallback(UnityWebRequest callback){
            if (callback.downloadHandler == null || callback.downloadHandler.text == null){
                LogHelper.Log($"[日志]：上传日志，服务器回调错误:{callback.error}", LogType.Error);
                return;
            }

            LogHelper.Log($"[日志] : {callback.downloadHandler.text}", LogType.Error);
            // JsonData convert = new JsonData(callback.downloadHandler.text);
            // LogHelper.Log($"[日志] : convert :{convert}");
        }

        private void OnUploadCallback(string callback){
            LogHelper.Log($"[日志] : convert :{callback}");
            if (string.IsNullOrEmpty(callback)){
                StopCoroutine(DelayOption(s_ResendInterval,  Resend));
                StartCoroutine(DelayOption(s_ResendInterval, Resend));
                return;
            }

            LogServerMsg msg  = JSONUtils.JsonToObject<LogServerMsg>(callback);
            int          code = msg.code;
            if (code == 0){
                LogHelper.Log($"[Log] : 上传日志成功 :{msg}");
            }
            else{
                StopCoroutine(DelayOption(s_ResendInterval,  Resend));
                StartCoroutine(DelayOption(s_ResendInterval, Resend));
                LogHelper.Log($"[Log] : 上传日志失败 :{msg} \n 重试:{_errorCount}", LogType.Error);
            }
        }

        private void Resend(){
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

        private void SetQuitFunc(){
            Application.quitting -= OnStandardQuit;
            Application.quitting += OnStandardQuit;
        }
        
        protected override void OnApplicationPause(bool pauseStatus){
            if (pauseStatus){
                LogHelper.Log(GAME_BACK_KEY);
            }
            else{
                LogHelper.Log(GAME_FRONT_KEY);
            }
        }

        private void OnApplicationQuit(){ LogHelper.Log("游戏退出！！"); }

        /// <summary>
        ///检查文件是否满足上传要求
        /// 1.不能包含正常退出游戏的======= Game End =======这个key
        /// 2.文件大小在1M内,超出则需要客户端手动切段
        /// </summary>
        private bool CheckFile(string logPath){
            if (string.IsNullOrEmpty(logPath)){
                return false;
            }

            FileInfo fileInfo = new FileInfo(string.Format(logPath));
            if (!fileInfo.Exists){
                return false;
            }

            using (StreamReader reader = fileInfo.OpenText()){
                string txt       = reader.ReadToEnd();
                int    manualKey = txt.CalcKeyInStr(GAME_MANUAL_KEY);
                if (manualKey > 0){
                    LogHelper.Log("手动设置上传!");
                    return true;
                }

                //正常退出 
                if (txt.Contains(GAME_END_KEY)){
                    LogHelper.Log("上次正常退出，不上传!");
                    return false;
                }

                //杀进程(如果是app崩溃，1：不会有backKey，2：backKey数量=frontKey)
                int backKeyCount  = txt.CalcKeyInStr(GAME_BACK_KEY);
                int frontKeyCount = txt.CalcKeyInStr(GAME_FRONT_KEY);
                if (backKeyCount > frontKeyCount){
                    //当前只统计前台崩溃，后台崩溃暂不处理
                    LogHelper.Log($"上次杀进程正常退出，不上传 -> 后台次数 {backKeyCount} : -> 前台次数 {frontKeyCount}!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 日志回调
        /// </summary>
        /// <param name="condition">日志消息</param>
        /// <param name="stackTrace">调用堆栈</param>
        /// <param name="type">日志类型</param>
        private void OnReceiveLog(string condition, string stackTrace, LogType type){
            // Log(string.Format(FORMAT_CONSOLE, type, condition));

            //存堆栈
            if (type == LogType.Assert && !Instance.StackTraceErrors){ return; }

            if (type == LogType.Error && !Instance.StackTraceErrors){ return; }

            if (type == LogType.Exception && !Instance.StackTraceErrors){ return; }

            if (type == LogType.Log && !Instance.StackTraceMessages){ return; }

            if (type == LogType.Warning && !Instance.StackTraceWarnings){ return; }

            if (type == LogType.Error){
                _errorLogCount++;
                if (_errorLogCount > ERROR_COUNT_2_HOOK_DEVICE_STATE){
                    _errorLogCount = 0;
                    LogHelper.Log($"[设备信息]:{DeviceInfo.GetAllMemory()}");
                }
            }

            string insertLog = $"{condition}\n{stackTrace}";
            WriteLog(insertLog);
        }

        /// <summary>
        /// 正常进入游戏
        /// </summary>
        private void OnStandardStart(){
            LogHelper.Log(GAME_START_KEY);
            DateTime date = DateTime.Now;
            LogHelper.Log(date.ToString("yyyy-M-d HH:mm:ss"));
        }

        /// <summary>
        /// 正常退出游戏
        /// </summary>
        private void OnStandardQuit(){ LogHelper.Log(GAME_END_KEY); }

        /// <summary>
        /// 简单创建压缩zip文件
        /// 需要注意如果使用ICSharpCode打移动包的话，需要把新版的Unity（如2019）的安装目录Editor\Data\MonoBleedingEdge\lib\mono\unityjit下找到I18N.dll和I18N.West.dll文件
        /// 单独拎出来到Assets目录或者Plugin目录一起打包才行
        /// </summary>
        /// <param name="file">需要压缩的文件</param>
        /// <param name="outputFilePath">输出的路径</param>
        /// <param name="compressLevel">压缩等级0-9</param>
        private void ZipFile(string file, string outputFilePath, int compressLevel){
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

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="binary">数据</param>
        /// <returns></returns>
        private byte[] Compress(byte[] binary){
            //TODO: old
            // MemoryStream     ms   = new MemoryStream();
            // GZipOutputStream gzip = new GZipOutputStream(ms);
            // gzip.Write(binary, 0, binary.Length);
            // gzip.Close();
            // byte[] press = ms.ToArray();
            // return press;
            return binary;
        }

        private static bool JudgeSave(string content){
            int len = _filterKeys?.Count ?? 0;
            for (int i = 0; i < len; ++i){
                if (content.Contains(_filterKeys[i])){
                    return false;
                }
            }

            return true;
        }

        private static void SaveLogInfo(string content){
            if (!s_initComplete || s_isDestroy || !JudgeSave(content)){
                return;
            }

            const int keyLen = 128;
            int       len    = content.Length;
            string    key    = content.Substring(0, len < keyLen ? len : keyLen);
            if (Instance._logRecord.TryGetValue(key, out var info)){
                info.AddCount(key);
            }
            else{
                string val = content;
                Instance._logRecord.Add(key, new LogInfo(key, val));
            }

            //TODO:根据测试来，频繁To和set会有性能问题
            // string json = JsonMapper.ToJson(Instance._logRecord);
            string json = JSONUtils.ObjectToJson(Instance._logRecord);
            PlayerPrefs.SetString(LOG_INFO_KEY, json);
        }

        private static void WriteLog(string content){
            try{
                SaveLogInfo(content);
                S_StreamWriter.WriteLine(content);
                S_StreamWriter.Flush();
            }
            catch (Exception e){
                Console.WriteLine($"[LogAnalyzer] 写入log错误:{e}");
            }
        }

        private void Release(){
            s_isDestroy          =  true;
            Application.quitting -= OnStandardQuit;
            ReceivedLog          =  false;
            S_StreamWriter.Flush();
            S_StreamWriter.Dispose();
        }

        IEnumerator DelayOption(float delayTime, Action callback){
            yield return new WaitForSeconds(delayTime);
            callback?.Invoke();
        }
    }
}