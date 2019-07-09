using EntityComponent;
using Hoba.ObjectPool;

namespace EntityVisualEffect
{
    public class EffectObjectPool
    {
        private static readonly ObjectPool<DissoveDeath> _DissoveDeathObjectList = new ObjectPool<DissoveDeath>(1, 5, () => { return new DissoveDeath(); });
        private static readonly ObjectPool<Frozen> _FrozenObjectList = new ObjectPool<Frozen>(1, 5, () => { return new Frozen(); });
        //private static readonly ObjectPool<FadeInOutEffect> _FadeInOutObjectPool = new ObjectPool<FadeInOutEffect>(1, 5, () => { return new FadeInOutEffect(); });

        private static readonly ObjectPool<HitTwinkleWhite> _TwinkleWhiteObjectPool = new ObjectPool<HitTwinkleWhite>(1, 10, () => { return new HitTwinkleWhite(); });
        private static readonly ObjectPool<HitFlawsColor> _HitFlawsColorObjectPool = new ObjectPool<HitFlawsColor>(1, 5, () => { return new HitFlawsColor(); });
        private static readonly ObjectPool<EliteBornColor> _EliteBornColorObjectPool = new ObjectPool<EliteBornColor>(1, 5, () => { return new EliteBornColor(); });

        public static BaseEffect Get(EntityEffectComponent man, EffectType effectType)
        {
            BaseEffect effect = null;
            if (effectType == EffectType.DissolveDeath)
                effect = _DissoveDeathObjectList.GetObject();
            else if (effectType == EffectType.HitTwinkleWhite)
                effect = _TwinkleWhiteObjectPool.GetObject();
            else if (effectType == EffectType.EliteBornColor)
                effect = _EliteBornColorObjectPool.GetObject();
            else if (effectType == EffectType.HitFlawsColor)
                effect = _HitFlawsColorObjectPool.GetObject();
            else if (effectType == EffectType.Frozen)
                effect = _FrozenObjectList.GetObject();
//             else if (effectType == EffectType.FadeInOut)
//                 effect = _FadeInOutObjectPool.GetObject();

            if(effect != null)
                effect.Register(man);

            return effect;
        }

        public static void ShowDiagnostics()
        {
#if UNITY_EDITOR
            _DissoveDeathObjectList.ShowDiagnostics("DissoveDeath");
            _FrozenObjectList.ShowDiagnostics("Frozen");
            //_FadeInOutObjectPool.ShowDiagnostics("FadeInOutEffect");
            _TwinkleWhiteObjectPool.ShowDiagnostics("HitTwinkleWhite");
            _HitFlawsColorObjectPool.ShowDiagnostics("HitFlawsColor");
            _EliteBornColorObjectPool.ShowDiagnostics("EliteBornColor");
#endif
        }
    }
}

