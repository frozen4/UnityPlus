using UnityEngine;
using System.Collections;

namespace PostProcess
{
    public class MotionBlur : PostProcessItem
    {
        int _Divide = 0;

        bool rebuildacc;
        RenderTexture accumulation;

        public float Duration = 0;
        public float Fade_in_timing = 0;
        public float Fade_out_timing = 0;
        public float Elapse = 0;

        private int _BlendFactorId = 0;


        public void SetTextureSize(int divide)
        {
            _Divide = divide;
        }

        protected override void OnInit()
        {
            base.OnInit();

            SetTextureSize(0);

            _BlendFactorId = Shader.PropertyToID("_BlendFactor");

            rebuildacc = true;

            Duration = 0;
            Fade_in_timing = 0;
            Fade_out_timing = 0;
            Elapse = 0;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (accumulation)
            {
                Object.Destroy(accumulation);
                accumulation = null;
            }

            rebuildacc = false;

            Duration = 0;
            Fade_in_timing = 0;
            Fade_out_timing = 0;
            Elapse = 0;
        }

        protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            accumulation = PostProcessChain.CreateRenderTexture(src.width >> _Divide, src.height >> _Divide, 0, RenderTextureFormat.Default, accumulation);

            if (rebuildacc)
            {
                Graphics.Blit(src, accumulation);

                Graphics.Blit(src, dest);
                rebuildacc = false;
            }
            else
            {
                float f = _parent.MotionBlurParamter.x;

                if (Duration > 0 && Elapse < Duration)
                {
                    float t = Elapse / Duration;
                    if (t < Fade_in_timing)
                    {
                        f *= Mathf.Clamp01(t / Fade_in_timing);
                    }
                    else if (t > Fade_out_timing)
                    {
                        f *= Mathf.Clamp01(((1 - t) / (1 - Fade_out_timing)));
                    }

                    _material.SetFloat(_BlendFactorId, f);
                    Graphics.Blit(src, accumulation, _material);

                    Graphics.Blit(accumulation, dest);

                    Elapse += Time.deltaTime;
                }
                else
                {
                    Graphics.Blit(src, dest);
                    _parent.EnableMotionBlur = false;
                }
            }
        }
    }
}