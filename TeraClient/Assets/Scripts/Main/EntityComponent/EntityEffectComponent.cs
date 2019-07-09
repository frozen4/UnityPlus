using System.Collections.Generic;
using GameLogic;
using UnityEngine;
using EntityVisualEffect;
using System;

namespace EntityComponent
{
    /*==========================================================
    //                    效果实现方式
    //==========================================================
    FadeInOut - 改变当前材质球的color.a
    改变当前材质球的_RimColor _RimPower 三种效果 有优先级 只生效一种:
    1） HitTwinkleWhite 闪白
    2） HitFlawsColor 破绽
    3） EliteBornColor 精英怪       
    Frozen - 更换Frozen的mat   
    DeathEffect 死亡溶解效果
    ============================================================*/
    public class RendererInfoItem
    {
        public RendererInfoItem(Renderer rd)
        {
            Render = rd;
            OriginMat = rd.sharedMaterial;
            CurrentMat = null;
        }
        public Renderer Render;
        public Material OriginMat;
        public Material CurrentMat;
    }

    public class EntityEffectComponent : MonoBehaviour, IRecyclable, ITickLogic
    {
        public static Action<EntityEffectComponent> OnRecycleCallback = null;

        // 材质球信息缓存
        private readonly Dictionary<Renderer, RendererInfoItem> _RendererInfoMap = new Dictionary<Renderer, RendererInfoItem>();

        // 以下效果，从上往下，优先级逐渐降低，上面的效果覆盖下面的效果
        private DissoveDeath _DeathEffect = null;
        private Frozen _Frozen = null;                              //冰冻效果
        //private FadeInOutEffect _FadeEffect = null;
        private readonly Dictionary<RimEffectType, EntityRimEffect> _EntityRimEffectMap = new Dictionary<RimEffectType, EntityRimEffect>();

        private int _CurEffectsMask = 0;
        private int _LastEffectMask = 0;

        public void Init()
        {
            if (_RendererInfoMap.Count > 0) return;

            _CurEffectsMask = 0;
            _LastEffectMask = 0;

            // 收集对象上所有的Renderer信息
            var mrs = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var v in mrs)
                AddRenderInfo(v);
        }

        // 在挂点增减模型（武器 翅膀等）
        public void OnModelChanged()
        {
            var mrs = gameObject.GetComponentsInChildren<Renderer>();
            for (var i = 0; i < mrs.Length; i++)
            {
                // 之前已经存在的
                RendererInfoItem info;
                if (_RendererInfoMap.TryGetValue(mrs[i], out info))
                {
                    if (info.CurrentMat != null && info.OriginMat != null)
                    {
                        // 如果处于某种状态中，需要先清空之前的状态，重新进行
                        if (info.Render != null)
                            info.Render.sharedMaterial = info.OriginMat;
                        MaterialPool.Instance.Recycle(info.CurrentMat);
                        info.CurrentMat = null;
                    }
                }
                else // 新添加的
                {
                    AddRenderInfo(mrs[i]);
                }
            }

            // 删除已经不要的
            var all = new List<Renderer>(_RendererInfoMap.Keys);
            for (var i = 0; i < all.Count; i++)
            {
                var toDelete = true;
                for (var j = 0; j < mrs.Length; j++)
                {
                    if (mrs[j] == all[i])
                    {
                        toDelete = false;
                        break;
                    }
                }

                if (toDelete)
                    _RendererInfoMap.Remove(all[i]);
            }

            _LastEffectMask = 0;
        }

        // 换装 （材质球发生变化）
        public void OnMaterialChanged(SkinnedMeshRenderer mr)
        {
            if (mr == null) return;

            RendererInfoItem info;
            if (_RendererInfoMap.TryGetValue(mr, out info))
            {
                if (info.CurrentMat != null && info.OriginMat != null)
                {
                    // 回收当前材质球
                    if (info.Render != null && info.Render.sharedMaterial == info.CurrentMat)
                    {
                        Common.HobaDebuger.LogErrorFormat("Logic Error: Material {0} is in use when call RecycleMaterial", info.OriginMat.name);
                        info.Render.sharedMaterial = info.OriginMat;
                    }
                    MaterialPool.Instance.Recycle(info.CurrentMat);
                }
                info.CurrentMat = null;
                info.OriginMat = mr.sharedMaterial;
                _LastEffectMask = 0;
            }
            else
            {
                Common.HobaDebuger.LogWarning("Can not find Renderer Info when OnMaterialChanged");
            }
        }

