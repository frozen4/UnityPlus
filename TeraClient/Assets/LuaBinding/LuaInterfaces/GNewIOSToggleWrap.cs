using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GNewIOSToggleWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("ToggleValue", ToggleValue),
			new LuaMethod("SetValue", SetValue),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("Value", get_Value, set_Value),
		};

		LuaScriptMgr.RegisterLib(L, "GNewIOSToggle", typeof(GNewIOSToggle), regs, fields, typeof(UnityEngine.UI.Selectable));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_Value(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewIOSToggle obj = (GNewIOSToggle)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name Value");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index Value on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.Value);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_Value(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		GNewIOSToggle obj = (GNewIOSToggle)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name Value");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index Value on a nil value");
			}
		}

		obj.Value = LuaScriptMgr.GetBoolean(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ToggleValue(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GNewIOSToggle obj = (GNewIOSToggle)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewIOSToggle");
		obj.ToggleValue();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetValue(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GNewIOSToggle obj = (GNewIOSToggle)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GNewIOSToggle");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		bool arg1 = LuaScriptMgr.GetBoolean(L, 3);
		obj.SetValue(arg0,arg1);
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

