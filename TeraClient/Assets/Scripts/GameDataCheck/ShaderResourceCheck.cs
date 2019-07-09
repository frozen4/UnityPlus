using System;
using Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Template;
using Object = UnityEngine.Object;

//搜集 Scene, Character, Sfx使用的shader 和 keyword
public class ShaderResourceCheck : Singleton<ShaderResourceCheck>
{
    public Dictionary<int, Scene> _SceneData = new Dictionary<int, Scene>();
    private WeatherSystemConfig _WeatherConfig = new WeatherSystemConfig();

    public List<string> _CharacterAssetPathList = new List<string>();

    public List<string> _SfxAssetPathList = new List<string>();

    //需要忽略的player的几个部位
    public string[] _IgnoredPlayerPrefabs = { 
                                                "alipriest_f.prefab", 
                                                "casassassin_m.prefab",
                                                "humwarrior_m.prefab",
                                                "sprarcher_f.prefab",
                                            };
    public string[] _IgnoredParts = { "body", "face", "hair", };

    public class CShaderEntry
    {
        public List<List<string>> keywordList = new List<List<string>>();

        public bool Contains(List<string> list)
        {
            for (int i = 0; i < keywordList.Count; ++i)
            {
                List<string> list2 = keywordList[i];

                bool bEqual = true;
                if (list2.Count != list.Count)
                {
                    bEqual = false;
                }
                else
                {
                    for(int n = 0; n < list2.Count; ++n)
                    {
                        if (list2[n] != list[n])
                        {
                            bEqual = false;
                            break;
                        }
                    }
                }

                if (bEqual)
                    return true;
            }
            return false;
        }
    }

    public SortedDictionary<string, CShaderEntry> _SceneShaderDic = new SortedDictionary<string, CShaderEntry>();
    public SortedDictionary<string, CShaderEntry> _CharacterShaderDic = new SortedDictionary<string, CShaderEntry>();
    public SortedDictionary<string, CShaderEntry> _SfxShaderDic = new SortedDictionary<string, CShaderEntry>();
    public SortedDictionary<string, CShaderEntry> _AllShaderDic = new SortedDictionary<string, CShaderEntry>();

    public class CUsedShaderEntry
    {
        public string shaderName = string.Empty;
        public string matName = string.Empty;
        public string goName = string.Empty;
        public string prafabName = string.Empty;
    }

    public List<CUsedShaderEntry> _UsedShaders_Standard = new List<CUsedShaderEntry>();
    public List<CUsedShaderEntry> _UsedShaders_NoUsedGrabPass = new List<CUsedShaderEntry>();
    public List<CUsedShaderEntry> _UsedShaders_KriptoFXParticle = new List<CUsedShaderEntry>();
    public List<CUsedShaderEntry> _UsedShaders_Distortion_NoL3 = new List<CUsedShaderEntry>();
    public List<CUsedShaderEntry> _UsedShaders_Distortion_L3 = new List<CUsedShaderEntry>();
    public List<CUsedShaderEntry> _UsedShaders_DepthTexture = new List<CUsedShaderEntry>();
    public List<CUsedShaderEntry> _UsedShaders_CutoutBorder = new List<CUsedShaderEntry>();
    public List<CUsedShaderEntry> _UsedShaders_NoOutput = new List<CUsedShaderEntry>();

