using LuaInterface;

namespace SDK
{
    public class PlatformControl
    {
        private static LTPlatformBase LT_Instance
        {
            get { return LTPlatformBase.ShareInstance(); }
        }
        private static bool IsUsePlatform
        {
            get
            {
                return LT_Instance != null;
            }
        }

        public static bool IsInited
        {
            get
            {
                if (!IsUsePlatform) return false;

                return LT_Instance.IsInited;
            }
        }

        public static bool IsLogined
        {
            get
            {
                if (!IsUsePlatform) return false;

                return LT_Instance.IsLogined;
            }
        }

        public static void Init(LuaFunction callback = null)
        {
            LT_RESULT_NOTIFICATION_DELEGATE fnResult = null;
            if (callback != null)
            {
                fnResult = (isSuccessful) =>
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                };
            }
            LT_Instance.Init(fnResult);
        }

        public static void RegisterCallback(LuaFunction fnLoginCallback,
            LuaFunction fnLogoutCallback,
            LuaFunction fnBuyCallback)
        {
            LT_LOGIN_NOTIFICATION_DELEGATE fnLogin = null;
            LT_LOGOUT_NOTIFICATION_DELEGATE fnLogout = null;
            LT_BUY_NOTIFICATION_DELEGATE fnBuy = null;

            if (fnLoginCallback != null)
            {
                fnLogin = (iState, pUser) => {
                    int eLoginState = (int)iState;
                    string strUserName = pUser.strUserName;
                    string strAccessToken = pUser.strAccessToken;
                    string strUserID = pUser.strUserID;
                    bool bGuest = pUser.bGuest;

                    fnLoginCallback.Call(eLoginState,
                                         strUserName,
                                         strAccessToken,
                                         strUserID,
                                         bGuest);
                };
            }
            
            if (fnLogoutCallback != null)
            {
                fnLogout = (iState) =>
                {
                    bool bSuccess = (iState == SDK.LOGOUT_STATE.LT_LOGOUT_SUCCEED);

                    fnLogoutCallback.Call(bSuccess);
                };
            }

            if (fnBuyCallback != null)
            {
                fnBuy = (iState, pProduct) =>
                {
                    bool bSuccess = (iState == SDK.BUY_STATE.LT_BUY_SUCCEED);
                    int iBillingType = pProduct.iBillingType;
                    string strOrderId = pProduct.strOrderId;
                    string strTransactionId = pProduct.strTransactionId;
                    string strReceipt = pProduct.strReceipt;

                    fnBuyCallback.Call(bSuccess,
                                       iBillingType,
                                       strOrderId,
                                       strTransactionId,
                                       strReceipt);
                };
            }

            LT_Instance.RegisterCallback(fnLogin, fnLogout, fnBuy);
        }

        public static PLATFORM_TYPE GetPlatformType()
        {
            return LT_Instance.GetPlatformType();
        }

        public static void Login()
        {
            LT_Instance.Login();
        }

        public static void Login(int type)
        {
            LT_Instance.Login(type);
        }

        public static void CheckLoginStatus()
        {
            LT_Instance.CheckLoginStatus();
        }

        public static void CheckAutoLogin()
        {
            LT_Instance.CheckAutoLogin();
        }

        public static void Logout()
        {
            LT_Instance.Logout();
        }

        public static void LogoutDirectly()
        {
            LT_Instance.LogoutDirectly();
        }

