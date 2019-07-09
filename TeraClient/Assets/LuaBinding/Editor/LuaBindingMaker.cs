using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Collections.Generic;
using EntityComponent;
using UnityEngine.EventSystems;

public static class LuaBinding
{
    public static string WrapFileSavedPath = "/LuaBinding/LuaInterfaces/";

    public class BindType
    {
        public string name;                 //类名称
        public Type type;
        public bool IsStatic;
        public string baseName = null;      //继承的类名字
        public string wrapName = "";        //产生的wrap文件名字
        public string libName = "";         //注册到lua的名字
        public bool Is2WrapAllMethods = true;  
        public List<string> wrapMethods = null;  //注册到lua的函数名字
        public bool Is2WrapAllFields = true;
        public List<string> wrapFields = null;   //注册到lua的属性名字

        string GetTypeStr(Type t)
        {
            string str = t.ToString();

            if (t.IsGenericType)
            {
                str = GetGenericName(t);
            }
            else if (str.Contains("+"))
            {
                str = str.Replace('+', '.');
            }

            return str;
        }

        private static string[] GetGenericName(Type[] types)
        {
            string[] results = new string[types.Length];

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsGenericType)
                {
                    results[i] = GetGenericName(types[i]);
                }
                else
                {
                    results[i] = ToLuaExport.GetTypeStr(types[i]);
                }

            }

