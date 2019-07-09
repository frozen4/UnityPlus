using System;
using Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Template;
using Object = UnityEngine.Object;


public partial class SceneResourceCheck : Singleton<SceneResourceCheck>
{
    public Dictionary<int, Scene> _SceneData = new Dictionary<int, Scene>();
    private WeatherSystemConfig _WeatherConfig = new WeatherSystemConfig();


    private int TerrainBaseMapSizeLimit = 256;
    private int TextureLimit = 1024;

    public void Init(int terrainBaseMapLimit, int textureLimit)
    {
        TerrainBaseMapSizeLimit = terrainBaseMapLimit;
        TextureLimit = textureLimit;

        //预先设置路径
        Template.Path.BasePath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/");
        Template.Path.BinPath = "Data/";

        var sceneManager = Template.SceneModule.Manager.Instance;
        sceneManager.ParseTemplateAll(true);
        _SceneData = sceneManager.GetTemplateMap();

        string configPath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/Configs");
        string path = System.IO.Path.Combine(configPath, "WeatherConfigXml.xml");
        byte[] bytes = System.IO.File.ReadAllBytes(path);
        bool isReadSuccess = _WeatherConfig.ParseFromXmlString(Encoding.UTF8.GetString(bytes));
        if (!isReadSuccess)
        {
            Debug.LogError("WeatherConfigXml文件读取失败！");
        }
    }

    public string _ErrorString = "";
    //public Dictionary<string, string> _DicErrorString = new Dictionary<string, string>();

    void AddErrorStringFormat(string shortName, string errorInfo, params object[] format)
    {
        string s = string.Format(errorInfo, format);
        _ErrorString += s;
        //_DicErrorString[shortName] += s;
    }

    void AddErrorString(string shortName, string errorInfo)
    {
        _ErrorString += errorInfo;
        //_DicErrorString[shortName] += errorInfo;
    }

    public WeatherData GetEffectByID(int effectId)
    {
        if (_WeatherConfig == null)
            return null;

        for (int i = 0; i < _WeatherConfig.WeatherDataList.Count; i++)
        {
            var data = _WeatherConfig.WeatherDataList[i];
            if (data._DataGuid == effectId)
            {
                return data;
            }
        }
        return null;
    }

    //
    public IEnumerable DoCheckSceneCoroutine(string prefabName, GameObject scenePrefab)
    {
        var _Config = scenePrefab.GetComponent<SceneConfig>();
        if (null == _Config)
        {
            HobaDebuger.LogErrorFormat("{0} Is not a scene prefab.", prefabName);
            yield break;
        }

        foreach (var item in DoCheckTerrains(prefabName, scenePrefab))
            yield return item;

        //加载Objects
        if (_Config._BlockPositionList.Count > 0)           //有block
        {
            foreach (var item in DoCheckSceneConfig(prefabName, _Config))
                yield return item;

            foreach (var item in DoCheckMeshRendererCoroutine(prefabName, _Config))
                yield return item;
        }
        else                     //无block
        {
            foreach (var item in DoCheckSceneConfig(prefabName, _Config))
                yield return item;

            foreach (var item in DoCheckMeshRendererCoroutine(prefabName, scenePrefab))
                yield return item;
        }

        yield return null;
    }

