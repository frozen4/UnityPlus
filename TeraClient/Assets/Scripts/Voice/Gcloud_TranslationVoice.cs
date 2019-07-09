using Common;

#if USING_GCLOUDVOICE && UNITY_EDITOR

using gcloud_voice;
using LuaInterface;
using System;
using System.Collections.Generic;

public class Gcloud_TranslationVoice : IVoiceModule
{
    private VoiceManager _Manager = null;
    private VoiceMode _Mode;
    private bool _bIsGetAuthKey = false;

    public Gcloud_TranslationVoice(VoiceManager manager, VoiceMode mode)
    {
        _Manager = manager;
        _Mode = mode;

        FileIdDownloadingSet = new HashSet<string>();
    }

    public bool IsStarted { get; private set; }

    public VoiceMode VoiceMode { get { return _Mode; } }

    private Action<string> LOG_STRING = HobaDebuger.Log;

    private HashSet<string> FileIdDownloadingSet;

    public bool Start()
    {
        if (!IsStarted)
        {
            IGCloudVoice engine = _Manager.Gcloud_VoiceMan.VoiceEngine;

            engine.OnApplyMessageKeyComplete += OnApplyMessageKeyComplete;
            engine.OnUploadReccordFileComplete += OnUploadReccordFileComplete;
            engine.OnSpeechToText += OnSpeechToTextComplete;
            engine.OnDownloadRecordFileComplete += OnDownloadRecordFileComplete;
            engine.OnPlayRecordFilComplete += PlayRecordFilComplete;

            if (!_bIsGetAuthKey)
            {
                engine.ApplyMessageKey(15000);
            }

            IsStarted = true;
        }

        return IsStarted;
    }

    public void Update()
    {
        if (IsStarted)
            _Manager.Gcloud_VoiceMan.VoiceEngine.Poll();
    }

    public void Stop()
    {
        if (IsStarted)
        {
            IGCloudVoice engine = _Manager.Gcloud_VoiceMan.VoiceEngine;

            engine.OnApplyMessageKeyComplete -= OnApplyMessageKeyComplete;
            engine.OnUploadReccordFileComplete -= OnUploadReccordFileComplete;
            engine.OnSpeechToText -= OnSpeechToTextComplete;
            engine.OnDownloadRecordFileComplete -= OnDownloadRecordFileComplete;
            engine.OnPlayRecordFilComplete -= PlayRecordFilComplete;

            IsStarted = false;
        }
    }

    private void OnApplyMessageKeyComplete(IGCloudVoice.GCloudVoiceCompleteCode code)
    {
        //LOG_STRING("OnApplyMessageKeyComplete c# callback");
        int retCode = 0;
        if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_MESSAGE_KEY_APPLIED_SUCC)
        {
            _bIsGetAuthKey = true;

            //LOG_STRING("OnApplyMessageKeyComplete succ11");
        }
        else
        {
            retCode = (int)code;
            //LOG_STRING(string.Format("OnApplyMessageKeyComplete error code: {0}", code));
        }

