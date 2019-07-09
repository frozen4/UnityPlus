using UnityEngine;

namespace PostProcess
{
    public class DepthOfField : PostProcessItem
    {
        int _Divide = 0;

        private int _BlurTex1Id = 0;
        private int _BokehTexId = 0;
        private int _DofParamId = 0;

        private void SetTextureSize(int divide)
        {
            _Divide = divide;
        }

        protected override void OnInit()
        {
            base.OnInit();

            SetTextureSize(1);

            _BlurTex1Id = Shader.PropertyToID("_blurTex1");
            _BokehTexId = Shader.PropertyToID("_bokehTex");
            _DofParamId = Shader.PropertyToID("_DepthOfFieldParamter");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            RenderTexture blur_texture1 = RenderTexture.GetTemporary(src.width >> _Divide, src.height >> _Divide, 0, src.format);
            blur_texture1.name = "DOFBlur1";
            RenderTexture blur_texture2 = RenderTexture.GetTemporary(src.width >> _Divide, src.height >> _Divide, 0, src.format);
            blur_texture2.name = "DOFBlur2";

            if (_parent.depthoffield_paramter.x < 0.01f && _parent.depthoffield_paramter.y < 0.01f)
            {
                _material.SetVector(_DofParamId, _parent.depthoffield_paramter);
                Graphics.Blit(src, blur_texture2, _material, 1);
                Graphics.Blit(blur_texture2, blur_texture1, _material, 2);
                Graphics.Blit(blur_texture1, dest);
            }
            else
            {
                RenderTexture bokeh_texture = RenderTexture.GetTemporary(src.width >> _Divide, src.height >> _Divide, 0, RenderTextureFormat.RHalf);
                blur_texture1.name = "DOFBokeh";

                _material.SetVector(_DofParamId, _parent.depthoffield_paramter);
                Graphics.Blit(src, bokeh_texture, _material, 0);

                _material.SetTexture(_BokehTexId, bokeh_texture);

                Graphics.Blit(src, blur_texture1, _material, 3);
                Graphics.Blit(blur_texture1, blur_texture2, _material, 1);

                Graphics.Blit(blur_texture2, blur_texture1, _material, 2);

                _material.SetTexture(_BlurTex1Id, blur_texture1);

                Graphics.Blit(src, dest, _material, 4);
                //Graphics.Blit(blurTex1, dest);

                RenderTexture.ReleaseTemporary(bokeh_texture);
            }

            RenderTexture.ReleaseTemporary(blur_texture2);
            RenderTexture.ReleaseTemporary(blur_texture1);
        }
    }
}