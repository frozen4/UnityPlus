using UnityEngine;

public class ArcReactor_Arc : ReusableFx
{
    #region 内部类型声明区
    public enum PropagationType
    {
        instant = 0,
        globalSpaceSpeed = 1,
    }

    public enum ArcsPlaybackType
    {
        once = 0,
        loop = 1,
    }

    public enum InterpolationType
    {
        CatmullRom_Splines = 0,
        Linear = 1
    }

    public enum SpatialNoiseType
    {
        TangentRandomization = 0,
        CubicRandomization = 1,
        BrokenTangentRandomization = 2
    }

    public enum OscillationType
    {
        sine_wave = 0,
        rectangular = 1,
        zigzag = 2
    }

    public enum FadeTypes
    {
        none = 0,
        worldspacePoint = 1,
        relativePoint = 2
    }

    [System.Serializable]
    public class EaseInOutOptions
    {
        public bool useEaseInOut;
        public AnimationCurve easeInOutCurve;
        public float distance;
    }

    [System.Serializable]
    public class ArcPropagationOptions
    {
        public PropagationType propagationType = PropagationType.instant;
        public float globalSpeed = 1.0f;
    }

    [System.Serializable]
    public class ArcColorOptions
    {
        public Gradient startColor;
        public bool onlyStartColor = true;
        public Gradient endColor;
        public Gradient coreColor;
        public AnimationCurve coreCurve;
        public float coreJitter;
        public FadeTypes fade;
        public float fadePoint;
    }

    [System.Serializable]
    public class ArcSizeOptions
    {
        public InterpolationType interpolation = InterpolationType.CatmullRom_Splines;
        public AnimationCurve startWidthCurve;
        public bool onlyStartWidth = true;
        public AnimationCurve endWidthCurve;
        public float segmentLength = 10;
        public bool snapSegmentsToShape = false;
        public int numberOfSmoothingSegments = 0;
        public int minNumberOfSegments = 1;
    }

    [System.Serializable]
    public class ArcSpatialNoiseOptions
    {
        public SpatialNoiseType type = SpatialNoiseType.TangentRandomization;
        public float scale = 0;
        public float scaleMovement = 0;
        public float resetFrequency = 0;
        public int invisiblePriority;
    }

    [System.Serializable]
    public class OscillationInfo
    {
        public OscillationType type = OscillationType.sine_wave;
        public float wavelength;
        public float amplitude;
        public float phase;
        public float phaseMovementSpeed;
    }

    [System.Serializable]
    public class ParticleEmissionOptions
    {
        public bool emit = false;
        public GameObject shurikenPrefab;
        public bool emitAfterRayDeath = false;
        public float particlesPerMeter = 0;
        public AnimationCurve emissionDuringLifetime;
        public AnimationCurve radiusCoefDuringLifetime;
        public AnimationCurve directionDuringLifetime;
        public bool startColorByRay;
        public ParticleRandomizationOptions randomizationOptions;
    }

    [System.Serializable]
    public class ParticleRandomizationOptions
    {
        public float sizeRndCoef = 0;
        public float velocityRndCoef = 0;
        public float angularVelocityRndCoef = 0;
        public float rotationRndCoef = 0;
        public float lifetimeRndCoef = 0;
    }

    [System.Serializable]
    public class LineRendererInfo
    {
        public Material material;
        public ArcColorOptions colorOptions;
        public ArcSizeOptions sizeOptions;
        public ArcPropagationOptions propagationOptions;
        public ParticleEmissionOptions[] emissionOptions;
        public OscillationInfo[] oscillations;
        public float fadeOutTime = 1.0f;
        public float StartFadeOutTime = 0;
    }
    #endregion

    #region 美术参数设置区
    // 为正确读取美术工程数据，代码格式不做修改  
    public Transform StartTransform = null;
    public Transform EndTransform = null;

    public LineRendererInfo[] arcs;
    public EaseInOutOptions easeInOutOptions;
    public float lifetime;
    public ArcsPlaybackType playbackType = ArcsPlaybackType.once;
    public float sizeMultiplier;
    #endregion

    #region 运行时逻辑变量区
    private const float REINIT_THRESHOLD = 0.5f;
    private const float EFFECT_FADEOUT_TIME = 2.0f;

