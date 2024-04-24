using System.Collections.Generic;
using System.Text;
using IG.Runtime.Utils;

namespace IG.Runtime.Extensions{
    public static class StringExtension{
        /// <summary>
        /// Rel length = return target length
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sign"></param>
        /// <param name="rel"></param>
        /// <returns></returns>
        public static string[] Split(this string str, char sign, string[] rel = null){
            StringBuilder buffer = StringUtils.GetBuffer();
            int           index  = 0, relIndex = 0;
            while (index <= str.Length){
                if ((index == str.Length || str[index].Equals(sign)) && buffer.Length > 0){
                    rel[relIndex] = buffer.ToString();
                    buffer.Clear();
                    relIndex++;
                }
                else{
                    buffer.Append(str[index]);
                }

                index++;
            }

            buffer.RecycleBuffer();
            return rel;
        }

        public static List<string> Split(this string str, char sign, List<string> rel = null){
            StringBuilder buffer = StringUtils.GetBuffer();
            int           index  = 0, relIndex = 0;
            while (index <= str.Length){
                if (((index == str.Length || str[index].Equals(sign))) && buffer.Length > 0){
                    rel.Add(buffer.ToString());
                    buffer.Clear();
                    relIndex++;
                }
                else{
                    buffer.Append(str[index]);
                }

                index++;
            }

            buffer.RecycleBuffer();
            return rel;
        }

    #region Builder

        private static StringBuilder stringBuilder = new StringBuilder();

        public static StringBuilder StringBuilder{
            get{
                stringBuilder.Length = 0;
                return stringBuilder;
            }
        }

        public static string Concat(params object[] param){
            StringBuilder stringBuilder = StringBuilder;
            int           len           = param == null ? 0 : param.Length;
            for (int i = 0; i < len; ++i){
                stringBuilder.Append(param[i]);
            }

            return stringBuilder.ToString();
        }

        public static string Concat(string string0, string string1){ return StringBuilder.Append(string0).Append(string1).ToString(); }

        public static string Concat(string string0, string string1, string string2){
            return StringBuilder.Append(string0).Append(string1).Append(string2).ToString();
        }

        public static string Concat(string string0, string string1, string string2, string string3){
            return StringBuilder.Append(string0).Append(string1).Append(string2).Append(string3).ToString();
        }

        public static string Concat(string string0, string string1, string string2, string string3, string string4){
            return StringBuilder.Append(string0).Append(string1).Append(string2).Append(string3).Append(string4).ToString();
        }

    #endregion
    }
}