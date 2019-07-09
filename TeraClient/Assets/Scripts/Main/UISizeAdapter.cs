using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LuaInterface;

//Stretch or Squeeze Panels to Fit IPhoneX Screen
public class UISizeAdapter : GNewUIBase
{
#if UNITY_EDITOR
    public enum DeviceType
    {
        Off,
        IPHONEX_L,
        IPHONEX_R,
        IPHONEX_LR,
    }

    public DeviceType FakeDevice;       //debug

    public float VDensity = 2;
#endif

    public static UISizeAdapter Instance;

    private RectTransform ThisPanel;
    //private Vector4 _SafePadding = new Vector4(0, 0, 0, 0);     //ltrb
    private Vector4[] _SafePadding = new Vector4[2];     //ltrb
    private ScreenOrientation _curScreenOri = ScreenOrientation.Unknown;
    //private ScreenOrientation _oriScreenOri = ScreenOrientation.Unknown;
    private bool _FirstRun = true;
    private Vector4 _LastUsingSafePadding = new Vector4(0, 0, 0, 0);     //ltrb
    private List<InteractableUIHolder> _KnownUI = new List<InteractableUIHolder>();

    private Vector4 CurSafePadding
    {
        get
        {
#if UNITY_EDITOR
            return _SafePadding[0];
#elif (UNITY_IOS||UNITY_ANDROID)
            if (_curScreenOri == ScreenOrientation.LandscapeRight)
            {
                return _SafePadding[1];
            }
            else
            {
                return _SafePadding[0];
            }
#else
            return Vector4.zero;
#endif

        }
        set
        {
            _SafePadding[0] = value;
            _SafePadding[1] = new Vector4(value.z, value.y, value.x, value.w);
            //_oriScreenOri = Screen.orientation;
        }
    }

    private bool _ShouldBeEnable
    {
        get { return this.enabled; }
    }


#if IN_GAME&&UNITY_IOS
	[DllImport("__Internal")]
    private extern static void GetSafeOffsetImpl(out float x, out float y, out float r, out float b);
#endif

    private Vector4 GetSafePadding()
    {
        Vector4 v_result = Vector4.zero;
#if UNITY_EDITOR
        if (FakeDevice == DeviceType.IPHONEX_L)
        {
            v_result.Set(48, 0, 0, 0);
        }
        else if (FakeDevice == DeviceType.IPHONEX_R)
        {
            v_result.Set(0, 0, 48, 0);
        }
        else if (FakeDevice == DeviceType.IPHONEX_LR)
        {
            v_result.Set(48, 0, 48, 0);
        }
#elif IN_GAME&&UNITY_IOS
        float l, t, r, b;
		GetSafeOffsetImpl(out l, out t, out r, out b);
        v_result.Set(l, t, r, b);
#elif IN_GAME&&UNITY_ANDROID
        float ratio = 1.0f;
        bool bHasNotch = AndroidUtil.HasNotchInScreen() && (!AndroidUtil.IgnoreNotchScreen());
        if (bHasNotch)
        {
            v_result.Set(48 * ratio, 0, 0, 0);
        }
#endif
        return v_result;
    }

    public void InitSafeArea()
    {
        SafeInit();

        //UI TODO: Find a Callback instead of Calling this every frame.
        Vector4 v_safeArea = GetSafePadding(); // or Screen.safeArea if you use a version of Unity that supports it
        //#if IN_GAME
        //        Common.HobaDebuger.Log("[UI] InitSafeArea " + v_safeArea + SystemInfo.deviceModel);
        //#else
        //        Debug.Log("[UI] InitSafeArea " + v_safeArea + SystemInfo.deviceModel);
        //#endif

#if !UNITY_EDITOR
        if (v_safeArea == Vector4.zero)
        {
            enabled = false;
        }
#endif

        if (!_ShouldBeEnable) return;
        float scale = 1;
#if IN_GAME && UNITY_IOS || IN_GAME && UNITY_ANDROID||UNITY_EDITOR
        CanvasScaler cs = GetComponentInParent<CanvasScaler>();
        if (cs == null) return;
        else
        {
            RectTransform rt = cs.GetComponent<RectTransform>();
            if (rt != null)
            {
                scale = rt.rect.width / Screen.width;
            }
        }
#endif

        Vector4 v_result = new Vector4();
        int i_platform = 0;
#if UNITY_EDITOR
        v_result = v_safeArea * VDensity;
#elif IN_GAME && UNITY_IOS
        v_result = new Vector4(v_safeArea.x, 0, 0, 0);
        Debug.Log("Safe padding " + v_result+" , " + " , " + scale);
        i_platform = 1;
#elif IN_GAME && UNITY_ANDROID
        float d=AndroidUtil.GetDensity();
        v_result = v_safeArea * d;
        Debug.Log("Safe padding " + v_result + " , " + d + " , " + scale);
        i_platform = 2;
#endif

#if IN_GAME
        object[] float4_results = LuaScriptMgr.Instance.CallLuaFunction("ConfirmSafeArea", v_result.x, v_result.y, v_result.z, v_result.w, SystemInfo.deviceModel, i_platform);
        if (float4_results != null)
        {
            CurSafePadding = new Vector4((float)(double)float4_results[0], (float)(double)float4_results[1], (float)(double)float4_results[2], (float)(double)float4_results[3]) * scale;
        }
#else
        CurSafePadding = v_result * scale;
#endif
    }