        //call lua
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "Voice_OnApplyMessageKeyComplete");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushinteger(L, retCode);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    private void OnUploadReccordFileComplete(IGCloudVoice.GCloudVoiceCompleteCode code, string filepath, string fileid)
    {
        //LOG_STRING("OnUploadReccordFileComplete c# callback");

        int retCode = 0;
        if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_UPLOAD_RECORD_DONE)
        {
            //LOG_STRING("OnUploadReccordFileComplete succ, filepath:" + filepath + " fileid:" + fileid + " fileid len=" + fileid.Length);
        }
        else
        {
            retCode = (int)code;
            // LOG_STRING(string.Format("OnUploadReccordFileComplete error code: {0}, filepath: {1}, fileid: {2}", code, filepath, fileid));
        }

        //call lua
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "Voice_OnUploadReccordFileComplete");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushinteger(L, retCode);
                LuaDLL.lua_pushstring(L, filepath);
                LuaDLL.lua_pushstring(L, fileid);
                if (LuaDLL.lua_pcall(L, 3, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    private void OnSpeechToTextComplete(IGCloudVoice.GCloudVoiceCompleteCode code, string fileID, string result)
    {
        int retCode = (int)code;

        //call lua
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "Voice_OnSpeechToTextComplete");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushinteger(L, retCode);
                LuaDLL.lua_pushstring(L, fileID);
                LuaDLL.lua_pushstring(L, result);
                if (LuaDLL.lua_pcall(L, 3, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    private void OnDownloadRecordFileComplete(IGCloudVoice.GCloudVoiceCompleteCode code, string filepath, string fileid)
    {
        //LOG_STRING("OnDownloadRecordFileComplete c# callback");

        int retCode = 0;
        if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_DOWNLOAD_RECORD_DONE)
        {
            //LOG_STRING("OnDownloadRecordFileComplete succ, filepath:" + filepath + " fileid:" + fileid);  
        }
        else
        {
            retCode = (int)code;
            //LOG_STRING(string.Format("OnDownloadRecordFileComplete error code: {0}, filepath: {1}, fileid: {2}", code, filepath, fileid));
        }

        FileIdDownloadingSet.Remove(fileid);

        //call lua
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "Voice_OnDownloadRecordFileComplete");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushinteger(L, retCode);
                LuaDLL.lua_pushstring(L, filepath);
                LuaDLL.lua_pushstring(L, fileid);
                if (LuaDLL.lua_pcall(L, 3, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    private void PlayRecordFilComplete(IGCloudVoice.GCloudVoiceCompleteCode code, string filepath)
    {
        //LOG_STRING("OnPlayRecordFilComplete c# callback");

        int retCode = 0;
        if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_PLAYFILE_DONE)
        {
            //LOG_STRING("OnPlayRecordFilComplete succ, filepath:" + filepath);
        }
        else
        {
            retCode = (int)code;
            //LOG_STRING(string.Format("OnPlayRecordFilComplete error code: {0}, filepath: {1}", code, filepath));
        }

        //call lua
        IntPtr L = LuaScriptMgr.Instance.GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "Voice_OnPlayRecordFileComplete");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushinteger(L, retCode);
                LuaDLL.lua_pushstring(L, filepath);
                if (LuaDLL.lua_pcall(L, 2, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    //逻辑
    public int DownloadRecordedFile(string fileId, string downloadPath, int timeout)
    {
        if (FileIdDownloadingSet.Contains(fileId))
            return 0;

        int ret = _Manager.Gcloud_VoiceMan.DownloadRecordedFile(fileId, downloadPath, timeout);
        if (ret == 0)
            FileIdDownloadingSet.Add(fileId);
        return ret;
    }

    public int UploadRecordedFile(string recordPath, int timeout)
    {
        return _Manager.Gcloud_VoiceMan.UploadRecordedFile(recordPath, timeout);
    }

    public int SpeechToText(string fileID, int language = 0, int msTimeout = 6000)
    {
        return _Manager.Gcloud_VoiceMan.SpeechToText(fileID, language, msTimeout);
    }

    public int PlayRecordedFile(string downloadFilePath)
    {
        return _Manager.Gcloud_VoiceMan.PlayRecordedFile(downloadFilePath);
    }

    public int StopPlayFile()
    {
        return _Manager.Gcloud_VoiceMan.StopPlayFile();
    }

    public int StartRecording(string recordPath)
    {
        return _Manager.Gcloud_VoiceMan.StartRecording(recordPath);
    }

    public int StopRecording(Action<int> callback)
    {
        return _Manager.Gcloud_VoiceMan.StopRecording(callback);
    }

    public float GetRecFileLength(string filePath)
    {
        return _Manager.Gcloud_VoiceMan.GetRecFileLength(filePath);
    }
}

#endif