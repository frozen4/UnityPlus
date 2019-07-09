using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using LuaInterface;
using System.Collections.Generic;

public class CBossActionCam : CECCamCtrl
{
    private const float CAMERA_FOV = 27.5f;

    private float _OrigionFOV = 0f;
    
    public override bool IsAllowManualAdjust() { return false; }

    public override void OnDisable()
    {
        _CameraTrans.DOKill();
        _CameraTrans.parent = null;
        if (_OrigionFOV > 0f)
            MainCamera.fieldOfView = _OrigionFOV;
    }

    public void Start(GameObject parent)
    {
        if (parent == null) return;

        _OrigionFOV = MainCamera.fieldOfView;
        MainCamera.fieldOfView = CAMERA_FOV;
        _CameraTrans.parent = parent.transform;
        _CameraTrans.localPosition = Vector3.zero;
        _CameraTrans.localRotation = Quaternion.identity;
    }

    public void StartRecoverByTween(LuaFunction callback, float duration)
    {
        _CameraTrans.parent = null;
        //if (_OrigionFOV > 0f)
        //    MainCamera.fieldOfView = _OrigionFOV;

        var game_cam = CCamCtrlMan.Instance.GetGameCamCtrl(); // 跟随相机
        float dist_offset = game_cam.DistOffset;
        float pitch_deg = game_cam.PitchDegDest;
        float yaw_deg = GetYawDeg(Main.HostPalyer.forward);
        Vector3 host_player_pos = CCamCtrlMan.Instance.GetLookAtPosOfGameModel();

        var config = EntryPoint.Instance.PlayerFollowCameraConfig;
        Vector3 camera_dir = GetDirFromPitchAndYaw(pitch_deg, yaw_deg);
        Vector3 dest_look_at_pos = _CameraTrans.position + dist_offset * camera_dir; // 视点

        float look_at_duration = config.BossCamLookAtPosRecoverDuration;
        float fov_duration = config.BossCamFOVRecoverDuration;
        float move_duration = config.BossCamPosRecoverDuration;
        if (duration > 0)
        {
            look_at_duration = duration;
            fov_duration = duration;
            move_duration = duration;
        }

        Vector3 dest_camera_pos = host_player_pos - dist_offset * camera_dir;
        dest_camera_pos = GetGameCamPosAfterCollionFix(dest_camera_pos, host_player_pos); // 相机位置

        MainCamera.DOFieldOfView(_OrigionFOV, fov_duration).SetEase(Ease.Linear);
        _CameraTrans.DOLookAt(dest_look_at_pos, look_at_duration).SetEase(Ease.Linear);
        _CameraTrans.DOMove(dest_camera_pos, move_duration).SetEase(Ease.Linear).
        OnComplete(() =>
        {
            if (callback != null)
            {
                callback.Call();
                callback.Release();
            }
        });
    }

    private float GetYawDeg(Vector3 dir)
    {
        dir.y = 0.0f;
        dir.Normalize();
        float yaw_deg_dest = Mathf.Acos(Vector3.Dot(Vector3.forward, dir)) * Mathf.Rad2Deg;
        if (dir.x < 0.0f)
        {
            yaw_deg_dest = 360.0f - yaw_deg_dest;
        }
        return yaw_deg_dest;
    }


    //获取经过地形和碰撞调整后的跟随相机的位置
    private Vector3 GetGameCamPosAfterCollionFix(Vector3 cameraPos, Vector3 lookAtPos)
    {
        var game_cam_pos = cameraPos;

        //根据地形和相机碰撞体，调整目标相机位置
        Vector3 vDelta = game_cam_pos - lookAtPos;
        Vector3 vTracePos;
        float fDistance;
        var ret = CMapUtil.CameraTrace(CUnityUtil.LayerMaskTerrainCameraCollision, lookAtPos, _CameraRadius, vDelta, out vTracePos, out fDistance);
        if (ret == CameraTraceResult.HitTerrain)
        {
            if (fDistance > 0.1f)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                game_cam_pos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                game_cam_pos = lookAtPos + vDelta * 0.1f;
            }
        }
        else if (ret == CameraTraceResult.HitCameraCollision)
        {
            if (fDistance > _MinCamCollisionDist)       //避免出现摄像机位置和碰撞位置重合的情况，这时方向会乱
            {
                game_cam_pos = vTracePos;
            }
            else
            {
                vDelta.Normalize();
                game_cam_pos = lookAtPos + vDelta * _MinCamCollisionDist;
            }
        }

        //确保在地面一定高度
        float fHeight = 0.0f;
        if (CMapUtil.GetMapHeight(game_cam_pos, _CameraRadius, out fHeight))
        {
            if (game_cam_pos.y < fHeight + _CameraTerrainHitHeight)
            {
                game_cam_pos.y = fHeight + _CameraTerrainHitHeight;
            }
        }

        return game_cam_pos;
    }
}