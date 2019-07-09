#define USE_DNS_RESOLVE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Downloader;
using System.Runtime.InteropServices;

//更新相对独立的功能
public partial class Patcher
{
    private ELEMENT_VER m_currentNewVer = new ELEMENT_VER();
    private ELEMENT_VER m_baseVer = new ELEMENT_VER();

    public ELEMENT_VER BaseVersion
    {
        get { return m_baseVer; }
    }

    //如果本地文件不存在，设置为初始版本, 如果当前版本 < 初始版本，则写新版本, ret是否新写入baseVersion
    public bool SetFirstVersion(ELEMENT_VER ver, bool bForceWrite)
    {
        m_baseVer = ver;

        if (bForceWrite || !FileOperate.IsFileExist(strGameOldVerFile))
        {
            if(!FileOperate.MakeDir(strGameOldVerFile))
            {
                LogString(HobaText.Format("[SetFirstVersion] MakeDir {0} Failed!", strGameOldVerFile));
            }
            UpdateRetCode ret = SetLocalVersion(ver);
            if(ret != UpdateRetCode.success)
            {
                LogString(HobaText.Format("[SetFirstVersion] SetLocalVersion {0} Failed!", strGameOldVerFile));
            }
            else
            {
                return true;
            }
        }
        else
        {
            ELEMENT_VER localVersion;
            if (GetLocalVersion(out localVersion) && localVersion < m_baseVer)
            {
                LogString(HobaText.Format("[SetFirstVersion] Local Version File Exist {0}! Write New Version From: {1} To {2}", strGameOldVerFile, localVersion.ToString(), ver.ToString()));

                if (!FileOperate.MakeDir(strGameOldVerFile))
                {
                    LogString(HobaText.Format("[SetFirstVersion] MakeDir {0} Failed2!", strGameOldVerFile));
                }
                UpdateRetCode ret = SetLocalVersion(ver);
                if (ret != UpdateRetCode.success)
                {
                    LogString(HobaText.Format("[SetFirstVersion] SetLocalVersion {0} Failed2!", strGameOldVerFile));
                }
                else
                {
                    return true;
                }
            }
            else
            {
                LogString(HobaText.Format("[SetFirstVersion] Local Version File Exist {0}!", strGameOldVerFile));
            }
        }

        return false;
    }

    public bool SetFirstVersion(string strVer, bool bForceWrite)
    {
        ELEMENT_VER ver = new ELEMENT_VER();
        if (!ver.Parse(strVer))
        {
            LogString(HobaText.Format("[SetFirstVersion] Version Parse Failed! {0}", strVer));
            ver.Set(0, 0, 0, 0);
        }

        return SetFirstVersion(ver, bForceWrite);
    }

    //写本地版本文件
    public UpdateRetCode SetLocalVersion(ELEMENT_VER ver)
    {
        try
        {
            StreamWriter writer = FileOperate.CreateTextFile(strGameOldVerFile);
            if (writer == null)
                return UpdateRetCode.file_err;

            writer.WriteLine(ver.ToString());
            writer.Close();
        }
        catch(Exception)
        {
            return UpdateRetCode.file_err;
        }
        
        return UpdateRetCode.success;
    }

    private bool GetLocalVersion(out ELEMENT_VER version)
    {
        bool success = false;
        version = new ELEMENT_VER(0, 0, 0, 0);
        string strLine;

        try
        {
            StreamReader reader = FileOperate.OpenTextFile(strGameOldVerFile);
            if (reader == null)
            {
                goto END;
            }

            strLine = reader.ReadLine();
            reader.Close();

            if (strLine != null)
            {
                if(version.Parse(strLine))
                {
                    success = true;
                }
            }
        }
        catch(Exception)
        {
            goto END;
        }
END:
        return success;
    }

    //从本地版本文件读取当前版本
    public UpdateRetCode GetLocalVersion()
    {
        string strLine = null;

        try
        {
            StreamReader reader = FileOperate.OpenTextFile(strGameOldVerFile);
            if (reader == null)
                return UpdateRetCode.element_no_ver_file;

            strLine = reader.ReadLine();
            if (strLine == null)
            {
                reader.Close();
                return UpdateRetCode.file_read_err;
            }

            reader.Close();
        }
        catch(Exception)
        {
            return UpdateRetCode.fail;
        }
       
        ELEMENT_VER ver = new ELEMENT_VER(0, 0, 0, 0);
        if (!ver.Parse(strLine))
            return UpdateRetCode.fail;

        m_CurrentVersion = ver;
        return UpdateRetCode.success;
    }

    //检查磁盘空间
    public UpdateRetCode CheckDiskFreeSpace(long lCheckSize)
    {
        long lFreeDiskSpace = OSUtility.GetFreeDiskSpace();
        if (lFreeDiskSpace != 0 && lFreeDiskSpace < lCheckSize)
            return UpdateRetCode.disk_no_space;

        return UpdateRetCode.success;
    }

    public UpdateRetCode DNSResolving(int nTryTimes, out string errUpdateServer, out string errClientServer)
    {
        errUpdateServer = string.Empty;
        errClientServer = string.Empty;

        //resolve UpdateServer1
        string strUrl = strUpdateServerDir1;
#if USE_DNS_RESOLVE
        DNSResolving(nTryTimes, strUpdateServerHostName1, ref strUrl, out errUpdateServer);
#endif 
        strUpdateServerDir1 = strUrl;

        //resolve UpdateServer2
        strUrl = strUpdateServerDir2;
#if USE_DNS_RESOLVE
        DNSResolving(nTryTimes, strUpdateServerHostName2, ref strUrl, out errUpdateServer);
#endif
        strUpdateServerDir2 = strUrl;

        //resolve UpdateServer3
        strUrl = strUpdateServerDir3;
#if USE_DNS_RESOLVE
        DNSResolving(nTryTimes, strUpdateServerHostName3, ref strUrl, out errUpdateServer);
#endif
        strUpdateServerDir3 = strUrl;

        //resolve ClientServer
        strUrl = strClientServerDir;
#if USE_DNS_RESOLVE
        DNSResolving(nTryTimes, strClientServerHostName, ref strUrl, out errClientServer);
#endif 
        strClientServerDir = strUrl;

        return UpdateRetCode.success;
    }

