using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using LuaInterface;
using Common;

public partial class EntryPoint
{
    private static string StrLanguageCode = string.Empty;

    //performance调试 
    public bool DebugOptionShowTerrain = true;
    public bool DebugOptionShowBlocks = true;
    public bool DebugOptionShowFx = true;
    public bool DebugOptionShowProjector = true;

    private RectTransform _CmdTransform = null;
    private RectTransform _LogTransform = null;
    private RectTransform _SkillSlotTransform = null;
    private RectTransform _UIHeadTransform = null;

    public string CurrentVersion
    {
        get { return Patcher.Instance.UpdateInfo.curVersion; }
    }

    public string ServerVersion
    {
        get { return Patcher.Instance.UpdateInfo.serverVersion; }
    }

    public string GetUserLanguageCode()
    {
        if (string.IsNullOrEmpty(StrLanguageCode))
            StrLanguageCode = ReadUserLanguageCode(UserLanguageFile);
        
        if (!string.IsNullOrEmpty(StrLanguageCode))
            return StrLanguageCode;

        return OSUtility.GetSystemLanguageCode();
    }

    private static string ReadUserLanguageCode(string userLanFile)
    {
        if (!File.Exists(userLanFile))
            return null;

        try
        {
            var languageCode = File.ReadAllText(userLanFile, System.Text.Encoding.UTF8);
            if (!string.IsNullOrEmpty(languageCode))
                languageCode = languageCode.ToUpper();

            return languageCode;
        }
        catch (Exception e)
        {
            HobaDebuger.LogErrorFormat("ReadUserLanguageCode raise an exception {0}", e);
        }

        return null;
    }

    public bool WriteUserLanguageCode(string lan)
    {
        bool ret = false;
        try
        {
            File.WriteAllText(UserLanguageFile, lan.ToUpper());
            ret = true;
        }
        catch(Exception e)
        {
            Common.HobaDebuger.LogErrorFormat("WriteUserLanguageCode Exception: {0}", e.Message);
        }

        if (ret)
            StrLanguageCode = lan.ToUpper();

        return ret;
    }

    public string GetFullResPath(string basePath, string fileName, bool isLocalized = false)
    {
        string fullBaseDir = Path.Combine(ResPath, basePath);
        string fullUpdateDir = Path.Combine(LibPath, basePath);

        if (isLocalized)
        {
            string languageCode = GetUserLanguageCode();
            if (languageCode != "CN")
            {
                fullBaseDir = Path.Combine(fullBaseDir, languageCode);
                fullUpdateDir = Path.Combine(fullUpdateDir, languageCode);
            }
        }

        string filePath = Path.Combine(fullUpdateDir, fileName);
        if (!File.Exists(filePath))
            filePath = Path.Combine(fullBaseDir, fileName);

        return filePath;
    }
    public string GetUserLanguagePostfix(bool bAssetBundle)
    {
        string code = GetUserLanguageCode();

        if (code == "CN")
            return "";

        if (bAssetBundle)                       //assetbundle只认小写
            return HobaText.Format("_{0}", code.ToLower());
        
        return HobaText.Format("_{0}", code);
    }

