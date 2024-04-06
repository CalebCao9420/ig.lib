using System.Collections.Generic;
using UnityEngine;

namespace IG.Runtime.Utils{
    using IG.Runtime.Extension;

    public static class MathUtils{
        /// <summary>
        /// 重映射
        /// </summary>
        /// <param name="v">当前数值</param>
        /// <param name="oldMin">旧最小值</param>
        /// <param name="oldMax">旧最大值</param>
        /// <param name="min">新最小值</param>
        /// <param name="max">新最大值</param>
        /// <returns></returns>
        public static float Remap(float v, float oldMin, float oldMax, float min = 0.0f, float max = 1.0f){
            // if (v - oldMin < 0){
            //     return min;
            // }
            if (oldMin > oldMax){
                Swap(ref oldMin, ref oldMax);
            }

            if (min > max){
                Swap(ref min, ref max);
            }

            float scale = (v - oldMin) / (oldMax - oldMin);
            return scale * (max - min);
        }

        /// <summary>
        /// 获取一个圆的点位
        /// </summary>
        /// <param name="rX">x轴半径</param>
        /// <param name="rY">y轴半径</param>
        /// <param name="angle">目标圆角度</param>
        /// <param name="sAngle">开始角度</param>
        /// <param name="count">点位数量</param>
        /// <param name="aInterval">角度间间隔</param>
        /// <param name="layer">圆的层数</param>
        /// <param name="lInterval">每层间隔</param>
        /// <param name="enter">接收结果的容器</param>
        /// <returns></returns>
        public static List<Vector3> GetCircle(
            float         rX,
            float         rY,
            float         angle,
            float         sAngle,
            int           count,
            float         aInterval,
            float         layer,
            float         lInterval,
            List<Vector3> enter){
            int index    = 0;
            int curCount = 0;
            int tarCount = count;
            while (curCount < tarCount){
                int toBeAssignCount = tarCount - curCount;
                float divisor = aInterval - index * layer <= 0
                    ? aInterval - (index - 1) * layer - index
                    : aInterval - index       * layer;
                int     single_layer_count = (int)(angle / divisor);
                int     remainder          = toBeAssignCount - single_layer_count;
                int     loopCount          = remainder <= 0 ? toBeAssignCount : single_layer_count;
                float   additive_inlayer   = remainder > 0 ? 0 : Mathf.Abs((float)remainder) / (float)loopCount;
                Vector2 tempPos            = Vector2.zero;

                //注意排序
                for (int j = loopCount - 1; j >= 0; j--){
                    float _angle = divisor * j * (1 + additive_inlayer) + sAngle;
                    tempPos.x = Mathf.Cos(_angle / 180 * Mathf.PI) *
                                (rX + lInterval * index);
                    tempPos.y = Mathf.Sin(_angle / 180 * Mathf.PI) *
                                (rY + lInterval * index);
                    enter.Add(tempPos);
                }

                curCount += loopCount;
                index++;
            }

            return enter;
        }

        /// <summary>
        /// Max count = Last circle get count
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="sAngle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="max"></param>
        /// <param name="enter"></param>
        /// <returns></returns>
        public static List<UnityEngine.Vector3> GetCirclePoints(
            float                     angle,
            float                     sAngle,
            float                     x,
            float                     y,
            int                       max,
            List<UnityEngine.Vector3> enter){
            float               tempAngle     = 0;
            UnityEngine.Vector3 coordinate    = UnityEngine.Vector3.zero;
            float               widthSubScale = x / max;
            float               highSubScale  = y / max;
            while (max > 0){
                tempAngle = angle / max;
                // MathUtils
                for (int i = 0; i < max; i++){
                    coordinate.x = Mathf.Cos(tempAngle / 180 * Mathf.PI * i + sAngle) * x;
                    coordinate.y = Mathf.Sin(tempAngle / 180 * Mathf.PI * i + sAngle) * y;
                    enter.Add(coordinate);
                }

                max--;
                x -= widthSubScale;
                y -= highSubScale;
            }

            return enter;
        }

