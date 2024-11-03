#if UNITY_EDITOR
using System.Collections.Generic;
using IG.Editor.Helper;
using IG.Runtime.Extensions;
using IG.Runtime.Utils;

namespace IG.Module.Editor{
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.UIElements;

    [InitializeOnLoad]
    [HelpURL("https://www.cnblogs.com/hont/p/15968054.html")]
    [Tooltip("摘抄的人家的拓展，可以单独抽象接口出来按接口实现，不过我认为没有必要在这个轮上浪费功夫(version:2021.x)")]
    public static class CustomToolbar{
        private static readonly Type                   s_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static          ScriptableObject       s_currentToolbar;
        private static          List<IToolbarFunction> s_rightFunctions;
        private static          List<IToolbarFunction> s_leftFunctions;
        static CustomToolbar(){ EditorApplication.update += OnUpdate; }

        private static void OnUpdate(){
            if (s_currentToolbar == null){
                InitToolbarFunc();
                UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(s_toolbarType);
                s_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (s_currentToolbar != null){
                    FieldInfo     root         = s_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    VisualElement concreteRoot = root.GetValue(s_currentToolbar) as VisualElement;
                    //Right
                    VisualElement  toolbarRight   = concreteRoot.Q("ToolbarZoneRightAlign");
                    VisualElement  parentRight    = new VisualElement(){ style ={ flexGrow = 1, flexDirection = FlexDirection.RowReverse, } };
                    IMGUIContainer containerRight = new IMGUIContainer(OnGuiRightBody);
                    parentRight.Add(containerRight);
                    toolbarRight.Add(parentRight);

                    //Left
                    VisualElement  toolbarLeft   = concreteRoot.Q("ToolbarZoneLeftAlign");
                    VisualElement  parentLeft    = new VisualElement(){ style ={ flexGrow = 1, flexDirection = FlexDirection.Row, } };
                    IMGUIContainer containerLeft = new IMGUIContainer(OnGuiLeftBody);
                    parentLeft.Add(containerLeft);
                    toolbarLeft.Add(parentLeft);
                }
            }
        }

        private static void InitToolbarFunc(){
            s_rightFunctions = InitFunctionArray(typeof(IToolbarRightFunction));
            s_leftFunctions  = InitFunctionArray(typeof(IToolbarLeftFunction));
        }

        private static List<IToolbarFunction> InitFunctionArray(Type type){
            List<System.Type>      types = ADFUtils.GetImplementingTypes(type) as List<System.Type>;
            List<IToolbarFunction> array = new();

            int InternalSort(Type x, Type y){
                var xPri = x.GetCustomAttribute<PriorityAttribute>();
                var yPri = y.GetCustomAttribute<PriorityAttribute>();
                return xPri.Priority - yPri.Priority;
            }

            types.Sort(InternalSort);
            void OnCheck(Type t){ array.Add(Activator.CreateInstance(t) as IToolbarFunction); }
            types.Ergodic(OnCheck);
            return array;
        }

        private static void OnGuiLeftBody() { EditorHelper.HorizontalPair(DrawGuiLeft); }
        private static void OnGuiRightBody(){ EditorHelper.HorizontalPair(DrawGuiRight); }

        private static void DrawGuiRight(){
            if (s_rightFunctions == null){
                return;
            }

            s_rightFunctions.Ergodic(t => t.DrawTool());
        }

        private static void DrawGuiLeft(){
            if (s_leftFunctions == null){
                return;
            }

            s_leftFunctions.Ergodic(t => t.DrawTool());
        }
    }

    public interface IToolbarFunction{
        void DrawTool();
    }

    public interface IToolbarRightFunction : IToolbarFunction{
    }

    public interface IToolbarLeftFunction : IToolbarFunction{
    }
}
#endif