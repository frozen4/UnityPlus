using System.Collections.Generic;
using UnityEngine;
using Common;

public class CEnvTransparentEffectMan : Singleton<CEnvTransparentEffectMan>
{
    private readonly HashSet<string> _UnsupportedSet = new HashSet<string>();
    private readonly HashSet<string> _CamtransOpaqueShaderNames = new HashSet<string>();
    private readonly HashSet<string> _MobileCamtransShaderNames = new HashSet<string>();

    public class CTransparentObject
    {
        public Renderer RendererObject = null;
        public Material[] OriginMaterials = null;
        public List<Material> CurrentMaterials = new List<Material>();

        public bool IsInUsed()
        {
            return RendererObject != null;
        }
    }

    private readonly List<CTransparentObject> _AlphaRenderInfos = new List<CTransparentObject>();
    private readonly List<Material[]> _ReusableMatArrays = new List<Material[]>();

    private readonly List<GameObject> _RaycastHittedGameObjects = new List<GameObject>();
    private readonly List<GameObject> _CameraTransGameObjects = new List<GameObject>();
    private readonly List<GameObject> _ToRevertCameraTransGameObjects = new List<GameObject>();

    public void RayAdjustBuildings(Vector3 vStart, Vector3 vDelta)
    {
        if (_CamtransOpaqueShaderNames.Count == 0)
        {
            _CamtransOpaqueShaderNames.Add("Mobile/Bumped Diffuse");
            _CamtransOpaqueShaderNames.Add("TERA/Environment/MobileDiffuse");
            _CamtransOpaqueShaderNames.Add("TERA/Environment/MobileDiffuseBump");
        }

        if (_MobileCamtransShaderNames.Count == 0)
        {
            _MobileCamtransShaderNames.Add("TERA/Environment/MobileDiffuseTwoSide");
            _MobileCamtransShaderNames.Add("TERA/Environment/MobileDiffuseTwoSideWithEmission");
            _MobileCamtransShaderNames.Add("TERA/Environment/MobileDiffuseBumpTwoSide");
            _MobileCamtransShaderNames.Add("TERA/Environment/MobileDiffuseBumpEnvMapTwoSide");
        }

        float fMaxDistance = vDelta.magnitude + 2.0f;          //fExtendLength从起始点沿反方向延长一定距离，避免出现起始点在collider中不被检测到的情况
        vDelta.Normalize();

        _RaycastHittedGameObjects.Clear();
        RaycastHit[] hitInfos = Physics.RaycastAll(vStart - vDelta * 2.0f, vDelta, fMaxDistance, CUnityUtil.LayerMaskCameraTrans);
        foreach (var hit in hitInfos)
        {
            var obj = hit.collider.gameObject;
            if (obj == null || !obj.activeInHierarchy)
                continue;

            _RaycastHittedGameObjects.Add(obj);

            if (!_CameraTransGameObjects.Contains(obj))
            {
                var renderers = obj.GetComponentsInChildren<MeshRenderer>();
                if (renderers == null)
                    continue;

                foreach (var rd in renderers)
                    MakeRenderTransparent(rd);
                _CameraTransGameObjects.Add(obj);
            }
        }

        //移除null game object
        for (int i = _CameraTransGameObjects.Count - 1; i >= 0; --i )
        {
            if (_CameraTransGameObjects[i] == null)
                _CameraTransGameObjects.RemoveAt(i);
        }

        _ToRevertCameraTransGameObjects.Clear();
        foreach (var v in _CameraTransGameObjects)
        {
            if (!_RaycastHittedGameObjects.Contains(v))
                _ToRevertCameraTransGameObjects.Add(v);
        }

        foreach (var v in _ToRevertCameraTransGameObjects)
        {
            var renderers = v.GetComponentsInChildren<MeshRenderer>();
            if (renderers == null)
                continue;

            foreach (var rd in renderers)
                MakeRenderOpaque(rd);
            _CameraTransGameObjects.Remove(v);
        }
    }

    private void MakeRenderTransparent(Renderer renderer)
    {
        if (renderer == null) return;

        CTransparentObject info = null;
        foreach (var v in _AlphaRenderInfos)
        {
            if (!v.IsInUsed())
            {
                info = v;
                break;
            }
        }

        if (info == null)
        {
            info = new CTransparentObject();
            _AlphaRenderInfos.Add(info);
        }

        info.RendererObject = renderer;
        info.OriginMaterials = info.RendererObject.sharedMaterials;

        for (int i = 0; i < info.OriginMaterials.Length; ++i)
        {
            if (info.OriginMaterials[i] != null)
            {
                var mat = MaterialPool.Instance.Get(info.OriginMaterials[i]);
                info.CurrentMaterials.Add(mat);

                string shaderName = info.OriginMaterials[i].shader.name;
                if (_CamtransOpaqueShaderNames.Contains(shaderName))
                {
                    var shader = ShaderManager.Instance.MobileCamtransOpaqueShader;
                    if (shader != null)
                        info.CurrentMaterials[i].shader = shader;
                }
                else if (_MobileCamtransShaderNames.Contains(shaderName))
                {
                    var shader = ShaderManager.Instance.MobileCamtransShader;
                    if (shader != null)
                        info.CurrentMaterials[i].shader = shader;
                }
                else if (!_UnsupportedSet.Contains(shaderName))
                {
                    _UnsupportedSet.Add(shaderName);
                    Common.HobaDebuger.LogWarningFormat("CRenderInfo.InitFromRenderer Unsupported Shader - {0}", shaderName);
                }
            }
            else
            {
                info.CurrentMaterials.Add(null);
            }
        }

        // use cam trans materials
        int nLen = info.OriginMaterials.Length;
        Material[] newMats = null;
        foreach (var v in _ReusableMatArrays)
        {
            if (v != null && v.Length == nLen)
            {
                newMats = v;
                break;
            }
        }

        if (newMats == null)
        {
            newMats = new Material[nLen];
            _ReusableMatArrays.Add(newMats);
        }

        for (int i = 0; i < nLen; ++i)
            newMats[i] = info.CurrentMaterials[i];            //设置半透material
        info.RendererObject.sharedMaterials = newMats;
        for (int i = 0; i < nLen; ++i)
            newMats[i] = null;
    }

    private void MakeRenderOpaque(Renderer renderer)
    {
        if (renderer == null) return;

        CTransparentObject info = null;
        foreach (var v in _AlphaRenderInfos)
        {
            if (v.RendererObject == renderer)
            {
                info = v;
                break;
            }
        }
        if(info == null) return;

        info.RendererObject.sharedMaterials = info.OriginMaterials;  // revert materials
        info.RendererObject = null;
        info.OriginMaterials = null;
        foreach (var v in info.CurrentMaterials)
        {
            if(v != null)
                MaterialPool.Instance.Recycle(v);
        }
        info.CurrentMaterials.Clear();
    }

    public void Cleanup()
    {
        _RaycastHittedGameObjects.Clear();
        _ToRevertCameraTransGameObjects.Clear();
        _CameraTransGameObjects.Clear();
        foreach (var v in _AlphaRenderInfos)
        {
            if (v.IsInUsed())
            {
                v.RendererObject.sharedMaterials = v.OriginMaterials; 
                v.RendererObject = null;
                v.OriginMaterials = null;
                foreach (var v1 in v.CurrentMaterials)
                {
                    if (v1 != null)
                        MaterialPool.Instance.Recycle(v1);
                }
                v.CurrentMaterials.Clear();
            }
        }
    }
}
