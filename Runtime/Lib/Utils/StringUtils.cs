using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = System.Object;

namespace IG.Runtime.Utils{
    using Extensions;

    public static class StringUtils{
    #region Const

        /// <summary>
        /// string 时间格式化-full时间格式
        /// </summary>
        public const string DATE_TIME_FORMAT_FULL = "yyyy-M-d HH:mm:ss";

        /// <summary>
        /// string 空格
        /// </summary>
        public const string EMPTY_SPACE_STR = " ";

        /// <summary>
        /// char 空格
        /// </summary>
        public const char EMPTY_SPACE = ' ';

        /// <summary>
        /// char { 
        /// </summary>
        public const char HASHMAP_BEGIN_SIGN = '{';

        /// <summary>
        /// char }
        /// </summary>
        public const char HASHMAP_END_SIGN = '}';

        /// <summary>
        /// string {
        /// </summary>
        public const string HASHMAP_BEGIN_SIGN_STR = "{";

        /// <summary>
        /// string }
        /// </summary>
        public const string HASHMAP_END_SIGN_STR = "}";

        /// <summary>
        /// char (
        /// </summary>
        public const char VECTOR_BEGIN_SIGN = '(';

        /// <summary>
        /// char )
        /// </summary>
        public const char VECTOR_END_SIGN = ')';

        /// <summary>
        /// string (
        /// </summary>
        public const string VECTOR_BEGIN_SIGN_STR = "(";

        /// <summary>
        /// string )
        /// </summary>
        public const string VECTOR_END_SIGN_STR = ")";

        /// <summary>
        /// char [
        /// </summary>
        public const char BRACKET_BEGIN_SIGN = '[';

        /// <summary>
        /// char ]
        /// </summary>
        public const char BRACKET_END_SIGN = ']';

        /// <summary>
        /// char |
        /// </summary>
        public const char VERTICAL_BAR_ARRAY_SIGN = '|';

        /// <summary>
        /// string |
        /// </summary>
        public const string VERTICAL_BAR_ARRAY_SIGN_STR = "|";

        /// <summary>
        /// char ,
        /// </summary>
        public const char ARRAY_NORMAL_SIGN = ',';

        /// <summary>
        /// string ,
        /// </summary>
        public const string ARRAY_NORMAL_SIGN_STR = ",";

        /// <summary>
        /// char .
        /// </summary>
        public const char PERIOD_SIGN = '.';

        /// <summary>
        /// char /
        /// </summary>
        public const char URL_FORWARD_SIGN = '/';

        /// <summary>
        /// char \
        /// </summary>
        public const char URL_REVERSE_SIGN = '\\';

        /// <summary>
        /// string /
        /// </summary>
        public const string URL_FORWARD_SIGN_STR = "/";

        /// <summary>
        /// string \
        /// </summary>
        public const string URL_REVERSE_SIGN_STR = "\\";

    #endregion

    #region Buffer

        private static List<StringBuilder> _builderCache = ListPool<StringBuilder>.GetList();

        public static StringBuilder GetBuffer(){
            StringBuilder rel = null;
            if (_builderCache.Count > 0){
                rel = _builderCache[_builderCache.Count - 1];
                _builderCache.Remove(rel);
            }
            else{
                rel = new StringBuilder();
            }

            return rel;
        }

        public static void RecycleBuffer(this StringBuilder builder){
            builder.Clear();
            _builderCache.Add(builder);
        }

    #endregion

    #region Utils

