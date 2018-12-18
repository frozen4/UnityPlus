using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationTools : EditorWindow
{
    public class PrefabTextInfo
    {
        public int _HashCode;
        public string _Info;
    }

    private static void GetNodePath(Transform trans, ref string path)
    {
        string name = trans.name;
        if (path == "")
        {
            path = name;
        }
        else
        {
            path = name + "/" + path;
        }

        if (trans.parent != null)
        {
            GetNodePath(trans.parent, ref path);
        }
    }

   [MenuItem("Localization Tools/检查UIText层级命名")]
    private static void CheckUITextName()
    {
        Dictionary<string, int> textFullPaths = new Dictionary<string, int>();

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs/UI" });

        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                List<PrefabTextInfo> textList = new List<PrefabTextInfo>();
                textList.Clear();
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Text[] allTextComp = go.GetComponentsInChildren<Text>();
                foreach (var item in allTextComp)
                {
                    string path = "";
                    GetNodePath(item.transform, ref path);
                    if (textFullPaths.ContainsKey(path))
                    {
                        textFullPaths[path] += 1;
                    }
                    else
                    {
                        textFullPaths.Add(path, 1);
                    }
                }
            }
        }

        if (textFullPaths.Count > 0)
        {
            string bundle_md5_info = Application.dataPath + "--UIFullPath.txt";
            using (FileStream fs = new FileStream(bundle_md5_info, FileMode.Create))
            {
                if (!fs.CanWrite)
                {
                    Debug.LogError(string.Format("The {0} Is Locked", bundle_md5_info));
                }
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                foreach (KeyValuePair<string, int> kv in textFullPaths)
                {
                    sw.WriteLine(string.Format("{0},{1}", kv.Key, kv.Value));
                }
                sw.Dispose();
                sw.Close();
                fs.Dispose();
                fs.Close();
            }
            EditorUtility.DisplayDialog("", "导出完成", "OK");
        }
    }

