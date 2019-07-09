using UnityEngine;

public class RFX4_ShaderFloatCurve : ReusableFx
{
    public RFX4_ShaderProperties ShaderFloatProperty = RFX4_ShaderProperties._Cutoff;
    public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;
    public bool UseSharedMaterial;

    private bool canUpdate;
    private float startTime;
    private Material mat;
    private float startFloat;
    private int propertyID;
    private string shaderProperty;
    private bool isInitialized;

    #region API
    public override void SetActive(bool active)
    {
        if (active)
        {
            Init();

            startTime = Time.time;
            canUpdate = true;
            if (mat != null)
            {
                var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
                mat.SetFloat(propertyID, eval);
            }
        }
        else
        {
            if (UseSharedMaterial)
            {
                if (mat != null)
                    mat.SetFloat(propertyID, startFloat);
            }
        }

        base.SetActive(active);
    }

    public override void Tick(float delta)
    {
        var time = Time.time - startTime;
        if (canUpdate)
        {
            if (mat != null)
            {
                var eval = FloatCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier;
                mat.SetFloat(propertyID, eval);
            }
        }
        if (time >= GraphTimeMultiplier)
        {
            if (IsLoop) startTime = Time.time;
            else canUpdate = false;
        }
    }
    #endregion

    private void Init()
    {
        if(isInitialized) return;

        var rend = GetComponent<Renderer>();
        if (rend == null)
        {
            var projector = GetComponent<Projector>();
            if (projector != null)
            {
                if (!UseSharedMaterial)
                {
                    if (projector.material != null && !projector.material.name.EndsWith("(Instance)"))
                        projector.material = new Material(projector.material) { name = projector.material.name + " (Instance)" };
                    mat = projector.material;
                }
                else
                {
                    mat = projector.material;
                }
            }
        }
        else
        {
            if (!UseSharedMaterial) mat = rend.material;
            else mat = rend.sharedMaterial;
        }

        shaderProperty = ShaderFloatProperty.ToString();

        if (mat != null)
        {
            if (mat.HasProperty(shaderProperty))
            {
                propertyID = Shader.PropertyToID(shaderProperty);
                startFloat = mat.GetFloat(propertyID);
                var eval = FloatCurve.Evaluate(0) * GraphIntensityMultiplier;
                mat.SetFloat(propertyID, eval);
            }
        }

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

    private void OnDestroy()
    {
        if (!UseSharedMaterial)
        {
            if (mat != null)
                Destroy(mat);
            mat = null;
        }
    }
}