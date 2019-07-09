using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR

/*
 * 把data字段翻译成可以填充csv的内容 key-value，动态生成cs文件，外部程序调用然后执行
 * 入口: 读取不翻译的列表: ReadForbidFieldList
 * 入口: 读取要翻译的列表: ReadAllowFieldList
 * 入口, 输出哪些字段: GetAllFieldInfos
 * 入口，输出自动翻译bat: GenerateTranslateBAT
 * 入口，输出列表，供填充csv: GenerateTranlateStringCS
 * 入口，根据csv列表的 List<Template.STranslateValue> 生成data reader: GenerateCsvToDataReaderCS
 * */

public static class TemplateFieldTranslator
{

    //输出的Template Type
    private static SortedDictionary<string, Type> GetOutputTypes()
    {
        SortedDictionary<string, Type> typeNameSet = new SortedDictionary<string, Type>();
        Assembly thisAssembly = Assembly.GetExecutingAssembly();
        foreach (string name in Template.TemplateManagerCollection.TemplateNameCollection)
        {
            Type t = thisAssembly.GetType("Template." + name, false);                            //是否输出list成员
            typeNameSet[name] = t;
        }
        return typeNameSet;
    }

    //返回一个类型中的字符串类型
    public static List<PropertyInfo> GetTypeFields(Type t)
    {
        List<PropertyInfo> fieldList = new List<PropertyInfo>();

        if (t != null && t.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false) != null)
        {
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false);
                if (propMember == null)
                    continue;

