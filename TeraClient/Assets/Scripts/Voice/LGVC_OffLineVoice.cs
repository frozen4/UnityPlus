using Common;

#if USING_LGVC

using System;
using System.Collections.Generic;
using LuaInterface;
using LGVC.VoiceMessage;

namespace LGVC
{
    public class LGVC_OffLineVoice : IVoiceModule
    {
        private VoiceManager _Manager = null;
        private VoiceMode _Mode;

        public bool IsStarted { get; private set; }

        public VoiceMode VoiceMode { get { return _Mode; } }

        private Action<string> LOG_STRING = HobaDebuger.Log;

        private HashSet<string> FileIdDownloadingSet;

        public LGVC_OffLineVoice(VoiceManager manager, VoiceMode mode)
        {
            _Manager = manager;
            _Mode = mode;

            FileIdDownloadingSet = new HashSet<string>();
        }

        public bool Start()
        {
            if (!IsStarted)
            {
                LGVC_VoiceMan engine = _Manager.LGVC_VoiceMan;

                engine.Storage.UploadFileEvent += OnUploadReccordFileComplete;
                engine.Storage.DownloadFileEvent += OnDownloadRecordFileComplete;
                engine.OnPlayRecordFileComplete += PlayRecordFileComplete;

                OnApplyMessageKeyComplete(0);

                IsStarted = true;
            }

            return IsStarted;
        }

        public void Update()
        {
        }

        public void Stop()
        {
            if (IsStarted)
            {
                LGVC_VoiceMan engine = _Manager.LGVC_VoiceMan;

                engine.Storage.UploadFileEvent -= OnUploadReccordFileComplete;
                engine.Storage.DownloadFileEvent -= OnDownloadRecordFileComplete;
                engine.OnPlayRecordFileComplete -= PlayRecordFileComplete;

                IsStarted = false;
            }
        }

        private void OnApplyMessageKeyComplete(int code)
        {
            //call lua
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "Voice_OnApplyMessageKeyComplete");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_pushinteger(L, code);
                    if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                }
                LuaDLL.lua_settop(L, oldTop);
            }
        }

        private void OnUploadReccordFileComplete(bool success, string filePath, string fileId, AudioFormat audioFormat)
        {
            int code = success ? 0 : 1;
            //call lua
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "Voice_OnUploadReccordFileComplete");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_pushinteger(L, code);
                    LuaDLL.lua_pushstring(L, filePath);
                    LuaDLL.lua_pushstring(L, fileId);
                    if (LuaDLL.lua_pcall(L, 3, 0, 0) != 0)
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                }
                LuaDLL.lua_settop(L, oldTop);
            }
        }

        private void OnDownloadRecordFileComplete(bool success, string filePath, string fileId, AudioFormat audioFormat)
        {
            FileIdDownloadingSet.Remove(fileId);

            int code = success ? 0 : 1;
            //call lua
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "Voice_OnDownloadRecordFileComplete");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_pushinteger(L, code);
                    LuaDLL.lua_pushstring(L, filePath);
                    LuaDLL.lua_pushstring(L, fileId);
                    if (LuaDLL.lua_pcall(L, 3, 0, 0) != 0)
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                }
                LuaDLL.lua_settop(L, oldTop);
            }
        }

        private void PlayRecordFileComplete(int code, string errMsg, string filepath)
        {
            //call lua
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "Voice_OnPlayRecordFileComplete");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_pushinteger(L, code);
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
        public int DownloadRecordedFile(string filename, string downloadPath)
        {
            if (FileIdDownloadingSet.Contains(filename))
                return 0;

            int ret = _Manager.LGVC_VoiceMan.DownloadRecordedFile(filename, downloadPath);
            if (ret == 0)
                FileIdDownloadingSet.Add(filename);
            return ret;
        }

        public int UploadRecordedFile()
        {
            string recordPath = _Manager.TempVoiceFile;
            return _Manager.LGVC_VoiceMan.UploadRecordedFile(recordPath);
        }

        public int PlayRecordedFile(string downloadFilePath)
        {
            return _Manager.LGVC_VoiceMan.PlayRecordedFile(downloadFilePath);
        }

        public int StopPlayFile()
        {
            return _Manager.LGVC_VoiceMan.StopPlayFile();
        }

        public int StartRecording()
        {
            string recordPath = _Manager.TempVoiceFile;
            return _Manager.LGVC_VoiceMan.StartRecording(recordPath);
        }

        public int StopRecording(Action<int> callback)
        {
            return _Manager.LGVC_VoiceMan.StopRecording(callback);
        }

        public float GetRecFileLength()
        {
            return _Manager.TempVoiceLength;
        }
    }
}

#endif