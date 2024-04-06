// using System.Collections.Generic;
// using Newtonsoft.Json;
// using UnityEngine;
// using Formatting = Newtonsoft.Json.Formatting;
//
// public class JSONUtils {
//     /// <summary>
//     /// json字符串转对象
//     /// </summary>
//     /// <typeparam name="T">对象类型</typeparam>
//     /// <param name="jsonContent">json字符串</param>
//     /// <returns></returns>
//     public static T JsonToObject<T>(string jsonContent) {
//         //TODO: 目前只需要这两种情况，因为json格式的问题，后边有情况再多加判断
//         T obj = default;
//         obj = JsonConvert.DeserializeObject<T>(jsonContent);
//
//         if (obj != null) {
//             return obj;
//         }
//         Debug.LogError("Data can not blank in JSONUtils");
//         return default(T);
//     }
//
//     public static List<T> JsonToListObj<T>(string jsonContent) {
//         List<T> obj = JsonConvert.DeserializeObject<List<T>>(jsonContent);
//         if (obj != null) {
//             return obj;
//         }
//
//         return default(List<T>);
//     }
//
//     /// <summary>
//     /// 对象转json字符串
//     /// </summary>
//     /// <typeparam name="T"></typeparam>
//     /// <param name="obj"></param>
//     /// <returns></returns>
//     public static string ObjectToJson<T>(T obj) {
//         string result = JsonConvert.SerializeObject(obj);
//         return result;
//     }
//
//     public static string ObjectToJson<T>(T obj, bool format) {
//         string result =
//             format ? JsonConvert.SerializeObject(obj, Formatting.Indented) : JsonConvert.SerializeObject(obj);
//         return result;
//     }
//
//     public static string ObjectToJson<T>(T obj, bool format , bool ignore = false) {
//         if (ignore) {
//             var json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings {
//                 ReferenceLoopHandling = ReferenceLoopHandling.Ignore,  //忽略实体中实体，不再序列化里面包含的实体
//                 Formatting = Formatting.Indented
//             });
//             return json;
//         }
//         string result =
//             format ? JsonConvert.SerializeObject(obj, Formatting.Indented) : JsonConvert.SerializeObject(obj);
//         return result;
//     }
// }