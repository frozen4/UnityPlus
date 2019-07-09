using UnityEngine;

public class RFX4_ShaderColorGradient : ReusableFx
{
    public RFX4_ShaderProperties ShaderColorProperty = RFX4_ShaderProperties._TintColor;
    public Gradient Color = new Gradient();
    public float TimeMultiplier = 1;
    public bool IsLoop;
    public bool UseSharedMaterial;

    [HideInInspector]
    public float HUE = -1;

    [HideInInspector]
    public bool canUpdate;

    private Material mat;
    private int propertyID;
    private float startTime;
    private Color startColor;

    private bool isInitialized;
    private string shaderProperty;

    #region API
    public override void SetActive(bool active)
    {
        if (active)
        {
            Init();
            startTime = Time.time;
            canUpdate = true;
        }
        else
        {
            if (mat != null)
            {
                if (UseSharedMaterial)
                    mat.SetColor(propertyID, new Color(0, 0, 0, 0));
                mat.SetColor(propertyID, new Color(0, 0, 0, 0));
            }
        }

        base.SetActive(active);
    }

    public override void Tick(float delta)
    {
        if (mat == null) return;
        var time = Time.time - startTime;
        if (canUpdate)
        {
            var eval = Color.Evaluate(time / TimeMultiplier);
            if (HUE > -0.9f)
            {
                eval = RFX4_ColorHelper.ConvertRGBColorByHUE(eval, HUE);
                startColor = RFX4_ColorHelper.ConvertRGBColorByHUE(startColor, HUE);
            }
            mat.SetColor(propertyID, eval * startColor);
        }
        if (time >= TimeMultiplier)
        {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }
    }

    #endregion

    private void Init()
    {
        if(isInitialized) return;

        shaderProperty = ShaderColorProperty.ToString();
        startTime = Time.time;
        canUpdate = true;
        var rend = GetComponent<Renderer>();
        if (rend == null)
        {
            var projector = GetComponent<Projector>();
            if (projector != null)
            {
                if (projector.material != null && !projector.material.name.EndsWith("(Instance)"))
                    projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
                mat = projector.material;
            }
        }
        else
        {
            if (!UseSharedMaterial) mat = rend.material;
            else mat = rend.sharedMaterial;
        }

        if (mat == null)
        {
            canUpdate = false;
            return;
        }

        if (!mat.HasProperty(shaderProperty))
        {
            canUpdate = false;
            return;
        }
        if (mat.HasProperty(shaderProperty))
            propertyID = Shader.PropertyToID(shaderProperty);

        startColor = mat.GetColor(propertyID);
        var eval = Color.Evaluate(0);
        mat.SetColor(propertyID, eval * startColor);
        isInitialized = true;
    }


    private void OnEnable()
    {
        SetActive(true);
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    private void OnDisable()
    {
        SetActive(false);
    }
}