using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class UIEventListenerWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("SetLuaHandlerLink", SetLuaHandlerLink),
			new LuaMethod("RegisterHandler", RegisterHandler),
			new LuaMethod("RegisterGuideObject", RegisterGuideObject),
			new LuaMethod("UnregisterGuideObject", UnregisterGuideObject),
			new LuaMethod("EnableUIEvent", EnableUIEvent),
			new LuaMethod("AddEvt_PlayAnim", AddEvt_PlayAnim),
			new LuaMethod("AddEvt_PlayDotween", AddEvt_PlayDotween),
			new LuaMethod("AddEvt_SetActive", AddEvt_SetActive),
			new LuaMethod("AddEvt_LuaCB", AddEvt_LuaCB),
			new LuaMethod("AddEvt_PlayFx", AddEvt_PlayFx),
			new LuaMethod("AddEvt_PlaySound", AddEvt_PlaySound),
			new LuaMethod("AddEvt_Shake", AddEvt_Shake),
			new LuaMethod("KillEvt", KillEvt),
			new LuaMethod("KillEvts", KillEvts),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "UIEventListener", typeof(UIEventListener), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int SetLuaHandlerLink(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		LuaTable arg0 = LuaScriptMgr.GetLuaTable(L, 2);
		obj.SetLuaHandlerLink(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RegisterHandler(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		obj.RegisterHandler();
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int RegisterGuideObject(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		obj.RegisterGuideObject(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int UnregisterGuideObject(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		obj.UnregisterGuideObject(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int EnableUIEvent(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		bool arg0 = LuaScriptMgr.GetBoolean(L, 2);
		obj.EnableUIEvent(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddEvt_PlayAnim(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		Animation arg2 = (Animation)LuaScriptMgr.GetUnityObject(L, 4, typeof(Animation));
		string arg3 = LuaScriptMgr.GetLuaString(L, 5);
		int o = obj.AddEvt_PlayAnim(arg0,arg1,arg2,arg3);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddEvt_PlayDotween(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		DG.Tweening.DOTweenPlayer arg2 = (DG.Tweening.DOTweenPlayer)LuaScriptMgr.GetUnityObject(L, 4, typeof(DG.Tweening.DOTweenPlayer));
		string arg3 = LuaScriptMgr.GetLuaString(L, 5);
		int o = obj.AddEvt_PlayDotween(arg0,arg1,arg2,arg3);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddEvt_SetActive(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		GameObject arg2 = (GameObject)LuaScriptMgr.GetUnityObject(L, 4, typeof(GameObject));
		bool arg3 = LuaScriptMgr.GetBoolean(L, 5);
		int o = obj.AddEvt_SetActive(arg0,arg1,arg2,arg3);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddEvt_LuaCB(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 4);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		LuaFunction arg2 = LuaScriptMgr.GetLuaFunction(L, 4);
		int o = obj.AddEvt_LuaCB(arg0,arg1,arg2);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddEvt_PlayFx(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 9);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		string arg2 = LuaScriptMgr.GetLuaString(L, 4);
		GameObject arg3 = (GameObject)LuaScriptMgr.GetUnityObject(L, 5, typeof(GameObject));
		GameObject arg4 = (GameObject)LuaScriptMgr.GetUnityObject(L, 6, typeof(GameObject));
		GameObject arg5 = (GameObject)LuaScriptMgr.GetUnityObject(L, 7, typeof(GameObject));
		float arg6 = (float)LuaScriptMgr.GetNumber(L, 8);
		int arg7 = (int)LuaScriptMgr.GetNumber(L, 9);
		int o = obj.AddEvt_PlayFx(arg0,arg1,arg2,arg3,arg4,arg5,arg6,arg7);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddEvt_PlaySound(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 4);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		string arg2 = LuaScriptMgr.GetLuaString(L, 4);
		int o = obj.AddEvt_PlaySound(arg0,arg1,arg2);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int AddEvt_Shake(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 5);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		float arg1 = (float)LuaScriptMgr.GetNumber(L, 3);
		float arg2 = (float)LuaScriptMgr.GetNumber(L, 4);
		float arg3 = (float)LuaScriptMgr.GetNumber(L, 5);
		int o = obj.AddEvt_Shake(arg0,arg1,arg2,arg3);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int KillEvt(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		int arg0 = (int)LuaScriptMgr.GetNumber(L, 2);
		obj.KillEvt(arg0);
		return 0;
	}

	[MonoPInvokeCallback(typeof(LuaCSFunction))]
	static int KillEvts(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		UIEventListener obj = (UIEventListener)LuaScriptMgr.GetUnityObjectSelf(L, 1, "UIEventListener");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		obj.KillEvts(arg0);
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

