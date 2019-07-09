using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITemplate : MonoBehaviour
{
    [System.Serializable]
    public class UIControl
    {
        public int ID;
        public RectTransform Object;
    }

    //public Dictionary<int, UIControl> ItemMap = new Dictionary<int, UIControl>();
    public List<UIControl> ItemList = new List<UIControl>();

    public GameObject GetControl(int id)
    {
        for (var i = 0; i < ItemList.Count; i++)
        {
            if (ItemList[i].ID == id)
            {
                if (ItemList[i].Object != null)
                    return ItemList[i].Object.gameObject;
                else
                    return null;
            }
                
        }

        return null;
    }

    public int GetLength()
    {
        return ItemList.Count;
    }
}
