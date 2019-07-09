#if PLATFORM_KAKAO
using System;
using System.Collections.Generic;

namespace Kakaogame.SDK.Editor
{
	public class ZinnyConfigurationBase
	{
		public string appId { get; set; }
		public string appSecret { get; set; }
		public string market { get; set; }
		public string appVersion { get; set; }
		public string debugLevel { get; set; }
		public string serverType { get; set; }
	}
	public partial class ZinnyConfigurationAndroid
	{
		public ZinnyConfigurationBase config { get; set; }

		public ZinnyConfigurationAndroid() {
			config = new ZinnyConfigurationBase ();
		}
	}
	
    public partial class ZinnyValues
    {
        public List<string> permissions { get; set; }
    }
    public partial class Permissions
	{
		public string platforms { get; set; }
		public string packageName { get; set; }
        public List<string> permissions { get; set; }
    }
	public partial class IDPKakao
	{
		public string appKey { get; set; }
		public string appSecret { get; set; }
	}
    public partial class IDPReachKakao
    {
        public string appKey { get; set; }
    }
	public partial class IDPFacebook
	{
		public string appKey { get; set; }

        public List<string> permissions { get; set; }
    }
    public partial class IDPGoogle
    {
		public string appId { get; set; }
        public string webappClientId { get; set; }
        public List<string> permissions { get; set; }
    }
	
	public partial class ManifestPush
	{
		public string packageName { get; set; }
	}
	public partial class ManifestKakao
	{
		public string clientId { get; set; }
	}
	public partial class ManifestForKakao
	{
		public string clientId { get; set; }
	}
	
	public partial class AndroidManifest
	{
		private List<string> _receiverNames;
		public List<string> receiverNames
		{
			get
			{
				return _receiverNames;
			}
			set
			{
				_receiverNames = value;
			}
		}

		private List<string> _receiverValues;
		public List<string> receiverValues
		{
			get
			{
				return _receiverValues;
			}
			set
			{
				_receiverValues = value;
			}
		}

		private string _packageName;
		public string packageName
		{
			get
			{
				return _packageName;
			}
			set
			{
				_packageName = push.packageName = permissions.packageName = value;
			}
		}
		
		private string _platforms;
		public string platforms
		{
			get
			{
				return _platforms;
			}
			set
			{
				_platforms = permissions.platforms = value;
			}
		}

		private string _forKakaoClientId;
		public string forKakaoClientId
		{
			get
			{
				return _forKakaoClientId;
			}
			set
			{
				var v = value;

				if(value == null)
					v = "";

                _forKakaoClientId = kakao.clientId = v;
			}
		}

		//public Receivers 
		public Permissions permissions { get; set; }
        public ManifestGoogle google { get; set; }
		public ManifestKakao kakao { get; set; }
		public ManifestPush push { get; set; }
        public ManifestFacebook facebook { get; set; }

		public AndroidManifest() {
			permissions = new Permissions ();
			kakao = new ManifestKakao ();
			push = new ManifestPush ();
            facebook = new ManifestFacebook();
            google = new ManifestGoogle();
		}
	}
}
#endif
