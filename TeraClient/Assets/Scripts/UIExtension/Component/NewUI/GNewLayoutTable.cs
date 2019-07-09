using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Common;


public sealed class GNewLayoutTable : GNewTableBase
{
    #region serialization
    public RectTransform poolNode;
    #endregion serialization

#if IN_GAME
    //[NoToLua]
    //public delegate void ItemDelegate(GameObject list, GameObject item, int index);

    [NoToLua]
    [NonSerialized]
    public ItemDelegate InitItemCallBack;
    [NoToLua]
    [NonSerialized]
    public ItemDelegate ClickItemCallBack;

    private int _MainSelection = -1;
    //private int _LastMainSelection = -1;
    public int MainSelected { private set { _MainSelection = value; } get { return _MainSelection; } }
    //public int LastMainSelected { private set { _LastMainSelection = value; } get { return _LastMainSelection; } }

    protected override void OnSafeInit()
    {
    	base.OnSafeInit();
    	if (poolNode == null)
    	{
            poolNode = RectTrans;
    	}

    	CellTemplate.SetParent(poolNode, false);
    }

    protected override GListItem TryCreateItem()
    {
        if (CellTemplate == null) return null;

        GListItem rv = null;
        rv = _CellPool.TakeOut();

        if (rv == null)
        {
            GameObject item = CUnityUtil.Instantiate(CellTemplate.gameObject) as GameObject;
            GListItem itemCon = GListItem.Get(item);
            rv = itemCon;

            if (HasChildButton && ClickItemButtonCallBack != null)
            {
                rv.OnItemClickButton = OnClickItemButton;
            }

            //register events
            if (InitItemCallBack != null)
            {
                rv.OnItemInit = OnShowItem;
            }

            if (ClickItemCallBack != null)
            {
                rv.OnItemClick = OnClickItem;
            }
        }

        rv.RectTrans.SetParent(_Content, false);
        GNewUITools.SetVisible(rv.RectTrans, true);

        return rv;
    }

    protected override void DisposeItem(List<GListItem> items, int index)
    {
        if (items.Count > index)
        {
            GListItem item = items[index];
            if (item != null)
            {
                if (_CellPool.PutIn(item))
                {
                    GNewUITools.SetVisible(item.RectTrans, false);

                    item.RectTrans.SetParent(poolNode, false);
                }
                else
                {
                    Destroy(item.gameObject);
                }
            }
            items.RemoveAt(index);
        }
    }

    public void AddItem(int index)
    {
        if (index < 0 || index > _ItemCount)
        {
            HobaDebuger.LogWarning("<GNewLayoutTable> AddItem: index out of range! ");
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
                _ViewItems[i].UpdateItem(i, false);
                _ViewItems[i].RectTrans.SetAsLastSibling();
            }

            if (_MainSelection >= index)
            {
                _MainSelection += 1;
            }

            //RepositionItems();
        }
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= _ItemCount)
        {
            HobaDebuger.LogWarning("<GNewLayoutTable> removeItem: index out of range! ");
            return;
        }

        if (MainSelected == index)
        {
            SetSelection(-1);
        }

        DisposeItem(_ViewItems, index);
        _ItemCount = _ViewItems.Count;

        for (int i = index; i < _ViewItems.Count; i++)
        {
            _ViewItems[i].UpdateItem(i, false);
        }
        //RepositionItems();
    }

    public void SelectItem(int main_index)
    {
        SetSelection(main_index);

        if (main_index != -1)
        {
            GListItem git = GetListItem(MainSelected);
            if (git != null)
            {
                if (ClickItemCallBack != null)
                {
                    ClickItemCallBack(this.gameObject, git.gameObject, main_index);
                }
            }
        }
    }

    public void RefreshItem(int i_index)
    {
        GListItem git = GetListItem(i_index);
        if (git != null)
        {
            //OnShowItem(git.gameObject, git.index);
            git.UpdateItem(i_index, true);
        }
    }

    public override void SetItemCount(int count)
    {
        if (count < 0)
        {
            Debug.LogError("GNewTableBase:SetItemCount < 0 " + count);
            return;
        }

        MainSelected = -1;
        base.SetItemCount(count);
    }

    private void OnClickItem(GameObject item, int index)
    {
        SetSelection(index);
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

    private void SwitchItem(GListItem item, bool on)
    {
        if (item != null)
        {
            item.IsOn = on;
        }
    }

    private void SetSelection(int main_index)
    {
        int last_MainSelected = MainSelected;

        if (MainSelected != main_index)
        {
            MainSelected = main_index;

            GListItem git = GetListItem(last_MainSelected);
            SwitchItem(git, false);

            git = GetListItem(MainSelected);
            SwitchItem(git, true);
        }
    }

    public override void ScrollToStep(int index)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_Content);
        base.ScrollToStep(index);
    }

#else
    protected override void OnSafeInit()
    {
        base.OnSafeInit();
        ScrollRect scrollRect = GetComponent<ScrollRect>();

        for (int i = 0; i < 8; i++)
        {
            RectTransform item = CUnityUtil.Instantiate(CellTemplate) as RectTransform;
            item.SetParent(scrollRect.content, false);
        }
    }
#endif //IN_GAME
}
