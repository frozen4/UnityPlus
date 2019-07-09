using System.Collections;
using Common;
using UnityEngine;
using System.IO;
using System;


public class GameUpdateMan : Singleton<GameUpdateMan>
{
    private bool _IsUpdateSucceed = false;

    public bool IsUpdateSucceed
    {
        get { return _IsUpdateSucceed; }
    }

    private HotUpdateViewer _HotUpdateViewer = null;
    public HotUpdateViewer HotUpdateViewer
    {
        get { return _HotUpdateViewer; }
    }

    private bool bWifiMsgBoxShow = false;
    //private const int nMemoryMinimum = 1900;        //2G最小内存
    private const float fExtraSpace = 1.25f;       //需要的存储空间是jup大小的比率

    public string m_CurrentVersion = string.Empty;
    public string m_ServerVersion = string.Empty;

    public GameUpdateMan()
    {
        _HotUpdateViewer = new HotUpdateViewer();
    }

    public bool IsVersionCanUpdate()
    {
        return Patcher.Instance.m_CurrentVersion == Patcher.Instance.m_VersionMan.VerSeperate || Patcher.Instance.m_CurrentVersion > Patcher.Instance.m_VersionMan.VerSeperate;
    }

    public bool IsVersionComplete()
    {
        ELEMENT_VER verLatest = Patcher.Instance.m_VersionMan.VerLastest;
        return (Patcher.Instance.m_CurrentVersion == verLatest || Patcher.Instance.m_CurrentVersion > verLatest);
    }

    public long GetDownloadTotalSize()
    {
        ELEMENT_VER verBegin = Patcher.Instance.m_CurrentVersion;
        ELEMENT_VER verLatest = Patcher.Instance.m_VersionMan.VerLastest;

        //检查磁盘空间
        long packSizeOverAll = Patcher.Instance.m_VersionMan.CalcSize(verBegin, verLatest);
        return packSizeOverAll;
    }

