using System;
using System.Collections;
using Downloader;

//更新的基本定义，变量
public sealed partial class Patcher
{
    private static Patcher _Instance = null;
    public static Patcher Instance
    {
        get 
        {
            if (_Instance == null)
                _Instance = new Patcher();
            return _Instance;
        }
    }

    public class ThreadInfo
    {
        public volatile bool IsRunning = false;
        public DownloadTaskErrorCode ErrorCode;
        public string ErrMsg = string.Empty;
    }

    //从压缩包中读取文件
    public class ExtractPackThreadInfo
    {
        public volatile bool IsRunning = false;
        public IntPtr Pointer = IntPtr.Zero;
        public int DataSize = 0;
    }

    public class CalcMd5ThreadInfo
    {
        public volatile bool IsRunning = false;
        public string Md5 = string.Empty;
    }

    public class UncompressToSepFileThreadInfo
    {
        public volatile bool IsRunning = false;
        public bool RetFlag = false;
    }

    private UnityEngine.WaitForSeconds _ShortWait = new UnityEngine.WaitForSeconds(0.1f);

    //路径
    public string strBaseDir { get; private set; }
    public string strDocDir { get; private set; }
    public string strLibDir { get; private set; }
    public string strTmpDir { get; private set; }

    public string strLibPatcherTmpDir { get; private set; }

    public string strUpdateServerDir1 { get; private set; }      //更新url，DNS解析后变为ip地址
    public string strUpdateServerHostName1 { get; private set; }     //更新的host1名称

    public string strUpdateServerDir2 { get; private set; }      //更新url，DNS解析后变为ip地址
    public string strUpdateServerHostName2 { get; private set; }     //更新的host2名称

    public string strUpdateServerDir3 { get; private set; }      //更新url，DNS解析后变为ip地址
    public string strUpdateServerHostName3 { get; private set; }     //更新的host3名称

    public string strClientServerDir { get; private set; }       //服务器列表url，DNS解析后变为ip地址
    public string strClientServerHostName { get; private set; }    //服务器列表的host名称

    public string strDynamicServerDir { get; private set; }     //动态服务器列表url，由中心服配置
    public string strDynamicAccountRoleDir { get; private set; }  //动态的账号角色信息列表，由中心服配置
    public string strDynamicServerHostName { get; private set; }    //动态服务器列表的host名称

    //文件名
    public string strGameOldVerFile { get; private set; }   //本地版本文件
    public string strGameNewVerFile { get; private set; }      //新版本文件
    public string strUpdateIncFile { get; private set; }     //临时的增量版本
    public string strDownloadPath { get; private set; }     //临时下载目录
    public string strDownloadUPackName { get; set; }         //当前下载的jup包名
    public string strLocalPackFileName { get; set; }
    public string strErrorLog { get; private set; }     //log
    public string strTempWritePackFileName { get; private set; }    //写包时创建临时文件
    public string strServerListXML { get; private set; }
    public string strServerListJSON { get; private set; }
    public string strAccountRoleListJSON { get; private set; }

    //版本
    public ELEMENT_VER m_CurrentVersion { get; private set; }
    public VER_PAIR m_PackFileVer { get; private set; }
    public VersionMan m_VersionMan { get; private set; }        //服务器版本升级信息

    //更新信息
    public CUpdateInfo UpdateInfo { get; private set; }

    //状态
    public bool IsDownloadDone { get; set; }

    //日志
    private UpdateLog _UpdateLog { get; set; }

    private ThreadInfo _ServerListResult = new ThreadInfo();
    public ThreadInfo ServerListResult { get { return _ServerListResult; } }

    private ThreadInfo _AccoutRoleListResult = new ThreadInfo();
    public ThreadInfo AccoutRoleListResult { get { return _AccoutRoleListResult; } }

    private ExtractPackThreadInfo _ExtractPackResult = new ExtractPackThreadInfo();

    private CalcMd5ThreadInfo _CalcMd5Result = new CalcMd5ThreadInfo();

    private UncompressToSepFileThreadInfo _UncompressToSepFileResult = new UncompressToSepFileThreadInfo();


