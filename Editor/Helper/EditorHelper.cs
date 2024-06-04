using System;
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

        public static Vector2 Scroll(Vector2 pos, System.Action cb, bool showHorizontal, bool showVertical){
            pos = GUILayout.BeginScrollView(pos, showHorizontal, showVertical);
            cb?.Invoke();
            GUILayout.EndScrollView();
            return pos;
        }

        public static string HasTitleSelectFolderPathHorizontal(string title, string content, float width = 100.0f, float height = 100f){
            string rel = string.Empty;
            HorizontalPair(() => rel = HasTitleSelectFolderPath(title, content, width, height));
            return rel;
        }

        public static string HasTitleSelectFolderPath(string title, string content, float width = 100.0f, float height = 100f){
            if (GUILayout.Button("[+]")){
                content = BrowseForFolder(title, true);
            }

            return EditorGUILayout.TextArea(content, GUILayout.Width(width), GUILayout.Height(height));
        }

        public static string HasTitleField(string title, string content, float width = 100.0f, float height = 100f){
            EditorGUILayout.LabelField(title, GUILayout.Width(width), GUILayout.Height(height));
            return EditorGUILayout.TextArea(content, GUILayout.Width(width), GUILayout.Height(height));
        }

        public static int HasTitleField(string title, int content, float width = 100.0f, float height = 100f){
            EditorGUILayout.LabelField(title, GUILayout.Width(width), GUILayout.Height(height));
            return EditorGUILayout.IntField(content, GUILayout.Width(width), GUILayout.Height(height));
        }

        public static Enum HasTitleField(string title, string subLabel, Enum content){
            EditorGUILayout.LabelField(title);
            return EditorGUILayout.EnumPopup(subLabel, content);
        }

        /// <summary>
        /// 打开Windows Folder选择文件夹路径
        /// </summary>
        /// <param name="title"></param>
        /// <param name="isRelative">是否是相对路径</param>
        /// <returns></returns>
        public static string BrowseForFolder(string title, bool isRelative = false){
            var newPath = EditorUtility.OpenFolderPanel(title, "", string.Empty);
            if (!string.IsNullOrEmpty(newPath) && isRelative){
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length){ newPath = newPath.Remove(0, gamePath.Length + 1); }
            }

            return newPath;
        }
    }
}