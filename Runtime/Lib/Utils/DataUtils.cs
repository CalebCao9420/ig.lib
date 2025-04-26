using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using IG.Runtime.Extensions;

namespace IG.Runtime.Utils{
    /// <summary>
    /// 数据处理工具
    /// </summary>
    public static class DataUtils{
        public const           string   SOLIT_SIGN     = ",";
        public static readonly string[] S_ArraySpeSign ={ ",", "\"", "\\r", "\\n" };

    #region DataTable - CSV

        public static DataTable ConvertDataTable<K, V>(List<Dictionary<K, V>> data, string tableName = ""){
            DataTable        dt = new DataTable(tableName);
            Dictionary<K, V> sd = data.First();
            if (null == sd){
                return dt;
            }

            foreach (var s in sd){
                dt.Columns.Add(s.Key.ToString());
            }

            int dataLen = data?.Count ?? 0;
            for (int i = 0; i < dataLen; ++i){
                Dictionary<K, V> single = data[i];
                DataRow          dr     = dt.NewRow();
                foreach (var s in single){
                    dr[s.Key.ToString()] = s.Value.ToString();
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

        /// <summary>
        /// 对象转DataTable 
        /// </summary>
        public static DataTable Object2DataTable<T>(List<T> ori, string tableName = "") where T : class{
            //result 
            DataTable dt = new DataTable(tableName);
            if (null == ori || ori.Count <= 0){
                return dt;
            }

            System.Type  type        = typeof(T);
            var          files       = type.GetFields();
            List<string> columnNames = new();
            int          len         = files?.Length ?? 0;
            for (int i = 0; i < len; ++i){
                dt.Columns.Add(files[i].Name);
                columnNames.Add(files[i].Name);
            }

            //single node
            int dataLen = ori?.Count ?? 0;
            for (int i = 0; i < dataLen; ++i){
                T       single = ori[i];
                DataRow dr     = dt.NewRow();
                foreach (var col in columnNames){
                    dr[col] = type.GetField(col).GetValue(single);
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

        public static void SaveDataTableToCSV(DataTable dt, string filePath, bool checkSpeSign = false){
            string dirPath = Path.GetDirectoryName(filePath);
            if (false == Directory.Exists(dirPath)){
                Directory.CreateDirectory(dirPath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)){
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8)){
                    StringBuilder sb = new StringBuilder();
                    //写入表头
                    int len = dt.Columns?.Count ?? 0;
                    for (int i = 0; i < len; ++i){
                        sb.Append(dt.Columns[i].ColumnName.ToString());
                        if (i < len - 1){
                            sb.Append(SOLIT_SIGN);
                        }
                    }

                    sw.WriteLine(sb.ToString());
                    //写入每一行每一列的数据
                    int rowLen = dt.Rows?.Count ?? 0;
                    for (int i = 0; i < rowLen; ++i){
                        sb.Clear();
                        int columnsLen = dt.Columns?.Count ?? 0;
                        for (int j = 0; j < columnsLen; ++j){
                            string str = dt.Rows[i][j].ToString();
                            if (checkSpeSign){
                                str = QuoteValue(str);
                            }

                            sb.Append(str);
                            if (j < columnsLen - 1){
                                sb.Append(SOLIT_SIGN);
                            }
                        }

                        sw.WriteLine(sb.ToString());
                    }

                    sw.Close();
                    fs.Close();
                }
            }
        }

    #region Private functions

        private static string QuoteValue(string value){
            bool isChecked = false;

            bool OnCheck(string sign){
                if (value.Contains(sign)){
                    isChecked = true;
                    return true;
                }

                return false;
            }

            S_ArraySpeSign.Ergodic(OnCheck);
            if (isChecked){
                // 使用双引号包裹值，并在内部将双引号替换为两个双引号来避免CSV解析错误。
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

    #endregion

    #endregion
    }
}