using System.Collections;
using UnityEngine.UI;
using Common;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;


public partial class GameDataCheckMan : Singleton<GameDataCheckMan>
{
    public bool WriteCsvFile(string csvFile, Dictionary<string, int> content)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in content)
            {
                string name = kv.Key;
                int count = kv.Value;

                if (!string.IsNullOrEmpty(name))
                    writer.WriteLine("{0},{1}", name, count);
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, Dictionary<string, string> content)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in content)
            {
                string name = kv.Key;
                string val = kv.Value;

                if (!string.IsNullOrEmpty(name))
                    writer.WriteLine("{0},{1}", name, val);
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, SortedList<string, LuaPrefabCheck.CLuaClass> content)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in content)
            {
                string name = kv.Key;
                string name1 = kv.Value.strName;
                int isRef = kv.Value.isReferenced ? 1 : 0;

                if (!string.IsNullOrEmpty(name))
                    writer.WriteLine("{0},{1},{2}", name, name1, isRef);
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, List<StaticMeshResourceCheck.CMeshEntry> listEntries)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in listEntries)
            {
                string name = kv.name;
                string parent = kv.parent;
                int faces = kv.faces;

                if (!string.IsNullOrEmpty(name))
                    writer.WriteLine("{0},{1},{2}", name, parent, faces);
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, Dictionary<string, StaticMeshResourceCheck.CBlockMeshInfo> dicPrefabMeshInfo, Dictionary<string, StaticMeshResourceCheck.CBlockMeshInfo> dicBlockMeshInfo)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in dicPrefabMeshInfo)
            {
                string name = kv.Key;
                StaticMeshResourceCheck.CBlockMeshInfo meshInfo = kv.Value;

                //string parent = name;
                //if (meshInfo.meshList.Count > 0)
                //    parent = meshInfo.meshList[0].parent;

                if (!string.IsNullOrEmpty(name))
                    writer.WriteLine("{0},{1}", name, meshInfo.CalcTotalFaces());
            }

            foreach (var kv in dicBlockMeshInfo)
            {
                string name = kv.Key;
                StaticMeshResourceCheck.CBlockMeshInfo meshInfo = kv.Value;

//                 string parent = name;
//                 if (meshInfo.meshList.Count > 0)
//                     parent = meshInfo.meshList[0].parent;

                if (!string.IsNullOrEmpty(name))
                    writer.WriteLine("{0},{1}", name, meshInfo.CalcTotalFaces());
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, List<CharacterResourceCheck.CTextureInfo> listTextures)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in listTextures)
            {
                string prefabName = kv.prefabName;
                string goName = kv.goName;
                string texturename = kv.texInfo.textureName;
                int width = kv.texInfo.width;
                int height = kv.texInfo.height;

                if (!string.IsNullOrEmpty(texturename))
                    writer.WriteLine("{0},{1},{2},{3},{4}", prefabName, goName, texturename, width, height);
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, List<CharacterResourceCheck.CSimpleTextureInfo> listTextures)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in listTextures)
            {
                string texturename = kv.textureName;
                int width = kv.width;
                int height = kv.height;

                if (!string.IsNullOrEmpty(texturename))
                    writer.WriteLine("{0},{1},{2}", texturename, width, height);
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, SortedDictionary<string, ShaderResourceCheck.CShaderEntry> shaderDic)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in shaderDic)
            {
                string name  = kv.Key;
                var keywordList = kv.Value.keywordList;
                string val = string.Empty;

                for (int i = 0; i < keywordList.Count; ++i)
                {
                    bool bAdd = false;
                    foreach (var kw in keywordList[i])
                    {
                        if (!string.IsNullOrEmpty(kw) && !kw.Contains("\r") && !kw.Contains("\n"))
                        {
                            val += kw;
                            val += " ";

                            bAdd = true;
                        }
                    }

                    if (bAdd && i + 1 != keywordList.Count)
                        val += "\n";
                }

                
                //val = Template.TemplateBase.ProcessStringForCsv(val);
                val = string.Format("\"{0}\"", val);
                writer.WriteLine("{0},{1}", name, val);
            }
        }
        return true;
    }

    public bool WriteCsvFile(string csvFile, List<ShaderResourceCheck.CUsedShaderEntry> usedShaders)
    {
        string logDir = System.IO.Path.Combine(Application.dataPath, "../../CheckResult_GameData/");
        if (!Directory.Exists(logDir))
            Directory.CreateDirectory(logDir);
        string strFile = System.IO.Path.Combine(logDir, csvFile);
        File.Delete(strFile);

        using (StreamWriter writer = new StreamWriter(strFile, false, new System.Text.UTF8Encoding(true)))
        {
            foreach (var kv in usedShaders)
            {
                writer.WriteLine("{0},{1},{2},{3}", kv.shaderName, kv.matName, kv.goName, kv.prafabName);
            }
        }
        return true;
    }
}


