using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Common;

public sealed class GNewListLoop : GNewListBase
{
    struct IntPack2
    {
        private int _a;
        private int _b;

        public int x { get { return _a; } set { _a = value; } }
        public int y { get { return _b; } set { _b = value; } }

        public int left { get { return _a; } set { _a = value; } }
        public int length { get { return _b; } set { _b = value; } }
        public int right { get { return _a + _b - 1; } }


        public static bool operator !=(IntPack2 a, IntPack2 b)
        {
            return a._a != b._a || a._b != b._b;
        }
        public static bool operator ==(IntPack2 a, IntPack2 b)
        {
            return !(a != b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is IntPack2))//if obj is not StructerPoint Type ,obviuosly return flase
                return false;
            return Equals((IntPack2)obj);//it have been ensure that it is StructerPoint Type at above codes,so this cast is safe and will not throw any exception
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", _a, _b);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public bool Equals(IntPack2 obj)
        {
            if (this._a != obj._b)
                return false;
            if (this._b != obj._b)
                return false;
            return true;
        }

        public IntPack2(int a, int b)
        {
            _a = a;
            _b = b;
        }

        public void Zerofy()
        {
            _a = 0; _b = 0;
        }

        public int GetOffset(int p)
        {
            return p - _a;
        }

        public int Offset(int i)
        {
            return _a + i;
        }

        public IntPack2 Clamp(IntPack2 other)
        {
            IntPack2 newPack = new IntPack2();
            newPack._a = Mathf.Max(_a, other._a);
            newPack._b = Mathf.Min(_a + _b, other._a + other._b) - newPack._a;
            return newPack;
        }

    }

    #region all members

    #region serialization

    //show items early before pos, times of column width
    public float PreOutLook = 1f;

    #endregion serialization

    //scroll pos
    private List<GListItem> _viewItems = new List<GListItem>(0);
    //protected List<GListItem> _poolItems = new List<GListItem>();
    //GNewPrivatePool<GListItem> _pool = new GNewPrivatePool<GListItem>(64);
    private IntPack2 _viewRange;
    RectTransform _viewRect;

    #endregion all members

    #region all proprties

    //scroll steps
    private int ScrollSteps { get { return IsVertical ? (int)_RealBound.y : (int)_RealBound.x; } }
    private int PageStepLen { get { return IsVertical ? PageHeight : PageWidth; } }

    #endregion all proprties

    #region interfaces and inhirents - sequenced with workflow

    protected override void OnSafeInit()
    {
        base.OnSafeInit();

        if (PreOutLook < 1)
        {
            PreOutLook = 1;
        }
    }

    public override void SetItemCount(int count)
    {
        ////update page size
        //UpdatePageSize();

        ////try to get best fit in memory
        //int size = Mathf.Min(count, PageSize);
        //if (_viewItems.Capacity < size)
        //{
        //    _viewItems.Capacity = size;
        //}

        base.SetItemCount(count);
    }

    private void UpdatePageSize()
    {
        //update page size
        if (_viewRect == null)
        {
            ScrollRect scr = ScrollRectControl;
            if (scr != null)
            {
                _viewRect = scr.viewport;
                if (_viewRect == null) _viewRect = scr.GetComponent<RectTransform>();
            }
        }

        if (_viewRect == null)
        {
            Debug.LogError("GNewListLoop wont work without a ScrollRect!!!" + name);
            enabled = false;
            return;
        }

        if (IsVertical)
        {
            int h = Mathf.CeilToInt(_viewRect.rect.height / CellDiv + PreOutLook * 2) + 1;
            //if (PageHeight < h)
            PageHeight = h;
        }
        else
        {
            int w = Mathf.CeilToInt(_viewRect.rect.width / CellDiv + PreOutLook * 2) + 1;
            //if (PageWidth < w)
            PageWidth = w;
        }
    }

    //no-base call
    public override void Repaint()
    {
        SafeInit();

        RecalculateBound();
        _viewRange.Zerofy();
        UpdateContents();
        //IsDirty = false;
    }

    //TODO:  Swap unchanged data and update pos only,
    //but i can see there'll be needs to update the Rank or something else.

    //no-base call
    public override void RefreshItem(int index)
    {
        SafeInit();

        int offset = index - _viewRange.left * PageDiv;

        if (offset < 0 || offset >= _viewItems.Count)
        {
            return;
        }
        else
        {
            UpdateItem(_viewItems, offset, OffsetByPage(_viewRange, offset));
        }
    }