    private IEnumerable DoCheckTerrains(string prefabName, GameObject scenePrefab)
    {
        var _Config = scenePrefab.GetComponent<SceneConfig>();
        if (null == _Config)
        {
            yield break;
        }

        List<TerrainsManager.CTerrainEntry> _CurrentSceneTerrain = new List<TerrainsManager.CTerrainEntry>();

        SceneConfig.LightmapsConfig cacheTerrainConfig = _Config._LightmapConfig;
        int curTerrainCount = cacheTerrainConfig._TerrainLightmapInfos.Length;
        _CurrentSceneTerrain.Clear();
        for (int i = 0; i < curTerrainCount; i++)
        {
            Terrain terrain = cacheTerrainConfig._TerrainLightmapInfos[i]._Terrain;
            if (terrain == null) continue;
            if (terrain.terrainData != null)
            {
                TerrainsManager.CTerrainEntry entry = new TerrainsManager.CTerrainEntry();
                entry.TerrainComp = terrain;
                entry.Position = terrain.transform.position;
                entry.SizeH = (terrain.terrainData.size.x + terrain.terrainData.size.z) * 0.5f;

                _CurrentSceneTerrain.Add(entry);
            }
        }

        //检查terrainData
        if (_CurrentSceneTerrain.Count > 0)         //有分块
        {
            int count = 0;
            int total = _CurrentSceneTerrain.Count;
            foreach (var entry in _CurrentSceneTerrain)
            {
                string name = entry.TerrainComp.name;
                TerrainData terrainData = entry.TerrainComp.terrainData;

                ++count;

                string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);
                GameDataCheckMan.Instance.SetDesc(string.Format("{0} 检查Terrain: {1}", shortName, name));
                GameDataCheckMan.Instance.SetPartProgress((float)count / total);
                yield return null;


                //会导致basemap过大
                if (terrainData.baseMapResolution > TerrainBaseMapSizeLimit)
                {
                    //AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 BaseTextureResolution 为 {2}, 大于 {3}，建议减小到{4}以内!\n", prefabName, name, terrainData.baseMapResolution, TerrainBaseMapSizeLimit, TerrainBaseMapSizeLimit);
                }

                for (int i = 0; i < terrainData.splatPrototypes.Length; ++i)
                {
                    var splat = terrainData.splatPrototypes[i];
                    var tex = splat.texture;

                    if (tex != null)
                    {
                        //if (tex.width > 512 || tex.height > 512)
                        //    _ErrorString += string.Format("{0} terrain 的 splatTexture 大小 大于 512，建议减小到512以内: {1}\n", prefabName, tex.name);

                        bool bValid = true;
                        string errorMsg = "";
                        for (int level = 0; level < tex.mipmapCount; ++level)
                        {
                            if (!CUnityUtil.CanGetPixel32(tex, level, out errorMsg))
                            {
                                bValid = false;
                                break;
                            }
                        }

                        if (!bValid)
                            AddErrorStringFormat(shortName, "{0} 的 terrain {1}  的 splatTexture {2} 无法读取像素，请确保Texture开启read/write权限: {3}, Message:{4}\n", prefabName, name, i, tex.name, errorMsg);
                        //else
                        //    _ErrorString += string.Format("{0} terrain 的 splatTexture {1} 正常读取像素: {2}\n", prefabName, i, tex.name);
                    }
                    else
                    {
                        AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 splatTexture {2} 为空！\n", prefabName, name, i);
                    }
                }

                if (terrainData.alphamapLayers > 4)
                {
                    AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 alphamapLayers 大于 4，必须减小到4以内!\n", prefabName, name);
                }

                if (terrainData.alphamapResolution > TerrainBaseMapSizeLimit)
                {
                    //AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 ControlTextureResolution 为{2}, 大于 {3}，建议减小到 {4}以内!\n", prefabName, name, terrainData.alphamapResolution, TerrainBaseMapSizeLimit, TerrainBaseMapSizeLimit);
                }

                for (int i = 0; i < terrainData.alphamapTextures.Length; ++i)
                {
                    var tex = terrainData.alphamapTextures[i];
                    if (tex != null)
                    {
                        //if (tex.width > TerrainBaseMapSizeLimit || tex.height > TerrainBaseMapSizeLimit)
                        //    _ErrorString += string.Format("{0} 的 terrain {1} 的 alphamapTexture 长宽 大于 {2}，建议减小到{3}以内: 宽:{4} 高:{5} {6}\n", prefabName, name, TerrainBaseMapSizeLimit, TerrainBaseMapSizeLimit, tex.width, tex.height, tex.name);
                    }
                    else
                    {
                        AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 alphamapTexture {2} 为空！\n", prefabName, name, i);
                    }
                }

                if (terrainData.detailPrototypes != null && terrainData.detailPrototypes.Length > 0)
                {
                    AddErrorStringFormat(shortName, "{0} 的 terrain {1} 包含detailMap(草地??) 个数: {2}，建议去掉!\n", prefabName, name, terrainData.detailPrototypes.Length);
                }
            }
        }
        else
        {
            List<GameObject> rendererList = new List<GameObject>();
            CUnityUtil.FindChildLeaf(typeof(Terrain), scenePrefab, rendererList);

            //检查terrainData
            int count = 0;
            int total = rendererList.Count;
            foreach (var go in rendererList)
            {
                Terrain terrain = go.GetComponent<Terrain>();

                string name = terrain.name;
                TerrainData terrainData = terrain.terrainData;

                ++count;

                string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);
                GameDataCheckMan.Instance.SetDesc(string.Format("{0} 检查Terrain: {1}", shortName, name));
                GameDataCheckMan.Instance.SetPartProgress((float)count / total);
                yield return null;

                //会导致basemap过大
                if (terrainData.baseMapResolution > TerrainBaseMapSizeLimit)
                {
                    //AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 BaseTextureResolution 大于 {2}，建议减小到{3}以内!\n", prefabName, name, TerrainBaseMapSizeLimit, TerrainBaseMapSizeLimit);
                }

                for (int i = 0; i < terrainData.splatPrototypes.Length; ++i)
                {
                    var splat = terrainData.splatPrototypes[i];
                    var tex = splat.texture;
                    if (tex != null)
                    {
                        //if (tex.width > 512 || tex.height > 512)
                        //    _ErrorString += string.Format("{0} terrain 的 splatTexture 大小 大于 512，建议减小到512以内: {1}\n", prefabName, tex.name);

                        bool bValid = true;
                        string errorMsg = "";
                        for (int level = 0; level < tex.mipmapCount; ++level)
                        {
                            if (!CUnityUtil.CanGetPixel32(tex, level, out errorMsg))
                            {
                                bValid = false;
                                break;
                            }
                        }

                        if (!bValid)
                            AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 splatTexture {2} 无法读取像素，请确保Texture开启read/write权限: {3}, Message: {4}\n", prefabName, name, i, tex.name, errorMsg);
                        //else
                        //    _ErrorString += string.Format("{0} terrain 的 splatTexture {1} 正常读取像素: {2}\n", prefabName, i, tex.name);
                    }
                    else
                    {
                        AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 splatTexture {2} 为空！\n", prefabName, name, i);
                    }
                }

                if (terrainData.alphamapLayers > 4)
                {
                    AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 alphamapLayers 大于 4，必须减小到4以内!\n", prefabName, name);
                }

                if (terrainData.alphamapResolution > TerrainBaseMapSizeLimit)
                {
                    //AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 ControlTextureResolution 大于 {2}，建议减小到{3}以内!\n", prefabName, name, TerrainBaseMapSizeLimit, TerrainBaseMapSizeLimit);
                }

                for (int i = 0; i < terrainData.alphamapTextures.Length; ++i)
                {
                    var tex = terrainData.alphamapTextures[i];
                    if (tex != null)
                    {
                        //if (tex.width > TerrainBaseMapSizeLimit || tex.height > TerrainBaseMapSizeLimit)
                        //    _ErrorString += string.Format("{0} terrain 的 alphamapTextures大小 大于 {1}，建议减小到{2}以内: {3}\n", prefabName, TerrainBaseMapSizeLimit, TerrainBaseMapSizeLimit, tex.name);
                    }
                    else
                    {
                        AddErrorStringFormat(shortName, "{0} 的 terrain {1} 的 alphamapTexture {2} 为空！\n", prefabName, name, i);
                    }
                }

                if (terrainData.detailPrototypes != null && terrainData.detailPrototypes.Length > 0)
                {
                    AddErrorStringFormat(shortName, "{0} 的 terrain {1} 包含detailMap(草地??) 个数: {2}，建议去掉!\n", prefabName, name, terrainData.detailPrototypes.Length);
                }
            }
        }

        yield return null;
    }

    private IEnumerable DoCheckSceneConfig(string prefabName, SceneConfig config)
    {
        string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);

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

        int count = 0;
        int total = config._RegionIdGroup.Count;
        for (int regionId = 0; regionId < config._RegionIdGroup.Count; ++regionId)
        {
            var regionConfig = config._RegionIdGroup[regionId];

            ++count;

            GameDataCheckMan.Instance.SetDesc(string.Format("{0} 检查Region Config: {1}", shortName, regionId));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            int[] idArray = new int[] { regionConfig.DayID, regionConfig.DuskID, regionConfig.MorningID, regionConfig.NightID };

            foreach (int id in idArray)
            {
                var data = GetEffectByID(id);
                if (data == null)
                    continue;

                var cubeMap = AssetBundleCheck.Instance.LoadAsset(data._SkyConifg._CubeMapPath) as Cubemap;
                if (cubeMap == null)
                {
                    if (!string.IsNullOrEmpty(data._SkyConifg._CubeMapPath))
                        AddErrorStringFormat(shortName, "{0} sceneConfig 的SKYBOX cubemap加载失败, 区域Id: {1}, CubeMapPath: {2}\n", prefabName, regionId, data._SkyConifg._CubeMapPath);
                }
                else
                {
                    var tex = cubeMap;
                    if (tex.width > TextureLimit || tex.height > TextureLimit)
                    {
                        string strMsg = string.Format("{0} 的SKYBOX CubeMap贴图大小大于{1}，建议缩小! 宽高: {2} {3} 贴图: {4} \n", prefabName, TextureLimit, tex.width, tex.height, tex.name);
                        if (!errorSet_TextureLarge.Contains(tex.name))
                        {
                            errorSet_TextureLarge.Add(tex.name);
                            errorList_TextureLarge.Add(strMsg);
                        }
                    }

                    if (tex.width != tex.height || !GameDataCheckMan.IsPOT((uint)tex.width))
                    {
                        string strMsg = string.Format("{0} 的SKYBOX CubeMap贴图长宽不相等或不是2的n次幂，无法压缩，建议修改! 宽高: {1} {2} 贴图: {3} \n", prefabName, tex.width, tex.height, tex.name);
                        if (!errorSet_TextureNPT.Contains(tex.name))
                        {
                            errorSet_TextureNPT.Add(tex.name);
                            errorList_TextureNPT.Add(strMsg);
                        }
                    }

                }

                var asset = AssetBundleCheck.Instance.LoadAsset(data._SkyBoxMatPath);
                if (asset != null)
                {
                    Material mat = GameObject.Instantiate(asset) as Material;
                    if (mat == null)
                    {
                        string strMsg = string.Format("{0} 的SKYBOX material丢失! 材质: {1}\n", prefabName, data._SkyBoxMatPath);
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
                        string strMsg = string.Format("{0} 的SKYBOX material shader 丢失! 材质: {1}\n", prefabName, data._SkyBoxMatPath);
                        if (!errorSet_MaterialShaderMissing.Contains(mat.name))
                        {
                            errorSet_MaterialShaderMissing.Add(mat.name);
                            errorList_MaterialShaderMissing.Add(strMsg);
                        }
                        continue;
                    }

                    if ((!shader.name.Contains("Standard")) && (shader.name.Contains("ErrorShader")))
                    {
                        string strMsg = string.Format("{0} 的SKYBOX material shader 错误! 材质: {1}, shader名称: {2}\n", prefabName, data._SkyBoxMatPath, shader.name);
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
                                    string strMsg = string.Format("{0} 的SKYBOX贴图大小大于{1}，建议缩小! 宽高: {2} {3} 贴图: {4} \n", prefabName, TextureLimit, tex.width, tex.height, tex.name);
                                    if (!errorSet_TextureLarge.Contains(tex.name))
                                    {
                                        errorSet_TextureLarge.Add(tex.name);
                                        errorList_TextureLarge.Add(strMsg);
                                    }
                                }

                                if (tex.width != tex.height || !GameDataCheckMan.IsPOT((uint)tex.width))
                                {
                                    string strMsg = string.Format("{0} 的SKYBOX贴图长宽不相等或不是2的n次幂，无法压缩，建议修改! 宽高: {1} {2} 贴图: {3} \n", prefabName, tex.width, tex.height, tex.name);
                                    if (!errorSet_TextureNPT.Contains(tex.name))
                                    {
                                        errorSet_TextureNPT.Add(tex.name);
                                        errorList_TextureNPT.Add(strMsg);
                                    }
                                }
                            }
                        }
                    }
