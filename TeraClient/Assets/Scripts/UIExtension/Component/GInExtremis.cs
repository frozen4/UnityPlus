using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GInExtremis : MonoBehaviour {
    private Image _ExtremisImage;

    private Color _StartColor;

    private Color _EndColor;

    public float Duration;

    // Use this for initialization
    void Start()
    {
        _ExtremisImage = gameObject.GetComponent<Image>();
        _StartColor = _ExtremisImage.color;
        _EndColor = new Color(_ExtremisImage.color.r, _ExtremisImage.color.g, _ExtremisImage.color.b, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        var lerp = Mathf.PingPong(Time.time, Duration) / Duration;
        _ExtremisImage.color = Color.Lerp(_StartColor, _EndColor, lerp);
    }
}
