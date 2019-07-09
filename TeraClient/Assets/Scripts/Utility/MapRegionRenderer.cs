using UnityEngine;
using Common;
using MapRegion;
using System;
using System.Collections.Generic;

public class MapRegionRenderer : Singleton<MapRegionRenderer>
{
    //mapRegionRenderer
    private List<GameObject> _mapRegionGameObjList = new List<GameObject>();
    FileMapRegion _fileMapRegion;
    string _lastFileMapRegionName;

    public bool ShowFileMapRegion(string filename)
    {
        if(_lastFileMapRegionName != filename)
        {
            FileMapRegion newFile = new FileMapRegion();

            try
            {
                byte[] region_data = Util.ReadFile(filename);
                if(!newFile.ReadFromMemory(region_data))
                {
                    HobaDebuger.LogWarning("ShowFileMapRegion, Failed to load " + filename);
                    return false;
                }
            }
            catch (Exception)
            {
                HobaDebuger.LogWarning("ShowFileMapRegion, Failed to load " + filename);
                return false;
            }

            _fileMapRegion = newFile;

            _lastFileMapRegionName = filename;
        }

        for(int i = 0; i < _mapRegionGameObjList.Count; ++i)
        {
            GameObject go = _mapRegionGameObjList[i];
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }
        _mapRegionGameObjList.Clear();
        List<int> Keys = new List<int>(_fileMapRegion.RegionMap.Keys);
        for(int i = 0; i < Keys.Count; ++i)
        {
            FileRegion fileRegion = _fileMapRegion.RegionMap[Keys[i]];
 
            //
            HashSet<uint> regionGridSet = fileRegion.RegionGridSet;

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            Vector3[] vGrid = new Vector3[4];
            int voffset = 0;
            foreach(uint v in regionGridSet)
            {
                if(vertices.Count + 4 > 60000)  //split
                {
                    int index = _mapRegionGameObjList.Count;
                    GameObject go = new GameObject(HobaText.Format("MapRegionGameObj_{0}_{1}", Keys[i], index));

                    MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                    meshRenderer.enabled = true;
                    meshRenderer.name = HobaText.Format("MapRegionGameObj_{0}_{1}", Keys[i], index);
                    meshRenderer.material.shader = Shader.Find("Legacy Shaders/Bumped Diffuse");
                    meshRenderer.material.color = GetMeshColor(Keys[i]);
                    CUnityUtil.DisableLightAndShadow(meshRenderer);

                    MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = new Mesh();
                    meshFilter.sharedMesh.name = HobaText.Format("MeshFilterMesh_{0}_{1}", Keys[i], index);
                    meshFilter.sharedMesh.vertices = vertices.ToArray();
                    meshFilter.sharedMesh.triangles = triangles.ToArray();

                    _mapRegionGameObjList.Add(go);

                    vertices.Clear();
                    triangles.Clear();
                    voffset = 0;
                }

                uint row = (v & 0xffff0000) >> 16;
                uint col = (v & 0xffff);

                _fileMapRegion.GetGridPositions(row, col, vGrid, 0);

                float fH0 = CUnityUtil.GetMapHeight(vGrid[0]);
                float fH1 = CUnityUtil.GetMapHeight(vGrid[1]);
                float fH2 = CUnityUtil.GetMapHeight(vGrid[2]);
                float fH3 = CUnityUtil.GetMapHeight(vGrid[3]);
//                 if (fH0 == 0.0f || fH1 == 0.0f || fH2 == 0.0f || fH3 == 0.0f)
//                     continue;

                vGrid[0].y = fH0 + 0.1f;
                vGrid[1].y = fH1 + 0.1f;
                vGrid[2].y = fH2 + 0.1f;
                vGrid[3].y = fH3 + 0.1f;

                vertices.Add(vGrid[0]);
                vertices.Add(vGrid[1]);
                vertices.Add(vGrid[2]);
                vertices.Add(vGrid[3]);

                triangles.Add(voffset + 0);
                triangles.Add(voffset + 1);
                triangles.Add(voffset + 2);
                triangles.Add(voffset + 3);
                triangles.Add(voffset + 2);
                triangles.Add(voffset + 1);

                voffset += 4;
            }

            if (vertices.Count > 0)
            {
                 int index = _mapRegionGameObjList.Count;
                 GameObject go = new GameObject(HobaText.Format("MapRegionGameObj_{0}_{1}", Keys[i], index));

                 MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                 meshRenderer.enabled = true;
                 meshRenderer.name = HobaText.Format("MapRegionGameObj_{0}_{1}", Keys[i], index);
                 meshRenderer.material.shader = Shader.Find("Legacy Shaders/Bumped Diffuse"); 
                 meshRenderer.material.color = GetMeshColor(Keys[i]);
                 CUnityUtil.DisableLightAndShadow(meshRenderer);
                 MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                 meshFilter.sharedMesh = new Mesh();
                 meshFilter.sharedMesh.name = HobaText.Format("MeshFilterMesh_{0}_{1}", Keys[i], index);
                 meshFilter.sharedMesh.vertices = vertices.ToArray();
                 meshFilter.sharedMesh.triangles = triangles.ToArray();

                 _mapRegionGameObjList.Add(go);

                 vertices.Clear();
                 triangles.Clear();
                 voffset = 0;
            }
        }

        return true;
    }