    private Patcher()
    {
        UpdateInfo = new CUpdateInfo();
        m_VersionMan = new VersionMan();

        //初始化文件名
        strGameOldVerFile = "patcher/config/game_ver.sw";
        strGameNewVerFile = "patcher/temp/game_servernewver.sw";
        strUpdateIncFile = "patcher/temp/inc.sw";
        strServerListXML = "patcher/ServerList.xml";
        strServerListJSON = "patcher/ServerList.json";
        strAccountRoleListJSON = "patcher/AccountRoleList.json";

        strDownloadPath = "patcher/temp/";
        strErrorLog = "Logs/update.log";
        strTempWritePackFileName = "writepack.tmp";
        //strAnnoucementFile = "patcher/temp/annoucement.txt";

        //strLibPatcherDir = "patcher/";
        //strLibPackageDir = "package/";
        //strLibPatcherConfigDir = "patcher/config/";
        strLibPatcherTmpDir = "patcher/temp/";
        //strLibAssetBundleDir = "AssetBundles/";

        _UpdateLog = new UpdateLog();
    }

    public IEnumerable Init(string baseDir,
        string docDir,
        string libDir,
        string tmpDir,
        string updateServerDir1,
        string updateServerDir2,
        string updateServerDir3,
        string clientServerDir,
        string dynamicServerDir,
        string dynamicAccountRoleDir)
    {
        strBaseDir = baseDir.NormalizeDir();
        strDocDir = docDir.NormalizeDir();
        strLibDir = libDir.NormalizeDir();
        strTmpDir = tmpDir.NormalizeDir();

        //
        strUpdateServerDir1 = updateServerDir1.NormalizeDir();
        strUpdateServerDir2 = updateServerDir2.NormalizeDir();
        strUpdateServerDir3 = updateServerDir3.NormalizeDir();
        strClientServerDir = clientServerDir.NormalizeDir();
        strDynamicServerDir = dynamicServerDir.NormalizeDir();
        strDynamicAccountRoleDir = dynamicAccountRoleDir;

        strUpdateServerHostName1 = UpdateUtility.GetHostName(strUpdateServerDir1);
        strUpdateServerHostName2 = UpdateUtility.GetHostName(strUpdateServerDir2);
        strUpdateServerHostName3 = UpdateUtility.GetHostName(strUpdateServerDir3);
        strClientServerHostName = UpdateUtility.GetHostName(strClientServerDir);
        strDynamicServerHostName = UpdateUtility.GetHostName(strDynamicServerDir);

        //
        strGameOldVerFile = strLibDir + strGameOldVerFile;
        strGameNewVerFile = strLibDir + strGameNewVerFile;
        strUpdateIncFile = strLibDir + strUpdateIncFile;
        strDownloadPath = strLibDir + strDownloadPath;
        strServerListXML = strLibDir + strServerListXML;
        strServerListJSON = strLibDir + strServerListJSON;
        strAccountRoleListJSON = strLibDir + strAccountRoleListJSON;
        strErrorLog = strDocDir + strErrorLog;
        strTempWritePackFileName = strLibDir + strTempWritePackFileName;
        //strAnnoucementFile = strLibDir + strAnnoucementFile;
        //strLibPatcherDir = strLibDir + strLibPatcherDir;
        //strLibPackageDir = strLibDir + strLibPackageDir;
        //strLibPatcherConfigDir = strLibDir + strLibPatcherConfigDir;
        strLibPatcherTmpDir = strLibDir + strLibPatcherTmpDir;
        //strLibAssetBundleDir = strLibDir + strLibAssetBundleDir;

        //create log
        FileOperate.MakeDir(strErrorLog);
        yield return null;

        _UpdateLog.CreateLog(strErrorLog);
        yield return null;

        LogString(HobaText.Format("skipUpdate: {0}", EntryPoint.Instance.SkipUpdate));

        LogString(HobaText.Format("baseDir: {0}", strBaseDir));
        LogString(HobaText.Format("docDir: {0}", strDocDir));
        LogString(HobaText.Format("libDir: {0}", strLibDir));
        LogString(HobaText.Format("TmpDir: {0}", strTmpDir));

        LogString(HobaText.Format("updateServerDir1: {0}, updateServerDir2: {1}, updateServerDir3: {2}",
            strUpdateServerDir1, strUpdateServerDir2, strUpdateServerDir3));
        LogString(HobaText.Format("clientServerDir: {0}", strClientServerDir));
        LogString(HobaText.Format("dynamicServerDir: {0}", strDynamicServerDir));
        LogString(HobaText.Format("dynamicAccountRoleDir: {0}", strDynamicAccountRoleDir));
        LogString(HobaText.Format("strUpdateServerHostName1: {0}, strUpdateServerHostName2: {1}, strUpdateServerHostName3: {2}",
            strUpdateServerHostName1, strUpdateServerHostName2, strUpdateServerHostName3));
        LogString(HobaText.Format("strClientServerHostName: {0}", strClientServerHostName));
        LogString(HobaText.Format("strDynamicServerHostName: {0}", strDynamicServerHostName));
        yield return null;

        LogString(HobaText.Format("strGameOldVerFile: {0}", strGameOldVerFile));
        LogString(HobaText.Format("strGameNewVerFile: {0}", strGameNewVerFile));
        LogString(HobaText.Format("strUpdateIncFile: {0}", strUpdateIncFile));
        LogString(HobaText.Format("strDownloadPath: {0}", strDownloadPath));
        LogString(HobaText.Format("strServerListXML: {0}", strServerListXML));
        LogString(HobaText.Format("strServerListJSON: {0}", strServerListJSON));
        LogString(HobaText.Format("strAccountRoleListJSON: {0}", strAccountRoleListJSON));
    }

