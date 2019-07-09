//----------------------------------------------
//            Xffect Editor
// Copyright © 2012- Shallway Studio
// http://shallway.net
//----------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace Xft
{
    #region 枚举区
    public enum STYPE
    {
        BILLBOARD,
        BILLBOARD_SELF,
        XY,
        BILLBOARD_Y,
    }

    public enum ORIPOINT
    {
        CENTER,
        LEFT_UP,
        LEFT_BOTTOM,
        RIGHT_BOTTOM,
        RIGHT_UP,
        BOTTOM_CENTER,
        TOP_CENTER,
        LEFT_CENTER,
        RIGHT_CENTER
    }

    public enum EMITTYPE
    {
        POINT,
        BOX,
        SPHERE,
        CIRCLE,
        LINE,
        Mesh,
    }

    public enum RSTYPE
    {
        NONE,
        SIMPLE,
        CURVE,
        CURVE01,
        RANDOM,
    }

    public enum MAGTYPE
    {
        Fixed,
        Curve_OBSOLETE,//deprecated
        Curve01,
    }

    public enum COLLITION_TYPE
    {
        Sphere,
        CollisionLayer,
        Plane,
    }

    public enum COLOR_GRADUAL_TYPE
    {
        CLAMP,
        LOOP,
        REVERSE,
        CURVE,
    }

    public enum WRAP_TYPE
    {
        CLAMP,
        LOOP,
        REVERSE,
    }

    public enum DIRECTION_TYPE
    {
        Planar,
        Cone,
        Sphere,
        Cylindrical,
        Spline,
    }

//     public enum RENDER_TYPE
//     {
//         Sprite,//0
//         RibbonTrail,//1
//         Cone,//2
//         CustomMesh,//3
//         Rope,//4
//         SphericalBillboard,//5
//         SplineTrail,//6
//         //Decal,//6
//     }
// 
//     public enum MESHEMIT_TYPE
//     {
//         ByVertex,
//         ByTriangle,
//     }

    public enum COLOR_CHANGE_TYPE
    {
        Constant,
        Gradient,
    }

//     public enum UVTYPE
//     {
//         None,
//         TextureSheetAnimation,
//         UVScroll,
//         UVScale,
//     }

    public enum UV_STRETCH
    {
        Vertical,
        Horizontal,
    }

//     public enum EEmitSplinePos
//     {
//         StartPoint,
//         Random,
//         Uniformly,
//     }

    #endregion

    [System.Serializable]
    public class XCurveParam
    {
        [SerializeField]
        public float MaxValue = 1f;
        [SerializeField]
        public float TimeLen = 1f;
        [SerializeField]
        public WRAP_TYPE WrapType = WRAP_TYPE.CLAMP;
        [SerializeField]
        public AnimationCurve Curve01 = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        //consider node life if TimeLen < 0;
        public float Evaluate(float time, EffectNode node)
        {

            float len = TimeLen;

            if (len < 0f)
                len = node.GetLifeTime();

            float t = time / len;

            if (t > 1f)
            {
                if (WrapType == WRAP_TYPE.CLAMP)
                {
                    t = 1f;
                }
                else if (WrapType == WRAP_TYPE.LOOP)
                {
                    int d = Mathf.FloorToInt(t);
                    t = t - (float)d;
                }
                else
                {
                    int n = Mathf.CeilToInt(t);
                    int d = Mathf.FloorToInt(t);
                    if (n % 2 == 0)
                    {
                        t = (float)n - t;
                    }
                    else
                    {
                        t = t - (float)d;
                    }
                }
            }
            return Curve01.Evaluate(t) * MaxValue;
        }

        public float Evaluate(float time)
        {
            float t = time / TimeLen;
            if (t > 1f)
            {
                if (WrapType == WRAP_TYPE.CLAMP)
                {
                    t = 1f;
                }
                else if (WrapType == WRAP_TYPE.LOOP)
                {
                    int d = Mathf.FloorToInt(t);
                    t = t - (float)d;
                }
                else
                {
                    int n = Mathf.CeilToInt(t);
                    int d = Mathf.FloorToInt(t);
                    if (n % 2 == 0)
                    {
                        t = (float)n - t;
                    }
                    else
                    {
                        t = t - (float)d;
                    }
                }
            }
            return Curve01.Evaluate(t) * MaxValue;
        }

    }

    public enum ERenderType
    {
        Sprite = 0,
        Ribbon,
        Cone,
        CustomMesh,
        //Rope,    // 当前版本中未使用

        Max = CustomMesh,
    }

    public class EffectLayer : MonoBehaviour
    {
        #region 变量区
        public VertexPool Vertexpool;

        //Main Config
        public Transform ClientTransform;
        public bool SyncClient;
        public Material Material;
        public int RenderType; //0 sprite 1 ribbon 2 cone, 3 custom mesh, 4 rope
        public float StartTime = 0f;
        public const float MaxFps = 30f;   /*deprecated This value is obsolete, default to 60*/
        public Color DebugColor = Color.white;
        public int Depth = 0;

        private ERenderType _RenderType;

        //Sprite Config
        public int SpriteType;
        public int OriPoint;
        public float SpriteWidth = 1;
        public float SpriteHeight = 1;
        public UV_STRETCH SpriteUVStretch = UV_STRETCH.Vertical;
        //public int SpriteUVStretch = 0;

        public bool RandomOriScale = false;
        public bool RandomOriRot = false;

        //Rotation Config
        public int OriRotationMin;
        public int OriRotationMax;
        public bool RotAffectorEnable = false;
        public RSTYPE RotateType = RSTYPE.NONE;
        public float DeltaRot;
        public AnimationCurve RotateCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 360));

        public WRAP_TYPE RotateCurveWrap;
        public float RotateCurveTime = 1f;
        public float RotateCurveMaxValue = 1f;
        public AnimationCurve RotateCurve01 = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        public float RotateSpeedMin = 0f;
        public float RotateSpeedMax = 0f;

        //Scale Config
        public bool UniformRandomScale = false;
        public float OriScaleXMin = 1f;
        public float OriScaleXMax = 1f;
        public float OriScaleYMin = 1f;
        public float OriScaleYMax = 1f;
        public bool ScaleAffectorEnable = false;
        public RSTYPE ScaleType = RSTYPE.NONE;
        public float DeltaScaleX = 0f;
        public float DeltaScaleY = 0f;

        //deprecated.
        public AnimationCurve ScaleXCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 5));
        public AnimationCurve ScaleYCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 5));

        public WRAP_TYPE ScaleWrapMode;
        public float ScaleCurveTime = 1f;
        public float MaxScaleCalue = 1f;
        public float MaxScaleValueY = 1f;
        public AnimationCurve ScaleXCurveNew = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve ScaleYCurveNew = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        public bool UseSameScaleCurve = false;

        public float DeltaScaleXMax = 0f;
        public float DeltaScaleYMax = 0f;

        //Color Config
        public bool ColorAffectorEnable = false;
        public float ColorGradualTimeLength = 1f;
        public COLOR_GRADUAL_TYPE ColorGradualType = COLOR_GRADUAL_TYPE.CLAMP;
        public bool IsRandomStartColor = false;
        public Gradient RandomColorGradient;
        public Color Color1 = Color.white;//start color.
        public COLOR_CHANGE_TYPE ColorChangeType = COLOR_CHANGE_TYPE.Constant;
        public Gradient ColorGradient;
        public AnimationCurve ColorGradualCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        //RibbonTrail Config
        public float RibbonWidth = 1f;
        public int MaxRibbonElements = 8;
        public float RibbonLen = 15f;
        public bool SyncTrailWithClient = false;
        public UV_STRETCH RibbonUVStretch = UV_STRETCH.Vertical;

        public bool FaceToObject = false;
        public Transform FaceObject;
        public bool UseRandomRibbon = false;
        public float RibbonWidthMin = 1f;
        public float RibbonWidthMax = 1f;
        public float RibbonLenMin = 15f;
        public float RibbonLenMax = 15f;

        //Cone Config
        public Vector2 ConeSize = new Vector2(1f, 3f);
        public float ConeAngle;
        public int ConeSegment = 12;
        public bool UseConeAngleChange = false;
        public AnimationCurve ConeDeltaAngle = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 60));

        //Custom Mesh Config
        public Mesh CMesh;
        public Vector3 MeshRotateAxis = Vector3.up;

        //Emitter Config
        public int EmitType; // 0:point 1:box, 2: sphere surface, 3: circle, 4: line. 5: mesh
        public Vector3 BoxSize;
        public Vector3 EmitPoint;
        public float Radius = 1f;
        public bool UseRandomCircle = false;
        public float CircleRadiusMin = 1f;
        public float CircleRadiusMax = 10f;
        public Vector3 CircleDir = Vector3.up;
        public bool EmitUniform = false;

        public Transform LineStartObj;
        public Transform LineEndObj;

        public int MaxENodes = 1;
        public bool IsNodeLifeLoop = true;
        public float NodeLifeMin = 1;
        public float NodeLifeMax = 1;

        public EEmitWay EmitWay = EEmitWay.ByRate;
        public XCurveParam EmitterCurveX;

        public float DiffDistance = 0.1f;
        public Mesh EmitMesh;
        public int EmitMeshType;

        public bool IsBurstEmit = false;
        public float ChanceToEmit = 100f;//deprecated.
        public float EmitDuration = 100f;
        public float EmitRate = 20;
        public int EmitLoop = -1;


        public DIRECTION_TYPE DirType = DIRECTION_TYPE.Planar;
        //public Transform DirCenter;
        public Vector3 OriVelocityAxis = Vector3.up;
        public Vector3 SpriteXYDir = Vector3.up;
        public int AngleAroundAxis;
        public bool UseRandomDirAngle = false;
        public int AngleAroundAxisMax;

        public float OriSpeed;
        //public bool AlongVelocity = false;
        public bool AlwaysSyncRotation = false;
        public bool IsRandomSpeed = false;
        public float SpeedMin = 0f;
        public float SpeedMax = 0f;

        //Jet Affector
        public bool JetAffectorEnable = false;
        public MAGTYPE JetMagType = MAGTYPE.Fixed;
        public float JetMag;
        public AnimationCurve JetCurve;
        public XCurveParam JetCurveX;

        //Vortex Affector
        public bool VortexAffectorEnable = false;
        public MAGTYPE VortexMagType = MAGTYPE.Fixed;
        public float VortexMag = 1f;
        public XCurveParam VortexCurveX;
        public AnimationCurve VortexCurve;
        public Vector3 VortexDirection = Vector3.up;
        public bool VortexInheritRotation = true;
        public Transform VortexObj;
        public bool IsFixedCircle = false;
        public bool IsRandomVortexDir = false;
        public bool IsVortexAccelerate = false;
        public float VortexAttenuation = 0f;
        public bool UseVortexMaxDistance = false;
        public float VortexMaxDistance = 0f;

        //UVRotationAffector
        public bool UVRotAffectorEnable = false;
        public bool RandomUVRotateSpeed = false;
        public float UVRotXSpeed = 0;
        public float UVRotYSpeed = 0;
        public float UVRotXSpeedMax = 0;
        public float UVRotYSpeedMax = 0;
        public Vector2 UVRotStartOffset = Vector2.zero;

        //UVScaleAffector
        public bool UVScaleAffectorEnable = false;
        public float UVScaleXSpeed = 0;
        public float UVScaleYSpeed = 0;

        //Gravity Affector
        public bool GravityAffectorEnable = false;
        public GAFTTYPE GravityAftType = GAFTTYPE.Planar;
        public MAGTYPE GravityMagType = MAGTYPE.Fixed;
        public float GravityMag;
        public AnimationCurve GravityCurve;
        public XCurveParam GravityCurveX;
        public Vector3 GravityDirection = Vector3.up;
        public Transform GravityObject;
        public bool IsGravityAccelerate = true;
        public bool GravityInheritRotation = false;

        //AirField Affector
        public bool AirAffectorEnable = false;
        public Transform AirObject;
        public MAGTYPE AirMagType = MAGTYPE.Fixed;
        public float AirMagnitude;
        public AnimationCurve AirMagCurve;
        public XCurveParam AirMagCurveX;
        public Vector3 AirDirection = Vector3.up;
        public float AirAttenuation;
        public bool AirUseMaxDistance;
        public float AirMaxDistance;
        public bool AirEnableSpread;
        public float AirSpread;
        public float AirInheritVelocity;
        public bool AirInheritRotation;

        //Bomb Affector
        public bool BombAffectorEnable = false;
        public Transform BombObject;
        public BOMBTYPE BombType = BOMBTYPE.Spherical;
        public BOMBDECAYTYPE BombDecayType = BOMBDECAYTYPE.None;
        public float BombMagnitude;
        public Vector3 BombAxis;
        public float BombDecay;

        //TurbulenceFieldAffector
        public bool TurbulenceAffectorEnable = false;
        public Transform TurbulenceObject;
        public MAGTYPE TurbulenceMagType = MAGTYPE.Fixed;
        public float TurbulenceMagnitude = 1f;
        public XCurveParam TurbulenceMagCurveX;
        public AnimationCurve TurbulenceMagCurve;
        public float TurbulenceAttenuation;
        public bool TurbulenceUseMaxDistance;
        public float TurbulenceMaxDistance;
        public Vector3 TurbulenceForce = Vector3.one;

        //DragAffector
        public bool DragAffectorEnable = false;
        public Transform DragObj;
        public bool DragUseDir = false;
        public Vector3 DragDir = Vector3.up;
        public float DragMag = 10f;
        public bool DragUseMaxDist = false;
        public float DragMaxDist = 50f;
        public float DragAtten = 0f;

        //UV Config
        public bool UVAffectorEnable = false;

        public int UVType = 0;   //0: none 1: UVAnimation, 2: UVRotate
        public Vector2 OriTopLeftUV = Vector2.zero;
        public Vector2 OriUVDimensions = Vector2.one;
        protected Vector2 UVTopLeft;
        protected Vector2 UVDimension;
        public int Cols = 1;
        public int Rows = 1;
        public int LoopCircles = -1;
        public float UVTime = 1;
        public bool RandomStartFrame = false;

        //rope config
        //public float RopeWidth = 1f;
        //public float RopeUVLen = 5f;
        //public bool RopeFixUVLen = true;

        //decal config
        public Vector3 DecalSize = new Vector3(10f, 10f, 10f);

        //sine affector
        public bool SineAffectorEnable = false;
        public MAGTYPE SineMagType = MAGTYPE.Fixed;
        public float SineMagnitude = 1f;
        public float SineTime = 1f;
        public XCurveParam SineMagCurveX;
        public Vector3 SineForce = Vector3.up;
        public bool SineIsAccelarate = false;

        //shader controller
        public bool UseShaderCurve1 = false;
        public bool UseShaderCurve2 = false;
        public bool UseFlowControl = false;
        public XCurveParam ShaderCurveX1; //displacement control
        public XCurveParam ShaderCurveX2; // dissolve control
        public float FlowSpeed = 1f;
        public float FlowStretchLength = 1f;

        protected ulong TotalAddedCount = 0;

        //time scale controller;
        protected float mElapsedTime = 0f;
        public bool TimeScaleEnabled = false;
        public XCurveParam TimeScaleCurve;
        public Emitter emitter;

        public EffectNode[] AvailableENodes;
        public EffectNode[] ActiveENodes;
        public int AvailableNodeCount;
        public Vector3 LastClientPos;

        public Camera MyCamera { get {return Owner.MyCamera;} }

        public XffectComponent Owner;
        public bool mStopped = false;