    public void OnDebugCmd(string cmd)
    {
        if (cmd.Contains("navmesh "))
        {
            NavMeshRenderer.Instance.OnDebugCmd(cmd);
        }
        else if (cmd.Contains("lightregion ") || cmd.Contains("regionset ") || cmd.Contains("obstacleset "))
        {
            MapRegionRenderer.Instance.OnDebugCmd(cmd);
        }
        else if (cmd.Contains("dn "))
        {
            ScenesManager.Instance.OnDebugCmd(cmd);
        }
        else if (cmd == "count")
        {
            U3DFuncWrap.Instance().OnDebugCmd(cmd);
        }
        else if (cmd == "abcount")
        {

        }
        else if (cmd.Contains("logtimer"))
        {
            HobaDebuger.LogWarningFormat("Timer Count: {0}", _TimerList.GetTimerCount());
            for (int i = 0; i < _TimerList.GetTimerCount(); ++i )
            {
                var tm = _TimerList.GetTimer(i);
                HobaDebuger.LogWarningFormat("Id:{0}, ttl:{1}, once:{2}, debug:{3}", tm.Id, tm.TTL, tm.IsOnce, tm.DebugInfo);
            }

            HobaDebuger.LogWarningFormat("Late Timer Count: {0}", _LateTimerList.GetTimerCount());
            for (int i = 0; i < _LateTimerList.GetTimerCount(); ++i)
            {
                var tm = _LateTimerList.GetTimer(i);
                HobaDebuger.LogWarningFormat("Id:{0}, ttl:{1}, once:{2}, debug:{3}", tm.Id, tm.TTL, tm.IsOnce, tm.DebugInfo);
            }
        }
        else if (cmd.Contains("showui "))
        {
            string[] result = cmd.Split(' ');
            if (result == null || result.GetLength(0) < 2) return;

            if (result[0].Equals("showui"))
            {
                if (_CmdTransform == null)
                    _CmdTransform = Main.PanelRoot.Find("UI_Debug(Clone)") as RectTransform;
                //if (cmdTransform == null)
                //    cmdTransform = Main.TopPateCanvas.Find("UI_Debug(Clone)") as RectTransform;

                if (_LogTransform == null)
                    _LogTransform = Main.PanelRoot.Find("UI_Log(Clone)") as RectTransform;
                //if (logTransform == null)
                //    logTransform = Main.TopPateCanvas.Find("UI_Log(Clone)") as RectTransform;

                if (_SkillSlotTransform == null)
                    _SkillSlotTransform = Main.PanelRoot.Find("Panel_Main_SkillNew(Clone)") as RectTransform;
                if (_UIHeadTransform == null)
                    _UIHeadTransform = Main.PanelRoot.Find("UI_Head(Clone)") as RectTransform;
                if (result[1].Equals("0"))
                {
                    if (_CmdTransform != null)
                        _CmdTransform.SetParent(Main.UIRootCanvas);
                    if (_LogTransform != null)
                        _LogTransform.SetParent(Main.UIRootCanvas);

                    if (_SkillSlotTransform != null)
                        _SkillSlotTransform.gameObject.SetActive(false);

                    if (_UIHeadTransform != null)
                        _UIHeadTransform.gameObject.SetActive(false);

                    Main.PanelRoot.gameObject.transform.localScale = Vector3.zero;
                    Main.PanelHUD.gameObject.SetActive(false);
                    Main.TopPateCanvas.gameObject.SetActive(false);
                }
                else
                {
                    Main.PanelRoot.gameObject.transform.localScale = Vector3.one;
                    Main.PanelHUD.gameObject.SetActive(true);
                    Main.TopPateCanvas.gameObject.SetActive(true);

                    if (_CmdTransform != null)
                        _CmdTransform.SetParent(Main.PanelRoot);
                    if (_LogTransform != null)
                        _LogTransform.SetParent(Main.PanelRoot);
                    if (_SkillSlotTransform != null)
                        _SkillSlotTransform.gameObject.SetActive(true);
                    if (_UIHeadTransform != null)
                        _UIHeadTransform.gameObject.SetActive(true);
                }
            }  
        }
        else if (cmd.Contains("perfs "))
        {
            string[] result = cmd.Split(' ');
            if (result == null || result.GetLength(0) < 3) return;

            if (result[0].Equals("perfs"))
            {
                int opt;
                if (int.TryParse(result[1], out opt))
                {
                    bool enable = !result[2].Equals("0");
                    switch(opt)
                    {
                        case 1: 
                            DebugOptionShowTerrain = enable; 
                            ShowTerrain();  
                            break;
                        case 2:
                            DebugOptionShowBlocks = enable; 
                            ShowBlocks(); break;
                        case 3:
                            DebugOptionShowFx = enable; 
                            ShowFx(); break;
                        case 4:
                            DebugOptionShowProjector = enable; 
                            break;
                        default: break;
                    }
                }
            }
        }
        else if (cmd.Contains("postprocess "))
        {
            string[] result = cmd.Split(' ');
            if (result == null || result.GetLength(0) < 2) return;

            if (result[0].Equals("postprocess"))
            {
                if (result[1].Equals("0"))
                {
                    Camera cam = Main.Main3DCamera;
                    PostProcessChain chain = null;
                    if (cam != null)
                        chain = cam.GetComponent<PostProcessChain>();
                    if (chain != null)
                        chain.enabled = false;
                }
                else
                {
                    Camera cam = Main.Main3DCamera;
                    PostProcessChain chain = null;
                    if (cam != null)
                        chain = cam.GetComponent<PostProcessChain>();
                    if (chain != null)
                        chain.enabled = true;
                }
            }
        }
        else if (cmd.Contains("errormessage "))
        {
            string[] result = cmd.Split(' ');
            if (result == null || result.GetLength(0) < 2) return;

            if (result[0].Equals("errormessage"))
            {
                int id = 0;
                if (int.TryParse(result[1], out id))
                    StartCoroutine(ShowErrorMessage(id));
            }
        }
    }

