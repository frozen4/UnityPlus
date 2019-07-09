using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

public enum CameraTraceResult
{
    NoHit = 0,
    HitTerrain,
    HitCameraCollision,
}

public static class CUnityUtil
{
	public static int SupportedRaycastMapLayerMask = 1<<8;

    public static float TraceForwardDistance = 1.5f;

    public static string Tag_UIDrag = "_UIDrag_"; //允许UI拖动的tag

    //定义Layer_，避免误写成LayerMask
    public static int Layer_Default = LayerMask.NameToLayer("Default");
    public static int Layer_UI = LayerMask.NameToLayer("UI");
    public static int Layer_Terrain = LayerMask.NameToLayer("Terrain");
    public static int Layer_Building = LayerMask.NameToLayer("Buildings");
    public static int Layer_Background = LayerMask.NameToLayer("Background");
    public static int Layer_Unblockable = LayerMask.NameToLayer("Unblockable");
    public static int Layer_Player = LayerMask.NameToLayer("Player");
    public static int Layer_NPC = LayerMask.NameToLayer("NPC");
    public static int Layer_Blockable = LayerMask.NameToLayer("Blockable");             //策划编辑的obstacle
    public static int Layer_ClientBlockable = LayerMask.NameToLayer("ClientBlockable");            
    public static int Layer_HostPlayer = LayerMask.NameToLayer("HostPlayer");
    public static int Layer_Shadow = LayerMask.NameToLayer("Shadow");

    public static int Layer_Reflection = LayerMask.NameToLayer("Reflection");
    public static int Layer_CameraCollision = LayerMask.NameToLayer("CameraCollision");     //阻挡摄像机
    public static int Layer_CameraTrans = LayerMask.NameToLayer("CameraTrans");         //穿透摄像机
    public static int Layer_Invisible = LayerMask.NameToLayer("Invisible");

    public static int LayerMaskDefault = LayerMask.GetMask("Default");
    public static int LayerMaskTerrain = LayerMask.GetMask("Terrain");
    public static int LayerMaskBuilding = LayerMask.GetMask("Buildings");
    public static int LayerMaskBackground = LayerMask.GetMask("Background");
    public static int LayerMaskReflection = LayerMask.GetMask("Reflection");
    public static int LayerMaskWater = LayerMask.GetMask("Water");
    public static int LayerMaskPlayer = LayerMask.GetMask("Player");
    public static int LayerMaskNPC = LayerMask.GetMask("NPC");
    public static int LayerMaskBlockable = LayerMask.GetMask("Blockable");              //策划编辑的obstacle
    public static int LayerMaskClientBlockable = LayerMask.GetMask("ClientBlockable");  
    public static int LayerMaskClickable = LayerMask.GetMask("Clickable");
    public static int LayerMaskHostPlayer = LayerMask.GetMask("HostPlayer");
    public static int LayerMaskShadow = LayerMask.GetMask("Shadow");
    public static int LayerMaskCameraCollistion = LayerMask.GetMask("CameraCollision");     //阻挡摄像机
    public static int LayerMaskCameraTrans = LayerMask.GetMask("CameraTrans");
    public static int LayerMaskUnblockable = LayerMask.GetMask("Unblockable");
    public static int LayerMaskUI = LayerMask.GetMask("UI");
    public static int LayerMaskCG = LayerMask.GetMask("CG");
    public static int LayerMaskCGCulling = LayerMask.GetMask("UI", "NPC", "Player", "Invisible", "EntityAttached", "Fx", "HostPlayer");
    public static int LayerMaskFx = LayerMask.GetMask("Fx");       //特效

    public static int LayerMaskEntity = LayerMaskPlayer | LayerMaskNPC | LayerMaskHostPlayer;
    public static int LayerMaskObstacle = LayerMaskEntity | LayerMaskBlockable;
    public static int LayerMaskMovementObstacle = LayerMaskClientBlockable | LayerMaskBlockable;
    public static int LayerMaskTerrainBuilding = LayerMaskBuilding | LayerMaskTerrain;
    public static int LayerMaskTerrainCameraCollision = LayerMaskTerrain | LayerMaskBuilding;
    public static int LayerMaskTerrainBuildingCameraCollision = LayerMaskTerrain | LayerMaskBuilding;
    //阴影mask
    //public static int LayerMaskShadows = LayerMaskTerrainBuilding | LayerMaskEntity | LayerMaskUnblockable | LayerMaskBlur | LayerMaskUI | LayerMaskCG | LayerMaskDefault;