#endregion

        public List<Affector> InitAffectors(EffectNode node)
        {
            List<Affector> AffectorList = new List<Affector>();

            if (UVAffectorEnable)
            {
                UVAnimation uvAnim = new UVAnimation();
                if (UVType == 1)
                {
                    float perWidth = OriUVDimensions.x / Cols;
                    float perHeight = Mathf.Abs(OriUVDimensions.y / Rows);
                    Vector2 cellSize = new Vector2(perWidth, perHeight);
                    uvAnim.BuildUVAnim(OriTopLeftUV, cellSize, Cols, Rows, Cols * Rows);
                }
                UVDimension = uvAnim.UVDimensions[0];
                UVTopLeft = uvAnim.frames[0];

                if (uvAnim.frames.Length != 1)
                {
                    uvAnim.loopCycles = LoopCircles;
                    Affector aft = new UVAffector(uvAnim, UVTime, node, RandomStartFrame);
                    AffectorList.Add(aft);
                }
            }
            else
            {
                UVDimension = OriUVDimensions;
                UVTopLeft = OriTopLeftUV;
            }


            if (RotAffectorEnable && RotateType != RSTYPE.NONE)
            {
                Affector aft;
                if (RotateType == RSTYPE.NONE)
                    aft = new RotateAffector(DeltaRot, node);
                else
                    aft = new RotateAffector(RotateType, node);
                AffectorList.Add(aft);
            }
            if (ScaleAffectorEnable && ScaleType != RSTYPE.NONE)
            {
                Affector aft;

                if (ScaleType == RSTYPE.NONE)
                    aft = new ScaleAffector(DeltaScaleX, DeltaScaleY, node);
                else
                    aft = new ScaleAffector(ScaleType, node);

                AffectorList.Add(aft);
            }
            if (ColorAffectorEnable /*&& ColorAffectType != 0*/)
            {
                ColorAffector aft = new ColorAffector(this, node);
                AffectorList.Add(aft);
            }
            if (JetAffectorEnable)
            {
                Affector aft = new JetAffector(JetMag, JetMagType, JetCurve, node);
                AffectorList.Add(aft);
            }
            if (VortexAffectorEnable)
            {
                Affector aft;
                aft = new VortexAffector(VortexObj, VortexDirection, VortexInheritRotation, node);

                AffectorList.Add(aft);
            }
            if (UVRotAffectorEnable)
            {
                Affector aft;

                float xscroll = UVRotXSpeed;
                float yscroll = UVRotYSpeed;
                if (RandomUVRotateSpeed)
                {
                    xscroll = Random.Range(UVRotXSpeed, UVRotXSpeedMax);
                    yscroll = Random.Range(UVRotYSpeed, UVRotYSpeedMax);
                }

                aft = new UVRotAffector(xscroll, yscroll, node);
                AffectorList.Add(aft);
            }

            if (UVScaleAffectorEnable)
            {
                Affector aft = new UVScaleAffector(node);
                AffectorList.Add(aft);
            }

            if (GravityAffectorEnable)
            {
                Affector aft;
                aft = new GravityAffector(GravityAftType, IsGravityAccelerate, GravityDirection, node);
                AffectorList.Add(aft);

                if (GravityAftType == GAFTTYPE.Spherical && GravityObject == null)
                {
                    Debug.LogWarning("Gravity Object is missing, automatically set to effect layer self:" + gameObject.name);
                    GravityObject = transform;
                }

            }
            if (AirAffectorEnable)
            {
                Affector aft = new AirFieldAffector(AirObject, AirDirection, AirAttenuation, AirUseMaxDistance,
                    AirMaxDistance, AirEnableSpread, AirSpread, AirInheritVelocity, AirInheritRotation, node);
                AffectorList.Add(aft);
            }
            if (BombAffectorEnable)
            {
                Affector aft = new BombAffector(BombObject, BombType, BombDecayType, BombMagnitude, BombDecay, BombAxis, node);
                AffectorList.Add(aft);
            }
            if (TurbulenceAffectorEnable)
            {
                Affector aft = new TurbulenceFieldAffector(TurbulenceObject, TurbulenceAttenuation, TurbulenceUseMaxDistance, TurbulenceMaxDistance, node);
                AffectorList.Add(aft);
            }
            if (DragAffectorEnable)
            {
                Affector aft = new DragAffector(DragObj, DragUseDir, DragDir, DragMag, DragUseMaxDist, DragMaxDist, DragAtten, node);
                AffectorList.Add(aft);
            }

            if (SineAffectorEnable)
            {
                Affector aft = new SineAffector(node);
                AffectorList.Add(aft);
            }
            return AffectorList;
        }

        public VertexPool GetVertexPool()
        {
            return Vertexpool;
        }

        public void RemoveActiveNode(EffectNode node)
        {
            if (AvailableNodeCount == MaxENodes)
                return;

            if (ActiveENodes[node.Index] == null) //already removed
                return;

            ActiveENodes[node.Index] = null;
            AvailableENodes[node.Index] = node;
            AvailableNodeCount++;
        }

        private void AddActiveNode(EffectNode node)
        {
            if (AvailableNodeCount == 0)
                Debug.LogError("out index!");
            if (AvailableENodes[node.Index] == null) //already added
                return;
            ActiveENodes[node.Index] = node;
            AvailableENodes[node.Index] = null;
            AvailableNodeCount--;

            node.TotalIndex = TotalAddedCount++;
        }

        protected void AddNodes(int num)
        {
            int added = 0;
            for (int i = 0; i < MaxENodes; i++)
            {
                if (added == num)
                    break;

                EffectNode node = AvailableENodes[i];
                if (node != null)
                {
                    AddActiveNode(node);
                    added++;
                    emitter.SetEmitPosition(node);
                    float nodeLife = 0;
                    if (IsNodeLifeLoop)
                        nodeLife = -1;
                    else
                        nodeLife = Random.Range(NodeLifeMin, NodeLifeMax);
                    Vector3 oriDir = emitter.GetEmitRotation(node);
                    float speed = OriSpeed;
                    if (IsRandomSpeed)
                        speed = Random.Range(SpeedMin, SpeedMax);

                    Color c = Color1;

                    if (IsRandomStartColor)
                    {
                        //c = RandomColorParam.GetGradientColor(Random.Range(0f, 1f));
                        c = RandomColorGradient.Evaluate(Random.Range(0f, 1f));
                    }

                    float oriScalex = Random.Range(OriScaleXMin, OriScaleXMax);
                    float oriScaley = Random.Range(OriScaleYMin, OriScaleYMax);

                    if (UniformRandomScale)
                    {
                        oriScaley = oriScalex;
                    }

                    node.Init(oriDir.normalized, speed, nodeLife, Random.Range(OriRotationMin, OriRotationMax),
                        oriScalex, oriScaley, c, UVTopLeft, UVDimension);
                }
                else
                    continue;
            }
        }

        public void Reset()
        {
            if (ActiveENodes == null)
                return;

            for (int i = 0; i < MaxENodes; i++)
            {
                EffectNode node = ActiveENodes[i];
                if (node != null)
                {
                    node.Reset();
                    RemoveActiveNode(node);
                }
            }
            emitter.Reset();
            mStopped = false;
            TotalAddedCount = 0;
            mElapsedTime = 0f;
            LastClientPos = ClientTransform.position;
        }

        public void Tick(float deltaTime)
        {
            mElapsedTime += deltaTime;

            if (TimeScaleEnabled)
                deltaTime *= TimeScaleCurve.Evaluate(mElapsedTime);

            int needToAdd = emitter.GetNodes(deltaTime);
            AddNodes(needToAdd);

            for (int i = 0; i < MaxENodes; i++)
            {
                EffectNode node = ActiveENodes[i];
                if (node == null)
                    continue;
                node.Update(deltaTime);
            }

//             if (RenderType == (int)ERenderType.Rope)
//                 RopeDatas.Update(deltaTime);
        }

        public void Init(XffectComponent parent)
        {
            Owner = parent;
            if (Owner == null)
                Debug.Log("you must set EffectLayer to be XffectComponent's child.");


            if (ClientTransform == null)
            {
                Debug.Log("effect layer: " + gameObject.name + " haven't assign a client transform, automaticly set to itself.");
                ClientTransform = transform;
            }

            if (RenderType < 0 || RenderType > (int) ERenderType.Max)
                Debug.Log("unspported RenderType.");

            _RenderType = (ERenderType) RenderType;

            AvailableENodes = new EffectNode[MaxENodes];
            for (int i = 0; i < MaxENodes; i++)
            {
                EffectNode n = new EffectNode(i, ClientTransform, SyncClient, this);
                var afts = InitAffectors(n);
                n.SetAffectorList(afts);

                n.SetRenderType(_RenderType);
                AvailableENodes[i] = n;
            }

            ActiveENodes = new EffectNode[MaxENodes];
            AvailableNodeCount = MaxENodes;
            emitter = new Emitter(this);
            mStopped = false;
            mElapsedTime = 0f;
            LastClientPos = ClientTransform.position;
        }

        //Xffect LifeTime < 0， 且又是EmitByRate的话，会自动判断是否已经没有活动节点，没有则自动Deactive()。
        public bool EmitOver(float deltaTime)
        {
            if (ActiveENodes == null) return false;

            if (AvailableNodeCount == MaxENodes)
            {
                return (EmitWay == EEmitWay.ByRate && Mathf.Abs(emitter.EmitLoop) < 1e-5)
                       || (EmitWay == EEmitWay.ByCurve && emitter.CurveEmitDone)
                       || (EmitWay == EEmitWay.ByDistance && mStopped);
            }

            return false;
        }
    }
}