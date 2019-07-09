using UnityEngine;
using System;
using System.Collections.Generic;

public class GUILinkHolder : MonoBehaviour
{
    [System.Serializable]
    public class LinkItem
    {
        public string Name;
        public RectTransform UIObject;
        [HideInInspector]
        public int ID;
    }

    public List<LinkItem> Links = new List<LinkItem>();

    public Dictionary<int, LinkItem> LinkItemMap = new Dictionary<int, LinkItem>();

    public GameObject GetUIObject(int id)
    {
        if (Links != null)       //build map
        {
            for (var i = 0; i < Links.Count; i++)
            {
                var item = Links[i];

                try
                {
                    LinkItemMap.Add(item.ID, item);
                }
                catch (Exception)
                {
                    //ID重复?
                }
            }
            Links = null;              //clear links
        }

        LinkItem linkItem;
        if (LinkItemMap.TryGetValue(id, out linkItem))
        {
            if (linkItem.UIObject == null)
                return null;
            else
                return linkItem.UIObject.gameObject;
        }

        return null;
    }
}