    private bool _IsValid = false;
    private float _ElapsedTime = 0;
    private float _ArcShapeLength;
    private Vector3[,] _ArcPoints;
    private Vector3[,] _ArcTangents;
    private Vector3[,] _ShiftVectors;
    private Quaternion[,] _ArcTangentsShift;
    private int[] _SegmentNums;
    private int[] _VertexCounts;
    private Vector3[][] _Vertices;
    private ParticleSystem[][] _EmitterSystems;
    private LineRenderer[] _LineRenderers;
    #endregion

    #region API

    public void SetData(Transform from, Transform to)
    {
        StartTransform = from;
        EndTransform = to;
    }

    public override void SetActive(bool active)
    {
        if (arcs == null || arcs.Length == 0)
        {
            Common.HobaDebuger.LogErrorFormat("arcs of ArcReactor_Arc {0} is null", gameObject.name);
            enabled = false;
            return;
        }

        if (active)
        {
            for (int i = 0; i < arcs.Length; i++)
            {
                var sizeOptions = arcs[i].sizeOptions;
                if (sizeOptions != null && sizeOptions.startWidthCurve.keys.Length == 0 && (sizeOptions.onlyStartWidth || sizeOptions.endWidthCurve.keys.Length == 0))
                {
                    sizeOptions.startWidthCurve.AddKey(0, 0.5f);
                    if (!sizeOptions.onlyStartWidth)
                        sizeOptions.endWidthCurve.AddKey(0, 0.5f);
                }

                var oscillations = arcs[i].oscillations;
                if (oscillations != null)
                {
                    for (int q = 0; q < oscillations.Length; q++)
                    {
                        if (Util.IsZero(oscillations[q].wavelength))
                        {
                            enabled = false;
                            return;
                        }
                    }
                }

                arcs[i].fadeOutTime = EFFECT_FADEOUT_TIME;
                arcs[i].StartFadeOutTime = 0;
            }
            _ElapsedTime = 0;

            #region 初始化
            if (!_IsValid)
            {

                _EmitterSystems = new ParticleSystem[arcs.Length][];
                _LineRenderers = new LineRenderer[arcs.Length];

                //Service array initialization, actual data creation happens at Initialize()
                for (int n = 0; n < arcs.Length; n++)
                {
                    _EmitterSystems[n] = new ParticleSystem[arcs[n].emissionOptions.Length];

                    var emissionOptions = arcs[n].emissionOptions;
                    for (int q = 0; q < emissionOptions.Length; q++)
                    {
                        var ps = _EmitterSystems[n][q];
                        if (ps == null && emissionOptions[q].shurikenPrefab != null)
                        {
                            var partGameObject = (GameObject)CUnityUtil.Instantiate(emissionOptions[q].shurikenPrefab);
                            ps = partGameObject.GetComponent<ParticleSystem>();
                            var em = ps.emission;
                            em.enabled = false;
                            partGameObject.transform.parent = transform;
                            partGameObject.transform.localPosition = Vector3.zero;
                            partGameObject.transform.localRotation = Quaternion.identity;

                            _EmitterSystems[n][q] = ps;
                        }
                    }

                    var rayLineRenderer = new GameObject("ArcLineRenderer");
                    rayLineRenderer.transform.parent = transform;
                    _LineRenderers[n] = rayLineRenderer.AddComponent<LineRenderer>();
                    _LineRenderers[n].material = arcs[n].material;
                    _LineRenderers[n].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    _LineRenderers[n].receiveShadows = false;
                    _LineRenderers[n].material.SetFloat(ShaderIDs.ARCAlphaFadeOut, 1.0f);
                }

                _SegmentNums = new int[arcs.Length];
                _VertexCounts = new int[arcs.Length];
                _Vertices = new Vector3[arcs.Length][];

                _IsValid = true;
            }
            #endregion
        }

        if (_IsValid)
        {
            foreach (var ess in _EmitterSystems)
            {
                if (ess != null)
                {
                    foreach (var ps in ess)
                    {
                        if (ps == null) continue;
                        var em = ps.emission;
                        em.enabled = active;
                    }
                }
            }

            foreach (var lr in _LineRenderers)
            {
                if (lr == null) continue;
                lr.enabled = active;
            }

            base.SetActive(active);
        }
    }

