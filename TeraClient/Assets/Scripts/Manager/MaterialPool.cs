using System.Collections.Generic;
using UnityEngine;
using Common;

public class MaterialPool : Singleton<MaterialPool>
{
    private const string RUN_TIME_SYMBOL = "[RunTime]";
    private const int MAX_MATERIALS = 40;

    private readonly Queue<Material> _PooledMaterials = new Queue<Material>();
    private Material _EmptyMat = null;

    private int _TotalInstancesCreated = 0;
    private int _TotalInstancesDestroyed = 0;

    public void Init()
    {
        if (_EmptyMat == null)
            _EmptyMat = Resources.Load<Material>("EmptyMat");
    }

    public Material Get(Material originMat)
    {
        if (originMat == null)
            return null;

        Material mat = null;
        if(_PooledMaterials.Count >= 1)
            mat = _PooledMaterials.Dequeue();

        if (mat != null)
        {
            mat.shader = originMat.shader;
            mat.CopyPropertiesFromMaterial(originMat);
        }
        else
        {
            _TotalInstancesCreated++;
            mat = new Material(originMat);
        }

        mat.name = HobaText.Format("{0}{1}", RUN_TIME_SYMBOL, originMat.name);
#if UNITY_EDITOR
        mat.hideFlags = HideFlags.DontSave;
#else
        mat.hideFlags = HideFlags.HideAndDontSave;
#endif

        return mat;
    }

    public void Recycle(Material mat)
    {
        if (mat == null) return;
        if (_PooledMaterials.Count >= MAX_MATERIALS)
        {
            Object.Destroy(mat);
            _TotalInstancesDestroyed++;
        }
        else
        {
            mat.shader = _EmptyMat.shader;
            mat.CopyPropertiesFromMaterial(_EmptyMat);
            _PooledMaterials.Enqueue(mat);
        }
    }

    public void Cleanup()
    {
        foreach (var v in _PooledMaterials)
            Object.Destroy(v);

        _PooledMaterials.Clear();
        _TotalInstancesCreated = 0;
        _TotalInstancesDestroyed = 0;
    }

    public void ShowDiagnostics()
    {
#if UNITY_EDITOR
        Common.HobaDebuger.LogWarningFormat("MaterialPool Diagnostics: TotalLiveInstancesCount = {0}, TotalInstancesCreated = {1}, TotalInstancesDestroyed = {2}",
            _PooledMaterials.Count, _TotalInstancesCreated, _TotalInstancesDestroyed);

#endif
    }
}
