using System.Collections.Generic;
using UnityEngine;
using EntityComponent;
using Hoba.ObjectPool;

namespace EntityVisualEffect
{
    // 与lua逻辑层保持一致
    public enum EffectType
    {
        Normal,
        //不可共存 
        HitFlawsColor = 6,
        HitTwinkleWhite = 7,
        EliteBornColor = 8,
        Frozen = 10,        
        // 死亡效果
        DissolveDeath = 14,
        // FadeIn/FadeOut，此过程中，其余效果都无效
        FadeInOut = 30,
    };

    public abstract class BaseEffect : PooledObject
	{
		protected EntityEffectComponent EffectMan;
        
        public EffectType Type { get; set; }

        public int Mask { get { return 1 << (int)Type; } }

        public Dictionary<Renderer, RendererInfoItem> RendererInfoMap
        {
            get
            {
                if (EffectMan != null)
                    return EffectMan.GetRendererInfoMap();
                else
                    return null;
            }
        }

        protected BaseEffect(){ }

	    public void Register(EntityEffectComponent man)
	    {
            EffectMan = man;
        }

        public virtual bool Update()
		{
            return true;
		}

        public virtual void Stop()
        {
            EffectMan = null;
            Dispose();
        }
    }
}
