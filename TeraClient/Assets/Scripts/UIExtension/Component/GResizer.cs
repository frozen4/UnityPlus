using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GResizer : ContentSizeFitter
{
    private RectTransform _Rect
    {
        get
        {
            return transform as RectTransform;
        }
    }

    [SerializeField]
    private RectTransform _TargetRect;
    [SerializeField]
    private Vector2 _TextSize;
	[SerializeField]
	private Vector2 _TargetSize;
    private Vector2 _Offset;

    protected override void Awake()
    {
        _Offset = _TargetSize - _TextSize;
    }
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
#if	!ART_USE
        Vector2 tsize = _Rect.sizeDelta + _Offset;
        float xx = Mathf.Max(tsize.x, _TextSize.x);
        float yy = Mathf.Max(tsize.y, _TextSize.y);
        tsize.Set(xx, yy);
        _TargetRect.sizeDelta = tsize;
#endif
    }
}
