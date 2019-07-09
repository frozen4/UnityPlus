using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//Clipping fx for UI rects.
//Dont ask why this is called sfx, I ve no ideal at all.
public class UISFxAreaClip : MonoBehaviour
{
    List<Material> MaterialList = new List<Material>();//存放需要修改Shader的Material
    //List<Renderer> RendererList = new List<Renderer>(); //

    Vector4 DefaultRect { get { return new Vector4(-999999, -999999, 999999, 999999); } }
    RectTransform _ClippingRect;

    static Shader GetShader(string shader_name)
    {
        Shader sh =
#if IN_GAME
 ShaderManager.Instance.FindShader(shader_name);
#else
 Shader.Find(shader_name);
#endif
        if (sh == null)
        {
            Debug.LogWarning("<color=#FF0000FF><FxAreaClip>shader not found: " + shader_name+"</color>");
        }
        return sh;
    }

    //Cache mats
    void CacheMaterials()
    {
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0, j = particleSystems.Length; i < j; i++)
        {
            Renderer ps_r = particleSystems[i].GetComponent<Renderer>();

            Material smat = ps_r.sharedMaterial;
            if (smat != null)
            {
                Shader shader = GetShader(smat.shader.name + "_ui2");
                if (shader != null)
                {
                    Material mat = ps_r.material;
                    MaterialList.Add(mat);
                    //Debug.LogWarning(mat.shader.name + "_ui2");
                    mat.shader = shader;
                }
            }
        }

        MeshRenderer[] renders = GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0, j = renders.Length; i < j; i++)
        {
            MeshRenderer ps = renders[i];

            Material smat = ps.sharedMaterial;
            if (smat != null)
            {
                Shader shader = GetShader(smat.shader.name + "_ui2");
                if (shader != null)
                {
                    Material mat = ps.material;
                    MaterialList.Add(mat);
                    //Debug.LogWarning(mat.shader.name + "_ui2");
                    //mat.shader = GetShader(mat.shader.name + "_ui2");
                    mat.shader = shader;
                }
            }
        }
    }

    public void SetRect(RectTransform rect_clip)
    {
        _ClippingRect = rect_clip;
        if (_ClippingRect != null && MaterialList.Count != 0)
        {
            enabled = true;
            ApplyClipping();
        }
        else
        {
            Revert();
            enabled = false;
        }
    }

    public void UpdateFxObject(bool is_clear)
    {
        if (is_clear) MaterialList.Clear();

        if (MaterialList.Count == 0)
        {
            CacheMaterials();
        }

        if (_ClippingRect != null && MaterialList.Count != 0)
        {
            enabled = true;
            ApplyClipping();
        }
        else
        {
            enabled = false;
        }
    }

    private static Vector3[] w_4c = new Vector3[4];
    private void ApplyClipping()
    {
        //Rect rt = rect_clip.rect;
        _ClippingRect.GetWorldCorners(w_4c);

        Vector4 area = new Vector4(w_4c[0].x, w_4c[0].y, w_4c[2].x, w_4c[2].y);
        for (int i = 0, len = MaterialList.Count; i < len; i++)
        {
            MaterialList[i].SetVector(ShaderIDs.UIClipArea, area);
        }
    }

    private void Revert()
    {
        for (int i = 0, j = MaterialList.Count; i < j; i++)
        {
            if (MaterialList[i] != null)
            {
                MaterialList[i].SetVector(ShaderIDs.UIClipArea, DefaultRect);
            }
        }
    }

    private void Update()
    {
        if (_ClippingRect != null && MaterialList.Count != 0)
        {
            ApplyClipping();
        }
        else
        {
            enabled = false;
        }
    }

    private void OnDestroy()
    {
        for (int i = 0, j = MaterialList.Count; i < j; i++)
        {
            if (MaterialList[i] != null)
            {
                Destroy(MaterialList[i]);
            }
        }
        MaterialList.Clear();
    }

    //public RectTransform ClipRect;
    //public void Test()
    //{
    //    SetRect(ClipRect);
    //}

}