    private void ApplySafeArea()
    {
        Vector4 v_curPad = CurSafePadding;
        Debug.Log("[UI] ApplySafeArea " + v_curPad);

        CleanNull();
        for (int i = 0; i < _KnownUI.Count; i++)
        {
            if (_KnownUI[i] != null)
            {
                TuneUIPanels(_KnownUI[i], v_curPad, _LastUsingSafePadding);        //need to undo last padding, and most of the UIs didnt record last area;
            }
        }
        _LastUsingSafePadding = v_curPad;
    }

    private void TuneUIPanels(InteractableUIHolder ui_holder, Vector4 v_cur_padding, Vector4 v_last_padding)
    {
        GameObject this_obj = ui_holder.gameObject;

        if (ui_holder.PanelInfoList.Length != 0)
        {
            for (int i = 0; i < ui_holder.PanelInfoList.Length; i++)
            {
                InteractableUIHolder.PanelInfo p_i = ui_holder.PanelInfoList[i];
                if (p_i.Panel != null && !p_i.IsIgnoreResizng)
                {
                    ////did this when build
                    ////if (ui_holder.PanelInfoList[i].Panel == this_obj && ui_holder.PanelInfoList[i].IsKeptFullScreen)
                    ////{
                    ////}
                    ////else
                    //_TuneRect(ui_holder.PanelInfoList[i].Panel, ui_holder.PanelInfoList[i].IsKeptFullScreen, v_cur_padding, v_last_padding, ref ui_holder.PanelInfoList[i].LastArea);
                    _TuneRect(ui_holder.PanelInfoList[i], v_cur_padding, v_last_padding);
                }
            }
        }
        //else
        //{
        //    Vector4 v_area = Vector4.zero;
        //    _TuneRect(this_obj, true, v_cur_padding, v_last_padding, ref v_area);
        //}
    }

    ////Adjust rect to full screen or safe area.
    //private void _TuneRect(GameObject target_panel, bool keep_fullScreen, Vector4 v_padding, Vector4 v_last_padding, ref Vector4 v_lastArea)
    //{
    //    //RectTransform rt_ui = target_panel.transform as RectTransform;
    //    //RectTransform rt_p = rt_ui.parent as RectTransform;
    //    //if (rt_p != null)
    //    //{
    //    //    Rect rt_pRT = GNewUITools.GetRelativeRect(ThisPanel, rt_p);
    //    //    Rect rt_sr = ThisPanel.rect;

    //    //    if (!keep_fullScreen)
    //    //    {
    //    //        Vector4 v_padding = CurSafePadding;
    //    //        rt_sr.xMin += v_padding.x;
    //    //        rt_sr.yMax -= v_padding.y;
    //    //        rt_sr.xMax -= v_padding.z;
    //    //        rt_sr.yMin += v_padding.w;
    //    //    }

    //    //    //calc the difference of parent and the screen
    //    //    Vector4 v_area = new Vector4(rt_sr.xMin - rt_pRT.xMin, rt_sr.yMax - rt_pRT.yMax, rt_sr.xMax - rt_pRT.xMax, rt_sr.yMin - rt_pRT.yMin);

