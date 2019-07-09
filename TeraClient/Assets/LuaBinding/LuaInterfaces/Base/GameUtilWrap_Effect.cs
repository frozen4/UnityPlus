using UnityEngine;
using System;
using LuaInterface;
using Common;
using EZCameraShake;

public static partial class GameUtilWrap
{
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int AddCameraOrScreenEffect(IntPtr L)
    {
        // 如果处于CG中，当前相机会enable false，此时忽略这些效果

        Camera cam = Main.Main3DCamera;
        if (cam == null || !cam.enabled) return 0;

        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        int effectType = (int)LuaScriptMgr.GetNumber(L, 1);

        if (effectType == 1)  // 相机振动
        {
            // 淡入时间，淡出时间，持续时间，振幅，频率
            float fadein_time = (float)LuaScriptMgr.GetNumber(L, 2);
            float fadeout_time = (float)LuaScriptMgr.GetNumber(L, 3);
            float last_time = (float)LuaScriptMgr.GetNumber(L, 4);
            float magnitude = (float)LuaScriptMgr.GetNumber(L, 5);
            float roughness = (float)LuaScriptMgr.GetNumber(L, 6);
            string shake_key = (string)LuaScriptMgr.GetString(L, 7);
            CameraShaker cs = cam.gameObject.GetComponent<CameraShaker>();
            if (cs == null)
                cam.gameObject.AddComponent<CameraShaker>();

            CameraShaker.Instance.ShakeOnce(magnitude, roughness, fadein_time, fadeout_time, last_time, shake_key);
        }
        else if (effectType == 2)  // CameraTransformEffect
        {
            float dis_ratio = (float)LuaScriptMgr.GetNumber(L, 2);   // 拉近距离系数，百分比
            float change_duration = (float)LuaScriptMgr.GetNumber(L, 3);  // 拉近时间
            float keep_duration = (float)LuaScriptMgr.GetNumber(L, 4);  // 最近点停留时间
            float change_back_duration = (float)LuaScriptMgr.GetNumber(L, 5); // 相机复位时间
            CCamCtrlMan.Instance.SetStretchingParams(dis_ratio, change_duration, keep_duration, change_back_duration);
        }
        else if (effectType == 3)  // 运动模糊
        {
            float blur_lv = (float)LuaScriptMgr.GetNumber(L, 2);  // 模糊程度
            float fadein_duration = (float)LuaScriptMgr.GetNumber(L, 3);
            float time = (float)LuaScriptMgr.GetNumber(L, 4);   // 模糊时间
            float fadeout_duration = (float)LuaScriptMgr.GetNumber(L, 5);
            float total_time = fadein_duration + time + fadeout_duration;
            var post = cam.GetComponent<PostProcessChain>();
            if (null != post)
            {
                post.MotionBlurParamter.x = blur_lv; // level
                post.MotionBlurParamter.y = total_time; // duration
                post.MotionBlurParamter.z = fadein_duration / total_time; // fade in
                post.MotionBlurParamter.w = (total_time - fadeout_duration) / total_time; // fade out
                post.EnableMotionBlur = true;

                post.DisableRadialBlur(total_time);
            }
        }
        else if (effectType == 4)  // 屏幕变色
        {
            var is_open = (bool)LuaScriptMgr.GetBoolean(L, 2);
            var fade_in = (float)LuaScriptMgr.GetNumber(L, 3);
            var keep = (float)LuaScriptMgr.GetNumber(L, 4);
            var fade_out = (float)LuaScriptMgr.GetNumber(L, 5);
            var r = (float)LuaScriptMgr.GetNumber(L, 6);
            var g = (float)LuaScriptMgr.GetNumber(L, 7);
            var b = (float)LuaScriptMgr.GetNumber(L,8);
            var a = (float)LuaScriptMgr.GetNumber(L,9);
            var mask = cam.GetComponent<CameraBlackMask>();
            if(null == mask)
                mask = cam.gameObject.AddComponent<CameraBlackMask>();
            if (is_open)
                mask.SetData(fade_in, keep, fade_out, r, g, b, a);
            mask.enabled = is_open;           
        }
        else
        {
            HobaDebuger.LogWarningFormat("AddCameraOrScreenEffect - Effect Type {0} is not defined", effectType);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int StopSkillScreenEffect(IntPtr L)
    {
        // 如果处于CG中，当前相机会enable false，此时忽略这些效果
        Camera cam = Main.Main3DCamera;
        if (cam == null || !cam.enabled)
            return 0;
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;
        if ( LuaScriptMgr.CheckTypes(L, 1, typeof(double)))
        {
            int effect_type = (int)LuaScriptMgr.GetNumber(L, 1);

            if (effect_type == 1)  // 打断相机振动
            {
                string key = (string)LuaScriptMgr.GetString(L, 2);
                CameraShaker cs = cam.gameObject.GetComponent<CameraShaker>();
                if (cs != null && key != string.Empty)
                {
                    cs.RemoveShakeNodeByKey(key);
                }
                else
                {
                    HobaDebuger.LogWarning("error in StopSkillScreenEffect value check !");
                }
            }
            else if (effect_type == 2)  // 打断相机距离变换
            {
                CCamCtrlMan.Instance.StopCameraStretching();
            }
            else if (effect_type == 3)  // 打断RadialBlur
            {
                var obj_go = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
                var blur = obj_go.GetComponentInChildren<RadialBlurBoot>();
                if (blur != null)
                {
                    blur.StopEffect();
                }
            }     
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableSpecialVisionEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        Camera cam = Main.Main3DCamera;
        if (cam == null)
            return CheckReturnNum(L, count, nRet);

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool isOn = LuaScriptMgr.GetBoolean(L, 1);
            DynamicEffectManager.Instance.EnableSpecialVisionEffect(isOn);
        }
        else
        {
            LogParamError("SpecialVisionEffect", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    private static void RemoveMainCameraEffect()
    {
        Camera cam = Main.Main3DCamera;
        if (cam == null)
            return;

        var postProcessChain = cam.gameObject.GetComponent<PostProcessChain>();
        if (postProcessChain != null)
            UnityEngine.Object.Destroy(postProcessChain);

        var cs = cam.gameObject.GetComponent<CameraShaker>();
        if (cs != null)
            UnityEngine.Object.Destroy(cs);

        var comMask = cam.gameObject.GetComponent<CameraBlackMask>();
        if (comMask != null)
            UnityEngine.Object.Destroy(comMask);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int StartScreenFade(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(float), typeof(float)))
        {
            var form = (float)LuaScriptMgr.GetNumber(L, 1);
            var to = (float)LuaScriptMgr.GetNumber(L, 2);
            var duration = (float)LuaScriptMgr.GetNumber(L, 3);
            ScreenFadeEffectManager.Instance.StartFade(form, to, duration);

        }
        else
        {
            LogParamError("StartScreenFade", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ClearScreenFadeEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            ScreenFadeEffectManager.Instance.ClearFadeEffect();

        }
        else
        {
            LogParamError("StartScreenFade", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetSceneEffect(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(GameObject)))
        {
            var paramGo = LuaScriptMgr.GetUnityObject<GameObject>(L, 1);

            if (paramGo != null)
            {
                var lightingSetting = paramGo.GetComponent<LightingSetting>();
                if (lightingSetting != null)
                    lightingSetting.Enable(true);

                var ppChainHolder = paramGo.GetComponent<PPChainHolder>();
                if (ppChainHolder != null)
                    ppChainHolder.EnablePostProcessChain();
            }
        }
        else
        {
            LogParamError("SetSceneEffect", count);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CaptureScreen(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            var main_cam = Main.Main3DCamera;
            if (main_cam == null)
            {
                HobaDebuger.LogError("CaptureScreen: Main3DCamera got null");
                return CheckReturnNum(L, count, nRet);
            }
            var post = main_cam.GetComponent<PostProcessChain>();
            if (post == null)
            {
                HobaDebuger.LogError("CaptureScreen: Main3DCamera has no PostProcessChain");
                return CheckReturnNum(L, count, nRet);
            }
            post.CaptureScreen();
        }
        else
        {
            LogParamError("CaptureScreen", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SaveScreenShot(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            CScreenShotMan.Instance.SaveScreenShot();
        }
        else
        {
            LogParamError("SaveScreenShot", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AbandonScreenShot(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            CScreenShotMan.Instance.AbandonScreenShot();
        }
        else
        {
            LogParamError("AbandonScreenShot", count);
        }
        return CheckReturnNum(L, count, nRet);
    }
}