        private static readonly char[] s_letter ={ 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        public static char NumberToLetter(int number){
            int charIndex = Math.Clamp(number, 0, s_letter.Length - 1);
            return s_letter[charIndex];
        }

        /// <summary>
        /// 字符串做MD5后输出
        /// </summary>
        public static string String2MD5(string str){
            string md5   = string.Empty;
            byte[] bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
            for (int i = 0; i < bytes.Length; ++i){
                md5 += bytes[i].ToString("x").PadLeft(2, '0');
            }

            return md5;
        }

    #endregion

    #region String Extension

        /// <summary>
        /// 非正则法匹配字符串内关键字数量 
        /// </summary>
        public static int CalcKeyInStr(this string str, string key){
            int count      = 0;
            int startIndex = 0;
            int strLen     = str?.Length ?? 0;
            while (startIndex < strLen){
                int y = str.IndexOf(key, startIndex, StringComparison.Ordinal);
                if (y < 0){
                    break;
                }

                count++;
                startIndex = y + 1;
            }

            return count;
        }

    #endregion

        /// <summary>
        /// 从字符串中提取数字
        /// 只提取首位
        /// </summary>
        public static int GetNumber(string str){
            try{
                //正则提取所有数字
                MatchCollection result = Regex.Matches(str, @"[\-\d\.]+");
                return StringUtils.ToInt(result[0].Value);
            }
            catch (Exception e){
                Debug.LogError($"Error :{e} , 错误数值 :{str} ");
            }

            return -1;
        }

        //public static fromat="";
        public static string[] SplitBySign(char sign, string target){
            if (string.IsNullOrEmpty(target)) return null;
            string[] result = target.Split(sign);
            return result;
        }

        public static string[] SplitBySign(string sign, string target){
            if (!string.IsNullOrEmpty(target) && target.Contains(sign)){
                string[] result = Regex.Split(target, sign, RegexOptions.IgnoreCase);
                return result;
            }

            return new string[]{ target };
        }

        public static string ReplaceFirstLetter(string content){
            char[] translate = content.ToCharArray();
            char[] upperArr  = content.ToUpper().ToCharArray();
            translate[0] = upperArr[0];
            return translate.ToString();
        }

        /// <summary>
        /// Sample apply,if need more  
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string CutSuffix(string param){
            //string[] result = param.Split('.').Length <= 1 ? param.Split('(') : param.Split('.');
            string result = CutSuffix(param, PERIOD_SIGN);
            if (result.Equals(param)){
                return param;
            }

            return result;
        }

        public static string CutSuffix(string param, char format, int getIndex = 0){
            string[] result = param.Split(format);
            if (result.Length <= 1){
#if UNITY_EDITOR
                UnityEngine.Debug.LogFormat("Not need to cut suffix");
#endif
                return param;
            }

            return result[getIndex];
        }

        public static string GetSuffix(string param, char format){
            string[] result = param.Split(format);
            if (result.Length <= 1){
#if UNITY_EDITOR
                UnityEngine.Debug.LogFormat("Not need suffix ,this string:" + param);
#endif
                return param;
            }

            return result[result.Length - 1];
        }

        public static UnityEngine.Vector2 GetVector2(string _param, char _sign){
            float[] arr = StringUtils.ToFloatArray(_param, _sign);
            if (arr == null || arr.Length > 2 || arr.Length <= 1) return UnityEngine.Vector2.zero;
            return new UnityEngine.Vector2(arr[0], arr[1]);
        }

        public static UnityEngine.Vector2 GetVector3(string _param, char _sign){
            float[] arr = StringUtils.ToFloatArray(_param, _sign);
            if (arr == null || arr.Length > 3 || arr.Length <= 1) return UnityEngine.Vector3.zero;
            return new UnityEngine.Vector3(arr[0], arr[1], arr[2]);
        }

        public static List<string> AssociateSearch(string keyWord, List<string> library){
            if (string.IsNullOrEmpty(keyWord)) return library;
            List<string> result = new List<string>();
            foreach (var item in library){
                if (item.Contains(keyWord)){
                    result.Add(item);
                }
            }

            return result;
        }

        public static List<string> MultipleAssociateSearch(string keyWord, List<string> library_1, List<string> library_2){
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(keyWord)){
                result.AddRange(library_1);
                result.AddRange(library_2);
                return result;
            }

            foreach (var item in library_1){
                if (item.Contains(keyWord)){
                    result.Add(item);
                }
            }

            foreach (var item in library_2){
                if (item.Contains(keyWord)){
                    result.Add(item);
                }
            }

            return result;
        }

        public static string PathRemoveDir(string _path){
            if (!IsDirPath(_path)) return string.Empty;
            string   result = string.Empty;
            string[] cut    = SplitBySign(URL_FORWARD_SIGN, _path);
            result = cut[^1];
            if (IsDirPath(result)){
                cut    = SplitBySign(URL_REVERSE_SIGN, _path);
                result = cut[^1];
            }

            return result;
        }

        public static bool IsDirPath(string _path){
            if (_path.Contains(URL_FORWARD_SIGN_STR) || _path.Contains(URL_REVERSE_SIGN_STR)) return true;
            return false;
        }

        public static string FirstLetterToUpper(string str){
            if (string.IsNullOrEmpty(str)) return null;
            if (str.Length > 1) return char.ToUpper(str[0]) + str.Substring(1);
            return str.ToUpper();
        }

        public static string FirstLetterToLow(string str){
            if (string.IsNullOrEmpty(str)) return null;
            if (str.Length > 1) return char.ToLower(str[0]) + str.Substring(1);
            return str.ToLower();
        }

        /// <summary>
        /// 字符运算
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        // public static object Operation (string param) {
        //     DataTable dr = new DataTable ();
        //     object result = dr.Compute (param, "");
        //     return result;
        // }
        public static char ToChar(string param){
            if (IsEmpty(param)){
                return '0';
            }

            char result;
            if (!char.TryParse(param, out result)){
                return '0';
            }

            return result;
        }

        public static bool ToBool(string param){
            if (IsEmpty(param)){
                UnityEngine.Debug.LogErrorFormat(string.Format("{0}:{1}", "Change bool param error ", param));
                return false;
            }

            if (ToInt(param) == 0 || ToFloat(param) == 0 || ToDouble(param) == 0){
                return false;
            }

            if (param.Equals("true") || param.Equals("True") || param.Equals("TRUE")){
                return true;
            }

            return false;
        }

        public static string ToUTF8(string unicodeString){
            UTF8Encoding utf8           = new UTF8Encoding();
            Byte[]       endcodingBytes = utf8.GetBytes(unicodeString);
            string       decodedStr     = utf8.GetString(endcodingBytes);
            return decodedStr;
        }

        public static uint ToUint(string param){
            if (IsEmpty(param)){
                return 0;
            }

            uint result;
            if (!uint.TryParse(param, out result)){
                return 0;
            }

            return result;
            //char[] array = param.ToCharArray();
            //int arr_length = array.Length;
        }

        public static int ToInt(string param){
            if (IsEmpty(param)){
                return 0;
            }

            int result;
            if (!int.TryParse(param, out result)){
                return 0;
            }

            return result;
            //char[] array = param.ToCharArray();
            //int arr_length = array.Length;
        }

