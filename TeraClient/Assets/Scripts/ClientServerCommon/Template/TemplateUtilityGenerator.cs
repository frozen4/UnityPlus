using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR

public static class TemplateUtilityGenerator
{
    private static List<string> ForbidTemplateNames = new List<string>()
    {
        "Npc",
        "Service",
        "Profession",
        "Faction",
        "Monster",
        "Mine",
        "Obstacle",
        "Item",
        "Map",
    };

    private static List<Type> GetGenericListMemberTypes(Type t)         //返回List成员的所有itemType
    {
        List<Type> types = new List<Type>();
        HashSet<string> typenames = new HashSet<string>();

        if (t != null && t.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false) != null)
        {
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false);
                if (propMember == null)
                    continue;

                Type propType = prop.PropertyType;

                bool bIsGenericList = propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>);

                if (bIsGenericList)
                {
                    Type itemType = propType.GetGenericArguments()[0];

                    if (!typenames.Contains(itemType.Name))
                    {
                        types.Add(itemType);
                        typenames.Add(itemType.Name);
                    }
                }
            }
        }
        return types;
    }

    //类型是否可以输出，类型是否符合输出的规则，太复杂的类型暂不输出
    private static bool IsTypeCanOutput(Type t)
    {
        bool bOutput = false;
        if (t != null && t.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false) != null)
        {
            bOutput = true;

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false);
                if (propMember == null)
                    continue;

                Type propType = prop.PropertyType;

                if (!propType.IsPrimitive &&
                    propType != typeof(string) &&
                    !propType.IsEnum)
                {

                    //Console.WriteLine(prop.PropertyType.Name);
                    bOutput = false;
                    break;
                }
            }
        }
        return bOutput;
    }

    private static void GetOutputTypes(List<Type> listOutputTypes)
    {
        listOutputTypes.Clear();
        Assembly thisAssembly = Assembly.GetExecutingAssembly();
        foreach (string name in Template.TemplateManagerCollection.TemplateNameCollection)
        {
            if (ForbidTemplateNames.Contains(name))
                continue;

            Type t = thisAssembly.GetType("Template." + name, false);
            if (IsTypeCanOutput(t))                               //是否输出list成员
                listOutputTypes.Add(t);
        }
    }

    public static void OutputTypeInfo()
    {
        List<Type> listOutputTypes = new List<Type>();
        GetOutputTypes(listOutputTypes);

        Console.WriteLine("Total Template: {0}, Output Template: {1}", Template.TemplateManagerCollection.TemplateNameCollection.Count, listOutputTypes.Count);
    }

    private static void GetOutputTypes(List<Type> listOutputTypes, List<string> outputTypeNames)
    {
        GetOutputTypes(listOutputTypes);

        SortedDictionary<string, Type> typeNameSet = new SortedDictionary<string, Type>();
        foreach (Type t in listOutputTypes)
        {
            typeNameSet.Add(t.Name, t);
        }
        outputTypeNames.Clear();
        outputTypeNames.AddRange(typeNameSet.Keys);
    }

    private static void GetOutputFields(List<string> outputFieldNames)
    {
        List<Type> listOutputTypes = new List<Type>();
        GetOutputTypes(listOutputTypes);

        SortedDictionary<string, int> fieldNameSet = new SortedDictionary<string, int>();
        foreach (Type t in listOutputTypes)
        {
            if (t != null && t.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false) != null)
            {
                PropertyInfo[] properties = t.GetProperties();
                foreach (PropertyInfo prop in properties)
                {
                    var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false);
                    if (propMember == null)
                        continue;

                    fieldNameSet[prop.Name] = 0;
                }
            }
        }

        outputFieldNames.Clear();
        outputFieldNames.AddRange(fieldNameSet.Keys);
    }

    public static void GenerateTemplateCS(string filename)
    {
        List<Type> listOutputTypes = new List<Type>();
        List<string> outputTypeNames = new List<string>();
        List<string> outputFieldNames = new List<string>();

        GetOutputTypes(listOutputTypes, outputTypeNames);
        GetOutputFields(outputFieldNames);

        StreamWriter writer = File.CreateText(filename);
        if (writer != null)
        {
            //输出所有类型
            writer.WriteLine("/*");
            writer.WriteLine("_G.pb_csharp_template_table = {}");
            for (int i = 0; i < outputTypeNames.Count; ++i)
            {
                writer.WriteLine("\tpb_csharp_template_table[\"{0}\"] =  {1}", outputTypeNames[i], i);
            }
            writer.WriteLine("");

            //输出所有字段
            writer.WriteLine("_G.pb_csharp_field_table = {}");
            for (int i = 0; i < outputFieldNames.Count; ++i)
            {
                writer.WriteLine("\tpb_csharp_field_table[\"{0}\"] =  {1}", outputFieldNames[i], i);
            }
            writer.WriteLine("*/");
            writer.WriteLine("");

            //类开始
            writer.WriteLine(classBegin);

            //定义id2templateName
            writer.WriteLine(id2TemplateNameBegin);
            for (int i = 0; i < outputTypeNames.Count; ++i)
            {
                writer.WriteLine("\t\t\"{0}\",\t\t//{1},", outputTypeNames[i], i);
            }
            writer.WriteLine(id2TemplateNameEnd);

            //定义id2fieldName
            writer.WriteLine(id2FieldNameBegin);
            for (int i = 0; i < outputFieldNames.Count; ++i)
            {
                writer.WriteLine("\t\t\"{0}\",\t\t//{1},", outputFieldNames[i], i);
            }
            writer.WriteLine(id2FieldNameEnd);

            //宏开始
            writer.WriteLine("#if !SERVER_USE");

            //输出构造函数
            writer.WriteLine("\tstatic TemplateUtility()");
            writer.WriteLine("\t{");
            foreach (Type t in listOutputTypes)
            {
                writer.WriteLine("\t\tInit_{0}();", t.Name);
            }
            writer.WriteLine("\t}");
            writer.WriteLine("");

            //定义map
            writer.WriteLine("\t//定义map");
            foreach (Type t in listOutputTypes)
            {
                writer.WriteLine("\tstatic Dictionary<string, Action<IntPtr, {0}>> ActionMap_{1};", t.Name, t.Name);
            }
            writer.WriteLine("");

            //生成Init方法
            foreach (Type t in listOutputTypes)
            {
                PrintProtoType(writer, t);
            }
            writer.WriteLine("");

            //生成template访问方法
            foreach (Type t in listOutputTypes)
            {
                PrintTemplateFunction(writer, t);
            }
            writer.WriteLine("");

            //生成总的入口访问方法
            PrintEntryTemplateEx(writer);

            PrintEntryTemplateBegin(writer);
            foreach (Type t in listOutputTypes)
            {
                PrintEntryTemplateCase(writer, t.Name);
            }
            PrintEntryTemplateEnd(writer);

            //宏结束
            writer.WriteLine("#endif");

            //类结束
            writer.WriteLine(classEnd);

            writer.WriteLine("");

            writer.Close();
        }
    }

    public static void GenerateElementDataUtilityLua(string filename)
    {
        List<Type> listOutputTypes = new List<Type>();
        List<string> outputTypeNames = new List<string>();
        List<string> outputFieldNames = new List<string>();

        GetOutputTypes(listOutputTypes, outputTypeNames);
        GetOutputFields(outputFieldNames);

        StreamWriter writer = File.CreateText(filename);
        if (writer != null)
        {
            //输出所有类型
            writer.WriteLine("_G.pb_csharp_template_table = {}");
            for (int i = 0; i < outputTypeNames.Count; ++i)
            {
                writer.WriteLine("\tpb_csharp_template_table[\"{0}\"] =  {1}", outputTypeNames[i], i);
            }
            writer.WriteLine("");

            //输出所有字段
            writer.WriteLine("_G.pb_csharp_field_table = {}");
            for (int i = 0; i < outputFieldNames.Count; ++i)
            {
                writer.WriteLine("\tpb_csharp_field_table[\"{0}\"] =  {1}", outputFieldNames[i], i);
            }

            writer.WriteLine("");

            writer.WriteLine(elementDataUtilityLua_AddMember);

            writer.WriteLine("");

            writer.WriteLine("");

            writer.WriteLine(elementDataUtilityLua);

            writer.WriteLine("");

            writer.Close();
        }
    }

    /*
     
     local pb_meta_listmember_table = {}

local AddTemplateListMember = function (template, member, listCount)
	local n = template._tname..'#'..member

	if pb_meta_listmember_table[n] == nil then
		pb_meta_listmember_table[n] = {

			__index = function(t, key)
                --print('__index:', key)
				if key == '_tid' or key == '_tname' or key == '_tidx' then
				  return t[key]
				else
				  local template_name_index = _G.pb_csharp_template_table[rawget(t, '_tname')]
				  local template_key_index = _G.pb_csharp_field_table[key]
				  return GameUtil.GetTemplateListMemberValue(template_name_index, rawget(t, '_tid'), template_key_index, rawget(t, '_tidx'))
			end
		}
	end

	local tmember = {}
	for i = 1, listCount do 
		tmember[i] = setmetatable({_tid = template.tid, _tname = template._tname, _tidx = i }, pb_meta_listmember_table[n])
	end
	template.member = tmember
end
     
     * */

    const string elementDataUtilityLua_AddMember = @"