    public override void Tick(float dt)
    {
        if (!enabled || !_IsValid) return;

        if (StartTransform == null || EndTransform == null) return;

        //Phase shifting
        for (int n = 0; n < arcs.Length; n++)
        {
            foreach (OscillationInfo osc in arcs[n].oscillations)
            {
                osc.phase += osc.phaseMovementSpeed * Time.deltaTime;
                if (osc.phase > 360)
                    osc.phase = osc.phase - 360;
                if (osc.phase < 0)
                    osc.phase = osc.phase + 360;
            }
        }

        //Time management
        _ElapsedTime += Time.deltaTime;

        if (_ElapsedTime > lifetime)
        {
            if (playbackType == ArcsPlaybackType.once)
                SetActive(false);
            else if (playbackType == ArcsPlaybackType.loop)
                _ElapsedTime -= lifetime;
        }
    }

    public override void LateTick(float dt)
    {
        if (!enabled || !_IsValid) return;

        if (StartTransform == null || EndTransform == null) return;

        UpdateSegments();

        float lifetimePos = _ElapsedTime / lifetime;
        for (int n = 0; n < arcs.Length; n++)
        {
            if (arcs[n].StartFadeOutTime > 0)
                continue;

            #region colorOptions
            var colorOptions = arcs[n].colorOptions;
            {
                var renderer = _LineRenderers[n].GetComponent<Renderer>();

                var startColor = colorOptions.startColor.Evaluate(lifetimePos);
                var endColor = startColor;
                if (!colorOptions.onlyStartColor)
                    endColor = colorOptions.endColor.Evaluate(lifetimePos);
                Color coreColor = colorOptions.coreColor.Evaluate(lifetimePos);

                renderer.material.SetColor(ShaderIDs.ARCStartColor, startColor);
                renderer.material.SetColor(ShaderIDs.ARCEndColor, endColor);
                renderer.material.SetColor(ShaderIDs.ARCCoreColor, coreColor);

                var coreCoef = colorOptions.coreCurve.Evaluate(lifetimePos);
                if (colorOptions.coreJitter > 0)
                    coreCoef += UnityEngine.Random.Range(-colorOptions.coreJitter * 0.5f, colorOptions.coreJitter * 0.5f);

                renderer.material.SetFloat(ShaderIDs.ARCCoreCoef, coreCoef);

                //Fading
                switch (colorOptions.fade)
                {
                    case FadeTypes.none:
                        renderer.material.SetFloat(ShaderIDs.ARCFadeLevel, 0);
                        break;
                    case FadeTypes.relativePoint:
                        renderer.material.SetFloat(ShaderIDs.ARCFadeLevel, colorOptions.fadePoint);
                        break;
                    case FadeTypes.worldspacePoint:
                        renderer.material.SetFloat(ShaderIDs.ARCFadeLevel, Mathf.Clamp01(colorOptions.fadePoint / _ArcShapeLength));
                        break;
                }
            }
            #endregion

            //Ray size change
            float startWidth = arcs[n].sizeOptions.startWidthCurve.Evaluate(lifetimePos);
            float endWidth;
            if (arcs[n].sizeOptions.onlyStartWidth)
                endWidth = startWidth;
            else
                endWidth = arcs[n].sizeOptions.endWidthCurve.Evaluate(lifetimePos);

            _LineRenderers[n].startWidth = startWidth;
            _LineRenderers[n].endWidth = endWidth;

            int vertexCnt = _VertexCounts[n];
            var propagationOptions = arcs[n].propagationOptions;
            if (propagationOptions.propagationType == PropagationType.globalSpaceSpeed)
            {
                var cnt = Mathf.Min(_VertexCounts[n] * propagationOptions.globalSpeed * _ElapsedTime / _ArcShapeLength, _VertexCounts[n]);
                vertexCnt = Mathf.CeilToInt(cnt);
                _LineRenderers[n].positionCount = vertexCnt;
            }

            UpdateArcShape(n);

            var curVertexPos = CalcArcPoint(0, n);
            var direction = Vector3.zero;
            var percent = (float)vertexCnt / _VertexCounts[n];
            for (int curVertex = 0; curVertex < vertexCnt - 1; curVertex++)
            {
                var pos = (float)curVertex / _VertexCounts[n];
                var nextVertexPos = CalcArcPoint((float)(curVertex + 1) / _VertexCounts[n], n);
                direction = nextVertexPos - curVertexPos;
                _Vertices[n][curVertex] = curVertexPos
                                        + CalculateOscillationShift(direction, pos * _ArcShapeLength, n) * GetShiftCoef(pos);
                _LineRenderers[n].SetPosition(curVertex, _Vertices[n][curVertex]);
                curVertexPos = nextVertexPos;
            }

            if (vertexCnt > 0 && vertexCnt <= _VertexCounts[n])
            {
                _Vertices[n][vertexCnt - 1] = CalculateOscillationShift(direction, _ArcShapeLength * percent, n) * GetShiftCoef(percent)
                                                               + CalcArcPoint(percent, n);
                _LineRenderers[n].SetPosition(vertexCnt - 1, _Vertices[n][vertexCnt - 1]);
            }

            //Particles emissions
            var emissionOptions = arcs[n].emissionOptions;
            for (int i = 0; i < arcs[n].emissionOptions.Length; i++)
            {
                if (emissionOptions[i].emit)
                {
                    int particleCount = (int)(UnityEngine.Random.value + percent * _ArcShapeLength * emissionOptions[i].particlesPerMeter * Time.deltaTime * emissionOptions[i].emissionDuringLifetime.Evaluate(lifetimePos));
                    var radiusCoef = emissionOptions[i].radiusCoefDuringLifetime.Evaluate(lifetimePos);
                    var directionCoef = emissionOptions[i].directionDuringLifetime.Evaluate(lifetimePos);

                    Vector3 spaceShiftVect;
                    if (_EmitterSystems[n][i].main.simulationSpace == ParticleSystemSimulationSpace.Local)
                        spaceShiftVect = -_EmitterSystems[n][i].transform.position;
                    else
                        spaceShiftVect = Vector3.zero;

                    var mainModel = _EmitterSystems[n][i].main;
                    var emitStartColor = mainModel.startColor.color;
                    var emitEndColor = emitStartColor;

                    for (int q = 1; q <= particleCount; q++)
                    {
                        var rand = 0.001f + UnityEngine.Random.value * (percent - 0.002f); //get random point without touching exact end of arc
                        var randomVect = UnityEngine.Random.rotation * Vector3.forward;
                        var radius = Mathf.Lerp(startWidth, endWidth, rand) * radiusCoef;
                        var emitPos = GetArcPoint(rand, n);
                        var emitDir = (GetArcPoint(rand + 0.001f, n) - emitPos).normalized;
                        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                        emitParams.angularVelocity = (arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef + arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef * UnityEngine.Random.value * 2);
                        emitParams.position = emitPos + randomVect * radius + spaceShiftVect;
                        emitParams.startLifetime = mainModel.startLifetimeMultiplier * (1 - arcs[n].emissionOptions[i].randomizationOptions.lifetimeRndCoef + arcs[n].emissionOptions[i].randomizationOptions.lifetimeRndCoef * UnityEngine.Random.value);
                        emitParams.velocity = (randomVect * (1f - Mathf.Clamp01(Mathf.Abs(directionCoef))) + emitDir * directionCoef) * mainModel.startSpeedMultiplier * (1 - arcs[n].emissionOptions[i].randomizationOptions.velocityRndCoef + arcs[n].emissionOptions[i].randomizationOptions.velocityRndCoef * UnityEngine.Random.value);
                        emitParams.rotation = mainModel.startRotationMultiplier * (1 - arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef + arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef * UnityEngine.Random.value);
                        emitParams.startSize = mainModel.startSizeMultiplier * (1 - arcs[n].emissionOptions[i].randomizationOptions.sizeRndCoef + arcs[n].emissionOptions[i].randomizationOptions.sizeRndCoef * UnityEngine.Random.value);
                        emitParams.startColor = Color.Lerp(emitStartColor, emitEndColor, rand);
                        _EmitterSystems[n][i].Emit(emitParams, 1);
                    }
                }
            }
        }
    }
    #endregion

