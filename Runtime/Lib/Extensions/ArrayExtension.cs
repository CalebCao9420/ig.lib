namespace IG.Runtime.Extensions{
    public static class ArrayExtension{
        /// <summary>
        /// 简单安全遍历
        /// </summary>
        public static void Ergodic<T>(this T[] array, System.Action<T> onCheck){
            int len = array?.Length ?? 0;
            for (int i = 0; i < len; ++i){
                T single = array[i];
                onCheck?.Invoke(single);
            }
        }

        /// <summary>
        /// 简单遍历
        /// 条件正确则break
        /// </summary>
        public static void Ergodic<T>(this T[] array, System.Func<T, bool> onCheck){
            int len = array?.Length ?? 0;
            for (int i = 0; i < len; ++i){
                T single = array[i];
                if (onCheck.Invoke(single)){
                    break;
                }
            }
        }
    }
}