using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LuaInterface;
using Common;
using EntityComponent;


public partial class EntryPoint
{
    // 中心服地址 + 头像服务器地址，生命周期：Login + InGame 
    private ServerConfigParams _ServerConfigParams = new ServerConfigParams();
    // 更新地址设置，生命周期：Patcher Update 
    private UpdateConfigParams _UpdateConfigParams = new UpdateConfigParams();
    private UpdateStringConfigParams _UpdateStringConfigParams = new UpdateStringConfigParams();
    private UpdatePromotionParams _UpdatePromotionParams = new UpdatePromotionParams();

    // 服务器列表 
    // 使用逻辑：先主动RequestServerList，然后在回调中去GetServerList；使用完毕即可清理
    private List<ServerInfo> _ServerInfoList = new List<ServerInfo>();
    private List<ServerInfo> _TestServerInfoList = new List<ServerInfo>();
    private List<AccountRoleInfo> _AccountRoleInfoList = new List<AccountRoleInfo>();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private DebugSettingParams _DebugSetting = new DebugSettingParams();
#endif

    private GameCustomConfigParams _GameCustomConfigParams = new GameCustomConfigParams();        //程序启动后读取的xml配置
    private WwiseBankConfigParams _WwiseBankConfigParams = new WwiseBankConfigParams();
    private WwiseSoundConfigParams _WwiseSoundConfigParams = new WwiseSoundConfigParams();
    private PlayerFollowCameraConfig _PlayerFollowCameraConfig = new PlayerFollowCameraConfig();  //游戏跟随相机配置
    private PlayerNearCameraConfig _PlayerNearCameraConfig = new PlayerNearCameraConfig(); //近景相机配置

    private GamePersistentConfigParams _GamePersistentConfigParams = new GamePersistentConfigParams();      //第一时间可以读取的配置

    private bool _SkipUpdate = false;

    public string GetUpdateServerUrl1()
    {
#if PLATFORM_KAKAO
        return LTPlatformBase.ShareInstance().GetCDNAddress() + CPlatformConfig.GetPlatForm();
#else
        return _UpdateConfigParams.GetUpdateServerUrl() + CPlatformConfig.GetPlatForm();
#endif
    }

    public string GetUpdateServerUrl2()
    {
#if PLATFORM_KAKAO
        return string.Empty; //LTPlatformBase.ShareInstance().GetCDNAddress() + CPlatformConfig.GetPlatForm();
#else
        return string.Empty;
#endif
    }

    public string GetUpdateServerUrl3()
    {
#if PLATFORM_KAKAO
        return string.Empty; //LTPlatformBase.ShareInstance().GetCDNAddress() + CPlatformConfig.GetPlatForm();
#else
        return string.Empty;
#endif
    }

    public string GetClientServerUrl()
    {
#if PLATFORM_KAKAO
        return LTPlatformBase.ShareInstance().GetGameServerAddress() + CPlatformConfig.GetPlatForm();
#else
        return _UpdateConfigParams.GetClientServerUrl() + CPlatformConfig.GetPlatForm();
#endif
    }

    public string GetDynamicServerUrl()
    {
        return _ServerConfigParams.GetDynamicServerUrl(); ;
    }

    public string GetDynamicAccountRoleUrl()
    {
        return _ServerConfigParams.GetDynamicAccountRoleUrl(); ;
    }

    public UpdateStringConfigParams UpdateStringConfigParams
    {
        get { return _UpdateStringConfigParams; }
    }

    public UpdatePromotionParams UpdatePromotionParams
    {
        get { return _UpdatePromotionParams; }
    }

    public List<ServerInfo> ServerInfoList
    {
        get { return _ServerInfoList; }
    }

    public List<ServerInfo> TestServerInfoList
    {
        get { return _TestServerInfoList; }
    }

    public List<AccountRoleInfo> AccountRoleInfoList
    {
        get { return _AccountRoleInfoList; }
    }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    public DebugSettingParams DebugSettingParams
    {
        get { return _DebugSetting; }
    }
#endif

    public bool SkipUpdate
    {
        get { return _SkipUpdate; }
    }

    public GameCustomConfigParams GameCustomConfigParams
    {
        get { return _GameCustomConfigParams; }
    }

    public PlayerFollowCameraConfig PlayerFollowCameraConfig
    {
        get { return _PlayerFollowCameraConfig; }
    }

    public PlayerNearCameraConfig PlayerNearCameraConfig
    {
        get { return _PlayerNearCameraConfig; }
    }

    public WwiseBankConfigParams WwiseBankConfigParams
    {
        get { return _WwiseBankConfigParams; }
    }

    public WwiseSoundConfigParams WwiseSoundConfigParams
    {
        get { return _WwiseSoundConfigParams; }
    }

    public GamePersistentConfigParams GamePersistentConfigParams
    {
        get { return _GamePersistentConfigParams; }
    }

    private void AddOneToServerList(ServerInfo serverInfo)
    {
        bool bContains = false;
        for (int i = 0; i < _ServerInfoList.Count; ++i)
        {
            if (serverInfo.name == _ServerInfoList[i].name)
            {
                bContains = true;
                break;
            }
        }

        if (!bContains)
            _ServerInfoList.Add(serverInfo);
    }

    private void AddToServerList(List<ServerInfo> serverInfoList)
    {
        if(serverInfoList == null || serverInfoList.Count == 0) return;

        foreach (var v in serverInfoList)
            AddOneToServerList(v);
    }

    public void RequestServerList(Action<bool> callback)
    {
        StartCoroutine(RequestServerListCoroutine(callback).GetEnumerator());
    }

