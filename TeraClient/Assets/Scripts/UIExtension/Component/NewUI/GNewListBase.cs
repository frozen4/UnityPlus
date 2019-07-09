using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using DG.Tweening;

//loop list
//inverse dir not supported;
//dirty data not supported;

public abstract class GNewGridBase : GNewUIBase
{
    [NoToLua]
    public delegate void ItemDelegate(GameObject list, GameObject item, int index);

    [NoToLua]
    public enum Direction
    {
        Horizontal = 1,
        Vertical
    }

    [NoToLua]
    public enum TweenDirection
    {
        In = 1,
        Out
    }

    #region serialization
    [SerializeField]
    protected int _NumberOfRow = 1;
    [SerializeField]
    protected int _NumberOfColumn = 1;

    [SerializeField]
    protected Vector2 _Spacing = Vector2.one;
    [SerializeField]
    protected Vector4 _Padding = Vector4.zero;

    [SerializeField]
    private Direction _Direction = Direction.Horizontal;
    [SerializeField]
    protected RectTransform _CellRect;

    [SerializeField]
    protected NewAlign _Align = NewAlign.Center;
    [SerializeField]
    protected bool _IsInverseDirection;       //Dont change at runtime! For proformance.

    [SerializeField]
    protected bool IsAddLayoutElem;       //Add LayoutElem

    [SerializeField]
    protected bool _IsExpandItem;

    //public float ItemAnimDelay;		//delay for tween item by index

    ///////animating/////
    //public bool IsItemAnimated;

    #endregion serialization

    protected LayoutElement _LayoutElem;

    protected int _ItemCount;
    //protected int _PageSize;
    //real size: cols,lines,w,h
    protected Vector4 _RealBound;

    [NoToLua]
    public bool IsVertical { get { return _Direction == Direction.Vertical; } }
    public int ItemCount { get { return _ItemCount; } }
    public int PageWidth { get { return _NumberOfColumn; } set { _NumberOfColumn = value; /*IsDirty = true;*/ } }    //ab
    public int PageHeight { get { return _NumberOfRow; } set { _NumberOfRow = value; /*IsDirty = true;*/ } }    //ab
    [NoToLua]
    public Vector2 Spacing { get { return _Spacing; } set { _Spacing = value; /*IsDirty = true;*/} }
    [NoToLua]
    public Vector4 Padding { get { return _Padding; } set { _Padding = value; /*IsDirty = true;*/} }
    [NoToLua]
    public RectTransform CellRect { get { return _CellRect; } set { _CellRect = value; /*IsDirty = true;*/} }

    protected int PageDiv { get { return IsVertical ? PageWidth : PageHeight; } }
    protected float CellDiv { get { return IsVertical ? (CellSize.y + _Spacing.y) : (CellSize.x + _Spacing.x); } }
    protected int PageSize { get { return PageWidth * PageHeight; } }
    protected float SpacingDiv { get { return IsVertical ? (_Spacing.y) : (_Spacing.x); } }
    protected float DirSign { get { return _IsInverseDirection ? 1f : -1f; } }        //for inverse dir use
    protected float CurDirectionPos { get { return DirSign * RelativePos; } }
    protected float RelativePos
    {
        get { return IsVertical ? RectTrans.anchoredPosition.y : RectTrans.anchoredPosition.x; }
    }

    protected virtual Vector2 CellSize { get { return _CellRect != null ? _CellRect.rect.size : new Vector2(100, 100); } }

    //need refresh
    //protected bool IsDirty { get; set; }

    //public float ItemAnimDelay;		//delay for tween item by index

    public virtual void Repaint()
    {
        SafeInit();

        RecalculateBound();
        UpdateContents();

        //IsDirty = false;
    }

    public void SetPageSize(int cols, int rows)
    {
        PageWidth = cols;
        PageHeight = rows;
    }

    protected virtual void AddLayoutElem()
    {
        //controls lo elem
        if (_LayoutElem == null)
        {
            _LayoutElem = gameObject.GetComponent<LayoutElement>();
            if (_LayoutElem == null)
            {
                _LayoutElem = gameObject.AddComponent<LayoutElement>();
            }
        }
    }

