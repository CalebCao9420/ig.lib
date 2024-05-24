using UnityEngine;

namespace IG.Runtime.Log{
    public static class LogHelper{
        public static void Log(object logContent, LogType logType = LogType.Log, bool displayConsole = true){
            if (displayConsole){
                Debug.unityLogger.Log(logType, logContent);
            }

            LogAnalyzer.Log(logContent, logType);
        }

        public static void Log(this object tag, object logContent, LogType logType = LogType.Log, bool displayConsole = true){
            string rel = $"[{nameof(tag)}] {logContent}";
            Log(rel, logType, displayConsole);
        }
    }
}