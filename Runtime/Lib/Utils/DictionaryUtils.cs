using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace IG.Runtime.Utils{
    public static class DictionaryUtils{
        public static TValue GetValue<Tkey, TValue>(this Dictionary<Tkey, TValue> dic, Tkey key){
            TValue value = default(TValue);
            dic.TryGetValue(key, out value);
            return value;
        }

        public static Tkey GetKey<Tkey, TValue>(this Dictionary<Tkey, TValue> dic, TValue value){
            Tkey key = default(Tkey);
            if (value == null){
                return key;
            }

            try{
                key = dic.GetKey(value);
            }
            catch{
                foreach (var data in dic){
                    if (data.Value.Equals(value)){
                        key = data.Key;
                    }
                }
            }

            return key;
        }

        public static List<TValue> Values<Tkey, TValue>(this Dictionary<Tkey, TValue> dic){
            List<TValue> values = new List<TValue>();
            foreach (var data in dic){
                values.Add(data.Value);
            }

            return values;
        }

        public static List<Tkey> Keys<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> dic){
            if (dic.Count <= 0){
                return default;
            }

            List<Tkey> result = new List<Tkey>();
            foreach (var temp in dic){
                result.Add(temp.Key);
            }

            return result;
        }

        public static void Readd<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> dic, Tkey key, Tvalue value){
            if (!dic.ContainsKey(key)){
                dic.Add(key, value);
                return;
            }

            dic.Remove(key);
            dic.Add(key, value);
        }
    }
}