    private void ShowTerrain()
    {
        SceneConfig config = ScenesManager.Instance.GetSceneConfig();
        if (config != null)
        {
            var terrain = config.transform.Find("Preload/Terrain");
            if (terrain)
                terrain.gameObject.SetActive(DebugOptionShowTerrain);
        }
    }
        
    private void ShowBlocks()
    {
        SceneConfig config = ScenesManager.Instance.GetSceneConfig();
        if (config != null)
        {
            var blockRoot = config.transform.Find("BlockObjects");
            if (blockRoot)
                blockRoot.gameObject.SetActive(DebugOptionShowBlocks);
        }
    }

    private void ShowFx()
    {
        CFxCache.LoadAssetButNotPlay = DebugOptionShowFx;
    }

    public void OnPhotoCameraFileResult(string strFile)
    {
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnPhotoCameraFileResult");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaScriptMgr.Push(L, strFile);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public delegate void UploadPictureCallback(string strFileName, string err, bool success);

    public bool UploadPicture(string strFile,string playerGuid, UploadPictureCallback callback)
    {
        string url = _ServerConfigParams.GetCustomPicUploadUrl();

        byte[] content = File.ReadAllBytes(strFile);
        if (content.Length == 0 || content.Length > 10 * 1024 * 1024)
            return false;

        string realUrl = HobaText.Format(url, playerGuid);
        StartCoroutine(UploadPictureToUrl(strFile, content, realUrl, callback));

        return true;
    }

    private IEnumerator UploadPictureToUrl(string strFileName, byte[] content, string url, UploadPictureCallback callback)
    {
        yield return new WaitForEndOfFrame();

        WWWForm form = new WWWForm();
        form.AddBinaryData("FILE_UPLOAD", content , strFileName);

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        string authorizationValue = GameCustomConfigParams.Authorization;
        request.SetRequestHeader("Authorization", authorizationValue);
        yield return request.Send();

        if (request.isDone && request.error == null)
            callback(strFileName, "SUCCESS", true);
        else if (request.isDone && request.error != null)
            callback(strFileName, request.error, false);
    }

    public delegate void DownloadPictureCallback(string strFileName, int retCode, string err);      //retCode: 下载图片的自定义错误码， err: 下载错误信息

    public bool DownloadPicture(string strFile, int timeout, DownloadPictureCallback callback)
    {
        string url = _ServerConfigParams.GetCustomPicDownloadUrl();
        string fullPath = Path.Combine(EntryPoint.Instance.CustomPicDir, strFile);
        string md5 = Util.md5file(fullPath);
        string realUrl = HobaText.Format(url, strFile, md5);
        StartCoroutine(DownloadPictureFromUrl(strFile, realUrl, timeout, callback));

        return true;
    }

    private IEnumerator DownloadPictureFromUrl(string strFileName, string url, int timeout, DownloadPictureCallback callback)
    {
        yield return new WaitForEndOfFrame();

        string strFile = Path.Combine(EntryPoint.Instance.CustomPicDir, strFileName);
        string contentType = Patcher.GetUrlContentType(url, timeout);
        int retCode = 0;
        if (contentType.StartsWith("text/"))
        {
            string tmpPath = Path.Combine(EntryPoint.Instance.TmpPath, "tmp.txt");
            string errMsg;
            var code = Patcher.FetchByUrl(url, tmpPath, timeout, out errMsg);
            if (code == Downloader.DownloadTaskErrorCode.Success)
            {
                try
                {
                    var bytes = File.ReadAllBytes(tmpPath);
                    var chars = System.Text.Encoding.UTF8.GetChars(bytes);
                    string str = new string(chars, 0, chars.Length);
                    retCode = int.Parse(str);
                }
                catch(Exception)
                {
                    retCode = -1;
                }
            }
            else
            {
                retCode = -1;
            }

            callback(strFileName, retCode, null);
        }
        else //下载图片
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            string authorizationValue = GameCustomConfigParams.Authorization;
            request.SetRequestHeader("Authorization", authorizationValue);
            yield return request.Send();

            int resCode = -1;
            if (request.responseCode == 200)
            {
                var dic = JsonUtility.FromJson<URLResult>(request.downloadHandler.text);
                url = dic.url;

                request = UnityWebRequest.Get(url);
                yield return request.Send();

                if (request.responseCode == 200)
                {
                    if (!FileOperate.MakeDir(strFile))
                        Common.HobaDebuger.LogWarning(HobaText.Format("[FetchByUrl] MakeDir {0} Failed!", strFileName));

                    if (FileOperate.IsFileExist(strFile))
                        FileOperate.DeleteFile(strFile);

                    var streamFile = new FileStream(strFile, FileMode.OpenOrCreate);
                    streamFile.Write(request.downloadHandler.data, 0, (int)request.downloadedBytes);
                    streamFile.Close();

                    //string errMsg;
                    //var code = Patcher.FetchByUrl(url, strFile, timeout, out errMsg);
                    if (dic.resCode == (int)Downloader.DownloadTaskErrorCode.Success)
                        callback(strFileName, retCode, null);
                    else
                        callback(strFileName, retCode, HobaText.Format("{0}, {1}", (int)dic.resCode, url));
                }
                else
                {
                    callback(strFileName, retCode, HobaText.Format("{0}, {1}", (int)resCode, url));
                }
            }
            else
            {
                callback(strFileName, retCode, HobaText.Format("{0}, {1}", (int)resCode, url));
            }
        }
    }

