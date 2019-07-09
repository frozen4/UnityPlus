using UnityEngine;
using DG.Tweening;
using LuaInterface;

public class CNpcDialogueCam : CECCamCtrl
{
    private static float CAMERA_IN_DURATION = 0.6f;
    private static float CAMERA_OUT_DURATION = 0.6f;
    private static float CAMERA_IN_FOV = 35;

    private static float CAMERA_CULL_NEAR_IN = 0.01f;
    private static float CAMERA_CULL_NEAR_OUT = 0.5f;

    private GameObject _Target;
    private Transform _NpcCamPos;
    private Transform _NpcCamLookAt;
    //private Vector3 _DestDir;

    private float _OriginalFOV = 60;

    //private static int _BlurLayer = LayerMask.NameToLayer("Blur");
    public CNpcDialogueCam()
    {
        //_DestDir = Vector3.zero;
    }

    public void SetDestDir(Vector3 vec)
    {
//         vec.y = 0;
//         if (Util.IsValidDir(vec))
//         {
//             _DestDir = vec.normalized;
//         }
//         else
//         {
//             //Common.HobaDebuger.LogWarningFormat("NpcDialogueCam SetDestDir, Invalid Dir {0}, {1}", vec.x, vec.z);
//             _DestDir = Vector3.zero;
//         }
    }

    private Vector3 _OriginPos;
    private Vector3 _OriginLookAtPos;

    enum MoveStage
    {
        NONE = -1,
        MOVING_NEAR = 0,
        LOOKING_AT,
        MOVING_FAR,
    };

    private MoveStage _CurStage = MoveStage.NONE;
    public void SetCameraLocation(GameObject target, int type, LuaFunction callback)
    {
        if (_CameraTrans == null) return;

        _Target = target;
        _CtrlType = type;

        if (_CtrlType == 1)
        {
            if (_CurStage == MoveStage.MOVING_NEAR) return;

            if (_CurStage == MoveStage.MOVING_FAR)
            {
                _CameraTrans.DOKill();
                if (MainCamera != null)
                {
                    MainCamera.DOKill();
                }
            }

            //get hang point
            GetHangPoint(_Target, ref _NpcCamPos, ref _NpcCamLookAt);

            if (_NpcCamPos == null || _NpcCamLookAt == null)
            {
                Debug.LogWarning("Npc Dialog [Camera Joint not Found!!!] At " + _Target.name);

                _NpcCamPos = new GameObject("Pos").transform;
                _NpcCamLookAt = new GameObject("LookAt").transform;
                Transform target_T = _Target.transform;
                _NpcCamPos.SetParent(target_T, false);
                _NpcCamLookAt.SetParent(target_T, false);
            }

            _CurStage = MoveStage.MOVING_NEAR;

            Vector3 dest_point = _NpcCamPos.position;
            _OriginPos = _CameraTrans.position;
            _OriginLookAtPos = CCamCtrlMan.Instance.GetLookAtPosOfGameModel();
            _OriginalFOV = MainCamera.fieldOfView;
            _CameraTrans.DOMove(dest_point, CAMERA_IN_DURATION).SetEase(Ease.OutCirc).OnUpdate(() =>
            {
                _CameraTrans.LookAt(_NpcCamLookAt);
            }).OnComplete(() =>
            {
                DynamicEffectManager.Instance.EnableDepthOfField(true, DOF_DISTANCE, DOF_RANGE, DOF_BOKEH);
                if (MainCamera != null)
                {
                    MainCamera.nearClipPlane = CAMERA_CULL_NEAR_IN;
                }

                if (callback != null)
                {
                    callback.Call();
                    callback.Release();
                }
                _CurStage = MoveStage.LOOKING_AT;
            });

            if (MainCamera != null)
            {
                MainCamera.DOFieldOfView(CAMERA_IN_FOV, CAMERA_OUT_DURATION);
            }
        }
        else if (_CtrlType == 2)
        {
            if (_CurStage == MoveStage.MOVING_FAR) return;

            if (_CurStage == MoveStage.MOVING_NEAR)
            {
                _CameraTrans.DOKill();
                MainCamera.DOKill();
            }

            _CameraTrans.DOMove(_OriginPos, CAMERA_OUT_DURATION).SetEase(Ease.OutCirc).OnUpdate(() =>
            {
                _CameraTrans.LookAt(_OriginLookAtPos, Vector3.up);
            }).OnComplete(() =>
            {
                DynamicEffectManager.Instance.EnableDepthOfField(false, DOF_DISTANCE, DOF_RANGE, DOF_BOKEH);
                if (MainCamera != null)
                {
                    MainCamera.nearClipPlane = CAMERA_CULL_NEAR_OUT;
                }

                if (callback != null)
                {
                    callback.Call();
                    callback.Release();
                }
                _CurStage = MoveStage.NONE;
            });

            if (MainCamera != null)
            {
                MainCamera.DOFieldOfView(_OriginalFOV, CAMERA_OUT_DURATION);
            }

            _CurStage = MoveStage.MOVING_FAR;
        }
    }

    public override void OnDisable()
    {
        //_CameraTrans.position = _OriginPos;
        //_CameraTrans.LookAt(_OriginLookAtPos, Vector3.up);

        DynamicEffectManager.Instance.EnableDepthOfField(false, DOF_DISTANCE, DOF_RANGE, DOF_BOKEH);

        base.OnDisable();
    }

    /// <summary>
    /// Select Camera Pos according to current playing animation
    /// </summary>
    /// <param name="t_pos">Camera Pos</param>
    /// <param name="t_lookat">LookAt Pos</param>
    private void GetHangPoint(GameObject target, ref Transform t_pos, ref Transform t_lookat)
    {
        int pos_set = 0;
        Animation anim = target.GetComponentInChildren<Animation>();
        if (anim != null)
        {
            var cam_info = target.GetComponentInChildren<NpcCameraPosInfo>();
            if (cam_info != null)
            {
                int c = cam_info.GetCount();
                for (int i = 0; i < c; i++)
                {
                    string anim_name = cam_info.GetAnimAt(i);
                    if (anim.IsPlaying(anim_name))
                    {
                        pos_set = cam_info.GetPos(anim_name);
                        break;
                    }
                }
            }
        }

        HangPointHolder hang_holder = target.GetComponentInChildren<HangPointHolder>();
        if (null != hang_holder)
        {
            GameObject g_camPos = null;
            GameObject g_camLookAt = null;

            if (pos_set == 0)
            {
                g_camPos = hang_holder.GetHangPoint(HangPointHolder.HangPointType.DCamPos);
                g_camLookAt = hang_holder.GetHangPoint(HangPointHolder.HangPointType.DCamPosLookAt);
            }
            else if (pos_set == 1)
            {
                g_camPos = hang_holder.GetHangPoint(HangPointHolder.HangPointType.DCamPos1);
                g_camLookAt = hang_holder.GetHangPoint(HangPointHolder.HangPointType.DCamPosLookAt1);
            }

            t_pos = g_camPos != null ? g_camPos.transform : null;
            t_lookat = g_camLookAt != null ? g_camLookAt.transform : null;
        }
    }

    public override bool IsAllowManualAdjust() { return false; }
}