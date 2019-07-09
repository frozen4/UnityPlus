using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using LuaInterface;
using Common;

[RequireComponent(typeof(Image))]
public class CooldownComponent : MonoBehaviour
{
    private Image _Sprite = null;

    private float _MaxTime = 0;
    private float _EndTime = 0;
    private LuaFunction _OnFinish = null;
    private Text _CDTimeTxt;
    private int _LastSeceond = 0;
    private bool _IsReverse = false;
    void Start()
    {
        _Sprite = gameObject.GetComponent<Image>();
    }

    void OnEnable()
    {
        _Sprite = gameObject.GetComponent<Image>();
    }
    
    public void SetParam(float max, float end, GameObject timeLabel, LuaFunction cb, bool bIsReverse = false)
    {
        _IsReverse = bIsReverse;
        _MaxTime = max;
        _EndTime = end + Time.time;
        _LastSeceond = 0;
        _CDTimeTxt = null;
        if (timeLabel != null)
            _CDTimeTxt = timeLabel.GetComponent<Text>();
        _OnFinish = cb;
    }

    void Update()
    {
        float cur_time = Time.time;
        float fill_amount = (_EndTime - cur_time) / _MaxTime;

        if(_IsReverse)
            _Sprite.fillAmount = 1-fill_amount;
        else
            _Sprite.fillAmount = fill_amount;

        var lastSec = Mathf.Abs(_EndTime - cur_time);
        if (lastSec <= 0.01f || ( cur_time > _EndTime))
        {
            if(_CDTimeTxt != null && _CDTimeTxt.gameObject.activeSelf)
            {
                _CDTimeTxt.text = "";
                _CDTimeTxt.gameObject.SetActive(false);
            }

            if (_OnFinish != null)
            {
                _OnFinish.Call();
                _OnFinish.Release();
                _OnFinish = null;
            }
            gameObject.SetActive(false);
        }
        else
        {
            if(_CDTimeTxt != null)
            {
                if (!_CDTimeTxt.gameObject.activeSelf)
                    _CDTimeTxt.gameObject.SetActive(true);
                
                if (_LastSeceond != Mathf.CeilToInt(lastSec))
                {
                    _LastSeceond = Mathf.CeilToInt(lastSec);

                    var sb = HobaText.GetStringBuilder();
                    sb.AppendFormat("{0}", Mathf.CeilToInt(lastSec));
                    _CDTimeTxt.text = sb.ToString();
                }
            }
        }
    }

}