    [Serializable]
    class URLResult
    {
        public int resCode;
        public string url;
    }

    //显示更新结束前可能出现的错误弹窗
    private IEnumerator ShowErrorMessage(int id)
    {
        if (UpdateStringConfigParams == null)
        {
            _UpdateStringConfigParams = new UpdateStringConfigParams();
            string strUserLanguage = GetUserLanguageCode();
            ReadUpdateStringXmlFromResources(strUserLanguage);
        }
        string title = null;
        string content = null;
        switch(id)
        {
            case 1:
                title = UpdateStringConfigParams.PlatformSDKString_InitFailedTitle;
                content = UpdateStringConfigParams.PlatformSDKString_UnknownErr;
                content += "\n" + string.Format(UpdateStringConfigParams.PlatformSDKString_ErrorCode, "9999");
                break;
            case 2:
                title = UpdateStringConfigParams.PlatformSDKString_InitFailedTitle;
                content = UpdateStringConfigParams.PlatformSDKString_NetworkFailure;
                content += "\n" + string.Format(UpdateStringConfigParams.PlatformSDKString_ErrorCode, "9999");
                break;
            case 3:
                title = UpdateStringConfigParams.PlatformSDKString_InitFailedTitle;
                content = UpdateStringConfigParams.PlatformSDKString_ServerTimeout;
                content += "\n" + string.Format(UpdateStringConfigParams.PlatformSDKString_ErrorCode, "9999");
                break;
            case 4:
                title = UpdateStringConfigParams.PlatformSDKString_InitFailedTitle;
                content = UpdateStringConfigParams.PlatformSDKString_InitializationFailed;
                content += "\n" + string.Format(UpdateStringConfigParams.PlatformSDKString_ErrorCode, "9999");
                break;
        }
        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(content))
        {
            yield return new WaitForUserClick(MessageBoxStyle.MB_OK, content, title);
        }
    }
}
