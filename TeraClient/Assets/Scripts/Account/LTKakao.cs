#if PLATFORM_KAKAO
using SDK;
using SDK.Kakao;
using Kakaogame.SDK;
using UnityEngine;
using System.Collections.Generic;
using System;
using Common;

public class LTKakao : LTPlatformBase
{
    private LT_LOGIN_NOTIFICATION_DELEGATE _LoginCallback = null;
    private LT_LOGOUT_NOTIFICATION_DELEGATE _LogoutCallback = null;
    private LT_BUY_NOTIFICATION_DELEGATE _BuyCallback = null;

    private readonly string _KakaoPrefix = "[Kakao]";

    public override PLATFORM_TYPE GetPlatformType()
    {
#if UNITY_IOS || UNITY_IPOINE
        return PLATFORM_TYPE.PLATFORM_KAKAO_IOS;
#elif UNITY_ANDROID
        return PLATFORM_TYPE.PLATFORM_KAKAO_ANDROID;
#else
        return PLATFORM_TYPE.PLATFORM_ANY;
#endif
    }

    public override void Login()
    {
        if (!_IsLogined)
        {
            KakaoUtil.Login((resultCode) =>
            {
                if (resultCode == KGResultCode.Success)
                {
                    LogResult(_KakaoPrefix, "Login Succeeded", resultCode.ToString());
                    OnLoginSucceed();
                }
                else
                {
                    LogResult(_KakaoPrefix, "Login Failed", resultCode.ToString());
                    LOGIN_STATE state = LOGIN_STATE.LT_LOGIN_UNKOWN_ERROR;

                    if (resultCode == KGResultCode.AlreadyUsedIDPAccount)
                        state = LOGIN_STATE.LT_LOGIN_HAS_ASSOCIATE;
                    else if (resultCode == KGResultCode.UserCanceled)
                        state = LOGIN_STATE.LT_LOGIN_USER_CANCEL;

                    if (_LoginCallback != null)
                        _LoginCallback(state, new USER_INFO());
                }
            });
        }
    }

    public override void Login(int type)
    {
        if (!_IsLogined)
        {
            var idpCode = (KGIdpCode)type;
            KakaoUtil.LoginWithCustomUI(idpCode, (resultCode) =>
            {
                if (resultCode == KGResultCode.Success)
                {
                    LogResult(_KakaoPrefix, "Login with idpCode Succeeded", resultCode.ToString());
                    OnLoginSucceed();
                }
                else
                {
                    LogResult(_KakaoPrefix, "Login with idpCode Failed", resultCode.ToString());
                    LOGIN_STATE state = LOGIN_STATE.LT_LOGIN_UNKOWN_ERROR;

                    if (resultCode == KGResultCode.AlreadyUsedIDPAccount)
                        state = LOGIN_STATE.LT_LOGIN_HAS_ASSOCIATE;
                    else if (resultCode == KGResultCode.UserCanceled)
                        state = LOGIN_STATE.LT_LOGIN_USER_CANCEL;

                    if (_LoginCallback != null)
                        _LoginCallback(state, new USER_INFO());
                }
            });
        }
    }

    public override void CheckLoginStatus()
    {
        bool oldStatus = _IsLogined;
        bool newStatus = KakaoUtil.IsLogined;
        if (!oldStatus && newStatus)
        {
            //旧状态为未登录，新状态为已登录，属于异常
            Common.HobaDebuger.LogErrorFormat("{0}CheckLoginStatus invalid", _KakaoPrefix);
            return;
        }

        _IsLogined = newStatus;
        if (oldStatus && !newStatus)
            SingularSDK.UnsetCustomUserId();
    }

    public override void CheckAutoLogin()
    {
        if (_IsLogined)
        {
            OnLoginSucceed();
        }
    }

    public override void LoginEx()
    {
    }

    public override void Logout(bool bCleanAutoLogin = false)
    {
        if (_IsLogined)
        {
            KakaoUtil.Logout((resultCode) =>
            {
                if (resultCode == KGResultCode.Success)
                {
                    LogResult(_KakaoPrefix, "Logout Succeeded", resultCode.ToString());
                    SingularSDK.UnsetCustomUserId();
                    Reset();
                    if (_LogoutCallback != null)
                        _LogoutCallback(LOGOUT_STATE.LT_LOGOUT_SUCCEED);
                }
                else
                {
                    LogResult(_KakaoPrefix, "Logout Failed", resultCode.ToString());
                    if (_LogoutCallback != null)
                        _LogoutCallback(LOGOUT_STATE.LT_LOGOUT_FAIL);
                }
            });
        }
    }

