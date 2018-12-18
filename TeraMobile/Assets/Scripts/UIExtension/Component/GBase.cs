using UnityEngine;
using UnityEngine.EventSystems;

public class GBase : UIBehaviour, IButtonClickCallBack,IData
{
    public object _Data;
    [HideInInspector]
    private Transform mTransform;
    public Transform m_Transform
    {
        get {
            if (mTransform == null)
                mTransform = transform;
            return mTransform;
         }
    }

    public RectTransform RectTrans
    {
        get
        {
            if (m_Transform != null)
                return m_Transform as RectTransform;
            return null;
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
    public virtual void SetData(object data)
    {
        _Data = data;
    }
    public virtual void OnButtonClickHandler(GameObject obj)
    {
       
    }

    public virtual void OnButtonSelectHandler(GameObject value)
    {
    
    }
    public object[] CallLuaFunc(string funcName,params object[] args)
    {
        return null;
// #if !ART_USE
//         UIEventListener listener = GetComponent<UIEventListener>();

//         if(listener==null)
//         {
//             listener = GetComponentInParent<UIEventListener>();
//         }

//         if(listener == null)
//         {
//             Debug.Log("there isn't a UIEventListener component in the graphic list.");
//             return null;
//         }

//         return listener.CallFunc(funcName,args);
// #endif
    }
}
