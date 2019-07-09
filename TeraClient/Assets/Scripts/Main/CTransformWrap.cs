using System;
using UnityEngine;

public class CTransformWrap
{
    private static float _StopRange = 1.0f;

    private readonly Transform _Transform;
    private Quaternion _DestRotation;
    private bool _UseGroundNormal = false;
    private Vector3 _GroundNormal = Vector3.up;

    private float _RotationLerpTime = 0f;
    private float _LerpStartTime = 0f;

    public CTransformWrap(Transform transform)
    {
        _Transform = transform;
    }

    public void SetDestDir(Vector3 vDir)
    {
        if (_Transform == null)
            return;

#if UNITY_EDITOR
        if (vDir.sqrMagnitude < 0.000004f)
        {
            Common.HobaDebuger.LogError("CTransformWrap.SetDir vDir is zero!");
        }
#endif

        vDir.Normalize();
        if (Math.Abs(Vector3.Dot(vDir, Vector3.up)) > 0.9995f)
        {
            Common.HobaDebuger.LogErrorFormat("CTransformWrap.SetDir vDir is WRONG!");
            return;
        }

        if (_UseGroundNormal)
        {
            Vector3 vRight = Vector3.Cross(Vector3.up, vDir).normalized;
            Vector3 vNormal = _GroundNormal - Vector3.Dot(_GroundNormal, vRight) * vRight;
            Vector3 vNewDir = Vector3.Cross(vRight, vNormal).normalized;
            _DestRotation = Quaternion.LookRotation(vNewDir, vNormal);
        }
        else
        {
            _DestRotation = Quaternion.LookRotation(vDir, Vector3.up);
        }

        _RotationLerpTime = Quaternion.Angle(_Transform.rotation, _DestRotation) / 360;
        if (_RotationLerpTime <= 1e-3) _RotationLerpTime = 0.01f;
        _LerpStartTime = Time.time;
    }

    public void SetUseGroundNormal(bool bUse, Vector3 vGroundNormal)
    {
        _UseGroundNormal = bUse;

        if (bUse)
            _GroundNormal = vGroundNormal;
    }

    public Vector3 GetDir()
    {
        return _Transform.rotation * Vector3.forward;
    }

    public void SetDir(Vector3 vDir)
    {
        if (_Transform == null)
            return;

#if UNITY_EDITOR
        if (vDir.sqrMagnitude < 0.000004f)
        {
            Common.HobaDebuger.LogError("CTransformWrap.SetDir vDir is zero!");
        }
#endif

        vDir.Normalize();
        if (Math.Abs(Vector3.Dot(vDir, Vector3.up)) > 0.9995f)
        {
            Common.HobaDebuger.LogErrorFormat("CTransformWrap.SetDir vDir is WRONG!");
            //_NeedAdjustDir = false;
            return;
        }

        if (_UseGroundNormal)
        {
            Vector3 vRight = Vector3.Cross(Vector3.up, vDir).normalized;
            Vector3 vNormal = _GroundNormal - Vector3.Dot(_GroundNormal, vRight) * vRight;

            vNormal = vNormal * 0.75f + Vector3.up * 0.25f;
            vNormal.Normalize();

            Vector3 vNewDir = Vector3.Cross(vRight, vNormal).normalized;
            _DestRotation = Quaternion.LookRotation(vNewDir, vNormal);
        }
        else
        {
            Vector3 vRight = Vector3.Cross(Vector3.up, vDir).normalized;
            Vector3 vNormal = Vector3.up;
            Vector3 vNewDir = Vector3.Cross(vRight, vNormal).normalized;
            _DestRotation = Quaternion.LookRotation(vNewDir, vNormal);
        }
        _Transform.rotation = _DestRotation;

        //_NeedAdjustDir = false;
    }

    public bool TickAdjustDir(float dt)
    {
        if (_Transform == null)
            return true;

        if (Quaternion.Angle(_Transform.rotation, _DestRotation) < _StopRange)
        {
            _Transform.rotation = _DestRotation;
            return true;
        }
        else
        {
            _Transform.rotation = Quaternion.Slerp(_Transform.rotation, _DestRotation, Math.Min((Time.time -_LerpStartTime)/ _RotationLerpTime, 1.0f));
            return false;
        }
    }
}

