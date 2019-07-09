using UnityEngine;
using Common;
using System.Collections.Generic;

public class NavMeshRenderer : Singleton<NavMeshRenderer>
{
	//navMeshRenderer
    private GameObject _LineGameObj = null;
    private List<GameObject> _navMeshGameObjList = new List<GameObject>();

    public bool CreateNavMesh()
    {
        if (_LineGameObj != null)
        {
            GameObject.Destroy(_LineGameObj);
            _LineGameObj = null;
        }

        if (_LineGameObj == null)
            _LineGameObj = new GameObject("LineGameObj");
        for(int i = 0; i < _navMeshGameObjList.Count; ++i)
        {
            GameObject go = _navMeshGameObjList[i];
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }
        _navMeshGameObjList.Clear();

        LineRenderer lineRenderer = _LineGameObj.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.enabled = false;
        CUnityUtil.DisableLightAndShadow(lineRenderer);

        //ground
        {
            LuaInterface.SamplePolyAreas area = LuaInterface.SamplePolyAreas.SAMPLE_POLYAREA_GROUND;

            Vector3[] vertices;
            int[] triangles;
            //Load NavMesh
            {
                int vCont;
                int iCont;

                NavMeshManager m_navMeshMan = NavMeshManager.Instance;
                m_navMeshMan.GetNavMeshVertexIndexCount(out vCont, out iCont, area);

                vertices = new Vector3[vCont];
                triangles = new int[iCont];

                m_navMeshMan.FillNavMeshVertexIndexBuffer(vertices, vCont, triangles, iCont, area);
            }

            CUnityUtil.SMeshData meshData = new CUnityUtil.SMeshData();
            meshData.vertices = vertices;
            meshData.indices = triangles;

            List<CUnityUtil.SMeshData> listMeshData = new List<CUnityUtil.SMeshData>();
            CUnityUtil.SplitMeshData(meshData, listMeshData);

            Color color = new Color(0, 0.75f, 1.0f);
            for (int i = 0; i < listMeshData.Count; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("NavMeshGameObj_{0}_{1}", (int)area, i));

                MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                meshRenderer.name = HobaText.Format("MeshRenderer_{0}_{1}", (int)area, i);
                meshRenderer.material.shader = Shader.Find("Legacy Shaders/Bumped Diffuse");
                meshRenderer.material.color = color;
                CUnityUtil.DisableLightAndShadow(meshRenderer);
                MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.name = HobaText.Format("MeshFilterMesh_{0}_{1}", (int)area, i);
                meshFilter.sharedMesh.vertices = listMeshData[i].vertices;
                meshFilter.sharedMesh.triangles = listMeshData[i].indices;

                _navMeshGameObjList.Add(go);
            }
        }