    public static bool DNSResolving(int nTryTimes, string strHostName, ref string strUrl, out string err)
    {
        err = "";
        bool bDNSResolved = false;

        if (string.IsNullOrEmpty(strHostName))
            return bDNSResolved;

        int times = nTryTimes;
        do
        {
			--times;
            string ipStr = null;
            try
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(strHostName);
                if (ipAddresses != null && ipAddresses.Length > 0)
                {
                    for (int i = 0; i < ipAddresses.Length; ++i)
                    {
                        if (ipAddresses[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipStr = ipAddresses[i].ToString();
                            break;
                        }
                    }

                    if (ipStr != null)
                    {
                        if (strUrl.IndexOf(ipStr) < 0)
                            strUrl = strUrl.Replace(strHostName, ipStr);
                        bDNSResolved = true;
                    }
                }
            }
            catch (Exception e)
            {
                err = e.Message;
                bDNSResolved = false;
                break;
            }

            if (bDNSResolved)
            {
                break;
            }

        } while (times > 0);
        
        return bDNSResolved;
    }

    private void LoadDNSCaches()
    {
        //TODO
    }

    private void SaveDNSCaches()
    {
        //TODO
    }

    //从服务器下载 version.txt 到 strGameNewVerFile
    public DownloadTaskErrorCode FetchServerVersionFile(string url, string hostName, string savedFile, int timeout, out string errMsg)
    {
        if (!FileOperate.MakeDir(savedFile))
        {
            LogString(HobaText.Format("[FetchServerVersionFile] MakeDir {0} Failed!", savedFile));
        }

        if (FileOperate.IsFileExist(savedFile))
            FileOperate.DeleteFile(savedFile);

        //测试
        //         int filesize2 = (int)SeasideResearch.LibCurlNet.External.CURL_GetUrlFileSize(url, timeout);
        //         LogString(HobaString.Format("c++ url FileSize: {0}, fileSize: {1}", url, filesize2));
        // 
        //         int filesize = (int)UpdateUtility.GetUrlFileSizeEx(url, hostName, timeout);
        //         LogString(HobaString.Format("url FileSize: {0}, fileSize: {1}", url, filesize));

        var code = UpdateUtility.GetByUrl(
            url,
            hostName,
            savedFile,
            timeout,          //10s
            null,
            null,
            out errMsg);

        return code;
    }

    //从服务器下载 ServerList.xml 到 strServerList
    public static DownloadTaskErrorCode FetchServerListFile(string url, string hostName, string savedFile, int timeout, out string errMsg)
    {
        if (!FileOperate.MakeDir(savedFile))
        {
            Common.HobaDebuger.LogErrorFormat("[FetchServerListFile] MakeDir {0} Failed!", savedFile);
        }

//         if (FileOperate.IsFileExist(strServerListFile))
//             FileOperate.DeleteFile(strServerListFile);

        var code = UpdateUtility.GetByUrl(
            url,
            hostName,
            savedFile,
            timeout,          //10s
            null,
            null,
            out errMsg);

        return code;
    }
    
    public static DownloadTaskErrorCode FetchAccountRoleListFile(string url, string hostName, string savedFile, int timeout, out string errMsg)
    {
        if (!FileOperate.MakeDir(savedFile))
        {
            Common.HobaDebuger.LogErrorFormat("[FetchAccountRoleListFile] MakeDir {0} Failed!", savedFile);
        }

        var code = UpdateUtility.GetByUrl(
            url,
            hostName,
            savedFile,
            timeout,          //10s
            null,
            null,
            out errMsg);

        return code;
    }

    public static string GetUrlContentType(string url, int timeout)
    {
        return UpdateUtility.GetUrlContentType(url, UpdateUtility.GetHostName(url), timeout);
    }

    public static DownloadTaskErrorCode FetchByUrl(string url, string destFile, int timeout, out string errMsg)
    {
        if (!FileOperate.MakeDir(destFile))
        {
            Common.HobaDebuger.LogWarning(HobaText.Format("[FetchByUrl] MakeDir {0} Failed!", destFile));
        }

        if (FileOperate.IsFileExist(destFile))
            FileOperate.DeleteFile(destFile);

        string hostName = UpdateUtility.GetHostName(url);

        var code = UpdateUtility.GetByUrl(
            url,
            hostName,
            destFile,
            timeout,          //10s
            null,
            null,
            out errMsg);

        return code;
    }


    //解析服务器版本信息
    public UpdateRetCode TryGetLatestVersionFromServer()
    {
        UpdateRetCode code = UpdateRetCode.success;

        try 
        {
            StreamReader reader = FileOperate.OpenTextFile(strGameNewVerFile);
            if (reader == null)
                return UpdateRetCode.patcher_no_ver_file;

            if (!m_VersionMan.LoadVersions(reader))
            {
                reader.Close();
                code = UpdateRetCode.patcher_invalid_ver_file;
                FileOperate.DeleteFile(strGameNewVerFile);
            }
            else
            {
                reader.Close();
            }
        }
        catch(Exception)
        {
            code = UpdateRetCode.patcher_ver_err;
        }

        
        return code;
    }