    IEnumerable RequestServerListCoroutine(Action<bool> callback)
    {
        var patcher = Patcher.Instance;
        if (patcher.ServerListResult.IsRunning)
        {
            Common.HobaDebuger.LogWarning("RequestServerListCoroutine Start Failed, ServerList Downloading");
            yield break;
        }

        _ServerInfoList.Clear();

        bool isSuccessful = true;
        // 先请求JSON
        {
            string url = patcher.strDynamicServerDir + UpdateConfig.ServerListRelativePathJSON;
            string hostName = patcher.strDynamicServerHostName;
            string jsonFile = patcher.strServerListJSON;
            FileOperate.DeleteFile(jsonFile);

            //Common.HobaDebuger.LogWarning("Start DownloadServerListJSONFile");
            patcher.DownloadServerListFile(url, hostName, jsonFile); //开启线程从中心服下载JSON文件
            while (patcher.ServerListResult.IsRunning)
            {
                yield return null;
            }

            var serverListJSON = new List<ServerInfo>();
            var dlCode = patcher.ServerListResult.ErrorCode;
            if (dlCode != Downloader.DownloadTaskErrorCode.Success)
            {
                // 下载失败
                Common.HobaDebuger.LogWarningFormat("DownloadServerListJSONFile Failed! DownloadTaskErrorCode: {0}", dlCode);
                isSuccessful = false;
            }
            else
            {
                var code = Patcher.ParseServerList(jsonFile, serverListJSON);
                if (code != UpdateRetCode.success)
                {
                    // 解析失败
                    Common.HobaDebuger.LogWarningFormat("Read ServerListJson Failed!");
                    isSuccessful = false;
                }
                else
                {
                    for (int i = 0; i < serverListJSON.Count; ++i)
                    {
                        string strUrl;
                        if (!OSUtility.DNSResolve(2, serverListJSON[i].ip, out strUrl))
                        {
                            serverListJSON[i].state = 3;
                            Common.HobaDebuger.LogWarningFormat("ServerListJSON DNSResolve Failed! {0}", serverListJSON[i].ip);
                        }
                        else
                        {
                            if (serverListJSON[i].ip != strUrl)
                                Common.HobaDebuger.LogWarningFormat("ServerListJSON DNSResolve Change {0} To {1}", serverListJSON[i].ip, strUrl);
                            serverListJSON[i].ip = strUrl;
                        }
                    }
                }
            }
            AddToServerList(serverListJSON);
        }

        // 再请求XML
        {
            bool customServerList = false;
            List<ServerInfo> serverListXML = new List<ServerInfo>();
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            customServerList = ReadCustomServerListXml(serverListXML);
#endif
            if (customServerList)
            {
                // 读取本地
                AddToServerList(serverListXML);
            }
            else
            {
                // 从CDN下载
                string url = HobaText.Format("{0}{1}", patcher.strClientServerDir, UpdateConfig.ServerListRelativePathXML);
                string hostName = patcher.strClientServerHostName;
                string xmlFile = patcher.strServerListXML;
                FileOperate.DeleteFile(xmlFile);

                patcher.DownloadServerListFile(url, hostName, xmlFile); //开启线程从CDN下载XML文件
                while (patcher.ServerListResult.IsRunning)
                {
                    yield return null;
                }

                var dlCode = patcher.ServerListResult.ErrorCode;
                if (dlCode != Downloader.DownloadTaskErrorCode.Success)
                {
                    // 下载失败
                    Common.HobaDebuger.LogWarningFormat("DownloadServerListXMLFile Failed! DownloadTaskErrorCode: {0} {1}", dlCode, patcher.ServerListResult.ErrMsg);
                    isSuccessful = false;
                }
                else
                {
                    var code = Patcher.ParseServerList(xmlFile, serverListXML);
                    if (code != UpdateRetCode.success)
                    {
                        // 解析失败
                        Common.HobaDebuger.LogWarningFormat("Read ServerListXML Failed!");
                        isSuccessful = false;
                    }
                    else
                    {
                        for (int i = 0; i < serverListXML.Count; ++i)
                        {
                            string strUrl;
                            if (!OSUtility.DNSResolve(2, serverListXML[i].ip, out strUrl))
                            {
                                serverListXML[i].state = 3;
                                Common.HobaDebuger.LogWarningFormat("ServerListXML DNSResolve Failed! {0}", serverListXML[i].ip);
                            }
                            else
                            {
                                if (serverListXML[i].ip != strUrl)
                                    Common.HobaDebuger.LogWarningFormat("ServerListXML DNSResolve Change {0} To {1}", serverListXML[i].ip, strUrl);
                                serverListXML[i].ip = strUrl;
                            }
                        }
                    }
                }
                AddToServerList(serverListXML);
            }
        }

        patcher.OnDynamicDownloadComplete();
        if (callback != null)
            callback(isSuccessful);
    }

    public void RequestAccountRoleList(string account, Action<bool> callback)
    {
        if (string.IsNullOrEmpty(account))
        {
            Common.HobaDebuger.LogWarning("RequestAccountRoleList failed, account got null or empty");
            return;
        }

        StartCoroutine(RequestAccountRoleListCoroutine(account, callback).GetEnumerator());
    }

    IEnumerable RequestAccountRoleListCoroutine(string account, Action<bool> callback)
    {
        var patcher = Patcher.Instance;
        if (patcher.AccoutRoleListResult.IsRunning)
        {
            Common.HobaDebuger.LogWarning("RequestAccountRoleListCoroutine Start Failed, AccountRoleList Downloading");
            yield break;
        }
        _AccountRoleInfoList.Clear();
        _TestServerInfoList.Clear();
        AccountRoleInfo.ResetOrderZoneId();

        string url = patcher.strDynamicAccountRoleDir + account;
        string hostName = patcher.strDynamicServerHostName;
        string jsonFile = patcher.strAccountRoleListJSON;
        FileOperate.DeleteFile(jsonFile);

        //Common.HobaDebuger.LogWarning("Start DownloadAccountListFile");
        patcher.DownloadAccountListFile(url, hostName, jsonFile); //开启线程从中心服下载JSON文件
        while (patcher.AccoutRoleListResult.IsRunning)
        {
            yield return null;
        }
        //Common.HobaDebuger.LogWarning("End DownloadAccountListFile");
        patcher.OnDynamicDownloadComplete();

        bool isSuccessful = true;
        var dlCode = patcher.AccoutRoleListResult.ErrorCode;
        if (dlCode != Downloader.DownloadTaskErrorCode.Success)
        {
            // 下载失败
            Common.HobaDebuger.LogWarningFormat("DownloadAccountRoleListFile Failed! DownloadTaskErrorCode: {0} {1}", dlCode, patcher.AccoutRoleListResult.ErrMsg);
            isSuccessful = false;
        }
        else
        {
            var code = Patcher.ParseRoleList(account, jsonFile, _AccountRoleInfoList, _TestServerInfoList);
            if (code != UpdateRetCode.success)
            {
                // 解析失败
                Common.HobaDebuger.LogWarningFormat("Read AccountRoleJson Failed!");
                isSuccessful = false;
            }
        }

        if (callback != null)
            callback(isSuccessful);
    }

