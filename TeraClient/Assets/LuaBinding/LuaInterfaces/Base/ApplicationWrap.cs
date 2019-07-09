using System;
using UnityEngine;
using LuaInterface;

public class ApplicationWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("platform", get_platform, null),
			new LuaField("isMobilePlatform", get_isMobilePlatform, null),
			new LuaField("backgroundLoadingPriority", get_backgroundLoadingPriority, set_backgroundLoadingPriority),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.Application", typeof(Application), regs, fields, typeof(object));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_platform(IntPtr L)
	{
		LuaScriptMgr.Push(L, (int)Application.platform);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_isMobilePlatform(IntPtr L)
	{
		LuaScriptMgr.Push(L, Application.isMobilePlatform);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_backgroundLoadingPriority(IntPtr L)
	{
		LuaScriptMgr.Push(L, (int)Application.backgroundLoadingPriority);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_backgroundLoadingPriority(IntPtr L)
	{
		Application.backgroundLoadingPriority = (ThreadPriority)LuaScriptMgr.GetNumber(L, 3);
		return 0;
	}
}

