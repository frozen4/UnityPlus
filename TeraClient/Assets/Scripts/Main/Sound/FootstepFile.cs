using Common;
using System;
using System.IO;
using UnityEngine;

public class FootstepFile
{
    /*
0 = 硬质土地；
1 = 草地；
2 = 石头；
3 = 雪地+沙地；
4 = 木质地板；
5 = 金属地板；
6 = 可行走水域浅；
7 = 可行走水域深；
     * */

    private bool _IsInited = false;

    private float _min_x;
    private float _min_z;

    private float _max_x;
    private float _max_z;

    private int _count_x;
    private int _count_z;

    private float _step_trace;

    private byte[,] _footstepData;

    public FootstepFile()
    {
        Clear();
    }

    public bool IsInited { get { return _IsInited; } }

    public bool Load(string strFileRegionPath)
    {
        Clear();
        if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(strFileRegionPath)))
            return true;

        _IsInited = false;
        string fullName = HobaText.Format("{0}/Maps/{1}", EntryPoint.Instance.ResPath, strFileRegionPath);

        byte[] file_data = Util.ReadFile(fullName);
        if (file_data == null)
        {
            HobaDebuger.LogWarningFormat("{0} not exist", fullName);
            return false;
        }

        return LoadFromMemory(file_data);
    }

    public bool LoadFromMemory(byte[] filebuffer)
    {
        Clear();

        if (filebuffer == null)
        {
            _IsInited = false;
            return false;
        }

        try
        {
            using (MemoryStream fs = new MemoryStream(filebuffer))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    _min_x = br.ReadSingle();
                    _min_z = br.ReadSingle();

                    _max_x = br.ReadSingle();
                    _max_z = br.ReadSingle();

                    _count_x = br.ReadInt32();
                    _count_z = br.ReadInt32();

                    _step_trace = br.ReadSingle();

                    _footstepData = new byte[_count_x, _count_z];
                    for (int iz = 0; iz < _count_z; ++iz)
                    {
                        for (int ix = 0; ix < _count_x; ++ix)
                        {
                            _footstepData[ix, iz] = br.ReadByte();
                        }
                    }
                }
            }
        }
        catch (IOException e)
        {
            HobaDebuger.LogWarning(e.Message);

            _IsInited = false;
            return _IsInited;
        }

        _IsInited = true;
        return _IsInited;
    }

    public void Load(FileStream file_stream)
    {
        Clear();

        try
        {
            using (BinaryReader br = new BinaryReader(file_stream))
            {
                _min_x = br.ReadSingle();
                _min_z = br.ReadSingle();

                _max_x = br.ReadSingle();
                _max_z = br.ReadSingle();

                _count_x = br.ReadInt32();
                _count_z = br.ReadInt32();

                _step_trace = br.ReadSingle();

                _footstepData = new byte[_count_x, _count_z];
                for (int iz = 0; iz < _count_z; ++iz)
                {
                    for (int ix = 0; ix < _count_x; ++ix)
                    {
                        _footstepData[ix, iz] = br.ReadByte();
                    }
                }
            }
        }
        catch (IOException e)
        {
            HobaDebuger.LogWarning(e.Message);

            _IsInited = false;
            return;
        }

        _IsInited = true;
    }

    public bool FootstepID(Vector2 pos, ref byte footstep_id)
    {
        return FootstepID(pos.x, pos.y, ref footstep_id);
    }

    public bool FootstepID(Vector3 pos, ref byte footstep_id)
    {
        return FootstepID(pos.x, pos.z, ref footstep_id);
    }

    public bool FootstepID(float x, float z, ref byte footstep_id)
    {
        if (!_IsInited)
            return false;

        float px = x - _min_x;
        float pz = z - _min_z;

        int ix = Mathf.CeilToInt(px / _step_trace);
        int iz = Mathf.CeilToInt(pz / _step_trace);

        ix = Mathf.Clamp(ix, 0, _count_x - 1);
        iz = Mathf.Clamp(iz, 0, _count_z - 1);

        footstep_id = _footstepData[ix, iz];
        return true;
    }

    public void Clear()
    {
        _IsInited = false;

        _min_x = 0;
        _min_z = 0;

        _max_x = 0;
        _max_z = 0;

        _count_x = 0;
        _count_z = 0;

        _step_trace = 0;

        _footstepData = null;
    }
}