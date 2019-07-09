using UnityEngine;
using System;
using Common;
using LuaInterface;
using System.Collections.Generic;
using GameLogic;

namespace EntityComponent
{
    public class OutwardComponent : MonoBehaviour, IRecyclable
    {
        /*
        [影响因素]  [实现方式]
        - 换脸      更换Face材质球+重设bones，材质球提前美术制作
        - 换发型    更换Hair材质球+重设bones，材质球提前美术制作
        - 换装备（盔甲） 更换Body材质球+重设bones，材质球提前美术制作
        - 时装      更换Body材质球+重设bones，材质球提前美术制作

        - 换发色    更换Hair材质球颜色，clone一个新材质球
        - 换肤色    更换Face + Body材质球颜色，clone一个新材质球
        - 刺绣      clone一个新材质球，更换属性，替换Body材质球
        */
        public enum OutwardPart
        {
            Body = 0,
            Face,
            Hair,

            Count,

            Hand = Count, // 仅对创建有效
            Leg,          // 仅对创建有效
            MaxCount,
        }

        private static string[] PartNames = new[] { "body", "face", "hair"};
        private static string ExtraBodyNames = "body2";

        public struct OutwardRenderInfo : IEquatable<OutwardRenderInfo>
        {
            public Mesh SharedMesh;
            public Material Mat;
            public Transform[] Bones;

            public bool Equals(OutwardRenderInfo other)
            {
                //throw new NotImplementedException();
                if (!(this.SharedMesh.Equals(other.SharedMesh)))
                    return false;
                if (!(this.Mat.Equals(other.Mat)))
                    return false;
                if (!(this.Bones.Equals(other.Bones)))
                    return false;
                else
                    return true;
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (!(obj is OutwardRenderInfo))
                    return false;
                return Equals((OutwardRenderInfo)obj);

            }
            public override int GetHashCode()
            {
                return SharedMesh.GetHashCode() ^ Bones.GetHashCode() ^ Mat.GetHashCode();
            }
        }

        public struct OutwardColorInfo   // 换色
        {
            public bool IsValid;
            public Color Color;
        }

        public struct EmbroiderySetting  // 刺绣配置信息
        {
            public bool IsValid;
            public int XMin;
            public int YMin;
            public int XMax;
            public int YMax;
        }

        private readonly SkinnedMeshRenderer[] _Renderers = new SkinnedMeshRenderer[(int)OutwardPart.MaxCount];

        // 记录换脸换发换盔甲之前的Render信息，用于回退操作
        private readonly OutwardRenderInfo[] _DefaultOutwards = new OutwardRenderInfo[(int)OutwardPart.Count];

        private OutwardColorInfo _HairColorInfo = new OutwardColorInfo();
        private OutwardColorInfo _SkinColorInfo = new OutwardColorInfo();
        private EmbroiderySetting _EmbroiderySetting = new EmbroiderySetting();

        private readonly LuaFunction[] _OnFinishCallbackRefs = new LuaFunction[(int)OutwardPart.Count];
        private readonly string[] _CurAssetPathPaths = new string[(int)OutwardPart.Count];

        // 异步加载状态，防止换装资源加载中换色无效
        private readonly bool[] _CurAssetLoadingState = new bool[(int)OutwardPart.Count];

        // DynamicBone节点信息
        private readonly List<DynamicBone>[] _DynamicBonesCompList = new List<DynamicBone>[(int)OutwardPart.Count];
        public List<DynamicBone>[] DynamicBonesCompList
        {
            get { return _DynamicBonesCompList; }
        }

        // key 值为 OutwardPart，未避免GC，使用 (int)OutwardPart
        private readonly Dictionary<int, List<Transform>> _ExtraTransDic = new Dictionary<int, List<Transform>>();

        private bool _IsHidRootSfx = false;
        private readonly List<GameObject> _RootSfxList = new List<GameObject>();

        //游戏中所有outwardManager列表
        private static List<OutwardComponent> OutwardManagerList = new List<OutwardComponent>();
        
        public static void RegisterToMan(OutwardComponent man)
        {
            if(!OutwardManagerList.Contains(man))
                OutwardManagerList.Add(man);
        }

