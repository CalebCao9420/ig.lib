using UnityEngine;

namespace IG.Runtime.Log{
    public static class LogHelper{
        public static void Log(this object tag, object logContent, LogType logType = LogType.Log){
            string rel = $"[{tag}] {logContent}";
            Debug.unityLogger.Log(logType, rel);
        }
        
        public static void Log(this object logContent, LogType logType = LogType.Log){
            Debug.unityLogger.Log(logType, logContent);
        }
        
        public static void Log(this string logContent, LogType logType = LogType.Log){
            Debug.unityLogger.Log(logType, logContent);
        }
    }
}