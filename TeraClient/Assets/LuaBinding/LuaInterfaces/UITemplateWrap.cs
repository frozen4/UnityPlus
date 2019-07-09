using System;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class UITemplateWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("GetControl", GetControl),
			new LuaMethod("GetLength", GetLength),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("ItemList", get_ItemList, set_ItemList),
		};

		LuaScriptMgr.RegisterLib(L, "UITemplate", typeof(UITemplate), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_ItemList(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UITemplate obj = (UITemplate)o;

		if (obj != null)
		{
			LuaDLL.lua_newtable(L); 
			for (int i = 0; i < obj.ItemList.Count; i++)
			{
				LuaScriptMgr.Push(L, i + 1);
				LuaScriptMgr.PushObject(L, obj.ItemList[i]);
				LuaDLL.lua_settable(L, -3);
			}
		}
		else
		{
			LuaDLL.lua_pushnil(L);
		}

		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_ItemList(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		UITemplate obj = (UITemplate)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name ItemList");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index ItemList on a nil value");
			}
		}

		obj.ItemList = (List<UITemplate.UIControl>)LuaScriptMgr.GetNetObject(L, 3, typeof(List<UITemplate.UIControl>));
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetControl(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		UITemplate obj = (UITemplate)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UITemplate");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		GameObject o = obj.GetControl(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GetLength(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		UITemplate obj = (UITemplate)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UITemplate");
		int o = obj.GetLength();
		LuaScriptMgr.Push(L, o);
		return 1;
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

