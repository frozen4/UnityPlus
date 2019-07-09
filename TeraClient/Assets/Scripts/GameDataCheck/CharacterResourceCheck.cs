using System;
using Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Template;
using Object = UnityEngine.Object;


public partial class CharacterResourceCheck : Singleton<CharacterResourceCheck>
{
    public List<string> _AssetPathList = new List<string>();

    private int TextureLimit = 1024;

    //需要忽略的player的几个部位
    public string[] _IgnoredPlayerPrefabs = { 
                                                "alipriest_f.prefab", 
                                                "casassassin_m.prefab",
                                                "humwarrior_m.prefab",
                                                "sprarcher_f.prefab",
                                            };
    public string[] _IgnoredParts = { "body", "face", "hair", };

    public class CSimpleTextureInfo
    {
        public string textureName = string.Empty;
        public int width = 0;
        public int height = 0;
    }

    public class CTextureInfo
    {
        public string prefabName = string.Empty;
        public string goName = string.Empty;
        public CSimpleTextureInfo texInfo = new CSimpleTextureInfo();
    }

    public List<CTextureInfo> LargeTextureList = new List<CTextureInfo>();
    public List<CTextureInfo> NoStandardTextureList = new List<CTextureInfo>();

    public List<CSimpleTextureInfo> GetSimpleTextureList(List<CTextureInfo> textureInfoList)
    {
        List<CSimpleTextureInfo> list = new List<CSimpleTextureInfo>();

        foreach(var entry in textureInfoList)
        {
            bool bContains = false;

            foreach(var info in list)
            {
                if (info.textureName == entry.texInfo.textureName && 
                    info.width == entry.texInfo.width &&
                    info.height == entry.texInfo.height)
                {
                    bContains = true;
                    break;
                }
            }

            if (bContains)
                continue;

            list.Add(entry.texInfo);
        }

        return list;
    }

    private void AddToTextureInfoList(List<CTextureInfo> textureList, string prefabName, string goName, string textureName, int width, int height)
    {
        foreach(var info in textureList)
        {
            if (info.prefabName == prefabName && info.goName == goName &&
                info.texInfo.textureName == textureName && info.texInfo.width == width && info.texInfo.height == height)
                return;
        }

        CTextureInfo texInfo = new CTextureInfo();
        texInfo.prefabName = prefabName;
        texInfo.goName = goName;
        texInfo.texInfo.textureName = textureName;
        texInfo.texInfo.width = width;
        texInfo.texInfo.height = height;

        textureList.Add(texInfo);
    }

    public void SortListEntries(List<CTextureInfo> textureList)
    {
        textureList.Sort(delegate(CTextureInfo x, CTextureInfo y)
        {
            int eq = x.prefabName.CompareTo(y.prefabName);

            if (eq == 0)
                eq = x.goName.CompareTo(y.goName);

            if (eq == 0)
                eq = x.texInfo.textureName.CompareTo(y.texInfo.textureName);

            if (eq == 0)
                eq = x.texInfo.width.CompareTo(y.texInfo.width);

            if (eq == 0)
                eq = x.texInfo.height.CompareTo(y.texInfo.height);

            return eq;
        });
    }

    public void SortListEntries(List<CSimpleTextureInfo> textureList)
    {
        textureList.Sort(delegate(CSimpleTextureInfo x, CSimpleTextureInfo y)
        {
            int eq = x.textureName.CompareTo(y.textureName);

            if (eq == 0)
                eq = x.width.CompareTo(y.width);

            if (eq == 0)
                eq = x.height.CompareTo(y.height);

            return eq;
        });
    }