        private static void UnregisterFromMan(OutwardComponent man)
        {
            if (OutwardManagerList.Contains(man))
                OutwardManagerList.Remove(man);
        }

        public static List<OutwardComponent> GetAll()
        {
            return OutwardManagerList;
        }

        public OutwardComponent()
        {
            _EmbroiderySetting.IsValid = false;
            _HairColorInfo.IsValid = false;
            _SkinColorInfo.IsValid = false;

            for (int i = 0; i < (int)OutwardPart.Count; ++i)
                _DynamicBonesCompList[i] = new List<DynamicBone>();

            for (var i = 0; i < _CurAssetLoadingState.Length; i++)
                _CurAssetLoadingState[i] = false;

            foreach (OutwardPart suit in Enum.GetValues(typeof(OutwardPart)))
            {
                var index = (int) suit;
                List<Transform> listTran;
                if (!_ExtraTransDic.TryGetValue(index, out listTran))
                {
                    listTran = new List<Transform>();
                    _ExtraTransDic.Add(index, listTran);
                }
                listTran.Clear();
            }
        }

        public void ChangeOutward(OutwardPart part, string assetPath, LuaFunction cbref)
        {
            if (_OnFinishCallbackRefs[(int)part] != null)
                _OnFinishCallbackRefs[(int)part].Release();
            _OnFinishCallbackRefs[(int)part] = cbref;

            if(_CurAssetPathPaths[(int)part] == assetPath)
                return;

            _CurAssetPathPaths[(int)part] = assetPath;
            _CurAssetLoadingState[(int)part] = true;

            if (_DynamicBonesCompList[(int)part] != null)
                _DynamicBonesCompList[(int)part].Clear();

            // assetPathId == 0表示换回初始设置
            if (string.IsNullOrEmpty(assetPath))
            {
                // 按照现行逻辑，此处应该是无用的，所有外观路径都会传，assetPath不可为空
                var smr = GetSkinnedMeshRenderer(part);
                var defaultData = _DefaultOutwards[(int)part];
                if (smr != null && defaultData.SharedMesh != null && defaultData.Mat != null && defaultData.Bones != null)
                {
                    smr.sharedMesh = defaultData.SharedMesh;
                    smr.sharedMaterial = defaultData.Mat;
                    smr.bones = defaultData.Bones;
                }
                _EmbroiderySetting.IsValid = false;
                _CurAssetLoadingState[(int)part] = false;

                CleanupExtraInfo(part);

                var man = transform.GetComponent<EntityEffectComponent>();
                if (null != man)
                    man.OnMaterialChanged(smr);

                OnChangeEnd(part);
            }
            else
            {
                Action<UnityEngine.Object> callback = (asset) =>
                {
                    // 资源加载过程中，数据发生变化
                    if (_CurAssetPathPaths[(int)part] != assetPath) return;
                    _CurAssetLoadingState[(int)part] = false;

                    var prefab = asset as GameObject;
                    if (prefab == null)
                    {
                        HobaDebuger.LogWarningFormat("Outward Asset is null, path = {0}", assetPath);
                        return;
                    }

                    var smr = GetSkinnedMeshRenderer(part);
                    if (null == smr)
                    {
                        HobaDebuger.LogWarningFormat("fashion SkinnedMeshRenderer is null  = {0}", assetPath);
                        return;
                    }

                    // 外观源信息存在以下两种数据中：都包含mesh material bones信息
                    // 1 FashionOutwardInfo 时装，可能存在多余的骨骼
                    // 2 OutwardInfo 基础外观，换脸换发型
                    var isValid = UpdateFashionInfo(smr, part, prefab, assetPath) || UpdateOutwardInfo(smr, part, prefab, assetPath);
                    if (isValid)
                    {
                        var man = transform.GetComponent<EntityEffectComponent>();
                        if (null != man)
                            man.OnMaterialChanged(smr);

                        OnChangeEnd(part);
                    }
                };
                CAssetBundleManager.AsyncLoadResource(assetPath, callback, false);
            }
        }

