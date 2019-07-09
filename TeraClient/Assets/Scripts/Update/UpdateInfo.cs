using System;
using System.Collections.Generic;
using System.Text;

public enum UPDATE_STATE
{
    UpdateState_None = 0,
    UpdateState_Success,
    UpdateState_UpdateVersionFileErr,
    UpdateState_OpenPackFileErr,
    UpdateState_DiskSpaceFullErr,
    UpdateState_GetLocalVersionErr,
    UpdateState_GetNewVersionErr,
    UpdateState_NetworkErr,
    UpdateState_NetConnectionErr,
    UpdateState_NetUnstable,
    UpdateState_DownloadCheck,
    UpdateState_LocalVersionTooOldErr,
    UpdateState_ServerMaintananceErr,
    UpdateState_AutoUpdateErr,
    UpdateState_UnknownErr,  
    UpdateState_DownloadingErrAutoRetry,

    UpdateStatus_Start,
    UpdateStatus_TryGetLocalVersion,
    UpdateStatus_TryCheckFreeSpace,
    UpdateStatus_TryDNSResolving,
    UpdateStatus_TryGetNewVersion,
    UpdateStatus_SuccessGetVersions,
    UpdateStatus_BeginUpdate,
    UpdateStatus_LastWritePackErr,
    UpdateStatus_Downloading,
    UpdateStatus_WritingPack, 
    UpdateStatus_DownloadingServerList,
    UpdateStatus_CheckingExistPack,
    UpdateStatus_PrepareWritingPack,

    UpdateString_CurrentVersion,
    UpdateString_ServerVersion,
    UpdateString_Yes,
    UpdateString_No,
    UpdateString_Ok,
    UpdateString_Cancel,
    UpdateString_HasErrorNotWifi,
    UpdateString_HasErrorRetry,
    UpdateString_HasError,
    UpdateString_HasFatalErrorNeedReinstall,
    UpdateString_HasFatalErrorNeedCleanUp,
    UpdateString_HasFatalError,
    UpdateString_TextUpdate,
    UpdateString_FileSize,
    UpdateString_DownloadSpeed,
    UpdateString_WifiTips,
    UpdateString_VideoTitle,
    UpdateString_PictureTitle,
}

//给UI显示的总Update信息
public sealed class CUpdateInfo
{
    public CUpdateInfo()
    {
        curUpdateProgress = 0.0f;
        totalUpdateProgress = 0.0f;
        curUpdateFileSize = 0;
        totalUpdateFileSize = 0;
        curVersion = "";
        serverVersion = "";
        strUpdateInfo = "";
        bCanPlay = false;
        bWrittingPack = false;
    }

    public float curUpdateProgress;
    public float totalUpdateProgress;
    public long curUpdateFileSize;          //当前下载总量
    public long totalUpdateFileSize;       //更新文件总量
    public string curVersion;
    public string serverVersion;
    public string strUpdateInfo;        //对外展示的更新信息
    public bool bCanPlay;
    public bool bWrittingPack;      //正在写包状态，如果停止可能把包写坏
}

public enum UPDATE_PROGRESS
{
    File,
    Total,
}

public enum UPDATE_VERSION
{
    Local,
    Server,
}

//专门写patcher里的UpdateInfo
public static partial class UpdateInfoUtil
{
    public static bool bShowProgress = false;
    public static bool bShowWritingPack = false;