#region Hide

   //[MenuItem("Localization Tools/一键导出所有文本(路径匹配)")]
    private static void ExportAllTextByPath()
    {
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs/UI" });
        int i = 0;
        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                List<PrefabTextInfo> textList = new List<PrefabTextInfo>();
                textList.Clear();
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Text[] allTextComp = go.GetComponentsInChildren<Text>();

                foreach (var item in allTextComp)
                {
                    //PrefabTextInfo textInfo = new PrefabTextInfo();
                    //textInfo._HashCode = item.GetHashCode();

                    //textInfo._Info = item.text.Replace("\n", "\\n");
                    string textPath = "";
                    GetNodePath(item.transform, ref textPath);
                    string info = item.text.Replace("\n", "\\n");
                    if (!LocalizationDic.ContainsKey(textPath))
                    {
                        LocalizationDic.Add(textPath, info + "export");
                    }
                }
            }
        }

        if (LocalizationDic.Count > 0)
        {
            string bundle_md5_info = Application.dataPath + "--LocalizationTextByPath.csv";
            using (FileStream fs = new FileStream(bundle_md5_info, FileMode.Create))
            {
                if (!fs.CanWrite)
                {
                    Debug.LogError(string.Format("The {0} Is Locked", bundle_md5_info));
                }

                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                foreach (KeyValuePair<string, string> item in LocalizationDic)
                {
                    sw.WriteLine(string.Format("{0},{1}", item.Key, item.Value));
                }
                sw.Dispose();
                sw.Close();
                fs.Dispose();
                fs.Close();
            }
            EditorUtility.DisplayDialog("", "导出完成", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("", "啥都没有导出个屁", "OK");
        }
    }

   //[MenuItem("Localization Tools/一键导入所有文本(路径匹配)")]
    private static void ImportAllTextByPath()
    {
        string ExportText = Application.dataPath + "/../Localization/LocalizationText.csv";
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();

        if (File.Exists(ExportText))
        {
            string line;
            string[] temp;
            int lineIndex = 0;
            StreamReader sr = new StreamReader(ExportText);
            while ((line = sr.ReadLine()) != null)
            {
                temp = line.Split(',');
                lineIndex++;
                if (temp.Length < 2)
                {
                    Debug.LogError("第" + lineIndex + "行数据不符合要求，已忽略");
                    continue;
                }
                string key = temp[0];

                string _Info = utf8.GetString(utf8.GetBytes(temp[1])).Replace("\\n", "\n");

                if (!LocalizationDic.ContainsKey(key))
                {
                    LocalizationDic.Add(key, _Info);
                }
            }
            sr.Close();

            sr.Dispose();
        }

        //int x = 0;

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs/UI" });

        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Text[] allTextComp = go.GetComponentsInChildren<Text>();

                foreach (var item in allTextComp)
                {
                    string textPath = "";
                    GetNodePath(item.transform, ref textPath);
                    string info = item.text.Replace("\n", "\\n");
                    if (LocalizationDic.ContainsKey(textPath))
                    {
                        item.text = LocalizationDic[textPath];
                    }
                }
                EditorUtility.SetDirty(go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        //foreach (var item in LocalizationDic)
        //{
        //    string assetPath = item.Key;
        //    var testList = item.Value;
        //    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        //    if (null == go) continue;
        //    Text[] allTextComp = go.GetComponentsInChildren<Text>();
        //    foreach (var textComp in allTextComp)
        //    {
        //        int _HashCode = textComp.GetHashCode();

        //        string textInfo = testList;
        //        if ("" != textInfo)
        //        {
        //            textComp.text = textInfo;
        //        }
        //    }

        //}

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("", "导入完成", "OK");
    }

   //[MenuItem("Localization Tools/一键导出所有文本(HashCode匹配)")]
    private static void ExportAllText()
    {
        string uiDir = "Assets/Prefabs/UI";
        string csvFileName = Application.dataPath + "/../Localization/LocalizationText.csv";

        if (!EditorUtility.DisplayDialog("提示", string.Format("将要把 {0} 下的prefab的所有text导出到文件 {1} 中, 确定吗?", uiDir, csvFileName), "确定"))
            return;

        Dictionary<string, List<PrefabTextInfo>> LocalizationDic = new Dictionary<string, List<PrefabTextInfo>>();
        LocalizationDic.Clear();

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { uiDir });
        int i = 0;
        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                List<PrefabTextInfo> textList = new List<PrefabTextInfo>();
                textList.Clear();
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Text[] allTextComp = go.GetComponentsInChildren<Text>();

                foreach (var item in allTextComp)
                {
                    PrefabTextInfo textInfo = new PrefabTextInfo();
                    textInfo._HashCode = item.GetHashCode();

                    textInfo._Info = item.text.Replace("\n", "\\n");
                    if (!textList.Contains(textInfo))
                    {
                        textList.Add(textInfo);
                    }
                    else
                    {
                        Debug.LogError(assetPath + item.GetHashCode() + "text出现同 HashCode 预制体有误，请检查");
                    }
                }

                if (!LocalizationDic.ContainsKey(assetPath))
                {
                    LocalizationDic.Add(assetPath, textList);
                }
            }
        }

        if (LocalizationDic.Count > 0)
        {
            using (FileStream fs = new FileStream(csvFileName, FileMode.Create))
            {
                if (!fs.CanWrite)
                {
                    Debug.LogError(string.Format("The {0} Is Locked", csvFileName));
                }
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                foreach (KeyValuePair<string, List<PrefabTextInfo>> item in LocalizationDic)
                {
                    foreach (var innerItem in item.Value)
                    {
                        sw.WriteLine(string.Format("{0},{1},{2}", item.Key, innerItem._HashCode, innerItem._Info));
                    }
                }
                sw.Dispose();
                sw.Close();
                fs.Dispose();
                fs.Close();
            }
            if (EditorUtility.DisplayDialog("", "导出完成, 请查看", "确定"))
            {
                OpenExplorer(csvFileName);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("", "啥都没有导出个屁", "OK");
        }
    }

    private static void ExportSinglePrefabText(string path)
    {
    }

   //[MenuItem("Localization Tools/一键导入所有文本(HashCode匹配)")]
    private static void ImportAllText()
    {
        string ExportText = Application.dataPath + "--LocalizationText.csv";
        Dictionary<string, List<PrefabTextInfo>> LocalizationDic = new Dictionary<string, List<PrefabTextInfo>>();
        LocalizationDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();

        if (File.Exists(ExportText))
        {
            string line;
            string[] temp;
            int lineIndex = 0;
            StreamReader sr = new StreamReader(ExportText);
            while ((line = sr.ReadLine()) != null)
            {
                temp = line.Split(',');
                lineIndex++;
                if (temp.Length < 3)
                {
                    Debug.LogError("第" + lineIndex + "行数据不符合要求，已忽略");
                    continue;
                }
                string key = temp[0];
                int _HashCode = int.Parse(temp[1]);

                string _Info = utf8.GetString(utf8.GetBytes(temp[2])).Replace("\\n", "\n");
                PrefabTextInfo textInfo = new PrefabTextInfo();
                textInfo._HashCode = _HashCode;
                textInfo._Info = _Info;
                if (LocalizationDic.ContainsKey(key))
                {
                    List<PrefabTextInfo> textList = LocalizationDic[key];
                    if (!textList.Contains(textInfo))
                    {
                        textList.Add(textInfo);
                    }

                    LocalizationDic[key] = textList;
                }
                else
                {
                    List<PrefabTextInfo> textList = new List<PrefabTextInfo>();

                    textList.Add(textInfo);
                    LocalizationDic.Add(key, textList);
                }
            }
            sr.Close();
            sr.Dispose();
        }

        //int x = 0;

        foreach (var item in LocalizationDic)
        {
            string assetPath = item.Key;
            var testList = item.Value;
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (null == go) continue;
            Text[] allTextComp = go.GetComponentsInChildren<Text>();
            foreach (var textComp in allTextComp)
            {
                int _HashCode = textComp.GetHashCode();

                string textInfo = GetText(_HashCode, testList);
                if ("" != textInfo)
                {
                    textComp.text = textInfo;
                }
            }

            EditorUtility.SetDirty(go);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("", "导入完成", "OK");
    }

    private static string GetText(int _HashCode, List<PrefabTextInfo> textList)
    {
        foreach (var item in textList)
        {
            if (_HashCode == item._HashCode)
            {
                return item._Info;
            }
        }
        return "";
    }

#endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    static private Dictionary<string, string> ReadOld()
    {
        string exportText = Application.dataPath + "/../Localization/LocalizationTextTest.csv";
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        string line;
        string[] temp;
        int lineIndex = 0;
        StreamReader sr = new StreamReader(exportText, System.Text.Encoding.UTF8);
        while ((line = sr.ReadLine()) != null)
        {
            temp = line.Split(',');
            lineIndex++;

            string key = utf8.GetString(utf8.GetBytes(temp[0]));
            key = key.Replace("\n", @"\n").TrimEnd();
            string value = string.Empty;
            if (temp.Length > 1)
            {
                value = temp[1];
            }
            if (!LocalizationDic.ContainsKey(key) && !string.IsNullOrEmpty(key))
            {
                LocalizationDic.Add(key, value);
            }
        }
        sr.Close();
        sr.Dispose();
        return LocalizationDic;
    }

     //[MenuItem("Localization Tools/XXXXXXXXXXX")]
    static private Dictionary<string, string> ReadSourceCSV()
    {
        string exportText = Application.dataPath + "/../Localization/LocalizationText.csv";
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        string line;
        string[] temp;
        int lineIndex = 0;
        StreamReader sr = new StreamReader(exportText, System.Text.Encoding.UTF8);
        while ((line = sr.ReadLine()) != null)
        {
            temp = line.Split(',');
            if (temp.Length < 2) continue;
            lineIndex++;
            string key = temp[0];

            string value = line.Substring(temp[0].Length+1);
            if (!LocalizationDic.ContainsKey(key))
            {
                LocalizationDic.Add(key, value);
            }
        }
        sr.Close();
        sr.Dispose();
        return LocalizationDic;
    }

    [MenuItem("Localization Tools/一键导出所有文本(路径匹配逗号分隔处理)")]
    private static void ExportAllTextByPathPlus()
    {
        string uiDir = "Assets/Prefabs/UI";
        string csvFileName = Application.dataPath + "/../Localization/LocalizationText.csv";

        if (!EditorUtility.DisplayDialog("提示", string.Format("将要把 {0} 下的prefab的所有text导出到文件 {1} 中, 确定吗?", uiDir, csvFileName), "确定"))
            return;

        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();


        //var TempDic = ReadOld();

        LocalizationDic = ReadSourceCSV();





        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs/UI" });
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Text[] allTextComp = go.GetComponentsInChildren<Text>(true);
                foreach (var item in allTextComp)
                {
                    string textPath = "";
                    GetNodePath(item.transform, ref textPath);
                    var text = utf8.GetString(utf8.GetBytes(item.text));
                    text = text.Replace(",", "&");
                    text = text.Replace("，", "&");
                    text = text.Replace("\n", @"\n").TrimEnd();

                    if (!LocalizationDic.ContainsKey(textPath))
                    {
                        LocalizationDic.Add(textPath, text);
                    }else
                    {
                        var value = LocalizationDic[textPath];
                        var innerTemp = value.Split(',');
                        if (innerTemp.Length > 1)
                        {
                            var xxxx = value.Substring(innerTemp[0].Length);
                           LocalizationDic[textPath]= text+xxxx;
                        }
                    }
                }
            }
        }

        if (LocalizationDic.Count > 0)
        {
            using (FileStream fs = new FileStream(csvFileName, FileMode.Create))
            {
                if (!fs.CanWrite)
                {
                    Debug.LogError(string.Format("The {0} Is Locked", csvFileName));
                }

                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                foreach (KeyValuePair<string, string> item in LocalizationDic)
                {
                    //string KrText = "";
                    //TempDic.TryGetValue(item.Value, out KrText);
                    sw.WriteLine(string.Format("{0},{1}", item.Key, item.Value));
                }
                sw.Dispose();
                sw.Close();
                fs.Dispose();
                fs.Close();
            }
            if (EditorUtility.DisplayDialog("", "导出完成, 请查看", "确定"))
            {
                OpenExplorer(csvFileName);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("", "啥都没有导出个屁", "OK");
        }
    }

    private int _LanguageIndex = 1;

    private void OnGUI()
    {
        _LanguageIndex = EditorGUILayout.IntField("输入要从csv中导入的列数(从0开始)", _LanguageIndex, GUILayout.Width(250));
        if (GUILayout.Button("确定导入"))
        {
            string exportText = Application.dataPath + "/../Localization/LocalizationText.csv";

            if (string.IsNullOrEmpty(filterPath) || !filterPath.Contains("Assets/Outputs"))
            {
                EditorUtility.DisplayDialog("提示", "请选择要导入的interfaces目录，必须在 Assets/Outputs 下\n方法: 选中目录，右键点击 将翻译的csv文本导入此文件夹", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("提示", string.Format("将要把 文件 {0} 中的第 {1} 列文本导入到 {2} 下的prefab的text中, 确定吗?", exportText, _LanguageIndex, filterPath), "确定"))
                return;
            
            LanguageImport(exportText, _LanguageIndex);
        }
    }

    private static void ShowWindow()
    {
        Rect rc = new Rect(100, 100, 1024, 768);
        LocalizationTools window = EditorWindow.GetWindowWithRect(typeof(LocalizationTools), rc, true) as LocalizationTools;
        if (null != window)
        {
            //window.titleContent = new GUIContent("场景编辑器");
            window.Init();
            window.Show();
        }
    }

    private static string filterPath = "";

    private void LanguageImport(string csvFileName, int column)
    {
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();

        if (File.Exists(csvFileName))
        {
            string line;
            string[] temp;
            StreamReader sr = new StreamReader(csvFileName, Encoding.Default);
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length == 0)
                    continue;

                temp = line.Split(',');
                if (temp.Length <= column)
                {
                    this.ShowNotification(new GUIContent("文件错误，没有那么多列"));
                    break;
                }

                string key = temp[0];
                //string test = temp[column].Replace("&", ",");

                string _Info = utf8.GetString(utf8.GetBytes(temp[column])).Replace("\\n", "\n");
                if (string.IsNullOrEmpty(_Info))
                {
                    _Info = utf8.GetString(utf8.GetBytes(temp[1])).Replace("\\n", "\n"); 
                }
                if (!LocalizationDic.ContainsKey(key))
                {
                    LocalizationDic.Add(key, _Info);
                }
            }
            sr.Close();
            sr.Dispose();
        }
        else
        {
            EditorUtility.DisplayDialog("错误", string.Format("要导入的csv不存在，请确认! {0}", csvFileName), "确定");
            return;
        }

        string[] filterPaths = new string[1];
        filterPaths[0] = filterPath;
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", filterPaths);

        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
               
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Text[] allTextComp = go.GetComponentsInChildren<Text>(true);

                foreach (var item in allTextComp)
                {
                    string textPath = "";
                    GetNodePath(item.transform, ref textPath);
                    
                    string info = item.text.Replace("\n", "\\n");
                    if (LocalizationDic.ContainsKey(textPath))
                    {
                        item.text = LocalizationDic[textPath].Replace("&",",");
                    }
                }
                EditorUtility.SetDirty(go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("", "导入完成", "OK");
    }

    public int _LanguageCount = 0;

    //public
    private void Init()
    {
        string exportText = Application.dataPath + "/../Localization/LocalizationText.csv";
        if (!File.Exists(exportText))
        {
            EditorUtility.DisplayDialog("错误", string.Format("要导入的csv不存在，请确认! {0}", exportText), "确定");
            return;
        }

        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();

        {
            string line;
            string[] temp;
            int lineIndex = 0;
            StreamReader sr = new StreamReader(exportText);
            if ((line = sr.ReadLine()) != null)
            {
                temp = line.Split(',');
                if (0 == lineIndex)
                {
                    _LanguageCount = temp.Length;
                    lineIndex++;
                }
            }
            sr.Close();
            sr.Dispose();
        }
    }

    private static void ImportAllTextByPathPlus()
    {
        string ExportText = Application.dataPath + "/../Localization/LocalizationText.csv";
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();

        if (File.Exists(ExportText))
        {
            string line;
            string[] temp;
            int lineIndex = 0;
            StreamReader sr = new StreamReader(ExportText);
            while ((line = sr.ReadLine()) != null)
            {
                temp = line.Split(',');
                lineIndex++;
                if (temp.Length < 2)
                {
                    Debug.LogError("第" + lineIndex + "行数据不符合要求，已忽略");
                    continue;
                }
                string key = temp[0];

                string _Info = utf8.GetString(utf8.GetBytes(temp[1])).Replace("\\n", "\n");

                if (!LocalizationDic.ContainsKey(key))
                {
                    LocalizationDic.Add(key, _Info);
                }
            }
            sr.Close();
            sr.Dispose();
        }

        //int x = 0;

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs/UI" });

        foreach (string guid in allGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Text[] allTextComp = go.GetComponentsInChildren<Text>();

                foreach (var item in allTextComp)
                {
                    string textPath = "";
                    GetNodePath(item.transform, ref textPath);
                    string info = item.text.Replace("\n", "\\n");
                    if (LocalizationDic.ContainsKey(textPath))
                    {
                        item.text = LocalizationDic[textPath];
                    }
                }
                EditorUtility.SetDirty(go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("", "导入完成", "OK");
    }

 

    private static void OpenExplorer(string outputPath)
    {
        string str = "";
        for (int i = 0; i < outputPath.Length; ++i)
        {
            char c = outputPath[i];
            if (c == '/')
                c = '\\';
            str += c;
        }
        Utility.ShellExecute(
            IntPtr.Zero,
            "open",
            "Explorer.exe",
             "/select, " + str,
            "",
            Utility.ShowCommands.SW_NORMAL);
    }

   [MenuItem("Assets/将翻译的csv文本导入至此文件夹")]
    private static void ImpoortToFolder()
    {
        var gp = Selection.activeObject;
        if (null == gp)
        {
            EditorUtility.DisplayDialog("", "没有选中哟？", "OK");
            return;
        }
        var assetPath = AssetDatabase.GetAssetPath(gp);
        if (!assetPath.Contains("Assets/Outputs"))
        {
            EditorUtility.DisplayDialog("", "不是所有文件夹都能导入的！！路径必须包含 Assets/Outputs", "OK");
            return;
        }
        filterPath = assetPath;
        ShowWindow();
    }

   [MenuItem("Assets/和Interfaces文件夹比较异同")]
    private static void CompareToInterfacesFolder()
    {
        string leftFolder = "Assets/Outputs/Interfaces";

        var gp = Selection.activeObject;
        if (null == gp)
        {
            EditorUtility.DisplayDialog("", "没有选中哟？", "OK");
            return;
        }

        var assetPath = AssetDatabase.GetAssetPath(gp);
        Debug.LogWarning(assetPath);
        if (!Directory.Exists(assetPath) || !assetPath.Contains("Assets/Outputs/Interfaces"))
        {
            EditorUtility.DisplayDialog("", "不是所有文件夹都能导入的！！请选择一个文件夹, 路径必须包含 Assets/Outputs/Interfaces", "OK");
            return;
        }

        string diffCsv = Application.dataPath + "--InterfaceDiff.csv";
        OutputDifferencce(leftFolder, assetPath, diffCsv);
    }

    private static void OutputDifferencce(string leftFolder, string rightFolder, string csvFileName)
    {
        string[] allLeftGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { leftFolder });
        List<string> leftFolderGameObjectNames = new List<string>();
        foreach (string guid in allLeftGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".prefab"))
                continue;

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (null == go) 
                continue;
            GetGameObjectNamesOfPrefab(go.transform, leftFolderGameObjectNames);
        }

        string[] allRightGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { rightFolder });
        List<string> rightFolderGameObjectNames = new List<string>();
        foreach (string guid in allRightGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.EndsWith(".prefab"))
                continue;

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (null == go)
                continue;
            GetGameObjectNamesOfPrefab(go.transform, rightFolderGameObjectNames);
        }

        List<string> leftOutputList = new List<string>();
        List<string> rightOutputList = new List<string>();

        foreach(string name in leftFolderGameObjectNames)
        {
            if (!rightFolderGameObjectNames.Contains(name))
                leftOutputList.Add(name);
        }

        foreach(string name in rightFolderGameObjectNames)
        {
            if (!leftFolderGameObjectNames.Contains(name))
                rightOutputList.Add(name);
        }

        if (leftOutputList.Count == 0 && rightOutputList.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", string.Format("两个Interfaces目录对比相同\n左： {0}\n右: {1}", leftFolder, rightFolder), "确定");
        }
        else
        {
            HashSet<string> prefabSet = new HashSet<string>();
            using (FileStream fs = new FileStream(csvFileName, FileMode.Create))
            {
                if (!fs.CanWrite)
                {
                    Debug.LogError(string.Format("The {0} Is Locked", csvFileName));
                }
                else
                {
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                    foreach (string name in leftOutputList)
                    {
                        prefabSet.Add(GetPrefabName(name));
                        sw.WriteLine(string.Format("{0},{1}", name, string.Empty));
                    }
                    foreach (string name in rightOutputList)
                    {
                        prefabSet.Add(GetPrefabName(name));
                        sw.WriteLine(string.Format("{0},{1}", string.Empty, name));
                    }
                    sw.Close();
                }
            }

            if (prefabSet.Count > 0)
            {
                string prefabs = "";
                foreach(string name in prefabSet)
                {
                    prefabs += name;
                    prefabs += "\n";
                }
                EditorUtility.DisplayDialog("不同的prefab", prefabs, "确定");
            }

            OpenExplorer(csvFileName);
        }
       
    }

    private static string GetPrefabName(string objName)
    {
        int l = objName.IndexOf('/');
        if (l < 0)
            return objName;
        return objName.Substring(0, l);
    }

    private static void GetGameObjectNamesOfPrefab(Transform root, List<string> nameList)
    {
        for (int i = 0; i < root.transform.childCount; ++i)
        {
            Transform child = root.transform.GetChild(i);

            string path = "";
            GetNodePath(child, ref path);
            nameList.Add(path);

            GetGameObjectNamesOfPrefab(child, nameList);
        }
    }

}