        public void EnableOutwardPart(OutwardPart part, bool enable)
        {
            if (part == OutwardPart.Face)
            {
                var trans = transform.Find(PartNames[1]);
                if (trans != null && trans.gameObject.activeSelf != enable)
                    trans.gameObject.SetActive(enable);
            }
            else if (part == OutwardPart.Hair)
            {
                var trans = transform.Find(PartNames[2]);
                if (trans != null && trans.gameObject.activeSelf != enable)
                    trans.gameObject.SetActive(enable);
            }
            else
                HobaDebuger.LogWarningFormat("EnableOutwarPart only work on part face or hair");
        }

        public void ChangeHairColor(Color color)
        {
            _HairColorInfo.IsValid = true;
            _HairColorInfo.Color = color;

            if (_CurAssetLoadingState[(int)OutwardPart.Hair]) return;

            var smr = GetSkinnedMeshRenderer(OutwardPart.Hair);
            if (smr != null)
            {
                if (smr.sharedMaterial != null)
                {
                    Material mat = new Material(smr.sharedMaterial);

                    if (mat.HasProperty(ShaderIDs.HairColorCustom))
                        mat.SetColor(ShaderIDs.HairColorCustom, color);

                    smr.sharedMaterial = mat;
                    var man = transform.GetComponent<EntityEffectComponent>();
                    if (null != man)
                        man.OnMaterialChanged(smr);
                }
            }
        }

        public void ChangeSkinColor(Color color)
        {
            _SkinColorInfo.IsValid = true;
            _SkinColorInfo.Color = color;

            var man = transform.GetComponent<EntityEffectComponent>();
            if (!_CurAssetLoadingState[(int)OutwardPart.Body])
            {
                var smr = GetSkinnedMeshRenderer(OutwardPart.Body);
                if (smr != null)
                {
                    if (smr.sharedMaterial != null)
                    {
                        Material mat = new Material(smr.sharedMaterial);

                        if (mat.HasProperty(ShaderIDs.SkinColor))
                            mat.SetColor(ShaderIDs.SkinColor, color);

                        //shader里面的旧属性，防止改漏的，二次判断一下
                        if (mat.HasProperty(ShaderIDs.SkinColor))
                            mat.SetColor(ShaderIDs.SkinColor, color);

                        smr.sharedMaterial = mat;
                        if (null != man)
                            man.OnMaterialChanged(smr);
                    }
                }
            }

            if (!_CurAssetLoadingState[(int)OutwardPart.Face])
            {
                var smr = GetSkinnedMeshRenderer(OutwardPart.Face);
                if (smr != null)
                {
                    if (smr.sharedMaterial != null)
                    {
                        Material mat = new Material(smr.sharedMaterial);

                        if (mat.HasProperty(ShaderIDs.SkinColor))
                            mat.SetColor(ShaderIDs.SkinColor, color);

                        //shader里面的旧属性，防止改漏的，二次判断一下
                        if (mat.HasProperty(ShaderIDs.SkinColor))
                            mat.SetColor(ShaderIDs.SkinColor, color);
                        smr.sharedMaterial = mat;

                        if (null != man)
                            man.OnMaterialChanged(smr);
                    }
                }
            }
            var smrHand = GetSkinnedMeshRenderer(OutwardPart.Hand);
            if (smrHand != null)
            {
                if (smrHand.sharedMaterial != null)
                {
                    Material mat = new Material(smrHand.sharedMaterial);

                    if (mat.HasProperty(ShaderIDs.SkinColor))
                        mat.SetColor(ShaderIDs.SkinColor, color);

                    //shader里面的旧属性，防止改漏的，二次判断一下
                    if (mat.HasProperty(ShaderIDs.SkinColor))
                        mat.SetColor(ShaderIDs.SkinColor, color);
                    smrHand.sharedMaterial = mat;
                }
            }

            var smrLeg = GetSkinnedMeshRenderer(OutwardPart.Leg);
            if (smrLeg != null)
            {
                if (smrLeg.sharedMaterial != null)
                {
                    Material mat = new Material(smrLeg.sharedMaterial);

                    if (mat.HasProperty(ShaderIDs.SkinColor))
                        mat.SetColor(ShaderIDs.SkinColor, color);

                    //shader里面的旧属性，防止改漏的，二次判断一下
                    if (mat.HasProperty(ShaderIDs.SkinColor))
                        mat.SetColor(ShaderIDs.SkinColor, color);
                    smrLeg.sharedMaterial = mat;
                }
            }
        }