        /// <summary>
        /// Int 版Lerp
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int Lerp(int a, int b, float t){ return Mathf.RoundToInt(a + (b - a) * t); }

        /// <summary>
        /// 根据长度获取整数
        /// length =3
        /// rel = 1000
        /// </summary>
        public static int GetInteger(int length){ return (int)System.Math.Pow(10, length); }

        /// <summary>
        /// 获取整数位
        /// </summary>
        public static int GetDigit(int number){
            int rel = 0;
            while (number >= 1){
                rel++;
                number /= 10;
            }

            return rel;
        }

    #region Fibonacci

        /// <summary>
        /// 斐波那契数列计算
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="lCount"></param>
        /// <param name="interval"></param>
        /// <param name="sAngle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="max"></param>
        /// <param name="enter"></param>
        /// <returns></returns>
        public static List<Vector3> GetCutCirclePoints(float angle, int lCount, float sAngle, float x, float y, int max, List<Vector3> enter){
            UnityEngine.Vector3 coordinate = UnityEngine.Vector3.zero;
            int                 tempCount  = 2;
            float               tempAngle  = angle / tempCount;
            float               unitWidth  = x     / lCount;
            float               unitHigh   = y     / lCount;
            float               tempWidth  = 0, tempHigh = 0;
            for (int i = 3; i < lCount + 3; ++i){
                for (int j = 0; j < tempCount; ++j){
                    if (enter.Count >= max){
                        return enter;
                    }

                    coordinate.x = Mathf.Cos(tempAngle / 180 * Mathf.PI * j + sAngle) * tempWidth;
                    coordinate.y = Mathf.Sin(tempAngle / 180 * Mathf.PI * j + sAngle) * tempHigh;
                    enter.Add(coordinate);
                }

                tempCount =  Fibonacci(i);
                tempAngle =  angle / tempCount;
                tempWidth += unitWidth;
                tempHigh  += unitHigh;
            }

            return enter;
        }

        private static int Fibonacci(int n){
            int f0 = 0;
            int f1 = 1;
            int f2 = 0;
            int t  = 2;
            if (n < 0){
                return 0;
            }
            else if (n == 0 || n == 1){
                return n;
            }
            else{
                while (t <= n){
                    f2 = f0 + f1;
                    f0 = f1;
                    f1 = f2;
                    t++;
                }

                return f2;
            }
        }

    #endregion

    #region Flower Parabola

