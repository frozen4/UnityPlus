using System.Collections.Generic;
using UnityEngine;

public class ScenesColliderManager : ISceneManagerLogic
{
    /// <summary>
    /// 功能定位 
    /// 场景Collider管理：根据玩家位置，刷新Collider信息
    /// 起因-大场景中Collider数量太多，uwa测试报告中最高4000，但是同时有效的数量有限
    /// </summary>
    private int _CurrentColliderIndex = -1;
    #region CapInfo
    // 格子长度
    private const int COLLIDER_CAP_LENGTH = 50;
    //在X ，Y轴半轴上的格子数
    private const int COLLIDER_CAP_X_COUNT = 5;
    private const int COLLIDER_CAP_Z_COUNT = 5;
    #endregion
    //超过MAX_COLLIDER_COUNT的时候进行新建操作，并缓存
    private readonly List<BoxCollider> _CacheBoxColliderList = new List<BoxCollider>();
    private readonly List<CapsuleCollider> _CacheCapsuleColliderList = new List<CapsuleCollider>();

    private int _CacheBoxColiderIndex = 0;
    private int _CacheCapsuleColliderIndex = 0;

    private readonly int[] _IndexArray = new int[9];
    private int _BoxIndex = 0;
    private int _CapsuleIndex = 0;
    private readonly Dictionary<int, List<SceneConfig.ColliderInfo>> _SplitInfoList = new Dictionary<int, List<SceneConfig.ColliderInfo>>();
    private BoxCollider[] _PrelaodedBoxColliders = null;
    private CapsuleCollider[] _PrelaodedCapsuleColliders = null;
    private Transform _ColiderRootTran = null;
    private bool _IsValid = false;

