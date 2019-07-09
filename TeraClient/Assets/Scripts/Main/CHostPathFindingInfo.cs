using System.Collections.Generic;
using UnityEngine;

public class CHostPathFindingInfo : Common.Singleton<CHostPathFindingInfo>
{
    public const int MAX_SMOOTH = 4096;
    private const float STEP = 0.5f;

    private Vector3 _TargetPos = Vector3.zero;

    private readonly float[] _SmoothPath = new float[MAX_SMOOTH * 3];
    private readonly float[] _SmoothDistance = new float[MAX_SMOOTH];
    private int _VertCount = 0;
    private float _TotalDistance = 0;
    private float _DistanceCompleted = 0;

    public float TotalDistance
    {
        get { return _TotalDistance; }
    }

    public float DistanceCompleted
    {
        get { return _DistanceCompleted; }
    }

    public bool IsNavigating()
    {
        return TotalDistance > 0 && DistanceCompleted < TotalDistance;
    }

    public bool RecalcPathFindFollow(Vector3 startPos, Vector3 endPos, float slop)
    {
        if (!NavMeshManager.Instance.IsInited) return false;

        int vertNum = MAX_SMOOTH;
        bool ret = NavMeshManager.Instance.RecalcPathFindFollow(startPos, endPos, STEP, slop, _SmoothPath, _SmoothDistance, ref vertNum);

        if (ret)
        {
            _TargetPos = endPos;
            _VertCount = vertNum;
            _TotalDistance = vertNum > 0 ? _SmoothDistance[vertNum - 1] : 0.0f;

            //debug draw
            if (NavMeshRenderer.Instance.IsLineRenderEnabled)
            {
                var list = CalcSimplePath(5.0f, 0.3f);
                NavMeshRenderer.Instance.DrawLine(list.ToArray());
            }
        }
        else
        {
            _TargetPos = Vector3.zero;
            _VertCount = 0;
            _TotalDistance = 0;
        }

        return ret;
    }

    private List<Vector3> CalcSimplePath(float fStepLength, float yOffset)
    {
         var simplePath = new List<Vector3>();
        if (_VertCount == 0 || fStepLength < 0.5f) return simplePath;

        simplePath.Add(new Vector3(_SmoothPath[0], _SmoothPath[1] + yOffset, _SmoothPath[2]));       //起始点

        float fDistanceCompleted = 0.0f;
        int iPath = 0;
        
        while (fDistanceCompleted < _TotalDistance)
        {
            Vector3 vPos;
            var bArrive = GetNextNavPosition(fStepLength, ref iPath, ref fDistanceCompleted, out vPos);
            vPos.y += yOffset;
            simplePath.Add(vPos);

            if (bArrive)
                break;
        }

        return simplePath;
    }

    private bool GetNextNavPosition(float distanceMoved, ref int iPath, ref float fDistanceCompleted, out Vector3 vPos)
    {
        if (_VertCount == 0)
        {
            vPos = _TargetPos;
            return true;
        }

        bool bArrive = false;
        float distanceTotal = _TotalDistance;
        if (distanceMoved > 0.0f)
        {
            if (fDistanceCompleted >= distanceTotal)
            {
                vPos = _TargetPos;
                fDistanceCompleted = distanceTotal;
                return true;
            }

            if (distanceMoved + fDistanceCompleted >= distanceTotal)
                distanceMoved = distanceTotal - fDistanceCompleted;

            fDistanceCompleted += distanceMoved;

            bArrive = fDistanceCompleted >= distanceTotal;
            if (bArrive)
            {
                iPath = _VertCount - 1;
            }
            else
            {
                for (int i = iPath; i < _VertCount; ++i)
                {
                    if (_SmoothDistance[i] > fDistanceCompleted)
                    {
                        iPath = i;
                        break;
                    }
                }

                if (iPath >= 1)
                {
                    float f2 = _SmoothDistance[iPath];
                    float f1 = _SmoothDistance[iPath - 1];
                    Debug.Assert(!Util.IsZero(f2 - f1));
                    float r = (fDistanceCompleted - f1) / (f2 - f1);

                    Vector3 vCurrent = Vector3.zero;
                    vCurrent.x = _SmoothPath[3 * (iPath - 1)] * (1.0f - r) + _SmoothPath[3 * iPath] * r;
                    vCurrent.y = _SmoothPath[3 * (iPath - 1) + 1] * (1.0f - r) + _SmoothPath[3 * iPath + 1] * r;
                    vCurrent.z = _SmoothPath[3 * (iPath - 1) + 2] * (1.0f - r) + _SmoothPath[3 * iPath + 2] * r;
                    vPos = vCurrent;
                    return false;
                }
            }
        }

        vPos = new Vector3(_SmoothPath[3 * iPath], _SmoothPath[3 * iPath + 1], _SmoothPath[3 * iPath + 2]);
        return bArrive;
    }

