namespace IG.Wrapper.PathFinding{
    public interface IPathFindingAgent{
        UnityEngine.Vector2 Pos     { get; }
        int                 AgentSid{ get; }
        bool                IsPause { get; }
        IPathFindingMgr     Mgr     { get; }
        bool                Tick(float deltaTime);
    }
}