using UnityEngine;

namespace CinemaDirector.Helpers
{

    public enum RevertType
    {
        None,
        GameObject,
        Transform,
        Camera,
    }

    public enum RevertValueType
    {
        None,
        Position,
        LocalPosition,
        Rotation,
        LocalEulerAngles,
        Scale,
        Parent,
        Fov,
        Near,
        Far,
        Active,
    }

    /// <summary>
    /// Holds info related to reverting objects to a former state.
    /// </summary>
    public class RevertInfo
    {
        private MonoBehaviour MonoBehaviour;
        private object _Actor;
        private object _Value;
        private RevertType _RevertType = RevertType.None;
        private RevertValueType _RevertValueType = RevertValueType.None;

        public RevertInfo(MonoBehaviour monoBehaviour, object actor,object value, RevertType type, RevertValueType valueType)
        {
            MonoBehaviour = monoBehaviour;
            _Actor = actor;
            _Value = value;
            _RevertType = type;
            _RevertValueType = valueType;

            string name;
            if (actor is UnityEngine.Object)
                name = ((UnityEngine.Object)actor).name;
            else
                name = actor.ToString();
            Common.HobaDebuger.LogWarningFormat("Revert {0}, {1}, {2}, {3}", name, _RevertType, _RevertValueType, _Value);
        }

        /// <summary>
        /// Revert the given object to its former state.
        /// </summary>
        public void Revert()
        {
            if (_Actor == null)
                return;
            switch (_RevertType)
            {
                case RevertType.None:
                    Debug.LogError("RevertType Error!!!");
                    break;
                case RevertType.GameObject:
                    GameObject go = _Actor as GameObject;
                    if (!go)
                        return;
                    switch (_RevertValueType)
                    {
                        case RevertValueType.Active:
                            go.SetActive((bool)_Value);
                            break;
                    }
                    break;
                case RevertType.Transform:
                    Transform trans = _Actor as Transform;
                    if (!trans)
                        return;
                    switch (_RevertValueType)
                    {
                        case RevertValueType.Position:
                            trans.position = (Vector3)_Value;
                            break;
                        case RevertValueType.LocalPosition:
                            trans.localPosition = (Vector3)_Value;
                            break;
                        case RevertValueType.Rotation:
                            trans.rotation = (Quaternion)_Value;
                            break;
                        case RevertValueType.LocalEulerAngles:
                            trans.localEulerAngles = (Vector3)_Value;
                            break;
                        case RevertValueType.Scale:
                            trans.localScale = (Vector3)_Value;
                            break;
                        case RevertValueType.Parent:
                            trans.parent = (Transform)_Value;
                            break;
                    }
                    break;
                case RevertType.Camera:
                    Camera camera = _Actor as Camera;
                    if (!camera)
                        return;
                    switch (_RevertValueType)
                    {
                        case RevertValueType.Fov:
                            camera.fieldOfView = (float)_Value;
                            break;
                        case RevertValueType.Near:
                            camera.nearClipPlane = (float)_Value;
                            break;
                        case RevertValueType.Far:
                            camera.farClipPlane = (float)_Value;
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Should we apply this revert in runtime.
        /// </summary>
        public RevertMode RuntimeRevert
        {
            get
            {
                return (MonoBehaviour as IRevertable).RuntimeRevertMode;
            }
        }

        /// <summary>
        /// Should we apply this revert in the editor.
        /// </summary>
        public RevertMode EditorRevert
        {
            get
            {
                return (MonoBehaviour as IRevertable).EditorRevertMode;
            }
        }
    }
}
