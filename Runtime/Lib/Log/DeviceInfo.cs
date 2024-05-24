using System;
using UnityEngine.Device;
using UnityEngine.Profiling;

namespace IG.Runtime.Log{
    public static class DeviceInfo{
    #region Memory info

        private const long   KB               = 1024;
        private const long   MB               = 1024 * 1024;
        private const string FORMAT_MB        = "{0}(MB)";
        private const string FORMAT_KB        = "{0}(KB)";
        private const string FORMAT_HEAP      = "[堆内存]:{0}";
        private const string FORMAT_USED      = "[使用大小]:{0}";
        private const string FORMAT_ALLOCATED = "[Unity分配]:{0}";
        private const string FORMAT_TOTAL     = "[总内存]:{0}";
        private const string FORMAT_UNUSED    = "[未使用内存]:{0}";
        private const string FORMAT_ALL       = "[堆内存]:{0},[Unity分配]:{1},[使用大小/未使用/总内存]:{2}/{3}/{4}";

        /// <summary>
        /// 堆内存
        /// </summary>
        public static string GetHeap(){ return string.Format(FORMAT_HEAP, string.Format(FORMAT_MB, Profiler.GetMonoHeapSizeLong() / MB)); }

        /// <summary>
        /// 使用的
        /// </summary>
        public static string GetUsed(){ return string.Format(FORMAT_USED, string.Format(FORMAT_MB, Profiler.GetMonoUsedSizeLong() / MB)); }

        /// <summary>
        /// unity分配
        /// </summary>
        public static string GetUnityAllocated(){
            return string.Format(FORMAT_ALLOCATED, string.Format(FORMAT_MB, Profiler.GetTotalAllocatedMemoryLong() / MB));
        }

        /// <summary>
        /// 总内存
        /// </summary>
        public static string GetTotal(){ return string.Format(FORMAT_TOTAL, string.Format(FORMAT_MB, Profiler.GetTotalReservedMemoryLong() / MB)); }

        // 未使用内存
        public static string GetUnused(){
            return string.Format(FORMAT_UNUSED, string.Format(FORMAT_MB, Profiler.GetTotalUnusedReservedMemoryLong() / MB));
        }

        //所有信息
        public static string GetAllMemory(){
            string heap      = string.Format(FORMAT_MB, Profiler.GetMonoHeapSizeLong() / MB);
            string used      = string.Format(FORMAT_MB, Profiler.GetMonoUsedSizeLong() / MB);
            string allocated = string.Format(FORMAT_MB, Profiler.GetTotalAllocatedMemoryLong() / MB);
            string total     = string.Format(FORMAT_MB, Profiler.GetTotalReservedMemoryLong() / MB);
            string unused    = string.Format(FORMAT_MB, Profiler.GetTotalUnusedReservedMemoryLong() / MB);
            return string.Format(FORMAT_ALL, heap, allocated, used, unused, total);
        }

    #endregion

    #region Device simple infp
        private const string DATE_FORMAT     = "yyyyMMdd_HHmmss";
        private const string DATE_FORMAT_DAY = "yyyyMMdd";


        /// <summary>
        /// 设备型号
        /// </summary>
        public static string GetDeviceModel(){
#if UNITY_EDITOR
            return $"[Editor]{SystemInfo.deviceModel}";
#elif UNITY_IPHONE
 return UnityEngine.iOS.Device.generation.ToString();
#else
return SystemInfo.deviceModel;
#endif
        }

        public static string GetDate(){
            string       dateStr       = DateTime.Now.ToString(DATE_FORMAT);
            return dateStr;
        }

        public static string GetDataForDay(){
            return  DateTime.Now.ToString(DATE_FORMAT_DAY);
        }

    #endregion
    }
}