    //更新服务器列表
    public static UpdateRetCode ParseServerList(string serverListFile, List<ServerInfo> serverList)
    {
        UpdateRetCode code = UpdateRetCode.success;

        try 
        {
            StreamReader reader = FileOperate.OpenTextFile(serverListFile);
            if (reader == null)
                return UpdateRetCode.server_list_no_file;

            string text = reader.ReadToEnd();
            reader.Close();

            string ext = Path.GetExtension(serverListFile);
            if (ext != null && ext.ToLower() == ".xml")
            {
                if (!ServerInfo.ParseFromXmlString(text, serverList))
                {
                    code = UpdateRetCode.server_list_parse_err;
                }
            }
            else if (ext != null && ext.ToLower() == ".json")
            {
                if (!ServerInfo.ParseFromJsonString(text, serverList))
                {
                    code = UpdateRetCode.server_list_parse_err;
                }
            }
            else
            {
                code = UpdateRetCode.server_list_parse_err;
                FileOperate.DeleteFile(serverListFile);
            }
        }
        catch(Exception)
        {
            code = UpdateRetCode.server_list_parse_err;
        }

        return code;
    }

    //更新服务器列表
    public static UpdateRetCode ParseRoleList(string account, string roleListFile, List<AccountRoleInfo> roleList, List<ServerInfo> testServerList)
    {
        try
        {
            StreamReader reader = FileOperate.OpenTextFile(roleListFile);
            if (reader == null)
                return UpdateRetCode.role_list_no_file;

            string text = reader.ReadToEnd();
            reader.Close();

            string ext = Path.GetExtension(roleListFile);
            if (ext != null && ext.ToLower() == ".json")  // Unity官方说 string.Equals效率优于string.Compare
            {
                if (!AccountRoleInfo.ParseFromJsonString(account, text, roleList, testServerList))
                    return UpdateRetCode.role_list_parse_err;
            }
            else
            {
                FileOperate.DeleteFile(roleListFile);
                return UpdateRetCode.role_list_parse_err;
            }
        }
        catch (Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("An Exception was raised when ParseRoleList, {0}", e);
            return UpdateRetCode.role_list_parse_err;
        }
        return UpdateRetCode.success;
    }

    //初始化pck环境
    public UpdateRetCode InitPackages()
    {
        UpdateRetCode code = UpdateRetCode.success;

        bool ret = PackFunc.PackInitialize(false);
        if (!ret)
            code = UpdateRetCode.pack_err;

        return code;
    }

    //关闭pck环境
    public UpdateRetCode ReleasePackages()
    {
        UpdateRetCode code = UpdateRetCode.success;
        PackFunc.PackFinalize(true);
        return code;
    }

    //找到版本pair
    public UpdateRetCode FindVersionPair()
    {
        UpdateRetCode code = UpdateRetCode.success;

        VER_PAIR verPair;
        bool bFind = m_VersionMan.FindVersionPair(m_CurrentVersion, out verPair);
        if(!bFind)
            code = UpdateRetCode.patcher_no_ver_file;
        else
            m_PackFileVer = verPair;

        return code;
    }

    public UpdateRetCode ReadIncFileList(string strFile, out FileIncList fileIncList, out ELEMENT_VER newVer)
    {
        UpdateRetCode ret = UpdateRetCode.success;
        newVer = new ELEMENT_VER();
        fileIncList = new FileIncList();

        using (StreamReader reader = FileOperate.OpenTextFile(strFile))
        {
            ELEMENT_VER verFrom = new ELEMENT_VER();
            ELEMENT_VER verTo = new ELEMENT_VER();

            //read header
            //"# %d.%d.%d %d.%d.%d %s %d"
            try
            {
                var strLine = reader.ReadLine();
                if (string.IsNullOrEmpty(strLine))
                    return UpdateRetCode.patcher_invalid_ver_file;

                char[] split = { ' ', '\t' };
                {
                    var arr = strLine.Split(split);
                    if (arr.Length >= 5 && arr[0] == "#")
                    {
                        verFrom.Parse(arr[1]);
                        verTo.Parse(arr[2]);
                        //projName = arr[3];
                        //nUpdateSize = Int32.Parse(arr[4]);
                    }
                    else
                    {
                        return UpdateRetCode.patcher_invalid_ver_file;
                    }
                }

                //检查版本
                if(m_CurrentVersion < verFrom)
                    return UpdateRetCode.patcher_version_too_new;

                if(m_CurrentVersion > verTo || m_CurrentVersion == verTo)
                    return UpdateRetCode.patcher_version_too_old;

                newVer = verTo;         //设置新版本

                while(true)
                {
                    strLine = reader.ReadLine();
                    if (strLine == null)
                        break;

                    if (strLine[0] == '#')
                        continue;

                    var arr = strLine.Split(split);
                    if (arr.Length >= 2)
                    {
                        string md5 = arr[0];
                        string filename = arr[1];
                        fileIncList.AddFile(md5, filename);
                    }
                    else
                    {
                        return UpdateRetCode.patcher_invalid_ver_file;
                    }
                }
            }
            catch (Exception)
            {
                return UpdateRetCode.fail;
            }
        }
        return ret;
    }