local pb_meta_listmember_table = {}

local AddTemplateListMember = function (template, member, listCount)
	    local n = template._tname..'#'..member

	    if pb_meta_listmember_table[n] == nil then
		    pb_meta_listmember_table[n] = {
			    __index = function(t, key)
                    --print('__index:', key)
				    if key == '_tid' or key == '_tname' or key == '_tidx' then
				      return t[key]
				    else
				      local template_name_index = _G.pb_csharp_template_table[rawget(t, '_tname')]
				      local template_key_index = _G.pb_csharp_field_table[key]
				      return GameUtil.GetTemplateListMemberValue(template_name_index, rawget(t, '_tid'), template_key_index, rawget(t, '_tidx'))
				      --return GameUtil.GetTemplateValue(rawget(t, '_tname'), rawget(t, '_tid'), key)
				    end
			    end
		    }
	    end

	    local tmember = {}
	    for i = 1, listCount do 
		    tmember[i] = setmetatable({_tid = template.tid, _tname = template._tname, _tidx = i }, pb_meta_listmember_table[n])
	    end
	    template.member = tmember
end
";


    const string elementDataUtilityLua = @"
local pb_meta_table = {}

_G.GetTemplateInternalCSharp = function (tid, name, map)
		if tid == 0 then return nil end
		
		if map ~= nil and map[tid] ~= nil then
			return map[tid]
		else
            if GameUtil.HasTemplateData(name, tid) == false then
				warn(name .. ' template data has error, tid = ' .. tid)
				return nil
			end 

			if pb_meta_table[name] == nil then
				pb_meta_table[name] = {
					__index = function(t, key)
                    --print('__index:', key)
					if key == '_tid' or key == '_tname' then
					  return t[key]
					else
					  local template_name_index = _G.pb_csharp_template_table[rawget(t, '_tname')]
					  local template_key_index = _G.pb_csharp_field_table[key]
					  return GameUtil.GetTemplateValueEx(template_name_index, rawget(t, '_tid'), template_key_index)
					  --return GameUtil.GetTemplateValue(rawget(t, '_tname'), rawget(t, '_tid'), key)
					end
				  end
				}
			end
		
			local template = setmetatable({_tid = tid, _tname = name}, pb_meta_table[name])
			if map ~= nil then
				map[tid] = template
			end
			return template
		end
	end
