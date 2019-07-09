using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class RFX4_Turbulence : ReusableFx
{
    public float TurbulenceStrenght = 1;
    public bool TurbulenceByTime;
    public float TimeDelay = 0;
    public AnimationCurve TurbulenceStrengthByTime = AnimationCurve.EaseInOut(1, 1, 1, 1);
    public Vector3 Frequency = new Vector3(1, 1, 1);
    public Vector3 OffsetSpeed = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 Amplitude = new Vector3(5, 5, 5);
    public Vector3 GlobalForce;
    public bool UseGlobalOffset = true;
    public MoveMethodEnum MoveMethod;
    public PerfomanceEnum Perfomance = PerfomanceEnum.High;
    public float ThreshholdSpeed = 0;
    public AnimationCurve VelocityByDistance = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public float AproximatedFlyDistance = -1;

    public enum MoveMethodEnum
    {
        Position,
        Velocity,
        // RelativePositionHalf,
        RelativePosition
    }

    public enum PerfomanceEnum
    {
        High,
        Low
    }

    private float lastStopTime;
    private Vector3 currentOffset;
    private float deltaTime;
    private float deltaTimeLastUpdateOffset;
    private ParticleSystem.Particle[] particleArray;
    private ParticleSystem particleSys;
    private float time;
    private int currentSplit;
    private float fpsTime;
    private int FPS;
    private int splitUpdate = 2;
    private PerfomanceEnum perfomanceOldSettings;
    private bool skipFrame;
    private Transform t;
    private float currentDelay;

    private bool isInitilised;
    private int vSyncCount = 0;

    #region API
    public override void SetActive(bool active)
    {
        if (active)
        {
            Init();
            currentDelay = 0;
        }

        base.SetActive(active);
    }

    public override void Tick(float dt)
    {
        deltaTime = dt;
        currentDelay += deltaTime;
        if (currentDelay < TimeDelay) return;
        if (!UseGlobalOffset)
            currentOffset += OffsetSpeed * deltaTime;
        else
        {
            if (Application.isPlaying) currentOffset = OffsetSpeed * Time.time;
            else currentOffset = OffsetSpeed * Time.realtimeSinceStartup;
        }
        if (Perfomance != perfomanceOldSettings)
        {
            perfomanceOldSettings = Perfomance;
            UpdatePerfomanceSettings();
        }
        time += deltaTime;

        if (vSyncCount == 2)
            UpdateTurbulence();
        else if (vSyncCount == 1)
        {
            if (Perfomance == PerfomanceEnum.Low)
            {
                if (skipFrame)
                    UpdateTurbulence();
                skipFrame = !skipFrame;
            }
            if (Perfomance == PerfomanceEnum.High)
                UpdateTurbulence();
        }
        else if (vSyncCount == 0)
        {
            if (time >= fpsTime)
            {
                time = 0;
                UpdateTurbulence();
                deltaTimeLastUpdateOffset = 0;
            }
            else
                deltaTimeLastUpdateOffset += deltaTime;
        }
    }

    #endregion

    private void Init()
    {
        if(isInitilised) return;

        t = transform;
        particleSys = GetComponent<ParticleSystem>();
        if (particleArray == null || particleArray.Length < particleSys.main.maxParticles)
            particleArray = new ParticleSystem.Particle[particleSys.main.maxParticles];

        perfomanceOldSettings = Perfomance;
        UpdatePerfomanceSettings();

        vSyncCount = QualitySettings.vSyncCount;

        isInitilised = true;
    }

    private void UpdatePerfomanceSettings()
    {
       if (Perfomance == PerfomanceEnum.High)
        {
            FPS = 80;
            splitUpdate = 2;
        }
        if (Perfomance == PerfomanceEnum.Low)
        {
            FPS = 40;
            splitUpdate = 2;
        }
        fpsTime = 1.0f / FPS;
    }

    private void UpdateTurbulence()
    {
        int start;
        int end;
        var numParticlesAlive = particleSys.GetParticles(particleArray);
        var turbulenceStrenghtMultiplier = 1;
        if (splitUpdate > 1)
        {
            start = (numParticlesAlive / splitUpdate) * currentSplit;
            end =  Mathf.CeilToInt((numParticlesAlive * 1.0f / splitUpdate) * (currentSplit + 1.0f));
            turbulenceStrenghtMultiplier = splitUpdate;
        }
        else
        {
            start = 0;
            end = numParticlesAlive;
        }
        for (int i = start; i < end; i++)
        {
            var particle = particleArray[i];
            float timeTurbulenceStrength = 1;
             if (TurbulenceByTime)
                timeTurbulenceStrength = TurbulenceStrengthByTime.Evaluate(1 - particle.remainingLifetime / particle.startLifetime);
            if (ThreshholdSpeed > 0.0000001f && timeTurbulenceStrength < ThreshholdSpeed) return;
            var pos = particle.position;
            pos.x /= (Frequency.x + 0.0000001f);
            pos.y /= (Frequency.y + 0.0000001f);
            pos.z /= (Frequency.z + 0.0000001f);
            var turbulenceVector = new Vector3();
            var timeOffset = deltaTime + deltaTimeLastUpdateOffset;
            turbulenceVector.x = ((Mathf.PerlinNoise(pos.z - currentOffset.z, pos.y - currentOffset.y) * 2 - 1) * Amplitude.x) * timeOffset;
            turbulenceVector.y = ((Mathf.PerlinNoise(pos.x - currentOffset.x, pos.z - currentOffset.z) * 2 - 1) * Amplitude.y) * timeOffset;
            turbulenceVector.z = ((Mathf.PerlinNoise(pos.y - currentOffset.y, pos.x - currentOffset.x) * 2 - 1) * Amplitude.z) * timeOffset;
            var lerpedTurbulence = TurbulenceStrenght * timeTurbulenceStrength * turbulenceStrenghtMultiplier;

            float velocityByDistanceMultiplier = 1;
            if (AproximatedFlyDistance > 0)
            {
                var distance = (particle.position - t.position).magnitude;
                velocityByDistanceMultiplier = VelocityByDistance.Evaluate(Mathf.Clamp01(distance / AproximatedFlyDistance));
            }

            turbulenceVector *= lerpedTurbulence;
            if (MoveMethod == MoveMethodEnum.Position)
                particleArray[i].position += turbulenceVector * velocityByDistanceMultiplier;
            if (MoveMethod == MoveMethodEnum.Velocity)
                particleArray[i].velocity += turbulenceVector * velocityByDistanceMultiplier;
            if (MoveMethod == MoveMethodEnum.RelativePosition)
            {
                particleArray[i].position += turbulenceVector * particleArray[i].velocity.magnitude;
                particleArray[i].velocity = particleArray[i].velocity * 0.85f + turbulenceVector.normalized * 0.15f * velocityByDistanceMultiplier + GlobalForce * velocityByDistanceMultiplier;
            }
        }
        particleSys.SetParticles(particleArray, numParticlesAlive);

        currentSplit++;
        if (currentSplit >= splitUpdate)
            currentSplit = 0;
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
