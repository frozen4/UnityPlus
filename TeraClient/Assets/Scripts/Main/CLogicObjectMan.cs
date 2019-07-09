using System.Collections.Generic;

public class CLogicObjectMan<T> where T : class, GameLogic.ITickLogic
{
    private static CLogicObjectMan<T> _Instance = null;
    public static CLogicObjectMan<T> Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new CLogicObjectMan<T>();
            return _Instance;
        }
    }

    private readonly List<T> _ActiveObjectBehaviours = new List<T>();

    public void Add(T obj)
    {
        if (!_ActiveObjectBehaviours.Contains(obj))
            _ActiveObjectBehaviours.Add(obj);
    }

    public void Remove(T obj)
    {
        _ActiveObjectBehaviours.Remove(obj);
    }

    public void Tick(float dt)
    {
        int num = _ActiveObjectBehaviours.Count;
        for(var i = 0; i < num; ++i)
        {
            var obj = _ActiveObjectBehaviours[i];
            
            if (obj != null)
                obj.Tick(dt);
        }
    }

    public void Cleanup()
    {
//         if (Main.HostPalyer != null)
//         {
//             int xxx = 0;
//         }
        _ActiveObjectBehaviours.Clear();
    }
}
