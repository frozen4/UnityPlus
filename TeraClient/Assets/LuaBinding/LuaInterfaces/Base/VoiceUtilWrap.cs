using Common;
using LuaInterface;
using System;
using System.IO;

public static class VoiceUtilWrap
{
    public static LuaMethod[] voice_regs = new LuaMethod[]
    {
        new LuaMethod("IsVoiceEnabled", IsVoiceEnabled),
        new LuaMethod("IsMicrophoneAvailable", IsMicrophoneAvailable),
        new LuaMethod("GetVoiceMode", GetVoiceMode),
        new LuaMethod("SwitchToVoiceMode", SwitchToVoiceMode),
        new LuaMethod("OffLine_IsVoiceFileExist", OffLine_IsVoiceFileExist),
        new LuaMethod("OffLine_GetVoiceFileSeconds", OffLine_GetVoiceFileSeconds),
        new LuaMethod("OffLine_StartRecording", OffLine_StartRecording),
        new LuaMethod("OffLine_StopRecording", OffLine_StopRecording),
        new LuaMethod("OffLine_UploadFile", OffLine_UploadFile),
        new LuaMethod("OffLine_DownloadFile", OffLine_DownloadFile),        //返回秒数
        new LuaMethod("OffLine_PlayRecordedFile", OffLine_PlayRecordedFile),
        new LuaMethod("OffLine_StopPlayFile", OffLine_StopPlayFile),
        new LuaMethod("Translation_StartRecording", Translation_StartRecording),
        new LuaMethod("Translation_StopRecording", Translation_StopRecording),
        new LuaMethod("Translation_SpeechToText", Translation_SpeechToText),
        new LuaMethod("Translation_GetVoiceFileSeconds", Translation_GetVoiceFileSeconds),
        new LuaMethod("Translation_DownloadFile", Translation_DownloadFile),        //返回秒数
        new LuaMethod("Translation_PlayRecordedFile", Translation_PlayRecordedFile),
        new LuaMethod("Translation_StopPlayFile", Translation_StopPlayFile),
    };

    public static void Register(IntPtr L)
    {
        LuaScriptMgr.RegisterLib(L, "VoiceUtil", voice_regs);
        voice_regs = null;
    }