    public override void LogoutDirectly()
    {
        if (_IsLogined)
        {
            KakaoUtil.LogoutWithCustomUI((resultCode) =>
            {
                if (resultCode == KGResultCode.Success)
                {
                    LogResult(_KakaoPrefix, "LogoutDirectly Succeeded", resultCode.ToString());
                    SingularSDK.UnsetCustomUserId();
                    Reset();
                    if (_LogoutCallback != null)
                        _LogoutCallback(LOGOUT_STATE.LT_LOGOUT_SUCCEED);
                }
                else
                {
                    LogResult(_KakaoPrefix, "LogoutDirectly Failed", resultCode.ToString());
                    if (_LogoutCallback != null)
                        _LogoutCallback(LOGOUT_STATE.LT_LOGOUT_FAIL);
                }
            });
        }
    }

    public override void Unregister(LT_RESULT_NOTIFICATION_DELEGATE callback)
    {
        if (_IsLogined)
        {
            KakaoUtil.Unregister((resultCode) =>
            {
                bool isSuccessful = resultCode == KGResultCode.Success;
                if (isSuccessful)
                {
                    LogResult(_KakaoPrefix, "Unregister Succeeded", resultCode.ToString());
                    _IsLogined = KakaoUtil.IsLogined; // 同步登录状态（注销成功，登录状态为false）
                    if (!_IsLogined)
                        SingularSDK.UnsetCustomUserId();
                }
                else
                    LogResult(_KakaoPrefix, "Unregister Failed", resultCode.ToString());

                if (callback != null)
                    callback(isSuccessful);
            });
        }
    }

    public override void Pause()
    {
        if (_IsInited)
        {
            KakaoUtil.Pause();
        }
    }

    private int _ResumeCount = 0;
    private const int MAX_RESUME_TIME = 3;
    public override void Resume()
    {
        if (_IsInited)
        {
            if (Util.GetNetworkStatus() == NetworkReachability.NotReachable) return;
            
            KakaoUtil.Resume((resultCode) =>
            {
                if (resultCode == KGResultCode.Success)
                    LogResult(_KakaoPrefix, "Resume Success", resultCode.ToString());
                else
                {
                    if (resultCode == KGResultCode.AuthFailure || resultCode == KGResultCode.IdpAuthFailure)
                    {
                        LogResult(_KakaoPrefix, "Resume Auth Failed, Do LogoutDirectly", resultCode.ToString());
                        _ResumeCount = 0;
                        LogoutDirectly();
                    }
                    else
                    {
                        LogResult(_KakaoPrefix, string.Format("Resume Failed, CurCount:{0}", _ResumeCount.ToString()), resultCode.ToString());
                        //OSUtility.ShowAlertView("Resume Failed", HobaText.Format("result code:{0}, do logout:{1}", resultCode.ToString(), _ResumeCount >= MAX_RESUME_TIME));
                        if (_ResumeCount < MAX_RESUME_TIME)
                        {
                            this.Resume();
                            _ResumeCount++;
                        }
                        else
                        {
                            LogResult(_KakaoPrefix, HobaText.Format("Resume Retry Over MaxTime:{0}, Do LogoutDirectly", MAX_RESUME_TIME), resultCode.ToString());
                            _ResumeCount = 0;
                            LogoutDirectly();
                        }
                    }
                }
                // PlatformControl.OnPlatformResume(isSuccess);
            });
        }
    }

    public override void Exit()
    {
    }

    public override void SwitchAccount()
    {
    }

