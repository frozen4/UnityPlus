#if PLATFORM_LONGTU
using SDK;
using UnityEngine;
using System;
using Common;

public sealed class LTLongtu : LTPlatformBase
{
    [Serializable]
    private class LTCallbackResult
    {
        public int code;
        public string message;
    }

    [Serializable]
    private class LTLoginResult
    {
        public int code;
        public string message;
        public LTLoginInfo loginInfo;
    }

    [Serializable]
    private class LTLoginInfo
    {
        public string uid;
        public string token;
        public string gameid;
        public string channelid;
    }

    private class LTResultCode
    {
        public const int InitSucceed = 1;
        public const int InitFailed = 2;
        public const int LoginSucceed = 4;
        public const int LoginFailed = 5;
        public const int LoginCancel = 6;
        public const int LoginTimeout = 7;
        public const int PaySucceed = 11;
        public const int PayFailed = 12;
        public const int PayCancel = 13;
        public const int LogoutSucceed = 9;
        public const int LogoutFailed = 10;
        public const int ExitSucceed = 14;
        public const int ExitFailed = 15;
    }

    private LT_LOGIN_NOTIFICATION_DELEGATE _LoginCallback = null;
    private LT_LOGOUT_NOTIFICATION_DELEGATE _LogoutCallback = null;
    private LT_BUY_NOTIFICATION_DELEGATE _BuyCallback = null;
    private LTLoginInfo _LoginInfo = null;

    private readonly string _LongtuPrefix = "[LongTu]";
    private readonly string _ClassName = "com.meteoritestudio.longtu.LongtuSDK";

    public override PLATFORM_TYPE GetPlatformType()
    {
#if UNITY_IOS || UNITY_IPOINE
        return PLATFORM_TYPE.PLATFORM_LONGTU_IOS;
#elif UNITY_ANDROID
        return PLATFORM_TYPE.PLATFORM_LONGTU_ANDROID;
#else
        return PLATFORM_TYPE.PLATFORM_ANY;
#endif
    }

    public override void Login()
    {
        if (!_IsLogined)
        {
            HobaDebuger.LogWarningFormat("{0}Login Start", _LongtuPrefix);
            AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
            LongtuSDK.CallStatic("PlatformLogin");
        }
    }

    public override void LoginEx()
    {

    }

    public override void Logout(bool bCleanAutoLogin = false)
    {
        if (_IsLogined)
        {
            HobaDebuger.LogWarningFormat("{0}Logout Start", _LongtuPrefix);
            AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
            LongtuSDK.CallStatic("PlatformLogout");
        }
    }

    public override void LogoutDirectly()
    {
        if (_IsLogined)
        {
            HobaDebuger.LogWarningFormat("{0}LogoutDirectly Start", _LongtuPrefix);
            AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
            LongtuSDK.CallStatic("PlatformLogout");
        }
    }

    public override void Unregister(LT_RESULT_NOTIFICATION_DELEGATE callback)
    {
        if (_IsLogined)
        {
        }
    }

    public override void Pause()
    {
        if (_IsInited)
        {
        }
    }

    public override void Resume()
    {
        if (_IsInited)
        {
        }
    }

    public override void Exit()
    {
        HobaDebuger.LogWarningFormat("{0}Exit Start", _LongtuPrefix);
        AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
        LongtuSDK.CallStatic("PlatformExitGame");
    }

    public override void SwitchAccount()
    {
    }

    public override void Buy(BUY_INFO pProduct)
    {

    }

    public override void ManageAccount()
    {
    }

    public override void InitSDK(LT_INITED_NOTIFICATION_DELEGATE fnInit)
    {
        Reset();
        _IsInited = true;
        //EntryPoint.Instance.gameObject.AddComponent<Test.TestLongtu>();
        if (fnInit != null)
            fnInit(INITED_STATE.LT_INITED_SUCCEED, 99);
    }


    public override void Init(LT_RESULT_NOTIFICATION_DELEGATE fnResult)
    {
        if (_IsInited)
        {
            if (fnResult != null)
                fnResult(_IsInited);
        }
    }

    public override void RegisterCallback(LT_LOGIN_NOTIFICATION_DELEGATE fnLogin,
        LT_LOGOUT_NOTIFICATION_DELEGATE fnLogout,
        LT_BUY_NOTIFICATION_DELEGATE fnBuy)
    {
        _LoginCallback = fnLogin;
        _LogoutCallback = fnLogout;
        _BuyCallback = fnBuy;
    }

