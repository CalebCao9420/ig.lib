using System;
using System.Collections.Generic;

namespace IG.Runtime.Utils{
    public static class ADFUtils{
        /// <summary>
        /// C#获取一个类在其所在的程序集中的所有子类
        /// </summary>
        /// <param name="parentType">给定的类型</param>
        /// <returns>所有子类的名称</returns>
        public static List<Type> GetSubClass(Type parentType,bool containsAbstract = false){
            var subTypeList = new List<Type>();
            //获取当前父类所在的程序集``
            var assembly = parentType.Assembly;
            //获取该程序集中的所有类型
            var assemblyAllTypes = assembly.GetTypes();

            //Old equals name
            // foreach (var itemType in assemblyAllTypes){
            //     var baseType = itemType.BaseType;
            //     //如果有基类
            //     if (baseType != null){
            //         if (baseType.Name.Equals(parentType.Name)){
            //             subTypeList.Add(itemType);
            //         }
            //     }
            // }
            //New judge subclassof
            foreach (var itemType in assemblyAllTypes){
                if (itemType != parentType && itemType.IsSubclassOf(parentType)){
                    if (!containsAbstract && itemType.IsAbstract){
                        continue;
                    }
                    
                    subTypeList.Add(itemType);
                }
            }

            return subTypeList;
        }

        /// <summary>
        /// C#获取一个类在所有程序集中的所有子类
        /// </summary>
        /// <param name="parentType">给定的类型</param>
        /// <returns>所有子类的名称</returns>
        public static List<Type> GetAllSubClass(Type parentType,bool containsAbstract){
            var subTypeList = new List<Type>();

            // 获取所有加载的程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies){
                var assemblyAllTypes = assembly.GetTypes();
                foreach (var itemType in assemblyAllTypes){
                    if (itemType != parentType && itemType.IsSubclassOf(parentType)){
                        if (!containsAbstract && itemType.IsAbstract){
                            continue;
                        }
                        
                        subTypeList.Add(itemType);
                    }
                }
            }

            return subTypeList;
        }

        // 获取所有继承自指定接口的类型
        public static IEnumerable<Type> GetImplementingTypes(Type interfaceType,bool containsAbstract = false){
            var subTypeList = new List<Type>();
            // 获取所有程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies){
                // 获取程序集中的所有类型
                Type[] types = assembly.GetTypes();
                foreach (Type type in types){
                    // 检查类型是否实现了目标接口
                    if (interfaceType.IsAssignableFrom(type) && !type.IsInterface){
                        if (!containsAbstract && type.IsAbstract){
                            continue;
                        }
                        subTypeList.Add(type);
                    }
                }
            }

            return subTypeList;
        }
    }
}