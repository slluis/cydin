using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cydin.Models;

namespace Cydin.Properties
{
	[DataType ("Configuration")]
	public class Settings
	{
		static Settings defaultInstance;
		
		public const string CydinVersion = "0.1";
		
        public static Settings Default {
            get {
				if (defaultInstance == null)
					defaultInstance = UserModel.GetSettings ();
                return defaultInstance;
            }
			set {
				defaultInstance = value;
			}
        }
		
		public static string BasePath { get; set; }
		
        [DataMember (DefaultValue = "..\\Files")]
        public string DataPath { get; set; }
        
        [DataMember (DefaultValue = "127.0.0.1")]
		public string BuildServiceAddress { get; set; }
		
        [DataMember (DefaultValue = false)]
		public bool AllowChangingService { get; set; }
		
        [DataMember]
		public string SmtpHost { get; set; }
		
        [DataMember (DefaultValue = 25)]
		public int SmtpPort { get; set; }
		
        [DataMember (DefaultValue = false)]
		public bool SmtpUseSSL { get; set; }
		
        [DataMember]
		public string SmtpUser { get; set; }
		
        [DataMember]
		public string SmtpPassword { get; set; }
		
        [DataMember]
		public string WebSiteHost { get; set; }
		
        [DataMember]
		public string PreviousWebSiteHost { get; set; }
		
		public string WebSiteUrl {
			get { return "http://" + WebSiteHost; }
		}
		
        [DataMember (DefaultValue = OperationMode.NotSet)]
		public OperationMode OperationMode { get; set; }
		
		public bool SupportsMultiApps {
			get { return OperationMode == OperationMode.MultiAppDomain || OperationMode == OperationMode.MultiAppPath; }
		}
		
        [DataMember (DefaultValue = true)]
		public bool InitialConfiguration { get; set; }
		
		public void Save ()
		{
			UserModel.GetCurrent ().UpdateSettings (this);
		}
	}
	
	public enum OperationMode {
		NotSet = 0,
		SingleApp = 1,
		MultiAppDomain = 2,
		MultiAppPath = 3
	}
}