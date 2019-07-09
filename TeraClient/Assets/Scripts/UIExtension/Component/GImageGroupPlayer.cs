using UnityEngine;
using UnityEngine.UI;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(GImageGroup))]
public class GImageGroupPlayer : GBase
{
    public bool IsLoop = false;

    [SerializeField]
    private int _FrameSpeed = 7; // x frmae pre second
    private int _MaxCount = 0;
    private GImageGroup _Group;
    private bool _IsPlaying = false;
    private Action _OnComplete;
    private float _StartTime = 0;

    protected override void Awake()
    {
        _Group = GetComponent<GImageGroup>();
        if(_Group == null) return;
        if(_Group._Sprites == null) return;
        _MaxCount = _Group._Sprites.Length;

        ResetAnimation();
    }

    protected void ResetAnimation()
    {
        _Group.SetImageIndex(0);
        _IsPlaying = false;
        _OnComplete = null;
        _StartTime = 0;
    }

    void Update()
    {
        if (!_IsPlaying) return;

        if(IsLoop) // 循环播放
        {
            var idx = Mathf.FloorToInt((Time.time - _StartTime) * _FrameSpeed) % _MaxCount;
            _Group.SetImageIndex(idx);
        }
        else
        {
            var idx = Mathf.FloorToInt((Time.time - _StartTime) * _FrameSpeed);
            if(idx < _MaxCount)
                _Group.SetImageIndex(idx);

            if(idx >= _MaxCount - 1)
            {
                _IsPlaying = false;
                if (_OnComplete != null)
                {
                    _OnComplete();
                    _OnComplete = null;
                }
            }
        }
    }

    public void Play(Action onComplete)
    {
        if(!gameObject.activeSelf)
            gameObject.SetActive(true);
        ResetAnimation();
        _IsPlaying = true;
        _OnComplete = onComplete;
        _StartTime = Time.time;
    }

    public void Stop()
    {
        gameObject.SetActive(false);
        ResetAnimation();
    }
}