        #region 对外效果接口
        public void StartTwinkleWhite(float duration, float r, float g, float b, float a, float power)
        {
            if (_DeathEffect != null /*|| _FadeEffect != null*/) return;
            var type = RimEffectType.TwinkleWhite;

            EntityRimEffect effect;
            if (!_EntityRimEffectMap.TryGetValue(type, out effect))
            {
                effect = EffectObjectPool.Get(this, EffectType.HitTwinkleWhite) as EntityRimEffect;
                _EntityRimEffectMap.Add(type, effect);
            }

            if (effect == null)
            {
                effect = EffectObjectPool.Get(this, EffectType.HitTwinkleWhite) as EntityRimEffect;
                _EntityRimEffectMap[type] = effect;
            }

            var hitTwinkleWhite = effect as HitTwinkleWhite;
            if (hitTwinkleWhite != null)
            {
                _CurEffectsMask |= hitTwinkleWhite.Mask;
                hitTwinkleWhite.Start(duration, r, g, b, a, power);
            }
        }

        public void StopTwinkleWhite()
        {
            var type = RimEffectType.TwinkleWhite;
            EntityRimEffect effect;
            if (_EntityRimEffectMap.TryGetValue(type, out effect))
            {
                effect.Stop();
                _CurEffectsMask &= ~effect.Mask;
                _EntityRimEffectMap.Remove(type);

                foreach (var kv in _EntityRimEffectMap)
                {
                    if (kv.Value != null)
                        kv.Value.Restart();
                }
            }
        }

        public void StartHitFlawsColor(float r, float g, float b, float power)
        {
            if (_DeathEffect != null /*|| _FadeEffect != null*/) return;
            var type = RimEffectType.HitFlawsColor;
            EntityRimEffect effect;
            if (!_EntityRimEffectMap.TryGetValue(type, out effect))
            {
                effect = EffectObjectPool.Get(this, EffectType.HitFlawsColor) as EntityRimEffect;
                _EntityRimEffectMap.Add(type, effect);
            }

            if (effect == null)
            {
                effect = EffectObjectPool.Get(this, EffectType.HitFlawsColor) as EntityRimEffect;
                _EntityRimEffectMap[type] = effect;
            }

            var hitFlawsColor = effect as HitFlawsColor;
            if (hitFlawsColor != null)
            {
                _CurEffectsMask |= hitFlawsColor.Mask;
                hitFlawsColor.Start(r, g, b, power);
            }
        }

        public void StopHitFlawsColor()
        {
            var type = RimEffectType.HitFlawsColor;
            EntityRimEffect effect;
            if (_EntityRimEffectMap.TryGetValue(type, out effect))
            {
                effect.Stop();
                _CurEffectsMask &= ~effect.Mask;
                _EntityRimEffectMap.Remove(type);

                foreach (var kv in _EntityRimEffectMap)
                {
                    if (kv.Value != null)
                        kv.Value.Restart();
                }
            }
        }

        public void EnableEliteBornColor(float r, float g, float b, float power)
        {
            if (_DeathEffect != null /*|| _FadeEffect != null*/) return;
            var type = RimEffectType.EliteBornColor;

            EntityRimEffect effect;
            if (!_EntityRimEffectMap.TryGetValue(type, out effect))
            {
                effect = EffectObjectPool.Get(this, EffectType.EliteBornColor) as EntityRimEffect;
                _EntityRimEffectMap.Add(type, effect);
            }

            if (effect == null)
            {
                effect = EffectObjectPool.Get(this, EffectType.EliteBornColor) as EntityRimEffect;
                _EntityRimEffectMap[type] = effect;
            }

            var eliteBornColor = effect as EliteBornColor;
            if (eliteBornColor != null)
            {
                _CurEffectsMask |= eliteBornColor.Mask;
                eliteBornColor.Start(r, g, b, power);
            }
        }

        public void EnableFrozenEffect(bool enable)
        {
            if (_DeathEffect != null) return;

            if (enable)
            {
                if (null == _Frozen)
                    _Frozen = EffectObjectPool.Get(this, EffectType.Frozen) as Frozen;
                
                if(_Frozen != null)
                    _Frozen.Start(() => { if (_Frozen != null) _CurEffectsMask |= _Frozen.Mask; });
            }
            else
            {
                if (_Frozen != null)
                {
                    _Frozen.Stop();
                    _CurEffectsMask &= ~_Frozen.Mask;
                    _Frozen = null;
                }
            }

            var animations = GetComponentsInChildren<Animation>();
            foreach (var v in animations)
            {
                if (null != v && v.enabled == enable)
                    v.enabled = !enable;
            }
        }

