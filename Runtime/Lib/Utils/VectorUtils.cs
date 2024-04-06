using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IG.Runtime.Utils{
    public static class VectorUtils{
        /// <summary>
        /// 两个点的角度
        /// 0°正右
        /// 90°正上
        /// -90°正下
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float PointToAngle(Vector2 p, Vector2 p1, Vector2 p2){
            p.x = p2.x - p1.x;
            p.y = p2.y - p1.y;
            return Mathf.Atan2(p.y, p.x) * 180 / Mathf.PI;
        }

        /// <summary>
        /// -180-180 转为 0-360
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float Angle180To360(float angle){
            if (angle >= 0 && angle <= 180){
                return angle;
            }
            else{
                return 360 + angle;
            }
        }

        /// <summary>
        /// 返回Unity的角度
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float GetUnityDirection(Vector2 p, Vector2 p1, Vector2 p2){
            float angle = Angle180To360(PointToAngle(p, p1, p2));
            float temp  = 360 * 0.125f; //分为8个方向
            float dir   = 0;
            for (int i = 0; i < 8; i++){
                if (angle >= (i * temp) - (temp * 0.5f) && angle < (i * temp) + (temp * 0.5f)){
                    dir = i * temp;
                    break;
                }
            }

            return dir;
        }

        ///n is direction
        public static float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n){
            float angle        = Vector3.Angle(a, b);
            float sign         = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));
            float signed_angle = angle * sign;
            return (signed_angle <= 0) ? 360 + signed_angle : signed_angle;
        }

        public static float SignedAngleBetween(Vector2 a, Vector2 b, Vector3 n){
            float angle        = Vector3.Angle(a, b);
            float sign         = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));
            float signed_angle = angle * sign;
            return (signed_angle <= 0) ? 360 + signed_angle : signed_angle;
        }

        public static Vector2 GetRandomPointInRectangle(Vector2 _center, Vector2 _size){
            float x = Random.Range(_center.x - _size.x / 2, _center.x + _size.x / 2);
            float y = Random.Range(_center.y - _size.y / 2, _center.y + _size.y / 2);
            return new Vector2(x, y);
        }

        /// <summary>
        /// 根据顶点数量和x与y轴的半径获取圆的点位
        /// </summary>
        /// <param name="vertexCount"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        /// <returns></returns>
        public static Vector3[] GetCircle(int vertexCount, float radiusX, float radiusY){
            if (vertexCount <= 3){
                Debug.LogWarning($"Get circle vertex error!please check it!");
            }

            Vector3[] rel = new Vector3[vertexCount];
            return GetCircle(rel, radiusX, radiusY);
        }

        /// <summary>
        /// 根据缓存数组长度获取圆点位
        /// </summary>
        /// <param name="ver"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        /// <returns></returns>
        public static Vector3[] GetCircle(Vector3[] ver, float radiusX, float radiusY){
            if (ver == null){
                Debug.LogWarning($"Circle cache vertex can not be null!");
                ver = new Vector3[10];
            }

            if (ver.Length <= 0){
                Debug.LogWarning($"Circle cache vertex error!please check it!");
            }

            int   vertices_count = ver.Length;
            float angledegree    = 360.0f;
            float angleRad       = Mathf.Deg2Rad * angledegree;
            float angleCur       = angleRad;
            float angledelta     = angleRad / (ver.Length - 1); //delta 计算少算一个vertex 方便填充整圆
            for (int i = 0; i < vertices_count; ++i){
                float cosA = Mathf.Cos(angleCur);
                float sinA = Mathf.Sin(angleCur);
                ver[i]   =  new Vector2(radiusX * cosA, radiusY * sinA);
                angleCur -= angledelta;
            }

            return ver;
        }

        /// <summary>
        /// 判断点是否在单位圆中
        /// </summary>
        /// <returns></returns>
        public static bool IsInCircle(Vector2 pos, Vector2 center, float radius){ return GetDistance(pos, center) < radius; }

        ///<summary>
        ///从矩形中随机取点
        ///<param>0: center 1:size</param>
        ///</summary>
        public static void GetRandomPointInRectangle(ref Vector2 point, Vector2[] center_size){
            if (center_size == null || center_size.Length <= 0){
                // _point= Vector2.zero;
                return;
            }

            point.x = Random.Range(center_size[0].x - center_size[1].x / 2, center_size[0].x + center_size[1].x / 2);
            point.y = Random.Range(center_size[0].y - center_size[1].y / 2, center_size[0].y + center_size[1].y / 2);
        }

        ///<summary>
        ///从矩形中随机取点
        ///</summary>
        public static Vector3 GetRandomPointInRectangle(Vector3 center, Vector3 size){
            float x = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
            float y = Random.Range(center.y - size.y / 2, center.y + size.y / 2);
            float z = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
            return new Vector3(x, y, z);
        }

        ///<summary>
        ///思路下次你看了还能明白就补齐吧
        ///</summary>
        public static bool IsOverlapRectangle(Vector2 center1, Vector2 size1, Vector2 center2, Vector2 size2){
            // Vector2 dis = new Vector2 (_center2.x - _center1.x, _center2.y - _center1.y);
            // if ((_size1.x + _size2.x) / 2 >= dis.x || (_size1.y + _size2.y) / 2 >= dis.y) return true;
            // if ((_size1.x + _size2.x) / 2 >= (_center2.x - _center1.x) || (_size1.y + _size2.y) / 2 >= (_center2.y - _center1.y)) return true;
            if ((size1.x + size2.x) / 2 >= (center2.x - center1.x) &&
                (size1.y + size2.y) / 2 >= (center2.y - center1.y)){
                return true;
            }

            // if (center1.x + size1.x > center2.x &&
            //     center2.x + size2.x > center1.x &&
            //     center1.y + center1.y > center2.y &&
            //     center2.y + center2.y > center1.y
            // ){
            //     return true;
            // }
            return false;
        }

        public static bool IsOverlapRectangle3D(Vector3 center1, Vector3 size1, Vector3 center2, Vector3 size2){
            Vector3 dis = new Vector3(center2.x - center1.x, center2.y - center1.y, center2.z - center1.z);
            if ((size1.x + size2.x) / 2 >= dis.x &&
                (size1.y + size2.y) / 2 >= dis.y &&
                (size2.z + size2.z) / 2 >= dis.z){
                return true;
            }

            return false;
        }

        public static float OverlapArea(Vector2 center1, Vector2 size1, Vector2 center2, Vector2 size2){
            if (VectorUtils.IsOverlapRectangle(center1, size1, center2, size2)){
                return ((Math.Max((center1.x + size1.x) / 2, (center2.x + size2.x) / 2) -
                         Math.Min((center1.x - size1.x) / 2, center2.x - size2.x)) *
                        ((Math.Min((center1.y + size1.y) / 2, (center2.y + size2.y) / 2) -
                          Math.Max((center1.y - size1.y) / 2, (center2.y - size2.y) / 2))));
            }

            return -1;
        }

        public static float GetArea(Vector2 size){ return Mathf.Abs(size.x * size.y); }

        ///<summary>
        ///物尽其用，尽量不用new太多的vector,没有考虑到包含情况(如果是包含情况，中心点就是小的那个矩形)
        ///</summary>
        public static Vector2[] GetOverlap2D(Vector2 center1, Vector2 size1, Vector2 center2, Vector2 size2){
            if (!VectorUtils.IsOverlapRectangle(center1, size1, center2, size2)) return null;
            Vector2[] reuslt = new Vector2[2];

            //取左下角和右上角做计算
            float r_x1  = center1.x - size1.x / 2;
            float r_y1  = center1.y - size1.y / 2;
            float r_x2  = center1.x + size1.x / 2;
            float r_y2  = center1.y + size1.y / 2;
            float r2_x1 = center2.x - size2.x / 2;
            float r2_y1 = center2.y - size2.y / 2;
            float r2_x2 = center2.x + size2.x / 2;
            float r2_y2 = center2.y + size2.y / 2;

            // Vector2 l_b_1 = new Vector2 (r_x1, r_y1);
            // Vector2 r_t_1 = new Vector2 (r_x2, r_y2);
            // Vector2 l_b_2 = new Vector2 (r2_x1, r2_y1);
            // Vector2 r_t_2 = new Vector2 (r2_x2, r2_y2);

            //需要额外注意坐标的问题，这个用法目前太过单一，只能计算边平行与坐标轴的情况
            float   width1      = Mathf.Min(r_x2, r2_x2) - Mathf.Max(r_x1, r2_x1);
            float   high1       = Mathf.Min(r_y2, r2_y2) - Mathf.Max(r_y1, r2_y1);
            Vector2 finallySize = new Vector2(width1, high1);
            //以大的受击碰撞为准，计算比例，按比例和比例参照，给定在两个重心位置间的中心位置 
            Vector3 center_point_crossrectangle = Vector2.zero;
            float   center_x                    = (Mathf.Max(r_x1, r2_x1) + Mathf.Min(r_x2, r2_x2)) / 2;
            float   center_y                    = (Mathf.Max(r_y1, r2_y1) + Mathf.Min(r_y2, r2_y2)) / 2;
            center_point_crossrectangle = new Vector2(center_x, center_y);
            reuslt[0]                   = center_point_crossrectangle;
            reuslt[1]                   = finallySize;
            return reuslt;
        }

        public static bool PointInRectangle(Vector2 point, Vector2 rectangleCenter, Vector2 rectAngleSize){
            float l_b_x = rectangleCenter.x - rectAngleSize.x / 2;
            float l_b_y = rectangleCenter.y - rectAngleSize.y / 2;
            float r_t_x = rectangleCenter.x + rectAngleSize.x / 2;
            float r_t_y = rectangleCenter.y + rectAngleSize.y / 2;
            return (l_b_x < point.x && point.x < r_t_x) && (l_b_y < point.y && point.y < r_t_y);
        }

        public static bool InBigRectangle(Vector2 _c, Vector2 _s, Vector2 _bigC, Vector2 _bigS){
            float c_l_b_x = _c.x    - _s.x    / 2;
            float c_l_b_y = _c.y    - _s.y    / 2;
            float c_r_t_x = _c.x    + _s.x    / 2;
            float c_r_t_y = _c.y    + _s.y    / 2;
            float b_l_b_x = _bigC.x - _bigS.x / 2;
            float b_l_b_y = _bigC.y - _bigS.y / 2;
            float b_r_t_x = _bigC.x + _bigS.x / 2;
            float b_r_t_y = _bigC.y + _bigS.y / 2;
            return (b_l_b_x < c_l_b_x && b_l_b_y < c_l_b_y) &&
                   (c_r_t_x < b_r_t_x && c_r_t_y < b_r_t_y) &&
                   (b_l_b_x < c_l_b_x && c_r_t_y < b_r_t_y) &&
                   (c_r_t_x < b_r_t_x && b_l_b_y < c_l_b_y);
        }

        // public static int ComputOverlapVertexCount (Vector2 _center1, Vector2 _size1, Vector2 _center2, Vector2 _size2) {

        // }

        /// <summary>
        /// 判断线与线之间的相交
        /// </summary>
        /// <param name="intersection">交点</param>
        /// <param name="p1">直线1上一点</param>
        /// <param name="v1">直线1方向</param>
        /// <param name="p2">直线2上一点</param>
        /// <param name="v2">直线2方向</param>
        /// <returns>是否相交</returns>
        public static bool LineLineIntersection(
            out Vector3 intersection,
            Vector3     p1,
            Vector3     v1,
            Vector3     p2,
            Vector3     v2){
            intersection = Vector3.zero;
            if (Vector3.Dot(v1, v2) == 1){
                // 两线平行
                return false;
            }

            Vector3 startPointSeg = p2 - p1;
            Vector3 vecS1         = Vector3.Cross(v1,            v2); // 有向面积1
            Vector3 vecS2         = Vector3.Cross(startPointSeg, v2); // 有向面积2
            float   num           = Vector3.Dot(startPointSeg, vecS1);

            // 打开可以在场景中观察向量
            Debug.DrawLine(p1, p1 + v1,            Color.white,  20000);
            Debug.DrawLine(p2, p2 + v2,            Color.black,  20000);
            Debug.DrawLine(p1, p1 + startPointSeg, Color.red,    20000);
            Debug.DrawLine(p1, p1 + vecS1,         Color.blue,   20000);
            Debug.DrawLine(p1, p1 + vecS2,         Color.yellow, 20000);

            // 判断两这直线是否共面
            if (num >= 1E-05f || num <= -1E-05f){
                return false;
            }

            // 有向面积比值，利用点乘是因为结果可能是正数或者负数
            float num2 = Vector3.Dot(vecS2, vecS1) / vecS1.sqrMagnitude;
            intersection = p1 + v1 * num2;
            return true;
        }

        public static Vector3[] GetNineAround(Vector3 center, Vector3[] culling = null, float unit = 1){
            int           count   = 9;
            List<Vector3> result  = new List<Vector3>();
            Vector3       tempPos = Vector3.zero;
            for (int i = 0; i < count; i++){
                int dx = i % 3;
                int dy = i / 3;
                dx--;
                dy--;
                dy *= -1;
                //TODO:后加的内容，随时可以去除
                if (culling != null){
                    for (int u = 0; u < culling.Length; u++){
                        if (culling[u].x != dx && culling[u].y != dy){
                            continue;
                        }
                    }
                }

                //不需要中心
                if (dx == 0 && dy == 0){
                    continue;
                }

                tempPos.x = center.x + dx * unit;
                tempPos.y = center.y + dy * unit;
                result.Add(tempPos);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取周围坐标
        /// Except self(center)
        /// 上下左右
        /// Runtime
        /// </summary>
        /// <param name="center"></param>
        /// <param name="unit_x"></param>
        /// <param name="unit_y"></param>
        /// <returns></returns>
        public static List<Vector2> GetRectangleAround(List<Vector2> result, Vector2 center, int unit_x = 1, int unit_y = 1){
            //Up
            Vector2 up = Vector2.zero;
            up.x = center.x;
            up.y = center.y + unit_y;
            //down
            Vector2 down = Vector2.zero;
            down.x = center.x;
            down.y = center.y - unit_y;
            //left
            Vector2 left = Vector2.zero;
            left.x = center.x - unit_x;
            left.y = center.y;
            //right
            Vector2 right = Vector2.zero;
            right.x = center.x + unit_x;
            right.y = center.y;
            result.Add(up);
            result.Add(down);
            result.Add(left);
            result.Add(right);
            return result;
        }

        /// <summary>
        /// 获取周围坐标
        /// Except self(center)
        /// 上下左右
        /// Runtime
        /// </summary>
        /// <param name="center"></param>
        /// <param name="unit_x"></param>
        /// <param name="unit_y"></param>
        /// <returns></returns>
        public static List<Vector2> GetRectangleAround(Vector2 center, int unit_x = 1, int unit_y = 1){
            List<Vector2> rel = new List<Vector2>();
            return GetRectangleAround(rel, center, unit_x, unit_y);
        }

        /// <summary>
        /// 根据点与点之间的间隔优化list 
        /// </summary>
        public static void OptimizationShape(ref List<Vector2> list, float tolerance = 0f){
            if (list == null || list.Count <= 0){
                return;
            }

            int length = list.Count;
            for (int i = length - 1; i >= 0; --i){
                // float less = 999999;
                // int nextIndex = -1;
                if (i == 0){
                    break;
                }

                Vector2 temp = list[i];
                for (int j = i - 1; j >= 0; --j){
                    Vector2 getTemp = list[j];
                    float   dis     = GetDistance(temp, getTemp);
                    if (dis < tolerance){
                        list.Remove(getTemp);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// 按距离重新排序(index=0为基准)
        /// </summary>
        /// <returns></returns>
        public static void Reorder(ref List<Vector2> list){
            if (list == null || list.Count <= 0){
                return;
            }

            Vector2 temp   = list[0];
            int     length = list.Count;
            for (int i = 0; i < length; ++i){
                float less      = 999999;
                int   nextIndex = -1;
                if (i == length - 1){
                    break;
                }

                for (int j = i + 1; j < length; ++j){
                    float dis = GetDistance(temp, list[j]);
                    if (dis <= less){
                        less      = dis;
                        nextIndex = j;
                    }
                }

                temp            = list[nextIndex];
                list[nextIndex] = list[i + 1];
                list[i                   + 1] = temp;
            }
        }

        public static List<Vector2> SignSquare(Vector2 center, Vector2 size, float unit){
            Vector2       tempPos  = Vector2.zero;
            float         startX   = center.x - size.x / 2;
            float         startY   = center.y - size.y / 2;
            float         endX     = center.x + size.x / 2;
            float         endY     = center.y + size.y / 2;
            List<Vector2> pointArr = new List<Vector2>();
            for (float x = startX; x <= endX + unit; x += unit){
                for (float y = startY; y <= endY + unit; y += unit){
                    tempPos.x = x;
                    tempPos.y = y;
                    if (pointArr.Contains(tempPos)){
                        continue;
                    }

                    pointArr.Add(tempPos);
                }
            }

            return pointArr;
        }

        public static List<Vector3> SignSquare(Vector3 center, Vector3 size, float unit){
            Vector3       tempPos  = Vector3.zero;
            float         startX   = center.x - size.x / 2;
            float         startY   = center.y - size.y / 2;
            float         startZ   = center.z - size.z / 2;
            float         endX     = center.x + size.x / 2;
            float         endY     = center.y + size.y / 2;
            float         endZ     = center.z + size.z / 2;
            List<Vector3> pointArr = new List<Vector3>();
            for (float x = startX; x <= endX + unit; x += unit){
                for (float y = startY; y <= endY + unit; y += unit){
                    for (float z = startZ; z <= endZ + unit; z += unit){
                        tempPos.x = x;
                        tempPos.y = y;
                        tempPos.z = z;
                        if (pointArr.Contains(tempPos)){
                            continue;
                        }

                        pointArr.Add(tempPos);
                    }
                }
            }

            return pointArr;
        }

        public static void Sort(this Vector2[] v){
            for (int x = 0; x < v.Length; ++x){
                for (int i = 0; i < v.Length; ++i){
                    if (i == 0){
                        continue;
                    }

                    if (v[i - 1].magnitude > v[i].magnitude){
                        Vector2 swap = v[i - 1];
                        v[i - 1] = v[i];
                        v[i]     = swap;
                    }
                }
            }

            // return v;
        }

        public static void Sort(this Vector3[] v){
            for (int x = 0; x < v.Length; ++x){
                for (int i = 0; i < v.Length; ++i){
                    if (i == 0){
                        continue;
                    }

                    if (v[i - 1].magnitude > v[i].magnitude){
                        (v[i - 1], v[i]) = (v[i], v[i - 1]);
                    }
                }
            }

            // return v;
        }

        public static void Sort(this List<Vector2> v){
            for (int x = 0; x < v.Count; ++x){
                for (int i = 0; i < v.Count; ++i){
                    if (i == 0){
                        continue;
                    }

                    if (v[i - 1].magnitude > v[i].magnitude){
                        Vector2 swap = v[i - 1];
                        v[i - 1] = v[i];
                        v[i]     = swap;
                    }
                }
            }

            // return v;
        }

        public static float GetDistance(Vector2 from, Vector2 to){
            Vector2 dir = to                - from;
            return Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);
        }

        public static float GetDistance(Vector2 from, Vector3 to){
            Vector2 dir = (Vector2)to       - from;
            return Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);
        }

        public static float GetDistance(Vector3 from, Vector2 to){
            Vector2 dir = to                - (Vector2)from;
            return Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);
        }

        public static float GetDistance(Vector3 from, Vector3 to){
            Vector3 dir = to                - from;
            return Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);
        }

        public static Vector2 GetVertical(Vector2 @in, Vector2 @in2){
            Vector2 dir = @in2 - @in;
            return new Vector2(dir.y, -dir.x);
        }

        /// <summary>
        /// 根据X轴获取垂线
        /// </summary>
        /// <param name="in"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Vector2 GetVerticalByX(Vector2 @in, bool reverse = false){
            //假设x相等来计算(垂直的线必过一点)
            Vector2 rel = Vector2.zero;
            if (@in.y != 0){
                rel.x = @in.x;
                rel.y = @in.x * rel.x / (reverse ? @in.y : -@in.y);
            }

            return rel;
        }

        /// <summary>
        /// 根据Y轴获取垂线
        /// </summary>
        /// <param name="in"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public static Vector2 GetVerticalByY(Vector2 @in, bool reverse = false){
            //假设y相等来计算(垂直的线必过一点)
            Vector2 rel = Vector2.zero;
            if (@in.x != 0){
                rel.y = @in.y;
                rel.x = @in.y * rel.y / (reverse ? @in.x : -@in.x);
            }

            return rel;
        }

        /// <summary>
        /// 判断点在线的左右侧
        /// rel > 0 左侧
        /// rel < 0 右侧
        /// rel = 0 在线上
        /// </summary>
        /// <param name="line0"></param>
        /// <param name="line1"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float JudgePointOnLine(Vector2 line0, Vector2 line1, Vector2 point){
            float rel = 0;
            // Vector2 line = line1 - line0;
            // Vector2 pointer = point - line0;
            float ax = line1.x - line0.x;
            float ay = line1.y - line0.y;
            float bx = point.x - line0.x;
            float by = point.y - line0.y;
            rel = ax * by - ay * bx;
            return rel;
        }

        /// <summary>
        /// 获取中点
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Vector3 GetCenterPoint(Vector3 start, Vector3 end){ return GetBetweenPoint(start, end, 0.5f); }

        /// <summary>
        /// 获取两点之间距离一定百分比的一个点
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="end">结束点</param>
        /// <param name="distance">起始点到目标点距离百分比</param>
        /// <returns></returns>
        public static Vector3 GetBetweenPoint(Vector3 start, Vector3 end, float percent = 0.5f){
            Vector3 normal   = (end - start).normalized;
            float   distance = Vector3.Distance(start, end);
            return normal * (distance * percent) + start;
        }

        /// <summary>
        /// 获取两点之间一定距离的点
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="end">结束点</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        // public static Vector3 GetBetweenPoint(Vector3 start, Vector3 end, float distance){
        //     Vector3 normal = (end - start).normalized;
        //     return normal * distance + start;
        // }

        /// <summary>
        /// 最小与最大间裁剪(会修改vec)
        /// 当做面处理
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector2 Clamp(this Vector2 vec, Vector2 min, Vector2 max){
            vec.x = Mathf.Clamp(vec.x, min.x, max.x);
            vec.y = Mathf.Clamp(vec.y, min.y, max.y);
            return vec;
        }

        /// <summary>
        /// 重映射
        /// </summary>
        /// <param name="target"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Remap(float oldMin, float oldMax, float newMin, float newMax, float val){
            float percent = (val - oldMin) / (oldMax - oldMin);
            return (newMax - newMin) * percent + newMin;
        }

        /// <summary>
        /// 重映射
        /// </summary>
        /// <param name="oldMin"></param>
        /// <param name="oldMax"></param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Vector2 Remap(Vector2 oldMin, Vector2 oldMax, Vector2 newMin, Vector2 newMax, Vector2 val){
            Vector2 percent = (val - oldMin) / (oldMax - oldMin);
            return (newMax - newMin) * percent + newMin;
        }
    }
    //
    // public class Area{
    //     Vector2
    // }
}