using UnityEngine;
using DG.Tweening;
using LuaInterface;

public class CDungeonEndCam : CECCamCtrl
{
    private static float CAMERA_IN_DURATION = 0.6f;
    private static Vector3 DELTA_POS = new Vector3(0, 1F, 0);
    private static Vector3 DELTA_ROT = new Vector3(0, 195F, 0);
    private GameObject _Target;

    private bool _IsOverwriteLocation = false;
    private Vector3 _OwPos = new Vector3(0, 0, 0);
    private Vector3 _OwRot = new Vector3(0, 0, 0);

    private float _OriginalFOV = 60;
    private float _EndFOV = 60;

    public CDungeonEndCam()
    {
    }

    enum MoveStage
    {
        NONE = -1,
        MOVING_NEAR = 0,
        LOOKING_AT,
        //MOVING_FAR,
    };
    private MoveStage _CurStage = MoveStage.NONE;

    public void SetCameraLocation(GameObject target, int type, LuaFunction callback)
    {
        if (_CameraTrans == null) return;

        _Target = target;
        _CtrlType = type;

        if (_CtrlType == 1)
        {
            if (_CurStage != MoveStage.NONE) return;

            //if (_CurStage == MoveStage.MOVING_FAR)
            //{
            //    _CameraTrans.DOKill();
            //    if (MainCamera != null)
            //    {
            //        MainCamera.DOKill();
            //    }
            //}

            _CurStage = MoveStage.MOVING_NEAR;
            Transform trans_target = _Target.transform;

            Vector3 dest_pos;
            Vector3 dest_rot;

            if (_IsOverwriteLocation)
            {
                _IsOverwriteLocation = false;

                dest_pos = trans_target.position + trans_target.TransformDirection(new Vector3(_OwPos.x, 0, _OwPos.z));
                dest_pos.y += _OwPos.y;
                dest_rot = trans_target.eulerAngles + _OwRot;
            }
            else
            {
                Vector3 trans_forward = trans_target.forward;
                trans_forward.y = 0;

                dest_pos = trans_target.position + trans_forward * 3 + DELTA_POS;
                dest_rot = trans_target.eulerAngles + DELTA_ROT;
            }

            if (MainCamera != null)
            {
                _OriginalFOV = MainCamera.fieldOfView;
                MainCamera.DOFieldOfView(_EndFOV, CAMERA_IN_DURATION);
            }

            _CameraTrans.DORotate(dest_rot, CAMERA_IN_DURATION).SetEase(Ease.OutCirc);
            _CameraTrans.DOMove(dest_pos, CAMERA_IN_DURATION).SetEase(Ease.OutCirc).OnComplete(() =>
            {
                DynamicEffectManager.Instance.EnableDepthOfField(true, DOF_DISTANCE, DOF_RANGE, DOF_BOKEH); //开启模糊效果
                if (callback != null)
                {
                    callback.Call();
                    callback.Release();
                }

                _CurStage = MoveStage.LOOKING_AT;
            });
        }
        else
        {
            Common.HobaDebuger.LogDebug("CDugeonEndCam Error Type");
        }
    }

    private void StopStage()
    {
        if (_CurStage != MoveStage.NONE)
        {
            if (_CurStage != MoveStage.LOOKING_AT)
            {
                if (_CameraTrans)
                {
                    _CameraTrans.DOKill();
                }
                if (MainCamera != null)
                {
                    MainCamera.DOKill();
                }
            }

            if (MainCamera != null)
            {
                MainCamera.fieldOfView = _OriginalFOV;
            }

            _CurStage = MoveStage.NONE;
        }
    }

    //public override bool Reset()
    //{
    //    bool ret = base.Reset();
    //    StopStage();
    //    return ret;
    //}

    public override void OnDisable()
    {
        base.OnDisable();

        DynamicEffectManager.Instance.EnableDepthOfField(false, DOF_DISTANCE, DOF_RANGE, DOF_BOKEH); //关闭模糊效果
        StopStage();
    }

    public override bool IsAllowManualAdjust() { return false; }

    public void SetParam(Vector3 v_pos, Vector3 v_rot, float f_fov)
    {
        _IsOverwriteLocation = true;
        _OwPos = v_pos;
        _OwRot = v_rot;
        _EndFOV = f_fov;
    }

}
