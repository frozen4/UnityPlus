using UnityEngine;

public enum BodyPartEnum
{
    ExPart1 =1 ,
    ExPart2,
    ExPart3,
    ExPart4,
    ExPart5,
    ExPart6,

    MaxCount = ExPart6,
}

public class BodyPartCollector : MonoBehaviour
{
    public Renderer _ExPart1;
    public Renderer _ExPart2;
    public Renderer _ExPart3;
    public Renderer _ExPart4;
    public Renderer _ExPart5;
    public Renderer _ExPart6;

    private bool[] _RevertInfos = new bool[(int)BodyPartEnum.MaxCount];

    private void Awake()
    {
        _RevertInfos[0] = null != _ExPart1 && _ExPart1.enabled;
        _RevertInfos[1] = null != _ExPart2 && _ExPart2.enabled;
        _RevertInfos[2] = null != _ExPart3 && _ExPart3.enabled;
        _RevertInfos[3] = null != _ExPart4 && _ExPart4.enabled;
        _RevertInfos[4] = null != _ExPart5 && _ExPart5.enabled;
        _RevertInfos[5] = null != _ExPart6 && _ExPart6.enabled;
    }

    public Renderer GetRenderById(BodyPartEnum id)
    {
        if (id == BodyPartEnum.ExPart1)
        {
            return _ExPart1;
        }
        else if (id == BodyPartEnum.ExPart2)
        {
            return _ExPart2;
        }
        else if (id == BodyPartEnum.ExPart3)
        {
            return _ExPart3;
        }
        else if (id == BodyPartEnum.ExPart4)
        {
            return _ExPart4;
        }
        else if (id == BodyPartEnum.ExPart5)
        {
            return _ExPart5;
        }
        else if (id == BodyPartEnum.ExPart6)
        {
            return _ExPart6;
        }
        else
        {
            Common.HobaDebuger.LogError("error occur in GetRenderById id = "+ id);
            return null;
        }
    }
    public void Revert()
    {
        if(null != _ExPart1)
        {
            _ExPart1.enabled = _RevertInfos[0];
        }
        if (null != _ExPart2)
        {
            _ExPart2.enabled = _RevertInfos[1];
        }
        if (null != _ExPart3)
        {
            _ExPart3.enabled = _RevertInfos[2];
        }
       if (null != _ExPart4)
        {
            _ExPart4.enabled = _RevertInfos[3];
        }
       if (null != _ExPart5)
        {
            _ExPart5.enabled = _RevertInfos[4];
        }
        if (null != _ExPart6)
        {
            _ExPart6.enabled = _RevertInfos[5];
        }
    }
}
