using SDK;
using System.Collections.Generic;
using System;
using Common;

namespace SDK
{
    public class USER_INFO
    {
        public string strUserName = string.Empty;
        public string strNickName = string.Empty;
        public string strAccessToken = string.Empty;
        public string strUserID = string.Empty;
        public string strGameID = string.Empty;
        public string strChannelID = string.Empty;
        public bool bGuest;
    };

    public class BUY_INFO
    {
        public int iBillingType;
        public string strOrderId = string.Empty;
        public string strProductId = string.Empty;
        public string strTransactionId = string.Empty;
        public string strReceipt = string.Empty;
    }

    [Serializable]
    public class ROLE_INFO
    {
        public int sendType;
        public int playerId;
        public string roleName;
        public int roleLevel;
        public int vipLevel;
        public int serverId;             // 服务器id
        public string serverName;           // 服务器名称
        public string laborUnion;           // 游戏公会
        public double roleCreateTime;       // 角色创建时间（服务器采集）参数类型：String时间类型：long   精度：秒  长度：10（必传）
        public double roleLevelMTime;       // 角色等级变化时间（服务器采集）参数类型：String 时间类型：long 精度：秒  长度：10（必传）

        public ROLE_INFO() { }

        public ROLE_INFO(int sendType, int playerId, string roleName, int roleLevel, int vipLevel, int serverId, string serverName, string laborUnion, double roleCreateTime, double roleLevelMTime)
        {
            this.sendType = sendType;
            this.playerId = playerId;
            this.roleName = roleName;
            this.roleLevel = roleLevel;
            this.vipLevel = vipLevel;
            this.serverId = serverId;
            this.serverName = serverName;
            this.laborUnion = laborUnion;
            this.roleCreateTime = roleCreateTime;
            this.roleLevelMTime = roleLevelMTime;
        }
    }

    // 渠道平台类型
    public enum PLATFORM_TYPE
    {
        PLATFORM_ANY = 0,

        PLATFORM_KAKAO_IOS = 1,    // kakao, ios
        PLATFORM_KAKAO_ANDROID = 2,    // kakao, android

        PLATFORM_TENCENT_IOS = 11,   // 腾讯, ios
        PLATFORM_TENCENT_ANDROID = 12,   // 腾讯, android

        PLATFORM_LONGTU_IOS = 21,   // 龙图，ios
        PLATFORM_LONGTU_ANDROID = 22,   // 龙图，android
    };

    // 初始化状态
    public enum INITED_STATE
    {
        LT_INITED_SUCCEED = 0,
        LT_INITED_FAIL,
    };

    // 登录状态
    public enum LOGIN_STATE
    {
        LT_LOGIN_SUCCEED = 0,
        LT_LOGIN_USER_CANCEL,
        LT_LOGIN_APP_KEY_INVALID,
        LT_LOGIN_APP_ID_INVALID,
        LT_LOGIN_HAS_ASSOCIATE,
        LT_LOGIN_TIME_OUT,
        LT_LOGIN_UNKOWN_ERROR = 99,
    };

    // 登出状态
    public enum LOGOUT_STATE
    {
        LT_LOGOUT_SUCCEED = 0,
        LT_LOGOUT_FAIL,
    };

    // 支付状态
    public enum BUY_STATE
    {
        LT_BUY_SUCCEED = 0,
        LT_BUY_USER_CANCEL,
        LT_BUY_NETWORK_FAILED,
        LT_BUY_PAY_FAILED,
        LT_BUY_UNKOWN_ERROR = 99,
    };

    // 打点节点
    public enum POINT_STATE
    {
        Game_Start = 0,                 // 启动游戏
        Game_Check_Update,              // 检查更新
        Game_Check_Update_Fail,         // 检查更新失败
        Game_Start_Update,              // 开始更新
        Game_End_Update,                // 结束更新
        Game_Get_Announcement,          // 获取公告
        Game_Get_Server_List,           // 获取服务器列表
        Game_Select_Server,             // 选择服务器
        Game_User_Login,                // 账号登录
        Game_User_Login_Fail,           // 账号登录失败
        Game_Create_Role,               // 角色创建
        Game_Create_Role_Fail,          // 角色创建失败
        Game_Role_Login,                // 角色登录
        Game_Role_Login_Fail,           // 角色登录失败
    };

    public delegate void LT_RESULT_NOTIFICATION_DELEGATE(bool isSuccessful);

    public delegate void LT_INITED_NOTIFICATION_DELEGATE(INITED_STATE iState, int resultCode);

    public delegate void LT_LOGIN_NOTIFICATION_DELEGATE(LOGIN_STATE iState, SDK.USER_INFO pUser);

    public delegate void LT_LOGOUT_NOTIFICATION_DELEGATE(LOGOUT_STATE iState);

    public delegate void LT_BUY_NOTIFICATION_DELEGATE(BUY_STATE iState, SDK.BUY_INFO pProduct);

    public delegate void LT_URL_NOTIFICATION_DELEGATE(string url);
}

public abstract class LTPlatformBase
{
    protected Dictionary<PLATFORM_TYPE, string> _mapAppKey = new Dictionary<PLATFORM_TYPE, string>();   // AppKey
    protected Dictionary<PLATFORM_TYPE, int> _mapAppId = new Dictionary<PLATFORM_TYPE, int>();          // AppId

    protected bool _IsInited = false;
    public bool IsInited
    {
        get { return _IsInited; }
    }

    protected bool _IsLogined = false;
    public bool IsLogined
    {
        get { return _IsLogined; }
    }

