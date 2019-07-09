using UnityEngine;
using System.Collections;

namespace PostProcess
{
    public class Fog : PostProcessItem
    {
        private int _InverseViewId = 0;
        private int _FogColorId = 0;
        private int _FogParamId = 0;

        protected override void OnInit()
        {
            _InverseViewId = Shader.PropertyToID("_InverseView");
            _FogColorId = Shader.PropertyToID("_FogColor");
            _FogParamId = Shader.PropertyToID("fog_paramter");
        }

        protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            _material.SetMatrix(_InverseViewId, _parent.target_camera.cameraToWorldMatrix);
            _material.SetColor(_FogColorId, _parent.fog_color);
            _material.SetVector(_FogParamId, _parent.fog_paramter);

            Graphics.Blit(src, dest, _material, 0);
        }
    }
}