                fieldList.Add(prop);
            }
        }

        return fieldList;
    }

    //类型内是否包含List成员
    public static bool ContainsGenericListFields(Type t)
    {
        bool bContain = false;
        if (t != null && t.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false) != null)
        {
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false);
                if (propMember == null)
                    continue;

                bContain = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>);

                if (bContain)
                    break;
            }
        }
        return bContain;
    }

    public class CStringFieldInfo
    {
        public List<PropertyInfo> Fields = new List<PropertyInfo>();              //string字段列表
        public SortedDictionary<string, CStringFieldInfo> ListFields = new SortedDictionary<string, CStringFieldInfo>();       //List<结构>中所包含的string字段列表
        public SortedDictionary<string, CStringFieldInfo> MemberFields = new SortedDictionary<string, CStringFieldInfo>();      //包含的成员是一个结构，里面包含字符串结构
    }

    public static CStringFieldInfo GenerateFieldInfo(Type t)
    {
        CStringFieldInfo fieldInfo = new CStringFieldInfo();

        if (t != null && t.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false) != null)
        {
            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false);
                if (propMember == null)
                    continue;

                if (prop.PropertyType == typeof(string))            //字符串类型
                    fieldInfo.Fields.Add(prop);

                //判断是否list类型
                bool bIsGenericList = prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>);

                //内置类型
                if (!bIsGenericList &&
                    !prop.PropertyType.IsPrimitive &&
                    prop.PropertyType != typeof(string) &&
                    !prop.PropertyType.IsEnum)
                {
                    CStringFieldInfo memberFieldInfo = GenerateFieldInfo(prop.PropertyType);
                    fieldInfo.MemberFields[prop.Name] = memberFieldInfo;
                }

                //List类型
                if (bIsGenericList)
                {
                    Type itemType = prop.PropertyType.GetGenericArguments()[0];

                    if (!fieldInfo.ListFields.ContainsKey(prop.Name))
                    {
                        CStringFieldInfo itemFieldInfo = GenerateFieldInfo(itemType);
                        fieldInfo.ListFields[prop.Name] = itemFieldInfo;
                    }
                }
            }
        }

        return fieldInfo;
    }

    //template名, 字段名
    public static SortedDictionary<string, CStringFieldInfo> GetOutputFields()
    {
        SortedDictionary<string, CStringFieldInfo> templateFieldMap = new SortedDictionary<string, CStringFieldInfo>();
        SortedDictionary<string, Type> templateNameSet = GetOutputTypes();

        foreach (var kv in templateNameSet)
        {
            string templateName = kv.Key;
            Type t = kv.Value;

            if (t != null && t.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false) != null)
            {
                PropertyInfo[] properties = t.GetProperties();
                foreach (PropertyInfo prop in properties)
                {
                    var propMember = prop.GetCustomAttributes(typeof(global::ProtoBuf.ProtoContractAttribute), false);
                    if (propMember == null)
                        continue;

                    CStringFieldInfo fieldInfo = GenerateFieldInfo(t);
                    templateFieldMap[templateName] = fieldInfo;
                }
            }
        }

        return templateFieldMap;
    }

    public static bool USE_FORBID = true;

    public static List<STranslateField> _ForbidFieldList = new List<STranslateField>();

    public static List<STranslateField> _AllowFieldList = new List<STranslateField>();

    //入口，读取不翻译的列表
    public static void ReadForbidFieldList(string filename)
    {
        _ForbidFieldList.Clear();

        string[] lines = File.ReadAllLines(filename);
        if (lines != null)
        {
            foreach (string line in lines)
            {
                STranslateField field = new STranslateField();
                int len = line.Length;
                for (int i = 0; i < len; ++i)
                {
                    char cc = line[i];
                    if (cc == '.')
                    {
                        field.parentMemberName = line.Substring(0, i);
                        if (i + 1 < len)
                            field.propName = line.Substring(i + 1);
                    }
                }

                _ForbidFieldList.Add(field);
            }
        }
    }

    //入口，读取翻译的列表
    public static void ReadAllowFieldList(string filename)
    {
        _AllowFieldList.Clear();

        string[] lines = File.ReadAllLines(filename);
        if (lines != null)
        {
            foreach (string line in lines)
            {
                STranslateField field = new STranslateField();
                int len = line.Length;
                for (int i = 0; i < len; ++i)
                {
                    char cc = line[i];
                    if (cc == '.')
                    {
                        field.parentMemberName = line.Substring(0, i);
                        if (i + 1 < len)
                            field.propName = line.Substring(i + 1);
                    }
                }

                _AllowFieldList.Add(field);
            }
        }
    }

    //入口, 输出哪些字段
    public static Dictionary<string, List<string>> GetAllFieldInfos(bool ignoreTranslateTxt)
    {
        SortedDictionary<string, TemplateFieldTranslator.CStringFieldInfo> dicTempFields = TemplateFieldTranslator.GetOutputFields();

        Dictionary<string, List<string>> dicFields = new Dictionary<string, List<string>>();
        foreach (var kv in dicTempFields)
        {
            string template_Name = kv.Key;
            TemplateFieldTranslator.CStringFieldInfo info = kv.Value;

            List<string> listFields = GetFieldInfo(template_Name, info, ignoreTranslateTxt);
            dicFields.Add(template_Name, listFields);
        }
        return dicFields;
    }

    //入口，输出自动翻译bat
    public static void GenerateTranslateBAT(string filename, string inFile, int inColumn, string outFile, int outColumn, bool bAddition)
    {
        SortedDictionary<string, TemplateFieldTranslator.CStringFieldInfo> dicTempFields = TemplateFieldTranslator.GetOutputFields();

        StreamWriter writer = File.CreateText(filename);
        if (writer != null)
        {
            string strHead = string.Format(@"@echo on

set CUR_DIR=%~dp0
set SRC_DIR=%CUR_DIR%{0}\
set DST_DIR=%CUR_DIR%{1}\

cd %CUR_DIR%", inFile, outFile);
            writer.WriteLine(strHead);

            foreach (var kv in dicTempFields)
            {
                string template_Name = kv.Key;

                if (bAddition)
                    writer.WriteLine(string.Format("AutoTranslateSharp %SRC_DIR%{0}_0.csv {1} %DST_DIR%{2}_0.csv {3}", template_Name, inColumn, template_Name, outColumn));
                else
                    writer.WriteLine(string.Format("AutoTranslateSharp %SRC_DIR%{0}.csv {1} %DST_DIR%{2}.csv {3}", template_Name, inColumn, template_Name, outColumn));
            }

            writer.WriteLine("pause");
        }
        writer.Close();
    }

    //入口，输出列表，供填充csv
    public static void GenerateTranlateStringCS(string filename, bool ignoreTranslateTxt)
    {
        const string classDefBegin = @"
using System;
using System.Collections.Generic;

namespace Template
{
    public static class TemplateTranslateUtility
    {
        private static List<STranslateValue> _ListTranslateValue = new List<STranslateValue>();

        public static List<STranslateValue> GetListTranslateValue() { return _ListTranslateValue; }
";

        const string classMethodBegin = @"
        private static void GenerateTranlateValues()
        {
            _ListTranslateValue.Clear();
        ";

        const string classMethodEnd = @"
        }  
";

        const string classDefEnd = @"
    }
}
";
        StreamWriter writer = File.CreateText(filename);
        if (writer != null)
        {
            //类开始
            writer.WriteLine(classDefBegin);

            //方法开始
            writer.WriteLine(classMethodBegin);

            //生成方法内容
            string strMethod = Get_GenerateTranlateValues_CS();
            writer.WriteLine(strMethod);

            //方法结束
            writer.WriteLine(classMethodEnd);

            //生成每个Template的方法
            {
                SortedDictionary<string, TemplateFieldTranslator.CStringFieldInfo> dicTempFields = TemplateFieldTranslator.GetOutputFields();

                foreach (var kv in dicTempFields)
                {
                    //每个template
                    string template_Name = kv.Key;
                    TemplateFieldTranslator.CStringFieldInfo info = kv.Value;

                    string strSubMethod = Get_GenerateTranslateValues_Template_CS(template_Name, info, ignoreTranslateTxt);
                    writer.WriteLine(strSubMethod);
                }
            }

            //类结束
            writer.WriteLine(classDefEnd);

        }
        writer.Close();
    }

    //入口，根据csv列表的 List<Template.STranslateValue> 生成data reader
    public static void GenerateCsvToDataReaderCS(string filename, List<Template.STranslateValue> listValues)
    {
        const string classDefBegin = @"
using System;
using System.Collections.Generic;

namespace Template
{
    public static class TemplateTranslateReader
    {
";

        const string classMethodBegin = @"
        public static void ReadTranslateValuesToData(List<STranslateValue> listValues)
        {   
        ";

        const string classMethodEnd = @"
        }  
";

        const string classDefEnd = @"
    }
}
";

        StreamWriter writer = File.CreateText(filename);
        if (writer != null)
        {
            //类开始
            writer.WriteLine(classDefBegin);

            //生成Actions
            Get_ReadTranslateValuesToDataActions_CS(writer, listValues);

            //生成多个读取方法内容
            Get_ReadTranslateValuesToData_CS(writer, listValues);

            //方法开始
            writer.WriteLine(classMethodBegin);

            Get_ReadTranslateValuesToDataAll_CS(writer, listValues);

            //方法结束
            writer.WriteLine(classMethodEnd);

            //类结束
            writer.WriteLine(classDefEnd);

        }
        writer.Close();
    }

    public struct STransEntry
    {
        public string name;
        public bool isList;
    }

    public struct STranslateField : IEquatable<STranslateField>
    {
        public string parentMemberName;
        public string propName;           //包含翻译文字


        public bool IsForbid(List<STranslateField> forbidFieldList)
        {
            bool bForbid = false;
            foreach (var v in forbidFieldList)
            {
                if (v.parentMemberName == "*")
                {
                    if (v.propName == propName)
                    {
                        bForbid = true;
                        break;
                    }
                }
                else
                {
                    if (string.Compare(v.parentMemberName, parentMemberName, true) == 0 &&
                        string.Compare(v.propName, propName, true) == 0)
                    {
                        bForbid = true;
                        break;
                    }
                }
            }
            return bForbid;
        }

        public bool IsAllow(List<STranslateField> allowFieldList)
        {
            bool bAllow = false;
            foreach (var v in allowFieldList)
            {
                if (string.Compare(v.parentMemberName, parentMemberName, true) == 0 &&
                                        string.Compare(v.propName, propName, true) == 0)
                {
                    bAllow = true;
                    break;
                }
            }
            return bAllow;
        }

        public static string fixParentMemberName(string memberName)
        {
            int len = memberName.Length;

            StringBuilder str = new StringBuilder();
            bool bLeft = false;
            bool bRight = false;
            for (int i = 0; i < len; ++i)
            {
                char cc = memberName[i];

                if (cc == '[')
                {
                    bRight = false;
                    if (!bLeft)
                        bLeft = true;
                    else
                        continue;
                }
                else if (cc == ']')
                {
                    bLeft = false;
                    if (!bRight)
                        bRight = true;
                    else
                        continue;
                }
                else
                {
                    bLeft = bRight = false;
                }

                str.Append(cc);
            }
            return str.ToString();
        }
        public bool Equals(STranslateField other)
        {
            if (this.parentMemberName == other.parentMemberName && this.propName == other.propName)
                return true;
            else
                return false;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is STranslateField))
                return false;
            return Equals((STranslateField)obj);

        }
        public override int GetHashCode()
        {
            return parentMemberName.GetHashCode() ^ propName.GetHashCode();
        }

    }

    private static List<string> GetFieldInfo(string fieldName, TemplateFieldTranslator.CStringFieldInfo info, bool ignoreTranslateTxt)
    {
        List<STranslateField> transFields = new List<STranslateField>();

        GetFieldInfo(transFields, fieldName, info, ignoreTranslateTxt);

        List<string> listFields = new List<string>();
        foreach (STranslateField transfield in transFields)
        {
            listFields.Add(string.Format("{0}.{1}", transfield.parentMemberName, transfield.propName));
        }

        return listFields;
    }

    private static void GetFieldInfo(List<STranslateField> transFields, string fieldName, TemplateFieldTranslator.CStringFieldInfo info, bool ignoreTranslateTxt)
    {
        string typeName;
        if (fieldName[fieldName.Length - 1] == '>')
            typeName = fieldName;
        else
            typeName = '[' + fieldName + ']';

        //结构中的string字段
        foreach (var prop in info.Fields)
        {
            //Console.WriteLine("{0}   {1}", typeName, prop.Name);
            STranslateField field = new STranslateField() { parentMemberName = STranslateField.fixParentMemberName(typeName), propName = prop.Name };

            if (USE_FORBID)
            {
                if (!field.IsForbid(_ForbidFieldList))
                    transFields.Add(field);
            }
            else
            {
                if (ignoreTranslateTxt || field.IsAllow(_AllowFieldList))
                    transFields.Add(field);
            }       
        }

        //结构中的list字段item中的string字段
        foreach (var listkv in info.ListFields)
        {
            /*
            string listFieldName = listkv.Key;
            List<PropertyInfo> props = listkv.Value;
            foreach (var prop in props)
            {
                //string parentMemberName = string.Format("{0}{1}", typeName, listFieldName);
                //Console.WriteLine("{0}  {1}", parentMemberName, prop.Name);
                STranslateField field = new STranslateField() { parentMemberName = STranslateField.fixParentMemberName(typeName), listName = listFieldName, propName = prop.Name };
                if (!field.IsForbid(_ForbidFieldList))
                    transFields.Add(field);
            }
             * */
            string memberFieldName = '<' + listkv.Key + '>';
            TemplateFieldTranslator.CStringFieldInfo fieldInfo = listkv.Value;
            GetFieldInfo(transFields, typeName + memberFieldName, fieldInfo, ignoreTranslateTxt);
        }

        //结构中的子结构中的string字段
        foreach (var mkv in info.MemberFields)
        {
            string memberFieldName = '[' + mkv.Key + ']';
            TemplateFieldTranslator.CStringFieldInfo fieldInfo = mkv.Value;
            GetFieldInfo(transFields, typeName + memberFieldName, fieldInfo, ignoreTranslateTxt);
        }
    }

    static string Get_GenerateTranslateValues_Template_CS(string template_Name, TemplateFieldTranslator.CStringFieldInfo info, bool ignoreTranslateTxt)
    {
        string strText = string.Format(@"
    public static void GenerateTranlateValues_{0}()", template_Name);

        strText += @"
{";

        strText += string.Format(@"
    _ListTranslateValue.Clear();
");

        //得到如 [[[ExecutionUnit][Event]][AddFightSpeciality]]
        List<STranslateField> transFields = new List<STranslateField>();
        GetFieldInfo(transFields, template_Name, info, ignoreTranslateTxt);

        //strText += string.Format("//{0}\n", template_Name);
        //strText += string.Format("//Template.{0}Module.Manager\n", template_Name);

        strText += string.Format(@"
//{0}
foreach (var kv in Template.{1}Module.Manager.Instance.GetTemplateMap())", template_Name, template_Name);

        strText += @"
{";

        strText += string.Format(@"
    int tid = kv.Key;
    var v_{0} = kv.Value;
", template_Name);

        //读取每个字段
        foreach (var transfield in transFields)
        {
            //字段简称
            string field = string.Format("{0}.{1}", transfield.parentMemberName, transfield.propName);

            strText += "\n";

            List<STransEntry> transEntryList = ParseParentMemberNameList(transfield);

            //生成
            string parentListName = null;
            string strMember = "v_" + transEntryList[0].name;
            int nListBraces = 0;
            for (int i = 1; i < transEntryList.Count; ++i)
            {
                STransEntry entry = transEntryList[i];

                if (!entry.isList)
                {
                    strMember += string.Format(".{0}", entry.name);

                    if (parentListName != null)
                    {
                        if (i + 1 == transEntryList.Count)
                        {
                            //get string desc
                            //strText += "\t\tstring desc = \"(\" + tid + \")\" + \"" + strMember + "." + transfield.propName + "\";";
                            strText += GetFieldDesc(transEntryList, transfield.propName);

                            strText += string.Format(@"
        string v = {0}.{1}.{2};", parentListName, entry.name, transfield.propName);

                            strText += "\nstring field = \"" + field + "\";\n";

                            //添加
                            strText += @"
        if (!string.IsNullOrEmpty(v))
            _ListTranslateValue.Add(new STranslateValue() {_field = field, _desc = desc, _val = v});";
                        }
                        else
                        {
                            parentListName = string.Format("{0}.{1}", parentListName, entry.name);
                        }
                    }
                }
                else
                {
                    strMember += string.Format(".{0}", entry.name);
                    if (parentListName != null)
                        parentListName = string.Format("{0}.{1}", parentListName, entry.name);

                    string strList = (parentListName != null ? parentListName : strMember);
                    string strVar = string.Format("v_{0}", entry.name);

                    strText += string.Format(@"
    for(int i{0} = 0; i{1} < {2}.Count; ++i{3})", nListBraces, nListBraces, strList, nListBraces);
                    //try
                    strText += @"
    {try {
";
                    strText += string.Format(@"
        var {0} = {1}[i{2}];
", strVar, strList, nListBraces);

                    if (i + 1 == transEntryList.Count)
                    {
                        //get string desc
                        //strText += "\t\tstring desc = \"(\" + tid + \")\" + \"" + strMember + "[\" + i + \"]" + "." + transfield.propName + "\";";
                        strText += GetFieldDesc(transEntryList, transfield.propName);

                        strText += string.Format(@"
        string v = {0}.{1};", strVar, transfield.propName);

                        strText += "\nstring field = \"" + field + "\";\n";

                        //添加
                        strText += @"
        if (!string.IsNullOrEmpty(v))
            _ListTranslateValue.Add(new STranslateValue() {_field = field, _desc = desc, _val = v});";
                    }

                    ++nListBraces;
                    parentListName = strVar;
                }
            }

            if (nListBraces == 0)       //没有List字段
            {
                //try
                strText += @"
    {try {
";
                //get string desc
                //strText += "\t\tstring desc = \"(\" + tid + \")\" + \"" + strMember + "." + transfield.propName + "\";";
                strText += GetFieldDesc(transEntryList, transfield.propName);

                strText += string.Format(@"
        string v = {0}.{1};", strMember, transfield.propName);

                strText += "\nstring field = \"" + field + "\";\n";

                //添加
                strText += @"
        if (!string.IsNullOrEmpty(v))
            _ListTranslateValue.Add(new STranslateValue() {_field = field, _desc = desc, _val = v});";

                //catch
                strText += @"
    } catch(NullReferenceException) {}}";
            }
            else
            {
                for (int i = 0; i < nListBraces; ++i)
                {
                    //catch
                    strText += @"
    } catch(NullReferenceException) {}}";
                }
            }
        }

        strText += @"
}
";

        strText += @"
}
";

        return strText;
    }

    static string Get_GenerateTranlateValues_CS()
    {
        string strText = "";

        SortedDictionary<string, TemplateFieldTranslator.CStringFieldInfo> dicTempFields = TemplateFieldTranslator.GetOutputFields();

        foreach (var kv in dicTempFields)
        {
            //每个template
            string template_Name = kv.Key;
            TemplateFieldTranslator.CStringFieldInfo info = kv.Value;

            //if (template_Name != "Actor")
            //    continue;

            strText += "Console.WriteLine(\"" + template_Name + "...\");";

            strText += string.Format(@"
            GenerateTranlateValues_{0}();
", template_Name);
        }

        return strText;
    }

    static string GetFieldDesc(List<STransEntry> transEntryList, string propName)
    {
        //string desc = "\t\tstring desc = \"(\" + tid + \")\" + \"" + "v_" + transEntryList[0].name;
        string desc = "\t\tstring desc = string.Format(\"({0})v_" + transEntryList[0].name;

        int nList = 0;
        for (int i = 1; i < transEntryList.Count; ++i)
        {
            STransEntry entry = transEntryList[i];

            desc += HobaText.Format(".{0}", entry.name);

            if (entry.isList)
            {
                //desc += HobaString.Format("[\" + i{1} + \"]", nList);

                ++nList;

                desc += "[{" + nList + "}]"; ;
            }
        }


        desc = HobaText.Format("{0}.{1}", desc, propName);
        desc += "\",";  //format end

        desc += " tid";

        for (int i = 0; i < nList; ++i)
        {
            desc += HobaText.Format(", i{0}", i);
        }

        //desc += "\";";
        desc += ");";

        return desc;
    }

    private static List<STransEntry> ParseParentMemberNameList(STranslateField transfield)      //解析如 [[[ExecutionUnit][Event]][AddFightSpeciality]]
    {
        List<STransEntry> valList = new List<STransEntry>();

        int start = -1;
        int len = transfield.parentMemberName.Length;
        for (int i = 0; i < len; ++i)
        {
            char cc = transfield.parentMemberName[i];
            if (cc == '[')
                start = i + 1;
            else if (cc == ']')
            {
                if (i > start)
                {
                    string v = transfield.parentMemberName.Substring(start, i - start);
                    valList.Add(new STransEntry() { name = v, isList = false });
                }
                start = i + 1;
            }
            else if (cc == '<')
                start = i + 1;
            else if (cc == '>')
            {
                if (i > start)
                {
                    string v = transfield.parentMemberName.Substring(start, i - start);
                    valList.Add(new STransEntry() { name = v, isList = true });
                }
                start = i + 1;
            }
        }

        return valList;
    }

    static void Get_ReadTranslateValuesToDataActions_CS(StreamWriter writer, List<Template.STranslateValue> listValues)
    {
        StringBuilder stringBuilder = new StringBuilder(512);

        stringBuilder.Length = 0;
        stringBuilder.AppendFormat("//count: {0}\n", listValues.Count);
        writer.WriteLine(stringBuilder.ToString());

        //收集template和memberAccess
        HashSet<string> TMemberSet = new HashSet<string>();

        for (int i = 0; i < listValues.Count; ++i)
        {
            stringBuilder.Length = 0;

            string desc = listValues[i]._desc;

            int tid;
            string template_name;
            string member_access;
            if (!ParseTranslateValueDesc(desc, out tid, out template_name, out member_access))
            {
                System.Diagnostics.Debug.Assert(false);
                continue;
            }

            string str = template_name + member_access;
            if (!TMemberSet.Contains(str))
            {
                TMemberSet.Add(str);

                string actionName = str.Replace('.', '_');
                actionName = actionName.Replace('.', '_');
                actionName = actionName.Replace('[', '_');
                actionName = actionName.Replace(']', '_');

                stringBuilder.AppendFormat(@"        private static System.Action<int, Template.STranslateValue> _Action_{0} = (tid, transVal) => ", actionName);
                stringBuilder.Append(@"{ ");
                stringBuilder.AppendFormat(@"
            var map = Template.{0}Module.Manager.Instance.GetTemplateMap();", template_name);

                stringBuilder.Append(@"
            try {");
                stringBuilder.AppendFormat(@"
            if (map.ContainsKey(tid))   map[tid]{0} = transVal._val;", member_access);
                stringBuilder.Append(@"
            } catch(Exception) {} };");

                writer.WriteLine(stringBuilder.ToString());
            }
        }
    }

    static void Get_ReadTranslateValuesToData_CS(StreamWriter writer, List<Template.STranslateValue> listValues)
    {
        StringBuilder stringBuilder = new StringBuilder(512);

        stringBuilder.Length = 0;
        stringBuilder.AppendFormat("//count: {0}\n", listValues.Count);
        writer.WriteLine(stringBuilder.ToString());

        const int interval = 100;       //每100项一个方法

        for (int i = 0; i < listValues.Count; ++i)
        {
            stringBuilder.Length = 0;

            string desc = listValues[i]._desc;

            int tid;
            string template_name;
            string member_access;
            if (!ParseTranslateValueDesc(desc, out tid, out template_name, out member_access))
            {
                System.Diagnostics.Debug.Assert(false);
                continue;
            }

            string str = template_name + member_access;
            string actionName = str.Replace('.', '_');
            actionName = actionName.Replace('.', '_');
            actionName = actionName.Replace('[', '_');
            actionName = actionName.Replace(']', '_');
            actionName = "_Action_" + actionName;

            if (i % interval == 0)
            {
                stringBuilder.AppendFormat(@"public static void ReadTranslateValuesToData_{0}(List<STranslateValue> listValues)", i / interval);
                stringBuilder.Append(@"
    {  
");
            }

            stringBuilder.AppendFormat("            {0}.Invoke({1}, listValues[{2}]);", actionName, tid, i);


            if ((i + 1) % interval == 0 ||
                (i + 1) == listValues.Count)
            {
                stringBuilder.Append(@"
    }");
            }

            writer.WriteLine(stringBuilder.ToString());

            /*
            stringBuilder.Append(@"         { ");
            stringBuilder.AppendFormat("//{0}", i+1);

            stringBuilder.AppendFormat(@"
            var map = Template.{0}Module.Manager.Instance.GetTemplateMap();
            if (map.ContainsKey({1}))   map[{2}]{3} = listValues[{4}]._val;
            ", template_name, tid, tid, member_access, i);

            stringBuilder.Append(@"}");

            writer.WriteLine(stringBuilder.ToString());
             * */
        }
    }

    static void Get_ReadTranslateValuesToDataAll_CS(StreamWriter writer, List<Template.STranslateValue> listValues)
    {
        StringBuilder stringBuilder = new StringBuilder(512);

        const int interval = 100;       //每100项一个方法

        int count = (listValues.Count + interval - 1) / interval;

        for (int i = 0; i < count; ++i)
        {
            stringBuilder.Length = 0;

            //string desc = listValues[i]._desc;

            stringBuilder.AppendFormat(@"       ReadTranslateValuesToData_{0}(listValues);", i);

            writer.WriteLine(stringBuilder.ToString());
        }
    }


    public static bool ParseTranslateValueDesc(string desc, out int tid, out string templateName, out string memberAccess)
    {
        tid = 0;
        templateName = "";
        memberAccess = "";

        int iIdStart = -1;
        int iTemplateStart = -1;
        int iMemberStart = -1;
        for (int i = 0; i < desc.Length; ++i)
        {
            char cc = desc[i];
            if (cc == '(')
            {
                iIdStart = i + 1;
            }
            else if (cc == ')')
            {
                string strId = desc.Substring(iIdStart, i - iIdStart);
                if (!int.TryParse(strId, out tid))
                    return false;
                iTemplateStart = i + 3;
            }

            if (iTemplateStart != -1 && cc == '.')
            {
                string strTemplate = desc.Substring(iTemplateStart, i - iTemplateStart);
                templateName = strTemplate;

                iTemplateStart = -1;

                iMemberStart = i;
                memberAccess = desc.Substring(iMemberStart);
            }
        }

        if (/*tid == 0 || */string.IsNullOrEmpty(templateName) || string.IsNullOrEmpty(memberAccess))
        {
            return false;
        }
        return true;
    }

    public static bool ParseTranslateValueDesc(string desc, out int tid)
    {
        tid = 0;

        int iIdStart = -1;
        for (int i = 0; i < desc.Length; ++i)
        {
            char cc = desc[i];
            if (cc == '(')
            {
                iIdStart = i + 1;
            }
            else if (cc == ')')
            {
                string strId = desc.Substring(iIdStart, i - iIdStart);
                tid = int.Parse(strId);
            }

        }

        if (tid == 0)
        {
            return false;
        }
        return true;
    }
}

#endif