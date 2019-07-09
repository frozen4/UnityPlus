using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class GNewUIBlur : MonoBehaviour
{
    private Camera _UICamera;
    private RenderTexture _Tex;
    private RawImage _Img;

    //public enum BlurPass
    //{
    //    ConeTap = 0,
    //    Disk16 = 1,
    //}
    //public BlurPass Tech;

    public int Dist = 2;    //blur size
    public int Itr = 2;     //blur itr
    public int FD = 2;      //first down size
    //[Range(0, 1)]
    //public float Alpha = 0.9f;

    private void OnEnable()
    {
        if (_UICamera == null)
        {
#if IN_GAME
            _UICamera = Main.UICamera;
#else
            _UICamera = GameObject.Find("UICamera").GetComponent<Camera>();
#endif
        }

        if (_Img == null)
        {
            _Img = GetComponent<RawImage>();
        }

        if (_Tex) RenderTexture.ReleaseTemporary(_Tex);
        _Img.color = new Color(0, 0, 0, 0);

        StopCoroutine("Show");
        StartCoroutine(Show());
    }

    private void OnDisable()
    {
        StopCoroutine("Show");
        if (_Tex)
        {
            RenderTexture.ReleaseTemporary(_Tex);
            _Tex = null;
        }
    }

    private IEnumerator Show()
    {
        yield return (new WaitForEndOfFrame());

        if (_UICamera != null)
        {
            _Tex = CaptureCamera(_UICamera, new Rect(0, 0, _UICamera.pixelWidth, _UICamera.pixelHeight));
            _Img.texture = _Tex;
        }

        yield return (new WaitForEndOfFrame());
        _Img.color = new Color(1, 1, 1, 1);
    }

    private RenderTexture CaptureCamera(Camera camera, Rect rect)
    {
        RenderTexture rt = RenderTexture.GetTemporary((int)rect.width, (int)rect.height, 0);
        rt.name = "NewUIBlurCaptureCamerRT";
        camera.targetTexture = rt;
        camera.Render();
        camera.targetTexture = null;

        BlitImage(ref rt, @"TERA/UI/Blur");

        return rt;
    }

    private void BlitImage(ref RenderTexture rt_target, string shader_name)
    {
        Shader s_shader =
#if IN_GAME
            ShaderManager.Instance.FindShader(shader_name);
#else
            Shader.Find(shader_name);
#endif

        Material mat = new Material(s_shader);
        RenderTexture rt = rt_target;
        RenderTexture rt2;

        if (FD > 1)
        {
            rt2 = RenderTexture.GetTemporary(rt_target.width / FD, rt_target.height / FD);
            rt2.name = "NewUIBlurRT";
            Graphics.Blit(rt, rt2);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
        }
        

        for (int i = 0; i < Itr; i++)
        {
            rt2 = RenderTexture.GetTemporary(rt.width / Dist, rt.height / Dist);
            rt2.name = string.Format("NewUIBlurRT-{0}", i);
            //rt2 = RenderTexture.GetTemporary(rt.width, rt.height);
            if (i % 2 == 0)
            {
                mat.SetVector(ShaderIDs.UIBlurOffsets, new Vector4(Dist - 1, Dist - 1, Dist - 1, 1 - Dist));
            }
            else
            {
                mat.SetVector(ShaderIDs.UIBlurOffsets, new Vector4(Dist - 1, 0, 0, Dist - 1));
            }


            Graphics.Blit(rt, rt2, mat, 0);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
        }

        Destroy(mat);

        rt_target = rt;
    }
}
