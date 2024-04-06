using System;
using System.Collections.Generic;

namespace IG.Runtime.Utils{
    public static class ADFUtils{
        /// <summary>
        /// C#获取一个类在其所在的程序集中的所有子类
        /// </summary>
        /// <param name="parentType">给定的类型</param>
        /// <returns>所有子类的名称</returns>
        public static List<Type> GetSubClass(Type parentType){
            var subTypeList = new List<Type>();
            //获取当前父类所在的程序集``
            var assembly = parentType.Assembly;
            //获取该程序集中的所有类型
            var assemblyAllTypes = assembly.GetTypes();
            foreach (var itemType in assemblyAllTypes){
                var baseType = itemType.BaseType;
                //如果有基类
                if (baseType != null){
                    if (baseType.Name.Equals(parentType.Name)){
                        subTypeList.Add(itemType);
                    }
                }
            }

            return subTypeList;
        }
    }
}