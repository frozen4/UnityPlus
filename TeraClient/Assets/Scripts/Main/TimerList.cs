using UnityEngine;
using LuaInterface;
using System;
using System.Collections.Generic;
using Common;
using Hoba.ObjectPool;

public class CTimerList
{
    private static int Uniqueid = 1;

    private static readonly ObjectPool<Timer> _TimerPool = new ObjectPool<Timer>(10, 50, () => { return new Timer(); });

    public static void ShowDiagnostics()
    {
#if UNITY_EDITOR
        _TimerPool.ShowDiagnostics("TimerPool");
#endif
    }

    public class Timer : PooledObject
    {
        public int Id;
        public float TTL;
        public float EndTime;
        public int LuaCallbackRef;
        public Action CallbackAction;
        public bool IsOnce;
        public string DebugInfo;

        public void Set(int id, float ttl, float endTime, int luaCbRef, Action cbAction, bool isOnce, string debugInfo)
        {
            Id = id;
            TTL = ttl;
            EndTime = endTime;
            LuaCallbackRef = luaCbRef;
            CallbackAction = cbAction;
            IsOnce = isOnce;
            DebugInfo = debugInfo;
        }

        protected override void OnResetState()
        {
            Set(0, 0, 0, LuaDLL.LUA_NOREF, null, true, string.Empty);
        }
    }

    //private int _CurActiveCount = 0;
    private readonly List<Timer> _List = new List<Timer>();
    private readonly List<Timer> _TempList = new List<Timer>();
    private readonly List<int> _TempDelList = new List<int>();
    private bool _IsTicking = false;

    public int GetTimerCount() { return _List.Count; }

    public Timer GetTimer(int index) { return _List[index]; }

    public int AddTimer(float ttl, bool bOnce, int cb_ref, string debugInfo)   // public int AddTimer(float ttl, bool bOnce, LuaFunction cb)
    {
        Timer tm = _TimerPool.GetObject();
        tm.Id = Uniqueid++;
        tm.TTL = ttl;
       
        //循环与非循环计时需区分
        if (bOnce)
            tm.EndTime = Time.time + ttl;
        else
            tm.EndTime = Time.time;
        tm.LuaCallbackRef = cb_ref;
        tm.CallbackAction = null;
        tm.IsOnce = bOnce;
        tm.DebugInfo = debugInfo;
        if (_IsTicking)
        {
            _TempList.Add(tm);
        }
        else
        {
            _List.Add(tm);
            //_CurActiveCount++;
        }

        return tm.Id;
    }

    public int AddTimer(float ttl, bool bOnce, Action cb)
    {
        Timer tm = _TimerPool.GetObject();
        tm.Id = Uniqueid++;
        tm.TTL = ttl;

        //循环与非循环计时需区分
        if (bOnce)
            tm.EndTime = Time.time + ttl;
        else
            tm.EndTime = Time.time;
        tm.LuaCallbackRef = LuaDLL.LUA_NOREF;
        tm.CallbackAction = cb;
        tm.IsOnce = bOnce;
        tm.DebugInfo = string.Empty;
        if (_IsTicking)
        {
            _TempList.Add(tm);
        }
        else
        {
            _List.Add(tm);
            //_CurActiveCount++;
        }

        return tm.Id;
    }

    public void RemoveTimer(int id)
    {
        if (_IsTicking)
        {
            _TempDelList.Add(id);
        }
        else
        {
            for (int i = 0; i < _List.Count; i++)
            {
                Timer tm = _List[i];

                if (tm.Id == id)
                {
                    //tm.callback.Release();
                    _List.RemoveAt(i);

                    if (tm.LuaCallbackRef != LuaDLL.LUA_NOREF)
                        LuaDLL.luaL_unref(LuaScriptMgr.Instance.GetL(), LuaIndexes.LUA_REGISTRYINDEX, tm.LuaCallbackRef);
                    tm.Dispose();
                    //_CurActiveCount--;
                    return;
                }
            }
        }
    }

    public void ResetTimer(int id)
    {
        for (int i = 0; i < _List.Count; i++)
        {
            Timer tm = _List[i];

            if (tm.Id == id)
            {
                tm.EndTime = Time.time + tm.TTL;
                _List[i] = tm;
                return;
            }
        }
    }

    public void Tick(bool global)
    {
        if (_List.Count == 0)
            return;

        float cur = Time.time;
        _IsTicking = true;
        IntPtr L = LuaScriptMgr.Instance.GetL();
        LuaState LS = LuaScriptMgr.Instance.GetLuaState();

        int i = 0;
        while (i < _List.Count)
        {
            Timer tm = _List[i];

            if (tm.EndTime <= cur)
            {
                if (tm.LuaCallbackRef != LuaDLL.LUA_NOREF)
                {
                    // tm.callback.Call();   -- LuaFunction版本
                    var oldTop = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, tm.LuaCallbackRef); // cb
                    LuaDLL.lua_pushvalue(L, -1);    //cb, cb

                    if (!LS.PCall(0, 0)) //-> cb, [err]
                    {
                        string errorInfo = LuaDLL.lua_tostring(L, -1);
                        if (errorInfo == null) errorInfo = "Unknown Lua Error!";

                        HobaDebuger.LogErrorFormat("LuaScriptException: {0} at {1} Timer Id:{2}, ttl:{3}, once:{4}, debug:{5}",
                            errorInfo, global ? "Global" : "Object", tm.Id, tm.TTL, tm.IsOnce, tm.DebugInfo);
                    }

                    if (tm.IsOnce)
                    {
                        // tm.callback.Dispose();
                        _List.RemoveAt(i);

                        if (tm.LuaCallbackRef != LuaDLL.LUA_NOREF)
                            LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, tm.LuaCallbackRef);
                        tm.Dispose();
                        //_CurActiveCount--;
                    }
                    else
                    {
                        tm.EndTime = tm.EndTime + tm.TTL;
                        _List[i] = tm;
                        i++;
                    }
                    LuaDLL.lua_settop(L, oldTop);
                }
                else if(tm.CallbackAction != null)
                {
                    tm.CallbackAction();

                    if (tm.IsOnce)
                    {
                        _List.RemoveAt(i);

                        if (tm.LuaCallbackRef != LuaDLL.LUA_NOREF)
                            LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, tm.LuaCallbackRef);
                        tm.Dispose();
                        //_CurActiveCount--;
                    }
                    else
                    {
                        tm.EndTime = tm.EndTime + tm.TTL;
                        _List[i] = tm;
                        i++;
                    }
                }
            }
            else
            {
                i++;
            }
        }

        _IsTicking = false;

        if (_TempList.Count > 0)
        {
            _List.AddRange(_TempList);

            //_CurActiveCount += _TempList.Count;
            _TempList.Clear();
        }

        if (_TempDelList.Count > 0)
        {
            for (i = 0; i < _TempDelList.Count; i++)
                RemoveTimer(_TempDelList[i]);

            _TempDelList.Clear();
        }
    }

    public void Clear()
    {
        if (_List.Count == 0)
            return;

        //_CurActiveCount -= _List.Count;

        LuaState LS = LuaScriptMgr.Instance.GetLuaState();
        IntPtr L = LuaScriptMgr.Instance.GetL();
        foreach (var t in _List)
        {
            if(t.LuaCallbackRef != LuaDLL.LUA_NOREF && LS != null && L != IntPtr.Zero)
                LuaDLL.luaL_unref(L, LuaIndexes.LUA_REGISTRYINDEX, t.LuaCallbackRef);
            t.Dispose();
        }

        _List.Clear();
    }
}