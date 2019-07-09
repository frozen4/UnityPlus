using UnityEngine;
using System.Collections;
using System.IO;
using System;

#if USING_LGVC

using LGVC.VoiceMessage;

namespace LGVC
{
    public enum ErrorCode
    {
        Succeed = 0,
        WWWError,
        AudioClipError,
        FormatError,
    }

    public delegate void UploadReccordFileCompleteHandler(int code, string errMsg, string filepath, string fileid);

    public delegate void DownloadRecordFileCompleteHandler(int code, string errMsg, string filepath, string fileid);

    public delegate void PlayRecordFileCompleteHandler(int code, string errMsg, string filepath);

    public class LGVC_VoiceMan : MonoBehaviour
    {
        public event PlayRecordFileCompleteHandler OnPlayRecordFileComplete;

        //
        private Storage _Storage;

        private AudioSource _AudioSource;

        private int MaxRecordingTimeInSeconds = 60;
        private int SampleRate = 16000;

        private Action<int> _RecordingCompleteCallback = null;

        private Coroutine _DownloadingCoroutine = null;
        private Coroutine _UploadingCoroutine = null;
        private Coroutine _RecordingCoroutine = null;
        private Coroutine _LoadAudioCoroutine = null;

        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public bool Init()
        {
            _Storage = GetComponent<Storage>();
            if (_Storage == null)
                _Storage = gameObject.AddComponent<Storage>();

            _AudioSource = GetComponent<AudioSource>();
            if (_AudioSource == null)
                _AudioSource = gameObject.AddComponent<AudioSource>();

            if (_Storage == null || _AudioSource == null)
                return false;

            _Storage.ServerAddr = EntryPoint.Instance.GameCustomConfigParams.LGVC_ServerAddr;
            _Storage.AppName = EntryPoint.Instance.GameCustomConfigParams.LGVC_AppName;
            _Storage.UserKey = EntryPoint.Instance.GameCustomConfigParams.LGVC_UserKey;

            _AudioSource.playOnAwake = false;

            Recorder.RecordResultEvent += OnRecordComplete;

            return true;
        }

        void OnDisable()
        {
            Recorder.RecordResultEvent -= OnRecordComplete;
        }

        public Storage Storage
        {
            get { return _Storage; }
        }

        void OnRecordComplete(int errorcode, string filePath, AudioFormat audioFormat, float audioLength)
        {
            VoiceManager.Instance.TempVoiceLength = audioLength;
            if (_RecordingCompleteCallback != null)
                _RecordingCompleteCallback(errorcode);
        }

        public AudioClip GetDownloadedAudioClilp()
        {
            return _Storage.DownloadedAudioClip;
        }

        public int DownloadRecordedFile(string fileid, string filePath)
        {
            if (_DownloadingCoroutine != null)
                StopCoroutine(_DownloadingCoroutine);
            _DownloadingCoroutine = StartCoroutine(_Storage.DownloadFile(fileid, filePath));
            return 0;
        }

        public int UploadRecordedFile(string filepath)
        {
            if (_UploadingCoroutine != null)
                StopCoroutine(_UploadingCoroutine);
            _UploadingCoroutine = StartCoroutine(_Storage.UploadFile(filepath));
            return 0;
        }

        public int PlayRecordedFile(string downloadFilePath)
        {
            if (_LoadAudioCoroutine != null)
                StopCoroutine(_LoadAudioCoroutine);

            if (!File.Exists(downloadFilePath))         //文件不存在
                return (int)ErrorCode.WWWError;

            AudioType audioType = GetAudioType(downloadFilePath);

            if (audioType != AudioType.WAV)
                return (int)ErrorCode.FormatError;      
            //if (audioType != AudioType.MPEG)
             //   return (int)ErrorCode.FormatError;
            _LoadAudioCoroutine = StartCoroutine(LoadAudioClipCoroutine(_AudioSource, downloadFilePath));

            return 0;
        }

        private IEnumerator LoadAudioClipCoroutine(AudioSource audioSource, string filePath)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                yield return null;
            }

            filePath = filePath.Replace('\\', '/');
            WWW www = new WWW("file://" + filePath);
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
            {
                if (OnPlayRecordFileComplete != null)
                    OnPlayRecordFileComplete.Invoke((int)ErrorCode.WWWError, www.error, filePath);
                yield break;
            }

            AudioType audioType = GetAudioType(filePath);
            AudioClip clip = www.GetAudioClip(false, false, audioType);
            while (clip != null && clip.loadState != AudioDataLoadState.Loaded && clip.loadState != AudioDataLoadState.Failed)
                yield return null;

            if (clip == null || clip.loadState == AudioDataLoadState.Failed)
            {
                if (OnPlayRecordFileComplete != null)
                    OnPlayRecordFileComplete.Invoke((int)ErrorCode.AudioClipError, www.error, filePath);
                yield break;
            }

            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();

            float clipLength = audioSource.clip.length;
            yield return new WaitForSeconds(clipLength);

            if (OnPlayRecordFileComplete != null)
                OnPlayRecordFileComplete.Invoke(0, string.Empty, filePath);
        }

        public int StopPlayFile()
        {
            _AudioSource.Stop();
            return 0;
        }

        public int StartRecording(string recordPath)
        {
//             if (!OSUtility.IsMicrophoneAvailable())
//             {
//                 return (int)Recorder.ERROR_CODE.AudioRecordPermissionNotGranted;
//             }

            _RecordingCompleteCallback = null;
            if (_RecordingCoroutine != null)
                StopCoroutine(_RecordingCoroutine);
            _RecordingCoroutine = StartCoroutine(RecordingCoroutine(recordPath));
            _RecordingCompleteCallback = null;
            return 0;
        }

        private IEnumerator RecordingCoroutine(string recordPath)
        {
            yield return Recorder.RecordWav(recordPath, MaxRecordingTimeInSeconds, SampleRate);
            //yield return Recorder.RecordMp3(recordPath, MaxRecordingTimeInSeconds, SampleRate);
        }

        public int StopRecording(Action<int> callback)
        {
            _RecordingCompleteCallback = callback;
            Recorder.StopRecord();
            return 0;
        }

        public AudioType GetAudioType(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);
            if (string.Compare(ext, ".wav", false) == 0)
                return AudioType.WAV;
            else if (string.Compare(ext, ".mp3", false) == 0)
                return AudioType.MPEG;
            else if (string.Compare(ext, ".ogg") == 0)
                return AudioType.OGGVORBIS;
            else
                return AudioType.UNKNOWN;
        }
    }
}

#endif