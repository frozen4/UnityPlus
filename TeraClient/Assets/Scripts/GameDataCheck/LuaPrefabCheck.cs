using System;
using Common;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class LuaPrefabCheck : Singleton<LuaPrefabCheck>
{
    public string _LuaBasePath = "";
    public string _ErrorString = "";

    private const string _prefabEndString = ".prefab";
    private const string _interfaceDir = "Assets/Outputs/Interfaces/";

    private List<string> _IgnoreLuaFiles = new List<string>()
        {
            "Lplus.lua",
            "Enum.lua",
            "PBHelper.lua",
            "Test.lua",
        };

    //resPath中的UI prefab
    public Dictionary<string, string> _ResPathUIPrefabMap = new Dictionary<string, string>();

    //resPath中的No UI prefab
    public HashSet<string> _ResPathNoUIPrefabSet = new HashSet<string>();

    //其他文件中引用的 prefab
    public HashSet<string> _LuaPrefabSet = new HashSet<string>();

    //ObjectCfg
    public Dictionary<string, Dictionary<string, int>> _ObjectCfgMap = new Dictionary<string, Dictionary<string, int>>();

    public void Init()
    {
        _LuaBasePath = Path.Combine(Application.dataPath, "../../GameRes/Lua/");
    }

    public void LoadAsset(string prefab)
    {
        Object asset = AssetBundleCheck.Instance.LoadAsset(prefab);
        if (asset == null)
            _ErrorString += string.Format("asset 加载失败: {0}\n", prefab);

        //创建
        if (asset != null)
        {
            var obj = Object.Instantiate(asset);
            GameDataCheck.Instance.DestroyObjectX(obj);
        }

        asset = null;
    }

    #region 收集Prefab
    public void CollectPrefabs()
    {
        _ResPathUIPrefabMap.Clear();
        _ResPathNoUIPrefabSet.Clear();
        _LuaPrefabSet.Clear();

        List<string> luaFileList = new List<string>();
        Common.Utilities.ListFiles(new DirectoryInfo(_LuaBasePath), ".lua", luaFileList);

        foreach (string filename in luaFileList)
        {
            string shortFileName = Path.GetFileName(filename);
            if (_IgnoreLuaFiles.Contains(shortFileName))
                continue;

            if (Path.GetFileName(filename) == "ResPath.lua")
            {
                ParseResPath(filename);
            }
            else
            {
                ParseOtherLuaFile(filename);
            }
        }
    }

    int GetNumberOfChars(string str, char cc)
    {
        int count = 0;
        for (int i = 0; i < str.Length; ++i)
        {
            if (str[i] == cc)
                ++count;
        }
        return count;
    }

    int GetStartQuoteIndex(string str, int end)
    {
        if (end >= str.Length || end <= 0)
            return -1;
        for (int i = end - 1; i >= 0; --i)
        {
            if (str[i] == '\'' || str[i] == '\"')
                return i;
        }
        return -1;
    }

    private void ParseResPath(string filename)
    {
        string[] lines = File.ReadAllLines(filename);

        bool bComment = false;
        foreach (var line in lines)
        {
            if (line.Contains("--[["))
                bComment = true;

            if (line.Contains("]]"))
                bComment = false;

            if (bComment)
                continue;

            string strLine = line;

            {
                int idx = strLine.IndexOf("--");            //comment
                if (idx >= 0)
                    strLine = strLine.Substring(0, idx);
            }

            if (strLine.IndexOf(_prefabEndString) < 0)          //包含.prefab
                continue;

            if (GetNumberOfChars(strLine, '=') == 1 &&
                (GetNumberOfChars(strLine, '\"') == 2 || GetNumberOfChars(strLine, '\'') == 2))
            {
                int idx0 = strLine.IndexOf("Panel_");
                int idx1 = strLine.IndexOf("UI_");
                if (idx0 >= 0 || idx1 >= 0)
                {
                    //处理 key = value
                    int index = strLine.IndexOf("=");
                    int end = strLine.IndexOf(_prefabEndString) + _prefabEndString.Length;
                    int start = strLine.IndexOf("\"");
                    if (start < 0)
                        start = strLine.IndexOf("'");

                    string strKey = strLine.Substring(0, index);
                    string strValue = strLine.Substring(start + 1, end - start - 1);

                    strKey = strKey.Trim(new char[] { ' ', '\t' });
                    strValue = strValue.Trim(new char[] { ' ', '\t' });

                    if (!_ResPathUIPrefabMap.ContainsKey(strKey))
                    {
                        _ResPathUIPrefabMap.Add(strKey, strValue);
                    }
                }
            }
            else
            {
                int end = strLine.IndexOf(_prefabEndString) + _prefabEndString.Length;
                int start = GetStartQuoteIndex(strLine, end);

                if (start >= 0)
                {
                    string strValue = strLine.Substring(start + 1, end - start - 1);
                    _ResPathNoUIPrefabSet.Add(strValue);
                }
            }
        }
    }

    private void ParseOtherLuaFile(string filename)
    {
        string[] lines = File.ReadAllLines(filename);

        bool bComment = false;
        foreach (var line in lines)
        {
            if (line.Contains("--[["))
                bComment = true;

            if (line.Contains("]]"))
                bComment = false;

            if (bComment)
                continue;

            string strLine = line;

            {
                int idx = strLine.IndexOf("--");            //comment
                if (idx >= 0)
                    strLine = strLine.Substring(0, idx);
            }

            if (strLine.IndexOf(_prefabEndString) < 0)          //包含.prefab
                continue;

            {
                int end = strLine.IndexOf(_prefabEndString) + _prefabEndString.Length;
                int start = GetStartQuoteIndex(strLine, end);

                if (start >= 0)
                {
                    string strValue = strLine.Substring(start + 1, end - start - 1);

                    _LuaPrefabSet.Add(strValue);
                }
            }
        }
    }

    #endregion

    #region 收集ObjectCfg
    public void CollectObjectCfgs()
    {
        _ObjectCfgMap.Clear();

        string dir = Path.Combine(_LuaBasePath, "GUI/ObjectCfg/");
        List<string> luaFileList = new List<string>();
        Common.Utilities.ListFiles(new DirectoryInfo(dir), ".lua", luaFileList);

        foreach (string filename in luaFileList)
        {
            ParseObjectCfg(filename);
        }
    }

    private void ParseObjectCfg(string filename)
    {
        Dictionary<string, int> cfgMap = new Dictionary<string, int>();

        string[] lines = File.ReadAllLines(filename);

        bool bComment = false;
        foreach (var line in lines)
        {
            if (line.Contains("--[["))
                bComment = true;

            if (line.Contains("]]"))
                bComment = false;

            if (bComment)
                continue;

            string strLine = line;

            {
                int idx = strLine.IndexOf("--"); //comment
                if (idx >= 0)
                    strLine = strLine.Substring(0, idx);
            }

            if (GetNumberOfChars(strLine, '=') == 1 && GetNumberOfChars(strLine, ',') == 1 &&
                (GetNumberOfChars(strLine, '\"') == 0 || GetNumberOfChars(strLine, '\'') == 0))
            {
                //处理 key = value
                int index = strLine.IndexOf("=");
                int end = strLine.LastIndexOf(',');

                string strKey = strLine.Substring(0, index);
                string strValue = strLine.Substring(index + 1, end - index - 1);

                strKey = strKey.Trim(new char[] { ' ', '\t' });
                strValue = strValue.Trim(new char[] { ' ', '\t' });
                int iValue = Int32.Parse(strValue);

                int outValue;
                bool bContain = cfgMap.TryGetValue(strKey, out outValue);
                if (!bContain)
                {
                    cfgMap.Add(strKey, iValue);
                }
                else
                {
                    if (outValue != iValue)
                        Debug.LogError("ObjectCfg Error!");
                }
            }
        }

        string shortname = Path.GetFileNameWithoutExtension(filename);
        _ObjectCfgMap.Add(shortname, cfgMap);
    }

    #endregion

    #region 构造luaClass信息

    public class CLuaClass
    {
        public CLuaClass parent;

        public string strName = string.Empty;

        public string prefabPath = string.Empty;

        public string fileName = string.Empty;

        public bool isReferenced = false;

        public HashSet<string> uiOjbectSet = new HashSet<string>();

        public HashSet<string> uiParentOjbectSet = new HashSet<string>();
    }

    public Dictionary<string, CLuaClass> _MapLuaClass = new Dictionary<string, CLuaClass>();

    private string[] _ParentPairClass =
        {
            "CPageGilliam", "CPanelUIDungeon",
            "CPageMiniMap", "CPanelMinimap",
            "CPageNormalDungeon", "CPanelUIDungeon",
            "CPageRide", "CPanelUIExterior",
            "CPageTowerDungeon", "CPanelUIDungeon",
        };

    public bool IsParentContains(CLuaClass luaClass, string strName)
    {
        CLuaClass parentClass = luaClass.parent;
        while (parentClass != null)
        {
            if (parentClass.strName == strName)
            {
                return true;
            }
            parentClass = parentClass.parent;
        }

        return false;
    }

    public string CheckUIClass(string className, CLuaClass luaClass)
    {
        string prefab = string.Empty;

        if (!IsParentContains(luaClass, "CPanelBase"))
            return prefab;

        string key = luaClass.prefabPath;
        if (string.IsNullOrEmpty(key))
        {
            //_ErrorString += string.Format("UI没有配置prefab: {0}\n", className);
            return prefab;
        }

        bool bError = false;
        if (!_ResPathUIPrefabMap.TryGetValue(key, out prefab))
        {
            if (!bError)
            {
                _ErrorString += string.Format("class: {0}\n", className);
                bError = true;
            }
            _ErrorString += string.Format("\tUI缺失prefab! class: {0}, PATH: {1}\n", className, key);
            return prefab;
        }

        prefab = _interfaceDir + prefab;
        Object asset = AssetBundleCheck.Instance.LoadAsset(prefab);
        if (asset == null)
        {
            if (!bError)
            {
                _ErrorString += string.Format("class: {0}\n", className);
                bError = true;
            }
            _ErrorString += string.Format("\tprefab asset 加载失败! class: {0}, PATH: {1}, prefab: {2}\n", className, key, prefab);
            return prefab;
        }

        GameObject uiGameObject = GameObject.Instantiate(asset) as GameObject;
        if (uiGameObject.GetComponent<RectTransform>() == null)
        {
            if (!bError)
            {
                _ErrorString += string.Format("class: {0}\n", className);
                bError = true;
            }
            _ErrorString += string.Format("\tprefab asset 没有RectTransform! class: {0}, PATH: {1}, prefab: {2}\n", className, key, prefab);
        }

        if (luaClass.uiOjbectSet.Count > 0)         //检查UIObject
        {
            string objCfgKey = System.IO.Path.GetFileNameWithoutExtension(prefab);
            Dictionary<string, int> objCfgMap;
            if (!_ObjectCfgMap.TryGetValue(objCfgKey, out objCfgMap))
            {
                if (!bError)
                {
                    _ErrorString += string.Format("class: {0}\n", className);
                    bError = true;
                }
                _ErrorString += string.Format("\t找不到对应的ObjectCfg文件! class: {0}, cfgPath: {1}\n", className, objCfgKey);
                return prefab;
            }

            var holder = uiGameObject.GetComponent<GUILinkHolder>();
            if (holder == null)
            {
                if (!bError)
                {
                    _ErrorString += string.Format("class: {0}\n", className);
                    bError = true;
                }
                _ErrorString += string.Format("\tprefab没有GUILinkHolder! class: {0}, prefab: {1}\n", className, prefab);

                GameDataCheck.Instance.DestroyObjectX(uiGameObject);

                return prefab;
            }

            foreach (string uiObj in luaClass.uiOjbectSet)
            {
                int id;
                if (!objCfgMap.TryGetValue(uiObj, out id))
                {
                    if (!bError)
                    {
                        _ErrorString += string.Format("class: {0}\n", className);
                        bError = true;
                    }
                    _ErrorString += string.Format("\t找不到ObjectCfg项! class: {0}, uiObj: {1}\n", className, uiObj);
                    continue;
                }

                if (null == holder.GetUIObject(id))
                {
                    if (!bError)
                    {
                        _ErrorString += string.Format("class: {0}\n", className);
                        bError = true;
                    }

                    _ErrorString += string.Format("\tLinkHolder GetUIObject失败! class: {0}, uiObj: {1}, id: {2}\n", className, uiObj, id);
                    continue;
                }
            }
        }
        GameDataCheck.Instance.DestroyObjectX(uiGameObject);

        return prefab;
    }

    public string CheckUIParentClass(string className, CLuaClass luaClass)
    {
        string prefab = string.Empty;

        if (luaClass.uiParentOjbectSet.Count == 0)
            return prefab;

        string parentClass = "";
        for (int i = 0; i < _ParentPairClass.Length; ++i)
        {
            if (i % 2 == 0)
            {
                if (_ParentPairClass[i] == className)
                {
                    parentClass = _ParentPairClass[i + 1];
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(parentClass))
            return prefab;

        CLuaClass parentLuaClass = GetLuaClass(parentClass);
        //             if (parentLuaClass == null)
        //                 return;

        {
            string parentClassName = parentLuaClass.strName;

            string key = parentLuaClass.prefabPath;
            if (string.IsNullOrEmpty(key))
            {
                //_ErrorString += string.Format("UI没有配置prefab: {0}\n", className);
                return prefab;
            }

            bool bError = false;  
            if (!_ResPathUIPrefabMap.TryGetValue(key, out prefab))
            {
                if (!bError)
                {
                    _ErrorString += string.Format("parent class: {0}\n", parentClassName);
                    bError = true;
                }
                _ErrorString += string.Format("\tUI缺失prefab! class: {0}, PATH: {1}\n", parentClassName, key);
                return prefab;
            }

            prefab = _interfaceDir + prefab;
            Object asset = AssetBundleCheck.Instance.LoadAsset(prefab);
            if (asset == null)
            {
                if (!bError)
                {
                    _ErrorString += string.Format("class: {0} parent class: {1}\n", className, parentClassName);
                    bError = true;
                }
                _ErrorString += string.Format("\tprefab asset 加载失败! class: {0}, PATH: {1}, prefab: {2}\n", parentClassName, key, prefab);
                return prefab;
            }

            string objCfgKey = System.IO.Path.GetFileNameWithoutExtension(prefab);
            Dictionary<string, int> objCfgMap;
            if (!_ObjectCfgMap.TryGetValue(objCfgKey, out objCfgMap))
            {
                if (!bError)
                {
                    _ErrorString += string.Format("class: {0} parent class: {1}\n", className, parentClassName);
                    bError = true;
                }
                _ErrorString += string.Format("\t找不到对应的ObjectCfg文件! class: {0}, parentClass: {1}, cfgPath: {2}\n", className, parentClassName, objCfgKey);
                return prefab;
            }

            GameObject uiGameObject = GameObject.Instantiate(asset) as GameObject;
            var holder = uiGameObject.GetComponent<GUILinkHolder>();
            if (holder == null)
            {
                if (!bError)
                {
                    _ErrorString += string.Format("class: {0} parent class: {1}\n", className, parentClassName);
                    bError = true;
                }
                _ErrorString += string.Format("\tprefab没有GUILinkHolder! class: {0}, parentClass: {1}, prefab: {2}\n", className, parentClassName, prefab);

                GameDataCheck.Instance.DestroyObjectX(uiGameObject);
                return prefab;
            }

            foreach (string uiObj in luaClass.uiParentOjbectSet)
            {
                int id;
                if (!objCfgMap.TryGetValue(uiObj, out id))
                {
                    if (!bError)
                    {
                        _ErrorString += string.Format("class: {0} parent class: {1}\n", className, parentClassName);
                        bError = true;
                    }
                    _ErrorString += string.Format("\t找不到ObjectCfg项! class: {0}, parentClass: {1}, uiObj: {2}\n", className, parentClassName, uiObj);
                    continue;
                }

                if (null == holder.GetUIObject(id))
                {
                    if (!bError)
                    {
                        _ErrorString += string.Format("class: {0} parent class: {1}\n", className, parentClassName);
                        bError = true;
                    }

                    _ErrorString += string.Format("\tLinkHolder GetUIObject失败! class: {0}, parentClass: {1}, uiObj: {2}, id: {3}\n", className, parentClassName, uiObj, id);
                    continue;
                }
            }

            GameDataCheck.Instance.DestroyObjectX(uiGameObject);
        }
        return prefab;
    }

    CLuaClass GetLuaClass(string name)
    {
        CLuaClass luaClass;
        if (_MapLuaClass.TryGetValue(name, out luaClass))
            return luaClass;
        return null;
    }

    CLuaClass AddLuaClass(string name)
    {
        _MapLuaClass.Add(name, new CLuaClass());
        return GetLuaClass(name);
    }

    public void CollectLuaClasses()
    {
        _MapLuaClass.Clear();

        string dir = Path.Combine(_LuaBasePath, "GUI/");
        List<string> luaFileList = new List<string>();
        Common.Utilities.ListFiles(new DirectoryInfo(dir), ".lua", luaFileList);

        foreach (string filename in luaFileList)
        {
            string shortFileName = Path.GetFileName(filename);
            if (_IgnoreLuaFiles.Contains(shortFileName))
                continue;

            if (shortFileName.Contains("/ObjectCfg/"))
                continue;

            BuildLuaClass(filename, _MapLuaClass);
        }
    }

    public void CheckReferenced(string filename, Dictionary<string, CLuaClass> luaClass)
    {
        string shortFileName = Path.GetFileName(filename);
        if (_IgnoreLuaFiles.Contains(shortFileName))
            return;

        if (shortFileName.Contains("/ObjectCfg/"))
            return;

        {
            string[] lines = File.ReadAllLines(filename);

            bool bComment = false;
            foreach (var line in lines)
            {
                if (line.Contains("--[["))
                    bComment = true;

                if (line.Contains("]]"))
                    bComment = false;

                if (bComment)
                    continue;

                string strLine = line;

                {
                    int idx = strLine.IndexOf("--"); //comment
                    if (idx >= 0)
                        strLine = strLine.Substring(0, idx);
                }

                if (strLine.IndexOf("Lplus.Class") >= 0 || strLine.IndexOf("Lplus.Extend") >= 0)
                    continue;

                foreach (var v in luaClass.Values)
                {
                    if (string.IsNullOrEmpty(v.prefabPath))
                        continue;

                    if (string.IsNullOrEmpty(v.fileName))
                    {
                        v.isReferenced = true;
                    }
                    else
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(v.fileName);
                        string str0 = "\'" + fileName + "\'";
                        string str1 = "\"" + fileName + "\"";
                        if (strLine.IndexOf(str0) >= 0 || strLine.IndexOf(str1) >= 0)
                            v.isReferenced = true;
                    }
                }
            }
        }
    }

    void BuildLuaClass(string filename, Dictionary<string, CLuaClass> luaClass)
    {
        BuildLuaClass0(filename, luaClass);
        BuildLuaClass1(filename, luaClass);
    }

    void BuildLuaClass0(string filename, Dictionary<string, CLuaClass> luaClass)
    {
        string[] lines = File.ReadAllLines(filename);

        CLuaClass current = null;
        bool bComment = false;
        foreach (var line in lines)
        {
            if (line.Contains("--[["))
                bComment = true;

            if (line.Contains("]]"))
                bComment = false;

            if (bComment)
                continue;

            string strLine = line;

            {
                int idx = strLine.IndexOf("--"); //comment
                if (idx >= 0)
                    strLine = strLine.Substring(0, idx);
            }

            if (strLine.IndexOf("Lplus.Class(") >= 0)
            {
                HandleLine_ClassDefine(filename, strLine, ref current);
            }
            else if (strLine.IndexOf("self:GetUIObject(") >= 0)
            {
                HandleLine_SelfUIObject(strLine, current);
            }
            else if (strLine.IndexOf("self._Parent:GetUIObject(") >= 0)
            {
                HandleLine_ParentUIObject(strLine, current);
            }
            else if (GetNumberOfChars(strLine, '=') == 1 &&
                strLine.IndexOf("._PrefabPath") >= 0 &&
                strLine.IndexOf("PATH.") >= 0)
            {
                HandleLine_PrefabPath(strLine, current);
            }
        }
    }

    void BuildLuaClass1(string filename, Dictionary<string, CLuaClass> luaClass)
    {
        string[] lines = File.ReadAllLines(filename);

        CLuaClass current = null;
        bool bComment = false;
        foreach (var line in lines)
        {
            if (line.Contains("--[["))
                bComment = true;

            if (line.Contains("]]"))
                bComment = false;

            if (bComment)
                continue;

            string strLine = line;

            {
                int idx = strLine.IndexOf("--"); //comment
                if (idx >= 0)
                    strLine = strLine.Substring(0, idx);
            }

            if (strLine.IndexOf("Lplus.Extend(") >= 0)
            {
                HandleLine_ClassExtend(filename, strLine, ref current);
            }
            else if (strLine.IndexOf("self:GetUIObject(") >= 0)
            {
                HandleLine_SelfUIObject(strLine, current);
            }
            else if (strLine.IndexOf("self._Parent:GetUIObject(") >= 0)
            {
                HandleLine_ParentUIObject(strLine, current);
            }
            else if (GetNumberOfChars(strLine, '=') == 1 &&
                strLine.IndexOf("._PrefabPath") >= 0 &&
                strLine.IndexOf("PATH.") >= 0)
            {
                HandleLine_PrefabPath(strLine, current);
            }
        }
    }

    void HandleLine_ClassDefine(string fileName, string strLine, ref CLuaClass current)
    {
        string str1 = "Lplus.Class(\"";
        string str2 = "Lplus.Class(\'";

        int idx = strLine.IndexOf(str1);
        if (idx >= 0)
        {
            int start = idx + str1.Length;
            int end = strLine.IndexOf("\")", start);

            if (end >= 0)
            {
                string name = strLine.Substring(start, end - start);
                name = name.Trim(new char[] { ' ', '\t' });

                CLuaClass luaClass = GetLuaClass(name);
                if (luaClass == null)
                {
                    luaClass = AddLuaClass(name);
                    luaClass.strName = name;
                    luaClass.fileName = fileName;
                    luaClass.parent = null;
                }
                else if (string.IsNullOrEmpty(luaClass.strName))
                {
                    luaClass.strName = name;
                    luaClass.fileName = fileName;
                }

                current = luaClass;
            }
        }
        else
        {
            idx = strLine.IndexOf(str2);
            if (idx >= 0)
            {
                int start = idx + str2.Length;
                int end = strLine.IndexOf("')", start);

                if (end >= 0)
                {
                    string name = strLine.Substring(start, end - start);
                    name = name.Trim(new char[] { ' ', '\t' });

                    CLuaClass luaClass = GetLuaClass(name);
                    if (luaClass == null)
                    {
                        luaClass = AddLuaClass(name);
                        luaClass.fileName = fileName;
                        luaClass.strName = name;
                    }
                    else if (string.IsNullOrEmpty(luaClass.strName))
                    {
                        luaClass.strName = name;
                        luaClass.fileName = fileName;
                    }

                    current = luaClass;
                }
            }
        }
    }

    void HandleLine_ClassExtend(string fileName, string strLine, ref CLuaClass current)
    {
        string str1 = "Lplus.Extend(";
        int idx = strLine.IndexOf(str1);
        if (idx < 0)
            return;
        idx += str1.Length;
        int start = idx;
        int end = strLine.IndexOf(',', start);
        if (end < 0)
            return;

        if (strLine.IndexOf('\"', end) >= 0 && strLine.IndexOf("\")", end) >= 0)
        {
            //base
            string basename = strLine.Substring(start, end - start);
            basename = basename.Trim(new char[] { ' ', '\t' });

            CLuaClass baseClass = GetLuaClass(basename);
            if (baseClass == null)
            {
                baseClass = AddLuaClass(basename);
                baseClass.strName = basename;
                baseClass.parent = null;
            }

            //extend
            start = strLine.IndexOf("\"", end) + 1;
            end = strLine.IndexOf("\")", start);

            string extname = strLine.Substring(start, end - start);
            extname = extname.Trim(new char[] { ' ', '\t' });

            CLuaClass luaClass = GetLuaClass(extname);
            if (luaClass == null)
            {
                luaClass = AddLuaClass(extname);
                luaClass.fileName = fileName;
                luaClass.strName = extname;
            }
            else if (string.IsNullOrEmpty(luaClass.strName))
            {
                luaClass.fileName = fileName;
                luaClass.strName = extname;
            }
            luaClass.parent = baseClass;

            current = luaClass;
        }
        else if (strLine.IndexOf('\'', end) >= 0 && strLine.IndexOf("\')", end) >= 0)
        {
            //base
            string basename = strLine.Substring(start, end - start);
            basename = basename.Trim(new char[] { ' ', '\t' });

            CLuaClass baseClass = GetLuaClass(basename);
            if (baseClass == null)
            {
                baseClass = AddLuaClass(basename);
                baseClass.strName = basename;
                baseClass.parent = null;
            }

            //extend
            start = strLine.IndexOf("\'", end) + 1;
            end = strLine.IndexOf("\')", start);

            string extname = strLine.Substring(start, end - start);
            extname = extname.Trim(new char[] { ' ', '\t' });

            CLuaClass luaClass = GetLuaClass(extname);
            if (luaClass == null)
            {
                luaClass = AddLuaClass(extname);
                luaClass.fileName = fileName;
                luaClass.strName = extname;
            }
            else if (string.IsNullOrEmpty(luaClass.strName))
            {
                luaClass.fileName = fileName;
                luaClass.strName = extname;
            }
            luaClass.parent = baseClass;

            current = luaClass;
        }
    }

    void HandleLine_PrefabPath(string strLine, CLuaClass current)
    {
        if (current == null)
            return;

        if (GetNumberOfChars(strLine, '=') != 1)
            return;

        if (strLine.IndexOf("._PrefabPath") < 0)
            return;

        if (strLine.IndexOf("PATH.") < 0)
            return;

        int index = strLine.IndexOf("=");
        int end = strLine.Length;

        string strValue = strLine.Substring(index + 1, end - index - 1);
        strValue = strValue.Trim(new char[] { ' ', '\t' });

        current.prefabPath = strValue.Replace("PATH.", "");
    }

    void HandleLine_SelfUIObject(string strLine, CLuaClass current)
    {
        if (current == null)
            return;

        int start, end;

        string str1 = "self:GetUIObject(";

        int idx = strLine.IndexOf(str1);
        if (idx < 0)
            return;
        idx += str1.Length;
        if (strLine[idx] == '\"' || strLine[idx] == '\'')
            start = idx + 1;
        else
            return;

        idx = strLine.IndexOf(")", idx);
        if (idx < 0)
            return;

        if (strLine[idx - 1] == '\"' || strLine[idx - 1] == '\'')
            end = idx - 1;
        else
            return;

        if (end > start)
        {
            string strObj = strLine.Substring(start, end - start);
            strObj = strObj.Trim(new char[] { ' ', '\t' });

            if (!strObj.Contains(".."))
                current.uiOjbectSet.Add(strObj);
        }
    }

    void HandleLine_ParentUIObject(string strLine, CLuaClass current)
    {
        if (current == null)
            return;

        int start, end;

        string str1 = "self._Parent:GetUIObject(";

        int idx = strLine.IndexOf(str1);
        if (idx < 0)
            return;
        idx += str1.Length;
        if (strLine[idx] == '\"' || strLine[idx] == '\'')
            start = idx + 1;
        else
            return;

        idx = strLine.IndexOf(")", idx);
        if (idx < 0)
            return;

        if (strLine[idx - 1] == '\"' || strLine[idx - 1] == '\'')
            end = idx - 1;
        else
            return;

        if (end > start)
        {
            string strObj = strLine.Substring(start, end - start);
            strObj = strObj.Trim(new char[] { ' ', '\t' });

            if (!strObj.Contains(".."))
                current.uiParentOjbectSet.Add(strObj);
        }
    }
    #endregion
}
