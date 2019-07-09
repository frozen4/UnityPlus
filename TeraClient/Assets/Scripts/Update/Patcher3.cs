using Downloader;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//更新相对整体的功能
public partial class Patcher
{
    //第一阶段更新，取得本地版本和服务器版本，下载服务器版本信息
    public static IEnumerable UpdateCoroutinePhase1()
    {
        //初始化
        UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.Total, 0.0f);
        UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 0.0f);
        UpdateInfoUtil.SetCanPlay(false);

        //取得本地版本
        UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_TryGetLocalVersion);
        yield return null;

        var code = Patcher.Instance.GetLocalVersion();
        if (code != UpdateRetCode.success)       //无法取得本地版本
        {
            UpdateInfoUtil.SetCanPlay(false);
            yield return new WaitForSeconds(1.0f);

            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_GetLocalVersionErr);

            Patcher.Instance.LogString("GetLocalVersion Failed!");
            yield return code;
        }
        else
        {
            //设置本地版本
            UpdateInfoUtil.SetVersion(UPDATE_VERSION.Local, Patcher.Instance.m_CurrentVersion);
        }

        //log
        Patcher.Instance.LogString(HobaText.Format("Client Current Version: {0}", Patcher.Instance.m_CurrentVersion.ToString()));
        Patcher.Instance.LogString(HobaText.Format("FreeDiskSpace: {0}", OSUtility.GetFreeDiskSpace()));

        //检查磁盘空间
        /*
        UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_TryCheckFreeSpace);
        yield return null;
        code = Patcher.Instance.CheckDiskFreeSpace(nCheckFreeSpaceMB * 1024 * 1024);
        if (code != UpdateRetCode.success)
        {
            UpdateInfoUtil.SetCanPlay(false);
            yield return new WaitForSeconds(1.0f);

            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_DiskSpaceFullErr);

            Patcher.Instance.LogString("CheckDiskFreeSpace Failed!");
            yield return code;
        }
        */

        //test
        /*
        string url = "https://www.google.com";
        string hostName = UpdateUtility.GetHostName(url);
        string errMsg1;
        var code1 = UpdateUtility.GetByUrl(url, hostName, System.IO.Path.Combine(EntryPoint.Instance.DocPath, "google.txt"), 10, null, null, out errMsg1);
        Patcher.Instance.LogString(HobaText.Format("Download Test: {0} {1} {2}", url, code1, errMsg1));
        */

        if (!EntryPoint.Instance.SkipUpdate)             //配置跳过更新
        {
            //DNS解析, 忽略解析错误
            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_TryDNSResolving);
            yield return null;
            string errUpdateServer;
            string errClientServer;
            code = Patcher.Instance.DNSResolving(2, out errUpdateServer, out errClientServer);
            if (!string.IsNullOrEmpty(errUpdateServer))
                Patcher.Instance.LogString("UpdateServer DNSResolving Error : " + errUpdateServer);
            if (!string.IsNullOrEmpty(errClientServer))
                Patcher.Instance.LogString("errClientServer DNSResolving Error : " + errClientServer);

            Patcher.Instance.LogString(string.Format("IP URL UpdateServer1: {0}, UpdateServer2: {1}, UpdateServer3: {2}",
                Patcher.Instance.strUpdateServerDir1, Patcher.Instance.strUpdateServerDir2, Patcher.Instance.strUpdateServerDir3));
            Patcher.Instance.LogString(string.Format("IP URL ClientServer: {0}", Patcher.Instance.strClientServerDir));
            Patcher.Instance.LogString(string.Format("IP URL DynamicServer : {0}", Patcher.Instance.strDynamicServerDir));
            Patcher.Instance.LogString(string.Format("IP URL DynamicAccountRole : {0}", Patcher.Instance.strDynamicAccountRoleDir));

            //从服务器获取version.txt
            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_TryGetNewVersion);
            yield return null;

            string errMsg = "";
            string savedFile = Patcher.Instance.strGameNewVerFile;

            DownloadTaskErrorCode dlCode = DownloadTaskErrorCode.Unknown;
            {
                //为了方便部署，内网version.txt用和更新资源jup同一目录，也就是cdn
                if (dlCode != DownloadTaskErrorCode.Success && !string.IsNullOrEmpty(Patcher.Instance.strUpdateServerDir1))
                {
                    string url1 = Patcher.Instance.strUpdateServerDir1 + UpdateConfig.VersionConfigRelativePath;
                    string hostName1 = Patcher.Instance.strUpdateServerHostName1;

                    int nTryConnect = 0;            //尝试多次连接
                    while (dlCode != DownloadTaskErrorCode.Success && nTryConnect < DownloadMan.maxTryConnect)
                    {
                        ++nTryConnect;
                        int timeout = (int)((float)DownloadMan.reqTimeOut * nTryConnect / DownloadMan.maxTryConnect);
                        dlCode = Patcher.Instance.FetchServerVersionFile(url1, hostName1, savedFile, timeout, out errMsg);
                    }
                }

                if (dlCode != DownloadTaskErrorCode.Success && !string.IsNullOrEmpty(Patcher.Instance.strUpdateServerDir2))
                {
                    string url2 = Patcher.Instance.strUpdateServerDir2 + UpdateConfig.VersionConfigRelativePath;
                    string hostName2 = Patcher.Instance.strUpdateServerHostName2;

                    int nTryConnect = 0;            //尝试多次连接
                    while (dlCode != DownloadTaskErrorCode.Success && nTryConnect < DownloadMan.maxTryConnect)
                    {
                        ++nTryConnect;
                        int timeout = (int)((float)DownloadMan.reqTimeOut * nTryConnect / DownloadMan.maxTryConnect);
                        dlCode = Patcher.Instance.FetchServerVersionFile(url2, hostName2, savedFile, DownloadMan.reqTimeOut, out errMsg);
                    }
                }

                if (dlCode != DownloadTaskErrorCode.Success && !string.IsNullOrEmpty(Patcher.Instance.strUpdateServerDir3))
                {
                    string url3 = Patcher.Instance.strUpdateServerDir3 + UpdateConfig.VersionConfigRelativePath;
                    string hostName3 = Patcher.Instance.strUpdateServerHostName3;

                    int nTryConnect = 0;            //尝试多次连接
                    while (dlCode != DownloadTaskErrorCode.Success && nTryConnect < DownloadMan.maxTryConnect)
                    {
                        ++nTryConnect;
                        int timeout = (int)((float)DownloadMan.reqTimeOut * nTryConnect / DownloadMan.maxTryConnect);
                        dlCode = Patcher.Instance.FetchServerVersionFile(url3, hostName3, savedFile, DownloadMan.reqTimeOut, out errMsg);
                    }
                }
            }
            

            if (dlCode != DownloadTaskErrorCode.Success)
            {
                UpdateInfoUtil.SetCanPlay(false);
                yield return new WaitForSeconds(1.0f);

                UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_GetNewVersionErr);

                if (dlCode == DownloadTaskErrorCode.Md5Dismatch)
                    code = UpdateRetCode.md5_not_match;
                else
                    code = UpdateRetCode.download_fail;

                Patcher.Instance.LogString("FetchServerVersionFile Failed! DownloadTaskErrorCode: " + dlCode + ": " + errMsg);

                yield return code;
            }

            //读取游戏新版本信息
            code = Patcher.Instance.TryGetLatestVersionFromServer();
        }
        else
        {
            //读取游戏新版本信息
            code = UpdateRetCode.success;
        }

        if (code != UpdateRetCode.success)
        {
            UpdateInfoUtil.SetCanPlay(false);
            yield return new WaitForSeconds(1.0f);

            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_GetNewVersionErr);

            Patcher.Instance.LogString("GetVersionMan Failed!");
            yield return code;
        }
        else
        {
            //设置服务器版本
            UpdateInfoUtil.SetVersion(UPDATE_VERSION.Server, Patcher.Instance.m_VersionMan.VerLastest);
        }

        //log
        Patcher.Instance.LogString(HobaText.Format("Server Version: {0}", Patcher.Instance.m_VersionMan.VerLastest.ToString()));

        UpdateInfoUtil.SetDownloadTotalSize(GameUpdateMan.Instance.GetDownloadTotalSize());

        UpdateInfoUtil.SetCanPlay(false);
        UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_SuccessGetVersions);
        yield return code;
    }

    //第二阶段更新，下载更新包
    public static IEnumerable UpdateCoroutinePhase2()
    {
        UpdateRetCode code = UpdateRetCode.success;

        if (EntryPoint.Instance.SkipUpdate)             //配置跳过更新
        {
            yield return code;
        }

        if (Patcher.Instance.IsWritingPackFileExist())
        {
            code = UpdateRetCode.pack_err;
            UpdateInfoUtil.SetCanPlay(false);

            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_LastWritePackErr);

            Patcher.Instance.LogString("Last IsWritingPackFileExist!");
            yield return code;
        }

        //初始化pck环境
        code = Patcher.Instance.InitPackages();
        if (code != UpdateRetCode.success)
        {
            Patcher.Instance.ReleasePackages();     //释放pck环境

            UpdateInfoUtil.SetCanPlay(false);
            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_OpenPackFileErr);

            Patcher.Instance.LogString("InitPackages Failed!");
            yield return code;
        }

        //清除写包标志，如果成功打开包
        Patcher.Instance.WritePacking(false);

        yield return null;

        //
        ELEMENT_VER verBegin = Patcher.Instance.m_CurrentVersion;
        ELEMENT_VER verLatest = Patcher.Instance.m_VersionMan.VerLastest;

        //检查磁盘空间
        long packSizeOverAll = Patcher.Instance.m_VersionMan.CalcSize(verBegin, verLatest);
        code = Patcher.Instance.CheckDiskFreeSpace(packSizeOverAll + 100 * 1024 * 1024);
        if (code != UpdateRetCode.success)
        {
            UpdateInfoUtil.SetCanPlay(false);
            yield return new WaitForSeconds(1.0f);

            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_DiskSpaceFullErr);

            Patcher.Instance.LogString("CheckDiskFreeSpace Failed!");
            yield return code;
        }

        while (true)
        {
            if (Patcher.Instance.m_CurrentVersion == verLatest ||                            //更新完成
                Patcher.Instance.m_CurrentVersion > verLatest)
            {
                Patcher.Instance.ReleasePackages();

                code = UpdateRetCode.success;

                UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.Total, 1.0f);
                UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 1.0f);

                UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_Success);
                yield return code;            //退出coroutine
            }
            else if (Patcher.Instance.m_CurrentVersion < Patcher.Instance.m_VersionMan.VerSeperate)                          //本地版本过旧
            {
                Patcher.Instance.ReleasePackages();

                code = UpdateRetCode.patcher_version_too_new;

                UpdateInfoUtil.SetCanPlay(false);
                yield return new WaitForSeconds(1.0f);

                UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_LocalVersionTooOldErr);
                yield return code;                //退出coroutine
            }

            //找到版本pair
            code = Patcher.Instance.FindVersionPair();
            if (code != UpdateRetCode.success)
            {
                Patcher.Instance.ReleasePackages();

                UpdateInfoUtil.SetCanPlay(false);
                yield return new WaitForSeconds(1.0f);

                UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_ServerMaintananceErr);

                Patcher.Instance.LogString("FindVersionPair Failed!");
                yield return code;               //退出coroutine
            }
            else
            {
                //当前version_pair
                Patcher.Instance.LogString(HobaText.Format("Will update from {0} to {1}", Patcher.Instance.m_PackFileVer.VerFrom.ToString(), Patcher.Instance.m_PackFileVer.VerTo.ToString()));
            }

            yield return null;

            //更新
            foreach (var item in Patcher.Instance.UpdateAutoCoroutine(verBegin, verLatest))
            {
                if (item is UpdateRetCode)
                {
                    code = (UpdateRetCode)item;
                    break;
                }

                yield return item;
            }
            if (code != UpdateRetCode.success)
            {
                break;
            }
        }

        switch (code)
        {
            case UpdateRetCode.success:
                {
                    UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.Total, 1.0f);
                    UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 1.0f);
                    yield return null;
                }
                break;
            case UpdateRetCode.md5_not_match:
                {
                    Patcher.Instance.LogString(HobaText.Format("AutoUpdate Failed! {0}", code));
                    UpdateInfoUtil.SetCanPlay(false);
                    UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_NetUnstable);
                    yield return null;
                }
                break;
            case UpdateRetCode.connect_fail:
                {
                    Patcher.Instance.LogString(HobaText.Format("AutoUpdate Failed! {0}", code));
                    UpdateInfoUtil.SetCanPlay(false);
                    UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_NetConnectionErr);
                    yield return null;
                }
                break;
            default:
                {
                    Patcher.Instance.LogString(HobaText.Format("AutoUpdate Failed! {0}", code));
                    UpdateInfoUtil.SetCanPlay(false);
                    UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_AutoUpdateErr);
                    yield return null;
                }
                break;
        }

        Patcher.Instance.ReleasePackages();     //释放pck环境
        Patcher.Instance.CleanPatcherTmpDir();      //清除Tmp目录

        yield return code;
    }

    //第三阶段更新，下载服务器列表，(废弃，服务器列表动态获取)
    public static IEnumerable UpdateCoroutinePhase3()
    {
        if (EntryPoint.Instance.SkipUpdate)             //配置跳过更新
        {
            yield return UpdateRetCode.success;
        }

        yield return null;

        /*
        //下载公告, 错误不处理
        {
            Patcher.Instance.LogString("FetchAnnoucementFile url: " + EntryPoint.Instance.GetAnnoucementUrl());

            DownloadTaskErrorCode dlCode2 = DownloadTaskErrorCode.Unknown;
            string errMsg2 = "";
            int nTryConnect2 = 0;            //尝试多次连接
            while (dlCode2 != DownloadTaskErrorCode.Success && nTryConnect2 < DownloadMan.maxTryConnect)
            {
                ++nTryConnect2;
                dlCode2 = Patcher.Instance.FetchAnnoucementFile((int)(DownloadMan.reqTimeOut * nTryConnect2 / DownloadMan.maxTryConnect), out errMsg2);
            }

            if (dlCode2 != DownloadTaskErrorCode.Success)
            {
                Patcher.Instance.LogString("FetchAnnoucementFile Failed! DownloadTaskErrorCode: " + dlCode2 + ": " + errMsg2);
            }
            else
            {
                Patcher.Instance.LogString("FetchAnnoucementFile Succeed!");
            }
        }

        yield return null;
         * */

        //RefreshServerList();
    }

    //删除所有更新文件，返回到基础版本
    public bool CleanAllUpdatesReturnToBase(ELEMENT_VER baseVer)
    {
        this.ReleasePackages();     //释放pck环境

        LuaDLL.HOBA_DeleteFilesInDirectory(this.strLibDir);

        bool ret1 = LuaDLL.HOBA_HasFilesInDirectory(this.strLibDir);
        if (ret1)
        {
            this.LogString(HobaText.Format("[CleanAllUpdatesReturnToBase] Cannot Delete Files in {0}", this.strLibDir));
        }

        this.SetFirstVersion(baseVer, true);

        return true;
    }

    public bool CleanPatcherTmpDir()
    {
        LuaDLL.HOBA_DeleteFilesInDirectory(this.strLibPatcherTmpDir);

        bool ret2 = LuaDLL.HOBA_HasFilesInDirectory(this.strLibPatcherTmpDir);
        if (ret2)
        {
            this.LogString(HobaText.Format("[CleanAllUpdatesReturnToBase] Cannot Delete Files in {0}", this.strLibPatcherTmpDir));
        }

        return true;
    }
}