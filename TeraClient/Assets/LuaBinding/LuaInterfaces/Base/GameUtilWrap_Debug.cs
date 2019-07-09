using UnityEngine;
using System;
using LuaInterface;
using Common;
using System.IO;

public static partial class GameUtilWrap
{
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowGameLogs(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 3 && LuaScriptMgr.CheckTypes(L, 1, typeof(int), typeof(GameObject), typeof(GameObject)))
        {
            var logType = (int)LuaScriptMgr.GetNumber(L, 1);
            var textGo = LuaScriptMgr.GetUnityObject<GameObject>(L, 2);
            var contentViewGo = LuaScriptMgr.GetUnityObject<GameObject>(L, 3);
            CLogFile.ShowGameLogs4Debug(logType, textGo, contentViewGo);
        }
        else
        {
            HobaDebuger.LogError("ShowGameLogs: must have 3 params");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int EnableFpsPingDisplay(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool)))
        {
            var enable = LuaScriptMgr.GetBoolean(L, 1);
            CDebugUIMan.Instance.EnableDisplay(enable);
        }
        else
        {
            HobaDebuger.LogError("EnableFPSDisplay: must have 1 params");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int UpdatePingDisplay(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float)))
        {
            var ping = (float)LuaScriptMgr.GetNumber(L, 1);
            CDebugUIMan.Instance.UpdatePing(ping);
        }
        else
        {
            HobaDebuger.LogError("UpdatePing: must have 1 params");
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DrawLine(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 2 && LuaScriptMgr.CheckTypes(L, 1, typeof(LuaTable), typeof(LuaTable))))
        {
            var from = LuaScriptMgr.GetVector3(L, 1);
            var to = LuaScriptMgr.GetVector3(L, 2);
            HobaDebuger.DrawLine(from, to, Color.red);
        }
        else
        {
            LogParamError("DrawLine", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int PrintCurrentTime(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string))))
        {
            var msg = LuaDLL.lua_tostring(L, 1);
            HobaDebuger.LogWarningFormat(msg, System.DateTime.Now.Ticks);
        }
        else
        {
            LogParamError("PrintCurrentTime", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ShowGameInfo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var infotype = LuaScriptMgr.GetString(L, 1);
            if (infotype == "fog")
            {
                string formate = @"RenderSettings.fog = {0}
                RenderSettings.fogColor = {1}
                RenderSettings.fogMode = {2}
                RenderSettings.fogStartDistance = {3}
                RenderSettings.fogEndDistance = {4}
                RenderSettings.ambientSkyColor = {5}
                RenderSettings.ambientEquatorColor = {6}
                RenderSettings.ambientGroundColor = {7}
                RenderSettings.ambientIntensity = {8} ";
                string content = HobaText.Format(formate, RenderSettings.fog, RenderSettings.fogColor,
                    RenderSettings.fogMode, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance,
                    RenderSettings.ambientSkyColor, RenderSettings.ambientEquatorColor,
                    RenderSettings.ambientGroundColor, RenderSettings.ambientIntensity);
                HobaDebuger.LogWarning(content);
            }
            else
            {
                HobaDebuger.LogWarningFormat("Undefined Info type - {0}", infotype);
            }
        }
        else
        {
            LogParamError("ShowGameInfo", count);
        }
        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DebugCommand(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var cmd = LuaScriptMgr.GetString(L, 1);
            EntryPoint.Instance.OnDebugCmd(cmd);
        }
        else
        {
            LogParamError("DebugCommand", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int LuaMemory(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            var luaMemory = LuaScriptMgr.GetString(L, 1);
            string path = Application.dataPath;
            path = path.Substring(0, path.Length - 6) + "Logs/LuaMemory.csv";
            File.WriteAllText(path, luaMemory);
        }
        else
        {
            LogParamError("LuaMemory", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int DebugKey(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        bool ret = false;
#if UNITY_EDITOR
        if (count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string)))
        {
            try
            {
                string str = LuaScriptMgr.GetString(L, 1);
                ret = Input.GetKey(str);
            }
            catch (Exception e)
            {
                HobaDebuger.LogWarning("TestKey:" + e);
            }
        }
        else
        {
            HobaDebuger.LogError("TestKey: param 1 must be string");
        }
#endif
        LuaScriptMgr.Push(L, ret);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int ClearLogOutput(IntPtr L)
    {
#if UNITY_EDITOR
        var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
#endif
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int LogMemoryInfo(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(string))))
        {
            var tag = LuaDLL.lua_tostring(L, 1);
            Util.PrintCurrentMemoryInfo(tag);
        }
        else
        {
            LogParamError("LogMemoryInfo", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetCSharpUsedMemoryCount(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

        if (count == 0)
        {
            var size = Util.GetUsedHeapSize();
            LuaScriptMgr.Push(L, size);
        }
        else
        {
            LuaScriptMgr.Push(L, 0);
            LogParamError("GetCSharpUsedMemoryCount", count);
        }
        return GameUtilWrap.CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetResLoadDelay(IntPtr L)
    {
#if DEBUG_DELAY
        CAssetBundleManager.ResLoadDelay = (float)LuaDLL.lua_tonumber(L, 1);
#endif
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int SetNetLatency(IntPtr L)
    {
        CGameSession.Instance().LatencyMillisecond = (int)LuaDLL.lua_tonumber(L, 1);
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetLogLevel(IntPtr luaState)
    {
        LuaScriptMgr.Push(luaState, (int)HobaDebuger.GameLogLevel);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetLogLevel(IntPtr luaState)
    {
        var count = LuaDLL.lua_gettop(luaState);

        if (count == 1 && LuaScriptMgr.CheckTypes(luaState, 1, typeof(int)))
        {
            var logLevel = LuaScriptMgr.GetNumber(luaState, 1);
            HobaDebuger.GameLogLevel = (LogLevel)logLevel;
            return 0;
        }
        LuaDLL.luaL_error(luaState, "invalid arguments to method: Common.HobaDebuger");
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int ChangeMasterTextureLimit(IntPtr luaState)
    {
        var count = LuaDLL.lua_gettop(luaState);

        if (count == 1 && LuaScriptMgr.CheckTypes(luaState, 1, typeof(int)))
        {
            var level = (int)LuaScriptMgr.GetNumber(luaState, 1);
            if (level < 0) level = 0;
            QualitySettings.masterTextureLimit = level;
            return 0;
        }
        LuaDLL.luaL_error(luaState, "invalid arguments to method: Common.HobaDebuger");
        return 0;
    }

    private static bool _monitorGC;
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int MonitorGC(IntPtr luaState)
    {
        var count = LuaDLL.lua_gettop(luaState);
        if (count == 1 && LuaScriptMgr.CheckTypes(luaState, 1, typeof(bool)))
        {
            var monitor = LuaScriptMgr.GetBoolean(luaState, 1);
            if (monitor && _monitorGC) return 0;
            if (!monitor && !_monitorGC) return 0;
            _monitorGC = monitor;
            if (_monitorGC)
            {
                new GCMonitorObject();
            }
            return 0;
        }
        LuaDLL.luaL_error(luaState, "invalid arguments to method: Common.HobaDebuger");
        return 0;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetCLRMemUseStringCur(IntPtr luaState)
    {
        LuaScriptMgr.Push(luaState, Uint64ToCapacityFormat(System.GC.GetTotalMemory(false)));
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetCLRMemUseStringOnLastGC(IntPtr luaState)
    {
        LuaScriptMgr.Push(luaState, _CLRMemOnLastGc);
        return 1;
    }
    private static string _CLRMemOnLastGc = "";

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetCLRMemGCCount(IntPtr luaState)
    {
        LuaScriptMgr.Push(luaState, _GCCount);
        return 1;
    }
    private static int _GCCount;


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetRegistryTableSize(IntPtr L)
    {
        LuaScriptMgr.Push(L, LuaScriptMgr.Instance.GetRegistryTableSize());
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int PrintRegistryTable(IntPtr L)
    {
        LuaScriptMgr.Push(L, LuaScriptMgr.Instance.PrintRegistryTable());
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetSceneQuality(IntPtr L)
    {
        LuaScriptMgr.Push(L, ScenesManager.Instance.GetSceneQuality());
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetFxLODLevel(IntPtr L)
    {
        LuaScriptMgr.Push(L, (int)GFXConfig.Instance.GetFxLODLevel());
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetScreenCurrentResolution(IntPtr L)
    {
        string str = Screen.currentResolution.width + "*" + Screen.currentResolution.height;
        LuaScriptMgr.Push(L, str);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IsEnableBloomHD(IntPtr L)
    {
        LuaScriptMgr.Push(L, GFXConfig.Instance.IsEnableBloomHD);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int GetMasterTextureLimit(IntPtr L)
    {
        LuaScriptMgr.Push(L, QualitySettings.masterTextureLimit);
        return 1;
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int EnableCmdsInputMode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(bool))))
        {
            var enbaled = LuaScriptMgr.GetBoolean(L, 1);
            InputManager.Instance.IsInCmdsInputting = enbaled;
        }
        else
        {
            LogParamError("EnableCmdsInputMode", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int SetRotationLerpFactor(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        if ((count == 1 && LuaScriptMgr.CheckTypes(L, 1, typeof(float))))
        {
            var factor = (float)LuaScriptMgr.GetNumber(L, 1);
            Behavior.RotationLerpFactor = factor;
        }
        else
        {
            LogParamError("SetRotationLerpFactor", count);
        }

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int DumpCSharpMemory(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 0;

        DateTime now = DateTime.Now;
        string fileName = string.Format("CS-{0}-{1}_{2}_{3}_{4}.txt", now.Month, now.Day, now.Hour, now.Minute, now.Second);
        fileName = Path.Combine(EntryPoint.Instance.DocPath, fileName); 

        UnityMemoryDump.Dump(fileName);

        return CheckReturnNum(L, count, nRet);
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    private static int GetDevelopMode(IntPtr L)
    {
        int count = LuaDLL.lua_gettop(L);
        const int nRet = 1;

#if UNITY_EDITOR
        //LuaScriptMgr.Push(L, true);
        LuaScriptMgr.Push(L, false);
#else
        LuaScriptMgr.Push(L, false);
#endif
        return CheckReturnNum(L, count, nRet);
    }
    

    public enum CapacityShift
    {
        B, //Byte
        K, //KiloByte,
        M, //MegaByte,
        G, //GigaByte,
        T, //TeraByte,
        P, //PetaByte,
        E, //ExaByte,
    }

    public static string Uint64ToCapacityFormat(long capacity)
    {
        var str = "";
        var capacityShiftNames = Enum.GetNames(typeof(CapacityShift));
        for (var i = 0; i < capacityShiftNames.Length; i++)
        {
            var offset = capacity % 1024;
            capacity /= 1024;
            if (offset == 0) continue;
            str = " " + offset + capacityShiftNames[i] + str;
        }
        return str;
    }

    private class GCMonitorObject
    {
        ~GCMonitorObject()
        {
            //OnGCCall();
            // ReSharper disable once ObjectCreationAsStatement
            _CLRMemOnLastGc = Uint64ToCapacityFormat(System.GC.GetTotalMemory(false));
            _GCCount++;
            if (_monitorGC)
            {
                // ReSharper disable once ObjectCreationAsStatement			{
                new GCMonitorObject();
            }
        }
    }
}