    //    //    rt_ui.offsetMin += new Vector2(v_area.x, v_area.w);
    //    //    rt_ui.offsetMax += new Vector2(v_area.z, v_area.y);
    //    //}

    //    RectTransform rt_ui = target_panel.transform as RectTransform;
    //    RectTransform rt_p = rt_ui.parent as RectTransform;
    //    if (rt_p != null)
    //    {
    //        Vector4 v_area = Vector4.zero;
    //        if (rt_p != ThisPanel)
    //        {
    //            Rect rect_sr = GNewUITools.GetRelativeRect(rt_p, ThisPanel);
    //            Rect rect_pt = rt_p.rect;

    //            //calc the difference of parent and the screen
    //            v_area.Set(rect_sr.xMin - rect_pt.xMin, rect_sr.yMax - rect_pt.yMax, rect_sr.xMax - rect_pt.xMax, rect_sr.yMin - rect_pt.yMin);
    //        }

    //        Vector4 v_area_new = v_area;
    //        if (!keep_fullScreen)
    //        {
    //            v_area_new.x += v_padding.x;
    //            v_area_new.y -= v_padding.y;
    //            v_area_new.z -= v_padding.z;
    //            v_area_new.w += v_padding.w;
    //        }
    //        else if (rt_p != ThisPanel)
    //        {
    //            v_area_new -= v_lastArea;
    //            v_lastArea = v_lastArea + v_area_new;
    //        }

    //        //if (rt_p != ThisPanel)
    //        //{
    //        //    v_areaUndo -= v_lastArea;
    //        //}
    //        //rt_ui.offsetMin += new Vector2(v_area.x * (rt_ui.pivot.x - rt_ui.anchorMin.x), v_area.w * (rt_ui.pivot.y - rt_ui.anchorMin.y));
    //        //rt_ui.offsetMax += new Vector2(v_area.z * (rt_ui.anchorMax.x - rt_ui.pivot.x), v_area.y * (rt_ui.anchorMax.y - rt_ui.pivot.y));

    //        Vector2 min_off = new Vector2(Mathf.Lerp(v_area_new.x, v_area_new.z, rt_ui.anchorMin.x),
    //            Mathf.Lerp(v_area_new.w, v_area_new.y, rt_ui.anchorMin.y));
    //        Vector2 max_off = new Vector2(Mathf.Lerp(v_area_new.x, v_area_new.z, rt_ui.anchorMax.x),
    //            Mathf.Lerp(v_area_new.w, v_area_new.y, rt_ui.anchorMax.y));

    //        rt_ui.offsetMin += min_off;
    //        rt_ui.offsetMax += max_off;

    //        //Check

    //        Vector4 v_area_chk = v_area;
    //        Vector4 v_padding_new = v_padding + v_last_padding;
    //        if (!keep_fullScreen)
    //        {
    //            v_area_chk.x += v_padding_new.x;
    //            v_area_chk.y -= v_padding_new.y;
    //            v_area_chk.z -= v_padding_new.z;
    //            v_area_chk.w += v_padding_new.w;
    //        }
    //        else if (rt_p != ThisPanel)
    //        {
    //            v_area_chk -= v_padding_new;
    //        }

    //        Vector2 min_off_2 = new Vector2(Mathf.Lerp(v_area_chk.x, v_area_chk.z, rt_ui.anchorMin.x),
    //            Mathf.Lerp(v_area_chk.w, v_area_chk.y, rt_ui.anchorMin.y));
    //        Vector2 max_off_2 = new Vector2(Mathf.Lerp(v_area_chk.x, v_area_chk.z, rt_ui.anchorMax.x),
    //            Mathf.Lerp(v_area_chk.w, v_area_chk.y, rt_ui.anchorMax.y));

    //        if (min_off != min_off_2 || max_off != max_off_2)
    //        {
    //            Common.HobaDebuger.LogError("TuneUI " + rt_ui.name + " " + min_off + ", " + max_off + " != " + min_off_2 + ", " + max_off_2);
    //        }
    //    }
    //}

