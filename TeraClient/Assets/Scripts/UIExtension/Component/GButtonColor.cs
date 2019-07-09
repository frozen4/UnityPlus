using UnityEngine;
using UnityEngine.UI;

//transition == Transition.ColorTint && _ColorForAllChildren == true 才会起作用
public class GButtonColor : GButton
{
    private Graphic[] _ChildGraphics;

    [SerializeField]
    private bool _UseAlpha = false;

    [SerializeField]
    private bool _ColorForAllChildren = true;

    public override void ButtonDownEffect()
    {
        base.ButtonDownEffect();
        StartChildrenColorTween(colors.pressedColor, false);
    }
    public override void ButtonUpEffect()
    {
        base.ButtonUpEffect();
        StartChildrenColorTween(colors.normalColor, false);
    }
    public override void ButtonSelectEffect()
    {
        base.ButtonSelectEffect();
        StartChildrenColorTween(colors.highlightedColor, false);
    }
    public override void ButtonDeselectEffect()
    {
        base.ButtonDeselectEffect();
        StartChildrenColorTween(colors.normalColor, false);
    }
    private void StartChildrenColorTween(Color target, bool instant)
    {

        if (transition == Transition.ColorTint && _ColorForAllChildren )
        {
            Graphic target_graphic = targetGraphic;
            if (target_graphic != null)
            {
                if (_ChildGraphics == null)
                {
                    _ChildGraphics = targetGraphic.GetComponentsInChildren<Graphic>(true);
                }

                if (_ChildGraphics == null || _ChildGraphics.Length < 1) return;
                for (var i = 0; i < _ChildGraphics.Length; i++)
                {
                    Graphic graphic = _ChildGraphics[i];
                    if (graphic != null && graphic.isActiveAndEnabled)
                    {
                        if (graphic != target_graphic && (!(graphic is GRayCastRect)))
                        {
                            graphic.CrossFadeColor(target, instant ? 0f : colors.fadeDuration, true, _UseAlpha);
                        }
                    }
                }
            }
        }
    }
}
