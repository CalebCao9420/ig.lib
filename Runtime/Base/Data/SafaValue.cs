namespace IG.Data{
    using System;
    public static class SafaValue{
        private static int j = 1;
        private static int k = 1;
        /// <summary>
        /// Init this instance.
        /// </summary>
        static SafaValue() {
            Random r = new Random();
            j = r.Next(1, 1000);
            k = r.Next(1, 10);
        }

        /// <summary>
        /// Encryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static int Encryption(this int value){
            return value * SafaValue.k + SafaValue.j;
        }

        /// <summary>
        /// Decryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static int Decryption(this int value){
            return (value - SafaValue.j) / SafaValue.k;
        }

        /// <summary>
        /// Encryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static float Encryption(this float value){
            return value * SafaValue.k + SafaValue.j;
        }

        /// <summary>
        /// Decryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static float Decryption(this float value){
            return (value - SafaValue.j) / SafaValue.k;
        }

        /// <summary>
        /// Encryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static double Encryption(this double value){
            return value * SafaValue.k + SafaValue.j;
        }

        /// <summary>
        /// Decryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static double Decryption(this double value){
            return (value - SafaValue.j) / SafaValue.k;
        }

        /// <summary>
        /// Encryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static long Encryption(this long value){
            return value * SafaValue.k + SafaValue.j;
        }

        /// <summary>
        /// Decryption the specified value.
        /// </summary>
        /// <param name="value">Value.</param>
        public static long Decryption(this long value){
            return (value - SafaValue.j) / SafaValue.k;
        }
    }
}