using UnityEngine;

public class AutoPlayFx : MonoBehaviour
{
#if !ART_USE 
    private CFxProxy _FxProxy = null;
    void Awake()
    {
        _FxProxy = gameObject.GetComponent<CFxProxy>();
        if(_FxProxy == null)
            _FxProxy = gameObject.AddComponent<CFxProxy>();

        if (_FxProxy != null)
            _FxProxy.Init();
    }

    void OnEnable()
    {
        if (_FxProxy != null)
            _FxProxy.Active(EFxLODLevel.L3);
    }

    void Update()
    {
        if (_FxProxy != null)
            _FxProxy.Tick(Time.deltaTime);
    }

    void OnDisable()
    {
        if (_FxProxy != null)
            _FxProxy.Deactive();
    }
#endif
}
