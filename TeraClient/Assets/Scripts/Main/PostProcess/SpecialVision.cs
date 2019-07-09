using UnityEngine;
using System.Collections;

namespace PostProcess
{
    public class SpecialVision : PostProcessItem
    {
        private int _MainColorId = 0;
        private int _AddFactorId = 0;

        protected override void OnInit()
        {
            _MainColorId = Shader.PropertyToID("_MainColor");
            _AddFactorId = Shader.PropertyToID("_AddFactor");
        }

        protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            _material.SetColor(_MainColorId, _parent.vision_color);
            _material.SetVector(_AddFactorId, _parent.special_vision_paramters);

			Graphics.Blit(src, dest, _material, 0);
        }
    }
}