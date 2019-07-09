using UnityEngine;
using System.Collections.Generic;

public static class CGhostEffectMan
{
    private static Transform DivideRoot = null;
    private static Transform PoolRoot = null;


    // 当前版本一个技能中只能有一个幻影事件
    private static readonly Dictionary<int, GameObject> _ShapeTemplateMap = new Dictionary<int, GameObject>();
    private static readonly Dictionary<int, UnityObjectPool> _ModelPoolMap = new Dictionary<int, UnityObjectPool>();
    
    public static GameObject CreateGhostModel(int skillId, GameObject model , float r, float g, float b, float pow)
    {
        if (model == null) return null;

        if (null == DivideRoot)
        {
            var root = GameObject.Find("ModeDivide");
            if (null == root)
                root = new GameObject("ModeDivide");

            if (PoolRoot == null)
            {
                PoolRoot = root.transform.Find("Caches");

                if (PoolRoot == null)
                {
                    var poolRoot = new GameObject("Caches");
                    PoolRoot = poolRoot.transform;
                    PoolRoot.parent = root.transform;
                    PoolRoot.localPosition = Vector3.zero;
                }
            }

            DivideRoot = root.transform;
            DivideRoot.position = new Vector3(-1000f, 0, 0);
        }

        
        if (!_ModelPoolMap.ContainsKey(skillId))
        {
            var role = Object.Instantiate(model);
#if UNITY_EDITOR
            role.name = "Template";
#endif
            // TODO: 暂时方案
            var wing = role.FindChildRecursively("HangPoint_Wing");
            if (null != wing)
                Object.Destroy(wing);

            var fxOnes = role.GetComponentsInChildren<CFxOne>();
            foreach (var fx in fxOnes)
            {
                // Instantiate复制的FxOne，非CFxCacheMan标准流程创建，故不能标准回收
                fx.OnStop = null;
                fx.Stop();
                Object.Destroy(fx.gameObject);
            }

            var list = role.GetComponentsInChildren<FxDuration>();
            for (int i = 0; i < list.Length; ++i)
            {
                if (null != list[i].gameObject)
                    Object.Destroy(list[i].gameObject);
            }

            var blurBoots = role.GetComponentsInChildren<RadialBlurBoot>();
            for (int i = 0; i < blurBoots.Length; ++i)
                Object.Destroy(blurBoots[i]);

            var effectManager = role.GetComponent<EntityComponent.EntityEffectComponent>();
            if (null != effectManager)
                Object.Destroy(effectManager);

            var col = role.GetComponent<CapsuleCollider>();
            if (null != col)
                Object.Destroy(col);

            var hp = role.GetComponent<HangPointHolder>();
            if (null != hp)
                Object.Destroy(hp);

            var am = role.GetComponent<EntityComponent.OutwardComponent>();
            if (null != am)
                Object.Destroy(am);

            var cscb = role.GetComponent<EntityComponent.CombatStateChangeBehaviour>();
            if (null != cscb)
                Object.Destroy(cscb);

            var pet = role.GetComponent<EntityComponent.CPhysicsEventTransfer>();
            if (null != pet)
                Object.Destroy(pet);

            var fst = role.GetComponent<Footsteptouch>();
            if (null != fst)
                Object.Destroy(fst);

            var au = role.GetComponent<Animation>();
            if (null != au)
                au.enabled = false;

            var mat = new Material(ShaderManager.Instance.FindShader("TERA/Character/GhostEffect"));
            var mrs = role.GetComponentsInChildren<Renderer>();
            foreach (var v in mrs)
                v.sharedMaterial = mat;

            role.transform.parent = PoolRoot;
            role.transform.localPosition = Vector3.zero;

            _ShapeTemplateMap.Add(skillId, role);
            _ModelPoolMap.Add(skillId, new UnityObjectPool(role, 10));
        }

        UnityObjectPool curPool;
        if (_ModelPoolMap.TryGetValue(skillId, out curPool))
        {
            var role = curPool.Get() as GameObject;
            if (role == null) return null;

            role.transform.SetParent(DivideRoot);
            role.transform.position = model.transform.position;
            role.transform.rotation = model.transform.rotation;

            Color shaderColor = new Color(r, g, b, 1);
            var mrs = role.GetComponentsInChildren<Renderer>();
            foreach (var v in mrs)
            {
                var mat = v.sharedMaterial;
                mat.SetColor(ShaderIDs.GhostEffectColor, shaderColor);
                mat.SetFloat(ShaderIDs.GhostEffectPower, pow);
            }

            var modelComp = model.GetComponent<EntityComponent.AnimationUnit>();
            var roleComp = role.GetComponent<EntityComponent.AnimationUnit>();
            if (null != modelComp && roleComp != null)
            {
                var curState = modelComp.GetCurAnimationStateAtLayer(0);
                if (curState != null)
                    roleComp.Pause(curState.name, curState.time);
                else
                    Common.HobaDebuger.LogWarningFormat("Failed to get Current AnimationState At Layer 0 ");
            }

            return role;
        }

        return null;
    }

    public static void ReleaseGhostModel(int skillId, GameObject model)
    {
        UnityObjectPool curPool;
        if (model != null && _ModelPoolMap.TryGetValue(skillId, out curPool))
        {
            model.transform.parent = PoolRoot;
            model.transform.localPosition = Vector3.zero;
            curPool.Release(model);
        }
    }
}
