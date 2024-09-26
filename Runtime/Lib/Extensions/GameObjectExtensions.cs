using UnityEngine.UI;

namespace IG.Runtime.Extensions{
    using UnityEngine;

    public static class GameObjectExtensions{
        /// <summary>
        /// Rects the transform.
        /// </summary>
        /// <returns>The transform.</returns>
        /// <param name="obj">Object.</param>
        public static RectTransform RectTransform(this MaskableGraphic obj){ return obj.transform as RectTransform; }

        /// <summary>
        /// Rects the transform.
        /// </summary>
        /// <returns>The transform.</returns>
        /// <param name="obj">Object.</param>
        public static RectTransform RectTransform(this MonoBehaviour obj){ return obj.transform as RectTransform; }

        /// <summary>
        /// Gets the or add component.
        /// </summary>
        /// <returns>The or add component.</returns>
        /// <param name="go">Go.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component{
            T t = go.GetComponent<T>();
            if (t == null){
                t = go.AddComponent<T>();
            }

            return t;
        }

        /// <summary>
        /// Gets the or add component.
        /// </summary>
        /// <returns>The or add component.</returns>
        /// <param name="component">Component.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T GetOrAddComponent<T>(this Component component) where T : Component{
            T t = component.gameObject.GetComponent<T>();
            if (t == null){
                t = component.gameObject.AddComponent<T>();
            }

            return t;
        }
    }
}