using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
[RequireComponent(typeof(ContentSizeFitter))]
[RequireComponent(typeof(Text))]
public class GContentSizeFitterSizeRestriction : GBase
{
    [SerializeField]
    private Vector2 _RestrictionSize = Vector2.one;
    private ContentSizeFitter _ContentSizeFitter;
    private ContentSizeFitter Fitter
    {
        get
        {
            if (_ContentSizeFitter == null)
            {
                _ContentSizeFitter = GetComponent<ContentSizeFitter>();
            }
            return _ContentSizeFitter;
        }
    }
    private Text _FitterText;
    private Text FitterText
    {
        get
        {
            if(_FitterText==null)
            {
                _FitterText = GetComponent<Text>();
            }
            return _FitterText;
        }
    }

    static Vector2 _TempSize;
    protected override void OnRectTransformDimensionsChange()
    {
        if (Fitter == null) return;
        if (_RestrictionSize.x > 0)
        {
            float preferredWidth = FitterText.preferredWidth;
            if (preferredWidth >= _RestrictionSize.x)
            {
                Fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                _TempSize.Set(_RestrictionSize.x, RectTrans.sizeDelta.y);
                RectTrans.sizeDelta = _TempSize;
            }
            else
            {
                Fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        if (_RestrictionSize.y > 0)
        {
            float preferredHeight = FitterText.preferredHeight;
            if (preferredHeight > _RestrictionSize.y)
            {
                Fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                _TempSize.Set(RectTrans.sizeDelta.x, _RestrictionSize.y);
                RectTrans.sizeDelta = _TempSize;
            }
            else
            {
                Fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
    }
}
