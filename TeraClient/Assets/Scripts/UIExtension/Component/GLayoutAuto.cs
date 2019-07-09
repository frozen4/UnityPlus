public class GLayoutAuto : GLayout
{
    [NoToLua]
    public bool watchEnable = true;
    [NoToLua]
    public bool watchSize = true;
    [NoToLua]
    public bool watchChildren = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (watchEnable)
        {
            LayoutChange(true);
        }
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (watchEnable)
        {
            LayoutChange(true);
        }
    }
    protected override void OnRectTransformDimensionsChange()
    {
        if (watchSize)
        {
            LayoutChange();
        }
    }
    protected virtual void OnTransformChildrenChanged()
    {
        if (watchChildren)
        {
            LayoutChange();
        }
    }
}