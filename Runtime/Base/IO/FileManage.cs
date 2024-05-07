using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using IG.Runtime.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IG.IO{
    /// <summary>
    /// File manage.
    /// TODO:打包考虑，去除TMPro和spine-unity内容
    /// </summary>
    public class FileManage{
    #region 判断文件是否存在.

        /// <summary>
        /// 判断文件是否存在.
        /// </summary>
        /// <param name="path">Path.</param>
        public static bool Exists(string path, DirectoryType directoryType = DirectoryType.UNKNOWN){ return File.Exists(PathFormat(path, directoryType)); }

    #endregion

    #region 判断文件夹是否存在.

        /// <summary>
        /// 判断文件夹是否存在.
        /// </summary>
        /// <returns><c>true</c>, if exists was directoryed, <c>false</c> otherwise.</returns>
        /// <param name="path">Path.</param>
        public static bool DirectoryExists(string path, DirectoryType directoryType = DirectoryType.UNKNOWN){
            return Directory.Exists(PathFormat(path, directoryType));
        }

    #endregion

    #region 生成路径.

        /// <summary>
        /// Assets路径生成.
        /// </summary>
        /// <returns>The assets path format.</returns>
        /// <param name="relativePath">Relative path.</param>
        private static string AssetsPathFormat(string relativePath){
            string path = "";
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android){
                path = (Application.persistentDataPath + "/" + relativePath).Replace(" ", "%20");
            }
            else{
                path = Application.dataPath + "/" + relativePath;
            }

            return path;
        }

        /// <summary>
        /// Resources路径生成.
        /// </summary>
        /// <returns>The assets path format.</returns>
        /// <param name="relativePath">Relative path.</param>
        private static string ResourcesPathFormat(string relativePath){
            string path = "";
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WebGLPlayer){
                path = (Application.persistentDataPath + "/Resources/" + relativePath).Replace(" ", "%20");
            }
            else{
                path = Application.dataPath + "/Resources/" + relativePath;
            }

            return path;
        }

        /// <summary>
        /// StreamingAssets路径生成.
        /// </summary>
        /// <returns>The assets path format.</returns>
        /// <param name="relativePath">Relative path.</param>
        private static string StreamingPathFormat(string relativePath){
            if (Application.platform == RuntimePlatform.Android){
                return (Application.streamingAssetsPath + "/" + relativePath).Replace(" ", "%20");
            }

            return (Application.streamingAssetsPath + "/" + relativePath).Replace(" ", "%20");
        }

        /// <summary>
        /// Paths the format.
        /// </summary>
        /// <returns>The format.</returns>
        /// <param name="relativePath">Relative path.</param>
        /// <param name="directoryType">Directory type.</param>
        public static string PathFormat(string relativePath, DirectoryType directoryType){
            switch (directoryType){
                case DirectoryType.ASSETS:    return AssetsPathFormat(relativePath);
                case DirectoryType.RESOURCES: return ResourcesPathFormat(relativePath);
                case DirectoryType.STREAMING: return StreamingPathFormat(relativePath);
                default:                      return relativePath;
            }
        }

    #endregion

    #region 把数据以二进制的形式写入到指定文件中.

        private static void SetNoBackupFlag(string filePath){
#if UNITY_IOS
            if (File.Exists (filePath)) {
                return;
            }
            UnityEngine.iOS.Device.SetNoBackupFlag (filePath);
#endif
        }

        /// <summary>
        /// Writes all bytes.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="bytes">Bytes.</param>
        /// <param name="directoryType">Directory type default UNKNOWN.</param>
        public static void WriteAllBytes(
            string        filePath,
            byte[]        bytes,
            DirectoryType directoryType = DirectoryType.UNKNOWN){
            SetNoBackupFlag(filePath);
            try{
                string dir = Path.GetDirectoryName(PathFormat(filePath, directoryType));
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e){
                //to do
                Debug.Log("IO error." + e.Message);
            }

            try{
                File.WriteAllBytes(PathFormat(filePath, directoryType), bytes);
            }
            catch (Exception e){
                Debug.Log("IO error ." + e.Message);
            }
        }

    #endregion

    #region 在文件内容末尾继续写入.

        /// <summary>
        /// Appends all text.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="str">String.</param>
        public static void AppendAllText(
            string        filePath,
            string        str,
            DirectoryType directoryType = DirectoryType.UNKNOWN){
            SetNoBackupFlag(filePath);
            try{
                string dir = Path.GetDirectoryName(PathFormat(filePath, directoryType));
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e){
                //to do
                Debug.Log("IO error." + e.Message);
            }

            try{
                File.AppendAllText(PathFormat(filePath, directoryType), str);
            }
            catch (Exception e){
                Debug.Log("IO error ." + e.Message);
            }
        }

        /// <summary>
        /// Appends all bytes.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="bytes">Bytes.</param>
        /// <param name="directoryType">Directory type.</param>
        public static void AppendAllBytes(
            string        filePath,
            byte[]        bytes,
            DirectoryType directoryType = DirectoryType.UNKNOWN){
            SetNoBackupFlag(filePath);
            try{
                string dir = Path.GetDirectoryName(PathFormat(filePath, directoryType));
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e){
                //to do
                Debug.Log("IO error." + e.Message);
            }

            try{
                using (FileStream fsWrite = new FileStream(PathFormat(filePath, directoryType), FileMode.Append)){
                    for (int i = 0; i < bytes.Length; i++){
                        fsWrite.WriteByte(bytes[i]);
                    }

                    //fsWrite.Write(bytes, 0, bytes.Length);
                    fsWrite.Flush();
                }

                ;
            }
            catch (Exception e){
                Debug.Log("IO error ." + e.Message);
            }
        }

        public static void AppendLineBytes(
            string        filePath,
            byte[]        bytes,
            DirectoryType directoryType = DirectoryType.UNKNOWN){
            SetNoBackupFlag(filePath);
            try{
                string dir = Path.GetDirectoryName(PathFormat(filePath, directoryType));
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e){
                //to do
                Debug.Log("IO error." + e.Message);
            }

            try{
                using (FileStream fsWrite = new FileStream(PathFormat(filePath, directoryType), FileMode.Append)){
                    AppendLineBytes(fsWrite, bytes);
                }

                ;
            }
            catch (Exception e){
                Debug.Log("IO error ." + e.Message);
            }
        }

        public static void AppendLineBytes(FileStream fsWrite, byte[] bytes){
            try{
                if (fsWrite.Position != 0){
                    fsWrite.WriteByte(10);
                }

                fsWrite.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e){
                Debug.Log("IO error ." + e.Message);
            }
        }

        public static GFileStream OpenFileStream(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            SetNoBackupFlag(filePath);
            try{
                string dir = Path.GetDirectoryName(PathFormat(filePath, directoryType));
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e){
                //to do
                Debug.Log("IO error." + e.Message);
            }

            try{
                return new GFileStream(PathFormat(filePath, directoryType), FileMode.Append);
            }
            catch (Exception e){
                Debug.Log("IO error ." + e.Message);
            }

            return null;
        }

    #endregion

    #region bytes to object

        public static object Bytes2Object(byte[] buff){
            object obj;
            using (Stream ms = new MemoryStream(buff)){
                IFormatter iFormatter = new BinaryFormatter();
                obj = iFormatter.Deserialize(ms);
            }

            return obj;
        }

        public static object FileToObject(string path){
            IFormatter iFormatter = new BinaryFormatter();
            Stream     stream     = null;
            object     obj        = null;
            try{
                Console.WriteLine("[FileManager] Path =" + path);
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                obj    = iFormatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception ex){
                if (stream != null) stream.Close();
                throw ex;
            }

            return obj;
        }

        public static object FileToObject(FileStream stream){
            IFormatter iFormatter = new BinaryFormatter();
            object     obj        = null;
            try{
                obj = iFormatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception ex){
                if (stream != null) stream.Close();
                throw ex;
            }

            return obj;
        }

        /// <summary>
        /// String to MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToMD5(string str){
            string md5   = string.Empty;
            byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < bytes.Length; ++i){
                md5 += bytes[i].ToString("x").PadLeft(2, '0');
            }

            return md5;
        }

        public static List<string> GetAllDirPath(string objPath){
            List<string> result = new List<string>();
            var          temp   = Directory.GetDirectories(objPath);
            for (int i = 0; i < temp.Length; i++){
                result.Add(temp[i]);
                var childDir = GetAllDirPath(temp[i]);
                if (childDir != null && childDir.Count > 0){
                    result.AddRange(childDir);
                }
            }

            return result;
        }

        public static T[] LoadAsset<T>(string path, string pattern, string format = ".") where T : Object{
            // string objPath = Application.dataPath + path;
            Console.WriteLine("[FileManager] Path =" + path);
            string   objPath = path;
            string[] directoryEntries;
            List<T>  objList = new List<T>();
            try{
                directoryEntries = Directory.GetFileSystemEntries(objPath);
                for (int i = 0; i < directoryEntries.Length; i++){
                    string p = directoryEntries[i];
                    if (p.EndsWith(".meta")) continue;
                    if (Directory.Exists(p)){
                        var tempList = LoadAsset<T>(p, pattern, format);
                        if (tempList != null && tempList.Length > 0){
                            objList.AddRange(tempList);
                        }
                    }

                    if (p.EndsWith(format + pattern)){
                        // T result = ResLoad.Instance.StarLoad<T> (p);
                        T   result = default; //Deserialize<T>(ReadBytes(p));
                        var _type  = typeof(T);
                        if (_type == typeof(Texture) || _type == typeof(Texture2D)){
                            result = ReadTexture2D(p, 1, 1) as T;
                        }

                        if (_type == typeof(TextAsset)){
                            TextAsset asset = new TextAsset(ReadString(p));
                            asset.name = StringUtils.PathRemoveDir(StringUtils.CutSuffix(p));
                            result     = asset as T;
                        }

                        if (result != null) objList.Add(result);
                    }
                }
            }
            catch (DirectoryNotFoundException){
                Debug.Log("The path encapsulated in the " + objPath + "Directory object does not exist.");
            }

            if (objList.Count > 0) return objList.ToArray();
            return null;
        }

        public static T Deserialize<T>(byte[] data) where T : class{
            if (data == null || !typeof(T).IsSerializable) return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data)){
                object obj = formatter.Deserialize(stream);
                return obj as T;
            }
        }

        public ArrayList LoadFile(string path, string name){
            StreamReader sr = null;
            try{
                sr = File.OpenText(path + "//" + name);
            }
            catch (Exception e){
                Console.WriteLine($"[FileManager] Load file not exist :{e}");
                return null;
            }

            string    line;
            ArrayList arrayList = new ArrayList();
            while ((line = sr.ReadLine()) != null){
                arrayList.Add(line);
            }

            sr.Close();
            sr.Dispose();
            return arrayList;
        }

    #endregion

    #region 把字符串写入到文件中.

        /// <summary>
        /// 把字符串写入到文件中.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <param name="contents">Contents.</param>
        public static void WriteString(
            string        filePath,
            string        contents,
            DirectoryType directoryType = DirectoryType.UNKNOWN){
            byte[] bytes = Encoding.UTF8.GetBytes(contents);
            WriteAllBytes(filePath, bytes, directoryType);
        }

    #endregion

    #region 读取本地文件返回字符串.

        /// <summary>
        /// 读取本地文件返回字符串.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="filePath">File path.</param>
        public static string ReadString(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            if (!Exists(PathFormat(filePath, directoryType))){
                return "";
            }

            string       contents = "";
            StreamReader reader   = null;
            try{
                reader   = new StreamReader(PathFormat(filePath, directoryType));
                contents = reader.ReadToEnd();
            }
            catch (Exception e){
                Debug.Log("IO Read error ." + e.Message);
            }
            finally{
                if (reader != null){
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }

            return contents;
        }

        public static string ReadString(string filePath){
            if (!Exists(filePath)){
                return "";
            }

            string       contents = "";
            StreamReader reader   = null;
            try{
                reader   = new StreamReader(filePath);
                contents = reader.ReadToEnd();
            }
            catch (Exception e){
                Debug.Log("IO Read error ." + e.Message);
            }
            finally{
                if (reader != null){
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }

            return contents;
        }

    #endregion

    #region 读取本地文件返回字符串集合.

        /// <summary>
        /// 读取本地文件返回字符串集合.
        /// </summary>
        /// <returns>The lines.</returns>
        /// <param name="filePath">File path.</param>
        public static List<string> ReadLines(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            List<string> contents = new List<string>();
            try{
                if (!Exists(PathFormat(filePath, directoryType))){
                    return contents;
                }

                using (StreamReader reader = new StreamReader(PathFormat(filePath, directoryType))){
                    while (!reader.EndOfStream){
                        contents.Add(reader.ReadLine());
                    }

                    reader.Close();
                }
            }
            catch (Exception e){
                Debug.Log("IO Read error ." + e.Message);
            }

            return contents;
        }

    #endregion

    #region 读取本地文件返回二进制流.

        /// <summary>
        /// 读取本地文件返回二进制流.
        /// </summary>
        /// <returns>The bytes.</returns>
        /// <param name="filePath">File path.</param>
        public static byte[] ReadBytes(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            StreamReader reader = null;
            byte[]       buffer = null;
            if (!Exists(PathFormat(filePath, directoryType))){
                return buffer;
            }

            try{
                reader = new StreamReader(PathFormat(filePath, directoryType));
                Stream stream = reader.BaseStream;
                if (stream.CanRead){
                    buffer = new byte[(int)stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }

                reader.Close();
                reader.Dispose();
                reader = null;
            }
            catch (Exception e){
                Debug.Log("IO Read error ." + e.Message);
            }
            finally{
                if (reader != null){
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }

            return buffer;
        }

    #endregion

    #region 读取本地文件返回Texture2D.

        /// <summary>
        /// 读取本地文件返回Texture2D.
        /// </summary>
        /// <returns>The texture2d.</returns>
        /// <param name="filePath">File path.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public static Texture2D ReadTexture2D(
            string        filePath,
            int           width,
            int           height,
            DirectoryType directoryType = DirectoryType.UNKNOWN){
            StreamReader reader = null;
            byte[]       buffer = null;
            try{
                reader = new StreamReader(PathFormat(filePath, directoryType));
                Stream stream = reader.BaseStream;
                if (stream.CanRead){
                    buffer = new byte[(int)stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }

                reader.Close();
                reader.Dispose();
                reader = null;
            }
            catch (Exception e){
                Debug.Log("IO Read error .path:" + filePath + ". message:" + e.Message);
            }
            finally{
                if (reader != null){
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }

            if (buffer == null || buffer.Length == 0){
                return null;
            }

            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture2D.LoadImage(buffer);
            texture2D.anisoLevel = 2;
            texture2D.Compress(true);
            return texture2D;
        }

        public static Texture2D ReadTexture2D(string filePath, int width, int height){
            StreamReader reader = null;
            byte[]       buffer = null;
            try{
                reader = new StreamReader(filePath);
                Stream stream = reader.BaseStream;
                if (stream.CanRead){
                    buffer = new byte[(int)stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }

                reader.Close();
                reader.Dispose();
                reader = null;
            }
            catch (Exception e){
                Debug.Log("IO Read error .path:" + filePath + ". message:" + e.Message);
            }
            finally{
                if (reader != null){
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }

            if (buffer == null || buffer.Length == 0){
                return null;
            }

            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture2D.LoadImage(buffer);
            texture2D.anisoLevel = 2;
            texture2D.Compress(true);
            texture2D.name = StringUtils.PathRemoveDir(StringUtils.CutSuffix(filePath));
            return texture2D;
        }

        public static void WriteTexture2D(Texture2D source, string path, string fileName){
            var bytes  = source.EncodeToPNG();
            var file   = new FileStream(path + "/" + fileName + ".png", FileMode.Create);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();
        }

    #endregion

    #region 获取文件的hash值.

        /// <summary>
        /// 获取文件的hash值.
        /// </summary>
        /// <returns>
        /// The file's hash.
        /// </returns>
        /// <param name='filePath'>
        /// File path.
        /// </param>
        public static string GetFileHash(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            string   hashcode = "";
            FileInfo info     = new FileInfo(PathFormat(filePath, directoryType));
            if (info.Exists){
                FileStream filestream = null;
                try{
                    info.Refresh();
                    filestream = info.Open(FileMode.Open, FileAccess.ReadWrite);
                    byte[] bytes = System.Security.Cryptography.SHA1.Create().ComputeHash(filestream);
                    hashcode = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
                catch (Exception ex){
                    throw ex;
                }
                finally{
                    if (filestream != null){
                        filestream.Close();
                        filestream.Dispose();
                    }

                    filestream = null;
                }
            }

            return hashcode;
        }

        public static string GetFileHash(FileInfo info){
            string hashcode = "";
            if (info.Exists){
                FileStream filestream = null;
                try{
                    info.Refresh();
                    filestream = info.Open(FileMode.Open, FileAccess.ReadWrite);
                    byte[] bytes = System.Security.Cryptography.SHA1.Create().ComputeHash(filestream);
                    hashcode = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                }
                catch (Exception ex){
                    throw ex;
                }
                finally{
                    if (filestream != null){
                        filestream.Close();
                        filestream.Dispose();
                    }

                    filestream = null;
                }
            }

            return hashcode;
        }

        public static string GetFileHash(byte[] data){
            string hashcode = "";
            try{
                byte[] bytes = System.Security.Cryptography.SHA1.Create().ComputeHash(data);
                hashcode = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
            catch (Exception ex){
                throw ex;
            }

            return hashcode;
        }

    #endregion

    #region 获取文本内容的hash值

        /// <summary>
        /// SHs the a1.
        /// </summary>
        /// <returns>The a1.</returns>
        /// <param name="text">Text.</param>
        public static string SHA1(string text){
            byte[] cleanBytes  = Encoding.Default.GetBytes(text);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }

        public static byte[] SHAToBytes(string text){
            byte[] cleanBytes = Encoding.Default.GetBytes(text);
            return System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
        }

    #endregion

    #region Creates the name of the directory.

        /// <summary>
        /// Creates the name of the directory.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public static void CreateDirectoryName(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            try{
                string dir = Path.GetDirectoryName(PathFormat(filePath, directoryType));
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e){
                //to do
                Debug.LogError("IO error." + e.Message);
            }
        }

    #endregion

    #region Gets the Directories.

        /// <summary>
        /// Gets the Directories.
        /// </summary>
        /// <returns>The directories.</returns>
        /// <param name="filePath">File path.</param>
        public static string[] GetDirectories(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            return Directory.GetDirectories(PathFormat(filePath, directoryType));
        }

    #endregion

    #region Gets the Files.

        /// <summary>
        /// Gets the Files.
        /// </summary>
        /// <returns>The Files.</returns>
        /// <param name="filePath">File path.</param>
        public static string[] GetFiles(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            return Directory.GetFiles(PathFormat(filePath, directoryType));
        }

        public static string[] GetFiles(string filePath, string suffix, bool recursion = false){
            // return Directory.GetFiles(filePath,suffix,SearchOption.AllDirectories).Where(s=>s.StartsWith(suffix));
            return Directory.GetFiles(filePath, suffix, recursion ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// 多重suffix
        /// 不判空，有问题直接抛错
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="suffix"></param>
        /// <param name="recursion"></param>
        /// <returns></returns>
        [Obsolete("存在逻辑问题,等待修复后重启")]
        public static string[] GetFiles(string filePath, string[] suffix, bool recursion = false){
            // StringBuilder suffixs = new StringBuilder();
            // suffix.Append("(");
            // int length = suffix.Length;
            // for (int i = 0; i < length; ++i){
            //     suffixs.Append(suffix[i]);
            //     if (i < length - 1){
            //         suffixs.Append("|");
            //     }
            // }
            //
            // suffix.Append(")");
            // string transSuffix = suffixs.ToString();
            // suffixs = null;
            // return Directory.GetFiles(filePath, transSuffix, recursion ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return null;
        }

        public static void FilterSymbol(ref string[] target, char format, char errorFormat){
            int length = target.Length;
            for (int i = 0; i < length; i++){
                if (target[i].Contains(errorFormat)){
                    int index = target[i].IndexOf(errorFormat);
                    target[i].Replace(errorFormat, format);
                }
            }
        }

        public static void FixSymbol(ref string target){
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            char format = '\\', errorFormat = '/';
#else
            char format = '/', errorFormat = '\\';
#endif
            while (target.Contains(errorFormat)){
                int index = target.IndexOf(errorFormat);
                var arr   = target.ToCharArray();
                arr[index] = format;
                // target = arr.ArrayToString ();
                target = arr.ToString();
                // target.Replace(errorFormat, format);
                // var bottom=target.Remove(index);
                // bottom.Remove(0);
                // target.Insert(index,format+bottom);
            }
        }

    #endregion

    #region Gets the files in directories.

        /// <summary>
        /// Gets the files in directories.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="files">Files.</param>
        /// <param name="fileType">File type.</param>
        public static void GetFilesInDirectories(
            string           path,
            ref List<string> files,
            string           fileType,
            bool             isEndsWith,
            DirectoryType    directoryType = DirectoryType.UNKNOWN){
            string[] filesT = GetFiles(path, directoryType);
            foreach (string item in filesT){
                if (item.EndsWith(fileType) == isEndsWith && !item.EndsWith(".DS_Store")){
                    files.Add(item);
                }
            }

            string[] pathsD = GetDirectories(path, directoryType);
            if (pathsD == null || pathsD.Length <= 0) return;

            //遍历所有的游戏对象
            foreach (string pathD in pathsD){
                GetFilesInDirectories(pathD, ref files, fileType, isEndsWith, directoryType);
            }
        }

    #endregion

    #region Gets the file info.

        /// <summary>
        /// Gets the File info.
        /// </summary>
        /// <returns>The File info.</returns>
        /// <param name="filePath">File path.</param>
        public static FileInfo GetFileInfo(string filePath, DirectoryType directoryType = DirectoryType.UNKNOWN){
            return new FileInfo(PathFormat(filePath, directoryType));
        }

    #endregion

    #region Deletes directory.

        /// <summary>
        /// Deletes directory.
        /// </summary>
        /// <param name="path">Path.</param>
        public static void DeleteDirectory(string path, DirectoryType directoryType = DirectoryType.UNKNOWN){
            if (DirectoryExists(path, directoryType)){
                string[] files = Directory.GetFiles(PathFormat(path, directoryType));
                foreach (string file in files){
                    DeleteFile(file);
                }

                string[] directorys = GetDirectories(path, directoryType);
                foreach (string directory in directorys){
                    DeleteDirectory(directory, directoryType);
                }

                Directory.Delete(PathFormat(path, directoryType));
            }
        }

    #endregion

    #region Delete the file.

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="path">Path.</param>
        public static void DeleteFile(string path, DirectoryType directoryType = DirectoryType.UNKNOWN){
            if (Exists(PathFormat(path, directoryType))){
                File.Delete(PathFormat(path, directoryType));
            }
        }

    #endregion

    #region Copy

        /// <summary>
        /// Copy the specified sourceFileName and destFileName.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destFileName">Destination file name.</param>
        public static void CopyFile(string sourceFileName, string destFileName){
            try{
                string dir = Path.GetDirectoryName(destFileName);
                if (!Directory.Exists(dir)){
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception e){
                //to do
                Debug.Log("IO error." + e.Message);
            }

            File.Copy(sourceFileName, destFileName);
        }

    #endregion

    #region 加密或解密

        /// <summary>
        /// 加密.
        /// </summary>
        /// <returns>The to bytes.</returns>
        /// <param name="data">Data.</param>
        /// <param name="password">Password.</param>
        public static byte[] EncryptionToBytes(byte[] data, string password){ return Encryption(data, data.Length, password); }

        /// <summary>
        /// Encryptions to bytes.
        /// </summary>
        /// <returns>The to bytes.</returns>
        /// <param name="data">Data.</param>
        /// <param name="len">Length.</param>
        /// <param name="password">Password.</param>
        public static byte[] EncryptionToBytes(byte[] data, int len, string password){ return Encryption(data, len, password); }

        /// <summary>
        /// 加密.
        /// </summary>
        /// <returns>The to bytes.</returns>
        /// <param name="str">String.</param>
        /// <param name="password">Password.</param>
        public static byte[] EncryptionToBytes(string str, string password){
            byte[] data = Encoding.UTF8.GetBytes(str);
            return EncryptionToBytes(data, password);
        }

        /// <summary>
        /// 加密.
        /// </summary>
        /// <returns>The to string.</returns>
        /// <param name="data">Data.</param>
        /// <param name="password">Password.</param>
        public static string EncryptionToString(byte[] data, string password){ return Encoding.UTF8.GetString(EncryptionToBytes(data, password)); }

        /// <summary>
        /// 加密.
        /// </summary>
        /// <returns>The to string.</returns>
        /// <param name="str">String.</param>
        /// <param name="password">Password.</param>
        public static string EncryptionToString(string str, string password){ return Encoding.UTF8.GetString(EncryptionToBytes(str, password)); }

        /// <summary>
        /// 解密.
        /// </summary>
        /// <returns>The to bytes.</returns>
        /// <param name="data">Data.</param>
        /// <param name="password">Password.</param>
        public static byte[] DecryptionToBytes(byte[] data, string password = "ZF"){ return Decryption(data, data.Length, password); }

        /// <summary>
        /// Decryptions to bytes.
        /// </summary>
        /// <returns>The to bytes.</returns>
        /// <param name="data">Data.</param>
        /// <param name="len">Length.</param>
        /// <param name="password">Password.</param>
        public static byte[] DecryptionToBytes(byte[] data, int len, string password = "ZF"){ return Decryption(data, len, password); }

        /// <summary>
        /// 解密.
        /// </summary>
        /// <returns>The to string.</returns>
        /// <param name="data">Data.</param>
        /// <param name="password">Password.</param>
        public static string DecryptionToString(byte[] data, string password = "ZF"){
            data = DecryptionToBytes(data, password);
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// 解密.
        /// </summary>
        /// <returns>The to bytes.</returns>
        /// <param name="data">Data.</param>
        /// <param name="password">Password.</param>
        public static byte[] DecryptionToBytes(string data, string password = "ZF"){
            byte[] datas = Encoding.UTF8.GetBytes(data);
            return DecryptionToBytes(datas, password);
        }

        /// <summary>
        /// 解密.
        /// </summary>
        /// <returns>The to string.</returns>
        /// <param name="data">Data.</param>
        /// <param name="password">Password.</param>
        public static string DecryptionToString(string data, string password = "ZF"){
            return Encoding.UTF8.GetString(DecryptionToBytes(data, password));
        }

    #endregion

    #region 加密或解密

        /// <summary>
        /// Encryption the specified data, len and password.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="len">Length.</param>
        /// <param name="password">Password.</param>
        private static byte[] Encryption(byte[] data, int len, string password = "ZF"){
            byte[] keys    = SHAToBytes(password);
            byte[] dataNew = new byte[data.Length + 1];
            dataNew[0] = 2;
            for (int i = 0; i < data.Length; i++){
                dataNew[i + 1] = data[i];
                if (i < len){
                    dataNew[i + 1] ^= keys[i & 15];
                }
            }

            return dataNew;
        }

        /// <summary>
        /// Decryption the specified data, len and password.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="len">Length.</param>
        /// <param name="password">Password.</param>
        private static byte[] Decryption(byte[] data, int len, string password = "ZF"){
            byte[] keys    = SHAToBytes(password);
            byte[] dataNew = new byte[data.Length - 1];
            for (int i = 1; i < data.Length; i++){
                dataNew[i - 1] = data[i];
                if (i < len){
                    dataNew[i - 1] ^= keys[i - 1 & 15];
                }
            }

            return dataNew;
        }

    #endregion

    #region 解析CSV.

        /// <summary>
        /// 解析CSV.
        /// </summary>
        /// <returns>数据列表.</returns>
        /// <param name="data">数据源.</param>
        /// <typeparam name="T">对象类型.</typeparam>
        public static List<T> CsvDecod<T>(string data) where T : class, new(){
            byte[] bytes = Encoding.Default.GetBytes(data);
            return CsvDecod<T>(bytes);
        }

        /// <summary>
        /// 解析CSV.
        /// </summary>
        /// <returns>数据列表.</returns>
        /// <param name="bytes">数据源.</param>
        /// <typeparam name="T">对象类型.</typeparam>
        public static List<T> CsvDecod<T>(byte[] bytes) where T : class, new(){
            // List<T> list = new List<T>();
            // using(MemoryStream stream = new MemoryStream(bytes)) {
            //     using(StreamReader reader = new StreamReader(stream)) {
            //         CsvContext mCsvContext = new CsvContext();
            //         list.AddRange(mCsvContext.Read<T>(reader));
            //     }
            // }
            // return list;
            return null;
        }

    #endregion

        public enum DirectoryType{
            /// <summary>
            /// The unknown.
            /// </summary>
            UNKNOWN,

            /// <summary>
            /// The assets.
            /// </summary>
            ASSETS,

            /// <summary>
            /// The resources.
            /// </summary>
            RESOURCES,

            /// <summary>
            /// The streaming.
            /// </summary>
            STREAMING,
        }
    }
}