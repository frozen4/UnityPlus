using UnityEngine;

[ExecuteInEditMode]
public class Snow : MonoBehaviour
{
    //[HideInInspector]
    public float SnowDensity = 0.1f;

    private Texture2D _SnowTexUpperFace = null;
    private Texture2D _SnowTexSideFace = null;

    void OnEnable()
    {
        if(_SnowTexUpperFace == null)
            _SnowTexUpperFace = Resources.Load("SceneEffect/SnowTexUpperFace") as Texture2D;
        if (_SnowTexSideFace == null)
            _SnowTexSideFace = Resources.Load("SceneEffect/SnowTexSideFace") as Texture2D;

        Shader.EnableKeyword("SNOW_SURFACE_ON");
    }

    void OnDisable()
    {
        Shader.DisableKeyword("SNOW_SURFACE_ON");
    }

    void Update()
    {
        if(_SnowTexUpperFace == null || _SnowTexSideFace == null) return;

        Shader.SetGlobalTexture(ShaderIDs.SnowTexUpperFace, _SnowTexUpperFace);
        Shader.SetGlobalTexture(ShaderIDs.SnowTexSideFace, _SnowTexSideFace);
        Shader.SetGlobalFloat(ShaderIDs.SnowDensity, SnowDensity);
    }
}
