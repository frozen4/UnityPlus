using UnityEngine;
using System;
using LuaInterface;
using Common;
using System.Collections.Generic;
using System.IO;

public static partial class GameUtilWrap
{
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetConfigPlatform(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, CPlatformConfig.GetPlatForm());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetConfigLocale(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, CPlatformConfig.GetLocale());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetResponseDeviceString(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, OSUtility.GetResponseDeviceString());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetResponseOSVersionString(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, OSUtility.GetResponseOSVersionString());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetResponseMACString(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, OSUtility.GetResponseMACString());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetLargeMemoryLimit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, OSUtility.GetLargeMemoryLimit());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetMemoryLimit(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, OSUtility.GetMemoryLimit());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int getTotalPss(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, OSUtility.getTotalPss());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int getMemotryStats(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        LuaScriptMgr.Push(L, OSUtility.getMemotryStats());
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ReadPersistentConfig(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        float fVolume = 0.5f;
        if (EntryPoint.Instance.GamePersistentConfigParams.ReadFromFile())
            fVolume = EntryPoint.Instance.GamePersistentConfigParams.BGMVolume;

        LuaScriptMgr.Push(L, fVolume);
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int WritePersistentConfig(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float fVolume = (float)LuaScriptMgr.GetNumber(L, 1);
            EntryPoint.Instance.GamePersistentConfigParams.BGMVolume = fVolume;
            EntryPoint.Instance.GamePersistentConfigParams.WriteToFile();
        }
        else
        {
            LogParamError("WritePersistentConfig", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetRuntimePlatform(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            LuaDLL.lua_pushnumber(L, (int)Application.platform);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetRuntimePlatform", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GC(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {

            System.GC.Collect();

            var unloadAssets = LuaScriptMgr.GetBoolean(L, 1);
            if(unloadAssets)
                Resources.UnloadUnusedAssets();
        }
        else
        {
            LogParamError("GC", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetOpenUDID(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            string udid = OSUtility.GetOpenUDID();
            LuaScriptMgr.Push(L, udid);
        }
        else
        {
            LogParamError("GetOpenUDID", count);
            LuaScriptMgr.Push(L, string.Empty);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetVirtualMemoryUsedSize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        long mem = OSUtility.GetVirtualMemoryUsedSize();
        LuaScriptMgr.Push(L, mem);

        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetPhysMemoryUsedSize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        long mem = OSUtility.GetPhysMemoryUsedSize();
        LuaScriptMgr.Push(L, mem);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OpenUrl(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1) && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var strUrl = LuaScriptMgr.GetString(L, 1);
            OSUtility.OpenUrl(strUrl);
        }
        else
        {
            LogParamError("OpenUrl", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int CopyTextToClipboard(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1) && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var strText = LuaScriptMgr.GetString(L, 1);
            OSUtility.CopyTextToClipboard(strText);
        }
        else
        {
            LogParamError("CopyTextToClipboard", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetResourceBasePath(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            string path = EntryPoint.Instance.ResPath;
            LuaScriptMgr.Push(L, path);
        }
        else
        {
            LogParamError("GetResourceBasePath", count);
            LuaScriptMgr.Push(L, string.Empty);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetDocumentPath(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            string path = EntryPoint.Instance.DocPath;
            LuaScriptMgr.Push(L, path);
        }
        else
        {
            LogParamError("GetDocumentPath", count);
            LuaScriptMgr.Push(L, string.Empty);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetVoiceDir(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            string path = EntryPoint.Instance.VoiceDir;
            LuaScriptMgr.Push(L, path);
        }
        else
        {
            LogParamError("GetVoiceDir", count);
            LuaScriptMgr.Push(L, string.Empty);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCustomPicDir(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            string path = EntryPoint.Instance.CustomPicDir;
            LuaScriptMgr.Push(L, path);
        }
        else
        {
            LogParamError("GetCustomPicDir", count);
            LuaScriptMgr.Push(L, string.Empty);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetScreenSize(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 2;

        if (count == 0)
        {
            LuaScriptMgr.Push(L, Screen.width);
            LuaScriptMgr.Push(L, Screen.height);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetScreenSize", count);
            LuaDLL.lua_pushnil(L);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetClientTime(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            var time = EntryPoint.GetClientTime();
            //var timestamp = Common.Net.Profiler.GetUtcTotalMilliseconds();
            LuaScriptMgr.Push(L, time);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetClientTime", count);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetServerTime(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            var time = EntryPoint.GetServerTime();
            LuaScriptMgr.Push(L, time);
            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("GetServerTime", count);
            LuaScriptMgr.Push(L, 0);
            return CheckReturnNum(L, count, nRet);
        }
    }

    /// <summary>
    /// 获取当前设备时间
    /// 假设玩家更改时间会导致前端显示出错
    /// </summary>
    /// <param name="L"></param>
    /// <returns></returns>
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCurrentSecondTime(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            DateTime now = DateTime.Now;
            var time = now.Hour * 3600 + now.Minute * 60 + now.Second;
            LuaScriptMgr.Push(L, time);
        }
        else
        {
            LogParamError("GetCurrentSecondTime", count);
            LuaScriptMgr.Push(L, 0);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetDateTimeNowTicks(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            var ticks = DateTime.Now.Ticks;
            LuaScriptMgr.Push(L, ticks);
        }
        else
        {
            LogParamError("GetDateTimeNowTicks", count);
            LuaScriptMgr.Push(L, 0);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetServerTimeGap(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1) && LuaScriptMgr.CheckTypes(L, 1, typeof(double)))
        {
            var serverTimeGap = LuaScriptMgr.GetNumber(L, 1);
            EntryPoint._ServerTimeGap = serverTimeGap;
        }
        else
        {
            LogParamError("SetServerTimeGap", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int UploadPicture(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(string), typeof(LuaFunction)))
        {
            string strFile = LuaScriptMgr.GetString(L, 1);
            string strPlayerGuid = LuaScriptMgr.GetString(L, 2);

            LuaDLL.lua_pushvalue(L, 3);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);

            EntryPoint.UploadPictureCallback callback = null;
            callback = delegate(string filename, string error, bool success)
            {
                if (LuaScriptMgr.Instance.GetLuaState() == null || callbackRef == 0)
                    return;
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                if (LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_settop(L, oldTop);
                    return;
                }
                LuaDLL.lua_pushvalue(L, -1);
                if (filename != null)
                    LuaScriptMgr.Push(L, filename);
                else
                    LuaDLL.lua_pushnil(L);

                if (error != null)
                    LuaScriptMgr.Push(L, error);
                else
                    LuaDLL.lua_pushnil(L);

                LuaDLL.lua_pushboolean(L, success);

                if (!LuaScriptMgr.Instance.GetLuaState().PCall(3, 0))
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                LuaDLL.lua_settop(L, oldTop);
            };

            EntryPoint.Instance.UploadPicture(strFile, strPlayerGuid, callback);

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("UploadPicture", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DownloadPicture(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaFunction)))
        {
            string strFile = LuaScriptMgr.GetString(L, 1);

            LuaDLL.lua_pushvalue(L, 2);
            int callbackRef = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);

            EntryPoint.DownloadPictureCallback callback = null;
            callback = delegate(string filename, int retCode, string error)
            {
                if (LuaScriptMgr.Instance.GetLuaState() == null || callbackRef == 0)
                    return;
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, callbackRef);
                if (LuaDLL.lua_isnil(L, -1))
                {
                    LuaDLL.lua_settop(L, oldTop);
                    return;
                }
                LuaDLL.lua_pushvalue(L, -1);

                if (filename != null)
                    LuaScriptMgr.Push(L, filename);
                else
                    LuaDLL.lua_pushnil(L);

                LuaScriptMgr.Push(L, retCode);

                if (error != null)
                    LuaScriptMgr.Push(L, error);
                else
                    LuaDLL.lua_pushnil(L);

                if (!LuaScriptMgr.Instance.GetLuaState().PCall(3, 0))
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                LuaDLL.lua_settop(L, oldTop);
            };

            EntryPoint.Instance.DownloadPicture(strFile, 10000, callback);

            return CheckReturnNum(L, count, nRet);
        }
        else
        {
            LogParamError("DownloadPicture", count);
            return CheckReturnNum(L, count, nRet);
        }
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int RequestServerList(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaFunction)))
        {
            var callback = LuaScriptMgr.GetLuaFunction(L, 1);
            EntryPoint.Instance.RequestServerList((isSuccessful) =>
            {
                if (callback != null)
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                }
            });
        }
        else
        {
            LogParamError("RequestServerList", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetServerList(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && (LuaScriptMgr.CheckTypes(L, 1, typeof(bool))))
        {
            int key = 1;
            LuaDLL.lua_newtable(L);

            var normalServerList = EntryPoint.Instance.ServerInfoList;
            foreach (var v in normalServerList)
            {
                var serverInfo = v;
                if (!serverInfo.show)
                    continue;

                LuaScriptMgr.Push(L, key);
                {
                    LuaDLL.lua_newtable(L);

                    LuaScriptMgr.Push(L, "name");
                    LuaScriptMgr.Push(L, serverInfo.name);
                    LuaDLL.lua_settable(L, -3);

                    LuaScriptMgr.Push(L, "ip");
                    LuaScriptMgr.Push(L, serverInfo.ip);
                    LuaDLL.lua_settable(L, -3);

                    LuaScriptMgr.Push(L, "port");
                    LuaScriptMgr.Push(L, serverInfo.port);
                    LuaDLL.lua_settable(L, -3);

                    LuaScriptMgr.Push(L, "recommend");
                    LuaScriptMgr.Push(L, serverInfo.recommend);
                    LuaDLL.lua_settable(L, -3);

                    LuaScriptMgr.Push(L, "state");
                    LuaScriptMgr.Push(L, serverInfo.state);
                    LuaDLL.lua_settable(L, -3);

                    LuaScriptMgr.Push(L, "zoneId");
                    LuaScriptMgr.Push(L, serverInfo.zoneId);
                    LuaDLL.lua_settable(L, -3);

                    LuaScriptMgr.Push(L, "newFlag");
                    LuaScriptMgr.Push(L, serverInfo.newFlag);
                    LuaDLL.lua_settable(L, -3);

                    LuaScriptMgr.Push(L, "roleCreateDisable");
                    LuaScriptMgr.Push(L, serverInfo.roleCreateDisable);
                    LuaDLL.lua_settable(L, -3);
                }
                LuaDLL.lua_settable(L, -3);
                key++;
            }

            EntryPoint.Instance.ServerInfoList.Clear();

            bool addTest = LuaScriptMgr.GetBoolean(L, 1);
            if (addTest)
            {
                var testSeverList = EntryPoint.Instance.TestServerInfoList;
                foreach (var v in testSeverList)
                {
                    var serverInfo = v;
                    if (!serverInfo.show)
                        continue;

                    LuaScriptMgr.Push(L, key);
                    {
                        LuaDLL.lua_newtable(L);

                        LuaScriptMgr.Push(L, "name");
                        LuaScriptMgr.Push(L, serverInfo.name);
                        LuaDLL.lua_settable(L, -3);

                        LuaScriptMgr.Push(L, "ip");
                        LuaScriptMgr.Push(L, serverInfo.ip);
                        LuaDLL.lua_settable(L, -3);

                        LuaScriptMgr.Push(L, "port");
                        LuaScriptMgr.Push(L, serverInfo.port);
                        LuaDLL.lua_settable(L, -3);

                        LuaScriptMgr.Push(L, "recommend");
                        LuaScriptMgr.Push(L, serverInfo.recommend);
                        LuaDLL.lua_settable(L, -3);

                        LuaScriptMgr.Push(L, "state");
                        LuaScriptMgr.Push(L, serverInfo.state);
                        LuaDLL.lua_settable(L, -3);

                        LuaScriptMgr.Push(L, "zoneId");
                        LuaScriptMgr.Push(L, serverInfo.zoneId);
                        LuaDLL.lua_settable(L, -3);

                        LuaScriptMgr.Push(L, "newFlag");
                        LuaScriptMgr.Push(L, serverInfo.newFlag);
                        LuaDLL.lua_settable(L, -3);

                        LuaScriptMgr.Push(L, "roleCreateDisable");
                        LuaScriptMgr.Push(L, serverInfo.roleCreateDisable);
                        LuaDLL.lua_settable(L, -3);
                    }
                    LuaDLL.lua_settable(L, -3);
                    key++;
                }
            }
        }
        else
        {
            LogParamError("GetServerList", count);

            LuaDLL.lua_newtable(L);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int RequestAccountRoleList(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string), typeof(LuaFunction)))
        {
            string account = LuaScriptMgr.GetString(L, 1);
            var callback = LuaScriptMgr.GetLuaFunction(L, 2);
            EntryPoint.Instance.RequestAccountRoleList(account, (isSuccessful)=>
            {
                if (callback != null)
                {
                    callback.Call(isSuccessful);
                    callback.Release();
                }
            });
        }
        else
        {
            LogParamError("RequestAccountRoleList", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetAccountRoleList(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            var accountRoleList = EntryPoint.Instance.AccountRoleInfoList;
            if (accountRoleList != null)
            {
                LuaDLL.lua_newtable(L);

                using (var iter = accountRoleList.GetEnumerator())
                {
                    int key = 1;
                    while (iter.MoveNext())
                    {
                        AccountRoleInfo accountRoleInfo = iter.Current;
                        if (accountRoleInfo == null) continue;

                        LuaScriptMgr.Push(L, key);
                        {
                            LuaDLL.lua_newtable(L);

                            LuaScriptMgr.Push(L, "roleId");
                            LuaScriptMgr.Push(L, accountRoleInfo.roleId);
                            LuaDLL.lua_settable(L, -3);

                            LuaScriptMgr.Push(L, "level");
                            LuaScriptMgr.Push(L, accountRoleInfo.level);
                            LuaDLL.lua_settable(L, -3);

                            LuaScriptMgr.Push(L, "name");
                            LuaScriptMgr.Push(L, accountRoleInfo.name);
                            LuaDLL.lua_settable(L, -3);

                            LuaScriptMgr.Push(L, "profession");
                            LuaScriptMgr.Push(L, accountRoleInfo.profession);
                            LuaDLL.lua_settable(L, -3);

                            LuaScriptMgr.Push(L, "customSet");
                            LuaScriptMgr.Push(L, accountRoleInfo.customSet);
                            LuaDLL.lua_settable(L, -3);

                            LuaScriptMgr.Push(L, "zoneId");
                            LuaScriptMgr.Push(L, accountRoleInfo.zoneId);
                            LuaDLL.lua_settable(L, -3);
                        }
                        LuaDLL.lua_settable(L, -3);
                        key++;
                    }
                }
            }
        }
        else
        {
            LuaDLL.lua_pushnil(L);
            LogParamError("GetAccountRoleList", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetOrderZoneId(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            LuaScriptMgr.Push(L, AccountRoleInfo.OrderZoneId);
        }
        else
        {
            LogParamError("GetOrderZoneId", count);
            LuaScriptMgr.Push(L, -1);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsCustomPicFileExist(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string filePath = LuaScriptMgr.GetString(L, 1);

            filePath = Path.Combine(EntryPoint.Instance.CustomPicDir, filePath);
            LuaScriptMgr.Push(L, File.Exists(filePath));
        }
        else
        {
            LogParamError("IsCustomPicFileExist", count);
            LuaScriptMgr.Push(L, false);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetUserLanguageCode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        string lan = EntryPoint.Instance.GetUserLanguageCode();
        LuaScriptMgr.Push(L, lan);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int WriteUserLanguageCode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string lan = LuaScriptMgr.GetString(L, 1);

            bool ret = EntryPoint.Instance.WriteUserLanguageCode(lan);
            LuaScriptMgr.Push(L, ret);
        }
        else
        {
            LogParamError("WriteUserLanguageCode", count);
            LuaScriptMgr.Push(L, false);
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetUserLanguagePostfix(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            bool bAssetBundle = LuaScriptMgr.GetBoolean(L, 1);

            string postfix = EntryPoint.Instance.GetUserLanguagePostfix(bAssetBundle);
            LuaScriptMgr.Push(L, postfix);
        }
        else
        {
            LogParamError("GetUserLanguagePostfix", count);
            LuaScriptMgr.Push(L, "");
        }

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int HasTouchOne(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool HasTouchOne = InputManager.HasTouchOne();
        LuaScriptMgr.Push(L, HasTouchOne);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ResetSleepingCD(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        //if (count > 0 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        //{
        //    float count_down = (float)LuaScriptMgr.GetNumber(L, 1);
        //    InputManager.Instance.SetSleepCounts(count_down);
        //}

        InputManager.Instance.ResetSleepCD();
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnterSleeping(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        InputManager.Instance.EnableSleepCD(false);
        Main.SetSleepingState(true);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int LeaveSleeping(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        Main.SetSleepingState(false);
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int StartSleepingCD(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        InputManager.Instance.EnableSleepCD(true);
        return CheckReturnNum(L, count, nRet);
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int StopSleepingCD(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        InputManager.Instance.EnableSleepCD(false);
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetSleepingCD(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            float cdTime = (float)LuaScriptMgr.GetNumber(L, 1);
            InputManager.Instance.SetSleepingCD(cdTime);
        }
        else
        {
            LogParamError("SetSleepingCD", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int SetServerOpenTime(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            string startTime = LuaScriptMgr.GetString(L, 1);
            double currentTime = EntryPoint.GetServerTime();
            DynamicEffectManager.Instance.SetServerStartTime(startTime, currentTime);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetABVersion(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            string abVersion = EntryPoint.Instance.ABVersion;
            LuaScriptMgr.Push(L, abVersion);
        }
        else
        {
            LogParamError("GetABVersion", count);
            LuaScriptMgr.Push(L, string.Empty);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetClientVersion(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            string clientVersion = EntryPoint.Instance.ClientVersion;
            LuaScriptMgr.Push(L, clientVersion);
        }
        else
        {
            LogParamError("GetClientVersion", count);
            LuaScriptMgr.Push(L, string.Empty);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OpenPhoto(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

#if UNITY_ANDROID
        AndroidUtil.OpenPhoto();
#elif UNITY_IPHONE
        IOSUtil.iosOpenPhotoAlbums(false);
#elif UNITY_STANDALONE_WIN
        OpenFileName.OpenPhoto();
#endif

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int OpenCamera(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

#if UNITY_ANDROID
        AndroidUtil.OpenCamera();
#elif UNITY_IPHONE
        IOSUtil.iosOpenCamera(true);
#endif

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int HasRecordAudioPermission(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool ret = OSUtility.HasRecordAudioPermission();
        LuaScriptMgr.Push(L, ret);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RequestRecordAudioPermission(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        OSUtility.RequestRecordAudioPermission();

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int HasCameraPermission(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool ret = OSUtility.HasCameraPermission();
        LuaScriptMgr.Push(L, ret);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RequestCameraPermission(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        OSUtility.RequestCameraPermission();

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int HasPhotoPermission(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool ret = OSUtility.HasPhotoPermission();
        LuaScriptMgr.Push(L, ret);

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int RequestPhotoPermission(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        OSUtility.RequestPhotoPermission();

        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ReportUserId(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var userId = LuaScriptMgr.GetString(L, 1);
            CLogReport.ReportUserId(userId);
        }
        else
        {
            LogParamError("ReportUserId", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ReportRoleInfo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var info = LuaScriptMgr.GetString(L, 1);
            CLogReport.ReportRoleInfo(info);
        }
        else
        {
            LogParamError("ReportRoleInfo", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ResetLogReporter(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 0)
        {
            CLogReport.Reset();
        }
        else
        {
            LogParamError("ResetLogReporter", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetNetworkStatus(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            int status = (int)Util.GetNetworkStatus();
            LuaScriptMgr.Push(L, status);
        }
        else
        {
            LogParamError("GetNetworkStatus", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetBatteryLevel(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            float batteryLv = Util.GetBatteryLevel();
            LuaScriptMgr.Push(L, batteryLv);
        }
        else
        {
            LogParamError("GetBatteryLevel", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetBatteryStatus(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            int batteryStatus = (int)Util.GetBatteryStatus();
            LuaScriptMgr.Push(L, batteryStatus);
        }
        else
        {
            LogParamError("GetBatteryStatus", count);
            LuaScriptMgr.Push(L, false);
        }
        return CheckReturnNum(L, count, nRet);
    }



}
