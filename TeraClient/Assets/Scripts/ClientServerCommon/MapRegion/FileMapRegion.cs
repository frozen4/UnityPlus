using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

#if SERVER_USE
using System;
#endif

namespace MapRegion
{
    //file header
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SMapRegionHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] magic;            //"MRGN"

        public int version;

        public float fWorldWid;

        public float fWorldLen;

        public float fGridSize;

        public int iNumRegion;
    }
    /// <summary>
    /// 区域内 最大内接矩形
    /// </summary>
    public class FileRegionMaxRect
    {
        public int Id { get; set; }
        public int XMin;
        public int XMax;
        public int YMin;
        public int YMax;
    }
    public class FileRegion
    {
        public int Id { get; set; }

        public int GridNum { get; set; }

        public HashSet<uint> RegionGridSet { get; set; }

    }

    public class FileMapRegion
    {
        public const int VERSION = 1;

        public SMapRegionHeader Header;
        private List<FileRegion> _regionOrderList;            //对应_regionMap的key
        private Dictionary<int, FileRegion> _regionMap;   //读取文件后建立的区域map
        private Dictionary<int, Dictionary<uint, int>> _PtToRegionSetMap;

        //private List<FileRegionMaxRect> _fileRegionMaxRectList;
        public Dictionary<uint, int> _posOffsetDic = new Dictionary<uint, int>();
        public  List<int> _offset2RegionIdsArray = new List<int>();

        private int[][] matrix;
        public int[] regionOffsets;
        public long totalCount;

        private float _fInvGridSize;
        private float _fLeft;
        private float _fTop;


        public FileMapRegion()
        {
            Header = new SMapRegionHeader();
            _regionOrderList = new List<FileRegion>();
            _regionMap = new Dictionary<int, FileRegion>();
            _PtToRegionSetMap = new Dictionary<int, Dictionary<uint, int>>();

            //_fileRegionMaxRectList = new List<FileRegionMaxRect>();
        }

        private int CompareFileRegion(FileRegion a, FileRegion b)
        {
            if (a.Id < b.Id)
                return -1;
            else if (a.Id > b.Id)
                return 1;
            else
                return 0;
        }

        public HashSet<int> ActiveIdSet
        {
            get;
            set;
        }
#if SERVER_USE
        public void BuildPtToRegionSetMap() //会分配大量内存，暂时服务器使用，客户端不用
        {
            BuildPtToRegionSetMapNew();
            _PtToRegionSetMap.Clear();

            foreach (FileRegion fileRegion in _regionOrderList)
            {

                foreach (var v2 in fileRegion.RegionGridSet)
                {

                    for (int i = 0; i < 10; i++)
                    {
                        Dictionary<uint, int> dic;
                        if (_PtToRegionSetMap.TryGetValue(i, out dic) == false)
                        {
                            dic = new Dictionary<uint, int>();
                            _PtToRegionSetMap.Add(i, dic);
                        }

                        if (dic.ContainsKey(v2) == true)
                            continue;

                        dic.Add(v2, fileRegion.Id);
                        break;
                    }

                }
            }
            //RegionPerformance();

            _regionMap.Clear();
            _regionOrderList.Clear();
            
        }
#endif
        public void BuildPtToRegionSetMapNew() 
        {
            _posOffsetDic.Clear();
            _offset2RegionIdsArray.Clear();

            //1. 整理Pos与区域的对应关系
            Dictionary<uint, List<int>> pos2RegionIdsDic = new Dictionary<uint, List<int>>();
            foreach (FileRegion fileRegion in _regionOrderList)
            {
                
                foreach (var v2 in fileRegion.RegionGridSet)
                {
                    List<int> regionIds = null;
                    if(pos2RegionIdsDic.TryGetValue(v2, out regionIds) == false)
                    {
                        regionIds = new List<int>();
                        pos2RegionIdsDic.Add(v2, regionIds);
                    }
                    if(regionIds.Contains(fileRegion.Id) == false)
                    {
                        regionIds.Add(fileRegion.Id);
                    }

                }
            }
            //2. 构建pos与区域字符串的对应关系
            Dictionary<uint, string> pos2RegionStrDic = new Dictionary<uint, string>();
            Func<List<int>, string> toString = (l) =>
            {
                string str = string.Empty;
                l.ForEach((i)=> str = str + "*" + i.ToString());
                return str;
            };
            foreach (var item in pos2RegionIdsDic)
            {
                item.Value.Sort();
                string str = toString(item.Value);
                pos2RegionStrDic.Add(item.Key, str);
            }
            //3. 构建 区域字符串 与 区域字符串索引 的关系：去重
            Dictionary<string, int> regionStrDic = new Dictionary<string, int>();
            int regionStrIndex = 0;
            foreach(var str in pos2RegionStrDic.Values)
            {
                if(regionStrDic.ContainsKey(str) == false)
                {
                    regionStrDic.Add(str, regionStrIndex);
                    regionStrIndex++;
                }
            }
            //4. 构建pos与区域字符串索引的关系
            Dictionary<uint, int> pos2RegionIndex = new Dictionary<uint, int>();
            foreach(var item in pos2RegionStrDic)
            {
                pos2RegionIndex.Add(item.Key, regionStrDic[item.Value]);
            }
            //5. 构建区域字符串索引 与 数组偏移量的关系
            //List<int> pos2RegionIdsArray = new List<int>();
            List<int> tmpList = new List<int>();
            Dictionary<int, int> regionIndex2Offset = new Dictionary<int, int>();
            foreach(var regionIdStr in regionStrDic.Keys)
            {
                var ids = regionIdStr.Split('*');
                tmpList.Clear();
                foreach (var str in ids)
                {
                    int regionId;
                    if (Int32.TryParse(str, out regionId))
                    {
                        tmpList.Add(regionId);
                    }
                }
                var offset = _offset2RegionIdsArray.Count;
                _offset2RegionIdsArray.Add(tmpList.Count);
                _offset2RegionIdsArray.AddRange(tmpList);
                regionIndex2Offset.Add(regionStrDic[regionIdStr], offset);
            }
            //6. 构建pos与数据偏移量的关系=》最终结果
            //Dictionary<uint, int> pos2Offset = new Dictionary<uint, int>();
            foreach(var item  in pos2RegionIndex)
            {
                var pos = item.Key;
                var index = item.Value;
                var offset = regionIndex2Offset[index];
                _posOffsetDic.Add(pos, offset);
            }

            pos2RegionIdsDic.Clear();
            pos2RegionStrDic.Clear();
            regionStrDic.Clear();
            pos2RegionIndex.Clear();
            /*
            var height = Math.Ceiling(Math.Abs(_fTop * 2) * _fInvGridSize);
            var width = Math.Ceiling(Math.Abs(_fLeft * 2) * _fInvGridSize);
            totalCount += (long)height * (long)width;
            //7. 构建矩阵
            matrix = new int[(int)height][];
            for (int i = 0; i < height; i++)
            {
                matrix[i] = new int[(int)width];
            }
            //8. 填充matrix
            for (var yMin = 0; yMin < height; yMin++)
            {
                for (var xMin =0; xMin < width; xMin++)
                {
                    var row = (uint)yMin;
                    var col = (uint)xMin;
                    var pos = (col | row << 16);
                    int offset;
                    if(pos2Offset.TryGetValue(pos, out offset))
                    {
                        matrix[yMin][xMin] = offset;
                    }
                }
            }
            //10. 填充regionOffsets
            regionOffsets = pos2RegionIdsArray.ToArray();
            
            */

            _regionMap.Clear();
            _regionOrderList.Clear();


        }

#region 正确性测试、性能对比测试
        struct Grid
        {
            public uint Pos;
            public uint Row;
            public uint Col;
            public int Id;
        }
        private void RegionPerformance()
        {
            /*
            var height = Math.Abs(_fTop * 2) * _fInvGridSize;
            var width = Math.Abs(_fLeft * 2) * _fInvGridSize;
            System.Random rd = new System.Random();
            List<Grid> testRegionIds = new List<Grid>();
            uint lowMask = 0;
            lowMask = ~lowMask;
            uint highMak = lowMask << 16;
            lowMask = lowMask >> 16;
 
            foreach (var region in _regionOrderList)
            {
                foreach (var pos in region.RegionGridSet)
                {
                    uint row = (highMak & pos) >> 16;
                    uint col = lowMask & pos;
                    testRegionIds.Add(new Grid { Pos = pos, Row = row, Col = col, Id = (int)row });
                }
            }

            var count = testRegionIds.Count;
            for (int i = 0; i < count; i++)
            {
                uint row = (uint)rd.Next(0, (int)height);
                uint col = (uint)rd.Next(0, (int)width);
                uint pos = (col | (row << 16));
                testRegionIds.Add(new Grid{Pos=pos, Row =row, Col= col, Id = (int)row});
            }
            //_log.errorFormat("TestCount: {0}", testRegionIds.Count);
            var retList1 = new List<bool>();
            var retList2 = new List<bool>();
            var retList3 = new List<bool>();
            var retList11 = new List<HashSet<int>>();
            var retList22 = new List<HashSet<int>>();
            var retList33 = new List<HashSet<int>>();
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            Func<long> funcDic = () =>
            {
                st.Restart();
                testRegionIds.ForEach(grid => { IsPosInAnyRegion(grid.Pos); });
                st.Stop();
                return st.ElapsedMilliseconds;
            };

            Func<long> funcMaxRect = () =>
            {
                st.Restart();
                testRegionIds.ForEach(grid => { IsPosInAnyRegionNew(grid.Pos); });
                st.Stop();

                return st.ElapsedMilliseconds;
            };



            var testHasSet = new HashSet<int>();

            Func<long> funcDicSet = ()=>{
                st.Restart();
                testRegionIds.ForEach(grid =>
                {
                    //var newhashset = new HashSet<int>();
                    testHasSet.Clear();
                    IsPosInAnyRegion(grid.Pos, testHasSet);
                    //retList11.Add(newhashset);
                });
                st.Stop();
                return st.ElapsedMilliseconds;
            };

            Func<long> funcMaxRectSet = () =>
            {
                st.Restart();
                testRegionIds.ForEach(grid =>
                {
                    //var newhashset = new HashSet<int>();
                    testHasSet.Clear();
                    IsPosInAnyRegionNew(grid.Pos, testHasSet);
                    //retList22.Add(newhashset);
                });

                st.Stop();
                return st.ElapsedMilliseconds;
            };

            /*
            for (int i = 0 ; i< retList1.Count; i++)
            {
                if (retList2[i] != retList1[i] )
                {
                    _log.errorFormat("true false error");
                }
            }

            for (int i =0 ; i< retList11.Count; i++)
            {
                foreach (var id in retList11[i])
                {
                    if (retList22[i].Contains(id) == false )
                    {
                        _log.errorFormat("set error");
                    }
                }
            }
            */
            /*
            var t1 = funcDic();
            var t2 = funcMaxRect();

            var t11 = funcDicSet();
            var t22 = funcMaxRectSet();



            var t55 = funcMaxRectSet();
            var t44 = funcDicSet();

            var t5 = funcMaxRect();
            var t4 = funcDic();
            


            _log.errorFormat("Calc 当前 {0}, MaxRect {1}, HasSet {2}  当前:{3}, MaxRect:{4}, HasSet:{5}", t1, t2, 0, t11, t22, 0);
            _log.errorFormat("Calc 当前 {0}, MaxRect {1}, HasSet {2}  当前:{3}, MaxRect:{4}, HasSet:{5}", t4, t5, 0, t44, t55, 0);
            */
        }

        public Dictionary<int, FileRegion> RegionMap
        {
            get { return _regionMap; }
        }
#endregion

        public void AddFileRegion(FileRegion fileRegion)
        {
            _regionMap.Add(fileRegion.Id, fileRegion);
            _regionOrderList.Add(fileRegion);
            _regionOrderList.Sort(CompareFileRegion);
        }

        public bool IsValid()
        {
            return Header.iNumRegion != 0 &&
                Header.iNumRegion == _regionMap.Count;
        }

        public uint ConvertPosition(float x, float z, out uint row, out uint col)
        {
            row = 0;
            col = 0;
            GetRowColByPosition(x, z, out row, out col);
            return (col | (row << 16));
        }

        public void GetGridPositions(uint row, uint col, Vector3[] positions, int offset)
        {
            float fGridSize = Header.fGridSize;

            positions[offset + 0].Set(_fLeft + fGridSize * col, 0, _fTop - fGridSize * row);
            positions[offset + 1].Set(_fLeft + fGridSize * (col + 1), 0, _fTop - fGridSize * row);
            positions[offset + 2].Set(_fLeft + fGridSize * col, 0, _fTop - fGridSize * (row + 1));
            positions[offset + 3].Set(_fLeft + fGridSize * (col + 1), 0, _fTop - fGridSize * (row + 1));
        }

        public void GetGridCentetPosition(uint row, uint col, ref float x, ref float z)
        {
            float fGridSize = Header.fGridSize;
            x = _fLeft + fGridSize * (col + 0.5f);
            z = _fTop - fGridSize * (row + 0.5f);
        }

        public void GetRowColByPosition(float x, float z, out uint row, out uint col)
        {
            //             float fInvGridSize = 1.0f / Header.fGridSize;
            // 
            //             float fLeft = -Header.fWorldWid * 0.5f;
            //             float fTop = Header.fWorldLen * 0.5f;

            float fRow = (_fTop - z) * _fInvGridSize;
            float fCol = (x - _fLeft) * _fInvGridSize;

            row = (uint)Mathf.FloorToInt(fRow);
            col = (uint)Mathf.FloorToInt(fCol);
        }

        /// <summary>
        /// 查看x z表示的点是否在障碍物区域内（x z对应的区域是否在区域集合activeObstacleSet中）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="activeObstacleSet"></param>
        /// <returns></returns>
        public bool IsInAnyRegion(float x, float z, HashSet<int> activeObstacleSet)
        {
            uint row;
            uint col;
            uint pos = ConvertPosition(x, z, out row, out col);

            //return IsPosInAnyRegion(pos);
            return IsPosInAnyRegionNew(pos, activeObstacleSet);
        }
        /// <summary>
        /// 查看pos表示的点是否在障碍物区域内（pos对应的区域是否在区域集合activeObstacleSet中）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="activeObstacleSet"></param>
        /// <returns></returns>
        private bool IsPosInAnyRegion(uint pos, HashSet<int> activeObstacleSet)
        {
            return IsPosInAnyRegionNew(pos, activeObstacleSet);
#region old
            /*
            if (_PtToRegionSetMap.Count > 0)         //use pt to region map
            {
                foreach (var v in _PtToRegionSetMap.Values)
                {
                    int id;
                    if (v.TryGetValue(pos, out id) == false)
                        break;

                    if (ActiveIdSet == null || ActiveIdSet.Contains(id))
                        return true;
                }
            }
            else              //iterate fileregion
            {
                for (int i = 0; i < _regionOrderList.Count; ++i )
                {
                    FileRegion fileRegion = _regionOrderList[i];
                    if (ActiveIdSet != null && !ActiveIdSet.Contains(fileRegion.Id))
                        continue;

                    if (fileRegion.RegionGridSet.Contains(pos))
                    {
                        return true;
                    }
                }
            }
            return false;
            */
#endregion
        }
        /// <summary>
        /// 查看pos表示的点是否在障碍物区域内（pos对应的区域是否在区域集合activeObstacleSet中）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="activeObstacleSet"></param>
        /// <returns></returns>
        private bool IsPosInAnyRegionNew(uint pos, HashSet<int> activeObstacleSet)
        {
            int offset;
            if (_posOffsetDic.TryGetValue(pos, out offset))
            {
                if (_offset2RegionIdsArray.Count <= offset)
                {
                    return false;
                }

                var count = _offset2RegionIdsArray[offset];
                var startIndex = offset + 1;
                for (var i = startIndex; i <= startIndex + count - 1; i++)
                {
                    if (activeObstacleSet != null && activeObstacleSet.Contains(_offset2RegionIdsArray[i]))
                        return true;

                }
            }
            return false;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="activeObstacleSet"></param>
        /// <param name="regionSet"></param>
        /// <returns></returns>
        private bool IsPosInAnyRegionNew(uint pos, HashSet<int> activeObstacleSet,HashSet<int> regionSet)
        {
            regionSet.Clear();
            int offset;
            if(_posOffsetDic.TryGetValue(pos, out offset))
            {
                if(_offset2RegionIdsArray.Count <= offset)
                {
                    return false;
                }

                var count = _offset2RegionIdsArray[offset];
                var startIndex = offset + 1;
                for (var i = startIndex; i<= startIndex + count- 1;i++ )
                {
                    if(_offset2RegionIdsArray.Count <= i)//调试使用
                    {
                        //_log.errorFormat("IsPosInAnyRegionNew");
                        return regionSet.Count >0 ;
                    }
                    if (activeObstacleSet != null && activeObstacleSet.Contains(_offset2RegionIdsArray[i]))
                        regionSet.Add(_offset2RegionIdsArray[i]);

                }
                return regionSet.Count >0 ;
            }
            return false;
        }
        /// <summary>
        /// 查看x z表示的点是否在障碍物区域内，返回所在的障碍物区域集合（x z对应的区域是否在区域集合activeObstacleSet中）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="activeObstacleSet"></param>
        /// <returns></returns>
        public bool IsInAnyRegion(float x, float z, HashSet<int> activeObstacleSet, HashSet<int> regionSet)
        {
            uint row;
            uint col;
            uint pos = ConvertPosition(x, z, out row, out col);

            //return IsPosInAnyRegion(pos, regionSet);
            return IsPosInAnyRegionNew(pos,activeObstacleSet, regionSet);
        }

        /// <summary>
        /// 单纯获取x/z所在区域（不管是什么区域）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="regionSet"></param>
        /// <returns></returns>
        public bool IsInNormalRegion(float x, float z,  HashSet<int> regionSet)
        {
            uint row;
            uint col;
            uint pos = ConvertPosition(x, z, out row, out col);
            return IsInNormalRegion(pos, regionSet);
        }

        public bool IsInNormalRegion(uint pos, HashSet<int> regionSet)
        {
            regionSet.Clear();
            int offset;
            if (_posOffsetDic.TryGetValue(pos, out offset))
            {
                if (_offset2RegionIdsArray.Count <= offset)
                {
                    return false;
                }

                var count = _offset2RegionIdsArray[offset];
                var startIndex = offset + 1;
                for (var i = startIndex; i <= startIndex + count - 1; i++)
                {
                    regionSet.Add(_offset2RegionIdsArray[i]);

                }
                return regionSet.Count > 0;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="activeObstacleSet">当前已经激活的障碍物对应的区域ID</param>
        /// <param name="regionSet">pos所在的障碍物区域（activeObstacleSet的子集）</param>
        /// <returns></returns>
        private bool IsPosInAnyRegion(uint pos, HashSet<int> activeObstacleSet, HashSet<int> regionSet)
        {
            return IsPosInAnyRegionNew(pos, activeObstacleSet, regionSet);
#region old
            /*
            regionSet.Clear();
            if (_PtToRegionSetMap.Count > 0)         //use pt to region map
            {
                foreach (var v in _PtToRegionSetMap.Values)
                {
                    int regionId;
                    if (v.TryGetValue(pos, out  regionId) == false)
                        break;

                    if (ActiveIdSet == null || ActiveIdSet.Contains(regionId))
                        regionSet.Add(regionId);
                }
            }
            else              //iterate fileregion
            {
                for (int i = 0; i < _regionOrderList.Count; ++i)
                {
                    FileRegion fileRegion = _regionOrderList[i];
                    if (ActiveIdSet != null && !ActiveIdSet.Contains(fileRegion.Id))
                        continue;

                    if (fileRegion.RegionGridSet.Contains(pos))
                    {
                        regionSet.Add(fileRegion.Id);
                    }
                }
            }
            return regionSet.Count > 0;
            */
#endregion
        }
        //获得两点之间的所有区域
        public bool RayCastALLRegion(Vector3 vStart, Vector3 vDelta, HashSet<int> RegionIds,float fStep = 0.2f)
        {
            vDelta.y = 0;

            float startX = vStart.x;
            float startZ = vStart.z;
            float deltaX = vDelta.x;
            float deltaZ = vDelta.z;
            float fDistance = vDelta.magnitude;
            if (fDistance < 0.0001f)             //退化为点
            {
                return IsInNormalRegion(startX, startZ,  RegionIds);
            }

            float fCur = 0.0f;
            float fLast = fCur;
            uint lastpos = 0;
            float fInvDistance = 1.0f / fDistance;
            while (fCur <= fDistance)
            {
                float d = fCur * fInvDistance;
                float x = startX + deltaX * d;
                float z = startZ + deltaZ * d;

                uint row;
                uint col;
                uint pos = ConvertPosition(x, z, out row, out col);
                if (fCur == 0.0f || lastpos != pos)
                {
                    lastpos = pos;
                    HashSet<int> tmpRegion = new HashSet<int>();
                    if (IsInNormalRegion(x, z, tmpRegion))
                    {
                        RegionIds.UnionWith(tmpRegion);
                    }
                }

                if (fCur == fDistance)          //结束
                    break;

                fLast = fCur;
                fCur += fStep;
                if (fCur > fDistance)
                    fCur = fDistance;
            }

            return false;
        }

        public bool RayCastAnyRegion( Vector3 vStart, Vector3 vDelta, HashSet<int> activeRegionIds, ref float fRatio, float fStep = 0.2f)
        {
            fRatio = 0;
            vDelta.y = 0;

            float startX = vStart.x;
            float startZ = vStart.z;
            float deltaX = vDelta.x;
            float deltaZ = vDelta.z;
            float fDistance = vDelta.magnitude;
            if (fDistance < 0.0001f)             //退化为点
            {
                return IsInAnyRegion(startX, startZ, activeRegionIds);
            }

            float fCur = 0.0f;
            float fLast = fCur;
            uint lastpos = 0;
            float fInvDistance = 1.0f / fDistance;
            while (fCur <= fDistance)
            {
                float d = fCur * fInvDistance;
                float x = startX + deltaX * d;
                float z = startZ + deltaZ * d;

                uint row;
                uint col;
                uint pos = ConvertPosition(x, z, out row, out col);
                if (fCur == 0.0f || lastpos != pos)
                {
                    lastpos = pos;

                    //IsPosInAnyRegionDebug(x,z, pos, row, col);
                    if (IsPosInAnyRegion(pos, activeRegionIds))
                    {
                        fRatio = fLast * fInvDistance;
                        return true;
                    }
                }

                if (fCur == fDistance)          //结束
                    break;

                fLast = fCur;
                fCur += fStep;
                if (fCur > fDistance)
                    fCur = fDistance;
            }

            return false;
        }

        /// <summary>
        /// 检测障碍物
        /// </summary>
        /// <param name="vStart"></param>
        /// <param name="vDelta"></param>
        /// <param name="activeRegionId">已经激活的障碍物集合</param>
        /// <param name="fRatio"></param>
        /// <param name="regionSet">vStart与vDelta之间遇到的障碍物（activeRegionId的子集）</param>
        /// <param name="fStep"></param>
        /// <returns></returns>
        public bool RayCastAnyRegion( Vector3 vStart, Vector3 vDelta, HashSet<int> activeRegionId, ref float fRatio, HashSet<int> regionSet, float fStep = 0.2f)
        {
            fRatio = 0.0f;
            vDelta.y = 0;

            float startX = vStart.x;
            float startZ = vStart.z;
            float deltaX = vDelta.x;
            float deltaZ = vDelta.z;
            float fDistance = vDelta.magnitude;
            if (fDistance < 0.0001f)             //退化为点
            {
                return IsInAnyRegion(startX, startZ, activeRegionId,regionSet);
            }

            float fCur = 0.0f;
            float fLast = fCur;
            uint lastpos = 0;
            float fInvDistance = 1.0f / fDistance;
            while (fCur <= fDistance)
            {
                float d = fCur * fInvDistance;
                float x = startX + deltaX * d;
                float z = startZ + deltaZ * d;

                uint row;
                uint col;
                uint pos = ConvertPosition(x, z, out row, out col);
                if (fCur == 0.0f || lastpos != pos)
                {
                    lastpos = pos;
                    var pt = IsPosInAnyRegion(pos, activeRegionId, regionSet);
                    if (pt)
                    {
                        fRatio = fLast * fInvDistance;
                        return true;
                    }
                }

                if (fCur == fDistance)          //结束
                    break;

                fLast = fCur;
                fCur += fStep;
                if (fCur > fDistance)
                    fCur = fDistance;
            }

            return false;
        }

#if SERVER_USE
        public bool ReadFile(string filename)
        {
            _regionOrderList.Clear();
            _regionMap.Clear();

            if (!File.Exists(filename))
                return false;

            try
            {
                 byte[] bytes;
                 using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                 {
                     using (BinaryReader br = new BinaryReader(fs))
                     {
                         bytes = br.ReadBytes(Marshal.SizeOf(typeof(SMapRegionHeader)));
                         Header = (SMapRegionHeader)RegionUtility.ByteToStruct(bytes, typeof(SMapRegionHeader));

                         for(int i = 0; i < Header.iNumRegion; ++i)
                         {
                             FileRegion fileRegion = new FileRegion();

                             fileRegion.Id = br.ReadInt32();
                             fileRegion.GridNum = br.ReadInt32();

                             HashSet<uint> gridSet = new HashSet<uint>();
                             for(int k = 0; k < fileRegion.GridNum; ++k)
                             {
                                 uint data = br.ReadUInt32();
                                 gridSet.Add(data);
                             }
                             fileRegion.RegionGridSet = gridSet;

                             //添加region
                             _regionMap.Add(fileRegion.Id, fileRegion);
                             _regionOrderList.Add(fileRegion);
                         }
                     }
                 }
            }
            catch (Exception ex)
            {
                return false;
            }

            _regionOrderList.Sort(CompareFileRegion);

            _fInvGridSize = 1.0f / Header.fGridSize;
            _fLeft = -Header.fWorldWid * 0.5f;
            _fTop = Header.fWorldLen * 0.5f;

            return true;
        }

        public bool WriteFile(string filename)
        {
            if (Header.version < FileMapRegion.VERSION)
                return false;

            try
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                BinaryWriter bw = new BinaryWriter(fs);

                byte[] headerBytes = RegionUtility.StructToBytes(Header, Marshal.SizeOf(typeof(SMapRegionHeader)));
                bw.Write(headerBytes);

                for (int i = 0; i < _regionOrderList.Count; ++i )
                {
                    FileRegion fileRegion = _regionOrderList[i];

                    bw.Write(fileRegion.Id);
                    bw.Write(fileRegion.GridNum);
                    foreach (uint d in fileRegion.RegionGridSet)
                    {
                        bw.Write(d);
                    }
                }

                bw.Close();
                fs.Close();
            }
            catch (IOException)
            {
                return false;
            }

            return true;
        }
#else
        public bool ReadFromMemory(byte[] filebuffer, bool skipId0 = false)
        {
            _regionOrderList.Clear();
            _regionMap.Clear();
            try
            {
                byte[] bytes;
                using (MemoryStream fs = new MemoryStream(filebuffer))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        bytes = br.ReadBytes(Marshal.SizeOf(typeof(SMapRegionHeader)));
                        Header = (SMapRegionHeader)RegionUtility.ByteToStruct(bytes, typeof(SMapRegionHeader));

                        for (int i = 0; i < Header.iNumRegion; ++i)
                        {
                            int id = br.ReadInt32();
                            int nGridNum = br.ReadInt32();
                            if (id == 0 && skipId0)                 //跳过id0
                            {
                                //跳过读取数据
                                br.ReadBytes(nGridNum * Marshal.SizeOf(typeof(uint)));
                                continue;
                            }

                            //Vector2 mins = new Vector2(99999.0f, 99999.0f);
                            //Vector2 maxs = new Vector2(-99999.0f, -99999.0f);
                            HashSet<uint> gridSet = new HashSet<uint>();
                            for (int k = 0; k < nGridNum; ++k)
                            {
                                uint data = br.ReadUInt32();
                                gridSet.Add(data);
                            }

                            FileRegion fileRegion = new FileRegion();
                            fileRegion.Id = id;
                            fileRegion.GridNum = nGridNum;
                            fileRegion.RegionGridSet = gridSet;

                            //添加region
                            _regionMap.Add(fileRegion.Id, fileRegion);
                            _regionOrderList.Add(fileRegion);
                        }

                    }
                }
            }
            catch (IOException)
            {
                return false;
            }

            _regionOrderList.Sort(CompareFileRegion);

            _fInvGridSize = 1.0f / Header.fGridSize;
            _fLeft = -Header.fWorldWid * 0.5f;
            _fTop = Header.fWorldLen * 0.5f;

            return true;
        }
#endif

#region 客户端在使用
        public int InWhichLightRegion(float x, float z)
        {
            int minRegionId = 0;
            bool bFind = (_regionMap.Count > 0) && IsInAnyRegion(x, z, out minRegionId);
            if (!bFind)
                minRegionId = 0;

            return minRegionId;
        }

        public bool IsInAnyRegion(float x, float z, out int minRegionId)            //找id最小的region 
        {
            uint row;
            uint col;
            uint pos = ConvertPosition(x, z, out row, out col);

            return IsPosInAnyRegion(pos, out minRegionId);
        }

        private bool IsPosInAnyRegion(uint pos, out int minRegionId)
        {
            bool bFind = false;
            minRegionId = int.MaxValue;
            if (_PtToRegionSetMap.Count > 0)         //use pt to region map
            {
                minRegionId = 0;
                foreach (var v in _PtToRegionSetMap.Values)
                {
                    if (v.ContainsKey(pos) == false)
                        break;

                    int id = v[pos];
                    if (id < minRegionId)
                        minRegionId = id;
                    bFind = true;
                }
            }
            else              //iterate fileregion
            {
                for (int i = 0; i < _regionOrderList.Count; ++i)
                {
                    FileRegion fileRegion = _regionOrderList[i];
                    if (fileRegion.RegionGridSet.Contains(pos))
                    {
                        minRegionId = fileRegion.Id;
                        bFind = true;
                        break;
                    }
                }
            }
            return bFind;
        }
#endregion
    }

}
