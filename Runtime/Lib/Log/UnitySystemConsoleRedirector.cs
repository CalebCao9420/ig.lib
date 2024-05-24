using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace IG.Runtime.Log{
    /// <summary>
    /// Redirects writes to System.Console to Unity3D's Debug.Log.
    /// </summary>
    /// <author>
    /// Jackson Dunstan, http://jacksondunstan.com/articles/2986
    /// </author>
    public static class UnitySystemConsoleRedirector{
        private static TextWriter _defaultTextWrite = null;

        private class UnityTextWriter : TextWriter{
            private StringBuilder buffer = new StringBuilder();

            public override void Flush(){
                Debug.Log(buffer.ToString());
                buffer.Length = 0;
            }

            public override void Write(string value){
                buffer.Append(value);
                if (value != null){
                    var len = value.Length;
                    if (len > 0){
                        var lastChar = value[len - 1];
                        if (lastChar == '\n'){
                            Flush();
                        }
                    }
                }
            }

            public override void Write(char value){
                buffer.Append(value);
                if (value == '\n'){
                    Flush();
                }
            }

            public override void     Write(char[] value, int index, int count){ Write(new string(value, index, count)); }
            public override Encoding Encoding{ get{ return Encoding.Default; } }
        }

        public static void Redirect(){
#if UNITY_2022_2_OR_NEWER
            Restore();
#endif
            if (_defaultTextWrite == null){
                _defaultTextWrite = Console.Out;
                Console.SetOut(new UnityTextWriter());
            }
        }

#if UNITY_2022_2_OR_NEWER
        /// <summary>
        /// If you using Unity 2022.2 above, You must call this before script compilation
        /// Otherwise, you may encounter an issue where script compilation never finishes.
        /// </summary>
        public static void Restore(){
            if (_defaultTextWrite != null){
                Console.SetOut(_defaultTextWrite);
            }
        }
#endif
    }
}