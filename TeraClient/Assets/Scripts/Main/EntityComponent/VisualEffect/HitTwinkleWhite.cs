using UnityEngine;
using EntityComponent;

namespace EntityVisualEffect
{
    //闪白逻辑
    //传入参数duration控制总时间
    //DestRimColor,DestRimPower定义最终满白色的颜色和权重
    //KeepMaxDurationPercent控制满白时间,然后开始线性衰减
    //支持重入
    //需要保证所有战斗相关材质(body,hair,face)都带有闪白相关参数

    public class HitTwinkleWhite : EntityRimEffect
    {
        private const float KEEP_MAX_DURATION_PERCENT = 0.66f;
        private const float MIN_DURATION = 0.1f;
        private const float MAX_DURATION = 10.0f;

		private float _TurningTime;
		private float _EndTime;
		private float _FadeoutDuration;
	    private Color _CurrentColor;
	    private float _CurrentPower;

        public HitTwinkleWhite()
		{
            Type = EffectType.HitTwinkleWhite;
            SubType = RimEffectType.TwinkleWhite;
            Priority = RimEffectPriority.TwinkleWhite;
        }

        public void Start(float duration, float r, float g, float b, float a, float power)
		{
            RimColor = new Color(r, g, b, a);
            RimPower = power;

            if (duration < MIN_DURATION) duration = MIN_DURATION;
			if (duration > MAX_DURATION) duration = MAX_DURATION;
            _TurningTime = Time.time + duration * KEEP_MAX_DURATION_PERCENT;
			_EndTime = Time.time + duration;
			_FadeoutDuration = (1 - KEEP_MAX_DURATION_PERCENT) * duration;
        }

        public override bool Update()
        {
            if (RendererInfoMap == null) return false;

			if (Time.time < _TurningTime)
			{
				_CurrentColor = RimColor;
				_CurrentPower = RimPower;
			}
			else
			{
				var ratio = (Time.time - _TurningTime) / _FadeoutDuration;
                _CurrentPower = Mathf.Lerp(RimPower, 0, ratio);
			}

            var e = RendererInfoMap.GetEnumerator();
            while (e.MoveNext())
            {
                var rendererInfo = e.Current.Value;
                if(rendererInfo.CurrentMat != null)
                {
                    if(rendererInfo.CurrentMat.HasProperty(ShaderIDs.ShaderPropertyRimColor))
                        rendererInfo.CurrentMat.SetColor(ShaderIDs.ShaderPropertyRimColor, _CurrentColor);                        
                        
                    if (rendererInfo.CurrentMat.HasProperty(ShaderIDs.ShaderPropertyRimPower))
                        rendererInfo.CurrentMat.SetFloat(ShaderIDs.ShaderPropertyRimPower, _CurrentPower);
                }
            }
            e.Dispose();
            
            return Time.time < _EndTime;
        }


        public override void Stop()
        {
            _TurningTime = 0;
            _EndTime = 0;
            base.Stop();
        }
    }
}
