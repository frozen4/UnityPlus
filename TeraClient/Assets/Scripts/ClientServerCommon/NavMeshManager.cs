using System;
using System.IO;
using Common;
using UnityEngine;
using LuaInterface;

using T = System.Int32;

#if SERVER_USE
// ReSharper disable once CheckNamespace
namespace Server.Game
{
    public class NavMeshManager
#else
public class NavMeshManager : Singleton<NavMeshManager>
#endif
{
    public const int MAX_SMOOTH = 4096;
    public const int MAX_NODES = 8192;          //寻路计算需要的结点数，数值不够大会导致寻路不完整
    public const float TRACE_EXTEND_DISTANCE = 0.2f;           //navmesh扩展的距离,可在navmesh外扩展一定距离以实现移动平滑

    public static readonly float[] TempAllPoints = new float[NavMeshManager.MAX_SMOOTH * 3];
    private static readonly float[] _NavMeshCheckExt = { 1, 512, 1 };

    private IntPtr _NavMesh = IntPtr.Zero;
    private IntPtr _NavQuery = IntPtr.Zero;

    private bool _IsInited = false;
    private string _NavMeshName = String.Empty;

    public string NavMeshName
    {
        get { return _NavMeshName; }
    }

    public bool Init(string strNavMeshPath)
    {
        _IsInited = false;

#if SERVER_USE
        var fullName = Path.Combine(Template.Path.BasePath, string.Format("Maps/{0}", strNavMeshPath));
            fullName =  fullName.Trim();
        if (!File.Exists(fullName))
        {
            throw new Exception($"{fullName} not exist");
        }
        var navmesh_data = File.ReadAllBytes(fullName);
#else
        var fullName = Path.Combine(EntryPoint.Instance.ResPath, HobaText.Format("Maps/{0}", strNavMeshPath));
        var navmesh_data = Util.ReadFile(fullName);
        if (navmesh_data == null)
        {
            HobaDebuger.Log(HobaText.Format("{0} not exist", fullName));
            return false;
        }
#endif

        Release();

        if (navmesh_data.Length == 0)
            return false;

        _NavMeshName = strNavMeshPath.Trim();
        Common.SCounters.Instance.Increase(EnumCountType.LoadNavMeshFromMemory);
        _NavMesh = LuaDLL.NM_LoadNavMeshFromMemory(navmesh_data, navmesh_data.Length);
        if (_NavMesh == IntPtr.Zero)
            return false;

        Common.SCounters.Instance.Increase(EnumCountType.CreateNavQuery);
        _NavQuery = LuaDLL.NM_CreateNavQuery(_NavMesh, MAX_NODES);
        if (_NavQuery == IntPtr.Zero)
            return false;

        _IsInited = true;

        return true;
    }

    public void SetAreaCosts(float distanceSqr)
    {
        var navQuery = _NavQuery;
#if !SERVER_USE
        var param = EntryPoint.Instance.GameCustomConfigParams;
        
        string shortname = System.IO.Path.GetFileNameWithoutExtension(_NavMeshName);
        var areacost = param.GetAreaCost(shortname);
        if (areacost == null)
            areacost = param.DefaultAreaCost;

        float fLimit = areacost.MinDistance * areacost.MinDistance;
        if (distanceSqr < fLimit)       //小于一定距离，不使用权重，寻路更平滑
        {
            LuaDLL.NM_SetAreaCost(navQuery, (int)SamplePolyAreas.SAMPLE_POLYAREA_GROUND, areacost.NavMeshGrassCost);
            LuaDLL.NM_SetAreaCost(navQuery, (int)SamplePolyAreas.SAMPLE_POLYAREA_GRASS, areacost.NavMeshGrassCost);
            LuaDLL.NM_SetAreaCost(navQuery, (int)SamplePolyAreas.SAMPLE_POLYAREA_ROAD, areacost.NavMeshGrassCost);
        }
        else
        {
            LuaDLL.NM_SetAreaCost(navQuery, (int)SamplePolyAreas.SAMPLE_POLYAREA_GROUND, areacost.NavMeshGroundCost);
            LuaDLL.NM_SetAreaCost(navQuery, (int)SamplePolyAreas.SAMPLE_POLYAREA_GRASS, areacost.NavMeshGrassCost);
            LuaDLL.NM_SetAreaCost(navQuery, (int)SamplePolyAreas.SAMPLE_POLYAREA_ROAD, areacost.NavMeshRoadCost);
        }
#endif
    }

    float[] _Float3_Start = new float[3];
    float[] _Float3_End = new float[3];
    float[] _Float3_Pick = new float[3];
    public bool CanNavigateTo(Vector3 startPos, Vector3 endPos, Vector3 polyPickExt, float stepSize, float slop = 0.01f)
    {
        if (!_IsInited) return false;

        _Float3_Start[0] = startPos.x;
        _Float3_Start[1] = startPos.y;
        _Float3_Start[2] = startPos.z;
        _Float3_End[0] = endPos.x;
        _Float3_End[1] = endPos.y;
        _Float3_End[2] = endPos.z;
        _Float3_Pick[0] = polyPickExt.x;
        _Float3_Pick[1] = polyPickExt.y;
        _Float3_Pick[2] = polyPickExt.z;

#if !SERVER_USE
        if (Util.IsZero(_Float3_End[1] - CUnityUtil.InvalidHeight))
            _Float3_End[1] = GetPosHeight(endPos);
#else
        _Float3_Start[1] = GetPosHeight(startPos);
        _Float3_End[1] = GetPosHeight(endPos);
#endif

        int outVertNum = TempAllPoints.Length / 3;
        bool ret = LuaDLL.NM_GetPathFindFollowPoints(_NavQuery, _Float3_Start, _Float3_End, _Float3_Pick, stepSize, slop, true, TempAllPoints, ref outVertNum);
        if (!ret || outVertNum <= 0 || outVertNum >= MAX_SMOOTH)
            return false;

        //终点判断
        float targetX = TempAllPoints[3 * (outVertNum - 1) + 0];
        float targetZ = TempAllPoints[3 * (outVertNum - 1) + 2];
        if (Utilities.SquareDistanceH(endPos.x, endPos.z, targetX, targetZ) > 1.0f)
            return false;

        return true;
    }

    float[] _Float3_Start100 = new float[3];
    float[] _Float3_End100 = new float[3];
    float[] _Float3_Pick100 = new float[3];

    //可以判断本navmesh以外的navmesh
    public bool CanNavigateTo(string strNavMeshPath, Vector3 startPos, Vector3 endPos, Vector3 polyPickExt, float stepSize, float slop = 0.01f)
    {
        if (strNavMeshPath == _NavMeshName && !string.IsNullOrEmpty(_NavMeshName))
            return CanNavigateTo(startPos, endPos, polyPickExt, stepSize, slop);

#if SERVER_USE
            var fullName = Path.Combine(Template.Path.BasePath, string.Format("Maps/{0}", strNavMeshPath));
            if (!File.Exists(fullName))
            {
                return false;
            }
            var navmesh_data = File.ReadAllBytes(fullName);
#else
        var fullName = Path.Combine(EntryPoint.Instance.ResPath, HobaText.Format("Maps/{0}", strNavMeshPath));
        var navmesh_data = Util.ReadFile(fullName);
        if (navmesh_data == null)
        {
            return false;
        }
#endif
        Common.SCounters.Instance.Increase(EnumCountType.LoadNavMeshFromMemory);
        IntPtr navMesh = LuaDLL.NM_LoadNavMeshFromMemory(navmesh_data, navmesh_data.GetLength(0));
        if (navMesh == IntPtr.Zero)
            return false;

        Common.SCounters.Instance.Increase(EnumCountType.CreateNavQuery);
        IntPtr navQuery = LuaDLL.NM_CreateNavQuery(navMesh, MAX_NODES);
        if (navQuery == IntPtr.Zero)
        {
            Common.SCounters.Instance.Increase(EnumCountType.ClearNavMesh);
            LuaDLL.NM_ClearNavMesh(navMesh);
            return false;
        }

        _Float3_Start100[0] = startPos.x;
        _Float3_Start100[1] = startPos.y;
        _Float3_Start100[2] = startPos.z;
        _Float3_End100[0] = endPos.x;
        _Float3_End100[1] = endPos.y;
        _Float3_End100[2] = endPos.z;
        _Float3_Pick100[0] = polyPickExt.x;
        _Float3_Pick100[1] = polyPickExt.y;
        _Float3_Pick100[2] = polyPickExt.z;

#if !SERVER_USE
        if (Util.IsZero(_Float3_End100[1] - CUnityUtil.InvalidHeight))
            _Float3_End100[1] = GetPosHeight(navQuery, endPos);
#else
            _Float3_Start100[1] = GetPosHeight(navQuery, startPos);
            _Float3_End100[1] = GetPosHeight(navQuery, endPos);
#endif

        int outVertNum = TempAllPoints.Length / 3;
        bool ret = LuaDLL.NM_GetPathFindFollowPoints(navQuery, _Float3_Start100, _Float3_End100, _Float3_Pick100, stepSize, slop, true, TempAllPoints, ref outVertNum);
        if (!ret || outVertNum <= 0 || outVertNum >= MAX_SMOOTH)
        {
            Common.SCounters.Instance.Increase(EnumCountType.ClearNavQuery);
            Common.SCounters.Instance.Increase(EnumCountType.ClearNavMesh);
            LuaDLL.NM_ClearNavQuery(navQuery);
            LuaDLL.NM_ClearNavMesh(navMesh);
            return false;
        }

        //终点判断
        float targetX = TempAllPoints[3 * (outVertNum - 1) + 0];
        float targetZ = TempAllPoints[3 * (outVertNum - 1) + 2];
        if (Utilities.SquareDistanceH(endPos.x, endPos.z, targetX, targetZ) > 1.0f)
        {
            Common.SCounters.Instance.Increase(EnumCountType.ClearNavQuery);
            Common.SCounters.Instance.Increase(EnumCountType.ClearNavMesh);
            LuaDLL.NM_ClearNavQuery(navQuery);
            LuaDLL.NM_ClearNavMesh(navMesh);
            return false;
        }
        Common.SCounters.Instance.Increase(EnumCountType.ClearNavMesh);
        Common.SCounters.Instance.Increase(EnumCountType.ClearNavQuery);
        LuaDLL.NM_ClearNavQuery(navQuery);
        LuaDLL.NM_ClearNavMesh(navMesh);

        return true;
    }

    public bool IsValidPositionStrict(Vector3 pos, bool adjustHeight = false, float fDistanceLimit = 0.6f)
    {
        if (!_IsInited) return false;

#if !SERVER_USE
        if (adjustHeight)
            pos.y = GetPosHeight(pos);
#endif

        Vector3 nearestPos = pos;
        if (!GetNearestValidPosition(pos, ref nearestPos, 1.0f))
            return false;

        float distance = Utilities.SqrDistanceH(nearestPos, pos);           //最大跨出navmesh0.6
        return distance <= fDistanceLimit * fDistanceLimit;
    }

#if SERVER_USE
    public bool IsValidPosition(Vector3 pos, bool adjustHeight = true)
    {
        return IsValidPositionStrict(pos, adjustHeight); //全部使用这个接口
    }

    public float GetPosHeightServer(Vector3 pos)
    {
        return GetPosHeight(pos);
    }
#endif
   
    //可以判断本navmesh以外的navmesh
    public bool IsValidPositionStrict(string strGameResPath, string strNavMeshPath, Vector3 pos, bool adjustHeight = true, float fDistanceLimit = 0.0f)
    {
        if (strNavMeshPath == _NavMeshName && !string.IsNullOrEmpty(_NavMeshName))
            return IsValidPositionStrict(_NavQuery, pos, adjustHeight, fDistanceLimit);

        string fullName = Path.Combine(strGameResPath, "Maps/") + strNavMeshPath;
        try
        {
            var navmesh_data = File.ReadAllBytes(fullName);
            Common.SCounters.Instance.Increase(EnumCountType.LoadNavMeshFromMemory);
            IntPtr navMesh = LuaDLL.NM_LoadNavMeshFromMemory(navmesh_data, navmesh_data.GetLength(0));
            if (navMesh == IntPtr.Zero)
                return false;

            Common.SCounters.Instance.Increase(EnumCountType.CreateNavQuery);
            IntPtr navQuery = LuaDLL.NM_CreateNavQuery(navMesh, MAX_NODES);
            if (navQuery == IntPtr.Zero)
                return false;

            bool ret = IsValidPositionStrict(navQuery, pos, adjustHeight, fDistanceLimit);
            Common.SCounters.Instance.Increase(EnumCountType.ClearNavMesh);
            Common.SCounters.Instance.Increase(EnumCountType.ClearNavQuery);
            LuaDLL.NM_ClearNavQuery(navQuery);
            LuaDLL.NM_ClearNavMesh(navMesh);

            return ret;
        }
        catch (Exception e)
        {
#if !SERVER_USE
                HobaDebuger.LogWarningFormat("raise Exception {1} when ReadAllBytes {0}", fullName, e);
#endif
            return false;
        }
    }

    float[] _Float3_Start200 = new float[3];
    float[] _Float3_End200 = new float[3];
    float[] _Float3_Pick200 = new float[3];
    public bool GetWayPointsAtCurrentMap(Vector3 startPos, Vector3 endPos, Vector3 polyPickExt, float stepSize, float slop, float[] pathPoints, out int outVertNum)
    {
        outVertNum = 0;

        Vector3 targetPos = endPos;

        _Float3_Start200[0] = startPos.x;
        _Float3_Start200[1] = startPos.y;
        _Float3_Start200[2] = startPos.z;
        _Float3_End200[0] = targetPos.x;
        _Float3_End200[1] = targetPos.y;
        _Float3_End200[2] = targetPos.z;
        _Float3_Pick200[0] = polyPickExt.x;
        _Float3_Pick200[1] = polyPickExt.y;
        _Float3_Pick200[2] = polyPickExt.z;

#if !SERVER_USE
        if (Util.IsZero(_Float3_End200[1] - CUnityUtil.InvalidHeight))
            _Float3_End200[1] = GetPosHeight(targetPos);
#else
        _Float3_Start200[1] = GetPosHeight(startPos);
        _Float3_End200[1] = GetPosHeight(targetPos);
#endif

#if !SERVER_USE
        SetAreaCosts(Util.SquareDistanceH(startPos, endPos));
#endif

        int nVerts = pathPoints.Length / 3;
        bool ret = LuaDLL.NM_GetPathFindFollowPoints(_NavQuery, _Float3_Start200, _Float3_End200, _Float3_Pick200, stepSize, slop, true, pathPoints, ref nVerts);

        if (ret && nVerts < MAX_SMOOTH)
        {
            outVertNum = nVerts;
            return true;
        }

        return false;
    }

    private float[] _Float3_Start300 = new float[3];
    private float[] _Float3_End300 = new float[3];
    private float[] _Float3_Pick300 = new float[3];

    //可以判断本navmesh以外的navmesh
    public bool GetWayPoints(string strNavMeshPath, Vector3 startPos, Vector3 endPos, Vector3 polyPickExt, float stepSize, float slop, float[] pathPoints, out int outVertNum)
    {
        outVertNum = 0;

        if (strNavMeshPath == _NavMeshName && !string.IsNullOrEmpty(_NavMeshName))
            return GetWayPointsAtCurrentMap(startPos, endPos, polyPickExt, stepSize, slop, pathPoints, out outVertNum);

#if SERVER_USE
            var fullName = Path.Combine(Template.Path.BasePath, string.Format("Maps/{0}", strNavMeshPath));
            if (!File.Exists(fullName))
            {
                return false;
            }
            var navmesh_data = File.ReadAllBytes(fullName);
#else
        var fullName = Path.Combine(EntryPoint.Instance.ResPath, HobaText.Format("Maps/{0}", strNavMeshPath));
        var navmesh_data = Util.ReadFile(fullName);
        if (navmesh_data == null)
            return false;
#endif
        Common.SCounters.Instance.Increase(EnumCountType.LoadNavMeshFromMemory);
        IntPtr navMesh = LuaDLL.NM_LoadNavMeshFromMemory(navmesh_data, navmesh_data.GetLength(0));
        if (navMesh == IntPtr.Zero)
            return false;
        Common.SCounters.Instance.Increase(EnumCountType.CreateNavQuery);
        IntPtr navQuery = LuaDLL.NM_CreateNavQuery(navMesh, MAX_NODES);
        if (navQuery == IntPtr.Zero)
            return false;

        Vector3 targetPos = endPos;

        _Float3_Start300[0] = startPos.x;
        _Float3_Start300[1] = startPos.y;
        _Float3_Start300[2] = startPos.z;
        _Float3_End300[0] = targetPos.x;
        _Float3_End300[1] = targetPos.y;
        _Float3_End300[2] = targetPos.z;
        _Float3_Pick300[0] = polyPickExt.x;
        _Float3_Pick300[1] = polyPickExt.y;
        _Float3_Pick300[2] = polyPickExt.z;

#if !SERVER_USE
        if (Util.IsZero(_Float3_End300[1] - CUnityUtil.InvalidHeight))
            _Float3_End300[1] = GetPosHeight(navQuery, targetPos);

#else
            _Float3_Start300[1] = GetPosHeight(navQuery, startPos);
            _Float3_End300[1] = GetPosHeight(navQuery, targetPos);
#endif

#if !SERVER_USE
        SetAreaCosts(Util.SquareDistanceH(startPos, endPos));
#endif
        int nVerts = pathPoints.Length;
        bool ret = LuaDLL.NM_GetPathFindFollowPoints(navQuery, _Float3_Start300, _Float3_End300, _Float3_Pick300, stepSize, slop, true, pathPoints, ref nVerts);

        if (ret && nVerts < MAX_SMOOTH)
            outVertNum = nVerts;
        Common.SCounters.Instance.Increase(EnumCountType.ClearNavMesh);
        Common.SCounters.Instance.Increase(EnumCountType.ClearNavQuery);
        LuaDLL.NM_ClearNavQuery(navQuery);
        LuaDLL.NM_ClearNavMesh(navMesh);

        return ret && nVerts < MAX_SMOOTH;
    }

#if SERVER_USE
    public IntPtr NavQuery
    {
        get { return _NavQuery; }
    }
#endif
    public bool IsInited
    {
        get { return _IsInited; }
    }

    public void Release()
    {
        if (!_IsInited)
            return;
        Common.SCounters.Instance.Increase(EnumCountType.ClearNavMesh);
        Common.SCounters.Instance.Increase(EnumCountType.ClearNavQuery);
        LuaDLL.NM_ClearNavQuery(_NavQuery);
        LuaDLL.NM_ClearNavMesh(_NavMesh);
        _IsInited = false;

        _NavMeshName = String.Empty;
        _NavQuery = IntPtr.Zero;
        _NavMesh = IntPtr.Zero;
    }

    public void GetNavMeshVertexIndexCount(out int vcount, out int icount, SamplePolyAreas area)
    {
        LuaDLL.NM_NavMeshGetVertexIndexCount(_NavMesh, out vcount, out icount, (int)area);
    }

    public void FillNavMeshVertexIndexBuffer(Vector3[] vertices, int vcount, int[] indices, int icount, SamplePolyAreas area)
    {
        float[] fvertices = new float[vcount * 3];
        LuaDLL.NM_NavMeshFillVertexIndexBuffer(_NavMesh, fvertices, vcount, indices, icount, (int)area);
        for (int i = 0; i < vcount; ++i)
        {
            vertices[i].x = fvertices[i * 3 + 0];
            vertices[i].y = fvertices[i * 3 + 1];
            vertices[i].z = fvertices[i * 3 + 2];
        }
    }

    private bool IsValidPositionStrict(IntPtr navQuery, Vector3 pos, bool adjustHeight = true, float fDistanceLimit = 0.0f)
    {
        if (adjustHeight)
            pos.y = GetPosHeight(navQuery, pos);

        Vector3 nearestPos = new Vector3();
        if (!GetNearestValidPosition(navQuery, pos, ref nearestPos, 1.0f))
            return false;

        float distance = Utilities.SqrDistanceH(nearestPos, pos);           //最大跨出navmesh0.6
        return distance <= fDistanceLimit * fDistanceLimit;
    }

    public bool IsConnected(Vector3 startPos, Vector3 endPos)
    {
        float t = 0;
        return IsConnected(startPos, endPos, ref t);
    }

    public bool IsConnected(Vector3 startPos, Vector3 endPos, ref float t)
    {
        if (!IsValidPositionStrict(startPos, false))
            return false;

        float stepSize = 0.5f;
        Vector3 delta = endPos - startPos;
        delta.y = 0;
        float length = delta.magnitude;
        if (length < stepSize)
            return IsValidPositionStrict(endPos, false);

        t = 0;
        Vector3 dir = delta.normalized;
        float curLen = 0;
        while (curLen + stepSize < length)
        {
            float lastLen = curLen;
            curLen += stepSize;
            Vector3 cur = startPos + dir * curLen;

            t = lastLen / length;
            if (!IsValidPositionStrict(cur, false))
                return false;
        }
        return IsValidPositionStrict(endPos, false);
    }

    private float[] _Float3_Start400 = new float[3];
    private float[] _Float3_Pick400 = new float[3];
    private float[] _Float3_Point400 = new float[3];
    public bool GetNearestValidPosition(Vector3 pos, ref Vector3 nerestPt, float fSearchScale = 1.0f)
    {
        if (!_IsInited) return false;

        _Float3_Start400[0] = pos.x;
        _Float3_Start400[1] = pos.y;
        _Float3_Start400[2] = pos.z;
        _Float3_Pick400[0] = _NavMeshCheckExt[0] * fSearchScale;
        _Float3_Pick400[1] = _NavMeshCheckExt[1];
        _Float3_Pick400[2] = _NavMeshCheckExt[2] * fSearchScale;

        bool ret = LuaDLL.NM_GetNearestValidPosition(_NavQuery, _Float3_Start400, _Float3_Pick400, _Float3_Point400);
        nerestPt.x = _Float3_Point400[0];
        nerestPt.y = _Float3_Point400[1];
        nerestPt.z = _Float3_Point400[2];
        return ret;
    }

    private float[] _Float3_Start500 = new float[3];
    private float[] _Float3_Pick500 = new float[3];
    private float[] _Float3_Point500 = new float[3];
    private bool GetNearestValidPosition(IntPtr navQuery, Vector3 pos, ref Vector3 nerestPt, float fSearchScale = 1.0f)
    {
        _Float3_Start500[0] = pos.x;
        _Float3_Start500[1] = pos.y;
        _Float3_Start500[2] = pos.z;
        _Float3_Pick500[0] = _NavMeshCheckExt[0] * fSearchScale;
        _Float3_Pick500[1] = _NavMeshCheckExt[1];
        _Float3_Pick500[2] = _NavMeshCheckExt[2] * fSearchScale;

        bool ret = LuaDLL.NM_GetNearestValidPosition(navQuery, _Float3_Start500, _Float3_Pick500, _Float3_Point500);
        nerestPt.x = _Float3_Point500[0];
        nerestPt.y = _Float3_Point500[1];
        nerestPt.z = _Float3_Point500[2];
        return ret;
    }

    private float[] _Float3_Start700 = new float[3];
    private float[] _Float3_End700 = new float[3];
    private float[] _Float3_Pick700 = new float[3];
    public bool GetNavDistOfTwoPoint(out float fDistance, Vector3 startPos, Vector3 endPos, Vector3 polyPickExt, float stepSize, float slop = 0.01f)
    {
        if (!_IsInited)
        {
            fDistance = 0.0f;
            return false;
        }

        _Float3_Start700[0] = startPos.x;
        _Float3_Start700[1] = startPos.y;
        _Float3_Start700[2] = startPos.z;
        _Float3_End700[0] = endPos.x;
        _Float3_End700[1] = endPos.y;
        _Float3_End700[2] = endPos.z;
        _Float3_Pick700[0] = polyPickExt.x;
        _Float3_Pick700[1] = polyPickExt.y;
        _Float3_Pick700[2] = polyPickExt.z;

        bool ret = LuaDLL.NM_GetPathFindFollowDistance(_NavQuery, _Float3_Start700, _Float3_End700, _Float3_Pick700, stepSize, slop, true, out fDistance);
        return ret;
    }


    private float[] _Float3_Point800 = new float[3];
    private float GetPosHeight(Vector3 pos)
    {
        if (!_IsInited)
            return 0.0f;

        _Float3_Point800[0] = pos.x;
        _Float3_Point800[1] = pos.y;
        _Float3_Point800[2] = pos.z;

        float fHeight = 0;
        bool ret = LuaDLL.NM_GetPosHeight(_NavQuery, _Float3_Point800, 0.01f, 1024.0f, ref fHeight);
        return ret ? fHeight : 0.0f;
    }

    private float[] _Float3_Point900 = new float[3];
    private float GetPosHeight(IntPtr navQuery, Vector3 pos)
    {
        _Float3_Point900[0] = pos.x;
        _Float3_Point900[1] = pos.y;
        _Float3_Point900[2] = pos.z;

        float fHeight = 0;
        bool ret = LuaDLL.NM_GetPosHeight(navQuery, _Float3_Point900, 0.01f, 1024.0f, ref fHeight);
        return ret ? fHeight : 0.0f;
    }

    private float[] _Float3_Start600 = new float[3];
    private float[] _Float3_End600 = new float[3];

    //stepSize: 步长，一般取1或0.5
    //slop: 坡度步长，一般取0.01
    public bool RecalcPathFindFollow(Vector3 startPos, Vector3 endPos, float stepSize, float slop, float[] smoothPath, float[] smoothDistance, ref int vertNum)
    {
        if (!_IsInited) return false;

        _Float3_Start600[0] = startPos.x;
        _Float3_Start600[1] = startPos.y;
        _Float3_Start600[2] = startPos.z;
        _Float3_End600[0] = endPos.x;
        _Float3_End600[1] = endPos.y;
        _Float3_End600[2] = endPos.z;

#if !SERVER_USE
            SetAreaCosts(Util.SquareDistanceH(startPos, endPos));
#endif

            vertNum = MAX_SMOOTH;
        bool ret = LuaDLL.NM_RecalcPathFindFollow(_NavQuery, _Float3_Start600, _Float3_End600, _NavMeshCheckExt, stepSize, slop, true, smoothPath, smoothDistance, ref vertNum);

        return (ret && vertNum < MAX_SMOOTH);
    }
}
#if SERVER_USE
}
#endif


