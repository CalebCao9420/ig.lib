using System;

namespace IG{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class AfterActionAttribute : System.Attribute{
        public Action Content{ get; private set; }
        public AfterActionAttribute(Action content){ this.Content = content; }
    }
}