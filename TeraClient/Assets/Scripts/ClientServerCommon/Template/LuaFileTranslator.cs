using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Template;

#if UNITY_EDITOR

/*  
 * 对Configs下面的几个lua文件进行解析
    1. lua文件文本转换成List<Template.STranslateValue>
    2. List<Template.STranslateValue> 对应成csv文件供外部翻译
    3. 将翻译过的 List<Template.STranslateValue> 导入lua文件进行替换
 * */

public delegate void TranslateValuePaser(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues);

public class LuaFileTranslator
{
    public struct SEntry
    {
        public SEntry(string file, string[] fields, TranslateValuePaser parser, string dir)
        {
            luaFile = file;
            ignoredFields = fields;
            valueParser = parser;
            luaDir = dir;
        }
        public string luaFile;
        public string luaDir;
        public string[] ignoredFields;
        public TranslateValuePaser valueParser;
    }

    public static SEntry[] LuaFiles = new SEntry[]
            {
                new SEntry("game_text.lua",
                    null,
                    ParseTranslateValue_Any,
                    null),
                new SEntry("chatcfg.lua",
                    null,
                    ParseTranslateValue_channelname_mainbtnname,
                    null),
                new SEntry("CommandList.lua",
                    null,
                    ParseTranslateValue_name_desc,
                    null),
                new SEntry("debug_text.lua",
                    null,
                    ParseTranslateValue_Any,
                    null),
                new SEntry("ModuleProfDiffCfg.lua",
                    null,
                    ParseTranslateValue_Any,
                    null),
//                 new SEntry("ActivityTypeCfg.lua",
//                     null,
//                     ParseTranslateValue_Any ),
                new SEntry("SystemEntranceCfg.lua",
                    null,
                    ParseTranslateValue_Name,
                    null),
                new SEntry("MapBasicInfo.lua",
                    null,
                    ParseTranslateValue_MapBasicInfo,
                    "MapBasicInfo"),
                new SEntry("AdventureGuideBasicInfo.lua",
                    null,
                    ParseTranslateValue_Name_Remarks_DateDisplayText_PlayPos_ShowNumString_TaskRequirement_PeopleNum_Produce, 
                    ""),
//                 new SEntry("EnchantFilterInfo.lua",
//                     null,
//                     ParseTranslateValue_Any),
                new SEntry("CgConfig.xml",
                    null,
                    ParseTranslateValue_name_content,
                    null),
                new SEntry("VideoConfig.xml",
                    null, 
                    ParseTranslateValue_name_content,
                    null),
                new SEntry("AppMsgBoxCfg.lua",
                    null,
                    ParseTranslateValue_Title_Desc1_Desc2,
                    null),
                new SEntry("QuickMsgCfg.lua",
                    null,
                    ParseTranslateValue_Any,
                    null),
            };

    public static SEntry[] LuaDirs = new SEntry[]
    {
        new SEntry(null,
            null,
            ParseTranslateValue_MapBasicInfo,
            "MapBasicInfo"),
    };

    private static int IndexOfString(string strLine, int nStart)
    {
        int ret = strLine.IndexOf('\"', nStart);
        if (ret < 0)
            ret = strLine.IndexOf('\'', nStart);
        return ret;
    }

    private static void ParseTranslateValue_Any(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();

        string shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);

        int nStart = 0;
        int nEnd = 0;

