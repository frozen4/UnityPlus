using System;
using UnityEngine;
using LuaInterface;

public class SystemInfoWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
		};

		LuaField[] fields = new LuaField[]
		{
            new LuaField("processorCount", get_processorCount, null),
            new LuaField("processorFrequency", get_processorFrequency, null),
			new LuaField("systemMemorySize", get_systemMemorySize, null),
			new LuaField("graphicsDeviceName", get_graphicsDeviceName, null),
			new LuaField("deviceModel", get_deviceModel, null),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.SystemInfo", typeof(SystemInfo), regs, fields, typeof(object));
	}

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_processorCount(IntPtr L)
    {
        LuaScriptMgr.Push(L, SystemInfo.processorCount);
        return 1;
    }
    
    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    static int get_processorFrequency(IntPtr L)
    {
        LuaScriptMgr.Push(L, SystemInfo.processorFrequency);
        return 1;
    }

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_systemMemorySize(IntPtr L)
	{
		LuaScriptMgr.Push(L, SystemInfo.systemMemorySize);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_graphicsDeviceName(IntPtr L)
	{
		LuaScriptMgr.Push(L, SystemInfo.graphicsDeviceName);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_deviceModel(IntPtr L)
	{
		LuaScriptMgr.Push(L, SystemInfo.deviceModel);
		return 1;
	}
}

