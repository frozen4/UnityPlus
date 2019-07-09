using Common;

namespace LuaInterface
{
	using System;
	using System.IO;
	using UnityEngine;

    public delegate byte[] ReadLuaFile(string name);
	
	public static class LuaStatic
	{
        public static ReadLuaFile Load = null;
        public static ReadLuaFile LoadFromAssetsPath = null;
        //private static int trace = 0;

        static LuaStatic()
        {
            Load = DefaultLoader;
        }

        //public static void InitTraceback(IntPtr L)
        //{
        //    int oldTop = LuaDLL.lua_gettop(L);
        //    LuaDLL.lua_getglobal(L, "debug");
        //    LuaDLL.lua_getfield(L, -1, "traceback");
        //    trace = LuaDLL.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
        //    LuaDLL.lua_settop(L, oldTop);
        //}

        static byte[] DefaultLoader(string path)
        {
            byte[] str = null;
            
            if (File.Exists(path))
            {
                str = File.ReadAllBytes(path);
            }

            return str;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int warn(IntPtr L)
        {
            if (HobaDebuger.GameLogLevel < LogLevel.Warning)
                return 0;

            int n = LuaDLL.lua_gettop(L);
            string s = String.Empty;

            LuaDLL.lua_getglobal(L, "tostring");

            for (int i = 1; i <= n; i++)
            {
                LuaDLL.lua_pushvalue(L, -1);  /* function to be called */
                LuaDLL.lua_pushvalue(L, i);   /* value to print */
                LuaDLL.lua_call(L, 1, 1);
                string ret = LuaDLL.lua_tostring(L, -1);
                if (ret == null)
                {
                    HobaDebuger.LogError("!!!! lua warn return null*");
                }
                else
                    s += ret;

                if (i < n)
                {
                    s += "\t";
                }

                LuaDLL.lua_pop(L, 1);  /* pop result */

            }

#if !SERVER_USE
            if (!EntryPoint.Instance.IsInited)
                LuaDLL.HOBA_LogString(HobaText.Format("warn LUA: {0}", s));
#endif

            HobaDebuger.LogWarningFormat("LUA: {0}", s);

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int breakFunc(IntPtr L)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Break();
#endif
            return 0;
        }

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int panic(IntPtr L)
		{
			string reason = String.Format("unprotected error in call to Lua API ({0})", LuaDLL.lua_tostring(L, -1));
            LuaDLL.lua_pop(L, 1);
			throw new LuaException(reason);
		}

        public static String GetTraceBackInfo(IntPtr L, string err = "Lua traceback:")
        {
            int oldTop = LuaDLL.lua_gettop(L);
            LuaDLL.lua_checkstack(L, 3);
            LuaDLL.lua_getglobal(L, "debug");
            LuaDLL.lua_getfield(L, -1, "traceback");
            LuaDLL.lua_pushstring(L, err);
            LuaDLL.lua_pushnumber(L, 1);
            LuaDLL.lua_call(L, 2, 1);
            string trace = LuaDLL.lua_tostring(L, -1);
            LuaDLL.lua_settop(L, oldTop);
            return trace;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int traceback(IntPtr L)
        {            
            LuaDLL.lua_getglobal(L, "debug");
            LuaDLL.lua_getfield(L, -1, "traceback");            
            LuaDLL.lua_pushvalue(L, 1);
            LuaDLL.lua_pushnumber(L, 2);
            LuaDLL.lua_call(L, 2, 1);                        
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int errorFunc_traceback(IntPtr L)
        {
            if (!LuaDLL.lua_isstring(L, 1))  /* 'message' not a string? */
                return 1;  /* keep it intact */
            LuaDLL.lua_getfield(L, LuaIndexes.LUA_GLOBALSINDEX, "debug");
            if (LuaDLL.lua_istable(L, -1))
            {
                LuaDLL.lua_pop(L, 1);
                return 1;
            }
            LuaDLL.lua_getfield(L, -1, "traceback");
            if (LuaDLL.lua_isfunction(L, -1))
            {
                LuaDLL.lua_pop(L, 2);
                return 1;
            }
            LuaDLL.lua_pushvalue(L, 1);  /* pass error message */
            LuaDLL.lua_pushinteger(L, 2);  /* skip this function and traceback */
            LuaDLL.lua_call(L, 2, 1);  /* call debug.traceback */
            return 1;
        }

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int print(IntPtr L)
		{
            if(HobaDebuger.GameLogLevel < LogLevel.Log)
                return 0;

            // For each argument we'll 'tostring' it
            int n = LuaDLL.lua_gettop(L);
			string s = String.Empty;
			
			LuaDLL.lua_getglobal(L, "tostring");
			
			for( int i = 1; i <= n; i++ ) 
			{
				LuaDLL.lua_pushvalue(L, -1);  /* function to be called */
				LuaDLL.lua_pushvalue(L, i);   /* value to print */
				LuaDLL.lua_call(L, 1, 1);

                string ret = LuaDLL.lua_tostring(L, -1);
                if (ret == null)
                {
                    HobaDebuger.LogError("!!!! lua print return null*");
                }
                else
                    s += ret;
				
				if( i < n ) 
				{
					s += "\t";
				}
				
				LuaDLL.lua_pop(L, 1);  /* pop result */
			}

#if !SERVER_USE
            if (!EntryPoint.Instance.IsInited)
                LuaDLL.HOBA_LogString(HobaText.Format("print LUA: {0}", s));
#endif
            HobaDebuger.LogFormat("LUA: {0}", s);

            return 0;
		}
		
		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int loader(IntPtr L)
		{            
			string fileName = String.Empty;
            fileName = LuaDLL.lua_tostring(L, 1);     
			fileName = fileName.Replace('.', '/');
			fileName += ".lua";
			
            byte[] text = Load(fileName);
			if( text == null )
			{
                LuaDLL.lua_pushstring(L, "loader failed to read file with path: " + fileName);  
                LuaDLL.lua_error(L); 
                return 0;
			}
			
			// 0: no errors;
            // LUA_ERRSYNTAX: syntax error during pre-compilation;
			// LUA_ERRMEM: memory allocation error.
		    if (LuaDLL.luaL_loadbuffer(L, text, text.Length, fileName) != 0)
		    {
		        // loadbuffer会在栈顶压入错误信息,弹出错误信息所在的最上层栈 
		        //LuaDLL.lua_pop(L, 1);
		        LuaDLL.lua_error(L);
		        HobaDebuger.LogErrorFormat("Failed to call luaL_loadbuffer, file: {0}", fileName);
		        return 0;
		    }

            return 1;
        }

        // fileName 返回文件名用于显示错误信息
        private static int loadfileFromAssetsPath(IntPtr L, out String filePath)
        {
            string fileName = LuaDLL.lua_tostring(L, 1);
            filePath = fileName;

            byte[] text = LoadFromAssetsPath(fileName);

            if (text == null)
            {
                LuaDLL.lua_pushstring(L, "loadfileFromAssetsPath failed to read file with path: " + filePath);
                return (int)LuaThreadStatus.LUA_ERRSYNTAX;   // 3 编译错误
            }
            else
            {
                return LuaDLL.luaL_loadbuffer(L, text, text.Length, fileName);
            }
        }

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int dofile(IntPtr L)
		{
            int n = LuaDLL.lua_gettop(L);
            if(0 == n)
            {
                LuaDLL.lua_pushstring(L, "dofile function - 1 param expected, got 0");
                LuaDLL.lua_error(L);
                return 0;
            }

            String filePath;
            bool bLoaded = (loadfileFromAssetsPath(L, out filePath) == 0);
            if (!bLoaded)
            {
                //loadfileFromAssetsPath 已准备好error信息
                LuaDLL.lua_error(L); 
                return 0;
            }

            LuaDLL.lua_call(L, 0, LuaDLL.LUA_MULTRET);

            return LuaDLL.lua_gettop(L);
		}
		
		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int loadfile(IntPtr L)
		{
            return loader(L);

            /*
            int n = LuaDLL.lua_gettop(L);
            if (n == 0)
            {
                LuaDLL.lua_pushstring(L, "loadfile - 1 param expected, got 0");
                LuaDLL.lua_error(L);
                return 0;
            }

            string filePath; 
            if (0 == loadfileFromAssetsPath(L, out filePath))
            {
                return 1; 
            }
            else
            {
                LuaDLL.lua_pushnil(L);  
                LuaDLL.lua_pushvalue(L, -2);  
                return 2;
            }
             * */
		}

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int pcall(IntPtr L)
        {
            int n = LuaDLL.lua_gettop(L);
            int ret = LuaDLL.lua_pcall(L, n - 1, -1, 0);
            int nn = LuaDLL.lua_gettop(L);
            LuaDLL.lua_pushboolean(L, ret == 0);
            LuaDLL.lua_insert(L, 1);
            return nn + 1;
        }

        public static string init_luanet =
            @"local metatable = {}
            local rawget = rawget
            local debug = debug
            local import_type = luanet.import_type
            local load_assembly = luanet.load_assembly
            luanet.error, luanet.type = error, type
            -- Lookup a .NET identifier component.
            function metatable:__index(key) -- key is e.g. 'Form'
            -- Get the fully-qualified name, e.g. 'System.Windows.Forms.Form'
            local fqn = rawget(self,'.fqn')
            fqn = ((fqn and fqn .. '.') or '') .. key

            -- Try to find either a luanet function or a CLR type
            local obj = rawget(luanet,key) or import_type(fqn)

            -- If key is neither a luanet function or a CLR type, then it is simply
            -- an identifier component.
            if obj == nil then
                -- It might be an assembly, so we load it too.
                    pcall(load_assembly,fqn)
                    obj = { ['.fqn'] = fqn }
            setmetatable(obj, metatable)
            end

            -- Cache this lookup
            rawset(self, key, obj)
            return obj
            end

            -- A non-type has been called; e.g. foo = System.Foo()
            function metatable:__call(...)
            error('No such type: ' .. rawget(self,'.fqn'), 2)
            end

            -- This is the root of the .NET namespace
            luanet['.fqn'] = false
            setmetatable(luanet, metatable)

            -- Preload the mscorlib assembly
            luanet.load_assembly('mscorlib')

            function traceback(msg)                
                return debug.traceback(msg, 1)                
            end";
	}
}
