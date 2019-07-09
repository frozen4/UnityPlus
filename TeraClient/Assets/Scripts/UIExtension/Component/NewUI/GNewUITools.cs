using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public struct GNewUITools
{
    //public enum SortingLayerID
    //{
    //    //-- BottomMost
    //    GameWorld = 1,
    //    //-- Bottom
    //    RootPanel = 2,
    //    //-- Normal
    //    SubPanel = 3,
    //    //-- Upper
    //    Dialog = 4,
    //    //-- Top
    //    NormalTip = 5,
    //    //-- Guide
    //    Guide = 6,
    //    //-- Topmost
    //    ImportantTip = 7,
    //    //-- PowerSaving
    //    PowerSaving = 8,
    //    //-- Debug
    //    Debug = 9
    //}

    //Related to UI_Sorting_Layer in ClientDef.lua
    public static int Num2SortingLayerID(int layer)
    {
        SortingLayer[] sl_array = SortingLayer.layers;
        if (sl_array.Length > layer && layer > 0)
        {
            //string s = SortingLayer.IDToName(sl_array[layer].id);
            return sl_array[layer].id;
        }

        return 0;
    }

    public static void SetVisible(Transform t, bool flag)
    {
        t.localScale = flag ? Vector3.one : Vector3.zero;
    }

    public static Vector2 GetDeltaSize(RectTransform rt, Vector2 size)
    {
        if (rt)
        {
            RectTransform pt = rt.parent as RectTransform;
            if (pt)
            {
                Vector2 v_size = pt.rect.size;
                Vector2 v2_min = Vector2.Scale(v_size, rt.anchorMax - rt.anchorMin);
                return size - v2_min;
            }
        }
        return Vector2.zero;
    }

    public static Vector2 GetAlignedPivot(NewAlign align)
    {
        Vector2 pivot = Vector2.zero;
        switch (align)
        {
            case NewAlign.Left:
                pivot = new Vector2(0f, 0.5f);
                break;
            case NewAlign.Right:
                pivot = new Vector2(1f, 0.5f);
                break;
            case NewAlign.Top:
                pivot = new Vector2(0.5f, 1f);
                break;
            case NewAlign.Bottom:
                pivot = new Vector2(0.5f, 0f);
                break;
            case NewAlign.Center:
                pivot = new Vector2(0.5f, 0.5f);
                break;
            case NewAlign.TopLeft:
                pivot = new Vector2(0f, 1f);
                break;
            case NewAlign.TopRight:
                pivot = new Vector2(1f, 1f);
                break;
            case NewAlign.BottomLeft:
                pivot = new Vector2(0f, 0f);
                break;
            case NewAlign.BottomRight:
                pivot = new Vector2(1f, 0f);
                break;
        }

        return pivot;
    }

    //l t r b
    public static Vector4 GetScrollViewBorder(ScrollRect src)
    {
        Vector4 b = Vector4.zero;
        if (src != null && src.viewport != null)
        {
            RectTransform rt = src.viewport;
            b.z = -rt.offsetMax.x;
            b.y = -rt.offsetMax.y;
            b.x = rt.offsetMin.x;
            b.w = rt.offsetMin.y;
        }
        return b;
    }

    //flat means unsigned
    public static float ClampScrollPos(float flat_pos, RectTransform rt, ScrollRect scroll)
    {
        if (scroll != null && rt != null)
        {
            //Vector2 v = rt.anchoredPosition;
            RectTransform scrollTransform = scroll.viewport as RectTransform;
            if (scrollTransform == null) scrollTransform = scroll.GetComponent<RectTransform>();
			if (scrollTransform != null)
			{
	            float max = scroll.vertical ? rt.rect.height - scrollTransform.rect.height : rt.rect.width - scrollTransform.rect.width;

	            if (flat_pos > max) flat_pos = max;
	            if (flat_pos < 0) flat_pos = 0;
	            //float_pos = max>0? Mathf.Clamp01(flat_pos / max):0;
			}
        }
        return flat_pos;
    }

    ////flat means unsigned
    //public static float ClampScrollNormalPos(float flat_pos, RectTransform rt, ScrollRect scroll)
    //{
    //    if (scroll != null)
    //    {
    //        //Vector2 v = rt.anchoredPosition;
    //        RectTransform scrollTransform = scroll.transform as RectTransform;
    //        float max = scroll.vertical ? rt.sizeDelta.y - scrollTransform.sizeDelta.y : rt.sizeDelta.x - scrollTransform.sizeDelta.x;

    //        //if (flat_pos > max) flat_pos = max;
    //        //if (flat_pos < 0) flat_pos = 0;

    //        flat_pos = max > 0 ? Mathf.Clamp01(flat_pos / max) : 0;
    //    }
    //    return flat_pos;
    //}

    public static string PrintScenePath(Transform trans, int i_max = 0)
    {
        if (trans == null) return string.Empty;

        string s = trans.name;
        if (i_max != 1)
        {
            while (trans.parent != null)
            {
                s = trans.parent.name + "/" + s;
                trans = trans.parent;

                if (i_max > 1)
                {
                    i_max -= 1;
                    if (i_max == 1) break;
                }
            }
        }
        return s;
    }

    public static T GetOrAddComponent<T>(GameObject go) where T : MonoBehaviour
    {
        T sd = go.GetComponent<T>();
        if (sd == null)
        {
            sd = go.gameObject.AddComponent<T>();
        }
        return sd;
    }

    public static T SecureComponetInParent<T>(Transform t) where T : MonoBehaviour
    {
        T tObj = null;

        if (t != null)
            t = t.parent;

        while (t != null)
        {
            tObj = t.GetComponent<T>();
            if (tObj != null)
                break;
            t = t.parent;
        }

        return tObj;
    }

    public static Rect GetRelativeBoundingRect(RectTransform root, RectTransform item)
    {
        //Vector3[] item_w4p = new Vector3[4];
        //item.GetWorldCorners(item_w4p);
        //RectHelper item_b = new RectHelper();
        //for (int i = 0; i < 4; i++)
        //{
        //    Vector2 corner;
        //    RectTransformUtility.ScreenPointToLocalPointInRectangle(root, item_w4p[i], cam, out corner);
        //    item_b.AddPoint(corner);
        //}
        //return item_b.ToRect();


        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(root, item);
        return new Rect(b.min.x, b.min.y, b.max.x - b.min.x, b.max.y - b.min.y);
    }

    public static Rect GetRelativeRect(RectTransform rt_root, RectTransform rt_item)
    {
        if (rt_item == null) return new Rect(0, 0, 0, 0);
        if (rt_root == null || rt_item == rt_root) return rt_item.rect;

        Vector3 l_pos = rt_root.InverseTransformPoint(rt_item.position);
        Rect rect_item = rt_item.rect;
        rect_item.x += l_pos.x;
        rect_item.y += l_pos.y;

        return rect_item;
    }

    public static GameObject PlayFx(Transform hook, Transform anchor, GameObject clipper, string fx_path, float life_time, bool is_ui, int order_offset = 0)
    {
        if (!is_ui)
        {
#if ART_USE && UNITY_EDITOR
            GameObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(fx_path, typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                obj = Object.Instantiate<GameObject>(obj);
                Transform t = obj.transform;
                t.SetParent(hook, false);
                obj.SetActive(true);
                Util.SetLayerRecursively(obj, LayerMask.NameToLayer("UIScene"));

                //re-position
                if (anchor != null && anchor != hook)
                {
                    t.position = anchor.position;
                }

                if (life_time < 0.0001f)
                {
                    FxDuration fxd = obj.GetComponent<FxDuration>();
                    if (fxd != null) life_time = fxd.duration;
                }

                Object.Destroy(obj, life_time > 0.0001f ? life_time : 5);

                Debug.Log("PlayFx " + hook.name + ", " + fx_path + ", " + life_time);
            }
            return obj;
#else
            //Debug.Log("TODO: PlayFx");
	#if IN_GAME
            int fxId = 0;
            CFxOne fxOne = CFxCacheMan.Instance.RequestFxOne(fx_path, -1, out fxId);
            if (fxOne != null)
            {
                Transform t_fx = fxOne.transform;
                GameObject g_fx = fxOne.gameObject;
                t_fx.SetParent(hook, false);

                //re-position
                if (anchor != null && anchor != hook)
                {
                    t_fx.position = anchor.position;
                }

                //g_fx.SetActive(true);
                Util.SetLayerRecursively(g_fx, LayerMask.NameToLayer("UIScene"));

                fxOne.Play(life_time);
                return fxOne.gameObject;
            }

            return null;
#else
			return null;
#endif
#endif
        }
        else
        {
            return UISfxBehaviour.Play(fx_path, anchor.gameObject, hook.gameObject, clipper, life_time, 20, order_offset);
        }
    }

    public static void SetupUILayerOrder(Canvas canv, int s_order, int s_layer_id, bool need_ray)
    {
        //Canvas canv = go.GetComponent<Canvas>();
        if (canv == null)
        {
            //canv = go.AddComponent<Canvas>();
            return;
        }

        GameObject go = canv.gameObject;
        if (need_ray)
        {
            if (go.GetComponent<GraphicRaycaster>() == null)
            {
                go.AddComponent<GraphicRaycaster>();
            }
        }

        canv.overrideSorting = true;
        if (canv.overrideSorting)
        {
            canv.sortingOrder = s_order;
            canv.sortingLayerID = s_layer_id;
        }
        else
        {
            GIACanvFixer ciaf = go.GetComponent<GIACanvFixer>();
            if (ciaf == null)
            {
                ciaf = go.AddComponent<GIACanvFixer>();
            }
            //ciaf.IsValid = true;
            //ciaf.HasLayer = true;
            ciaf.SLayerID = s_layer_id;
            ciaf.SOrder = s_order;
            //ciaf.NeedBlock = need_ray;
            ciaf.enabled = true;
        }
    }

    //public static void SetupUIOrder(Canvas canv, int s_order)
    //{
    //    //Canvas canv = go.GetComponent<Canvas>();
    //    if (canv == null)
    //    {
    //        //canv = go.AddComponent<Canvas>();
    //        return;
    //    }

    //    GameObject go = canv.gameObject;
    //    canv.overrideSorting = true;
    //    if (canv.overrideSorting)
    //    {
    //        canv.sortingOrder = s_order;
    //    }
    //    else
    //    {
    //        GIACanvFixer ciaf = go.GetComponent<GIACanvFixer>();
    //        if (ciaf == null)
    //        {
    //            ciaf = go.AddComponent<GIACanvFixer>();
    //        }
    //        //ciaf.IsValid = true;
    //        ciaf.SOrder = s_order;
    //        ciaf.enabled = true;
    //    }
    //}

    //public static void SetupUILayer(Canvas canv, int s_layer)
    //{
    //    //Canvas canv = go.GetComponent<Canvas>();
    //    if (canv == null)
    //    {
    //        //canv = go.AddComponent<Canvas>();
    //        return;
    //    }

    //    GameObject go = canv.gameObject;
    //    canv.overrideSorting = true;
    //    if (canv.overrideSorting)
    //    {
    //        canv.sortingLayerID = s_layer;
    //    }
    //    else
    //    {
    //        GIACanvFixer ciaf = go.GetComponent<GIACanvFixer>();
    //        if (ciaf == null)
    //        {
    //            ciaf = go.AddComponent<GIACanvFixer>();
    //        }
    //        //ciaf.IsValid = true;
    //        ciaf.HasLayer = true;
    //        ciaf.SLayerID = s_layer;
    //        ciaf.enabled = true;
    //    }
    //}

    public static int GetUIOrder(Canvas cvs)
    {
        int i_ret;
        if (cvs.overrideSorting)
        {
            i_ret = cvs.sortingOrder;
        }
        else
        {
            GIACanvFixer giac = cvs.GetComponent<GIACanvFixer>();
            if (giac != null)
            {
                i_ret = giac.SOrder;
            }
            else
            {
                i_ret = -1;
            }
        }
        return i_ret;
    }

    public static int GetUILayer(Canvas cvs)
    {
        int i_ret;
        if (cvs.overrideSorting)
        {
            i_ret = cvs.sortingLayerID;
        }
        else
        {
            GIACanvFixer giac = cvs.GetComponent<GIACanvFixer>();
            //if (giac != null && giac.HasLayer)
            //{
            i_ret = giac.SLayerID;
            //}
            //else
            //{
            //    i_ret = -1;
            //}
        }
        return i_ret;
    }

    public static void SetAspectMode(GameObject target, float ratio, int mode)
    {
        if (target != null)
        {
            AspectRatioFitter asp = target.GetComponent<AspectRatioFitter>();
            if (asp)
            {
                asp.aspectMode = (AspectRatioFitter.AspectMode)mode;
                asp.aspectRatio = ratio;
            }
        }
    }

}