    public void LogString(string msg)
    {
        _UpdateLog.LogWriteLine(msg);
    }

    public void UpdateExit()
    {
        _UpdateLog.DestroyLog();
    }

    public void WritePacking(bool bWrite)
    {
        string strFile = strTempWritePackFileName;

        if (bWrite && UpdateInfo.bWrittingPack == false)
        {
            char[] buffer = new char[1]{'0'};
            FileOperate.WriteToTextFile(strFile, buffer, 1);
        }
        else
        {
            if (FileOperate.IsFileExist(strFile))
                FileOperate.DeleteFile(strFile);
        }

        UpdateInfo.bWrittingPack = bWrite;
    }

    public bool IsWritingPackFileExist()
    {
        string strFile = strTempWritePackFileName;
        return FileOperate.IsFileExist(strFile);
    }

    private AssetUtility.SingleThreadWorker _DynamicServerThread;
    public ThreadInfo DownloadServerListFile(string url, string hostName, string jsonFile)
    {
        if (_ServerListResult.IsRunning) return _ServerListResult;

        if (_DynamicServerThread == null)
            _DynamicServerThread = new AssetUtility.SingleThreadWorker();
        
        _ServerListResult.IsRunning = true;
        _DynamicServerThread.StartTask(() =>
        {
            string errMsg = string.Empty;
            _ServerListResult.ErrorCode = FetchServerListFile(url, hostName, jsonFile, DownloadMan.reqTimeOut, out errMsg);
            if (!string.IsNullOrEmpty(errMsg))
                _ServerListResult.ErrMsg = errMsg;
            _ServerListResult.IsRunning = false;
        });

        return _ServerListResult;
    }

    public ThreadInfo DownloadAccountListFile(string url, string hostName, string jsonFile)
    {
        if (_AccoutRoleListResult.IsRunning) return _AccoutRoleListResult;

        if (_DynamicServerThread == null)
            _DynamicServerThread = new AssetUtility.SingleThreadWorker();
        
        _AccoutRoleListResult.IsRunning = true;
        _DynamicServerThread.StartTask(() =>
        {
            string errMsg = string.Empty;
            _AccoutRoleListResult.ErrorCode = FetchAccountRoleListFile(url, hostName, jsonFile, DownloadMan.reqTimeOut, out errMsg);
            if (!string.IsNullOrEmpty(errMsg))
                _AccoutRoleListResult.ErrMsg = errMsg;
            _AccoutRoleListResult.IsRunning = false;
        });

        return _AccoutRoleListResult;
    }

