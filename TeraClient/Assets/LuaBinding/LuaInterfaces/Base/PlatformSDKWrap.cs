using System;
using UnityEngine;
using LuaInterface;
using Common;
using SDK;
using System.Collections.Generic;

public static class PlatformSDKWrap
{
    public static LuaMethod[] platform_regs = new LuaMethod[]
    {
        new LuaMethod("RegisterCallback", RegisterCallback),
        new LuaMethod("GetPlatformType", GetPlatformType),

        new LuaMethod("Initialize", Initialize),
        new LuaMethod("Login", Login),
        new LuaMethod("GetLoginStatus", GetLoginStatus),
        new LuaMethod("CheckLoginStatus", CheckLoginStatus),
        new LuaMethod("CheckAutoLogin", CheckAutoLogin),
        new LuaMethod("Logout", Logout),
        new LuaMethod("LogoutDirectly", LogoutDirectly),
        new LuaMethod("AccountConversion", AccountConversion),
        new LuaMethod("Unregister", Unregister),
        new LuaMethod("EnablePush", EnablePush),
        new LuaMethod("GetPushStatus", GetPushStatus),
        new LuaMethod("ShowCustomerCenter", ShowCustomerCenter),
        new LuaMethod("ShowAnnouncement", ShowAnnouncement),
        new LuaMethod("ShowInAppWeb", ShowInAppWeb),
        new LuaMethod("ShowPromotion", ShowPromotion),
        new LuaMethod("ShowGachaOdds", ShowGachaOdds),
        new LuaMethod("ShowCoupon", ShowCoupon),
        new LuaMethod("GetCustomData", GetCustomData),
        new LuaMethod("IsGoogleGameLogined", IsGoogleGameLogined),
        new LuaMethod("GoogleGameLogin", GoogleGameLogin),
        new LuaMethod("GoogleGameLogout", GoogleGameLogout),
        new LuaMethod("ShowGoogleAchievementView", ShowGoogleAchievementView),
        new LuaMethod("CompleteGoogleAchievement", CompleteGoogleAchievement),
        new LuaMethod("SetGoogleAchievementCompletionLevel", SetGoogleAchievementCompletionLevel),
        new LuaMethod("GetLoginJson", GetLoginJson),
        new LuaMethod("UploadRoleInfo", UploadRoleInfo),
        new LuaMethod("SetBreakPoint", SetBreakPoint),
        new LuaMethod("IsPlatformExitGame", IsPlatformExitGame),
        new LuaMethod("ExitGame", ExitGame),

        new LuaMethod("InitializeIAP", InitializeIAP),
        new LuaMethod("DoPurchase", DoPurchase),
        new LuaMethod("CleanPurchaseInfo", CleanPurchaseInfo),
        new LuaMethod("ProcessPurchaseCache", ProcessPurchaseCache),
        new LuaMethod("SetProductIds", SetProductIds),

        // Test FileBilling
        new LuaMethod("DoFillBillTest", DoFillBillTest),
    };

    public static void Register(IntPtr L)
    {
        LuaScriptMgr.RegisterLib(L, "PlatformSDK", platform_regs);
        platform_regs = null;
    }

