using CinemaDirector.Helpers;
using System;
using System.Collections.Generic;
//using System.Reflection;
using UnityEngine;

namespace CinemaDirector
{
    [Serializable, CutsceneItemAttribute("Curve Clip", "MultiActor Curve Clip", CutsceneItemGenre.MultiActorCurveClipItem)]
    public class CinemaMultiActorCurveClip : CinemaClipCurve, IRevertable
    {
        // Options for reverting in editor.
        [SerializeField]
        private RevertMode editorRevertMode = RevertMode.Revert;

        // Options for reverting during runtime.
        [SerializeField]
        private RevertMode runtimeRevertMode = RevertMode.Revert;

        //public List<string> Components = new List<string>();
        //public List<string> Properties = new List<string>();

        public CinemaMultiActorCurveClip()
        {
            CurveData.Add(new MemberClipCurveData());
        }

        public void SampleTime(float time)
        {
            //List<Transform> actors = GetActors();

            //if (Firetime <= time && time <= Firetime + Duration)
            //{
            //    MemberClipCurveData data = CurveData[0];
            //    if (data == null) return;

            //    if (data.PropertyType == PropertyTypeInfo.None)
            //    {
            //        return;
            //    }

            //    for (int i = 0; i < Components.Count; i++)
            //    {
            //        object value = null;
            //        switch (data.PropertyType)
            //        {
            //            case PropertyTypeInfo.Color:
            //                Color c;
            //                c.r = data.Curve1.Evaluate(time);
            //                c.g = data.Curve2.Evaluate(time);
            //                c.b = data.Curve3.Evaluate(time);
            //                c.a = data.Curve4.Evaluate(time);
            //                value = c;
            //                break;

            //            case PropertyTypeInfo.Double:
            //            case PropertyTypeInfo.Float:
            //            case PropertyTypeInfo.Long:
            //                value = data.Curve1.Evaluate(time);
            //                break;
            //            case PropertyTypeInfo.Int:
            //                value = Mathf.RoundToInt(data.Curve1.Evaluate(time));
            //                break;
            //            case PropertyTypeInfo.Quaternion:
            //                Quaternion q;
            //                q.x = data.Curve1.Evaluate(time);
            //                q.y = data.Curve2.Evaluate(time);
            //                q.z = data.Curve3.Evaluate(time);
            //                q.w = data.Curve4.Evaluate(time);
            //                value = q;
            //                break;

            //            case PropertyTypeInfo.Vector2:
            //                Vector2 v2;
            //                v2.x = data.Curve1.Evaluate(time);
            //                v2.y = data.Curve2.Evaluate(time);
            //                value = v2;
            //                break;

            //            case PropertyTypeInfo.Vector3:
            //                Vector3 v3;
            //                v3.x = data.Curve1.Evaluate(time);
            //                v3.y = data.Curve2.Evaluate(time);
            //                v3.z = data.Curve3.Evaluate(time);
            //                value = v3;
            //                break;

            //            case PropertyTypeInfo.Vector4:
            //                Vector4 v4;
            //                v4.x = data.Curve1.Evaluate(time);
            //                v4.y = data.Curve2.Evaluate(time);
            //                v4.z = data.Curve3.Evaluate(time);
            //                v4.w = data.Curve4.Evaluate(time);
            //                value = v4;
            //                break;
            //        }
            //        if (Components[i] != null && Properties[i] != null && Properties[i] != "None")
            //        {
            //            Component component = actors[i].GetComponent(Components[i]);
            //            PropertyInfo propertyInfo = ReflectionHelper.GetProperty(component.GetType(), Properties[i]);
            //            propertyInfo.SetValue(component, value, null);
            //        }
            //    }
            //}
        }

        //public List<Transform> GetActors()
        //{
        //    List<Transform> actors;
        //    if (transform.parent != null)
        //    {
        //        MultiCurveTrack track = transform.parent.GetComponentInParent<MultiCurveTrack>();
        //        if(track == null)
        //        {
        //            MultiActorTrackGroup trackgroup = (track.GetTrackGroup() as MultiActorTrackGroup);
        //            if (trackgroup != null)
        //                actors = trackgroup.Actors;
        //            else
        //                actors = new List<Transform>();
        //        }
        //        else
        //        {
        //            actors = new List<Transform>();
        //        }
        //    }
        //    else 
        //    {
        //        actors = new List<Transform>();
        //    }
        //    return actors;
        //}


        /// <summary>
        /// Cache the initial state of the curve clip manipulated values.
        /// </summary>
        /// <returns>The Info necessary to revert this clip.</returns>
        public List<RevertInfo> CacheState()
        {
            List<RevertInfo> reverts = new List<RevertInfo>();

            //List<Transform> actors = GetActors();
            //for (int i = 0; i < actors.Count; i++)
            //{
            //    if (i >= Components.Count || i >= Properties.Count)
            //        continue;

            //    if (Components[i] != null && Properties[i] != null && Properties[i] != "None")
            //    {
            //        RevertInfo info = null;
            //        switch (CurveData[0]._CurveDataType)
            //        {
            //            case CurveDataType.Transform:
            //                Transform trans = actors[i];
            //                if (!trans)
            //                    continue;
            //                switch (CurveData[0]._CurveDataPropertyName)
            //                {
            //                    case CurveDataPropertyName.Position:
            //                        info = new RevertInfo(this, trans, trans.localPosition, RevertType.Transform, RevertValueType.LocalPosition);
            //                        break;
            //                    case CurveDataPropertyName.Rotation:
            //                        info = new RevertInfo(this, trans, trans.localEulerAngles, RevertType.Transform, RevertValueType.LocalEulerAngles);
            //                        break;
            //                    case CurveDataPropertyName.Scale:
            //                        info = new RevertInfo(this, trans, trans.localScale, RevertType.Transform, RevertValueType.Scale);
            //                        break;
            //                }
            //                break;
            //            case CurveDataType.Camera:
            //                Camera cam = actors[i].GetComponent<Camera>();
            //                if (!cam)
            //                    continue;
            //                switch (CurveData[0]._CurveDataPropertyName)
            //                {
            //                    case CurveDataPropertyName.Fov:
            //                        info = new RevertInfo(this, cam, cam.fieldOfView, RevertType.Camera, RevertValueType.Fov);
            //                        break;
            //                    case CurveDataPropertyName.Near:
            //                        info = new RevertInfo(this, cam, cam.nearClipPlane, RevertType.Camera, RevertValueType.Near);
            //                        break;
            //                    case CurveDataPropertyName.Far:
            //                        info = new RevertInfo(this, cam, cam.farClipPlane, RevertType.Camera, RevertValueType.Far);
            //                        break;
            //                }
            //                break;
            //        }
            //        if (info != null)
            //            reverts.Add(info);
            //    }
            //}

            return reverts;
        }

        internal void Revert()
        {
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
}