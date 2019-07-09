using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using Common;


[RequireComponent(typeof(UniWebView))]	
public class GWebView : MonoBehaviour {
	private bool _isRunWindows = false;
	public bool IsRunWindows { get { return _isRunWindows; } private set{ _isRunWindows = value; } }
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
	private UniWebView _UniWebView;
#endif
	public delegate void LoadCompletedDelegate (bool success, string errorMessage);
	public delegate void LoadBeginDelegate(string loadingUrl);
	public delegate void ReceivedMessageDelegate(string message);
	public delegate void EvalJavaScriptFinishedDelegate(string result);
	[NoToLua]
	public LoadCompletedDelegate OnLoadCompleted;
	[NoToLua]
	public LoadBeginDelegate OnLoadBegin;
	[NoToLua]
	public ReceivedMessageDelegate OnReceiveMessage;
	[NoToLua]
	public EvalJavaScriptFinishedDelegate OnJavaScriptFinished;

	public string URL { 
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		get { 
			return _UniWebView.url; 
		} 
		set { 
			_UniWebView.url = value; 
		}
#else
		get { return ""; }
		set { }
#endif
	}

	public void Init(GameObject go)
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		if (_UniWebView == null)
		{
			_UniWebView = gameObject.GetComponent<UniWebView>();
		}
		RectTransform rt = go.GetComponent<RectTransform>();
		if (rt == null)
			rt = GetComponent<RectTransform>();
		SetViewEdgeInsets(Mathf.Abs((int)rt.offsetMax.y), Mathf.Abs((int)rt.offsetMin.x), Mathf.Abs((int)rt.offsetMin.y), Mathf.Abs((int)rt.offsetMax.x));
		_UniWebView.OnLoadComplete += OnLoadCompletedMethod;
		_UniWebView.OnLoadBegin += OnLoadBeginMethod;
		_UniWebView.OnReceivedMessage += OnReceiveMessageMethod;
		_UniWebView.OnEvalJavaScriptFinished += OnJavaScriptFinishedMethod;
		IsRunWindows = false;
#else
        IsRunWindows = true;
#endif
	}

	public void Load(string url)
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.Load(url);
		_UniWebView.Show();
#endif
	}

	public void LoadHTMLString(string htmlString, string baseUrl)
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.LoadHTMLString(htmlString, baseUrl);
#endif
	}

	public void Reload()
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.Reload();
#endif
	}
	public void Stop()
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.Stop();
#endif
	}
	[NoToLua]
	public void SetViewEdgeInsets(int top, int left, int bottom, int right)
	{
#if !ART_USE
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		Transform root = GameObjectUtil.GetUIRootTranform();
		if (root == null)
		{
			HobaDebuger.LogError("SetViewEdgeInsets error, UIRootTransform is null");
			return;
		}
		RectTransform rectTrans = root.GetComponent<RectTransform>();
		float ratew = (float)Screen.width / (float)rectTrans.rect.width;
		float rateh = (float)Screen.height / (float)rectTrans.rect.height;
		int screenTop = (int)(rateh * top);
		int screenLeft = (int)(ratew * left);
		int screenBottom = (int)(rateh * bottom);
		int screenRight = (int)(ratew * right);
#if UNITY_IOS
        float scale = IOSUtil.getScreenScale();
        screenTop = (int)((float)screenTop / scale);
        screenLeft = (int)((float)screenLeft / scale);
        screenBottom = (int)((float)screenBottom / scale);
        screenRight = (int)((float)screenRight / scale);
#endif
		_UniWebView.insets = new UniWebViewEdgeInsets(screenTop, screenLeft, screenBottom, screenRight);
#endif
#endif
	}

	public void Hide()
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.CleanCache();
		_UniWebView.Hide();
#endif	
	}
	public void CleanCache()
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.CleanCache();
#endif
	}
	public void AddJavaScript(string javaScript)
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.AddJavaScript(javaScript);
#endif
	}
	public void EvaluatingJavaScript(string javaScript)
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		_UniWebView.EvaluatingJavaScript(javaScript);
#endif
	}
	public void SetHeaderField(string key, string value)
	{
#if !ART_USE		
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		if( key != null && key != "")
			_UniWebView.SetHeaderField(key, value);
		else
			HobaDebuger.LogLuaError(@"URL's head key is null or "".");
#endif
#endif
	}
	#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
	[NoToLua]
	public void OnLoadCompletedMethod(UniWebView webView, bool success, string errorMessage)
	{
		if (this.OnLoadCompleted != null)
		{
			this.OnLoadCompleted(success, errorMessage);
		}
	}

	[NoToLua]
	public void OnLoadBeginMethod(UniWebView webView, string loadingUrl)
	{
		if (this.OnLoadBegin != null)
		{
			this.OnLoadBegin(loadingUrl);
		}
	}
	[NoToLua]
	public void OnReceiveMessageMethod(UniWebView webView, UniWebViewMessage message)
	{
		string agrs = "";
		agrs += "path#";
		agrs += message.path + ",";
		foreach (var it in message.args)
		{
			agrs += (it.Key + "#" + it.Value + ",");
		}
		Debug.Log(string.Format("Full OnReceiveMessage is {0}", agrs));
		agrs = agrs.Substring(0, agrs.Length - 1);
		if (this.OnReceiveMessage != null)
		{
			this.OnReceiveMessage(agrs);
		}
		else
		{
			Debug.Log("Unity - OnReceiveMessageMethod 's OnReceiveMessage is null");
		}
	}
	[NoToLua]
	public void OnJavaScriptFinishedMethod(UniWebView webView, string result)
	{
		if (this.OnJavaScriptFinished != null)
		{
			this.OnJavaScriptFinished(result);
		}
	}

#endif

	void OnDestory()
	{
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8
		if (_UniWebView == null) return;
		_UniWebView.OnLoadComplete -= OnLoadCompletedMethod;
		_UniWebView.OnLoadBegin -= OnLoadBeginMethod;
		_UniWebView.OnReceivedMessage -= OnReceiveMessageMethod;
		_UniWebView.OnEvalJavaScriptFinished -= OnJavaScriptFinishedMethod;
        _UniWebView = null;
		//DestroyImmediate(_UniWebView);  //±¨´í Error : Can't remove UniWebView (Script) because GWebView (Script) depends on it
#endif
    }
}
