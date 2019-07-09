using UnityEngine;
using System.Collections.Generic;

public class Delay : MonoBehaviour
{
    public float delayTime = 1.0f;

    public static bool IsAutoRun = false;

    private List<GameObject> _Children = null;
    void OnEnable()
    {
        if(!IsAutoRun) return;

        if (_Children == null)
        {
            _Children = new List<GameObject>();
            for (int i = 0; i != transform.childCount; ++i)
            {
                if (null != transform.GetChild(i))
                    _Children.Add(transform.GetChild(i).gameObject);
            }
        }

        foreach (var child in _Children)
        {
            if(child != null && child.activeSelf)
                child.SetActive(false);
        }

        Invoke("DoActivateGo", delayTime);
    }

    private void DoActivateGo()
    {
        foreach (var child in _Children)
        {
            if (child != null && !child.activeSelf)
                child.SetActive(true);
        }
    }

    void OnDisable()
    {
        if (!IsAutoRun) return;

        if (IsInvoking("DoActivateGo"))
            CancelInvoke("DoActivateGo");
    }
}