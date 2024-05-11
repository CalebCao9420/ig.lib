namespace IG.Attribute{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class IGBCEventAttribute : System.Attribute{
        public LoopEventType EventType{ get; private set; }
        public IGBCEventAttribute(LoopEventType eventType){ EventType = eventType; }
    }
}