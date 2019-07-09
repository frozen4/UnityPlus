using UnityEngine;

// public enum SKILL_CAM_ENUM
// {
//     NONE = 0,
//     YAW_XZ = 1,
//     DESIGN_ANGLE = 2,
// 
public class CSKillActionCam : CECCamCtrl
{
    private float _AimingOffset;
    private float _CamYawAngle;
    private float _Duration;
    private float _OriAngle;
    //private SKILL_CAM_ENUM _ActionType;
    private float _BeginTime;
    private Vector3 _TargetPos;
    private Vector3 _CameraPos;
    private float _TargetDistance;
    private Quaternion _CameraStartRotation;
    private Quaternion _CameraEndRotation;

    public void SetData(float angle, float offset, float duration, float ori_angle/*, SKILL_CAM_ENUM type*/)
    {
        _AimingOffset = offset;
        _CamYawAngle = angle;
        //_ActionType = type;
        _Duration = duration;
        _BeginTime = Time.time;
        _OriAngle = ori_angle;
    }

    public override void OnDisable()
    {
        _AimingOffset = 0;
        _CamYawAngle = 0;
        _Duration = 0;
        _OriAngle = 0;
        _BeginTime = 0;
        _TargetDistance = 0;
        //_ActionType = SKILL_CAM_ENUM.NONE;
    }

    private void SetCamDefaultAngle(GameObject target)
    {
        if (null == target)
            return;
        var pos = target.transform.position;
        pos.y += 3.25f;
        _CameraTrans.transform.position = pos;
        _CameraTrans.forward = target.transform.forward;
        _CameraTrans.transform.position = _CameraTrans.transform.position + (-5) * _CameraTrans.forward.normalized;
        _CameraTrans.forward = Quaternion.Euler(_OriAngle, 0, 0) * (_CameraTrans.forward);
    }


    public void Start(GameObject target)
    {
        if (null == target)
            return;
        SetCamDefaultAngle(target);
        Vector3 dir = target.transform.forward;
        _TargetPos = target.transform.position + dir.normalized * _AimingOffset;
        _TargetPos.y = target.transform.position.y;
        _CameraPos = _CameraTrans.position;
        _CameraPos.y = 0;

        var cam_dir = (_CameraPos - _TargetPos);
        cam_dir.y = 0;
        var dest_dir = Quaternion.Euler(0, _CamYawAngle, 0) * (cam_dir.normalized);
        _CameraStartRotation = _CameraTrans.rotation;
        _TargetDistance = Util.DistanceH(_TargetPos, _CameraPos);
        var end_pos = _TargetPos + _TargetDistance * dest_dir;
        end_pos.y = _CameraTrans.position.y;
        _CameraEndRotation = Quaternion.LookRotation((_TargetPos - end_pos).normalized, Vector3.up);     
    }

    public override bool Tick(float dt)
    {
        if (_CameraTrans == null || Time.time > (_BeginTime + _Duration))
        {        
            return false;
        }

        var cam_dir = (_CameraPos - _TargetPos);
        cam_dir.y = 0;
        var tar_move_pos = _TargetPos + _TargetDistance * (Quaternion.Euler(0, _CamYawAngle * ((Time.time - _BeginTime) / _Duration), 0) * (cam_dir.normalized));
        _CameraTrans.position = new Vector3(tar_move_pos.x, _CameraTrans.position.y, tar_move_pos.z);

        _CameraTrans.rotation = Quaternion.Slerp(_CameraStartRotation, _CameraEndRotation, (Time.time - _BeginTime) / _Duration);
        return true;
    }
}