    public void InitUpdateUI()
    {
        //设置UI语言
        _HotUpdateViewer.SetDownloadInfo(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_TextUpdate),
            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_FileSize), UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_DownloadSpeed));
        _HotUpdateViewer.SetTopTabsTitle(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_VideoTitle),
            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_PictureTitle));

        EntryPoint.Instance.PanelHotUpdate.SetActive(true);
        _HotUpdateViewer.SetAllProgress(0.0f);
        _HotUpdateViewer.SetPartProgress(0.0f);
        _HotUpdateViewer.SetDesc(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_Start));
        _HotUpdateViewer.SetCurrentVersion("0.0.0");
        _HotUpdateViewer.SetServerVersion("0.0.0");

        //新UI
        _HotUpdateViewer.SetInstallPercent(-1.0f);
        _HotUpdateViewer.SetCircle(true, UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_Start));
        //_HotUpdateViewer.SetFileDownloadInfo(0, 0, 0);
    }

    public IEnumerable UpdateRoutine()
    {        
        //检查系统内存
        /*
        if (SystemInfo.systemMemorySize < nMemoryMinimum)
        {
            yield return new WaitForUserClick(MessageBoxStyle.MB_YesNo,
                    HobaText.Format(EntryPoint.Instance.UpdateStringConfigParams.SystemRequire_Memory, nMemoryMinimum),
                    UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasError));

            if (WaitForUserClick.RetCode == 0)
            {
                Patcher.Instance.UpdateExit();
                EntryPoint.ExitGame();
                yield break;
            }
        }
         * */

        LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update); //平台SDK打点：检查更新

        yield return null;

        //更新资源，测试
        {
            string baseDir = EntryPoint.Instance.ResPath;
            string docDir = EntryPoint.Instance.DocPath;
            string libDir = EntryPoint.Instance.LibPath;
            string tmpDir = EntryPoint.Instance.TmpPath;

            string updateServerDir1 = EntryPoint.Instance.GetUpdateServerUrl1();
            string updateServerDir2 = EntryPoint.Instance.GetUpdateServerUrl2();
            string updateServerDir3 = EntryPoint.Instance.GetUpdateServerUrl3();
            string clientServerDir = EntryPoint.Instance.GetClientServerUrl();
            string dynamicServerDir = EntryPoint.Instance.GetDynamicServerUrl();
            string dynamicAccountRoleDir = EntryPoint.Instance.GetDynamicAccountRoleUrl();

            foreach (var item in Patcher.Instance.Init(baseDir, docDir, libDir, tmpDir,
                updateServerDir1, updateServerDir2, updateServerDir3, clientServerDir, dynamicServerDir, dynamicAccountRoleDir))
            {
                yield return item;
            }

            //设置初始版本
#if  UNITY_EDITOR //|| UNITY_STANDALONE_WIN
            Patcher.Instance.CleanAllUpdatesReturnToBase(Patcher.Instance.BaseVersion);
#else     
            string baseVersion = CPlatformConfig.GetBaseVersion();
            bool bWriteVersion = Patcher.Instance.SetFirstVersion(baseVersion, false);
            if (bWriteVersion)
                Patcher.Instance.CleanAllUpdatesReturnToBase(Patcher.Instance.BaseVersion);
#endif

            bool bSkipPhase2 = false;
            UpdateRetCode retCode = UpdateRetCode.success;

        UpdateStartPhase1:
            //阶段1...
            int nTryTime = 0;
            do
            {
                _HotUpdateViewer.SetInstallPercent(-1.0f);

                foreach (var item in Patcher.UpdateCoroutinePhase1())
                {
                    CUpdateInfo updateInfo = UpdateInfoUtil.GetUpdateInfo();
                    _HotUpdateViewer.SetPartProgress(updateInfo.curUpdateProgress);
                    _HotUpdateViewer.SetAllProgress(updateInfo.totalUpdateProgress);
                    _HotUpdateViewer.SetDesc(updateInfo.strUpdateInfo);

                    //新UI
                    _HotUpdateViewer.SetCurrentVersion(updateInfo.curVersion);
                    _HotUpdateViewer.SetServerVersion(updateInfo.serverVersion);

                    yield return null;

                    if (item is UpdateRetCode)
                    {
                        retCode = (UpdateRetCode)item;
                        break;
                    }

                    yield return item;
                }

                m_CurrentVersion = Patcher.Instance.m_CurrentVersion.ToString();
                m_ServerVersion = Patcher.Instance.m_VersionMan.VerLastest.ToString();

                if (retCode != UpdateRetCode.success)               //错误处理
                {
                    if (nTryTime == 0)
                    {
                        _HotUpdateViewer.SetCircle(false);
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateState_NetworkErr),
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasError));

                        UpdateInfoUtil.SetCanPlay(false);    //重试
                    }
                    else
                    {
                        LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update_Fail); //平台SDK打点：检查更新失败

                        _HotUpdateViewer.SetCircle(false);
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                        UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateState_NetConnectionErr),
                        UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasError));

#if !UNITY_EDITOR
                        Patcher.Instance.UpdateExit();
                        EntryPoint.ExitGame();
                        yield break;
#else                                                            //忽略错误，直接游戏
                        retCode = UpdateRetCode.success;
                        bSkipPhase2 = true;
                        break;
