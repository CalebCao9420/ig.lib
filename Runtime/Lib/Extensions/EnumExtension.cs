using System;

namespace IG.Runtime.Extensions{
    public static class EnumExtension{
        public static T GetByIndex<T>(this     T      en,   int index) where T : Enum{ return (T)Enum.ToObject(en.GetType(), index); }
        public static T GetEnumByIndex<T>(this Type   type, int index) where T : Enum{ return (T)Enum.ToObject(type,         index); }
        public static T ToEnum<T>(this         string name) where T : System.Enum{ return (T)Enum.Parse(typeof(T), name); }
    }
}