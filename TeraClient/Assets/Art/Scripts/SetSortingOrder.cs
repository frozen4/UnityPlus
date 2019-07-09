using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SetSortingOrder : MonoBehaviour
{
    [System.NonSerialized]
    public int _SetLayer = 0;
    public int setOrder = 0;
    public bool setChild = true;
    public bool isSharedMaterial = true;

    private Renderer[] _ChildRendererList = null;
    private Renderer _Renderer = null;


    void OnEnable()
    {
        UpdateSortingOrder();
    }

    public void UpdateSortingOrder()
    {
		if (setChild)
        {
            if (_ChildRendererList == null)
            {
                _ChildRendererList = gameObject.GetComponentsInChildren<Renderer>(true);
            }

            for (int i = 0; i < _ChildRendererList.Length; i++)
            {
                SetSortingOrderImp(_ChildRendererList[i]);
            }
        }
        else
        {
            if (_Renderer == null)
                _Renderer = this.GetComponent<Renderer>();

            SetSortingOrderImp(GetComponent<Renderer>());
        }
    }

    void SetSortingOrderImp(Renderer renderer)
    {
        if (renderer != null)
        {
			renderer.sortingOrder = setOrder;

			if (_SetLayer != 0)
            {
				renderer.sortingLayerID = _SetLayer;
            }
        }
    }
}