    public static void LogParamError(string methodName, int count)
    {
        HobaDebuger.LogErrorFormat("invalid arguments to method: VoiceUtilWrap.{0} count: {1}", methodName, count);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsVoiceEnabled(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool bEnable = VoiceManager.Instance.IsEnabled;
        LuaScriptMgr.Push(L, bEnable);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsMicrophoneAvailable(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool bEnable = OSUtility.IsMicrophoneAvailable();
        LuaScriptMgr.Push(L, bEnable);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetVoiceMode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        int mode = (int)VoiceManager.Instance.VoiceMode;
        LuaScriptMgr.Push(L, mode);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SwitchToVoiceMode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(int)))
        {
            int mode = (int)LuaDLL.lua_tonumber(L, 1);
            bool ret = VoiceManager.Instance.SwitchToVoiceMode((VoiceMode)mode);
            LuaScriptMgr.Push(L, ret);
        }
        else
        {
            LogParamError("SwitchToVoiceMode", count);
            LuaScriptMgr.Push(L, false);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_IsVoiceFileExist(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string filePath = LuaScriptMgr.GetString(L, 1);

            filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);
            LuaScriptMgr.Push(L, File.Exists(filePath));
        }
        else
        {
            LogParamError("OffLine_IsVoiceFileExist", count);
            LuaScriptMgr.Push(L, false);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_GetVoiceFileSeconds(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            float length = VoiceManager.Instance.OffLineVoice.GetRecFileLength();
            LuaScriptMgr.Push(L, length);
#elif USING_LGVC
            float length = VoiceManager.Instance.OffLineVoice.GetRecFileLength();
            LuaScriptMgr.Push(L, length);
#else
            LuaScriptMgr.Push(L, 0.0f);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_StartRecording(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            int result = VoiceManager.Instance.OffLineVoice.StartRecording();
            LuaScriptMgr.Push(L, result == 0);
#elif USING_LGVC
            int result = VoiceManager.Instance.OffLineVoice.StartRecording();
            LuaScriptMgr.Push(L, result == 0);
#else
            LuaScriptMgr.Push(L, false);
#endif
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_StopRecording(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
            if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
            {
                LuaDLL.lua_pushvalue(L, 1);
                int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX); // cb

                int ret = OffLine_DoStopRecording(L, callbackRef);

                if (ret != 0)
                {
                    LuaScriptMgr.Push(L, false);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }

                LuaScriptMgr.Push(L, true);
            }
            else 
            {
                LogParamError("OffLine_StopRecording", count);
                LuaScriptMgr.Push(L, false);
            }
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    public static int OffLine_DoStopRecording(IntPtr L, int callbackRef)
    {
        int ret = 0;

#if USING_GCLOUDVOICE && UNITY_EDITOR
        Action<int> callback = (code) =>
        {
            if (LuaScriptMgr.Instance.GetLuaState() == null)
                return;

            var oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef); // cb
            LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
            if (LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_settop(L, oldTop);
                return;
            }

            LuaDLL.lua_pushvalue(L, -1);    //-> cb, cb
            LuaScriptMgr.Push(L, code);

            if (!LuaScriptMgr.Instance.GetLuaState().PCall(1, 0)) //-> cb, [err]
            {
                HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
            }
            LuaDLL.lua_settop(L, oldTop);
        };

        var offLineVoice = VoiceManager.Instance.OffLineVoice;
        ret = offLineVoice.StopRecording(callback);

#elif USING_LGVC
        Action<int> callback = (code) =>
        {
            if (LuaScriptMgr.Instance.GetLuaState() == null)
                return;

            var oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef); // cb
            LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
            if (LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_settop(L, oldTop);
                return;
            }

            LuaDLL.lua_pushvalue(L, -1);    //-> cb, cb
            LuaScriptMgr.Push(L, code);

            if (!LuaScriptMgr.Instance.GetLuaState().PCall(1, 0)) //-> cb, [err]
            {
                HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
            }
            LuaDLL.lua_settop(L, oldTop);
        };

        var offLineVoice = VoiceManager.Instance.OffLineVoice;
        ret = offLineVoice.StopRecording(callback);
#endif
        return ret;
    }

    /*
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_StopRecordingUpload(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
            LuaScriptMgr.Push(L, 0.0f);
        }
        else
        {

#if USING_GCLOUDVOICE && UNITY_EDITOR
            float fSecondsLimit = 0;
            if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
            {
                fSecondsLimit = (float)LuaScriptMgr.GetNumber(L, 1);
            }

            var offLineVoice = VoiceManager.Instance.OffLineVoice;
            if (0 != offLineVoice.StopRecording())
            {
                LuaScriptMgr.Push(L, false);
                LuaScriptMgr.Push(L, 0.0f);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            float fLength = 0.0f;
            {
                float fSeconds = offLineVoice.GetRecFileLength();
                fLength = fSeconds;
            }

            if (fLength > fSecondsLimit)
            {
                if (0 != offLineVoice.UploadRecordedFile(10000))
                {
                    LuaScriptMgr.Push(L, false);
                    LuaScriptMgr.Push(L, fLength);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }
            }
            else
            {
                LuaScriptMgr.Push(L, false);
                LuaScriptMgr.Push(L, fLength);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            LuaScriptMgr.Push(L, true);
            LuaScriptMgr.Push(L, fLength);

#else
            LuaScriptMgr.Push(L, false);
            LuaScriptMgr.Push(L, 0.0f);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    */

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_UploadFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            int ret = VoiceManager.Instance.OffLineVoice.UploadRecordedFile(10000);
            LuaScriptMgr.Push(L, ret == 0);
#elif USING_LGVC
            int ret = VoiceManager.Instance.OffLineVoice.UploadRecordedFile();
            LuaScriptMgr.Push(L, ret == 0);
#else
            LuaScriptMgr.Push(L, false);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_DownloadFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, true);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string)))
            {
                string fileId = LuaScriptMgr.GetString(L, 1);
                string filePath = LuaScriptMgr.GetString(L, 2);

                filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);

                var offLineVoice = VoiceManager.Instance.OffLineVoice;
                bool ret = 0 == offLineVoice.DownloadRecordedFile(fileId, filePath, 10000);

                LuaScriptMgr.Push(L, ret);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            else
            {
                LogParamError("OffLine_DownloadFile", count);
                LuaScriptMgr.Push(L, false);
            }
