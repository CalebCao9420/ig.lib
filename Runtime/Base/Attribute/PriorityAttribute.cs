namespace IG{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class PriorityAttribute : System.Attribute{
        public int Priority{ get; private set; }
        public PriorityAttribute(int priority){ this.Priority = priority; }
    }
}