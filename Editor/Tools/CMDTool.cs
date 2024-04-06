using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace IG.Editor.Tools{
    /// <summary>
    /// 命令行工具
    /// </summary>
    public sealed class CMDTool{
        private const string CHAR_ARGUMENT = "-";

        private static readonly string PATH_SH = Application.temporaryCachePath +
                                                 (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows ? "/temp.bat" : "/temp.sh");

        private static readonly string NAME_SH = SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows ? "cmd.exe" : "/bin/sh";

        /// <summary>
        /// 获取命令行参数
        /// </summary>
        /// <returns>返回命令行参数</returns>
        public static Dictionary<string, string> GetArguments(){
            string[]                   cmdArgs = Environment.GetCommandLineArgs();
            Dictionary<string, string> argMap  = new Dictionary<string, string>();
            for (int i = 0; i < cmdArgs.Length - 1; ++i){
                string arg1 = cmdArgs[i];
                string arg2 = cmdArgs[i + 1];
                if (!arg1.StartsWith(CHAR_ARGUMENT) || arg2.StartsWith(CHAR_ARGUMENT)){
                    continue;
                }

                argMap[arg1.Substring(1)] = arg2;
            }

            return argMap;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="name">命令名</param>
        /// <param name="args">命令参数</param>
        /// <param name="path">调用目录</param>
        public static void Excute(string name, string args, string path = null){
            ProcessStartInfo processInfo = new ProcessStartInfo(name);
            processInfo.Arguments = args;
            if (!string.IsNullOrEmpty(path)){
                processInfo.WorkingDirectory = path;
            }

            processInfo.UseShellExecute        = false;
            processInfo.CreateNoWindow         = true;
            processInfo.RedirectStandardError  = true;
            processInfo.RedirectStandardOutput = true;
            Process process = Process.Start(processInfo);
            string  output  = process.StandardOutput.ReadToEnd();
            string  error   = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.Close();
            if (!string.IsNullOrEmpty(output)){
                UnityEngine.Debug.Log(output);
            }

            if (!string.IsNullOrEmpty(error)){
                UnityEngine.Debug.LogError(error);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">命令内容</param>
        public static void Excute(string command){
            File.WriteAllText(PATH_SH, command);
            Excute(NAME_SH, PATH_SH);
            File.Delete(PATH_SH);
        }
    }
}