using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

namespace IG.Editor.Helper{
    using UnityEditor;

    public static class EditorHelper{
        /// <summary>
        /// 纵向布局
        /// </summary>
        /// <param name="cb"></param>
        public static void VerticalPair(System.Action cb){
            EditorGUILayout.BeginVertical();
            cb?.Invoke();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 横向布局
        /// </summary>
        /// <param name="cb"></param>
        public static void HorizontalPair(System.Action cb){
            EditorGUILayout.BeginHorizontal();
            cb?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Scroll
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="cb"></param>
        /// <param name="showHorizontal"></param>
        /// <param name="showVertical"></param>
        /// <returns></returns>
        public static Vector2 Scroll(Vector2 pos, System.Action cb, bool showHorizontal, bool showVertical){
            pos = GUILayout.BeginScrollView(pos, showHorizontal, showVertical);
            cb?.Invoke();
            GUILayout.EndScrollView();
            return pos;
        }

        /// <summary>
        /// Dropdown 菜单
        /// </summary>
        /// <param name="label"></param>
        /// <param name="content"></param>
        /// <param name="check"></param>
        /// <param name="onSelect"></param>
        /// <param name="style"></param>
        /// <typeparam name="T"></typeparam>
        public static void DropdownMenu<T>(string label, T[] content, Func<T, bool> check, GenericMenu.MenuFunction2 onSelect, GUIStyle style){
            if (GUILayout.Button(label, style)){
                int         len  = content?.Length ?? 0;
                GenericMenu menu = null;
                if (len > 0){
                    menu = new GenericMenu();
                }

                for (int i = 0; i < len; ++i){
                    var        single     = content[i];
                    GUIContent guiContent = new GUIContent($"{single}");
                    menu.AddItem(guiContent, check.Invoke(single), onSelect, single);
                }

                menu.ShowAsContext();
            }
        }

        /// <summary>
        /// Dropdown 菜单
        /// </summary>
        /// <param name="label"></param>
        /// <param name="content"></param>
        /// <param name="check"></param>
        /// <param name="onSelect"></param>
        /// <param name="style"></param>
        /// <typeparam name="T"></typeparam>
        public static void DropdownMenu<T>(string label, IEnumerable<T> content, Func<T, bool> check, GenericMenu.MenuFunction2 onSelect, GUIStyle style){
            if (GUILayout.Button(label, style)){
                GenericMenu menu = new GenericMenu();
                foreach (var t in content){
                    var        single     = t;
                    GUIContent guiContent = new GUIContent($"{single}");
                    menu.AddItem(guiContent, check.Invoke(single), onSelect, single);
                }

                menu.ShowAsContext();
            }
        }

        /// <summary>
        /// Dropdown 菜单
        /// </summary>
        /// <param name="selectElement"></param>
        /// <param name="content"></param>
        /// <param name="onClick"></param>
        /// <param name="width"></param>
        /// <typeparam name="T"></typeparam>
        public static void DropdownMenu<T>(T selectElement, IEnumerable<T> content, Action<T> onClick) where T : class{
            bool       isOpenInList = null != selectElement;
            string     openTypeName = isOpenInList ? selectElement.ToString() : string.Empty;
            GUIContent menuGui      = new GUIContent(openTypeName);
            if (GUILayout.Button(menuGui, EditorStyles.toolbarDropDown)){
                var menu = new GenericMenu();
                foreach (var t in content){
                    var single = t;

                    void OnItemClick(){
                        onClick?.Invoke(single);
                        selectElement = single;
                    }

                    GUIContent guiContent = new GUIContent($"{single}");
                    menu.AddItem(guiContent, isOpenInList && selectElement == single, OnItemClick);
                }

                menu.ShowAsContext();
            }
        }
        
        /// <summary>
        /// Dropdown 菜单
        /// </summary>
        /// <param name="selectKey"></param>
        /// <param name="dict"></param>
        /// <param name="onClick"></param>
        /// <param name="width"></param>
        /// <param name="displayKey"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        public static void DropDown<T, V>(T selectKey,  IDictionary<T, V> dict, Action<KeyValuePair<T, V>> onClick, float width = 120, bool displayKey = false){
            bool       isOpenInList = dict.TryGetValue(selectKey, out V value);
            string     openTypeName = isOpenInList ? (displayKey ? selectKey.ToString() : value.ToString()) : string.Empty;
            GUIContent guiContent   = new GUIContent(openTypeName);
            if (GUILayout.Button(guiContent, EditorStyles.toolbarDropDown, GUILayout.Width(width))){
                var menu = new GenericMenu();
                foreach (var keyValuePair in dict){
                    string keyStr = keyValuePair.Key.ToString();
                    string valueStr = keyValuePair.Value.ToString();
                    string curKeyStr = displayKey ? keyStr : valueStr;

                    void OnItemClick(){
                        onClick?.Invoke(keyValuePair);
                        selectKey = keyValuePair.Key;
                    }

                    menu.AddItem(new GUIContent(curKeyStr), isOpenInList && selectKey.Equals(keyStr), OnItemClick);
                }

                menu.ShowAsContext();
            }
        }

        public static string HasTitleSelectFolderPathHorizontal(string title, string content, float width = 100.0f, float height = 100f){
            string rel = string.Empty;
            EditorGUILayout.LabelField(title, GUILayout.Width(width), GUILayout.Height(height));
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
#endif