    public void Init(SceneConfig cfg)
    {
        _IsValid = false;
        if (null == cfg) return;
        _PrelaodedBoxColliders = cfg._BoxColliders;
        _PrelaodedCapsuleColliders = cfg._CapsuleColliders;
        _ColiderRootTran = cfg.transform.Find("ColidersObjects");
        _IsValid = (null != cfg._BoxColliders)
            && (null != cfg._CapsuleColliders)
            && (null != cfg._ColliderInfos)
            && (0 != cfg._BoxColliders.Length)
            && (0 != cfg._CapsuleColliders.Length);

        if (_IsValid)
        {
            SplitColliders(cfg._ColliderInfos);
        }
    }
    private void SplitColliders(SceneConfig.ColliderInfo[] _ColliderInfos)
    {
        var totalListCount = (COLLIDER_CAP_X_COUNT * 2 + 1) * (COLLIDER_CAP_Z_COUNT * 2 + 1);
        for (int i = 0; i < totalListCount; i++)
        {
            var list = new List<SceneConfig.ColliderInfo>();
            _SplitInfoList.Add(i, list);
        }
        for (int i = 0; i < _ColliderInfos.Length; i++)
        {
            var item = _ColliderInfos[i];
            var index = CalcCurrentIndex(item);
            _SplitInfoList[index].Add(item);
        }
    }
    private int CalcCurrentIndex(SceneConfig.ColliderInfo item)
    {
        var x = Mathf.CeilToInt(item.Pos.x / COLLIDER_CAP_LENGTH);
        x = Mathf.Clamp(x, -COLLIDER_CAP_X_COUNT, COLLIDER_CAP_X_COUNT);
        var z = Mathf.CeilToInt(item.Pos.z / COLLIDER_CAP_LENGTH);
        z = Mathf.Clamp(z, -COLLIDER_CAP_Z_COUNT, COLLIDER_CAP_Z_COUNT);
        var index = (x + COLLIDER_CAP_X_COUNT) * (COLLIDER_CAP_Z_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT);
        return index;
    }
    public void Preload(float posx, float posz)
    {
        if (!_IsValid) return;
        ProcessColliderInfo(posx, posz);
    }
    private CapsuleCollider GetCapsuleCollider()
    {
        CapsuleCollider colliderObj = null;
        if (_CapsuleIndex >= _PrelaodedCapsuleColliders.Length || null == _PrelaodedCapsuleColliders[_CapsuleIndex])
        {
            if (_CacheCapsuleColliderIndex < _CacheCapsuleColliderList.Count)
            {
                colliderObj = _CacheCapsuleColliderList[_CacheCapsuleColliderIndex];
                _CacheCapsuleColliderIndex++;
            }
        }
        else
        {
            colliderObj = _PrelaodedCapsuleColliders[_CapsuleIndex];
            _CapsuleIndex++;
        }
        if (null == colliderObj)
        {
            var go = new GameObject();
            colliderObj = go.AddComponent<CapsuleCollider>();
            _CacheCapsuleColliderList.Add(colliderObj);
            _CacheCapsuleColliderIndex++;
        }
        return colliderObj;
    }
    private BoxCollider GetBoxCollider()
    {
        BoxCollider colliderObj = null;
        if (_BoxIndex >= _PrelaodedBoxColliders.Length || null == _PrelaodedBoxColliders[_BoxIndex])
        {
            if (_CacheBoxColiderIndex < _CacheBoxColliderList.Count)
            {
                colliderObj = _CacheBoxColliderList[_CacheBoxColiderIndex];
                _CacheBoxColiderIndex++;
            }
        }
        else
        {
            colliderObj = _PrelaodedBoxColliders[_BoxIndex];
            _BoxIndex++;
        }
        if (null == colliderObj)
        {
            var go = new GameObject();
            colliderObj = go.AddComponent<BoxCollider>();
            _CacheBoxColliderList.Add(colliderObj);
            _CacheBoxColiderIndex++;
        }
        return colliderObj;
    }
    private void LoadBoxCollider(SceneConfig.ColliderInfo colliderInfo, int index, int innerIndex, string colliderName)
    {
        BoxCollider colliderObj = GetBoxCollider();
        colliderObj.transform.parent = _ColiderRootTran;
        colliderObj.transform.localPosition = colliderInfo.Pos;
        colliderObj.transform.localEulerAngles = colliderInfo.Rot;
        colliderObj.transform.localScale = colliderInfo.Scale;
        colliderObj.center = colliderInfo.Center;
        colliderObj.size = colliderInfo.Size;
        colliderObj.gameObject.layer = colliderInfo.Layer;
#if UNITY_EDITOR
        colliderObj.name = colliderName;
#endif

    }
    private void LoadCapsuleCollider(SceneConfig.ColliderInfo colliderInfo, int index, int innerIndex, string colliderName)
    {
        var colliderObj = GetCapsuleCollider();
        colliderObj.transform.parent = _ColiderRootTran;
        colliderObj.transform.localPosition = colliderInfo.Pos;
        colliderObj.transform.localEulerAngles = colliderInfo.Rot;
        colliderObj.transform.localScale = colliderInfo.Scale;
        colliderObj.center = colliderInfo.Center;
        colliderObj.radius = colliderInfo.Radius;
        colliderObj.height = colliderInfo.Height;
        colliderObj.gameObject.layer = colliderInfo.Layer;
#if UNITY_EDITOR
        colliderObj.name = colliderName;
#endif
    }
    private int[] CalcIndexArray(float posx, float posz)
    {
        _BoxIndex = 0;
        _CapsuleIndex = 0;
        _CacheBoxColiderIndex = 0;
        _CacheCapsuleColliderIndex = 0;

        var x = Mathf.CeilToInt(posx / COLLIDER_CAP_LENGTH);
        x = Mathf.Clamp(x, -COLLIDER_CAP_X_COUNT, COLLIDER_CAP_X_COUNT);
        var z = Mathf.CeilToInt(posz / COLLIDER_CAP_LENGTH);
        z = Mathf.Clamp(z, -COLLIDER_CAP_Z_COUNT, COLLIDER_CAP_Z_COUNT);

        #region  Calc func
        int tmpIndex = (x + COLLIDER_CAP_X_COUNT) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT);
        _IndexArray[0] = tmpIndex;

