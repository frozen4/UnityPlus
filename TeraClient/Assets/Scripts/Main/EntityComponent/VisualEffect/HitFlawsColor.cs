using UnityEngine;
using EntityComponent;

namespace EntityVisualEffect
{
    public class HitFlawsColor : EntityRimEffect
    {
        private bool _HasRimPropertySetted = false;

        public HitFlawsColor()
        {
            Type = EffectType.HitFlawsColor;
            SubType = RimEffectType.HitFlawsColor;
            Priority = RimEffectPriority.HitFlawsColor;
        }

        public void Start(float r, float g, float b, float power)
        {
            RimColor = new Color(r, g, b, 1);
            RimPower = power;
            _HasRimPropertySetted = false;
        }

        public override void Restart()
        {
            _HasRimPropertySetted = false;
        }

        public override bool Update()
        {
            if (!_HasRimPropertySetted)
            {
                var e = RendererInfoMap.GetEnumerator();
                while (e.MoveNext())
                {
                    var rendererInfo = e.Current.Value;
                    if (rendererInfo.CurrentMat != null)
                    {
                        if (rendererInfo.CurrentMat.HasProperty(ShaderIDs.ShaderPropertyRimColor))
                            rendererInfo.CurrentMat.SetColor(ShaderIDs.ShaderPropertyRimColor, RimColor);

                        if (rendererInfo.CurrentMat.HasProperty(ShaderIDs.ShaderPropertyRimPower))
                            rendererInfo.CurrentMat.SetFloat(ShaderIDs.ShaderPropertyRimPower, RimPower);

                        _HasRimPropertySetted = true;
                    }
                }
                e.Dispose();
            }

            return true;
        }

        public override void Stop()
        {
            _HasRimPropertySetted = false;
            base.Stop();
        }
    }
}
