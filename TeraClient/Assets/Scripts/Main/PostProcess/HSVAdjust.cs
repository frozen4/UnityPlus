using UnityEngine;

namespace PostProcess
{
    public class HSVAdjust : PostProcessItem
    {
        private int _HSV1Id = 0;
        private int _HSV2Id = 0;

        protected override void OnInit()
        {
            _HSV1Id = Shader.PropertyToID("_HSV1");
            _HSV2Id = Shader.PropertyToID("_HSV2");
        }
        protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_material == null) return;

            _material.SetVector(_HSV1Id, _parent._hsv_adjust_paramters);
            _material.SetVector(_HSV2Id, _parent.brightness_contrast_paramters);

            Graphics.Blit(src, dest, _material, 0);
        }
    }
}