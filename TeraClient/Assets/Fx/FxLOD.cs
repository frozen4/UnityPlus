using Common;
using UnityEngine;
using Xft;

public enum EFxLODLevel
{
    L0 = 0,
    L1,
    L3,

    Count,
    Invaild = 1000,
    All = 10000,
}

public class FxLOD : MonoBehaviour
{
    public GameObject[] allKeyGameObjects;

    //----------------------------------------
    // for inspector editor only
    public int[] generation;
    public EFxLODLevel[] priorities = null;
    public bool[] priorities_only = null;
    public bool[] collapse_status = null;
    public EFxLODLevel lodLevel;
    //-----------------------------------------

#if false
    [HideInInspector]
    private ParticleSystem[] _ParticleSystems;
    private XffectComponent[] _XffectSystems;

    [System.NonSerialized]
    private GameObject _KeyGameObjectL1 = null;
    [System.NonSerialized]
    private GameObject _KeyGameObjectL2 = null;
    [System.NonSerialized]
    private GameObject _KeyGameObjectL3 = null;

    [System.NonSerialized]
    private Animator[] _AnimatorArrayL1 = null;
    [System.NonSerialized]
    private Animator[] _AnimatorArrayL2 = null;
    [System.NonSerialized]
    private Animator[] _AnimatorArrayL3 = null;

    void Awake()
    {
        if (_ParticleSystems == null)
            _ParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>(true);

        if (_XffectSystems == null)
            _XffectSystems = gameObject.GetComponentsInChildren<XffectComponent>(true);

        FindKeyGameObject();
        TurnEnable(false);
    }

    void OnDestroy()
    {
        _ParticleSystems = null;
        _XffectSystems = null;

        _KeyGameObjectL1 = null;
        _KeyGameObjectL2 = null;
        _KeyGameObjectL3 = null;
        allKeyGameObjects = null;

        _AnimatorArrayL1 = null;
        _AnimatorArrayL2 = null;
        _AnimatorArrayL3 = null;
    }

