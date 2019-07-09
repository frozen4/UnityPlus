using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;

/*  
**  Unity中Transform之间的关系是 父级包含子集，同时父级也可以是别的Transform的子集，子集也可以是别的Transform的父级。
**  这是典型的树状结构，这种结构的一种典型的遍历方式就是递归，这里我们用递归来解决这种树状结构的动态布局的问题。
**  这里的思路如下：
**  1.每当某个子集发生变化需要重新布局的时候，可以调用这个子集的LayoutChange方法，此方法会向上寻找父节点并且记录下经过的节点，直到找到树形结构的跟，调用根节点的LayoutChildren方法。
**  2.LayoutChildren方法会一直向下寻找子节点并且调用节点中的LayoutChildren方法。
**  3.每个节点的LayoutChildren方法总体来说会做两件事：1）根据当前子节点排序方式依次调用所有子节点的LayoutChildren。2）计算当前所有子节点的新位置，并重置位置。
**
**  --李志雄
*/

//Modify
//1. Register Parent when start not find in children
//2. Layout in LateUpdate
//3. detect parent change
//4. consider scale and pivot

//[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class GLayout : GBase, ILayout
{
    [SerializeField]
    protected int _Priority = 0;
    public int Priority { get { return _Priority; } set { _Priority = value; } }
    [SerializeField]
    protected Vector2 _Size = Vector2.one;//x一行多少个，y一列多少个
    public Vector2 Size { get { return _Size; } set { _Size = value; } }
    [SerializeField]
    protected Vector2 _Spacing = Vector2.one * 5;
    public Vector2 Spacing { get { return _Spacing; } set { _Spacing = value; } }
    [SerializeField]
    protected RectOffset _Padding = new RectOffset();
    public RectOffset Padding { get { return _Padding; } set { _Padding = value; } }
    [SerializeField]
    protected SortType _SortType = SortType.Priority;
    public SortType SortedType { get { return _SortType; } set { _SortType = value; } }
    [SerializeField]
    protected GridLayoutGroup.Corner _StartCorner = GridLayoutGroup.Corner.UpperLeft;
    public GridLayoutGroup.Corner StartCorner { get { return _StartCorner; } set { _StartCorner = value; } }
    [SerializeField]
    protected GridLayoutGroup.Axis _StartAxis = GridLayoutGroup.Axis.Horizontal;
    public GridLayoutGroup.Axis StartAxis { get { return _StartAxis; } set { _StartAxis = value; } }

    protected List<GLayout> _ChildNodes = new List<GLayout>();

    //NewUI
    protected const bool USE_DELAY_UPDATE = true;       //experimental

    protected GLayout _ParentLayout = null;
    protected bool _IsDirty = false;
    protected bool _ShouldBeUpdated = true;

    //protected bool _needUpdate = false;

    [NoToLua]
    public enum Mode
    {
        Both = 0,
        ParentOnly,
        SelfAndLower,
    }

    [SerializeField]
    protected Mode _Mode;

    protected bool IsSelfIncluded { get { return _Mode == Mode.SelfAndLower || _Mode == Mode.Both; } }
    protected bool IsParentIncluded { get { return _Mode == Mode.ParentOnly || _Mode == Mode.Both; } }

    public void LayoutChange()
    {
        LayoutChange(false);
    }

    public void Layout()
    {
#if _UI_DEBUG
        Debug.LogWarning(name + " Layout");
#endif

        //UnityEngine.Profiling.Profiler.BeginSample("LayOut");

        //UpdateChildren();
        UpdateChildAnchors();
        SortChildren();
        LayoutChildren();
        CheckGText();

        _IsDirty = false;
        _ShouldBeUpdated = false;

        //UnityEngine.Profiling.Profiler.EndSample();
    }

    protected override void Awake()
    {
        base.Awake();
        UpdateParent(true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UpdateParent(false);
    }

    protected void LayoutChange(bool notDirty)
    {
        if (_ParentLayout == null || !IsParentIncluded)
        {
            if (USE_DELAY_UPDATE)
            {
                if (!_IsDirty && enabled && gameObject.activeInHierarchy)
                {
                    _IsDirty = true;
                    _ShouldBeUpdated = true;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UpdateChildren();
                        Layout();
                    }
#endif
                }
            }
            else
            {
                Layout();
            }
        }
        else
        {
            if (!notDirty && IsSelfIncluded)
            {
                _IsDirty = true;
                _ShouldBeUpdated = true;
            }

            if (IsParentIncluded)
            {
                _ParentLayout.LayoutChange();
            }
        }

    }

    //For preview
    protected virtual void UpdateChildren()
    {
        Vector2 anchor = Vector2.zero;
        if (_StartCorner == GridLayoutGroup.Corner.UpperLeft)
        {
            anchor = Vector2.up;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.UpperRight)
        {
            anchor = Vector2.one;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.LowerRight)
        {
            anchor = Vector2.right;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.LowerLeft)
        {
            anchor = Vector2.zero;
        }

        _ChildNodes.Clear();
        int childCount = transform.childCount;
        int i;
        for (i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            GLayout layout = child.GetComponent<GLayout>();
            if (!child.gameObject.activeSelf || layout == null || layout.RectTrans.sizeDelta.x <= 0 || layout.RectTrans.sizeDelta.y <= 0) continue;
            layout.RectTrans.anchorMax = layout.RectTrans.anchorMin = layout.RectTrans.pivot = anchor;
            layout._ParentLayout = this;
            _ChildNodes.Add(layout);
        }

    }

    //RealTime
    protected void UpdateParent(bool active)
    {
        if (active)
        {
            if (_ParentLayout == null)
            {
                Transform parent = Trans.parent;
                if (parent != null)
                {
                    _ParentLayout = parent.GetComponent<GLayout>();
                    if (_ParentLayout != null)
                    {
                        _ParentLayout.RegisterChild(this);
                        //break;
                    }
                    //parent = parent.parent;
                }
            }
        }
        else
        {
            if (_ParentLayout != null)
            {
                _ParentLayout.UnRegisterChild(this);
                _ParentLayout = null;
            }
        }
    }

    protected void RegisterChild(GLayout glo)
    {
        if (!_ChildNodes.Contains(glo))
        {
            _ChildNodes.Add(glo);

        }
    }

    protected void UnRegisterChild(GLayout glo)
    {
        _ChildNodes.Remove(glo);
    }

    protected virtual void SortChildren()
    {
        if (_SortType == SortType.Position)
        {
            if (_StartAxis == GridLayoutGroup.Axis.Horizontal)
            {
                switch (_StartCorner)
                {
                    case GridLayoutGroup.Corner.UpperLeft:
                    case GridLayoutGroup.Corner.LowerLeft:
                        _ChildNodes.Sort((v1, v2) => v1.RectTrans.anchoredPosition.x.CompareTo(v2.RectTrans.anchoredPosition.x));
                        break;
                    case GridLayoutGroup.Corner.UpperRight:
                    case GridLayoutGroup.Corner.LowerRight:
                        _ChildNodes.Sort((v1, v2) => v2.RectTrans.anchoredPosition.x.CompareTo(v1.RectTrans.anchoredPosition.x));
                        break;
                }
            }
            else if (_StartAxis == GridLayoutGroup.Axis.Vertical)
            {
                switch (_StartCorner)
                {
                    case GridLayoutGroup.Corner.UpperLeft:
                    case GridLayoutGroup.Corner.UpperRight:
                        _ChildNodes.Sort((v1, v2) => v2.RectTrans.anchoredPosition.y.CompareTo(v1.RectTrans.anchoredPosition.y));
                        break;
                    case GridLayoutGroup.Corner.LowerLeft:
                    case GridLayoutGroup.Corner.LowerRight:
                        _ChildNodes.Sort((v1, v2) => v1.RectTrans.anchoredPosition.y.CompareTo(v2.RectTrans.anchoredPosition.y));
                        break;
                }
            }
        }
        else if (_SortType == SortType.Priority)
        {
            _ChildNodes.Sort((v1, v2) => v1.Priority.CompareTo(v2.Priority));
        }
    }

    protected virtual void LayoutChildren()
    {
        if (_ChildNodes.Count <= 0) return;
        float fx = 0;
        float fy = 0;
        int paddingx1 = 0;
        int paddingx2 = 0;
        int paddingy1 = 0;
        int paddingy2 = 0;
        if (_StartCorner == GridLayoutGroup.Corner.UpperLeft)
        {
            fx = 1;
            fy = -1;
            paddingx1 = _Padding.left;
            paddingx2 = _Padding.right;
            paddingy1 = _Padding.top;
            paddingy2 = _Padding.bottom;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.UpperRight)
        {
            fx = -1;
            fy = -1;
            paddingx1 = _Padding.right;
            paddingx2 = _Padding.left;
            paddingy1 = _Padding.top;
            paddingy2 = _Padding.bottom;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.LowerRight)
        {
            fx = -1;
            fy = 1;
            paddingx1 = _Padding.right;
            paddingx2 = _Padding.left;
            paddingy1 = _Padding.bottom;
            paddingy2 = _Padding.top;

        }
        else if (_StartCorner == GridLayoutGroup.Corner.LowerLeft)
        {
            fx = 1;
            fy = 1;
            paddingx1 = _Padding.left;
            paddingx2 = _Padding.right;
            paddingy1 = _Padding.bottom;
            paddingy2 = _Padding.top;
        }

        int i;
        int count;
        int index;
        float width = 0;
        float height = 0;
        float offset = 0;
        float x;
        float y;
        float max;
        if (_StartAxis == GridLayoutGroup.Axis.Horizontal)
        {
            count = Mathf.RoundToInt(_Size.x);
            if (count < 1)
            {
                HobaDebuger.LogWarning("GLayout size_x==0! " + GNewUITools.PrintScenePath(this.Trans));
                //Debug.LogError("GLayout size_x==0!" + GNewUITools.PrintScenePath(this.Trans));
                count = 1;
            }

            x = paddingx1 * fx;
            y = paddingy1 * fy;
            for (i = 0; i < _ChildNodes.Count; i++)
            {
                index = i % count;
                GLayout layout = _ChildNodes[i];
                if (layout == null || !layout.gameObject.activeSelf /*|| layout.RectTrans.sizeDelta.x <= 0 || layout.RectTrans.sizeDelta.y <= 0*/) continue;

                ApplyLayoutToChild(layout);

                RectTransform rt = layout.RectTrans;

                float dsx = rt.sizeDelta.x /* * rt.localScale.x*/;
                float dsy = rt.sizeDelta.y /* * rt.localScale.y*/;

                layout.RectTrans.anchoredPosition = new Vector2(x + (fx > 0 ? rt.pivot.x : 1 - rt.pivot.x) * dsx, y + (fy > 0 ? rt.pivot.y : 1 - rt.pivot.y) * dsy);

                max = Mathf.Abs(x) + dsx;
                if (max > width)  //最宽的那一行的宽度
                {
                    width = max;
                }
                if (dsy > offset)  //一行中最高的那一个
                {
                    offset = dsy;
                }
                if (index < count - 1)
                {
                    x += dsx * fx;
                    x += _Spacing.x * fx;
                }
                else if (index == count - 1)
                {
                    x = paddingx1 * fx;
                    y += offset * fy;
                    if (i < _ChildNodes.Count - 1)
                        y += _Spacing.y * fy;
                    offset = 0;
                }
            }
            width += paddingx2;
            y += offset * fy;
            y += paddingy2 * fy;
            height = Mathf.Abs(y);
            SetSize(width, height);
        }
        else if (_StartAxis == GridLayoutGroup.Axis.Vertical)
        {
            count = Mathf.RoundToInt(_Size.y);
            if (count < 1)
            {
                HobaDebuger.LogWarning("GLayout size_y==0! " + GNewUITools.PrintScenePath(this.Trans));
                //Debug.LogError("GLayout size_x==0!" + GNewUITools.PrintScenePath(this.Trans));
                count = 1;
            }

            x = paddingx1 * fx;
            y = paddingy1 * fy;
            for (i = 0; i < _ChildNodes.Count; i++)
            {
                index = i % count;
                GLayout layout = _ChildNodes[i];
                if (layout == null || !layout.gameObject.activeSelf /*|| layout.RectTrans.sizeDelta.x <= 0 || layout.RectTrans.sizeDelta.y <= 0*/) continue;

                ApplyLayoutToChild(layout);

                RectTransform rt = layout.RectTrans;

                float dsx = rt.sizeDelta.x /* * rt.localScale.x */;
                float dsy = rt.sizeDelta.y /* * rt.localScale.y*/;

                layout.RectTrans.anchoredPosition = new Vector2(x + (fx > 0 ? rt.pivot.x : 1 - rt.pivot.x) * dsx, y + (fy > 0 ? rt.pivot.y : 1 - rt.pivot.y) * dsy);

                max = Mathf.Abs(y) + dsy;
                if (max > height)  //最高的那一行的高度
                {
                    height = max;
                }
                if (dsx > offset)  //一列中最宽的那一个
                {
                    offset = dsx;
                }
                if (index < count - 1)
                {
                    y += dsy * fy;
                    y += _Spacing.y * fy;
                }
                else if (index == count - 1)
                {
                    y = paddingy1 * fy;
                    x += offset * fx;
                    if (i < _ChildNodes.Count - 1)
                        x += _Spacing.x * fx;
                    offset = 0;
                }
            }
            height += paddingy2;
            x += offset * fx;
            x += paddingx2 * fx;
            width = Mathf.Abs(x);
            SetSize(width, height);
        }
    }

    private void CheckGText()
    {
        GText gt = GetComponent<GText>();
        if (gt)
        {
            gt.FormatBoard();
        }
    }

    private void SetSize(float width, float height)
    {
        GLayoutSizeRestriction restriction = GetComponent<GLayoutSizeRestriction>();
        if (restriction != null)
        {
            Vector2 size = restriction.GetRestrictionSize();
            if (width > size.x)
            {
                width = size.x;
            }
            if (height > size.y)
            {
                height = size.y;
            }
            RectTrans.sizeDelta = new Vector2(width, height);
        }
        else
        {
            RectTrans.sizeDelta = new Vector2(width, height);
        }
    }

    private void ApplyLayoutToChild(GLayout layout)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            if (layout._ShouldBeUpdated && layout.IsParentIncluded)
            {
                layout.Layout();
            }
        }
        else
        {
            if (layout.IsParentIncluded)
            {
                layout.UpdateChildren();
                layout.Layout();
            }
        }
#else
                if (layout.IsParentIncluded)
                {
                    layout.Layout();
                }
#endif
    }

    private void UpdateChildAnchors()
    {
        //

        Vector2 anchor = Vector2.zero;
        if (_StartCorner == GridLayoutGroup.Corner.UpperLeft)
        {
            anchor = Vector2.up;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.UpperRight)
        {
            anchor = Vector2.one;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.LowerRight)
        {
            anchor = Vector2.right;
        }
        else if (_StartCorner == GridLayoutGroup.Corner.LowerLeft)
        {
            anchor = Vector2.zero;
        }

        //

        for (int i = 0; i < _ChildNodes.Count; i++)
        {
            GLayout glo = _ChildNodes[i];
            glo.RectTrans.anchorMax = glo.RectTrans.anchorMin = glo.RectTrans.pivot = anchor;
        }
    }

    //Some time it just doesnt work

    //private IEnumerator DelayLayOut()
    //{
    //    yield return new WaitForEndOfFrame();
    //    Layout();
    //}

    private void LateUpdate()
    {
        if (_ShouldBeUpdated)
        {
            Layout();
        }
    }

}