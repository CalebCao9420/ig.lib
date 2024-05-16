namespace IG.Runtime.Extensions{
    public static class FloatExtension{
        /// <summary>
        /// 可以抽到工具方法里边去 
        /// </summary>
        public static float CullOverflowsVal(this float increase, float total, float target = 1.0f){
            if (total + increase >= target){
                increase = target - total;
            }

            return increase;
        }
    }
}