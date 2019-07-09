using UnityEngine;

public class RFX4_UVScroll : ReusableFx
{
    public Vector2 UvScrollMultiplier = new Vector2(1.0f, 0.0f);
    public RFX4_TextureShaderProperties TextureName = RFX4_TextureShaderProperties._MainTex;

    Vector2 uvOffset = Vector2.zero;

    private Material mat;
    private bool isInitialized;

    #region API
    public override void SetActive(bool active)
    {
        if (active)
            Init();

        base.SetActive(active);
    }

    public override void Tick(float delta)
    {
        uvOffset += (UvScrollMultiplier * delta);
        if (mat != null)
        {
            mat.SetTextureOffset(TextureName.ToString(), uvOffset);
        }
    }

    #endregion

    private void Init()
    {
        if(isInitialized) return;

        var currentRenderer = GetComponent<Renderer>();
        if (currentRenderer == null)
        {
            var projector = GetComponent<Projector>();
            if (projector != null)
            {
                if (!projector.material.name.EndsWith("(Instance)"))
                    projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
                mat = projector.material;
            }
        }
        else
            mat = currentRenderer.material;

        isInitialized = true;
    }

    void OnEnable()
    {
        SetActive(true);
    }

    void OnDisable()
    {
        SetActive(false);
    }

    void Update()
    {
        Tick(Time.deltaTime);
    }
}
