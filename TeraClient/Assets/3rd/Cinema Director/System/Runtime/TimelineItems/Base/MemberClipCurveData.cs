using UnityEngine;

namespace CinemaDirector
{
    public enum CurveDataType
    {
        Transform,
        Camera,
        Particle,
    }
    public enum CurveDataPropertyName
    {
        Position,
        Rotation,
        Scale,
        Fov,
        Near,
        Far,
    }
    [System.Serializable]
    public class MemberClipCurveData
    {
        public CurveDataType _CurveDataType = CurveDataType.Transform;
        public CurveDataPropertyName _CurveDataPropertyName = CurveDataPropertyName.Position;
        public PropertyTypeInfo PropertyType = PropertyTypeInfo.None;

        public AnimationCurve Curve1 = new AnimationCurve();
        public AnimationCurve Curve2 = new AnimationCurve();
        public AnimationCurve Curve3 = new AnimationCurve();
        public AnimationCurve Curve4 = new AnimationCurve();

        public AnimationCurve GetCurve(int i)
        {
            if (i == 1) return Curve2;
            else if (i == 2) return Curve3;
            else if (i == 3) return Curve4;
            else return Curve1;
        }

        public void SetCurve(int i, AnimationCurve newCurve)
        {
            if (i == 1)
            {
                Curve2 = newCurve;
            }
            else if (i == 2)
            {
                Curve3 = newCurve;
            }
            else if (i == 3)
            {
                Curve4 = newCurve;
            }
            else
            {
                Curve1 = newCurve;
            }
        }

        public void Initialize(GameObject Actor)
        {
        }

        internal void Reset(GameObject Actor)
        {
        }

        internal object GetCurrentValue(Component component)
        {
            object value = null;
            switch (_CurveDataType)
            {
                case CurveDataType.Transform:
                    switch (_CurveDataPropertyName)
                    {
                        case CurveDataPropertyName.Position:
                            value = component.transform.localPosition;
                            break;
                        case CurveDataPropertyName.Rotation:
                            value = component.transform.localEulerAngles;
                            break;
                        case CurveDataPropertyName.Scale:
                            value = component.transform.localScale;
                            break;
                    }
                    break;
                case CurveDataType.Camera:
                    Camera camera = component as Camera;
                    switch (_CurveDataPropertyName)
                    {
                        case CurveDataPropertyName.Fov:
                            value = camera.fieldOfView;
                            break;
                        case CurveDataPropertyName.Near:
                            value = camera.nearClipPlane;
                            break;
                        case CurveDataPropertyName.Far:
                            value = camera.farClipPlane;
                            break;
                    }
                    break;
                case CurveDataType.Particle:
                    break;
                default:
                    Debug.LogError("GetCurrentValue Unknown CurveDataType!");
                    break;
            }

            return value;
        }

    }
}