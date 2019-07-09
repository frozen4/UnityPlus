using UnityEngine;
using EntityComponent;

namespace EntityVisualEffect
{
    public class FadeInOutEffect : BaseEffect
    {
        private const string ColorProp = "_Color";

        private float _StartAlpha = 0;
        private float _EndAlpha = 1;
        private float _Duration = 2.0f;
        private float _StartTime = 0;

        public FadeInOutEffect()
		{
            Type = EffectType.FadeInOut;
        }

        public void Start(float duration, float startAlpha, float endAlpha)
        {
            _Duration = duration;
            _StartAlpha = startAlpha;
            _EndAlpha = endAlpha;
            _StartTime = Time.time;
        }

        public override bool Update()
        {
            if (Time.time - _StartTime > _Duration)
                return false;

            if(RendererInfoMap != null)
            {
                float curAlpha = Mathf.Lerp(_StartAlpha, _EndAlpha, (Time.time - _StartTime) / _Duration);
                var e = RendererInfoMap.GetEnumerator();
                while (e.MoveNext())
                {
                    RendererInfoItem rendererInfo = e.Current.Value;
                    if (rendererInfo.CurrentMat != null && rendererInfo.CurrentMat.HasProperty(ColorProp))
                    {
                        var color = rendererInfo.OriginMat.GetColor(ColorProp);
                        color.a = color.a * curAlpha;
                        rendererInfo.CurrentMat.SetColor(ColorProp, color);

                        // TODO: 不用放在Update中
                        if (rendererInfo.OriginMat.renderQueue == MaterialUtility.RenderQueue_AlphaTest)
                            MaterialUtility.SetMaterialRenderingMode(rendererInfo.CurrentMat, RenderingMode.CutOut);
                        else if (rendererInfo.OriginMat.renderQueue == MaterialUtility.RenderQueue_Geometry)
                            MaterialUtility.SetMaterialRenderingMode(rendererInfo.CurrentMat, RenderingMode.Fade);
                    }
                }
                e.Dispose();
            }

            return true;
        }

        public override void Stop()
        {
            base.Stop();
            _Duration = 2f;
            _StartAlpha = 0;
            _EndAlpha = 1;
            _StartTime = 0;
            Dispose();
        }
    }
}

