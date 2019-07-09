using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GIACanvFixer : MonoBehaviour
{
    //InteractableUIHolder.PanelInfo panelInfo;
    public int SLayerID = -1;
    public int SOrder = -1;
    //public bool NeedBlock = false;
    //public bool IsValid = false;

    //[System.NonSerialized]
    //[HideInInspector]
    //public bool HasLayer = false;


    // Use this for initialization
    void OnEnable()
    {
        //if (IsValid)
        {
            Canvas canv = gameObject.GetComponent<Canvas>();
            if (canv)
            {
                canv.overrideSorting = true;
                if (SOrder > -1)
                {
                    canv.sortingOrder = SOrder;
                    SOrder = -1;
                }
                //if (HasLayer)
                {
                    canv.sortingLayerID = SLayerID;
                    //HasLayer = false;
                    SLayerID = -1;
                }
            }
            //IsValid = false;

            //GNewUITools.SetupLayerOrder(canv, SOrder, SLayerID, NeedBlock);
            //IsValid = false;
			
			enabled = false;
        }
    }


}
