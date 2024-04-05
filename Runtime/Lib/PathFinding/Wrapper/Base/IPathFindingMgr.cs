using System.Collections.Generic;

namespace IG.Wrapper.PathFinding{
    using Vector2 = UnityEngine.Vector2;
    public interface IPathFindingMgr{
        Dictionary<int, IPathFindingAgent> Agents{ get; }
        void                               Setup(IPathFindingCnf cnf);
        bool                               Tick(float            deltaTime);

    #region agent option

        bool AddAgent(IPathFindingAgent    agent);
        bool DeleteAgent(int               sid);
        bool DeleteAgent(IPathFindingAgent agent);

    #endregion

    #region Obstacle option
        bool AddObstacle(IList<Vector2> vertices);
    #endregion
    }
}