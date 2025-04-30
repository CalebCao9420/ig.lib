using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace IG.Runtime.Utils{
    public class JSONUtils{
        /// <summary>
        /// json字符串转对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonContent">json字符串</param>
        /// <returns></returns>
        public static T JsonToObject<T>(string jsonContent){
            if (string.IsNullOrEmpty(jsonContent)){
                Debug.LogError("Deserialize string can not be empty!!");
                return default(T);
            }

            T obj = JsonConvert.DeserializeObject<T>(jsonContent);
            if (obj != null){
                return obj;
            }

            Debug.LogError("Data can not blank in JSONUtils");
            return default(T);
        }

        /// <summary>
        /// json字符串转对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="jsonContent">json字符串</param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string jsonContent, JsonSerializerSettings settings){
            if (string.IsNullOrEmpty(jsonContent)){
                Debug.LogError("Deserialize string can not be empty!!");
                return default(T);
            }

            T obj = JsonConvert.DeserializeObject<T>(jsonContent, settings);
            if (obj != null){
                return obj;
            }

            Debug.LogError("Data can not blank in JSONUtils");
            return default(T);
        }

        /// <summary>
        /// 对象转json字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson<T>(T obj){
            if (null == obj){
                Debug.LogError("SerializedObject can not be null!!");
                return string.Empty;
            }

            string result = JsonConvert.SerializeObject(obj);
            return result;
        }

        /// <summary>
        /// 对象转json字符串
        /// 带格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ObjectToJson<T>(T obj, Formatting format){
            if (null == obj){
                Debug.LogError("SerializedObject can not be null!!");
                return string.Empty;
            }

            string result = JsonConvert.SerializeObject(obj, format);
            return result;
        }

        /// <summary>
        /// 对象转json字符串
        /// 使用NewtonSoft 序列化Setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static string ObjectToJson<T>(T obj, JsonSerializerSettings settings){
            if (null == obj){
                Debug.LogError("SerializedObject can not be null!!");
                return string.Empty;
            }

            var json = JsonConvert.SerializeObject(obj, settings);
            return json;
        }
    }
}