#endif
                    }

                    ++nTryTime;
                }
                else                 //成功
                {
                    if (IsVersionCanUpdate() && !IsVersionComplete())
                    {
                        long downloadSize = GetDownloadTotalSize();
                        string num = string.Empty;
                        float fMB = downloadSize / (1024.0f * 1024.0f);
                        if (fMB >= 1f)
                        {
                            fMB = ((fMB * 10) - (fMB * 10) % 1) / 10; //保留一位小数
                            if (fMB % 1 > 0)
                                //有小数点
                                num = HobaText.Format("{0:0.0} MB", fMB);
                            else
                                num = HobaText.Format("{0:0} MB", fMB);
                        }
                        else
                        {
                            float fKB = downloadSize / 1024.0f;
                            num = HobaText.Format("{0:0} KB", fKB);
                        }

                        //下载总量提示
                        _HotUpdateViewer.SetCircle(false);
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OkCancel,
                                UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateState_DownloadCheck),
                                UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_BeginUpdate),
                                UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_WifiTips),
                                num);

                        if (WaitForUserClick.RetCode == 0)
                        {
                            LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update_Fail); //平台SDK打点：检查更新失败
                            Patcher.Instance.UpdateExit();
                            EntryPoint.ExitGame();
                            yield break;
                        }

                        //存储空间检查，提示
                        long lDiskSize = OSUtility.GetFreeDiskSpace();
                        long lNeedSize = (long)(downloadSize * fExtraSpace) + 100 * 1024 * 1024;

                        Patcher.Instance.LogString(HobaText.Format("CheckDownloadSize! FreeDiskSize: {0}, NeedSize: {1}", lDiskSize, lNeedSize));

                        if (lDiskSize != 0 && lDiskSize < lNeedSize)
                        {
                            LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update_Fail); //平台SDK打点：检查更新失败

                            long iNeedSizeMB = lNeedSize / (1024 * 1024);
                            string strNoSpaceMessage = HobaText.Format(EntryPoint.Instance.UpdateStringConfigParams.SystemRequire_Space, iNeedSizeMB);
                            yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                                        strNoSpaceMessage,
                                        UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasFatalError));

                            Patcher.Instance.UpdateExit();
                            EntryPoint.ExitGame();
                            yield break;
                        }

                        _HotUpdateViewer.StartPromotion();

                    }
                    break;              //确认下载，进入下一阶段
                }
            } while (true);

            if (!bSkipPhase2 && retCode == UpdateRetCode.success)
            {
                yield return new WaitForSeconds(0.2f);

                if (!IsVersionComplete() && !bWifiMsgBoxShow)           //需要下载更新包，如果不是wifi需要提示
                {
                    if (OSUtility.IsNetAvailable() && !OSUtility.IsWifi())
                    {
                        _HotUpdateViewer.SetCircle(false);
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OkCancel,
                          UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasErrorNotWifi),
                          UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasError));

                        bWifiMsgBoxShow = true;

                        if (WaitForUserClick.RetCode == 0)
                        {
                            LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update_Fail); //平台SDK打点：检查更新失败
                            Patcher.Instance.UpdateExit();
                            EntryPoint.ExitGame();
                            yield break;
                        }
                        //继续
                    }
                }

                //阶段2...
                do
                {
                    foreach (var item in Patcher.UpdateCoroutinePhase2())
                    {
                        CUpdateInfo updateInfo = UpdateInfoUtil.GetUpdateInfo();
                        _HotUpdateViewer.SetPartProgress(updateInfo.curUpdateProgress);
                        _HotUpdateViewer.SetAllProgress(updateInfo.totalUpdateProgress);
                        _HotUpdateViewer.SetDesc(updateInfo.strUpdateInfo);
                        _HotUpdateViewer.SetCurrentVersion(updateInfo.curVersion);
                        _HotUpdateViewer.SetServerVersion(updateInfo.serverVersion);

                        yield return null;

                        if (item is UpdateRetCode)
                        {
                            retCode = (UpdateRetCode)item;
                            break;
                        }
                        else
                        {
                            yield return item;
                        }
                    }

                    if (retCode == UpdateRetCode.pack_err ||                  //包错误, 严重错误，需要清空所有更新目录重试
                        retCode == UpdateRetCode.pack_open_err ||
                        retCode == UpdateRetCode.pack_read_err ||
                        retCode == UpdateRetCode.pack_write_err)
                    {
                        _HotUpdateViewer.SetCircle(false);
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasFatalErrorNeedCleanUp),
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasFatalError));

                        yield return new WaitForSeconds(1.0f);

                        //上次写包错误，这时需要删除所有更新包，从基础版本重新更新，待测试
                        Patcher.Instance.CleanAllUpdatesReturnToBase(Patcher.Instance.BaseVersion);
                        UpdateInfoUtil.SetCanPlay(false);    //重试

                        yield return new WaitForSeconds(1.0f);

                        goto UpdateStartPhase1;       //重新开始更新，阶段1
                    }
                    else if (retCode == UpdateRetCode.patcher_version_too_new)           //版本太老，不能通过自动更新解决, 需要重新下载程序
                    {
                        _HotUpdateViewer.SetCircle(false);
#if UNITY_STANDALONE_WIN
                        //FIXME:: 原逻辑版本不一致都不能进入游戏， 策划需求Windows 特殊处理点取消进入游戏，忽略程序版本限制
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OkCancel,
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasFatalErrorNeedReinstall),
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_BeginUpdate));

                        if (WaitForUserClick.RetCode == 0)
                        {
                            retCode = UpdateRetCode.success;       //走Phase3
                            break;
                        }
                        else
                        {
                            Patcher.Instance.UpdateExit();
                            EntryPoint.ExitGame();
                            yield break;
                        }
#else
                        //FIXME:: 原逻辑
                        LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update_Fail); //平台SDK打点：检查更新失败