        public static int ToInt(char param){
            string pra = param.ToString();
            int    result;
            if (!int.TryParse(pra, out result)){
                return 0;
            }

            return result;
            //char[] array = param.ToCharArray();
            //int arr_length = array.Length;
        }

        public static float ToFloat(object param){
            if (param == null){
                return 0;
            }

            string string_param = ToString(param);
            return ToFloat(string_param);
        }

        /// <summary>
        /// 保罗小数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="reserved"></param>
        /// <returns></returns>
        public static float ToFloat(object param, int reserved){
            if (param == null){
                return 0;
            }

            float  result        = ToFloat(param);
            string reservedCount = string.Format("{0}{1}", "F", reserved);
            string show          = result.ToString(reservedCount);
            return ToFloat(show);
        }

        public static float ToFloat(string param){
            if (IsEmpty(param)){
                return 0;
            }

            float result;
            if (!float.TryParse(param, out result)){
                return 0;
            }

            return result;
        }

        public static double ToDouble(string param){
            if (param == null){
                return 0;
            }

            double result;
            if (!double.TryParse(param, out result)){
                return 0;
            }

            return result;
        }

        /// <summary>
        /// 已知数组长度位置
        /// 无GC
        /// </summary>
        public static float[] ToFloatArray(string param, char format, string[] strArr, float[] arr){
            if (IsEmpty(param) || strArr.Length != arr.Length){
                return null;
            }

            strArr = param.Split(format, strArr);
            for (int i = 0; i < arr.Length; i++){
                arr[i] = ToFloat(strArr[i]);
            }

            return arr;
        }

        /// <summary>
        /// 未知数组长度
        /// </summary>
        public static float[] ToFloatArray(string param, char format){
            if (IsEmpty(param)){
                return null;
            }

            float[]  result;
            string[] split_result = param.Split(format);
            int      length       = split_result.Length;
            if (length <= 0){
                return null;
            }

            result = new float[length];
            for (int i = 0; i < length; i++){
                result[i] = ToFloat(split_result[i]);
            }

            return result;
        }

        public static SortedList<TK, TV> ToSortedList<TK, TV>(string param, char formatPair, char formatRegion){
            if (IsEmpty(param)){
                return null;
            }

            SortedList<TK, TV> rel     = new SortedList<TK, TV>();
            String[]           allPair = StringUtils.ToStringArray(param, formatRegion);
            if (allPair == null || allPair.Length <= 0){
                return rel;
            }

            String[] tempSingle = null;
            for (int i = 0; i < allPair.Length; ++i){
                tempSingle = StringUtils.ToStringArray(allPair[i], formatPair);
                TK key   = StringUtils.ToObject<TK>(tempSingle[0]);
                TV value = StringUtils.ToObject<TV>(tempSingle[1]);
                rel.Add(key, value);
            }

            return rel;
        }

