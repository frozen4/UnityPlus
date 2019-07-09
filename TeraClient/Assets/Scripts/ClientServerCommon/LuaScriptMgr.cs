using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using System.Collections;
using Common;

public partial class LuaScriptMgr : Singleton<LuaScriptMgr>, GameLogic.ITickLogic
{
    #region 变量区
    private LuaState _lua;

    HashSet<string> _fileList = null;
    Dictionary<string, LuaBase> _dict = null;

    int unpackVec3 = 0;    
    int unpackVec2 = 0;
    int unpackVec4 = 0;
    int unpackQuat = 0;
    int unpackColor = 0;
    int unpackRay = 0;
    //int unpackRect = 0;
    int unpackBounds = 0;

    int packVec3 = 0;
    int packVec2 = 0;
    int packVec4 = 0;
    int packQuat = 0;
    LuaFunction packTouch = null;
    int packRay = 0;
    LuaFunction packRaycastHit = null;
    int packColor = 0;
    int packRect = 0;
    int packBounds = 0;

 	int enumMetaRef = 0;
    int typeMetaRef = 0;
    int delegateMetaRef = 0;
    int iterMetaRef = 0;
    int arrayMetaRef = 0;
    public static LockFreeQueue<LuaRef> _refGCList = new LockFreeQueue<LuaRef>(1024);    
    static ObjectTranslator _translator = null;    

    static HashSet<Type> checkBaseType = new HashSet<Type>();
	static LuaFunction traceback = null;
    string luaIndex =
    @"        
        local rawget = rawget
        local rawset = rawset
        local getmetatable = getmetatable      
        local type = type  
        local function index(obj,name)  
            local o = obj            
            local meta = getmetatable(o)            
            local parent = meta
            local v = nil
            
            while meta~= nil do
                v = rawget(meta, name)
                
                if v~= nil then
                    if parent ~= meta then rawset(parent, name, v) end

                    local t = type(v)

                    if t == 'function' then                    
                        return v
                    else
                        local func = v[1]
                
                        if func ~= nil then
                            return func(obj)                         
                        end
                    end
                    break
                end
                
                meta = getmetatable(meta)
            end

           error('unknown member name '..name, 2)
           return nil	        
        end
        return index";

    string luaNewIndex =
    @"
        local rawget = rawget
        local getmetatable = getmetatable   
        local rawset = rawset     
        local function newindex(obj, name, val)            
            local meta = getmetatable(obj)            
            local parent = meta
            local v = nil
            
            while meta~= nil do
                v = rawget(meta, name)
                
                if v~= nil then
                    if parent ~= meta then rawset(parent, name, v) end
                    local func = v[2]
                    
                    if func ~= nil then                        
                        return func(obj, nil, val)                        
                    end
                    break
                end
                
                meta = getmetatable(meta)
            end  
       
            error('field or property '..name..' does not exist', 2)
            return nil		
        end
        return newindex";

    string luaTableCall =
    @"
        local rawget = rawget
        local getmetatable = getmetatable     

        local function call(obj, ...)
            local meta = getmetatable(obj)
            local fun = rawget(meta, 'New')
            
            if fun ~= nil then
                return fun(...)
            else
                error('unknow function __call',2)
            end
        end

        return call
    ";

    string luaEnumIndex =
    @"
        local rawget = rawget                
        local getmetatable = getmetatable         

        local function indexEnum(obj,name)
            local v = rawget(obj, name)
            
            if v ~= nil then
                return v
            end

            local meta = getmetatable(obj)  
            local func = rawget(meta, name)            
            
            if func ~= nil then
                v = func()
                rawset(obj, name, v)
                return v
            else
                error('field '..name..' does not exist', 2)
            end
        end

        return indexEnum
    ";

    #endregion

    public LuaScriptMgr()
    {

#if !SERVER_USE
        LuaStatic.Load = EntryPoint.Instance.CustomLoader;
#else

#endif
        _lua = new LuaState();
        _translator = _lua.GetTranslator();

        LuaDLL.setup_luastate(_lua.L);

        LuaDLL.luaopen_pb(_lua.L);
        LuaDLL.luaopen_lfs(_lua.L); 

        LuaDLL.luaopen_bit(_lua.L);

        LuaDLL.tolua_openlibs(_lua.L);

        LuaDLL.luaopen_profiler(_lua.L);

        LuaDLL.luaopen_LuaUInt64(_lua.L);

        LuaDLL.luaopen_SkillCollision(_lua.L);

        LuaDLL.luaopen_cbinary(_lua.L);

#if UNITY_STANDALONE_WIN

        LuaDLL.luaopen_socket_core(_lua.L);

        LuaDLL.luaopen_mime_core(_lua.L);

        LuaDLL.luaopen_snapshot(_lua.L);
#endif

        _fileList = new HashSet<string>();
        _dict = new Dictionary<string,LuaBase>();        
        //dictBundle = new Dictionary<string, IAssetFile>();

        LuaDLL.lua_pushstring(_lua.L, "ToLua_Index");
        LuaDLL.luaL_dostring(_lua.L, luaIndex);
        LuaDLL.lua_rawset(_lua.L, (int)LuaIndexes.LUA_REGISTRYINDEX);

        LuaDLL.lua_pushstring(_lua.L, "ToLua_NewIndex");
        LuaDLL.luaL_dostring(_lua.L, luaNewIndex);
        LuaDLL.lua_rawset(_lua.L, (int)LuaIndexes.LUA_REGISTRYINDEX);

        LuaDLL.lua_pushstring(_lua.L, "luaTableCall");
        LuaDLL.luaL_dostring(_lua.L, luaTableCall);
        LuaDLL.lua_rawset(_lua.L, (int)LuaIndexes.LUA_REGISTRYINDEX);

		LuaDLL.lua_pushstring(_lua.L, "ToLua_EnumIndex");
        LuaDLL.luaL_dostring(_lua.L, luaEnumIndex);
        LuaDLL.lua_rawset(_lua.L, (int)LuaIndexes.LUA_REGISTRYINDEX);
		
        //Bind();                

		//LuaDLL.lua_pushnumber(_lua.L, 0);
        //LuaDLL.lua_setglobal(_lua.L, "LuaScriptMgr");
    }

