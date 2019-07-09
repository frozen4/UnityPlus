using UnityEngine;
using System.Collections.Generic;
public class GTextLinkModel
{
    private int _StartIndex;
    private int _EndIndex;
    private string _LinkText;
    public int StartIndex { get { return _StartIndex; } set { _StartIndex = value; } }
    public int EndIndex { get { return _EndIndex; } set { _EndIndex = value; } }
    public string LinkText { get { return _LinkText; } set { _LinkText = value; } }
    private List<Rect> _Rects = new List<Rect>();
    public List<Rect> Rects { get { return _Rects; } }
    public void AddRect(Rect rect)
    {
        _Rects.Add(rect);
    }
}