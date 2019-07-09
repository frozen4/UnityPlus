using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

//TabList 
//每个Tab展开为一个List 
//只能单选 
//只有竖排 
//只有两层 

//Lua
//SetItemCount
//OpenTab


public sealed class GNewTabList : GNewTableBase
{
    //[NoToLua]
    public new delegate void ItemDelegate(GameObject list, GameObject item, int index1, int index2);

    #region serialization
    [NoToLua]
    public RectTransform SubViewTemplate; //当前节点的子节点的模板

    [NoToLua]
    public bool DefaultSubNotSelected;

    [NoToLua]
    public bool IsManualSelect;
    #endregion serialization

    private GNewList _SubList;

    [NoToLua]
    [NonSerialized]
    public ItemDelegate InitItemCallBack;
    [NoToLua]
    [NonSerialized]
    public ItemDelegate ClickItemCallBack;
    [NoToLua]
    [NonSerialized]
    public ItemDelegate ClickSubListItemButtonCallBack;

    private int _MainSelection = -1;
    private int _SubSelection = -1;
    private int _LastMainSelection = -1;
    private int _LastSubSelection = -1;

    public int MainSelected { private set { _MainSelection = value; } get { return _MainSelection; } }
    public int SubSelected { private set { _SubSelection = value; } get { return _SubSelection; } }
    public int LastMainSelected { private set { _LastMainSelection = value; } get { return _LastMainSelection; } }
    public int LastSubSelected { private set { _LastSubSelection = value; } get { return _LastSubSelection; } }

    public override void SetItemCount(int count)
    {
        if (count < 0)
        {
            Debug.LogError("GNewTableBase:SetItemCount < 0 " + count);
            return;
        }

        MainSelected = -1;
        SubSelected = -1;
        LastMainSelected = -1;
        LastSubSelected = -1;

        OpenTab(0, false);

        base.SetItemCount(count);
    }

    public void OpenTab(int count)
    {
        if (IsInited)
        {
            OpenTab(count, true);
        }
    }

    public void CloseTab()
    {
        if (IsInited)
        {
            OpenTab(0, true);
        }
    }

    public void SelectItem(int main_index, int sub_index)
    {
        SetSelection(main_index, sub_index);

        if (main_index == -1)
        {
            //xxx Dont know yet...
        }
        else
        {
            GListItem git = GetListItem(MainSelected);
            if (git != null)
            {
                if (ClickItemCallBack != null)
                {
                    ClickItemCallBack(this.gameObject, git.gameObject, main_index, -1);
                }
            }

            if (_SubList != null)
            {
                if (SubSelected != -1)
                {
                    git = _SubList.GetListItem(SubSelected);
                    if (git != null)
                    {
                        ClickItemCallBack(this.gameObject, git.gameObject, main_index, sub_index);
                    }
                }

                //_SubList.ScrollToStep(SubSelected);
            }

            ScrollToStep(MainSelected);
        }
    }

    public override void PlayEffect()
    {
        base.PlayEffect();

        //if (IsInited)
        //{
        //    if (_SubList != null)
        //    {
        //        _SubList.PlayEffect();
        //    }
        //}
    }

    //public void UpdateSelection(int index1, int index2)
    //{
    //    SetSelection(index1, index2);
    //}

    float CalcPosY(int id)
    {
        GListItem item = GetListItem(id);
        if (item)
        {
            Rect old_subRT = item.RectTrans.rect;
            Vector3 old_subPos = item.RectTrans.localPosition;

            return old_subPos.y + old_subRT.yMax;
        }

        return 0;
    }


    private void OpenTab(int count, bool needRepos)
    {
        if (SubViewTemplate != null && _SubList != null)
        {
            if (MainSelected == -1)
            {
                count = 0;
            }

            float y_oldPos = CalcPosY(MainSelected);

            _SubList.SetItemCount(count);

            if (count > 0)
            {
                //Vector2 ori_size = SubViewTemplate.sizeDelta;
                ////Vector2 ori_size = Vector2.zero;//_subList.RectTrans.sizeDelta;
                //float rows = (count > 5) ? 5.5f : count;

                //Vector4 vb = GNewUITools.GetScrollViewBorder(_SubList.ScrollRectControl);

                ////ori_size.x = RectTrans.sizeDelta.x - 2 * _spacing.x;
                //ori_size.y = _SubList.Spacing.y * Mathf.Floor(rows + 1) + _SubList.CellRect.sizeDelta.y * rows + vb.w + vb.y;
                //SubViewTemplate.sizeDelta = ori_size;

                //_SubList.ScrollToStep(SubSelected);

                GUISound.PlaySoundClip(GUISound.PlayType.RollDown);
            }
            else
            {
                SubViewTemplate.sizeDelta = new Vector2(SubViewTemplate.sizeDelta.x, 0);
            }

            if (needRepos)
            {
                RepositionItems();

                //fix position after sub list changing, MainSelected will remain in the same place
                float y_newPos = CalcPosY(MainSelected);
                ScrollToPosition(_Content.anchoredPosition.y + y_oldPos - y_newPos);
            }
        }
    }

