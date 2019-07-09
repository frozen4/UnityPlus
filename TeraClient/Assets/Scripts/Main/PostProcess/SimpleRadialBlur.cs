using UnityEngine;

namespace PostProcess
{
    public class SimpleRadialBlur : PostProcessItem
    {
        private int _PosId = 0;
        private int _ParamsId = 0;

        protected override void OnInit()
        {
            _PosId = Shader.PropertyToID("_Pos");
            _ParamsId = Shader.PropertyToID("_Params");
        }

        protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            _material.SetVector(_PosId, _parent.radial_blur_pos);
            _material.SetVector(_ParamsId, _parent.radial_blur_paramters);
            Graphics.Blit(src, dest, _material);
        }
    }
}