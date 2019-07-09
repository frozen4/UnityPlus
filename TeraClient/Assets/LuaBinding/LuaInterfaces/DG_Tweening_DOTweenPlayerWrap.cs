using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class DG_Tweening_DOTweenPlayerWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Restart", Restart),
			new LuaMethod("Stop", Stop),
			new LuaMethod("GoToStartPos", GoToStartPos),
			new LuaMethod("GoToEndPos", GoToEndPos),
			new LuaMethod("FindAndDoRestart", FindAndDoRestart),
			new LuaMethod("FindAndDoKill", FindAndDoKill),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "DG.Tweening.DOTweenPlayer", typeof(DG.Tweening.DOTweenPlayer), regs, fields, null);
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Restart(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DG.Tweening.DOTweenPlayer obj = (DG.Tweening.DOTweenPlayer)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenPlayer");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.Restart(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Stop(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DG.Tweening.DOTweenPlayer obj = (DG.Tweening.DOTweenPlayer)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenPlayer");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.Stop(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GoToStartPos(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DG.Tweening.DOTweenPlayer obj = (DG.Tweening.DOTweenPlayer)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenPlayer");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.GoToStartPos(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int GoToEndPos(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DG.Tweening.DOTweenPlayer obj = (DG.Tweening.DOTweenPlayer)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenPlayer");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.GoToEndPos(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int FindAndDoRestart(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DG.Tweening.DOTweenPlayer obj = (DG.Tweening.DOTweenPlayer)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenPlayer");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.FindAndDoRestart(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int FindAndDoKill(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		DG.Tweening.DOTweenPlayer obj = (DG.Tweening.DOTweenPlayer)LuaScriptMgr.GetUnityObjectSelf(L, 1, "DG.Tweening.DOTweenPlayer");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.FindAndDoKill(arg0);
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

