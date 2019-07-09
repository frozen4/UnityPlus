//----------------------------------------------
//            Xffect Editor
// Copyright © 2012- Shallway Studio
// http://shallway.net
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hoba.ObjectPool;

namespace Xft
{

    public class VertexPool : PooledObject
    {
        //private static ObjectPool<VertexPool> _PrtcDataPool = new ObjectPool<VertexPool>(10, 100, (Mesh mesh, Material mat) => { return new VertexPool(mesh, mat); });
        public static int CreateCount = 0;

        public class VertexSegment
        {
            public int VertStart;
            public int IndexStart;
            public int VertCount;
            public int IndexCount;
            public VertexPool Pool;

            public VertexSegment(int start, int count, int istart, int icount, VertexPool pool)
            {
                VertStart = start;
                VertCount = count;
                IndexCount = icount;
                IndexStart = istart;
                Pool = pool;
            }


            public void ClearIndices()
            {
                for (int i = IndexStart; i < IndexStart + IndexCount; i++)
                {
                    Pool.Indices[i] = 0;
                }

                Pool.IndiceChanged = true;
            }

        }

        public List<Vector3> Vertices = new List<Vector3>();
        public List<int> Indices = new List<int>();
        public List<Vector2> UVs = new List<Vector2>();
        public List<Color> Colors = new List<Color>();

        //added in version 4.0.0, to store additional parameter.
        public List<Vector2> UVs2 = new List<Vector2>();

        private List<Vector3> ActualVetices = new List<Vector3>();

        public bool IndiceChanged;
        public bool ColorChanged;
        public bool UVChanged;
        public bool VertChanged;
        public bool UV2Changed;

        public Mesh Mesh;
        public Material Material;

        protected int VertexTotal;
        protected int VertexUsed;
        protected int IndexTotal = 0;
        protected int IndexUsed = 0;
        public bool FirstUpdate = true;

        protected bool VertCountChanged;


        public const int BlockSize = 30;

        public float BoundsScheduleTime = 1f;
        public float ElapsedTime = 0f;


        protected List<VertexSegment> SegmentList = new List<VertexSegment>();

        public void RecalculateBounds()
        {
            Mesh.RecalculateBounds();
        }

        public VertexPool(Mesh mesh, Material material)
        {
            VertexTotal = VertexUsed = 0;
            VertCountChanged = false;
            Mesh = mesh;
            Material = material;
            InitArrays();

            //***BUG FIXED Vertices又初始化为Mesh的，那么InitArray等于没有用了，VertexTotal增加。
            //搞清楚功能，是Vertex LateUpdate赋值给Mesh
            //Vertices = Mesh.vertices;
            //Indices = Mesh.triangles;
            //Colors = Mesh.colors;
            //UVs = Mesh.uv;
            IndiceChanged = ColorChanged = UVChanged = UV2Changed = VertChanged = true;

            CreateCount++;
        }


        //the decal's vertices are retrieved in the first update, so we just pass the pool to it.
        public Decal AddDecal()
        {
            return new Decal(this);
        }

        public CustomMesh AddCustomMesh(Mesh mesh,Vector3 dir, float maxFps)
        {
            VertexSegment segment = GetVertices(mesh.vertices.Length, mesh.triangles.Length);
            CustomMesh cmesh = new CustomMesh(segment,mesh,dir,maxFps);
            return cmesh;
        }
        
        public Cone AddCone(Vector2 size, int numSegment, float angle, Vector3 dir, int uvStretch, float maxFps, bool usedelta, AnimationCurve deltaAngle)
        {
            VertexSegment segment = GetVertices((numSegment + 1) * 2, numSegment * 6);
            Cone f = new Cone(segment, size, numSegment, angle, dir, uvStretch, maxFps);
            f.UseDeltaAngle = usedelta;
            f.CurveAngle = deltaAngle;
            return f;
        }

        public XftSprite AddSprite(float width, float height, STYPE type, ORIPOINT ori, float maxFps, bool simple)
        {
            VertexSegment segment = GetVertices(4, 6);
            XftSprite s = new XftSprite(segment, width, height, type, ori, maxFps, simple);
            return s;
        }

        public RibbonTrail AddRibbonTrail(bool useFaceObj, Transform faceobj,float width, int maxelemnt, float len, Vector3 pos, float maxFps)
        {
            VertexSegment segment = GetVertices(maxelemnt * 2, (maxelemnt - 1) * 6);
            RibbonTrail trail = new RibbonTrail(segment,useFaceObj,faceobj, width, maxelemnt, len, pos, maxFps);
            return trail;
        }

