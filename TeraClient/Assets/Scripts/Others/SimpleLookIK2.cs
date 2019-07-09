using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleLookIK2 : MonoBehaviour
{
    public Vector3 lookForward { get { return HeadNode.rotation * Vector3.up; } }

    public Transform HeadNode = null;
    public Transform EyeNode = null;
    public Transform LookTarget = null;
    public float LookAngleLerp = 60;
    public float LookAngleMax = 85;

    AnimationCurve curve;

    public float time = 0.7f;
    float elapse = 0;
    Quaternion targetQuat = Quaternion.identity;
    Quaternion startQuat = Quaternion.identity;
    Quaternion lastQuat = Quaternion.identity;
    float targetAngle = float.MaxValue;

    void Awake()
    {
        this.enabled = false;
    }

    void Start()
    {
        curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0, 0.1f, 0, 2));
        curve.AddKey(new Keyframe(1, 1, 0, 1));
    }

    void OnEnable()
    {
        targetAngle = float.MaxValue;
        targetQuat = HeadNode.rotation;
        startQuat = HeadNode.rotation;
        lastQuat = HeadNode.rotation;
    }

    void LateUpdate()
    {
        if (EyeNode == null)
        {
            EyeNode = HeadNode;
        }

        if (HeadNode != null &&
            LookTarget != null)
        {
            Vector3 lookDirection = LookTarget.position - EyeNode.position;
            lookDirection.Normalize();

            float discard_angle = Vector3.Angle(lookDirection, transform.forward);
            //if (discard_angle >= LookAngleMax)
            //{
            //    //Debug.LogFormat("Discard Look Angle: {0}", discard_angle);
            //    //return;
            //    if (discard_angle >= LookAngleMax + 2)
            //    {
            //        discard_angle = 0;
            //        lookDirection = transform.forward;
            //    }
            //}
            if (Mathf.Abs(targetAngle - discard_angle) > 0.5f)
            {
                targetAngle = discard_angle;
                // interp
                float clamp = 1.0f - (LookAngleMax / 180.0f);
                float angle = Vector3.Angle(lookForward, lookDirection);
                float dot = 1.0f - (angle / 180.0f);

                float targetClampMlp = Mathf.Clamp01(1f - ((clamp - dot) / (1f - dot)));
                float clampMlp = Mathf.Clamp01(dot / clamp);
                float t = clampMlp * targetClampMlp;
                Vector3 targetDirection = Vector3.Slerp(lookForward, lookDirection, t);

                Quaternion fromTo = Quaternion.FromToRotation(lookForward, targetDirection);
                targetQuat = fromTo * HeadNode.rotation;
                startQuat = lastQuat;

                elapse = 0;
            }
            else
            {
                if (elapse < time)
                {
                    elapse += Time.deltaTime;
                }
            }

            //float t2 = Mathf.Clamp01(1 * (elapse / time));
            float t3 = curve.Evaluate(elapse / time);
            lastQuat = Quaternion.Lerp(startQuat, targetQuat, t3);
            HeadNode.rotation = lastQuat;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(SimpleLookIK2))]
public class SimpleLookIK2Ed : Editor
{
    void OnSceneGUI()
    {
        SimpleLookIK2 ik = target as SimpleLookIK2;
        if (ik.HeadNode != null)
        {
            Handles.color = Color.blue;
            Handles.DrawLine(ik.HeadNode.position, ik.HeadNode.position + ik.lookForward);
        }
    }
}

#endif