    protected LTPlatformBase()
    {
        _mapAppKey.Add(PLATFORM_TYPE.PLATFORM_KAKAO_IOS, "");
        _mapAppId.Add(PLATFORM_TYPE.PLATFORM_KAKAO_IOS, 100001);
        _mapAppKey.Add(PLATFORM_TYPE.PLATFORM_KAKAO_ANDROID, "");
        _mapAppId.Add(PLATFORM_TYPE.PLATFORM_KAKAO_ANDROID, 100002);
        _mapAppKey.Add(PLATFORM_TYPE.PLATFORM_LONGTU_IOS, "");
        _mapAppId.Add(PLATFORM_TYPE.PLATFORM_LONGTU_IOS, 100003);
        _mapAppKey.Add(PLATFORM_TYPE.PLATFORM_LONGTU_ANDROID, "");
        _mapAppId.Add(PLATFORM_TYPE.PLATFORM_LONGTU_ANDROID, 100004);
    }

    private static LTPlatformBase _instance = null;

    public static LTPlatformBase ShareInstance()
    {
        if (_instance == null)
        {
#if PLATFORM_KAKAO
            _instance = new LTKakao();
#elif PLATFORM_TENCENT
#elif PLATFORM_LONGTU
            _instance = new LTLongtu();
#else
            _instance = new LTPlatformWindows();
#endif
        }

        return _instance;
    }

    public virtual PLATFORM_TYPE GetPlatformType()
    {
        return PLATFORM_TYPE.PLATFORM_ANY;
    }

    public string GetPlatformAppKey()
    {
        string strRet = "";
        if (_mapAppKey.TryGetValue(GetPlatformType(), out strRet))
            return strRet;
        return string.Empty;
    }

    public int GetPlatformAppId()
    {
        int iRet = 0;
        if (_mapAppId.TryGetValue(GetPlatformType(), out iRet))
            return iRet;
        return 0;
    }

    public abstract void InitPurchaseVerifyUrl(string url);

    public abstract void ProcessPurchaseCache();

    public abstract void Login();

    public abstract void LoginEx();

    public abstract void Logout(bool bCleanAutoLogin = false);

    public abstract void Unregister(LT_RESULT_NOTIFICATION_DELEGATE callback);

    public abstract void Pause();

    public abstract void Resume();

    public abstract void Exit();

    public abstract void SwitchAccount();

    public abstract void ManageAccount();

    public abstract void Buy(SDK.BUY_INFO pProduct);

    public abstract void InitSDK(LT_INITED_NOTIFICATION_DELEGATE fnInit);

    public abstract void Init(LT_RESULT_NOTIFICATION_DELEGATE fnResult);
        
    public abstract void RegisterCallback(LT_LOGIN_NOTIFICATION_DELEGATE fnLogin,
        LT_LOGOUT_NOTIFICATION_DELEGATE fnLogout,
        LT_BUY_NOTIFICATION_DELEGATE fnBuy);

    public virtual void DoLoginCallBack(string param) { }

    public virtual void DoLogoutCallBack(string param) { }

    public virtual void DoExitCallBack(string param) { }

    public virtual void CheckLoginStatus() { }

    public virtual void CheckAutoLogin() { }

    public virtual void Login(int type) { }

    public virtual void LogoutDirectly() { }

    public virtual void EnablePush(bool enable, int type, LT_RESULT_NOTIFICATION_DELEGATE callback = null) { }

    public virtual bool GetPushStatus(int type)
    {
        return false;
    }

    //展示客户中心
    public virtual void ShowCustomerCenter(LT_URL_NOTIFICATION_DELEGATE callback) { }

    //展示公告
    public virtual void ShowAnnouncement(LT_URL_NOTIFICATION_DELEGATE callback) { }

    //展示网页
    public virtual void ShowInAppWeb(string url, LT_URL_NOTIFICATION_DELEGATE callback) { }

    //展示推广
    public virtual void ShowPromotion(LT_URL_NOTIFICATION_DELEGATE callback) { }

    //展示抽奖
    public virtual void ShowGachaOdds(LT_URL_NOTIFICATION_DELEGATE callback) { }

    //展示优惠券
    public virtual void ShowCoupon(LT_RESULT_NOTIFICATION_DELEGATE callback) { }

    //未完成订单 再次验单
    public virtual void ReProcessingOrder() { }

    //初始化IAP
    public virtual void InitializeIAP(int roleID = 0) { }

    //获取CDN地址
    public virtual string GetCDNAddress()
    {
        return string.Empty;
    }

    //获取GameServer地址
    public virtual string GetGameServerAddress()
    {
        return string.Empty;
    }

    //
    public virtual string GetDynamicServerAddress()
    {
        return string.Empty;
    }

    //获取自定义数据
    public virtual string GetCustomData(string key)
    {
        return string.Empty;
    }

    public virtual void AccountConversion(LT_RESULT_NOTIFICATION_DELEGATE callback) { }

    public virtual bool IsGoogleGameLogined { get { return false; } }

    public virtual void GoogleGameLogin(LT_RESULT_NOTIFICATION_DELEGATE callback) { }

    public virtual void GoogleGameLogout(LT_RESULT_NOTIFICATION_DELEGATE callback) { }

    public virtual void ShowGoogleAchievementView() { }

    public virtual void CompleteGoogleAchievement(string id) { }

    public virtual void SetGoogleAchievementCompletionLevel(string id, int level) { }

    public virtual void UploadRoleInfo(ROLE_INFO roleInfo) { }

    public virtual bool IsPlatformExitGame() { return false; }

    public virtual string GetLoginJson() { return string.Empty; }

    public virtual void SetBreakPoint(POINT_STATE iState) { }
    public virtual void SetBreakPoint(string strPoint) { }

    public virtual string GetErrStr(int errorCode) { return string.Empty; }
}