    protected virtual void RecalculateBound()
    {
        if (_IsExpandItem)
        {
            if (IsVertical)
                _NumberOfColumn = 1;
            else
                _NumberOfRow = 1;
        }

        SetPivotAnchor(RectTrans, _Align);
        _RealBound = this.PackBound(this._ItemCount);

        if (_RealBound.z < 0)
        {
            _RealBound.z = RectTrans.rect.width;
        }
        if (_RealBound.w < 0)
        {
            _RealBound.w = RectTrans.rect.height;
        }

        //SetPivotAnchor(RectTrans, _Align);
        Vector2 new_size = new Vector2(_RealBound.z, _RealBound.w);

        this.RectTrans.sizeDelta = GNewUITools.GetDeltaSize(RectTrans, new_size);

        if (_LayoutElem != null)
        {
            _LayoutElem.preferredHeight = new_size.y;
            _LayoutElem.preferredWidth = new_size.x;

            _LayoutElem.enabled = false;
            _LayoutElem.enabled = true;
        }
    }

    protected virtual void SetPivotAnchor(RectTransform rect, NewAlign align)
    {
        Vector2 new_pos = GNewUITools.GetAlignedPivot(align);

        rect.pivot = new_pos;

        rect.anchorMax = new_pos;
        rect.anchorMin = new_pos;
    }

    //making total into a rect
    protected virtual Vector4 PackBound(int total)
    {
        Vector4 pack = new Vector4();

        //_PageSize = PageWidth * PageHeight;

        if (IsVertical)
        {
            pack.x = PageWidth;
            pack.y = Mathf.CeilToInt(total / pack.x);
        }
        else
        {
            pack.y = PageHeight;
            pack.x = Mathf.CeilToInt(total / pack.y);
        }

        if (_IsExpandItem && IsVertical)
        {
            pack.z = -1;
        }
        else
        {
            pack.z = pack.x * (CellSize.x + _Spacing.x) + Padding.x + Padding.z;
            if (pack.x > 0)
                pack.z -= Spacing.x;
        }

        if (_IsExpandItem && !IsVertical)
        {
            pack.w = -1;
        }
        else
        {
            pack.w = pack.y * (CellSize.y + _Spacing.y) + Padding.y + Padding.w;
            if (pack.y > 0)
                pack.w -= Spacing.y;
        }

        return pack;
    }

    protected virtual void UpdateContents() { }

    protected override void OnSafeInit()
    {
        base.OnSafeInit();

        //RectTrans.anchorMax = RectTrans.anchorMin = RectTrans.pivot = Vector2.up;
        if (_CellRect == null)
        {
            if (Trans.childCount > 0)
            {
                _CellRect = Trans.GetChild(0) as RectTransform;
            }

            if (_CellRect == null)
            {
                Debug.Log("<GNewList>: Item template not found!");
            }
        }

        if (_CellRect != null)
        {
            //_cellRect.gameObject.SetActive(false);
            GNewUITools.SetVisible(_CellRect, false);
        }

        if (_NumberOfRow < 1) _NumberOfRow = 1;
        if (_NumberOfColumn < 1) _NumberOfColumn = 1;

        if (IsAddLayoutElem)
        {
            AddLayoutElem();
        }
    }

    //public virtual void PlayEffect() { }