    public long TotalSizeToCopy = 0;               //listfile.txt中第一行标记的大小
    public List<String> ListFilesToCopy = null;

    IEnumerable PreInitGameCoroutine()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        {
            DeviceLogger.Instance.WriteLog("AndroidPermissionManage Start ...");
            bool bComplete = false;
            SDK.PlatformControl.KakaoAndroidPermissionManage(() =>
            {
                bComplete = true;
            });

            while (!bComplete)
                yield return null;
        }
#endif

#if (UNITY_IPHONE || UNITY_ANDROID)
        foreach (var item in ShowSplash())
        {
            yield return item;
        }

        yield return null;
        yield return null;
#endif
#if PLATFORM_LONGTU
        _PanelMonition.SetActive(true);
        yield return new WaitForSeconds(2f);
        _PanelMonition.SetActive(false);
        yield return null;
#endif
        //Init VideoManager
        InitVideoManager();

        {
            bool isOpenVideoFinish = false;
            string oepnVideoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "TERA_Open.mp4");
            VideoManager.PlayVideo(oepnVideoPath, null, null, () => { isOpenVideoFinish = true; }, false, false, true);
            while (!isOpenVideoFinish)
            {
                VideoManager.Tick(Time.deltaTime); //特殊处理检测点击
                yield return null;
            }
        }

#if (UNITY_ANDROID && !UNITY_EDITOR)
        //生成用于接收安卓通信的脚本
        {
            var androidBridge = new GameObject("AndroidBridge");
            androidBridge.transform.SetParent(null);
            androidBridge.transform.position = Vector3.zero;
            androidBridge.transform.rotation = Quaternion.identity;
            androidBridge.transform.localScale = Vector3.zero;
            androidBridge.AddComponent<AndroidBridge>();
        }
#endif

        //读取和语言无关的配置，若要读取和语言相关的配置，需要在获取语言后再读取
        ReadConfigTextFromResources();      //从Resources目录中读取配置信息
        yield return null;
        ReadConfigXmlFromResources();       //读取xml配置
        yield return null;
        ReadUpdatePromotionXmlFromResources();
    }

#if (UNITY_IPHONE || UNITY_ANDROID)

    IEnumerable ShowSplash()
    {
        _PanelSplash.SetActive(true);
        //kakao
#if PLATFORM_KAKAO
        ScreenFadeEffectManager.Instance.StartFade(1f, 0f, 0.2f);
        ShowKakaoSplash();
        var img_splash = _PanelSplash.FindChild("Img_KakaoSplash");
        img_splash.SetActive(true);
        yield return new WaitForSeconds(1.3f);
        ScreenFadeEffectManager.Instance.StartFade(0f, 1f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        img_splash.SetActive(false);
#endif
        // lantu
        ScreenFadeEffectManager.Instance.StartFade(1f, 0f, 0.2f);
        var img_lantu = _PanelSplash.FindChild("Img_LantuSplash");
        img_lantu.SetActive(true);
        yield return new WaitForSeconds(1.3f);
        ScreenFadeEffectManager.Instance.StartFade(0f, 1f, 0.2f);
        yield return new WaitForSeconds(0.2f);

        _PanelSplash.SetActive(false);
        ScreenFadeEffectManager.Instance.ClearFadeEffect();
    }
#endif

#if PLATFORM_KAKAO
    private void ShowKakaoSplash()
    {
        if (_PanelSplash == null) return;
        var obj_splash = _PanelSplash.FindChild("Img_KakaoSplash");
        if (obj_splash != null)
        {
            // 按分辨率比例从大到小，再按分辨率从大到小
            List<Vector2> splashList = new List<Vector2>
            {
#if UNITY_ANDROID
                new Vector2(1600, 720),     //2.222222
                new Vector2(3120, 1440),    //2.166666
                new Vector2(1280, 720),     //1.777777
#elif UNITY_IPHONE
                new Vector2(1386, 640),     //2.165625
                new Vector2(2436, 1125),    //2.165333
                new Vector2(1334, 750),     //1.778666
                new Vector2(1920, 1080),    //1.777777
                new Vector2(1136, 640),     //1.775
                new Vector2(960, 640),      //1.5
                new Vector2(2048, 1536),    //1.333333
                new Vector2(1024, 768),     //1.333333
#endif
            };
            var image = obj_splash.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                float width = splashList[0].x;
                float height = splashList[0].y;
                float screenRatio = (float)Screen.width / (float)Screen.height;
                int index = 1;
                // 先找分辨率比例，没有符合的往大一级找
                for (int i = 1; i < splashList.Count; i++)
                {
                    var info = splashList[i];
                    float ratio = info.x / info.y;
                    if (screenRatio > ratio && Mathf.Abs(screenRatio-ratio) != Util.FloatZero) break;

                    index = i;
                    width = info.x;
                    height = info.y;
                }
                // 在同一分辨率比例下找分辨率，没有符合的往大一级找
                float curRatio = width / height;
                for (int i = index - 1; i >= 0; i--)
                {
                    var info = splashList[i];
                    float ratio = info.x / info.y;
                    if (Screen.width > info.x && Screen.height > info.y || curRatio - ratio != Util.FloatZero) break;
                    
                    width = info.x;
                    height = info.y;
                }

                string path = string.Format("KakaoSplash/Kakaogame_{0}x{1}", width, height);
                DeviceLogger.Instance.WriteLog(path);
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                    image.sprite = sprite;
                var arf = obj_splash.GetComponent<UnityEngine.UI.AspectRatioFitter>();
                if (arf != null)
                    arf.aspectRatio = curRatio;
            }
        }
    }