            return results;
        }

        private static string GetGenericName(Type type)
        {
            if (type.GetGenericArguments().Length == 0)
            {
                return type.Name;
            }

            Type[] gArgs = type.GetGenericArguments();
            string typeName = type.Name;
            string pureTypeName = typeName.Substring(0, typeName.IndexOf('`'));

            return pureTypeName + "<" + string.Join(",", GetGenericName(gArgs)) + ">";
        }

        public BindType(Type t)
        {
            type = t;

            name = ToLuaExport.GetTypeStr(t);

            if (t.IsGenericType)
            {
                libName = ToLuaExport.GetGenericLibName(t);
                wrapName = ToLuaExport.GetGenericLibName(t);
            }
            else
            {
                libName = t.FullName.Replace("+", ".");
                wrapName = name.Replace('.', '_');

                if (name == "object")
                {
                    wrapName = "System_Object";
                }
            }

            if (t.BaseType != null)
            {
                baseName = ToLuaExport.GetTypeStr(t.BaseType);

                if (baseName == "ValueType")
                {
                    baseName = null;
                }
            }

            if (t.GetConstructor(Type.EmptyTypes) == null && t.IsAbstract && t.IsSealed)
            {
                baseName = null;
                IsStatic = true;
            }
        }

        public BindType SetBaseName(string str)
        {
            baseName = str;
            return this;
        }

        public BindType SetWrapName(string str)
        {
            wrapName = str;
            return this;
        }

        public BindType SetLibName(string str)
        {
            libName = str;
            return this;
        }

        public BindType SetWrapMethods(string[] methods)
        {
            Is2WrapAllMethods = false;
            if (methods != null)
            {
                wrapMethods = new List<string>();
                for(var i = 0; i < methods.Length; i++)
                    wrapMethods.Add(methods[i]);
            }
            return this;
        }
        public BindType SetWrapFields(string[] fields)
        {
            Is2WrapAllFields = false;
            if (fields != null)
            {
                wrapFields = new List<string>();
                for (var i = 0; i < fields.Length; i++)
                    wrapFields.Add(fields[i]);
            }
            return this;
        }
    }

    static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    static BindType[] binds = new BindType[]
    {
        //_GT(typeof(UnityEngine.Object)),  // Base中，无需导出
        _GT(typeof(Component)).SetWrapMethods(null).SetWrapFields(new []{"gameObject"}),
        _GT(typeof(Behaviour)).SetWrapMethods(null).SetWrapFields(new []{"enabled"}),
        _GT(typeof(MonoBehaviour)).SetWrapMethods(null).SetWrapFields(null),
        _GT(typeof(UIBehaviour)).SetWrapMethods(null).SetWrapFields(null),
        _GT(typeof(Selectable)).SetWrapMethods(null).SetWrapFields(null),
        
        //Unity Class
        //_GT(typeof(GameObject)), // 为lua书写简单，将Transform部分接口整合进GameObjectWrap类，不再自动生成
        _GT(typeof(SystemInfo)).SetWrapMethods(null).SetWrapFields(new []{"graphicsDeviceName","deviceModel", "systemMemorySize"}),
        //_GT(typeof(Animation)), // 放在Base接口中
        _GT(typeof(AnimationClip)).SetBaseName("UnityEngine.Object").SetWrapMethods(null).SetWrapFields(new []{"length"}),
        //_GT(typeof(Application)).SetWrapMethods(null).SetWrapFields(new []{"platform", "isMobilePlatform", "backgroundLoadingPriority"}), // 放在Base接口中
        _GT(typeof(Time)).SetWrapMethods(null).SetWrapFields(new []{ "time", "frameCount"}),
        _GT(typeof(Resources)).SetWrapMethods(new []{"Load"}).SetWrapFields(null),
        _GT(typeof(Light)).SetWrapMethods(null).SetWrapFields(new []{"intensity"}),

        //Unity enum
        _GT(typeof(PlayMode)),

        // UGUI相关
        _GT(typeof(RectTransform)).SetBaseName(null).SetWrapMethods(null),
        _GT(typeof(Canvas)).SetWrapMethods(null).SetWrapFields(null),
        _GT(typeof(Button)).SetWrapMethods(null).SetWrapFields(null),
        _GT(typeof(GraphicRaycaster)).SetWrapMethods(null).SetWrapFields(null),
        _GT(typeof(Text)).SetWrapMethods(null).SetWrapFields(new []{ "text", "fontSize", "preferredWidth", "preferredHeight"}),
        _GT(typeof(Slider)).SetWrapMethods(null).SetWrapFields(new []{ "value"}),
        _GT(typeof(Image)).SetWrapMethods(null).SetWrapFields(new []{ "fillAmount"}),  // 
        _GT(typeof(InputField)).SetWrapMethods(null).SetWrapFields(new []{ "text"}),
        _GT(typeof(Toggle)).SetWrapMethods(null).SetWrapFields(new []{ "isOn"}),
        _GT(typeof(Scrollbar)).SetWrapMethods(null).SetWrapFields(new []{ "size"}),

        // 第三方插件
        _GT(typeof(DG.Tweening.DOTweenAnimation)).SetBaseName(null).SetWrapMethods(new []{ "DOPause", "DORestart"}).SetWrapFields(null),
        _GT(typeof(DG.Tweening.DOTweenPlayer)).SetBaseName(null).SetWrapMethods(new []{ "Restart" , "Stop", "GoToStartPos","GoToEndPos","FindAndDoRestart","FindAndDoKill"}).SetWrapFields(null), 

        // 协议 - 通过[NoToLua]限定lua接口
        _GT(typeof(CGameSession)),

        // 自定义类 - 通过[NoToLua]限定lua接口
        _GT(typeof(CFxOne)),
        _GT(typeof(CMotor)),
        _GT(typeof(CLinearMotor)),
        _GT(typeof(CTargetMotor)),
        _GT(typeof(CBallCurvMotor)),        
        _GT(typeof(CParabolicMotor)),
		_GT(typeof(CMutantBezierMotor)),
        _GT(typeof(CHUDFollowTarget)),
        _GT(typeof(GameObjectPool)),
        _GT(typeof(AnimationUnit)),
        _GT(typeof(CombatStateChangeBehaviour)),
        _GT(typeof(NpcStandBehaviour)),
        _GT(typeof(HorseStandBehaviour)),
        _GT(typeof(CFxFollowTarget)),
        _GT(typeof(CGhostEffectMan)),
        
        // 自定义UI - 通过[NoToLua]限定lua接口
        _GT(typeof(UIEventListener)),
        _GT(typeof(GBase)).SetBaseName("Behaviour"),
        _GT(typeof(GNewUIBase)).SetBaseName("Behaviour"),
        _GT(typeof(GNewGridBase)),
        _GT(typeof(GNewListBase)),
        _GT(typeof(GNewList)),
        _GT(typeof(GNewListLoop)),
        //_GT(typeof(GImageModel)), 此类有手动代码，不能自动暴口，但保留ClassID 
        _GT(typeof(GBlood)),
        //_GT(typeof(GLayout)),  // Lua中不操作，应该不用wrap，类保留
        //_GT(typeof(GLayoutAuto)),  // Lua中不操作，应该不用wrap，类保留
        _GT(typeof(GText)),
        _GT(typeof(UITemplate)),    
        _GT(typeof(GNewTableBase)),
        _GT(typeof(GNewTabList)),
        _GT(typeof(GNewLayoutTable)),
        _GT(typeof(GButton)),
        _GT(typeof(GScaleScroll)),
        _GT(typeof(GNewUI.GUIScene)),
        _GT(typeof(GNewIOSToggle)),
        _GT(typeof(GDragablePageView)),
    };

    [MenuItem("Hoba Tools/Lua Binding/导出指定wrap类文件", false, 11)]
    public static void Binding()
    {
        if (!Check()) return;

        BindType[] list = binds;

        for (int i = 0; i < list.Length; i++)
        {
            ToLuaExport.Clear();
            ToLuaExport.className = list[i].name;
            ToLuaExport.type = list[i].type;
            ToLuaExport.isStaticClass = list[i].IsStatic;
            ToLuaExport.baseClassName = list[i].baseName;
            ToLuaExport.wrapClassName = list[i].wrapName;
            ToLuaExport.libClassName = list[i].libName;
            ToLuaExport.wrapAllMethods = list[i].Is2WrapAllMethods;
            ToLuaExport.methodsFilter = list[i].wrapMethods;
            ToLuaExport.wrapAllFields = list[i].Is2WrapAllFields;
            ToLuaExport.fieldsFilter = list[i].wrapFields;
            ToLuaExport.Generate(null);
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < list.Length; i++)
        {
            sb.AppendFormat("\t\t{0}Wrap.Register();\r\n", list[i].wrapName);
        }

        EditorApplication.isPlaying = false;

        GenLuaBinder();
        GenTypeIDMap();
        Debug.Log("Generate lua binding files over");
        AssetDatabase.Refresh();
    }
    
    static bool Check()
    {
        for (int i = 0; i < binds.Length; i++)
        {
            var t = binds[i].type;
            if (!t.IsEnum && !t.IsClass)
            {
                Debug.LogErrorFormat("Can not wrap {0}", t);
                return false;
            }
        }

        return true;
    }

    static void GenTypeIDMap()
    {
        List<Type> list = new List<Type>();
        for (int i = 0; i < binds.Length; i++)
        {
            var t = binds[i].type;
            if (t.IsClass)
                list.Add(t);
        }

        // 手动添加 因不能自动暴口 
        {
            list.Add(typeof(Animation));
            list.Add(typeof(GImageModel));
            list.Add(typeof(Camera));
            list.Add(typeof(GWebView));
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("public class WrapClassID");
        sb.AppendLine("{");
        sb.AppendFormat("\tprivate static Type[] _WrapList = new Type[{0}];\r\n", list.Count);

        sb.AppendLine("\tpublic static void Register()");
        sb.AppendLine("\t{");
        for (int i = 0; i < list.Count; i++)
        {
            sb.AppendFormat("\t\t_WrapList[{0}] = typeof({1});\r\n", i, list[i]);
        }
        sb.AppendLine("\t}");

        sb.AppendLine("\tpublic static Type GetClassType(int id)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\tif (id < 0 || id >= _WrapList.Length)");
        sb.AppendLine("\t\t\treturn null;");
        sb.AppendLine("\t\treturn _WrapList[id];");
        sb.AppendLine("\t}");

        sb.AppendLine("}");

        string file = Application.dataPath + WrapFileSavedPath + "Base/WrapClassID.cs";

        using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
        {
            textWriter.Write(sb.ToString());
            textWriter.Flush();
            textWriter.Close();
        }

        sb.Length = 0;
        sb.AppendLine("_G.ClassType = {\r\n");
        for (int i = 0; i < list.Count; i++)
        {
            var typeName = list[i].ToString();

            if (typeName == "System.Object")
                typeName = "SystemObject";
            else if (typeName == "System.Type")
                typeName = "SystemType";
            else
            {
                var splits = typeName.Split('.');
                typeName = splits[splits.Length - 1];
            }
            sb.AppendFormat("\t{0} = {1},\r\n", typeName, i);
        }
        sb.AppendLine("}");

        string luaFile = Application.dataPath + "/../../GameRes/Lua/UnityClass/WrapClassID.lua";

        var utf8WithoutBom = new System.Text.UTF8Encoding(false);
        using (StreamWriter textWriter = new StreamWriter(luaFile, false, utf8WithoutBom))
        {
            textWriter.Write(sb.ToString());
            textWriter.Flush();
            textWriter.Close();
        }

        AssetDatabase.Refresh();
    }

    static void GenLuaBinder()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("public static class AutoLuaBinder");
        sb.AppendLine("{");
        sb.AppendLine("\tpublic static void Bind(IntPtr L)");
        sb.AppendLine("\t{");

        string[] files = Directory.GetFiles("Assets" + WrapFileSavedPath, "*.cs", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files.Length; i++)
        {
            string wrapName = Path.GetFileName(files[i]);
            int pos = wrapName.LastIndexOf(".");
            wrapName = wrapName.Substring(0, pos);
            sb.AppendFormat("\t\t{0}.Register(L);\r\n", wrapName);
        }

        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string file = Application.dataPath + WrapFileSavedPath + "Base/AutoLuaBinder.cs";

        using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
        {
            textWriter.Write(sb.ToString());
            textWriter.Flush();
            textWriter.Close();
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("Hoba Tools/Lua Binding/清理所有wrap类文件", false, 13)]
    static void ClearLuaBinder()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("public static class AutoLuaBinder");
        sb.AppendLine("{");
        sb.AppendLine("\tpublic static void Bind(IntPtr L)");
        sb.AppendLine("\t{");
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        string file = Application.dataPath + WrapFileSavedPath + "Base/AutoLuaBinder.cs";

        using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
        {
            textWriter.Write(sb.ToString());
            textWriter.Flush();
            textWriter.Close();
        }
        string path = Application.dataPath + WrapFileSavedPath;
        string[] names = Directory.GetFiles(path);
        for (int i = 0; i < names.Length; ++i)
        {
            File.Delete(names[i]); //删除缓存
        }
        AssetDatabase.Refresh();
    }

}
