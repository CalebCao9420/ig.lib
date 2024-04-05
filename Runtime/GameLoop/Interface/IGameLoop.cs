namespace IG{
    public delegate bool GameEvent(float deltaTime);
    
    public enum GameEventType{
        Init,
        FrameUpdate,
        Update,
        FixedIntervalUpdate,
        FixedUpdate,
        LateUpdate,
        AsyncUpdate,
    }

    public interface IGBC{
        string GUID{ get; }

        bool Init(float deltaTime);
        bool FrameTick(float deltaTime);
        bool Tick(float deltaTime);
        bool FixedIntervalTick(float deltaTime);
        bool FixedTick(float deltaTime);
        bool LateTick(float deltaTime);
        bool AsyncTick(float deltaTime);

        bool OnDestroy();
    }

    public interface IGameLoop{
        GameEvent OnInit{ get; }
        GameEvent OnFrameTick{ get; }
        GameEvent OnFixedIntervalTick{ get; }
        GameEvent OnTick{ get; }
        GameEvent OnFixedTick{ get; }
        GameEvent OnLateTick{ get; }

        GameEvent OnAsyncTick{ get; }

        void RegisterEvent(GameEventType type, GameEvent @event);
        void DeregisterEvent(GameEventType type, GameEvent @event);
    }
}