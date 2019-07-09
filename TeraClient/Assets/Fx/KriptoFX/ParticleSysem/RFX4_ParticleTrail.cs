using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(ParticleSystem))]
public class RFX4_ParticleTrail : ReusableFx
{
    public GameObject Target;
    public Vector2 DefaultSizeMultiplayer = Vector2.one;
    public float VertexLifeTime = 2;
    public float TrailLifeTime = 2;
    public bool UseShaderMaterial;
    public Material TrailMaterial;
    public bool UseColorOverLifeTime = false;
    public Gradient ColorOverLifeTime = new Gradient();
    public float ColorLifeTime = 1;

    public bool UseUvAnimation = false;
    public int TilesX = 4;
    public int TilesY = 4;
    public int FPS = 30;
    public bool IsLoop = true;

    [Range(0.001f, 1)]
    public float MinVertexDistance = 0.01f;
    public bool GetVelocityFromParticleSystem = false;
    public float Gravity = 0.01f;
    public Vector3 Force = new Vector3(0, 0.01f, 0);
    public float InheritVelocity = 0;
    public float Drag = 0.01f;
     [Range(0.001f, 10)]
    public float Frequency = 1;
     [Range(0.001f, 10)]
    public float OffsetSpeed = 0.5f;
    public bool RandomTurbulenceOffset = false;
     [Range(0.001f, 10)]
    public float Amplitude = 2;
    public float TurbulenceStrength = 0.1f;
    public AnimationCurve VelocityByDistance = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public float AproximatedFlyDistance = -1;
    public bool SmoothCurves = true;

    public class LineRendererInfo
    {
        public GameObject Go;
        public LineRenderer LineRendererComp;
        public float EndTime;
        public RFX4_ShaderColorGradient ColorGradientComp;
        public RFX4_UVAnimation UVAnimationComp;
        public RFX4_TrailRenderer TrailRendererComp;
    }

    private readonly Dictionary<int, LineRendererInfo> dict = new Dictionary<int, LineRendererInfo>(); 
    ParticleSystem ps = null;
    ParticleSystem.Particle[] particles;

    private Color psColor;
    private Transform targetT;
    private int layer;
    private bool isLocalSpace = true;
    private Transform t;

    private bool isInitialized;

    #region API
    public override void SetActive(bool active)
    {
        if (active)
        {
            Init();
            if(ps != null)
                ps.Play();
        }
        else
        {
            if (ps != null)
            {
                ps.Stop();
                ps.Clear();
            }

            foreach (var v in dict)
                Destroy(v.Value.Go);

            dict.Clear();
        }

        base.SetActive(active);
    }

