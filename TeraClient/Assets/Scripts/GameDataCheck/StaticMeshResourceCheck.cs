using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Common;
using UnityEngine;
using Template;
using Object = UnityEngine.Object;


public partial class StaticMeshResourceCheck : Singleton<StaticMeshResourceCheck>
{
    public class CMeshEntry           //每个mesh
    {
        public string name = string.Empty;
        public string parent = string.Empty;

        public int faces = 0;
    }

    public class CBlockMeshInfo            //每个block块，或者每个prefab
    {
        public List<CMeshEntry> meshList = new List<CMeshEntry>();

        public void Sort()
        {
            meshList.Sort(delegate(CMeshEntry x, CMeshEntry y)
                {
                    int eq = y.faces.CompareTo(x.faces);

                    if (eq == 0)
                        eq = x.name.CompareTo(y.name);

                    if (eq == 0)
                        eq = x.parent.CompareTo(y.parent);

                    return eq;
                });
        }

        public int CalcTotalFaces()
        {
            int count = 0;
            foreach (var entry in meshList)
                count += entry.faces;
            return count;
        }
    }

    public Dictionary<string, CBlockMeshInfo> _DicBlockMeshInfo = new Dictionary<string, CBlockMeshInfo>();

    public Dictionary<string, CBlockMeshInfo> _DicPrefabMeshInfo = new Dictionary<string, CBlockMeshInfo>();

    public Dictionary<int, Scene> _SceneData = new Dictionary<int, Scene>();

    public int MeshFaceLimit = 10000;

    public void Init(int nMeshFaceLimit)
    {
        MeshFaceLimit = nMeshFaceLimit;

        //预先设置路径
        Template.Path.BasePath = System.IO.Path.Combine(Application.dataPath, "../../GameRes/");
        Template.Path.BinPath = "Data/";

        var sceneManager = Template.SceneModule.Manager.Instance;
        sceneManager.ParseTemplateAll(true);
        _SceneData = sceneManager.GetTemplateMap();
    }

    public string _ErrorString = "";
    public List<CMeshEntry> _ListEntries = new List<CMeshEntry>();

    public void SortListEntries()
    {
        _ListEntries.Sort(delegate(CMeshEntry x, CMeshEntry y)
        {
            int eq = y.faces.CompareTo(x.faces);

            if (eq == 0)
                eq = x.name.CompareTo(y.name);

            if (eq == 0)
                eq = x.parent.CompareTo(y.parent);

            return eq;
        });
    }

    //检查流程
    public IEnumerable DoCheckMeshCoroutine(string prefabName, GameObject scenePrefab)
    {
        var _Config = scenePrefab.GetComponent<SceneConfig>();
        if (null == _Config)
        {
            HobaDebuger.LogErrorFormat("{0} Is not a scene prefab.", prefabName);
            yield break;
        }

        //加载Objects
        if (_Config._BlockPositionList.Count > 0)           //有block
        {
            foreach (var item in DoCheckMeshRendererCoroutine(prefabName, _Config))
                yield return item;
        }
        else                     //无block
        {
            foreach (var item in DoCheckMeshRendererCoroutine(prefabName, scenePrefab))
                yield return item;
        }

        yield return null;
    }

    IEnumerable DoCheckMeshRendererCoroutine(string prefabName, SceneConfig config)
    {
        Transform blockRootTran = config.transform.Find("BlockObjects");
        if (null == blockRootTran)
        {
            blockRootTran = config.transform;
        }

        int count = 0;
        int total = config._BlockPositionList.Count;
        foreach (SceneConfig.CPositionSetting currentPos in config._BlockPositionList)
        {
            ++count;

            string shortName = System.IO.Path.GetFileNameWithoutExtension(prefabName);
            string blockName = System.IO.Path.GetFileName(currentPos._BlockName);
            GameDataCheckMan.Instance.SetDesc(string.Format("{0} 检查Obj: {1}", shortName, blockName));
            GameDataCheckMan.Instance.SetPartProgress((float)count / total);
            yield return null;

            var asset = AssetBundleCheck.Instance.LoadAsset(currentPos._BlockName);
            if (asset == null)
            {
                _ErrorString += string.Format("terrain加载Object错误: {0}\n", currentPos._BlockName);
                continue;
            }

            //创建
            //TextLogger.Instance.WriteLine(string.Format("Checking Scene Object: {0}", currentPos._BlockName));
            var obj = GameObject.Instantiate(asset) as GameObject;
            obj.transform.parent = blockRootTran;

            List<GameObject> rendererList = new List<GameObject>();
            CUnityUtil.FindChildLeaf(typeof(MeshRenderer), obj, rendererList);

            CBlockMeshInfo meshInfo = new CBlockMeshInfo();

            //检查每个包含MeshRenderer的go
            foreach (var go in rendererList)
            {
                //MeshRenderer renderer = go.GetComponent<MeshRenderer>();

                MeshFilter[] meshes = go.GetComponentsInChildren<MeshFilter>();

                int nFaces = 0;
                foreach (var ms in meshes)
                {
                    Mesh mesh = ms.sharedMesh != null ? ms.sharedMesh : ms.mesh;
                    nFaces += mesh.triangles.Length / 3;
                }

                var entry = new CMeshEntry()
                {
                    name = go.name,
                    parent = currentPos._BlockName,
                    faces = nFaces,
                };
                meshInfo.meshList.Add(entry);

                _ListEntries.Add(entry);
            }

            meshInfo.Sort();
            _DicBlockMeshInfo.Add(currentPos._BlockName, meshInfo);

            GameObject.DestroyImmediate(obj);
        }

        yield return null;
    }

    IEnumerable DoCheckMeshRendererCoroutine(string prefabName, GameObject scenePrefab)
    {
        List<GameObject> rendererList = new List<GameObject>();
        CUnityUtil.FindChildLeaf(typeof(MeshRenderer), scenePrefab, rendererList);

        CBlockMeshInfo meshInfo = new CBlockMeshInfo();

        //检查每个包含MeshRenderer的go
        foreach (var go in rendererList)
        {
            //MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            MeshFilter[] meshes = go.GetComponentsInChildren<MeshFilter>();

            int nFaces = 0;
            foreach (var ms in meshes)
            {
                Mesh mesh = ms.sharedMesh != null ? ms.sharedMesh : ms.mesh;
                nFaces += mesh.triangles.Length / 3;
            }

            var entry = new CMeshEntry()
            {
                name = go.name,
                parent = prefabName,
                faces = nFaces,
            };
            meshInfo.meshList.Add(entry);

            _ListEntries.Add(entry);
        }

        meshInfo.Sort();
        _DicPrefabMeshInfo.Add(prefabName, meshInfo);

        yield return null;
    }
}

