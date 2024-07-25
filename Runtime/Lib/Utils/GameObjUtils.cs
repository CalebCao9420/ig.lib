using System.Collections.Generic;
using UnityEngine;

namespace IG.Runtime.Utils{
    using Extensions;

    /// <summary>
    /// 考虑一下 是否需要存一下引用
    /// </summary>
    public static class GameObjUtils{
        public static void TransformZero(this GameObject gameObject,bool identityRotate = true){
            Transform tr = gameObject.transform;
            TransformZero(tr,identityRotate);
        }

        public static void TransformZero(this Transform tr,bool identityRotate = true){
            tr.localPosition = Vector3.zero;
            tr.localScale = Vector3.one;
            if (identityRotate){
                tr.localRotation = Quaternion.identity;
            }
        }

        public static GameObject CreateGameObject(string name = "", bool isStatic = false){
            GameObject obj = new GameObject(name);
            TransformZero(obj,false);
            obj.isStatic                = isStatic;
            return obj;
        }

        public static RectTransform CreateGameObject(this RectTransform parent, string name = "", bool isStatic = false){
            GameObject obj       = new GameObject(name);
            var        rectTrans = obj.GetOrAddComponent<RectTransform>();
            rectTrans.localScale = Vector3.one;
            rectTrans.position   = parent.position;
            rectTrans.SetParent(parent);
            obj.isStatic = isStatic;
            return rectTrans;
        }

        public static GameObject CreateGameObject(this Transform parent, string name = "", bool isStatic = false){
            GameObject obj = new GameObject(name);
            obj.transform.localScale = Vector3.one;
            obj.transform.position   = parent.position;
            obj.transform.SetParent(parent);
            obj.isStatic = isStatic;
            return obj;
        }

        public static GameObject CreateGameObject<T>(this Transform parent, string name = "", bool isStatic = false)
            where T : Component{
            GameObject obj = new GameObject(name);
            _                        = obj.GetOrAddComponent<T>();
            obj.transform.localScale = Vector3.one;
            obj.transform.position   = parent.position;
            obj.transform.SetParent(parent);
            obj.isStatic = isStatic;
            return obj;
        }

        public static GameObject CreateGameObject<T>(string name = "", bool isStatic = false) where T : Component{
            GameObject result = new GameObject(name);
            TransformZero(result);
            result.isStatic                = isStatic;
            _                              = result.GetOrAddComponent<T>();
            return result;
        }

        public static T CreateGameObjectAndComponent<T>(string name = "", UnityEngine.Transform parent = null, bool isStatic = false)
            where T : Component{
            GameObject obj = new GameObject(name);
            TransformZero(obj);
            obj.transform.SetParent(parent);
            obj.isStatic = isStatic;
            // T result = obj.GetOrAddComponent<T>();
            T result = obj.GetComponent<T>();
            if (result == null){
                result = obj.AddComponent<T>();
            }

            return result;
        }

        /// <summary>
        /// 简单遍历
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="onCheck">检测到时</param>
        public static void Ergodic(this Transform parent, System.Action<Transform> onCheck){
            int childrenCount = parent?.childCount ?? 0;
            for (int i = 0; i < childrenCount; i++){
                Transform tr = parent.GetChild(i);
                onCheck?.Invoke(tr);
            }
        }

        public static void DestroyParent(this GameObject parent, bool detachChildren = false){
            if (detachChildren){
                parent.transform.DetachChildren();
            }

            DestroyObj(parent);
        }

        public static void DestroyKids(this GameObject parent){
            int childCount = parent.transform?.childCount ?? 0;
            for (int i = 0; i < childCount; i++){
                DestroyObj(parent.transform.GetChild(i));
            }
        }

        public static void DestroyKids(this Transform parent){
            foreach (Transform child in parent){
                DestroyObj(child);
            }
        }

        public static void DestroyObj(GameObject obj){
            if (Application.isPlaying){
                GameObject.Destroy(obj);
            }
            else{
                GameObject.DestroyImmediate(obj);
            }
        }

        public static void DestroyObj(Transform obj){
            if (Application.isPlaying){
                GameObject.Destroy(obj.gameObject);
            }
            else{
                GameObject.DestroyImmediate(obj.gameObject);
            }
        }

        public static void DestroyObj<T>(T obj) where T : Component{
            if (Application.isPlaying){
                GameObject.Destroy(obj.gameObject);
            }
            else{
                GameObject.DestroyImmediate(obj.gameObject);
            }
        }