    public static void LogParamError(string methodName, int count)
    {
        HobaDebuger.LogErrorFormat("invalid arguments to method: PlatformSDK.{0} count: {1}", methodName, count);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RegisterCallback(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction), typeof(LuaFunction), typeof(LuaFunction)))
        {
            var fnLoginCallback = LuaScriptMgr.GetLuaFunction(L, 1);
            var fnLogoutCallback = LuaScriptMgr.GetLuaFunction(L, 2);
            var fnBuyCallback = LuaScriptMgr.GetLuaFunction(L, 3);
            PlatformControl.RegisterCallback(fnLoginCallback, fnLogoutCallback, fnBuyCallback);
        }
        else
        {
            LogParamError("RegisterCallback", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetPlatformType(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            int type = (int)PlatformControl.GetPlatformType();
            LuaScriptMgr.Push(L, type);
        }
        else
        {
            LogParamError("GetPlatformType", count);
            LuaScriptMgr.Push(L, -1);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Initialize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.Init(callback);
        }
        else
        {
            LogParamError("Initialize", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Login(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.Login();
        }
        else if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int))))
        {
            int type = (int)LuaScriptMgr.GetNumber(L, 1);
            PlatformControl.Login(type);
        }
        else
        {
            LogParamError("Login", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetLoginStatus(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            LuaScriptMgr.Push(L, PlatformControl.IsLogined);
        }
        else
        {
            LogParamError("GetLoginStatus", count);
            LuaScriptMgr.Push(L, false);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CheckLoginStatus(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.CheckLoginStatus();
        }
        else
        {
            LogParamError("CheckLoginStatus", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CheckAutoLogin(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.CheckAutoLogin();
        }
        else
        {
            LogParamError("CheckAutoLogin", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Logout(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.Logout();
        }
        else
        {
            LogParamError("Logout", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int LogoutDirectly(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.LogoutDirectly();
        }
        else
        {
            LogParamError("LogoutDirectly", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int AccountConversion(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.AccountConversion(callback);
        }
        else
        {
            LogParamError("AccountConversion", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Unregister(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.Unregister(callback);
        }
        else
        {
            LogParamError("Unregister", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnablePush(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(int)))
        {
            bool enable = LuaScriptMgr.GetBoolean(L, 1);
            int type = (int)LuaScriptMgr.GetNumber(L, 2);
            PlatformControl.EnablePush(enable, type);
        }
        else if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool), typeof(int), typeof(LuaFunction)))
        {
            bool enable = LuaScriptMgr.GetBoolean(L, 1);
            int type = (int)LuaScriptMgr.GetNumber(L, 2);
            var callback = LuaScriptMgr.GetLuaFunction(L, 3);
            PlatformControl.EnablePush(enable, type, callback);
        }
        else
        {
            LogParamError("EnablePush", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetPushStatus(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int type = (int)LuaScriptMgr.GetNumber(L, 1);
            bool enable = PlatformControl.GetPushStatus(type);
            LuaScriptMgr.Push(L, enable);
        }
        else
        {
            LogParamError("GetPushStatus", count);
            LuaScriptMgr.Push(L, false);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowCustomerCenter(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.ShowCustomerCenter();
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.ShowCustomerCenter(callback);
        }
        else
        {
            LogParamError("ShowCustomerCenter", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowAnnouncement(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.ShowAnnouncement();
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.ShowAnnouncement(callback);
        }
        else
        {
            LogParamError("ShowAnnouncement", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowInAppWeb(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string url = LuaScriptMgr.GetString(L, 1);
            PlatformControl.ShowInAppWeb(url);
        }
        else if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            string url = LuaScriptMgr.GetString(L, 1);
            var callback = LuaScriptMgr.GetLuaFunction(L, 2);
            PlatformControl.ShowInAppWeb(url, callback);
        }
        else
        {
            LogParamError("ShowInAppWeb", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowPromotion(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.ShowPromotion();
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.ShowPromotion(callback);
        }
        else
        {
            LogParamError("ShowPromotion", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowGachaOdds(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.ShowGachaOdds();
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.ShowGachaOdds(callback);
        }
        else
        {
            LogParamError("ShowGachaOdds", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowCoupon(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.ShowCoupon();
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.ShowCoupon(callback);
        }
        else
        {
            LogParamError("ShowCoupon", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCustomData(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string key = LuaScriptMgr.GetString(L, 1);
            string data = PlatformControl.GetCustomData(key);
            LuaScriptMgr.Push(L, data);
        }
        else
        {
            LogParamError("GetCustomData", count);
            LuaScriptMgr.Push(L, string.Empty);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsGoogleGameLogined(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            bool isGoogleLogined = PlatformControl.IsGoogleGameLogined();
            LuaScriptMgr.Push(L, isGoogleLogined);
        }
        else
        {
            LogParamError("IsGoogleGameLogined", count);
            LuaScriptMgr.Push(L, false);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GoogleGameLogin(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.GoogleGameLogin(callback);
        }
        else
        {
            LogParamError("GoogleGameLogin", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GoogleGameLogout(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            PlatformControl.GoogleGameLogout(callback);
        }
        else
        {
            LogParamError("GoogleGameLogout", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowGoogleAchievementView(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.ShowGoogleAchievementView();
        }
        else
        {
            LogParamError("ShowGoogleAchievementView", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CompleteGoogleAchievement(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string id = LuaScriptMgr.GetString(L, 1);
            PlatformControl.CompleteGoogleAchievement(id);
        }
        else
        {
            LogParamError("CompleteGoogleAchievement", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetGoogleAchievementCompletionLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(int)))
        {
            string id = LuaScriptMgr.GetString(L, 1);
            int level = (int)LuaScriptMgr.GetNumber(L, 2);
            PlatformControl.SetGoogleAchievementCompletionLevel(id, level);
        }
        else
        {
            LogParamError("SetGoogleAchievementCompletionLevel", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetLoginJson(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            LuaScriptMgr.Push(L, PlatformControl.GetLoginJson());
        }
        else
        {
            LuaScriptMgr.Push(L, string.Empty);
            LogParamError("GetLoginJson", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int UploadRoleInfo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 10 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(double), typeof(double)))
        {
            int sendType = (int)LuaScriptMgr.GetNumber(L, 1);
            int playerId = (int)LuaScriptMgr.GetNumber(L, 2);
            string roleName = LuaScriptMgr.GetString(L, 3);
            int roleLevel = (int)LuaScriptMgr.GetNumber(L, 4);
            int vipLevel = (int)LuaScriptMgr.GetNumber(L, 5);
            int serverId = (int)LuaScriptMgr.GetNumber(L, 6);
            string serverName = LuaScriptMgr.GetString(L, 7);
            string laborUnion = LuaScriptMgr.GetString(L, 8);
            double roleCreateTime = (double)LuaScriptMgr.GetNumber(L, 9);
            double roleLevelMTime = (double)LuaScriptMgr.GetNumber(L, 10);

            PlatformControl.UploadRoleInfo(sendType, playerId, roleName, roleLevel, vipLevel, serverId, serverName, laborUnion, roleCreateTime, roleLevelMTime);
        }
        else
        {
            LogParamError("UploadRoleInfo", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetBreakPoint(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int type = (int)LuaScriptMgr.GetNumber(L, 1);
            PlatformControl.SetBreakPoint(type);
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string strPoint = LuaScriptMgr.GetString(L, 1);
            PlatformControl.SetBreakPoint(strPoint);
        }
        else
        {
            LogParamError("SetBreakPoint", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsPlatformExitGame(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            LuaScriptMgr.Push(L, PlatformControl.IsPlatformExitGame());
        }
        else
        {
            LogParamError("IsPlatformExitGame", count);
            LuaScriptMgr.Push(L, false);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ExitGame(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            PlatformControl.ExitGame();
        }
        else
        {
            LogParamError("ExitGame", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    
    /*****************************************
     *              IAP Function
     *****************************************/
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int InitializeIAP(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            int roleId = (int)LuaScriptMgr.GetNumber(L, 1);
            LTPlatformBase.ShareInstance().InitializeIAP(roleId);
        }
        else
        {
            LogParamError("InitIAP", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int DoPurchase(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(string), typeof(string)))
        {
            int purchaseType = (int)LuaScriptMgr.GetNumber(L, 1);
            string orderId = LuaScriptMgr.GetString(L, 2);
            string productId = LuaScriptMgr.GetString(L, 3);

            // 开启支付 sdk
            var productInfo = new SDK.BUY_INFO();
            productInfo.iBillingType = purchaseType;
            productInfo.strOrderId = orderId;
            productInfo.strProductId = productId;
            LTPlatformBase.ShareInstance().Buy(productInfo);
        }
        else if (count == 4 && LuaScriptMgr.CheckTypes(L, 1, typeof(float), typeof(string), typeof(string), typeof(string)))
        {
            int purchaseType = (int)LuaScriptMgr.GetNumber(L, 1);
            string orderId = LuaScriptMgr.GetString(L, 2);
            string productId = LuaScriptMgr.GetString(L, 3);
            string transactionId = LuaScriptMgr.GetString(L, 4);

            // 开启支付 sdk
            var productInfo = new SDK.BUY_INFO();
            productInfo.iBillingType = purchaseType;
            productInfo.strOrderId = orderId;
            productInfo.strProductId = productId;
            productInfo.strTransactionId = transactionId;
            LTPlatformBase.ShareInstance().Buy(productInfo);
        }
        else
        {
            LogParamError("DoPurchase", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CleanPurchaseInfo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            //int purchaseType = (int)LuaScriptMgr.GetNumber(L, 1);
            // 删除本地数据
            //FileBilling.Instance.Remove(purchaseType);
            //FileBilling.Instance.WriteFile();
        }
        else
        {
            LogParamError("CleanPurchaseInfo", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ProcessPurchaseCache(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
#if PLATFORM_KAKAO
            Debug.Log("ProcessPurchaseCache=======================");
            IAPManager.Instance.ProcessPurchaseCache();
#endif
        }
        else
        {
            LogParamError("ProcessPurchaseCache", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetProductIds(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable)))
        {
#if PLATFORM_KAKAO
            Debug.Log("SetProductIds=======================");
            string[] arrayPids = LuaScriptMgr.GetArrayString(L, 1);
            List<string> listPid = new List<string>(arrayPids);
            IAPManager.Instance.SetProductIds(listPid);
#endif
        }
        else
        {
            LogParamError("SetProductIds", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int DoFillBillTest(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            Debug.Log("Read DoFillBillTest=======================");
            FileBilling.Instance.ReadFile();
            var entry = FileBilling.Instance.Get(1);
            if (entry != null)
                Debug.Log( string.Format("Read Bill : {0}", entry.Receipt));
        }
        else if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string strReceipt = LuaScriptMgr.GetString(L, 1);

            FileBilling.Instance.ReadFile();
            FileBilling.CReceiptInfo entry = new FileBilling.CReceiptInfo();
            entry.BillingType = 1;
            entry.Receipt = strReceipt;
            FileBilling.Instance.Update(1, entry);
            FileBilling.Instance.WriteFile();

            Debug.Log("Write DoFillBillTest======================");
        }
        else
        {
            LogParamError("DoFillBillTest", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
}