    //no-base call
    public override void AddItem(int index, int count)
    {
        SafeInit();

        if (index < 0 || index > _ItemCount)
        {
            HobaDebuger.LogWarning("<GNewListLoop> AddItem: index out of range! ");
            return;
        }

        if (count < 1)
        {
            return;
        }

        //update range
        _ItemCount += count;

        //add empty slot
        int max = Mathf.Min(_viewItems.Count + count, PageSize);
        int deltaSize = max - _viewItems.Count;
        for (int i = 0; i < deltaSize; i++)
        {
            _viewItems.Add(null);
        }

        int offset = GetPageOffset(_viewRange, index);
        if (offset < 0) offset = 0;
        for (int i = offset; i < _viewItems.Count; i++)
        {
            UpdateItem(_viewItems, i, OffsetByPage(_viewRange, i));
        }

        RecalculateBound();
        UpdateContents();
    }

    //no-base call
    public override void RemoveItem(int index, int count)
    {
        if (IsInited)
        {
            if (index < 0 || index >= _ItemCount)
            {
                HobaDebuger.LogWarning("<GNewListLoop> RemoveItem: index out of range! ");
                return;
            }

            if (count < 1)
            {
                return;
            }

            _ItemCount -= count;
            if (_ItemCount < index) _ItemCount = index;
            if (_ItemCount < 0) _ItemCount = 0;

            int offset = GetPageOffset(_viewRange, index);
            int max = GetPageOffset(_viewRange, ItemCount);

            if (offset < 0) offset = 0;

            for (int i = offset; i < _viewItems.Count && i < max; i++)
            {
                UpdateItem(_viewItems, i, OffsetByPage(_viewRange, i));
            }

            for (int i = _viewItems.Count; i > max && i > 0; i--)
            {
                DisposeItem(_viewItems, i - 1);
            }

            RecalculateBound();
            UpdateContents();
        }
    }

    //no-base call
    [NoToLua]
    public override GListItem GetListItem(int index)
    {
        if (IsInited)
        {
            int offset = index - _viewRange.left * PageDiv;
            if (offset > -1 && offset < _viewItems.Count)
            {
                GListItem item = _viewItems[offset];
                return item;
            }
        }
        return null;
    }

    public override void ScrollToStep(int scrollStep)
    {
        base.ScrollToStep(scrollStep / PageDiv);
        UpdateContents();
    }

    public bool IsListItemVisible(int index, float err = 1f)
    {
        if (IsInited)
        {
            RectTransform rt_v = _ScrollRect.viewport;
            GListItem item = GetListItem(index);
            if (item != null)
            {
                Rect rect_child = GNewUITools.GetRelativeRect(rt_v, item.RectTrans);
                Rect rect_v = rt_v.rect;

                if (IsVertical)
                {
                    bool is_inside = rect_child.yMin + err >= rect_v.yMin && rect_child.yMax - err <= rect_v.yMax;
                    return is_inside;
                }
                else
                {
                    bool is_inside = rect_child.xMin + err >= rect_v.xMin && rect_child.xMax - err <= rect_v.xMax;
                    return is_inside;
                }
            }
        }
        return false;
    }

    //public override void PlayEffect()
    //{
    //if (IsInited)
    //{
    //    for (int i = 0; i < _viewItems.Count; i++)
    //    {
    //        GListItem gi = _viewItems[i];
    //        if (gi != null)
    //        {
    //            gi.PlayTween(ItemAnimDelay * i);
    //        }
    //    }
    //}
    //}

    //no-base call
    protected override void AddLayoutElem()
    {
        Debug.LogWarning("List loop doesnt support this!");
    }