    public LuaState GetLuaState()
    {
        return _lua;
    }

    public void ReloadAll()
    {
        foreach (string str in _fileList)
        {
            _lua.DoFile(str, null);
        }

#if UNITY_EDITOR
        HobaDebuger.Log("Reload lua files over");
#endif
    }
	
    public string GetConfigsDir()
    {
        string ret = EntryPoint.Instance.ConfigPath;
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "GetConfigsDir");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                if (LuaDLL.lua_pcall(L, 0, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    ret = LuaDLL.lua_tostring(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return ret;
    }

    public string GetDebugLineInfo(int stack)
    {
        string info = string.Empty;
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "GetDebugLineInfo");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, stack);
                if (LuaDLL.lua_pcall(L, 1, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    info = LuaDLL.lua_tostring(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return info;
    }

    public void CallLuaOnClickGroundFunc(Vector3 pos)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnClickGround");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, pos);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        CCamCtrlMan.Instance.GetGameCamCtrl().StopRecoverCamera();
    }

	public void CallLuaOnConnectionEventFunc(int errorCode)
	{
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnConnectionEvent");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, errorCode);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
	}

    public void CallLuaOnWeatherEventFunc(int weatherType)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "ChangeCurrentWeather");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, weatherType);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }


    public void CallLuaOnWeatherEventFunc2(int type, int EntityId, Vector3 CurrentPosition, Vector3 CurrentOrientation,
        int MoveType, Vector3 MoveDirection, float MoveSpeed, int TimeInterval, Vector3 DstPosition, bool IsDestPosition)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "ChangeCurrentWeather2");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, type);
                Push(L, EntityId);
                Push(L, CurrentPosition);
                Push(L, CurrentOrientation);
                Push(L, MoveType);
                Push(L, MoveDirection);
                Push(L, MoveSpeed);
                Push(L, TimeInterval);
                Push(L, DstPosition);
                Push(L, IsDestPosition);
                if (LuaDLL.lua_pcall(L, 10, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }


    public int GetHandlerTotalCount()
    {
        int count = 0;
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "GetHandlerTotalCount");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                if (LuaDLL.lua_pcall(L, 0, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    count = (int)LuaDLL.lua_tonumber(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return count;
    }

    public int GetHostActiveEventCount()
    {
        var size = 0;
        var L = GetL();
        if (L != IntPtr.Zero)
        {
            var oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "GetHostActiveEventCount");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushvalue(L, LuaIndexes.LUA_REGISTRYINDEX);
                if (LuaDLL.lua_pcall(L, 1, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    size = (int)LuaDLL.lua_tonumber(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return size;
    }

    public void CallOnTraceBack()
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnTraceBack");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                if (LuaDLL.lua_pcall(L, 0, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public void CallOnInputKeyCode(int keycode)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnInputKeyCode");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, keycode);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public void CallOnDoubleInputKeyCode(int keycode)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnDoubleInputKeyCode");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, keycode);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public void GetDesignWidthAndHeight(ref int width, ref int height)
    {
        width = 0;
        height = 0;

        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "GetDesignWidthAndHeight");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                if (LuaDLL.lua_pcall(L, 0, 2, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    height = (int)LuaDLL.lua_tonumber(L, -1);
                    width = (int)LuaDLL.lua_tonumber(L, -2);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public long GetLuaMemory()
    {
        Int64 memory = 0;
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "GetLuaMemory");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                if (LuaDLL.lua_pcall(L, 0, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    memory = (long)LuaDLL.lua_tonumber(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return memory;
    }

    public long OnLowMemory()
    {
        Int64 memory = 0;
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnLowMemory");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                if (LuaDLL.lua_pcall(L, 0, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    memory = (long)LuaDLL.lua_tonumber(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return memory;
    }
    

	public int GetRegistryTableSize()
	{
		var size = 0;
		var L = GetL();
		if (L != IntPtr.Zero)
		{
			var oldTop = LuaDLL.lua_gettop(L);
			LuaDLL.lua_getglobal(L, "__GetTableSize");
			if (!LuaDLL.lua_isnil(L, -1))
			{
                LuaDLL.lua_pushvalue(L, LuaIndexes.LUA_REGISTRYINDEX);
				if (LuaDLL.lua_pcall(L, 1, 1, 0) != 0)
				{
					HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
				}
				else
				{
					size = (int)LuaDLL.lua_tonumber(L, -1);
				}
			}
			LuaDLL.lua_settop(L, oldTop);
		}
		return size;
	}

    public int PrintRegistryTable()
    {
        var size = 0;
        var L = GetL();
        if (L != IntPtr.Zero)
        {
            var oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "__PrintTable");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushvalue(L, LuaIndexes.LUA_REGISTRYINDEX);
                if (LuaDLL.lua_pcall(L, 1, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    size = (int)LuaDLL.lua_tonumber(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return size;
    }

	public string GetResPath(string key)
    {
        string val = string.Empty;
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "GetResPath");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushstring(L, key);
                if (LuaDLL.lua_pcall(L, 1, 1, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                else
                {
                    val = LuaDLL.lua_tostring(L, -1);
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
        return val;
    }

    //private int _LuaObjectRef = -2;

    public void CallNotifyClickFunc(string objName)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "NotifyClick");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, objName);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public void CallLuaOnSingleDragFunc(float delta)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnSingleDrag");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, delta);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public void CallLuaOnTwoFingersDragFunc(float delta)
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "OnTwoFingersDrag");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                Push(L, delta);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public void CallLuaBeginSleeping()
    {
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "BeginSleeping");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                if (LuaDLL.lua_pcall(L, 0, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
    }

    public void Tick(float dt)
    {
#if UseLuaTick
        IntPtr L = GetL();
        if (L != IntPtr.Zero)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_getglobal(L, "TickGame");
            if (!LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pushnumber(L, dt);
                if (LuaDLL.lua_pcall(L, 1, 0, 0) != 0)
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1), LuaStatic.GetTraceBackInfo(L));
                }
            }
            LuaDLL.lua_settop(L, oldTop);
        }
#endif
        TickUnref();
    }

    public IEnumerable Bind()
    {
        IntPtr L = _lua.L;
        BindArray(L);
        WrapClassID.Register();
        yield return null;

        AutoLuaBinder.Bind(L);
        yield return null;

        ManualLuaBinder.Bind(L);
        yield return null;

        foreach (var item in GameUtilWrap.Register2(L))
            yield return item;

        enumMetaRef = GetTypeMetaRef(typeof(System.Enum));
        typeMetaRef = GetTypeMetaRef(typeof(System.Type));
        delegateMetaRef = GetTypeMetaRef(typeof(System.Delegate));
        iterMetaRef = GetTypeMetaRef(typeof(IEnumerator));

        checkBaseType.Clear();
    }

	void BindArray(IntPtr L)
    {
        LuaDLL.luaL_newmetatable(L, "luaNet_array");
        LuaDLL.lua_pushstring(L, "__index");
        LuaDLL.lua_pushstdcallcfunction(L, IndexArray);
        LuaDLL.lua_rawset(L, -3);
        LuaDLL.lua_pushstring(L, "__gc");
        LuaDLL.lua_pushstdcallcfunction(L, __gc);
        LuaDLL.lua_rawset(L, -3);
        LuaDLL.lua_pushstring(L, "__newindex");
        LuaDLL.lua_pushstdcallcfunction(L, NewIndexArray);
        LuaDLL.lua_rawset(L, -3);
        arrayMetaRef = LuaDLL.luaL_ref(_lua.L, LuaIndexes.LUA_REGISTRYINDEX);
        LuaDLL.lua_settop(L, 0);
    }
	
	public IntPtr GetL()
    {
        return _lua.L;
    }

    public void LuaGC(params string[] param)
    {        
        LuaDLL.lua_gc(_lua.L, LuaGCOptions.LUA_GCCOLLECT, 0);        
    }

    public float LuaMemKB()
    {
        int b = LuaDLL.lua_gc(_lua.L, LuaGCOptions.LUA_GCCOUNTB, 0);
        float v = ((float)b / 1024);
        return v;
    }

    void LuaMem(params string[] param)
    {               
        CallLuaFunction("mem_report");
    }

    public void Start()
    {      
        PreloadLuaFunctions();
    }

	int GetLuaReference(string str)
    {
        LuaFunction func = GetLuaFunction(str);

        if (func != null)
        {
            return func.GetReference();
        }

        return -1;
    }

    int GetTypeMetaRef(Type t)
    {
        string metaName = t.AssemblyQualifiedName;
        LuaDLL.luaL_getmetatable(_lua.L, metaName);
        return LuaDLL.luaL_ref(_lua.L, LuaIndexes.LUA_REGISTRYINDEX);
    }
    public static string preload =
        @"
require 'UnityClass.Math'
require 'UnityClass.Vector3'
require 'UnityClass.Vector2'
require 'UnityClass.Quaternion'
require 'UnityClass.Vector4'
require 'UnityClass.Color'
require 'UnityClass.Ray'
require 'UnityClass.Rect'


-- LPlus config
local LuaCheckLevelEnum =
{
    None = 0,
    Limited = 1,
    Strict = 2,
}

local LuaCheckingLevel = LuaCheckLevelEnum.Strict

package.loaded.Lplus_config =
{
	reflection = false,
	declare_checking = LuaCheckingLevel >= 1,
	accessing_checking = LuaCheckingLevel >= 2,
	calling_checking = LuaCheckingLevel >= 2,
	reload = false,
}

        ";

    void PreloadLuaFunctions()
    {
#if !SERVER_USE
        DoString(preload);

        unpackVec3 = GetLuaReference("Vector3.Get");
        unpackVec2 = GetLuaReference("Vector2.Get");
        unpackVec4 = GetLuaReference("Vector4.Get");
        unpackQuat = GetLuaReference("Quaternion.Get");
        unpackColor = GetLuaReference("Color.Get");
        unpackRay = GetLuaReference("Ray.Get");
        //unpackRect = GetLuaReference("Rect.Get");

        packVec3 = GetLuaReference("Vector3.New");
        packVec2 = GetLuaReference("Vector2.New");
        packVec4 = GetLuaReference("Vector4.New");
        packQuat = GetLuaReference("Quaternion.New");
        packColor = GetLuaReference("Color.New");
        packRect = GetLuaReference("Rect.New");

        traceback = GetLuaFunction("traceback");
#endif
    }

    public void OnLevelLoaded(int level)
    {
        //levelLoaded.Call(level);
    }

    void TickUnref()
    {
        if (Time.frameCount % 30 != 0) return;

        while (!_refGCList.IsEmpty())
        {
            LuaRef lf = _refGCList.Dequeue();
            LuaDLL.lua_unref(lf.L, lf.reference);
        }
    }

    void SafeRelease(ref LuaFunction func)
    {
        if (func != null)
        {
            func.Release();
            func = null;
        }
    }
    
    public void SafeUnRef(ref int reference)
    {
        if (_lua != null && reference > 0)
        {
            LuaDLL.lua_unref(_lua.L, reference);
            reference = -1;
        }
    }

    public void Destroy()
    {        
        SafeUnRef(ref enumMetaRef);
        SafeUnRef(ref typeMetaRef);
        SafeUnRef(ref delegateMetaRef);
        SafeUnRef(ref iterMetaRef);
        SafeUnRef(ref arrayMetaRef);
                                                                                  
        SafeRelease(ref packRaycastHit);                
        SafeRelease(ref packTouch);

		LuaDLL.clear_luastate();

        LuaDLL.lua_gc(_lua.L, LuaGCOptions.LUA_GCCOLLECT, 0);

        foreach(KeyValuePair<string, LuaBase> kv in _dict)
        {
            kv.Value.Dispose();
        }

        _dict.Clear();
        _fileList.Clear();

        _lua.Close();
        _lua.Dispose();
        _lua = null;

        ClearInstance();            //clear instance
        HobaDebuger.Log("Lua module destroy");
    }

    public object[] DoString(string str)
    {
        return _lua.DoString(str);
    }

    public object[] DoFile(string fileName)
    {
#if LUA_ZIP
        if (file == null || !file.Contains(fileName))
        {
            return null;
        }
#endif

        return _lua.DoFile(fileName, null);
    }


    //不缓存LuaFunction
    public object[] CallLuaFunction(string name, params object[] args)
    {
        LuaBase lb = null;

        if (_dict.TryGetValue(name, out lb))
        {
            LuaFunction func = lb as LuaFunction;
            return func.Call(args);
        }
        else
        {
            IntPtr L = _lua.L;
            LuaFunction func = null;
            int oldTop = LuaDLL.lua_gettop(L);

            if (PushLuaFunction(L, name))
            {
                int reference = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
                func = new LuaFunction(reference, _lua);
                LuaDLL.lua_settop(L, oldTop);
                object[] objs = func.Call(args);
                func.Dispose();
                return objs;            
            }

            return null;
        }        
    }

    //会缓存LuaFunction
    public LuaFunction GetLuaFunction(string name)
    {
        LuaBase func = null;

        if (!_dict.TryGetValue(name, out func))
        {
            IntPtr L = _lua.L;
            int oldTop = LuaDLL.lua_gettop(L);

            if (PushLuaFunction(L, name))
            {
                int reference = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
                func = new LuaFunction(reference, _lua);
                func.name = name;
                _dict.Add(name, func);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Lua function " + name + " not exists");
            }

            LuaDLL.lua_settop(L, oldTop);
        }
        else
        {
            func.AddRef();
        }

        return func as LuaFunction;
    }

    public int GetFunctionRef(string name)
    {
        IntPtr L = _lua.L;
        int oldTop = LuaDLL.lua_gettop(L);
        int reference = -1;

        if (PushLuaFunction(L, name))
        {
            reference = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
        }
        else
        {
             UnityEngine.Debug.LogWarning("Lua function " + name + " not exists");
        }

        LuaDLL.lua_settop(L, oldTop);
        return reference;
    }

    public bool IsFuncExists(string name)
    {
        IntPtr L = _lua.L;
        int oldTop = LuaDLL.lua_gettop(L);

        if (PushLuaFunction(L, name))
        {
            LuaDLL.lua_settop(L, oldTop);
            return true;
        }

        return false;
    }

    static bool PushLuaTable(IntPtr L, string fullPath)
    {
        string[] path = fullPath.Split('.');

        int oldTop = LuaDLL.lua_gettop(L);
       // LuaDLL.lua_getglobal(L, path[0]);
        LuaDLL.lua_pushstring(L, path[0]);
        LuaDLL.lua_rawget(L, LuaIndexes.LUA_GLOBALSINDEX);   

        LuaTypes type = LuaDLL.lua_type(L, -1);

        if (type != LuaTypes.LUA_TTABLE)
        {
            LuaDLL.lua_settop(L, oldTop);
			LuaDLL.lua_pushnil(L);
            UnityEngine.Debug.LogError("Push lua table " + path[0] + " failed");
            return false;
        }

        for (int i = 1; i < path.Length; i++)
        {
            LuaDLL.lua_pushstring(L, path[i]);
            LuaDLL.lua_rawget(L, -2);
            type = LuaDLL.lua_type(L, -1);

            if (type != LuaTypes.LUA_TTABLE)
            {
                LuaDLL.lua_settop(L, oldTop);
                UnityEngine.Debug.LogError("Push lua table " + fullPath + " failed");
                return false;
            }
        }

        if (path.Length > 1)
        {
            LuaDLL.lua_insert(L, oldTop + 1);
            LuaDLL.lua_settop(L, oldTop + 1);
        }

        return true;
    }

    static bool PushLuaFunction(IntPtr L, string fullPath)
    {
        int oldTop = LuaDLL.lua_gettop(L);
        int pos = fullPath.LastIndexOf('.');

        if (pos > 0)
        {
            string tableName = fullPath.Substring(0, pos);

            if (PushLuaTable(L, tableName))
            {
                string funcName = fullPath.Substring(pos + 1);
                LuaDLL.lua_pushstring(L, funcName);
                LuaDLL.lua_rawget(L, -2);
            }

            LuaTypes type = LuaDLL.lua_type(L, -1);

            if (type != LuaTypes.LUA_TFUNCTION)
            {
                LuaDLL.lua_settop(L, oldTop);
                return false;
            }

            LuaDLL.lua_insert(L, oldTop + 1);
            LuaDLL.lua_settop(L, oldTop + 1);
        }
        else
        {
            LuaDLL.lua_getglobal(L, fullPath);
            LuaTypes type = LuaDLL.lua_type(L, -1);

            if (type != LuaTypes.LUA_TFUNCTION)
            {
                LuaDLL.lua_settop(L, oldTop);
                return false;
            }
        }

        return true;
    }

    public LuaTable GetLuaTable(string tableName)
    {
        LuaBase lt = null;

        if (!_dict.TryGetValue(tableName, out lt))
        {
            IntPtr L = _lua.L;
            int oldTop = LuaDLL.lua_gettop(L);

            if (PushLuaTable(L, tableName))
            {
                int reference = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
                lt = new LuaTable(reference, _lua);
                lt.name = tableName;
                _dict.Add(tableName, lt);
            }

            LuaDLL.lua_settop(L, oldTop);
        }
        else
        {
            lt.AddRef();
        }

        return lt as LuaTable;
    }

    public void RemoveLuaRes(string name)
    {
        _dict.Remove(name);
    }

    public static void CreateTable(IntPtr L, string fullPath)
    {
        string[] path = fullPath.Split('.');
        int oldTop = LuaDLL.lua_gettop(L);

        if (path.Length > 1)
        {
            LuaDLL.lua_getglobal(L, path[0]);
            LuaTypes type = LuaDLL.lua_type(L, -1);

            if (type == LuaTypes.LUA_TNIL)
            {
                LuaDLL.lua_pop(L, 1);
                LuaDLL.lua_createtable(L, 0, 0);
                LuaDLL.lua_pushstring(L, path[0]);
                LuaDLL.lua_pushvalue(L, -2);
                LuaDLL.lua_settable(L, LuaIndexes.LUA_GLOBALSINDEX);
            }

            for (int i = 1; i < path.Length - 1; i++)
            {
                LuaDLL.lua_pushstring(L, path[i]);
                LuaDLL.lua_rawget(L, -2);

                type = LuaDLL.lua_type(L, -1);

                if (type == LuaTypes.LUA_TNIL)
                {
                    LuaDLL.lua_pop(L, 1);
                    LuaDLL.lua_createtable(L, 0, 0);
                    LuaDLL.lua_pushstring(L, path[i]);
                    LuaDLL.lua_pushvalue(L, -2);
                    LuaDLL.lua_rawset(L, -4);
                }
            }

            LuaDLL.lua_pushstring(L, path[path.Length - 1]);
            LuaDLL.lua_rawget(L, -2);

            type = LuaDLL.lua_type(L, -1);

            if (type == LuaTypes.LUA_TNIL)
            {
                LuaDLL.lua_pop(L, 1);
                LuaDLL.lua_createtable(L, 0, 0);
                LuaDLL.lua_pushstring(L, path[path.Length - 1]);
                LuaDLL.lua_pushvalue(L, -2);
                LuaDLL.lua_rawset(L, -4);
            }
        }
        else
        {
            LuaDLL.lua_getglobal(L, path[0]);
            LuaTypes type = LuaDLL.lua_type(L, -1);

            if (type == LuaTypes.LUA_TNIL)
            {
                LuaDLL.lua_pop(L, 1);
                LuaDLL.lua_createtable(L, 0, 0);
                LuaDLL.lua_pushstring(L, path[0]);
                LuaDLL.lua_pushvalue(L, -2);
                LuaDLL.lua_settable(L, LuaIndexes.LUA_GLOBALSINDEX);
            }
        }

        LuaDLL.lua_insert(L, oldTop + 1);
        LuaDLL.lua_settop(L, oldTop + 1);
    }

    //注册一个枚举类型
    public static void RegisterLib(IntPtr L, string libName, Type t, LuaMethod[] regs)
    {
        CreateTable(L, libName);

        LuaDLL.luaL_getmetatable(L, t.AssemblyQualifiedName);

        if (LuaDLL.lua_isnil(L, -1))
        {
            LuaDLL.lua_pop(L, 1);
            LuaDLL.luaL_newmetatable(L, t.AssemblyQualifiedName);
        }

        LuaDLL.lua_pushstring(L, "ToLua_EnumIndex");
        LuaDLL.lua_rawget(L, (int)LuaIndexes.LUA_REGISTRYINDEX);
        LuaDLL.lua_setfield(L, -2, "__index");

        LuaDLL.lua_pushstring(L, "__gc");
        LuaDLL.lua_pushstdcallcfunction(L, __gc);
        LuaDLL.lua_rawset(L, -3);

        for (int i = 0; i < regs.Length - 1; i++)
        {
            LuaDLL.lua_pushstring(L, regs[i].name);            
            LuaDLL.lua_pushstdcallcfunction(L, regs[i].func);
            LuaDLL.lua_rawset(L, -3);
        }
                
        int pos = regs.Length - 1;
        LuaDLL.lua_pushstring(L, regs[pos].name);
        LuaDLL.lua_pushstdcallcfunction(L, regs[pos].func);
        LuaDLL.lua_rawset(L, -4);

        LuaDLL.lua_setmetatable(L, -2);
        LuaDLL.lua_settop(L, 0);     
    }

    public static void RegisterLib(IntPtr L, string libName, LuaMethod[] regs)
    {
        CreateTable(L, libName);

        for (int i = 0; i < regs.Length; i++)
        {                        
            LuaDLL.lua_pushstring(L, regs[i].name);
            LuaDLL.lua_pushstdcallcfunction(L, regs[i].func);
            LuaDLL.lua_rawset(L, -3);                    
        }
          
        LuaDLL.lua_settop(L, 0);        
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int __gc(IntPtr luaState)
    {
        int udata = LuaDLL.luanet_rawnetobj(luaState, 1);

        if (udata != -1)
        {
            ObjectTranslator translator = ObjectTranslator.FromState(luaState);
            translator.collectObject(udata);
        }

        return 0;
    }
    

    public static void RegisterLib(IntPtr L, string libName, Type t, LuaMethod[] regs, LuaField[] fields, Type baseType)
    {
        CreateTable(L, libName);

        LuaDLL.luaL_getmetatable(L, t.AssemblyQualifiedName);

        if (LuaDLL.lua_isnil(L, -1))
        {
            LuaDLL.lua_pop(L, 1);
            LuaDLL.luaL_newmetatable(L, t.AssemblyQualifiedName);
        }

        if (baseType != null)
        {
            LuaDLL.luaL_getmetatable(L, baseType.AssemblyQualifiedName);

            if (LuaDLL.lua_isnil(L, -1))
            {
                LuaDLL.lua_pop(L, 1);
                LuaDLL.luaL_newmetatable(L, baseType.AssemblyQualifiedName);
                checkBaseType.Add(baseType);
            }
            else
            {
                checkBaseType.Remove(baseType);
            }

            LuaDLL.lua_setmetatable(L, -2);
        }

        LuaDLL.tolua_setindex(L);
        LuaDLL.tolua_setnewindex(L);

        LuaDLL.lua_pushstring(L, "__call");
        LuaDLL.lua_pushstring(L, "ToLua_TableCall");
        LuaDLL.lua_rawget(L, (int)LuaIndexes.LUA_REGISTRYINDEX);
        LuaDLL.lua_rawset(L, -3);

        LuaDLL.lua_pushstring(L, "__gc");
        LuaDLL.lua_pushstdcallcfunction(L, __gc);
        LuaDLL.lua_rawset(L, -3);

        if(regs != null)
        {
            for (int i = 0; i < regs.Length; i++)
            {
                LuaDLL.lua_pushstring(L, regs[i].name);
                LuaDLL.lua_pushstdcallcfunction(L, regs[i].func);
                LuaDLL.lua_rawset(L, -3);
            }
        }
        
        if(fields != null)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                LuaDLL.lua_pushstring(L, fields[i].name);
                LuaDLL.lua_createtable(L, 2, 0);

                if (fields[i].getter != null)
                {
                    LuaDLL.lua_pushstdcallcfunction(L, fields[i].getter);
                    LuaDLL.lua_rawseti(L, -2, 1);
                }

                if (fields[i].setter != null)
                {
                    LuaDLL.lua_pushstdcallcfunction(L, fields[i].setter);
                    LuaDLL.lua_rawseti(L, -2, 2);
                }

                LuaDLL.lua_rawset(L, -3);
            }
        }

        LuaDLL.lua_setmetatable(L, -2);
        LuaDLL.lua_settop(L, 0);

        checkBaseType.Remove(t);
    }

    private static bool IsClassOf(Type child, Type parent)
    {
        return child == parent || parent.IsAssignableFrom(child);
    }

    static ObjectTranslator GetTranslator(IntPtr L)
    {
        
        if (_translator == null)
        {
            return ObjectTranslator.FromState(L);
        }

        return _translator;     
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0)
    {
        return CheckType(L, type0, begin);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2, Type type3)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2) && CheckType(L, type3, begin + 3);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2, Type type3, Type type4)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2) && CheckType(L, type3, begin + 3) && CheckType(L, type4, begin + 4);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2, Type type3, Type type4, Type type5)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2) && CheckType(L, type3, begin + 3) && CheckType(L, type4, begin + 4) &&
               CheckType(L, type5, begin + 5);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2, Type type3, Type type4, Type type5, Type type6)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2) && CheckType(L, type3, begin + 3) && CheckType(L, type4, begin + 4) &&
               CheckType(L, type5, begin + 5) && CheckType(L, type6, begin + 6);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2, Type type3, Type type4, Type type5, Type type6, Type type7)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2) && CheckType(L, type3, begin + 3) && CheckType(L, type4, begin + 4) &&
               CheckType(L, type5, begin + 5) && CheckType(L, type6, begin + 6) && CheckType(L, type7, begin + 7);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2, Type type3, Type type4, Type type5, Type type6, Type type7, Type type8)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2) && CheckType(L, type3, begin + 3) && CheckType(L, type4, begin + 4) &&
               CheckType(L, type5, begin + 5) && CheckType(L, type6, begin + 6) && CheckType(L, type7, begin + 7) && CheckType(L, type8, begin + 8);
    }

    public static bool CheckTypes(IntPtr L, int begin, Type type0, Type type1, Type type2, Type type3, Type type4, Type type5, Type type6, Type type7, Type type8, Type type9)
    {
        return CheckType(L, type0, begin) && CheckType(L, type1, begin + 1) && CheckType(L, type2, begin + 2) && CheckType(L, type3, begin + 3) && CheckType(L, type4, begin + 4) &&
               CheckType(L, type5, begin + 5) && CheckType(L, type6, begin + 6) && CheckType(L, type7, begin + 7) && CheckType(L, type8, begin + 8) && CheckType(L, type9, begin + 9);
    }

    //当进入这里时势必会有一定的gc alloc, 因为params Type[]会分配内存, 可以像上面扩展来避免gc alloc
    public static bool CheckTypes(IntPtr L, int begin, params Type[] types)
    {
        for (int i = 0; i < types.Length; i++)
        {            
            if (!CheckType(L, types[i], i + begin))
            {
                return false;
            }
        }

        return true;
    }

    public static bool CheckParamsType(IntPtr L, Type t, int begin, int count)
    {
        //默认都可以转 object
        if (t == typeof(object))
        {
            return true;
        }

        for (int i = 0; i < count; i++)
        {            
            if (!CheckType(L, t, i + begin))
            {
                return false;
            }
        }

        return true;
    }

    static bool CheckTableType(IntPtr L, Type t, int stackPos)
    {
        if (t.IsArray)
        {
            return true;
        }
        else if (t == typeof(LuaTable))
        {
            return true;
        }
        else if (t.IsValueType)
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_pushvalue(L, stackPos);
            LuaDLL.lua_pushstring(L, "class");
            LuaDLL.lua_gettable(L, -2);

            string cls = LuaDLL.lua_tostring(L, -1);
            LuaDLL.lua_settop(L, oldTop);

            if (cls == "Vector3")
            {
                return t == typeof(Vector3);
            }
            else if (cls == "Vector2")
            {
                return t == typeof(Vector2);
            }
            else if (cls == "Quaternion")
            {
                return t == typeof(Quaternion);
            }
            else if (cls == "Color")
            {
                return t == typeof(Color);
            }
            else if (cls == "Vector4")
            {
                return t == typeof(Vector4);
            }
            else if (cls == "Ray")
            {
                return t == typeof(Ray);
            }
        }

        return false;
    }

    public static bool CheckType(IntPtr L, Type t, int pos)
    {
        //默认都可以转 object
        if (t == typeof(object))
        {
            return true;
        }

        LuaTypes luaType = LuaDLL.lua_type(L, pos);

        switch (luaType)
        {
            case LuaTypes.LUA_TNUMBER:
                return t.IsPrimitive;
            case LuaTypes.LUA_TSTRING:
                return t == typeof(string);
            case LuaTypes.LUA_TUSERDATA:
                return CheckUserData(L, luaType, t, pos);
            case LuaTypes.LUA_TBOOLEAN:
                return t == typeof(bool);
            case LuaTypes.LUA_TFUNCTION:
                return t == typeof(LuaFunction);
            case LuaTypes.LUA_TTABLE:
                return CheckTableType(L, t, pos);
            case LuaTypes.LUA_TNIL:
                return true; //t == null;
            default:
                break;
        }

        return false;
    }

    static Type monoType = typeof(Type).GetType();

    static bool CheckUserData(IntPtr L, LuaTypes luaType, Type t, int pos)
    {
        object obj = GetLuaObject(L, pos);
        if (obj == null) return true;
        Type type = obj.GetType();

        if (t == type)
        {
            return true;
        }

        if (t == typeof(Type))
        {                                    
            return type == monoType;
        }  
        else
        {
            return t.IsAssignableFrom(type);            
        }        
    }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int IndexArray(IntPtr L)
    { 
        Array obj = GetLuaObject(L, 1) as Array;

        if (obj == null)
        {
            LuaDLL.luaL_error(L, "trying to index an invalid Array reference");
            LuaDLL.lua_pushnil(L);
            return 1;
        }

        LuaTypes luaType = LuaDLL.lua_type(L, 2);

        if (luaType == LuaTypes.LUA_TNUMBER)
        {
            int index = (int)LuaDLL.lua_tonumber(L, 2);

            if (index >= obj.Length)
            {
                LuaDLL.luaL_error(L, "array index out of bounds: " + index + " " + obj.Length);
                return 0;
            }
            
            object val = obj.GetValue(index);

            if (val == null)
            {
                LuaDLL.luaL_error(L, HobaText.Format("array index {0} is null", index));
                return 0;
            }

            PushVarObject(L, val);

            //Type et = val.GetType();

            //if (et.IsValueType)
            //{
            //    if (et == typeof(Vector3))
            //    {
            //        Push(L, (Vector3)val);                    
            //    }
            //    else
            //    {
            //        GetTranslator(L).push(L, val);
            //    }
            //}
            //else
            //{
            //    PushObject(L, val);                
            //}            
        }
        else if (luaType == LuaTypes.LUA_TSTRING)
        {
            string field = GetLuaString(L, 2);

            if (field == "Length")
            {                
                Push(L, obj.Length);
            }
        }

        return 1;
    }


    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int NewIndexArray(IntPtr L)
    {
        Array obj = GetLuaObject(L, 1) as Array;

        if (obj == null)
        {
            LuaDLL.luaL_error(L, "trying to index and invalid object reference");
            return 0;
        }

        int index = (int)GetNumber(L, 2);
        object val = GetVarObject(L, 3);
        Type type = obj.GetType().GetElementType();        

        if (!CheckType(L, type, 3))
        {
            LuaDLL.luaL_error(L, "trying to set object type is not correct");
            return 0;
        }

        val = Convert.ChangeType(val, type);
        obj.SetValue(val, index);

        return 0;
    }


    public static void DumpStack(IntPtr L)
    {
        int top = LuaDLL.lua_gettop(L);

        for (int i = 1; i <= top; i++)
        {
            LuaTypes t = LuaDLL.lua_type(L, i);

            switch (t)
            {
                case LuaTypes.LUA_TSTRING:
                    HobaDebuger.LogFormat("<String>: {0}", LuaDLL.lua_tostring(L, i));
                    break;
                case LuaTypes.LUA_TBOOLEAN:
                    HobaDebuger.LogFormat("<Boolean>: {0}", LuaDLL.lua_toboolean(L, i).ToString());
                    break;
                case LuaTypes.LUA_TNUMBER:
                    HobaDebuger.LogFormat("<Number>: {0}", LuaDLL.lua_tonumber(L, i).ToString());
                    break;
                case LuaTypes.LUA_TTABLE:
                    HobaDebuger.Log("<Table>: ");
                    break;
                default:
                    HobaDebuger.LogFormat("{0}: {1}", t.GetType().ToString(), LuaDLL.lua_tostring(L, i));
                    break;
            }
        }
    }

    static Dictionary<Enum, object> enumMap = new Dictionary<Enum, object>();

    static object GetEnumObj(Enum e)
    {
        object o = null;

        if (!enumMap.TryGetValue(e, out o))
        {
            o = e;
            enumMap.Add(e, o);
        }

        return o;
    }
   
}