";

    const string classBegin = @"
using System.Collections.Generic;
using Common;
using System.IO;
using System;
using LuaInterface;

namespace Template
{
    public static class TemplateUtility
    {
";

    const string classEnd = @"
    }
}
";

    const string id2TemplateNameBegin = @"
    private static string[] id2TemplateNameArray = 
    {    
";

    const string id2TemplateNameEnd = @"
    };
";

    const string id2FieldNameBegin = @"
    private static string[] id2FieldNameArray =
    {
";

    const string id2FieldNameEnd = @"
    };
";

    static void PrintProtoType(StreamWriter writer, Type t)
    {
        writer.WriteLine("\tstatic void Init_{0}()", t.Name);
        writer.WriteLine("\t{");

        writer.WriteLine("\t\tActionMap_{0} = new Dictionary<string, Action<IntPtr, {1}>>();", t.Name, t.Name);

        PropertyInfo[] properties = t.GetProperties();
        foreach (PropertyInfo prop in properties)
        {
            var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoMemberAttribute), false);
            if (propMember == null)
                continue;

            // Console.WriteLine(prop.Name);
            string str1 = HobaText.Format("\t\tActionMap_{0}[\"{1}\"] = (L, prop) => ", t.Name, prop.Name);
            string str2;

            // if (prop.PropertyType == typeof(string))    //转utf8
            //    str2 = "{" + HobaString.Format("LuaScriptMgr.Push(L, Util.unicode_to_utf8(prop.{0}));", prop.Name) + "};";
            //else

            if (prop.PropertyType.IsEnum)
                str2 = "{" + HobaText.Format("LuaScriptMgr.Push(L, (int)prop.{0});", prop.Name) + "};";
            else
                str2 = "{" + HobaText.Format("LuaScriptMgr.Push(L, prop.{0});", prop.Name) + "};";

            writer.WriteLine(str1 + str2);
        }

        writer.WriteLine("\t}\n");
    }

    const string strTemplateFunc = @"
    static void PushTemplateFieldValue_{0}(IntPtr L, int tid, string field)
    {{
        var manager = Template.{1}Module.Manager.Instance;
        var prop = manager.GetTemplate(tid);
        if (prop == null)
        {{
            LuaDLL.lua_pushnil(L);
            return;
        }}

        Action<IntPtr, {2}> action;
        if (!ActionMap_{3}.TryGetValue(field, out action))
        {{
            LuaDLL.lua_pushnil(L);
            return;
        }}
        else
        {{
            action.Invoke(L, prop);
        }}
    }}
    ";

    static void PrintTemplateFunction(StreamWriter writer, Type t)
    {
        writer.WriteLine(strTemplateFunc, t.Name, t.Name, t.Name, t.Name);
    }

    static void PrintTemplateFunction(StreamWriter writer, string module, Type t)
    {
        writer.WriteLine(strTemplateFunc, module, t.Name, t.Name, t.Name);
    }

    const string strEntryTemplateFunctionEx = @"
        public static void PushTemplateFieldValueEx(IntPtr L, int template_name_index, int tid, int template_field_index)
        {
            string template_name = id2TemplateNameArray[template_name_index];
            string field_name = id2FieldNameArray[template_field_index];
            PushTemplateFieldValue(L, template_name, tid, field_name);
        }
";

    const string strEntryTemplateFunctionBegin = @"
        public static void PushTemplateFieldValue(IntPtr L, string template_name, int tid, string template_field)
        {
#if SERVER_USE
            LuaDLL.lua_pushnil(L);
#else
            switch(template_name)
            {
";

    const string strEntryTemplateFunctionEnd = @"
                default:
                    LuaDLL.lua_pushnil(L);
                    break;
            }
#endif
        }
";

    const string strEntryTemplateCase = @"
                case ""{0}"":
                    PushTemplateFieldValue_{1}(L, tid, template_field);
                    break;
";

    static void PrintEntryTemplateEx(StreamWriter writer)
    {
        writer.Write(strEntryTemplateFunctionEx);
    }

    static void PrintEntryTemplateBegin(StreamWriter writer)
    {
        writer.Write(strEntryTemplateFunctionBegin);
    }

    static void PrintEntryTemplateEnd(StreamWriter writer)
    {
        writer.Write(strEntryTemplateFunctionEnd);
    }

    static void PrintEntryTemplateCase(StreamWriter writer, string name)
    {
        writer.Write(strEntryTemplateCase, name, name);
    }
}

#endif