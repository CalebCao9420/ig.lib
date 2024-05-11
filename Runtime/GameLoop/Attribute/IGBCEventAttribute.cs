namespace IG.Attribute{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class IGBCEventAttribute : System.Attribute{
        public GameEventType EventType{ get; private set; }
        public IGBCEventAttribute(GameEventType eventType){ EventType = eventType; }
    }
}