    protected Vector2 Index2Pos2(int index, RectTransform item)
    {
        int x, y;

        if (IsVertical)
        {
            y = index / PageWidth;
            x = index - y * PageWidth;
        }
        else
        {
            x = index / PageHeight;
            //y = (x + 1) * PageHeight - index - 1;
            y = index - x * PageHeight;
        }

        Vector2 pivot = item.pivot;
        Vector2 pp = new Vector2(x * (CellSize.x + Spacing.x), y * (CellSize.y + Spacing.y));

        if (IsVertical)
        {
            pp.y *= -DirSign;
            if (_IsInverseDirection)
            {
                pivot.y = pivot.y - 1;
                pp.y -= Padding.y;
            }
            else
            {
                pp.y += Padding.w;
            }

            if (!_IsExpandItem)
            {
                pp.x += Padding.x - pivot.x * (_RealBound.z - CellSize.x);
            }
            else
            {
                pp.x = Mathf.Lerp(Padding.x, -Padding.z, pivot.x);
            }
            pp.y += -pivot.y * (_RealBound.w - CellSize.y);
        }
        else
        {
            pp.x *= -DirSign;
            if (_IsInverseDirection)
            {
                pivot.x = pivot.x - 1;
                pp.x -= Padding.z;
            }
            else
            {
                pp.x += Padding.x;
            }

            if (!_IsExpandItem)
            {
                pp.y += Padding.y + (1 - pivot.y) * (CellSize.y - _RealBound.w);
            }
            else
            {
                pp.y = Mathf.Lerp(-Padding.w, Padding.y, pivot.y);
            }
            pp.y *= -1;
            pp.x += -pivot.x * (_RealBound.z - CellSize.x);
        }

        //post align

        //if (IsVertical)
        //{
        //    pp.x += RectTrans.pivot.x * (RectTrans.sizeDelta.x - _RealBound.z);
        //}
        //else
        //{
        //    pp.y += RectTrans.pivot.y * (RectTrans.sizeDelta.y - _RealBound.w);
        //}

        //if (IsVertical)
        //{
        //    pp.x += RectTrans.pivot.x * (RectTrans.rect.width - _RealBound.z);
        //}
        //else
        //{
        //    pp.y += RectTrans.pivot.y * (RectTrans.rect.height - _RealBound.w);
        //}

        return pp;
    }

    //[ContextMenu("PlayTween")]
    //public virtual void PlayEffect()
    //{
    //    if (IsInited)
    //    {
    //        //for (int i = 0; i < _viewItems.Count; i++)
    //        //{
    //        //    GListItem gi = _viewItems[i];
    //        //    if (gi != null)
    //        //    {
    //        //        gi.PlayTween(ItemAnimDelay * i);
    //        //    }
    //        //}
    //    }
    //}
}

public abstract class GNewListBase : GNewGridBase
{
    #region serialization
    [NoToLua]
    public bool noScroll;

    [NoToLua]
    public bool HasChildButton;

    [NoToLua]
    public bool CenterOnMinimu;

    [NoToLua]
    public int MiniCount;

    public bool SingleSelect;

    [NoToLua]
    //-1 means default, 0 means dont use pool;
    public int PoolSize = -1;

    #endregion serialization

    protected ScrollRect _ScrollRect;
    protected int _SingleSelectIndex = -1;

    /////interaction/////

    [NoToLua]
    [NonSerialized]
    public ItemDelegate InitItemCallBack;
    //[NoToLua]
    //[NonSerialized]
    //public ItemDelegate PressItemCallBack;
    //[NoToLua]
    //[NonSerialized]
    //public ItemDelegate ReleaseItemCallBack;

    [NoToLua]
    [NonSerialized]
    public ItemDelegate LongPressCallBack;

    [NoToLua]
    [NonSerialized]
    public ItemDelegate ClickItemCallBack;
    [NoToLua]
    [NonSerialized]
    public ItemDelegate ClickItemButtonCallBack;

    [NoToLua]
    public ScrollRect ScrollRectControl { get { return _ScrollRect; } }

    public GameObject GetItem(int index)
    {
        GListItem g_item = GetListItem(index);
        if (g_item != null)
        {
            return g_item.gameObject;
        }
        return null;
    }

    ///call this on data source updated
    ///void
    ///int: total number of data
    public virtual void SetItemCount(int count)
    {
        if (count < 0)
        {
            Debug.LogError("GNewListBase:SetItemCount < 0 " + count);
            return;
        }

        _SingleSelectIndex = -1;
        this._ItemCount = count;
        //IsDirty = true;
        Repaint();
    }

    public void SetSelection(int index)
    {
        if (SingleSelect)
        {
            if (_SingleSelectIndex != index)
            {
                GListItem item_com = GetListItem(_SingleSelectIndex);
                if (item_com != null)
                {
                    item_com.IsOn = false;
                }
                _SingleSelectIndex = index;

                item_com = GetListItem(_SingleSelectIndex);
                if (item_com != null)
                {
                    item_com.IsOn = true;
                }
                else
                {
                    _SingleSelectIndex = -1;
                }
            }
        }
    }

