using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[System.Serializable]
public class OutwardSfxInfo
{
    public string HangPointPath;
    public string SfxPath;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;
}
public class OutwardSfx : MonoBehaviour
{
    public OutwardSfxInfo[] OutwardSfxInfos;
}