#if PLATFORM_KAKAO
                        while (true)
                        {
                            yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                                UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasFatalErrorNeedReinstall),
                                UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_BeginUpdate));

                            
                            string url = string.Empty;
                            if (Application.platform == RuntimePlatform.Android)
                            {
                                string id = "com.longtugame.xxaxc"; //写死
                                url = string.Format("market://details?id={0}", id);
                            }
                            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                            {
                                string id = "1073000645"; //写死
                                url = string.Format("itms-apps://itunes.apple.com/app/id{0}?action=write-review", id);
                            }
                            Patcher.Instance.LogString(string.Format("OpenUrl:{0}", url));
                            OSUtility.OpenUrl(url);  //跳转到商店

                            yield return null;
                            yield return null;
                        }
#else
                        // 非Kakao平台
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasFatalErrorNeedReinstall),
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_BeginUpdate));

                        Patcher.Instance.UpdateExit();
                        EntryPoint.ExitGame();
                        yield break;
#endif
#endif

                    }
                    else if (retCode == UpdateRetCode.pack_file_broken)         //某些安卓机型上出现解包错误，重启app继续更新即可
                    {
                        LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update_Fail); //平台SDK打点：检查更新失败

                        _HotUpdateViewer.SetCircle(false);
                        CUpdateInfo updateInfo = UpdateInfoUtil.GetUpdateInfo();
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                            updateInfo.strUpdateInfo,
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasError));

                        {
                            Patcher.Instance.UpdateExit();
#if UNITY_ANDROID
                            AndroidUtil.DoRestart(200);
#else
                            EntryPoint.ExitGame();
#endif
                            yield break;
                        }
                    }
                    else if (retCode != UpdateRetCode.success)               //普通错误处理
                    {
                        CUpdateInfo updateInfo = UpdateInfoUtil.GetUpdateInfo();
                        string msgText = string.Format("{0} {1}", updateInfo.strUpdateInfo, retCode.ToString());

                        _HotUpdateViewer.SetCircle(false);
                        yield return new WaitForUserClick(MessageBoxStyle.MB_OkCancel,
                            msgText,
                            UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_HasError));

                        if (WaitForUserClick.RetCode == 0)
                        {
#if !UNITY_EDITOR
                            LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Check_Update_Fail); //平台SDK打点：检查更新失败
                            Patcher.Instance.UpdateExit();
                            EntryPoint.ExitGame();
                            yield break;
#else
                            retCode = UpdateRetCode.success;       //走Phase3
                            break;
#endif
                        }
                        else
                        {
                            UpdateInfoUtil.SetCanPlay(false);    //重试
                        }
                    }
                    else                   //成功
                    {
                        break;
                    }

                } while (true);
            }

            Patcher.Instance.ReleasePackages();

            //第三阶段，下载服务器列表
            if (retCode == UpdateRetCode.success)
            {
                _HotUpdateViewer.SetCircle(false);
                _HotUpdateViewer.PrepareEnterGame();
                //_HotUpdateViewer.SetInstallPercent(-1.0f);

                //yield return new WaitForSeconds(0.2f);

                float progress = 0f;
                float count = 0;
                foreach (var item in Patcher.UpdateCoroutinePhase3())
                {
                    CUpdateInfo updateInfo = UpdateInfoUtil.GetUpdateInfo();
                    _HotUpdateViewer.SetPartProgress(updateInfo.curUpdateProgress);
                    _HotUpdateViewer.SetAllProgress(updateInfo.totalUpdateProgress);
                    _HotUpdateViewer.SetDesc(updateInfo.strUpdateInfo);
                    _HotUpdateViewer.SetCurrentVersion(updateInfo.curVersion);
                    _HotUpdateViewer.SetServerVersion(updateInfo.serverVersion);

                    //新UI
                    _HotUpdateViewer.SetInstallInfo(updateInfo.strUpdateInfo);
                    _HotUpdateViewer.SetEnterGameTips(updateInfo.strUpdateInfo);
                    //模拟进度条
                    _HotUpdateViewer.SetEnterGameProgress(progress);
                    float maxProgress = (count < 2) ? 0.5f : 1f;
                    progress = UnityEngine.Random.Range(progress, maxProgress);
                    count++;

                    yield return null;

                    if (item is UpdateRetCode)
                    {
                        retCode = (UpdateRetCode)item;
                        break;
                    }
                    else
                    {
                        yield return item;
                    }
                }

                if (retCode != UpdateRetCode.success)
                {
                    yield return new WaitForSeconds(1.0f);
                    UpdateInfoUtil.SetCanPlay(false);
                }
                else
                {
                    _HotUpdateViewer.SetEnterGameProgress(1f);
                    yield return new WaitForSeconds(0.3f);
                    UpdateInfoUtil.SetCanPlay(true);
                }
            }
        }

        Patcher.Instance.UpdateExit();

        if (UpdateInfoUtil.GetUpdateInfo().bCanPlay)
        {
            _IsUpdateSucceed = true;
            yield return null;
        }

        //yield return new WaitForSeconds(2.0f);
    }


