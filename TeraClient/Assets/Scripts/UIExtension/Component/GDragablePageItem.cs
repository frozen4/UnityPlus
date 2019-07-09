using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class GDragablePageItem : GBase, IPointerClickHandler
{
    [NoToLua]
    public delegate void PageItemDelegate(GameObject go, int index);

    [NoToLua]
    [System.NonSerialized]
    public int index;

    [NoToLua]
    protected PageItemDelegate _OnPageItemInit;

    [NoToLua]
    protected PageItemDelegate _OnPageItemClick;

    [NoToLua]
    protected PageItemDelegate _OnPageItemClickButton;

    public PageItemDelegate OnItemInit
    {
        get { return _OnPageItemInit; }
        set { _OnPageItemInit = value; }
    }

    public PageItemDelegate OnItemClick
    {
        get { return _OnPageItemClick; }
        set { _OnPageItemClick = value; }
    }

    public PageItemDelegate OnItemClickButton
    {
        get { return _OnPageItemClickButton; }
        set
        {
            _OnPageItemClickButton = value;
            InitChildButtons();
        }
    }

    public virtual void OnInitPageItem(int index, bool bNeedInit)
    {
        this.index = index;
        this.gameObject.name = "page_item_" + index;
        if (bNeedInit && this.OnItemInit != null)
        {
            this.OnItemInit(gameObject, index);
        }
    }

    private void OnItemButtonClicked(GameObject go)
    {
        if (OnItemClickButton != null)
            OnItemClickButton(go, index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_OnPageItemClick != null)
        {
            _OnPageItemClick(gameObject, index);
        }
    }

    private void InitChildButtons()
    {
        GButton[] gbtns = gameObject.GetComponentsInChildren<GButton>(true);
        for (int i = 0; i < gbtns.Length; i++)
        {
            gbtns[i].OnClick += OnItemButtonClicked;
        }
        Button[] buttons = gameObject.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button btn = buttons[i];

            //this doesnt leak
            btn.onClick.AddListener(delegate() { OnItemButtonClicked(btn.gameObject); });
        }
    }
    [NoToLua]
    public void SetPosition(Vector2 new_pos)
    {
        if (RectTrans != null)
        {
            RectTrans.anchoredPosition = new_pos;
        }
    }
}
