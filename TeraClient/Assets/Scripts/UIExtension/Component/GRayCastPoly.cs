using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class GRayCastPoly : GRayCastRect, ICanvasRaycastFilter
{
    private PolygonCollider2D _polygon = null;
    private PolygonCollider2D polygon
    {
        get
        {
            if (_polygon == null)
                _polygon = GetComponent<PolygonCollider2D>();
            return _polygon;
        }
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (polygon != null)
        {
            return polygon.OverlapPoint(eventCamera.ScreenToWorldPoint(screenPoint));
        }

        return true;
    }
}