        /** 裁剪字符串(二维数据) **/
        public static Dictionary<K, T> SplitToMap<K, T>(string str, char regexA, char regexB, ISplitMap<K, T> split){
            if (string.IsNullOrEmpty(str)){
                return new Dictionary<K, T>();
            }

            try{
                String[] strs  = StringUtils.ToStringArray(str, regexA);
                int      ssize = (strs != null) ? strs.Length : 0;
                // 遍历处理
                Dictionary<K, T> map = new Dictionary<K, T>(ssize);
                for (int i = 0; i < ssize; i++){
                    // String[] strs0 = StringUtils.ToStringArray(strs[i], regexB, 2);
                    String[] strs0  = StringUtils.ToStringArray(strs[i], regexB);
                    int      ssize0 = (strs0 != null) ? strs0.Length : 0;
                    if (ssize0 < 2){
                        continue; // 错误
                    }

                    // 解析参数
                    K key   = split.SplitKey(strs0[0]);
                    T value = split.Split(strs0[1]);
                    map.Add(key, value);
                }

                return map;
            }
            catch (Exception e){
                throw e;
                // Console.Write("拆分字符串异常,data:" + str + ",regexA:" + regexA + ",regexB:" + regexB);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="formatPair">母键值对分隔符</param>
        /// <param name="formatRegion">element分隔符</param>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <typeparam name="TSK"></typeparam>
        /// <typeparam name="TSV"></typeparam>
        /// <returns></returns>
        // public static Dictionary<TK, TV> ToDictionary<TK, TV, TSK, TSV>(string param, char formatPair, char formatRegion)
        //     where TV : IDictionary<TSK, TSV>{
        //     if (IsEmpty(param) || param.Length <= 2){
        //         return null;
        //     }
        //
        //     Dictionary<TK, TV> rel = new Dictionary<TK, TV>();
        //     //移除map通配符
        //     param.Remove(0);
        //     param.Remove(param.Length - 1);
        //     String[] allPair = StringUtils.ToStringArray(param, formatRegion);
        //     if (allPair == null || allPair.Length <= 0){
        //         return rel;
        //     }
        //
        //     String[] tempSingle = null;
        //     for (int i = 0; i < allPair.Length; ++i){
        //         tempSingle = StringUtils.ToStringArray(allPair[i], formatPair);
        //         TK key = StringUtils.ToObject<TK>(tempSingle[0]);
        //         TV value = (TV)StringUtils.ToDictionary<TSK, TSV>(tempSingle[1], formatPair, formatRegion);
        //         rel.Add(key, value);
        //     }
        //
        //     return rel;
        // }
        public static Dictionary<TK, TV> ToDictionary<TK, TV>(string param, char formatPair, char formatRegion){
            if (IsEmpty(param)){
                return null;
            }

            Dictionary<TK, TV> rel     = new Dictionary<TK, TV>();
            String[]           allPair = StringUtils.ToStringArray(param, formatRegion);
            if (allPair == null || allPair.Length <= 0){
                return rel;
            }

            String[] tempSingle = null;
            for (int i = 0; i < allPair.Length; ++i){
                tempSingle = StringUtils.ToStringArray(allPair[i], formatPair);
                if (tempSingle.Length < 2){
                    continue; // 错误
                }

                TK key   = StringUtils.ToObject<TK>(SubEmptyOrSpecial(tempSingle[0]));
                TV value = StringUtils.ToObject<TV>(SubEmptyOrSpecial(tempSingle[1]));
                rel.Add(key, value);
            }

            return rel;
        }

        public static Dictionary<int, int> ToDictionary(string param, char formatPair, char formatRegion, bool allowSameKey = false){
            if (IsEmpty(param)){
                return null;
            }

            Dictionary<int, int> rel     = new Dictionary<int, int>();
            String[]             allPair = StringUtils.ToStringArray(param, formatRegion);
            if (allPair == null || allPair.Length <= 0){
                return rel;
            }

            String[] tempSingle = null;
            for (int i = 0; i < allPair.Length; ++i){
                tempSingle = StringUtils.ToStringArray(allPair[i], formatPair);
                if (tempSingle.Length < 2){
                    continue; // 错误
                }

                int key   = StringUtils.ToObject<int>(SubEmptyOrSpecial(tempSingle[0]));
                int value = StringUtils.ToObject<int>(SubEmptyOrSpecial(tempSingle[1]));
                if (rel.ContainsKey(key) && allowSameKey){
                    rel[key] += value;
                }
                else{
                    rel.Readd(key, value);
                }
            }

            return rel;
        }

        /// <summary>
        /// 已知数组长度
        /// 无GC
        /// </summary>
        public static int[] ToIntArray(string param, char format, string[] strArr, int[] arr){
            if (IsEmpty(param) || strArr.Length != arr.Length){
                return null;
            }

            strArr = param.Split(format, strArr);
            for (int i = 0; i < arr.Length; i++){
                arr[i] = ToInt(strArr[i]);
            }

            return arr;
        }

        /// <summary>
        /// 未知数组长度 
        /// </summary>
        public static int[] ToIntArray(string param, char format = VERTICAL_BAR_ARRAY_SIGN){
            if (IsEmpty(param)){
                return null;
            }

            int[]    result;
            string[] split_result = param.Split(format);
            int      length       = split_result.Length;
            if (length <= 0){
                return null;
            }

            result = new int[length];
            for (int i = 0; i < length; i++){
                result[i] = ToInt(split_result[i]);
            }

            return result;
        }

        /// <summary>
        /// 未知数组长度 
        /// </summary>
        public static List<int> ToIntArray(string param, List<string> strBuffer, List<int> intBuffer, char format = VERTICAL_BAR_ARRAY_SIGN){
            if (IsEmpty(param)){
                return null;
            }

            strBuffer.Clear();
            intBuffer.Clear();
            strBuffer = param.Split(format, strBuffer);
            int length = strBuffer.Count;
            if (length <= 0){
                return null;
            }

            for (int i = 0; i < length; i++){
                intBuffer.Add(ToInt(strBuffer[i]));
            }

            return intBuffer;
        }

        /// <summary>
        /// 已知list容量
        /// 无GC
        /// </summary>
        /// <param name="param"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static List<int> ToIntList(string param, string[] strArr, List<int> arr, char format = VERTICAL_BAR_ARRAY_SIGN){
            if (IsEmpty(param)){
                return null;
            }

            strArr = param.Split(format, strArr);
            for (int i = 0; i < strArr.Length; i++){
                arr[i] = ToInt(strArr[i]);
                // arr.Add(ToInt(split_result[i]));
            }

            return arr;
        }

        /// <summary>
        /// 未知list容量 
        /// </summary>
        public static List<int> ToIntList(string param, char format = VERTICAL_BAR_ARRAY_SIGN){
            if (IsEmpty(param)){
                return null;
            }

            List<int> result;
            string[]  split_result = param.Split(format);
            int       length       = split_result.Length;
            if (length <= 0){
                return null;
            }

            result = new List<int>(length);
            for (int i = 0; i < length; i++){
                // result[i] = ToInt(split_result[i]);
                result.Add(ToInt(split_result[i]));
            }

            return result;
        }

        /// <summary>
        /// 已知数组长度
        /// 无GC
        /// </summary>
        /// <returns></returns>
        public static string[] ToStringArray(string param, char format, string[] arr){
            if (IsEmpty(param)){
                return null;
            }

            arr = param.Split(format, arr);
            return arr;
        }

        /// <summary>
        /// 未知数组长度
        /// </summary>
        public static string[] ToStringArray(string param, char format){
            if (IsEmpty(param)){
                return null;
            }

            string[] split_result = param.Split(format);
            return split_result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="format"></param>
        /// <param name="length">返回结果长度</param>
        /// <returns></returns>
        public static string[] ToStringArray(string param, char format, int length){
            if (param.IndexOf(format) < 0){
                return null;
            }

            string[]   rel       = new string[length];
            int        charCount = param.Length;
            int        setIndex  = 0;
            List<char> charList  = ListPool<char>.GetList();
            for (int i = 0; i < charCount; ++i){
                if (!param[i].Equals(format)){
                    charList.Add(param[i]);
                }
                else{
                    rel[setIndex] = StringUtils.ToString(charList);
                    setIndex++;
                    charList.Clear();
                }

                if (setIndex == length - 1){
                    rel[setIndex] = param.Substring(i + 1, charCount - i - 1);
                    break;
                }
            }

            charList.Recycle();
            return rel;
        }

        //public static T[] ToObjectArray<T>(string param, char format)
        //{
        //    if (IsEmpty(param))
        //    {
        //        return default(T[]);
        //    }

        //    string[] split_result = param.Split(format);
        //    int length = split_result.Length;
        //    if (length <= 0)
        //    {
        //        return default(T[]);
        //    }

        //    Stack<T> tempResult = new Stack<T>();
        //    for (int i = 0; i < length; i++)
        //    {
        //        //未测试。不清楚正确否
        //        tempResult.Push(split_result[i]);
        //    }

        //    T[] result = tempResult.ToArray();
        //    tempResult.Clear();
        //    return result;
        //}

        /// <summary>
        /// Split 返回值是new string[]，如果频繁需要则使用这个方法
        /// texStr length = 2,并且初始化好放入
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="param"></param>
        /// <param name="texStr"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2 ToVector2(UnityEngine.Vector2 rel, string param, string[] texStr){
            if (string.IsNullOrEmpty(param) || !param.Contains(ARRAY_NORMAL_SIGN_STR)){
                return rel;
            }

            if (param.Contains(VECTOR_BEGIN_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_BEGIN_SIGN_STR));
            }

            if (param.Contains(VECTOR_END_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_END_SIGN_STR));
            }

