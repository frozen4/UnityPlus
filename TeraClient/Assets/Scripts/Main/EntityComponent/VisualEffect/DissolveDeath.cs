using EntityComponent;
using UnityEngine;

namespace EntityVisualEffect
{
    public class DissoveDeath : DeathEffect
    {
        private Color _DeathColor = new Color(174.0f / 255.0f, 69.0f / 255.0f, 0, 128.0f / 255.0f);

        private float _CorpseStayDuration = 0;

        private float _StartTime;
        private bool _HasRimPropertySetted = false;

        public DissoveDeath()
        {
            Type = EffectType.DissolveDeath;
            SubType = DeathEffectType.DissolveDeath;
        }

        public void Start(float r, float g, float b, float a, float duration)
        {
            _DeathColor = new Color(r, g, b, a);
            _StartTime = Time.time;
            _CorpseStayDuration = duration;
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
                        rendererInfo.CurrentMat.SetFloat(ShaderIDs.SinceLevelLoadedTime, Time.timeSinceLevelLoad);
                        rendererInfo.CurrentMat.SetFloat(ShaderIDs.DeathDuration, _CorpseStayDuration);
                        rendererInfo.CurrentMat.renderQueue = 3900;
                        rendererInfo.CurrentMat.SetColor(ShaderIDs.ShaderPropertyDeathColor, _DeathColor);

                        _HasRimPropertySetted = true;
                    }
                }
                e.Dispose();
            }

            return Time.time - _StartTime < _CorpseStayDuration;
        }

        public override void Stop()
        {
            _CorpseStayDuration = 0;
            _HasRimPropertySetted = false;
            _StartTime = 0;
            base.Stop();
        }
    }
}