#endif

    private void ReadConfigTextFromResources()
    {
        TotalSizeToCopy = 0;           //默认值

        var listFileTxt = (TextAsset)Resources.Load("listfile", typeof(TextAsset));
        if (listFileTxt == null)
        {
            DeviceLogger.Instance.WriteLog("listfile.txt missing in Resources!");
            return;
        }

        try
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(listFileTxt.text)))
            {
                var sr = new StreamReader(ms);
                var str = sr.ReadLine();
                if (str != null)
                {
                    long.TryParse(str, out TotalSizeToCopy);
                    DeviceLogger.Instance.WriteLog(HobaText.Format("listfile lTotalSizeToCopy: {0}", TotalSizeToCopy));
                }

                if(ListFilesToCopy == null)
                    ListFilesToCopy = new List<string>();
                else
                    ListFilesToCopy.Clear();

                while (str != null)
                {
                    str = sr.ReadLine();
                    if (str == null)
                        break;

                    ListFilesToCopy.Add(str);
                }

                sr.Close();
                sr.Dispose();

                ms.Close();
            }
            ;
            
            Resources.UnloadAsset(listFileTxt);
        }
        catch (IOException e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("IOException : {0}", e.Message));
        }
        catch (Exception e)
        {
            DeviceLogger.Instance.WriteLog(HobaText.Format("Exception {0}", e.Message));
        }
}

//读取更新配置
private void ReadConfigXmlFromResources()
    {
        string strLocale = CPlatformConfig.GetLocale();
        try
        {
            //read UpdateConfig.xml
            _UpdateConfigParams.DefaultValue();
            TextAsset updateConfigTextAsset = (TextAsset)Resources.Load("UpdateConfig", typeof(TextAsset));
            if (updateConfigTextAsset != null)
            {
                _UpdateConfigParams.ParseFromXmlString(updateConfigTextAsset.text, strLocale);
                Resources.UnloadAsset(updateConfigTextAsset);
            }
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("Failed to Parse UpdateConfig:{0}", e);
        }
    }

    //现在更新字符串配置需要根据语言来切换
    private void ReadUpdateStringXmlFromResources(string strLanguage)
    {
        try
        {
            //read UpdateStringConfig.xml
            //_UpdateStringConfigParams.DefaultValue();
            var ppdateConfigFileName = HobaText.Format("UpdateStringConfig{0}", strLanguage);
            TextAsset updateStringConfigTextAsset = (TextAsset)Resources.Load(ppdateConfigFileName, typeof(TextAsset));
            if (updateStringConfigTextAsset != null)
            {
                _UpdateStringConfigParams.ParseFromXmlString(updateStringConfigTextAsset.text);
                Resources.UnloadAsset(updateStringConfigTextAsset);
            }
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("Failed to Parse UpdateStringXml:{0}", e);
        }
    }

    //读取更新推广内容配置
    private void ReadUpdatePromotionXmlFromResources()
    {
        try
        {
            TextAsset updateConfigTextAsset = (TextAsset)Resources.Load("UpdatePromotionConfig", typeof(TextAsset));
            if (updateConfigTextAsset != null)
            {
                _UpdatePromotionParams.ParseFromXmlString(updateConfigTextAsset.text);
                Resources.UnloadAsset(updateConfigTextAsset);
            }
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("Failed to Parse UpdatePromotionXml:{0}", e);
        }
    }

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
    private bool ReadDebugSettingXml()
    {
        //read DebugSetting.xml
        _DebugSetting.DefaultValue();
        try
        {
            string path = Path.Combine(_ResPath, "DebugSetting.xml");
            if (File.Exists(path))
            {
                string text = File.ReadAllText(path);
                _DebugSetting.ParseFromXmlString(text);
                return true;
            }
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("Failed to Parse DebugSettingXml:{0}", e);
        }
        return false;
    }
