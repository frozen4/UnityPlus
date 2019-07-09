using UnityEngine;
using System;
using Mono.Xml;
using LuaInterface;

public class Main
{
    // 缓存游戏过程中的一些常量，减少不必要的查询消耗

    // TODO: 
    public static Transform HostPalyer;
    public static Camera Main3DCamera;
    public static Camera TopPateCamera;
    public static PostProcessChain MainPostProcessChain;
    public static Camera UICamera;
    public static Transform UIRootCanvas;
    public static Transform TopPateCanvas;
    public static Transform PanelRoot;
    public static Transform PanelHUD;
    public static float HostPlayerRadius = 0.1f;       //hostplayer使用的碰撞半径

    public static GameObject PlayerLight { get; set; }
    public static GameObject UILight { get; set; }
    public static Transform BlockCanvas { get; set; }

    public static DateTime DateTimeBegin = new DateTime(1970, 1, 1);

    public static SecurityParser XMLParser = new SecurityParser();

#if IN_GAME
    //教学的LuaObject
    public static LuaTable LuaGuideMan;
#endif

    //private static bool _IsShowTopPate = false;
    //public static bool IsTopPateVisible = true;
    public static bool IsSleeping = false;
    public static bool IsHideTopPate = false;
    public static bool IsCGUsingUICam = false;
    public static bool IsCGGoing = false;
    public static int CGUICameraDepth = UICameraDepthDef;
    // UI摄像机原本层级
    private const int UICameraDepthDef = 2;
    public static bool OpenUIBlock = false;

    public enum CameraEnableMask
    {
        FullScreenUI = 0x01,
        VideoPlaying = 0x02,
    }

    private static int _CurCameraEnableMask = 0;

    public static void Tick(float dt)
    {
        if (MainPostProcessChain != null)
            MainPostProcessChain.Tick(dt);
    }

    public static bool IsInGame()
    {
        return Main3DCamera != null;
    }

    public static void SetShowHeadInfo(bool is_show)
    {
        if (TopPateCamera != null)
        {
            TopPateCamera.gameObject.SetActive(is_show);
        }
        //if (TopPateCanvas != null)
        //{
        //    TopPateCanvas.gameObject.SetActive(is_show);
        //}
    }

    public static void SetTopPatesVisible(bool enabled)
    {
        //if (TopPateCamera != null)
        //{
            //IsTopPateVisible = is_enable;
            //TopPateCamera.enabled = is_enable;
        //}

        if (TopPateCanvas != null)
        {
            GNewUITools.SetVisible(TopPateCanvas, enabled);
            //TopPateCanvas.localScale = is_enable ? Vector3.one : Vector3.zero;
        }
    }

    public static void EnableBlockCanvas(bool enabled)
    {
        if (Main.BlockCanvas != null)
        {
            GNewUITools.SetVisible(Main.BlockCanvas, enabled);
        }
    }

    public static void EnableMainCamera(bool enabled, CameraEnableMask mask)
    {
        if (enabled)
            _CurCameraEnableMask = (_CurCameraEnableMask & (~(int)mask));
        else
            _CurCameraEnableMask = (_CurCameraEnableMask | (int)mask);

        if (Main.Main3DCamera != null)
            Main.Main3DCamera.enabled = (_CurCameraEnableMask == 0);

        if (TopPateCamera != null)
            TopPateCamera.enabled = (_CurCameraEnableMask == 0);
    }

    public static void SetUIStateByCG(bool beReset, bool useUiCamera, int uiCameraDepth)
    {
        //var topPateCanvas = Main.TopPateCanvas;
        //if (topPateCanvas != null)
        //{
        //    topPateCanvas.gameObject.SetActive(beReset);
        //}
        SetTopPatesVisible(beReset);

        IsCGUsingUICam = useUiCamera;
        CGUICameraDepth = uiCameraDepth;
        IsCGGoing = !beReset;

        if (!useUiCamera)
        {
            //var blockCanvas = Main.BlockCanvas;
            //if (blockCanvas != null)
            //{
            //    //if (beReset == blockCanvas.activeSelf)
            //    //    blockCanvas.SetActive(!beReset);
            //    GNewUITools.SetVisible(blockCanvas, !beReset);
            //}
            EnableBlockCanvas(!beReset);
        }
        UpdateUICameraState();
    }

    public static void SetSleepingState(bool b_sleep)
    {
        IsSleeping = b_sleep;
        UpdateUICameraState();
    }

    public static void UpdateUICameraState()
    {
        var uiCamera = Main.UICamera;
        if (uiCamera != null)
        {
            if (IsSleeping)
            {
                uiCamera.depth = 99;
            }
            else
            {
                if (IsCGUsingUICam)
                {
                    uiCamera.depth = IsCGGoing ? CGUICameraDepth : UICameraDepthDef;
                }
                else
                {
                    uiCamera.enabled = !IsCGGoing;
                    uiCamera.depth = UICameraDepthDef;
                }
            }
        }
    }

}
