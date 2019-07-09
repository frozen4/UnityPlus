using UnityEngine;

public class RFX4_ScaleCurves : ReusableFx
{
    public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;

    private bool canUpdate;
    private float startTime;
    private Transform t;
    private int nameId;
    private Projector proj;
    private Vector3 startScale;

    private bool isInitialized;
    private float _runTimeScaleFactor = 1f;

    #region API
    public override void SetActive(bool active)
    {
        if (active)
        {
            Init();
            startTime = Time.time;
            canUpdate = true;
            t.localScale = Vector3.zero;
        }

        base.SetActive(active);
    }

    public override void Tick(float delta)
    {
        var time = Time.time - startTime;
        if (canUpdate)
        {
            var eval = FloatCurve.Evaluate(time / GraphTimeMultiplier) * GraphIntensityMultiplier * _runTimeScaleFactor;
            t.localScale = eval * startScale;
            if (proj != null)
                proj.orthographicSize = eval;
        }

        if (time >= GraphTimeMultiplier)
        {
            if (IsLoop)
                startTime = Time.time;
            else
                canUpdate = false;
        }
    }

    #endregion

    public void SetScaleFactor(float f)
    {
        _runTimeScaleFactor = f;
    }

    private void Init()
    {
        if(isInitialized) return;

        t = GetComponent<Transform>();
        startScale = t.localScale;
        t.localScale = Vector3.zero;
        proj = GetComponent<Projector>();

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
}
