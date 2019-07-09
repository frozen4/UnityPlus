using SDK;
using UnityEngine;
using System.Collections.Generic;
using System;
using Common;
using System.Text.RegularExpressions;

public class LTPlatformWindows : LTPlatformBase
{
    public override PLATFORM_TYPE GetPlatformType()
    {
        return PLATFORM_TYPE.PLATFORM_ANY;
    }
    public override void Login() { }

    public override void LoginEx() { }

    public override void Logout(bool bCleanAutoLogin = false) { }

    public override void Unregister(LT_RESULT_NOTIFICATION_DELEGATE callback) { }

    public override void Pause() { }

    public override void Resume() { }

    public override void Exit() { }

    public override void SwitchAccount() { }

    public override void ManageAccount() { }

    public override void Buy(SDK.BUY_INFO pProduct) { }

    public override void InitSDK(LT_INITED_NOTIFICATION_DELEGATE fnInit)
    {
        _IsInited = true;
        if (fnInit != null)
            fnInit(INITED_STATE.LT_INITED_SUCCEED, -1);
    }

    public override void Init(LT_RESULT_NOTIFICATION_DELEGATE fnResult)
    {
        if (fnResult != null)
            fnResult(true);
    }

    public override void RegisterCallback(LT_LOGIN_NOTIFICATION_DELEGATE fnLogin,
        LT_LOGOUT_NOTIFICATION_DELEGATE fnLogout,
        LT_BUY_NOTIFICATION_DELEGATE fnBuy)
    {
        Debug.Log("C# RegisterCallback");
    }

    public override void ShowInAppWeb(string url, LT_URL_NOTIFICATION_DELEGATE callback)
    {
        var normalUrl = url;
        if (!IsUrl(url))
            normalUrl = string.Format("www.{0}.com", url);
        Application.OpenURL(normalUrl);
    }

    public static bool IsUrl(string str)
    {
        try
        {
            string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
            return Regex.IsMatch(str, Url);
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public override string GetCustomData(string key)
    {
        return key;
    }

    public override void InitPurchaseVerifyUrl(string url) { }
    public override void ProcessPurchaseCache() { }
}