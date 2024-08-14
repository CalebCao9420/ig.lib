using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Runtime.Log{
    public class IGLogHandler : ILogHandler{
        private readonly ILogHandler _defaultLogHandler = Debug.unityLogger.logHandler;

        public void LogException(Exception exception, UnityEngine.Object context){
            _defaultLogHandler.LogException(exception, context);
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args){
            string formatMsg = string.Format(format, args);
            
            string logMessage = string.Empty;
            switch (logType){
                case LogType.Log:
                    logMessage = string.Format(LogAnalyzer.LOG_PRINT_KEY, formatMsg);
                    break;
                case LogType.Warning:
                    logMessage = string.Format(LogAnalyzer.WARING_PRINT_KEY, formatMsg);
                    break;
                case LogType.Error:
                    logMessage = string.Format(LogAnalyzer.ERROR_PRINT_KEY, formatMsg);
                    break;
            }

            if (null != context){
                logMessage = $"[{context}] {logMessage}";
            }

            _defaultLogHandler.LogFormat(logType, context, format, logMessage);
        }
    }
}