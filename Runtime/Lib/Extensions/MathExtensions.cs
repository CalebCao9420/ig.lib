using IG.Runtime.Utils;
using UnityEngine;

namespace IG.Runtime.Extensions{
    public static class MathExtensions{
        public static int[] ToKetaSplit(this int self){
            if (self < 0){
                return ToKetaSplit(-1 * self);
            }
            else if (self == 0){
                return new int[1]{ 0 };
            }

            int   len = MathUtils.GetDigit(self);
            int[] rel = new int[len];
            for (int i = 0; i < len; ++i){
                rel[i] =  self % 10;
                self   /= 10;
            }

            return rel;
        }

        /// <summary>
        /// 获取百分比 
        /// </summary>
        public static float ToPercent(this float num, int decimalCount = 0){
            float pow = Mathf.Pow(10, decimalCount);
            return Mathf.Round(num * 100f * pow) / pow;
        }

        public enum FloatToInPercentType{
            Round = 0,
            Ceil  = 1,
            Floor = 2,
        }

        /// <summary>
        /// 获取百分比 
        /// </summary>
        public static int ToIntPercent(this float num, FloatToInPercentType type = FloatToInPercentType.Round){
            float F = ToPercent(num, 3);
            switch (type){
                case FloatToInPercentType.Ceil:  return Mathf.CeilToInt(F);
                case FloatToInPercentType.Floor: return Mathf.FloorToInt(F);
                default:                         return Mathf.RoundToInt(F);
            }
        }
    }
}