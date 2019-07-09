using UnityEngine;

public class CameraBlackMask : MonoBehaviour
{
    private float _FadeinDuration = 0;
    private float _KeepDuration = 0;
    private float _FadeoutDuration = 0;
    private float _Duration = 0;
    private float _StartTime = 0;
    private GameObject _BlackMaskGameObject = null;
    private Material _TargetMat = null;
    public Color _StartColor = new Color(0, 0, 0, 0);
    public Color _EndColor = new Color(0, 0, 0, 1.0f);
    public void SetData(float fade_in, float keep, float fade_out, float r, float g, float b, float a)
    {
        _FadeinDuration = fade_in / 1000;
        _KeepDuration = keep / 1000;
        _FadeoutDuration = fade_out / 1000;
        _Duration = _FadeinDuration + _KeepDuration + _FadeoutDuration;
        _StartTime = Time.time;
        _EndColor = new Color((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255);
    }

    void OnDisable()
    {
        if (null != _BlackMaskGameObject && _BlackMaskGameObject.activeSelf)
        {
            _BlackMaskGameObject.SetActive(false);
        }
    }
    
    void OnEnable()
    {
        if (_BlackMaskGameObject == null)
        {
            var obj = Resources.Load<GameObject>("HideGeo");
            _BlackMaskGameObject = Instantiate(obj);
            _BlackMaskGameObject.transform.parent = Main.Main3DCamera.transform;
            _BlackMaskGameObject.transform.localPosition = new Vector3(0, 0, 0.3f);
            _BlackMaskGameObject.transform.localRotation = Quaternion.identity;
        }

        if (null != _BlackMaskGameObject && !_BlackMaskGameObject.activeSelf)
        {
            _BlackMaskGameObject.SetActive(true);
        }
        _TargetMat = _BlackMaskGameObject.GetComponent<Renderer>().material;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= (_StartTime + _Duration))
        {
            _TargetMat.SetColor(ShaderIDs.GhostEffectColor, _StartColor);
            if (null != _BlackMaskGameObject && !_BlackMaskGameObject.activeSelf)
                _BlackMaskGameObject.SetActive(true);
            enabled = false;
            return;
        }

        Color tempColor = _StartColor;
        //fade in 
        if (Time.time < _StartTime + _FadeinDuration)
            tempColor = Color.Lerp(_StartColor, _EndColor, (Time.time - _StartTime) / _FadeinDuration);
        //keep
        else if (Time.time >= (_StartTime+ _FadeinDuration)&& Time.time <= (_StartTime + _FadeinDuration + _KeepDuration))
            tempColor = Color.Lerp(_StartColor, _EndColor, (Time.time - _StartTime) / _FadeinDuration);
        //fade out
        else if (Time.time > (_StartTime + _FadeinDuration + _KeepDuration) && Time.time < (_StartTime + _Duration))
            tempColor = Color.Lerp(_EndColor,  _StartColor, (Time.time - (_StartTime + _FadeinDuration + _KeepDuration)) / _FadeoutDuration);            

        _TargetMat.SetColor(ShaderIDs.GhostEffectColor, tempColor);
    }
}

