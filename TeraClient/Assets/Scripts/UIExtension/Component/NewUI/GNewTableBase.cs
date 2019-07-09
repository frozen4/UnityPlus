using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;



public abstract class GNewTableBase : GNewUIBase //, IGPopUIEffectHandler
{
    [NoToLua]
    public delegate void ItemDelegate(GameObject list, GameObject item, int index);

    #region serialization
    [NoToLua]
    public RectTransform CellTemplate; //当前节点的子节点的模板

    [NoToLua]
    public bool HasChildButton;

    [SerializeField]
    protected Vector2 _Spacing = Vector2.one;
    #endregion serialization

    [NoToLua]
    [NonSerialized]
    public ItemDelegate ClickItemButtonCallBack;

    protected ScrollRect _ScrollRect;
    protected RectTransform _Content;

    protected GNewPrivatePool<GListItem> _CellPool;
    protected List<GListItem> _ViewItems;

    protected int _ItemCount;

    //protected bool _IsInited = false;

    //public float ItemAnimDelay;		//delay for tween item by index

    public int ItemCount { get { return _ItemCount; } }
    [NoToLua]
    public Vector2 Spacing { get { return _Spacing; } set { _Spacing = value; /*IsDirty = true;*/} }

    public virtual void ScrollToStep(int index)
    {
        if (IsInited)
        {
            GListItem item = GetListItem(index);
            if (item != null)
            {
                ScrollToPosition(Mathf.Abs(item.RectTrans.anchoredPosition.y) - Spacing.y);
            }
        }
    }

    [NoToLua]
    public void ScrollToPosition(float pos)
    {
        if (IsInited)
        {
            _ScrollRect.StopMovement();
            Vector2 v = _Content.anchoredPosition;
            v.y = GNewUITools.ClampScrollPos(pos, _Content, _ScrollRect);

            _Content.anchoredPosition = v;
        }
    }

    public virtual void SetItemCount(int count)
    {
        if (count < 0)
        {
            Debug.LogError("GNewTableBase:SetItemCount < 0 " + count);
            return;
        }

        SafeInit();

        _ItemCount = count;

        //try to best fit in memory
        int size = Mathf.Min(count, 64);
        if (_ViewItems.Capacity < size)
        {
            _ViewItems.Capacity = size;
        }

        UpdateContents();
        RepositionItems();
    }

    public GameObject GetItem(int index)
    {
        if (IsInited)
        {
            GListItem g_item = GetListItem(index);
            if (g_item != null)
            {
                return g_item.gameObject;
            }
        }
        return null;
    }

    protected override void OnSafeInit()
    {
        base.OnSafeInit();
        //Transform t = transform;
        if (_ScrollRect == null)
        {
            _ScrollRect = GetComponent<ScrollRect>();
        }
        if (_ScrollRect != null)
        {
            _Content = _ScrollRect.content;
        }
        else
        {
            _Content = RectTrans;
        }

        _CellPool = new GNewPrivatePool<GListItem>();
        _CellPool.SetSize(10);

        if (CellTemplate != null)
        {
            CellTemplate.localScale = Vector3.zero;
        }
        else
        {
            Debug.LogError("GNewTable: CellTemplate is Null!!!");
        }

        _ViewItems = new List<GListItem>();
    }

    protected GListItem GetListItem(int index)
    {
        if (index > -1 && index < _ViewItems.Count)
            return _ViewItems[index];
        return null;
    }

    protected void UpdateContents()
    {
        for (int i = 0; i < ItemCount; i++)
        {
            UpdateItem(_ViewItems, i);
        }

        while (_ViewItems.Count > ItemCount)
        {
            DisposeItem(_ViewItems, _ViewItems.Count - 1);
        }
    }

