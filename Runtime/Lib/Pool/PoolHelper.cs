using IG.Pool.SubPool;

namespace IG.Pool{
    public static class PoolHelper{
        public static ISubPool CreateSubPool(PoolResourceType resType,string path){
            switch (resType){
                case PoolResourceType.Audio: return new AudioSubPool(resType, path);
                case PoolResourceType.Texture:
                    break;
                case PoolResourceType.GameObject: return new GameObjectSubPool(resType, path);
            }

            return default;
        }
    }
}