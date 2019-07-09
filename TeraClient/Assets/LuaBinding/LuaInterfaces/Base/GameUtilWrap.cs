using System;
using LuaInterface;
using Common;
using System.Collections;

public static partial class GameUtilWrap
{
    #region Register
    public static LuaMethod[] common_regs = new LuaMethod[]
    {
        new LuaMethod("AsyncLoad", AsyncLoad),
        new LuaMethod("UnloadBundle", UnloadBundle),
        new LuaMethod("UnloadBundleOfAsset", UnloadBundleOfAsset),
        new LuaMethod("ClearAssetBundleCache", ClearAssetBundleCache),

        new LuaMethod("AddObjectComponent", AddObjectComponent),
        new LuaMethod("EnableHostPosSyncWhenMove", EnableHostPosSyncWhenMove),  
        new LuaMethod("AddMoveBehavior", AddMoveBehavior),
        new LuaMethod("SetMoveBehaviorSpeed", SetMoveBehaviorSpeed),
        new LuaMethod("AddJoyStickMoveBehavior", AddJoyStickMoveBehavior),
        //new LuaMethod("AddWanderMoveBehavior", AddWanderMoveBehavior),
        //new LuaMethod("AddFadeInBehavior", AddFadeInBehavior),  -- 关闭此功能
        //new LuaMethod("AddFadeOutBehavior", AddFadeOutBehavior),  -- 关闭此功能
        new LuaMethod("AddFollowBehavior", AddFollowBehavior),
        new LuaMethod("AddTurnBehavior", AddTurnBehavior),
        new LuaMethod("AddDashBehavior", AddDashBehavior),
        new LuaMethod("AddAdsorbEffect", AddAdsorbEffect),
        new LuaMethod("RemoveAdsorbEffect", RemoveAdsorbEffect),
        //new LuaMethod("RemoveAdsorbBehavior", RemoveAdsorbBehavior),
        new LuaMethod("RemoveBehavior", RemoveBehavior),
        new LuaMethod("HasBehavior", HasBehavior),
        new LuaMethod("ChangeOutward", ChangeOutward),
        new LuaMethod("ChangeHairColor",ChangeHairColor),
        new LuaMethod("ChangeSkinColor",ChangeSkinColor),
        new LuaMethod("GetPanelUIObjectByID", GetPanelUIObjectByID),
        new LuaMethod("EnableGroundNormal", EnableGroundNormal),
        new LuaMethod("SetGameObjectYOffset", SetGameObjectYOffset),

        //new LuaMethod("SetMapLayers", SetMapLayers),
        new LuaMethod("GetResourceBasePath", GetResourceBasePath),
        new LuaMethod("GetDocumentPath", GetDocumentPath),
        new LuaMethod("RequestFx", RequestFx),
        new LuaMethod("PreloadFxAsset", PreloadFxAsset),
        new LuaMethod("RequestUncachedFx", RequestUncachedFx),
        new LuaMethod("RequestArcFx", RequestArcFx),
        new LuaMethod("SetFxScale", SetFxScale),
        new LuaMethod("SetMineObjectScale", SetMineObjectScale),        
        new LuaMethod("BluntAttachedFxs", BluntAttachedFxs),
        new LuaMethod("ChangeAttach", ChangeAttach),
        new LuaMethod("GetVoiceDir", GetVoiceDir),
        new LuaMethod("GetCustomPicDir", GetCustomPicDir),
        new LuaMethod("ChangePartMesh", ChangePartMesh),

        new LuaMethod("ClearFxManCache", ClearFxManCache),
        new LuaMethod("FxCacheManCleanup", FxCacheManCleanup),

        new LuaMethod("ShakeUIScreen", ShakeUIScreen),
        new LuaMethod("DoSlider",DoSlider),
        new LuaMethod("DoKillSlider",DoKillSlider),
        new LuaMethod("ShowHUDText",ShowHUDText),
        new LuaMethod("ClearHUDTextFontCache",ClearHUDTextFontCache),

        new LuaMethod("FindChild", FindChild),
        new LuaMethod("GetHangPoint", GetHangPoint),
        new LuaMethod("EnablePhysicsCollision", EnablePhysicsCollision),
        new LuaMethod("SetLayerRecursively", SetLayerRecursively),
        new LuaMethod("AddObjectEffect", AddObjectEffect),
        new LuaMethod("RefreshObjectEffect", RefreshObjectEffect),
        new LuaMethod("SetButtonInteractable", SetButtonInteractable),
        new LuaMethod("SetImageColor", SetImageColor),
        new LuaMethod("SetTextColor", SetTextColor),
        new LuaMethod("AddCameraOrScreenEffect", AddCameraOrScreenEffect),
        new LuaMethod("EnableSpecialVisionEffect", EnableSpecialVisionEffect),
        new LuaMethod("GC", GC),
        //new LuaMethod("UnloadUnusedAssets", UnloadUnusedAssets),
        new LuaMethod("GetOpenUDID", GetOpenUDID),
        new LuaMethod("OpenUrl", OpenUrl),
        new LuaMethod("GetVirtualMemoryUsedSize", GetVirtualMemoryUsedSize),
        new LuaMethod("GetPhysMemoryUsedSize", GetPhysMemoryUsedSize),
        
        new LuaMethod("CopyTextToClipboard", CopyTextToClipboard),

        new LuaMethod("AddFootStepTouch", AddFootStepTouch),
        new LuaMethod("RemoveFootStepTouch", RemoveFootStepTouch),
        new LuaMethod("SetSoundBGMVolume", SetSoundBGMVolume),
        new LuaMethod("SetBGMSysVolume", SetBGMSysVolume), // 设置系统设置里的背景音量
        new LuaMethod("GetBGMSysVolume", GetBGMSysVolume),                          //系统设置的背景音乐大小
        new LuaMethod("IsBackgroundMusicEnable", IsBackgroundMusicEnable),          //背景音乐开关

        new LuaMethod("SetSoundLanguage", SetSoundLanguage),
        new LuaMethod("SetSoundEffectVolume", SetSoundEffectVolume),
        new LuaMethod("SetEffectSysVolume", SetEffectSysVolume),
        new LuaMethod("GetEffectSysVolume", GetEffectSysVolume),                          //音效大小
        new LuaMethod("IsEffectAudioEnable", IsEffectAudioEnable),                  //音效开关
        new LuaMethod("SetCutSceneVolume", SetCutSceneVolume),
        new LuaMethod("SetCutSceneSysVolume", SetCutSceneSysVolume),
        new LuaMethod("GetCutSceneSysVolume", GetCutSceneSysVolume), 
        new LuaMethod("SetUIVolume", SetUIVolume),
        new LuaMethod("SetUISysVolume", SetUISysVolume),
        new LuaMethod("GetUISysVolume", GetUISysVolume), 

        //画质设定
        new LuaMethod("SetPostProcessLevel", SetPostProcessLevel),      //后处理效果                
        new LuaMethod("GetPostProcessLevel", GetPostProcessLevel),
        new LuaMethod("SetShadowLevel", SetShadowLevel),                //阴影质量      
        new LuaMethod("GetShadowLevel", GetShadowLevel),
        new LuaMethod("SetCharacterLevel", SetCharacterLevel),          //角色效果            
        new LuaMethod("GetCharacterLevel", GetCharacterLevel),
        new LuaMethod("SetSceneDetailLevel", SetSceneDetailLevel),      //场景细节                
        new LuaMethod("GetSceneDetailLevel", GetSceneDetailLevel),
        new LuaMethod("SetFxLevel", SetFxLevel),                      //特效级别
        new LuaMethod("GetFxLevel", GetFxLevel),
        new LuaMethod("IsUseDOF", IsUseDOF),                            //景深效果
        new LuaMethod("EnableDOF", EnableDOF),
        new LuaMethod("IsUsePostProcessFog", IsUsePostProcessFog),      //高级雾效
        new LuaMethod("EnablePostProcessFog", EnablePostProcessFog),
        new LuaMethod("IsUseWaterReflection", IsUseWaterReflection),        //水面反射
        new LuaMethod("EnableWaterReflection", EnableWaterReflection),  
        new LuaMethod("IsUseWeatherEffect", IsUseWeatherEffect),        //全局天气特效
        new LuaMethod("EnableWeatherEffect", EnableWeatherEffect),
        new LuaMethod("IsUseDetailFootStepSound", IsUseDetailFootStepSound),    //细节音效
        new LuaMethod("EnableDetailFootStepSound", EnableDetailFootStepSound),
        new LuaMethod("SetFPSLimit", SetFPSLimit),          //帧率
        new LuaMethod("GetFPSLimit", GetFPSLimit),
        new LuaMethod("SetSimpleBloomHDParams", SetSimpleBloomHDParams),
        new LuaMethod("ApplyGfxConfig", ApplyGfxConfig),

        new LuaMethod("OnHostPlayerPosChange", OnHostPlayerPosChange),
        new LuaMethod("GetMapHeight", GetMapHeight),
        new LuaMethod("WorldPositionToCanvas", WorldPositionToCanvas),
        new LuaMethod("AlignUiElementWithOther", AlignUiElementWithOther),
        new LuaMethod("StopSkillScreenEffect", StopSkillScreenEffect),
        new LuaMethod("SetCurLayerVisible", SetCurLayerVisible),
        new LuaMethod("SetupWorldCanvas", SetupWorldCanvas),
        new LuaMethod("SetScrollPositionZero", SetScrollPositionZero),
        new LuaMethod("SetScrollEnabled", SetScrollEnabled),
        new LuaMethod("SetupUISorting", SetupUISorting),
        new LuaMethod("SetAllTogglesOff", SetAllTogglesOff),

        new LuaMethod("PlayCG", PlayCG),
        new LuaMethod("StopCG", StopCG),
        
        new LuaMethod("StartScreenFade", StartScreenFade),
        new LuaMethod("ClearScreenFadeEffect", ClearScreenFadeEffect),
        new LuaMethod("IsBlockedByObstacle" ,IsBlockedByObstacle),
        new LuaMethod("IsValidPosition", IsValidPosition),
        new LuaMethod("GetNearestValidPosition", GetNearestValidPosition),
        new LuaMethod("IsValidPositionXZ", IsValidPositionXZ),
        new LuaMethod("AddCooldownComponent", AddCooldownComponent),
        new LuaMethod("RemoveCooldownComponent", RemoveCooldownComponent),
        new LuaMethod("SetCircleProgress",SetCircleProgress),
        //new LuaMethod("EnableRealTimeShadow", EnableRealTimeShadow),        
        new LuaMethod("GetJoystickAxis", GetJoystickAxis),
        new LuaMethod("ResizeCollider", ResizeCollider),
        new LuaMethod("DisableCollider", DisableCollider),
        new LuaMethod("RotateByAngle", RotateByAngle),
        //new LuaMethod("FetchProtocols", FetchProtocols),
        //new LuaMethod("PopupOneProtocolDetail", PopupOneProtocolDetail),
        new LuaMethod("SendProtocol", SendProtocol),
        new LuaMethod("GenHmacMd5", GenHmacMd5),
        new LuaMethod("MD5ComputeHash", MD5ComputeHash),
        new LuaMethod("HMACMD5ComputeHash", HMACMD5ComputeHash),
        new LuaMethod("ComputeRNGCryptoNonce", ComputeRNGCryptoNonce),
        new LuaMethod("OnWorldLoaded", OnWorldLoaded),
        new LuaMethod("OnWorldRelease", OnWorldRelease),
        new LuaMethod("OnFinishEnterWorld", OnFinishEnterWorld),
        new LuaMethod("OnLoadingShow", OnLoadingShow),
        new LuaMethod("PassLuaGuideMan",PassLuaGuideMan),
        new LuaMethod("OnEntityModelChanged", OnEntityModelChanged),
        new LuaMethod("LoadSceneBlocks", LoadSceneBlocks),
        new LuaMethod("PlayVideo",PlayVideo),
        new LuaMethod("StopVideo",StopVideo),
        new LuaMethod("IsPlayingVideo",IsPlayingVideo),
        new LuaMethod("PrepareVideoUnit", PrepareVideoUnit),
        new LuaMethod("ActivateVideoUnit", ActivateVideoUnit),
        new LuaMethod("DeactivateVideoUnit", DeactivateVideoUnit),
        new LuaMethod("ReleaseVideoUnit", ReleaseVideoUnit),
        new LuaMethod("ContinueLogoMaskFade",ContinueLogoMaskFade),
        new LuaMethod("SetCameraGreyOrNot", SetCameraGreyOrNot),
        new LuaMethod("ChangeSceneWeather",ChangeSceneWeather),
        new LuaMethod("ChangeSceneWeatherByMemory",ChangeSceneWeatherByMemory),
        
        new LuaMethod("OpenUIWithEffect", OpenUIWithEffect),
        new LuaMethod("LeaveUIEffect", LeaveUIEffect),
        
        // UI相关
        new LuaMethod("SetSprite", SetSprite),
        new LuaMethod("CleanSprite", CleanSprite),
        new LuaMethod("SetItemIcon", SetItemIcon),
        new LuaMethod("SetSpriteFromImageFile", SetSpriteFromImageFile),
        new LuaMethod("MakeImageGray", MakeImageGray),
        new LuaMethod("ChangeGraphicAlpha", ChangeGraphicAlpha),
        new LuaMethod("SetGroupImg",SetGroupImg),
        new LuaMethod("SetBtnExpress",SetBtnExpress),
		new LuaMethod("SetNativeSize",SetNativeSize),
		new LuaMethod("SetMaskTrs",SetMaskTrs),
        new LuaMethod("SetSpriteFromResources", SetSpriteFromResources),        //用Resources下图片替换sprite

        new LuaMethod("RegisterUIEventHandler",RegisterUIEventHandler),
        new LuaMethod("GetRootCanvasPosAndSize",GetRootCanvasPosAndSize),

        new LuaMethod("RegisterTip",RegisterTip),
        new LuaMethod("UnregisterTip",UnregisterTip),

        new LuaMethod("GetCurrentVersion", GetCurrentVersion),
        //new LuaMethod("GetCanvasPostion",GetCanvasPostion),
        new LuaMethod("PreLoadUIFX", PreLoadUIFX),
        new LuaMethod("IsPlayingUISfx",IsPlayingUISfx),
        new LuaMethod("PlayUISfx",PlayUISfx),
        new LuaMethod("PlayUISfxClipped",PlayUISfxClipped),
        new LuaMethod("StopUISfx",StopUISfx),
        new LuaMethod("SetUISfxLayer",SetUISfxLayer),
        //new LuaMethod("SetUISfxVLayerWithUI", SetUISfxVLayerWithUI),
        new LuaMethod("EnableBlockCanvas", EnableBlockCanvas),
        new LuaMethod("ShowScreenShot", ShowScreenShot),
        new LuaMethod("EnableReversedRaycast", EnableReversedRaycast),
        new LuaMethod("SetCanvasGroupAlpha", SetCanvasGroupAlpha),
        //new LuaMethod("LayoutTopTabs", LayoutTopTabs),
        new LuaMethod("SetIgnoreLayout", SetIgnoreLayout),
        new LuaMethod("ResetMask2D", ResetMask2D),
        new LuaMethod("SetShowHeadInfo",SetShowHeadInfo),

        new LuaMethod("SetTemplatePath", SetTemplatePath),
        new LuaMethod("AddSpecialTemplateDataPath", AddSpecialTemplateDataPath),
        new LuaMethod("AddPreloadTemplateData", AddPreloadTemplateData),
        new LuaMethod("PreloadGameData", PreloadGameData),
        new LuaMethod("GetTemplateData", GetTemplateData),

        new LuaMethod("GetRuntimePlatform", GetRuntimePlatform),

        new LuaMethod("GetConfigPlatform", GetConfigPlatform),
        new LuaMethod("GetConfigLocale", GetConfigLocale),

        new LuaMethod("GetResponseDeviceString", GetResponseDeviceString),
        new LuaMethod("GetResponseOSVersionString", GetResponseOSVersionString),
        new LuaMethod("GetResponseMACString", GetResponseMACString),
        new LuaMethod("GetLargeMemoryLimit", GetLargeMemoryLimit),
        new LuaMethod("GetMemoryLimit", GetMemoryLimit),
        new LuaMethod("getTotalPss", getTotalPss),
        new LuaMethod("getMemotryStats", getMemotryStats),

        new LuaMethod("ReadPersistentConfig", ReadPersistentConfig),
        new LuaMethod("WritePersistentConfig", WritePersistentConfig),

        //new LuaMethod("HasTemplateData", HasTemplateData),
        //new LuaMethod("GetTemplateValue", GetTemplateValue),
        //new LuaMethod("GetTemplateValueEx", GetTemplateValueEx),

        new LuaMethod("DrawLine", DrawLine),
        new LuaMethod("PrintCurrentTime", PrintCurrentTime),
        new LuaMethod("FetchResFromCache", FetchResFromCache),
        new LuaMethod("AddResToCache", AddResToCache),
        new LuaMethod("GetEntityBaseRes", GetEntityBaseRes),
        new LuaMethod("RecycleEntityBaseRes", RecycleEntityBaseRes),
        new LuaMethod("ClearEntityModelCache", ClearEntityModelCache),

        new LuaMethod("OnHostPlayerCreate", OnHostPlayerCreate),
        new LuaMethod("OnHostPlayerDestroy", OnHostPlayerDestroy),
        new LuaMethod("OnMainCameraCreate", OnMainCameraCreate),
        new LuaMethod("OnMainCameraDestroy", OnMainCameraDestroy),
        new LuaMethod("EnableMainCamera", EnableMainCamera),

        new LuaMethod("SetCameraParams", SetCameraParams),
        new LuaMethod("SetCameraParamsEX", SetCameraParamsEX),

        new LuaMethod("SetGameCamCtrlParams", SetGameCamCtrlParams),
        new LuaMethod("SetGameCamCtrlMode", SetGameCamCtrlMode),            //设置摄像机控制模式
        new LuaMethod("GetGameCamCtrlMode", GetGameCamCtrlMode),
        new LuaMethod("SetTransToPortal", SetTransToPortal),
        new LuaMethod("SetProDefaultSpeed", SetProDefaultSpeed),
        new LuaMethod("SetGameCamHeightOffsetInterval", SetGameCamHeightOffsetInterval),
        new LuaMethod("SetGameCamOwnDestDistOffset", SetGameCamOwnDestDistOffset),
        new LuaMethod("SetGameCamDefaultDestDistOffset", SetGameCamDefaultDestDistOffset),
        new LuaMethod("SetGameCamDestDistOffset", SetGameCamDestDistOffset),
        new LuaMethod("SetDestDistOffsetAndDestPitchDeg", SetDestDistOffsetAndDestPitchDeg),
        new LuaMethod("SetSkillActCamMode", SetSkillActCamMode),        
        new LuaMethod("GetGameCamDestDistOffset", GetGameCamDestDistOffset),
        new LuaMethod("GetGameCamCurDistOffset", GetGameCamCurDistOffset),  // 给策划调试用，以后删
        new LuaMethod("SetCamToDefault", SetCamToDefault),
        new LuaMethod("QuickRecoverCamToDest", QuickRecoverCamToDest),
        new LuaMethod("SetCamLockState", SetCamLockState),
        new LuaMethod("ShowGameInfo", ShowGameInfo),
        new LuaMethod("GetChargeDistance", GetChargeDistance),
        new LuaMethod("CameraLookAtNpc",CameraLookAtNpc),

        new LuaMethod("GetMainCameraPosition",GetMainCameraPosition),
        new LuaMethod("GetScreenSize", GetScreenSize),
        new LuaMethod("GetClientTime", GetClientTime),
        new LuaMethod("GetServerTime", GetServerTime),
        new LuaMethod("GetCurrentSecondTime", GetCurrentSecondTime),
        new LuaMethod("SetServerTimeGap", SetServerTimeGap),
        new LuaMethod("GetDateTimeNowTicks", GetDateTimeNowTicks),
        new LuaMethod("EnableButton",EnableButton),
        //new LuaMethod("PreloadBgm", PreloadBgm),
        //new LuaMethod("ClearBgmCache", ClearBgmCache),
        new LuaMethod("PlayBackgroundMusic", PlayBackgroundMusic),
        new LuaMethod("PlayEnvironmentMusic", PlayEnvironmentMusic),
        new LuaMethod("Play3DAudio", Play3DAudio),
        new LuaMethod("PlayAttached3DAudio", PlayAttached3DAudio),
        new LuaMethod("Stop3DAudio", Stop3DAudio),
        new LuaMethod("Stop2DAudio", Stop2DAudio),
        new LuaMethod("Play3DVoice", Play3DVoice),
        new LuaMethod("Play3DShout", Play3DShout),
        new LuaMethod("Play2DAudio", Play2DAudio),
        new LuaMethod("Play2DHeartBeat", Play2DHeartBeat),
        new LuaMethod("EnableEffectAudio", EnableEffectAudio),
        new LuaMethod("EnableBackgroundMusic", EnableBackgroundMusic),
        new LuaMethod("ResetSoundMan", ResetSoundMan),
        new LuaMethod("DebugCommand", DebugCommand),
        new LuaMethod("LuaMemory", LuaMemory),
        new LuaMethod("GetAllTid", GetAllTid),
        new LuaMethod("GetStringLength", GetStringLength),
        new LuaMethod("SetStringLength", SetStringLength),
        new LuaMethod("RequestServerList", RequestServerList),
        new LuaMethod("GetServerList", GetServerList),
        new LuaMethod("RequestAccountRoleList", RequestAccountRoleList),
        new LuaMethod("GetAccountRoleList", GetAccountRoleList),
        new LuaMethod("GetOrderZoneId", GetOrderZoneId),
        new LuaMethod("UploadPicture", UploadPicture),
        new LuaMethod("DownloadPicture", DownloadPicture),
        // new LuaMethod("SetVisibleRadius", SetVisibleRadius),
        //new LuaMethod("CanNavigateTo", CanNavigateTo),              //只判断寻路是否可达
        new LuaMethod("CanNavigateToXYZ", CanNavigateToXYZ),
        //new LuaMethod("CancelNavigation", CancelNavigation),
        //new LuaMethod("IsNavigating", IsNavigating),
        new LuaMethod("GetPointInPath", GetPointInPath),
        new LuaMethod("GetCurrentCompleteDistance", GetCurrentCompleteDistance),
        new LuaMethod("GetCurrentTotalDistance", GetCurrentTotalDistance),
        new LuaMethod("GetCurrentTargetPos", GetCurrentTargetPos),
        new LuaMethod("GetAllPointsInNavMesh",GetAllPointsInNavMesh),
        new LuaMethod("GetNavDistOfTwoPoint",GetNavDistOfTwoPoint),
        new LuaMethod("IsCollideWithBlockable", IsCollideWithBlockable),
        new LuaMethod("PathFindingIsConnected", PathFindingIsConnected),        //从startPos到endPos, 是否直线连通，包括obstacle和navmesh
        new LuaMethod("PathFindingIsConnectedWithPoint", PathFindingIsConnectedWithPoint),
        new LuaMethod("PathFindingCanNavigateTo", PathFindingCanNavigateTo),    //从startPos到endPos, 是否寻路连通，包括obstacle和navmesh
        new LuaMethod("PathFindingCanNavigateToXYZ", PathFindingCanNavigateToXYZ),
        new LuaMethod("FindFirstConnectedPoint", FindFirstConnectedPoint),      //从startPos到endPos, 找到第一个连通点

        new LuaMethod("SetPanelSortingLayerOrder", SetPanelSortingLayerOrder),
        //new LuaMethod("SetPanelSortingOrder", SetPanelSortingOrder),
        new LuaMethod("MovePanelSortingOrder", MovePanelSortingOrder),
        new LuaMethod("GetPanelSortingOrder", GetPanelSortingOrder),
        //new LuaMethod("SetPanelSortingLayer", SetPanelSortingLayer),
        new LuaMethod("GetPanelSortingLayer", GetPanelSortingLayer),
		new LuaMethod("SetFxSorting", SetFxSorting),
        new LuaMethod("Num2SortingLayerID", Num2SortingLayerID),
        new LuaMethod("HidePanel", HidePanel),
        new LuaMethod("SetRenderTexture",SetRenderTexture),
        new LuaMethod("EnableRotate",EnableRotate),
        new LuaMethod("GetScreenPosToTargetPos",GetScreenPosToTargetPos),
        new LuaMethod("DoMove", DoMove),
        new LuaMethod("DoLocalMove", DoLocalMove),
        new LuaMethod("DoScale", DoScale),
        new LuaMethod("DoAlpha", DoAlpha),
        new LuaMethod("DoScaleFrom",DoScaleFrom),
        new LuaMethod("DoLoopRotate", DoLoopRotate),
        new LuaMethod("DoLocalRotateQuaternion",DoLocalRotateQuaternion),
        new LuaMethod("DoKill", DoKill),
        new LuaMethod("ChangeGradientBtmColor",ChangeGradientBtmColor),

        new LuaMethod("PlaySequenceFrame", PlaySequenceFrame),
        new LuaMethod("StopSequenceFrame", StopSequenceFrame),

        new LuaMethod("PlayEarlyWarningGfx", PlayEarlyWarningGfx),
        new LuaMethod("StopGfx", StopGfx),
        new LuaMethod("ChangeGfxPlaySpeed", ChangeGfxPlaySpeed),
        new LuaMethod("GetEmojiCount", GetEmojiCount),
        new LuaMethod("SetEmojiSprite", SetEmojiSprite),
        new LuaMethod("InputEmoji", InputEmoji),
        new LuaMethod("ClearAllEmoji", ClearAllEmoji),

        new LuaMethod("IsGameObjectInCamera", IsGameObjectInCamera),
        new LuaMethod("SubUnicodeString", SubUnicodeString),
        new LuaMethod("GetUnicodeStrLength", GetUnicodeStrLength),

        new LuaMethod("SetCurrentMapInfo", SetCurrentMapInfo),
        new LuaMethod("GetCurrentMapId", GetCurrentMapId),
        new LuaMethod("GetCurrentSceneTid", GetCurrentSceneTid),
        new LuaMethod("SetSceneEffect", SetSceneEffect),
        new LuaMethod("CaptureScreen", CaptureScreen),
        new LuaMethod("SaveScreenShot", SaveScreenShot),
        new LuaMethod("AbandonScreenShot", AbandonScreenShot),

        new LuaMethod("IsCustomPicFileExist", IsCustomPicFileExist),
        new LuaMethod("ChangeDressColor", ChangeDressColor),
        new LuaMethod("ChangeDressEmbroidery", ChangeDressEmbroidery),
        new LuaMethod("EnableLockWingYZRotation", EnableLockWingYZRotation),
        new LuaMethod("EnableAnimationBulletTime", EnableAnimationBulletTime),
        new LuaMethod("SetEntityColliderRadius", SetEntityColliderRadius),
        new LuaMethod("EnableDressUnderSfx", EnableDressUnderSfx),
        new LuaMethod("EnableOutwardPart", EnableOutwardPart),
        
        new LuaMethod("EnableBackUICamera", EnableBackUICamera),

        new LuaMethod("GetUserLanguageCode", GetUserLanguageCode),
        new LuaMethod("WriteUserLanguageCode", WriteUserLanguageCode),
        new LuaMethod("GetUserLanguagePostfix", GetUserLanguagePostfix),

        new LuaMethod("SetActiveFxMaxCount", SetActiveFxMaxCount),
        new LuaMethod("GetFxManUncachedFxsRoot", GetFxManUncachedFxsRoot),

        new LuaMethod("AddOrSubForTest", AddOrSubForTest), // 给策划调参数用
        new LuaMethod("SetExteriorDebugParams", SetExteriorDebugParams), // 给策划调参数用
        new LuaMethod("SetExteriorCamParams", SetExteriorCamParams),
        new LuaMethod("SetExteriorCamHeightOffset", SetExteriorCamHeightOffset),

        new LuaMethod("AdjustDropdownRect", AdjustDropdownRect),
        new LuaMethod("SetDropdownValue", SetDropdownValue),
        new LuaMethod("SetTipsPosition", SetTipsPosition),
        new LuaMethod("SetApproachPanelPosition", SetApproachPanelPosition),
        new LuaMethod("SetGiftItemPosition", SetGiftItemPosition),
        new LuaMethod("GetTipLayoutHeight", GetTipLayoutHeight),
        new LuaMethod("SetOutlineColor", SetOutlineColor),
        new LuaMethod("SetActiveOutline", SetActiveOutline),
        new LuaMethod("HasTouchOne", HasTouchOne),
        new LuaMethod("StartSleepingCD", StartSleepingCD),
        new LuaMethod("StopSleepingCD", StopSleepingCD),
        new LuaMethod("SetSleepingCD", SetSleepingCD),
        new LuaMethod("ResetSleepingCD", ResetSleepingCD),
        new LuaMethod("EnterSleeping", EnterSleeping),
        new LuaMethod("LeaveSleeping", LeaveSleeping),
        new LuaMethod("SetGameCam2DHeightOffset", SetGameCam2DHeightOffset),
        new LuaMethod("ReadNearCameraProfConfig", ReadNearCameraProfConfig),
        new LuaMethod("SetNearCamProfCfg", SetNearCamProfCfg),
        new LuaMethod("SetNearCamRollSensitivity", SetNearCamRollSensitivity), // 调参测试用
        new LuaMethod("EnableNearCamLookIK", EnableNearCamLookIK),
        new LuaMethod("GetGameCamDirXZ", GetGameCamDirXZ),
        new LuaMethod("StartBossCamMove", StartBossCamMove),

        new LuaMethod("EnableLightShadow", EnableLightShadow),
        new LuaMethod("EnableBloomHD", EnableBloomHD),
        new LuaMethod("FixCameraSetting", FixCameraSetting),
        new LuaMethod("EnableCastShadows", EnableCastShadows),

        new LuaMethod("SetServerOpenTime",SetServerOpenTime),
        new LuaMethod("OpenOrCloseLoginLogo",OpenOrCloseLoginLogo),
        new LuaMethod("SetUIAllowDrag",SetUIAllowDrag),
        new LuaMethod("SetTextAlignment",SetTextAlignment),
        new LuaMethod("ShowGameLogs",ShowGameLogs),
        new LuaMethod("EnableFpsPingDisplay",EnableFpsPingDisplay),
        new LuaMethod("UpdatePingDisplay",UpdatePingDisplay),
        new LuaMethod("DebugKey",DebugKey),
        new LuaMethod("LogMemoryInfo", LogMemoryInfo),
        new LuaMethod("GetCSharpUsedMemoryCount", GetCSharpUsedMemoryCount),
        new LuaMethod("SetResLoadDelay", SetResLoadDelay),
        new LuaMethod("SetNetLatency", SetNetLatency),
        new LuaMethod("GetLogLevel", GetLogLevel),
        new LuaMethod("SetLogLevel", SetLogLevel),
        new LuaMethod("MonitorGC", MonitorGC),
        new LuaMethod("GetCLRMemUseStringCur", GetCLRMemUseStringCur),
        new LuaMethod("GetCLRMemUseStringOnLastGC", GetCLRMemUseStringOnLastGC),
        new LuaMethod("GetCLRMemGCCount", GetCLRMemGCCount),
        new LuaMethod("GetRegistryTableSize", GetRegistryTableSize),
        new LuaMethod("PrintRegistryTable", PrintRegistryTable),
        new LuaMethod("GetSceneQuality", GetSceneQuality),
        new LuaMethod("GetFxLODLevel", GetFxLODLevel),
        new LuaMethod("GetScreenCurrentResolution", GetScreenCurrentResolution),
        new LuaMethod("IsEnableBloomHD", IsEnableBloomHD),
        new LuaMethod("GetMasterTextureLimit", GetMasterTextureLimit),
        new LuaMethod("EnableCmdsInputMode", EnableCmdsInputMode),
        new LuaMethod("SetRotationLerpFactor", SetRotationLerpFactor),
        new LuaMethod("DumpCSharpMemory", DumpCSharpMemory),
        new LuaMethod("GetDevelopMode",GetDevelopMode),

        new LuaMethod("ChangeMasterTextureLimit", ChangeMasterTextureLimit),
        
        new LuaMethod("GetPreferredHeight",GetPreferredHeight),
        new LuaMethod("GetABVersion",GetABVersion),
        new LuaMethod("GetClientVersion",GetClientVersion),
        new LuaMethod("OpenCamera", OpenCamera),
        new LuaMethod("OpenPhoto", OpenPhoto),
        new LuaMethod("HasCameraPermission", HasCameraPermission),
        new LuaMethod("RequestCameraPermission", RequestCameraPermission),
        new LuaMethod("HasPhotoPermission", HasPhotoPermission),
        new LuaMethod("RequestPhotoPermission", RequestPhotoPermission),
        new LuaMethod("HasRecordAudioPermission", HasRecordAudioPermission),
        new LuaMethod("RequestRecordAudioPermission", RequestRecordAudioPermission),

        new LuaMethod("ReportUserId", ReportUserId),
        new LuaMethod("ReportRoleInfo", ReportRoleInfo),
        new LuaMethod("ResetLogReporter", ResetLogReporter),
        new LuaMethod("GetNetworkStatus", GetNetworkStatus),
        new LuaMethod("GetBatteryLevel", GetBatteryLevel),
        new LuaMethod("GetBatteryStatus", GetBatteryStatus),

        new LuaMethod("GetModelHeight", GetModelHeight),
        new LuaMethod("ClearLogOutput", ClearLogOutput),
        
        new LuaMethod("CheckName_ContainMainWord", CheckName_ContainMainWord),
        new LuaMethod("CheckName_IsValidWord", CheckName_IsValidWord),
        new LuaMethod("CheckName_IsValidWord_KR", CheckName_IsValidWord_KR),
        new LuaMethod("SetLayoutElementPreferredSize", SetLayoutElementPreferredSize),
        new LuaMethod("GetScenePath", GetScenePath),
        new LuaMethod("CheckUserDataDir", CheckUserDataDir),

        new LuaMethod("SetupDynamicBones", SetupDynamicBones),

        new LuaMethod("QuitGame", QuitGame),
        new LuaMethod("ShowAlertView", ShowAlertView),
        new LuaMethod("RegisterLocalNotificationPermission", RegisterLocalNotificationPermission),
        new LuaMethod("RegisterLocalNotificationMessage", RegisterLocalNotificationMessage),
        new LuaMethod("CleanLocalNotification", CleanLocalNotification),
        
        //new LuaMethod("SetImageAlpha", SetImageAlpha),
        //new LuaMethod("SetTextAlpha", SetTextAlpha),

        new LuaMethod("DebugLogUIRT",DebugLogUIRT),
        new LuaMethod("Test", Test),
    };
    #endregion

    public static void Register(IntPtr L)
    {
        LuaScriptMgr.RegisterLib(L, "GameUtil", common_regs);
        common_regs = null;
    }

    public static IEnumerable Register2(IntPtr L)
    {
        LuaScriptMgr.CreateTable(L, "GameUtil");

        var regs = common_regs;
        for (int i = 0; i < regs.Length; i++)
        {
            LuaDLL.lua_pushstring(L, regs[i].name);
            LuaDLL.lua_pushstdcallcfunction(L, regs[i].func);
            LuaDLL.lua_rawset(L, -3);
            if (i % 200 == 0)
                yield return null;
        }

        LuaDLL.lua_settop(L, 0);
        common_regs = null;
    }    

    public static int CheckReturnNum(IntPtr L, int nTop, int nRet)
    {
        if (LuaDLL.lua_gettop(L) - nTop != nRet)
            HobaDebuger.LogErrorFormat("CheckReturnNum Failed! nTop: {0}, nRet: {1}", nTop, nRet);

        return nRet;
    }

    public static void LogParamError(string methodName, int count)
    {
        HobaDebuger.LogErrorFormat("invalid arguments to method: GameUtilWrap.{0} count: {1}", methodName, count);
    }
}
