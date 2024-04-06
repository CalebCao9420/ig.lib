using System;
using UnityEngine;

namespace IG.Runtime.Utils{
    public class ColorUtils{
        public static UnityEngine.Color SafeColor()     { return UnityEngine.Color.green; }
        public static UnityEngine.Color DangerousColor(){ return UnityEngine.Color.red; }
        public static UnityEngine.Color NullColor()     { return UnityEngine.Color.grey; }
        public static UnityEngine.Color NormalColor()   { return UnityEngine.Color.white; }

        //TODO: If not match true color ,what do you want
        ///And must have # in first
        public static UnityEngine.Color GetColorByStr(string str, bool _isFormat = false){
            if (!_isFormat){
                str.Insert(0, "#");
            }

            UnityEngine.Color result;
            // if (str.Contains ("#")) {
            //     str.Substring (str.IndexOf ("#"));
            // }
            if (!ColorUtility.TryParseHtmlString(str, out result)){
                Debug.LogWarning(String.Format("{0}:{1}", "Color switch error", str));
            }

            return result;
        }

        //Hex to color
        public static UnityEngine.Color GetColorByStr(string str){
            UnityEngine.Color result;
            if (str.Length > 8){
                Debug.LogError(string.Format("{0}:{1}", "Error color to get", str));
                result = UnityEngine.Color.red;
                return result;
            }

            // if (str.Contains ("#")) {
            //     str.Substring (str.IndexOf ("#"));
            // }
            // int v = int.Parse (str, System.Globalization.NumberStyles.HexNumber);
            // return new Color (
            //     (float) (v >> 16) / 255f,
            //     (float) (v >> 8) / 255f,
            //     (float) (v >> 0) / 255f
            // );
            byte  br = byte.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte  bg = byte.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte  bb = byte.Parse(str.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte  bc = byte.Parse(str.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            float r  = br / 255f;
            float g  = bg / 255f;
            float b  = bb / 255f;
            float c  = bc / 255f;
            result = new UnityEngine.Color(r, g, b, c);
            return result;
        }

        public static string GetColorValue(UnityEngine.Color _in){ return ColorUtility.ToHtmlStringRGB(_in); }

        //Uint to color
        public static UnityEngine.Color ParseColor(uint v){
            return new UnityEngine.Color(
                                         ((v >> 24) & 0xff) / 255.0f,
                                         ((v >> 16) & 0xff) / 255.0f,
                                         ((v >> 8)  & 0xff) / 255.0f,
                                         (v         & 0xff) / 255.0f
                                        );
        }
    }
}