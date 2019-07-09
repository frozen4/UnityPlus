using CinemaDirector.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CinemaDirector
{
    [Serializable, CutsceneItemAttribute("Curve Clip", "Actor Curve Clip", CutsceneItemGenre.CurveClipItem)]
    public class CinemaActorClipCurve : CinemaClipCurve, IRevertable
    {
        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

        [SerializeField]
        [HideInInspector]
        public StartInfoType _StartInfoType = StartInfoType.Common;

        [SerializeField]
        [HideInInspector]
        public EndInfoType _EndInfoType = EndInfoType.Common;

        public GameObject GetActor()
        {
            GameObject actor = null;
            if (transform.parent != null)
            {
                CurveTrack track = transform.parent.GetComponentInParent<CurveTrack>();
                if (track != null)
                {
                    Transform actorTransform = track.GetActor();
                    if (actorTransform != null)
                        actor = actorTransform.gameObject;
                }
            }
            return actor;
        }

        protected override bool initializeClipCurves(MemberClipCurveData data, Component component)
        {
            object value = data.GetCurrentValue(component);
            PropertyTypeInfo typeInfo = data.PropertyType;
            float startTime = Firetime;
            float endTime = Firetime + Duration;

            if (typeInfo == PropertyTypeInfo.Int || typeInfo == PropertyTypeInfo.Long || typeInfo == PropertyTypeInfo.Float || typeInfo == PropertyTypeInfo.Double)
            {
                float x;
                float.TryParse(value.ToString(), out x);

                if (float.IsInfinity(x) || float.IsNaN(x))
                    return false;

                data.Curve1 = AnimationCurve.Linear(startTime, x, endTime, x);
            }
            else if (typeInfo == PropertyTypeInfo.Vector2)
            {
                Vector2 vec2 = (Vector2)value;

                if (float.IsInfinity(vec2.x) || float.IsNaN(vec2.x) ||
                    float.IsInfinity(vec2.y) || float.IsNaN(vec2.y))
                    return false;

                data.Curve1 = AnimationCurve.Linear(startTime, vec2.x, endTime, vec2.x);
                data.Curve2 = AnimationCurve.Linear(startTime, vec2.y, endTime, vec2.y);
            }
            else if (typeInfo == PropertyTypeInfo.Vector3)
            {
                Vector3 vec3 = (Vector3)value;

                if (float.IsInfinity(vec3.x) || float.IsNaN(vec3.x) ||
                    float.IsInfinity(vec3.y) || float.IsNaN(vec3.y) ||
                    float.IsInfinity(vec3.z) || float.IsNaN(vec3.z))
                    return false;

                data.Curve1 = AnimationCurve.Linear(startTime, vec3.x, endTime, vec3.x);
                data.Curve2 = AnimationCurve.Linear(startTime, vec3.y, endTime, vec3.y);
                data.Curve3 = AnimationCurve.Linear(startTime, vec3.z, endTime, vec3.z);
            }
            else if (typeInfo == PropertyTypeInfo.Vector4)
            {
                Vector4 vec4 = (Vector4)value;

                if (float.IsInfinity(vec4.x) || float.IsNaN(vec4.x) ||
                    float.IsInfinity(vec4.y) || float.IsNaN(vec4.y) ||
                    float.IsInfinity(vec4.z) || float.IsNaN(vec4.z) ||
                    float.IsInfinity(vec4.w) || float.IsNaN(vec4.w))
                    return false;

                data.Curve1 = AnimationCurve.Linear(startTime, vec4.x, endTime, vec4.x);
                data.Curve2 = AnimationCurve.Linear(startTime, vec4.y, endTime, vec4.y);
                data.Curve3 = AnimationCurve.Linear(startTime, vec4.z, endTime, vec4.z);
                data.Curve4 = AnimationCurve.Linear(startTime, vec4.w, endTime, vec4.w);
            }
            else if (typeInfo == PropertyTypeInfo.Quaternion)
            {
                Quaternion quaternion = (Quaternion)value;

                if (float.IsInfinity(quaternion.x) || float.IsNaN(quaternion.x) ||
                    float.IsInfinity(quaternion.y) || float.IsNaN(quaternion.y) ||
                    float.IsInfinity(quaternion.z) || float.IsNaN(quaternion.z) ||
                    float.IsInfinity(quaternion.w) || float.IsNaN(quaternion.w))
                    return false;

                data.Curve1 = AnimationCurve.Linear(startTime, quaternion.x, endTime, quaternion.x);
                data.Curve2 = AnimationCurve.Linear(startTime, quaternion.y, endTime, quaternion.y);
                data.Curve3 = AnimationCurve.Linear(startTime, quaternion.z, endTime, quaternion.z);
                data.Curve4 = AnimationCurve.Linear(startTime, quaternion.w, endTime, quaternion.w);
            }
            else if (typeInfo == PropertyTypeInfo.Color)
            {
                Color color = (Color)value;

                if (float.IsInfinity(color.r) || float.IsNaN(color.r) ||
                    float.IsInfinity(color.g) || float.IsNaN(color.g) ||
                    float.IsInfinity(color.b) || float.IsNaN(color.b) ||
                    float.IsInfinity(color.a) || float.IsNaN(color.a))
                    return false;

                data.Curve1 = AnimationCurve.Linear(startTime, color.r, endTime, color.r);
                data.Curve2 = AnimationCurve.Linear(startTime, color.g, endTime, color.g);
                data.Curve3 = AnimationCurve.Linear(startTime, color.b, endTime, color.b);
                data.Curve4 = AnimationCurve.Linear(startTime, color.a, endTime, color.a);
            }

            return true;
        }

        public override void Initialize()
        {
            GameObject actor = GetActor();
            for (int i = 0; i < CurveData.Count; i++)
            {
                SetStartInfo(CurveData[i]);
                SetEndInfo(CurveData[i]);
                CurveData[i].Initialize(actor);
            }
        }

        private void SetStartInfo(MemberClipCurveData data)
        {
            if (_StartInfoType != StartInfoType.Common)
            {
                Keyframe kf1 = new Keyframe();
                Keyframe kf2 = new Keyframe();
                Keyframe kf3 = new Keyframe();
                kf1.time = 0;
                kf2.time = 0;
                kf3.time = 0;
                Transform trans = null;
                if (_StartInfoType == StartInfoType.HostPlayer)
                    trans = Main.HostPalyer;
                else
                    trans = Main.Main3DCamera.transform;
                if (data._CurveDataType == CurveDataType.Transform)
                {
                    if (data._CurveDataPropertyName == CurveDataPropertyName.Scale)
                    {
                        kf1.value = trans.localScale.x;
                        kf2.value = trans.localScale.y;
                        kf3.value = trans.localScale.z;
                    }
                    else if (data._CurveDataPropertyName == CurveDataPropertyName.Rotation)
                    {
                        kf1.value = trans.localEulerAngles.x;
                        kf2.value = trans.localEulerAngles.y;
                        kf3.value = trans.localEulerAngles.z;
                    }
                    else
                    {
                        kf1.value = trans.position.x;
                        kf2.value = trans.position.y;
                        kf3.value = trans.position.z;
                    }
                }
                data.Curve1.MoveKey(0, kf1);
                data.Curve2.MoveKey(0, kf2);
                data.Curve3.MoveKey(0, kf3);
            }
        }

        private void SetEndInfo(MemberClipCurveData data)
        {
            if (_EndInfoType != EndInfoType.Common)
            {
                Keyframe kf1 = new Keyframe();
                Keyframe kf2 = new Keyframe();
                Keyframe kf3 = new Keyframe();
                int length1 = data.Curve1.length;
                int length2 = data.Curve2.length;
                int length3 = data.Curve3.length;
                kf1.time = data.Curve1.keys[length1 - 1].time;
                kf2.time = data.Curve2.keys[length2 - 1].time;
                kf3.time = data.Curve3.keys[length3 - 1].time;
                Transform trans = null;
                if (_EndInfoType == EndInfoType.HostPlayer)
                    trans = Main.HostPalyer;
                else
                    trans = Main.Main3DCamera.transform;
                if (data._CurveDataType == CurveDataType.Transform)
                {
                    if (data._CurveDataPropertyName == CurveDataPropertyName.Scale)
                    {
                        kf1.value = trans.localScale.x;
                        kf2.value = trans.localScale.y;
                        kf3.value = trans.localScale.z;
                    }
                    else if (data._CurveDataPropertyName == CurveDataPropertyName.Rotation)
                    {
                        kf1.value = trans.localEulerAngles.x;
                        kf2.value = trans.localEulerAngles.y;
                        kf3.value = trans.localEulerAngles.z;
                    }
                    else
                    {
                        kf1.value = trans.position.x;
                        kf2.value = trans.position.y;
                        kf3.value = trans.position.z;
                    }
                }
                data.Curve1.MoveKey(length1 - 1, kf1);
                data.Curve2.MoveKey(length2 - 1, kf2);
                data.Curve3.MoveKey(length3 - 1, kf3);
            }
        }

        /// <summary>
        /// Cache the initial state of the curve clip manipulated values.
        /// </summary>
        /// <returns>The Info necessary to revert this event.</returns>
        public List<RevertInfo> CacheState()
        {
            List<RevertInfo> reverts = new List<RevertInfo>();

            GameObject actor = GetActor();

            if (actor != null)
            {
                for (int i = 0; i < CurveData.Count; i++)
                {
                    RevertInfo info = null;
                    switch (CurveData[i]._CurveDataType)
                    {
                        case CurveDataType.Transform:
                            Transform trans = actor.GetComponent<Transform>();
                            if (!trans)
                                continue;
                            switch (CurveData[i]._CurveDataPropertyName)
                            {
                                case CurveDataPropertyName.Position:
                                    info = new RevertInfo(this, trans, trans.localPosition, RevertType.Transform, RevertValueType.LocalPosition);
                                    break;
                                case CurveDataPropertyName.Rotation:
                                    info = new RevertInfo(this, trans, trans.localEulerAngles, RevertType.Transform, RevertValueType.LocalEulerAngles);
                                    break;
                                case CurveDataPropertyName.Scale:
                                    info = new RevertInfo(this, trans, trans.localScale, RevertType.Transform, RevertValueType.Scale);
                                    break;
                            }
                            break;
                        case CurveDataType.Camera:
                            Camera cam = actor.GetComponent<Camera>();
                            if (!cam)
                                continue;
                            switch (CurveData[i]._CurveDataPropertyName)
                            {
                                case CurveDataPropertyName.Fov:
                                    info = new RevertInfo(this, cam, cam.fieldOfView, RevertType.Camera, RevertValueType.Fov);
                                    break;
                                case CurveDataPropertyName.Near:
                                    info = new RevertInfo(this, cam, cam.nearClipPlane, RevertType.Camera, RevertValueType.Near);
                                    break;
                                case CurveDataPropertyName.Far:
                                    info = new RevertInfo(this, cam, cam.farClipPlane, RevertType.Camera, RevertValueType.Far);
                                    break;
                            }
                            break;
                    }
                    if (info != null)
                        reverts.Add(info);
                }
            }

            return reverts;
        }

        /// <summary>
        /// Sample the curve clip at the given time.
        /// </summary>
        /// <param name="time">The time to evaulate for.</param>
        public void SampleTime(float time)
        {
            GameObject actor = GetActor();
            if (actor == null) return;
            if (Firetime <= time && time <= Firetime + Duration)
            {
                for (int i = 0; i < CurveData.Count; i++)
                {
                    if (CurveData[i] == null)
                    {
                        Common.HobaDebuger.LogErrorFormat("CurveData is null: {0}", i);
                    }

                    Component component = null;
                    switch (CurveData[i]._CurveDataType)
                    {
                        case CurveDataType.Transform:
                            component = actor.GetComponent<Transform>();
                            break;
                        case CurveDataType.Camera:
                            component = actor.GetComponent<Camera>();
                            break;
                    }
                    if (component == null) return;

                    object value = evaluate(CurveData[i], time);

                    switch (CurveData[i]._CurveDataPropertyName)
                    {
                        case CurveDataPropertyName.Position:
                            component.transform.localPosition = (Vector3)value;
                            break;
                        case CurveDataPropertyName.Rotation:
                            component.transform.localEulerAngles = (Vector3)value;
                            break;
                        case CurveDataPropertyName.Scale:
                            component.transform.localScale = (Vector3)value;
                            break;
                        case CurveDataPropertyName.Fov:
                            var cameraCache = component as Camera;
                            cameraCache.fieldOfView = (float)value;
                            break;
                    }

                }
            }
        }

        internal void Reset()
        {
            //foreach (MemberClipCurveData memberData in CurveData)
            //{
            //    memberData.Reset(Actor);
            //}
        }

        /// <summary>
        /// Option for choosing when this curve clip will Revert to initial state in Editor.
        /// </summary>
        public RevertMode EditorRevertMode
        {
            get { return editorRevertMode; }
            set { editorRevertMode = value; }
        }

        /// <summary>
        /// Option for choosing when this curve clip will Revert to initial state in Runtime.
        /// </summary>
        public RevertMode RuntimeRevertMode
        {
            get { return runtimeRevertMode; }
            set { runtimeRevertMode = value; }
        }
    }

    public enum StartInfoType
    {
        Common,
        HostPlayer,
        MainCamera,
    }

    public enum EndInfoType
    {
        Common,
        HostPlayer,
        MainCamera,
    }

}