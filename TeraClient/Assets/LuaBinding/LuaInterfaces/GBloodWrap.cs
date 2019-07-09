using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class GBloodWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetParam", SetParam),
			new LuaMethod("SetValue", SetValue),
			new LuaMethod("SetGuardValue", SetGuardValue),
			new LuaMethod("MakeInvalid", MakeInvalid),
			new LuaMethod("SetValueImmediately", SetValueImmediately),
			new LuaMethod("SetLineStyle", SetLineStyle),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "GBlood", typeof(GBlood), regs, fields, typeof(GBase));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetParam(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		GBlood obj = (GBlood)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GBlood");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		obj.SetParam(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetValue(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GBlood obj = (GBlood)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GBlood");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		obj.SetValue(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetGuardValue(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GBlood obj = (GBlood)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GBlood");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		obj.SetGuardValue(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int MakeInvalid(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		GBlood obj = (GBlood)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GBlood");
		obj.MakeInvalid();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetValueImmediately(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GBlood obj = (GBlood)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GBlood");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		obj.SetValueImmediately(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetLineStyle(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		GBlood obj = (GBlood)LuaScriptMgr.GetUnityObjectSelf(L, 1, "GBlood");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.SetLineStyle(arg0);
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

