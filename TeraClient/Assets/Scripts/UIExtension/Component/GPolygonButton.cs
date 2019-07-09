using UnityEngine;
using UnityEngine.UI;

public class GPolygonButton : GButton
{
    private Image _Image;

    [Range(0.0f, 0.5f)]
    public float Alpha;

    protected override void Awake()
    {
        _Image = transform.GetComponent<Image>();
        _Image.alphaHitTestMinimumThreshold = Mathf.Max(0f,Alpha);
    }
}
