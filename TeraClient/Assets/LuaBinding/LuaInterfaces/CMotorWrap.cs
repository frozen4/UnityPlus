using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class CMotorWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("Fly", Fly),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "CMotor", typeof(CMotor), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int Fly(IntPtr L)
	{
		int count = LuaDLL.lua_gettop(L);

		if (count == 6 && LuaScriptMgr.CheckTypes(L, 2, typeof(LuaTable), typeof(LuaTable), typeof(float), typeof(float), typeof(LuaInterface.LuaFunction)))
		{
			CMotor obj = (CMotor)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CMotor");
			Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
			Vector3 arg1 = LuaScriptMgr.GetVector3(L, 3);
			float arg2 = (float)LuaDLL.lua_tonumber(L, 4);
			float arg3 = (float)LuaDLL.lua_tonumber(L, 5);
			LuaFunction arg4 = LuaScriptMgr.ToLuaFunction(L, 6);
			obj.Fly(arg0,arg1,arg2,arg3,arg4);
			return 0;
		}
		else if (count == 6 && LuaScriptMgr.CheckTypes(L, 2, typeof(LuaTable), typeof(GameObject), typeof(float), typeof(float), typeof(LuaInterface.LuaFunction)))
		{
			CMotor obj = (CMotor)LuaScriptMgr.GetUnityObjectSelf(L, 1, "CMotor");
			Vector3 arg0 = LuaScriptMgr.GetVector3(L, 2);
			GameObject arg1 = (GameObject)LuaScriptMgr.GetLuaObject(L, 3);
			float arg2 = (float)LuaDLL.lua_tonumber(L, 4);
			float arg3 = (float)LuaDLL.lua_tonumber(L, 5);
			LuaFunction arg4 = LuaScriptMgr.ToLuaFunction(L, 6);
			obj.Fly(arg0,arg1,arg2,arg3,arg4);
			return 0;
		}
		else
		{
			LuaDLL.luaL_error(L, "invalid arguments to method: CMotor.Fly");
		}

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

