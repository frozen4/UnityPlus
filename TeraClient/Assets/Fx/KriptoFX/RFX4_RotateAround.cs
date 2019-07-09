using UnityEngine;

public class RFX4_RotateAround : ReusableFx
{
    public Vector3 Offset = Vector3.forward;
    public Vector3 RotateVector = Vector3.forward;
    public float LifeTime = 1;

    private Transform t;
    private float currentTime;
    private Quaternion rotation;
    private bool isInitialized;

    public override void SetActive(bool active)
    {
        if (active)
        {
            Init();

            currentTime = 0;
            if (t != null) t.rotation = rotation;
        }

        base.SetActive(active);
    }

    public override void Tick(float dt)
    {
        if (currentTime >= LifeTime && LifeTime > 0.0001f)
            return;
        currentTime += dt;
        t.Rotate(RotateVector * dt);
    }

    private void Init()
    {
        if(isInitialized) return;
        t = transform;
        rotation = t.rotation;

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

    private void Update()
    {
        Tick(Time.deltaTime);
    }
}