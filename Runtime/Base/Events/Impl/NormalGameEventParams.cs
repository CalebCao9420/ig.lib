namespace IG.Events.Impl{
    public class StringEventParam : IGameEventParam{
        public string Content{ get; private set; }
        public StringEventParam(string ctx){ this.Content = ctx; }
    }

    public class TwoStringEventParam : IGameEventParam{
        public string Content1{ get; private set; }
        public string Content2{ get; private set; }

        public TwoStringEventParam(string ctx1, string ctx2){
            this.Content1 = ctx1;
            this.Content2 = ctx2;
        }
    }

    public class IntEventParam : IGameEventParam{
        public int Content{ get; private set; }
        public IntEventParam(int ctx){ this.Content = ctx; }
    }

    public class FloatEventParam : IGameEventParam{
        public float Content{ get; private set; }
        public FloatEventParam(float ctx){ this.Content = ctx; }
    }

    public class BoolEventParam : IGameEventParam{
        public bool Content{ get; private set; }
        public BoolEventParam(bool ctx){ this.Content = ctx; }
    }
}