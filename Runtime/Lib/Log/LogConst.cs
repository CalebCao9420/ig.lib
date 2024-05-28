namespace IG.Runtime.Log{
    public static class LogConst{
        public const string GAME_MANUAL_KEY = "======= Game Manual Upload =======";
        public const string GAME_START_KEY  = "======= Game Start =======";
        public const string GAME_END_KEY    = "======= Game End =======";
        public const string GAME_BACK_KEY   = "======= Game Back =======";
        public const string GAME_FRONT_KEY  = "======= Game Front =======";

        /// <summary>
        /// 缓存记录日志路径key
        /// </summary>
        public const string LOG_PATH_KEY = "LogPath";

        /// <summary>
        /// 当前缓存记录日志Info的key
        /// </summary>
        public const string LOG_INFO_KEY = "LogInfo";

        /// <summary>
        /// 缓存记录之前日志路径key
        /// </summary>
        public const string PREV_LOG_PATH_KEY = "PrevLogPath";

        /// <summary>
        /// 缓存记录日志Info的key(存在PREV_LOG_PATH_KEY,一定存在LogInfo)
        /// </summary>
        public const string PREV_LOG_INFO_KEY = "PrevLogInfo";

        public const string DEVICE_ID = "ig.id";
    }
}