#endif

    private bool ReadServerConfigXml(string fileName)
    {
        _ServerConfigParams.DefaultValue();
        try
        {
            string text = File.ReadAllText(fileName);
            return _ServerConfigParams.ParseFromXmlString(text);
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("Failed to Parse ServerConfigXml:{0}", e);
        }
        return false;
    }

    private bool ReadCustomServerListXml(List<ServerInfo> serverList)
    {
        try
        {
            string configsDir = Path.Combine(_ResPath, "Configs"); 
            string path = Path.Combine(configsDir, "ServerList.xml");
            string text = File.ReadAllText(path);
            return ServerInfo.ParseFromXmlString(text, serverList);
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("Failed to Parse ServerConfigXml:{0}", e);
        }
        return false;
    }

    private void ReadGameCustomConfigParams()
    {
        _GameCustomConfigParams.DefaultValue();
        try
        {
            string path = Path.Combine(_ConfigPath, "GameCustomConfigParams.xml");
            byte[] bytes = Util.ReadFile(path);
            if (bytes != null)
                _GameCustomConfigParams.ParseFromXmlString(Encoding.UTF8.GetString(bytes));
        }
        catch (Exception e)
        {
            HobaDebuger.LogWarningFormat("ReadGameCustomConfigParams raise an Exception, {0}", e);
        }
    }

    private void ReadPlayerFollowCameraConfig()
    {
        _PlayerFollowCameraConfig.DefaultValue();
        try
        {
            string path = Path.Combine(_ConfigPath, "PlayerFollowCameraConfig.xml");
            byte[] bytes = Util.ReadFile(path);
            if (bytes != null)
                _PlayerFollowCameraConfig.ParseFromXmlString(Encoding.UTF8.GetString(bytes));
        }
        catch (Exception e)
        {
            HobaDebuger.LogWarningFormat("ReadPlayerFollowCameraConfig raise an Exception, {0}", e);
        }
    }

    public bool ReadPlayerNearCameraConfig(int prof)
    {
        _PlayerNearCameraConfig.DefaultValue(prof);
        try
        {
            string path = Path.Combine(_ConfigPath, "PlayerNearCameraConfig.xml");
            byte[] bytes = Util.ReadFile(path);
            if (bytes != null)
            {
                _PlayerNearCameraConfig.ParseFromXmlString(Encoding.UTF8.GetString(bytes), prof);
                return true;
            }
        }
        catch (Exception e)
        {
            HobaDebuger.LogWarningFormat("ReadPlayerNearCameraConfig raise an Exception, {0}", e);
        }
        return false;
    }

    private void ReadWwiseSoundConfigParams()
    {
        //read WwiseSoundConfig.xml
        _WwiseSoundConfigParams.DefaultValue();
        try
        {
            string path = Path.Combine(_ConfigPath, "WwiseSoundConfig.xml");
            byte[] bytes = Util.ReadFile(path);
            if (bytes != null)
                _WwiseSoundConfigParams.ParseFromXmlString(Encoding.UTF8.GetString(bytes));
        }
        catch (Exception e)
        {
            HobaDebuger.LogWarningFormat("ReadWwiseSoundConfigParams raise an Exception, {0}", e);
        }
    }

    private void ReadWwiseBankConfigParams()
    {
        _WwiseBankConfigParams.DefaultValue();
        try
        {
            string path = Path.Combine(_ConfigPath, "WwiseBankConfig.xml");
            byte[] bytes = Util.ReadFile(path);
            if (bytes != null)
                _WwiseBankConfigParams.ParseFromXmlString(Encoding.UTF8.GetString(bytes));
        }
        catch (Exception e)
        {
            HobaDebuger.LogWarningFormat("ReadWwiseBankConfigParams raise an Exception, {0}", e);
        }
    }

    IEnumerable InitGameCoroutine()
    {
        DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameCoroutine Start...");

        foreach (var item in PreInitGameCoroutine())
            yield return item;

        SetupPath();

        HobaDebuger.GameLogLevel = WriteLogLevel;

        //IOS Application.persistentDataPath  /var/mobile/Containers/Data/Application/app sandbox/Documents
        //Android /storage/emulated/0/Android/data/package name/files
#if UNITY_IOS
        _DocPath = Application.persistentDataPath;
        _LibPath = Path.Combine(Application.persistentDataPath, "UpdateRes");
        _TmpPath = Path.Combine(Application.persistentDataPath, "Tmp");
#elif UNITY_ANDROID
        _DocPath = Application.persistentDataPath;
        _LibPath = Path.Combine(Application.persistentDataPath, "UpdateRes");
        _TmpPath = Path.Combine(Application.persistentDataPath, "Tmp");
#else
        _DocPath = Environment.CurrentDirectory;
        _LibPath = Path.Combine(Environment.CurrentDirectory, "UpdateRes");
        _TmpPath = Path.Combine(Environment.CurrentDirectory, "Tmp");
#endif
        yield return null;

#if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS
        //初始化异常上报SDK
        CLogReport.Init();
        yield return null;  
#endif

        //初始化基础目录
        {
            string path = EntryPoint.Instance.ResPath;
            path = path.Replace("file://", "");
            LuaDLL.HOBA_Init(path, _DocPath, _LibPath, _TmpPath);
        }

        _VoiceDir = Path.Combine(_TmpPath, "Voice");
        _CustomPicDir = Path.Combine(_TmpPath, "CustomPic");
        _UserLanguageFile = Path.Combine(_DocPath, "userlanguage.txt");
        _UserBillingFile = Path.Combine(_DocPath, "userbilling.bin");
        _UserDataDir = Path.Combine(_DocPath, "UserData");

        string strOSLanguage = OSUtility.GetSystemLanguageCode();
        string strUserLanguage = GetUserLanguageCode();

        if (!Directory.Exists(_LibPath))
            Directory.CreateDirectory(_LibPath);
        if (!Directory.Exists(_VoiceDir))
            Directory.CreateDirectory(_VoiceDir);
        if (!Directory.Exists(_CustomPicDir))
            Directory.CreateDirectory(_CustomPicDir);
        if (!Directory.Exists(_UserDataDir))
            Directory.CreateDirectory(_UserDataDir);
        LuaDLL.HOBA_DeleteFilesInDirectory(_VoiceDir);               //清空Tmp目录
        LuaDLL.HOBA_DeleteFilesInDirectory(_CustomPicDir);               //清空Tmp目录
        yield return null;

        if (!File.Exists(_UserLanguageFile))                //创建语言配置文件
            WriteUserLanguageCode(strOSLanguage);
        yield return null;

        //目录信息
//#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
//         var pType = LTPlatformBase.ShareInstance().GetPlatformType();
//         HobaDebuger.LogWarningFormat("LTPlatformType: {0}", pType.ToString());
//         HobaDebuger.LogWarningFormat("AssetBundlePath: {0}", _AssetBundlePath);
//         HobaDebuger.LogWarningFormat("ResPath: {0}", ResPath);
//         HobaDebuger.LogWarningFormat("DocPath: {0}", _DocPath);
//         HobaDebuger.LogWarningFormat("LibPath: {0}", _LibPath);
//         HobaDebuger.LogWarningFormat("TmpPath: {0}", _TmpPath);
//         HobaDebuger.LogWarningFormat("ConfigPath: {0}", _ConfigPath);
//         HobaDebuger.LogWarningFormat("LuaPath: {0}", _LuaPath);
//         HobaDebuger.LogWarningFormat("VoiceDir: {0}", _VoiceDir);
//         HobaDebuger.LogWarningFormat("CustomPickDir: {0}", _CustomPicDir);
//         HobaDebuger.LogWarningFormat("OSLanguage: {0}, UserLanguage: {1}", strOSLanguage, strUserLanguage);
//         yield return null;
//#endif

        //根据语言设置更新语言
        ReadUpdateStringXmlFromResources(strUserLanguage);
        yield return null;

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)               //只在windows下起作用
        bool bDebugSetting = ReadDebugSettingXml();
