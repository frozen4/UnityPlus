using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class UnityEngine_UI_ToggleWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("isOn", get_isOn, set_isOn),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.UI.Toggle", typeof(UnityEngine.UI.Toggle), regs, fields, typeof(UnityEngine.UI.Selectable));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_isOn(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Toggle obj = (UnityEngine.UI.Toggle)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name isOn");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index isOn on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.isOn);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_isOn(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Toggle obj = (UnityEngine.UI.Toggle)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name isOn");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index isOn on a nil value");
			}
		}

		obj.isOn = LuaScriptMgr.GetBoolean(L, 3);
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

