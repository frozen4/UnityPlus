using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class RFX4_TrailRenderer : ReusableFx
{
    public float VertexLifeTime = 2;
    public float TrailLifeTime = 2;

    [Range(0.001f, 1)]
    public float MinVertexDistance = 0.01f;
    public float Gravity = 0.01f;
    public Vector3 Force = new Vector3(0, 0, 0);
    public float InheritVelocity = 0;
    public float Drag = 0.01f;
    [Range(0.001f, 10)]
    public float Frequency = 1;
    [Range(0.001f, 10)]
    public float OffsetSpeed = 0.5f;
    public bool RandomTurbulenceOffset = false;
    [Range(0.001f, 10)]
    public float Amplitude = 2;
    public float TurbulenceStrength = 0.1f;
    public AnimationCurve VelocityByDistance = AnimationCurve.EaseInOut(0, 1, 1, 1);
    public float AproximatedFlyDistance = -1;
    public bool SmoothCurves = false;

    private LineRenderer lineRenderer;
    private List<Vector3> positions = new List<Vector3>();
    private List<float> currentTimes = new List<float>();
    private List<Vector3> velocities = new List<Vector3>();
    
    [HideInInspector]
    public float currentLifeTime;
    private Transform t;
    private Vector3 prevPosition;
    private Vector3 startPosition;
    private List<Vector3> controlPoints = new List<Vector3>();
    private int curveCount;
    private const float MinimumSqrDistance = 0.01f;
    private const float DivisionThreshold = -0.99f;
    private const float SmoothCurvesScale = 0.5f;

    private float currentVelocity;
    private float turbulenceRandomOffset;
    private bool isInitialized = false;

    public override void SetActive(bool active)
    {
        if (active)
        {
            Init();

            currentLifeTime = 0;
            curveCount = 0;
            currentVelocity = 0;
            t = transform;
            prevPosition = t.position;
            startPosition = t.position;
            lineRenderer.positionCount = 0;
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = Color.white;
            positions.Add(t.position);
            currentTimes.Add(currentLifeTime);
            velocities.Add(Vector3.zero);
            turbulenceRandomOffset = RandomTurbulenceOffset ? Random.Range(0, 10000f) / 1000f : 0;
        }
        else
        {
            if(!isInitialized) return;

            positions.Clear();
            currentTimes.Clear();
            velocities.Clear();
        }
        lineRenderer.enabled = active;

        base.SetActive(active);
    }

    public override void Tick(float dt)
    {
        UpdatePositionsCount(dt);

        UpdateForce(dt);
        UpdateImpulse(dt);
        UpdateVelocity(dt);
        var lastDeletedIndex = GetLastDeletedIndex(dt);

        RemovePositionsBeforeIndex(lastDeletedIndex);
        if (SmoothCurves && positions.Count > 2)
        {
            InterpolateBezier(positions, SmoothCurvesScale);
            var bezierPositions = GetDrawingPoints();
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = bezierPositions.Count;
                lineRenderer.SetPositions(bezierPositions.ToArray());
            }
            
        }
        else
        {
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = positions.Count;
                lineRenderer.SetPositions(positions.ToArray());
            }
        }
    }

    private void Init()
    {
        if(isInitialized) return;

        lineRenderer = GetComponent<LineRenderer>();

        isInitialized = true;
    }


    private void OnEnable()
    {
        SetActive(true);
    }

    private void OnDisable()
    {
        SetActive(false);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        Tick(dt);
    }

    private int GetLastDeletedIndex(float dt)
    {
        int lastDeletedIndex = -1;
        var count = currentTimes.Count;
       
        for (int i = 1; i < count; i++)
        {
            currentTimes[i] -= dt;
            if (currentTimes[i] <= 0)
                lastDeletedIndex = i;
        }
        return lastDeletedIndex;
    }

    private void UpdatePositionsCount(float dt)
    {
        if (positions == null || positions.Count == 0)
            return;

        if (TrailLifeTime > 0.0001f && currentLifeTime > TrailLifeTime)
            return;
        
        currentLifeTime += dt;

        var lastPosition = positions.Count != 0 ? positions[positions.Count - 1] : Vector3.zero;

        float distance = (t.position - lastPosition).magnitude;
        if (distance > MinVertexDistance && positions.Count > 0)
        {
            AddInterpolatedPositions(lastPosition, t.position, distance);
        }
    }

    void AddInterpolatedPositions(Vector3 start, Vector3 end, float distance)
    {
        var count = (int)(distance / MinVertexDistance);
        var previousTime = 0f;
        if(currentTimes.Count > 0) previousTime = currentTimes[currentTimes.Count-1];
        
        var vectorZero = Vector3.zero;
        for (int i = 1; i <= count-1; i++) {
            var interpolatedPoint = start + (end - start) * i * 1.0f / count;
            var interpolatedTime = previousTime + (VertexLifeTime - previousTime) * i * 1.0f / count;
           
            positions.Add(interpolatedPoint);
            currentTimes.Add(interpolatedTime);
            velocities.Add(vectorZero);
        }
    }

    private void RemovePositionsBeforeIndex(int lastDeletedIndex)
    {
        if (lastDeletedIndex == -1)
            return;
        var newSize = positions.Count - lastDeletedIndex;

        if (newSize == 1)
        {
            positions.Clear();
            currentTimes.Clear();
            velocities.Clear();
            return;
        }

        positions.RemoveRange(0, lastDeletedIndex);
        currentTimes.RemoveRange(0, lastDeletedIndex);
        velocities.RemoveRange(0, lastDeletedIndex);
    }

    private void UpdateForce(float dt)
    {
        if (positions == null || positions.Count == 0)
            return;
        var gravity = Gravity * Vector3.down * dt;
        var force = t.rotation * Force * Time.deltaTime;

        var factor = Amplitude * dt * TurbulenceStrength * 0.1f;
        var speed = (Time.time + turbulenceRandomOffset) * OffsetSpeed;

        for (int i = 0; i < positions.Count; i++) {

            var turbulenceVel = Vector3.zero;
            if (TurbulenceStrength > 0.000001f) {
                var pos = positions[i] / Frequency - speed * Vector3.one;
                turbulenceVel.x += (Mathf.PerlinNoise(pos.z, pos.y) * 2 - 1) * factor;
                turbulenceVel.y += (Mathf.PerlinNoise(pos.x, pos.z) * 2 - 1) * factor;
                turbulenceVel.z += (Mathf.PerlinNoise(pos.y, pos.x) * 2 - 1) * factor;
            }
            var currentForce = (gravity + force + turbulenceVel);
            if (AproximatedFlyDistance > 0.01f)
            {
                var distance = Mathf.Abs((positions[i] - startPosition).magnitude);
                currentForce *= VelocityByDistance.Evaluate(Mathf.Clamp01(distance / AproximatedFlyDistance));
            }
            velocities[i] += currentForce;
        }
    }

    private void UpdateImpulse(float dt)
    {
        if (velocities.Count == 0)
            return;

        currentVelocity = ((t.position - prevPosition).magnitude) / dt;
        var directionVelocity = (t.position - prevPosition).normalized;
        prevPosition = t.position;
        velocities[velocities.Count - 1] += currentVelocity * InheritVelocity * directionVelocity * dt;

    }

    private void UpdateVelocity(float dt)
    {
        if (velocities.Count==0)
            return;

        var count = positions.Count;
        for (int i = 0; i < count; i++) {
            if (Drag > 0.00001f)
                velocities[i] -= Drag * velocities[i] * dt;
            if (velocities[i].magnitude < 0.00001f)
                velocities[i] = Vector3.zero;
            positions[i] += velocities[i] * dt;
        }
    }