    public static string GetStateString(UPDATE_STATE state)
    {
        var updateStringConfig = EntryPoint.Instance.UpdateStringConfigParams;
        string strStatus = "";
        if (updateStringConfig != null)
        {
            switch (state)
            {
                case UPDATE_STATE.UpdateState_None:
                    strStatus = "";
                    break;
                case UPDATE_STATE.UpdateState_Success:
                    strStatus = updateStringConfig.UpdateState_Success;
                    break;
                case UPDATE_STATE.UpdateState_UpdateVersionFileErr:
                    strStatus = updateStringConfig.UpdateState_UpdateVersionFileErr;
                    break;
                case UPDATE_STATE.UpdateState_OpenPackFileErr:
                    strStatus = updateStringConfig.UpdateState_OpenPackFileErr;
                    break;
                case UPDATE_STATE.UpdateState_DiskSpaceFullErr:
                    strStatus = updateStringConfig.UpdateState_DiskSpaceFullErr;
                    break;
                case UPDATE_STATE.UpdateState_GetLocalVersionErr:
                    strStatus = updateStringConfig.UpdateState_GetLocalVersionErr;
                    break;
                case UPDATE_STATE.UpdateState_GetNewVersionErr:
                    strStatus = updateStringConfig.UpdateState_GetNewVersionErr;
                    break;
                case UPDATE_STATE.UpdateState_NetworkErr:
                    strStatus = updateStringConfig.UpdateState_NetworkErr;
                    break;
                case UPDATE_STATE.UpdateState_NetConnectionErr:
                    strStatus = updateStringConfig.UpdateState_NetConnectionErr;
                    break;
                case UPDATE_STATE.UpdateState_NetUnstable:
                    strStatus = updateStringConfig.UpdateState_NetUnstable;
                    break;
                case UPDATE_STATE.UpdateState_DownloadCheck:
                    strStatus = updateStringConfig.UpdateState_DownloadCheck;
                    break;
                case UPDATE_STATE.UpdateState_LocalVersionTooOldErr:
                    strStatus = updateStringConfig.UpdateState_LocalVersionTooOldErr;
                    break;
                case UPDATE_STATE.UpdateState_ServerMaintananceErr:
                    strStatus = updateStringConfig.UpdateState_ServerMaintananceErr;
                    break;
                case UPDATE_STATE.UpdateState_AutoUpdateErr:
                    strStatus = updateStringConfig.UpdateState_AutoUpdateErr;
                    break;
                case UPDATE_STATE.UpdateState_UnknownErr:
                    strStatus = updateStringConfig.UpdateState_UnknownErr;
                    break;
                case UPDATE_STATE.UpdateState_DownloadingErrAutoRetry:
                    strStatus = updateStringConfig.UpdateState_DownloadingErrAutoRetry;
                    break;
                case UPDATE_STATE.UpdateStatus_Start:
                    strStatus = updateStringConfig.UpdateStatus_Start;
                    break;
                case UPDATE_STATE.UpdateStatus_TryGetLocalVersion:
                    strStatus = updateStringConfig.UpdateStatus_TryGetLocalVersion;
                    break;
                case UPDATE_STATE.UpdateStatus_TryCheckFreeSpace:
                    strStatus = updateStringConfig.UpdateStatus_TryCheckFreeSpace;
                    break;
                case UPDATE_STATE.UpdateStatus_TryDNSResolving:
                    strStatus = updateStringConfig.UpdateStatus_TryDNSResolving;
                    break;
                case UPDATE_STATE.UpdateStatus_TryGetNewVersion:
                    strStatus = updateStringConfig.UpdateStatus_TryGetNewVersion;
                    break;
                case UPDATE_STATE.UpdateStatus_SuccessGetVersions:
                    strStatus = updateStringConfig.UpdateStatus_SuccessGetVersions;
                    break;
                case UPDATE_STATE.UpdateStatus_BeginUpdate:
                    strStatus = updateStringConfig.UpdateStatus_BeginUpdate;
                    break;
                case UPDATE_STATE.UpdateStatus_LastWritePackErr:
                    strStatus = updateStringConfig.UpdateStatus_LastWritePackErr;
                    break;
                case UPDATE_STATE.UpdateStatus_Downloading:
                    strStatus = updateStringConfig.UpdateStatus_Downloading;
                    break;
                case UPDATE_STATE.UpdateStatus_WritingPack:
                    strStatus = updateStringConfig.UpdateStatus_WritingPack;
                    break;
                case UPDATE_STATE.UpdateStatus_DownloadingServerList:
                    strStatus = updateStringConfig.UpdateStatus_DownloadingServerList;
                    break;
                case UPDATE_STATE.UpdateStatus_CheckingExistPack:
                    strStatus = updateStringConfig.UpdateStatus_CheckingExistPack;
                    break;
                case UPDATE_STATE.UpdateStatus_PrepareWritingPack:
                    strStatus = updateStringConfig.UpdateStatus_PrepareWritingPack;
                    break;
                case UPDATE_STATE.UpdateString_CurrentVersion:
                    strStatus = updateStringConfig.UpdateString_CurrentVersion;
                    break;
                case UPDATE_STATE.UpdateString_ServerVersion:
                    strStatus = updateStringConfig.UpdateString_ServerVersion;
                    break;
                case UPDATE_STATE.UpdateString_HasErrorNotWifi:
                    strStatus = updateStringConfig.UpdateString_HasErrorNotWifi;
                    break;
                case UPDATE_STATE.UpdateString_HasErrorRetry:
                    strStatus = updateStringConfig.UpdateString_HasErrorRetry;
                    break;
                case UPDATE_STATE.UpdateString_Yes:
                    strStatus = updateStringConfig.UpdateString_Yes;
                    break;
                case UPDATE_STATE.UpdateString_No:
                    strStatus = updateStringConfig.UpdateString_No;
                    break;
                case UPDATE_STATE.UpdateString_Ok:
                    strStatus = updateStringConfig.UpdateString_Ok;
                    break;
                case UPDATE_STATE.UpdateString_Cancel:
                    strStatus = updateStringConfig.UpdateString_Cancel;
                    break;
                case UPDATE_STATE.UpdateString_HasError:
                    strStatus = updateStringConfig.UpdateString_HasError;
                    break;
                case UPDATE_STATE.UpdateString_HasFatalErrorNeedReinstall:
                    strStatus = updateStringConfig.UpdateString_HasFatalErrorNeedReinstall;
                    break;
                case UPDATE_STATE.UpdateString_HasFatalErrorNeedCleanUp:
                    strStatus = updateStringConfig.UpdateString_HasFatalErrorNeedCleanUp;
                    break;
                case UPDATE_STATE.UpdateString_HasFatalError:
                    strStatus = updateStringConfig.UpdateString_HasFatalError;
                    break;
                case UPDATE_STATE.UpdateString_TextUpdate:
                    strStatus = updateStringConfig.UpdateString_TextUpdate;
                    break;
                case UPDATE_STATE.UpdateString_FileSize:
                    strStatus = updateStringConfig.UpdateString_FileSize;
                    break;
                case UPDATE_STATE.UpdateString_DownloadSpeed:
                    strStatus = updateStringConfig.UpdateString_DownloadSpeed;
                    break;
                case UPDATE_STATE.UpdateString_WifiTips:
                    strStatus = updateStringConfig.UpdateString_WifiTips;
                    break;
                case UPDATE_STATE.UpdateString_VideoTitle:
                    strStatus = updateStringConfig.UpdateString_VideoTitle;
                    break;
                case UPDATE_STATE.UpdateString_PictureTitle:
                    strStatus = updateStringConfig.UpdateString_PictureTitle;
                    break;
                default:
                    strStatus = "";
                    break;
            }
        }

        return strStatus;
    }

