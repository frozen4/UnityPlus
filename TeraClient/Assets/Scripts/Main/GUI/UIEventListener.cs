using UnityEngine;
using UnityEngine.UI;
using LuaInterface;
using UnityEngine.EventSystems;
using GNewUI;
//using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using Common;
using GNewUI.Timeline;
using DG.Tweening;

public class UIEventListener : MonoBehaviour, IPointerClickHandler
{
    //public enum ControlType
    //{
    //    Button = 0,
    //    GText = 1,
    //    GImageModel = 2,
    //    Slider = 3,
    //    GScaleScroll = 4,
    //    GWebView = 5,
    //    GNewIOSToggle = 6,
    //    GNewListBase = 7,
    //    // TODO: 待扩充
    //};

    private bool _HasRegistered = false;
    private LuaTable _LuaPanelObject = null;
    private string _PanelName = "";
    //教学监视
    [NoToLua]
    [System.NonSerialized]
    public List<GameObject> _GuideWatchList = null;

    private bool _CanDispatchEvents = false;    //allow to disptch events to Lua
    private static bool _CanUseSchedule = true; //allow to update schedule

    public void SetLuaHandlerLink(LuaTable lua_panel)
    {
        _LuaPanelObject = lua_panel;
    }

    //[NoToLua]
    //[ContextMenu("ClearLuaHandlerLink")]
    //public void ClearLuaHandlerLink()
    //{
    //    _LuaPanelObject = null;
    //}