    public void Init()
    {
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

        _CharacterAssetPathList.Clear();
        _CharacterAssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("characters"));
        _CharacterAssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("outward"));
        _CharacterAssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("monsters"));

        _SfxAssetPathList.Clear();
        _SfxAssetPathList.AddRange(AssetBundleCheck.Instance.GetAllAssetNamesOfBundle("sfx"));
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

    public void ClearAllShaderList()
    {
        _SceneShaderDic.Clear();
        _CharacterShaderDic.Clear();
        _SfxShaderDic.Clear();
        _AllShaderDic.Clear();

        _UsedShaders_Standard.Clear();
        _UsedShaders_NoUsedGrabPass.Clear();
        _UsedShaders_KriptoFXParticle.Clear();
        _UsedShaders_Distortion_L3.Clear();
        _UsedShaders_Distortion_NoL3.Clear();
    }

    bool ContainsKeyword(string[] keywords, string kw)
    {
        for (int i = 0; i < keywords.Length; ++i)
        {
            if (keywords[i] == kw)
                return true;
        }
        return false;
    }

    void AddSceneShader(string name, string[] keywords)
    {

        CShaderEntry entry;
        if (!_SceneShaderDic.TryGetValue(name, out entry))
        {
            entry = new CShaderEntry();
            _SceneShaderDic.Add(name, entry);
        }
        if (keywords.Length >= 0)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < keywords.Length; ++i)
            {
                if (!IsShaderFilterd(name, keywords[i]))
                    list.Add(keywords[i]);
            }
            if (list.Count > 0)
            {
                list.Sort();
                if (!entry.Contains(list))
                    entry.keywordList.Add(list);
            }
        }

        AddAllShader(name, keywords);
    }

    void AddCharacterShader(string name, string[] keywords)
    {
        CShaderEntry entry;
        if (!_CharacterShaderDic.TryGetValue(name, out entry))
        {
            entry = new CShaderEntry();
            _CharacterShaderDic.Add(name, entry);
        }
        if (keywords.Length >= 0)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < keywords.Length; ++i)
            {
                if (!IsShaderFilterd(name, keywords[i]))
                    list.Add(keywords[i]);
            }
            if (list.Count > 0)
            {
                list.Sort();
                if (!entry.Contains(list))
                    entry.keywordList.Add(list);
            }
        }

        AddAllShader(name, keywords);
    }

    void AddSfxShader(string name, string[] keywords)
    {

        CShaderEntry entry;
        if (!_SfxShaderDic.TryGetValue(name, out entry))
        {
            entry = new CShaderEntry();
            _SfxShaderDic.Add(name, entry);
        }
        if (keywords.Length >= 0)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < keywords.Length; ++i)
            {
                if (!IsShaderFilterd(name, keywords[i]))
                    list.Add(keywords[i]);
            }
            if (list.Count > 0)
            {
                list.Sort();
                if (!entry.Contains(list))
                    entry.keywordList.Add(list);
            }
        }

        AddAllShader(name, keywords);

    }

    void AddAllShader(string name, string[] keywords)
    {
        CShaderEntry entry;
        if (!_AllShaderDic.TryGetValue(name, out entry))
        {
            entry = new CShaderEntry();
            _AllShaderDic.Add(name, entry);
        }
        if (keywords.Length >= 0)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < keywords.Length; ++i)
            {
                if (!IsShaderFilterd(name, keywords[i]))
                    list.Add(keywords[i]);
            }
            if (list.Count > 0)
            {
                list.Sort();
                if (!entry.Contains(list))
                    entry.keywordList.Add(list);
            }
        }
    }

    public bool IsPlayerPart(string prefabName, string partName)
    {
        foreach (string playerPrefab in _IgnoredPlayerPrefabs)
        {
            if (prefabName.Contains(playerPrefab))
            {
                foreach (string part in _IgnoredParts)
                {
                    if (part == partName)
                        return true;
                }
            }
        }
        return false;
    }

    public IEnumerable DoCheckSceneCoroutine(string prefabName, GameObject go)
    {
        var _Config = go.GetComponent<SceneConfig>();
        if (null == _Config)
        {
            HobaDebuger.LogErrorFormat("{0} Is not a scene prefab.", prefabName);
            yield break;
        }

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

            foreach (var item in DoCheckMeshRendererCoroutine(prefabName, go))
                yield return item;
        }

        yield return null;
    }

    public IEnumerable DoCheckCharacterCoroutine(string prefabName, GameObject char_go)
    {
        List<GameObject> rendererList = new List<GameObject>();
        CUnityUtil.FindChildLeaf(typeof(SkinnedMeshRenderer), typeof(MeshRenderer), char_go, rendererList);

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

            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];

                if (IsPlayerPart(prefabName, go.name))
                    continue;

                if (mat == null)
                    continue;

                Shader shader = mat.shader;
                if (shader == null)
                    continue;

                AddCharacterShader(shader.name, mat.shaderKeywords);

                //
                CheckShader_Standard(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoUsed_GrabPass(shader.name, mat.name, go.name, prefabName);
                CheckShader_KriptoFXParticle(shader.name, mat.name, go.name, prefabName, mat.shaderKeywords);
                CheckShader_Distortion(shader.name, mat.name, go.name, prefabName, false);
                CheckShader_DepthTexture(shader.name, mat.name, go.name, prefabName);
                CheckShader_CutoutBorder(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoOutput(shader.name, mat.name, go.name, prefabName);
            }
        }

        yield return null;
    }

    public IEnumerable DoCheckSfxCoroutine(string prefabName, GameObject sfx_go)
    {
        List<GameObject> rendererList = new List<GameObject>();
        CUnityUtil.FindChildLeaf(typeof(ParticleSystem), typeof(MeshRenderer), sfx_go, rendererList);

        bool hasLOD = sfx_go.GetComponent<FxLOD>();

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

            //是否为L3 GameObject
            bool isL3 = hasLOD && CUnityUtil.HasNameInParent(go, sfx_go, "_L3");

            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                    continue;

                Shader shader = mat.shader;
                if (shader == null)
                    continue;

                AddSfxShader(shader.name, mat.shaderKeywords);

                //
                CheckShader_Standard(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoUsed_GrabPass(shader.name, mat.name, go.name, prefabName);
                CheckShader_KriptoFXParticle(shader.name, mat.name, go.name, prefabName, mat.shaderKeywords);
                CheckShader_Distortion(shader.name, mat.name, go.name, prefabName, isL3);
                CheckShader_DepthTexture(shader.name, mat.name, go.name, prefabName);
                CheckShader_CutoutBorder(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoOutput(shader.name, mat.name, go.name, prefabName);
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

            //是否为L3 GameObject
            bool isL3 = hasLOD && CUnityUtil.HasNameInParent(go, sfx_go, "_L3");

            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                    continue;

                Shader shader = mat.shader;
                if (shader == null)
                    continue;

                AddSfxShader(shader.name, mat.shaderKeywords);
                //
                CheckShader_Standard(shader.name + " [ps.shader.meshRenderer]", mat.name, go.name, prefabName);
                CheckShader_NoUsed_GrabPass(shader.name, mat.name, go.name, prefabName);
                CheckShader_KriptoFXParticle(shader.name, mat.name, go.name, prefabName, mat.shaderKeywords);
                CheckShader_Distortion(shader.name, mat.name, go.name, prefabName, isL3);
                CheckShader_DepthTexture(shader.name, mat.name, go.name, prefabName);
                CheckShader_CutoutBorder(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoOutput(shader.name, mat.name, go.name, prefabName);
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

            //是否为L3 GameObject
            bool isL3 = hasLOD && CUnityUtil.HasNameInParent(go, sfx_go, "_L3");

            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                    continue;

                Shader shader = mat.shader;
                if (shader == null)
                    continue;

                AddSfxShader(shader.name, mat.shaderKeywords);

                //
                CheckShader_Standard(shader.name + " ps.shape.skinnedMeshRenderer", mat.name, go.name, prefabName);
                CheckShader_NoUsed_GrabPass(shader.name, mat.name, go.name, prefabName);
                CheckShader_KriptoFXParticle(shader.name, mat.name, go.name, prefabName, mat.shaderKeywords);
                CheckShader_Distortion(shader.name, mat.name, go.name, prefabName, isL3);
                CheckShader_DepthTexture(shader.name, mat.name, go.name, prefabName);
                CheckShader_CutoutBorder(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoOutput(shader.name, mat.name, go.name, prefabName);
            }
        }

        yield return null;
    }

    private IEnumerable DoCheckSceneConfig(string prefabName, SceneConfig config)
    {
        string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);

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

                var asset = AssetBundleCheck.Instance.LoadAsset(data._SkyBoxMatPath);
                if (asset == null)
                    continue;

                Material mat = GameObject.Instantiate(asset) as Material;
                if (mat == null)
                    continue;

                Shader shader = mat.shader;
                if (shader == null)
                    continue;

                AddSceneShader(shader.name, mat.shaderKeywords);

                //
                CheckShader_Standard(shader.name, mat.name, asset.name, prefabName);
                CheckShader_NoUsed_GrabPass(shader.name, mat.name, asset.name, prefabName);
                CheckShader_KriptoFXParticle(shader.name, mat.name, asset.name, prefabName, mat.shaderKeywords);
                CheckShader_Distortion(shader.name, mat.name, asset.name, prefabName, false);
                CheckShader_DepthTexture(shader.name, mat.name, asset.name, prefabName);
                CheckShader_CutoutBorder(shader.name, mat.name, asset.name, prefabName);
                CheckShader_NoOutput(shader.name, mat.name, asset.name, prefabName);
            }
        }

        yield return null;
    }

    private IEnumerable DoCheckMeshRendererCoroutine(string prefabName, SceneConfig config)
    {
        string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);

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
                continue;

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

                for (int i = 0; i < materials.Length; ++i)
                {
                    Material mat = materials[i];
                    if (mat == null)
                        continue;

                    Shader shader = mat.shader;
                    if (shader == null)
                        continue;

                    AddSceneShader(shader.name, mat.shaderKeywords);

                    //
                    CheckShader_Standard(shader.name, mat.name, go.name, prefabName);
                    CheckShader_NoUsed_GrabPass(shader.name, mat.name, go.name, prefabName);
                    CheckShader_KriptoFXParticle(shader.name, mat.name, go.name, prefabName, mat.shaderKeywords);
                    CheckShader_Distortion(shader.name, mat.name, go.name, prefabName, false);
                    CheckShader_DepthTexture(shader.name, mat.name, go.name, prefabName);
                    CheckShader_CutoutBorder(shader.name, mat.name, go.name, prefabName);
                    CheckShader_NoOutput(shader.name, mat.name, go.name, prefabName);
                }
            }

            GameObject.DestroyImmediate(obj);
        }
    }

    IEnumerable DoCheckMeshRendererCoroutine(string prefabName, GameObject scenePrefab)
    {
        //string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);

        List<GameObject> rendererList = new List<GameObject>();
        CUnityUtil.FindChildLeaf(typeof(MeshRenderer), scenePrefab, rendererList);

        foreach (var go in rendererList)
        {
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            Material[] materials = renderer.sharedMaterials == null ? renderer.materials : renderer.sharedMaterials;

            //检查
            for (int i = 0; i < materials.Length; ++i)
            {
                Material mat = materials[i];
                if (mat == null)
                    continue;

                Shader shader = mat.shader;
                if (shader == null)
                    continue;

                AddSceneShader(shader.name, mat.shaderKeywords);

                //
                CheckShader_Standard(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoUsed_GrabPass(shader.name, mat.name, go.name, prefabName);
                CheckShader_KriptoFXParticle(shader.name, mat.name, go.name, prefabName, mat.shaderKeywords);
                CheckShader_Distortion(shader.name, mat.name, go.name, prefabName, false);
                CheckShader_DepthTexture(shader.name, mat.name, go.name, prefabName);
                CheckShader_CutoutBorder(shader.name, mat.name, go.name, prefabName);
                CheckShader_NoOutput(shader.name, mat.name, go.name, prefabName);
            }
        }

        yield return null;
    }

    public static bool IsShaderFilterd(string name, string kw)
    {
        //跳过特殊的无用keyword
        if (name != "KriptoFX/RFX4/Particle")
        {
            if (kw == "BlendAdd" || kw == "BlendAlpha" || kw == "BlendMul" || kw == "BlendMul2")
                return true;

            if (kw == "Clip_OFF" || kw == "Clip_ON" || kw == "Clip_ON_Alpha")
                return true;
        }

        if (kw == "FrameBlend_OFF" || kw == "FrameBlend_ON")
            return true;

        if (kw == "VertLight_OFF" || kw == "VertLight4_ON" || kw == "VertLight4Normal_ON")
            return true;

        if (kw == "_USEREFLECTIONS_ON" || kw == "_ENABLEREFLECTIONS_ON" || kw == "_ENABLECUSTOMFOG_ON")
            return true;


        if (kw == "_INVERT_ON" || kw == "HERO_COLOR_ADD_ON")
            return true;

        //standard
        if (kw == "_NORMALMAP" || kw == "_SPECGLOSSMAP" || kw == "_ALPHAPREMULTIPLY_ON" ||
            kw == "_METALLICGLOSSMAP" || kw == "_EMISSION" || kw == "_ALPHATEST_ON")
            return true;

        if (kw == "SoftParticles_OFF" || kw == "SoftParticles_ON")
            return true;



        return false;
    }

    private bool CheckShader_Standard(string shaderName, string matName, string goName, string prefabName)
    {
        if (shaderName == "Standard")
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            _UsedShaders_Standard.Add(entry);
            return true;
        }
        return false;
    }

    private bool CheckShader_NoUsed_GrabPass(string shaderName, string matName, string goName, string prefabName)
    {
        if (shaderName == "AQUAS/Camera Effects/Under Water" ||
            shaderName == "AQUAS/Camera Effects/Wet Lens" ||
            shaderName == "AQUAS/Desktop and Web/Double-Sided/Double-Textured" ||
            shaderName == "AQUAS/Desktop and Web/Double-Sided/Single-Textured" ||
            shaderName == "AQUAS/Desktop and Web/Double-Sided/Triple-Textured Bumpy" ||
        shaderName == "AQUAS/Desktop and Web/One-Sided/Double-Textured" ||
        shaderName == "AQUAS/Desktop and Web/Double-Sided/River" ||
        shaderName == "AQUAS/Desktop and Web/One-Sided/Single-Textured" ||
        shaderName == "KriptoFX/RFX4/Decal/Ice" ||
        //shaderName == "KriptoFX/RFX4/DistortionParticles" ||
        //shaderName == "KriptoFX/RFX4/DistortionParticlesAdditive" ||
        //shaderName == "KriptoFX/RFX4/Ice" ||
        shaderName == "KriptoFX/RFX4/WaterSplash" ||
        shaderName == "KriptoFX/RFX4/WaterSplashTex"
        //shaderName == "Xffect/displacement/screen"
        )
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            _UsedShaders_NoUsedGrabPass.Add(entry);

            return true;
        }

        return false;
    }

    private bool CheckShader_KriptoFXParticle(string shaderName, string matName, string goName, string prefabName, string[] keywords)
    {
        if (shaderName == "KriptoFX/RFX4/Particle" && ContainsKeyword(keywords, "FrameBlend_ON"))
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            _UsedShaders_KriptoFXParticle.Add(entry);
            return true;
        }

        return false;
    }

    private bool CheckShader_Distortion(string shaderName, string matName, string goName, string prefabName, bool isL3)
    {
        if (shaderName == "KriptoFX/RFX4/DistortionParticles" ||
            shaderName == "KriptoFX/RFX4/DistortionParticlesAdditive" ||
            shaderName == "KriptoFX/RFX4/Ice" ||
            shaderName == "Xffect/displacement/screen")
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            if (isL3)
                _UsedShaders_Distortion_L3.Add(entry);
            else
                _UsedShaders_Distortion_NoL3.Add(entry);

            return true;
        }
        return false;
    }

    private bool CheckShader_DepthTexture(string shaderName, string matName, string goName, string prefabName)
    {
        if (shaderName == "AQUAS/Desktop and Web/Double-Sided/Double-Textured" ||
            shaderName == "AQUAS/Desktop and Web/One-Sided/Double-Textured" ||
            shaderName == "AQUAS/Desktop and Web/Double-Sided/River" ||
            shaderName == "AQUAS/Desktop and Web/One-Sided/Single-Textured" ||
            shaderName == "Hidden/Colorful/Bilateral Gaussian Blur" ||
            shaderName == "TeraPP/DepthOfField" ||
            shaderName == "TeraPP/Fog" ||
            shaderName == "Tera/FoamyBiColor" )
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            _UsedShaders_DepthTexture.Add(entry);

            return true;
        }
        return false;
    }

    private bool CheckShader_CutoutBorder(string shaderName, string matName, string goName, string prefabName)
    {
        if (shaderName == "KriptoFX/RFX4/CutoutBorder")
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            _UsedShaders_CutoutBorder.Add(entry);

            return true;
        }
        return false;
    }

    private bool CheckShader_Clip(string shaderName, string matName, string goName, string prefabName)
    {
        if (shaderName == "AQUAS/Camera Effects/Wet Lens" ||
            shaderName == "BuildinShader/Legacy Shaders/Transparent/Cutout/VertexLit" ||
            shaderName == "BuildinShader/Hidden/Nature/Tree Soft Occlusion Leaves Rendertex" ||
            shaderName == "Character/Character_Heroic" ||
            shaderName == "Character/Character_Heroic_Creat" ||
            shaderName == "Character/Character_Heroic_Face" ||
            shaderName == "Character/Character_Heroic_Face_Female" ||
            shaderName == "Character/Character_Heroic_Face_Creat" ||
            shaderName == "Character/Character_Heroic_Face2" ||
            shaderName == "Character/Character_Heroic_Face3" ||
            shaderName == "Character/Character_Heroic_Fashion" ||
            shaderName == "Character/Character_Heroic_Fashion_Transparent" ||
            shaderName == "Character/Character_Heroic_withFX" ||
            shaderName == "Character/Character_Heroic_Hair" ||
            shaderName == "Character/Character_Heroic_Hair_Creat" ||
            shaderName == "Character/Character_Heroic_Hair_test" ||
            shaderName == "Character/Character_Heroic_Hair_test_strandSP" ||
            shaderName == "Character/Character_Heroic_WeaponWing" ||
            shaderName == "Character/Character_Heroic_Weapon_Creat" ||
            shaderName == "Character/Character_Heroic2" ||
            shaderName == "Character/Character_Heroic3" ||
             shaderName == "Character/Character_NPC" ||
            shaderName == "Character/Character_NPC_vip" ||
            shaderName == "Character/CharacterHair" ||
             shaderName == "Environment/Env_Dstveiw_TransparentCutout" ||
            shaderName == "Environment/FX_Water_Distortadd_masked" ||
            shaderName == "Tera/MobileCamtrans" ||
             shaderName == "Tera/MobileCamtransOpaque" ||
            shaderName == "Tera/MobileDiffuseBumpEnvMapTwoSide" ||
            shaderName == "Tera/MobileDiffuseBumpTwoSide" ||
             shaderName == "TERA/Environment/MobileDiffuseTwoSide" ||
            shaderName == "Tera/MobileDiffuseTwoSideWithEmission" ||
            shaderName == "Tera/MobileDiffuseTwoSideWithEmissionVertexAnim" ||
            shaderName == "Tera/MobileDiffuseTwoSideVertexAnim")
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            _UsedShaders_CutoutBorder.Add(entry);

            return true;
        }
        return false;
    }

    private bool CheckShader_NoOutput(string shaderName, string matName, string goName, string prefabName)
    {
        if (//shaderName == "Environment/Env_Fadeable" ||
            shaderName == "Environment/Env_fadeoutrim" ||
            shaderName == "Character/Character_NPC_vip_old" ||
            shaderName == "Character/Character_Heroic_Female" ||
            shaderName == "Hidden/InternalErrorShader" ||
            //shaderName == "KriptoFX/RFX4/WaterTurbulence" ||
            //shaderName == "MADFINGER/Environment/Scroll 2 Layers Sine Add" ||
            //shaderName == "MADFINGER/Environment/Scroll 2 Layers Sine AlphaBlended" ||
            shaderName == "Toony/DiffuseAnisoOutline")
        {
            CUsedShaderEntry entry = new CUsedShaderEntry();
            entry.shaderName = shaderName;
            entry.matName = matName;
            entry.goName = goName;
            entry.prafabName = prefabName;

            _UsedShaders_NoOutput.Add(entry);

            return true;
        }
        return false;
    }

}