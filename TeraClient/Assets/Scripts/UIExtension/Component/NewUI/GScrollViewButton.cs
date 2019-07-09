using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GScrollViewButton : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    public ScrollRect Scroll;
    public float Speed = 5f;
    public float OutLook = 0.1f;

    public Direction Dir;

    GButton _gButton;
    bool _pressing = false;

	void OnPress (GameObject go) {
        _pressing = true;
        PointerEventData pe = new PointerEventData(EventSystem.current);

        Scroll.OnBeginDrag(pe);
	}

    void OnRelease(GameObject go)
    {
        _pressing = false;
        PointerEventData pe = new PointerEventData(EventSystem.current);

        Scroll.OnEndDrag(pe);
    }

    void OnHold()
    {
        PointerEventData pe = new PointerEventData(EventSystem.current);

        switch (Dir)
        {
            case Direction.Up:
                pe.scrollDelta = Vector2.up * Speed;
                if (Scroll.vertical && Scroll.verticalNormalizedPosition < 1 + OutLook)
                {
                    Scroll.OnScroll(pe);
                }
                break;
            case Direction.Down:
                pe.scrollDelta = Vector2.down * Speed;
                if (Scroll.vertical && Scroll.verticalNormalizedPosition > 0 - OutLook)
                {
                    Scroll.OnScroll(pe);
                }
                break;
            case Direction.Left:
                pe.scrollDelta = Vector2.left * Speed;
                if (Scroll.horizontal && Scroll.horizontalNormalizedPosition < 1 + OutLook )
                {
                    Scroll.OnScroll(pe);
                }
                break;
            case Direction.Right:
                pe.scrollDelta = Vector2.right * Speed;
                if (Scroll.horizontal && Scroll.horizontalNormalizedPosition > 0 - OutLook)
                {
                    Scroll.OnScroll(pe);
                }
                break;
        }
    }

    void OnEnable()
    {
        if (_gButton == null)
        {
            _gButton = GetComponent<GButton>();
        }

        if (_gButton != null)
        {
            _gButton.PointerDownHandler += OnPress;
            _gButton.PointerUpHandler += OnRelease;
        }
    }

    void OnDisable()
    {
        if (_gButton != null)
        {
            _gButton.PointerDownHandler -= OnPress;
            _gButton.PointerUpHandler -= OnRelease;
        }
    }

    void Update()
    {
        if(_pressing)
        {
            OnHold();
        }
    }

}