    #region PrivateFunctions
    private void UpdateSegments()
    {
        var curShapeLength = (EndTransform.position - StartTransform.position).magnitude;

        if (Mathf.Abs((_ArcShapeLength - curShapeLength) / curShapeLength) < REINIT_THRESHOLD)
            return;

        _ArcShapeLength = curShapeLength;

        var maxSegmNum = 0;
        for (int n = 0; n < arcs.Length; n++)
        {
            //Segment and vertex initialization
            var sizeOptions = arcs[n].sizeOptions;
            var segmCount = Mathf.Max((int)(_ArcShapeLength / sizeOptions.segmentLength) + sizeOptions.minNumberOfSegments, 2);
            if (segmCount > maxSegmNum) maxSegmNum = segmCount;
            _SegmentNums[n] = segmCount;
            _VertexCounts[n] = segmCount * (sizeOptions.numberOfSmoothingSegments + 1) + 1;
            _Vertices[n] = new Vector3[_VertexCounts[n]];
            _LineRenderers[n].positionCount = _VertexCounts[n];
        }
        _ArcPoints = new Vector3[arcs.Length, maxSegmNum + 2];
        _ShiftVectors = new Vector3[arcs.Length, maxSegmNum + 2];
        _ArcTangents = new Vector3[arcs.Length, maxSegmNum + 2];
        _ArcTangentsShift = new Quaternion[arcs.Length, maxSegmNum * 2 + 2];

        for (int n = 0; n < arcs.Length; n++)
        {
            UpdateArcNoise(n, true);

            for (int i = 0; i < _SegmentNums[n]; i++)
            {
                var point = (float)i / _SegmentNums[n];
                _ArcPoints[n, i] = CalcShapePoint(point) + _ShiftVectors[n, i];
            }
        }
    }