    public void Init(int textureLimit)
    {
        TextureLimit = textureLimit;

        _AssetPathList.Clear();
        _AssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("characters"));
        _AssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("outward"));
        _AssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("monsters"));
    }

    public bool IsPlayerPart(string prefabName, string partName)
    {
       foreach(string playerPrefab in _IgnoredPlayerPrefabs)
       {
           if (prefabName.Contains(playerPrefab))
           {
               foreach(string part in _IgnoredParts)
               {
                   if (part == partName)
                       return true;
               }
           }
       }
       return false;
    }

    public string _ErrorString = "";

    //
    public IEnumerable DoCheckSkinMeshRendererCoroutine(string prefabName, GameObject scenePrefab)
    {
        List<string> errorList_MaterialMissing = new List<string>();
        HashSet<string> errorSet_MaterialMissing = new HashSet<string>();
        List<string> errorList_MaterialShaderMissing = new List<string>();
        HashSet<string> errorSet_MaterialShaderMissing = new HashSet<string>();
        List<string> errorList_MaterialShaderError = new List<string>();
        HashSet<string> errorSet_MaterialShaderError = new HashSet<string>();
        List<string> errorList_TextureLarge = new List<string>();
        HashSet<string> errorSet_TextureLarge = new HashSet<string>();
        List<string> errorList_TextureNPT = new List<string>();
        HashSet<string> errorSet_TextureNPT = new HashSet<string>();
        List<string> errorList_TextureNull = new List<string>();
        //HashSet<string> errorSet_TextureNull = new HashSet<string>();

        List<GameObject> rendererList = new List<GameObject>();
        CUnityUtil.FindChildLeaf(typeof(SkinnedMeshRenderer), typeof(MeshRenderer), scenePrefab, rendererList);

        //检查每个包含MeshRenderer的go
        foreach (var go in rendererList)
        {
            Material[] materials;
            SkinnedMeshRenderer renderer = go.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                materials = renderer.sharedMaterials == null ? renderer.materials : renderer.sharedMaterials;
            }
            else
            {
                MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
                materials = meshRenderer.sharedMaterials == null ? meshRenderer.materials : meshRenderer.sharedMaterials;
            }

            //检查
            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];

                if (IsPlayerPart(prefabName, go.name))
                {
                    if (mat != null)
                        _ErrorString += string.Format("{0} player部位的material必须为null! 组件名: {1}", prefabName, go.name);
                    continue;
                }

                if (mat == null)
                {
                    string strMsg = string.Format("{0} material丢失! 组件名: {1} 材质: {2}\n", prefabName, go.name, i);
                    if (!errorSet_MaterialMissing.Contains(strMsg))
                    {
                        errorSet_MaterialMissing.Add(strMsg);
                        errorList_MaterialMissing.Add(strMsg);
                    }
                    continue;
                }
                Shader shader = mat.shader;
                if (shader == null)
                {
                    string strMsg = string.Format("{0} material shader 丢失! 组件名: {1} 材质: {2}\n", prefabName, go.name, i);
                    if (!errorSet_MaterialShaderMissing.Contains(mat.name))
                    {
                        errorSet_MaterialShaderMissing.Add(mat.name);
                        errorList_MaterialShaderMissing.Add(strMsg);
                    }
                    continue;
                }

                if ((!shader.name.Contains("Standard")) && (shader.name.Contains("ErrorShader")))
                {
                    string strMsg = string.Format("{0} material shader 错误! 组件名: {1} 材质: {2}, shader名称: {3}\n", prefabName, go.name, i, shader.name);
                    if (!errorSet_MaterialShaderError.Contains(mat.name))
                    {
                        errorSet_MaterialShaderError.Add(mat.name);
                        errorList_MaterialShaderError.Add(strMsg);
                    }

                    continue;
                }

#if UNITY_EDITOR
                for (int innerIndex = 0; innerIndex < UnityEditor.ShaderUtil.GetPropertyCount(shader); ++innerIndex)
                {
                    if (UnityEditor.ShaderUtil.GetPropertyType(shader, innerIndex) == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        string propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, innerIndex);
                        Texture tex = mat.GetTexture(propertyName);

                        if (tex != null)
                        {
                            if (tex.width > TextureLimit || tex.height > TextureLimit)
                            {
                                string strMsg = string.Format("{0} 贴图大小大于{1}，建议缩小! 组件名: {2} 材质: {3} 宽高: {4} {5} 贴图: {6} \n", prefabName, TextureLimit, go.name, i, tex.width, tex.height, tex.name);

                                //排除允许过大贴图的一些目录
                                bool bSpecial = prefabName.Contains("/Boss/") ||
                                    prefabName.Contains("/Fashion/") ||
                                    (prefabName.Contains("/Outward/") && prefabName.Contains("/equipment/")) ||
                                    prefabName.Contains("/Ride/") ||
                                    prefabName.Contains("/wing/") ||
                                    prefabName.Contains("_create_");

                                if (!bSpecial)
                                {
                                    AddToTextureInfoList(LargeTextureList, prefabName, go.name, tex.name, tex.width, tex.height);

                                    if (!errorSet_TextureLarge.Contains(tex.name))
                                    {
                                        errorSet_TextureLarge.Add(tex.name);
                                        errorList_TextureLarge.Add(strMsg);
                                    }
                                }
                            }

                            if (tex.width != tex.height || !GameDataCheckMan.IsPOT((uint)tex.width))
                            {
                                string strMsg = string.Format("{0} 贴图长宽不相等或不是2的n次幂，无法压缩，建议修改! 组件名: {1} 材质: {2} 宽高: {3} {4} 贴图: {5} \n", prefabName, go.name, i, tex.width, tex.height, tex.name);

                                AddToTextureInfoList(NoStandardTextureList, prefabName, go.name, tex.name, tex.width, tex.height);

                                if (!errorSet_TextureNPT.Contains(tex.name))
                                {
                                    errorSet_TextureNPT.Add(tex.name);
                                    errorList_TextureNPT.Add(strMsg);
                                }
                            }
                        }
                        else
                        {
//                             if (false) //(propertyName == "_MainTex")
//                             {
//                                 string strMsg = string.Format("{0} 贴图为空! 组件名: {1} 材质: {2} 贴图: {3}\n", prefabName, go.name, i, propertyName);
//                                 if (!errorSet_TextureNull.Contains(strMsg))
//                                 {
//                                     errorSet_TextureNull.Add(strMsg);
//                                     errorList_TextureNull.Add(strMsg);
//                                 }
//                             }
                        }
                    }
                }
#endif
            }
        }

        //输出错误信息
        foreach (var str in errorList_MaterialMissing)
        {
            _ErrorString += str;
        }

        foreach (var str in errorList_MaterialShaderMissing)
        {
            _ErrorString += str;
        }

        foreach (var str in errorList_MaterialShaderError)
        {
            _ErrorString += str;
        }

        foreach (var str in errorList_TextureLarge)
        {
            _ErrorString += str;
        }

        foreach (var str in errorList_TextureNPT)
        {
            _ErrorString += str;
        }

        foreach (var str in errorList_TextureNull)
        {
            _ErrorString += str;
        }

        yield return null;
    }
}