//         HobaDebuger.LogWarningFormat("DebugSetting: {0}", bDebugSetting);
//         HobaDebuger.LogWarningFormat("DebugSetting SkipUpdate: {0}, Shortcut: {1}, LocalData: {2}, LocalLua: {3}, Is1080P: {4}, FullScreen: {5}", 
//             _DebugSetting.SkipUpdate, 
//             _DebugSetting.ShortCut, 
//             _DebugSetting.LocalData, 
//             _DebugSetting.LocalLua, 
//             _DebugSetting.Is1080P,
//             _DebugSetting.FullScreen);

        _SkipUpdate = _DebugSetting.SkipUpdate || File.Exists(Path.Combine(_DocPath, "skip.txt"));
        //HobaDebuger.LogWarningFormat("SkipUpdate: {0}", _SkipUpdate);
#endif

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        if (_DebugSetting.Is1080P)
            Screen.SetResolution(1920, 1080, _DebugSetting.FullScreen);

		if (_DebugSetting.FPSLimit > 0)
            Application.targetFrameRate = _DebugSetting.FPSLimit;
#endif

        //初始化平台SDK
        {
            GameUpdateMan.Instance.InitUpdateUI(); //显示更新界面

            bool isFinish = false;
            int code = -1;
            LTPlatformBase.ShareInstance().InitSDK((_, resultCode) =>
            {
                code = resultCode;
                isFinish = true;
            });
            LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Start); //平台SDK打点：开始游戏

            while (!isFinish)
                yield return null;
            
            bool isInited = LTPlatformBase.ShareInstance().IsInited;
            var pType = LTPlatformBase.ShareInstance().GetPlatformType();
            DeviceLogger.Instance.WriteLogFormat("LTPlatform InitSDK result:{0}, return code:{1}, platform type:{2}", isInited.ToString(), code.ToString(), pType.ToString());
            if (!isInited)
            {
                // 初始化失败，弹窗提示，退出游戏
                GameUpdateMan.Instance.HotUpdateViewer.SetCircle(false);
                string errStr = LTPlatformBase.ShareInstance().GetErrStr(code);
                yield return new WaitForUserClick(MessageBoxStyle.MB_OK, errStr, _UpdateStringConfigParams.PlatformSDKString_InitFailedTitle);

                ExitGame();
                yield break;
            }
        }

        {
            // copy base res
#if UNITY_ANDROID
            string srcDir = "res_base";
            string destDir = Path.Combine(Application.persistentDataPath, "res_base");

            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            IsInstallFinished = File.Exists(destDir + "/.lock"); 
            if (!IsInstallFinished)
            {
                DeviceLogger.Instance.WriteLog(string.Format("Begin RunInstallStage... from {0} to {1}", srcDir, destDir));
                foreach (var item in GameUpdateMan.Instance.RunInstallStage(srcDir, destDir))
                    yield return item;
                DeviceLogger.Instance.WriteLog("End RunInstallStage...");
            }

            if (IsInstallFinished)
            {
                string lockFile = Path.Combine(destDir, ".lock");
                StreamWriter writer = File.CreateText(lockFile);
                writer.Write(TotalSizeToCopy);
                writer.Close();
                DeviceLogger.Instance.WriteLog("EntryPoint InitGameCoroutine EntryPoint.Instance.RunInstallStage() Success...");
            }
            else
            {
                DeviceLogger.Instance.WriteLog("EntryPoint InitGameCoroutine EntryPoint.Instance.RunInstallStage() Failed!");
            }
            yield return null;  
#endif
            //在更新开始前，获取ServerConfig.xml
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            {
                string configsDir = Path.Combine(_ResPath, "Configs");
                string path = Path.Combine(configsDir, "ServerConfig.xml");
                if (!ReadServerConfigXml(path))
                    HobaDebuger.LogWarningFormat("Read ServerConfig Failed: {0}", path);
            }
#else
            {
                string url = EntryPoint.Instance.GetClientServerUrl().NormalizeDir() + "ServerConfig.xml";
                string tmpPath = Path.Combine(EntryPoint.Instance.TmpPath, "ServerConfig.xml");
                string errMsg;
                var code = Patcher.FetchByUrl(url, tmpPath, Downloader.DownloadMan.reqTimeOut, out errMsg);
                if (code == Downloader.DownloadTaskErrorCode.Success)
                {
                    if (!ReadServerConfigXml(tmpPath))
                        HobaDebuger.LogWarningFormat("Read ServerConfig Failed: {0}", url);
                }
                else
                {
                    HobaDebuger.LogWarningFormat("Download ServerConfig Failed: {0}, {1}", url, code);
                }
            }
#endif
            yield return null;

            DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameCoroutine UpdateRoutine Start...");
            //IAP Verify url init and check receipt cache.
            LTPlatformBase.ShareInstance().InitPurchaseVerifyUrl(_ServerConfigParams.GetPurchaseVerifyUrl());
            LTPlatformBase.ShareInstance().ProcessPurchaseCache();
            LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_Start_Update); //平台SDK打点：开始更新

            // App & 资源更新 
            foreach (var item in GameUpdateMan.Instance.UpdateRoutine())
            {
                yield return item;
            }
            LTPlatformBase.ShareInstance().SetBreakPoint(SDK.POINT_STATE.Game_End_Update); //平台SDK打点：结束更新

            DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameCoroutine UpdateRoutine End...");
        }

        //初始化pck包, 编辑器模式下不使用pck，策划编辑器模式下使用
#if !UNITY_EDITOR
        {
            string path = _ResPath;
            path = path.Replace("file://", "");
            LuaDLL.HOBA_InitPackages(path);
        }
