using System;
using Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Template;
using Object = UnityEngine.Object;


public class SfxResourceCheck : Singleton<SfxResourceCheck>
{
    public List<string> _AssetPathList = new List<string>();

    private int TextureLimit = 512;

    public void Init(int textureLimit)
    {
        TextureLimit = textureLimit;

        _AssetPathList = AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("sfx");
    }

    public string _ErrorString = "";

    //
    public IEnumerable DoCheckParticleSystemCoroutine(string prefabName, GameObject sfxPrefab)
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
        CUnityUtil.FindChildLeaf(typeof(ParticleSystem), typeof(MeshRenderer), sfxPrefab, rendererList);

        foreach (var go in rendererList)
        {
            Material[] materials = null;
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer != null && renderer.enabled)
                    materials = renderer.sharedMaterials == null ? renderer.materials : renderer.sharedMaterials;
            }
            else
            {
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null && renderer.enabled)
                    materials = renderer.sharedMaterials == null ? renderer.materials : renderer.sharedMaterials;
            }

            if (materials == null)
                continue;

            //检查
            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                {
                    if (i == 0)
                    {
                        string strMsg = string.Format("{0} material丢失! prefab: {1}\n", prefabName, go.name);
                        if (!errorSet_MaterialMissing.Contains(strMsg))
                        {
                            errorSet_MaterialMissing.Add(strMsg);
                            errorList_MaterialMissing.Add(strMsg);
                        }
                    }
                    continue;
                }
                Shader shader = mat.shader;
                if (shader == null)
                {
                    string strMsg = string.Format("{0} material shader 丢失! prefab: {1}\n", prefabName, go.name);
                    if (!errorSet_MaterialShaderMissing.Contains(mat.name))
                    {
                        errorSet_MaterialShaderMissing.Add(mat.name);
                        errorList_MaterialShaderMissing.Add(strMsg);
                    }
                    continue;
                }

                if ((!shader.name.Contains("Standard")) && (shader.name.Contains("ErrorShader")))
                {
                    string strMsg = string.Format("{0} material shader 错误! prefab: {1} shader名称: {2}\n", prefabName, go.name, shader.name);
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
                                string strMsg = string.Format("{0} 贴图大小大于{1}，建议缩小! prefab: {2} 宽高: {3} {4} 贴图: {5} \n", prefabName, TextureLimit, go.name, tex.width, tex.height, tex.name);
                                if (!errorSet_TextureLarge.Contains(tex.name))
                                {
                                    errorSet_TextureLarge.Add(tex.name);
                                    errorList_TextureLarge.Add(strMsg);
                                }
                            }

                            if (tex.width != tex.height || !GameDataCheckMan.IsPOT((uint)tex.width))
                            {
                                string strMsg = string.Format("{0} 贴图长宽不相等或不是2的n次幂，无法压缩，建议修改! prefab: {1} 材质: {2} 宽高: {3} {4} 贴图: {5} \n", prefabName, go.name, innerIndex, tex.width, tex.height, tex.name);
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
//                                 string strMsg = string.Format("{0} 贴图为空! prefab: {1} 贴图: {3}\n", prefabName, go.name, propertyName);
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


        foreach (var go in rendererList)
        {
            Material[] materials = null;
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps == null || ps.shape.shapeType != ParticleSystemShapeType.MeshRenderer)
                continue;

            if (ps.shape.meshRenderer != null)
                materials = ps.shape.meshRenderer.sharedMaterials == null ? ps.shape.meshRenderer.materials : ps.shape.meshRenderer.sharedMaterials;

            if (materials == null)
                continue;

            //检查
            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                {
                    if (i == 0)
                    {
                        string strMsg = string.Format("{0} ParticleSystem.shape.meshRenderer material丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
                        if (!errorSet_MaterialMissing.Contains(strMsg))
                        {
                            errorSet_MaterialMissing.Add(strMsg);
                            errorList_MaterialMissing.Add(strMsg);
                        }
                    }
                    continue;
                }
                Shader shader = mat.shader;
                if (shader == null)
                {
                    string strMsg = string.Format("{0} ParticleSystem.shape.meshRenderer material shader 丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
                    if (!errorSet_MaterialShaderMissing.Contains(mat.name))
                    {
                        errorSet_MaterialShaderMissing.Add(mat.name);
                        errorList_MaterialShaderMissing.Add(strMsg);
                    }
                    continue;
                }

                if ((!shader.name.Contains("Standard")) && (shader.name.Contains("ErrorShader")))
                {
                    string strMsg = string.Format("{0} ParticleSystem.shape.meshRenderer material shader 错误! prefab: {1} 材质: {2}, shader名称: {3}\n", prefabName, go.name, i, shader.name);
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
                                string strMsg = string.Format("{0} 贴图大小大于{1}，建议缩小! prefab: {2} 材质: {3} 宽高: {4} {5} 贴图: {6} \n", prefabName, TextureLimit, go.name, i, tex.width, tex.height, tex.name);
                                if (!errorSet_TextureLarge.Contains(tex.name))
                                {
                                    errorSet_TextureLarge.Add(tex.name);
                                    errorList_TextureLarge.Add(strMsg);
                                }
                            }

                            if (tex.width != tex.height || !GameDataCheckMan.IsPOT((uint)tex.width))
                            {
                                string strMsg = string.Format("{0} 贴图长宽不相等或不是2的n次幂，无法压缩，建议修改! prefab: {1} 材质: {2} 宽高: {3} {4} 贴图: {5} \n", prefabName, go.name, i, tex.width, tex.height, tex.name);
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
//                                 string strMsg = string.Format("{0} 贴图为空! prefab: {1} 材质: {2} 贴图: {3}\n", prefabName, go.name, i, propertyName);
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

        foreach (var go in rendererList)
        {
            Material[] materials = null;
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps == null || ps.shape.shapeType != ParticleSystemShapeType.SkinnedMeshRenderer)
                continue;

            if (ps.shape.skinnedMeshRenderer != null)
                materials = ps.shape.skinnedMeshRenderer.sharedMaterials == null ? ps.shape.skinnedMeshRenderer.materials : ps.shape.skinnedMeshRenderer.sharedMaterials;

            if (materials == null)
                continue;

            //检查
            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                {
                    if (i == 0)
                    {
                        string strMsg = string.Format("{0} ParticleSystem.shape.skinnedMeshRenderer material丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
                        if (!errorSet_MaterialMissing.Contains(strMsg))
                        {
                            errorSet_MaterialMissing.Add(strMsg);
                            errorList_MaterialMissing.Add(strMsg);
                        }
                    }
                    continue;
                }
                Shader shader = mat.shader;
                if (shader == null)
                {
                    string strMsg = string.Format("{0} ParticleSystem.shape.skinnedMeshRenderer material shader 丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
                    if (!errorSet_MaterialShaderMissing.Contains(mat.name))
                    {
                        errorSet_MaterialShaderMissing.Add(mat.name);
                        errorList_MaterialShaderMissing.Add(strMsg);
                    }
                    continue;
                }

                if ((!shader.name.Contains("Standard")) && (shader.name.Contains("ErrorShader")))
                {
                    string strMsg = string.Format("{0} ParticleSystem.shape.skinnedMeshRenderer material shader 错误! prefab: {1} 材质: {2}, shader名称: {3}\n", prefabName, go.name, i, shader.name);
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
                                string strMsg = string.Format("{0} 贴图大小大于{1}，建议缩小! prefab: {2} 材质: {3} 宽高: {4} {5} 贴图: {6} \n", prefabName, TextureLimit, go.name, i, tex.width, tex.height, tex.name);
                                if (!errorSet_TextureLarge.Contains(tex.name))
                                {
                                    errorSet_TextureLarge.Add(tex.name);
                                    errorList_TextureLarge.Add(strMsg);
                                }
                            }

                            if (tex.width != tex.height || !GameDataCheckMan.IsPOT((uint)tex.width))
                            {
                                string strMsg = string.Format("{0} 贴图长宽不相等或不是2的n次幂，无法压缩，建议修改! prefab: {1} 材质: {2} 宽高: {3} {4} 贴图: {5} \n", prefabName, go.name, i, tex.width, tex.height, tex.name);
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
//                                 string strMsg = string.Format("{0} 贴图为空! prefab: {1} 材质: {2} 贴图: {3}\n", prefabName, go.name, i, propertyName);
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

    public IEnumerable DoCheckFxLOD(string prefabName, GameObject sfxPrefab)
    {
        FxLOD lod = sfxPrefab.GetComponent<FxLOD>();

        if (lod != null)
        {
            bool missing = false;
            GameObject[] objs = lod.allKeyGameObjects;
            for (int i = 0; i < objs.Length; ++i)
            {
                if (objs[i] == null)
                {
                    missing = true;
                }
            }

            if (missing)
                _ErrorString += string.Format("特效包含FOD，但allKeyGameObjects中有GameObject为空! {0}\n", prefabName);
        }

        yield return null;
    }

}
