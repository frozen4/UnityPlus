using Common;
using UnityEngine;

public class PathFindingManager : Singleton<PathFindingManager>
{
    private readonly FootstepFile _FootStepRegionFile = new FootstepFile();

    public string FootStepFileName { get; private set; }

    public bool Init(string footstepRegionFileName)
    {
        _FootStepRegionFile.Load(footstepRegionFileName);
        FootStepFileName = footstepRegionFileName;
        return true;
    }

    public bool CanNavigateTo(Vector3 startPos, Vector3 endPos, Vector3 polyPickExt, float stepSize = 0.5f)
    {
        if (!NavMeshManager.Instance.IsInited)
            return true;

        float[] tempAllPoints = NavMeshManager.TempAllPoints;

        int outVertNum = 0;
        Vector3 targetPos = endPos;
        if (!NavMeshManager.Instance.GetWayPointsAtCurrentMap(startPos, targetPos, polyPickExt, stepSize, 0.1f, tempAllPoints, out outVertNum) || outVertNum <= 0)
            return false;

        float targetX = tempAllPoints[3 * (outVertNum - 1) + 0];
        float targetZ = tempAllPoints[3 * (outVertNum - 1) + 2];
        if (Utilities.SquareDistanceH(endPos.x, endPos.z, targetX, targetZ) > 1.0f)
            return false;

        //float ratio0 = 0;
        {
            bool bIn0 = false;
            bool bIn1 = false;

            bIn0 = IsInObstacle(startPos);
            bIn1 = IsInObstacle(tempAllPoints[0], tempAllPoints[1], tempAllPoints[2]);
            if (bIn0 || bIn1)       //line两点在region内，判断两点是否和region碰撞
            {
                //_tempVec1.Set(_tempAllPoints[0], _tempAllPoints[1], _tempAllPoints[2]);
                //if (_MapRegion.RayCastAnyRegion(startPos, _tempVec1 - startPos, ref ratio0))
                return false;
            }
        }

        int iComputed = 0;

        for (int i = 0; i < outVertNum - 1; ++i)
        {
            bool bIn0 = false;
            bool bIn1 = false;

            if (iComputed < i)
            {
                bIn0 = IsInObstacle(tempAllPoints[i * 3], tempAllPoints[i * 3 + 1], tempAllPoints[i * 3 + 2]);
                iComputed = i;
            }

            if (iComputed < i + 1)
            {
                bIn1 = IsInObstacle(tempAllPoints[(i + 1) * 3], tempAllPoints[(i + 1) * 3 + 1], tempAllPoints[(i + 1) * 3 + 2]);
                iComputed = iComputed + 1;
            }

            if (bIn0 || bIn1)       //line两点在region内，判断两点是否和region碰撞
            {
                //_tempVec0.Set(_tempAllPoints[i * 3], _tempAllPoints[i * 3 + 1], _tempAllPoints[i * 3 + 2]);
                //_tempVec1.Set(_tempAllPoints[(i + 1) * 3], _tempAllPoints[(i + 1) * 3 + 1], _tempAllPoints[(i + 1) * 3 + 2]);
                //if (_MapRegion.RayCastAnyRegion(_tempVec0, _tempVec1 - _tempVec0, ref ratio0))
                return false;
            }
        }

        return true;
    }

    private bool IsInObstacle(Vector3 pos)
    {
        var vec = new Vector3(pos.x, pos.y + 50, pos.z);
        return Physics.Raycast(vec, Vector3.down, 100, CUnityUtil.LayerMaskBlockable);
    }

    private bool IsInObstacle(float x, float y, float z)
    {
        var vec = new Vector3(x, y + 50, z);
        return Physics.Raycast(vec, Vector3.down, 100, CUnityUtil.LayerMaskBlockable);
    }

    public bool IsCollideWithBlockable(Vector3 startPos, Vector3 endPos)
    {
        RaycastHit hitInfo;
        Vector3 dir = endPos - startPos;
        float length = dir.sqrMagnitude;
        if (length > 0.01f)
        {
            dir.Normalize();
            if (CUnityUtil.RayCastWithRadius(0, startPos, dir, out hitInfo, length, CUnityUtil.LayerMaskBlockable))
                return true;
        }
        return false;
    }

