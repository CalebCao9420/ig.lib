namespace IG.Attribute{
    using System;

    //TODO:后边可以考虑 接入github的 DirectAttribute，搜索速度会快上很多，看下牺牲多少内存
    [AttributeUsage(AttributeTargets.Method)]
    public class IGBCEventAttribute : System.Attribute{
        public GameEventType EventType{ get; private set; }
        public IGBCEventAttribute(GameEventType eventType){ EventType = eventType; }
    }
}