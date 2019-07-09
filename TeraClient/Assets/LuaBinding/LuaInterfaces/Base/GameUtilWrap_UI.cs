using UnityEngine;
using System;
using LuaInterface;
using Common;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using DG.Tweening;
using EZCameraShake;

public static partial class GameUtilWrap
{
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ShakeUIScreen(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        float magnitude = (float)LuaScriptMgr.GetNumber(L, 1);
        float roughness = (float)LuaScriptMgr.GetNumber(L, 2);
        float fadein_time = (float)LuaScriptMgr.GetNumber(L, 3);
        float fadeout_time = (float)LuaScriptMgr.GetNumber(L, 4);
        float last_time = (float)LuaScriptMgr.GetNumber(L, 5);
        //string shake_key = (string)LuaScriptMgr.GetString(L, 6);

        CameraShaker cs = Main.PanelRoot.GetComponent<CameraShaker>();
        if (cs != null)
        {
            cs.ShakeOnce(magnitude, roughness, fadein_time, fadeout_time, last_time, "ui");
        }
        else
        {
            HobaDebuger.LogWarning("error in ShakeUIScreen CameraShaker not found!");
        }

        return CheckReturnNum(L, count, nRet);
    }

    /// <summary>
    /// Tween slider value
    /// </summary>
    /// <param name="..">GameObject, endPos, dur, ease, callback</param>
    /// <returns>succeeded?</returns>
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoSlider(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float), typeof(float), typeof(int), typeof(LuaFunction)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoSlider -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            float toPos = (float)LuaScriptMgr.GetNumber(L, 2);
            float interval = (float)LuaScriptMgr.GetNumber(L, 3);
            Ease easeType = (Ease)LuaScriptMgr.GetNumber(L, 4);

            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 5);

            doTween.DoSlider(toPos, interval, easeType, callbackRef);
        }
        else
        {
            LogParamError("GameUtil :: DoSlider", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    /// <summary>
    /// Kill Tween slider from an object
    /// </summary>
    /// <param name="..">GameObject</param>
    /// <returns>succeeded?</returns>
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DoKillSlider(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: DoKillSlider -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            DoTweenComponent doTween = obj.GetComponent<DoTweenComponent>();
            if (doTween == null)
                doTween = obj.AddComponent<DoTweenComponent>();

            doTween.DoKillSlider();
        }
        else
        {
            LogParamError("GameUtil :: DoKillSlider", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ShowHUDText(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(int)))
        {
            string content = LuaScriptMgr.GetString(L, 1);
            if (string.IsNullOrEmpty(content))
            {
                HobaDebuger.LogError("ShowHUDText: param 1 cannot be null or empty");
                return CheckReturnNum(L, count, nRet);
            }
             
            GameObject target = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (target == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("ShowHUDText: param 2 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            int type = (int)LuaScriptMgr.GetNumber(L, 3);

            GameObject hurtobj = null;
            //             var holder = target.GetComponent<HangPointHolder>();
            //             if (holder != null)
            //                 hurtobj = holder.GetHangPoint(HangPointHolder.HangPointType.Hurt);
            //             else
            //                 hurtobj = target.FindChildRecursively("HangPoint_Hurt");
            // 
            //             if (hurtobj == null)
            //             {
            //                 HobaDebuger.LogError("ShowHUDText: GameObject 'HangPoint_Hurt' cant be found");
            //                 return CheckReturnNum(L, count, nRet);
            //             }
            hurtobj = target;   // 瑞龙需求，不跟挂点走 20171127
            var pos = hurtobj.transform.position;
            pos.y += (CUnityUtil.GetModelHeight(target, true) / 2);

            //Debug.LogWarning("HUDTextMan.Instance.ShowText " + bg_type);

            HUDTextMan.Instance.ShowText(content, pos, type);
        }
        else
        {
            LogParamError("ShowHUDText", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ClearHUDTextFontCache(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        HUDTextMan.Instance.Clear();
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetButtonInteractable(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetButtonInteractable: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            GButton gObj = obj.GetComponent<GButton>();
            if (gObj != null)
            {
                gObj.interactable = (bool)LuaScriptMgr.GetBoolean(L, 2);
            }

            Toggle iToggle = obj.GetComponent<Toggle>();
            if (iToggle != null)
            {
                iToggle.interactable = (bool)LuaScriptMgr.GetBoolean(L, 2);
            }
        }
        else
        {
            LogParamError("SetButtonInteractable", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetImageColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable))))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("SetImageColor: param 1 can not be null");
                return CheckReturnNum(L, count, nRet);
            }

            var image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogError("SetImageColor: param 1 has no Image Component");
                return CheckReturnNum(L, count, nRet);
            }
            image.color = LuaScriptMgr.GetColor(L, 2);

        }
        else if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("SetImageColor: param 1 can not be null");
                return CheckReturnNum(L, count, nRet);
            }

            var image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogError("SetImageColor: param 1 has no Image Component");
                return CheckReturnNum(L, count, nRet);
            }
            string colorS = LuaScriptMgr.GetString(L, 2);
            string[] colorRGB = colorS.Split(',');
            Color color = new Color();
            color.r = float.Parse(colorRGB[0]) / 255;
            color.g = float.Parse(colorRGB[1]) / 255;
            color.b = float.Parse(colorRGB[2]) / 255;
            color.a = 1;
            image.color = color;
        }
        else
        {
            LogParamError("SetImageColor", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetTextColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(Text), typeof(LuaTable))))
        {
            Text obj = LuaScriptMgr.GetUnityObject<Text>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("SetTextColor: param 1 must be Text");
                return CheckReturnNum(L, count, nRet);
            }

            obj.color = LuaScriptMgr.GetColor(L, 2);
        }
        else
        {
            LogParamError("SetTextColor", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetPanelUIObjectByID(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                //LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogWarning("GetPanelUIObjectByID: param 1 must be GameObject");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }

            var holder = obj.GetComponent<GUILinkHolder>();
            if (holder == null)
            {
                HobaDebuger.LogWarning("GetPanelUIObjectByID's param 1 must have component GUILinkHolder");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }

            int id = (int)LuaScriptMgr.GetNumber(L, 2);
            var res = holder.GetUIObject(id);

            if (res == null)
            {
                //LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogWarningFormat("GUILinkHolder.GetUIObject is null, holdername: {0}, id: {1}", holder.name, id);
            }

            LuaScriptMgr.Push(L, res);
        }
        else
        {
            LogParamError("GetPanelUIObjectByID", count);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AddCooldownComponent(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(double), typeof(double), typeof(GameObject), typeof(LuaFunction), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddCooldownComponent: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Image img_comp = obj.GetComponent<Image>();
            if (img_comp == null)
            {
                HobaDebuger.LogError("The GameObject to AddCooldownComponent must have Image Component");
                return CheckReturnNum(L, count, nRet);
            }

            obj.SetActive(true);

            CooldownComponent cdc = obj.GetComponent<CooldownComponent>();
            if (cdc == null)
                cdc = obj.AddComponent<CooldownComponent>();
            float elapsed_time = (float)LuaScriptMgr.GetNumber(L, 2) * 0.001f;
            float max_time = (float)LuaScriptMgr.GetNumber(L, 3) * 0.001f;
            var cd_time_label = LuaScriptMgr.GetUnityObject<GameObject>(L, 4);
            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 5);
            var bIsReverse = (bool)LuaScriptMgr.GetBoolean(L, 6);
            cdc.SetParam(max_time, max_time - elapsed_time, cd_time_label, callbackRef, bIsReverse);
        }
        else
        {
            LogParamError("AddCooldownComponent", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int RemoveCooldownComponent(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("AddCooldownComponent: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Image img_comp = obj.GetComponent<Image>();
            if (img_comp == null)
            {
                HobaDebuger.LogError("The GameObject to AddCooldownComponent must have Image Component");
                return CheckReturnNum(L, count, nRet);
            }

            obj.SetActive(false);

            CooldownComponent cdc = obj.GetComponent<CooldownComponent>();
            UnityEngine.Object.Destroy(cdc);
            var cd_time_label = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);

            if (cd_time_label != null)
                cd_time_label.SetActive(false);
        }
        else
        {
            LogParamError("RemoveCooldownComponent", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetCircleProgress(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(double))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("SetCircleProgress: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            obj.SetActive(true);

            GCircleProgress cdc = obj.GetComponent<GCircleProgress>();
            if (cdc == null)
            {
                HobaDebuger.LogError("GCircleProgress Component cant be found!");
                return CheckReturnNum(L, count, nRet);
            }

            float elapsed_time = (float)LuaScriptMgr.GetNumber(L, 2);
            cdc.SetTime(elapsed_time);
        }
        else
        {
            LogParamError("SetCircleProgress", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetJoystickAxis(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        Camera cam = Main.Main3DCamera;
        if (cam == null)
        {
            LuaScriptMgr.Push(L, 0);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }

        if (count == 1)
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("GetJoystickAxis: param 1 must be GameObject");

                LuaScriptMgr.Push(L, 0);
                LuaScriptMgr.Push(L, 0);
                return CheckReturnNum(L, count, nRet);
            }

            Joystick js = Joystick.Instance;
            if (js == null)
            {
                HobaDebuger.LogError("GetJoystickAxis's GameObject must have Joystick Component");

                LuaScriptMgr.Push(L, 0);
                LuaScriptMgr.Push(L, 0);
                return CheckReturnNum(L, count, nRet);
            }

            {
                var camera = cam;
                var camForward = camera.transform.forward;
                camForward.y = 0;
                camForward.Normalize();
                var camRight = camera.transform.right;
                camRight.y = 0;
                camRight.Normalize();
                var moveDir = camForward * js.JoystickAxis.y + camRight * js.JoystickAxis.x;

                LuaScriptMgr.Push(L, moveDir.x);
                LuaScriptMgr.Push(L, moveDir.z);
            }

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetJoystickAxis", count);

            LuaScriptMgr.Push(L, 0);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ContinueLogoMaskFade(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        Image mask = EntryPoint.Instance.ImgLogoMask;
        if (mask != null)
        {
            ScreenFadeEffectManager.Instance.StartFade(0.3f, 0.0f, 1f, mask);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSprite(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                return CheckReturnNum(L, count, nRet);
            }

            Image image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogWarning("the GameObject to SetSprite must have Image component");
                return CheckReturnNum(L, count, nRet);
            }

            var assetPath = LuaScriptMgr.GetString(L, 2);

            var sprite = CAssetBundleManager.SyncLoadAssetFromBundle<Sprite>(assetPath, "commonatlas", true);
            if (sprite != null)
                image.overrideSprite = sprite;
            else
            {
                HobaDebuger.LogFormat("the Sprite to set is not Sprite: {0}", assetPath);
            }
        }
        else
        {
            LogParamError("SetSprite", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSpriteFromResources(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                return CheckReturnNum(L, count, nRet);
            }

            Image image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogWarning("the GameObject to SetSprite must have Image component");
                return CheckReturnNum(L, count, nRet);
            }

            var iconPath = LuaScriptMgr.GetString(L, 2);

            var sprite = Resources.Load<Sprite>(iconPath);
            if (sprite != null)
                image.overrideSprite = sprite;
            else
            {
                HobaDebuger.LogFormat("the Sprite to set is not Sprite: {0}", iconPath);
            }
        }
        else
        {
            LogParamError("SetSpriteFromResources", count);
        }

        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int CleanSprite(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogWarning("CleanSprite: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Image image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogWarning("the GameObject to CleanSprite must have Image component");
                return CheckReturnNum(L, count, nRet);
            }

            image.sprite = null;
            image.overrideSprite = null;
        }
        else
        {
            LogParamError("CleanSprite", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    private static string DefaultItemIconPath = "Assets/Outputs/CommonAtlas/Icon/Item/Img_Item_Mark.png";
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetItemIcon(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogWarning("SetItemIcon: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Image image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogWarning("the GameObject to SetItemIcon must have Image component");
                return CheckReturnNum(L, count, nRet);
            }

            var assetPath = LuaScriptMgr.GetString(L, 2);

            if (string.IsNullOrEmpty(assetPath))
                assetPath = DefaultItemIconPath;

            var sprite = CAssetBundleManager.SyncLoadAssetFromBundle<Sprite>(assetPath, "commonatlas", true);

            if (sprite == null)
                sprite = CAssetBundleManager.SyncLoadAssetFromBundle<Sprite>(DefaultItemIconPath, "commonatlas", true);

            image.overrideSprite = sprite;
        }
        else
        {
            LogParamError("SetSprite", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    private static Texture2D CalculateTexture(Texture2D sourceTex)
    {
        int h = sourceTex.height;
        int w = sourceTex.width;
        float cx = sourceTex.width / 2;
        float cy = sourceTex.height / 2;
        float r = Mathf.Min(h, w) / 2;
        Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
        Texture2D b = new Texture2D(h, w);
        for (int i = 0; i < (h * w); i++)
        {
            int y = Mathf.FloorToInt(((float)i) / ((float)w));
            int x = Mathf.FloorToInt(((float)i - ((float)(y * w))));
            if (r * r >= (x - cx) * (x - cx) + (y - cy) * (y - cy))
            {
                b.SetPixel(x, y, c[i]);
            }
            else
            {
                b.SetPixel(x, y, Color.clear);
            }
        }
        b.Apply();
        return b;
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSpriteFromImageFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string))))
        {
            var filePath = LuaScriptMgr.GetString(L, 2);
            filePath = filePath.Replace('\\', '/');
            byte[] bytes = Util.ReadFile(filePath);
            if (bytes == null)
            {
                HobaDebuger.LogFormat("SetSpriteFromImageFile Failed to load: {0}", filePath);
                return CheckReturnNum(L, count, nRet);
            }

            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(bytes))
            {
                HobaDebuger.LogFormat("SetSpriteFromImageFile Failed to load: {0}", filePath);
                return CheckReturnNum(L, count, nRet);
            }

            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogWarning("SetSpriteFromImageFile: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            GRawImage gRawImage = obj.GetComponent<GRawImage>();

            if (null != gRawImage)
            {
                gRawImage.texture = texture;
            }
            else
            {
                Image image = obj.GetComponent<Image>();
                if (null == image)
                {
                    HobaDebuger.LogWarning("the GameObject to SetSpriteFromImageFile must have gRawImage component");
                    return CheckReturnNum(L, count, nRet);
                }
                texture = CalculateTexture(texture);
                byte[] newbytes = texture.EncodeToPNG();
                string tempPath = Application.persistentDataPath + "/HeadIcon.png";
                FileStream newFs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
                newFs.Write(newbytes, 0, newbytes.Length);
                newFs.Close();
                newFs.Dispose();
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.overrideSprite = sprite;
            }

        }
        else
        {
            LogParamError("SetSpriteFromImageFile", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    private static Material UIGrayMaterial = null;
    private static Material UIGrayMaterialClone = null;

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int MakeImageGray(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogWarning("MakeImageGray: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Image image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogWarning("the GameObject to MakeImageGray must have Image component");
                return CheckReturnNum(L, count, nRet);
            }

            var beGray = LuaScriptMgr.GetBoolean(L, 2);
            if (beGray)
            {
                if (UIGrayMaterial == null)
                    UIGrayMaterial = Resources.Load<Material>("Gray");

                image.material = UIGrayMaterial;
            }
            else
            {
                image.material = null;
            }
        }
        else if ((count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogWarning("MakeImageGray: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            Image image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogWarning("the GameObject to MakeImageGray must have Image component");
                return CheckReturnNum(L, count, nRet);
            }
            if (UIGrayMaterial == null)
                UIGrayMaterial = Resources.Load<Material>("Gray");
            if (UIGrayMaterialClone == null)
                UIGrayMaterialClone = CUnityUtil.Instantiate(UIGrayMaterial) as Material;
            Color color;
            var useDefalut = LuaScriptMgr.GetBoolean(L, 2);
            if (useDefalut)
            {
                color = new Color(0.39f, 0.39f, 0.39f);
            }
            else
            {
                var vec = LuaScriptMgr.GetVector3(L, 3);
                color = new Color(vec.x, vec.y, vec.z);
            }
            UIGrayMaterialClone.color = color;
            image.material = UIGrayMaterialClone;
        }
        else
        {
            LogParamError("MakeImageGray", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ChangeGraphicAlpha(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogWarning("ChangeGraphicAlpha: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            var graphic = obj.GetComponent<Graphic>();
            if (graphic == null)
            {
                HobaDebuger.LogWarning("the GameObject to ChangeGraphicAlpha must have graphic component");
                return CheckReturnNum(L, count, nRet);
            }

            var alpha = (float)LuaScriptMgr.GetNumber(L, 2);
            alpha = Mathf.Clamp01(alpha);
            var color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
        else
        {
            LogParamError("ChangeGraphicAlpha", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //public static int GetCanvasPostion(IntPtr L)
    //{
    //    int count = LuaDLL.lua_gettop(L);
    //    const int nRet = 1;

    //    if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
    //    {
    //        /**
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, transform.position, canvas.camera, out pos)
    //         * */
    //        GameObject go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
    //        if (null == go)
    //        {
    //            HobaDebuger.LogWarning("GetCanvasPostion: param 1 must be GameObject");
    //            return CheckReturnNum(L, count, nRet);
    //        }

    //        //GameObject canvas = GameObject.Find("UIRootCanvas");
    //        GameObject camobj = GameObject.Find("GUICamera");
    //        if (null == camobj)
    //        {
    //            HobaDebuger.LogWarning("GUICamera: Can not find!");
    //            return CheckReturnNum(L, count, nRet);
    //        }

    //        Camera uicam = camobj.GetComponent<Camera>();
    //        //Camera cam = Camera.main ?? Camera.current;
    //        var p = uicam.WorldToScreenPoint(go.transform.position);
    //        //HobaDebuger.Log(HobaString.Format("GetScreenHeight(): {0} ,{1}", Screen.height, Screen.width ));
    //        var itempos = new Vector2(p.x, p.y);

    //        LuaScriptMgr.Push(L, itempos);
    //        return CheckReturnNum(L, count, nRet);
    //    }
    //    else
    //    {
    //        LogParamError("GetCanvasPostion", count);
    //        LuaDLL.lua_pushnil(L);
    //        return CheckReturnNum(L, count, nRet);
    //    }
    //}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetMainCameraPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable))))
        {
            var pos = LuaScriptMgr.GetVector3(L, 1);
            Camera mainCam = Main.Main3DCamera;
            var p = mainCam.WorldToScreenPoint(pos);
            var screenPos = new Vector2(p.x, p.y);
            LuaScriptMgr.Push(L, screenPos);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetMainCameraPostion", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }

    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PreLoadUIFX(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var fx_path = LuaScriptMgr.GetString(L, 1);
            UISfxBehaviour.PreLoadUIFX(fx_path);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int IsPlayingUISfx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(GameObject)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject hookPoint = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            var isPlaying = UISfxBehaviour.IsPlaying(gfx, obj, hookPoint);
            LuaScriptMgr.Push(L, isPlaying);
        }
        else
        {
            LuaScriptMgr.Push(L, false);
            LogParamError("IsPlayingUISfx", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlayUISfx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(GameObject), typeof(float)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject hook_point = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            float duraton = (float)LuaScriptMgr.GetNumber(L, 4);
            GameObject fx_obj=UISfxBehaviour.Play(gfx, obj, hook_point, null, duraton);
            LuaScriptMgr.Push(L, fx_obj);
        }
        else if (count == 5 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(GameObject), typeof(float), typeof(float)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject hook_point = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            float duraton = (float)LuaScriptMgr.GetNumber(L, 4);
            float secondsStayInCache = (float)LuaScriptMgr.GetNumber(L, 5);
            GameObject fx_obj = UISfxBehaviour.Play(gfx, obj, hook_point, null, duraton, secondsStayInCache);
            LuaScriptMgr.Push(L, fx_obj);
        }
        else if (count == 6 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(GameObject), typeof(float), typeof(float), typeof(int)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject hook_point = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            float duraton = (float)LuaScriptMgr.GetNumber(L, 4);
            float secondsStayInCache = (float)LuaScriptMgr.GetNumber(L, 5);
            int orderOffset = (int)LuaScriptMgr.GetNumber(L, 6);
            GameObject fx_obj = UISfxBehaviour.Play(gfx, obj, hook_point, null, duraton, secondsStayInCache, orderOffset);
            LuaScriptMgr.Push(L, fx_obj);
        }
        else if (count == 7 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(GameObject), typeof(float), typeof(float), typeof(int), typeof(LuaFunction)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject hook_point = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            float duraton = (float)LuaScriptMgr.GetNumber(L, 4);
            float secondsStayInCache = (float)LuaScriptMgr.GetNumber(L, 5);
            int orderOffset = (int)LuaScriptMgr.GetNumber(L, 6);
            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 7);
            GameObject fx_obj = UISfxBehaviour.Play(gfx, obj, hook_point, null, duraton, secondsStayInCache, orderOffset, callbackRef);
            LuaScriptMgr.Push(L, fx_obj);
        }
        else
        {
            LogParamError("PlayUISfx", count);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlayUISfxClipped(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(GameObject), typeof(GameObject)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject hook_point = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            GameObject clipper = LuaScriptMgr.GetUnityObject<GameObject>(L, 4);
            GameObject fx_obj = UISfxBehaviour.Play(gfx, obj, hook_point, clipper);
            LuaScriptMgr.Push(L, fx_obj);
        }
        //duraton=-1,secondsStayInCache=20,orderOffset=1,callbackRef=nil
        else if (count == 8 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(GameObject), typeof(GameObject), typeof(float), typeof(float), typeof(int), typeof(LuaFunction)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject hook_point = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            GameObject clipper = LuaScriptMgr.GetUnityObject<GameObject>(L, 4);
            float duration = (float)LuaScriptMgr.GetNumber(L, 5);
            float secondsStayInCache = (float)LuaScriptMgr.GetNumber(L, 6);
            int orderOffset = (int)LuaScriptMgr.GetNumber(L, 7);
            LuaFunction callbackRef = LuaScriptMgr.GetLuaFunction(L, 8);
            GameObject fx_obj = UISfxBehaviour.Play(gfx, obj, hook_point, clipper, duration, secondsStayInCache, orderOffset, callbackRef);
            LuaScriptMgr.Push(L, fx_obj);
        }
        else
        {
            LogParamError("PlayUISfxClipped", count);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    //fx_name, target, hook_point, duration [,stay_in_cache_time]
    private static int StopUISfx(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (obj == null)
            {
                HobaDebuger.LogError("StopUISfx: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            UISfxBehaviour.Stop(gfx, obj);
        }else if(count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject), typeof(bool)))
        {
            var gfx = LuaScriptMgr.GetString(L, 1);
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (obj == null)
            {
                HobaDebuger.LogError("StopUISfx2: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }
            bool bDestroy = LuaScriptMgr.GetBoolean(L, 3);
            UISfxBehaviour.Stop(gfx, obj, bDestroy);
        }
        else
        {
            LogParamError("StopUISfx", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetUISfxLayer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            int nlayer = (int)LuaDLL.lua_tonumber(L, 2);

            if (obj != null)
            {
                UISfxLayer uilayer = obj.GetComponent<UISfxLayer>();
                if (uilayer == null)
                {
                    uilayer = obj.AddComponent<UISfxLayer>();
                }

                uilayer.SetSortingLayer(nlayer);
            }
        }
        else
        {
            LogParamError("SetUISfxLayer", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int EnableButton(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {
            GameObject target = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (target == null)
            {
                HobaDebuger.Log("EnableButton 1st param cant be null!");
                return CheckReturnNum(L, count, nRet);
            }

            GButton btn = target.GetComponent<GButton>();
            if (btn != null)
            {
                btn.interactable = LuaScriptMgr.GetBoolean(L, 2);
                btn.TransToSelectedWithoutCall();
            }
            else
            {
                Button btn2 = target.GetComponent<Button>();
                if (btn2 != null)
                    btn2.interactable = LuaScriptMgr.GetBoolean(L, 2);
            }
        }
        else
        {
            LogParamError("EnableButton", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetPanelSortingLayerOrder(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            int layer = (int)LuaScriptMgr.GetNumber(L, 2);
            int order = (int)LuaScriptMgr.GetNumber(L, 3);
            if (obj == null)
            {
                HobaDebuger.LogError("the GameObject param to method SetPanelSortingOrder is null. ");
                return CheckReturnNum(L, count, nRet);
            }
            Canvas cvs = obj.GetComponent<Canvas>();
            if (cvs == null)
            {
                cvs = obj.AddComponent<Canvas>();
            }

            GNewUITools.SetupUILayerOrder(cvs, order, layer, false);
        }
        else
        {
            LogParamError("SetPanelSortingLayerOrder", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //private static int SetPanelSortingOrder(IntPtr L)
    //{
    //    int count = LuaDLL.lua_gettop(L);
    //    const int nRet = 0;

    //    if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
    //    {
    //        GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
    //        int order = (int)LuaScriptMgr.GetNumber(L, 2);
    //        if (obj == null)
    //        {
    //            HobaDebuger.LogError("the GameObject param to method SetPanelSortingOrder is null. ");
    //            return CheckReturnNum(L, count, nRet);
    //        }
    //        Canvas cvs = obj.GetComponent<Canvas>();
    //        if (cvs == null)
    //        {
    //            cvs = obj.AddComponent<Canvas>();
    //        }
    //        //cvs.overrideSorting = true;
    //        //cvs.sortingOrder = order;
    //        ////GraphicRaycaster gr = obj.GetComponent<GraphicRaycaster>();
    //        ////if (gr == null)
    //        ////{
    //        ////    gr = obj.AddComponent<GraphicRaycaster>();
    //        ////}

    //        GNewUITools.SetupUIOrder(cvs, order);
    //        //TODO lizhixiong to check other things
    //    }
    //    else
    //    {
    //        LogParamError("SetPanelSortingOrder", count);
    //    }
    //    return CheckReturnNum(L, count, nRet);
    //}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int MovePanelSortingOrder(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            int order_mov = (int)LuaScriptMgr.GetNumber(L, 2);
            if (obj == null)
            {
                HobaDebuger.LogError("the GameObject param to method MovePanelSortingOrder is null. ");
                return CheckReturnNum(L, count, nRet);
            }

            Canvas[] cvs = obj.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < cvs.Length; i++)
            {
                if (cvs[i].overrideSorting)
                {
                    cvs[i].sortingOrder = cvs[i].sortingOrder + order_mov;
                }
            }

            GIACanvFixer[] gcf = obj.GetComponentsInChildren<GIACanvFixer>(true);
            for (int i = 0; i < gcf.Length; i++)
            {
                //if (gcf[i].IsValid)
                //{
                gcf[i].SOrder = gcf[i].SOrder + order_mov;
                //}
            }

            UISfxBehaviour[] sfx = obj.GetComponentsInChildren<UISfxBehaviour>(true);
            for (int i = 0; i < sfx.Length; i++)
            {
                sfx[i].MoveUSort(order_mov);
            }
        }
        else
        {
            LogParamError("MovePanelSortingOrder", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetPanelSortingOrder(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("the GameObject param to method GetPanelSortingOrder is null. ");
                //LuaDLL.lua_pushnil(L);
                LuaScriptMgr.Push(L, -1);
                return CheckReturnNum(L, count, nRet);
            }
            Canvas cvs = obj.GetComponent<Canvas>();
            if (cvs == null)
            {
                HobaDebuger.LogError("cannot find any canvas component on this GameObject.");
                //LuaDLL.lua_pushinteger(L, -1);
                LuaScriptMgr.Push(L, -1);
                return CheckReturnNum(L, count, nRet);
            }

            LuaScriptMgr.Push(L, GNewUITools.GetUIOrder(cvs));
        }
        else
        {
            LogParamError("GetPanelSortingOrder", count);
            LuaScriptMgr.Push(L, -1);
        }
        return CheckReturnNum(L, count, nRet);
    }

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //private static int SetPanelSortingLayer(IntPtr L)
    //{
    //    int count = LuaDLL.lua_gettop(L);
    //    const int nRet = 0;

    //    if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
    //    {
    //        GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
    //        int layer = (int)LuaScriptMgr.GetNumber(L, 2);
    //        if (obj == null)
    //        {
    //            HobaDebuger.LogError("the GameObject param to method SetPanelSortingLayer is null. ");
    //            return CheckReturnNum(L, count, nRet);
    //        }
    //        Canvas cvs = obj.GetComponent<Canvas>();
    //        if (cvs == null)
    //        {
    //            cvs = obj.AddComponent<Canvas>();
    //        }
    //        //cvs.overrideSorting = true;
    //        //cvs.sortingLayerID = layer;
    //        ////GraphicRaycaster gr = obj.GetComponent<GraphicRaycaster>();
    //        ////if (gr == null)
    //        ////{
    //        ////    gr = obj.AddComponent<GraphicRaycaster>();
    //        ////}

    //        GNewUITools.SetupUILayer(cvs, layer);

    //        //TODO lizhixiong to check other things
    //    }
    //    else
    //    {
    //        LogParamError("SetPanelSortingLayer", count);
    //    }
    //    return CheckReturnNum(L, count, nRet);
    //}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetPanelSortingLayer(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("the GameObject param to method GetPanelSortingLayer is null. ");
                //LuaDLL.lua_pushnil(L);
                LuaScriptMgr.Push(L, -1);
                return CheckReturnNum(L, count, nRet);
            }
            Canvas cvs = obj.GetComponent<Canvas>();
            if (cvs == null)
            {
                HobaDebuger.LogError("cannot find any canvas component on this GameObject.");
                //LuaDLL.lua_pushnil(L);
                LuaScriptMgr.Push(L, -1);
                return CheckReturnNum(L, count, nRet);
            }
            ////LuaScriptMgr.Push(L, cvs.sortingLayerID);
            //if (cvs.overrideSorting)
            //{
            //    LuaScriptMgr.Push(L, cvs.sortingLayerID);
            //}
            //else
            //{
            //    GIACanvFixer giac = cvs.GetComponent<GIACanvFixer>();
            //    if (giac)
            //    {
            //        LuaScriptMgr.Push(L, giac.SLayerID);
            //    }
            //    else
            //    {
            //        LuaScriptMgr.Push(L, -1);
            //    }
            //}
            LuaScriptMgr.Push(L, GNewUITools.GetUILayer(cvs));
        }
        else
        {
            LogParamError("GetPanelSortingLayer", count);
            LuaScriptMgr.Push(L, -1);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetFxSorting(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(int), typeof(bool)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            int setLayer = (int)LuaScriptMgr.GetNumber(L, 2);
            int setOrder = (int)LuaScriptMgr.GetNumber(L, 3);
            bool setChild = (bool)LuaScriptMgr.GetBoolean(L, 4);
            if (obj == null)
            {
                HobaDebuger.LogError("the GameObject param to method SetPanelSortingLayer is null. ");
                return CheckReturnNum(L, count, nRet);
            }

            Renderer[] _ChildRendererList = null;
            if (setChild)
            {
                if (_ChildRendererList == null)
                {
                    _ChildRendererList = obj.GetComponentsInChildren<Renderer>(true);
                }

                for (int i = 0; i < _ChildRendererList.Length; i++)
                {
                    _ChildRendererList[i].sortingOrder = setOrder;
                    _ChildRendererList[i].sortingLayerID = setLayer;
                }
            }
            else
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = setOrder;
                    renderer.sortingLayerID = setLayer;
                }
            }
        }
        else
        {
            LogParamError("SetPanelSortingLayer", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int Num2SortingLayerID(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int layer = (int)LuaScriptMgr.GetNumber(L, 1);
            LuaScriptMgr.Push(L, GNewUITools.Num2SortingLayerID(layer));
        }
        else
        {
            LogParamError("Num2SortingLayerID", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int HidePanel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                LuaScriptMgr.Instance.CallOnTraceBack();
                HobaDebuger.LogError("HidePanel: param 1 must be GameObject");
                return CheckReturnNum(L, count, nRet);
            }

            bool isHide = (bool)LuaScriptMgr.GetBoolean(L, 2);
            if (isHide)
            {
                Util.SetLayerRecursively(obj, CUnityUtil.Layer_Invisible);
            }
            else
            {
                Util.SetLayerRecursively(obj, CUnityUtil.Layer_UI);
            }

            GraphicRaycaster[] grs = obj.GetComponentsInChildren<GraphicRaycaster>(true);
            for (int i = 0; i < grs.Length; i++)
            {
                grs[i].enabled = !isHide;
            }
        }
        else
        {
            LogParamError("HidePanel", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetRenderTexture(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(GameObject), typeof(GameObject)))
        {
            int type = (int)LuaScriptMgr.GetNumber(L, 1);
            GameObject obj_img = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            GameObject obj_src = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);

            if (obj_img == null || obj_src == null)
            {
                HobaDebuger.LogError("null reference : obj_img or obj_src");
                return CheckReturnNum(L, count, nRet);
            }

            // tran_target.localRotation =
            if (type == 1)
            {
                Transform trans_camera = GameObject.Find("GUICamera").transform;
                Transform trans_objsrc = obj_src.transform;

                trans_objsrc.SetParent(trans_camera, false);
                trans_objsrc.localPosition = new Vector3(0f, -1.1f, 5f);
                trans_objsrc.Rotate(Vector3.up, 180f);
                Util.SetLayerRecursively(obj_src, CUnityUtil.Layer_UI);
                RawImage rimg = obj_img.GetComponent<RawImage>();
                if (rimg == null)
                {
                    rimg = obj_img.AddComponent<RawImage>();
                }
                rimg.texture = trans_camera.GetComponent<Camera>().targetTexture;
            }
            else
            {
            }
        }
        else
        {
            LogParamError("SetRenderTexture", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetGroupImg(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2)
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: SetGroupImg -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            GImageGroup img_grp = obj.GetComponent<GImageGroup>();
            if (img_grp == null)
            {
                string err_msg = "GameUtil :: SetGroupImg -> the GImageGroup component is null.4593 At .../" + GNewUITools.PrintScenePath(obj.transform, 5);
                HobaDebuger.LogError(err_msg);
                return CheckReturnNum(L, count, nRet);
            }

            if (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
            {
                int sprit_index = (int)LuaScriptMgr.GetNumber(L, 2);
                img_grp.SetImageIndex(sprit_index);
            }
            else if (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(string)))
            {
                string sprit_name = LuaScriptMgr.GetString(L, 2);
                img_grp.SetData(sprit_name);
            }
            else
            {
                LogParamError("SetGroupImg", count);
            }
        }
        else
        {
            LogParamError("SetGroupImg", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetBtnExpress(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2)
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GameUtil :: SetBtnExpress -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            GNewBtnExpress btn_exp = obj.GetComponent<GNewBtnExpress>();
            if (btn_exp == null)
            {
                HobaDebuger.LogError("GameUtil :: SetBtnExpress -> the GNewBtnExpress component is null");
                return CheckReturnNum(L, count, nRet);
            }

            if (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(Boolean)))
            {
                bool is_gray = (bool)LuaScriptMgr.GetBoolean(L, 2);
                btn_exp.OnValueChanged(!is_gray);
            }
            else
            {
                LogParamError("SetBtnExpress", count);
            }
        }
        else
        {
            LogParamError("SetBtnExpress", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetNativeSize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("SetImageColor: param 1 can not be null");
                return CheckReturnNum(L, count, nRet);
            }

            var image = obj.GetComponent<Image>();
            if (image == null)
            {
                HobaDebuger.LogError("SetImageColor: param 1 has no Image Component");
                return CheckReturnNum(L, count, nRet);
            }
            image.SetNativeSize();

        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetMaskTrs(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

		if ((count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(GameObject), typeof(GameObject),typeof(bool))))
        {
            var b = LuaScriptMgr.GetBoolean(L, 1);
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            var objmask = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
			var isLerp = LuaScriptMgr.GetBoolean(L, 4);
            if (b)
            {
                if (obj != null && objmask != null)
                {
                    var UIGuideMaskTrsSet = obj.GetComponent<UIGuideMaskTrsSet>();
                    UIGuideMaskTrsSet.enabled = b;
					UIGuideMaskTrsSet.IsLerp = isLerp;
                    UIGuideMaskTrsSet.SetMaskTrs(objmask);
                }
            }
            else
            {
                if (obj != null)
                {
                    var UIGuideMaskTrsSet = obj.GetComponent<UIGuideMaskTrsSet>();
                    UIGuideMaskTrsSet.MaskTrsReset();
                    //Debug.Log("MaskTrsReset");
                }
            }

        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int RegisterUIEventHandler(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject), typeof(int)))
        {
            var panelObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            var ctrlObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (panelObj == null || ctrlObj == null)
            {
                HobaDebuger.LogError("One of RegisterClickEvent's params is null");
                return CheckReturnNum(L, count, nRet);
            }
            var uiEventListener = panelObj.GetComponent<UIEventListener>();
            if (uiEventListener == null)
            {
                HobaDebuger.LogError("Can NOT Register ClickEvent to a panel without UIEventListener");
                return CheckReturnNum(L, count, nRet);
            }

            var type = (int)LuaScriptMgr.GetNumber(L, 3);
            uiEventListener.RegisterSingleObjHandler(ctrlObj, type, false);
        }
        else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject), typeof(int), typeof(bool)))
        {
            var panelObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            var ctrlObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (panelObj == null || ctrlObj == null)
            {
                HobaDebuger.LogError("One of RegisterClickEvent's params is null");
                return CheckReturnNum(L, count, nRet);
            }
            var uiEventListener = panelObj.GetComponent<UIEventListener>();
            if (uiEventListener == null)
            {
                HobaDebuger.LogError("Can NOT Register ClickEvent to a panel without UIEventListener");
                return CheckReturnNum(L, count, nRet);
            }
            var type = (int)LuaScriptMgr.GetNumber(L, 3);
            var recursion = (bool)LuaScriptMgr.GetBoolean(L, 4);
            uiEventListener.RegisterSingleObjHandler(ctrlObj, type, recursion);
        }
        else
        {
            LogParamError("RegisterClickEvent", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetScreenPosToTargetPos(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj != null)
            {
                //Transform root = GameObject.Find("UIRootCanvas").transform;
                Transform root = GameObjectUtil.GetUIRootTranform();
                if (root == null)
                    return 0;

                Canvas rootCanvas = root.GetComponent<Canvas>();
                Vector2 pos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(obj.transform as RectTransform, Input.mousePosition, rootCanvas.worldCamera, out pos))
                {
                    RectTransform rectTarget = obj.GetComponent<RectTransform>();
                    if (rectTarget)
                    {
                        if ((pos.x < -rectTarget.rect.width / 2) ||
                            (pos.x > rectTarget.rect.width / 2) ||
                            (pos.y < -rectTarget.rect.height / 2) ||
                            (pos.y > rectTarget.rect.height / 2))
                        {
                            LuaDLL.lua_pushnil(L);
                        }
                        else
                        {
                            LuaScriptMgr.Push(L, pos);
                        }

                        return CheckReturnNum(L, count, nRet);
                    }
                }
            }
        }
        else
        {
            LogParamError("GetScreenPosToTargetPos", count);
            LuaDLL.lua_pushnil(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ChangeGradientBtmColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaTable)))
        {
            GameObject textObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (textObj == null)
            {
                HobaDebuger.LogError("GameUtil :: ChangeGradientBtmColor -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }
            Color iColor = LuaScriptMgr.GetColor(L, 2);

            GGradient iGradient = textObj.GetComponent<GGradient>();
            if (iGradient == null)
            {
                HobaDebuger.LogError("GameUtil :: ChangeGradientBtmColor -> havn't GGradient component");
                return CheckReturnNum(L, count, nRet);
            }
            iGradient.ChangeBottomColor(iColor);
        }
        else
        {
            LogParamError("ChangeGradientBtmColor", count);
            LuaDLL.lua_pushnil(L);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PlaySequenceFrame(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(LuaFunction)))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("PlaySequenceFrame -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            var player = obj.GetComponent<GImageGroupPlayer>();
            if (player == null)
            {
                HobaDebuger.LogError("the GameObject to PlaySequenceFrame has no GImageGroupPlayer component.");
                return CheckReturnNum(L, count, nRet);
            }

            Action callback = null;
            if (LuaDLL.lua_isfunction(L, 2))
            {
                LuaDLL.lua_pushvalue(L, 3);
                int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
                callback = () =>
                {
                    if (LuaScriptMgr.Instance.GetLuaState() == null || callbackRef == 0)
                        return;

                    var oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                    LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                    if (LuaDLL.lua_isnil(L, -1))
                    {
                        LuaDLL.lua_settop(L, oldTop);
                        return;
                    }
                    if (!LuaScriptMgr.Instance.GetLuaState().PCall(0, 0))
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                    LuaDLL.lua_settop(L, oldTop);
                };
            }
            if (player.IsLoop && callback != null)
                HobaDebuger.LogWarning("The SequenceFrame to play is loop, and the Complete callback should be null");

            player.Play(callback);
        }
        else
        {
            LogParamError("PlaySequenceFrame", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int StopSequenceFrame(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("StopSequenceFrame -> the GameObject is null.");
                return CheckReturnNum(L, count, nRet);
            }

            var player = obj.GetComponent<GImageGroupPlayer>();
            if (player == null)
            {
                HobaDebuger.LogError("the GameObject to StopSequenceFrame has no GImageGroupPlayer component.");
                return CheckReturnNum(L, count, nRet);
            }
            player.Stop();
        }
        else
        {
            LogParamError("StopSequenceFrame", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AdjustDropdownRect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("AdjustDropdownRect: param 1 must be UnityEngine.Object");
                return CheckReturnNum(L, count, nRet);
            }
            Dropdown dd = go.GetComponent<Dropdown>();
            if (dd == null)
            {
                HobaDebuger.LogError("AdjustDropdownRect: GameObject does't hava Dropdown");
                return CheckReturnNum(L, count, nRet);
            }
            float height = 32.3f;
            var item = (RectTransform)dd.template.Find("Viewport/Content/Item");
            if (item != null)
                height = item.rect.height;
            int num = (int)LuaScriptMgr.GetNumber(L, 2);
            RectTransform template = dd.template;
            if (template != null)
            {
                template.sizeDelta = new Vector2(template.rect.width, num * height);
            }
        }
        else
        {
            LogParamError("AdjustDropdownRect", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetDropdownValue(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("SetDropdownValue: param 1 must be UnityEngine.Object");
                return CheckReturnNum(L, count, nRet);
            }
            Dropdown dd = go.GetComponent<Dropdown>();
            if (dd == null)
            {
                HobaDebuger.LogError("SetDropdownValue: GameObject does't hava Dropdown");
                return CheckReturnNum(L, count, nRet);
            }
            int index = (int)LuaScriptMgr.GetNumber(L, 2);
            dd.value = index;
        }
        else
        {
            LogParamError("SetDropdownValue", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetTipsPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject)))
        {
            GameObject itemObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            GameObject tipsObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);

            Transform root = GameObjectUtil.GetUIRootTranform();
            if (root == null)
                return 0;
            Canvas rootCanvas = root.GetComponent<Canvas>();
            if (itemObj != null && tipsObj != null)
            {
                RectTransform root_rect = rootCanvas.GetComponent<RectTransform>();
                RectTransform item_rect = itemObj.GetComponent<RectTransform>();
                RectTransform tip_rect = tipsObj.GetComponent<RectTransform>();

                Rect item_bd = GNewUITools.GetRelativeRect(root_rect, item_rect);
                Rect root_bd = root_rect.rect;
                Rect tip_bd = tip_rect.rect;

                Vector2 a_pos = new Vector2();

                if (item_bd.xMin - root_bd.xMin > tip_bd.width)
                {
                    a_pos.x = item_bd.xMin - tip_bd.width;
                }
                else
                {
                    a_pos.x = item_bd.xMax;
                }

                float tip_height = tip_bd.height;
                if (item_bd.yMax - root_bd.yMin > tip_height)
                {
                    a_pos.y = item_bd.yMax;
                }
                else if (root_bd.yMax - item_bd.yMin > tip_height)
                {
                    a_pos.y = item_bd.yMin + tip_height;
                }
                //上面不够下面不够 放中间
                else //if (item_bd.yMax - root_bd.yMin < tip_height && root_bd.yMax - item_bd.yMax < tip_height)
                {
                    a_pos.y = root_bd.yMax / 2 + 110;
                }

                a_pos.x += tip_bd.width * tip_rect.pivot.x;
                a_pos.y += tip_height * (tip_rect.pivot.y - 1);

                Vector3 new_pos = new Vector3(a_pos.x, a_pos.y, 0);
                new_pos = root_rect.TransformPoint(new_pos);
                tip_rect.position = new_pos;
            }
        }
        else
        {
            LogParamError("SetTipsPosition", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetApproachPanelPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject)))
        {
            GameObject itemObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            GameObject tipsObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);

            Transform root = GameObjectUtil.GetUIRootTranform();
            if (root == null)
                return 0;
            Canvas rootCanvas = root.GetComponent<Canvas>();
            if (itemObj != null && tipsObj != null)
            {
                RectTransform root_rect = rootCanvas.GetComponent<RectTransform>();
                RectTransform item_rect = itemObj.GetComponent<RectTransform>();
                RectTransform panel_rect = tipsObj.GetComponent<RectTransform>();

                Rect item_bd = GNewUITools.GetRelativeRect(root_rect, item_rect);
                Rect root_bd = root_rect.rect;
                Rect panel_bd = panel_rect.rect;
                Vector2 a_pos = new Vector2();

                if (root_bd.xMax - item_bd.xMax > panel_bd.width)
                {
                    a_pos.x = item_bd.xMax;
                }
                else
                {
                    a_pos.x = item_bd.xMin - panel_bd.width;
                }
                if (root_bd.yMax - item_bd.yMax < panel_bd.height)
                {
                    if (root_bd.yMax - item_bd.yMin > panel_bd.height)
                        a_pos.y = item_bd.yMin + panel_bd.height;
                    else
                        a_pos.y = item_bd.yMin;
                }
                else
                {
                    if (item_bd.yMax - item_bd.yMin < panel_bd.height)
                        a_pos.y = item_bd.yMin + panel_bd.height;
                    else
                        a_pos.y = item_bd.yMax;
                }
                a_pos.x += panel_bd.width * panel_rect.pivot.x;
                a_pos.y += panel_bd.height * (1 - panel_rect.pivot.y);
                Vector3 new_pos = new Vector3(a_pos.x, a_pos.y, 0);
                new_pos = root_rect.TransformPoint(new_pos);
                panel_rect.position = new_pos;
            }
        }
        else
        {
            LogParamError("SetApproachPanelPosition", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetGiftItemPosition(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject)))
        {
            GameObject itemObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            GameObject tipsObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);

            Transform root = GameObjectUtil.GetUIRootTranform();
            if (root == null)
                return 0;
            Canvas rootCanvas = root.GetComponent<Canvas>();
            if (itemObj != null && tipsObj != null)
            {
                RectTransform root_rect = rootCanvas.GetComponent<RectTransform>();
                RectTransform item_rect = itemObj.GetComponent<RectTransform>();
                RectTransform panel_rect = tipsObj.GetComponent<RectTransform>();

                Rect item_bd = GNewUITools.GetRelativeRect(root_rect, item_rect);
                Rect root_bd = root_rect.rect;
                Rect panel_bd = panel_rect.rect;
                Vector2 a_pos = new Vector2();

                if (root_bd.xMax - item_bd.xMax > panel_bd.width)
                {
                    a_pos.x = item_bd.xMax;
                }
                else
                {
                    a_pos.x = item_bd.xMin - panel_bd.width;
                }
                a_pos.y = item_bd.yMax;
                a_pos.x += panel_bd.width * panel_rect.pivot.x;
                a_pos.y += panel_bd.height * (1 - panel_rect.pivot.y);
                Vector3 new_pos = new Vector3(a_pos.x, a_pos.y, 0);
                new_pos = root_rect.TransformPoint(new_pos);
                panel_rect.position = new_pos;
            }
        }
        else
        {
            LogParamError("SetGiftItemPosition", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetTipLayoutHeight(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;
        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj == null)
            {
                HobaDebuger.LogError("GetTipLayoutHeight -> the GameObject is null.");
                LuaScriptMgr.Push(L, 0);
                return CheckReturnNum(L, count, nRet);
            }

            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null)
            {
                HobaDebuger.LogError("GetTipLayoutHeight -> the GameObject RectTransform is null.");
                LuaScriptMgr.Push(L, 0);
                return CheckReturnNum(L, count, nRet);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            float tip_height = LayoutUtility.GetPreferredHeight(rect);
            if (tip_height > 580)
            {
                tip_height = 580;
            }
            LuaScriptMgr.Push(L, tip_height);
        }
        else
        {
            LogParamError("GetTipLayoutHeight", count);
            LuaScriptMgr.Push(L, -1);
        }
        return CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetOutlineColor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(GameObject)))
        {
            string str = LuaScriptMgr.GetString(L, 1);
            string color = "#" + str;
            var Obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (Obj == null)
            {
                HobaDebuger.LogWarning("SetOutlineColor: param 2 must be UnityEngine.Object");
                return CheckReturnNum(L, count, nRet);
            }
            Outline outline = Obj.GetComponent<Outline>();
            if (outline == null)
            {
                HobaDebuger.LogWarning("SetOutlineColor: GameObject does't hava Outline");
                return CheckReturnNum(L, count, nRet);
            }
            Color value;
            ColorUtility.TryParseHtmlString(color, out value);
            outline.effectColor = value;
        }
        else
        {
            LogParamError("SetOutlineColor", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetActiveOutline(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool)))
        {

            var Obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            bool isActive = LuaScriptMgr.GetBoolean(L, 2);
            if (Obj == null)
            {
                HobaDebuger.LogWarning("SetActiveOutline: param 2 must be UnityEngine.Object");
                return CheckReturnNum(L, count, nRet);
            }
            Outline outline = Obj.GetComponent<Outline>();
            if (outline == null)
            {
                HobaDebuger.LogWarning("SetActiveOutline: GameObject does't hava Outline");
                return CheckReturnNum(L, count, nRet);
            }
            outline.enabled = isActive;
        }
        else
        {
            LogParamError("SetActiveOutline", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetPreferredHeight(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        float ret = 0;
        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(RectTransform)))
        {
            RectTransform rect = LuaScriptMgr.GetUnityObject<RectTransform>(L, 1);
            if (rect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                ret = LayoutUtility.GetPreferredHeight(rect);
            }
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(Text)))
        {
            Text text = LuaScriptMgr.GetUnityObject<Text>(L, 1);
            if (text)
            {
                ret = text.preferredHeight;
            }
        }
        else
        {
            HobaDebuger.LogError("GetPreferredHeight: param 1 must be RectTransform");
        }
        LuaScriptMgr.Push(L, ret);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetTextAlignment(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogWarning("SetAlignmentType: param 1 must be GameObject");
            }
            else
            {
                var textComp = go.GetComponent<Text>();
                if (textComp != null)
                {
                    var aligment = (TextAnchor)((int)LuaScriptMgr.GetNumber(L, 2));
                    textComp.alignment = aligment;
                }
            }
        }
        else
        {
            LogParamError("SetTextAlignment", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OpenOrCloseLoginLogo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool isOpen = LuaScriptMgr.GetBoolean(L, 1);
            EntryPoint.Instance.EnablePanelLogo(isOpen);
        }
        else
        {
            HobaDebuger.LogError("OpenOrCloseLoginLogo: param 1 must be boolean");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetUIAllowDrag(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("SetUIAllowDrag: GameObject is null");
                return CheckReturnNum(L, count, nRet);
            }
            string dragTag = CUnityUtil.Tag_UIDrag;

#if UNITY_EDITOR
            bool hasTag = CUnityUtil.HasTag(dragTag);
            if (!hasTag)
            {
                HobaDebuger.LogError("SetUIAllowDrag: Do not have _UIDrag_ tag");
                return CheckReturnNum(L, count, nRet);
            }
#endif

            go.tag = dragTag;
        }
        else
        {
            HobaDebuger.LogError("SetUIAllowDrag: param 1 must be GameObject");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AlignUiElementWithOther(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(GameObject), typeof(int), typeof(int))))
        {
            GameObject aligneWithObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (null == aligneWithObj)
            {
                HobaDebuger.LogWarning("AlignUiElementWithOther:: aligneWithObj = null");
                return CheckReturnNum(L, count, nRet);
            }

            GameObject targetObj = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            if (null == targetObj)
            {
                HobaDebuger.LogWarning("AlignUiElementWithOther:: targetObj = null");
                return CheckReturnNum(L, count, nRet);
            }

            var aligneWithObjTrans = aligneWithObj.GetComponent<RectTransform>();
            var targetRectTrans = targetObj.GetComponent<RectTransform>();
            targetRectTrans.position = aligneWithObjTrans.position;
            var anchoredPos = targetRectTrans.anchoredPosition;
            anchoredPos.x += (int)LuaScriptMgr.GetNumber(L, 3);
            anchoredPos.y += (int)LuaScriptMgr.GetNumber(L, 4);
            targetRectTrans.anchoredPosition = anchoredPos;
        }
        else
        {
            LogParamError("AlignUiElementWithOther", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int WorldPositionToCanvas(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (null == obj)
            {
                HobaDebuger.LogWarning("WorldPositionToCanvas:: obj = null");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }

            //Transform root = GameObject.Find("UIRootCanvas").transform;
            Transform root = GameObjectUtil.GetUIRootTranform();
            var pos = root.worldToLocalMatrix.MultiplyPoint(obj.transform.position);

            // var pos = CUnityUtil.WorldPositionToCanvas(obj);
            LuaScriptMgr.Push(L, pos);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("WorldPositionToCanvas", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    //Add a canvas and set to world space
    private static int SetupWorldCanvas(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count > 0 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (null == obj)
            {
                HobaDebuger.LogWarning("SetupWorldCanvas obj = null");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }

            Canvas canv = obj.GetComponent<Canvas>();
            if (canv == null)
            {
                canv = obj.AddComponent<Canvas>();
            }
            canv.renderMode = RenderMode.WorldSpace;
            canv.worldCamera = Main.Main3DCamera;

            if (count == 2 && LuaScriptMgr.CheckType(L, typeof(float), 2))
            {
                float scale = (float)LuaScriptMgr.GetNumber(L, 2);
                CanvasScaler cs = obj.GetComponent<CanvasScaler>();
                if (cs == null) cs = obj.AddComponent<CanvasScaler>();
                cs.dynamicPixelsPerUnit = scale;
            }

            //CanvasScaler cvs = obj.GetComponent<CanvasScaler>();
            //if (cvs == null)
            //{
            //    cvs = obj.AddComponent<CanvasScaler>();
            //}
            //cvs.dynamicPixelsPerUnit = 1.5f;
        }
        else
        {
            LogParamError("SetupWorldCanvas", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    ///if you want to use this function,check your anchors pivot and the origin you want
    private static int SetScrollPositionZero(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (null == obj)
            {
                HobaDebuger.LogWarning("SetScrollPositionZero obj = null");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }
            ScrollRect scrollRect = obj.GetComponent<ScrollRect>();
            float x = scrollRect.content.anchoredPosition.x;

#if UNITY_EDITOR
            Debug.Assert(scrollRect.content.pivot == scrollRect.content.anchorMax && scrollRect.content.anchorMax == scrollRect.content.anchorMin, "<SetScrollPositionZero> 节点不对找UE " + GNewUITools.PrintScenePath(scrollRect.transform) + "\n scrollRect.content.pivot != scrollRect.content.anchorMax || scrollRect.content.anchorMax != scrollRect.content.anchorMin!");

#endif

            obj.GetComponent<ScrollRect>().content.anchoredPosition = new Vector2(x, 0);
        }
        else
        {
            LogParamError("SetScrollPositionZero", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]

    private static int SetScrollEnabled(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(Boolean))))
        {
            GameObject obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            bool state = LuaScriptMgr.GetBoolean(L, 2);
            if (null == obj)
            {
                HobaDebuger.LogWarning("SetScrollEnabled obj = null");
                LuaDLL.lua_pushnil(L);
                return CheckReturnNum(L, count, nRet);
            }
            ScrollRect scrollRect = obj.GetComponent<ScrollRect>();
            scrollRect.enabled = state;
        }
        else
        {
            LogParamError("SetScrollEnabled", count);
        }

        return CheckReturnNum(L, count, nRet);
    }
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetupUISorting(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(int), typeof(int)))
        {
            GameObject go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("SetupUISorting: GameObject is null");
                return CheckReturnNum(L, count, nRet);
            }

            int arg_layer = (int)LuaScriptMgr.GetNumber(L, 2);
            int arg_order = (int)LuaScriptMgr.GetNumber(L, 3);

            InteractableUIHolder holder = go.GetComponent<InteractableUIHolder>();
            if (holder == null)
            {
                HobaDebuger.LogError("SetupUISorting: InteractableUIHolder not found at " + go.name);
                return CheckReturnNum(L, count, nRet);
            }

            holder.SetupUISorting(GNewUITools.Num2SortingLayerID(arg_layer), arg_order);
        }
        else
        {
            HobaDebuger.LogError("SetupUISorting: params must be GameObject, int, int");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetAllTogglesOff(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("SetAllTogglesOff: GameObject is null");
                return CheckReturnNum(L, count, nRet);
            }
            var group = go.GetComponent<ToggleGroup>();
            if (group == null)
            {
                HobaDebuger.LogError("SetAllTogglesOff: GameObject has no ToggleGroup");
                return CheckReturnNum(L, count, nRet);
            }
            group.SetAllTogglesOff();
        }
        else
        {
            LogParamError("SetAllTogglesOff", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    ///获得物体在UI画板上的坐标和画板尺寸
    ///returns x, y: local pos of the object , z, w: size of the canvas
    public static int GetRootCanvasPosAndSize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go != null && Main.UIRootCanvas != null)
            {
                Transform t_go = go.transform;
                RectTransform t_root = Main.UIRootCanvas as RectTransform;
                Rect rt_root = t_root.rect;
                Vector4 v4 = t_root.InverseTransformPoint(t_go.position);
                v4.z = rt_root.width;
                v4.w = rt_root.height;
                LuaScriptMgr.Push(L, v4);
                return CheckReturnNum(L, count, nRet);
            }
        }

        LogParamError("GetUIRootPosAndSize, ", count);
        LuaDLL.lua_pushnil(L);
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RegisterTip(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject g_obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (g_obj != null)
            {
                InputManager.Instance.RegisterTip(g_obj);
                return CheckReturnNum(L, count, nRet);
            }
            Debug.LogError("RegisterTip failed.");
        }
        else
        {
            LogParamError("RegisterTip(GameObject, LuaFunction)", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int UnregisterTip(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            GameObject g_obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (g_obj != null)
            {
                InputManager.Instance.UnregisterTip(g_obj);
                return CheckReturnNum(L, count, nRet);
            }
            Debug.LogError("UnregisterTip failed.");
        }
        else
        {
            LogParamError("UnregisterTip(GameObject, LuaFunction)", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCurrentVersion(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            LuaDLL.lua_pushstring(L, EntryPoint.Instance.CurrentVersion);
        }
        else
        {
            LuaDLL.lua_pushstring(L, string.Empty);
            LogParamError("GetCurrentVersion", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableBlockCanvas(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(bool))))
        {
            bool enable = LuaScriptMgr.GetBoolean(L, 1);
            Main.EnableBlockCanvas(enable);
        }
        else
        {
            LogParamError("EnableBlockCanvas", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowScreenShot(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("ShowScreenShot: GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }
            var raw_image = go.GetComponent<RawImage>();
            if (raw_image == null)
            {
                HobaDebuger.LogError("ShowScreenShot: GameObject has no RawImage component");
                return CheckReturnNum(L, count, nRet);
            }
            raw_image.texture = CScreenShotMan.Instance.ScreenShot;
        }
        else
        {
            LogParamError("ShowScreenShot", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    //开启反面射线检测 
    public static int EnableReversedRaycast(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("EnableReversedRaycast: GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }
            var ray_caster = go.GetComponent<GraphicRaycaster>();
            if (ray_caster == null)
            {
                HobaDebuger.LogError("EnableReversedRaycast: GameObject has no GraphicRaycaster component");
                return CheckReturnNum(L, count, nRet);
            }
            ray_caster.ignoreReversedGraphics = false;
        }
        else
        {
            LogParamError("EnableReversedRaycast", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    //修改组透明度
    public static int SetCanvasGroupAlpha(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(float))))
        {
            var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (go == null)
            {
                HobaDebuger.LogError("SetCanvasGroupAlpha: GameObject got null");
                return CheckReturnNum(L, count, nRet);
            }
            var canvas_group = go.GetComponent<CanvasGroup>();
            if (canvas_group == null)
            {
                HobaDebuger.LogError("SetCanvasGroupAlpha: GameObject has no CanvasGroup component");
                return CheckReturnNum(L, count, nRet);
            }
            float alpha = (float)LuaScriptMgr.GetNumber(L, 2);
            canvas_group.alpha = alpha;
        }
        else
        {
            LogParamError("SetCanvasGroupAlpha", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //public static int LayoutTopTabs(IntPtr L)
    //{
    //    UnityEngine.Profiling.Profiler.BeginSample("LayoutTopTabs");
    //    int count = LuaDLL.lua_gettop(L);
    //    const int nRet = 0;

    //    if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
    //    {
    //        var go = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
    //        if (go == null)
    //        {
    //            HobaDebuger.LogError("LayoutTopTabs: GameObject got null");
    //            return CheckReturnNum(L, count, nRet);
    //        }

    //        HorizontalLayoutGroup hg = go.GetComponent<HorizontalLayoutGroup>();
    //        hg.enabled = false;
    //        Transform t_root = go.transform;
    //        int c_cnt = t_root.childCount;
    //        float x_pos = hg.padding.left;
    //        for (int i = 0; i < c_cnt; i++)
    //        {
    //            RectTransform t_child = t_root.GetChild(i) as RectTransform;
    //            if (!t_child.gameObject.activeSelf) continue;
    //            LayoutElement ile = t_child.GetComponent<LayoutElement>();
    //            if (!ile.ignoreLayout)
    //            {
    //                Vector2 p = t_child.pivot;
    //                Vector2 pos = t_child.localPosition;
    //                pos.x = x_pos + p.x * t_child.rect.width;
    //                pos.y = (1 - p.y) * t_child.rect.height;
    //                t_child.localPosition = pos;

    //                x_pos += t_child.rect.width;
    //            }
    //        }
    //        //hg.enabled = true;
    //    }
    //    else
    //    {
    //        LogParamError("LayoutTopTabs", count);
    //    }

    //    UnityEngine.Profiling.Profiler.EndSample();
    //    return CheckReturnNum(L, count, nRet);
    //}

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //static int SetImageAlpha(IntPtr L)
    //{
    //    int count = LuaDLL.lua_gettop(L);
    //    const int nRet = 0;
    //    if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(UnityEngine.UI.Image), typeof(float))))
    //    {
    //        var obj = LuaScriptMgr.GetUnityObject<UnityEngine.UI.Image>(L, 1);
    //        var col = obj.color;
    //        obj.color = new Color(col.r, col.g, col.b, (float)LuaScriptMgr.GetNumber(L, 2));
    //    }
    //    else
    //    {
    //        HobaDebuger.LogError("SetImageAlpha: Input Params not correct !");
    //    }
    //    return CheckReturnNum(L, count, nRet);
    //}

    //[MonoPInvokeCallback(typeof(LuaCSFunction))]
    //static int SetTextAlpha(IntPtr L)
    //{
    //    int count = LuaDLL.lua_gettop(L);
    //    const int nRet = 0;
    //    if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(UnityEngine.UI.Text), typeof(float))))
    //    {
    //        var obj = LuaScriptMgr.GetUnityObject<UnityEngine.UI.Text>(L, 1);
    //        var col = obj.color;
    //        obj.color = new Color(col.r, col.g, col.b, (float)LuaScriptMgr.GetNumber(L, 2));
    //    }
    //    else
    //    {
    //        HobaDebuger.LogError("SetTextAlpha: Input Params have correct !");
    //    }
    //    return CheckReturnNum(L, count, nRet);
    //}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetIgnoreLayout(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 2 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject), typeof(bool))))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            LayoutElement le = obj.GetComponent<LayoutElement>();
            if (le == null)
            {
                le = obj.AddComponent<LayoutElement>();
            }
            le.ignoreLayout = LuaScriptMgr.GetBoolean(L, 2);
        }
        else
        {
            HobaDebuger.LogError("SetIgnoreLayout: Input Params have correct !");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetShowHeadInfo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(bool))))
        {
            Main.SetShowHeadInfo(LuaScriptMgr.GetBoolean(L, 1));
        }
        else
        {
            HobaDebuger.LogError("SetShowHeadInfo: Input Params have correct !");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int ResetMask2D(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject))))
        {
            var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            if (obj != null)
            {
                Transform t = obj.transform;
                RectMask2D mask = t.GetComponentInParent<RectMask2D>();
                if (mask != null)
                {
                    mask.enabled = false;
                    mask.enabled = true;
                }
            }
        }
        else
        {
            HobaDebuger.LogError("ResetMask2D: Input Params have correct !");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int DebugLogUIRT(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(string))))
        {
            //var obj = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);
            //if (obj != null)
            //{
            //    obj = Main.PanelRoot.gameObject;
            //}

            Transform root = Main.PanelRoot;
            string str = LuaScriptMgr.GetLuaString(L, 1);
            if (string.IsNullOrEmpty(str))
            {
                root = root.Find(str);
            }

            RectTransform rt = root.gameObject.GetComponent<RectTransform>();
            if (rt)
            {
                Debug.LogWarning(rt.gameObject.name + ": " + rt.localPosition + ",[" + rt.anchorMax + "," + rt.anchorMin + "," + rt.offsetMax + "," + rt.offsetMin + "]," + rt.pivot + ", " + rt.rect);
            }
        }
        else
        {
            HobaDebuger.LogError("DebugLogUIRT: Input Params have correct !");
        }
        return CheckReturnNum(L, count, nRet);
    }

}