#elif USING_LGVC
            if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string)))
            {
                string fileId = LuaScriptMgr.GetString(L, 1);
                string filePath = LuaScriptMgr.GetString(L, 2);

                filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);

                var offLineVoice = VoiceManager.Instance.OffLineVoice;
                bool ret = 0 == offLineVoice.DownloadRecordedFile(fileId, filePath);

                LuaScriptMgr.Push(L, ret);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            else
            {
                LogParamError("OffLine_DownloadFile", count);
                LuaScriptMgr.Push(L, false);
            }
#else
            LuaScriptMgr.Push(L, false);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_PlayRecordedFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
            {
                string filePath = LuaScriptMgr.GetString(L, 1);

                filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);

                var offLineVoice = VoiceManager.Instance.OffLineVoice;
                if (0 != offLineVoice.PlayRecordedFile(filePath))
                {
                    LuaScriptMgr.Push(L, false);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }

                LuaScriptMgr.Push(L, true);
            }
            else
            {
                LogParamError("OffLine_PlayRecordedFile", count);
                LuaScriptMgr.Push(L, false);
            }
#elif USING_LGVC
            if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
            {
                string filePath = LuaScriptMgr.GetString(L, 1);

                filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);

                var offLineVoice = VoiceManager.Instance.OffLineVoice;
                if (0 != offLineVoice.PlayRecordedFile(filePath))
                {
                    LuaScriptMgr.Push(L, false);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }

                LuaScriptMgr.Push(L, true);
            }
            else
            {
                LogParamError("OffLine_PlayRecordedFile", count);
                LuaScriptMgr.Push(L, false);
            }
#else
            LuaScriptMgr.Push(L, false);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OffLine_StopPlayFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            var offLineVoice = VoiceManager.Instance.OffLineVoice;
            bool ret = 0 != offLineVoice.StopPlayFile();
            LuaScriptMgr.Push(L, ret);
#elif USING_LGVC
            var offLineVoice = VoiceManager.Instance.OffLineVoice;
            bool ret = 0 != offLineVoice.StopPlayFile();
            LuaScriptMgr.Push(L, ret);
#else
            LuaScriptMgr.Push(L, false);
#endif
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_StartRecording(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.Translation)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            string tmpFile = VoiceManager.Instance.TempVoiceFile;
            int result = VoiceManager.Instance.TranslationVoice.StartRecording(tmpFile);
            LuaScriptMgr.Push(L, result == 0);
#else
            LuaScriptMgr.Push(L, false);
