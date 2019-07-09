using System.Collections.Generic;
using Common;
using UnityEngine;
using System;
using LuaInterface;
using System.Collections;

public class ShaderManager : Singleton<ShaderManager>
{
    private readonly Dictionary<string, Shader> _ShaderMap = new Dictionary<string, Shader>();
    private readonly HashSet<string> _MissingShaderSet = new HashSet<string>();

    private Shader _MobileCamtransOpaqueShader = null;

    public Shader MobileCamtransOpaqueShader
    {
        get { return _MobileCamtransOpaqueShader; }
    }

    private Shader _MobileCamtransShader = null;

    public Shader MobileCamtransShader
    {
        get { return _MobileCamtransShader; }
    }
    //private ShaderVariantCollection _ShaderCollection = null;

    private bool _IsLoading = false;
    private AssetBundle _AssetBundle = null;

    public void LoadAssets()
    {
        Action<UnityEngine.Object> callback = (asset) =>
        {
            _IsLoading = false;
            _AssetBundle = null;

            var bundle = asset as AssetBundle;
            if (bundle == null)
            {
                HobaDebuger.LogError("Failed to load AssetBundle: shaders");
                return;
            }
            _AssetBundle = bundle;
        };

        var bundleName = "shader";
        _IsLoading = true;
        _AssetBundle = null;
        CAssetBundleManager.AsyncLoadBundle(bundleName, callback);
    }

    public IEnumerable Init()
    {
        while (_IsLoading)
        {
            yield return null;
        }

        if (_AssetBundle != null)
        {
            if (EntryPoint.Instance.GameCustomConfigParams.ShaderWarmUp)
            {
                ShaderVariantCollection shaderCollection = _AssetBundle.LoadAsset<ShaderVariantCollection>("svc");
                yield return null;

                if (null != shaderCollection)
                {
                    shaderCollection.WarmUp();
                    yield return null;
                }
            }

            var shaderListPath = "Assets/Outputs/Shader/ShaderList.prefab";
            var shaderListObj = _AssetBundle.LoadAsset(shaderListPath) as GameObject;
            yield return null;

            BuildShaderList(shaderListObj);
            _MobileCamtransOpaqueShader = FindShader("TERA/Environment/MobileCamtransOpaque");
            _MobileCamtransShader = FindShader("TERA/Environment/MobileCamtrans");
            yield return null;
        }
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
            LuaDLL.HOBA_LogString(HobaText.Format("ShaderManager.FindShader Failed, Shader Missing! {0}", name));
            _MissingShaderSet.Add(name);
        }
        return null;
    }
}