using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

public class CheckDependencies : Editor
{
    public class TextureExportInfo
    {
        public bool IsSprite2D;
        public int Width;
        public int Height;
        public List<string> References;

        public TextureExportInfo(bool isSprite2D, int width = 0, int height = 0, List<string> references = null)
        {
            IsSprite2D = isSprite2D;
            Width = width;
            Height = height;
            if (references == null)
                References = new List<string>();
            else
                References = references;
        }
    }

    [MenuItem("Tools/资源/检查Texture所有引用", false, 20)]
    private static void CheckTexturesDependencies()
    {
        string barTitle = "CheckTexturesDependencies";
        var referenceMap = new Dictionary<string, TextureExportInfo>();
        try
        {
            //找到所有Texture
            var textureGuids = AssetDatabase.FindAssets("t:Texture");
            for (int i = 0; i < textureGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                var texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                if (texture != null)
                {
                    string name = GetAssetName(assetPath);
                    referenceMap.Add(name, new TextureExportInfo(false, texture.width, texture.height));
                }

                EditorUtility.DisplayProgressBar(barTitle, "Check Sprite", (float)i / (float)textureGuids.Length);
            }
            var spriteGuids = AssetDatabase.FindAssets("t:Sprite");
            for (int i = 0; i < spriteGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
                string name = GetAssetName(assetPath);
                if (referenceMap.ContainsKey(name))
                    referenceMap[name].IsSprite2D = true;

                EditorUtility.DisplayProgressBar(barTitle, "Check Sprite", (float)i / (float)spriteGuids.Length);
            }
            //反向查找依赖
            var guids = AssetDatabase.FindAssets("t:Prefab t:Scene t:Material t:ScriptableObject t:Font");
            for (int i = 0; i < guids.Length; i++)
            {
                CheckTexturesDependenciesInternal(guids[i], referenceMap);
                EditorUtility.DisplayProgressBar(barTitle, "Check Assets has dependencies possible", (float)i / (float)guids.Length);
            }
            //导出结果文件
            List<string> exportInfos = new List<string>();
            string title = "Texture,Width*Height,IsSprite2D,References";
            exportInfos.Add(title);
            using (var enumerator = referenceMap.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string texture = enumerator.Current.Key;
                    string isSprite2D = enumerator.Current.Value.IsSprite2D.ToString().ToLower();
                    string w_h = enumerator.Current.Value.Width + "*" + enumerator.Current.Value.Height;
                    if (enumerator.Current.Value.References.Count <= 1)
                    {
                        string references = string.Join("\n,,", enumerator.Current.Value.References.ToArray());
                        exportInfos.Add(string.Format("{0},{1},{2},{3}", texture, w_h, isSprite2D, references));
                    }
                    else
                    {
                        foreach (string reference in enumerator.Current.Value.References)
                        {
                            exportInfos.Add(string.Format("{0},{1},{2},{3}", texture, w_h, isSprite2D, reference));
                        }
                    }
                }
            }
            ExportCSV(exportInfos, "TextureCheck", "Texture引用检查结果");
        }
        catch(System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void CheckTexturesDependenciesInternal(string guids, Dictionary<string, TextureExportInfo> map)
    {
        if (map == null) return;

        string assetPath = AssetDatabase.GUIDToAssetPath(guids);
        string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
        string assetName = GetAssetName(assetPath);
        for (int i = 0; i < dependencies.Length; i++)
        {
            var obj = AssetDatabase.LoadAssetAtPath(dependencies[i], typeof(UnityEngine.Object));
            if (obj != null && obj is Texture)
            {
                string name = GetAssetName(dependencies[i]);
                if (map.ContainsKey(name))
                {
                    var exportInfo = map[name];
                    if (!exportInfo.References.Contains(assetName))
                        exportInfo.References.Add(assetName);
                }
                else
                    Debug.LogErrorFormat("Get Invaild Texture:{0}", name);
            }
        }
    }

    private static string GetAssetName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return path.Substring("Assets/".Length);
    }

    private static void ExportCSV(List<string> infos, string textName, string tag = "")
    {
        try
        {
            string folderPath = Application.dataPath + "/../Export/";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string textPath = folderPath + textName + ".csv";
            using (FileStream fs = new FileStream(textPath, FileMode.Create))
            {
                if (!fs.CanWrite)
                {
                    Debug.LogError(string.Format("The {0} Is Locked", textPath));
                }

                StreamWriter sw = new StreamWriter(fs);
                foreach (var cacheData in infos)
                {
                    sw.WriteLine(cacheData);
                }
                sw.Dispose();
                sw.Close();
                fs.Dispose();
                fs.Close();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(tag + "导出完成", "导出路径为" + textPath, "OK");
        }
        catch(System.Exception e)
        {
            Debug.LogException(e);
        }
    }
}