#region Bezier

    public void InterpolateBezier(List<Vector3> segmentPoints, float scale)
    {
        controlPoints.Clear();

        if (segmentPoints.Count < 2)
            return;

        for (int i = 0; i < segmentPoints.Count; i++)
        {
            if (i == 0) // is first
            {
                Vector3 p1 = segmentPoints[i];
                Vector3 p2 = segmentPoints[i + 1];

                Vector3 tangent = (p2 - p1);
                Vector3 q1 = p1 + scale * tangent;

                controlPoints.Add(p1);
                controlPoints.Add(q1);
            }
            else if (i == segmentPoints.Count - 1) //last
            {
                Vector3 p0 = segmentPoints[i - 1];
                Vector3 p1 = segmentPoints[i];
                Vector3 tangent = (p1 - p0);
                Vector3 q0 = p1 - scale * tangent;

                controlPoints.Add(q0);
                controlPoints.Add(p1);
            }
            else
            {
                Vector3 p0 = segmentPoints[i - 1];
                Vector3 p1 = segmentPoints[i];
                Vector3 p2 = segmentPoints[i + 1];
                Vector3 tangent = (p2 - p0).normalized;
                Vector3 q0 = p1 - scale * tangent * (p1 - p0).magnitude;
                Vector3 q1 = p1 + scale * tangent * (p2 - p1).magnitude;

                controlPoints.Add(q0);
                controlPoints.Add(p1);
                controlPoints.Add(q1);
            }
        }

        curveCount = (controlPoints.Count - 1) / 3;
    }

    private List<Vector3> drawingPoints = new List<Vector3>();
    public List<Vector3> GetDrawingPoints()
    {
        drawingPoints.Clear();

        for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
        {
            List<Vector3> bezierCurveDrawingPoints = FindDrawingPoints(curveIndex);

            if (curveIndex != 0)
                //remove the fist point, as it coincides with the last point of the previous Bezier curve.
                bezierCurveDrawingPoints.RemoveAt(0);

            drawingPoints.AddRange(bezierCurveDrawingPoints);
        }

        return drawingPoints;
    }

    private List<Vector3> pointList = new List<Vector3>();
    private List<Vector3> FindDrawingPoints(int curveIndex)
    {
        pointList.Clear();

        Vector3 left;
        Vector3 right;
        CalculateBezierPoints(curveIndex, 0, 1, out left, out right);
        
        pointList.Add(left);
        pointList.Add(right);

        FindDrawingPoints(curveIndex, 0, 1, pointList, 1);

        return pointList;
    }

    private int FindDrawingPoints(int curveIndex, float t0, float t1,
        List<Vector3> pointList, int insertionIndex)
    {
        Vector3 left;
        Vector3 right;
        CalculateBezierPoints(curveIndex, t0, t1, out left, out right);

        if ((left - right).sqrMagnitude < MinimumSqrDistance)
            return 0;

        float tMid = (t0 + t1) / 2;
        Vector3 mid = CalculateBezierPoint(curveIndex, tMid);

        Vector3 leftDirection = left - mid;
        Vector3 rightDirection = right - mid;
        if (Mathf.Abs(tMid - 0.5f) < 0.0001f ||
            Vector3.Dot(leftDirection, rightDirection) > DivisionThreshold * leftDirection.magnitude * rightDirection.magnitude)
        {
            int pointsAddedCount = 0;

            pointsAddedCount += FindDrawingPoints(curveIndex, t0, tMid, pointList, insertionIndex);
            pointList.Insert(insertionIndex + pointsAddedCount, mid);
            pointsAddedCount++;
            pointsAddedCount += FindDrawingPoints(curveIndex, tMid, t1, pointList, insertionIndex + pointsAddedCount);

            return pointsAddedCount;
        }
        return 0;
    }

    public Vector3 CalculateBezierPoint(int curveIndex, float t)
    {
        int nodeIndex = curveIndex * 3;
        Vector3 p0 = controlPoints[nodeIndex];
        Vector3 p1 = controlPoints[nodeIndex + 1];
        Vector3 p2 = controlPoints[nodeIndex + 2];
        Vector3 p3 = controlPoints[nodeIndex + 3];

        return CalculateBezierPoint(t, p0, p1, p2, p3);
    }

    public void CalculateBezierPoints(int curveIndex, float t0, float t1, out Vector3 v0, out Vector3 v1)
    {
        int nodeIndex = curveIndex * 3;
        Vector3 p0 = controlPoints[nodeIndex];
        Vector3 p1 = controlPoints[nodeIndex + 1];
        Vector3 p2 = controlPoints[nodeIndex + 2];
        Vector3 p3 = controlPoints[nodeIndex + 3];

        v0 = CalculateBezierPoint(t0, p0, p1, p2, p3);
        v1 = CalculateBezierPoint(t1, p0, p1, p2, p3);
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        if (t == 0.0f)
            return p0;
        if (t == 1.0f)
            return p3;
        if (t == 0.5f)
            return 0.125f * (p0 + 3 * p1 + 3 * p2 + p3);

        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; //first term

        p += 3 * uu * t * p1; //second term
        p += 3 * u * tt * p2; //third term
        p += ttt * p3; //fourth term

        return p;
    }

#endregion
}