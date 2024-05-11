namespace IG.Events{
    public interface IGameEventParam{ }

    public class GameEvent{
        public readonly IGameEventParam EventParam = null;
        public readonly string          EventType;

        public GameEvent(string eventType, IGameEventParam eventParams = null){
            this.EventType  = eventType;
            this.EventParam = eventParams;
        }
    }
}