using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using DG.Tweening;
using Common;

public sealed class GNewList : GNewListBase
{
    #region all members

    private List<GListItem> _viewItems = new List<GListItem>();
    //private GNewPrivatePool<GListItem> _pool = new GNewPrivatePool<GListItem>(64);
    #endregion all members

    #region funcs - sequenced with workflow
    public override void SetItemCount(int count)
    {
        if (count < 0)
        {
            Debug.LogError("GNewListBase:SetItemCount < 0 " + count);
            return;
        }

        //try to best fit in memory
        int size = Mathf.Min(count, 64);
        if (_viewItems.Capacity < size)
        {
            _viewItems.Capacity = size;
        }

        base.SetItemCount(count);
    }

    //no base call
    public override void RefreshItem(int index)
    {
        SafeInit();

        if (index < 0 || index >= _viewItems.Count)
        {
            return;
        }
        else
        {
            UpdateItem(_viewItems, index, true);
        }
    }

    //no-base call
    public override void AddItem(int index, int count)
    {
        SafeInit();

        if (index < 0 || index > _ItemCount)
        {
            HobaDebuger.LogWarning("<GNewList> AddItem: index out of range! ");
            return;
        }

        if (count < 1)
        {
            return;
        }

        _ItemCount += count;
        RecalculateBound();

        //InternalUpdateContent(index);

        for (int i = 0; i < count; i++)
        {
            GListItem g_item = TryCreateItem();
            _viewItems.Insert(index + i, g_item);
            UpdateItem(_viewItems, index + i, true);
        }

        for (int i = index + count, max = _viewItems.Count; i < max; i++)
        {
            UpdateItem(_viewItems, i, false);
        }
    }

    //no-base call
    public override void RemoveItem(int index, int count)
    {
        if (!IsInited) return;

        if (index < 0 || index >= _ItemCount)
        {
            HobaDebuger.LogWarning("<GNewList> RemoveItem: index out of range! ");
            return;
        }

        if (count < 1)
        {
            return;
        }

        _ItemCount -= count;
        if (_ItemCount < index) _ItemCount = index;
        RecalculateBound();

        while (_viewItems.Count > _ItemCount)
        {
            DisposeItem(_viewItems, index);
        }

        for (int i = index, max = _viewItems.Count; i < max; i++)
        {
            UpdateItem(_viewItems, i, false);
        }
    }

    [NoToLua]
    public override GListItem GetListItem(int index)
    {
        if (IsInited)
        {
            if (index > -1 && index < _viewItems.Count)
            {
                GListItem item = _viewItems[index];
                if (item != null)
                {
                    return item;
                }
            }
        }
        return null;
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

    //public override void ScrollToStep(int scrollStep)
    //{
    //    base.ScrollToStep(scrollStep);
    //}

    //protected override void OnFinishTweenItem(int index, TweenDirection dir)
    //{
    //    //Tweener ter;
    //    //for (int i = index; i < _viewItems.Count; i++)
    //    //{
    //    //	if (dir == TweenDirection.IN)
    //    //	{
    //    //		_viewItems[i].index = i + 1;

    //    //		RectTransform rt = _viewItems[i].RectTrans;
    //    //		// DOTween.Kill(rt.gameObject);
    //    //		ter = rt.DOAnchorPosY(rt.anchoredPosition.y + CellDiv + SpacingDiv, 0.3f).SetEase(Ease.OutBack);
    //    //	}
    //    //	else
    //    //	{
    //    //		_viewItems[i].index = i - 1;

    //    //		RectTransform rt = _viewItems[i].RectTrans;
    //    //		// DOTween.Kill(rt.gameObject);
    //    //		ter = rt.DOAnchorPosY(rt.anchoredPosition.y + CellDiv + SpacingDiv, 0.3f).SetEase(Ease.OutBack);
    //    //	}
    //    //}
    //}

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _viewItems.Clear();
        _viewItems = null;
    }

    //no-base call
    protected override void UpdateContents()
    {
        InternalUpdateContent(0);
    }

    private void InternalUpdateContent(int unchangCount)
    {
        for (int i = unchangCount; i < ItemCount; i++)
        {
            UpdateItem(_viewItems, i, true);
        }

        while (_viewItems.Count > ItemCount)
        {
            DisposeItem(_viewItems, _viewItems.Count - 1);
        }
    }

    #endregion

    #region Internal process

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

    private void UpdateItem(List<GListItem> items, int pos, bool is_need_init)
    {
        //Debug.Log("UpdateItem " + pos);

        GListItem item = null;
        if (items.Count > pos)
        {
            item = items[pos];
        }
        else
        {
            item = TryCreateItem();
            items.Add(item);
            //OnAddItem(item.gameObject, item.index);
        }

        AdjustItemAnchor(item.RectTrans);
        item.SetPosition(Index2Pos2(pos, item.RectTrans));
        //item.RectTrans.anchoredPosition = Index2Pos2(pos, item.RectTrans);
        //item.SetLocalPosition(Index2Pos2(pos, item.RectTrans));

        item.UpdateItem(pos, is_need_init);

        if (SingleSelect)
        {
            item.IsOn = (_SingleSelectIndex == pos);
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
    #endregion Internal process
}
