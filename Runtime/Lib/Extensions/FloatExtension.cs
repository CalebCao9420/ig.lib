namespace IG.Runtime.Extensions{
    public static class FloatExtension{
        /// <summary>
        /// 根据当前已有值和最大值裁剪自身
        /// </summary>
        public static float TrimExcess(this float increase, float total, float max = 1.0f){
            if (total + increase >= max){
                increase = max - total;
            }

            return increase;
        }
    }
}