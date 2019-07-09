using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UISfxLayer : MonoBehaviour
{
    public void SetSortingLayer(int _sortingOrder)
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();

        foreach (Renderer render in renders)
        {
            render.sortingOrder = _sortingOrder;
        }
    }
}
