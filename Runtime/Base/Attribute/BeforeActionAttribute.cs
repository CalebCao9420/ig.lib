using System;

namespace IG{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method, Inherited = true)]
    public class BeforeActionAttribute : System.Attribute{
        public Action Content{ get; private set; }
        public BeforeActionAttribute(Action content){ this.Content = content; }
    }
}