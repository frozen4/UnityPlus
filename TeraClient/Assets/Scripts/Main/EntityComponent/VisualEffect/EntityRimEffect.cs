using EntityComponent;
using UnityEngine;

namespace EntityVisualEffect
{
    //类型
    public enum RimEffectType
    {
        TwinkleWhite,
        EliteBornColor,
        HitFlawsColor,
        MaxCount,

        None = -1,
    }

    //优先级
    public enum RimEffectPriority
    {
        TwinkleWhite,
        HitFlawsColor,
        EliteBornColor,
        MaxCount,
    }

    public abstract class EntityRimEffect : BaseEffect
    {
        public Color RimColor;
        public float RimPower;

        public RimEffectType SubType { get; set; }
        public RimEffectPriority Priority { get; set; }

        protected EntityRimEffect() {}

        public virtual void Restart() {}
    }
}
