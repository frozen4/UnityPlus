using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class GDragablePageView : GNewUIBase, IBeginDragHandler, IEndDragHandler
{
    [NoToLua]
    public enum Direction
    {
        Horizontal = 1,
        Vertical
    }

    [NoToLua]
    public ScrollRect _ScrollRect;
    [NoToLua]
    public RectTransform _CellItem;
    private RectTransform _ScrollRectTrans;

    [SerializeField]
    private Direction _Direction = Direction.Horizontal;
    protected Vector4 _RealBound;

    private GNewPrivatePool<GDragablePageItem> _Pool;       //缓存GDragablePageItem的池子
    private List<GDragablePageItem> _Items;                 //items

    [NoToLua]
    public bool _HasChildButton = false;
    [NoToLua]
    public float _Smooting = 4;                             //滑动速度


    [NoToLua]
    public bool IsVertical 
    { 
        get { return _Direction == Direction.Vertical; }
        set { _Direction = (value == true) ? Direction.Vertical : Direction.Horizontal; } 
    }

    [NoToLua]

    protected virtual Vector2 CellSize { get { return _CellItem != null ? _CellItem.rect.size : new Vector2(100, 100); } }

    [NoToLua]
    public delegate void PageItemDelegate(GameObject pageView, GameObject pageItem, int index);
    [NoToLua]
    public PageItemDelegate _PageItemInitCallBack;          //初始化PageItem的回调事件
    [NoToLua]
    public PageItemDelegate _ClickPageItemCallBack;         //点击PageItem的回调事件
    [NoToLua]
    public PageItemDelegate _ClickPageItemButtonCallBack;   //点击Page里面的Button的回调事件
    [NoToLua]
    public PageItemDelegate _PageItemIndexChangeCallBack;   //Page Index 变化之后的回调事件

    private int _ItemCount;                                 //item的数量
    private int _CurItemIndex;                              //当前的PageItem的index
    private float _TimeInterval;                            //时间间隔
    private float _ListenTimeInterval;                      //监听ScrollRect的间隔时间
    private float _StartListenTime;                         //开始监听时间
    private float _TotalTime;                               //用于计时的time
    private bool _IsDraging;                                //是否正在拖拽中
    private bool _IsStopMove;                               //是否已经是停止差值移动了
    private float _StartMoveTime;                           //开始移动的时间
    private float _StartDragPos;                            //开始拖动的起始normalizeedPosition
    private float _LastScrollRectPos;                       //上次记录的ScrollRectPos的位置

    protected override void OnSafeInit()
    {
        base.OnSafeInit();
        if (_ScrollRect == null)
            _ScrollRect = GetComponent<ScrollRect>();
        _ScrollRectTrans = _ScrollRect.transform.GetComponent<RectTransform>();
        _Pool = new GNewPrivatePool<GDragablePageItem>(16);
        _Items = new List<GDragablePageItem>();
        _IsDraging = false;
        _IsStopMove = true;
        _TimeInterval = 0;
        _TotalTime = 0;
        _StartMoveTime = 0;
        _ItemCount = 0;
        _CurItemIndex = 0;
        _StartListenTime = 0;
        _ListenTimeInterval = 0.1f;
        _LastScrollRectPos = 0;
        _ScrollRect.onValueChanged.AddListener(OnScorllRectChanged);
        GNewUITools.SetVisible(_CellItem, false);
        //测试用
        //SetPageItemCount(6);
        //_TimeInterval = 3;
    }

    void OnScorllRectChanged(Vector2 pos)
    {
        if (_ItemCount <= 0) return;

        if (_IsStopMove && Time.time - _StartListenTime > _ListenTimeInterval)
        {
            _StartListenTime = Time.time;
            float perLength = 1.0f / _ItemCount;
            float p = 0;
            if (IsVertical)
                p = pos.y;
            else
                p = pos.x;
            int now_index = Mathf.Max(0, (int)(Mathf.Ceil(p / perLength) - 1));
            now_index = Mathf.Min(now_index, _ItemCount - 1);
            if (now_index != _CurItemIndex)
            {
                _CurItemIndex = now_index;
                if (_PageItemIndexChangeCallBack != null)
                {
                    _PageItemIndexChangeCallBack(gameObject, this._Items[_CurItemIndex].gameObject, _CurItemIndex);
                }
                _IsStopMove = false;
            }
            else
            {
                // 虽然是同一个index，但是已经不滑动了，就回到当前index应该在的位置；
                if (Mathf.Abs(p - _LastScrollRectPos) / _ListenTimeInterval < 0.05f)
                {
                    _IsStopMove = false;
                }
            }
            _TotalTime = 0;
            _LastScrollRectPos = p;
            _IsDraging = true;
        }
        //print("pos " + pos + " { " + Time.time + _ScrollRect.horizontalNormalizedPosition);
    }

    //矫正位置
    void CorrectedPosition()
    {
        float perLength = 1.0f / (_ItemCount - 1);

        float target_normal_pos = perLength * _CurItemIndex;

        target_normal_pos = Mathf.Max(target_normal_pos, 0);
        target_normal_pos = Mathf.Min(target_normal_pos, 1);

        if (IsVertical)
            _ScrollRect.verticalNormalizedPosition = target_normal_pos;
        else
            _ScrollRect.horizontalNormalizedPosition = target_normal_pos;
    }

	// Update is called once per frame
	void Update () {
        if (_Items.Count <= 0) return;

        if (_ScrollRect != null)
        {
            if (IsVertical)
            {
                if (Mathf.Abs(_ScrollRect.velocity.x) > 0.001f)
                {
                    _IsDraging = true;
                }
                else
                {
                    _IsDraging = false;
                }
            }
            else
            {
                if (Mathf.Abs(_ScrollRect.velocity.y) > 0.001f)
                {
                    _IsDraging = true;
                }
                else
                {
                    _IsDraging = false;
                }
            }
        }

        if (!_IsStopMove)
        {
            float perLength = 1.0f / (_ItemCount - 1);

            float target_normal_pos = perLength * _CurItemIndex;
            float after_pos = 0;

            target_normal_pos = Mathf.Max(target_normal_pos, 0);
            target_normal_pos = Mathf.Min(target_normal_pos, 1);

            _StartMoveTime += Time.deltaTime;
            float t = _StartMoveTime * _Smooting;
            Mathf.Clamp01(t);
            if (IsVertical)
            {
                _ScrollRect.verticalNormalizedPosition = Mathf.Lerp(_ScrollRect.verticalNormalizedPosition, target_normal_pos, t);
                after_pos = _ScrollRect.verticalNormalizedPosition;
            }
            else
            {
                _ScrollRect.horizontalNormalizedPosition = Mathf.Lerp(_ScrollRect.horizontalNormalizedPosition, target_normal_pos, t);
                after_pos = _ScrollRect.horizontalNormalizedPosition;
            }

            if (Mathf.Abs(target_normal_pos - after_pos) < perLength/10)
            {
                _IsStopMove = true;
                _IsDraging = false;
                _StartMoveTime = 0;
                CorrectedPosition();
            }
        }
        else
        {
            if (!_IsDraging && _TimeInterval > 0)
            {
                _TotalTime += Time.deltaTime;
                if (_TotalTime > _TimeInterval)
                {
                    _TotalTime -= _TimeInterval;
                    ++_CurItemIndex;
                    if (_CurItemIndex >= this._Items.Count)
                    {
                        ChangePageIndexRightNow(0);
                    }
                    else
                    {
                        if (_PageItemIndexChangeCallBack != null)
                        {
                            _PageItemIndexChangeCallBack(gameObject, this._Items[_CurItemIndex].gameObject, _CurItemIndex);
                        }
                    }
                    _IsStopMove = false;
                }
            }
        }
	}

    protected GDragablePageItem CreateItem()
    {
        GDragablePageItem page_item = null;

        page_item = _Pool.TakeOut();

        if (page_item == null)
        {
            RectTransform item = CUnityUtil.Instantiate(_CellItem) as RectTransform;
            item.SetParent(Trans, false);
            Vector2 new_pos = GNewUITools.GetAlignedPivot(NewAlign.Left);
            if (IsVertical)
                new_pos = GNewUITools.GetAlignedPivot(NewAlign.Top);
            item.pivot = new_pos;
            item.anchorMax = new_pos;
            item.anchorMin = new_pos;

            GDragablePageItem item_com = item.GetComponent<GDragablePageItem>();
            if (item_com == null)
            {
                item_com = item.gameObject.AddComponent<GDragablePageItem>();
            }

            if (this._PageItemInitCallBack != null)
            {
                item_com.OnItemInit = OnInitPageItem;
            }

            if (this._ClickPageItemCallBack != null)
            {
                item_com.OnItemClick = OnClickPageItem;
            }

            if (_HasChildButton && this._ClickPageItemButtonCallBack != null)
            {

                item_com.OnItemClickButton = this.OnClickPageItemButton;
            }

            page_item = item_com;
        }

        GNewUITools.SetVisible(page_item.RectTrans, true);

        return page_item;
    }

    protected virtual void OnInitPageItem(GameObject item, int index)
    {
        if (_PageItemInitCallBack != null)
        {
            _PageItemInitCallBack(gameObject, item, index);
        }
    }

    protected virtual void OnClickPageItem(GameObject item, int index)
    {
        if (_ClickPageItemCallBack != null)
        {
            _ClickPageItemCallBack(gameObject, item, index);
        }
    }

    protected virtual void OnClickPageItemButton(GameObject item, int index)
    {
        if (_ClickPageItemButtonCallBack != null)
        {
            _ClickPageItemButtonCallBack(gameObject, item, index);
        }
    }


    public void SetPageItemCount(int count)
    {
        if (this._ItemCount > count)
        {
            for(int i = count; i < _ItemCount; i++){
                DisPosePageItem(_Items, i);
            }
        }
        this._ItemCount = count;
        _CurItemIndex = 0;
        
        Repaint();

        if (_ItemCount > 0)
        {
            ChangePageIndexRightNow(_CurItemIndex);
        }
    }

    protected void UpdatePageItem(List<GDragablePageItem> items, int index)
    {
        GDragablePageItem item = null;
        if (items.Count > index)
        {
            item = items[index];
        }
        else
        {
            item = CreateItem();
            items.Add(item);
        }
        item.SetPosition(CalculatePos(index, item.RectTrans));
        item.OnInitPageItem(index, true);
    }

    private Vector2 CalculatePos(int index, RectTransform item)
    {
        Vector2 new_pos = Vector3.zero;

        int x, y;
        if (IsVertical)
        {
            x = 0;
            y = index;
        }
        else
        {
            x = index;
            y = 0;
        }
        new_pos = new Vector2(x * (CellSize.x), y * (CellSize.y));
        return new_pos;
    }

    protected void DisPosePageItem(List<GDragablePageItem> items, int index)
    {
        if (items.Count > index)
        {
            GDragablePageItem item = items[index];
            if (item != null)
            {
                if (_Pool.PutIn(item))
                {
                    GNewUITools.SetVisible(item.RectTrans, false);
                }
            }
            items.RemoveAt(index);
        }
    }

    [NoToLua]
    public virtual void Repaint()
    {
        RecalculateBound();
        UpdateContents();
    }

    protected virtual Vector4 PackBound(int total)
    {
        Vector4 pack = new Vector4();
        if (IsVertical)
        {
            pack.x = 1;
            pack.y = total;
        }
        else
        {
            pack.y = 1;
            pack.x = total;
        }

        pack.z = pack.x * (CellSize.x);

        pack.w = pack.y * (CellSize.y);

        return pack;
    }

    protected virtual void RecalculateBound()
    {
        Vector2 new_pos = GNewUITools.GetAlignedPivot(NewAlign.Left);
        if (IsVertical)
            new_pos = GNewUITools.GetAlignedPivot(NewAlign.Top);
        RectTrans.pivot = new_pos;
        RectTrans.anchorMax = new_pos;
        RectTrans.anchorMin = new_pos;

        _RealBound = PackBound(_ItemCount);
        if (_RealBound.z < 0)
        {
            _RealBound.z = RectTrans.rect.width;
        }
        if (_RealBound.w < 0)
        {
            _RealBound.w = RectTrans.rect.height;
        }

        Vector2 new_size = new Vector2(_RealBound.z, _RealBound.w);

        this.RectTrans.sizeDelta = GNewUITools.GetDeltaSize(RectTrans, new_size);
    }

    protected virtual void UpdateContents()
    {
        for (int i = 0; i < _ItemCount; i++)
        {
            UpdatePageItem(_Items, i);
        }
        while (_Items.Count > _ItemCount)
        {
            DisPosePageItem(_Items, _Items.Count - 1);
        }
    }

    public void ChangePageIndex(int index)
    {
        if (index >= this._Items.Count || index < 0)
        {
            Debug.LogError("Error !!! GDragablePageView.ChangePageIndex 数组越界！");
            return;
        }

        if (this._CurItemIndex == index)
            return;

        this._CurItemIndex = index;
        this._IsStopMove = false;
        if (_PageItemIndexChangeCallBack != null)
        {
            _PageItemIndexChangeCallBack(gameObject, this._Items[index].gameObject, index);
        }
    }

    public void ChangePageIndexRightNow(int index)
    {
        if (index >= this._Items.Count || index < 0)
        {
            Debug.LogError("Error !!! GDragablePageView.ChangePageIndexRightNow 数组越界！");
            return;
        }

        //if (this._CurItemIndex == index)
        //    return;

        this._CurItemIndex = index;
        this._IsStopMove = true;
        if (_PageItemIndexChangeCallBack != null)
        {
            _PageItemIndexChangeCallBack(gameObject, this._Items[index].gameObject, index);
        }
        CorrectedPosition();
    }

    public void SetTimeInterval(float interval)
    {
        _TimeInterval = interval;
        _TotalTime = 0;
    }

    [NoToLua]
    public void OnBeginDrag(PointerEventData eventData)
    {
        _IsDraging = true;
        _StartMoveTime = 0;
        if (IsVertical)
            _StartDragPos = _ScrollRect.verticalNormalizedPosition;
        else
            _StartDragPos = _ScrollRect.horizontalNormalizedPosition;
    }

    [NoToLua]
    public void OnEndDrag(PointerEventData eventData)
    {
        float pos_cur;
        if (IsVertical)
        {
            pos_cur = _ScrollRect.verticalNormalizedPosition;
        }
        else
        {
            pos_cur = _ScrollRect.horizontalNormalizedPosition;
        }

        pos_cur = (pos_cur - _StartDragPos);
        if (pos_cur > 0)
        {
            pos_cur = pos_cur < 0.5 ? 0 : pos_cur;
            pos_cur = Mathf.Floor(pos_cur + 0.5f);
        }
        else
        {
            pos_cur = pos_cur > -0.5 ? 0 : pos_cur;
            pos_cur = Mathf.Ceil(pos_cur - 0.5f);
        }
        _CurItemIndex += (int)pos_cur;
        _CurItemIndex = Mathf.Min(_CurItemIndex, this._Items.Count - 1);
        _CurItemIndex = Mathf.Max(_CurItemIndex, 0);
        if (_PageItemIndexChangeCallBack != null)
        {
            _PageItemIndexChangeCallBack(gameObject, this._Items[_CurItemIndex].gameObject, _CurItemIndex);
        }
        _IsDraging = false;
        _IsStopMove = false;
    }
}