    protected override void OnSafeInit()
    {
        base.OnSafeInit();

        if (SubViewTemplate != null)
        {
            SubViewTemplate.SetParent(_Content, false);

            _SubList = SubViewTemplate.GetComponentInChildren<GNewList>(true);
            _SubList.InitItemCallBack = OnShowSubItem;
            _SubList.ClickItemCallBack = OnClickSubItem;
            _SubList.ClickItemButtonCallBack = OnClickSubItemButton;
            //_SubList.DontKeepFullSize = true;
        }

        OpenTab(0, false);
    }

    public void SetSelection(int index1, int index2)
    {
        //if (index2 == -1 && index1 == MainSelected)
        //{
        //    index1 = -1;
        //}

        LastMainSelected = MainSelected;

        if (MainSelected != index1)
        {
            MainSelected = index1;

            LastSubSelected = -1;
            SubSelected = index2;
            if (SubSelected == -1)
            {
                SubSelected = DefaultSubNotSelected ? -1 : 0;
            }

            GListItem git = GetListItem(LastMainSelected);
            SwitchItem(git, false);

            git = GetListItem(MainSelected);
            SwitchItem(git, true);
        }
        else
        {
            if (_SubList != null)
            {
                if (SubSelected != index2 && index2 != -1)
                {
                    LastSubSelected = SubSelected;
                    SubSelected = index2;

                    GListItem git = _SubList.GetListItem(LastSubSelected);
                    SwitchItem(git, false);

                    git = _SubList.GetListItem(SubSelected);
                    SwitchItem(git, true);
                }
            }
        }
    }

    private void SwitchItem(GListItem item, bool on)
    {
        if (item != null)
        {
            item.IsOn = on;
            if (on)
            {
                GUISound.PlaySoundClip(GUISound.PlayType.RollDown);
            }
            else
            {
                GUISound.PlaySoundClip(GUISound.PlayType.RollUp);
            }
        }
    }

    protected override Vector2 GetItemSize(GListItem item, float x, float y, float fx, float fy)
    {
        Vector2 real_size = new Vector2();
        if (item != null)
        {
            RectTransform rt = item.RectTrans;

            item.SetPosition(AlignXY(rt, y, fy));
            //rt.anchoredPosition = AlignXY(rt, y, fy);
            //item.SetLocalPosition(AlignXY(rt, y, fy));

            real_size.x = rt.sizeDelta.x;
            real_size.y = rt.sizeDelta.y;

            y += real_size.y * fy;

            if (item.index == MainSelected)
            {
                //float dsx = SubViewTemplate.sizeDelta.x;
                //float dsy = SubViewTemplate.sizeDelta.y /* * rt.localScale.y*/;

                Vector2 size = SubViewTemplate.rect.size;

                SubViewTemplate.anchoredPosition = AlignXY(SubViewTemplate, y, fy);

                real_size.x = Mathf.Max(size.x, real_size.x);
                real_size.y += size.y;
            }
        }

        return real_size;
    }

    protected override GListItem TryCreateItem()
    {
        GListItem rv = base.TryCreateItem();
        if (rv != null)
        {
            //register events
            if (this.InitItemCallBack != null)
            {
                rv.OnItemInit = this.OnShowItem;
            }

            if (this.ClickItemCallBack != null)
            {
                rv.OnItemClick = this.OnClickItem;
            }
        }
        return rv;
    }

    private void OnClickSubItem(GameObject list, GameObject item, int index)
    {
        if (!IsManualSelect)
        {
            SetSelection(MainSelected, index);
        }

        if (ClickItemCallBack != null)
        {
            ClickItemCallBack(this.gameObject, item, MainSelected, index);
        }
    }

    private void OnShowSubItem(GameObject list, GameObject item, int index)
    {
        if (_SubList == null) return;
        GListItem git = _SubList.GetListItem(index);
        if (git != null)
        {
            git.IsOn = git.index == SubSelected;
        }

        if (InitItemCallBack != null)
        {
            InitItemCallBack(this.gameObject, item, MainSelected, index);
        }
    }

    private void OnClickSubItemButton(GameObject list, GameObject item, int index)
    {
        if (!IsManualSelect)
        {
            SetSelection(MainSelected, index);
        }

        if (ClickSubListItemButtonCallBack != null)
        {
            ClickSubListItemButtonCallBack(this.gameObject, item, MainSelected, index);
        }
    }

    private void OnClickItem(GameObject item, int index)
    {
        if (!IsManualSelect)
        {
            SetSelection(index, -1);
        }

        if (ClickItemCallBack != null)
        {
            ClickItemCallBack(this.gameObject, item, index, -1);
        }
    }
    private void OnShowItem(GameObject item, int index)
    {
        GListItem git = GetListItem(index);
        if (git != null)
        {
            git.IsOn = git.index == MainSelected;
        }
        if (InitItemCallBack != null)
        {
            InitItemCallBack(this.gameObject, item, index, -1);
        }
    }

    //[NoToLua]
    //public override void OnPopEffectStart()
    //{
    //    SafeInit();

    //    if (_ScrollRect != null)
    //    {
    //        _ScrollRect.enabled = false;
    //    }

    //    if (_SubList != null)
    //    {
    //        _SubList.OnPopEffectStart();
    //    }
    //}

    //[NoToLua]
    //public override void OnPopEffectOver()
    //{
    //    if (_ScrollRect != null)
    //    {
    //        _ScrollRect.enabled = true;
    //    }

    //    if (_SubList != null)
    //    {
    //        _SubList.OnPopEffectOver();
    //    }
    //}
}
