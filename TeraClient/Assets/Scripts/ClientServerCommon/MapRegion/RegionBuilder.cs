using System;
using System.Collections.Generic;
using System.Text;
using Template;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MapRegion
{
#if ART_USE
    public class RegionBuilder
    {
        Scene.SceneRegionRoot.Region _region;
        Scene _map;
        float _mapXLen;
        float _mapZLen;            
        float _mapGridSize;
        Rect  _mapRect;
        private readonly List<Vector2> _polygon = new List<Vector2>();

        public RegionBuilder(Scene map, Scene.SceneRegionRoot.Region region, float fGridSize)
        {
            _map = map;
            _region = region;
            _mapXLen = map.Width;
            _mapZLen = map.Length;
            _mapGridSize = fGridSize;

            _mapRect = new Rect(new Vector2(-_mapXLen * 0.5f, -_mapZLen * 0.5f), new Vector2(_mapXLen, _mapZLen));
        }

        //col沿着x轴增大的方向, row沿着z轴减小的方向
        public void GetRowColByPosition(Vector2 pos, out uint row, out uint col)
        {
            float fInvGridSize = 1.0f/ _mapGridSize;

            float fRow = (_mapRect.yMax - pos.y) * fInvGridSize;
            float fCol = (pos.x - _mapRect.xMin) * fInvGridSize;

            row = (uint)Mathf.FloorToInt(fRow);
            col = (uint)Mathf.FloorToInt(fCol);
        }

        public HashSet<uint> BuildRegionHashSet(out string errMsg)
        {
            var bezierCurve = _map.GetBezierCurve(_region.BezierCurveId);
            if(bezierCurve == null)
            {
                errMsg = "RegionBuilder.BuildRegionHashSet: bezierCurve is null! BezierCurveId: " + _region.BezierCurveId;
                return null;
            }
            if (!bezierCurve.Closed)
            {
                errMsg = "RegionBuilder.BuildRegionHashSet: bezierCurve is not Closed! BezierCurveId: " + _region.BezierCurveId;
                return null;
            }

            errMsg = "";

            float fInvGridSize = 1.0f / _mapGridSize;
            Vector2 mins = _mapRect.max;
            Vector2 maxs = _mapRect.min;

            _polygon.Clear();
            foreach (var point in bezierCurve.BezierNodes)
            {
                Vector2 v = new Vector2(point.PositionX, point.PositionZ);
                _polygon.Add(v);

                if (v.x < mins.x)
                    mins.x = v.x;

                if (v.y < mins.y)
                    mins.y = v.y;

                if (v.x > maxs.x)
                    maxs.x = v.x;

                if (v.y > maxs.y)
                    maxs.y = v.y;
            }

            Rect rcRegion = new Rect(mins, maxs - mins);

            int nWidth = (int)(_mapXLen * fInvGridSize);
            int nHeight = (int)(_mapZLen * fInvGridSize);
            HashSet<uint> regionSet = new HashSet<uint>();
            Vector2 vOrg = new Vector2(_mapRect.xMin + 0.5f * _mapGridSize, _mapRect.yMax - 0.5f * _mapGridSize);
            for (int h = 0; h < nHeight; ++h)
            {
                for (int w = 0; w < nWidth; ++w)
                {
                    Vector2 vPos = vOrg + new Vector2(w * _mapGridSize, -h * _mapGridSize);
                    if (!RegionUtility.IsPointInRect(vPos, rcRegion))     //快速判断点是否在region的rect内 
                        continue;

                    if (PolygonFunc.IsInPolygon2(vPos.x, vPos.y, _polygon))
                    {
                        uint col = (uint)w;
                        uint row = (uint)h;
                        uint data = (col | row << 16);
                        regionSet.Add(data);
                    }
                }
            }

            return regionSet;
        }
    }

    public class RegionSetBuilder
    {
        Scene _map;

        public RegionSetBuilder(Scene map)
        {
            _map = map;
        }

        public FileMapRegion CreateFileMapRegion(float fGridSize, out string errMsg)
        {
            FileMapRegion fileMapRegion = new FileMapRegion();

            var regions = _map.RegionRoot.Regions;

            //build header
            fileMapRegion.Header.magic = new byte[] { (byte)'M', (byte)'R', (byte)'G', (byte)'N' };
            fileMapRegion.Header.version = 1;
            fileMapRegion.Header.fWorldWid = _map.Width;
            fileMapRegion.Header.fWorldLen = _map.Length;
            fileMapRegion.Header.fGridSize = fGridSize;
            fileMapRegion.Header.iNumRegion = regions.Count;

            errMsg = "";
            foreach(var region in regions)
            {
                string err;
                RegionBuilder builder = new RegionBuilder(_map, region, fGridSize);
                HashSet<uint> gridSet = builder.BuildRegionHashSet(out err);
                if(gridSet == null)
                {
                    errMsg += err;
                    errMsg += "\n";
                    continue;
                }

                FileRegion fileRegion = new FileRegion();
                fileRegion.Id = region.Id;
                fileRegion.GridNum = gridSet.Count;
                fileRegion.RegionGridSet = gridSet;

                //add file region
                fileMapRegion.AddFileRegion(fileRegion);
            }

            return fileMapRegion;
        }
    }
#endif
}
