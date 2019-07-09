using UnityEngine;
using System.Collections.Generic;
using System;

public class NpcCameraPosInfo : MonoBehaviour
{
    [Serializable]
    public struct CamInfo
    {
        public string AnimName;     //动画名
        public int PosSet;    //使用第几组挂点
    }

    [SerializeField]
    private List<CamInfo> AnimList = new List<CamInfo>();

    private int FindAnim(string anim_name)
    {
        for (int i = 0; i < AnimList.Count; i++)
        {
            if (anim_name == AnimList[i].AnimName)
            {
                return i;
            }
        }
        return -1;
    }


    public bool IsAnimExisted(string anim_name)
    {
        return FindAnim(anim_name)!=-1;
    }

    public int GetPos(string anim_name)
    {
        int id = FindAnim(anim_name);
        if (id != -1)
        {
            return AnimList[id].PosSet;
        }
        return 0;
    }

    public int GetCount()
    {
        return AnimList.Count;
    }

    public string GetAnimAt(int id)
    {
        return AnimList[id].AnimName;
    }
}
