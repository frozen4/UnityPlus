using System.Collections.Generic;
using Common;
using UnityEngine;
using System;

public class ShaderManager : Singleton<ShaderManager>
{
    private readonly Dictionary<string, Shader> _ShaderMap = new Dictionary<string, Shader>();
    private readonly HashSet<string> _MissingShaderSet = new HashSet<string>();

    //private ShaderVariantCollection _ShaderCollection = null;

    public void Init(string shaderBundleName, string shaderListPath)
    {
        Action<UnityEngine.Object> callback = (asset) =>
        {
            var bundle = asset as AssetBundle;
            if (bundle == null)
            {
                HobaDebuger.LogError("Failed to load AssetBundle: shaders");
                return;
            }

            if (false)
            {
                ShaderVariantCollection shaderCollection = bundle.LoadAsset<ShaderVariantCollection>("svc1");
                if (null != shaderCollection)
                {
                    HobaDebuger.LogWarningFormat("SVC WarmUp: {0} shaders, {1} variants", shaderCollection.shaderCount, shaderCollection.variantCount);
                    shaderCollection.WarmUp();
                }
            }
            else
            {
                bundle.LoadAllAssets();
                Shader.WarmupAllShaders();
                HobaDebuger.LogWarningFormat("Skip SVC WarmUp!");
            }

            var shaderListObj = bundle.LoadAsset(shaderListPath) as GameObject;

            BuildShaderList(shaderListObj);
        };

        CAssetBundleManager.AsyncLoadBundle(shaderBundleName, callback, true);
    }

    private void BuildShaderList(GameObject shaderListObj)
    {
        _ShaderMap.Clear();

        if (shaderListObj != null)
        {
            ShaderList compShaderList = shaderListObj.GetComponent<ShaderList>();
            if (compShaderList != null && compShaderList.Shaders != null)
            {
                try
                {
                    int nCount = compShaderList.Shaders.Length;
                    HobaDebuger.LogFormat("ShaderManager.Init, Shaders Count: {0}", nCount);
                    for (int i = 0; i < nCount; ++i)
                    {
                        Shader shader = compShaderList.Shaders[i];
                        if (shader != null && !_ShaderMap.ContainsKey(shader.name))
                        {
                            _ShaderMap.Add(shader.name, shader);
                            //HobaDebuger.LogFormat("ShaderManager.Init, Load Shader: {0}", shader.name);
                        }
                        else
                        {
                            HobaDebuger.LogWarning(HobaText.Format("ShaderManager.Init, Shader {0} is missing!", i));
                        }
                    }
                }
                catch (ArgumentException ae)
                {
                    HobaDebuger.LogError(ae.Message);
                }
            }
        }
    }

    public Shader FindShader(string name)
    {
        Shader val;
        if (_ShaderMap.TryGetValue(name, out val))
        {
            return val;
        }

        val = Shader.Find(name);
        if (val != null)
        {
            return val;
        }

        if (!_MissingShaderSet.Contains(name))
        {
            _MissingShaderSet.Add(name);
        }
        return null;
    }
}