    public void SetManualSelection(int index, bool is_on)
    {
        if (!SingleSelect)
        {
            GListItem item = GetListItem(index);
            if (item != null)
            {
                item.IsOn = is_on;
            }
        }
    }

    //public override void Repaint()
    //{
    //    base.Repaint();
    //    if (ScrollRectControl != null)
    //    {
    //        RectTrans.anchoredPosition = Vector2.zero;
    //    }
    //}

    //[NoToLua]
    //public void OnPopEffectStart()
    //{
    //    SafeInit();

    //    if (_ScrollRect != null)
    //    {
    //        _ScrollRect.enabled = false;
    //    }
    //}

    //[NoToLua]
    //public void OnPopEffectOver()
    //{
    //    if (_ScrollRect != null)
    //    {
    //        _ScrollRect.enabled = true;
    //    }
    //}

    protected void FitView()
    {
        if (CenterOnMinimu)
        {
            Vector2 center = RectTrans.pivot;
            Vector2 pos = RectTrans.anchoredPosition;
            if (this._ItemCount < this.MiniCount)
            {
                if (ScrollRectControl != null)
                {
                    ScrollRectControl.enabled = false;
                }
                if (IsVertical)
                {
                    center.y = 0.5f;
                    pos.y = 0;
                }
                else
                {
                    center.x = 0.5f;
                    pos.x = 0;
                }
                RectTrans.anchorMax = RectTrans.anchorMin = RectTrans.pivot = center;
                RectTrans.anchoredPosition = pos;
            }
            else
            {
                if (ScrollRectControl != null)
                {
                    ScrollRectControl.enabled = true;
                }
            }
        }
    }

    protected float ScrollStep2ScrollPos(int step)
    {
        //inverse not supported

        if (step < 0) step = 0;

        float pos = step * CellDiv; //+SpacingDiv;
        return pos;
    }

    //id starts at 0, pos is towards postive side
    protected int ScrollPos2ScrollStep(float pos)
    {
        pos -= SpacingDiv;

        int spos = Mathf.FloorToInt(pos / CellDiv);

        return spos;
    }

    public void EnableScroll(bool enable)
    {
        SafeInit();

        if (ScrollRectControl != null)
        {
            ScrollRectControl.enabled = enable;
        }
    }

    protected override void RecalculateBound()
    {
        base.RecalculateBound();

        FitView();
    }

    protected override Vector4 PackBound(int total)
    {
        Vector4 pack = new Vector4();

        //_PageSize = PageWidth * PageHeight;

        if (IsVertical)
        {
            pack.x = (IsMiniState() && total < PageWidth) ? total : PageWidth;
            pack.y = Mathf.CeilToInt(total / (float)PageWidth);
        }
        else
        {
            pack.y = (IsMiniState() && total < PageHeight) ? total : PageHeight;
            pack.x = Mathf.CeilToInt(total / (float)PageHeight);
        }

        if (_IsExpandItem && IsVertical)
        {
            pack.z = -1;
        }
        else
        {
            pack.z = pack.x * (CellSize.x + _Spacing.x) + Padding.x + Padding.z;
            if (pack.x > 0)
                pack.z -= Spacing.x;
        }

        if (_IsExpandItem && !IsVertical)
        {
            pack.w = -1;
        }
        else
        {
            pack.w = pack.y * (CellSize.y + _Spacing.y) + Padding.y + Padding.w;
            if (pack.y > 0)
                pack.w -= Spacing.y;
        }

        return pack;
    }


    bool IsMiniState()
    {
        return CenterOnMinimu && (this._ItemCount < this.MiniCount);
    }

