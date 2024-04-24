using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace IG.Runtime.Utils{

    public static class RandomUtils{
        private static int    _seed;
        public static  Random Random = new Random();

        public static int Seed{
            get{ return _seed; }
            set{
                _seed  = value;
                Random = new Random(value);
            }
        }

    #region Utils

        /// <summary>
        /// 圆内随机
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2 RandomCircle(float radiusMin, float radiusMax){
            Vector2 rel = Vector2.zero;
            //圆内随机需要半径和角度
            float angle = Range(0.0f, 360.0f);
            // rel.x = (float) Math.Cos(angle);
            // rel.y = (float) Math.Sin(angle);
            rel.x = (float)Math.Cos(angle / 180 * Math.PI);
            rel.y = (float)Math.Sin(angle / 180 * Math.PI);
            return rel * Range(radiusMin, radiusMax);
        }

        /// <summary>
        /// 圆内随机
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2 RandomCircle(float radius){
            Vector2 rel = Vector2.zero;
            //圆内随机需要半径和角度
            float angle = Range(0.0f, 360.0f);
            // rel.x = (float) Math.Cos(angle);
            // rel.y = (float) Math.Sin(angle);
            rel.x = (float)Math.Cos(angle / 180 * Math.PI);
            rel.y = (float)Math.Sin(angle / 180 * Math.PI);
            return rel * Range(0.0f, radius);
        }

        public static void RandomCircle(ref Vector2 rel, float radius){
            //圆内随机需要半径和角度
            float angle = Range(0.0f, 360.0f);
            // rel.x = (float) Math.Cos(angle);
            // rel.y = (float) Math.Sin(angle);
            rel.x =  (float)Math.Cos(angle / 180 * Math.PI);
            rel.y =  (float)Math.Sin(angle / 180 * Math.PI);
            rel   *= Range(0.0f, radius);
        }

        public static Vector2 RandomUnitCircle()               { return RandomCircle(1.0f); }
        public static void                RandomUnitCircle(ref Vector2 pos){ RandomCircle(ref pos, 1.0f); }

        /// <summary>
        /// 圆边随机
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Vector2 RandomOnCircleEdge(float radius){
            Vector2 rel = Vector2.zero;
            // float unit = Range(0.0f, 1.0f);
            // float theta = (float) (2 * Math.PI * unit);
            float theta = Range(0.0f, 360.0f);
            rel.x = (float)Math.Cos(theta / 180 * Math.PI);
            rel.y = (float)Math.Sin(theta / 180 * Math.PI);
            return rel * radius;
        }

        public static Vector2 RandomSquare(float scale){
            Vector2 rel = Vector2.one;
            float               x   = Range(0.0f, rel.x);
            float               y   = Range(0.0f, rel.y);
            rel.x = x;
            rel.y = y;
            return rel * scale;
        }

        public static long RandomId(int idLength){
            long rel = 0;
            for (int i = idLength - 1; i >= 0; --i){
                long integer   = MathUtils.GetInteger(i);
                long randomNum = Math.Abs(RandomInt(i == idLength - 1 ? 1 : 0, 10));
                rel += integer * randomNum;
            }

            return rel;
        }

        public static int RandomInt(int min, int max){ return Random.Next(min, max); }

        public static int RandomInt(int seed, int min, int max){
            Random temp = Random;
            Random = new Random(seed);
            int resule = Random.Next(min, max);
            Random = temp;
            return resule;
        }

        public static float  Range(float  min, float  max){ return min + Random.RandomFloat() * (max - min); }
        public static double Range(double min, double max){ return min + Random.NextDouble()  * (max - min); }

        public static float Range(int seed, float min, float max){
            Random temp = Random;
            Random = new Random(seed);
            float resule = Range(min, max);
            Random = temp;
            return resule;
        }

        public static int Next(){ return Random.Next(); }

        //public static float RandomFloat { get { return (float)Random.NextDouble(); } }

    #endregion

    #region Extension

        public static T RandomIndex<T>(List<T> list){
            int index = list != null && list.Count > 0 ? RandomInt(0, list.Count) : -1;
            return index >= 0 ? list[index] : default;
        }

        //public static int RandomInt(this System.Random random, int min, int max) {
        //    return random.Next(min, max);
        //}
        public static float RandomFloat(this Random random)                      { return (float)random.NextDouble(); }
        public static int   Next(this        Random random)                      { return random.Next(); }
        public static float Range(this       Random random, float min, float max){ return min + RandomFloat(random) * (max - min); }
        public static bool  RandomBool(){ return RandomInt(0, 2) == 0; }

    #endregion

    }
}