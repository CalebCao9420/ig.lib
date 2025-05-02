using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IG.Runtime.Extensions;

namespace IG.Runtime.Utils{
    public static class ADFUtils{
        /// <summary>
        /// C#获取一个类在其所在的程序集中的所有子类
        /// </summary>
        /// <param name="parentType">给定的类型</param>
        /// <returns>所有子类的名称</returns>
        public static List<Type> GetSubClass(Type parentType, bool containsAbstract = false){
            var subTypeList = new List<Type>();
            //获取当前父类所在的程序集``
            var assembly = parentType.Assembly;
            //获取该程序集中的所有类型
            var assemblyAllTypes = assembly.GetTypes();
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
        public static List<Type> GetAllSubClass(Type parentType, bool containsAbstract){
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
        public static IEnumerable<Type> GetImplementingTypes(Type interfaceType, bool containsAbstract = false){
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

        /// <summary>
        /// 获取所有有T特性的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetAllTypesByAttribute<T>() where T : System.Attribute{
            var res = new List<Type>();
            res = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttributes(typeof(T), false).Any()).ToList();
            return res;
        }

        /// <summary>
        /// 获取所有有T特性的方法
        /// 仅Static
        /// 消耗较大,建议仅Editor使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<MethodInfo> GetAllMethodByAttribute<T>() where T : System.Attribute{
            var res = new List<MethodInfo>();

            void OnCheck(Type t){
                var methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var mts     = methods.Where(t => t.GetCustomAttributes(typeof(T)).Any());
                res.AddRange(mts);
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            assemblies.Ergodic(t => t.GetTypes().Ergodic(OnCheck));
            return res;
        }

        /// <summary>
        /// 通过限定类型Attribute获取改类带有TM Attribute的方法,已经该方法的Attribute
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        public static List<(MethodInfo, TM)> GetAllMethodByAttribute<TC, TM>() where TC : System.Attribute where TM : System.Attribute{
            var rel = new List<(MethodInfo, TM)>();
            // 获取所有程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies){
                // 获取程序集中的所有类型
                Type[] types = assembly.GetTypes();
                foreach (Type type in types){
                    if (type.IsInterface || type.IsAbstract){ continue; }

                    var attributes = GetAttribute<TC>(type, false);
                    if (attributes == null){
                        continue;
                    }

                    MethodInfo[] methods = type.GetMethods();
                    foreach (var sm in methods){
                        var afs = GetAttribute<TM>(sm, false);
                        if (afs == null){
                            continue;
                        }

                        rel.Add((sm, afs));
                    }
                }
            }

            return rel;
        }

        /// <summary>
        /// 通过限定类型Attribute获取改类带有TM Attribute的方法,已经该方法的Attribute
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <typeparam name="TM"></typeparam>
        /// <returns></returns>
        public static List<(MethodInfo, TM)> GetAllMethodByAttribute<TC, TM>(BindingFlags functionFlags)
            where TC : System.Attribute where TM : System.Attribute{
            var rel = new List<(MethodInfo, TM)>();
            // 获取所有程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies){
                // 获取程序集中的所有类型
                Type[] types = assembly.GetTypes();
                foreach (Type type in types){
                    if (type.IsInterface || type.IsAbstract){ continue; }

                    var attributes = GetAttribute<TC>(type, false);
                    if (attributes == null){
                        continue;
                    }

                    MethodInfo[] methods = type.GetMethods(functionFlags);
                    foreach (var sm in methods){
                        var afs = GetAttribute<TM>(sm, false);
                        if (afs == null){
                            continue;
                        }

                        rel.Add((sm, afs));
                    }
                }
            }

            return rel;
        }

        /// <summary>
        /// 获取属性，通过方法限定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(MethodInfo method, bool inherit = false){
            var attributeType = typeof(T);
            var afs           = method.GetCustomAttributes(attributeType, inherit);
            var att           = afs?.Length > 0 ? afs[0] : null;
            if (att == null){ return default; }

            return (T)att;
        }

        /// <summary>
        /// 获取属性，通过类型限定
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inherit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttribute<T>(Type type, bool inherit = false){
            var attributeType = typeof(T);
            var afs           = type.GetCustomAttributes(attributeType, inherit);
            var att           = afs?.Length > 0 ? afs[0] : null;
            if (att == null){ return default; }

            return (T)att;
        }

    #region Sort custom attribute

        public enum SortType{
            Order   = 1,  //顺序
            Reverse = -1, //倒序
        }

        public static void SortByPriority(List<Type> classes, SortType sortType = SortType.Order){
            classes.Sort((x, y) => {
                             var xPri = x.GetCustomAttribute(typeof(PriorityAttribute)) as PriorityAttribute;
                             var yPri = y.GetCustomAttribute(typeof(PriorityAttribute)) as PriorityAttribute;
                             if (xPri == null){
                                 return 1 * (int)sortType;
                             }

                             if (yPri == null){
                                 return -1 * (int)sortType;
                             }

                             return (xPri.Priority - yPri.Priority) * (int)sortType;
                         }
                        );
        }

    #endregion
    }
}