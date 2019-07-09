using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class CBallCurvMotorWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetParam", SetParam),
			new LuaMethod("BallCurvFly", BallCurvFly),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "CBallCurvMotor", typeof(CBallCurvMotor), regs, fields, typeof(CMotor));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetParam(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		CBallCurvMotor obj = (CBallCurvMotor)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CBallCurvMotor");
		float arg0 = (float)LuaScriptMgr.GetNumber(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		obj.SetParam(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int BallCurvFly(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		CBallCurvMotor obj = (CBallCurvMotor)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CBallCurvMotor");
		Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
		GameObject arg1 = (GameObject)LuaScriptMgr.GetUnityObject(L, 3, typeof(GameObject));
		GameObject arg2 = (GameObject)LuaScriptMgr.GetUnityObject(L, 4, typeof(GameObject));
		float arg3 = (float)LuaScriptMgr.GetNumber(L, 5);
		obj.BallCurvFly(arg0,arg1,arg2,arg3);
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