    public override void DoLoginCallBack(string param)
    {
        HobaDebuger.LogWarningFormat("{0}DoLoginCallBack param: {1}", _LongtuPrefix, param);
        var result = JsonUtility.FromJson<LTLoginResult>(param);
        if (result != null)
        {
            if (result.code == LTResultCode.LoginSucceed)
            {
                OnLoginSucceed(result.loginInfo);
            }
            else
            {
                LOGIN_STATE state = LOGIN_STATE.LT_LOGIN_UNKOWN_ERROR;

                if (result.code == LTResultCode.LoginTimeout)
                    state = LOGIN_STATE.LT_LOGIN_TIME_OUT;
                else if (result.code == LTResultCode.LoginCancel)
                    state = LOGIN_STATE.LT_LOGIN_USER_CANCEL;

                if (_LoginCallback != null)
                    _LoginCallback(state, new USER_INFO());
            }
        }
    }

    public override void DoLogoutCallBack(string param)
    {
        HobaDebuger.LogWarningFormat("{0}DoLogoutCallBack param: {1}", _LongtuPrefix, param);
        var result = JsonUtility.FromJson<LTCallbackResult>(param);
        if (result != null)
        {
            if (result.code == LTResultCode.LogoutSucceed)
            {
                _IsLogined = false;
                if (_LogoutCallback != null)
                    _LogoutCallback(LOGOUT_STATE.LT_LOGOUT_SUCCEED);
            }
            else if(result.code == LTResultCode.LogoutFailed)
            {
                if (_LogoutCallback != null)
                    _LogoutCallback(LOGOUT_STATE.LT_LOGOUT_FAIL);
            }
        }
    }

    public override void DoExitCallBack(string param)
    {
        var result = JsonUtility.FromJson<LTCallbackResult>(param);
        if (result != null)
        {
            if (result.code == LTResultCode.ExitSucceed)
            {
                // TODO
            }
            else if (result.code == LTResultCode.ExitFailed)
            {
                // TODO
            }
        }
    }

    public override void UploadRoleInfo(ROLE_INFO roleInfo)
    {
        if (_IsLogined && roleInfo != null)
        {
            string roleInfoJson = JsonUtility.ToJson(roleInfo);
            HobaDebuger.LogWarningFormat("{0}UploadRoleInfo Start, RoleInfo Json:{1}", _LongtuPrefix, roleInfoJson);
            AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
            LongtuSDK.CallStatic("UploadRoleInfo", roleInfoJson);
        }
    }

    public override bool IsPlatformExitGame()
    {
        AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
        bool isExit = LongtuSDK.CallStatic<bool>("IsPlatformExitGame");
        HobaDebuger.LogWarningFormat("{0}IsPlatformExitGame isExit: {1}", _LongtuPrefix, isExit.ToString());
        return isExit;
    }

    public override string GetLoginJson()
    {
        if (_IsLogined && _LoginInfo != null)
        {
            return JsonUtility.ToJson(_LoginInfo);
        }
        return string.Empty;
    }

    public override void ShowAnnouncement(LT_URL_NOTIFICATION_DELEGATE callback)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActitivy = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
        LongtuSDK.CallStatic("ShowAnnouncement", currentActitivy);
    }

    public override void SetBreakPoint(POINT_STATE iState)
    {
        AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
        LongtuSDK.CallStatic("SetDataBreakPoint", iState.ToString());
    }

    public override void SetBreakPoint(string strPoint)
    {
        AndroidJavaClass LongtuSDK = new AndroidJavaClass(_ClassName);
        LongtuSDK.CallStatic("SetDataBreakPoint", strPoint);
    }

    private void OnLoginSucceed(LTLoginInfo loginInfo)
    {
        if (loginInfo == null) return;

        USER_INFO userInfo = new USER_INFO();
        userInfo.strUserName = string.Empty;
        userInfo.strNickName = string.Empty;
        userInfo.strUserID = loginInfo.uid;
        userInfo.strAccessToken = loginInfo.token;
        userInfo.strGameID = loginInfo.gameid;
        userInfo.strChannelID = loginInfo.channelid;
        userInfo.bGuest = false;

        _LoginInfo = loginInfo;
        _IsLogined = true;
        if (_LoginCallback != null)
            _LoginCallback(LOGIN_STATE.LT_LOGIN_SUCCEED, userInfo);
    }

    private void Reset()
    {
        _IsLogined = false;
        _LoginInfo = null;
    }

    private void LogResult(string prefix, string name, string code)
    {
        HobaDebuger.LogWarningFormat("{0}{1}, result code:{2}", prefix, name, code);
    }
}
#endif