    //no-base call
    protected override void SetPivotAnchor(RectTransform rect, NewAlign align)
    {
        if (_IsExpandItem)
        {
            if (IsVertical)
            {
                if (align == NewAlign.BottomLeft || align == NewAlign.BottomRight) align = NewAlign.Bottom;
                else if (align == NewAlign.TopLeft || align == NewAlign.TopRight) align = NewAlign.Top;
                else if (align == NewAlign.Left || align == NewAlign.Right) align = NewAlign.Center;
            }
            else
            {
                if (align == NewAlign.BottomLeft || align == NewAlign.TopLeft) align = NewAlign.Left;
                else if (align == NewAlign.TopRight || align == NewAlign.BottomRight) align = NewAlign.Right;
                else if (align == NewAlign.Top || align == NewAlign.Bottom) align = NewAlign.Center;
            }
        }

        if (_ScrollRect != null)
        {
            if (IsVertical)
            {
                if (align == NewAlign.BottomLeft || align == NewAlign.Left || align == NewAlign.TopLeft) align = _IsInverseDirection ? NewAlign.TopLeft : NewAlign.BottomLeft;
                else if (align == NewAlign.BottomRight || align == NewAlign.Right || align == NewAlign.TopRight) align = _IsInverseDirection ? NewAlign.TopRight : NewAlign.BottomRight;
                else align = _IsInverseDirection ? NewAlign.Top : NewAlign.Bottom;
            }
            else
            {
                if (align == NewAlign.BottomLeft || align == NewAlign.Bottom || align == NewAlign.BottomRight) align = _IsInverseDirection ? NewAlign.BottomRight : NewAlign.BottomLeft;
                else if (align == NewAlign.TopLeft || align == NewAlign.Top || align == NewAlign.TopRight) align = _IsInverseDirection ? NewAlign.TopRight : NewAlign.TopLeft;
                else align = _IsInverseDirection ? NewAlign.Right : NewAlign.Left;
            }
        }

        Vector2 new_pos = GNewUITools.GetAlignedPivot(align);

        //Vector2 new_pos_cell = CellRect.anchorMin;

        //if (_ScrollRect != null)
        //{
        //    if (IsVertical)
        //    {
        //        new_pos.y = _IsInverseDirection ? 1 : 0;
        //    }
        //    else
        //    {
        //        new_pos.x = _IsInverseDirection ? 1 : 0;
        //    }
        //}

        CellRect.pivot = new_pos;
        Vector2 size = CellSize;
        //CellRect.anchorMax = new_pos_cell;
        //CellRect.anchorMin = new_pos_cell;
        //AdjustAnchor(CellRect, align);
        CellRect.anchorMax = new_pos;
        CellRect.anchorMin = new_pos;

        if (size.x > 0 && size.y > 0)
        {
            CellRect.sizeDelta = size;
        }

        rect.pivot = new_pos;
        //rect.anchorMax = new_pos;
        //rect.anchorMin = new_pos;
        //AdjustContentAnchor(rect, align);
        AdjustItemAnchor(rect);
    }

    //void AdjustContentAnchor(RectTransform rect, NewAlign align)
    //{
    //    Vector2 pivot = rect.pivot;
    //    if (align == NewAlign.BottomLeft || align == NewAlign.BottomRight || align == NewAlign.TopLeft || align == NewAlign.TopRight || align == NewAlign.Center)
    //    {
    //        Vector2 d_size = CellSize;
    //        rect.anchorMax = pivot;
    //        rect.anchorMin = pivot;
    //        CellRect.sizeDelta = d_size;
    //    }
    //    else if (align == NewAlign.Bottom || align == NewAlign.Top)
    //    {
    //        if (_IsExpandItem)
    //        {
    //            Vector2 d_size = rect.rect.size;
    //            rect.anchorMin = new Vector2(rect.anchorMin.x, pivot.y);
    //            rect.anchorMax = new Vector2(rect.anchorMax.x, pivot.y);


    //            d_size.x = rect.sizeDelta.x;
    //            rect.sizeDelta = d_size;
    //        }
    //        else
    //        {
    //            rect.anchorMin = rect.anchorMax = pivot;
    //        }
    //    }
    //    else if (align == NewAlign.Left || align == NewAlign.Right)
    //    {
    //        if (_IsExpandItem)
    //        {
    //            Vector2 d_size = rect.rect.size;
    //            rect.anchorMin = new Vector2(pivot.x, rect.anchorMax.y);
    //            rect.anchorMax = new Vector2(pivot.x, rect.anchorMax.y);
    //            d_size.y = rect.sizeDelta.y;
    //            rect.sizeDelta = d_size;
    //        }
    //        else
    //        {
    //            rect.anchorMin = rect.anchorMax = pivot;
    //        }
    //    }
    //}

