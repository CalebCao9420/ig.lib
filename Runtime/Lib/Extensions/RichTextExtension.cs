namespace IG.Runtime.Extensions{
    public static class RichTextExtension{
        public static string SystemColor(this     string self, string _color){ return string.Format("<color={0}>{1}</color>", _color, self); }
        public static string HEXColor(this        string self, string _hex)  { return SystemColor(self, "#" + _hex); }
        public static string RedColor(this        string self)           { return self.HEXColor("ff0000"); }
        public static string AliceblueColor(this  string self)           { return self.HEXColor("f0f8ff"); }
        public static string AquaColor(this       string self)           { return self.HEXColor("00ffff"); }
        public static string AquaMarineColor(this string self)           { return self.HEXColor("7fffd4"); }
        public static string BlueColor(this       string self)           { return self.HEXColor("#0000FF"); }
        public static string YellowColor(this     string self)           { return SystemColor(self, "yellow"); }
        public static string GreenColor(this      string self)           { return SystemColor(self, "green"); }
        public static string BlackColor(this      string self)           { return SystemColor(self, "black"); }
        public static string BrownColor(this      string self)           { return SystemColor(self, "brown"); }
        public static string DarkblueColor(this   string self)           { return SystemColor(self, "darkblue"); }
        public static string GreyColor(this       string self)           { return SystemColor(self, "grey"); }
        public static string FuchsiaColor(this    string self)           { return SystemColor(self, "fuchsia"); }
        public static string LightblueColor(this  string self)           { return SystemColor(self, "lightblue"); }
        public static string LimeColor(this       string self)           { return SystemColor(self, "lime"); }
        public static string MagentaColor(this    string self)           { return SystemColor(self, "magenta"); }
        public static string MaroonColor(this     string self)           { return SystemColor(self, "maroon"); }
        public static string NavyColor(this       string self)           { return SystemColor(self, "navy"); }
        public static string OliveColor(this      string self)           { return SystemColor(self, "olive"); }
        public static string OrangeColor(this     string self)           { return SystemColor(self, "orange"); }
        public static string PurpleColor(this     string self)           { return SystemColor(self, "purple"); }
        public static string SilverColor(this     string self)           { return SystemColor(self, "silver"); }
        public static string TealColor(this       string self)           { return SystemColor(self, "teal"); }
        public static string WhiteColor(this      string self)           { return SystemColor(self, "white"); }
        public static string FontSize(this        string self, int _size){ return string.Format("<size={0}>{1}</size>", _size, self); }
    }
}