    //Adjust rect to full screen or safe area.
    private void _TuneRect(InteractableUIHolder.PanelInfo p_info, Vector4 v_cur_padding, Vector4 v_last_padding)
    {
        RectTransform rt_ui = p_info.Panel.transform as RectTransform;
        RectTransform rt_p = rt_ui.parent as RectTransform;
        if (rt_p != null)
        {
            Vector4 v_padding_new = v_cur_padding;
            if (!p_info.IsKeptFullScreen)
            {
                Vector4 v_area_chk = Vector4.zero;
                v_area_chk.x += v_padding_new.x;
                v_area_chk.y -= v_padding_new.y;
                v_area_chk.z -= v_padding_new.z;
                v_area_chk.w += v_padding_new.w;

                Vector2 min_off_2 = p_info.OffsetMin + new Vector2(Mathf.Lerp(v_area_chk.x, v_area_chk.z, rt_ui.anchorMin.x),
                    Mathf.Lerp(v_area_chk.w, v_area_chk.y, rt_ui.anchorMin.y));
                Vector2 max_off_2 = p_info.OffsetMax + new Vector2(Mathf.Lerp(v_area_chk.x, v_area_chk.z, rt_ui.anchorMax.x),
                    Mathf.Lerp(v_area_chk.w, v_area_chk.y, rt_ui.anchorMax.y));

                rt_ui.offsetMin = min_off_2;
                rt_ui.offsetMax = max_off_2;
            }
            else if (rt_p != ThisPanel)
            {
                Rect rect_sr = GNewUITools.GetRelativeRect(rt_p, ThisPanel);
                Rect rect_pt = rt_p.rect;

                ////calc the difference of parent and the screen
                //v_area_chk.Set(rect_sr.xMin - rect_pt.xMin, rect_sr.yMax - rect_pt.yMax, rect_sr.xMax - rect_pt.xMax, rect_sr.yMin - rect_pt.yMin);

                rt_ui.anchorMin = Vector2.zero;
                rt_ui.anchorMax = Vector2.one;

                rt_ui.offsetMin = new Vector2(rect_sr.xMin - rect_pt.xMin, rect_sr.yMin - rect_pt.yMin);
                rt_ui.offsetMax = new Vector2(rect_sr.xMax - rect_pt.xMax, rect_sr.yMax - rect_pt.yMax);
            }

        }
    }

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    // Commented, Since Desinger want to cull from both sides.    >|    |<
    // no need to check for a change. 
    private void Update()
    {
#if IN_GAME
        if (EntryPoint.Instance.IsInited)
#endif
        {
            if (_FirstRun)
            {
                _FirstRun = false;
                InitSafeArea();
            }

            if (_curScreenOri != Screen.orientation)
            {
                _curScreenOri = Screen.orientation;
                ApplySafeArea();
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        ThisPanel = null;
        _KnownUI.Clear();
        Instance = null;
    }

    protected override void OnSafeInit()
    {
        base.OnSafeInit();
        if (ThisPanel == null)
        {
            ThisPanel = transform as RectTransform;
        }
    }

    public void Register(GameObject o_ui)
    {
        SafeInit();

        if (!_ShouldBeEnable) return;
#if UNITY_EDITOR
        if (_KnownUI.Count > 40)
        {
            Debug.Log("<UISizeAdapter> KnowUI>40 Clean");
            CleanNull();
        }
#endif

        InteractableUIHolder it_holder = o_ui.GetComponent<InteractableUIHolder>();
        if (it_holder != null)
        {
            if (!_KnownUI.Contains(it_holder))
            {
                _KnownUI.Add(it_holder);

                if (!_FirstRun)
                {
                    TuneUIPanels(it_holder, _LastUsingSafePadding, Vector4.zero);
                }
            }
        }
    }

    public void UnRegister(GameObject o_ui)
    {
        SafeInit();

        if (!_ShouldBeEnable) return;

        for (int i = 0; i < _KnownUI.Count; i++)
        {
            if (_KnownUI[i].gameObject == o_ui)
            {
                _KnownUI.RemoveAt(i);
                break;
            }
        }

        CleanNull();
    }

    [ContextMenu("CleanNull")]
    public void CleanNull()
    {
        int i = 0;
        while (i < _KnownUI.Count)
        {
            if (_KnownUI[i] == null)
            {
                _KnownUI.RemoveAt(i);
            }
            else
            {
                i += 1;
            }
        }
    }

    [ContextMenu("ChangeDevice")]
    public void ChangeDevice()
    {
        CurSafePadding = Vector4.zero;
        _FirstRun = true;
        _curScreenOri = ScreenOrientation.Unknown;
        enabled = true;
    }

}