public class LocalizationWindow2 : EditorWindow
{
   [MenuItem("Assets/将该文件夹下的图片进行替换")]
    public static void ReplaceAtlas()
    {
        //找到所有图
        string filterPath = "";
        var gp = Selection.activeObject;
        if (null == gp)
        {
            EditorUtility.DisplayDialog("", "没有选中哟？", "OK");
            return;
        }

        var folderPath = AssetDatabase.GetAssetPath(gp);
        Debug.LogError(folderPath);
        if (!folderPath.Contains("Assets/Outputs"))
        {
            EditorUtility.DisplayDialog("", "不是所有文件夹都能导入的！！路径必须包含 Assets/Outputs", "OK");
            return;
        }

        if (folderPath.EndsWith(".png") || folderPath.EndsWith(".tpsheet"))
        {
            EditorUtility.DisplayDialog("", "请选择文件夹的空白处，点击右键！！", "OK");
            return;
        }

        filterPath = folderPath;
        EditorUtility.DisplayProgressBar("提示", "收集图集信息", 0.01f);

        string[] temp = filterPath.Split(new char[] { '/' });
        string lang = "Assets/Outputs/Interfaces_" + temp[temp.Length - 1];

        string[] allGuids = AssetDatabase.FindAssets("t:Sprite", new string[] { filterPath });

        string[] fileList = Directory.GetFiles(filterPath, "*.*", SearchOption.AllDirectories);

        List<string> atlasNameList = new List<string>();

        for (int i = 0; i < fileList.Length; i++)
        {
            if (fileList[i].EndsWith(".png"))
            {
                string atlasName = fileList[i].Replace("\\", "/").Trim();
                if (!atlasNameList.Contains(atlasName))
                {
                    atlasNameList.Add(atlasName);
                }
            }
        }

        List<UnityEngine.Object> ObjList = new List<UnityEngine.Object>();
        foreach (var item in atlasNameList)
        {
            UnityEngine.Object[] x = AssetDatabase.LoadAllAssetsAtPath(item);
            foreach (var innerItem in x)
            {
                if (!ObjList.Contains(innerItem))
                {
                    //Debug.LogError(innerItem.name);
                    ObjList.Add(innerItem);
                }
            }
        }

        EditorUtility.DisplayProgressBar("提示", "开始遍历预设", 0.05f);
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { lang });

