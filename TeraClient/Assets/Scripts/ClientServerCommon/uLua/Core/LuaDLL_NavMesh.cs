using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;

namespace LuaInterface
{
    public enum E_NAV_AGENT_STATE
    {
        None = 0,
        Search,
        Moving,
        Finished,
    }

    public enum SamplePolyAreas
    {
        SAMPLE_POLYAREA_GROUND = 0,
        SAMPLE_POLYAREA_WATER,
        SAMPLE_POLYAREA_ROAD,
        SAMPLE_POLYAREA_DOOR,
        SAMPLE_POLYAREA_GRASS,
        SAMPLE_POLYAREA_JUMP,
    } 

    [StructLayout(LayoutKind.Sequential)]
    public struct SCrowdAgentParams
    {
        public SCrowdAgentParams(float agentRadius, float agentHeight)
        {
            radius = agentRadius;
            height = agentHeight;
            maxAcceleration = 8.0f;
            maxSpeed = 3.5f;
            collisionQueryRange = radius * 8.0f;
            pathOptimizationRange = radius * 16.0f;
            separationWeight = 2.0f;
            bAnticipateTurns = true;
            bOptimizeVis = true;
            bOptimizeTopo = true;
            bObstacleAvoidance = false;
            bSeparation = false;
            obstacleAvoidanceType = 2;
        }

        public float radius;
        public float height;
        public float maxAcceleration;
        public float maxSpeed;
        public float collisionQueryRange;
        public float pathOptimizationRange;
        public float separationWeight;

        [MarshalAs(UnmanagedType.I1)]
        public bool bAnticipateTurns;

        [MarshalAs(UnmanagedType.I1)]
        public bool bOptimizeVis;

        [MarshalAs(UnmanagedType.I1)]
        public bool bOptimizeTopo;

        [MarshalAs(UnmanagedType.I1)]
        public bool bObstacleAvoidance;

        [MarshalAs(UnmanagedType.I1)]
        public bool bSeparation;

        public byte obstacleAvoidanceType;
    }

#if !UNITY_IPHONE
    [SuppressUnmanagedCodeSecurity]
#endif
    public partial class LuaDLL
    {
        //nav mesh
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NM_LoadNavMesh([MarshalAs(UnmanagedType.LPStr)]string path);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NM_LoadNavMeshFromMemory([MarshalAs(UnmanagedType.LPArray)]byte[] bytes, int numBytes);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_ClearNavMesh(IntPtr navMesh);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NM_CreateNavQuery(IntPtr navMesh, int nMaxNodes);           //maxNodes

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_ClearNavQuery(IntPtr navQuery);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_NavMeshGetVertexIndexCount(IntPtr navMesh, out int vcount, out int icount, int areaId);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_NavMeshFillVertexIndexBuffer(IntPtr navMesh, float[] vertices, int vcount, int[] indices, int icount, int areaId);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_GetNearestValidPosition(IntPtr navQuery, float[] pos, float[] ext, float[] nearest);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_RecalcPathFindFollow(IntPtr navQuery, float[] startpos, float[] endpos, float[] polypickExt, float StepSize, float slop, [MarshalAs(UnmanagedType.I1)]bool bOnlyXZ, [In, Out]float[] pathPoints, [In, Out]float[] pathDistance, ref int vertNum);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_GetPathFindFollowDistance(IntPtr navQuery, float[] startpos, float[] endpos, float[] polypickExt, float StepSize, float slop, [MarshalAs(UnmanagedType.I1)]bool bOnlyXZ, out float fDistance);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_GetPathFindFollowPoints(IntPtr navQuery, float[] startpos, float[] endpos, float[] polypickExt, float StepSize, float slop, [MarshalAs(UnmanagedType.I1)]bool bOnlyXZ, [In, Out]float[] pathPoints, ref int vertNum);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_Raycast(IntPtr navQuery, float[] startpos, float[] endpos, float[] polypickExt, ref float t);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_GetPosHeight(IntPtr navQuery, float[] pos, float fExtRadius, float fExtHeight, ref float fHeight);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_SetAreaCost(IntPtr navQuery, int areaId, float fCost);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern float NM_GetAreaCost(IntPtr navQuery, int areaId);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NM_CreateCrowd(IntPtr pNavQuery, int nMaxAgents, float agentRadius, float fHeightExt);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_ClearCrowd(IntPtr pNavCrowd);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdIsValid(IntPtr pNavCrowd);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NM_crowdGetMaxAgentCount(IntPtr pNavCrowd);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NM_crowdGetActiveAgentCount(IntPtr pNavCrowd);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdGetAgentInfo(IntPtr pNavCrowd, int idx, [In, Out]float[] pos, [In, Out]float[] targetPos, [In, Out]float[] vel);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int NM_crowdAddAgent(IntPtr pNavCrowd, float[] pos, ref SCrowdAgentParams param);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_crowdRemoveAgent(IntPtr pNavCrowd, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdSetAgentPos(IntPtr pNavCrowd, int idx, float[] pos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdGetAgentParam(IntPtr pNavCrowd, int idx, out SCrowdAgentParams param);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdUpdateAgentParam(IntPtr pNavCrowd, int idx, ref SCrowdAgentParams param);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdSetMoveTarget(IntPtr pNavCrowd, int idx, float[] pos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdCancelMove(IntPtr pNavCrowd, int idx);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_crowdSetAllMoveTarget(IntPtr pNavCrowd, float[] pos);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_crowdCancelAllMove(IntPtr pNavCrowd);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NM_crowdUpdateTick(IntPtr pNavCrowd, int deltaTime);          //deltaTime：毫秒

        //更新一个agent
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool NM_crowdUpdateOneAgent(IntPtr pNavCrowd, 
            int idx, int deltaTime, float[] pos, float[] target, float maxspeed, 
            [MarshalAs(UnmanagedType.I1)]bool bIgnoreCollision,
            [MarshalAs(UnmanagedType.I1)]bool bKeepSpeed);          //deltaTime：毫秒
        
    }
}
