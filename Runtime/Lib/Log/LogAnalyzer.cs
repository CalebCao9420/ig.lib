using System;
using System.Collections.Generic;
using System.IO;
using IG.Runtime.Utils;
using UnityEngine;

namespace IG.Runtime.Log{
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

        private static string s_SavePath;

    #endregion

    #region Const params

        public const string NULL_STR         = "NULL";
        public const string LOG_PRINT_KEY    = "[LOG]:{0}";
        public const string WARING_PRINT_KEY = "[WARNIING]:{0}";
        public const string ERROR_PRINT_KEY  = "[ERROR]:{0}";

        /// <summary>
        /// 错误日志达到一定数量hook 设备信息
        /// </summary>
        private const int ERROR_COUNT_2_HOOK_DEVICE_STATE = 10;

        private const int    SIZE_LIMIT  = 1024 * 1024;
        private const string FORMAT_PATH = "{0}/Log_{1}.txt";

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
        private        int                         _errorLogCount = 0;
        private        Dictionary<string, LogInfo> _logRecord     = new Dictionary<string, LogInfo>();
        private static List<string>                _filterKeys    = new List<string>(){ "[玩家路径]", "[设备信息]", };

        public override void OnDispose(){
            // OnStandardQuit();
            Release();
        }

        public override void Init(){
            base.Init();
            SetLogHandler();
            SetQuitFunc();
            s_ipAddress    = IPAddressHelper.GetIP(ADDRESSFAM.IPv4);
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
        public void Setup(bool receiveLog){
            // this.Init();
            ReceivedLog = receiveLog;
            s_SavePath  = $"{Application.persistentDataPath}/Log";
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

            const string logInfoKey    = LogConst.LOG_INFO_KEY;
            const string logPreInfoKey = LogConst.PREV_LOG_INFO_KEY;
            const string logPathKey    = LogConst.LOG_PATH_KEY;
            const string logPrePathKey = LogConst.PREV_LOG_PATH_KEY;

            //设置日志Info参数
            string logInfoJsonStr = PlayerPrefs.HasKey(logInfoKey) ? PlayerPrefs.GetString(logInfoKey) : null;
            PlayerPrefs.SetString(logPreInfoKey, logInfoJsonStr); //设置之前的日志Infokey
            PlayerPrefs.SetString(logInfoKey,    String.Empty);

            //设置日志参数
            var prevLogPath = PlayerPrefs.HasKey(logPathKey) ? PlayerPrefs.GetString(logPathKey) : null; //记录之前日志的路径
            PlayerPrefs.SetString(logPrePathKey, prevLogPath);                                           //设置之前的日志路径key
            PlayerPrefs.SetString(logPathKey,    logPath);
            Path           = logPath; //路径地址
            LogHelper.Log("[Current][日志路径]:" + logPath);
            LogHelper.Log("[Pre][日志路径]:"     + prevLogPath);
            s_initComplete = true;
            s_isDestroy    = false;
            OnStandardStart();
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

        private void SetLogHandler(){
            Debug.unityLogger.logHandler = new IGLogHandler(Debug.unityLogger.logHandler);
        }

        private void SetQuitFunc(){
            Application.quitting -= OnStandardQuit;
            Application.quitting += OnStandardQuit;
        }

        protected override void OnApplicationPause(bool pauseStatus){
            if (pauseStatus){
                LogHelper.Log(LogConst.GAME_BACK_KEY);
            }
            else{
                LogHelper.Log(LogConst.GAME_FRONT_KEY);
            }
        }

        private void OnApplicationQuit(){ LogHelper.Log("游戏退出！！"); }

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
            LogHelper.Log(LogConst.GAME_START_KEY);
            DateTime date = DateTime.Now;
            LogHelper.Log(date.ToString(StringUtils.DATE_TIME_FORMAT_FULL));
            LogHelper.Log($"[设备信息]:{DeviceInfo.GetAllMemory()}");
        }

        /// <summary>
        /// 正常退出游戏
        /// </summary>
        private void OnStandardQuit(){ LogHelper.Log(LogConst.GAME_END_KEY); }

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
            PlayerPrefs.SetString(LogConst.LOG_INFO_KEY, json);
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
    }
}