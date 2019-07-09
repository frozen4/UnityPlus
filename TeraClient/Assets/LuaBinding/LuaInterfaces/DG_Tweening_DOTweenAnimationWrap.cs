using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class DG_Tweening_DOTweenAnimationWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("DOPause", DOPause),
			new LuaMethod("DORestart", DORestart),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "DG.Tweening.DOTweenAnimation", typeof(DG.Tweening.DOTweenAnimation), regs, fields, null);
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int DOPause(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		DG.Tweening.DOTweenAnimation obj = (DG.Tweening.DOTweenAnimation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenAnimation");
		obj.DOPause();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int DORestart(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DG.Tweening.DOTweenAnimation obj = (DG.Tweening.DOTweenAnimation)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenAnimation");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.DORestart(arg0);
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

