using System;

namespace IG.Wrapper.PathFinding{
    public interface IPathFindingAgent : IDisposable{
        UnityEngine.Vector2 Pos     { get; }
        int                 AgentSid{ get; }
        bool                IsPause { get; }
        IPathFindingMgr     Mgr     { get; }
        void                Setup(int  sid);
        bool                Tick(float deltaTime);
    }
}