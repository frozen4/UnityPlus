using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{
    public enum EnumCountType
    {
        CreateShape = 0,
        DestroyShape = 1,
        CreateNavQuery = 2,
        ClearNavQuery = 3,
        LoadNavMeshFromMemory = 4,
        ClearNavMesh = 5,
        CreateCrowd = 6,
        ClearCrowd = 7,
        crowdAddAgent = 8,
        crowdRemoveAgent = 9,
        luadllCountMax = 10,

    }
    public class SCounters : Singleton<SCounters>
    {
        private Dictionary<EnumCountType, long> _dicIncCounts;
        public SCounters()
        {
            _dicIncCounts = new Dictionary<EnumCountType, long>();
        }

        public void Increase(EnumCountType countType)
        {
            if (_dicIncCounts.ContainsKey(countType))
            {
                _dicIncCounts[countType] = _dicIncCounts[countType] + 1;
            }
            else
            {
                _dicIncCounts.Add(countType, 1);
            }
        }

        public string GetLuaDllCounterStr()
        {
            StringBuilder sbLuaDllStr = new StringBuilder();
            sbLuaDllStr.Append("GetLuaDllCounterStr: ");
            for ( int i = 0 ; i < (int)EnumCountType.luadllCountMax; i++)
            {
                sbLuaDllStr.AppendFormat(" {0}:{1} ", (EnumCountType)i, GetCount((EnumCountType)i));
            }

            return sbLuaDllStr.ToString();
        }

        public long GetCount(EnumCountType countType)
        {
            if (_dicIncCounts.ContainsKey(countType))
            {
                return _dicIncCounts[countType] ;
            }
            else
            {
                return 0;
            }
        }

    }
}