        /// <summary>
        /// 类花瓣圆(尖锥形)2D
        /// </summary>
        /// <param name="s">起始点(也是终点)</param>
        /// <param name="angle">花瓣旋转角度</param>
        /// <param name="width">花瓣宽度</param>
        /// <param name="length">花瓣直径</param>
        /// <param name="ovalPointCount">椭圆部分精度</param>
        /// <returns></returns>
        public static List<Vector2> FlowerParabola2D(Vector2 s, float angle, float sAngle, float width, float length, int ovalPointCount){
            Vector2 coordinate = Vector2.zero;
            Vector2 offset     = new Vector2(width + s.x, length * 0.5f + s.y);
            // Vector2 offsetRadius = offset - start;
            float newX = (offset.x - s.x) * Mathf.Cos(sAngle / 180.0f * Mathf.PI) -
                         (offset.y - s.y) * Mathf.Sin(sAngle / 180.0f * Mathf.PI) +
                         s.x;
            float newY = (offset.y - s.y) * Mathf.Cos(sAngle / 180.0f * Mathf.PI) +
                         (offset.x - s.x) * Mathf.Sin(sAngle / 180.0f * Mathf.PI) +
                         s.y;

            //Offset 要跟随开口方向
            // Vector2 newOffset = new Vector2(Mathf.Cos(startAngle / 180.0f * Mathf.PI + offset.x),
            //     Mathf.Cos(startAngle / 180.0f * Mathf.PI + offset.y));
            Vector2 newOffset     = new Vector2(newX, newY);
            float   increaseAngle = Vector3.Angle(offset - s, Vector3.right);
            sAngle += increaseAngle;

            //单位间隔(适用于单独直线部分)
            float singleAngle = angle / ovalPointCount;
            //数量必须为偶数
            if (ovalPointCount % 2 != 0){
                ovalPointCount++;
            }

            List<Vector2> result = new List<Vector2>();
            for (int i = -ovalPointCount / 2; i < ovalPointCount / 2; ++i){
                coordinate.x = Mathf.Cos(singleAngle / 180.0f * Mathf.PI * (float)i + sAngle / 180.0f * Mathf.PI) * width  + newOffset.x;
                coordinate.y = Mathf.Sin(singleAngle / 180.0f * Mathf.PI * (float)i + sAngle / 180.0f * Mathf.PI) * length + newOffset.y;
                result.Add(coordinate);
            }

            //左右开弓补充剩下的
            float   remainScale = (1.0f - angle / 360.0f);
            float   dis1        = VectorUtils.GetDistance(result.First(), s);
            float   dis2        = VectorUtils.GetDistance(result.Last(),  s);
            float   arc         = 360.0f * remainScale * Mathf.Deg2Rad;
            int     count1      = Mathf.RoundToInt(dis1 / arc);
            int     count2      = Mathf.RoundToInt(dis2 / arc);
            Vector2 dir1        = (result.First() - s).normalized;
            for (int i = 0; i < count1; ++i){
                result.Insert(i, (s + dir1 * i * arc));
            }

            Vector2 dir2 = (result.Last() - s).normalized;
            for (int i = 0; i < count2; ++i){
                result.Add((s + dir2 * i * arc));
            }

            result.Insert(0, s);
            result.Add(s);
            return result;
        }

        /// <summary>
        /// 类花瓣圆(尖锥形)2D
        /// </summary>
        /// <param name="s">起始点(也是终点)</param>
        /// <param name="angle">花瓣旋转角度</param>
        /// <param name="width">花瓣宽度</param>
        /// <param name="length">花瓣直径</param>
        /// <param name="ovalPointCount">椭圆部分精度</param>
        /// <returns></returns>
        public static List<Vector2> FlowerParabola2D(
            Vector2       s,
            float         angle,
            float         sAngle,
            float         width,
            float         length,
            int           ovalPointCount,
            List<Vector2> result){
            Vector2 coordinate = Vector2.zero;
            Vector2 offset     = new Vector2(width + s.x, length * 0.5f + s.y);
            // Vector2 offsetRadius = offset - start;
            float newX = (offset.x - s.x) * Mathf.Cos(sAngle / 180.0f * Mathf.PI) -
                         (offset.y - s.y) * Mathf.Sin(sAngle / 180.0f * Mathf.PI) +
                         s.x;
            float newY = (offset.y - s.y) * Mathf.Cos(sAngle / 180.0f * Mathf.PI) +
                         (offset.x - s.x) * Mathf.Sin(sAngle / 180.0f * Mathf.PI) +
                         s.y;

            //Offset 要跟随开口方向
            // Vector2 newOffset = new Vector2(Mathf.Cos(startAngle / 180.0f * Mathf.PI + offset.x),
            //     Mathf.Cos(startAngle / 180.0f * Mathf.PI + offset.y));
            Vector2 newOffset     = new Vector2(newX, newY);
            float   increaseAngle = Vector3.Angle(offset - s, Vector3.right);
            sAngle += increaseAngle;

            //单位间隔(适用于单独直线部分)
            float singleAngle = angle / ovalPointCount;
            //数量必须为偶数
            if (ovalPointCount % 2 != 0){
                ovalPointCount++;
            }

            for (int i = -ovalPointCount / 2; i < ovalPointCount / 2; ++i){
                coordinate.x = Mathf.Cos(singleAngle / 180.0f * Mathf.PI * (float)i + sAngle / 180.0f * Mathf.PI) * width  + newOffset.x;
                coordinate.y = Mathf.Sin(singleAngle / 180.0f * Mathf.PI * (float)i + sAngle / 180.0f * Mathf.PI) * length + newOffset.y;
                result.Add(coordinate);
            }

            //左右开弓补充剩下的
            float   remainScale = (1.0f - angle / 360.0f);
            float   dis1        = VectorUtils.GetDistance(result.First(), s);
            float   dis2        = VectorUtils.GetDistance(result.Last(),  s);
            float   arc         = 360.0f * remainScale * Mathf.Deg2Rad;
            int     count1      = Mathf.RoundToInt(dis1 / arc);
            int     count2      = Mathf.RoundToInt(dis2 / arc);
            Vector2 dir1        = (result.First() - s).normalized;
            for (int i = 0; i < count1; ++i){
                result.Insert(i, (s + dir1 * i * arc));
            }

            Vector2 dir2 = (result.Last() - s).normalized;
            for (int i = 0; i < count2; ++i){
                result.Add((s + dir2 * i * arc));
            }

            result.Insert(0, s);
            result.Add(s);
            return result;
        }

