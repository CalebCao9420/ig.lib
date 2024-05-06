using UnityEngine;

namespace IG.Editor.Helper{
    using UnityEditor;

    public static class EditorHelper{
        public static void VerticalPair(System.Action cb){
            EditorGUILayout.BeginVertical();
            cb?.Invoke();
            EditorGUILayout.EndVertical();
        }

        public static void HorizontalPair(System.Action cb){
            EditorGUILayout.BeginHorizontal();
            cb?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        public static Vector2 Scroll(Vector2 pos,System.Action cb,bool showHorizontal,bool showVertical){
            pos = GUILayout.BeginScrollView(pos, showHorizontal, showVertical);
            cb?.Invoke();
            GUILayout.EndScrollView();
            return pos;
        }
    }
}