#endif
        //FIX ME:: 加载不等待2帧 Windows崩溃 待查
        yield return null;  //等待一帧，否则部分 Android 设置闪烁
        yield return null;  //等待一帧，否则部分 Android 设置闪烁

        ReadGameCustomConfigParams();
        yield return null;

        ReadWwiseBankConfigParams();
        yield return null;

        ReadWwiseSoundConfigParams();
        yield return null;

        ReadPlayerFollowCameraConfig();
        yield return null;

        CLogFile.Init();
        yield return null;

        CGameSession.Instance().PingInterval = GameCustomConfigParams.PingInterval;
        CGameSession.Instance().MaxProcessProtocol = GameCustomConfigParams.MaxProcessProtocol;
        CGameSession.Instance().MaxProcessSpecialProtocol = GameCustomConfigParams.MaxProcessSpecialProtocol;

        foreach (var item in InitGameInternal())
            yield return item;

        CleanupUpdateResources();

        if (PanelLogo != null)
        {
            PanelLogo.SetActive(true);
            yield return null;
        }

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "TERA_BackgroundStory.mp4");
        VideoManager.PlayVideo(videoPath, null, null, null, true);
        yield return null;

        GFXConfig.Instance.Init();

        foreach (var item in DoStartGame())
            yield return item;

        //DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameCoroutine DoStartGame End...");

        DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameCoroutine End...");

        //TestLoadResource("Assets/Outputs/Sfx/Scene/scene_chuansong_chuanzou01.prefab");
    }

    private void TestLoadResource(string assetname)
    {
        Action<UnityEngine.Object> callback = (asset) =>
        {
            var obj = GameObject.Instantiate(asset) as GameObject;
            HobaDebuger.LogWarningFormat("GameObject Loaded! name: {0}", obj.name);
            GameObject.DestroyImmediate(obj);
        };
        CAssetBundleManager.AsyncLoadResource(assetname, callback, false);
    }

    public static String GetResBaseRelativePath()
    {
        return "res_base";
    }

    static String GetResBaseFullPath()
    {
#if UNITY_ANDROID      //android 需拷贝到另一目录
        return "assets://" + GetResBaseRelativePath();
#elif UNITY_IPHONE  // iOS平台直接用StreamingAsset中的目录
        return Application.dataPath + "/Raw/res_base";
#else      //其他平台直接用 StreamingAsset 中的目录
        return CStreamingAssetHelper.MakePath(GetResBaseRelativePath());
#endif
    }

    public void SetupPath()
    {
        //HobaDebuger.Log(HobaString.Format("BackupAssetsPath: {0}", _InitialAssetsPath));

#if UNITY_IPHONE
        _ResPath = Application.streamingAssetsPath + "/res_base";
        _AssetBundlePath = _ResPath;
#elif (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX) && !UNITY_ANDROID
        if (string.IsNullOrEmpty(_ResPath))
        {
            _ResPath = Application.streamingAssetsPath;
        }
        else if (_ResPath.StartsWith("./") || _ResPath.StartsWith("../"))
        {
            _ResPath = HobaText.Format("{0}/{1}", Application.dataPath, _ResPath);
        }
        _AssetBundlePath = _ResPath;
#elif UNITY_ANDROID
        //_AssetBundlePath = Application.dataPath + "!assets/res_base";           //android直接从手机内存中读取代替 www 部分机型读取速度太慢，暂时不使用
        _ResPath = HobaText.Format("{0}/res_base", Application.persistentDataPath);
        _AssetBundlePath = _ResPath;
#else
		if (string.IsNullOrEmpty(_ResPath))
		{
			_ResPath = Application.streamingAssetsPath;
		}

		if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            _ResPath = HobaText.Format("{0}/../GameRes", Application.dataPath);
        }
        _AssetBundlePath = _ResPath;