    public bool GetNextNavPosition(float distanceMoved, ref Vector3 vCurPos, ref int iPath, out Vector3 vPos)
    {
        if (_VertCount == 0)
        {
            vPos = vCurPos;
            Clear();
            return true;
        }

        bool bArrive = false;
        float distanceTotal = _TotalDistance;
        if (distanceMoved >= 0.0f)
        {
            if (_DistanceCompleted >= distanceTotal)
            {
                vPos = _TargetPos;
                _DistanceCompleted = distanceTotal;
                Clear();
                return true;
            }

            if (distanceMoved + _DistanceCompleted >= distanceTotal)
                distanceMoved = distanceTotal - _DistanceCompleted;

            _DistanceCompleted += distanceMoved;

            bArrive = _DistanceCompleted >= distanceTotal;          //到达 
            if (bArrive)
            {
                iPath = _VertCount - 1;
                Clear();
            }
            else
            {
                for (int i = iPath; i < _VertCount; ++i)
                {
                    if (_SmoothDistance[i] > _DistanceCompleted)
                    {
                        iPath = i;
                        break;
                    }
                }

                if (iPath >= 1)
                {
                    float f2 = _SmoothDistance[iPath];
                    float f1 = _SmoothDistance[iPath - 1];
                    Debug.Assert(!Util.IsZero(f2 - f1));
                    float r = (_DistanceCompleted - f1) / (f2 - f1);

                    Vector3 vCurrent = Vector3.zero;
                    vCurrent.x = _SmoothPath[3 * (iPath - 1)] * (1.0f - r) + _SmoothPath[3 * iPath] * r;
                    vCurrent.y = _SmoothPath[3 * (iPath - 1) + 1] * (1.0f - r) + _SmoothPath[3 * iPath + 1] * r;
                    vCurrent.z = _SmoothPath[3 * (iPath - 1) + 2] * (1.0f - r) + _SmoothPath[3 * iPath + 2] * r;
                    vPos = vCurrent;
                    return false;
                }
            }
        }

        vPos = new Vector3(_SmoothPath[3 * iPath], _SmoothPath[3 * iPath + 1], _SmoothPath[3 * iPath + 2]);
        return bArrive;
    }

    public bool GetPointInPath(float fDistanceCompleted, out Vector3 vPos)
    {
        if (_VertCount == 0)
        {
            vPos = _TargetPos;
            return false;
        }

        if (fDistanceCompleted <= 0)
        {
            vPos = new Vector3(_SmoothPath[0], _SmoothPath[1], _SmoothPath[2]);
            return true;
        }

        if (fDistanceCompleted >= _TotalDistance)
        {
            vPos = _TargetPos;
            return true;
        }

        int iPath = 0;
        for (int i = iPath; i < _VertCount; ++i)
        {
            if (_SmoothDistance[i] > fDistanceCompleted)
            {
                iPath = i;
                break;
            }
        }

        if (iPath >= 1)
        {
            float f2 = _SmoothDistance[iPath];
            float f1 = _SmoothDistance[iPath - 1];
            Debug.Assert(f2 != f1);
            float r = (fDistanceCompleted - f1) / (f2 - f1);

            Vector3 vCurrent = Vector3.zero;
            vCurrent.x = _SmoothPath[3 * (iPath - 1)] * (1.0f - r) + _SmoothPath[3 * iPath] * r;
            vCurrent.y = _SmoothPath[3 * (iPath - 1) + 1] * (1.0f - r) + _SmoothPath[3 * iPath + 1] * r;
            vCurrent.z = _SmoothPath[3 * (iPath - 1) + 2] * (1.0f - r) + _SmoothPath[3 * iPath + 2] * r;

            vPos = vCurrent;
            return true;
        }

        vPos = new Vector3(_SmoothPath[0], _SmoothPath[1], _SmoothPath[2]);
        return false;
    }

    public void Clear()
    {
        _VertCount = 0;
        _TotalDistance = 0;
        _DistanceCompleted = 0;
    }
}

