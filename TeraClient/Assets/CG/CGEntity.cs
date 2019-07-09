using UnityEngine;
using System;
using System.Collections.Generic;
using CinemaDirector;
using Common;

public class CGEntity : MonoBehaviour
{
    public enum EntityType
    {
        None,
        Camera,
        HostPlayer,
        OtherPlayer,   // 废弃 
        Monster,
        Npc,
        Obstacle,
        Scene,       // 场景中节点
        Sfx,
        CGAnimator   // 这是啥？？？
    }

    /// 资源路径(加载)
    public string path;
    /// 资源路径(ART_USE)
    public string artPath;
    /// 资源路径(客户端)
    public string clientPath;


    /// Entity唯一标识，在同一父物体下是唯一的
    public string EntityId;
    /// 创建位置
    public Vector3 position;
    /// 创建角度
    public Vector3 rotation;
    /// Entity类型
    public EntityType type = EntityType.None;

    // 如果为false，则操作场景中已经存在的对象
    public bool beCreated = true;

    // CinemaDirector.DirectorRuntimeHelper中有类之间的关系
    public List<ActorTrackGroup> actor = new List<ActorTrackGroup>();
    public List<MultiActorTrackGroup> multiActor = new List<MultiActorTrackGroup>();

    // CinemaShot + SetParent + SetTransformEvent需要通过CGEntity传入多余参数，目前游戏中只是用到了以下三种
    // 如果有新增，需要事件需要通过CGEntity设置除Actor外的参数，需要补充到下面
    public List<CinemaShot> cinemaShot = new List<CinemaShot>();                       // 设置 shotCamera
    public List<SetParent> setParent = new List<SetParent>();                          // 设置 parent
    public List<SetTransformEvent> setTransformEvent = new List<SetTransformEvent>();  // 设置 Transform

    //CG层级
    private const int LAYER_CG = 27;

    void Awake()
    {
        if (!beCreated)
            return;

        switch (type)
        {
            case EntityType.None:
                break;
            case EntityType.Camera:
                CreateCamera();
                break;
            case EntityType.HostPlayer:
                CreateHostPlayer();
                break;
            case EntityType.Scene:
                FindScene();
                break;
            default:
                CreateDefault();
                break;
        }
    }

    private void CreateCamera()
    {
        Transform parent = GetTransformParent();
        if (!parent)
            return;
        var  obj = new GameObject(EntityId);
        obj.name = EntityId;

        var cameraComp = obj.AddComponent<Camera>();
        cameraComp.cullingMask = (-1 & (~CUnityUtil.LayerMaskCGCulling));
        cameraComp.depth = 5F;
        cameraComp.nearClipPlane = 0.1f;
        cameraComp.farClipPlane = 2500;
        cameraComp.useOcclusionCulling = false;

        var cameraT = cameraComp.transform;
        cameraT.parent = parent;
        cameraT.position = position;
        cameraT.rotation = Quaternion.Euler(rotation);
        
        obj.AddComponent<GUILayer>();
        obj.AddComponent<PostProcessChain>();
        //obj.SetActive(false);

        foreach (var v in cinemaShot)
            v.shotCamera = cameraComp;

        SetTransform(cameraT);
    }

    private void CreateHostPlayer()
    {
        Transform parent = GetTransformParent();
        if (!parent) return;

        var hostPlayer = CUnityUtil.Instantiate(Main.HostPalyer.gameObject) as GameObject;
        if (hostPlayer != null)
        {
            var ob = hostPlayer.GetComponent<EntityComponent.ObjectBehaviour>();
            ob.enabled = false;
            hostPlayer.name = EntityId;
            var hostPlayerT = hostPlayer.transform;
            hostPlayerT.parent = parent;
            hostPlayerT.position = position;
            hostPlayerT.rotation = Quaternion.Euler(rotation);
            Util.SetLayerRecursively(hostPlayer, LAYER_CG);
            SetTransform(hostPlayerT);
        }
    }
    
    private void FindScene()
    {
        var index = path.IndexOf('/');
        var sceneName = path.Substring(0, index);
        var scene = GameObject.Find(sceneName);
        if (scene != null)
        {
            var targetName = path.Substring(index + 1, path.Length - index - 1);
            var target = scene.transform.Find(targetName);
            SetTransform(target);
        }
    }

    private void CreateDefault()
    {
        var parentTrans = GetTransformParent();
        if (parentTrans == null) return;

        var prefab = CAssetBundleManager.SyncLoadAssetFromBundle<GameObject>(path);
        if (prefab == null) return;

        var go = CUnityUtil.Instantiate(prefab) as GameObject;
        if (go != null)
        {
            go.name = EntityId;
            var goT = go.transform;
            if (clientPath != string.Empty)
            {
                GameObject goParent = GameObject.Find(clientPath);
                if (goParent)
                    goT.parent = goParent.transform;
                goT.localPosition = position;
                goT.localRotation = Quaternion.Euler(rotation);
            }
            else
            {
                goT.parent = parentTrans;
                goT.position = position;
                goT.rotation = Quaternion.Euler(rotation);
            }
            
            Util.SetLayerRecursively(go, LAYER_CG);
            SetTransform(goT);

            if (type == EntityType.Sfx)
                go.SetActive(false);
        }
    }

    private void SetTransform(Transform trans)
    {
        foreach (var v in actor)
        {
            v.Actor = trans;

            var components = v.GetComponentsInChildren<TextBubbleEvent>(true);
            foreach (var v1 in components)
                v1.textBubble.target = trans;
        }

        for (int i = 0; i < multiActor.Count; i++)
        {
            for (int j = 0; j < multiActor[i].Actors.Count; j++)
            {
                if (multiActor[i].Actors[j] == null)
                {
                    multiActor[i].Actors[j] = trans;
                    break;
                }
            }
        }
        foreach (var v in setParent)
        {
            if (null == v)
            {
                HobaDebuger.LogWarning("CGEntity :this is just a info :" + path + artPath);
                continue;
            }

            if (string.IsNullOrEmpty(v.child))
            {
                v.parent = trans.gameObject;
            }
            else
            {
                var child = trans.Find(v.child);
                if (null != child)
                    v.parent = child.gameObject;
                else
                    HobaDebuger.LogWarning("CGEntity :Cannot find child  :" + path + artPath + v.child);
            }
        }

        foreach (var v in setTransformEvent)
            v.Transform = trans;
    }

    private Transform GetTransformParent()
    {
        // Fabric收集到NullReferenceException异常
        if (transform == null)
        {
            Common.HobaDebuger.LogErrorFormat("CGEntity {0} has no transform", gameObject != null ? gameObject.name : "Unknown");
            return null;
        }

        var parent = transform.parent;
        while (parent != null)
        {
            if (parent.parent == null)
                return parent;
            parent = parent.parent;
        }
        return transform;
    }
}