        var tmpIndex1 = (x + COLLIDER_CAP_X_COUNT + 1) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT);
        _IndexArray[1] = tmpIndex1;
        //左
        var tmpIndex2 = (x + COLLIDER_CAP_X_COUNT - 1) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT);
        _IndexArray[2] = tmpIndex2;
        //上
        var tmpIndex3 = (x + COLLIDER_CAP_X_COUNT) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT + 1);
        _IndexArray[3] = tmpIndex3;
        //下
        var tmpIndex4 = (x + COLLIDER_CAP_X_COUNT) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT - 1);
        _IndexArray[4] = tmpIndex4;
        //左上
        var tmpIndex5 = (x + COLLIDER_CAP_X_COUNT - 1) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT + 1);
        _IndexArray[5] = tmpIndex5;
        //右上
        var tmpIndex6 = (x + COLLIDER_CAP_X_COUNT + 1) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT + 1);
        _IndexArray[6] = tmpIndex6;
        //左下
        var tmpIndex7 = (x + COLLIDER_CAP_X_COUNT - 1) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT - 1);
        _IndexArray[7] = tmpIndex7;
        //右下
        var tmpIndex8 = (x + COLLIDER_CAP_X_COUNT + 1) * (COLLIDER_CAP_X_COUNT * 2 + 1) + (z + COLLIDER_CAP_Z_COUNT - 1);
        _IndexArray[8] = tmpIndex8;
        #endregion

        return _IndexArray;
    }
  
    private void ProcessColliderInfo(float posx, float posz)
    {
        var indexArray = CalcIndexArray(posx, posz);

        if (_CurrentColliderIndex == indexArray[0]) return;
        _CurrentColliderIndex = indexArray[0];

        for (int i = 0; i < indexArray.Length; i++)
        {
            var index = indexArray[i];

            if (index > _SplitInfoList.Count || index < 0) continue;
            List<SceneConfig.ColliderInfo> colliderInfoList;
            if (_SplitInfoList.TryGetValue(index, out colliderInfoList))
            {
                for (int innerIndex = 0; innerIndex < colliderInfoList.Count; innerIndex++)
                {
                    var colliderInfo = colliderInfoList[innerIndex];

                    if (1 == colliderInfo.ColiderType)
                    {
                        var colliderName = HobaText.Format("Box_Force_{0}_{1}", index, innerIndex);
                        LoadBoxCollider(colliderInfo, index, innerIndex, colliderName);
                    }
                    else
                    {
                        var colliderName = HobaText.Format("Cap_Force_{0}_{1}", index, innerIndex);
                        LoadCapsuleCollider(colliderInfo, index, innerIndex, colliderName);
                    }
                }
            }
        }
    }

    public void Update(Vector3 position)
    {
        if (!_IsValid) return;
        ProcessColliderInfo(position.x, position.z);
    }

    public void OnDebugCmd(bool value )
    {
        _IsValid = value;
    }
    public void Clear()
    {
        for (int i = 0; i < 9; i++)
            _IndexArray[i] = -1;
        _CurrentColliderIndex = -1;
        _SplitInfoList.Clear();
        _PrelaodedBoxColliders = null;
        _PrelaodedCapsuleColliders = null;
        _ColiderRootTran = null;
        _CacheBoxColiderIndex = 0;
        for (int i = 0; i < _CacheBoxColliderList.Count; i++)
        {
            if (null != _CacheBoxColliderList[i])
                Object.Destroy(_CacheBoxColliderList[i].gameObject);
        }
        _CacheBoxColliderList.Clear();

        _CacheCapsuleColliderIndex = 0;
        for (int i = 0; i < _CacheCapsuleColliderList.Count; i++)
        {
            if (null != _CacheCapsuleColliderList[i])
                Object.Destroy(_CacheCapsuleColliderList[i].gameObject);
        }
        _CacheBoxColliderList.Clear();
    }
}
