using UnityEngine;

[ExecuteInEditMode]
public class Rain : MonoBehaviour
{
    public float Wetness = 0.3f;
    public float Porosity = 1f;
    public float RainIntensity = 100f;

    private Texture2D _RaindropRippleTex;
    private Vector4 _RainParamter = Vector4.zero;

    void OnEnable()
    {
        if (_RaindropRippleTex == null)
            _RaindropRippleTex = Resources.Load("SceneEffect/RaindropRippleTex") as Texture2D;

        Shader.EnableKeyword("RAIN_SURFACE_ON");        
    }

    void OnDisable()
    {
        Shader.DisableKeyword("RAIN_SURFACE_ON");
    }

    void Update()
    {
        if (_RaindropRippleTex == null) return;

        Shader.SetGlobalTexture(ShaderIDs.RaindropRipple, _RaindropRippleTex);

        _RainParamter.x = Mathf.Clamp01(1 - Wetness);
        _RainParamter.y = RainIntensity;
        _RainParamter.z = Mathf.Clamp01(1 - Porosity);
        Shader.SetGlobalVector(ShaderIDs.RainParamters, _RainParamter);
    }
}
