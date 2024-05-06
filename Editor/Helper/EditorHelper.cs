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
    }
}