#endif
                }
            }
        }

        //输出错误信息
        foreach (var str in errorList_MaterialMissing)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_MaterialShaderMissing)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_MaterialShaderError)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureLarge)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureNPT)
        {
            AddErrorString(shortName, str);
        }

        yield return null;
    }

    private IEnumerable DoCheckMeshRendererCoroutine(string prefabName, SceneConfig config)
    {
        string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);

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

        Transform blockRootTran = config.transform.Find("BlockObjects");
        if (null == blockRootTran)
        {
            blockRootTran = config.transform;
        }

        int count = 0;
        int total = config._BlockPositionList.Count;
        foreach (SceneConfig.CPositionSetting currentPos in config._BlockPositionList)
        {
            ++count;

            string blockName = System.IO.Path.GetFileName(currentPos._BlockName);
            GameDataCheckMan.Instance.SetDesc(string.Format("{0} 检查Obj: {1}", shortName, blockName));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            var asset = AssetBundleCheck.Instance.LoadAsset(currentPos._BlockName);
            if (asset == null)
            {
                AddErrorStringFormat(shortName, "terrain加载Object错误: {0}\n", currentPos._BlockName);
                continue;
            }

            //创建
            //TextLogger.Instance.WriteLine(string.Format("Checking Scene Object: {0}", currentPos._BlockName));
            var obj = GameObject.Instantiate(asset) as GameObject;
            obj.transform.parent = blockRootTran;

            List<GameObject> rendererList = new List<GameObject>();
            CUnityUtil.FindChildLeaf(typeof(MeshRenderer), obj, rendererList);

            //检查每个包含MeshRenderer的go
            foreach (var go in rendererList)
            {
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                Material[] materials = renderer.sharedMaterials == null ? renderer.materials : renderer.sharedMaterials;

                //检查
                for (int i = 0; i < materials.Length; ++i)
                {
                    Material mat = materials[i];
                    if (mat == null)
                    {
                        string strMsg = string.Format("{0} material丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
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
                        string strMsg = string.Format("{0} material shader 丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
                        if (!errorSet_MaterialShaderMissing.Contains(mat.name))
                        {
                            errorSet_MaterialShaderMissing.Add(mat.name);
                            errorList_MaterialShaderMissing.Add(strMsg);
                        }
                        continue;
                    }

                    if ((!shader.name.Contains("Standard")) && (shader.name.Contains("ErrorShader")))
                    {
                        string strMsg = string.Format("{0} material shader 错误! prefab: {1} 材质: {2}, shader名称: {3}\n", prefabName, go.name, i, shader.name);
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
//                                 if (false) //(propertyName == "_MainTex")
//                                 {
//                                     string strMsg = string.Format("{0} 贴图为空! prefab: {1} 材质: {2} 贴图: {3}\n", prefabName, go.name, i, propertyName);
//                                     if (!errorSet_TextureNull.Contains(strMsg))
//                                     {
//                                         errorSet_TextureNull.Add(strMsg);
//                                         errorList_TextureNull.Add(strMsg);
//                                     }
//                                 }
                            }
                        }
                    }
#endif
                }
            }

            GameObject.DestroyImmediate(obj);
        }

        //输出错误信息
        foreach (var str in errorList_MaterialMissing)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_MaterialShaderMissing)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_MaterialShaderError)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureLarge)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureNPT)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureNull)
        {
            AddErrorString(shortName, str);
        }

        yield return null;
    }

    IEnumerable DoCheckMeshRendererCoroutine(string prefabName, GameObject scenePrefab)
    {
        string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);

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
        CUnityUtil.FindChildLeaf(typeof(MeshRenderer), scenePrefab, rendererList);

        //检查每个包含MeshRenderer的go
        foreach (var go in rendererList)
        {
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            Material[] materials = renderer.sharedMaterials == null ? renderer.materials : renderer.sharedMaterials;

            //检查
            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                {
                    string strMsg = string.Format("{0} material丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
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
                    string strMsg = string.Format("{0} material shader 丢失! prefab: {1} 材质: {2}\n", prefabName, go.name, i);
                    if (!errorSet_MaterialShaderMissing.Contains(mat.name))
                    {
                        errorSet_MaterialShaderMissing.Add(mat.name);
                        errorList_MaterialShaderMissing.Add(strMsg);
                    }
                    continue;
                }

                if ((!shader.name.Contains("Standard")) && (shader.name.Contains("ErrorShader")))
                {
                    string strMsg = string.Format("{0} material shader 错误! prefab: {1} 材质: {2}, shader名称: {3}\n", prefabName, go.name, i, shader.name);
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
//                             if (false) //if (propertyName == "_MainTex")
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
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_MaterialShaderMissing)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_MaterialShaderError)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureLarge)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureNPT)
        {
            AddErrorString(shortName, str);
        }

        foreach (var str in errorList_TextureNull)
        {
            AddErrorString(shortName, str);
        }

        yield return null;
    }


}