    private void FindKeyGameObject()
    {
        if (_KeyGameObjectL1 != null || _KeyGameObjectL2 != null || _KeyGameObjectL3 != null) return;

        for (var i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child != null)
            {
                if (child.name.Contains("_L0"))
                {
                    _KeyGameObjectL1 = child.gameObject;
                    _AnimatorArrayL1 = _KeyGameObjectL1.GetComponentsInChildren<Animator>(true);
                }
                else if (child.name.Contains("_L1"))
                {
                    _KeyGameObjectL2 = child.gameObject;
                    _AnimatorArrayL2 = _KeyGameObjectL2.GetComponentsInChildren<Animator>(true);
                }
                else if (child.name.Contains("_L3"))
                {
                    _KeyGameObjectL3 = child.gameObject;
                    _AnimatorArrayL3 = _KeyGameObjectL3.GetComponentsInChildren<Animator>(true);
                }
            }
            else
            {
                HobaDebuger.LogWarningFormat("gfx {0}'s allKeyGameObjects has null child");
            }     
        }
    }

    public void TurnEnable(bool enable, int threshold = (int)EFxLODLevel.L3)
    {
        FindKeyGameObject();

        EFxLODLevel lodlevel = threshold >= (int)EFxLODLevel.L0 && threshold <= (int)EFxLODLevel.L3 ? (EFxLODLevel)threshold : EFxLODLevel.L0;
        // base on particle system`s "play on awake" property
        {
            if (_ParticleSystems != null)
            {
                for (int i = 0; i < _ParticleSystems.Length; i++)
                {
                    _ParticleSystems[i].Stop();
                    _ParticleSystems[i].Clear();
                }
            }

            if (_XffectSystems != null)
            {
                for (var i = 0; i < _XffectSystems.Length; i++)
                {
                    _XffectSystems[i].Reset();
                }
            }
        }

        if (enable)
        {
            do
            {
                if (_KeyGameObjectL1 != null && _KeyGameObjectL1.activeSelf)
                {
                    if (_AnimatorArrayL1 != null)   //处理AnimatorList
                    {
                        for (int i = 0; i < _AnimatorArrayL1.Length; ++i)
                            _AnimatorArrayL1[i].enabled = false;
                    }
                    _KeyGameObjectL1.SetActive(false);
                }

                if (_KeyGameObjectL2 != null && _KeyGameObjectL2.activeSelf)
                {

                    if (_AnimatorArrayL2 != null)   //处理AnimatorList
                    {
                        for (int i = 0; i < _AnimatorArrayL2.Length; ++i)
                            _AnimatorArrayL2[i].enabled = false;
                    }
                    _KeyGameObjectL2.SetActive(false);
                }

                if (_KeyGameObjectL3 != null && _KeyGameObjectL3.activeSelf)
                {
                    if (_AnimatorArrayL3 != null)   //处理AnimatorList
                    {
                        for (int i = 0; i < _AnimatorArrayL3.Length; ++i)
                            _AnimatorArrayL3[i].enabled = false;
                    }
                    _KeyGameObjectL3.SetActive(false);
                }

                if (lodlevel == EFxLODLevel.L3)
                {
                    if (_KeyGameObjectL3 != null)
                    {
                        _KeyGameObjectL3.SetActive(true);
                        if (_AnimatorArrayL3 != null)   //处理AnimatorList
                        {
                            for (int i = 0; i < _AnimatorArrayL3.Length; ++i)
                                _AnimatorArrayL3[i].enabled = true;
                        }

                        break;
                    }
                    else
                    {
                        lodlevel = EFxLODLevel.L1;
                    }
                }
                if (lodlevel == EFxLODLevel.L1)
                {
                    if (_KeyGameObjectL2 != null)
                    {
                        _KeyGameObjectL2.SetActive(true);
                        if (_AnimatorArrayL2 != null)   //处理AnimatorList
                        {
                            for (int i = 0; i < _AnimatorArrayL2.Length; ++i)
                                _AnimatorArrayL2[i].enabled = true;
                        }

                        break;
                    }
                    else
                    {
                        lodlevel = EFxLODLevel.L0;
                    }
                }
                if (lodlevel == EFxLODLevel.L0 && _KeyGameObjectL1 != null)
                {
                    _KeyGameObjectL1.SetActive(true);
                    if (_AnimatorArrayL1 != null)   //处理AnimatorList
                    {
                        for (int i = 0; i < _AnimatorArrayL1.Length; ++i)
                            _AnimatorArrayL1[i].enabled = true;
                    }

                    break;
                }
            } while (false);

            if (_ParticleSystems != null)
            {
                for (int i = 0; i < _ParticleSystems.Length; ++i)
                {
                    ParticleSystem ps = _ParticleSystems[i];
                    if (ps.gameObject.activeInHierarchy)
                        ps.Play();
                }
            }
        }
        else
        {
            if (_KeyGameObjectL1 != null && _KeyGameObjectL1.activeSelf)
            {
                if (_AnimatorArrayL1 != null)   //处理AnimatorList
                {
                    for (int i = 0; i < _AnimatorArrayL1.Length; ++i)
                        _AnimatorArrayL1[i].enabled = false;
                }
                _KeyGameObjectL1.SetActive(false);
            }

            if (_KeyGameObjectL2 != null && _KeyGameObjectL2.activeSelf)
            {
                if (_AnimatorArrayL2 != null)   //处理AnimatorList
                {
                    for (int i = 0; i < _AnimatorArrayL2.Length; ++i)
                        _AnimatorArrayL2[i].enabled = false;
                }
                _KeyGameObjectL2.SetActive(false);
            }

            if (_KeyGameObjectL3 != null && _KeyGameObjectL3.activeSelf)
            {
                if (_AnimatorArrayL3 != null)   //处理AnimatorList
                {
                    for (int i = 0; i < _AnimatorArrayL3.Length; ++i)
                        _AnimatorArrayL3[i].enabled = false;
                }
                _KeyGameObjectL3.SetActive(false);
            }
        }
    }

    public GameObject GetCfxObjBaseByLevel(int lod_level)
    {
        FindKeyGameObject();

        EFxLODLevel lodlevel = lod_level >= (int)EFxLODLevel.L0 && lod_level <= (int)EFxLODLevel.L3 ? (EFxLODLevel)lod_level : EFxLODLevel.L0;
        if (lodlevel == EFxLODLevel.L3)
        {
            if (_KeyGameObjectL3 != null)
                return _KeyGameObjectL3;
            else
                return _KeyGameObjectL1;
        }
        else if (lodlevel == EFxLODLevel.L1)
        {
            if (_KeyGameObjectL2 != null)
                return _KeyGameObjectL2;
            else
                return _KeyGameObjectL1;
        }
        else if (lodlevel == EFxLODLevel.L0 && _KeyGameObjectL1 != null)
            return _KeyGameObjectL1;
        else
            return null;
    }
#endif
}