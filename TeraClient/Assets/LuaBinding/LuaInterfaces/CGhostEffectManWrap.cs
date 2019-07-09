using System;
using UnityEngine;
using LuaInterface;

public class CGhostEffectManWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("CreateGhostModel", CreateGhostModel),
			new LuaMethod("ReleaseGhostModel", ReleaseGhostModel),
		};

		LuaScriptMgr.RegisterLib(L, "CGhostEffectMan", regs);
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int CreateGhostModel(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 6);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 1);
		GameObject arg1 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		float arg2 = (float)LuaScriptMgr.GetNumber(L, 3);
		float arg3 = (float)LuaScriptMgr.GetNumber(L, 4);
		float arg4 = (float)LuaScriptMgr.GetNumber(L, 5);
		float arg5 = (float)LuaScriptMgr.GetNumber(L, 6);
		GameObject o = CGhostEffectMan.CreateGhostModel(arg0,arg1,arg2,arg3,arg4,arg5);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int ReleaseGhostModel(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 1);
		GameObject arg1 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		CGhostEffectMan.ReleaseGhostModel(arg0,arg1);
		return 0;
	}
}

