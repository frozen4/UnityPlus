using UnityEngine;
using System.Collections.Generic;

namespace Xft
{
    public class XffectComponent : ReusableFx
    {
        #region Member
        //Editable
        public float LifeTime = -1f;
        public bool IgnoreTimeScale = false;
        public bool EditView = false;    // Client下无效
        public float Scale = 1f;
        public float InitScale = 1f;
        public bool AutoDestroy = false;  // Client下走统一管理
        public bool Paused = false;
        public const bool UpdateWhenOffScreen = false;
        public bool UseWith2DSprite = false;
        public string SortingLayerName = "Fx";
        public int SortingOrder = 0;
        public float PlaybackTime = 1f;
        public bool TimeScaleEnabled = false;
        public XCurveParam TimeScaleCurve;

        //protected bool mIsActive = false;
        protected float ElapsedTime = 0f;
        protected List<MeshRenderer> MeshList = new List<MeshRenderer>();
        protected double LastTime = 0f;
        protected double CurTime = 0f;

        private Dictionary<string, VertexPool> MatDic = new Dictionary<string, VertexPool>();
        private List<EffectLayer> EflList = new List<EffectLayer>();

        protected Camera mCamera;
        public Camera MyCamera
        {
            get
            {
                if (mCamera == null)
                    FindMyCamera();
                return mCamera;
            }
            set
            {
                mCamera = value;
            }
        }
        #endregion

        private void FindMyCamera()
        {
            int layerMask = 1 << gameObject.layer;
            if(Main.Main3DCamera != null)
            {
                if ((Main.Main3DCamera.cullingMask & layerMask) != 0)
                    mCamera = Main.Main3DCamera;
            }
            else
            {
                Camera[] cameras = GameObject.FindObjectsOfType(typeof(Camera)) as Camera[];
                foreach (var cam in cameras)
                {
                    if ((cam.cullingMask & layerMask) != 0)
                    {
                        mCamera = cam;
                        return;
                    }
                }
            }
        }

        private void Init()
        {
            if(EflList.Count == 0 && MeshList.Count == 0 && MatDic.Count == 0) return;

            EflList.Clear();
            MeshList.Clear();
            MatDic.Clear();

            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                var el = child.GetComponent<EffectLayer>();
                if (el == null)
                    continue;

                if (el.Material == null)
                {
                    Debug.LogWarning("effect layer: " + el.gameObject.name + " has no material, please assign a material first!");
                    continue;
                }

                Material mat = el.Material;
                mat.renderQueue = mat.shader.renderQueue;
                mat.renderQueue += el.Depth;
                EflList.Add(el);

                if (!MatDic.ContainsKey(mat.name))
                {
                    MeshFilter mf;
                    MeshRenderer mr;
                    CreateMeshObj(mat, out mf, out mr);
                    MeshList.Add(mr);
                    MatDic.Add(mat.name, new VertexPool(mf.sharedMesh, mat));
                }
            }

            foreach (var efl in EflList)
            {
                efl.Vertexpool = MatDic[efl.Material.name];
                efl.Init(this);
            }

            //now set each gameobject's parent.
            foreach (var obj in MeshList)
            {
                //after the editor reimported scripts, the MeshList in memory will be cleared.
                if (obj == null)
                    continue;
                obj.transform.parent = transform;

                //fixed 2012.6.25, 
                //obj.transform.localPosition = Vector3.zero;
                //obj.transform.localRotation = Quaternion.identity;
                obj.transform.position = Vector3.zero;
                obj.transform.rotation = Quaternion.identity;

                //fixed 2012.7.11, avoid the lossy scale influence the mesh object.
                Vector3 realLocalScale = Vector3.zero;
                Vector3 lossyScale = obj.transform.parent.lossyScale;
                realLocalScale.x = 1 / lossyScale.x;
                realLocalScale.y = 1 / lossyScale.y;
                realLocalScale.z = 1 / lossyScale.z;

                obj.transform.localScale = realLocalScale * Scale;
            }

