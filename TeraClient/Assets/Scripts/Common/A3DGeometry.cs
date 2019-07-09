using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Common
{
    public class A3DAABB
    {
        public Vector3 Center = Vector3.zero;
        public Vector3 Extents = Vector3.zero;
        public Vector3 Mins = Vector3.zero;
        public Vector3 Maxs = Vector3.zero;

        public A3DAABB()
        {
            Clear();
        }

        public A3DAABB(Vector3 vMins, Vector3 vMaxs)
        {
            Mins = vMins;
            Maxs = vMaxs;
            CompleteCenterExts();
        }

        public void Clear()
        {
            Mins.Set(999999.0f, 999999.0f, 999999.0f);
            Maxs.Set(-999999.0f, -999999.0f, -999999.0f);
        }

        public 	void CompleteCenterExts()
	    {
		    Center = (Mins + Maxs) * 0.5f;
		    Extents = Maxs - Center;
	    }

        public void CompleteMinsMaxs()
        {
            Mins = Center - Extents;
            Maxs = Center + Extents;
        }

        public void AddVertex(Vector3 v)
        {
            if (v.x < Mins.x)
                Mins.x = v.x;

            if (v.x > Maxs.x)
                Maxs.x = v.x;

            if (v.y < Mins.y)
                Mins.y = v.y;

            if (v.y > Maxs.y)
                Maxs.y = v.y;

            if (v.z < Mins.z)
                Mins.z = v.z;

            if (v.z > Maxs.z)
                Maxs.z = v.z;
        }

        public void GetVertices(Vector3[] aVertPos)
        {
            Vector3 ExtX = Vector3.right * Extents.x;
            Vector3 ExtY = Vector3.up * Extents.y;
            Vector3 ExtZ = Vector3.forward * Extents.z;

            //	Left Up corner;
            aVertPos[0] = Center - ExtX + ExtY + ExtZ;
            //	right up corner;
            aVertPos[1] = aVertPos[0] + 2.0f * ExtX;
            //	right bottom corner;
            aVertPos[2] = aVertPos[1] - 2.0f * ExtZ;
            //	left bottom corner;
            aVertPos[3] = aVertPos[2] - 2.0f * ExtX;

            //	Down 4 vertex;
            //	Left up corner;
            aVertPos[4] = Center - ExtX - ExtY + ExtZ;
            //	right up corner;
            aVertPos[5] = aVertPos[4] + 2.0f * ExtX;
            //	right bottom corner;
            aVertPos[6] = aVertPos[5] - 2.0f * ExtZ;
            //	left bottom corner;
            aVertPos[7] = aVertPos[6] - 2.0f * ExtX;
        }

        public bool IsPointIn(Vector3 v)
        {
            if (v.x > Maxs.x || v.x < Mins.x ||
                v.y > Maxs.y || v.y < Mins.y ||
                v.z > Maxs.z || v.z < Mins.z)
                return false;

            return true;
        }

        public bool IsAABBIn(A3DAABB aabb)
        {
            Vector3 vDelta = aabb.Center - Center;

            vDelta.x = (float)Math.Abs(vDelta.x);
            if (vDelta.x + aabb.Extents.x > Extents.x)
                return false;

            vDelta.y = (float)Math.Abs(vDelta.y);
            if (vDelta.y + aabb.Extents.y > Extents.y)
                return false;

            vDelta.z = (float)Math.Abs(vDelta.z);
            if (vDelta.z + aabb.Extents.z > Extents.z)
                return false;

            return true;
        }

        public bool IsValid() { return Mins.x <= Maxs.x && Mins.y <= Maxs.y && Mins.z <= Maxs.z; }

        public float GetRadius() { return (Extents.x + Extents.y + Extents.z) * 0.333f; }

	    public float GetRadiusH() { return (Extents.x + Extents.z) * 0.5f; }

        public bool IntersectsWithLine(Vector3 vStart, Vector3 vEnd)
        {
	        if (!IsValid())
		        return false;

            Vector3 vLineMiddle = (vStart + vEnd) * 0.5f;
            Vector3 vLineVect = vEnd - vStart;
	        float fHalfLength = vLineVect.magnitude * 0.5f;

            Vector3 e = (Maxs - Mins) * 0.5f;
            Vector3 t = (Mins + e) - vLineMiddle;
	        float r;
            Vector3 n = vLineVect;
            n.Normalize();

            if ((Math.Abs(t.x) > e.x + fHalfLength * Math.Abs(n.x)) ||
                (Math.Abs(t.y) > e.y + fHalfLength * Math.Abs(n.y)) ||
                (Math.Abs(t.z) > e.z + fHalfLength * Math.Abs(n.z)))
		        return false;

            r = e.y * (float)Math.Abs(vLineVect.z) + e.z * (float)Math.Abs(vLineVect.y);
            if (Math.Abs(t.y * vLineVect.z - t.z * vLineVect.y) > r)
		        return false;

            r = e.x * (float)Math.Abs(vLineVect.z) + e.z * (float)Math.Abs(vLineVect.x);
            if (Math.Abs(t.z * vLineVect.x - t.x * vLineVect.z) > r)
		        return false;

            r = e.x * (float)Math.Abs(vLineVect.y) + e.y * (float)Math.Abs(vLineVect.x);
            if (Math.Abs(t.x * vLineVect.y - t.y * vLineVect.x) > r)
		        return false;

	        return true;
        }
    }

    public class A3DOBB
    {
        public Vector3 Center;
        public Vector3 XAxis;
        public Vector3 YAxis;
        public Vector3 ZAxis;
        public Vector3 Extents;
        public Vector3 ExtX;
        public Vector3 ExtY;
        public Vector3 ExtZ;

        public A3DOBB()
        {
            Clear();
        }

        public void Clear()
        {
            Center = Vector3.zero;
            XAxis = Vector3.right;
            YAxis = Vector3.up;
            ZAxis = Vector3.forward;
            Extents = Vector3.zero;
            ExtX = Vector3.zero;
            ExtY = Vector3.zero;
            ExtZ = Vector3.zero;
        }

        public void CompleteExtAxis()
        {
            ExtX = XAxis * Extents.x;
            ExtY = YAxis * Extents.y;
            ExtZ = ZAxis * Extents.z;
        }

    }
}
