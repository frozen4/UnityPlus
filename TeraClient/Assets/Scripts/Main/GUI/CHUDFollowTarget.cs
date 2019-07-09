using UnityEngine;
using UnityEngine.EventSystems;

public class CHUDFollowTarget : UIBehaviour
{
    public const float UI_NORMAL_DIST = 5f;        //this is the standrad distance to normalise the size of view
    public const float UI_CULL_NEAR_DIST = 1.8f;    //Cull..
    public const float UI_CULL_FAR_DIST = 1000f;        //Temp offwa

    //a small amount of perspective effect
    public const float UI_ADD_SCALE = 0.5f;
    public const float UI_ADD_SCALE_NEAR_DIST = 2f;
    public const float UI_ADD_SCALE_FAR_DIST = 20f;

	public GameObject FollowTarget;
    public Vector3 Offset = Vector3.zero;
    public Vector3 PreferScale = new Vector3(0.01f, 0.01f, 0.01f);

    private RectTransform _CachedRectTrans;         //Pate
    private Transform _CachedTrans;         //Model

    private Vector3 _LastAnchorPos = new Vector3(-9999, -9999, -9999);
    private Vector3 _LastCameraDir = Vector3.zero;
    private float _LastDistScale = 1f;
    private bool _LastVisible = true;

    ////LiJian Test
    //[NoToLua]
    //public static float _MaxViewAngle = 140f;
    //[NoToLua]
    //public static float cullDistance = 0f;
    //[NoToLua]
    //public static float cullZDepth = 6f;
    //private bool _IsVisible = false;
    //private static Camera UICamera = null;
    //private RectTransform _RootTrans;
    //private static int FrameCount = 0;
    //private static Matrix4x4 CameraMatrix;
    //private static Matrix4x4 ProjectMatrix;

    public void AdjustOffset(GameObject effectiveModel, float fAddHeight)
    {
        if (FollowTarget == null) return;

        if (effectiveModel == null) effectiveModel = FollowTarget;

        var height = CUnityUtil.GetModelHeight(effectiveModel);
        Offset.y = height + .2f;
        Offset.y += fAddHeight;
    }

    public void AdjustOffsetWithScale(GameObject effectiveModel, float fAddHeight, float fScale)
    {
        if (FollowTarget == null) return;

        if (effectiveModel == null) effectiveModel = FollowTarget;

        var height = CUnityUtil.GetModelHeight(effectiveModel, fScale, false);
        Offset.y = height + .2f;
        Offset.y += fAddHeight;
    }

    protected override void OnEnable()
    {
        if (_CachedTrans == null)
        {
            _CachedTrans = transform;
        }

        if (_CachedRectTrans == null)
        {
            _CachedRectTrans = _CachedTrans as RectTransform;
        }
        _LastDistScale = float.MinValue;
    }

    void LateUpdate()
    {
        Camera mainCamera = Main.Main3DCamera;
        if (mainCamera == null || mainCamera.enabled == false)
            return;

        if (FollowTarget == null)
            return;

        if (_CachedRectTrans != null)
        {
            Transform mainCameraTrans = mainCamera.transform;

			Vector3 position = FollowTarget.transform.position + Offset;
            Vector3 dir = mainCameraTrans.forward;

            Vector3 camDis = position - mainCameraTrans.position;
            float d = Vector3.Dot(camDis, dir);

            if (d < UI_CULL_FAR_DIST && d > UI_CULL_NEAR_DIST)
            {
                if ((_LastAnchorPos - position).sqrMagnitude > 0.0001f)
                {
                    _CachedRectTrans.anchoredPosition3D = position;
                    _LastAnchorPos = mainCameraTrans.position;
                }

                Vector3 dir_ref = mainCameraTrans.up + dir;
                if ((_LastCameraDir - dir_ref).sqrMagnitude > 0.0001f)
                {
                    _CachedRectTrans.forward = dir;
                    _LastCameraDir = dir_ref;
                }

                if (Mathf.Abs(_LastDistScale - d) > 0.01f)
                {
                    _LastDistScale = d;

                    //special scale add
                    float addScale = Mathf.Clamp((d - UI_ADD_SCALE_NEAR_DIST) / (UI_ADD_SCALE_FAR_DIST - UI_ADD_SCALE_NEAR_DIST + 0.0001f), 0, 1);
                    addScale = Mathf.Lerp(1f, UI_ADD_SCALE, addScale);
                    _CachedTrans.localScale = PreferScale * addScale * d / UI_NORMAL_DIST;
                }

                _LastVisible = true;
            }
            else
            {
                if (_LastVisible)
                {
                    _CachedTrans.localScale = Vector3.zero;
                    _LastVisible = false;
                }

                _LastDistScale = d;
            }
        }
        else if (_CachedTrans != null)
        {
            Vector3 dir = -CCamCtrlMan.Instance.MainCameraDir;
            if (_LastCameraDir != dir)
            {
                _CachedTrans.forward = dir;
                _LastCameraDir = dir;
            }
        }
    }
}