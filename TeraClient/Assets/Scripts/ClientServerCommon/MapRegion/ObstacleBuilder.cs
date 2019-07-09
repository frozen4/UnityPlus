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
    public enum EntityType
    {
        Role = 0,
        Monster = 1,
        Npc = 2,
        Subobject = 3,
        Obstacle = 4,
    }

    public class ObstacleBuilder
    {
        Scene.SceneEntityGeneratorRoot.EntityGenerator _obstacle;
        Template.Obstacle _template;
        Scene _map;
        float _mapXLen;
        float _mapZLen;
        float _mapGridSize;
        Rect _mapRect;
        private readonly List<Vector2> _polygon = new List<Vector2>();

        public ObstacleBuilder(Scene map, Scene.SceneEntityGeneratorRoot.EntityGenerator obstacle, float fGridSize)
        {
            _map = map;
            _obstacle = obstacle;
            _mapXLen = map.Width;
            _mapZLen = map.Length;
            _mapGridSize = fGridSize;

            _mapRect = new Rect(new Vector2(-_mapXLen * 0.5f, -_mapZLen * 0.5f), new Vector2(_mapXLen, _mapZLen));
            _template = Template.ObstacleModule.Manager.Instance.GetTemplate(_obstacle.EntityInfos[0].EntityId);
        }

        //col沿着x轴增大的方向, row沿着z轴减小的方向
        public void GetRowColByPosition(Vector2 pos, out uint row, out uint col)
        {
            float fInvGridSize = 1.0f / _mapGridSize;

            float fRow = (_mapRect.yMax - pos.y) * fInvGridSize;
            float fCol = (pos.x - _mapRect.xMin) * fInvGridSize;

            row = (uint)Mathf.FloorToInt(fRow);
            col = (uint)Mathf.FloorToInt(fCol);
        }

        public HashSet<uint> BuildRegionHashSet(out string errMsg)
        {
            if (_template == null)
            {
                errMsg = "ObstacleBuilder.BuildRegionHashSet: _template is null! ObstacleId: " + _obstacle.EntityInfos[0].EntityId;
                return null;
            }

            errMsg = "";

            //欧拉角到方向
            var quaternion = Quaternion.Euler(_obstacle.RotationX, _obstacle.RotationY, _obstacle.RotationZ);
            Vector3 vDir = quaternion * Vector3.forward;
            vDir.y = 0;
            vDir.Normalize();
            Vector3 vCenter = new Vector3(_obstacle.PositionX, _obstacle.PositionY, _obstacle.PositionZ);

            Vector3 ExtX;
            Vector3 ExtY;
            Vector3 ExtZ;
            Vector3 vExtent = new Vector3(_obstacle.Length * 0.5f, _template.Height * 0.5f, _template.Width * 0.5f);

            Matrix4x4 mat;
            RegionUtility.a3d_TransformMatrix(vDir, Vector3.up, vCenter, out mat);
            ExtX = new Vector3(mat.m00, mat.m10, mat.m20) * vExtent.x;
            ExtY = new Vector3(mat.m01, mat.m11, mat.m21) * vExtent.y;
            ExtZ = new Vector3(mat.m02, mat.m12, mat.m22) * vExtent.z;

            float fInvGridSize = 1.0f / _mapGridSize;
            Vector2 mins = _mapRect.max;
            Vector2 maxs = _mapRect.min;

            Vector3[] points = new Vector3[4];
            points[0] = vCenter - ExtX + ExtY + ExtZ;
            points[1] = points[0] + 2 * ExtX;
            points[2] = points[1] - 2 * ExtZ;
            points[3] = points[2] - 2 * ExtX;

            _polygon.Clear();
            foreach (var point in points)
            {
                Vector2 v = new Vector2(point.x, point.z);
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

            if (regionSet.Count == 0)
            {
                errMsg = HobaText.Format("ObstacleBuilder.BuildRegionHashSet: RegionSet Count is zero! ObstacleId: {0}, EntityGeneratorId: {1} generator len: {2}, template wid: {3}, template hei: {4}",
                    _obstacle.EntityInfos[0].EntityId, _obstacle.Id, _obstacle.Length, _template.Width, _template.Height);
            }

            return regionSet;
        }
    }

    public class ObstacleSetBuilder
    {
        Scene _map;

        public ObstacleSetBuilder(Scene map)
        {
            _map = map;
        }

        public FileMapRegion CreateFileMapRegion(float fGridSize, out string errMsg)
        {
            FileMapRegion fileMapRegion = new FileMapRegion();

            var obstacles = _map.EntityGeneratorRoot.EntityGenerators;

            //build header
            fileMapRegion.Header.magic = new byte[] { (byte)'M', (byte)'R', (byte)'G', (byte)'N' };
            fileMapRegion.Header.version = FileMapRegion.VERSION;
            fileMapRegion.Header.fWorldWid = _map.Width;
            fileMapRegion.Header.fWorldLen = _map.Length;
            fileMapRegion.Header.fGridSize = fGridSize;

            errMsg = "";
            foreach (var obstacle in obstacles)
            {
                if (obstacle.EntityType != (int)EntityType.Obstacle)
                    continue;

                string err;
                ObstacleBuilder builder = new ObstacleBuilder(_map, obstacle, fGridSize);
                HashSet<uint> gridSet = builder.BuildRegionHashSet(out err);
                if (gridSet == null || gridSet.Count == 0)
                {
                    errMsg += err;
                    errMsg += "\n";
                    continue;
                }

                FileRegion fileRegion = new FileRegion();
                fileRegion.Id = obstacle.Id;
                fileRegion.GridNum = gridSet.Count;
                fileRegion.RegionGridSet = gridSet;

                //add file region
                fileMapRegion.AddFileRegion(fileRegion);
            }
            fileMapRegion.Header.iNumRegion = fileMapRegion.RegionMap.Count;

            return fileMapRegion;
        }
    }
#endif
}
