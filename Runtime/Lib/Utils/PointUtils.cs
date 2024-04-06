using System;
using UnityEngine;
using Object = System.Object;

namespace IG.Runtime.Utils{
    public static class PointUtils{
        /// <summary>
        /// 屏幕坐标转世界坐标，相对距离有特殊需求需要重新赋值
        /// </summary>
        /// <param name="renderCamera"></param>
        /// <param name="screenPos"></param>
        /// <param name="cameraRelativeDistance"></param>
        /// <returns></returns>
        public static Vector3 ScreenToWorld(Camera renderCamera, Vector3 screenPos, float cameraRelativeDistance = 10){
            screenPos.z = cameraRelativeDistance;
            return renderCamera.ScreenToWorldPoint(screenPos);
        }

        /// <summary>
        /// 世界坐标转屏幕坐标，相对距离有特殊需求需要重新赋值
        /// </summary>
        /// <param name="renderCamera"></param>
        /// <param name="worldPos"></param>
        /// <param name="cameraRelativeDistance"></param>
        /// <returns></returns>
        public static Vector3 WorldToScreen(Camera renderCamera, Vector3 worldPos, float cameraRelativeDistance = -10){
            worldPos.z = cameraRelativeDistance;
            return renderCamera.WorldToScreenPoint(worldPos);
        }

        public static bool PointIsInArea(Vector2 _point, Vector2 _center, Vector2 _bounds){
            float _left  = _center.x - _bounds.x / 2;
            float _right = _center.x + _bounds.x / 2;
            float _down  = _center.y - _bounds.y / 2;
            float _up    = _center.y + _bounds.y / 2;
            if (_point.x >= _left && _point.x <= _right && _point.y >= _down && _point.y <= _up){
                return true;
            }

            return false;
        }
    }
}