    //Add Callbacks to UI Components in LinkHolder
    public void RegisterHandler()
    {
        if (!_HasRegistered)
        {
            GButton[] gBtns;
            Button[] buttons;
            Toggle[] toggles;
            Scrollbar[] scrollbars;
            InputField[] inputFields;
            Dropdown[] dropDowns;

            GNewListBase[] newLists;
            GNewTabList[] newTabLists;
            GNewLayoutTable[] newLayoutTables;

            //GUIAnim[] newUIAnims;
            GWebView[] newWebViews;
            GDragablePageView[] dragablePageViews;
            DG.Tweening.DOTweenAnimation[] newDOTAnims;

            GScrollWatcher[] scrollWatchers;
            GSlideButton[] slideBtns;

            InteractableUIHolder holder = gameObject.GetComponent<InteractableUIHolder>();
            if (holder != null)
            {
                gBtns = holder.GBtns;
                buttons = holder.Buttons;
                toggles = holder.Toggles;
                scrollbars = holder.Scrollbars;
                inputFields = holder.InputFields;
                dropDowns = holder.Dropdowns;

                //GNewUI
                newLists = holder.NewLists;
                newTabLists = holder.NewTabLists;
                newLayoutTables = holder.NewLayoutTables;
                //newUIAnims = holder.newUIAnims;
                newWebViews = holder.newWebViews;
                dragablePageViews = holder.dragablePageViews;
                //DOT
                newDOTAnims = holder.newDOTAnims;
                scrollWatchers = holder.ScrollDrags;
                slideBtns = holder.SlideButtons;
            }
            else
            {
                gBtns = gameObject.GetComponentsInChildren<GButton>(true);
                buttons = gameObject.GetComponentsInChildren<Button>(true);
                toggles = gameObject.GetComponentsInChildren<Toggle>(true);
                scrollbars = gameObject.GetComponentsInChildren<Scrollbar>(true);
                inputFields = gameObject.GetComponentsInChildren<InputField>(true);
                dropDowns = gameObject.GetComponentsInChildren<Dropdown>(true);

                //GNewUI
                newLists = gameObject.GetComponentsInChildren<GNewListBase>(true);
                newTabLists = gameObject.GetComponentsInChildren<GNewTabList>(true);
                newLayoutTables = gameObject.GetComponentsInChildren<GNewLayoutTable>(true);
                //newUIAnims = gameObject.GetComponentsInChildren<GUIAnim>(true);
                newWebViews = gameObject.GetComponentsInChildren<GWebView>(true);
                dragablePageViews = gameObject.GetComponentsInChildren<GDragablePageView>(true);

                newDOTAnims = gameObject.GetComponentsInChildren<DG.Tweening.DOTweenAnimation>(true);
                scrollWatchers = gameObject.GetComponentsInChildren<GScrollWatcher>(true);
                slideBtns = gameObject.GetComponentsInChildren<GSlideButton>(true);
            }

            for (int i = 0; i < gBtns.Length; i++)
            {
                GButton gbtn = gBtns[i];
                gbtn.OnClick = OnClick;
                if (gbtn.IsProfessionModel)
                {
                    gbtn.PointerUpHandler = go =>
                    {
                        //Debug.LogWarning("OnPointerUp");
                        CallPanelFunc("OnPointerUp", go.name);
                    };
                    gbtn.PointerDownHandler = go =>
                    {
                        //Debug.LogWarning("OnPointerDown");
                        CallPanelFunc("OnPointerDown", go.name);
                    };
                    gbtn.OnPointerEnterHandler = go =>
                    {
                        CallPanelFunc("OnPointerEnter", go.name);
                    };
                    gbtn.OnPointerExitHandler = go =>
                    {
                        CallPanelFunc("OnPointerExit", go.name);
                    };

                    gbtn.OnPointerLongPressHandler = go =>
                    {
                        CallPanelFunc("OnPointerLongPress", go.name);
                    };
                }
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = buttons[i];
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(delegate() { OnClick(btn.gameObject); });
            }

            for (int i = 0; i < toggles.Length; i++)
            {
                Toggle toggle = toggles[i];
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(delegate(bool ischeck) { OnToggle(toggle.gameObject, ischeck); });

                GNewBtnExpress btx = toggle.GetComponent<GNewBtnExpress>();
                if (btx) btx.RegisterToToggle(toggle);
            }

            for (int i = 0; i < scrollbars.Length; i++)
            {
                Scrollbar scrollbar = scrollbars[i];
                scrollbar.onValueChanged.RemoveAllListeners();
                scrollbar.onValueChanged.AddListener(delegate(float value) { OnScroll(scrollbar.gameObject, value); });
            }

            for (int i = 0; i < inputFields.Length; i++)
            {
                InputField inputField = inputFields[i];
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onValueChanged.RemoveAllListeners();
                inputField.onEndEdit.AddListener(delegate(string str) { OnEndEdit(inputField.gameObject, str); });

                //try to cut out emoji
                if (inputField.contentType == InputField.ContentType.Standard)
                {
                    inputField.onValidateInput = OnValidInputdelegate;
                }

                //inputField.onValueChanged.AddListener(delegate(string str)
                //{
                //    string str_new = Regex.Replace(str, @"\p{Cs}", ""); //屏蔽emoji     
                //    if (string.CompareOrdinal(str, str_new) != 0)
                //    {
                //        inputField.text = str_new;
                //    }
                //});
                inputField.onValueChanged.AddListener(delegate(string str) { OnValueChanged(inputField.gameObject, str); });
            }

            for (int i = 0; i < dropDowns.Length; i++)
            {
                Dropdown dpd = dropDowns[i];

                GNewDropDown gndpd = dpd as GNewDropDown;
                if (gndpd != null)
                {
                    gndpd.OnInitItem = this.OnInitItem;
                }

                dpd.onValueChanged.RemoveAllListeners();
                dpd.onValueChanged.AddListener(index => { OnDropDown(dpd.gameObject, index); });
            }

            //NewUI
            for (int i = 0; i < newLists.Length; i++)
            {
                GNewListBase list = newLists[i];

                list.ClickItemCallBack = this.OnSelectItem;
                //list.PressItemCallBack = this.OnPressItem;
                //list.ReleaseItemCallBack = this.OnReleaseItem;
                list.LongPressCallBack = this.OnLongPressItem;
                list.InitItemCallBack = this.OnInitItem;

                if (list.HasChildButton)
                {
                    list.ClickItemButtonCallBack = this.OnSelectItemButton;
                }
            }

            for (int i = 0; i < newTabLists.Length; i++)
            {
                GNewTabList list = newTabLists[i];

                list.ClickItemCallBack = this.OnTabListSelectItem;
                list.InitItemCallBack = this.OnTabListInitItem;
                if (list.HasChildButton)
                {
                    list.ClickItemButtonCallBack = this.OnSelectItemButton;
                }
                list.ClickSubListItemButtonCallBack = this.OnTabSubListItemButton;
            }

            for (int i = 0; i < newLayoutTables.Length; i++)
            {
                GNewLayoutTable list = newLayoutTables[i];

                list.ClickItemCallBack = this.OnSelectItem;
                list.InitItemCallBack = this.OnInitItem;
                if (list.HasChildButton)
                {
                    list.ClickItemButtonCallBack = this.OnSelectItemButton;
                }
            }

            for (int i = 0; i < newWebViews.Length; i++)
            {
                GWebView gwv = newWebViews[i];
                gwv.OnReceiveMessage = this.OnReceiveWebViewMessage;
            }

            for (int i = 0; i < dragablePageViews.Length; i++)
            {
                GDragablePageView gdv = dragablePageViews[i];
                gdv._PageItemInitCallBack = this.OnInitItem;
                gdv._ClickPageItemCallBack = this.OnSelectItem;
                gdv._ClickPageItemButtonCallBack = this.OnSelectItemButton;
                gdv._PageItemIndexChangeCallBack = this.OnDragablePageIndexChange;
            }

            for (int i = 0; i < newDOTAnims.Length; i++)
            {
                if (newDOTAnims[i].IsCallLuaOnComplete)
                {
                    newDOTAnims[i].OnComplete2Lua = this.OnDOTComplete;
                }
            }

            for (int i = 0; i < scrollWatchers.Length; i++)
            {
                scrollWatchers[i].SetHandler(OnScrollWithoutFocus);
            }

            for (int i = 0; i < slideBtns.Length; i++)
            {
                slideBtns[i].OnSlide = this.OnButtonSlide;
            }

            _PanelName = this.name;
            Schedule.SetTag(_PanelName);

            _HasRegistered = true;
        }
    }

