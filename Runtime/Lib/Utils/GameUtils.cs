using System;
using UnityEngine;

namespace IG.Runtime.Utils{
    public static class GameUtils{
        public static void CleanMemory(){
            GC.Collect();
            //只走System清理内存，不动资源部分
            // Resources.UnloadUnusedAssets();
        }

        public static void SetTargetFrame(int frame){ Application.targetFrameRate = frame; }

        public static void CleanPathFindingCache(){
            // Pathfinding.RVO.RVOSimulator.active.GetSimulator().ClearAgents();
        }
    }
}