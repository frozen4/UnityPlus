using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
public class AutoLocalizationTools : Editor
{
    [MenuItem("Localization Tools/Sync Interfaces/请谨慎！！")]
    static void SyncInterfacesExt()
    {
        string dirAssetBundles = Application.dataPath;
        TextLogger.Path = Path.Combine(dirAssetBundles, "SyncInterfaces日志.log");
        File.Delete(TextLogger.Path);
        SyncInterfaces();

    }

    static void SyncInterfaces()
    {
        //TextLogger.Instance.WriteLine(string.Format("准备BuildAssetBundles Output: {0}", outputPath));
        TextLogger.Instance.WriteLine("开始同步Interfaces 文件夹 至 Interface_KR 文件夹:");
        string sourcePath = "Assets/Outputs/Interfaces";
        string targetPath = "Assets/Outputs/Interfaces_KR";
        FileUtil.ReplaceDirectory(sourcePath, targetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        TextLogger.Instance.WriteLine("文件夹同步完成");
        TextLogger.Instance.WriteLine("开始进行本地化文本自动导入");
        string exportText = Application.dataPath + "/../Localization/LocalizationText.csv";
        LanguageImport(exportText, 2, targetPath);
    }

    static void LanguageImport(string csvFileName, int column, string targetPath)
    {
        Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();
        LocalizationDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
        if (!File.Exists(csvFileName))
        {
            TextLogger.Instance.WriteLine("本地化文本csv不存在");
            return;
        }
        string line;
        string[] temp;
        StreamReader sr = new StreamReader(csvFileName, Encoding.Default);
        int lineIndex = 0;
        while ((line = sr.ReadLine()) != null)
        {
            ++lineIndex;
            if (line.Length == 0) continue;
            temp = line.Split(',');
            if (temp.Length <= column)
            {
                TextLogger.Instance.WriteLine(string.Format("本地化文本第{0}行不存在，不予翻译，程序继续执行", lineIndex));
                continue;
            }

            string key = temp[0];
            string test = temp[column].Replace("\\n", "\n");
            string _Info = utf8.GetString(utf8.GetBytes(temp[column])).Replace("\\n", "\n");
            if (!LocalizationDic.ContainsKey(key))
            {
                LocalizationDic.Add(key, _Info);
            }
        }
        sr.Close();
        sr.Dispose();


        ReadFontInfo();



        var filterPath = "Assets/Outputs/CommonAtlas/Atlas/KR";
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
                    ObjList.Add(innerItem);
                }
            }
        }

        string[] filterPaths = new string[] { targetPath };
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
                    string textPath = UnityUtil.GetNodePath(item.transform);
                    string info = item.text.Replace("\n", "\\n");
                    if (LocalizationDic.ContainsKey(textPath))
                    {
                        item.text = LocalizationDic[textPath].Replace("&", ",");
                    }
                    Font targetFont;
                    if (fontDic.TryGetValue(item.font, out targetFont))
                    {
                        var sourceSize = item.fontSize;
                        int size;
                        fontSizeDic.TryGetValue(item.font, out size);
                        item.fontSize = sourceSize + size;
                        item.font = targetFont;
                    }
                }


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

                GImageGroup[] allImageGroup = go.GetComponentsInChildren<GImageGroup>(true);

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
                }

                EditorUtility.SetDirty(go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        AssetDatabase.Refresh();
        TextLogger.Instance.WriteLine("本地化文本导入完成");
        //EditorUtility.DisplayDialog("", "导入完成", "OK");

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

    static Dictionary<Font, Font> fontDic = new Dictionary<Font, Font>();
    static Dictionary<Font, int> fontSizeDic = new Dictionary<Font, int>();
    static void ReadFontInfo()
    {
        string csvFileName = Application.dataPath + "/../Localization/FontToKR.csv";

        fontDic.Clear();
        fontSizeDic.Clear();
        System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
        if (!File.Exists(csvFileName))
        {
            TextLogger.Instance.WriteLine("不存在字体替换关系表" + csvFileName);
            return;
        }
        string line;
        string[] temp;
        StreamReader sr = new StreamReader(csvFileName, Encoding.Default);
        int lineIndex = 0;
        while ((line = sr.ReadLine()) != null)
        {
            ++lineIndex;
            if (line.Length == 0) continue;
            temp = line.Split(',');
            if (temp.Length <= 2)
            {
                TextLogger.Instance.WriteLine(string.Format("本地化文本第{0}行不存在，不予翻译，程序继续执行", lineIndex));
                continue;
            }
            string sourcePath = temp[0];
            string targetPath = temp[1];
            int size = 0;
            int.TryParse(temp[2], out size);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
            Font targetFont = AssetDatabase.LoadAssetAtPath<Font>(targetPath);
            if (null == sourceFont || null == targetPath) continue;
            if (!fontDic.ContainsKey(sourceFont)) fontDic.Add(sourceFont, targetFont);
            if (!fontSizeDic.ContainsKey(sourceFont)) fontSizeDic.Add(sourceFont, size);
        }
        sr.Close();
        sr.Dispose();
    }


    [MenuItem("Assets/Tools/CopyAssetPath")]
    static void CopyAssetPath()
    {
        var go = Selection.activeInstanceID;
        //string exportText = Application.dataPath + "/../Localization/FontToKR.csv";
        var assetPath = AssetDatabase.GetAssetPath(go);
        TextEditor t = new TextEditor();
        t.text = assetPath;
        t.OnFocus();
        t.Copy();
    }
}