#if UNITY_ANDROID

    public IEnumerable RunInstallStage(string srcDir, string destDir)
    {   
        EntryPoint.Instance.PanelHotUpdate.SetActive(true);

        //新UI
        _HotUpdateViewer.SetInstallInfo(EntryPoint.Instance.UpdateStringConfigParams.UpdateString_PrepareForFirstTimeUse);
        _HotUpdateViewer.SetInstallPercent(-1.0f);

        _HotUpdateViewer.SetAllProgress(0.0f);
        _HotUpdateViewer.SetPartProgress(0.0f);
        _HotUpdateViewer.SetDesc(EntryPoint.Instance.UpdateStringConfigParams.UpdateString_PrepareForFirstTimeUse);
        _HotUpdateViewer.SetCurrentVersion("0.0.0");
        _HotUpdateViewer.SetServerVersion("0.0.0");

        yield return null;
        yield return null;

        long lDiskSize = OSUtility.GetFreeDiskSpace();
        long lTotalSize = (long)Math.Ceiling(EntryPoint.Instance.TotalSizeToCopy * 1.1f);

        if (lDiskSize != 0 && lDiskSize < lTotalSize)         //没有足够的空间，退出
        {
            string sizeStr = string.Empty;
            long iNeedSizeMB = lTotalSize / (1024 * 1024);
            if (iNeedSizeMB >= 1f)
            {
                iNeedSizeMB = ((iNeedSizeMB * 10) - (iNeedSizeMB * 10) % 1) / 10; //保留一位小数
                if (iNeedSizeMB % 1 > 0)
                    //有小数点
                    sizeStr = HobaText.Format("{0:0.0} MB", iNeedSizeMB);
                else
                    sizeStr = HobaText.Format("{0:0} MB", iNeedSizeMB);
            }
            else
            {
                long iNeedSizeKB = lTotalSize / 1024;
                sizeStr = HobaText.Format("{0:0} KB", iNeedSizeKB);
            }

            _HotUpdateViewer.SetCircle(false);
            yield return new WaitForUserClick(MessageBoxStyle.MB_OK,
                        EntryPoint.Instance.UpdateStringConfigParams.UpdateString_EnsureEnoughSpaceMB,
                        EntryPoint.Instance.UpdateStringConfigParams.UpdateState_DiskSpaceFullErr,
                        string.Empty,
                        sizeStr);

            EntryPoint.ExitGame();
            yield break;
        }

        lTotalSize = EntryPoint.Instance.TotalSizeToCopy;
        bool succeed = false;
        float fLastProgress = 0.0f;
        var listFile = EntryPoint.Instance.ListFilesToCopy;
        if (lTotalSize > 0 && listFile != null)
        {
            long sizeFinished = 0;
            for (int i = 0; i < listFile.Count; ++i)
            {
                long size = CStreamingAssetHelper.CopyAssetFileToPath(listFile[i], srcDir, destDir);
                if (size == -1)
                {
                    DeviceLogger.Instance.WriteLog(string.Format("RunInstallStage CopyAssetFileToPath Failed! {0}", listFile[i]));
                    continue;
                }

                sizeFinished += size;

                float progress = (float)sizeFinished / lTotalSize;

                if (Util.IsZero(fLastProgress - progress))
                {
                    fLastProgress = progress;
                    _HotUpdateViewer.SetAllProgress(progress);
                    _HotUpdateViewer.SetPartProgress(progress);
                    _HotUpdateViewer.SetInstallPercent(progress);

                    yield return null;
                    yield return null;
                }
            }
            succeed = (sizeFinished == lTotalSize);
        }
        else
        {
            succeed = true;
        }

        EntryPoint.Instance.IsInstallFinished = succeed;
        yield return new WaitForSeconds(0.5f);
    }
#endif

}