    public IEnumerable UpdateAutoCoroutine(ELEMENT_VER verBegin, ELEMENT_VER verLatest)
    {
        long packSizeOverAll = m_VersionMan.CalcSize(verBegin, verLatest);
        if (packSizeOverAll <= 0)
            packSizeOverAll = 1;
        long packFinishedSize = m_VersionMan.CalcSize(verBegin, m_PackFileVer.VerFrom);

        int nTotalPack = m_VersionMan.CalcPackCount(verBegin, verLatest);
        int nCurrentPack = m_VersionMan.CalcPackCount(verBegin, m_PackFileVer.VerTo);
        GameUpdateMan.Instance.HotUpdateViewer.SetPackageNum(nCurrentPack, nTotalPack);

        UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.Total, (float)((double)packFinishedSize / (double)packSizeOverAll));
        UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 0.0f);
        UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_BeginUpdate);

        yield return null;

        UpdateRetCode ret = UpdateRetCode.success;
        string strMd5 = this.m_PackFileVer.md5;

        //要下载的路径
        this.strDownloadUPackName = HobaText.Format(
            "{0}-{1}.jup",
            this.m_PackFileVer.VerFrom.ToString(),
            this.m_PackFileVer.VerTo.ToString());
        string strDownloadUPack = this.strDownloadPath + this.strDownloadUPackName;

        //计时
        float nStartTime = Time.time;
        float nLasTime = nStartTime;
        float nNowTime = nStartTime;

        //准备下载
        using (DownloadMan downloadMan = new DownloadMan(this.strDownloadPath))              //DownloadMan
        {
            downloadMan.TaskEndEvent += delegate { this.IsDownloadDone = true; };

            int nTryTimes = 0;

            bool bFileEqual = false;
            while (!bFileEqual)
            {
                if (ret == UpdateRetCode.net_err || ret == UpdateRetCode.io_err || ret == UpdateRetCode.urlarg_error)
                {
                    ++nTryTimes;

                    //重连次数超过
                    if (nTryTimes > UpdateConfig.TotalReconnectTime)
                    {
                        ret = UpdateRetCode.connect_fail;
                        break;          //这次更新错误，等待选择重试
                    }

                    //重连，间隔一定时间
                    do
                    {
                        yield return new WaitForSeconds(1.0f);
                        nNowTime = Time.time;
                    }
                    while (nNowTime - nLasTime <= UpdateConfig.ReconnectTime);
                    nLasTime = nNowTime;

                    this.LogString(HobaText.Format("DownloadMan net_err begin retry {0} ... file: {1}", nTryTimes, this.strDownloadUPackName));
                }
                else
                {
                    nTryTimes = 0;
                }
                
                if (ret == UpdateRetCode.md5_not_match || ret == UpdateRetCode.download_fail)      
                {
                    break;
                }

                //如果文件已存在，判断md5 
                if (FileOperate.IsFileExist(strDownloadUPack))
                {
                    yield return new WaitForSeconds(0.01f);

                    UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 100.0f);
                    UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_CheckingExistPack);
                    yield return null;

                    //string md5_exist = FileOperate.CalcFileMd5(strDownloadUPack);
                    CalcMd5ThreadInfo calcMd5Info = CalcFileMd5(strDownloadUPack);
                    while (calcMd5Info.IsRunning)
                    {
                        yield return _ShortWait;
                    }
                    OnCalcMd5Complete();
                    string md5_exist = calcMd5Info.Md5;

                    if (md5_exist == strMd5)
                    {
                        bFileEqual = true;
                        break;
                    }
                    FileOperate.DeleteFile(strDownloadUPack);       //删除旧文件
                }

                //重新开始下载
                this.IsDownloadDone = false;
                if (!FileOperate.MakeDir(strDownloadUPack))
                {
                    LogString(HobaText.Format("[UpdateAutoCoroutine] MakeDir {0} Failed!", strDownloadUPack));
                }

                //
                UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 0.0f);
                UpdateInfoUtil.SetDownStatusString(0.0f);
                GameUpdateMan.Instance.HotUpdateViewer.SetDownloadInfo_TextUpate(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_TextUpdate));

                yield return null;