        public static void AccountConversion(LuaFunction callback = null)
        {
            LT_RESULT_NOTIFICATION_DELEGATE fnResult = null;
            if (callback != null)
            {
                fnResult = (isSuccessful) =>
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                };
            }
            LT_Instance.AccountConversion(fnResult);
        }

        public static void Unregister(LuaFunction callback = null)
        {
            LT_RESULT_NOTIFICATION_DELEGATE fnResult = null;
            if (callback != null)
            {
                fnResult = (isSuccessful) =>
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                };
            }
            LT_Instance.Unregister(fnResult);
        }

        public static void KakaoAndroidPermissionManage(System.Action callback)
        {
#if PLATFORM_KAKAO
            var lt_kakao = LT_Instance as LTKakao;
            lt_kakao.ManagePermissions(() =>
            {
                callback();
            });
#else
            callback();
#endif
        }

        public static void EnablePush(bool enable, int type, LuaFunction callback = null)
        {
            LT_RESULT_NOTIFICATION_DELEGATE fnResult = null;
            if (callback != null)
            {
                fnResult = (isSuccessful) =>
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                };
            }
            LT_Instance.EnablePush(enable, type, fnResult);
        }

        public static bool GetPushStatus(int type)
        {
            return LT_Instance.GetPushStatus(type);
        }

        public static void ShowCustomerCenter(LuaFunction callback = null)
        {
            LT_URL_NOTIFICATION_DELEGATE fnUrl = null;
            if (callback != null)
            {
                fnUrl = (url) =>
                {
                    callback.Call(url);
                    callback.Release();
                };
            }
            LT_Instance.ShowCustomerCenter(fnUrl);
        }

        public static void ShowAnnouncement(LuaFunction callback = null)
        {
            LT_URL_NOTIFICATION_DELEGATE fnUrl = null;
            if (callback != null)
            {
                fnUrl = (url) =>
                {
                    callback.Call(url);
                    callback.Release();
                };
            }
            LT_Instance.ShowAnnouncement(fnUrl);
        }
        
        public static void ShowInAppWeb(string url, LuaFunction callback = null)
        {
            LT_URL_NOTIFICATION_DELEGATE fnUrl = null;
            if (callback != null)
            {
                fnUrl = (deepLinkUrl) =>
                {
                    callback.Call(deepLinkUrl);
                    callback.Release();
                };
            }
            LT_Instance.ShowInAppWeb(url, fnUrl);
        }

        public static void ShowPromotion(LuaFunction callback = null)
        {
            LT_URL_NOTIFICATION_DELEGATE fnUrl = null;
            if (callback != null)
            {
                fnUrl = (url) =>
                {
                    callback.Call(url);
                    callback.Release();
                };
            }
            LT_Instance.ShowPromotion(fnUrl);
        }

        public static void ShowGachaOdds(LuaFunction callback = null)
        {
            LT_URL_NOTIFICATION_DELEGATE fnUrl = null;
            if (callback != null)
            {
                fnUrl = (url) =>
                {
                    callback.Call(url);
                    callback.Release();
                };
            }
            LT_Instance.ShowGachaOdds(fnUrl);
        }

        public static void ShowCoupon(LuaFunction callback = null)
        {
            LT_RESULT_NOTIFICATION_DELEGATE fnResult = null;
            if (callback != null)
            {
                fnResult = (isSuccessful) =>
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                };
            }
            LT_Instance.ShowCoupon(fnResult);
        }

        public static string GetCustomData(string key)
        {
            return LTPlatformBase.ShareInstance().GetCustomData(key);
        }

        public static void OnApplicationStateChange(bool pauseStatus)
        {
            if (!IsUsePlatform) return;

            if (pauseStatus)
                LT_Instance.Pause();
            else
                LT_Instance.Resume();
        }

        public static bool IsGoogleGameLogined()
        {
            return LT_Instance.IsGoogleGameLogined;
        }

        public static void GoogleGameLogin(LuaFunction callback = null)
        {
            LT_RESULT_NOTIFICATION_DELEGATE fnResult = null;
            if (callback != null)
            {
                fnResult = (isSuccessful) =>
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                };
            }
            LT_Instance.GoogleGameLogin(fnResult);
        }

        public static void GoogleGameLogout(LuaFunction callback = null)
        {
            LT_RESULT_NOTIFICATION_DELEGATE fnResult = null;
            if (callback != null)
            {
                fnResult = (isSuccessful) =>
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                };
            }
            LT_Instance.GoogleGameLogout(fnResult);
        }

        public static void ShowGoogleAchievementView()
        {
            LT_Instance.ShowGoogleAchievementView();
        }

        public static void CompleteGoogleAchievement(string id)
        {
            LT_Instance.CompleteGoogleAchievement(id);
        }

        public static void SetGoogleAchievementCompletionLevel(string id, int level)
        {
            LT_Instance.SetGoogleAchievementCompletionLevel(id, level);
        }

        public static string GetLoginJson()
        {
            return LT_Instance.GetLoginJson();
        }

        public static void UploadRoleInfo(int sendType, int playerId, string roleName, int roleLevel, int vipLevel, int serverId, string serverName, string laborUnion, double roleCreateTime, double roleLevelMTime)
        {
            var roleInfo = new ROLE_INFO(sendType, playerId, roleName, roleLevel, vipLevel, serverId, serverName, laborUnion, roleCreateTime, roleLevelMTime);
            LT_Instance.UploadRoleInfo(roleInfo);
        }

        public static void SetBreakPoint(int type)
        {
            var state = (POINT_STATE)type;
            LT_Instance.SetBreakPoint(state);
        }

        public static void SetBreakPoint(string strPoint)
        {
            LT_Instance.SetBreakPoint(strPoint);
        }

        public static bool IsPlatformExitGame()
        {
            return LT_Instance.IsPlatformExitGame();
        }

        public static void ExitGame()
        {
            LT_Instance.Exit();
        }

        public static void OnPlatformResume(bool isSuccessful)
        {
        }

        public static void OnPlatformSwitchAccount(bool isSuccessful)
        {
        }
    }
}
