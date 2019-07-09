using UnityEngine;
using System.Collections.Generic;

public class GBMFontAndMotionTextModel : ScriptableObject
{
    //public bool _IsWorldPos = false;
    //public float _LifeTime = 1f;
    public Vector2 _Offset;
    public float _XRandomNum;
    public Font _Font;
    //public int _FontSize = 28;
    public float _FontScale = 3;
    public bool _IsOnTopLayer = false;
    public int _BGType = -1;    //used by image group

    public List<GMotionModel> _MotionList = new List<GMotionModel>();
}
