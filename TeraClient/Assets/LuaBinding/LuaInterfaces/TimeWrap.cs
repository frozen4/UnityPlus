using System;
using UnityEngine;
using LuaInterface;

public class TimeWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("time", get_time, null),
			new LuaField("frameCount", get_frameCount, null),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.Time", typeof(Time), regs, fields, typeof(object));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_time(IntPtr L)
	{
		LuaScriptMgr.Push(L, Time.time);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_frameCount(IntPtr L)
	{
		LuaScriptMgr.Push(L, Time.frameCount);
		return 1;
	}
}

