using UnityEngine;
using UnityEngine.EventSystems;

public class GBase : UIBehaviour, IButtonClickCallBack, IData
{
#if !ART_USE
    [NoToLua]
#endif
    public object Data;
    [HideInInspector]
    private Transform _Transform;
    private RectTransform _RectTransform;

#if !ART_USE
    [NoToLua]
#endif
    public Transform Trans
    {
        get
        {
            if (_Transform == null)
                _Transform = transform;
            return _Transform;
        }
    }

#if !ART_USE
    [NoToLua]
#endif
    public RectTransform RectTrans
    {
        get
        {
            if (_RectTransform == null)
                _RectTransform = Trans as RectTransform;
            if (_RectTransform == null)
                _RectTransform = gameObject.AddComponent<RectTransform>();
            return _RectTransform;
        }
    }

    protected override void Awake()
    {

    }
    protected override void Start()
    {
        // to be override
    }
    protected override void OnEnable()
    {
        // to be override 
    }
    protected override void OnDisable()
    {
        //to be override
    }
    protected override void OnDestroy()
    {
        //to be override
    }
#if !ART_USE
    [NoToLua]
#endif
    public virtual void SetData(object data)
    {
        Data = data;
    }
#if !ART_USE
    [NoToLua]
#endif
    public virtual void OnButtonClickHandler(GameObject obj)
    {

    }
#if !ART_USE
    [NoToLua]
#endif
    public virtual void OnButtonSelectHandler(GameObject value)
    {

    }
}