    public override void Tick(float dt)
    {
        if(ps == null) return;

        var count = ps.GetParticles(particles);
        float curTime = Time.time;
        for (int i = 0; i < count; i++)
        {
            var hash = (particles[i].rotation3D).GetHashCode();
            LineRendererInfo trail;
            if (!dict.TryGetValue(hash, out trail))
            {
                var info = new LineRendererInfo();
                var go = new GameObject(hash.ToString());
                go.transform.parent = transform;
                go.transform.position = ps.transform.position;
                info.Go = go;

                if (TrailLifeTime > 0.00001f)
                    info.EndTime = (curTime + TrailLifeTime + VertexLifeTime);
                else
                    info.EndTime = -1;

                go.layer = layer;
                var lineRenderer = go.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0;
                lineRenderer.endWidth = 0;
                lineRenderer.sharedMaterial = TrailMaterial;
                lineRenderer.useWorldSpace = false;
                info.LineRendererComp = lineRenderer;

                if (UseColorOverLifeTime)
                {
                    var shaderColor = go.AddComponent<RFX4_ShaderColorGradient>();
                    shaderColor.Color = ColorOverLifeTime;
                    shaderColor.TimeMultiplier = ColorLifeTime;
                    info.ColorGradientComp = shaderColor;
                }

                if (UseUvAnimation)
                {
                    var uvAnimation = go.AddComponent<RFX4_UVAnimation>();
                    uvAnimation.TilesX = TilesX;
                    uvAnimation.TilesY = TilesY;
                    uvAnimation.FPS = FPS;
                    uvAnimation.IsLoop = IsLoop;
                    info.UVAnimationComp = uvAnimation;
                }

                dict.Add(hash, info);
            }
            else
            {
                if (trail == null) continue;

				if (!trail.LineRendererComp.useWorldSpace)
                {
                    var trailRenderer = trail.Go.AddComponent<RFX4_TrailRenderer>();
                    trailRenderer.Amplitude = Amplitude;
                    trailRenderer.Drag = Drag;
                    trailRenderer.Gravity = Gravity;
                    trailRenderer.Force = Force;
                    trailRenderer.Frequency = Frequency;
                    trailRenderer.InheritVelocity = InheritVelocity;
                    trailRenderer.VertexLifeTime = VertexLifeTime;
                    trailRenderer.TrailLifeTime = TrailLifeTime;
                    trailRenderer.MinVertexDistance = MinVertexDistance;
                    trailRenderer.OffsetSpeed = OffsetSpeed;
                    trailRenderer.SmoothCurves = SmoothCurves;
                    trailRenderer.AproximatedFlyDistance = AproximatedFlyDistance;
                    trailRenderer.VelocityByDistance = VelocityByDistance;
                    trailRenderer.RandomTurbulenceOffset = RandomTurbulenceOffset;
                    trailRenderer.TurbulenceStrength = TurbulenceStrength;
                    trail.LineRendererComp.useWorldSpace = true;
                    trail.TrailRendererComp = trailRenderer;
                }

                var size = DefaultSizeMultiplayer * particles[i].GetCurrentSize(ps);
                trail.LineRendererComp.startWidth = size.y;
                trail.LineRendererComp.endWidth = size.x;

                if (Target != null)
                {
                    var time = 1 - particles[i].remainingLifetime / particles[i].startLifetime;
                    var pos = Vector3.Lerp(particles[i].position, targetT.position, time);
                    trail.Go.transform.position = Vector3.Lerp(pos, targetT.position, Time.deltaTime * time);
                }
                else
                {
                    trail.Go.transform.position = isLocalSpace ? ps.transform.TransformPoint(particles[i].position) : particles[i].position;
                }
                trail.Go.transform.rotation = t.rotation;
                var particleColor = particles[i].GetCurrentColor(ps);
                var color = psColor * particleColor;
                trail.LineRendererComp.startColor = color;
                trail.LineRendererComp.endColor = color;
            }
        }
        ps.SetParticles(particles, count);

        
        foreach (var v in dict)
        {
            if (v.Value.EndTime > 0 && curTime > v.Value.EndTime)
            {
                Destroy(v.Value.Go);
                dict.Remove(v.Key);
                break;
            }

            if(v.Value.ColorGradientComp != null)
                v.Value.ColorGradientComp.Tick(dt);

            if (v.Value.UVAnimationComp != null)
                v.Value.UVAnimationComp.Tick(dt);

            if (v.Value.TrailRendererComp != null)
                v.Value.TrailRendererComp.Tick(dt);
        }
    }

    #endregion

    private void Init()
    {
        if(isInitialized) return;

        if (Target != null)
            targetT = Target.transform;

        ps = GetComponent<ParticleSystem>();
        t = transform;

        isLocalSpace = ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        particles = new ParticleSystem.Particle[ps.main.maxParticles];

        if (TrailMaterial != null)
        {
            var id = TrailMaterial.HasProperty(ShaderIDs.TintColor) ? ShaderIDs.TintColor : ShaderIDs.Color;
            psColor = TrailMaterial.GetColor(id);

        }

        layer = gameObject.layer;

        isInitialized = true;
    }

    void OnEnable()
    {
        SetActive(true);
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    void OnDisable()
    {
        SetActive(false);
    }
}
