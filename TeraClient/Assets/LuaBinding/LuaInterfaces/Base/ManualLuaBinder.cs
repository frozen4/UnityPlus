using System;
using UnityEngine;

public static class ManualLuaBinder
{
    public static void Bind(IntPtr L)
    {
        AnimationWrap.Register(L);
        ApplicationWrap.Register(L);
        //GameUtilWrap.Register(L);
        TimerUtilWrap.Register(L);
        UnityUtilWrap.Register(L);
        PlatformSDKWrap.Register(L);
        GUIWrap.Register(L);
        ObjectWrap.Register(L);
        GameObjectWrap.Register(L);
        CameraWrap.Register(L);
        VoiceUtilWrap.Register(L);
        GImageModelWrap.Register(L);
        GWebViewWrap.Register(L);
    }
}
