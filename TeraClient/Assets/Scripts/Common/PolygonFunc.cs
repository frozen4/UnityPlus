using System;
using System.Collections.Generic;

using TV3 = UnityEngine.Vector3;//PB.vector3;
using TV2 = UnityEngine.Vector2;//PB.vector2;

public class PolygonFunc         //TV3 = PB.Vector3, TV3 = UnityEngine.Vector3
{
    static TV3 ptLinePreV3 = default(TV3);
    static TV2 ptLinePreV2 = default(TV2);

    public static bool IsInPolygon2(float ptX, float ptZ, IList<TV3> ptList)
    {
        // int    polySides  =  how many corners the polygon has
        // float  polyX[]    =  horizontal coordinates of corners
        // float  polyY[]    =  vertical coordinates of corners
        // float  x,y       =  point to be tested
        int poly_sildes = ptList.Count;

        if (poly_sildes < 3)// 不是多边形
            return false;
     
        int i, j = poly_sildes - 1;
        bool odd_nodes = false;
        float x = ptX;
        float z = ptZ;

        for (i = 0; i < poly_sildes; i++)
        {
            if ((ptList[i].z < z && ptList[j].z >= z || ptList[j].z < z && ptList[i].z >= z) && (ptList[i].x <= x || ptList[j].x <= x))
            {
                odd_nodes ^= (ptList[i].x + (z - ptList[i].z) / (ptList[j].z - ptList[i].z) * (ptList[j].x - ptList[i].x) < x);
            }

            j = i;
        }

        return odd_nodes;
    }

    public static bool IsInPolygon2(float ptX, float ptZ, IList<TV2> ptList)
    {
        // int    polySides  =  how many corners the polygon has
        // float  polyX[]    =  horizontal coordinates of corners
        // float  polyY[]    =  vertical coordinates of corners
        // float  x,y       =  point to be tested
        int poly_sildes = ptList.Count;

        if (poly_sildes < 3)// 不是多边形
            return false;

        int i, j = poly_sildes - 1;
        bool odd_nodes = false;
        float x = ptX;
        float z = ptZ;

        for (i = 0; i < poly_sildes; i++)
        {
            if ((ptList[i].y < z && ptList[j].y >= z || ptList[j].y < z && ptList[i].y >= z) && (ptList[i].x <= x || ptList[j].x <= x))
            {
                odd_nodes ^= (ptList[i].x + (z - ptList[i].y) / (ptList[j].y - ptList[i].y) * (ptList[j].x - ptList[i].x) < x);
            }

            j = i;
        }

        return odd_nodes;
    }

    public static bool IsInPolygon(float ptX, float ptZ, IList<TV3> ptList)
    {
        int nNumLine = ptList.Count;

        if (nNumLine < 3)// 不是多边形
            return false;

        int nNumCross = 0;// 点与多边形交点个数

        for (int i = 0; i < nNumLine; i++)
        {
            if (IsCrossLine(ptX, ptZ, ptList, i))// 从检测点向右侧引出的射线是否与该边相交
                nNumCross++;
        }

        if ((nNumCross & 1) == 0)// 共有偶数个交点，则不在多边形内
            return false;
        else// 共有奇数个交点，在多边形内
            return true;
    }

    public static bool IsInPolygon(float ptX, float ptZ, IList<TV2> ptList)
    {
        int nNumLine = ptList.Count;

        if (nNumLine < 3)// 不是多边形
            return false;

        int nNumCross = 0;// 点与多边形交点个数

        for (int i = 0; i < nNumLine; i++)
        {
            if (IsCrossLine(ptX, ptZ, ptList, i))// 从检测点向右侧引出的射线是否与该边相交
                nNumCross++;
        }

        if ((nNumCross & 1) == 0)// 共有偶数个交点，则不在多边形内
            return false;
        else// 共有奇数个交点，在多边形内
            return true;
    }

