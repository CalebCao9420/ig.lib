using System.IO;
using System;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace IG.Editor.Tools{
    public static class BuildTool{
        /// <remarks>计算MD5</remarks>
        public static string CalculateMD5(string file){
            string     result = "";
            FileStream fs     = new FileStream(file, FileMode.Open);
            try{
                System.Security.Cryptography.MD5 md5    = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[]                           retVal = md5.ComputeHash(fs);
                fs.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++){
                    sb.Append(retVal[i].ToString("x2"));
                }

                result = sb.ToString();
            }
            catch (Exception e){
                UnityEngine.Debug.Log("md5file() fail, error:" + e.Message);
            }
            finally{
                fs.Close();
            }

            return result;
        }

        /// <remarks>文件复制</remarks>
        public static void CopyFile(string filePath, string targetPath){
            FileInfo file = new FileInfo(filePath);
            if (!Directory.Exists(targetPath)){
                Directory.CreateDirectory(targetPath);
            }

            if (file != null){
                string tempPath = Path.Combine(targetPath, file.Name);
                file.CopyTo(tempPath, true); //如果文件存在则覆盖
            }
        }

        /// <remarks>文件夹复制</remarks>
        public static void CopyDirectory(string sourcePath, string targetPath, bool containSubDirs){
            DirectoryInfo dir = new DirectoryInfo(sourcePath);
            if (!dir.Exists){
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(targetPath)){
                Directory.CreateDirectory(targetPath);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files){
                string tempPath = Path.Combine(targetPath, file.Name);
                file.CopyTo(tempPath, true); //如果文件存在则覆盖
            }

            if (containSubDirs){
                foreach (var subDir in dirs){
                    string tempPath = Path.Combine(targetPath, subDir.Name);
                    CopyDirectory(subDir.FullName, tempPath, containSubDirs);
                }
            }
        }

        /// <remarks>删除指定的文件夹</remarks>
        public static void DeleteFolder(string path){
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists){
                FileInfo[] files = dir.GetFiles();
                for (int i = 0; i < files.Length; i++){
                    files[i].Delete();
                    files[i] = null;
                }

                files = null;
                var dirs = dir.GetDirectories();
                for (int i = 0; i < dirs.Length; i++){
                    Directory.Delete(dirs[i].FullName, true);
                }
            }
        }
    }
}