#if PLATFORM_KAKAO
using UnityEngine;
using Kakaogame.SDK;
using System.Collections.Generic;
using System;

namespace SDK.Kakao
{
    public class KakaoUtil
    {
        #region Basic
        public static bool IsLogined { get { return KGSession.isLoggedIn; } }

        public static void Start(Action<bool, int> callback)
        {
            KGSession.Start(
                (result, isAuthorized) => {
                    if (callback != null)
                    {
                        if (result.isSuccess)
                        {
                            // Start succeeded
                            callback(isAuthorized, result.code);
                        }
                        else
                        {
                            // Start failed
                            callback(false, result.code);
                        }
                    }
                });
        }

        public static void Pause()
        {
            KGSession.Pause(_ => { });
        }

        public static void Resume(Action<int> callback)
        {
            KGSession.Resume(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void Login(Action<int> callback)
        {
            KGSession.Login(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void LoginWithCustomUI(KGIdpCode idpCode, Action<int> callback)
        {
            KGSessionForCustomUI.Login(
                idpCode,
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void Logout(Action<int> callback)
        {
            KGSession.Logout(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void LogoutWithCustomUI(Action<int> callback)
        {
            KGSessionForCustomUI.Logout(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void AccountConversion(Action<int> callback)
        {
            KGSession.Connect(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void AccountConversionWithCustomUI(KGIdpCode idpCode, Action<int> callback)
        {
            KGSessionForCustomUI.Connect(
                idpCode,
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void Unregister(Action<int> callback)
        {
            KGSession.Unregister(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }
        #endregion

        #region AndroidPermissionManagement
        public static void ShowPermissionsDescription(List<string> requiredPermissions, List<string> optionalPermissions, string resourceId, Action<bool, bool> callback)
        {
            KGApplication.RequestPermissionsWithDescriptionPopup(
                requiredPermissions,
                optionalPermissions,
                resourceId,
                (result, granted) => {
                    if (callback != null)
                    {
                        if (result.isSuccess)
                        {
                            // Success Request : All required permissions are acquired.
                            if (granted)
                            {
                                // All optional permissions is obtained.
                                callback(true, true);
                            }
                            else
                            {
                                // Not all optional permissions is obtained.
                                callback(true, false);
                            }
                        }
                        else
                        {
                            // Fail Request.
                            callback(false, false);
                        }
                    }
                });
        }

        public static void CheckPermission(string permission, Action<bool, bool> callback)
        {
            KGApplication.CheckPermission(
                permission,
                (result, granted) => {
                    if (callback != null)
                    {
                        if (result.isSuccess)
                        {
                            if (granted)
                            {
                                // Permission granted
                                callback(true, true);
                            }
                            else
                            {
                                // No permission granted
                                callback(true, false);
                            }
                        }
                        else
                        {
                            // Request Failed
                            callback(false, false);
                        }
                    }
                });
        }

        public static void CheckPermissions(List<string> permissions, Action<bool, bool> callback)
        {
            KGApplication.CheckPermissions(
                permissions,
                (result, granted) => {
                    if (callback != null)
                    {
                        if (result.isSuccess)
                        {
                            if (granted)
                            {
                                // Permission granted
                                callback(true, true);
                            }
                            else
                            {
                                // No permission granted
                                callback(true, false);
                            }
                        }
                        else
                        {
                            // Request Failed
                            callback(false, false);
                        }
                    }
                });
        }

        public static void RequestPermission(string permission, Action<bool, bool> callback)
        {
            KGApplication.RequestPermission(
                permission,
                (result, granted) => {
                    if (callback != null)
                    {
                        if (result.isSuccess)
                        {
                            if (granted)
                            {
                                // Permission granted
                                callback(true, true);
                            }
                            else
                            {
                                // No permission granted
                                callback(true, false);
                            }
                        }
                        else
                        {
                            // Request Failed
                            callback(false, false);
                        }
                    }
                });
        }

        public static void RequestPermissions(List<string> permissions, Action<bool, bool> callback)
        {
            KGApplication.RequestPermissions(
                permissions,
                (result, granted) => {
                    if (callback != null)
                    {
                        if (result.isSuccess)
                        {
                            if (granted)
                            {
                                // Permission granted
                                callback(true, true);
                            }
                            else
                            {
                                // No permission granted
                                callback(true, false);
                            }
                        }
                        else
                        {
                            // Request Failed
                            callback(false, false);
                        }
                    }
                });
        }
        #endregion

        #region Push

        public static void EnablePush(KGPushOption option, bool enable, Action<bool> callback)
        {
            KGPush.EnablePush(
                option,
                enable,
                (result) => {
                    if (callback != null)
                    {
                        if (result.isSuccess)
                        {
                            // Setting of push receipt option successful
                            callback(true);
                        }
                        else
                        {
                            // Setting of push receipt option failed
                            callback(false);
                        }
                    }
                });
        }

        public static bool IsPushEnable(KGPushOption option)
        {
            return KGPush.IsEnablePush(option);
        }
        #endregion

        #region App Webview
        public static void ShowCustomerCenter(Action<bool, string> callback)
        {
            KGSupport.ShowCSView(
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowAnnouncement(Action<bool, string> callback)
        {
            KGSupport.ShowNoticeView(
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowAnnouncement(string noticeKey, Action<bool, string> callback)
        {
            KGSupport.ShowNoticeView(
                noticeKey,
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowNotice(Action<bool, string> callback)
        {
            KGSupport.ShowNoticeViewForOneDay(
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowNotice(string noticeKey, Action<bool, string> callback)
        {
            KGSupport.ShowNoticeViewForOneDay(
                noticeKey,
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowEvent(string eventKey)
        {
            KGSupport.ShowEventView(eventKey);
        }

        public static void ShowEvent(string eventKey, Action<bool, string> callback)
        {
            KGSupport.ShowEventView(
                eventKey,
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowCommunity(Action<bool, string> callback)
        {
            KGSupport.ShowCommunityView(
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowCommunity(string pageCode, Action<bool, string> callback)
        {
            KGSupport.ShowCommunityView(
                pageCode,
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowCommunity(long articleId, Action<bool, string> callback)
        {
            KGSupport.ShowCommunityView(
                articleId,
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowGachaOdds(Action<bool, string> callback)
        {
            KGSupport.ShowGachaOddsView(
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }

        public static void ShowInAppWeb(string url, Action<bool, string> callback)
        {
            KGSupport.ShowInAppWebView(
                url,
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }
        #endregion

        #region Promotion
        public static void ShowStartingPromotion(Action<bool, string> callback)
        {
            KGPromotion.ShowStartingPromotionPopups(
                (result, deepLinkUrl) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess, deepLinkUrl);
                    }
                });
        }
        #endregion

        #region Coupon
        public static void ShowCoupon(Action<bool> callback)
        {
            KGCoupon.ShowCouponPopup(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.isSuccess);
                    }
                });
        }
        #endregion

        #region GooglePlayService
        public static bool IsGoogleLogined
        {
            get { return KGGoogleGames.isLoggedIn; }
        }

        public static void GoogleLogin(Action<int> callback)
        {
            KGGoogleGames.Login(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void GoogleLogout(Action<int> callback)
        {
            KGGoogleGames.Logout(
                (result) => {
                    if (callback != null)
                    {
                        callback(result.code);
                    }
                });
        }

        public static void ShowAchievementView()
        {
            KGGoogleGamesAchievements.ShowAchievementView();
        }

        public static void ComlpeteAchievement(string id)
        {
            KGGoogleGamesAchievements.Unlock(id);
        }

        public static void ShowSingleAchievement(string id)
        {
            KGGoogleGamesAchievements.Reveal(id);
        }

        public static void IncrementalAchievement(string id, int numSteps)
        {
            KGGoogleGamesAchievements.Increment(id, numSteps);
        }

        public static void SetAchievementCompletionLevel(string id, int numSteps)
        {
            KGGoogleGamesAchievements.SetSteps(id, numSteps);
        }
        #endregion

        #region AppData
        public static bool IsInReview { get { return KGAppOption.isInReview; } }

        public static string CDNAddress { get { return KGAppOption.cdnAddress; } }

        public static string GameServerAddress{ get { return KGAppOption.gameServerAddress; } }

        public static string GetCustomData(string key)
        {
            return KGAppOption.GetValue(key);
        }
        #endregion
    }
}
#endif