    public override void AccountConversion(LT_RESULT_NOTIFICATION_DELEGATE callback)
    {
        if (_IsLogined)
        {
            var idpProfile = KGLocalPlayer.currentPlayer.idpProfile;
            if (idpProfile.idpCode.Equals(KGIdpCodeString.Guest))
            {
                //KakaoUtil.AccountConversionWithCustomUI(KGIdpCode.Kakao, (resultCode) =>
                KakaoUtil.AccountConversion((resultCode) =>
                {
                    bool isSuccessful = resultCode == KGResultCode.Success;
                    if (isSuccessful)
                    {
                        LogResult(_KakaoPrefix, "AccountConversion Succeeded", resultCode.ToString());

                        _IsLogined = KakaoUtil.IsLogined; // 同步登录状态（切换成功，登录状态为false）
                    }
                    else
                    {
                        LogResult(_KakaoPrefix, "AccountConversion Failed", resultCode.ToString());
                    }

                    if (callback != null)
                        callback(isSuccessful);
                });
            }
            else
            {
                HobaDebuger.LogErrorFormat("{0}AccountConversion failed, not logined as guest.", _KakaoPrefix);
            }
        }
    }

    public override void ManageAccount()
    {
    }

    public override void Buy(SDK.BUY_INFO pProduct)
    {
        Debug.Log("LTKakao::Buy");
#if (UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID)
        IAPManager.Instance.BuyProductByID(pProduct.iBillingType, pProduct.strOrderId, pProduct.strProductId);
#else
        Debug.Log("LTKakao::Buy()! Can not use on this platform type!");
#endif
    }

    public override void InitSDK(LT_INITED_NOTIFICATION_DELEGATE fnInit)
    {
        if (!_IsInited)
        {
            Reset();
            //EntryPoint.Instance.gameObject.AddComponent<Test.TestKakao>();
            //初始化打点SDK
            var singularSDK = new GameObject("SingularSDK");
            singularSDK.AddComponent<SingularSDK>();
            
            KakaoUtil.Start((isAuthorized, resultCode) =>
            {
                if (resultCode == KGResultCode.Success)
                {
                    _IsInited = true;
                    _IsLogined = isAuthorized;
                    DeviceLogger.Instance.WriteLog(HobaText.Format("Start succeeded, isAthorized: {0}, ", isAuthorized.ToString()));

                    if (fnInit != null)
                        fnInit(INITED_STATE.LT_INITED_SUCCEED, resultCode);
                }
                else
                {
                    DeviceLogger.Instance.WriteLog(HobaText.Format("Start failed, result code: {0}, ", resultCode.ToString()));
                    if (fnInit != null)
                        fnInit(INITED_STATE.LT_INITED_FAIL, resultCode);
                }
            });
        }
    }


    public override void Init(LT_RESULT_NOTIFICATION_DELEGATE fnResult)
    {
        if (fnResult != null)
            fnResult(_IsInited);
    }

    public override void InitPurchaseVerifyUrl(string url)
    {
        IAPManager.Instance.InitPurchaseVerifyUrl(url);
    }
    public override void ProcessPurchaseCache()
    {
        IAPManager.Instance.ProcessPurchaseCache();
    }

    public override void InitializeIAP(int roleID)
    {
        IAPManager.LT_PURCHASE_NOTIFICATION_DELEGATE fnPurchaseCallback = (bSuccess, pProduct) =>
        {
            SDK.BUY_STATE state = bSuccess ? BUY_STATE.LT_BUY_SUCCEED : BUY_STATE.LT_BUY_PAY_FAILED;
            SDK.BUY_INFO info = new SDK.BUY_INFO();
            info.iBillingType = pProduct.iBillingType;
            info.strOrderId = "";//pProduct.strOrderId;

            if (bSuccess)
            {
                //Callback
                info.strTransactionId = pProduct.strTransactionId;
                info.strReceipt = pProduct.strReceipt;
            }
            else
            {

            }

            if (_BuyCallback != null)
                _BuyCallback(state, info);
        };

        IAPManager.Instance.InitializeIAP(roleID, fnPurchaseCallback);
    }

    public override void RegisterCallback(LT_LOGIN_NOTIFICATION_DELEGATE fnLogin,
        LT_LOGOUT_NOTIFICATION_DELEGATE fnLogout,
        LT_BUY_NOTIFICATION_DELEGATE fnBuy)
    {
        _LoginCallback = fnLogin;
        _LogoutCallback = fnLogout;
        _BuyCallback = fnBuy;
    }


#region AndroidPermissionManagement
    private List<string> _RequiredPermissions = new List<string>();
    private List<string> _OptionalPermissions = new List<string>();
    private string resourceId = string.Empty; // If you want to put an icon, insert the name of the image file in 'KakaoGameSDK.plugin/res/drawable'