        public void ChangeDressColor(string matName, int channel, Color color, bool set = true)
        {
            if (string.IsNullOrEmpty(matName)) return;

            var man = transform.GetComponent<EntityEffectComponent>();
            if (man == null)
            {
                man = gameObject.AddComponent<EntityEffectComponent>();
                man.Init();
            }
            if (man != null)
            {
                var renderIndos = man.GetRendererInfoMap();
                foreach (var kv in renderIndos)
                {
                    if (kv.Key.name == matName)
                    {
                        var mat = kv.Value.CurrentMat == null ? kv.Value.OriginMat : kv.Value.CurrentMat;
                        if (mat == null) break;

                        if (set)
                        {
//                             mat.EnableKeyword("HERO_COLOR_ADD_ON");
//                             mat.DisableKeyword("HERO_COLOR_ADD_OFF");
                        }
                        else
                        {
//                             mat.EnableKeyword("HERO_COLOR_ADD_OFF");
//                             mat.DisableKeyword("HERO_COLOR_ADD_ON");

                            if (mat.HasProperty(ShaderIDs.FlakeColor1))
                                mat.SetColor(ShaderIDs.FlakeColor1, Color.white);
                            if (mat.HasProperty(ShaderIDs.FlakeColor2))
                                mat.SetColor(ShaderIDs.FlakeColor2, Color.white);

                            break;
                        }
                        if (channel == 1)
                        {
                            if (mat.HasProperty(ShaderIDs.FlakeColor1))
                                mat.SetColor(ShaderIDs.FlakeColor1, color);
                        }
                        else if (channel == 2)
                        {
                            if (mat.HasProperty(ShaderIDs.FlakeColor2))
                                mat.SetColor(ShaderIDs.FlakeColor2, color);
                        }
                        break;
                    }
                }
            }
        }

        public void ChangeArmorEmbroidery(string path)
        {
            if (!_EmbroiderySetting.IsValid)
            {
                HobaDebuger.LogWarning("Current Armor does not support ChangeEmbroidery");
                return;
            }

            var bodyRender = GetSkinnedMeshRenderer(OutwardPart.Body);
            if (bodyRender == null)
            {
                HobaDebuger.LogWarningFormat("Current Armor named {0} has no Body Render", gameObject.name);
                return;
            }

            Material mat = new Material(bodyRender.sharedMaterial);

            if (string.IsNullOrEmpty(path))
            {
                ChangeArmorEmbroidery(mat, false, null, null, null, Vector4.zero);
                bodyRender.sharedMaterial = mat;

                var man = transform.GetComponent<EntityEffectComponent>();
                if (null != man)
                    man.OnMaterialChanged(bodyRender);
            }
            else
            {
                Vector4 rect = new Vector4(_EmbroiderySetting.XMin, _EmbroiderySetting.YMin, _EmbroiderySetting.XMax, _EmbroiderySetting.YMax);

                Action<UnityEngine.Object> callback = (asset) =>
                {
                    GameObject go = asset as GameObject;
                    if (go != null)
                    {
                        var imgset = go.GetComponent<EmbroideryImg>();
                        if (imgset != null)
                        {
                            ChangeArmorEmbroidery(mat, true, imgset.DiffuseTex as Texture2D,
                                imgset.NormalTex as Texture2D, imgset.SpecularTex as Texture2D, rect);
                            bodyRender.sharedMaterial = mat;

                            var man = transform.GetComponent<EntityEffectComponent>();
                            if (null != man)
                                man.OnMaterialChanged(bodyRender);
                        }
                    }
                    else
                    {
                        HobaDebuger.LogWarningFormat("Resource named {0} is null", path);
                    }
                };
                CAssetBundleManager.AsyncLoadResource(path, callback, false, "outward");
            }
        }

