using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class CFxFollowTargetWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Apply", Apply),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "CFxFollowTarget", typeof(CFxFollowTarget), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Apply(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		CFxFollowTarget obj = (CFxFollowTarget)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CFxFollowTarget");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		GameObject arg1 = (GameObject)LuaScriptMgr.GetUnityObject(L, 3, typeof(GameObject));
		float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
		float arg3 = (float)LuaScriptMgr.GetNumber(L, 5);
		obj.Apply(arg0,arg1,arg2,arg3);
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

