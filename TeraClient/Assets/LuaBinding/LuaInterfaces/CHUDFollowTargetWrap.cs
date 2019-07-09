using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class CHUDFollowTargetWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("AdjustOffset", AdjustOffset),
			new LuaMethod("AdjustOffsetWithScale", AdjustOffsetWithScale),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
			new LuaField("UI_NORMAL_DIST", get_UI_NORMAL_DIST, null),
			new LuaField("UI_CULL_NEAR_DIST", get_UI_CULL_NEAR_DIST, null),
			new LuaField("UI_CULL_FAR_DIST", get_UI_CULL_FAR_DIST, null),
			new LuaField("UI_ADD_SCALE", get_UI_ADD_SCALE, null),
			new LuaField("UI_ADD_SCALE_NEAR_DIST", get_UI_ADD_SCALE_NEAR_DIST, null),
			new LuaField("UI_ADD_SCALE_FAR_DIST", get_UI_ADD_SCALE_FAR_DIST, null),
			new LuaField("FollowTarget", get_FollowTarget, set_FollowTarget),
			new LuaField("Offset", get_Offset, set_Offset),
			new LuaField("PreferScale", get_PreferScale, set_PreferScale),
		};

		LuaScriptMgr.RegisterLib(L, "CHUDFollowTarget", typeof(CHUDFollowTarget), regs, fields, typeof(UnityEngine.EventSystems.UIBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_UI_NORMAL_DIST(IntPtr L)
	{
		LuaScriptMgr.Push(L, CHUDFollowTarget.UI_NORMAL_DIST);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_UI_CULL_NEAR_DIST(IntPtr L)
	{
		LuaScriptMgr.Push(L, CHUDFollowTarget.UI_CULL_NEAR_DIST);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_UI_CULL_FAR_DIST(IntPtr L)
	{
		LuaScriptMgr.Push(L, CHUDFollowTarget.UI_CULL_FAR_DIST);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_UI_ADD_SCALE(IntPtr L)
	{
		LuaScriptMgr.Push(L, CHUDFollowTarget.UI_ADD_SCALE);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_UI_ADD_SCALE_NEAR_DIST(IntPtr L)
	{
		LuaScriptMgr.Push(L, CHUDFollowTarget.UI_ADD_SCALE_NEAR_DIST);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_UI_ADD_SCALE_FAR_DIST(IntPtr L)
	{
		LuaScriptMgr.Push(L, CHUDFollowTarget.UI_ADD_SCALE_FAR_DIST);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_FollowTarget(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CHUDFollowTarget obj = (CHUDFollowTarget)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name FollowTarget");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index FollowTarget on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.FollowTarget);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_Offset(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CHUDFollowTarget obj = (CHUDFollowTarget)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name Offset");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index Offset on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.Offset);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int get_PreferScale(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CHUDFollowTarget obj = (CHUDFollowTarget)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PreferScale");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PreferScale on a nil value");
			}
		}

		LuaScriptMgr.Push(L, obj.PreferScale);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_FollowTarget(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CHUDFollowTarget obj = (CHUDFollowTarget)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name FollowTarget");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index FollowTarget on a nil value");
			}
		}

		obj.FollowTarget = (GameObject)LuaScriptMgr.GetUnityObject(L, 3, typeof(GameObject));
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_Offset(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CHUDFollowTarget obj = (CHUDFollowTarget)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name Offset");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index Offset on a nil value");
			}
		}

		obj.Offset = LuaScriptMgr.GetVector3(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int set_PreferScale(IntPtr L)
	{
		object o = LuaScriptMgr.GetLuaObject(L, 1);
		CHUDFollowTarget obj = (CHUDFollowTarget)o;

		if (obj == null)
		{
			LuaTypes types = LuaDLL.lua_type(L, 1);

			if (types == LuaTypes.LUA_TTABLE)
			{
				LuaDLL.luaL_error(L, "unknown member name PreferScale");
			}
			else
			{
				LuaDLL.luaL_error(L, "attempt to index PreferScale on a nil value");
			}
		}

		obj.PreferScale = LuaScriptMgr.GetVector3(L, 3);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AdjustOffset(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		CHUDFollowTarget obj = (CHUDFollowTarget)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CHUDFollowTarget");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		obj.AdjustOffset(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AdjustOffsetWithScale(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 4);
		CHUDFollowTarget obj = (CHUDFollowTarget)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CHUDFollowTarget");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
		obj.AdjustOffsetWithScale(arg0,arg1,arg2);
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