    //Remove Callbacks of UI Components in LinkHolder, seem to be of no use?
    void RemoveHandler()
    {
        if (_HasRegistered)
        {
            GButton[] gBtns;
            Button[] buttons;
            Toggle[] toggles;
            Scrollbar[] scrollbars;
            InputField[] inputFields;
            Dropdown[] dropDowns;

            GNewListBase[] newLists;
            GNewTabList[] newTabLists;
            GNewLayoutTable[] newLayoutTables;

            GWebView[] newWebViews;
            GDragablePageView[] dragablePageViews;
            DG.Tweening.DOTweenAnimation[] newDOTAnims;

            GScrollWatcher[] scrollWatchers;
            GSlideButton[] slideBtns;

            InteractableUIHolder holder = gameObject.GetComponent<InteractableUIHolder>();
            if (holder != null)
            {
                gBtns = holder.GBtns;
                buttons = holder.Buttons;
                toggles = holder.Toggles;
                scrollbars = holder.Scrollbars;
                inputFields = holder.InputFields;
                dropDowns = holder.Dropdowns;

                //GNewUI
                newLists = holder.NewLists;
                newTabLists = holder.NewTabLists;
                newLayoutTables = holder.NewLayoutTables;

                newWebViews = holder.newWebViews;
                dragablePageViews = holder.dragablePageViews;
                newDOTAnims = holder.newDOTAnims;
                scrollWatchers = holder.ScrollDrags;
                slideBtns = holder.SlideButtons;
            }
            else
            {
                gBtns = gameObject.GetComponentsInChildren<GButton>(true);
                buttons = gameObject.GetComponentsInChildren<Button>(true);
                toggles = gameObject.GetComponentsInChildren<Toggle>(true);
                scrollbars = gameObject.GetComponentsInChildren<Scrollbar>(true);
                inputFields = gameObject.GetComponentsInChildren<InputField>(true);
                dropDowns = gameObject.GetComponentsInChildren<Dropdown>(true);

                //GNewUI
                newLists = gameObject.GetComponentsInChildren<GNewListBase>(true);
                newTabLists = gameObject.GetComponentsInChildren<GNewTabList>(true);
                newLayoutTables = gameObject.GetComponentsInChildren<GNewLayoutTable>(true);

                newWebViews = gameObject.GetComponentsInChildren<GWebView>(true);
                dragablePageViews = gameObject.GetComponentsInChildren<GDragablePageView>(true);
                newDOTAnims = gameObject.GetComponentsInChildren<DG.Tweening.DOTweenAnimation>(true);
                scrollWatchers = gameObject.GetComponentsInChildren<GScrollWatcher>(true);
                slideBtns = gameObject.GetComponentsInChildren<GSlideButton>(true);
            }

            for (int i = 0; i < gBtns.Length; i++)
            {
                GButton gbtn = gBtns[i];
                gbtn.OnClick = null;
                if (gbtn.IsProfessionModel)
                {
                    gbtn.PointerUpHandler = null;
                    gbtn.PointerDownHandler = null;
                    gbtn.OnPointerEnterHandler = null;
                    gbtn.OnPointerExitHandler = null;
                    gbtn.OnPointerLongPressHandler = null;
                }
            }

            for (int i = 0; i < buttons.Length; i++)
                buttons[i].onClick.RemoveAllListeners();

            for (int i = 0; i < toggles.Length; i++)
                toggles[i].onValueChanged.RemoveAllListeners();

            for (int i = 0; i < scrollbars.Length; i++)
                scrollbars[i].onValueChanged.RemoveAllListeners();

            for (int i = 0; i < inputFields.Length; i++)
            {
                inputFields[i].onEndEdit.RemoveAllListeners();
                inputFields[i].onValueChanged.RemoveAllListeners();
            }

            for (int i = 0; i < dropDowns.Length; i++)
            {
                GNewDropDown gndpd = dropDowns[i] as GNewDropDown;
                if (gndpd != null)
                {
                    gndpd.OnInitItem = null;
                }
                dropDowns[i].onValueChanged.RemoveAllListeners();
            }

            //NewUI
            for (int i = 0; i < newLists.Length; i++)
            {
                newLists[i].ClickItemCallBack = null;
                newLists[i].LongPressCallBack = null;
                newLists[i].InitItemCallBack = null;
                newLists[i].ClickItemButtonCallBack = null;
            }

            for (int i = 0; i < newTabLists.Length; i++)
            {
                GNewTabList list = newTabLists[i];

                list.ClickItemCallBack = null;
                list.InitItemCallBack = null;
                list.ClickItemButtonCallBack = null;
                list.ClickSubListItemButtonCallBack = null;
            }

            for (int i = 0; i < newLayoutTables.Length; i++)
            {
                GNewLayoutTable list = newLayoutTables[i];

                list.ClickItemCallBack = null;
                list.InitItemCallBack = null;
                list.ClickItemButtonCallBack = null;
            }

            for (int i = 0; i < newWebViews.Length; i++)
            {
                GWebView gwv = newWebViews[i];
                gwv.OnReceiveMessage = null;
            }

            for (int i = 0; i < dragablePageViews.Length; i++)
            {
                GDragablePageView gdv = dragablePageViews[i];
                gdv._PageItemInitCallBack = null;
                gdv._ClickPageItemCallBack = null;
                gdv._ClickPageItemButtonCallBack = null;
                gdv._PageItemIndexChangeCallBack = null;
            }

            for (int i = 0; i < newDOTAnims.Length; i++)
            {
                if (newDOTAnims[i].IsCallLuaOnComplete)
                {
                    newDOTAnims[i].OnComplete2Lua = null;
                }
            }

            for (int i = 0; i < scrollWatchers.Length; i++)
            {
                scrollWatchers[i].SetHandler(null);
            }

            for (int i = 0; i < slideBtns.Length; i++)
            {
                slideBtns[i].OnSlide = null;
            }
            _HasRegistered = false;
        }
    }