    public static void SetDownStatusString(float progress)
    {
        if (bShowProgress)
        {
            SetStatus(HobaText.Format("{0}: {1:P}", GetStateString(UPDATE_STATE.UpdateStatus_Downloading), progress));
        }
    }

    public static void SetWritingPackStatusString(float progress)
    {
        if (bShowWritingPack)
        {
            if(progress == 0.0f)
                SetStatus(HobaText.Format("{0}", GetStateString(UPDATE_STATE.UpdateStatus_PrepareWritingPack)));
            else
                SetStatus(HobaText.Format("{0}: {1:P}", GetStateString(UPDATE_STATE.UpdateStatus_WritingPack), progress));
        }
    }

    public static CUpdateInfo GetUpdateInfo()
    {
        return Patcher.Instance.UpdateInfo;
    }

    public static void AddDownloadedSize(long lsize)
    {
        Patcher.Instance.UpdateInfo.curUpdateFileSize += lsize;
    }

    public static void SetDownloadTotalSize(long lsize)
    {
        Patcher.Instance.UpdateInfo.totalUpdateFileSize = lsize;
    }
 
    public static void SetStateString(UPDATE_STATE state)
    {
        SetStatus(GetStateString(state));
    }

    public static void SetVersion(UPDATE_VERSION which, ELEMENT_VER version)
    {
        if(which == UPDATE_VERSION.Local)
        {
            Patcher.Instance.UpdateInfo.curVersion = version.ToString();
        }
        else if(which == UPDATE_VERSION.Server)
        {
            Patcher.Instance.UpdateInfo.serverVersion = version.ToString();
        }
    }

    public static void SetProgress(UPDATE_PROGRESS which, float progress)
    {
        if (which == UPDATE_PROGRESS.Total)
        {
            Patcher.Instance.UpdateInfo.totalUpdateProgress = progress;
        }
        else if(which == UPDATE_PROGRESS.File)
        {
            Patcher.Instance.UpdateInfo.curUpdateProgress = progress;
        }
    }

    public static void SetStatus(string strMsg)
    {
        Patcher.Instance.UpdateInfo.strUpdateInfo = strMsg;
    }

    public static void SetCanPlay(bool bCanPlay)
    {
        Patcher.Instance.UpdateInfo.bCanPlay = bCanPlay;
    }

}
