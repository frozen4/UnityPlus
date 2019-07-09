using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CustomTrailRenderer : ReusableFx
{
    public float DurationTime;
    public float StartWidth;
    public float EndWidth;

    public float VertexDistance;

    public bool WithFixedUpdate;
    public bool WithSlerp;

    private MeshFilter _MeshFilter;
    private Mesh _Mesh;

    private List<Vector3> _Vertexs = new List<Vector3>();
    private List<Vector2> _UVs = new List<Vector2>();
    private List<int> _Indexs = new List<int>();

    struct Stamp
    {
        public Vector3 position;
        public Quaternion quaternion;
        public float time;
    }
    private List<Stamp> _Positions = new List<Stamp>();

    private Vector3 _Width = Vector3.zero;
    private float _SqrVertexDistance = 10000;

    public override void SetActive(bool active)
    {
        if (active)
        {
            if (_MeshFilter == null)
            {
                _MeshFilter = GetComponent<MeshFilter>();
                if (_MeshFilter.mesh == null)
                    _MeshFilter.mesh = new Mesh();
                _Mesh = _MeshFilter.mesh;
                _Width = Vector3.forward * StartWidth;
            }
        }
        else
        {
            _Positions.Clear();
            _Vertexs.Clear();
            _UVs.Clear();
            _Indexs.Clear();
            if (_Mesh != null)
                _Mesh.Clear();
        }

        _SqrVertexDistance = VertexDistance * VertexDistance;

        base.SetActive(active);
    }

    public override void LateTick(float dt)
    {
        if (_Mesh == null) return;

        int rmCount = 0;
        float now = Time.time;
        for (int i = 0; i < _Positions.Count; ++i)
        {
            if ((_Positions[i].time + DurationTime) <= now)
                ++rmCount;
        }
        if (rmCount > 0)
            _Positions.RemoveRange(0, rmCount);

        //Vector3 current_position = Anchor == null ? transform.position : Anchor.position;
        var curPos = transform.position;
        if (_Positions.Count == 0 || Vector3.SqrMagnitude(_Positions[_Positions.Count - 1].position - curPos) > _SqrVertexDistance)
        {
            _Positions.Add(new Stamp()
            {
                position = curPos,
                quaternion = transform.rotation,
                time = Time.time
            });
        }

        if (WithSlerp)
            BuildLineWithLerp();
        else
            BuildLine();
    }


    private Vector3 TransformToLocal(Matrix4x4 worldToLocal, Vector3 localPos)
    {
        return worldToLocal.MultiplyPoint(localPos);
    }

    private void BuildLineWithLerp()
    {
        _Mesh.Clear();
        _Vertexs.Clear();
        _UVs.Clear();
        _Indexs.Clear();

        if (_Positions.Count < 2) return;

        Matrix4x4 matWorldToLocal = transform.worldToLocalMatrix;

        for (int i = 0; i < _Positions.Count - 1; ++i)
        {
            Vector3 pos_1 = _Positions[i].position;
            Vector3 pos_2 = _Positions[i + 1].position;
            
            float sqr_length = Vector3.SqrMagnitude(pos_1 - pos_2);
            if (sqr_length > _SqrVertexDistance)
            {
                int lerp_count = 3;// Mathf.FloorToInt(sqr_length / VertexDistance * VertexDistance);
                for (int j = 1; j < lerp_count; ++j)
                {
                    Quaternion quat_1 = _Positions[i].quaternion;
                    Quaternion quat_2 = _Positions[i + 1].quaternion;

                    float t = (float)j / (float)lerp_count;
                    Vector3 pos = Vector3.Slerp(pos_1, pos_2, t);
                    Quaternion quat = Quaternion.Slerp(quat_1, quat_2, t);

                    Vector3 v = quat * _Width;
                    Vector3 pos1 = v + pos;
                    Vector3 pos2 = -v + pos;

                    _Vertexs.Add(TransformToLocal(matWorldToLocal, pos1));
                    _Vertexs.Add(TransformToLocal(matWorldToLocal, pos2));
                }
            }
            else
            {
                Vector3 pos = _Positions[i].position;
                Quaternion quat = _Positions[i].quaternion;

                Vector3 v = quat * _Width;
                Vector3 pos1 = v + pos;
                Vector3 pos2 = -v + pos;

                _Vertexs.Add(TransformToLocal(matWorldToLocal, pos1));
                _Vertexs.Add(TransformToLocal(matWorldToLocal, pos2));
            }
        }

        //last
        {
            int i = _Positions.Count - 1;

            Vector3 pos = _Positions[i].position;
            Quaternion quat = _Positions[i].quaternion;

            Vector3 v = quat * _Width;
            Vector3 pos1 = v + pos;
            Vector3 pos2 = -v + pos;

            _Vertexs.Add(TransformToLocal(matWorldToLocal, pos1));
            _Vertexs.Add(TransformToLocal(matWorldToLocal, pos2));
        }

        int vertex_count = (int)(_Vertexs.Count * 0.5f);
        for (int i = 0; i < vertex_count; i++)
        {
            float u = i / (float)(vertex_count - 1);

            _UVs.Add(new Vector2(u, 0));
            _UVs.Add(new Vector2(u, 1));
        }

        int trianglr_count = (vertex_count - 1) * 2;
        for (int i = 0; i < trianglr_count; i += 2)
        {
            _Indexs.Add(i + 3);
            _Indexs.Add(i + 0);
            _Indexs.Add(i + 1);
            _Indexs.Add(i + 0);
            _Indexs.Add(i + 3);
            _Indexs.Add(i + 2);
        }

        _Mesh.SetVertices(_Vertexs);
        _Mesh.SetUVs(0, _UVs);
        _Mesh.SetTriangles(_Indexs, 0, true);
    }

    private void BuildLine()
    {
        _Mesh.Clear();
        _Vertexs.Clear();
        _UVs.Clear();
        _Indexs.Clear();

        if (_Positions.Count < 2) return;

        Matrix4x4 matWorldToLocal = transform.worldToLocalMatrix;
        for (int i = 0; i < _Positions.Count; ++i)
        {
            float u = i / (float)(_Positions.Count - 1);

            Vector3 pos = _Positions[i].position;
            Quaternion quat = _Positions[i].quaternion;

            Vector3 v = quat * _Width;
            Vector3 pos1 = v + pos;
            Vector3 pos2 = -v + pos;

            _Vertexs.Add(TransformToLocal(matWorldToLocal, pos1));
            _Vertexs.Add(TransformToLocal(matWorldToLocal, pos2));

            _UVs.Add(new Vector2(u, 0));
            _UVs.Add(new Vector2(u, 1));
        }

        int trianglr_count = (_Positions.Count - 1) * 2;
        for (int i = 0; i < trianglr_count; i += 2)
        {
            _Indexs.Add(i + 3);
            _Indexs.Add(i + 0);
            _Indexs.Add(i + 1);
            _Indexs.Add(i + 0);
            _Indexs.Add(i + 3);
            _Indexs.Add(i + 2);
        }

        _Mesh.SetVertices(_Vertexs);
        _Mesh.SetUVs(0, _UVs);
        _Mesh.SetTriangles(_Indexs, 0, true);
    }

    void OnEnable()
    {
        SetActive(true);
    }

    void OnDisable()
    {
        SetActive(false);
    }

    void LateUpdate()
    {
        LateTick(Time.deltaTime);
    }
}