    public void ManagePermissions(Action callback)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        KakaoUtil.ShowPermissionsDescription(_RequiredPermissions, _OptionalPermissions, resourceId, (isSuccess, isGranted) =>
        {
            if (isSuccess)
            {
                DeviceLogger.Instance.WriteLog(HobaText.Format("{0}ManagePermissions Succeed, isGranted: {1}", _KakaoPrefix, isGranted.ToString()));
                if (isGranted)
                {
                    // All optional permissions is obtained.
                }
                else
                {
                    // Not all optional permissions is obtained.
                }
                callback();
            }
        });
#endif
    }
#endregion

    public override void EnablePush(bool enable, int type, LT_RESULT_NOTIFICATION_DELEGATE callback)
    {
        var option = (KGPushOption)type;
        KakaoUtil.EnablePush(option, enable, (isSuccess) =>
        {
            if (callback != null)
                callback(isSuccess);
        });
    }

    public override bool GetPushStatus(int type)
    {
        var option = (KGPushOption)type;
        return KakaoUtil.IsPushEnable(option);
    }

#region App Webview
    public override void ShowCustomerCenter(LT_URL_NOTIFICATION_DELEGATE callback)
    {
        KakaoUtil.ShowCustomerCenter((isSuccess, url) =>
        {
            if (isSuccess)
            {
                if (callback != null)
                    callback(url);
            }
            else
            {
                LogResult(_KakaoPrefix, "ShowCustomerCenter", "Failed");
            }
        });
    }

    public override void ShowAnnouncement(LT_URL_NOTIFICATION_DELEGATE callback)
    {
        KakaoUtil.ShowAnnouncement((isSuccess, url) =>
        {
            if (isSuccess)
            {
                if (callback != null)
                    callback(url);
            }
            else
            {
                LogResult(_KakaoPrefix, "ShowAnnouncement", "Failed");
            }
        });
    }

    public override void ShowPromotion(LT_URL_NOTIFICATION_DELEGATE callback)
    {
        KakaoUtil.ShowStartingPromotion((isSuccess, url) =>
        {
            if (isSuccess)
            {
                if (callback != null)
                    callback(url);
            }
            else
            {
                LogResult(_KakaoPrefix, "ShowPromotion", "Failed");
            }
        });
    }

    public override void ShowCoupon(LT_RESULT_NOTIFICATION_DELEGATE callback)
    {
        KakaoUtil.ShowCoupon((isSuccess) =>
        {
            if (callback != null)
                callback(isSuccess);
        });
    }

    public override void ShowInAppWeb(string url, LT_URL_NOTIFICATION_DELEGATE callback)
    {
        KakaoUtil.ShowInAppWeb(url, (isSuccess, deepLinkUrl) =>
        {
            if (isSuccess)
            {
                if (callback != null)
                    callback(deepLinkUrl);
            }
            else
            {
                LogResult(_KakaoPrefix, "ShowInAppWeb", "Failed");
            }
        });
    }
#endregion

#region AppData
    public override string GetCDNAddress()
    {
        if (_IsInited)
        {
            string val = KakaoUtil.CDNAddress;
            if (val != null)
                return val;
        }

        return string.Empty;
    }

    public override string GetGameServerAddress()
    {
        if (_IsInited)
        {
            string val = KakaoUtil.GameServerAddress;
            if (val != null)
                return val;
        }

        return string.Empty;
    }

    public override string GetCustomData(string key)
    {
        if (_IsInited)
        {
            string val = KakaoUtil.GetCustomData(key);
            if (val != null)
                return val;
        }

        return string.Empty;
    }
#endregion