        private bool UpdateFashionInfo(SkinnedMeshRenderer smr, OutwardPart part, GameObject prefab, string assetPath)
        {
            var fashionOutInfo = prefab.GetComponent<FashionOutwardInfo>();
            if (fashionOutInfo == null) return false;

            CleanupExtraInfo(part);

            string partName = PartNames[(int)part];
            var info1 = GetFashionOutward(partName, fashionOutInfo.FashionOutwardArray);

            if (null == info1)
            {
                HobaDebuger.LogWarningFormat("fashion info is null");
                return true;
            }
            SaveDefaultOutward(part, smr);
            SetSmrPropertyByFashionInfo(part, smr, info1);
            
            // body时装可能用到两套材质球
            if (part == OutwardPart.Body)
            {
                var bodyRoot = smr.gameObject;
                var extraInfo = GetFashionOutward(ExtraBodyNames, fashionOutInfo.FashionOutwardArray);
                if (null != extraInfo)
                {
                    var extrabodyTrans = transform.Find(ExtraBodyNames);
                    if (extrabodyTrans == null)
                    {
                        var extrabody = GameObject.Instantiate(bodyRoot, transform);
                        extrabody.name = ExtraBodyNames;
                        extrabodyTrans = extrabody.transform;
                        _ExtraTransDic[(int)part].Add(extrabodyTrans);
                    }
                    
                    var extraSmr = extrabodyTrans.GetComponent<SkinnedMeshRenderer>();

                    if (null != extraSmr)
                    {
                        extraSmr.rootBone = transform.Find(extraInfo.RootBonesPath);
                        SetSmrPropertyByFashionInfo(part, extraSmr, extraInfo);
                    }
                }
            }

            //加载特效信息
            var outwardSfx = prefab.GetComponent<OutwardSfx>();
            if (null != outwardSfx)
                LoadOutwardGfx(part, outwardSfx, assetPath);

            //UpdateDynamicBoneInfo(part, prefab);
            UpdateDynamicBoneInfo(part, fashionOutInfo.DynamicBoneInfoArray);
            return true;
        }

        private FashionOutward GetFashionOutward(string rootName, FashionOutward[] fashionOutwards)
        {
            for (int i = 0; i < fashionOutwards.Length; i++)
            {
                if (rootName.Equals(fashionOutwards[i].SkinnedMeshName))
                    return fashionOutwards[i];
            }
            return null;
        }

        private void SetSmrPropertyByFashionInfo(OutwardPart part, SkinnedMeshRenderer smr, FashionOutward fashionInfo)
        {
            if (smr == null) return;

            smr.sharedMesh = fashionInfo.Mesh;
            smr.sharedMaterial = fashionInfo.Material;

            if (fashionInfo.BoneInfos == null)
            {
                HobaDebuger.LogWarning("Bones info is null");
                return;
            }

            var boneCount = fashionInfo.BoneInfos.Length;
            Transform[] bones = new Transform[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                var info = fashionInfo.BoneInfos[i];
                Transform newAddedRoot = null;
                bones[i] = FindBone(info.Bones, info.Position, info.Rotation, info.Scale,true, out newAddedRoot);
                if (newAddedRoot != null)
                    _ExtraTransDic[(int)part].Add(newAddedRoot);
            }

            smr.bones = bones;
        }

        private bool UpdateOutwardInfo(SkinnedMeshRenderer smr, OutwardPart part, GameObject prefab, string assetPath)
        {
            var outwardInfo = prefab.GetComponent<OutwardInfo>();
            if (outwardInfo == null) return false;

            CleanupExtraInfo(part);

            if (smr != null)
            {
                SaveDefaultOutward(part, smr);
                SetSmrPropertyByOutwardInfo(smr, outwardInfo);
            }

            //加载特效信息
            var outwardSfx = prefab.GetComponent<OutwardSfx>();
            if (null != outwardSfx)
                LoadOutwardGfx(part, outwardSfx, assetPath);

            // 保存当然的刺绣位置信息
            var embrRect = prefab.GetComponent<EmbroideryRect>();
            if (embrRect != null)
            {
                _EmbroiderySetting.IsValid = true;
                _EmbroiderySetting.XMax = embrRect.XMax;
                _EmbroiderySetting.XMin = embrRect.XMin;
                _EmbroiderySetting.YMax = embrRect.YMax;
                _EmbroiderySetting.YMin = embrRect.YMin;
            }
            else
            {
                _EmbroiderySetting.IsValid = false;
            }
            return true;
        }