        public VertexSegment GetRopeVertexSeg(int maxcount)
        {
            VertexSegment segment = GetVertices(maxcount * 2, (maxcount - 1) * 6);
            return segment;
        }

//         public Rope AddRope()
//         {
//             Rope rope = new Rope();
//             return rope;
//         }
// 
//         public SplineTrail AddSplineTrail(int elemCount)
//         {
//             VertexSegment segment = GetVertices(elemCount * 3, (elemCount - 1) * 12);
//             SplineTrail trail = new SplineTrail(segment);
//             return trail;
//         }
// 
//         public SphericalBillboard AddSphericalBillboard()
//         {
//             VertexSegment segment = GetVertices(4, 6);
//             SphericalBillboard sb = new SphericalBillboard(segment);
//             
//             return sb;
//         }

//         public Material GetMaterial()
//         {
//             return Material;
//         }

        public VertexSegment GetVertices(int vcount, int icount)
        {
            int vertNeed = 0;
            int indexNeed = 0;
            if (VertexUsed + vcount >= VertexTotal)
            {
                vertNeed = (vcount / BlockSize + 1) * BlockSize;
            }
            if (IndexUsed + icount >= IndexTotal)
            {
                indexNeed = (icount / BlockSize + 1) * BlockSize;
            }
            VertexUsed += vcount;
            IndexUsed += icount;
            if (vertNeed != 0 || indexNeed != 0)
            {
                EnlargeArrays(vertNeed, indexNeed);
                VertexTotal += vertNeed;
                IndexTotal += indexNeed;
            }

            VertexSegment ret = new VertexSegment(VertexUsed - vcount, vcount, IndexUsed - icount, icount, this);

            return ret;
        }


        protected void InitArrays()
        {
            Vertices.Clear();   Vertices.AddRange(new Vector3[4]);

            UVs.Clear();    UVs.AddRange(new Vector2[4]);
            UVs2.Clear();   UVs2.AddRange(new Vector2[4]);
            Colors.Clear();  Colors.AddRange(new Color[4]);
            Indices.Clear();    Indices.AddRange(new int[6]);
            VertexTotal = 4;
            IndexTotal = 6;

            for (int i = 0; i < UVs2.Count; i++)
                UVs2[i].Set(1f, 0f);
        }

        private void EnlargeArrays(int count, int icount)
        {
            Vertices.AddRange(new Vector3[count]);

            UVs.AddRange(new Vector2[count]);

            UVs2.AddRange(new Vector2[count]);
            for (int i = 0; i < UVs2.Count; i++)
                UVs2[i].Set(1f, 0f);

            Colors.AddRange(new Color[count]);

            Indices.AddRange(new int[icount]);

            VertCountChanged = true;
            IndiceChanged = true;
            ColorChanged = true;
            UVChanged = true;
            VertChanged = true;
            UV2Changed = true;
        }

        public void LateUpdate()
        {
            if (VertCountChanged)
            {
                Mesh.Clear();
            }

            if (Vertices.Count > VertexUsed)
            {
                Vertices.RemoveRange(VertexUsed, VertexTotal - VertexUsed);
                UVs.RemoveRange(VertexUsed, VertexTotal - VertexUsed);
                UVs2.RemoveRange(VertexUsed, VertexTotal - VertexUsed);
                Colors.RemoveRange(VertexUsed, VertexTotal - VertexUsed);
                VertexTotal = VertexUsed;
            }

            ActualVetices.Clear();
            for (int i = 0; i < Vertices.Count; ++i)
            {
                if (Vertices[i].IsZero())
                    ActualVetices.Add(Vertices[0]);
                else
                    ActualVetices.Add(Vertices[i]);
            }

            // we assume the vertices are always changed.
            Mesh.SetVertices(ActualVetices);
            if (UVChanged)
            {
                Mesh.SetUVs(0, UVs);
            }

            if (UV2Changed)
            {
                Mesh.SetUVs(1, UVs2);
            }

            if (ColorChanged)
            {
                Mesh.SetColors(Colors);
            }

            if (IndiceChanged)
            {
                if (Indices.Count > IndexUsed)
                {
                    Indices.RemoveRange(IndexUsed, IndexTotal - IndexUsed);
                    IndexTotal = IndexUsed;
                }

                Mesh.SetTriangles(Indices, 0);
            }

            ElapsedTime += Time.deltaTime;
            if (ElapsedTime > BoundsScheduleTime || FirstUpdate)
            {
                RecalculateBounds();
                ElapsedTime = 0f;
            }

            //how to recognise the first update?..
            if (ElapsedTime > BoundsScheduleTime)
                FirstUpdate = false;

            VertCountChanged = false;
            IndiceChanged = false;
            ColorChanged = false;
            UVChanged = false;
            UV2Changed = false;
            VertChanged = false;
        }
    }
}