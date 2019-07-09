using UnityEngine;
using UnityEngine.UI;

public class GCircleProgress : GBase 
{
    [SerializeField]
    private Image _Ring;
    [SerializeField]
    private Image _Spot;
    [SerializeField]
    private float _OffsetRadius = 6f;

    private float _MaxSecond = 0f;
    private float _Second = 0f;
    private float _OffsetAngle = 0f;
    private float _OffsetRadian = 0f;
    private float _Radius = 0f;

    private Quaternion _OldRotation;
    protected override void Awake()
    {
        base.Awake();
        _OldRotation = this._Spot.rectTransform.localRotation;
        this._OffsetAngle = (this._Ring.fillOrigin) * 90f;
        this._OffsetRadian = (this._Ring.fillOrigin - 1) * 0.25f;
    }
    public void SetTime(float second)
    {
        if (_Second == second) return;
        this._Radius = this._Ring.rectTransform.sizeDelta.x / 2f - _OffsetRadius;
        this._Spot.rectTransform.localRotation = _OldRotation;
        this._Spot.rectTransform.Rotate(Vector3.forward, _OffsetAngle);
        this._MaxSecond = _Second = second;
        TimeChange();
    }

    private void TimeChange(float delta = 0f)
    {
        this._Second -= delta;

        float rate = _Second / _MaxSecond;
        rate = Mathf.Max(rate, 0f);
        this._Ring.fillAmount = rate;

        //计算位置
        float radian = Mathf.PI * 2f * (rate + _OffsetRadian);
        float xx = Mathf.Cos(radian) * _Radius;
        float yy = Mathf.Sin(radian) * _Radius;
        this._Spot.rectTransform.anchoredPosition = new Vector2(xx, yy);

        //计算旋转角度
        float offsetAngle = -(delta / _MaxSecond) * 360f;
        this._Spot.rectTransform.Rotate(Vector3.forward, offsetAngle);
    }

    void Update()
    {
        if (_Second > 0)
        {
            TimeChange(Time.deltaTime);
        }
        else
        {
            if (IsActive())
            {
                gameObject.SetActive(false);
            }
        }
    }
}
