using System;
using System.Collections.Generic;
using IG.Runtime.Utils;

namespace IG.Manager{
    public static class ManagerLauncher{
        private static List<IManager> _managers;

        public static void Launch(){
            _managers = new List<IManager>();
            List<Type> subClass = ADFUtils.GetImplementingTypes(typeof(IManager)) as List<Type>;
            int length = subClass?.Count ?? 0;
            for (int i = 0; i < length; ++i){
                var subMgr = Activator.CreateInstance(subClass[i], true);
                if (subMgr is IManager createIns){
                    createIns.Init();
                    _managers.Add(createIns);
                }
            }
        }
    }
}