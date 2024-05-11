namespace IG{
    public delegate bool LoopEvent(float deltaTime);

    public enum LoopEventType{
        Init                = 1 << 2,
        FrameUpdate         = 2 << 2,
        Update              = 3 << 2,
        FixedIntervalUpdate = 4 << 2,
        FixedUpdate         = 5 << 2,
        LateUpdate          = 6 << 2,
        AsyncUpdate         = 7 << 2,
        Destroy             = 8 << 2,
    }

    public interface IGBC{
        string GUID{ get; }
    }

    public interface IGameLoop{
        // GameEvent OnInit             { get; }
        // GameEvent OnFrameTick        { get; }
        // GameEvent OnFixedIntervalTick{ get; }
        // GameEvent OnTick             { get; }
        // GameEvent OnFixedTick        { get; }
        // GameEvent OnLateTick         { get; }
        // GameEvent OnAsyncTick        { get; }
        // GameEvent OnDestroy        { get; }
        void      RegisterEvent(LoopEventType   type, LoopEvent @event);
        void      DeregisterEvent(LoopEventType type, LoopEvent @event);
    }
}