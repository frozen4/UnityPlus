using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationToolsPlus : EditorWindow
{
    static string exportText = Application.dataPath + "/../Localization/LocalizationText.csv";
    static string uiDir = "Assets/Prefabs/UI";
    static string KRDir = "Assets/Outputs/Interfaces_KR";
	[MenuItem("Localization Tools/TEMP/导出中文")]
    private static void ExportChinese()
    {
        if (!File.Exists(exportText))
        {
            if (!EditorUtility.DisplayDialog("Warning", "不存在原有文件，本次导出将重新导出，是否继续?", "OK", "Cancel")) return;
        }

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
  

        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { uiDir });
         
        
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
                   var text = utf8.GetString(utf8.GetBytes(item.text));
                   text = text.Replace(",", "&");
                   text = text.Replace("，", "&");
                   text = text.Replace("\n", @"\n").TrimEnd();
                    var text2 = text.Replace("\n", "");
                    if (!LocalizationDic.ContainsKey(text) && !string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2) && 0!=text2.Length)
                    {
                        LocalizationDic.Add(text, string.Empty);
                    }
                }
            }
        }



        using (FileStream fs = new FileStream(exportText, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                Debug.LogError(string.Format("The {0} Is Locked", exportText));
            }
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            foreach (var item in LocalizationDic)
            {
                sw.WriteLine(string.Format("{0},{1}", item.Key, item.Value));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();
           
        }
        if (EditorUtility.DisplayDialog("", "导出完成, 请查看", "确定"))
        {
            OpenExplorer(exportText);
        }
    }

    [MenuItem("Localization Tools/TEMP/转换文本")]
    private static void Change()
    {
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
        var csvFileName = Application.dataPath + "--LocalizationTextByPathPlus.csv";
        var column = 3;
        if (File.Exists(csvFileName))
        {
            string line;
            string[] temp;
            StreamReader sr = new StreamReader(csvFileName, Encoding.Default);
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length == 0) continue;
                temp = line.Split(',');
                if (temp.Length <= column) continue;
                string key = temp[1].Replace(@"\n", "\n").TrimEnd();
                string test = temp[column].Replace(@"\n", "\n").TrimEnd();
                string _Info = utf8.GetString(utf8.GetBytes(temp[column])).Replace(@"\n", "\n").TrimEnd();
                if (!LocalizationDic.ContainsKey(key) && !string.IsNullOrEmpty(key))
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


        using (FileStream fs = new FileStream(exportText, FileMode.Create))
        {
            if (!fs.CanWrite)
            {
                Debug.LogError(string.Format("The {0} Is Locked", exportText));
            }
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            foreach (var item in LocalizationDic)
            {
                if (item.Key.Contains("传奇属性"))
                {
                    int xx = 1;
                }
                sw.Write(string.Format("{0},{1}\n", item.Key, item.Value));
            }
            sw.Dispose();
            sw.Close();
            fs.Dispose();
            fs.Close();

        }
        if (EditorUtility.DisplayDialog("", "导出完成, 请查看", "确定"))
        {
            OpenExplorer(exportText);
        }

    }
    [MenuItem("Localization Tools/TEMP/导入韩文")]
    private static void ImportKR()
    {

        Dictionary<string, string> LocalizationDic = new Dictionary<string,string>();
        string line;
        string[] temp;
        int lineIndex = 0;
        StreamReader sr = new StreamReader(exportText);
        while ((line = sr.ReadLine()) != null)
        {
            temp = line.Split(',');
            lineIndex++;
          
            string key = temp[0];
            key = key.Replace("\n", @"\n").TrimEnd();
            string value = string.Empty;
            if (temp.Length >1)
            {
                value = temp[1];
            }
            if (!LocalizationDic.ContainsKey(key)&& !string.IsNullOrEmpty(key))
            {
                LocalizationDic.Add(key, value);
            }
        }
        sr.Close();
        sr.Dispose();


        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { KRDir });
        int i = 0;
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
                    var text = utf8.GetString(utf8.GetBytes(item.text));
                    text = text.Replace(",", "&");
                    text = text.Replace("，", "&");
                    text = text.Replace("\n", @"\n").TrimEnd();
                    var text2 = text.Replace("\n", "");

                    if (LocalizationDic.ContainsKey(text))
                    {
                        if (!string.IsNullOrEmpty(LocalizationDic[text]))
                        {
                            item.text = LocalizationDic[text].Replace("&",",");
                        }
                        
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
     [MenuItem("Localization Tools/TEMP/导入繁体")]
     private static void ImportTW()
     {

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
}