        public void DissoveToDeath(float r, float g, float b, float a, float corpseStayDuration)
        {
            if (_DeathEffect != null) return;

            _DeathEffect = EffectObjectPool.Get(this, EffectType.DissolveDeath) as DissoveDeath;
            if (_DeathEffect != null)
            {
                _DeathEffect.Start(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f, corpseStayDuration);
                _CurEffectsMask |= _DeathEffect.Mask;
            }
        }
        #endregion

        public void Tick(float dt)
        {
            if (_CurEffectsMask != _LastEffectMask)
            {
                ChangeMaterials();
                _LastEffectMask = _CurEffectsMask;
            }

            #region Frozen
            if (_Frozen != null)
            {
                // 冰冻效果无Update，通过服务器逻辑进行效果增删
                // 冰冻效果覆盖以下所有效果
                return;
            }
            #endregion // Frozen

            #region Death
            if (_DeathEffect != null)
            {
                if (!_DeathEffect.Update())
                {
                    _DeathEffect.Stop();
                    _CurEffectsMask &= ~_DeathEffect.Mask;
                    _DeathEffect = null;
                }
                return;
            }
            #endregion // Death

            #region FadeEffect
//             if (_FadeEffect != null)
//             {
//                 if (!_FadeEffect.Update())
//                 {
//                     _FadeEffect.Stop();
//                     _CurEffectsMask &= ~_FadeEffect.Mask;
//                     _FadeEffect = null;
//                 }
//                 return;
//             }
            #endregion // FadeEffect

            #region RimEffect
            var effectType = GetMostPriorRimEffect();
            if (effectType != RimEffectType.None)
            {
                var mostPriorEffect = _EntityRimEffectMap[effectType];
                if (!mostPriorEffect.Update())
                {
                    mostPriorEffect.Stop();
                    _CurEffectsMask &= ~mostPriorEffect.Mask;
                    _EntityRimEffectMap.Remove(effectType);

                    // 重启其他可能存在的Rim效果
                    foreach (var kv in _EntityRimEffectMap)
                    {
                        if (kv.Value != null)
                            kv.Value.Restart();
                    }
                }
            }
            #endregion // RimEffect
        }

        public Dictionary<Renderer, RendererInfoItem> GetRendererInfoMap()
        {
            return _RendererInfoMap;
        }

        private bool HasEffect(EffectType efftype)
        {
            if ((_CurEffectsMask & (1 << (int)efftype)) != 0)
                return true;
            return false;
        }

        private void AddRenderInfo(Renderer r)
        {
            var psr = r as ParticleSystemRenderer;
            if (psr == null)
            {
                // 特效不参与效果，特效组件 XffectComponent 和 CustomTrailRenderer 会创建MeshRenderer
                if (r.gameObject.GetComponentInParent<ReusableFx>() == null)
                    _RendererInfoMap.Add(r, new RendererInfoItem(r));
            }
        }

