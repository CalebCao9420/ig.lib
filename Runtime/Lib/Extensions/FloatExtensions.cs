using UnityEngine;

namespace IG.Runtime.Extensions{
    public static class FloatExtensions{
        /// <summary>
        /// 根据当前已有值和最大值裁剪自身
        /// </summary>
        public static float TrimExcess(this float increase, float total, float max = 1.0f){
            if (total + increase >= max){
                increase = max - total;
            }

            return increase;
        }

        /// <summary>
        /// 小数位数裁剪 
        /// </summary>
        public static float RoundFloor(this float self, int cot){
            float pow = Mathf.Pow(10, cot);
            return Mathf.Floor((self * pow)) / pow;
        }

        /// <summary>
        /// 小数位数裁剪 
        /// </summary>
        public static float RoundCeil(this float self, int cot){
            float pow = Mathf.Pow(10, cot);
            return Mathf.Ceil((self * pow)) / pow;
        }
    }
}