        private void SetSmrPropertyByOutwardInfo(SkinnedMeshRenderer smr, OutwardInfo outwardInfo)
        {
            if (smr == null) return;

            smr.sharedMesh = outwardInfo.Mesh;
            smr.sharedMaterial = outwardInfo.Material;

            if (outwardInfo.Bones == null)
            {
                HobaDebuger.LogWarning("Bones info is null");
                return;
            }

            var boneCount = outwardInfo.Bones.Length;
            Transform[] bones = new Transform[boneCount];
            Transform newAddedRoot = null;
            for (int i = 0; i < boneCount; i++)
                bones[i] = FindBone(outwardInfo.Bones[i], Vector3.zero, Vector3.zero, Vector3.one, false, out newAddedRoot);

            smr.bones = bones;
        }

        private Transform FindBone(string bonesPath, Vector3 pos, Vector3 rot, Vector3 scale, bool updateTrans, out Transform newAddedRoot)
        {
            newAddedRoot = null;
            var boneTrans = transform.Find(bonesPath);
            if (boneTrans != null)
            {
                if (updateTrans)
                {
                    boneTrans.transform.localPosition = pos;
                    boneTrans.transform.localRotation = Quaternion.Euler(rot);
                    boneTrans.transform.localScale = scale;
                }
                return boneTrans;
            }

            // 如果基础骨骼中没有该骨骼，则需要加新的骨骼
            var nodeNames = bonesPath.Split('/');
            var curParent = transform;
            foreach (var nodeName in nodeNames)
            {
                var curNode = curParent.Find(nodeName);
                if (curNode == null)
                {
                    var go = new GameObject(nodeName);
                    go.transform.SetParent(curParent);
                    go.transform.localPosition = pos;
                    go.transform.localRotation = Quaternion.Euler(rot);
                    go.transform.localScale = scale;
                    curNode = go.transform;
                    if (newAddedRoot == null)
                        newAddedRoot = go.transform;
                }
                curParent = curNode;
            }

            return curParent;
        }

        private void LoadOutwardGfx(OutwardPart part, OutwardSfx outwardSfx, string assetPath)
        {
            if (null == outwardSfx.OutwardSfxInfos || 0 == outwardSfx.OutwardSfxInfos.Length) return;

            for (int i = 0; i < outwardSfx.OutwardSfxInfos.Length; i++)
            {
                var boneRoot = transform;
                var curSfxInfo = outwardSfx.OutwardSfxInfos[i];
                var isRootGfx = string.IsNullOrEmpty(curSfxInfo.HangPointPath);
                if (!isRootGfx)
                    boneRoot = transform.Find(curSfxInfo.HangPointPath);

                if(boneRoot == null) return;

                Action<UnityEngine.Object> callback = (asset) =>
                {
                    // 资源加载过程中，数据发生变化
                    if (_CurAssetPathPaths[(int)part] != assetPath) return;

                    var gameObj = GameObject.Instantiate(asset) as GameObject;
                    if (null == gameObj) return;

                    var obj = gameObj;
                    obj.transform.SetParent(boneRoot);
                    obj.transform.localPosition = curSfxInfo.Position;
                    obj.transform.localRotation = Quaternion.Euler(curSfxInfo.Rotation);
                    obj.transform.localScale = curSfxInfo.Scale;
                    Util.SetLayerRecursively(obj, boneRoot.gameObject.layer);

                    _ExtraTransDic[(int)part].Add(obj.transform);

                    if (isRootGfx)
                    {
                        // 处于模型底部的特效
                        _RootSfxList.Add(obj);
                        if (_IsHidRootSfx)
                            obj.SetActive(false);
                    }
                };
                CAssetBundleManager.AsyncLoadResource(outwardSfx.OutwardSfxInfos[i].SfxPath, callback, false);
            }
        }

        private void CleanupExtraInfo(OutwardPart part)
        {
            _RootSfxList.Clear();

            List<Transform> transExtra;
            if (_ExtraTransDic.TryGetValue((int)part, out transExtra))
            {
                for (int i = 0; i < transExtra.Count; i++)
                {
                    if (null != transExtra[i])
                        GameObject.Destroy(transExtra[i].gameObject);
                }
                transExtra.Clear();
            }
        }

