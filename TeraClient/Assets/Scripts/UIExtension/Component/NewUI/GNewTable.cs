using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Common;


public sealed class GNewTable : GNewTableBase
{

    [NoToLua]
    [NonSerialized]
    public ItemDelegate InitItemCallBack;
    [NoToLua]
    [NonSerialized]
    public ItemDelegate ClickItemCallBack;

    protected override GListItem TryCreateItem()
    {
        if (CellTemplate == null) return null;

        GListItem rv = null;
        rv = _CellPool.TakeOut();

        if (rv == null)
        {
            RectTransform item = CUnityUtil.Instantiate(CellTemplate) as RectTransform;
            item.SetParent(_Content, false);

            GListItem itemCon = item.GetComponent<GListItem>();
            if (itemCon == null)
            {
                itemCon = item.gameObject.AddComponent<GListItem>();
            }

            //register events
            if (this.InitItemCallBack != null)
            {
                itemCon.OnItemInit = this.OnShowItem;
            }

            if (this.ClickItemCallBack != null)
            {
                itemCon.OnItemClick = this.OnClickItem;
            }

            rv = itemCon;
        }

        GNewUITools.SetVisible(rv.RectTrans, true);

        return rv;
    }

    private void OnClickItem(GameObject item, int index)
    {
        if (ClickItemCallBack != null)
        {
            ClickItemCallBack(this.gameObject, item, index);
        }
    }

    private void OnShowItem(GameObject item, int index)
    {
        if (InitItemCallBack != null)
        {
            InitItemCallBack(this.gameObject, item, index);
        }
    }

    public void AddItem(int index)
    {
        if (index < 0 || index > _ItemCount)
        {
            HobaDebuger.LogWarning("<GNewTable> AddItem: index out of range! ");
            return;
        }

        //InsertItem(_ViewItems, index);

        GListItem item = null;
        item = TryCreateItem();
        if (item != null)
        {
            _ViewItems.Insert(index, item);
            _ItemCount = _ViewItems.Count;

            //OnAddItem(item.gameObject, item.index);
            item.UpdateItem(index, true);

            for (int i = index + 1, max = _ViewItems.Count; i < max; i++)
            {
                _ViewItems[i].index = i;
            }
            RepositionItems();
        }
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= _ItemCount)
        {
            HobaDebuger.LogWarning("<GNewTable> removeItem: index out of range! ");
            return;
        }

        DisposeItem(_ViewItems, index);
        _ItemCount = _ViewItems.Count;

        for (int i = index, max = _ViewItems.Count; i < max; i++)
        {
            _ViewItems[i].index = i;
        }
        RepositionItems();
    }

}