    public static int LayerMaskShadows_L0 = LayerMaskTerrain | LayerMaskEntity | LayerMaskUnblockable | LayerMaskClickable | LayerMaskCG | LayerMaskShadow;
    public static int LayerMaskShadows_L1 = LayerMaskShadows_L0;
    public static int LayerMaskShadows_L2 = LayerMaskShadows_L0;

    public static float InvalidHeight = -199.0f;

    //public static int LayerMaskInputClick = LayerMaskTerrainBuilding | LayerMaskEntity | LayerMaskClickable;         //点击鼠标时需要考虑的layers

    public static Object Instantiate(Object original, Vector3 position, Quaternion rotation)
    {
        Object obj = Object.Instantiate(original, position, rotation);
#if NEED_COUNTER
        if (obj != null)
            U3DFuncWrap.Instance().AddStatisticsCount(obj.name);
#endif
        return obj;
    }

    public static Object Instantiate(Object original)
    {
        Object obj = null;
        if (original != null)
            obj = Object.Instantiate(original);
#if NEED_COUNTER
        if (obj != null)
            U3DFuncWrap.Instance().AddStatisticsCount(obj.name);
#endif

        return obj;
    }

    public static float GetMapHeight(Vector3 pos, float radius = 0f)
    {
#if SERVER_USE
        return NavMeshManager.Instance.GetPosHeight(pos);
#else
        RaycastHit hit_info;
        if (pos.y <= InvalidHeight)
            pos.y = 200.0f;
        else
            pos.y += 300.0f;

        float max_distance = Mathf.Max(400f - InvalidHeight, pos.y + 200.0f);

        //bool ret = (radius == 0) ? Physics.Raycast(pos, Vector3.down, out hit_info, max_distance, SupportedRaycastMapLayerMask) : Physics.SphereCast(pos, radius, Vector3.down, out hit_info, max_distance, SupportedRaycastMapLayerMask);
        bool ret = RayCastWithRadius(radius, pos, Vector3.down, out hit_info, max_distance, SupportedRaycastMapLayerMask);
        
//         if (!ret)
//         {
//             Common.HobaDebuger.LogWarningFormat("GetMapHeight Failed! x: {0}, z: {1}", pos.x, pos.z);
//         }
        if (ret)
            return hit_info.point.y;

        return InvalidHeight;
#endif
    }

    public static float GetModelHeight(GameObject go, bool isColliderInChild = false)
    {
        return GetModelHeight(go, 1.0f, isColliderInChild);
    }

    public static float GetModelHeight(GameObject go, float fScale, bool isColliderInChild = false)
    {
        if (go == null)
            return 0;

        if (!isColliderInChild)
        {
            var cc = go.GetComponent<CapsuleCollider>();

            if (cc != null)
            {
                var halfHeight = cc.height / 2;
                if (halfHeight < cc.radius) halfHeight = cc.radius;
				return cc.center.y * fScale + halfHeight * cc.transform.localScale.y * fScale;
            }
            else
            {
                var bc = go.GetComponent<BoxCollider>();
                if (bc != null)
                {
                    return (bc.center.y + bc.size.y / 2) * bc.transform.localScale.y * fScale;
                }
                else
                {
                    var sc = go.GetComponent<SphereCollider>();
                    if (sc != null)
                        return (sc.center.y + sc.radius) * sc.transform.localScale.y * fScale;
                }
            }
        }
        else
        {
            var cc = go.GetComponentInChildren<CapsuleCollider>();

            if (cc != null)
            {
                return (cc.center.y + cc.height / 2) * cc.transform.localScale.y * fScale;
            }
            else
            {
                var bc = go.GetComponentInChildren<BoxCollider>();
                if (bc != null)
                {
                    return (bc.center.y + bc.size.y / 2) * bc.transform.localScale.y * fScale;
                }
                else
                {
                    var sc = go.GetComponentInChildren<SphereCollider>();
                    if (sc != null)
                        return (sc.center.y + sc.radius) * sc.transform.localScale.y * fScale;
                }
            }
        }

        return 0;
    }

    public static bool RayCastWithRadius(float radius, Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
    {
        if (radius < 1e-3f)
            return Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
        else
            return Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask) && hitInfo.collider != null;
    }