    [NoToLua]
    public void AdjustItemAnchor(RectTransform rect_item)
    {
        if (_IsExpandItem)
        {
            if (IsVertical)
            {
                rect_item.pivot = _CellRect.pivot;
                rect_item.anchorMin = new Vector2(0, rect_item.pivot.y);
                rect_item.anchorMax = new Vector2(1, rect_item.pivot.y);

                rect_item.offsetMin = new Vector2(Padding.x, rect_item.offsetMin.y);
                rect_item.offsetMax = new Vector2(-Padding.z, rect_item.offsetMax.y);
            }
            else
            {
                rect_item.pivot = _CellRect.pivot;
                rect_item.anchorMin = new Vector2(rect_item.pivot.x, 0);
                rect_item.anchorMax = new Vector2(rect_item.pivot.x, 1);

                rect_item.offsetMin = new Vector2(rect_item.offsetMin.x, Padding.w);
                rect_item.offsetMax = new Vector2(rect_item.offsetMax.x, -Padding.y);
            }
        }
        else
        {
            Vector2 size = rect_item.rect.size;
            rect_item.anchorMin = rect_item.anchorMax = rect_item.pivot = _CellRect.pivot;
            //rect_item.sizeDelta = CellSize;

            if (size.x > 0 && size.y > 0)
            {
                rect_item.sizeDelta = size;
            }
        }
    }


    protected void DisposeItem(List<GListItem> items, int index)
    {
        if (items.Count > index)
        {
            GListItem item = items[index];
            if (item != null)
            {
#if IN_GAME
                if (_Pool.PutIn(item))
                {
                    GNewUITools.SetVisible(item.RectTrans, false);
                }
                else
#endif
                {
                    Destroy(item.gameObject);
                }
            }
            items.RemoveAt(index);
        }
    }

    protected GListItem TryCreateItem()
    {
        GListItem rv = null;

#if IN_GAME
        rv = _Pool.TakeOut();
#endif

        if (rv == null)
        {
            RectTransform item = CUnityUtil.Instantiate(_CellRect) as RectTransform;
            item.SetParent(Trans, false);

            GListItem item_com = item.GetComponent<GListItem>();
            if (item_com == null)
            {
                item_com = item.gameObject.AddComponent<GListItem>();
            }

            //register events
            if (this.InitItemCallBack != null)
            {
                item_com.OnItemInit = this.OnShowItem;
            }

            if (this.ClickItemCallBack != null)
            {
                item_com.OnItemClick = this.OnClickItem;
            }

            //if (this.PressItemCallBack != null)
            //{
            //    item_com.OnItemPointerDown = this.OnPressItem;
            //}

            //if (this.ReleaseItemCallBack != null)
            //{
            //    item_com.OnItemPointerUp = this.OnReleaseItem;
            //}

            if (this.LongPressCallBack != null)
            {
                item_com.OnItemLongPress = this.OnLongPressItem;
            }

            if (HasChildButton && this.ClickItemButtonCallBack != null)
            {

                item_com.OnItemClickButton = this.OnClickItemButton;
            }

            rv = item_com;
        }

        GNewUITools.SetVisible(rv.RectTrans, true);

        return rv;
    }

    /////selections/////

    protected virtual void OnClickItem(GameObject item, int index)
    {
        //SetSelection(index);

        if (ClickItemCallBack != null)
        {
            ClickItemCallBack(this.gameObject, item, index);
        }
    }

    //protected virtual void OnPressItem(GameObject item, int index)
    //{
    //    if (PressItemCallBack != null)
    //    {
    //        PressItemCallBack(this.gameObject, item, index);
    //    }
    //}
    //protected virtual void OnReleaseItem(GameObject item, int index)
    //{
    //    if (ReleaseItemCallBack != null)
    //    {
    //        ReleaseItemCallBack(this.gameObject, item, index);
    //    }
    //}

    protected virtual void OnLongPressItem(GameObject item, int index)
    {
        if (LongPressCallBack != null)
        {
            LongPressCallBack(this.gameObject, item, index);
        }
    }

    protected virtual void OnShowItem(GameObject item, int index)
    {
        if (InitItemCallBack != null)
        {
            InitItemCallBack(this.gameObject, item, index);
        }
    }