        do
        {
            nStart = IndexOfString(strLine, nStart);
            if (nStart < 0 || nStart + 1 == strLine.Length)
                break;

            nEnd = IndexOfString(strLine, nStart + 1);
            if (nEnd < 0)
                break;

            {
                if (IsIgnored(strLine, ignoreFields))
                {
                    nStart = nEnd + 1;
                    continue;
                }

                ++nStart;
                string val = strLine.Substring(nStart, nEnd - nStart);
                string desc = HobaText.Format("{0}^{1}", shortFileName, val);

                if (!string.IsNullOrEmpty(val))
                    translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
            }

            nStart = nEnd + 1;

        } while (nStart < strLine.Length);

    }

    private static void ParseTranslateValue_channelname_mainbtnname(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;
        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "channelname", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "mainbtnname", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
    }

    private static void ParseTranslateValue_name_desc(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;
        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "name", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "desc", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
    }

    private static void ParseTranslateValue_Name(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;
        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Name", out desc, out val);

        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
    }

    private static void ParseTranslateValue_Name_Remarks_DateDisplayText_PlayPos_ShowNumString_TaskRequirement_PeopleNum_Produce(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;
        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Name", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Remarks", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "DateDisplayText", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "PlayPos", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "ShowNumString", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "TaskRequirement", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "PeopleNum", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Produce", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
    }

    private static void ParseTranslateValue_name(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;
        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "name", out desc, out val);

        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
    }

    private static void ParseTranslateValue_MapBasicInfo(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;

        bool skipName = strLine.Contains("name =") && strLine.Contains("worldId =") && !strLine.Contains("isShowName = true");

        if (!skipName)
        {
            ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "name", out desc, out val);
            if (!string.IsNullOrEmpty(val))
                translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
        }

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "TextDisplayName", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Describe", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "FunctionName", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
        
    }

    private static void ParseTranslateValue_name_content(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "name", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "content", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
    }

    private static void ParseTranslateValue_Title_Desc1_Desc2(string fileName, string[] ignoreFields, string strLine, List<STranslateValue> translateValues)
    {
        translateValues.Clear();
        string desc, val;

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Title", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Desc1", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });

        ParseTranslateValue_Keyword(fileName, ignoreFields, strLine, "Desc2", out desc, out val);
        if (!string.IsNullOrEmpty(val))
            translateValues.Add(new STranslateValue() { _desc = desc, _val = val });
    }

    //查找 keword 后的 第一个 "XXX" 匹配
    private static void ParseTranslateValue_Keyword(string fileName, string[] ignoreFields, string strLine, string keyword, out string desc, out string val)
    {
        desc = "";
        val = "";

        int nNameStr = strLine.IndexOf(keyword + " =");
        if (nNameStr < 0)
            return;

        //Name = 前面不能是其他单词的一部分
        if (nNameStr > 0)
        {
            if (Char.IsLetter(strLine, nNameStr - 1) || Char.IsDigit(strLine, nNameStr - 1))
                return;
        }

        string shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        int nStart = IndexOfString(strLine, nNameStr);
        if (nStart < 0)
            return;

        int nEnd = IndexOfString(strLine, nStart + 1);
        if (nStart >= 0 && nEnd >= 0 && nEnd > nStart)
        {
            if (IsIgnored(strLine, ignoreFields))
                return;

            ++nStart;
            val = strLine.Substring(nStart, nEnd - nStart);
            desc = HobaText.Format("{0}^({1})^{2}", shortFileName, keyword, val);
        }
    }

    public static bool ParseLuaFile(string fileName, string[] ignoreFields, out List<Template.STranslateValue> translateValues, TranslateValuePaser parser)
    {
        translateValues = new List<Template.STranslateValue>();
        string[] lines = null;

        try
        {
            lines = File.ReadAllLines(fileName);
        }
        catch (Exception)
        {
            return false;
        }

        //string fname = System.IO.Path.GetFileName(fileName);
        //string shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        int nLine = 0;
        bool bComment = false;
        foreach (string line in lines)
        {
            ++nLine;

            if (line.IndexOf("--[[") >= 0)
                bComment = true;

            if (line.IndexOf("]]") >= 0)
                bComment = false;

            if (bComment)
                continue;

            string strLine = line;

            int idx = line.IndexOf("--");
            if (idx > 0)
                strLine = line.Substring(0, idx);

            //解析要翻译的
            List<STranslateValue> transList = new List<STranslateValue>();
            parser(fileName, ignoreFields, strLine, transList);

            foreach (var trans in transList)
            {
                if (!string.IsNullOrEmpty(trans._val))
                {
                    if (!translateValues.Contains(trans))
                        translateValues.Add(trans);
                }
            }
        }

        return true;
    }

    public static bool IsIgnored(string strLine, string[] ignoreFields)
    {
        if (ignoreFields == null)
            return false;

        int nStart = strLine.IndexOf('"');
        if (nStart > 0)
        {
            foreach (var field in ignoreFields)
            {
                int nIndex = strLine.IndexOf(field);
                if (nIndex > 0 && nIndex < nStart)
                    return true;
            }
        }
        return false;
    }

    public static bool TranslateValuesToLuaFile(List<Template.STranslateValue> translateValues, string[] ignoreFields, string fileName, string outFileName, out string logMessage)
    {
        logMessage = "";
        StringBuilder sb = new StringBuilder();

        string[] lines = null;
        try
        {
            lines = File.ReadAllLines(fileName);
        }
        catch (Exception)
        {
            return false;
        }

        string key = System.IO.Path.GetFileNameWithoutExtension(fileName) + "^";
        foreach (var transVal in translateValues)
        {
            if (transVal._desc.IndexOf(key) < 0)            //跳过不需要的行
                continue;

            int bStart = transVal._desc.IndexOf(key) + key.Length;

            string valOrig = transVal._desc.Substring(bStart);

            if (string.IsNullOrEmpty(valOrig))
                continue;

            string valReplace = transVal._val;

            int nLine = 0;
            bool bComment = false;
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];

                ++nLine;

                if (line.IndexOf("--[[") >= 0)
                    bComment = true;

                if (line.IndexOf("]]") >= 0)
                    bComment = false;

                if (bComment)
                    continue;

                string strLine = line;

                int idx = line.IndexOf("--");
                if (idx > 0)
                    strLine = line.Substring(0, idx);

                if (!strLine.Contains("\"" + valOrig + "\"") && !strLine.Contains("\'" + valOrig + "\'"))
                    continue;

                if (string.IsNullOrEmpty(valReplace))
                {
                    //sb.AppendFormat("警告! {0}行 要替换的值为Empty! 使用原文: {1}\n", nLine, valOrig);
                }
                else
                {
                    if (strLine.Contains("\"" + valOrig + "\""))
                    {
                        lines[i] = line.Replace("\"" + valOrig + "\"", "\"" + valReplace + "\"");       //替换
                        sb.AppendFormat("{0}行替换: \"{1}\" => \"{2}\"\n", nLine, valOrig, valReplace);
                    }

                    if (strLine.Contains("\'" + valOrig + "\'"))
                    {
                        lines[i] = line.Replace("\'" + valOrig + "\'", "\'" + valReplace + "\'");       //替换
                        sb.AppendFormat("{0}行替换: \"{1}\" => \"{2}\"\n", nLine, valOrig, valReplace);
                    }
                }
            }
        }

        logMessage = sb.ToString();

        try
        {
            File.WriteAllLines(outFileName, lines);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}

#endif