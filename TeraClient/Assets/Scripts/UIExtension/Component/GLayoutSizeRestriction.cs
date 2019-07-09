using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(GLayout))]
public class GLayoutSizeRestriction : GBase
{
    [SerializeField]
    private bool _UseDefaultSize = true;
    [SerializeField]
    private Vector2 _Value = Vector2.zero;
    private Vector2 _FirstGetter = Vector2.zero;
    public Vector2 GetRestrictionSize()
    {
        if (_UseDefaultSize)
        {
            if (_FirstGetter == Vector2.zero)
                _FirstGetter = RectTrans.sizeDelta;
            return _FirstGetter;
        }
        else
        {
            return _Value;
        }
    }
}