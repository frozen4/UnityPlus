using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using LuaInterface;

namespace LT.Lua
{
    public class LuaTableCacheCheck : EditorWindow
    {
        private static bool canUse
        {
            get
            {
                return (EditorApplication.isPlaying && EntryPoint.Instance.IsInited);
            }
        }

        private LuaScriptMgr _luaScriptMgr;
        public LuaScriptMgr LuaScriptMgr
        {
            get
            {
                if (_luaScriptMgr == null)
                {
                    _luaScriptMgr = LuaScriptMgr.Instance;
                    _luaScriptMgr.DoString("require [[Tools.LuaTableCacheCheck]]");
                }
                return _luaScriptMgr;
            }
        }

        [MenuItem("Hoba Tools/Lua/LuaTableCacheCheck", false, 2)]
        static void OpenWindow()
        {
            EditorWindow window = GetWindow<LuaTableCacheCheck>(false, "LuaTableCacheCheck");
            if (window.position.position == Vector2.zero)
            {
                Resolution res = Screen.currentResolution;
                window.position = new Rect(res.width / 2 - 300, res.height / 2 - 300, 600, 600);
            }
            window.Show();
        }

        void OnGUI()
        {
            if (GUILayout.Button("ShowTableLength"))
            {
                LuaScriptMgr.CallLuaFunction("Hoba_TravelTableLength");
            }
        }
    }
}