#endif
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_StopRecording(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.OffLine)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
            if (count >= 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
            {
                LuaDLL.lua_pushvalue(L, 1);
                //int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX); // cb

                int ret = Translation_DoStopRecording(L, 0);

                if (ret != 0)
                {
                    LuaScriptMgr.Push(L, false);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }

                LuaScriptMgr.Push(L, true);
            }
            else
            {
                LogParamError("Translation_StopRecording", count);
                LuaScriptMgr.Push(L, false);
            }
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    public static int Translation_DoStopRecording(IntPtr L, int callbackRef)
    {
        int ret = 0;

#if USING_GCLOUDVOICE && UNITY_EDITOR
        Action<int> callback = (code) =>
        {
            if (LuaScriptMgr.Instance.GetLuaState() == null)
                return;

            var oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef); // cb
            LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
            if (LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_settop(L, oldTop);
                return;
            }

            LuaDLL.lua_pushvalue(L, -1);    //-> cb, cb
            LuaScriptMgr.Push(L, code);

            if (!LuaScriptMgr.Instance.GetLuaState().PCall(1, 0)) //-> cb, [err]
            {
                HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
            }
            LuaDLL.lua_settop(L, oldTop);
        };

        var translationVoice = VoiceManager.Instance.TranslationVoice;
        ret = translationVoice.StopRecording(callback);

#elif USING_LGVC

#endif
        return ret;
    }

    /*
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_StopRecordingUpload(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.Translation)
        {
            LuaScriptMgr.Push(L, false);
            LuaScriptMgr.Push(L, 0.0f);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            float fSecondsLimit = 0;
            if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
            {
                fSecondsLimit = (float)LuaScriptMgr.GetNumber(L, 1);
            }

            var translationVoice = VoiceManager.Instance.TranslationVoice;
            if (0 != translationVoice.StopRecording())
            {
                LuaScriptMgr.Push(L, false);
                LuaScriptMgr.Push(L, 0.0f);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            float fLength = 0.0f;
            {
                string filePath = VoiceManager.Instance.TempVoiceFile;
                float fSeconds = VoiceManager.Instance.TranslationVoice.GetRecFileLength(filePath);
                fLength = fSeconds;
            }

            if (fLength > fSecondsLimit)
            {
                string tmpFile = VoiceManager.Instance.TempVoiceFile;
                if (0 != translationVoice.UploadRecordedFile(tmpFile, 10000))
                {
                    LuaScriptMgr.Push(L, false);
                    LuaScriptMgr.Push(L, fLength);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }
            }
            else
            {
                LuaScriptMgr.Push(L, false);
                LuaScriptMgr.Push(L, fLength);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }

            LuaScriptMgr.Push(L, true);
            LuaScriptMgr.Push(L, fLength);
#else
            LuaScriptMgr.Push(L, false);
            LuaScriptMgr.Push(L, 0.0f);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
    */

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_SpeechToText(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.Translation)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
            {
                string filePath = LuaScriptMgr.GetString(L, 1);

                //filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);

                var offLineVoice = VoiceManager.Instance.TranslationVoice;
                if (0 != offLineVoice.SpeechToText(filePath))
                {
                    LuaScriptMgr.Push(L, false);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }

                LuaScriptMgr.Push(L, true);
            }
            else
            {
                LogParamError("Translation_PlayRecordedFile", count);
                LuaScriptMgr.Push(L, false);
            }
#else
            LuaScriptMgr.Push(L, false);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_GetVoiceFileSeconds(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.Translation)
        {
            LuaScriptMgr.Push(L, 0.0f);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            string filePath = VoiceManager.Instance.TempVoiceFile;
            float fSeconds = VoiceManager.Instance.TranslationVoice.GetRecFileLength(filePath);
            LuaScriptMgr.Push(L, fSeconds);
#else
            LuaScriptMgr.Push(L, 0.0f);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_DownloadFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.Translation)
        {
            LuaScriptMgr.Push(L, true);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string)))
            {
                string fileId = LuaScriptMgr.GetString(L, 1);
                string filePath = LuaScriptMgr.GetString(L, 2);

                filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);

                var offLineVoice = VoiceManager.Instance.TranslationVoice;
                bool ret = 0 == offLineVoice.DownloadRecordedFile(fileId, filePath, 10000);

                LuaScriptMgr.Push(L, ret);
                return GameUtilWrap.CheckReturnNum(L, count, nRet);
            }
            else
            {
                LogParamError("Translation_DownloadFile", count);
                LuaScriptMgr.Push(L, false);
            }
#else
            LuaScriptMgr.Push(L, false);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_PlayRecordedFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.Translation)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
            {
                string filePath = LuaScriptMgr.GetString(L, 1);

                filePath = Path.Combine(VoiceManager.Instance.VoiceDir, filePath);

                var offLineVoice = VoiceManager.Instance.TranslationVoice;
                if (0 != offLineVoice.PlayRecordedFile(filePath))
                {
                    LuaScriptMgr.Push(L, false);
                    return GameUtilWrap.CheckReturnNum(L, count, nRet);
                }

                LuaScriptMgr.Push(L, true);
            }
            else
            {
                LogParamError("Translation_PlayRecordedFile", count);
                LuaScriptMgr.Push(L, false);
            }
#else
            LuaScriptMgr.Push(L, false);
#endif
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int Translation_StopPlayFile(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (VoiceManager.Instance.VoiceMode != VoiceMode.Translation)
        {
            LuaScriptMgr.Push(L, false);
        }
        else
        {
#if USING_GCLOUDVOICE && UNITY_EDITOR
            var translationVoice = VoiceManager.Instance.TranslationVoice;
            bool ret = 0 != translationVoice.StopPlayFile();
            LuaScriptMgr.Push(L, ret);
#else
            LuaScriptMgr.Push(L, false);
#endif
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }
}