    private static Vector3 HermiteCurvePoint(float t, Vector3 p0, Vector3 p1)
    {
        var m = p1 - p0;

        float tsq = t * t;
        float tcub = t * t * t;
        return (2 * tcub - 3 * tsq + 1) * p0
                + (2 * tcub - 3 * tsq + t) * m
                + (-2 * tcub + 3 * tsq) * p1;
    }

    protected Vector3 CalculateOscillationShift(Vector3 direction, float position, int arcInd)
    {
        Vector3 sumShift = Vector3.zero;
        OscillationInfo[] oscillations = arcs[arcInd].oscillations;
        for (int i = 0; i < oscillations.Length; ++i)
        {
            OscillationInfo osc = oscillations[i];
            if (_LineRenderers[arcInd].isVisible)
            {
                float wavelength = osc.wavelength;
                float effectiveWavelength = wavelength;
                float angle = osc.phase * Mathf.Deg2Rad + (position - effectiveWavelength * ((int)(position / effectiveWavelength))) / effectiveWavelength * Mathf.PI * 2;

                float shift;
                switch (osc.type)
                {
                    case OscillationType.sine_wave:
                        shift = osc.amplitude * Mathf.Sin(angle);
                        break;
                    case OscillationType.rectangular:
                        if ((angle * Mathf.Rad2Deg) % 360 > 180)
                            shift = -osc.amplitude;
                        else
                            shift = osc.amplitude;
                        break;
                    case OscillationType.zigzag:
                        shift = osc.amplitude * (Mathf.Abs(((angle * Mathf.Rad2Deg) % 180) / 45 - 2) - 1);
                        break;
                    default:
                        shift = 0;
                        break;
                }
                Vector3 normal = Vector3.Cross(direction, Vector3.up);
                sumShift += normal.normalized * shift;
            }
        }
        return sumShift * sizeMultiplier;
    }

    protected void UpdateArcShape(int n)
    {
        UpdateArcNoise(n, (UnityEngine.Random.value > 5 * Time.deltaTime));

        for (int i = 0; i < _SegmentNums[n] + 1; i++)
            _ArcPoints[n, i] = CalcShapePoint((float)i / _SegmentNums[n]) + _ShiftVectors[n, i] * sizeMultiplier;

        if (arcs[n].sizeOptions.interpolation == InterpolationType.CatmullRom_Splines)
        {
            _ArcTangents[n, 0] = _ArcPoints[n, 1] - _ArcPoints[n, 0];
            _ArcTangents[n, _SegmentNums[n]] = _ArcPoints[n, _SegmentNums[n]] - _ArcPoints[n, _SegmentNums[n] - 1];
            for (int i = 1; i < _SegmentNums[n]; i++)
                _ArcTangents[n, i] = (_ArcPoints[n, i + 1] - _ArcPoints[n, i - 1]) / 2;
        }
    }