        public enum Pivot{
            MidCenter,
            MidLeft,
            MidRight,
            UpLeft,
            UpCenter,
            UpRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        /// <summary>
        /// 给Obj创建碰撞
        /// TODO:2022/06/05想想怎么做这个bound的描边，描了边，加plolygon即可
        /// TODO:要么还是多循环读取像素点算了
        /// </summary>
        public static void GenerateCollider(
            ref List<Vector2> edge,
            SpriteRenderer    body,
            Vector2           centerPos,
            float             unit,
            float             tolerance = 0,
            bool              isTrigger = false,
            Pivot             pivot     = Pivot.MidCenter){
            Sprite sprite = body.sprite;
            GetSpriteEdge(ref edge, sprite, centerPos, unit, tolerance, pivot);
            Vector2[]         edgeArr    = edge.ToArray();
            PolygonCollider2D collider2D = body.GetOrAddComponent<PolygonCollider2D>();
            collider2D.points = edgeArr;
            // collider2D.offset = ;
            collider2D.isTrigger = isTrigger;
        }

        /// <summary>
        /// 获取Sprite 边缘 
        /// </summary>
        public static List<Vector2> GetSpriteEdge(
            ref List<Vector2> edge,
            Sprite            sprite,
            Vector2           centerPos,
            float             unit,
            float             tolerance = 0f,
            Pivot             pivot     = Pivot.MidCenter){
            Texture2D tex = sprite.texture;
            if (!tex.isReadable){
#if UNITY_EDITOR
                UnityEditor.TextureImporter ti = (UnityEditor.TextureImporter)UnityEditor.TextureImporter.GetAtPath(
                                                                                                                    UnityEditor.AssetDatabase.GetAssetPath(tex)
                                                                                                                   );
                ti.isReadable = true;
                UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(tex));
#endif
            }

            int minX = Mathf.RoundToInt(sprite.rect.xMin);
            int minY = Mathf.RoundToInt(sprite.rect.yMin);
            int maxX = Mathf.RoundToInt(sprite.rect.width);
            int maxY = Mathf.RoundToInt(sprite.rect.height);

            //Get pivot offset
            Vector2 offset = new Vector2(sprite.rect.width, sprite.rect.height) / unit;
            switch (pivot){
                case Pivot.BottomLeft:
                    // offset = Vector2.zero;
                    break;
                case Pivot.BottomCenter:
                    offset.x *= 0.5f;
                    break;
                case Pivot.BottomRight:
                    offset.x = 0;
                    break;
                case Pivot.MidLeft:
                    offset.y *= 0.5f;
                    break;
                case Pivot.MidCenter:
                    offset *= 0.5f;
                    break;
                case Pivot.MidRight:
                    offset.x =  0;
                    offset.y *= 0.5f;
                    break;
                case Pivot.UpLeft:
                    // offset.x = 0;
                    offset.y = 0;
                    break;
                case Pivot.UpCenter:
                    offset.x *= 0.5f;
                    offset.y =  0;
                    break;
                case Pivot.UpRight:
                    offset = Vector2.zero;
                    break;
            }

            //先拿到所有color
            // Color[] pixels = tex.GetPixels(minX, minY, maxX, maxY);
            Vector2 center     = new Vector2();
            Vector2 tempAround = new Vector2();
            int     inner      = 0, outer = 0, length = 0;
            //alpha 小于等于0 数量>3 和alpha 大于0 数量>3 直接判断为边缘
            for (int x = minX; x < maxX; ++x){
                for (int y = minY; y < maxY; ++y){
                    //本身就是透明像素跳过
                    if (tex.GetPixel(x, y).a <= 0.0001f){
                        continue;
                    }

                    center.x = x;
                    center.y = y;
                    List<Vector2> around = VectorUtils.GetRectangleAround(center, 1, 1);
                    length = around != null ? around.Count : 0;
                    inner  = outer = 0;
                    for (int i = 0; i < length; ++i){
                        tempAround = around[i];
                        if (tempAround.x < minX || tempAround.x >= maxX || tempAround.y < minY || tempAround.y >= maxY){
                            outer++;
                            continue;
                        }

                        Color temp = tex.GetPixel((int)tempAround.x, (int)tempAround.y);
                        if (temp.a <= 0.0001f){
                            outer++;
                        }
                        else{
                            inner++;
                        }

                        //节约性能考虑
                        Vector2 newPos = (center + centerPos) / unit;
                        if (outer > 0 && inner > 0 && !edge.Contains(newPos)){
                            edge.Add(newPos - offset);
                            break;
                        }
                    }

                    // if (tex.GetPixel(tempAround.X, tempAround.Y).a <= 0.01f && tex.GetPixel(x, y).a > 0.01f){
                    //     edge.Add(tempAround);
                    // }

                    // tempAround = center;
                }
            }

            // VectorUtils.Reorder(ref edge);
            VectorUtils.OptimizationShape(ref edge, tolerance);
            VectorUtils.Reorder(ref edge);
            return edge;
        }
    }
}