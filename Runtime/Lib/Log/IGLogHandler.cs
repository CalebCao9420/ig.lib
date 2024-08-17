using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.Runtime.Log{
    public class IGLogHandler : ILogHandler{
        private readonly ILogHandler _defaultLogHandler;

        public IGLogHandler(ILogHandler logHandler){
            _defaultLogHandler = logHandler;
        }

        public void LogException(Exception exception, UnityEngine.Object context){
            _defaultLogHandler.LogException(exception, context);
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args){
            string formatStr = string.Empty;
            switch (logType){
                case LogType.Log:
                    formatStr = LogAnalyzer.LOG_PRINT_KEY;
                    break;
                case LogType.Warning:
                    formatStr = LogAnalyzer.WARING_PRINT_KEY;
                    break;
                case LogType.Error:
                    formatStr = LogAnalyzer.ERROR_PRINT_KEY;
                    break;
            }

            if (null != context){
                formatStr = $"[{context}] {formatStr}";
            }

            _defaultLogHandler.LogFormat(logType, context, formatStr, args);
        }
    }
}