using UnityEngine;
using System;
using System.Collections.Generic;

//CUnityUtil中地图相关的部分
public static class CMapUtil
{
    public static bool GetMapHeightNormal(Vector3 pos, float radius, out float height, out Vector3 normal)
    {
        RaycastHit hit_info;
        if (Math.Abs(pos.y) < 1e-3)
            pos.y = 200f;
        else
            pos.y += 200.0f;

        float max_distance = Mathf.Max(300f, pos.y + 100f);
        bool ret = RayCastWithRadius(radius, pos, Vector3.down, out hit_info, max_distance, CUnityUtil.LayerMaskTerrain);
        if (ret)
        {
            height = hit_info.point.y;
            normal = hit_info.normal.normalized;
        }
        else
        {
            height = 0;
            normal = Vector3.up;
        }
        return ret;
    }

    public static bool GetMapHeight(Vector3 pos, float radius, out float height)
    {
        RaycastHit hit_info;
        if (Math.Abs(pos.y) < 1e-3)
            pos.y = 200f;
        else
            pos.y += 200.0f;

        float maxDistance = Mathf.Max(300f, pos.y + 100f);
        bool ret = RayCastWithRadius(radius, pos, Vector3.down, out hit_info, maxDistance, CUnityUtil.LayerMaskTerrain);
        if (ret)
            height = hit_info.point.y;
        else
            height = 0;

        return ret;
    }

    public static bool RayCastWithRadius(float radius, Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
    {
        if (Math.Abs(radius) < 1e-3)
            return Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
        else
            return Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask) && hitInfo.collider != null;
    }

    public static Quaternion GetMapNormalRotation(Vector3 vPos, Vector3 vDir, float fRatio)
    {
        float gHeight;
        Vector3 gNormal;
        if (!GetMapHeightNormal(vPos, 0.0f, out gHeight, out gNormal))
        {
            return Quaternion.LookRotation(vDir, Vector3.up);
        }

        Vector3 vRight = Vector3.Cross(Vector3.up, vDir).normalized;
        Vector3 vNormal = gNormal - Vector3.Dot(gNormal, vRight) * vRight;

        if (fRatio < 1.0f)
            vNormal = vNormal * fRatio + Vector3.up * (1.0f - fRatio);
        vNormal.Normalize();

        Vector3 vNewDir = Vector3.Cross(vRight, vNormal).normalized;
        return Quaternion.LookRotation(vNewDir, vNormal);
    }

    public static Quaternion GetUpNormalRotation(Vector3 vDir)
    {
        Vector3 vRight = Vector3.Cross(Vector3.up, vDir).normalized;
        Vector3 vNormal = Vector3.up;
        Vector3 vNewDir = Vector3.Cross(vRight, vNormal).normalized;
        return Quaternion.LookRotation(vNewDir, vNormal);
    }

    public static Quaternion GetMapNormalRotationWithDistance(Vector3 vPos, Vector3 vDir, float fDistance)
    {
        float gHeight;
        Vector3 gNormal;
        if (!GetMapHeightNormal(vPos, 0.0f, out gHeight, out gNormal))
        {
            return Quaternion.LookRotation(vDir, Vector3.up);
        }

        Vector3 vPos2 = vPos + vDir * fDistance;
        Vector3 gNormal2;
        if (GetMapHeightNormal(vPos2, 0.0f, out gHeight, out gNormal2))
        {
            if (Math.Abs(Vector3.Dot(gNormal2, Vector3.up)) < 0.707f)
                gNormal2 = Vector3.up;

            gNormal = gNormal * 0.5f + gNormal2 * 0.5f;
            gNormal.Normalize();
        }

        Vector3 vRight = Vector3.Cross(Vector3.up, vDir).normalized;
        Vector3 vNormal = gNormal - Vector3.Dot(gNormal, vRight) * vRight;
        Vector3 vNewDir = Vector3.Cross(vRight, vNormal).normalized;
        if(!vNewDir.IsZero())
            return Quaternion.LookRotation(vNewDir, vNormal);
        else
            return Quaternion.identity;
    }

    public static CameraTraceResult CameraTrace(int layerMask, Vector3 vCenter, float radius, Vector3 vDelta, out Vector3 vEndPos, out float fDistance)
    {
        CameraTraceResult result = CameraTraceResult.NoHit;

        RaycastHit hitInfo;
        float fMaxDistance = vDelta.magnitude;
        vDelta.Normalize();
        if (Physics.SphereCast(vCenter, radius, vDelta, out hitInfo, fMaxDistance, layerMask))
        {
            fDistance = hitInfo.distance;
            vEndPos = hitInfo.point;

            GameObject goHit = hitInfo.collider.gameObject;
            if (goHit != null)
            {
                if (goHit.layer == CUnityUtil.Layer_Terrain)
                    result = CameraTraceResult.HitTerrain;
                else //if (goHit.layer == CUnityUtil.Layer_CameraCollision)
                    result = CameraTraceResult.HitCameraCollision;
            }
        }
        else
        {
            fDistance = 0.0f;
            vEndPos = vCenter;
        }
        return result;
    }
}

