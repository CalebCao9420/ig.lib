using System;
using System.Collections.Generic;
using UnityEngine;

namespace IG.Runtime.Extension{
    using IG.Runtime.Utils;

    public static class ListExtension{
        public static T RemoveAt<T>(this List<T> list, int index){
            if (list == null || list.Count < index){
                return default;
            }

            var rel = list[index];
            list.RemoveAt(index);
            return rel;
        }

        public static void Recycle<T>(this List<T> list){
            //TODO:清除与否需要测试
            if (list.Count > 0){
                list.Clear();
            }

            ListPool<T>.Recycle(list);
        }

        public static bool IsValidIndex<T>(this T[]     array, int idx){ return idx >= 0 && idx < array.Length; }
        public static bool IsValidIndex<T>(this List<T> list,  int idx){ return idx >= 0 && idx < list.Count; }

        public static bool Find<T>(this T[] array, T element, out int idx) where T : class{
            int i   = 0;
            int num = array.Length;
            while (i < num){
                if (array[i] == element){
                    idx = i;
                    return true;
                }

                i++;
            }

            idx = -1;
            return false;
        }

        public static T[] Clone<T>(this T[] array){
            int num    = array.Length;
            T[] array2 = new T[num];
            for (int i = 0; i < num; i++){
                array2[i] = array[i];
            }

            return array2;
        }

        public static List<T> Clone<T>(this List<T> list){
            int     count = list.Count;
            List<T> list2 = new List<T>(count);
            for (int i = 0; i < count; i++){
                list2.Add(list[i]);
            }

            return list2;
        }

        public static T First<T>(this List<T> list){ return (list.Count <= 0) ? default(T) : list[0]; }

        public static T First<T>(this List<T> list, bool remove){
            if (!remove){
                return First(list);
            }

            return (list.Count <= 0) ? default(T) : list.Remove<T>(0);
        }

        public static T Last<T>(this List<T> list){ return (list.Count <= 0) ? default(T) : list[list.Count - 1]; }

        public static T Last<T>(this List<T> list, bool remove){
            if (!remove){
                return Last(list);
            }

            return (list.Count <= 0) ? default(T) : list.Remove(list.Count - 1);
        }

        public static T Pop<T>(this List<T> list){
            int num = list.Count - 1;
            if (num >= 0){
                T result = list[num];
                list.RemoveAt(num);
                return result;
            }

            return default(T);
        }

        public static T GetElementClamped<T>(this T[]     array, int idx){ return array[Mathf.Clamp(idx, 0, array.Length - 1)]; }
        public static T GetElementClamped<T>(this List<T> list,  int idx){ return list[Mathf.Clamp(idx,  0, list.Count   - 1)]; }

        public static void Shuffle<T>(this List<T> collection){
            int i     = 0;
            int count = collection.Count;
            while (i < count - 1){
                int index = UnityEngine.Random.Range(i, count);
                T   value = collection[i];
                collection[i]     = collection[index];
                collection[index] = value;
                i++;
            }
        }

        public static void Shuffle<T>(this T[] collection){
            int i   = 0;
            int num = collection.Length;
            while (i < num - 1){
                int num2 = UnityEngine.Random.Range(i, num);
                T   t    = collection[i];
                collection[i]    = collection[num2];
                collection[num2] = t;
                i++;
            }
        }

    #region Normal extension func

        public static T Remove<T>(this List<T> dir, T target){
            if (!dir.Contains(target)) return default(T);
            int index = 0;
            T   rel   = default;
            for (int i = 0; i < dir.Count; i++){
                if (dir[i].Equals(target)){
                    index = i;
                    rel   = dir[i];
                }
            }

            dir.RemoveAt(index);
            return rel;
        }

        public static T Remove<T>(this List<T> dir, int rmIndex){
            if (dir?.Count <= rmIndex) return default(T);
            T rel = dir[rmIndex];
            dir.RemoveAt(rmIndex);
            return rel;
        }

        /// <summary>
        /// 乱序(无GC)
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> Disrupt<T>(this List<T> list){
            if (list == null || list.Count <= 1){
                return list;
            }

            int loopCount   = list.Count;
            int randomIndex = 0;
            for (int i = 0; i < loopCount; ++i){
                randomIndex = RandomUtils.RandomInt(0, loopCount);
                T swap = list.Remove(randomIndex);
                randomIndex = RandomUtils.RandomInt(0, loopCount - 1);
                list.Insert(randomIndex, swap);
            }

            return list;
        }

        public static List<T> GetNotContainsElements<T>(this List<T> arr, List<T> target){
            List<T> rel = new List<T>();
            for (int i = 0; i < arr.Count; i++){
                if (target.Contains(arr[i])){
                    rel.Add(arr[i]);
                }
            }

            return rel;
        }

        public static string ToString<T>(this System.Collections.IEnumerable list){
            string str = "";
            foreach (var item in list){
                str += item.ToString();
                str += ",";
            }

            return str;
        }

        public static List<T> GetNotContainsElements<T>(this List<T> arr, T[] target){
            List<T> rel = new List<T>();
            for (int i = 0; i < target.Length; i++){
                if (arr.Contains(target[i])){
                    rel.Add(arr[i]);
                }
            }

            return rel;
        }

        public static string ToString<T>(this List<T> list){
            int length = list.Count;
            if (length <= 0){
                return string.Empty;
            }

            string str = "";
            for (int i = 0; i < length; i++){
                str += list[i].ToString();
                str += ",";
            }

            return str;
        }

        private static void AssignWeights<T>(
            IEnumerable<T>       collection,
            Dictionary<T, float> weights,
            Func<T, float>       weightFunc){
            weights.Clear();
            foreach (T t in collection){
                float num   = weightFunc(t);
                float value = num * UnityEngine.Random.value;
                weights.Add(t, value);
            }
        }

    #endregion

    #region Sun and take

        public static float TakeSum(this IList<float> array, int count){
            if (array == null){
                return 0.0f;
            }

            int   length = array.Count;
            float rel    = 0.0f;
            for (int i = 0; count > 0 && i < length; ++i, count--){
                rel += array[i];
            }

            return rel;
        }

        public static float Sum(this IList<float> array){
            int   length = array.Count;
            float rel    = 0;
            for (int i = 0; i < length; ++i){
                rel += array[i];
            }

            return rel;
        }

    #endregion
    }
}