    public static bool IsHeightAccept(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.y - b.y) < 1.5f;
    }

    public static bool IsWwiseAudioName(string audioName)
    {
        return !System.IO.Path.HasExtension(audioName);
    }

    public static bool GetWwiseAudioName(string val, out string eventname, out string switchgroup, out string switchstate)
    {
        switchgroup = "";
        switchstate = "";

        int idx = val.IndexOf('/');
        if (idx < 0)
        {
            eventname = val;
            return false;
        }

        eventname = val.Substring(0, idx);
        int idx1 = val.IndexOf('/', idx + 1);
        if (idx1 < 0)
        {
            switchgroup = val.Substring(idx + 1);
            return false;
        }

        switchgroup = val.Substring(idx + 1, idx1 - (idx + 1));
        switchstate = val.Substring(idx1 + 1);
        return true;
    }

    public static Vector3 WorldPositionToCanvas(GameObject obj)
    {
        var worldPos = obj.transform.position;
       // GameObject ui_root = GameObject.Find("UIRootCanvas");
        Transform ui_root = GameObjectUtil.GetUIRootTranform();
        if (null == ui_root)
        {
            Debug.LogWarning("WorldPositionToCanvas Can not find UIRootCanvas!!");
            return Vector3.zero;
        }

        return ui_root.worldToLocalMatrix.MultiplyPoint(worldPos);
    }

    /// 搜索子物体组件-GameObject版
    public static T Get<T>(GameObject go, string subnode) where T : Component
    {
        if (go != null)
        {
            Transform sub = go.transform.Find(subnode);
            if (sub != null) return sub.GetComponent<T>();
        }
        return null;
    }

    /// 搜索子物体组件-Transform版
    public static T Get<T>(Transform go, string subnode) where T : Component
    {
        if (go != null)
        {
            Transform sub = go.Find(subnode);
            if (sub != null) return sub.GetComponent<T>();
        }
        return null;
    }

    /// 搜索子物体组件-Component版
    public static T Get<T>(Component go, string subnode) where T : Component
    {
        return go.transform.Find(subnode).GetComponent<T>();
    }

    /// 添加组件
    public static T Add<T>(GameObject go) where T : Component
    {
        if (go != null)
        {
            T[] ts = go.GetComponents<T>();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] != null) UnityEngine.Object.Destroy(ts[i]);
            }
            return go.gameObject.AddComponent<T>();
        }
        return null;
    }

    /// 添加组件
    public static T Add<T>(Transform go) where T : Component
    {
        return Add<T>(go.gameObject);
    }

    /// 查找子对象
    public static GameObject FindChild(GameObject go, string subnode)
    {
        return FindChild(go.transform, subnode);
    }

    /// 查找子对象
    public static GameObject FindChild(Transform go, string subnode)
    {
        Transform tran = go.Find(subnode);
        if (tran == null) return null;
        return tran.gameObject;
    }

    /// 清除所有子节点
    public static void ClearChild(Transform go)
    {
        if (go == null) return;
        for (int i = go.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(go.GetChild(i).gameObject);
        }
    }

    public struct SMeshData
    {
        public Vector3[] vertices;
        public int[] indices;
    }

    public static bool SplitMeshData(SMeshData meshData, List<SMeshData> subMeshDataList)
    {
        const int VLIMIT = 60000;

        int icount = meshData.indices.Length;
        int vcount = meshData.vertices.Length;

        subMeshDataList.Clear();
        int minv = 0;
        int maxv = 0;
        int iOffset = 0;
        for(int i = 0; i < icount; i += 3)
        {
            int idx0 = meshData.indices[i];
            int idx1 = meshData.indices[i + 1];
            int idx2 = meshData.indices[i + 2];

            int minIndex = minv;
            int maxIndex = maxv;
            maxIndex = System.Math.Max(maxIndex, idx0);
            maxIndex = System.Math.Max(maxIndex, idx1);
            maxIndex = System.Math.Max(maxIndex, idx2);

            if ((maxIndex - minIndex) >= VLIMIT)             //split
            {
                SMeshData subMeshData = new SMeshData();

                int cvcount = maxv - minv + 1;
                int cicount = i - iOffset;

                subMeshData.vertices = new Vector3[cvcount];
                subMeshData.indices = new int[cicount];

                System.Array.Copy(meshData.vertices, minv, subMeshData.vertices, 0, cvcount);
                System.Array.Copy(meshData.indices, iOffset, subMeshData.indices, 0, cicount);

                for (int t = 0; t < cicount; ++t)
                {
                    subMeshData.indices[t] -= minv;
                }

                subMeshDataList.Add(subMeshData);

                //recalculate
                iOffset = i;
                minv = minv + cvcount;
                maxv = minv;

                continue;
            }

            maxv = maxIndex;

            if(i+3 >= icount && maxv - minv > 0)           //last
            {
                SMeshData subMeshData = new SMeshData();

                int cvcount = maxv - minv + 1;
                int cicount = i+3- iOffset;

                subMeshData.vertices = new Vector3[cvcount];
                subMeshData.indices = new int[cicount];

                System.Array.Copy(meshData.vertices, minv, subMeshData.vertices, 0, cvcount);
                System.Array.Copy(meshData.indices, iOffset, subMeshData.indices, 0, cicount);

                for (int t = 0; t < cicount; ++t)
                {
                    subMeshData.indices[t] -= minv;
                }

                subMeshDataList.Add(subMeshData);

                iOffset = i;
            }
        }

        return true;
    }


    public static bool CanGetPixel(Texture2D tex)
    {
        try
        {
            tex.GetPixel(0, 0);
        }
        catch(UnityException e)
        {
            if (e.Message.StartsWith(HobaText.Format("Texture '{0}' is not readable", tex.name)))
            {
                return false;
            }
        }
        return true;
    }

    public static bool CanGetPixel32(Texture2D tex, int level, out string error)
    {
        error = "";
        try
        {
            tex.GetPixels32(level);
        }
        catch (UnityException e)
        {
            if (e.Message.StartsWith(HobaText.Format("Texture '{0}' is not readable", tex.name)))
            {
                error = e.Message;
                return false;
            }
        }
        return true;
    }

    public static void DisableLightAndShadow(Renderer renderer)
    {
        renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

#if UNITY_EDITOR
    /// 项目中是否有该Tag
    public static bool HasTag(string tag)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(tag))
            {
                return true;
            }
        }
        return false;
    }