        private void ChangeMaterials()
        {
            // 无任何效果，切回最初材质球，回收当前材质球
            if (_CurEffectsMask == 0)
            {
                var hasFrozen = (_LastEffectMask & (1 << (int) EffectType.Frozen)) != 0;
                foreach (var v in _RendererInfoMap)
                {
                    var info = v.Value;
                    if (info.Render != null)
                    {
                        if (!hasFrozen)
                        {
                            MaterialPool.Instance.Recycle(info.CurrentMat);
                            info.CurrentMat = null;
                            info.Render.sharedMaterial = info.OriginMat;
                        }
                        else
                        {
                            var newMats = new Material[1];
                            newMats[0] = info.OriginMat;
                            info.Render.sharedMaterials = newMats;
                        }
                    }
                    //else
                   //     Common.HobaDebuger.LogWarningFormat("Render has been destroyed when recycle Material {0}", info.OriginMat.name);

                    info.CurrentMat = null;
                }
            }
            else  // 存在效果时，根据效果切换材质球属性
            {
                // add frozen effect
                if ((_LastEffectMask & (1 << (int)EffectType.Frozen)) == 0 && HasEffect(EffectType.Frozen))
                {
                    foreach (var v in _RendererInfoMap)
                    {
                        if (v.Value.CurrentMat != null)
                        {
                            MaterialPool.Instance.Recycle(v.Value.CurrentMat);
                            v.Value.CurrentMat = null;
                        }

                        if (v.Value.Render != null)
                        {
                            var newMats = new Material[2];
                            newMats[0] = v.Value.OriginMat;
                            newMats[1] = Frozen.ForzenMaterial;

                            v.Value.Render.sharedMaterials = newMats;
                        }
                    }
                    return;
                }

                // remove frozen effect
                if ((_LastEffectMask & (1 << (int)EffectType.Frozen)) != 0 && !HasEffect(EffectType.Frozen))
                {
                    foreach (var v in _RendererInfoMap)
                    {
                        if (v.Value.Render != null)
                        {
                            var newMats = new Material[1];
                            newMats[0] = v.Value.OriginMat;
                            v.Value.Render.sharedMaterials = newMats;
                        }
                    }
                }

                // add 其他效果
                if (((_LastEffectMask & (1 << (int) EffectType.FadeInOut)) == 0 && HasEffect(EffectType.FadeInOut))
                    || ((_LastEffectMask & (1 << (int) EffectType.HitTwinkleWhite)) == 0 && HasEffect(EffectType.HitTwinkleWhite))
                    || ((_LastEffectMask & (1 << (int) EffectType.EliteBornColor)) == 0 && HasEffect(EffectType.EliteBornColor))
                    || ((_LastEffectMask & (1 << (int) EffectType.HitFlawsColor)) == 0 && HasEffect(EffectType.HitFlawsColor))
                    || ((_LastEffectMask & (1 << (int) EffectType.DissolveDeath)) == 0 && HasEffect(EffectType.DissolveDeath)))
                {
                    var e = _RendererInfoMap.GetEnumerator();
                    while (e.MoveNext())
                    {
                        RendererInfoItem rendererInfo = e.Current.Value;
                        if (rendererInfo.CurrentMat == null)
                        {
                            rendererInfo.CurrentMat = MaterialPool.Instance.Get(rendererInfo.OriginMat);
                            if (rendererInfo.Render != null)
                                rendererInfo.Render.sharedMaterial = rendererInfo.CurrentMat;
                        }
                    }
                    e.Dispose();
                }
            }
        }

        private RimEffectType GetMostPriorRimEffect()
        {
            var it = _EntityRimEffectMap.GetEnumerator();

            EntityRimEffect result = null;

            while (it.MoveNext())
            {
                var keyValue = it.Current;
                if (keyValue.Value != null)
                {
                    if (result == null || (int)result.Priority > (int)keyValue.Value.Priority)
                        result = keyValue.Value;
                }
            }
            it.Dispose();

            return result != null ? result.SubType : RimEffectType.None;
        }

        public void CopyEntityEffect(ref EntityEffectComponent ins)
        {
            if (null != ins)
            {
                if (HasEffect(EffectType.EliteBornColor))
                {
                    EntityRimEffect eff;
                    if (_EntityRimEffectMap.TryGetValue(RimEffectType.EliteBornColor, out eff))
                        EnableEliteBornColor(eff.RimColor.r, eff.RimColor.g, eff.RimColor.b, eff.RimPower);
                    
                }

                if (HasEffect(EffectType.HitFlawsColor))
                {
                    EntityRimEffect eff;
                    if (_EntityRimEffectMap.TryGetValue(RimEffectType.HitFlawsColor, out eff))
                        StartHitFlawsColor(eff.RimColor.r, eff.RimColor.g, eff.RimColor.b, eff.RimPower);
                }

                if (HasEffect(EffectType.Frozen))
                    EnableFrozenEffect(true);

                // 其他效果可以忽略
            }
        }

        public void OnRecycle()
        {
            if (OnRecycleCallback != null)
                OnRecycleCallback(this);

            _CurEffectsMask = 0;
            ChangeMaterials();
            _LastEffectMask = 0;

//             if (_FadeEffect != null)
//             {
//                 _FadeEffect.Stop();
//                 _FadeEffect = null;
//             }

            if (_DeathEffect != null)
            {
                _DeathEffect.Stop();
                _DeathEffect = null;
            }

            foreach (var kv in _EntityRimEffectMap)
            {
                if (kv.Value != null)
                    kv.Value.Stop();
            }
            _EntityRimEffectMap.Clear();

            if (_Frozen != null)
            {
                _Frozen.Stop();
                _Frozen = null;
            }
        }
    }
}