    protected override void UpdateContents()
    {
        UpdatePageSize();

        IntPack2 clampedRange = CalcViewRange();

        if (clampedRange != _viewRange)
        {
            UpdateViewRange(clampedRange.left);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _viewItems.Clear();
        _viewItems = null;
    }

    #endregion

    #region Internal process

    //Calc. index range
    private IntPack2 CalcViewRange()
    {
        float curPos = CurDirectionPos - (CellDiv) * PreOutLook;  //prevent blanks when moving too fast, so show item a bit earlier

        int scrollIndex = ScrollPos2ScrollStep(curPos);

        IntPack2 clampedRange = new IntPack2();
        clampedRange.left = scrollIndex + PageStepLen > ScrollSteps ? ScrollSteps - PageStepLen : scrollIndex;
        if (clampedRange.left < 0) clampedRange.left = 0;
        clampedRange.length = PageStepLen;
        return clampedRange;
    }


    private void Update()
    {
        if (/*IsDirty||*/ItemCount > 0) //preserve for delayed use
        {
            UpdateContents();
        }
    }

    private void UpdateViewRange(int scrollStep)
    {
        //if (scrollIndex < 0 || scrollIndex >= ScrollLength - PageLen) return;
        //if (_viewItems == null) return;

        IntPack2 target = new IntPack2();
        target.left = scrollStep;
        target.length = PageStepLen;

        //actual number of item in target view
        int viewCount = Mathf.Min(_ItemCount - target.left * PageDiv, PageSize);
        if (viewCount < 0) viewCount = 0;

        int deltaSize = viewCount - _viewItems.Count;
        for (int i = 0; i < deltaSize; i++)
        {
            _viewItems.Add(null);
        }

        IntPack2 intersection = target.Clamp(_viewRange);
        int sstart = 0;
        int scount = 0;
        int sdir = 0;

        //keep non-changed
        if (intersection.length > 0)
        {
            sstart = Mathf.Max(0, target.GetOffset(intersection.left) * PageDiv);
            scount = Mathf.Min(intersection.length * PageDiv, viewCount);
            sdir = target.GetOffset(_viewRange.left) * PageDiv;

            MoveItems(_viewItems, sstart - sdir, scount, sdir);
        }

        for (int i = 0; i < -deltaSize; i++)
        {
            DisposeItem(_viewItems, _viewItems.Count - 1);
        }

        //now in the form of C1 - a - NC - b - C2
        int a = sstart;
        int b = a + scount - 1;
        for (int i = 0; i < viewCount; i++)
        {
            if (i < a || i > b /*|| forceAll*/)
            {
                UpdateItem(_viewItems, i, OffsetByPage(target, i));
            }
        }

        _viewRange.left = scrollStep;
        _viewRange.length = PageStepLen;
    }

    private void SwapItem(List<GListItem> items, int destPos, int srcPos)
    {
        //Debug.Log("swap " + destPos + " " + srcPos);

        GListItem tmp = items[destPos];
        items[destPos] = items[srcPos];
        items[srcPos] = tmp;
    }

    private void MoveItems(List<GListItem> items, int start, int count, int dir)
    {
        if (dir != 0)
        {
            if (dir > 0) //>>
            {
                for (int i = start + count - 1; i >= start; i--)
                {
                    SwapItem(items, i + dir, i);
                }
            }
            else//<<
            {
                for (int i = start; i <= start + count - 1; i++)
                {
                    SwapItem(_viewItems, i + dir, i);
                }
            }
        }
    }

    //private void DisposeItem(List<GListItem> items)
    //{
    //    if (items.Count > 0)
    //    {
    //        GListItem item = items[items.Count - 1];
    //        if (item != null)
    //        {
    //            if (_pool.PutIn(item))
    //            {
    //                GNewUITools.SetVisible(item.RectTrans, false);
    //            }
    //            else
    //            {
    //                Destroy(item.gameObject);
    //            }
    //        }
    //        items.RemoveAt(items.Count - 1);
    //    }
    //}

    private void UpdateItem(List<GListItem> items, int pos, int index)
    {
        if (items.Count > pos)
        {
            //Debug.Log("UpdateItem " + pos + " " + index);
            if (items[pos] == null)
            {
                items[pos] = TryCreateItem();
            }

            GListItem item = items[pos];

            //dont want to wast time here
            //SetPivotAnchor(item.RectTrans, _Align);

            AdjustItemAnchor(item.RectTrans);

            item.SetPosition(Index2Pos2(index, item.RectTrans));
            //item.gameObject.SetActive(id < this._DataCount);
            //item.SetLocalPosition(Index2Pos2(index, item.RectTrans));

            item.UpdateItem(index, true);

            if (SingleSelect)
            {
                item.IsOn = (_SingleSelectIndex == index);
            }
        }
    }

    //private GListItem TryCreateItem()
    //{
    //    GListItem rv = null;
    //    rv = _pool.TakeOut();

    //    if (rv == null)
    //    {
    //        RectTransform item = CUnityUtil.Instantiate(_cellRect) as RectTransform;
    //        item.SetParent(transform, false);
    //        //item.anchorMax = Vector2.up;
    //        //item.anchorMin = Vector2.up;
    //        //item.pivot = Vector2.up;
    //        //SetPivotAnchor(item, _Align);
    //        //item.anchoredPosition = Vector2.zero;

    //        GListItem itemCon = item.GetComponent<GListItem>();
    //        if (itemCon == null)
    //        {
    //            itemCon = item.gameObject.AddComponent<GListItem>();
    //        }

    //        if (this.InitItemCallBack != null)
    //        {
    //            itemCon.OnItemInit = this.OnShowItem;
    //        }

    //        if (this.ClickItemCallBack != null)
    //        {
    //            itemCon.OnItemClick = this.OnClickItem;
    //        }

    //        if (this.ClickItemButtonCallBack != null)
    //        {
    //            itemCon.OnItemClickButton = this.OnClickItemButton;
    //        }
    //        rv = itemCon;
    //    }

    //    GNewUITools.SetVisible(rv.RectTrans, true);

    //    return rv;
    //}

    private int GetPageOffset(IntPack2 view, int index)
    {
        return index - view.left * PageDiv;
    }

    private int OffsetByPage(IntPack2 view, int offset)
    {
        return offset + view.left * PageDiv;
    }

    #endregion Internal process


}