    protected virtual void OnClickItemButton(GameObject item, int index)
    {
        if (ClickItemButtonCallBack != null)
        {
            ClickItemButtonCallBack(this.gameObject, item, index);
        }
    }

#if IN_GAME
    private GNewPrivatePool<GListItem> _Pool;

    [NoToLua]
    public abstract GListItem GetListItem(int index);

    public abstract void RefreshItem(int index);

    public abstract void AddItem(int index, int count);

    public abstract void RemoveItem(int index, int count);

    public virtual void ScrollToStep(int scrollStep)
    {
        SafeInit();

        float pos = ScrollStep2ScrollPos(scrollStep);
        ScrollToPosition(pos);
    }

    [NoToLua]
    //flat means unsigned
    public void ScrollToPosition(float flatPos)
    {
        SafeInit();

        if (_ScrollRect != null)
        {
            _ScrollRect.StopMovement();
            Vector2 v = RectTrans.anchoredPosition;
            if (IsVertical)
            {
                //v.y = ClampScrollPos(flatPos) * DirSign;
                v.y = GNewUITools.ClampScrollPos(flatPos, RectTrans, _ScrollRect) * DirSign;
            }
            else
            {
                //v.x = ClampScrollPos(flatPos) * DirSign;
                v.x = GNewUITools.ClampScrollPos(flatPos, RectTrans, _ScrollRect) * DirSign;
            }

            RectTrans.anchoredPosition = v;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _ScrollRect = null;
        if (_Pool != null)
        {
            _Pool.Clear();
        }

        this.InitItemCallBack = null;

        this.ClickItemCallBack = null;

        ClickItemButtonCallBack = null;
    }

    protected override void OnSafeInit()
    {
        base.OnSafeInit();

        if (!noScroll)
        {
            //_ScrollRect = GetComponentInParent<ScrollRect>(true);
            _ScrollRect = GNewUITools.SecureComponetInParent<ScrollRect>(RectTrans);
        }
        if (_ScrollRect != null)
        {
            _ScrollRect.horizontal = !IsVertical;
            _ScrollRect.vertical = IsVertical;
        }

        _Pool = new GNewPrivatePool<GListItem>(PoolSize);
    }

#elif ART_USE

    //all items in the view
    private List<GListItem> _viewItems = new List<GListItem>();

    protected override void OnSafeInit()
    {
        base.OnSafeInit();

        if (!noScroll)
        {
            _ScrollRect = GetComponentInParent<ScrollRect>();
            //_ScrollRect = GNewUITools.SecureComponetInParent<ScrollRect>(RectTrans);
        }
        if (_ScrollRect != null)
        {
            _ScrollRect.horizontal = !IsVertical;
            _ScrollRect.vertical = IsVertical;
        }
    }

    protected override void Start()
    {
        GNewTabList tab = GetComponentInParent<GNewTabList>();
        if (tab == null)
        {
            Preview(PageWidth * PageHeight);
        }
    }

    public void Preview(int count)
    {
        SafeInit();

        for (int i = 0; i < _viewItems.Count; i++)
        {
            Destroy(_viewItems[i].gameObject);
        }
        _viewItems.Clear();

        this._ItemCount = count;
        RecalculateBound();

        //_CellRect.localScale = Vector3.zero;

        for (int i = 0; i < _ItemCount; i++)
        {
            GListItem g_li = TryCreateItem();
            if (g_li != null)
            {
                SetPivotAnchor(g_li.RectTrans, _Align);
                g_li.RectTrans.anchoredPosition = Index2Pos2(i, g_li.RectTrans);
            }
            g_li.UpdateItem(i);
            //PostSetupItem(g_li);
            _viewItems.Add(g_li);
        }

        //_CellRect.localScale = Vector3.zero;
        //_CellRect.gameObject.SetActive(false);
        //PlayEffect();
    }

    //protected virtual void PostSetupItem(GListItem g_li)
    //{
    //}

    public virtual GListItem GetListItem(int index)
    {
        if (index > -1 && index < _viewItems.Count)
        {
            return _viewItems[index];
        }
        return null;
    }

    public virtual void RefreshItem(int index) { }

    public virtual void AddItem(int index, int count) { }

    public virtual void RemoveItem(int index, int count) { }

#endif
}