            //assign vertex pool & start each effect layer
            transform.localScale = Vector3.one;
        }

        private bool IsInCameraView()
        {
            if (mCamera == null || !mCamera.gameObject.activeInHierarchy || !mCamera.enabled)
                return false;

            foreach (var mr in MeshList)
            {
                if (mr != null && mr.isVisible)
                    return true;
            }

            foreach (var el in EflList)
            {
                Vector3 cpoint = el.ClientTransform.position;
                Vector3 vpoint = MyCamera.WorldToViewportPoint(cpoint);

                if (vpoint.x >= 0 && vpoint.x <= 1 && vpoint.y >= 0 && vpoint.y <= 1 && vpoint.z > 0)
                    return true;
            }

            return false;
        }

        //note this function also modified MeshList.
        private void CreateMeshObj(Material mat, out MeshFilter meshfilter, out MeshRenderer meshrenderer)
        {
            GameObject obj = new GameObject("xftmesh " + mat.name);
            obj.layer = gameObject.layer;
            meshfilter = obj.AddComponent<MeshFilter>();

            meshrenderer = obj.AddComponent<MeshRenderer>();
            meshrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshrenderer.receiveShadows = false;
            meshrenderer.sharedMaterial = mat;

            if (UseWith2DSprite)
            {
                meshrenderer.sortingLayerName = SortingLayerName;
                meshrenderer.sortingOrder = SortingOrder;
            }

            meshfilter.sharedMesh = new Mesh();
        }

        private void UpdateMeshObj()
        {
            //make sure each mesh position is always zero.
            foreach (var mr in MeshList)
            {
                if (mr != null)
                {
                    mr.transform.position = Vector3.zero;
                    mr.transform.rotation = Quaternion.identity;
                }
            }

            var enumerator = MatDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                // loop body goes here
                element.Value.LateUpdate();
            }
            enumerator.Dispose();
        }

        #region API

        public override void SetActive(bool active)
        {
            if (enabled ==  active) return;

            if (active)
            {
                FindMyCamera();
                InitScale = Scale;
                Init();
                LastTime = Time.realtimeSinceStartup;
            }
            else
            {
                foreach (var el in EflList)
                    el.Reset();
            }

            ElapsedTime = 0f;
            foreach (var mr in MeshList)
            {
                if (mr != null)
                    mr.enabled = active;
            }

            base.SetActive(active);
        }

        public override void Tick(float dt)
        {
            CurTime = Time.realtimeSinceStartup;

            float deltaTime = (float)(CurTime - LastTime);
            ElapsedTime += deltaTime;

            //simple method to check game delay: the game must run above 10 FPS.
            if (deltaTime > 0.1f) deltaTime = 0.0333f;

            if (Paused)
            {
                LastTime = CurTime;
                return;
            }

            if (!UpdateWhenOffScreen && !IsInCameraView())
            {
                LastTime = CurTime;
                return;
            }

            if (!IgnoreTimeScale)
                deltaTime *= Time.timeScale;

            foreach (var el in EflList)
            {
                if (ElapsedTime > el.StartTime)
                    el.Tick(deltaTime);
            }

            LastTime = CurTime;
        }

        public override void LateTick(float dt)
        {
            if (!UpdateWhenOffScreen && !IsInCameraView())
                return;

            UpdateMeshObj();

            if (ElapsedTime > LifeTime && LifeTime >= 0)
            {
                SetActive(false);
            }
            else if (LifeTime < 0 && EflList.Count > 0)
            {
                //Xffect LifeTime < 0， 且又是EmitByRate的话，会自动判断是否已经没有活动节点，没有则自动Deactive()。
                float deltaTime = (float)(CurTime - LastTime);
                bool allEfLFinished = true;

                foreach (var el in EflList)
                {
                    if (!el.EmitOver(deltaTime))
                        allEfLFinished = false;
                }

                if (allEfLFinished)
                    SetActive(false);
            }
        }
        #endregion

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

        void LateUpdate()
        {
            LateTick(Time.deltaTime);
        }
    }
}