    protected Vector3 CalcArcPoint(float point, int n)
    {
        var st = Mathf.FloorToInt(point * _SegmentNums[n]);
        var end = st + 1;
        if (Util.IsZero(point - 1))
        {
            end = st;
            st -= 1;
        }

        if (arcs[n].sizeOptions.interpolation == InterpolationType.CatmullRom_Splines)
            return HermiteCurvePoint(point * _SegmentNums[n] - st, _ArcPoints[n, st], _ArcPoints[n, end]);

        return _ArcPoints[n, st] + (_ArcPoints[n, end] - _ArcPoints[n, st]) * (point * _SegmentNums[n] - st);
    }

    protected Vector3 CalcShapePoint(float point)
    {
        if (StartTransform == null || EndTransform == null) return Vector3.zero;
        return HermiteCurvePoint(point, StartTransform.position, EndTransform.position);
    }

    protected Vector3 GetArcPoint(float point, int arcIndex)
    {
        var pos = point * (_VertexCounts[arcIndex] - 1);
        var ind1 = Mathf.Clamp(Mathf.FloorToInt(pos), 0, _VertexCounts[arcIndex] - 1);
        var ind2 = Mathf.Clamp(Mathf.CeilToInt(pos), 0, _VertexCounts[arcIndex] - 1);
        float koef = pos - Mathf.Floor(pos);
        Vector3 vert1;
        Vector3 vert2;
        if (_Vertices[arcIndex][ind1] == Vector3.zero)
            vert1 = CalcArcPoint(point, arcIndex);
        else
            vert1 = _Vertices[arcIndex][ind1];
        if (_Vertices[arcIndex][ind2] == Vector3.zero)
            vert2 = CalcArcPoint(point, arcIndex);
        else
            vert2 = _Vertices[arcIndex][ind2];
        return vert1 * (1 - koef) + vert2 * koef;
    }

    protected float GetShiftCoef(float point)
    {
        if (easeInOutOptions.useEaseInOut)
        {
            float length = point * _ArcShapeLength;
            if (length > easeInOutOptions.distance / 2 && length < _ArcShapeLength - easeInOutOptions.distance / 2)
                return easeInOutOptions.easeInOutCurve.Evaluate(0.5f);
            if (length < easeInOutOptions.distance / 2)
                return easeInOutOptions.easeInOutCurve.Evaluate(length / easeInOutOptions.distance);

            return easeInOutOptions.easeInOutCurve.Evaluate(1 - (_ArcShapeLength - length) / easeInOutOptions.distance);
        }

        return 1;
    }

    protected void UpdateArcNoise(int segmentIdx, bool reset)
    {
        for (int i = 0; i <= _SegmentNums[segmentIdx]; i++)
        {
            var factor = GetShiftCoef((float)i / _SegmentNums[segmentIdx]);
            if (reset)
            {
                _ArcTangentsShift[segmentIdx, i * 2] = CUnityUtil.RandomXYQuaternion(30 * factor);
                _ArcTangentsShift[segmentIdx, i * 2 + 1] = CUnityUtil.RandomXYQuaternion(30 * factor);
                _ShiftVectors[segmentIdx, i] = CUnityUtil.RandomVector3(0.3f * factor);
            }
            else
            {
                _ArcTangentsShift[segmentIdx, i * 2] *= CUnityUtil.RandomXYQuaternion(10f * factor);
                _ArcTangentsShift[segmentIdx, i * 2 + 1] *= CUnityUtil.RandomXYQuaternion(10f * factor);
                _ShiftVectors[segmentIdx, i] += CUnityUtil.RandomVector3(6 * Time.deltaTime) * factor;
            }
        }
    }
    #endregion


#if ART_USE
    void Start()
    {
        SetData(StartTransform, EndTransform);
        SetActive(true);
    }
#endif

    void Update()
    {
        Tick(Time.deltaTime);
    }

    void LateUpdate()
    {
        LateTick(Time.fixedDeltaTime);
    }

}