        public void EnableRootSfx(bool enable)
        {
            _IsHidRootSfx = !enable;
            for (int i = 0; i < _RootSfxList.Count; i++)
            {
                var sfxGo = _RootSfxList[i];
                if (sfxGo.activeSelf != enable)
                    sfxGo.SetActive(enable);
            }
        }
        private void UpdateDynamicBoneInfo(OutwardPart part, DynamicBoneInfo[] prefabBones)
        {
            if (null == prefabBones || 0 == prefabBones.Length) return;

            for (int i = 0; i < prefabBones.Length; i++)
            {
                var dynamicBoneInfo = prefabBones[i];
                string fullPath = dynamicBoneInfo.HangPath;
                if (string.IsNullOrEmpty(fullPath)) continue;
                var attachedBone = transform.Find(fullPath);
                if (null == attachedBone)
                {
                    HobaDebuger.LogErrorFormat("DynamicBone数据中骨骼路径配置错误，{0}", fullPath);
                    continue;
                }
                string dbRootPath = dynamicBoneInfo.rootPath;
                if (string.IsNullOrEmpty(dbRootPath)) continue;
                var dbRootTrans = transform.Find(dbRootPath);
                if (null == dbRootTrans) continue;

                #region Setup Params
                var targetBone = attachedBone.GetComponent<DynamicBone>();
                if (null == targetBone)
                    targetBone = attachedBone.gameObject.AddComponent<DynamicBone>();
                targetBone.m_Root = dbRootTrans;
                targetBone.m_UpdateRate = dynamicBoneInfo.m_UpdateRate;
                targetBone.m_UpdateMode = dynamicBoneInfo.m_UpdateMode;
                targetBone.m_Damping = dynamicBoneInfo.m_Damping;
                targetBone.m_DampingDistrib = dynamicBoneInfo.m_DampingDistrib;
                targetBone.m_Elasticity = dynamicBoneInfo.m_Elasticity;
                targetBone.m_ElasticityDistrib = dynamicBoneInfo.m_ElasticityDistrib;
                targetBone.m_Stiffness = dynamicBoneInfo.m_Stiffness;
                targetBone.m_StiffnessDistrib = dynamicBoneInfo.m_StiffnessDistrib;
                targetBone.m_Inert = dynamicBoneInfo.m_Inert;
                targetBone.m_InertDistrib = dynamicBoneInfo.m_InertDistrib;
                targetBone.m_Radius = dynamicBoneInfo.m_Radius;
                targetBone.m_RadiusDistrib = dynamicBoneInfo.m_RadiusDistrib;
                targetBone.m_EndLength = dynamicBoneInfo.m_EndLength;
                targetBone.m_EndOffset = dynamicBoneInfo.m_EndOffset;
                targetBone.m_Gravity = dynamicBoneInfo.m_Gravity;
                targetBone.m_Force = dynamicBoneInfo.m_Force;
                targetBone.m_Colliders = dynamicBoneInfo.m_Colliders;
                targetBone.m_Exclusions = dynamicBoneInfo.m_Exclusions;
                targetBone.m_FreezeAxis = dynamicBoneInfo.m_FreezeAxis;
                targetBone.m_DistantDisable = dynamicBoneInfo.m_DistantDisable;
                targetBone.m_ReferenceObject = dynamicBoneInfo.m_ReferenceObject;
                //targetBone.m_DistanceToObject = sourceBone.m_DistanceToObject;
                targetBone.m_DistanceToObject = GFXConfig.Instance.GetDynamicBoneDistance();
                targetBone.Setup();
                #endregion

                _DynamicBonesCompList[(int)part].Add(targetBone);
            }
        }
 
        private void ChangeArmorEmbroidery(Material mat, bool set, Texture2D diffuse, Texture2D normal, Texture2D specular, Vector4 rect)
        {
            diffuse = diffuse != null ? diffuse : Texture2D.whiteTexture;
            normal = normal != null ? normal : Texture2D.whiteTexture;
            specular = specular != null ? specular : Texture2D.whiteTexture;

            if (mat.HasProperty(ShaderIDs.AdditionTex) &&
                mat.HasProperty(ShaderIDs.AdditionNormal) &&
                mat.HasProperty(ShaderIDs.AdditionSpecular) &&
                mat.HasProperty(ShaderIDs.AdditionOffset))
            {
                mat.SetTexture(ShaderIDs.AdditionTex, diffuse);
                mat.SetTexture(ShaderIDs.AdditionNormal, normal);
                mat.SetTexture(ShaderIDs.AdditionSpecular, specular);
                mat.SetVector(ShaderIDs.AdditionOffset, rect);
            }
//             if (set)
//             {
//                 mat.EnableKeyword("HERO_ADDITION_ON");
//                 mat.DisableKeyword("HERO_ADDITION_OFF");
//             }
//             else
//             {
//                 mat.EnableKeyword("HERO_ADDITION_OFF");
//                 mat.DisableKeyword("HERO_ADDITION_ON");
//             }
        }

