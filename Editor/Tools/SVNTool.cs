using System.IO;

namespace IG.Editor.Tools{
    /// <summary>
    /// SVN工具
    /// </summary>
    public sealed class SVNTool{
        private const string CMD_SVN    = "svn";
        private const string CMD_CLEAN  = "cleanup \"{0}\" --remove-unversioned";
        private const string CMD_REVERT = "revert \"{0}\" -R";
        private const string CMD_UPDATE = "update \"{0}\"";
        private const string CMD_COMMIT = "commit \"{0}\" -m \"{1}\"";
        private const string CMD_IMPORT = "import \"{0}\" \"{1}\" -m \"{2}\"";
        private const string CMD_EXPORT = "export \"{0}\" \"{1}\" --force";

        /// <summary>
        /// 清理
        /// </summary>
        /// <param name="path">路径</param>
        public static void Clean(string path = null){ CMDTool.Excute(CMD_SVN, string.Format(CMD_CLEAN, path ?? Directory.GetCurrentDirectory())); }

        /// <summary>
        /// 还原
        /// </summary>
        /// <param name="path">路径</param>
        public static void Revert(string path = null){ CMDTool.Excute(CMD_SVN, string.Format(CMD_REVERT, path ?? Directory.GetCurrentDirectory())); }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="path">路径</param>
        public static void Update(string path = null){ CMDTool.Excute(CMD_SVN, string.Format(CMD_UPDATE, path ?? Directory.GetCurrentDirectory())); }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="msg">注释</param>
        public static void Commit(string path = null, string msg = null){
            CMDTool.Excute(CMD_SVN, string.Format(CMD_COMMIT, path ?? Directory.GetCurrentDirectory(), msg ?? string.Empty));
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="url">地址</param>
        /// <param name="msg">注释</param>
        public static void Import(string path, string url, string msg = null){
            CMDTool.Excute(CMD_SVN, string.Format(CMD_IMPORT, path, url, msg ?? string.Empty));
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="path">路径</param>
        public static void Export(string url, string path){ CMDTool.Excute(CMD_SVN, string.Format(CMD_EXPORT, url, path)); }
    }
}