        EditorUtility.DisplayProgressBar("提示", "开始遍历预设图片", 0.05f);
        int index = 0;
        foreach (string guid in allPrefabGuids)
        {
            index++;
            EditorUtility.DisplayProgressBar("提示", ((float)index / (float)allPrefabGuids.Length).ToString(), (float)index / (float)allPrefabGuids.Length);
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == go) continue;
                Image[] allImageComp = go.GetComponentsInChildren<Image>(true);
                foreach (var item in allImageComp)
                {
                    if (null == item.sprite)
                    {
                        continue;
                    }
                    Sprite sprite = GetSprite(ObjList, item.sprite.name);
                    if (null != sprite)
                    {
                        item.sprite = sprite;
                        item.overrideSprite = sprite;
                    }
                }

                GImageGroup[] allImageGroup = go.GetComponentsInChildren<GImageGroup>();

                foreach (var item in allImageGroup)
                {
                    if (null == item._Sprites || 0 == item._Sprites.Length)
                    {
                        continue;
                    }

                    for (int i = 0; i < item._Sprites.Length; i++)
                    {
                        if (null == item._Sprites[i]) continue;
                        Sprite sprite = GetSprite(ObjList, item._Sprites[i].name);
                        if (null != sprite)
                        {
                            item._Sprites[i] = sprite;
                        }
                        EditorUtility.SetDirty(item._Sprites[i]);
                    }
                    //Sprite sprite = GetSprite(ObjList, item.sprite.name);
                    //if (null != sprite)
                    //{
                    //    item.sprite = sprite;
                    //    item.overrideSprite = sprite;
                    //}
                }

                EditorUtility.SetDirty(go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("", "导入完成", "OK");
    }

    public static Sprite GetSprite(List<UnityEngine.Object> ObjList, string name)
    {
        foreach (var item in ObjList)
        {
            if (item.name == name && item.GetType() == typeof(Sprite))
            {
                return item as Sprite;
            }
        }
        return null;
    }
}