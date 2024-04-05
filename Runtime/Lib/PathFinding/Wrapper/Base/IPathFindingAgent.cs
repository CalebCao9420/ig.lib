namespace IG.Wrapper.PathFinding{
    public interface IPathFindingAgent{
        int             AgentSid{ get; }
        bool            IsPause { get; }
        IPathFindingMgr Mgr     { get; }
        bool            Tick(float deltaTime);
    }
}