    public void OnDynamicDownloadComplete()
    {
        if (!_AccoutRoleListResult.IsRunning && !_ServerListResult.IsRunning)
        {
            if (_DynamicServerThread != null)
            {
                _DynamicServerThread.Stop();
                _DynamicServerThread.Abort();
                _DynamicServerThread = null;
            }
        }
    }

    private AssetUtility.SingleThreadWorker _ExtractPackThread;
    private ExtractPackThreadInfo ExtractFileFrom7zReader(SevenZReader reader, int iFile)
    {
        if (_ExtractPackResult.IsRunning)
            return _ExtractPackResult;

        if (reader == null)
        {
            _ExtractPackResult.Pointer = IntPtr.Zero;
            _ExtractPackResult.IsRunning = false;
            return _ExtractPackResult;
        }

        if (_ExtractPackThread == null)
            _ExtractPackThread = new AssetUtility.SingleThreadWorker();

        _ExtractPackResult.IsRunning = true;
        _ExtractPackThread.StartTask(() =>
        {
            _ExtractPackResult.Pointer = reader.ExtractFile(iFile, out _ExtractPackResult.DataSize);
            _ExtractPackResult.IsRunning = false;
        });

        return _ExtractPackResult;
    }

    public void OnExtractFileComplete()
    {
        if (!_ExtractPackResult.IsRunning)
        {
            if (_ExtractPackThread != null)
            {
                _ExtractPackThread.Stop();
                _ExtractPackThread.Abort();
                _ExtractPackThread = null;
            }
        }
    }

    private AssetUtility.SingleThreadWorker _CalcMd5Thread;

    private CalcMd5ThreadInfo CalcFileMd5(string fileName)
    {
        if (_CalcMd5Result.IsRunning)
            return _CalcMd5Result;

        if (_CalcMd5Thread == null)
            _CalcMd5Thread = new AssetUtility.SingleThreadWorker();

        _CalcMd5Result.IsRunning = true;
        _CalcMd5Thread.StartTask(() =>
        {
            _CalcMd5Result.Md5 = FileOperate.CalcFileMd5(fileName);
            _CalcMd5Result.IsRunning = false;
        });

        return _CalcMd5Result;
    }

    private CalcMd5ThreadInfo CalcMemMd5(IntPtr pData, int dataSize)
    {
        if (_CalcMd5Result.IsRunning)
            return _CalcMd5Result;

        if (_CalcMd5Thread == null)
            _CalcMd5Thread = new AssetUtility.SingleThreadWorker();

        _CalcMd5Result.IsRunning = true;
        _CalcMd5Thread.StartTask(() =>
        {
            _CalcMd5Result.Md5 = FileOperate.CalcMemMd5(pData, dataSize);
            _CalcMd5Result.IsRunning = false;
        });

        return _CalcMd5Result;
    }

    public void OnCalcMd5Complete()
    {
        if (!_CalcMd5Result.IsRunning)
        {
            if (_CalcMd5Thread != null)
            {
                _CalcMd5Thread.Stop();
                _CalcMd5Thread.Abort();
                _CalcMd5Thread = null;
            }
        }
    }

    private AssetUtility.SingleThreadWorker _UncompressToSepFileThread;

    private UncompressToSepFileThreadInfo UncompressToSepFile(string filename, IntPtr pData, int dataSize)
    {
        if (_UncompressToSepFileResult.IsRunning)
            return _UncompressToSepFileResult;

        if (_UncompressToSepFileThread == null)
            _UncompressToSepFileThread = new AssetUtility.SingleThreadWorker();

        _UncompressToSepFileResult.IsRunning = true;
        _UncompressToSepFileThread.StartTask(() =>
        {
            _UncompressToSepFileResult.RetFlag = PackFunc.UncompressToSepFile(filename, pData, dataSize);
            _UncompressToSepFileResult.IsRunning = false;
        });

        return _UncompressToSepFileResult;
    }

    public void OnUncompressToSepFileComplete()
    {
        if (!_UncompressToSepFileResult.IsRunning)
        {
            if (_UncompressToSepFileThread != null)
            {
                _UncompressToSepFileThread.Stop();
                _UncompressToSepFileThread.Abort();
                _UncompressToSepFileThread = null;
            }
        }
    }

}