#endif

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        System.Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch
                {
                    // catch nothing 
                    // In case of NotImplementedException being thrown. 
                    //For some reason specifying that exception didn't seem to catch it.

                }
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }
    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }

    public static bool HasNameInParent(GameObject obj, GameObject parent, string name)
    {
        GameObject p = obj;
        while(p != null)
        {
            if (p.name.Contains(name))
                return true;


            p = p.transform.parent != null ? p.transform.parent.gameObject : null;
        }
        return false;
    }

    public static void FindChildLeaf(System.Type type, GameObject obj, List<GameObject> listObjects)
    {
        if (obj == null)
            return;

        foreach (Transform child in obj.transform)
        {
            if (child.gameObject == null)
                continue;

            var renderer = child.gameObject.GetComponent(type);
            if (renderer != null)
            {
                listObjects.Add(child.gameObject);
            }

            FindChildLeaf(type, child.gameObject, listObjects);
        }
    }

    public static void FindChildLeaf(System.Type type0, System.Type type1, GameObject obj, List<GameObject> listObjects)
    {
        if (obj == null)
            return;

        foreach (Transform child in obj.transform)
        {
            if (child.gameObject == null)
                continue;

            var comp = child.gameObject.GetComponent(type0);
            var comp1 = child.gameObject.GetComponent(type1);
            if (comp != null || comp1 != null)
            {
                listObjects.Add(child.gameObject);
            }

            FindChildLeaf(type0, type1, child.gameObject, listObjects);
        }
    }

    public static void SaveRenderTextureToFile(RenderTexture rt, string filename)
    {
         Texture2D tex2d = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
         tex2d.name = "CaptureImage";
         RenderTexture prev = RenderTexture.active;
         RenderTexture.active = rt;
         tex2d.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
         tex2d.Apply();
         RenderTexture.active = prev;

         string file_name = Path.Combine(Application.dataPath, filename);
         FileStream file = File.Open(file_name, FileMode.Create, FileAccess.Write);

         BinaryWriter bw = new BinaryWriter(file);
         byte[] bytes = tex2d.EncodeToPNG();
         bw.Write(bytes);
         file.Close();
    }

    public static void EnableCastShadows(GameObject go, bool bOn)
    {
        SkinnedMeshRenderer[] skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skinnedMeshRenderers.Length; ++i)
        {
            if (bOn)
                skinnedMeshRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                skinnedMeshRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < meshRenderers.Length; ++i)
        {
            if (bOn)
                meshRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            else
                meshRenderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    public static Vector3 RandomVector3(float range)
    {
        return new Vector3(Random.Range(-range, range),
            Random.Range(-range, range),
            Random.Range(-range, range));
    }

    public static Quaternion RandomXYQuaternion(float angle)
    {
        if (angle > 0)
            return Quaternion.Euler(new Vector3(Random.Range(-angle, angle),
                Random.Range(-angle, angle),
                0));
        else
            return Quaternion.identity;
    }
}