    public Color GetMeshColor(int index)
    {
        //Debug.LogError(index);
        Color meshColor = new Color(0, 0, 0); ;
        switch (index)
        {

            case 1:
                meshColor = new Color(1.0f, 0, 0);
                break;
            case 2:
                meshColor = new Color(0 , 0, 0);
                break;
            case 3:
                meshColor = new Color(0, 1.0f, 0);
                break;
            case 4:
                meshColor = new Color(0.5f, 1.0f, 0.5f);
                break;
            case 5:
                meshColor = new Color(0, 0, 1.0f);
                break;
            default:
                meshColor = new Color(0.5f, 0.5f, 1.0f);
                break;
        }
        return meshColor;
    }

    public void HideFileMapRegion()
    {
        for(int i = 0; i < _mapRegionGameObjList.Count; ++i)
        {
            GameObject go = _mapRegionGameObjList[i];
            if (go != null)
            {
                GameObject.Destroy(go);
            }
        }
        _mapRegionGameObjList.Clear();
    }

    public void OnDebugCmd(string cmd)
    {
        string[] result = cmd.Split(' ');
        if (result == null || result.GetLength(0) < 2) return;

        if (result[0].Equals("lightregion"))
        {
            if (result[1].Equals("0"))
            {
                HideFileMapRegion();
            }
            else
            {
                string str = EntryPoint.Instance.ResPath + "/Maps/";
                str += result[1];
                str += ".lightregion";
                ShowFileMapRegion(str);
            }
        }else  if (result[0].Equals("blockregion"))
        {
            if (result[1].Equals("0"))
            {
                HideFileMapRegion();
            }
            else
            {
                string str = EntryPoint.Instance.ResPath + "/Maps/";
                str += result[1];
                str += ".blockregion";
                ShowFileMapRegion(str);
            }
        }
        else if(result[0].Equals("regionset"))
        {
            if (result[1].Equals("0"))
            {
                HideFileMapRegion();
            }
            else
            {
                string str = EntryPoint.Instance.ResPath + "/MapInfo/";
                str += result[1];
                str += ".regionset";
                ShowFileMapRegion(str);
            }
        }
        else if (result[0].Equals("obstacleset"))
        {
            if (result[1].Equals("0"))
            {
                HideFileMapRegion();
            }
            else
            {
                string str = EntryPoint.Instance.ResPath + "/MapInfo/";
                str += result[1];
                str += ".obstacleset";
                ShowFileMapRegion(str);
            }
        }
    }

}