//              foreach (var item in FetchPackByUrlCoroutine(downloadMan, 
//                     this.strUpdateServerDir1, this.strUpdateServerHostName1, this.strDownloadUPackName, strMd5))
                foreach (var item in FetchPackCoroutine(downloadMan,
                    this.strUpdateServerDir1, this.strUpdateServerHostName1, 
                    this.strUpdateServerDir2, this.strUpdateServerHostName2,
                    this.strUpdateServerDir3, this.strUpdateServerHostName3,
                    this.strDownloadUPackName, strMd5))
                {
                    if (item is UpdateRetCode)
                    {
                        ret = (UpdateRetCode)item;
                        break;
                    }
                    else
                    {
                        yield return item;
                    }
                }

                if (ret != UpdateRetCode.success)
                    bFileEqual = false;
            }

            if(bFileEqual)
                ret = UpdateRetCode.success;
        }

        if (ret == UpdateRetCode.success)        //下载成功
        {
            UpdateInfoUtil.bShowWritingPack = true;

            //设置本地包路径
            strLocalPackFileName = strDownloadPath + strDownloadUPackName;
            UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 0.0f);

            yield return null;

            //打开本地包,更新...
            //提示正在写包
            GameUpdateMan.Instance.HotUpdateViewer.SetInstallInfo(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_WritingPack));
            GameUpdateMan.Instance.HotUpdateViewer.SetInstallPercent(-1.0f);

            foreach (var item in UpdateFileFromPackCoroutine(strLocalPackFileName, verBegin, verLatest))
            {
                if (item is UpdateRetCode)
                {
                    ret = (UpdateRetCode)item;
                    break;
                }

                yield return item;
            }

            //关闭临时包
            FileOperate.DeleteFile(this.strLocalPackFileName);
            this.strLocalPackFileName = "";

            UpdateInfoUtil.bShowWritingPack = false;
        }

        if (ret == UpdateRetCode.invalid_param)
        {
            ret = UpdateRetCode.cancel;   
            yield return ret;
        }
        else if (ret == UpdateRetCode.pack_file_broken ||
            ret == UpdateRetCode.net_err ||
            ret == UpdateRetCode.connect_fail ||
            ret == UpdateRetCode.md5_not_match ||
            ret == UpdateRetCode.io_err ||
            ret == UpdateRetCode.urlarg_error ||
            ret == UpdateRetCode.download_fail)
        {
            yield return ret;
        }
        else if (ret != UpdateRetCode.success)
        {
            ret = UpdateRetCode.fail;
            yield return ret;
        }

        //写入本地版本
        UpdateInfoUtil.SetVersion(UPDATE_VERSION.Local, this.m_CurrentVersion);

        yield return UpdateRetCode.success;
    }

    private static IEnumerable FetchPackByUrlCoroutine(DownloadMan downloadMan,
        string urlDir, string hostName, string downloadPackName, string strMd5)
    {
        string urlFileName = urlDir + downloadPackName;
        long dwJupSize = -1;
        if (dwJupSize <= 0)
        {
            dwJupSize = UpdateUtility.GetUrlFileSizeEx(urlFileName, hostName, DownloadMan.reqTimeOut);
        }

        if (dwJupSize < 0)      //无法取得url文件大小，网络不通重试
        {
            yield return new WaitForSeconds(1.0f);
            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_DownloadingErrAutoRetry);
            GameUpdateMan.Instance.HotUpdateViewer.SetDownloadInfo_TextUpate(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateState_DownloadingErrAutoRetry));
            yield return null;

            Patcher.Instance.LogString(HobaText.Format("DownloadMan fail to get pack size, file: {0}, host: {1}", urlFileName, hostName));
            yield return UpdateRetCode.net_err;
        }

        //开始下载
        downloadMan.AddTask(strMd5, urlFileName, hostName, downloadPackName, dwJupSize);
        if (!downloadMan.StartTask(strMd5))
        {
            Patcher.Instance.LogString(HobaText.Format("DownloadMan fail to start download pack, file: {0}", urlFileName));

            yield return new WaitForSeconds(1.0f);
            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_DownloadingErrAutoRetry);
            GameUpdateMan.Instance.HotUpdateViewer.SetDownloadInfo_TextUpate(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateState_DownloadingErrAutoRetry));
            yield return null;

            Patcher.Instance.LogString(HobaText.Format("DownloadMan fail to start DownloadTask, file: {0}, host: {1}", urlFileName, hostName));
            yield return UpdateRetCode.net_err;
        }
        else
        {
            Patcher.Instance.LogString(HobaText.Format("DownloadMan StartTask, file: {0}", urlFileName));
        }

        //下载过程
        UpdateInfoUtil.bShowProgress = true;
        DownloadTaskInfo tmpInfo;
        long taskFinishedSize = 0;
        long lastFinishedSize = 0;
        int lastTime = System.Environment.TickCount;
        downloadMan.AddDownloadStamp(0, 0);
        int zeroDownloadTime = 0;               //下载量为0的计时，超过10秒无下载量，下载出错重试
        while (downloadMan.IsWorkerRunning())       //
        {
            //Thread.Sleep(100);
            yield return new WaitForSeconds(0.1f);

            int now = System.Environment.TickCount;
            int deltaMs = now - lastTime;
            lastTime = now;

            if (downloadMan.FindTask(strMd5, out tmpInfo))
            {
                if (tmpInfo.status.HasError() || tmpInfo.totalSize <= 0.0f)          //下载失败，退出，等待重试
                {
                    yield return new WaitForSeconds(1.0f);
                    UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateState_DownloadingErrAutoRetry);
                    GameUpdateMan.Instance.HotUpdateViewer.SetDownloadInfo_TextUpate(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateState_DownloadingErrAutoRetry));
                    yield return null;

                    break;
                }

                //更新进度        
                {
                    long deltaSize = tmpInfo.finishedSize - lastFinishedSize;

                    if (deltaSize < 10)
                        zeroDownloadTime += deltaMs;
                    else
                        zeroDownloadTime = 0;

                    downloadMan.AddDownloadStamp(deltaSize, deltaMs);        //下载多少
                    lastFinishedSize = tmpInfo.finishedSize;

                    //新UI                    
                    CUpdateInfo updateInfo = UpdateInfoUtil.GetUpdateInfo();
                    GameUpdateMan.Instance.HotUpdateViewer.SetFileDownloadInfo(
                        updateInfo.curUpdateFileSize + tmpInfo.finishedSize,
                        updateInfo.totalUpdateFileSize,
                        downloadMan.GetDownloadSpeedKBS());

                    float progress = (float)tmpInfo.finishedSize / tmpInfo.totalSize;
                    UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, progress);
                    //                             float totalProgress = (float)(updateInfo.curUpdateFileSize + tmpInfo.finishedSize) / updateInfo.totalUpdateFileSize;
                    //                             UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.Total, totalProgress);

                    if (progress < 1.0f)
                    {
                        UpdateInfoUtil.SetDownStatusString(progress);
                        GameUpdateMan.Instance.HotUpdateViewer.SetDownloadInfo_TextUpate(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateString_TextUpdate));
                    }
                    else
                    {
                        zeroDownloadTime = 0;
                        UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_CheckingExistPack);
                        GameUpdateMan.Instance.HotUpdateViewer.SetDownloadInfo_TextUpate(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_CheckingExistPack));
                    }

                    yield return null;
                }

                taskFinishedSize = tmpInfo.finishedSize;

                if (zeroDownloadTime >= UpdateConfig.MaxZeroDownloadTime)           //下载为0时间超时
                    break;
            }
            else
            {
                break;              //error
            }
        }
        downloadMan.AddDownloadStamp(0, 0);
        UpdateInfoUtil.bShowProgress = false;

        //这时下载已经完成
        UpdateRetCode ret = UpdateRetCode.success;

        if (Patcher.Instance.IsDownloadDone)
        {
            UpdateInfoUtil.SetStateString(UPDATE_STATE.UpdateStatus_CheckingExistPack);
            GameUpdateMan.Instance.HotUpdateViewer.SetDownloadInfo_TextUpate(UpdateInfoUtil.GetStateString(UPDATE_STATE.UpdateStatus_CheckingExistPack));
            yield return null;

            if (downloadMan.FindTask(strMd5, out tmpInfo))
            {
                
                switch (tmpInfo.status)
                {
                    case DownloadTaskStatus.Finished:
                        break;
                    case DownloadTaskStatus.Failed:
                        {
                            DownloadTaskErrorInfo errInfo;
                            downloadMan.GetTaskErrorInfo(strMd5, out errInfo);

                            if (errInfo.errorCode == DownloadTaskErrorCode.NetworkError && errInfo.errorMessage.Contains("CURLE_PARTIAL_FILE"))
                            {
                                ret = UpdateRetCode.net_partial_file;

                                Patcher.Instance.LogString(HobaText.Format("DownloadMan net_partial_file! file: {0}", urlFileName));
                            }
                            else if (errInfo.errorCode == DownloadTaskErrorCode.Unknown && errInfo.errorMessage.Contains("CURLE_OPERATION_TIMEOUTED"))
                            {
                                ret = UpdateRetCode.operation_timeouted;

                                Patcher.Instance.LogString(HobaText.Format("DownloadMan operation_timeouted! file: {0}", urlFileName));
                            }
                            else
                            {
                                if (errInfo.errorCode == DownloadTaskErrorCode.Md5Dismatch)
                                    ret = UpdateRetCode.md5_not_match;
                                else if (errInfo.errorCode == DownloadTaskErrorCode.NetworkError)
                                    ret = UpdateRetCode.net_err;
                                else if (errInfo.errorCode == DownloadTaskErrorCode.IOError)
                                    ret = UpdateRetCode.io_err;
                                else if (errInfo.errorCode == DownloadTaskErrorCode.UrlArgError)
                                    ret = UpdateRetCode.urlarg_error;
                                else
                                    ret = UpdateRetCode.download_fail;

                                Patcher.Instance.LogString(HobaText.Format("DownloadMan fail to download pack, file: {0}, host: {1}, msg: {2}", urlFileName, hostName, errInfo.errorMessage));
                            }
                        }
                        break;
                    default:
                        break;
                }


            }
            else
            {   
                ret = UpdateRetCode.download_fail;
                Patcher.Instance.LogString(HobaText.Format("DownloadMan fail to download pack 0, file: {0}", urlFileName));
            }
        }
        else
        {
            ret = UpdateRetCode.download_fail;
            Patcher.Instance.LogString(HobaText.Format("DownloadMan fail to download pack 1, file: {0}, zeroDownloadTime: {1}", urlFileName, zeroDownloadTime));
        }

        yield return ret;
    }

    private static IEnumerable FetchPackCoroutine(DownloadMan downloadMan,
        string urlDir1, string hostName1, string urlDir2, string hostName2, string urlDir3, string hostName3,
        string downloadPackName, string strMd5)
    {
        UpdateRetCode code = UpdateRetCode.success;

        foreach (var item in FetchPackByUrlCoroutine(downloadMan, urlDir1, hostName1, downloadPackName, strMd5))
        {
            if (item is UpdateRetCode)
            {
                code = (UpdateRetCode)item;
                break;
            }
            else
            {
                yield return item;
            }
        }

        if (code == UpdateRetCode.success || code == UpdateRetCode.net_partial_file || code == UpdateRetCode.operation_timeouted)
        {
            yield return code;
        }

        code = UpdateRetCode.success;

        foreach (var item in FetchPackByUrlCoroutine(downloadMan, urlDir2, hostName2, downloadPackName, strMd5))
        {
            if (item is UpdateRetCode)
            {
                code = (UpdateRetCode)item;
                break;
            }
            else
            {
                yield return item;
            }
        }

        if (code == UpdateRetCode.success || code == UpdateRetCode.net_partial_file || code == UpdateRetCode.operation_timeouted)
        {
            yield return code;
        }

        //下载第三个url
        code = UpdateRetCode.success;

        foreach (var item in FetchPackByUrlCoroutine(downloadMan, urlDir3, hostName3, downloadPackName, strMd5))
        {
            if (item is UpdateRetCode)
            {
                code = (UpdateRetCode)item;
                break;
            }
            else
            {
                yield return item;
            }
        }

        yield return code;
    }

    public IEnumerable UpdateFileFromPackCoroutine(string strPackFile, ELEMENT_VER verBegin, ELEMENT_VER verLatest)
    {
        UpdateRetCode code = UpdateRetCode.success;
        if (this.m_CurrentVersion.IsValid())
        {
            code = GetLocalVersion();
            if (code != UpdateRetCode.success)
            {
                yield return UpdateRetCode.element_ver_err;
            }
        }

        UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 0.0f);
        UpdateInfoUtil.SetWritingPackStatusString(0.0f);

        //新UI
        GameUpdateMan.Instance.HotUpdateViewer.SetInstallPercent(-1);

        yield return null;

        //code = DoUpdateFrom7z(strPackFile);
        foreach (var item in this.DoUpdateFrom7zCoroutine(strPackFile, verBegin, verLatest))
        {
            if (item is UpdateRetCode)
            {
                code = (UpdateRetCode)item;
                break;
            }
            yield return null;
        }

        if (code != UpdateRetCode.success)
        {
            FileOperate.DeleteFile(strUpdateIncFile);           //删除inc.sw文件
        }
        else              //更新成功,写新版本
        {
            if (m_currentNewVer < BaseVersion)
            {
                code = UpdateRetCode.patcher_version_too_old;
            }
            else 
            {
                ELEMENT_VER newVer = m_currentNewVer;
                UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, 1.0f);
                //UpdateInfoUtil.SetWritingPackStatusString(1.0f);

                //新UI
                //GameUpdateMan.Instance.HotUpdateViewer.SetInstallPercent(1.0f);

                yield return null;

                long sizeAdd = this.m_VersionMan.CalcSize(m_CurrentVersion, m_currentNewVer);
                UpdateInfoUtil.AddDownloadedSize(sizeAdd);

                //更新本地版本
                SetLocalVersion(newVer);
                m_CurrentVersion = newVer;
        
                VER_PAIR verPair = m_PackFileVer;           //更新m_PackFileVer，避免重复下载，下一个版本pair需要FindVersionPair
                verPair.VerTo = newVer;
                verPair.VerFrom = newVer;
                m_PackFileVer = verPair;
            }
        }

        yield return code;
    }

    public IEnumerable DoUpdateFrom7zCoroutine(string strPack, ELEMENT_VER verBegin, ELEMENT_VER verLatest)
    {
        //this.LogString(HobaText.Format("DoUpdateFrom7zCoroutine: {0}, {1}, {2}", strPack, verBegin.ToString(), verLatest.ToString()));

        UpdateRetCode code = UpdateRetCode.success;

        SevenZReader reader = new SevenZReader();
        if (!reader.Init(strPack))
        {
            code = UpdateRetCode.pack_file_broken;
            reader.Release();
            yield return code;            //
        }

        int fileCount = reader.GetFileCount();
        if (fileCount > 0)
        {
            GameUpdateMan.Instance.HotUpdateViewer.SetInstallProgress(1, fileCount);
            yield return null;
        }

        FileIncList fileIncList = new FileIncList();
        //找到7z包中的版本文件, inc, 得到版本
        bool bIncValid = false;
        
        for (int iFile = 0; iFile < fileCount; ++iFile)
        {
            string name = reader.GetFileName(iFile);

            //            LogString(HobaString.Format("7z pack file: {0}: {1}", iFile, name));
            if (name == "")
            {
                this.LogString(HobaText.Format("Fail to get file name: {0}", iFile));
                code = UpdateRetCode.pack_file_broken;
                reader.Release();
                yield return code;        //
            }

            if (!bIncValid && name == "inc")
            {
                bIncValid = true;

                //读取
                ExtractPackThreadInfo threadInfo = ExtractFileFrom7zReader(reader, iFile);
                while (threadInfo.IsRunning)
                {
                    yield return _ShortWait;
                }
                OnExtractFileComplete();

                int dataSize = threadInfo.DataSize;
                IntPtr pData = threadInfo.Pointer;
                if (pData == IntPtr.Zero)
                {
                    this.LogString(HobaText.Format("Fail to extract file name: {0}", iFile));
                    code = UpdateRetCode.pack_file_broken;
                    reader.Release();
                    yield return code;    //
                }

                //写入
                if(!FileOperate.MakeDir(this.strUpdateIncFile))
                {
                    LogString(HobaText.Format("[UpdateAutoCoroutine] MakeDir {0} Failed!", this.strUpdateIncFile));
                }

                byte[] pText = new byte[dataSize];
                Marshal.Copy(pData, pText, 0, dataSize);
                char[] text = Encoding.UTF8.GetChars(pText);
                bool ret = FileOperate.WriteToTextFile(this.strUpdateIncFile, text, text.Length);

                if (!ret)
                {
                    this.LogString(HobaText.Format("Fail to write inc file name: %d", iFile));
                    code = UpdateRetCode.file_write_err;
                    reader.Release();
                    yield return code;    //
                }

                //读取inc内容
                code = ReadIncFileList(this.strUpdateIncFile, out fileIncList, out m_currentNewVer);
                //删除本地inc文件
                FileOperate.DeleteFile(this.strUpdateIncFile);
            }

            if (bIncValid)
                break;
        }

        if (!bIncValid || code != UpdateRetCode.success)          //找不到inc文件
        {
            this.LogString("Pack has no list file: " + strPack);
            code = UpdateRetCode.pack_file_broken;
            reader.Release();
            yield return code;    //
        }

        //计算进度
        long packSizeOverAll = m_VersionMan.CalcSize(verBegin, verLatest);
        if (packSizeOverAll <= 0)
            packSizeOverAll = 1;
        long packFinishedSize = m_VersionMan.CalcSize(verBegin, m_CurrentVersion);
        long packNextFinishSize = m_VersionMan.CalcSize(verBegin, m_currentNewVer);

        float fFileProgress = 0.0f;
        float fTotalProgress = (float)((packFinishedSize + (packNextFinishSize - packFinishedSize) * (double)fFileProgress) / (double)packSizeOverAll);
       
        UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, fFileProgress);
        UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.Total, fTotalProgress);
        //UpdateInfoUtil.SetWritingPackStatusString(fFileProgress);

        //新UI
        fileCount = reader.GetFileCount();
        if (fileCount > 0)
            GameUpdateMan.Instance.HotUpdateViewer.SetInstallProgress(1, fileCount);
        else
            GameUpdateMan.Instance.HotUpdateViewer.SetInstallPercent(-1.0f);

        yield return null;

        //读取patch文件列表
        for (int iFile = 0; iFile < fileCount; ++iFile)
        {
            //新UI
            GameUpdateMan.Instance.HotUpdateViewer.SetInstallProgress(iFile + 1, fileCount);
            yield return null;

            if (reader.IsDir(iFile))
                continue;

            string name = reader.GetFileName(iFile);

            //            LogString(HobaString.Format("7z pack file: {0}: {1}", iFile, name));
            if (string.IsNullOrEmpty(name))
            {
                this.LogString(HobaText.Format("Fail to get file name: {0}", iFile));
                code = UpdateRetCode.pack_file_broken;
                reader.Release();
                yield return code;    //
            }

            if (name == "inc")      //skip inc
                continue;

            string strMd5;
            if (!fileIncList.GetFileMd5(name, out strMd5))
            {
                this.LogString(HobaText.Format("Fail to get file md5: {0} {1}", iFile, name));
                code = UpdateRetCode.pack_file_broken;
                reader.Release();
                yield return code;        //
            }

            bool bInPack = PackFunc.IsFileInPack(name);     //是否属于指定的包
            bool bSameFileExist = false;

            if (bInPack)         //在包里的文件是否和更新文件md5一致，如果是则跳过更新
            {
                if (PackFunc.CalcPackFileMd5(name) == strMd5)
                    bSameFileExist = true;
            }
            if (bSameFileExist)
            {
                //if (iFile % (10 * 3) == 0)
                { 
                    fFileProgress = (float)(iFile + 1) / fileCount;
                    fTotalProgress = (float)(packFinishedSize + (packNextFinishSize - packFinishedSize) * fFileProgress) / packSizeOverAll;

                    UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.File, fFileProgress);
                    UpdateInfoUtil.SetProgress(UPDATE_PROGRESS.Total, fTotalProgress);
                    //UpdateInfoUtil.SetWritingPackStatusString(fFileProgress);
                    GameUpdateMan.Instance.HotUpdateViewer.SetInstallProgress(fileCount, fileCount);

                    yield return null;
                }
                continue;
            }

            //读取patch文件
            ExtractPackThreadInfo threadInfo = ExtractFileFrom7zReader(reader, iFile);
            while (threadInfo.IsRunning)
            {
                yield return _ShortWait;
            }
            OnExtractFileComplete();

            int dataSize = threadInfo.DataSize;
            IntPtr pData = threadInfo.Pointer;
        
            if (pData == IntPtr.Zero)
            {
                this.LogString(HobaText.Format("Fail to extract file name: {0}", iFile));
                code = UpdateRetCode.pack_file_broken;
                reader.Release();
                yield return code;        //
            }

            //检查md5
            //string memMd5 = FileOperate.CalcMemMd5(pData, dataSize);
            CalcMd5ThreadInfo calcMd5Info = CalcMemMd5(pData, dataSize);
            while (calcMd5Info.IsRunning)
            {
                yield return _ShortWait;
            }
            OnCalcMd5Complete();
            string memMd5 = calcMd5Info.Md5;

            if (memMd5 != strMd5)
            {
                this.LogString(HobaText.Format("File md5 mismatch: {0} {1}", iFile, name));
                code = UpdateRetCode.pack_file_broken;
                reader.Release();
                yield return code;        //
            }

            if (dataSize < 4)        //不是压缩文件
            {
                this.LogString(HobaText.Format("Compressed file has no header {0} {1}", iFile, name));
                code = UpdateRetCode.pack_file_broken;
                reader.Release();
                yield return code;        //
            }

            if (bInPack) //add to package
            {
                this.WritePacking(true);
                bool retFlag = PackFunc.AddCompressedDataToPack(name, pData, dataSize);

                if (!retFlag)
                {
                    this.LogString(HobaText.Format("Update fail to add file: {0} {1}", iFile, name));
                    code = UpdateRetCode.pack_file_broken;
                    reader.Release();
                    yield return code;        //
                }
                else
                {
                    //                  this.LogString(HobaString.Format("Success to add pack file: {0} {1}", iFile, name));
                    code = UpdateRetCode.success;
                }
            }
            else                   //解压，写文件
            {
                //使用更新目录, 将 AssetBundle/<Plaform> 和 AssetBundle/<Plaform>/Update 整合成AssetBundle
                string actualName;
                if (PackFunc.IsFileInAssetBundles(name))
                {
                    actualName = PackFunc.MakeShortAssetBundlesFileName(name);
                    //                    this.LogString(HobaString.Format("MakeShortAssetBundlesFileName: {0} {1} TO {2}", iFile, name, actualName));
                }
                else
                {
                    actualName = name;
                }

                UncompressToSepFileThreadInfo uncompressInfo = UncompressToSepFile(actualName, pData, dataSize);
                while (uncompressInfo.IsRunning)
                {
                    yield return _ShortWait;
                }
                OnUncompressToSepFileComplete();

                bool retFlag = uncompressInfo.RetFlag;
                if (!retFlag)
                {
                    this.LogString(HobaText.Format("Update fail to uncompress file: {0} {1}", iFile, actualName));
                    code = UpdateRetCode.pack_file_broken;
                    reader.Release();
                    yield return code;        //
                }
                else
                {
                    //                  this.LogString(HobaString.Format("Success to add sep file: {0} {1}", iFile, name));
                    code = UpdateRetCode.success;
                }
            }
        }

        reader.Release();

        PackFunc.FlushWritePack();
        if (!PackFunc.SaveAndOpenUpdatePack())
        {
            this.LogString(HobaText.Format("PackFunc.SaveAndOpenUpdatePack() Failed!!! {0}", strPack));
        }
        else
        { 
            this.WritePacking(false);       //清除写包标志
        }

        yield return code;        //
    }
}
