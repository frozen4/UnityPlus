using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct BonesInfo
{
    public string Bones;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
}

[System.Serializable]
public class DynamicBoneInfo
{
    ///与原脚本保持一致，不再修改变量名称
    public string HangPath = "";
    public string rootPath = "";
    public float m_UpdateRate = 60.0f;
    public DynamicBone.UpdateMode m_UpdateMode = DynamicBone.UpdateMode.Normal;
    [Range(0, 1)]
    public float m_Damping = 0.1f;
    public AnimationCurve m_DampingDistrib = null;
    [Range(0, 1)]
    public float m_Elasticity = 0.1f;
    public AnimationCurve m_ElasticityDistrib = null;
    [Range(0, 1)]
    public float m_Stiffness = 0.1f;
    public AnimationCurve m_StiffnessDistrib = null;
    [Range(0, 1)]
    public float m_Inert = 0;
    public AnimationCurve m_InertDistrib = null;
    public float m_Radius = 0;
    public AnimationCurve m_RadiusDistrib = null;
    public float m_EndLength = 0;
    public Vector3 m_EndOffset = Vector3.zero;
    public Vector3 m_Gravity = Vector3.zero;
    public Vector3 m_Force = Vector3.zero;
    public List<DynamicBoneCollider> m_Colliders = null;
    public List<Transform> m_Exclusions = null;
    public DynamicBone.FreezeAxis m_FreezeAxis = DynamicBone.FreezeAxis.None;
    public bool m_DistantDisable = false;
    public Transform m_ReferenceObject = null;
    public float m_DistanceToObject = 20;
}

[System.Serializable]
public class FashionOutward
{
    public string SkinnedMeshName;
    public string RootBonesPath;
    public Mesh Mesh;
    public BonesInfo[] BoneInfos;
    public Material Material;
    public Color RimColor;
    public float RimPower;
    public Color FlakeColorR;
    public Color FlakeColorG;
    public Color FlakeColorB;
    public Color FlakeColorA;
}

public class FashionOutwardInfo : MonoBehaviour
{
    public FashionOutward[] FashionOutwardArray;
    public DynamicBoneInfo[] DynamicBoneInfoArray;
}