using UnityEngine;

namespace IG.Runtime.Extensions{
    public class NotDestroy : MonoBehaviour{
        void Start(){ DontDestroyOnLoad(this); }
    }
}