using System;
using UnityEngine;

public class UnityUtil
{
    public static string GetNodePath(Transform trans, Transform root=null)
    {
        string s = "";

        if (trans != null)
        {
            s = trans.name;
            trans = trans.parent;
        }

        while (trans != root)
        {
            s = trans.name + "/" + s;
            trans = trans.parent;
        }
        return s;
    }
}