using System;
using System.Collections.Generic;
using System.Reflection;
using IG.Runtime.Utils;

namespace IG.Manager{
    public static class ManagerLauncher{
        private static List<IManager> _managers;

        private static IManager ExecuteInstance(Type type){
            IManager rel     = null;
            // 调用子类的 Instance 属性来触发单例实例的创建
            var instanceProperty = type.GetProperty(
                                                    "Instance",
                                                    BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy
                                                   );
            if (instanceProperty != null){
                var method = instanceProperty.GetGetMethod();
                rel = method?.Invoke(null, null) as IManager;
            }

            return rel;
        }

        public static void Launch(){
            _managers = new List<IManager>();
            List<Type> subClass = ADFUtils.GetImplementingTypes(typeof(IManager)) as List<Type>;
            ADFUtils.SortByPriority(subClass);
            int length = subClass?.Count ?? 0;
            for (int i = 0; i < length; ++i){
                var hook = ExecuteInstance(subClass[i]);
                _managers.Add(hook);
            }
        }
    }
}