#endif

        _LuaPath = HobaText.Format("{0}/Lua", _ResPath);
        _ConfigPath = HobaText.Format("{0}/Configs", _ResPath);
    }

    IEnumerable InitGameInternal()
    {
        //Util.PrintCurrentMemoryInfo("InitGameInternal Begin");

        while (!GameUpdateMan.Instance.IsUpdateSucceed)
            yield return null;

#if UNITY_EDITOR || UNITY_STANDALONE
        //HobaDebuger.LogWarningFormat("CurrentVersion: {0}", GameUpdateMan.Instance.m_CurrentVersion);
        //HobaDebuger.LogWarningFormat("ServerVersion: {0}", GameUpdateMan.Instance.m_ServerVersion);
#endif
        DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameInternal Start...");

        CCamCtrlMan.Instance.SetCurCamCtrl(CAM_CTRL_MODE.LOGIN, true);

        if (!InputManager.Instance.Init())
        {
            HobaDebuger.LogError("Failed to initialize game");
            ExitGame();
            yield break;
        }
        yield return null;

        ResCacheMan.Instance.Init();
        yield return null;

        CFxCacheMan.Instance.Init();
        yield return null;

        ObjectBehaviour.OnRecycleCallback = CLogicObjectMan<ObjectBehaviour>.Instance.Remove;
        EntityEffectComponent.OnRecycleCallback = CLogicObjectMan<EntityEffectComponent>.Instance.Remove;
        yield return null;

        foreach (var item in CAssetBundleManager.Instance.Init(_ResPath, _AssetBundlePath))
            yield return item;
        yield return null;

        foreach (var item in AssetBundlesPreprocess())
            yield return item;

        LoadVersionData();
        yield return null;

        //DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameInternal Progress ShaderManager.Init Start...");
        ShaderManager.Instance.LoadAssets();
        yield return null;

        foreach (var item in ShaderManager.Instance.Init())
            yield return item;

        //DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameInternal Progress ShaderManager.Init End...");
        VoiceManager.Instance.Init();
        yield return null;

        HUDTextMan.Instance.Init();
        yield return null;

        MaterialPool.Instance.Init();
        yield return null;

        //DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameInternal Progress InitSoundMan Start...");
        //Init SoundMan
        if (GameCustomConfigParams.UseSound)
        {
            foreach (var item in InitSoundMan())
                yield return item;
        }

        //DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameInternal Progress InitSoundMan End...");
        //DeviceLogger.Instance.WriteLogFormat("EntryPoint InitGameInternal End...");
    }

    public byte[] CustomLoader(string name)
    {
        string path = HobaText.Format(@"{0}/{1}", _LuaPath, name);
        byte[] bytes = Util.ReadFile(path);
        return bytes;
    }

    public byte[] LoadFromAssetsPath(string name)
    {
        byte[] bytes = null;
        string path = HobaText.Format(@"{0}/{1}", _DocPath, name);
        if (File.Exists(path))
            bytes = Util.ReadFile(path);

        if (bytes == null)
        {
            path = HobaText.Format(@"{0}/{1}", _ResPath, name);
            bytes = Util.ReadFile(path);
        }
        return bytes;
    }

    private IEnumerable AssetBundlesPreprocess()
    {
        // Set DoNotReleaseBundle Info
        
        /*
        var bundleNames = new []
        {
            "animations", "commonatlas", "cganimator", "cg",
             "outward", "monsters", "characters",  
            "interfaces_kr", "interfaces_tw", "interfaces",
             "sfx", "others",
        };
         * */

        // AssetBundlesPreprocess: AnimationClip & Icon 
        // 采用的是同步加载实现，需要预先加载
        foreach (var v in GameCustomConfigParams.PreloadBundles)
        {
            CAssetBundleManager.SyncLoadAssetBundle(v);
            yield return null;
        }
    }

    private void ReadResPath()
    {
        GUISound.ReadSoundResPath();
    }

    public void EnablePanelLogo(bool enable)
    {
        if (PanelLogo == null) return;

        if (enable)
        {
            PanelLogo.SetActive(true);
        }
        else
        {
            GameObject.Destroy(PanelLogo);
            PanelLogo = null;
        }
    }

    private void CleanupUpdateResources()
    {
        if (_PanelSplash != null)
        {
            GameObject.Destroy(_PanelSplash);
            _PanelSplash = null;
        }

        if (PanelHotUpdate != null)
        {
            GameObject.Destroy(PanelHotUpdate);
            PanelHotUpdate = null;
        }

        if (_PanelMessageBox != null)
        {
            GameObject.Destroy(_PanelMessageBox);
            _PanelMessageBox = null;
        }

        if (_PanelMonition != null)
        {
            GameObject.Destroy(_PanelMonition);		//If NoUse
            _PanelMonition = null;
        }

        if (_PanelCirle != null)
        {
            GameObject.DestroyImmediate(_PanelCirle);
            _PanelCirle = null;
        }

        //clean up
        GameUpdateMan.Instance.HotUpdateViewer.Destroy();
        GameUpdateMan.ClearInstance();
        ListFilesToCopy = null;

        _UpdateConfigParams = null;
        _UpdateStringConfigParams = null;
        _UpdatePromotionParams = null;

        //if (!CAssetBundleManager.IsAssetBundleLoading())
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }

    private IEnumerable DoStartGame()
    {
        LuaStatic.Load = CustomLoader;
        LuaStatic.LoadFromAssetsPath = LoadFromAssetsPath;

        LuaScriptMgr sm = LuaScriptMgr.Instance;
        yield return null;

        foreach (var item in sm.Bind())
            yield return item;

        sm.Start();
        yield return null;

        try
        {
            sm.DoString("require [[init]]");
            sm.DoString("require [[preload]]");
        }
        catch (LuaScriptException e)
        {
            HobaDebuger.LogErrorFormat("LuaScriptException: {0}", e.Message);
            yield break;
        }
        yield return null;

        var luaState = sm.GetLuaState();
        if (luaState.L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(luaState.L);

            do
            {
                LuaDLL.lua_getglobal(luaState.L, "preload");
                if (LuaDLL.lua_pcall(luaState.L, 0, 1, 0) != 0)
                {
                    HobaDebuger.LogErrorFormat("LuaScriptException: {0}", LuaDLL.lua_tostring(luaState.L, -1));
                    yield break;
                }
                bool ret = LuaDLL.lua_toboolean(luaState.L, -1);
                LuaDLL.lua_pop(luaState.L, 1);
                if (ret)
                    break;

                yield return null;
            } while (true);

            LuaDLL.lua_settop(luaState.L, oldTop);
            yield return null;
        }

        //设置scale
        sm.GetDesignWidthAndHeight(ref _DesignWidth, ref _DesignHeight);

        if (_DesignWidth > 0 && _DesignHeight > 0)
            OSUtility.SetDesignContentScale(_DesignWidth, _DesignHeight);

        yield return null;

        try
        {
            if (!_BeForArtTest)
                sm.CallLuaFunction("StartGame");
            else
                sm.CallLuaFunction("SceneTest");

            _IsInited = true;
        }
        catch (LuaScriptException e)
        {
            HobaDebuger.LogErrorFormat("LuaScriptException: {0}", e.Message);
            _IsInited = false;
        }
        yield return null;

        ReadResPath();                  //读取C#端需要的lua路径

        StartCoroutine(TickCoroutine().GetEnumerator());
        yield return null;
    }

    public void StartPrelaodDataCoroutine()
    {
        StartCoroutine(PrelaodGameDatas().GetEnumerator());
    }

    private IEnumerable PrelaodGameDatas()
    {
        var mans = Template.TemplateDataManagerCollection.TemplateDataManagerMap;
        foreach (var kv in mans)
            kv.Value.Clear();
        mans.Clear();

        var dataPaths = Template.TemplateDataManagerCollection.PrelaodDataPaths;
        foreach (var v in dataPaths)
        {
            Template.TemplateDataManagerCollection.GetTemplateDataManager(v);
            yield return null;
        }
    }
}