#region GoogleGame
    public override bool IsGoogleGameLogined
    {
        get
        {
            return KakaoUtil.IsGoogleLogined;
        }
    }

    public override void GoogleGameLogin(LT_RESULT_NOTIFICATION_DELEGATE callback)
    {
        if (_IsLogined)
        {
            var idpProfile = KGLocalPlayer.currentPlayer.idpProfile;
            if (idpProfile.idpCode.Equals(KGIdpCodeString.Kakao))
            {
                //Kakao IDP
                KakaoUtil.GoogleLogin((resutlCode) =>
                {
                    LogResult(_KakaoPrefix, "GoogleLogin", resutlCode.ToString());
                    if (callback != null)
                        callback(resutlCode == KGResultCode.Success);
                });
            }
        }
    }

    public override void GoogleGameLogout(LT_RESULT_NOTIFICATION_DELEGATE callback)
    {
        if (IsGoogleGameLogined)
        {
            KakaoUtil.GoogleLogout((resutlCode) =>
            {
                LogResult(_KakaoPrefix, "GoogleLogout", resutlCode.ToString());
                if (callback != null)
                    callback(resutlCode == KGResultCode.Success);
            });
        }
    }

    public override void ShowGoogleAchievementView()
    {
        if (IsGoogleGameLogined)
        {
            KakaoUtil.ShowAchievementView();
        }
    }

    public override void CompleteGoogleAchievement(string id)
    {
        if (IsGoogleGameLogined)
        {
            KakaoUtil.ComlpeteAchievement(id);
        }
    }

    public override void SetGoogleAchievementCompletionLevel(string id, int level)
    {
        if (IsGoogleGameLogined)
        {
            KakaoUtil.SetAchievementCompletionLevel(id, level);
        }
    }
    #endregion

    // 打点部分暂时写死
    public override void UploadRoleInfo(ROLE_INFO roleInfo)
    {
        if (roleInfo == null) return;

        if (roleInfo.roleLevel == 1 || roleInfo.roleLevel == 5 || roleInfo.roleLevel == 10)
        {
            HobaDebuger.LogWarningFormat("{0}UploadRoleInfo level:{1}", _KakaoPrefix, roleInfo.roleLevel.ToString());
            SingularSDK.Event("Level", "level", roleInfo.roleLevel.ToString());
        }
    }

    public override string GetErrStr(int errorCode)
    {
        string ret = string.Empty;
        if (errorCode == KGResultCode.NetworkFailure)
            ret = EntryPoint.Instance.UpdateStringConfigParams.PlatformSDKString_NetworkFailure;
        else if (errorCode == KGResultCode.ServerTimeout)
            ret = EntryPoint.Instance.UpdateStringConfigParams.PlatformSDKString_ServerTimeout;
        else if (errorCode == KGResultCode.InitializationFailed)
            ret = EntryPoint.Instance.UpdateStringConfigParams.PlatformSDKString_InitializationFailed;
        else
            ret = EntryPoint.Instance.UpdateStringConfigParams.PlatformSDKString_UnknownErr;

        ret += "\n" + string.Format(EntryPoint.Instance.UpdateStringConfigParams.PlatformSDKString_ErrorCode, errorCode.ToString());
        return ret;
    }

    private void OnLoginSucceed()
    {
        // Current user id issued by Kakao platform
        string playerId = KGLocalPlayer.currentPlayer.playerId;
        // Access token issued by Kakao platform
        string accessToken = KGSession.accessToken;
        // IDP information 
        var idpProfile = KGLocalPlayer.currentPlayer.idpProfile;

        SingularSDK.SetCustomUserId(playerId);

        string nickname = string.Empty;
        if (idpProfile.idpCode.Equals(KGIdpCodeString.Kakao))
        {
            KGKakaoProfile kakaoProfile = idpProfile as KGKakaoProfile;
            nickname = kakaoProfile.nickname;
        }

        USER_INFO userInfo = new USER_INFO();
        userInfo.strUserName = string.Empty;
        userInfo.strNickName = nickname;
        userInfo.strUserID = playerId;
        userInfo.strAccessToken = accessToken;
        userInfo.bGuest = idpProfile.idpCode.Equals(KGIdpCodeString.Guest);

        _IsLogined = true;
        if (_LoginCallback != null)
            _LoginCallback(LOGIN_STATE.LT_LOGIN_SUCCEED, userInfo);
    }

    private void Reset()
    {
        _IsLogined = false;
        _ResumeCount = 0;
    }

    private void LogResult(string prefix, string name, string code)
    {
        HobaDebuger.LogWarningFormat("{0}{1}, result code:{2}", prefix, name, code);
    }

    //未完成订单 再次验单
    public override void ReProcessingOrder()
    {
    }
}
#endif