    protected void RepositionItems()
    {
        int i;
        //int index;
        float width = _Content.sizeDelta.x;
        float height = 0;
        float offset = 0;
        float x = 0;
        float y = 0;
        //float max;

        float fx = 1;
        float fy = -1;

        //x = _Spacing.x;
        //y = _Spacing.y * fy;

        for (i = 0; i < _ViewItems.Count; i++)
        {
            //index = i;
            GListItem item = _ViewItems[i];
            if (item == null || !item.gameObject.activeSelf /*|| layout.RectTrans.sizeDelta.x <= 0 || layout.RectTrans.sizeDelta.y <= 0*/|| item.RectTrans.localScale.x < Mathf.Epsilon) continue;

            Vector2 cell_size = GetItemSize(item, x, y, fx, fy);

            //max = Mathf.Abs(x) + cell_size.x;
            if (cell_size.y > offset)  //一行中最高的那一个
            {
                offset = cell_size.y;
            }

            y += offset * fy;
            if (i < _ViewItems.Count - 1)
            {
                y += _Spacing.y * fy;
            }
            offset = 0;
        }
        //width += _Spacing.x;
        y += offset * fy;
        y += _Spacing.y * fy;
        height = Mathf.Abs(y);

        _Content.sizeDelta = new Vector2(width, height);
    }

    protected Vector2 AlignXY(RectTransform rt, float y, float fy)
    {
        Vector2 rc = rt.rect.size;
        Vector2 an = (rt.anchorMax + rt.anchorMin) * 0.5f;

        Vector2 new_pos;
        new_pos.x = (1 - an.x * 2) * _Spacing.x;
        new_pos.y = y + (fy > 0 ? rt.pivot.y : 1 - rt.pivot.y) * rc.y;

        return new_pos;
    }

    protected virtual Vector2 GetItemSize(GListItem item, float x, float y, float fx, float fy)
    {
        Vector2 real_size = new Vector2();
        if (item != null)
        {
            RectTransform rt = item.RectTrans;
            item.SetPosition(AlignXY(rt, y, fy));
            //item.SetLocalPosition(AlignXY(rt, y, fy));
            //rt.anchoredPosition = AlignXY(rt, y, fy);

            real_size = rt.rect.size;

            y += real_size.y * fy;
        }

        return real_size;
    }

    protected virtual void UpdateItem(List<GListItem> items, int pos)
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
            if (item != null)
            {
                items.Add(item);
                //OnAddItem(item.gameObject, item.index);
            }
        }

        if (item != null)
        {
            item.UpdateItem(pos, true);
        }
    }

    protected virtual void DisposeItem(List<GListItem> items, int index)
    {
        if (items.Count > 0)
        {
            GListItem item = items[items.Count - 1];
            if (item != null)
            {
                if (_CellPool.PutIn(item))
                {
                    GNewUITools.SetVisible(item.RectTrans, false);
                }
                else
                {
                    Destroy(item.gameObject);
                }
            }
            items.RemoveAt(index);
        }
    }

    protected virtual GListItem TryCreateItem()
    {
        if (CellTemplate == null) return null;

        GListItem rv = null;
        rv = _CellPool.TakeOut();

        if (rv == null)
        {
            GameObject item = CUnityUtil.Instantiate(CellTemplate.gameObject) as GameObject;
            GListItem itemCon = GListItem.Get(item);
            itemCon.RectTrans.SetParent(_Content, false);

            rv = itemCon;

            if (HasChildButton && this.ClickItemButtonCallBack != null)
            {

                itemCon.OnItemClickButton = this.OnClickItemButton;
            }
        }

        GNewUITools.SetVisible(rv.RectTrans, true);

        return rv;
    }

    protected virtual void OnClickItemButton(GameObject item, int index)
    {
        if (ClickItemButtonCallBack != null)
        {
            ClickItemButtonCallBack(this.gameObject, item, index);
        }
    }

	public void EnableScroll(bool enable)
	{
		SafeInit();

		if (_ScrollRect != null)
		{
			_ScrollRect.enabled = enable;
		}
	}

    //[NoToLua]
    //public virtual void OnPopEffectStart()
    //{
    //    SafeInit();
    //    if (_ScrollRect != null)
    //    {
    //        _ScrollRect.enabled = false;
    //    }
    //}

    //[NoToLua]
    //public virtual void OnPopEffectOver()
    //{
    //    if (_ScrollRect != null)
    //    {
    //        _ScrollRect.enabled = true;
    //    }
    //}

    public virtual void PlayEffect()
    {
        //if (IsInited)
        //{
        //    for (int i = 0; i < _ViewItems.Count; i++)
        //    {
        //        GListItem gi = _ViewItems[i];
        //        if (gi != null)
        //        {
        //            gi.PlayTween(ItemAnimDelay * i);
        //        }
        //    }
        //}
    }
}
