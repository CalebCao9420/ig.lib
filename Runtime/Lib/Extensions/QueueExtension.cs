using System.Collections.Generic;

namespace IG.Runtime.Extensions{
    public static class QueueExtension{
        /// <summary>
        /// 简单遍历
        /// 每个遍历即删除
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="onCheck"></param>
        /// <typeparam name="T"></typeparam>
        public static void Ergodic<T>(this Queue<T> queue, System.Action<T> onCheck){
            while (queue.Count > 0){
                T t = queue.Dequeue();
                onCheck?.Invoke(t);
            }
        }

        /// <summary>
        /// 简单遍历
        /// 每个遍历即删除
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="count">固定遍历数量</param>
        /// <param name="onCheck"></param>
        /// <typeparam name="T"></typeparam>
        public static void Ergodic<T>(this Queue<T> queue, int count, System.Action<T> onCheck){
            int curCount = 0;
            while (queue.Count > 0 && curCount < count){
                T t = queue.Dequeue();
                onCheck?.Invoke(t);
                curCount++;
            }
        }
    }
}