    [NoToLuaAttribute]
    //This overrides the callback of a UI Component to a single call
    public void RegisterSingleObjHandler(GameObject control, int class_type, bool recursion)
    {
        if (control == null)
        {
            Debug.LogError("Cannot register event since the control object is null!");
            return;
        }

        System.Type type = WrapClassID.GetClassType(class_type);

        if (type == typeof(UnityEngine.UI.Button) || type == typeof(GButton))
        {
            GButton gbtn = control.GetComponent<GButton>();
            if (gbtn != null)
            {
                gbtn.OnClick = OnClick;
                return;
            }

            Button btn = control.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(delegate() { OnClick(btn.gameObject); });
            }
            if (recursion)
            {
                GButton[] btns = control.GetComponentsInChildren<GButton>(true);
                for (int i = 0; i < btns.Length; i++)
                {
                    if (btns[i] != null)
                    {
                        btns[i].OnClick = OnClick;
                    }
                }
            }
        }
        else if (type == typeof(Toggle))
        {
            if (recursion)
            {
                Toggle[] toggles = control.GetComponentsInChildren<Toggle>(true);
                for (int i = 0; i < toggles.Length; i++)
                {
                    if (toggles[i] != null)
                    {
                        toggles[i].onValueChanged.RemoveAllListeners();
                        GameObject go = toggles[i].gameObject;
                        toggles[i].onValueChanged.AddListener(delegate(bool ischeck) { OnToggle(go, ischeck); });
                    }
                }
            }
            else
            {
                Toggle toggle = control.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.onValueChanged.AddListener(delegate(bool ischeck) { OnToggle(toggle.gameObject, ischeck); });
                }
            }
        }
        else if (type == typeof(GNewIOSToggle))
        {
            GNewIOSToggle gTgl = control.GetComponent<GNewIOSToggle>();
            if (gTgl != null)
            {
                gTgl.OnClick = OnClick;
                //gTgl.OnValueChanged = OnToggleSpecial;
            }
            if (recursion)
            {
                GNewIOSToggle[] gToggles = control.GetComponentsInChildren<GNewIOSToggle>(true);
                for (int i = 0; i < gToggles.Length; i++)
                {
                    if (gToggles[i] != null)
                    {
                        gToggles[i].OnClick = OnClick;
                        //gToggles[i].OnValueChanged = OnToggleSpecial;
                    }
                }
            }
        }
        else if (type == typeof(GText))
        {
            // 现在需求中GText不在root上，所以这么处理；其实不太好
            GText gtext = control.GetComponentInChildren<GText>();
            if (gtext != null)
                gtext.OnClick = OnGTextClick;
            if (recursion)
            {
                GText[] gts = control.GetComponentsInChildren<GText>(true);
                for (int i = 0; i < gts.Length; i++)
                {
                    if (gts[i] != null)
                    {
                        gts[i].OnClick = OnGTextClick;
                    }
                }
            }
        }
        else if (type == typeof(GImageModel))
        {
            GImageModel gim = control.GetComponent<GImageModel>();
            if (gim != null)
                gim.OnModelClick = OnClick;
            if (recursion)
            {
                GImageModel[] gims = control.GetComponentsInChildren<GImageModel>(true);
                for (int i = 0; i < gims.Length; i++)
                {
                    if (gims[i] != null)
                    {
                        gims[i].OnModelClick = OnClick;
                    }
                }
            }
        }
        else if (type == typeof(Slider))
        {
            Slider sld = control.GetComponent<Slider>();
            if (sld != null)
                sld.onValueChanged.AddListener(delegate(float value) { OnSliderChanged(sld.gameObject, value); });
            if (recursion)
            {
                Slider[] sliders = control.GetComponentsInChildren<Slider>(true);
                for (int i = 0; i < sliders.Length; i++)
                {
                    if (sliders[i] != null)
                    {
                        sliders[i].onValueChanged.AddListener(delegate(float value) { OnSliderChanged(sliders[i].gameObject, value); });
                    }
                }
            }
        }
        else if (type == typeof(GScaleScroll))
        {
            GScaleScroll gss = control.GetComponent<GScaleScroll>();
            if (gss != null)
                gss.onScaleChanged = OnScaleChanged;
            if (recursion)
            {
                GScaleScroll[] gsss = control.GetComponentsInChildren<GScaleScroll>(true);
                for (int i = 0; i < gsss.Length; i++)
                {
                    if (gsss[i] != null)
                    {
                        gsss[i].onScaleChanged = OnScaleChanged;
                    }
                }
            }
        }
        else if (type == typeof(GBlood))
        {
            GBlood gwv = control.GetComponent<GBlood>();
            if (gwv != null)
            {
                gwv.OnTweenFinishCallBack = this.OnDOTComplete;
            }
            if (recursion)
            {
                GBlood[] gwvs = control.GetComponentsInChildren<GBlood>(true);
                for (int i = 0; i < gwvs.Length; i++)
                {
                    gwvs[i].OnTweenFinishCallBack = this.OnDOTComplete;
                }
            }
        }
        else if (type == typeof(GWebView))
        {
            GWebView gwv = control.GetComponent<GWebView>();
            if (gwv != null)
            {
                gwv.OnReceiveMessage = this.OnReceiveWebViewMessage;
            }
            if (recursion)
            {
                GWebView[] gwvs = control.GetComponentsInChildren<GWebView>(true);
                for (int i = 0; i < gwvs.Length; i++)
                {
                    gwvs[i].OnReceiveMessage = OnReceiveWebViewMessage;
                }
            }
        }
        else if (type == typeof(GNewListBase))
        {
            GNewListBase list = control.GetComponent<GNewListBase>();
            if (list != null)
            {
                list.ClickItemCallBack = this.OnSelectItem;
                //list.PressItemCallBack = this.OnPressItem;
                //list.ReleaseItemCallBack = this.OnReleaseItem;
                list.LongPressCallBack = this.OnLongPressItem;
                list.InitItemCallBack = this.OnInitItem;

                if (list.HasChildButton)
                {
                    list.ClickItemButtonCallBack = this.OnSelectItemButton;
                }
            }
            if (recursion)
            {
                GNewListBase[] lists = control.GetComponentsInChildren<GNewListBase>(true);
                for (int i = 0; i < lists.Length; i++)
                {
                    lists[i].ClickItemCallBack = this.OnSelectItem;
                    lists[i].LongPressCallBack = this.OnLongPressItem;
                    lists[i].InitItemCallBack = this.OnInitItem;
                    if (lists[i].HasChildButton)
                    {
                        lists[i].ClickItemButtonCallBack = this.OnSelectItemButton;
                    }
                }
            }
        }
        else
        {
            // TODO: 将来根据需要自行扩展
            HobaDebuger.LogError("<RegisterUIEventHandler>WrapClassID not supported!" + class_type);
        }
    }

    //Add a UI Component to the guide watch list
    public void RegisterGuideObject(GameObject g)
    {
        if (_GuideWatchList == null) _GuideWatchList = new List<GameObject>(0);

        if (!_GuideWatchList.Contains(g))
        {
            _GuideWatchList.Add(g);
        }
    }

    public void UnregisterGuideObject(GameObject g)
    {
        if (_GuideWatchList != null)
        {
            _GuideWatchList.Remove(g);
        }
    }

    public void EnableUIEvent(bool b_flag)
    {
        _CanDispatchEvents = b_flag;
    }

    [NoToLua]
    public void HandleClick4Tip(GameObject g_clicked)
    {
        bool is_clickedSelf = false;

        if (g_clicked != null)
        {
            Transform t = g_clicked.transform;
            is_clickedSelf = t.IsChildOf(transform);
        }

        if (is_clickedSelf == false)
        {
            CallPanelFunc("CloseThisTip");
        }
    }

    #region Dotween
    void OnDOTComplete(string go_name, string s_value)
    {
        CallPanelFunc("OnDOTComplete", go_name, s_value);
    }

    #endregion

    [NoToLuaAttribute]
    public void OnPointerClick(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(eventData.pointerEnter);
        NotifyClick(_PanelName);
        CallPanelFunc("HandlePointerClick", eventData.pointerEnter);
    }

    #region UI Evt Handler

    void OnClick(GameObject go)
    {
        if (!_CanDispatchEvents) return;

        //Debug.LogWarning("OnClick");
        EventSystem.current.SetSelectedGameObject(go);  //in case of clicking on any else than button
        CallPanelFunc("OnClick", go.name);
        NotifyClick(_PanelName);

        if (go != null)
        {
            //仅用于调试使用，正式上线时应该去掉，传递GameObject消耗比较大
            CallPanelFunc("OnClickGameObject", go);
        }

        //CallFunc(Main.LuaGuideMan, "Test", go);
        if (go != null)
        {
            //教学
            if (_GuideWatchList != null && _GuideWatchList.Contains(go))
            {
                //OnGuideClick(go);
                CallFunc(Main.LuaGuideMan, "OnClick", go.name);
            }
        }
    }

    void OnGTextClick(int textId, int linkId)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnGTextClick", textId, linkId);
    }

    void OnToggle(GameObject go, bool is_checked)
    {
        if (!_CanDispatchEvents) return;
        if (EventSystem.current.currentSelectedGameObject != go) return;
        CallPanelFunc("OnToggle", go.name, is_checked);
        NotifyClick(_PanelName);

        if (go != null)
        {
            //教学
            if (_GuideWatchList != null && _GuideWatchList.Contains(go))
            {
                //OnGuideToggle(go,is_checked);
                CallFunc(Main.LuaGuideMan, "OnToggle", go.name, is_checked);
            }
        }
    }

    void OnScroll(GameObject go, float value)
    {
        if (!_CanDispatchEvents) return;
        EventSystem.current.SetSelectedGameObject(go);
        CallPanelFunc("OnScroll", go.name, value);
    }

    void OnScrollWithoutFocus(GameObject go, float value)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnScroll", go.name, value);
    }

    void OnInitItem(GameObject list, GameObject item, int index)
    {
        //if (!_CanDispatchEvents) return;
        CallPanelFunc("OnInitItem", item, list.name, index);
    }

    void OnSelectItem(GameObject list, GameObject item, int index)
    {
        if (!_CanDispatchEvents) return;
        //EventSystem.current.SetSelectedGameObject(item);
        CallPanelFunc("OnSelectItem", item, list.name, index);
        NotifyClick(_PanelName);

        if (list != null)
        {
            //教学
            if (_GuideWatchList != null && _GuideWatchList.Contains(list))
            {
                //OnGuideSelectItem(list, item, index);
                CallFunc(Main.LuaGuideMan, "OnSelectItem", item, list.name, index);
            }
        }
    }

    void OnLongPressItem(GameObject list, GameObject item, int index)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnLongPressItem", item, list.name, index);
    }

    void OnSelectItemButton(GameObject list, GameObject button, int index)
    {
        if (!_CanDispatchEvents) return;
        //EventSystem.current.SetSelectedGameObject(button);
        CallPanelFunc("OnSelectItemButton", button, list.name, button.name, index);
        NotifyClick(_PanelName);

        if (button != null)
        {
            //教学
            if (_GuideWatchList != null && _GuideWatchList.Contains(list))
            {
                //OnGuideSelectItem(list, item, index);
                CallFunc(Main.LuaGuideMan, "OnSelectItemButton", button, list.name, button.name, index);
            }
        }
    }

    void OnTabListSelectItem(GameObject list, GameObject item, int main_index, int sub_index)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnTabListSelectItem", list, item, main_index, sub_index);
        NotifyClick(_PanelName);

        //教学
        if (_GuideWatchList != null && _GuideWatchList.Contains(list))
        {
            //OnGuideSelectItem(list, item, index);
            CallFunc(Main.LuaGuideMan, "OnTabListSelectItem", list, item, main_index, sub_index);
        }
    }

    void OnTabListInitItem(GameObject list, GameObject item, int main_index, int sub_index)
    {
        //if (!_CanDispatchEvents) return;
        CallPanelFunc("OnTabListInitItem", list, item, main_index, sub_index);
    }

    void OnTabSubListItemButton(GameObject list, GameObject item, int main_index, int sub_index)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnTabListItemButton", list, item, main_index, sub_index);
        NotifyClick(_PanelName);
    }

    void OnEndEdit(GameObject go, string str)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnEndEdit", go.name, str);
    }

    void OnValueChanged(GameObject go, string str)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnValueChanged", go.name, str);
    }

    void OnDropDown(GameObject go, int index)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnDropDown", go.name, index);
    }

    void OnSliderChanged(GameObject go, float value)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnSliderChanged", go.name, value);
    }

    void OnScaleChanged(GameObject go, float value)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnScaleChanged", go.name, value);
    }

    void OnSeqEvent(GameObject go, string value)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnSceneEvent", go.name, value);
    }

    void OnButtonSlide(GameObject go)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnButtonSlide", go.name);
    }

    //表情字检查 EMO char check for all Inputs
    char OnValidInputdelegate(string text, int charIndex, char addedChar)
    {
        //string s = text;

        //方法1
        //bool isEomji=false;
        // // surrogate pair
        // if (0xd800 <= addedChar && addedChar <= 0xdbff)
        // {
        //     if (text.Length - charIndex > 1)
        //     {
        //         char ls = text[charIndex + 1];
        //         int uc = ((addedChar - 0xd800) * 0x400) + (ls - 0xdc00) + 0x10000;
        //         if (0x1d000 <= uc && uc <= 0x1f77f) {
        //             isEomji = true;
        //         }
        //     }
        // }
        // else
        // {
        //     // non surrogate
        //     if (0x2100 <= addedChar && addedChar <= 0x27ff && addedChar != 0x263b) {
        //         isEomji = true;
        //     } else if (0x2B05 <= addedChar && addedChar <= 0x2b07) {
        //         isEomji = true;
        //     } else if (0x2934 <= addedChar && addedChar <= 0x2935) {
        //         isEomji = true;
        //     } else if (0x3297 <= addedChar && addedChar <= 0x3299) {
        //         isEomji = true;
        //     } else if (addedChar == 0xa9 || addedChar == 0xae || addedChar == 0x303d || addedChar == 0x3030 || addedChar == 0x2b55 || addedChar == 0x2b1c || addedChar == 0x2b1b || addedChar == 0x2b50|| addedChar == 0x231a ) {
        //         isEomji = true;
        //     }
        //     if (!isEomji && text.Length - charIndex > 1) {
        //         char ls = text[charIndex+1];
        //         if (ls == 0x20e3) {
        //             isEomji = true;
        //         }
        //     }
        // }

        ////方法2
        //UnicodeCategory ucy = char.GetUnicodeCategory(addedChar);
        //if (ucy == UnicodeCategory.UppercaseLetter || ucy == UnicodeCategory.LowercaseLetter //|| ucy == UnicodeCategory.LetterNumber
        //    || ucy == UnicodeCategory.OtherLetter || ucy == UnicodeCategory.DecimalDigitNumber
        //    )
        //{
        //}
        //else if (char.IsPunctuation(addedChar))
        //{
        //}
        //else if(addedChar=="")
        //{
        //}
        //else
        //{
        //    Debug.Log("Input Invalid char :" + ucy);
        //    addedChar = (char)0;
        //}

        //方法3
        //if ((addedChar >= 0x0000) && (addedChar <= 0xFFFF))
        //{
        //    addedChar = (char)0;
        //}

        //if ((addedChar == 0x0) || (addedChar == 0x9) || (addedChar == 0xA) || (addedChar == 0xD) || ((addedChar >= 0x20) && (addedChar <= 0xD7FF))
        //        || ((addedChar >= 0xE000) && (addedChar <= 0xFFFD)) || ((addedChar >= 0x10000) && (addedChar <= 0x10FFFF)))
        //{
        //    addedChar = (char)0;
        //}
        UnicodeCategory ucy = char.GetUnicodeCategory(addedChar);
        if (ucy == UnicodeCategory.Control || ucy == UnicodeCategory.OtherSymbol
            || ucy == UnicodeCategory.Surrogate || ucy == UnicodeCategory.Format
            || ucy == UnicodeCategory.OtherNotAssigned || ucy == UnicodeCategory.NonSpacingMark)
        {
            addedChar = (char)0;
        }

        return addedChar;
    }

    //[NoToLuaAttribute]
    //public void OnEventTrigger(string s_value)
    //{
    //  if (!_CanDispatchEvents) return;
    //    CallFunc("OnEventTrigger", s_value);
    //}

    void OnReceiveWebViewMessage(string message)
    {
        if (!_CanDispatchEvents) return;
        Debug.Log(string.Format("OnReceiveWebViewMessage  --- unity :{0}", message));
        CallPanelFunc("OnReceiveWebViewMessage", message);
    }

    void OnDragablePageIndexChange(GameObject pageView, GameObject pageItem, int index)
    {
        if (!_CanDispatchEvents) return;
        CallPanelFunc("OnDragablePageIndexChange", pageItem, pageView.name, index);
    }

    #endregion UI Evt Handler

    private void NotifyClick(string name)
    {
        LuaScriptMgr.Instance.CallNotifyClickFunc(name);
    }

    object[] CallPanelFunc(string funcName, params object[] args)
    {
        return CallFunc(_LuaPanelObject, funcName, args);
    }

    object[] CallFunc(LuaTable luaObject, string funcName, params object[] args)
    {
        object[] results = null;
        if (luaObject == null)
            return results;

        LuaFunction lua_func = luaObject.RawGetFunc(funcName);
        if (lua_func != null)
        {
            // lua_func.Call(_LuaPanelObject, args);
            if (args.Length == 0)
                results = lua_func.Call(luaObject);
            else if (args.Length == 1)
                results = lua_func.Call(luaObject, args[0]);
            else if (args.Length == 2)
                results = lua_func.Call(luaObject, args[0], args[1]);
            else if (args.Length == 3)
                results = lua_func.Call(luaObject, args[0], args[1], args[2]);
            else if (args.Length == 4)
                results = lua_func.Call(luaObject, args[0], args[1], args[2], args[3]);
            else if (args.Length == 5)
                results = lua_func.Call(luaObject, args[0], args[1], args[2], args[3], args[4]);
            else if (args.Length == 6)
                results = lua_func.Call(luaObject, args[0], args[1], args[2], args[3], args[4], args[5]);
            else
                HobaDebuger.Log("there are too many args.please write a new lua call.");
        }
        else
        {
            HobaDebuger.Log("the function " + funcName + " you want to call can not be found.");
        }
        if (results != null && results.Length > 0)
        {
            HobaDebuger.Log(results[0]);
        }
        return results;
    }

    #region Time Schedule

    private GUISchedule Schedule = new GUISchedule();

    public int AddEvt_PlayAnim(string s_key, float f_time, Animation a_anim, string s_path)
    {
        return Schedule.AddPlayAnim(s_key, f_time, a_anim, s_path);
    }

    public int AddEvt_PlayDotween(string s_key, float f_time, DOTweenPlayer dot_player, string s_id)
    {
        return Schedule.AddPlayDotween(s_key, f_time, dot_player, s_id);
    }

    public int AddEvt_SetActive(string s_key, float f_time, GameObject g_go, bool b_activeState)
    {
        return Schedule.AddSetActive(s_key, f_time, g_go, b_activeState);
    }

    public int AddEvt_LuaCB(string s_key, float f_time, LuaFunction lf_func)
    {
        return Schedule.AddLuaCB(s_key, f_time, lf_func);
    }

    public int AddEvt_PlayFx(string s_key, float f_time, string s_path, GameObject g_hook, GameObject g_target,GameObject g_clipper, float f_lifeTime, int i_order)
    {
        Transform t_hook = g_hook != null ? g_hook.transform : null;
        Transform t_target = g_target != null ? g_target.transform : null;
        return Schedule.AddPlayFx(s_key, f_time, s_path, t_hook, t_target, g_clipper, f_lifeTime, i_order);
    }

    public int AddEvt_PlaySound(string s_key, float f_time, string s_path)
    {
        return Schedule.AddPlaySound(s_key, f_time, s_path);
    }

    public int AddEvt_Shake(string s_key, float f_time, float f_mag, float f_lifeTime)
    {
        return Schedule.AddShake(s_key, f_time, f_mag, f_lifeTime);
    }

    public void KillEvt(int i_key)
    {
        Schedule.CloseEvent(i_key);
    }

    public void KillEvts(string s_key)
    {
        Schedule.CloseSchedule(s_key);
    }

    #endregion

    void Update()
    {
        float dt = Time.deltaTime;
        Schedule.Tick(dt);
    }

    void OnDestroy()
    {
        RemoveHandler();
        if (_LuaPanelObject != null)
        {
            _LuaPanelObject.Dispose();
            _LuaPanelObject = null;
        }

        if (Schedule != null)
        {
            Schedule.Clear();
            Schedule = null;
        }
    }

    void OnApplicationQuit()
    {
        RemoveHandler();
        if (_LuaPanelObject != null)
        {
            _LuaPanelObject.Dispose();
            _LuaPanelObject = null;
        }
    }


}