    private static bool IsCrossLine(float ptX, float ptZ, IList<TV3> ptList, int nIndex)
    {
        int nNumLine = ptList.Count;

        TV3 ptLine1 = ptList[nIndex];// 线段顶点1
        TV3 ptLine2 = ptList[(nIndex + 1) % nNumLine];// 线段顶点2

        if (ptLine1.x < ptX && ptLine2.x < ptX)// 如果线段在射线左边
            return false;

        if (ptLine1.z < ptZ && ptLine2.z < ptZ)// 如果线段在射线上边
            return false;

        if (ptLine1.z > ptZ && ptLine2.z > ptZ)// 如果线段在射线下边
            return false;

        if (ptLine1.z == ptLine2.z)// 如果线段与射线在同一水平线上
            return false;

        if (ptZ == ptLine1.z)// 如果射线穿过线段顶点1
        {
            // 找到顶点1之前最近且与射线不在同一水平线上的点
            ptLinePreV3 = default(TV3);   // 顶点1之前的点
            bool bFind = false;
            for (int i = 1; i < nNumLine; i++)
            {
                ptLinePreV3 = ptList[(nIndex + nNumLine - i) % nNumLine];
                if (ptLinePreV3.z != ptZ)// 不在扫描线上
                {
                    bFind = true;
                    break;
                }
            }
            if (!bFind)// 没找到符合条件的点（多边形所有点在同一水平线上）
                return false;

            if ((ptLinePreV3.z < ptZ && ptLine2.z > ptZ)
                || (ptLinePreV3.z > ptZ && ptLine2.z < ptZ))// 如果顶点1之前点与顶点2在射线两侧
                return false;
        }

        // 计算射线与边的交点
        float fSlope = (ptLine2.x - ptLine1.x) / (ptLine2.z - ptLine1.z);
        float fCrossX = (ptZ - ptLine1.z) * fSlope + ptLine1.x;
        if (fCrossX <= ptX)//交点在测试点左侧
            return false;

        return true;
    }

    private static bool IsCrossLine(float ptX, float ptZ, IList<TV2> ptList, int nIndex)
    {
        int nNumLine = ptList.Count;

        TV2 ptLine1 = ptList[nIndex];// 线段顶点1
        TV2 ptLine2 = ptList[(nIndex + 1) % nNumLine];// 线段顶点2

        if (ptLine1.x < ptX && ptLine2.x < ptX)// 如果线段在射线左边
            return false;

        if (ptLine1.y < ptZ && ptLine2.y < ptZ)// 如果线段在射线上边
            return false;

        if (ptLine1.y > ptZ && ptLine2.y > ptZ)// 如果线段在射线下边
            return false;

        if (ptLine1.y == ptLine2.y)// 如果线段与射线在同一水平线上
            return false;

        if (ptZ == ptLine1.y)// 如果射线穿过线段顶点1
        {
            // 找到顶点1之前最近且与射线不在同一水平线上的点
            ptLinePreV2 = default(TV2);   // 顶点1之前的点
            bool bFind = false;
            for (int i = 1; i < nNumLine; i++)
            {
                ptLinePreV2 = ptList[(nIndex + nNumLine - i) % nNumLine];
                if (ptLinePreV2.y != ptZ)// 不在扫描线上
                {
                    bFind = true;
                    break;
                }
            }
            if (!bFind)// 没找到符合条件的点（多边形所有点在同一水平线上）
                return false;

            if ((ptLinePreV2.y < ptZ && ptLine2.y > ptZ)
                || (ptLinePreV2.y > ptZ && ptLine2.y < ptZ))// 如果顶点1之前点与顶点2在射线两侧
                return false;
        }

        // 计算射线与边的交点
        float fSlope = (ptLine2.x - ptLine1.x) / (ptLine2.y - ptLine1.y);
        float fCrossX = (ptZ - ptLine1.y) * fSlope + ptLine1.x;
        if (fCrossX <= ptX)//交点在测试点左侧
            return false;

        return true;
    }
}
