using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class UnityEngine_UI_ImageWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("fillAmount", get_fillAmount, set_fillAmount),
		};

		LuaScriptMgr.RegisterLib(L, "UnityEngine.UI.Image", typeof(UnityEngine.UI.Image), regs, fields, typeof(UnityEngine.UI.MaskableGraphic));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_fillAmount(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Image obj = (UnityEngine.UI.Image)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name fillAmount");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index fillAmount on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.fillAmount);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_fillAmount(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UnityEngine.UI.Image obj = (UnityEngine.UI.Image)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name fillAmount");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index fillAmount on a nil value");
			}
		}

		obj.fillAmount = (float)LuaScriptMgr.GetNumber(L, 3);
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