    //从startPos到endPos, 是否连通，包括obstacle和navmesh
    public bool IsConnected(Vector3 startPos, Vector3 endPos, out Vector3 hitPos)
    {
        hitPos = startPos;

        RaycastHit hitInfo;
        Vector3 dir = endPos - startPos;
        float length = dir.magnitude;
        dir.Normalize();

        float ratio0 = 0;
        if (length > 0.1f)
        {
            if (CUnityUtil.RayCastWithRadius(0, startPos, dir, out hitInfo, length, CUnityUtil.LayerMaskBlockable))
            {
                ratio0 = hitInfo.distance / length;
                hitPos = startPos + (endPos - startPos) * ratio0;
                hitPos.y = CUnityUtil.GetMapHeight(hitPos);
                return false;
            }
        }

        float ratio1 = 0;
        if (NavMeshManager.Instance.IsInited && !NavMeshManager.Instance.IsConnected(startPos, endPos, ref ratio1))
        {
            if (ratio1 < ratio0)
            {
                hitPos = startPos + (endPos - startPos) * ratio1;
                hitPos.y = CUnityUtil.GetMapHeight(hitPos);
            }

            return false;
        }

        return true;
    }

    public bool IsConnected(Vector3 startPos, Vector3 endPos)
    {
        //float ratio0 = 0;
        float ratio1 = 0;
        bool bHit0 = false;
        bool bHit1 = false;

        RaycastHit hitInfo;
        Vector3 dir = endPos - startPos;
        float length = dir.magnitude;
        dir.Normalize();
        if (length > 0.1f && CUnityUtil.RayCastWithRadius(0, startPos, dir, out hitInfo, length, CUnityUtil.LayerMaskBlockable))
        {
            //ratio0 = hitInfo.distance / length;
            bHit0 = true;
        }

        if (NavMeshManager.Instance.IsInited && !NavMeshManager.Instance.IsConnected(startPos, endPos, ref ratio1))
        {
            bHit1 = true;
        }

        return !bHit0 && !bHit1;
    }

    //找到寻路点上第一个和目标点连通的点
    public bool FindFirstConnectedPoint(Vector3 startPos, Vector3 endPos, Vector3 polyPickExt, float stepSize, out Vector3 selectPoint)
    {
        selectPoint = endPos;

        if (IsConnected(startPos, endPos))
        {
            selectPoint = startPos;
            return true;
        }

        if (!NavMeshManager.Instance.IsInited)
            return false;

        float[] tempAllPoints = NavMeshManager.TempAllPoints;

        int outVertNum = 0;
        Vector3 targetPos = endPos;
        if (!NavMeshManager.Instance.GetWayPointsAtCurrentMap(startPos, targetPos, polyPickExt, stepSize, 0.1f, tempAllPoints, out outVertNum) || outVertNum <= 0)
            return false;

        for (int i = 0; i < outVertNum; ++i)
        {
            var vec = new Vector3(tempAllPoints[i * 3], tempAllPoints[i * 3 + 1], tempAllPoints[i * 3 + 2]);
            vec.y = CUnityUtil.GetMapHeight(vec);

            //看是否被阻挡
            RaycastHit hitInfo;
            Vector3 dir = endPos - startPos;
            float length = dir.magnitude;  
            if (length > 0.1f)
            {
                dir.Normalize();
                if (CUnityUtil.RayCastWithRadius(0, startPos, dir, out hitInfo, length, CUnityUtil.LayerMaskBlockable))
                {
                    break;
                }
            }

            if (NavMeshManager.Instance.IsInited && NavMeshManager.Instance.IsConnected(vec, endPos))
            {
                selectPoint = vec;
                return true;
            }
        }

        return false;
    }

    public bool GetFootStepID(Vector3 pos, ref byte footstep_id)
    {
        return _FootStepRegionFile.FootstepID(pos.x, pos.z, ref footstep_id);
    }

    public void Clear()
    {
        _FootStepRegionFile.Clear();
        FootStepFileName = string.Empty;
    }
}
