using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class CFxOneWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Play", Play),
			new LuaMethod("Stop", Stop),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("IsPlaying", get_IsPlaying, null),
		};

		LuaScriptMgr.RegisterLib(L, "CFxOne", typeof(CFxOne), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_IsPlaying(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CFxOne obj = (CFxOne)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name IsPlaying");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index IsPlaying on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.IsPlaying);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Play(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		CFxOne obj = (CFxOne)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CFxOne");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		obj.Play(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Stop(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		CFxOne obj = (CFxOne)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CFxOne");
		obj.Stop();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Lua_Eq(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		Object arg0 = LuaScriptMgr.GetLuaObject(L, 1) as Object;
		Object arg1 = LuaScriptMgr.GetLuaObject(L, 2) as Object;
		bool o = arg0 == arg1;
		LuaScriptMgr.Push(L, o);
		return 1;
	}
}

