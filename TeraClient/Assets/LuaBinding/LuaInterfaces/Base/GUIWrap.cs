using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using LuaInterface;
using Common;

public static class GUIWrap
{
    public static LuaMethod[] gui_regs = new LuaMethod[]
    {
        new LuaMethod("SetGroupToggleOn", SetGroupToggleOn),
        new LuaMethod("SetText", SetText),
	    //new LuaMethod("SetArtFontText", SetArtFontText),
		new LuaMethod("SetTextAndChangeLayout", SetTextAndChangeLayout),
        new LuaMethod("SetImageAndChangeLayout", SetImageAndChangeLayout),
        new LuaMethod("SetTextColor", SetTextColor),
        new LuaMethod("SetAlpha", SetAlpha),
        new LuaMethod("GetChildFromTemplate", GetChildFromTemplate),
        new LuaMethod("SetDropDownOption", SetDropDownOption),
		new LuaMethod("SetDropDownOption2", SetDropDownOption2),
        new LuaMethod("SetTextAlignment",SetTextAlignment),
		new LuaMethod("SetRectTransformStretch", SetRectTransformStretch),
        //new LuaMethod("UseStandardInputField", UseStandardInputField),      //只接受标准文字
    };

    public static void Register(IntPtr L)
    {
        LuaScriptMgr.RegisterLib(L, "GUI", gui_regs);
        gui_regs = null;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetGroupToggleOn(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int))))
        {
            var groupObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (groupObj == null)
            {
                HobaDebuger.LogWarning("SetGroupToggleOn: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            var group = groupObj.GetComponent<ToggleGroup>();
            if (group == null)
            {
                HobaDebuger.LogWarning("SetGroupToggleOn: param 1 must have ToggleGroup component " + GNewUITools.PrintScenePath(groupObj.transform, 5));
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            var toggleIndex = (int)LuaScriptMgr.GetNumber(L, 2);
            var toggleObj = groupObj.transform.GetChild(toggleIndex - 1);
            //var toggleObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (toggleObj == null)
            {
                HobaDebuger.LogWarning("SetGroupToggleOn: param 2 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            var toggle = toggleObj.GetComponent<Toggle>();
            if (toggle == null)
            {
                HobaDebuger.LogWarning("SetGroupToggleOn: param 2 must have Toggle component " + GNewUITools.PrintScenePath(groupObj.transform, 5));
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            group.SetAllTogglesOff();
            toggle.isOn = true;
        }
        else
        {
            GameUtilWrap.LogParamError("SetGroupToggleOn", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetText(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            var labelObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (labelObj == null)
            {
                HobaDebuger.LogWarning("SetText: param 1 must be GameObject");
                LuaScriptMgr.Instance.CallOnTraceBack();
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            var textComp = labelObj.GetComponent<Text>();
            GText gtextComp = null;
            if (textComp == null)
                gtextComp = labelObj.GetComponent<GText>();

            if (textComp == null && gtextComp == null)
            {
                HobaDebuger.LogWarning("SetText: param 1 must have text component");
                LuaScriptMgr.Instance.CallOnTraceBack();
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            var content = LuaScriptMgr.GetString(L, 2);
            if (null != textComp)
                textComp.text = content;
            else
                gtextComp.text = content;
        }
        else
        {
            //Debug.LogError(LuaStatic.GetTraceBackInfo(L));

            GameUtilWrap.LogParamError("SetText", count);
            LuaScriptMgr.Instance.CallOnTraceBack();
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

//     [MonoPInvokeCallback(typeof(LuaCSFunction))]
//     public static int SetArtFontText(IntPtr L)
//     {
//         int count = LuaDLL.lua_gettop(L);
//         const int nRet = 0;
// 
//         if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
//         {
//             var labelObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
//             if (labelObj == null)
//             {
//                 HobaDebuger.LogWarning("SetArtFontText: param 1 must be GameObject");
//                 return GameUtilWrap.CheckReturnNum(L, count, nRet);
//             }
// 
//             var textComp = labelObj.GetComponent<Text>();
// 
//             if (textComp == null)
//             {
//                 HobaDebuger.LogWarning("SetArtFontText: param 1 must have text component");
//                 return GameUtilWrap.CheckReturnNum(L, count, nRet);
//             }
// 
//             var content = LuaScriptMgr.GetString(L, 2);
//             textComp.text = content;
// 
//             var fontGo = Resources.Load("UI/ArtFontList") as GameObject;
//             var fontList = fontGo.GetComponent<ArtFontList>();
//             textComp.font = fontList.Fonts[0];
//         }
//         else
//         {
//             GameUtilWrap.LogParamError("SetArtFontText", count);
//         }
//         return GameUtilWrap.CheckReturnNum(L, count, nRet);
//     }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetTextAndChangeLayout(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string), typeof(int))))
        {
            var labelObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (labelObj == null)
            {
                HobaDebuger.LogWarning("SetTextAndChangeLayout: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            var textComp = labelObj.GetComponent<Text>();
            if (textComp == null)
            {
                HobaDebuger.LogWarning("SetTextAndChangeLayout: param 1 must have text component");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            var content = LuaScriptMgr.GetString(L, 2);
            textComp.text = content;
            var maxPreferredWidth = (int)LuaScriptMgr.GetNumber(L, 3);
            var layout = labelObj.GetComponent<LayoutElement>();
            if (maxPreferredWidth > 0 && layout != null)
            {
                float pw = textComp.preferredWidth;

                if (pw > maxPreferredWidth)
                    layout.preferredWidth = maxPreferredWidth;
                else
                    layout.preferredWidth = pw + 1;
            }
        }
        else
        {
            GameUtilWrap.LogParamError("SetTextAndChangeLayout", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetImageAndChangeLayout(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        // gameObject, voiceSecond, maxWidth
        if ((count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(int))))
        {
            var labelObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (labelObj == null)
            {
                HobaDebuger.LogWarning("SetImageAndChangeLayout: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            Image imgComp = labelObj.GetComponent<Image>();
            if (imgComp == null)
            {
                HobaDebuger.LogWarning("SetImageAndChangeLayout: param 1 must have image component");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            int imageWidth = (int)LuaScriptMgr.GetNumber(L, 2);

            var maxPreferredWidth = (int)LuaScriptMgr.GetNumber(L, 3);
            var layout = labelObj.GetComponent<LayoutElement>();
            if (maxPreferredWidth > 0 && layout != null)
            {

                if (imageWidth > maxPreferredWidth)
                    layout.preferredWidth = maxPreferredWidth;
                else
                    layout.preferredWidth = imageWidth;
            }
        }
        else
        {
            GameUtilWrap.LogParamError("SetImageAndChangeLayout", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetTextColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetTextColor: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            Color color = LuaScriptMgr.GetColor(L, 2);
            var text = obj.GetComponent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.color = color;
            }
            else
            {
                HobaDebuger.LogError("SetTextColor: param 2 must be color");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
        }
        else
        {
            GameUtilWrap.LogParamError("SetTextColor", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetAlpha(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetAlpha: param 1 must be GameObject");
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            var a = (int)LuaScriptMgr.GetNumber(L, 2);
            var graphic = obj.GetComponent<Graphic>();
            if (graphic != null)
            {
                var c = graphic.color;
                c.a = a / 255f;
                graphic.color = c;
            }
            else
            {
                HobaDebuger.LogError("Failed to call SetAlpha");
            }
        }
        else
        {
            GameUtilWrap.LogParamError("SetAlpha", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetChildFromTemplate(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int))))
        {
            var labelObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (labelObj == null)
            {
                HobaDebuger.LogWarning("GetChildFromTemplate: param 1 must be GameObject");
                LuaDLL.lua_pushnil(L);
            }
            else
            {
                var tempComp = labelObj.GetComponent<UITemplate>();
                if (tempComp == null)
                {
                    HobaDebuger.LogWarning("GetChildFromTemplate: param 1 must have UITemplate component");
                    LuaDLL.lua_pushnil(L);
                }
                else
                {
                    var id = (int)LuaScriptMgr.GetNumber(L, 2);
                    var ret = tempComp.GetControl(id);
                    if (ret != null)
                        LuaScriptMgr.Push(L, ret);
                    else
                        LuaDLL.lua_pushnil(L);
                }
            }
        }
        else
        {
            GameUtilWrap.LogParamError("GetChildFromTemplate", count);
            LuaDLL.lua_pushnil(L);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetDropDownOption(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            var target = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (target == null)
            {
                HobaDebuger.LogWarning("GUIWrap :: AddDrowDownOption: param 1 must be GameObject");
            }
            else
            {
                var dorp = target.GetComponent<Dropdown>();
                if (!dorp)
                {
                    HobaDebuger.LogWarning("GUIWrap :: AddDrowDownOption: there isnt a DropDown component on this GameObject.");
                }
                else
                {
                    var options = LuaScriptMgr.GetString(L, 2);
                    if (string.IsNullOrEmpty(options))
                    {
                        HobaDebuger.LogWarning("GUIWrap :: AddDrowDownOption: param 2 IsNullOrEmpty.");
                    }
                    else
                    {
                        dorp.ClearOptions();
                        string[] opts = options.Split(',');
                        dorp.AddOptions(new List<string>(opts));
                    }
                }
            }
        }
        else
        {
            GameUtilWrap.LogParamError("SetDropDownOption", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetDropDownOption2(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string), typeof(string))))
        {
            var target = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (target == null)
            {
                HobaDebuger.LogWarning("GUIWrap :: SetDropDownOption2: param 1 must be GameObject");
            }
            else
            {
                var dorp = target.GetComponent<Dropdown>();
                var img_group = target.GetComponent<GImageGroup>();
                if (!dorp)
                {
                    HobaDebuger.LogWarning("GUIWrap :: SetDropDownOption2: there isnt a DropDown component on this GameObject.");
                }
                else if (!img_group)
                {
                    HobaDebuger.LogWarning("GUIWrap :: SetDropDownOption2: there isnt a ImageGroup component on this GameObject.");
                }
                else
                {
                    var options = LuaScriptMgr.GetString(L, 2);
                    var options2 = LuaScriptMgr.GetString(L, 3);
                    if (string.IsNullOrEmpty(options))
                    {
                        HobaDebuger.LogWarning("GUIWrap :: SetDropDownOption2: param 2 IsNullOrEmpty.");
                    }
                    else if (string.IsNullOrEmpty(options2))
                    {
                        HobaDebuger.LogWarning("GUIWrap :: SetDropDownOption2: param 3 IsNullOrEmpty.");
                    }
                    else
                    {
                        dorp.ClearOptions();
                        string[] opts = options.Split(',');
                        string[] opts2 = options2.Split(',');

#if UNITY_EDITOR
                        if (opts.Length != opts2.Length)
                        {
                            HobaDebuger.LogError("GUIWrap :: SetDropDownOption2: options options2 length not equal!!! " + options+" ; " + options2);
                        }
#endif

                        List<UnityEngine.UI.Dropdown.OptionData> optionDatas = new List<UnityEngine.UI.Dropdown.OptionData>();
                        for (int i = 0; i < opts.Length && i < opts2.Length; i++)
                        {
                            int i_op;
                            if (!int.TryParse(opts2[i], out i_op)) { i_op = -1; }
                            var sprite = img_group.GetSprite(i_op);

                            //CAssetBundleManager.SyncLoadAssetFromBundle<Sprite>(opts2[i], "commonatlas");
                            UnityEngine.UI.Dropdown.OptionData data = new UnityEngine.UI.Dropdown.OptionData(opts[i], sprite);
                            optionDatas.Add(data);
                        }
                        dorp.AddOptions(optionDatas);

                    }
                }
            }
        }
        else
        {
            GameUtilWrap.LogParamError("SetDropDownOption2", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetTextAlignment(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int))))
        {
            var target = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (target == null)
            {
                HobaDebuger.LogWarning("SetTextAlingment :: param 1 must be GameObject");
            }
            else
            {
                int num = (int)LuaScriptMgr.GetNumber(L, 2);
                Text t = target.GetComponent<Text>();
                t.alignment = (TextAnchor)num;
            }
        }
        else
        {
            GameUtilWrap.LogParamError("SetTextAlignment", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetRectTransformStretch(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var target = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (target == null)
            {
                HobaDebuger.LogWarning("AddListOperationEvent :: param 1 must be GameObject");
            }
            else
            {
                RectTransform rt = target.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 1);
                rt.offsetMin = new Vector2(0, 0);
                rt.offsetMax = new Vector2(0, 0);
            }
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
}