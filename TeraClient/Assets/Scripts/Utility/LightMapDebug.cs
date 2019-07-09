using UnityEngine;
using Common;

public class LightMapDebug : Singleton<LightMapDebug>
{

    static GameObject _LightMapDebugObj;

    public bool CreateLightObj()
    {
        if (null == _LightMapDebugObj)
        {
            GameObject[] goArray = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject go in goArray)
            {
                if ("LigthObj" == go.name )
                    _LightMapDebugObj = go;
            }
            if (null == _LightMapDebugObj)
            {
                _LightMapDebugObj = new GameObject("LigthObj");
            }
                
        }

        _LightMapDebugObj.AddComponent<MeshRenderer>();
        _LightMapDebugObj.GetComponent<MeshRenderer>().enabled = false;
        _LightMapDebugObj.AddComponent<MeshFilter>();
        _LightMapDebugObj.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        _LightMapDebugObj.GetComponent<MeshFilter>().sharedMesh.name = "LigthMapRegion";



        ////获取顶点和三角面
        return true;

    }

    public void ShowLightMap(bool showLightMap)
    {
        if (null == _LightMapDebugObj)     
        {
            CreateLightObj();
        }
        if (_LightMapDebugObj)
        {
            _LightMapDebugObj.GetComponent<MeshRenderer>().enabled = showLightMap;
        }
    }


    public void OnDebugCmd(bool showLightMap)
    {
        ShowLightMap(showLightMap);
    }
}