    #endregion

    #region Parabola

        public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t){
            float Func(float x) => 4 * (-height * x * x + height * x);
            var mid = Vector3.Lerp(start, end, t);
            return new Vector3(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
            // return new Vector3(Func(t) + Mathf.Lerp(start.x, end.x, t), Func(t) + Mathf.Lerp(start.y, end.y, t), Func(t) + Mathf.Lerp(start.z, end.z, t));
        }

        // public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t){
        //     float Func(float x) => 4 * (-height * x * x + height * x);
        //
        //     var mid = Vector2.Lerp(start, end, t);
        //
        //     return new Vector2(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t));
        // }

        // public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t){
        //     var mid = Vector2.Lerp(start, end, t);
        //     return BezierUtils.GetBezierPoint(height, start, mid, end);
        //     ;
        // }

        public static Vector3 Parabola(Vector3 start, Vector3 end, Vector3 dynamic, float height, float t){
            float Func(float x) => 4 * (-height * x * x + height * x);
            var mid = Vector3.Lerp(start, end, t);
            dynamic.x = mid.x;
            dynamic.y = Func(t) + Mathf.Lerp(start.y, end.y, t);
            dynamic.z = mid.z;
            return dynamic;
        }

        // public static Vector2 Parabola(Vector2 start, Vector2 end, Vector2 dynamic, float height, float t){
        //     float Func(float x) => 4 * (-height * x * x + height * x);
        //
        //     var mid = Vector2.Lerp(start, end, t);
        //
        //     dynamic.x = mid.x;
        //     dynamic.y = Func(t) + Mathf.Lerp(start.y, end.y, t);
        //     return dynamic;
        // }

        // public static Vector2 Parabola(Vector2 start, Vector2 end, Vector2 dynamic, float height, float t){
        //     var mid = Vector2.Lerp(start, end, t);
        //
        //     dynamic = BezierUtils.GetBezierPoint(t, start, mid, end);
        //     return dynamic;
        // }

    #endregion

    #region Internal utils

        /// <summary>
        /// 交换值
        /// 需要区分.NET版本
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        public static void Swap(ref int o1, ref int o2){
            // int swap = o1;
            // o1 = o2;
            // o2 = swap;
            (o1, o2) = (o2, o1);
        }

        /// <summary>
        /// 交换值
        /// 需要区分.NET版本
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        public static void Swap(ref float o1, ref float o2){
            // float swap = o1;
            // o1 = o2;
            // o2 = swap;
            (o1, o2) = (o2, o1);
        }

    #endregion
    }
}