        //grass
        {
            LuaInterface.SamplePolyAreas area = LuaInterface.SamplePolyAreas.SAMPLE_POLYAREA_GRASS;

            Vector3[] vertices;
            int[] triangles;
            //Load NavMesh
            {
                int vCont;
                int iCont;

                NavMeshManager m_navMeshMan = NavMeshManager.Instance;
                m_navMeshMan.GetNavMeshVertexIndexCount(out vCont, out iCont, area);

                vertices = new Vector3[vCont];
                triangles = new int[iCont];

                m_navMeshMan.FillNavMeshVertexIndexBuffer(vertices, vCont, triangles, iCont, area);
            }

            CUnityUtil.SMeshData meshData = new CUnityUtil.SMeshData();
            meshData.vertices = vertices;
            meshData.indices = triangles;

            List<CUnityUtil.SMeshData> listMeshData = new List<CUnityUtil.SMeshData>();
            CUnityUtil.SplitMeshData(meshData, listMeshData);

            Color color = new Color(0, 0.75f, 0.25f);
            for (int i = 0; i < listMeshData.Count; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("NavMeshGameObj_{0}_{1}", (int)area, i));

                MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                meshRenderer.name = HobaText.Format("MeshRenderer_{0}_{1}", (int)area, i);
                meshRenderer.material.shader = Shader.Find("Legacy Shaders/Bumped Diffuse");
                meshRenderer.material.color = color;
                CUnityUtil.DisableLightAndShadow(meshRenderer);
                MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.name = HobaText.Format("MeshFilterMesh_{0}_{1}", (int)area, i);
                meshFilter.sharedMesh.vertices = listMeshData[i].vertices;
                meshFilter.sharedMesh.triangles = listMeshData[i].indices;

                _navMeshGameObjList.Add(go);
            }
        }

        //road
        {
            LuaInterface.SamplePolyAreas area = LuaInterface.SamplePolyAreas.SAMPLE_POLYAREA_ROAD;

            Vector3[] vertices;
            int[] triangles;
            //Load NavMesh
            {
                int vCont;
                int iCont;

                NavMeshManager m_navMeshMan = NavMeshManager.Instance;
                m_navMeshMan.GetNavMeshVertexIndexCount(out vCont, out iCont, area);

                vertices = new Vector3[vCont];
                triangles = new int[iCont];

                m_navMeshMan.FillNavMeshVertexIndexBuffer(vertices, vCont, triangles, iCont, area);
            }

            CUnityUtil.SMeshData meshData = new CUnityUtil.SMeshData();
            meshData.vertices = vertices;
            meshData.indices = triangles;

            List<CUnityUtil.SMeshData> listMeshData = new List<CUnityUtil.SMeshData>();
            CUnityUtil.SplitMeshData(meshData, listMeshData);

            Color color = new Color(0.75f, 0.25f, 0f);
            for (int i = 0; i < listMeshData.Count; ++i)
            {
                GameObject go = new GameObject(HobaText.Format("NavMeshGameObj_{0}_{1}", (int)area, i));

                MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                meshRenderer.name = HobaText.Format("MeshRenderer_{0}_{1}", (int)area, i);
                meshRenderer.material.shader = Shader.Find("Legacy Shaders/Bumped Diffuse");
                meshRenderer.material.color = color;
                CUnityUtil.DisableLightAndShadow(meshRenderer);
                MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.name = HobaText.Format("MeshFilterMesh_{0}_{1}", (int)area, i);
                meshFilter.sharedMesh.vertices = listMeshData[i].vertices;
                meshFilter.sharedMesh.triangles = listMeshData[i].indices;

                _navMeshGameObjList.Add(go);
            }
        }
       

        //Default false
/*        showNavMesh(true);*/
        return true;
    }

    public void ShowNavMesh(bool showNav, bool showLine, bool bCreate)
    {
        if (bCreate)         //重新建立mesh
        {
            CreateNavMesh();
        }
        for(int i = 0; i < _navMeshGameObjList.Count; ++i)
        {
            MeshRenderer renderer = _navMeshGameObjList[i].GetComponent<MeshRenderer>();
            renderer.enabled = showNav;
        }

        if (_LineGameObj)
        {
            _LineGameObj.GetComponent<LineRenderer>().enabled = showLine;
        }
    }

    public bool IsLineRenderEnabled
    {
        get
        {
            if (_LineGameObj == null)
                return false;
            LineRenderer render = _LineGameObj.GetComponent<LineRenderer>();
            if (render == null)
                return false;
            return render.enabled;
        }
    }

    public void DrawLine(Vector3[] points)
    {
        if (_LineGameObj == null)
            return;
        LineRenderer render = _LineGameObj.GetComponent<LineRenderer>();
        if (render == null)
            return;

        if (points != null)
        {
            render.positionCount = points.Length;
            render.SetPositions(points);
        }
        else
        {
            render.positionCount = 0;
        }
    }

    public void OnDebugCmd(string cmd)
    {
        string[] result = cmd.Split(' ');
        if (result == null || result.GetLength(0) < 2) return;

        if (result[0].Equals("navmesh"))
        {
            if (result[1].Equals("1"))          //重建mesh
            {
                ShowNavMesh(true, true, true);
            }
            else if (result[1].Equals("0"))
            {
                ShowNavMesh(false, false, false);
            }
            else if (result[1].Equals("2"))         //重建mesh
            {
                ShowNavMesh(true, true, true);
            }
            else if (result[1].Equals("3"))
            {
                ShowNavMesh(false, true, false);
            }
        }  
    }
}