            texStr = param.Split(ARRAY_NORMAL_SIGN, texStr);
            if (texStr == null || texStr.Length <= 0){
                return rel;
            }

            rel.x = StringUtils.ToFloat(texStr[0]);
            rel.y = StringUtils.ToFloat(texStr[1]);
            return rel;
        }

        /// <summary>
        /// 频繁使用切换使用楼上方法
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2 ToVector2(string param){
            if (string.IsNullOrEmpty(param) || !param.Contains(ARRAY_NORMAL_SIGN_STR)){
                return UnityEngine.Vector2.zero;
            }

            if (param.Contains(VECTOR_BEGIN_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_BEGIN_SIGN_STR));
            }

            if (param.Contains(VECTOR_END_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_END_SIGN_STR));
            }

            string[] tex = param.Split(ARRAY_NORMAL_SIGN);
            if (tex == null || tex.Length <= 0){
                return UnityEngine.Vector2.zero;
            }

            return new UnityEngine.Vector2(StringUtils.ToFloat(tex[0]), StringUtils.ToFloat(tex[1]));
        }

        /// <summary>
        /// Split 返回值是new string[]，如果频繁需要则使用这个方法
        /// texStr length = 3,并且初始化好放入
        /// </summary>
        /// <param name="rel"></param>
        /// <param name="param"></param>
        /// <param name="texStr"></param>
        /// <returns></returns>
        public static UnityEngine.Vector3 ToVector3(UnityEngine.Vector3 rel, string param, string[] texStr){
            if (string.IsNullOrEmpty(param) || !param.Contains(ARRAY_NORMAL_SIGN_STR)){
                return rel;
            }

            if (param.Contains(VECTOR_BEGIN_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_BEGIN_SIGN_STR));
            }

            if (param.Contains(VECTOR_END_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_END_SIGN_STR));
            }

            texStr = param.Split(ARRAY_NORMAL_SIGN, texStr);
            if (texStr == null || texStr.Length <= 0){
                return rel;
            }

            rel.x = StringUtils.ToFloat(texStr[0]);
            rel.y = StringUtils.ToFloat(texStr[1]);
            rel.z = StringUtils.ToFloat(texStr[2]);
            return rel;
        }

        public static UnityEngine.Vector3 ToVector3(string param){
            if (string.IsNullOrEmpty(param) || !param.Contains(ARRAY_NORMAL_SIGN_STR)){
                return UnityEngine.Vector3.zero;
            }

            if (param.Contains(VECTOR_BEGIN_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_BEGIN_SIGN_STR));
            }

            if (param.Contains(VECTOR_END_SIGN_STR)){
                param.Remove(param.IndexOf(VECTOR_END_SIGN_STR));
            }

            string[] tex = param.Split(ARRAY_NORMAL_SIGN);
            if (tex == null || tex.Length <= 0){
                return UnityEngine.Vector3.zero;
            }

            return new UnityEngine.Vector3(
                                           StringUtils.ToFloat(tex[0]),
                                           StringUtils.ToFloat(tex[1]),
                                           StringUtils.ToFloat(tex[2])
                                          );
        }

        private static T ToObject<T>(string param){
            Type tarType = typeof(T);
            T    rel     = default;
            try{
                var convertObj = Convert.ChangeType(param, tarType);
                if (convertObj != null){
                    rel = (T)convertObj;
                }
            }
            catch (Exception e){
                Debug.LogError($"Convert param error:{e}");
                throw;
            }

            return rel;
        }

        private static Object ToObject(string param){
            int    int_result;
            float  float_result;
            double double_result;
            int_result = ToInt(param);
            if (int_result > 0){
                return int_result;
            }

            float_result = ToFloat(param);
            if (float_result > 0){
                return float_result;
            }

            double_result = ToDouble(param);
            if (double_result > 0){
                return double_result;
            }

            return param;
        }

        public static string ToString(object str){
            if (str == null){
                // return "";
                return null;
            }

            string result = Convert.ToString(str);
            if (IsEmpty(result)){
                // return "";
                return null;
            }

            return result;
        }

        public static string ToString<T>(IList<T> str){
            if (str == null){
                // return "";
                return null;
            }

            int length = str.Count;
            var buffer = GetBuffer();
            for (int i = 0; i < length; ++i){
                buffer.Append(str[i]);
            }

            string rel = buffer.ToString();
            buffer.RecycleBuffer();
            return rel;
        }

        public static string ToString<TK, TV, TSK, TSV>(IDictionary<TK, TV> param, char regexPair, char regexElement, char subPair, char subElement)
            where TV : IDictionary<TSK, TSV>{
            if (param == null){
                return "";
            }

            int           count     = param.Count;
            StringBuilder buffer    = GetBuffer();
            string        tempStr   = null;
            string        tempKey   = null;
            string        tempValue = null;
            foreach (var pair in param){
                count--;
                tempKey   = StringUtils.ToString(pair.Key);
                tempValue = StringUtils.ToString(pair.Value, subPair, subElement);
                tempStr   = $"{tempKey}{regexPair}{tempValue}";
                buffer.Append(tempStr);
                if (count > 0){
                    buffer.Append(regexElement);
                }
            }

            string rel = buffer.ToString();
            buffer.RecycleBuffer();
            return rel;
        }

        public static string ToString<TK, TV>(IDictionary<TK, TV> param, char regexPair, char regexElement){
            if (param == null){
                return string.Empty;
            }

            int           count  = param.Count;
            StringBuilder buffer = GetBuffer();
            // KeyValuePair<TK, TV> tempPair = default;
            string tempStr   = null;
            string tempKey   = null;
            string tempValue = null;
            buffer.Append(HASHMAP_BEGIN_SIGN);
            foreach (var pair in param){
                count--;
                tempKey   = StringUtils.ToString(pair.Key);
                tempValue = StringUtils.ToString(pair.Value);
                tempStr   = $"{tempKey}{regexPair}{tempValue}";
                buffer.Append(tempStr);
                if (count > 0){
                    buffer.Append(regexElement);
                }
            }

            buffer.Append(HASHMAP_END_SIGN);
            string rel = buffer.ToString();
            buffer.RecycleBuffer();
            return rel;
        }

        public static string ToString<T>(IEnumerable<T> param, char sign){
            if (param == null){
                // return "";
                return null;
            }

            var           iEnumerator = param.GetEnumerator();
            StringBuilder buffer      = GetBuffer();
            bool          isContinue  = true;
            while (isContinue){
                isContinue = iEnumerator.MoveNext();
                buffer.Append(iEnumerator.Current);
                if (isContinue){
                    buffer.Append(sign);
                }
            }

            if (buffer.Length > 0){
                buffer.Remove(buffer.Length - 1, 1);
            }

            iEnumerator.Dispose();
            string rel = buffer.ToString();
            buffer.RecycleBuffer();
            return rel;
        }

        public static string ToString<T>(IList<T> param, char sign){
            if (param == null){
                // return "";
                return null;
            }

            StringBuilder buffer = GetBuffer();
            int           length = param.Count;
            for (int i = 0; i < length; ++i){
                buffer.Append(param[i]);
                if (i != length - 1){
                    buffer.Append(sign);
                }
            }

            string rel = buffer.ToString();
            buffer.RecycleBuffer();
            return rel;
        }

        public static string ToString(object str, int reserved){
            if (str == null){
                return string.Empty;
            }

            string result = Convert.ToString(str);
            if (IsEmpty(result)){
                return string.Empty;
            }

            return result;
        }

        public static string ToString(float[] param, char format){
            if (param == null || param.Length <= 0){
                return string.Empty;
            }

            string result = string.Empty;
            int    length = param.Length;
            for (int i = 0; i < length; i++){
                result += param[i];
                if (i < length - 1){
                    result += format;
                }
            }

            if (IsEmpty(result)){
                return string.Empty;
            }

            return result;
        }

        public static string ToString(UnityEngine.Vector2 vec){
            StringBuilder buffer = GetBuffer();
            buffer.Append(vec.x);
            buffer.Append(ARRAY_NORMAL_SIGN_STR);
            buffer.Append(vec.y);
            string rel = buffer.ToString();
            buffer.RecycleBuffer();
            return rel;
        }

        public static bool IsEmpty(string str){
            if (string.IsNullOrEmpty(str)){
                return true;
            }

            return false;
        }

        public static string GetVariableName<T>(T param){
            if (param == null){
                return string.Empty;
            }

            // return nameof (param);
            return param.ToString();
        }

        /// <summary>
        /// 去除字符串特殊符号(包含空字符) 
        /// </summary>
        public static string SubEmptyOrSpecial(string str){
            // 去除特殊符号
            if (str.IndexOf(HASHMAP_BEGIN_SIGN) >= 0){
                int index = str.IndexOf(HASHMAP_BEGIN_SIGN);
                str = str.Substring(index + 1, str.Length - 1);
            }

            if (str.IndexOf(HASHMAP_END_SIGN) >= 0){
                int index = str.IndexOf(HASHMAP_END_SIGN);
                str = str.Substring(0, index);
            }

            // 去除空字符
            if (str.IndexOf(EMPTY_SPACE) >= 0){
                int index = str.IndexOf(EMPTY_SPACE);
                str = index < str.Length ? str.Substring(index + 1, str.Length - 1) : str.Substring(0, index);
            }

            return str;
        }

    #region 逻辑服分割技能使用，单独列出来写(相同key的skill，value会相加)

        /** 裁剪字符串(二维数据) **/
        public static Dictionary<int, int> SplitSkillsToMap(string str, char regexA, char regexB, ISplitMap<int, int> split){
            if (string.IsNullOrEmpty(str)){
                return new Dictionary<int, int>();
            }

            try{
                String[] strs  = StringUtils.ToStringArray(str, regexA);
                int      ssize = (strs != null) ? strs.Length : 0;
                // 遍历处理
                Dictionary<int, int> map = new Dictionary<int, int>(ssize);
                for (int i = 0; i < ssize; i++){
                    // String[] strs0 = StringUtils.ToStringArray(strs[i], regexB, 2);
                    String[] strs0  = StringUtils.ToStringArray(strs[i], regexB);
                    int      ssize0 = (strs0 != null) ? strs0.Length : 0;
                    if (ssize0 < 2){
                        continue; // 错误
                    }

                    // 解析参数
                    int key   = split.SplitKey(strs0[0]);
                    int value = split.Split(strs0[1]);
                    if (!map.ContainsKey(key)){
                        map.Add(key, value);
                    }
                    else{
                        map[key] += value;
                    }
                }

                return map;
            }
            catch (Exception e){
                throw e;
                // Console.Write("拆分字符串异常,data:" + str + ",regexA:" + regexA + ",regexB:" + regexB);
            }
        }

        public static Dictionary<int, Dictionary<int, int>> SplitSpecifySkillsToMap(
            string                               str,
            char                                 regexA,
            char                                 regexB,
            ISplitMap<int, Dictionary<int, int>> split){
            if (string.IsNullOrEmpty(str)){
                return new Dictionary<int, Dictionary<int, int>>();
            }

            try{
                String[] strs  = StringUtils.ToStringArray(str, regexA);
                int      ssize = (strs != null) ? strs.Length : 0;
                // 遍历处理
                Dictionary<int, Dictionary<int, int>> map = new Dictionary<int, Dictionary<int, int>>(ssize);
                for (int i = 0; i < ssize; i++){
                    // String[] strs0 = StringUtils.ToStringArray(strs[i], regexB, 2);
                    String[] strs0  = StringUtils.ToStringArray(strs[i], regexB);
                    int      ssize0 = (strs0 != null) ? strs0.Length : 0;
                    if (ssize0 < 2){
                        continue; // 错误
                    }

                    // 解析参数
                    int                  key   = (int)split.SplitKey(strs0[0]);
                    Dictionary<int, int> value = split.Split(strs0[1]);
                    map.Add(key, value);
                }

                return map;
            }
            catch (Exception e){
                throw e;
                // Console.Write("拆分字符串异常,data:" + str + ",regexA:" + regexA + ",regexB:" + regexB);
            }
        }

        /** Map切割模板(记得检测)(可以抽象成通用的) **/
        public class SplitSpecifySkillsMapTemplate : StringUtils.ISplitMap<int, int>{
            private static SplitSpecifySkillsMapTemplate _template;

            public static SplitSpecifySkillsMapTemplate GetTemplate(){
                if (null == _template){
                    _template = new SplitSpecifySkillsMapTemplate();
                }

                return _template;
            }

            public int Split(String    value){ return StringUtils.ToInt(SubEmptyOrSpecial(value)); }
            public int SplitKey(String key)  { return StringUtils.ToInt(SubEmptyOrSpecial(key)); }

            public string SubEmptyOrSpecial(string str){
                // 去除特殊符号
                if (str.IndexOf("{") >= 0){
                    int index = str.IndexOf("{");
                    str = str.Substring(index + 1, str.Length - 1);
                }

                if (str.IndexOf("}") >= 0){
                    int index = str.IndexOf("}");
                    str = str.Substring(0, index);
                }

                // 去除空字符
                if (str.IndexOf(" ") >= 0){
                    int index = str.IndexOf(" ");
                    str = index < str.Length ? str.Substring(index + 1, str.Length - 1) : str.Substring(0, index);
                }

                return str;
            }
        }

    #endregion

    #region Sub str

        public enum GetStrType{
            T2D, //从头到尾
            D2T, //从尾到头
            M2T, //从中间到头
            M2D, //从中间到尾
        }

        public static string GetCutStr(int length, GetStrType getStrType, string str){
            // int startIndex = 0;
            string rel        = String.Empty;
            int    beginIndex = 0;
            switch (getStrType){
                case GetStrType.T2D: break;
                case GetStrType.D2T:
                    beginIndex = str.Length - System.Math.Min(length, str.Length);
                    break;
                case GetStrType.M2D:
                    beginIndex = (int)(str.Length / 2);
                    break;
                case GetStrType.M2T:
                    int mid = (int)(str.Length / 2);
                    beginIndex = mid - System.Math.Min(length, mid);
                    break;
            }

            rel = GetCutStr(beginIndex, length, str);
            return rel;
        }

        public static string GetCutStr(int beginIndex, int length, string str){
            string rel = String.Empty;
            if (string.IsNullOrEmpty(str) || str.Length <= beginIndex){
                return rel;
            }

            StringBuilder builder   = GetBuffer();
            int           calLength = 0;
            for (int i = beginIndex; i < str.Length; ++i){
                if (calLength >= length){
                    break;
                }

                calLength++;
                builder.Append(str[i]);
            }

            rel = builder.ToString();
            RecycleBuffer(builder);
            return rel;
        }

    #endregion

    #region 摘抄Java 服务器方法/** 裁剪解析接口 **/

        public interface ISplit<T>{
            T Split(String str);
        }

        /** 裁剪解析接口 **/
        public interface ISplitMap<K, T> : ISplit<T>{
            K SplitKey(String str);
        }

    #endregion

    #region template 简单模板(当前时间不充足，充足后再来抽象方法)

        public class SplitMapStringValueTemplate : StringUtils.ISplitMap<int, string>{
            private static SplitMapStringValueTemplate _template;

            public static SplitMapStringValueTemplate GetTemplate(){
                if (null == _template){
                    _template = new SplitMapStringValueTemplate();
                }

                return _template;
            }

            public string Split(String    value){ return value; }
            public int    SplitKey(String key)  { return StringUtils.ToInt(SubEmptyOrSpecial(key)); }

            public string SubEmptyOrSpecial(string str){
                // 去除特殊符号
                if (str.IndexOf(HASHMAP_BEGIN_SIGN) >= 0){
                    int index = str.IndexOf(HASHMAP_BEGIN_SIGN);
                    str = str.Substring(index + 1, str.Length - 1);
                }

                if (str.IndexOf(HASHMAP_END_SIGN) >= 0){
                    int index = str.IndexOf(HASHMAP_END_SIGN);
                    str = str.Substring(0, index);
                }

                // 去除空字符
                if (str.IndexOf(EMPTY_SPACE) >= 0){
                    int index = str.IndexOf(EMPTY_SPACE);
                    str = index < str.Length ? str.Substring(index + 1, str.Length - 1) : str.Substring(0, index);
                }

                return str;
            }
        }

        /** Map切割模板(记得检测)(可以抽象成通用的) **/
        public class SplitMapTemplate : StringUtils.ISplitMap<int, int>{
            private static SplitMapTemplate _template;

            public static SplitMapTemplate GetTemplate(){
                if (null == _template){
                    _template = new SplitMapTemplate();
                }

                return _template;
            }

            public int Split(String    value){ return StringUtils.ToInt(SubEmptyOrSpecial(value)); }
            public int SplitKey(String key)  { return StringUtils.ToInt(SubEmptyOrSpecial(key)); }

            public string SubEmptyOrSpecial(string str){
                // 去除特殊符号
                if (str.IndexOf(HASHMAP_BEGIN_SIGN) >= 0){
                    int index = str.IndexOf(HASHMAP_BEGIN_SIGN);
                    str = str.Substring(index + 1, str.Length - 1);
                }

                if (str.IndexOf(HASHMAP_END_SIGN) >= 0){
                    int index = str.IndexOf(HASHMAP_END_SIGN);
                    str = str.Substring(0, index);
                }

                // 去除空字符
                if (str.IndexOf(EMPTY_SPACE) >= 0){
                    int index = str.IndexOf(EMPTY_SPACE);
                    str = index < str.Length ? str.Substring(index + 1, str.Length - 1) : str.Substring(0, index);
                }

                return str;
            }
        }

        /** 嵌套Map切割模板 (可以抽象成通用的) **/
        public class SplitNestedMapTemplate : StringUtils.ISplitMap<int, Dictionary<int, int>>{
            private static SplitNestedMapTemplate _template;
            private static char                   _PAIR_SIGN    = ' ';
            private static char                   _ELEMENT_SIGN = ' ';

            public static SplitNestedMapTemplate GetTemplate(char pair, char element){
                if (null == _template){
                    _template = new SplitNestedMapTemplate();
                }

                _PAIR_SIGN    = pair;
                _ELEMENT_SIGN = element;
                return _template;
            }

            public Dictionary<int, int> Split(String value){
                // return StringUtils.SplitToMap<int, int>(SubEmptyOrSpecial(value), _PAIR_SIGN, _ELEMENT_SIGN, SplitMapTemplate.GetTemplate());
                return StringUtils.SplitSkillsToMap(SubEmptyOrSpecial(value), _PAIR_SIGN, _ELEMENT_SIGN, SplitMapTemplate.GetTemplate());
            }

            public int SplitKey(String key){ return StringUtils.ToInt(SubEmptyOrSpecial(key)); }

            public string SubEmptyOrSpecial(string str){
                // 去除特殊符号
                if (str.IndexOf(HASHMAP_BEGIN_SIGN) >= 0){
                    int index = str.IndexOf(HASHMAP_BEGIN_SIGN);
                    str = str.Substring(index + 1, str.Length - 1);
                }

                if (str.IndexOf(HASHMAP_END_SIGN) >= 0){
                    int index = str.IndexOf(HASHMAP_END_SIGN);
                    str = str.Substring(0, index);
                }

                // 去除空字符
                if (str.IndexOf(EMPTY_SPACE_STR) >= 0){
                    int index = str.IndexOf(EMPTY_SPACE_STR);
                    str = index < str.Length ? str.Substring(index + 1, str.Length - 1) : str.Substring(0, index);
                }

                return str;
            }
        }

    #endregion
    }
}