        private SkinnedMeshRenderer GetSkinnedMeshRenderer(OutwardPart part)
        {
            int idx = (int)part;
            if (_Renderers[idx] == null)
            {
                var msrs = transform.GetComponent<SMRInfos>();
                if (msrs != null)
                {
                    if (part == OutwardPart.Body)
                        _Renderers[idx] = msrs.BodyMeshRenderers;
                    else if (part == OutwardPart.Face)
                        _Renderers[idx] = msrs.FaceMeshRenderer;
                    else if (part == OutwardPart.Hair)
                        _Renderers[idx] = msrs.HairMeshRenderer;
                    else if (part == OutwardPart.Hand)
                        _Renderers[idx] = msrs.HandMeshRenderers;
                    else if (part == OutwardPart.Leg)
                        _Renderers[idx] = msrs.LegMeshRenderers;
                }
                else
                {
                    Transform trans = null;
                    if (part == OutwardPart.Body)
                        trans = transform.Find("body");
                    else if (part == OutwardPart.Face)
                        trans = transform.Find("face");
                    else if (part == OutwardPart.Hair)
                        trans = transform.Find("hair");

                    if (trans != null)
                        _Renderers[idx] = trans.GetComponent<SkinnedMeshRenderer>();
                }
            }

            return _Renderers[idx];
        }

        private void SaveDefaultOutward(OutwardPart part, SkinnedMeshRenderer smr)
        {
            if (smr == null) return;

            int idx = (int)part;
            if (_DefaultOutwards[idx].SharedMesh != null) return;

            _DefaultOutwards[idx].SharedMesh = smr.sharedMesh;
            _DefaultOutwards[idx].Mat = smr.sharedMaterial;
            _DefaultOutwards[idx].Bones = smr.bones;
        }

        private void OnChangeEnd(OutwardPart part)
        {
            int idx = (int)part;
            // 回调

            var func = _OnFinishCallbackRefs[idx];
            if (func != null)
            {
                func.Call();
                func.Release();
                _OnFinishCallbackRefs[idx] = null;
            }

            // 设置颜色信息
            if (part == OutwardPart.Hair && _HairColorInfo.IsValid)
            {
                ChangeHairColor(_HairColorInfo.Color);
            }

            if (part == OutwardPart.Body && _SkinColorInfo.IsValid)
            {
                ChangeSkinColor(_SkinColorInfo.Color);
            }
        }

        private void ResetOutward(OutwardPart part)
        {
            if (_OnFinishCallbackRefs[(int)part] != null)
                _OnFinishCallbackRefs[(int)part].Release();
            _OnFinishCallbackRefs[(int)part] = null;

            _CurAssetPathPaths[(int)part] = string.Empty;
            _CurAssetLoadingState[(int)part] = false;

            if (_DynamicBonesCompList[(int)part] != null)
                _DynamicBonesCompList[(int)part].Clear();

            var smr = GetSkinnedMeshRenderer(part);
            var defaultData = _DefaultOutwards[(int)part];
            if (smr != null && defaultData.SharedMesh != null && defaultData.Mat != null && defaultData.Bones != null)
            {
                smr.sharedMesh = defaultData.SharedMesh;
                smr.sharedMaterial = defaultData.Mat;
                smr.bones = defaultData.Bones;
            }
            _EmbroiderySetting.IsValid = false;
            _CurAssetLoadingState[(int)part] = false;

            CleanupExtraInfo(part);
        }

        public void OnRecycle()
        {
            ResetOutward(OutwardPart.Body);
            ResetOutward(OutwardPart.Face);
            ResetOutward(OutwardPart.Hair);

            UnregisterFromMan(this);
        }
    }

}

