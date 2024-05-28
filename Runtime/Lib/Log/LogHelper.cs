using UnityEngine;

namespace IG.Runtime.Log{
    public static class LogHelper{
        public static void Log(object logContent, LogType logType = LogType.Log){
            string content = string.Empty;
            switch (logType){
                case LogType.Log:
                    content = string.Format(LogAnalyzer.LOG_PRINT_KEY, logContent);
                    break;
                case LogType.Warning:
                    content = string.Format(LogAnalyzer.WARING_PRINT_KEY, logContent);
                    break;
                case LogType.Error:
                    content = string.Format(LogAnalyzer.ERROR_PRINT_KEY, logContent);
                    break;
            }

            Debug.unityLogger.Log(logType, content);
            LogAnalyzer.Log(logContent, logType);
        }

        public static void Log(this object tag, object logContent, LogType logType = LogType.Log){
            string rel = $"[{tag.ToString()}] {logContent}";
            Log(rel, logType);
        }
    }
}