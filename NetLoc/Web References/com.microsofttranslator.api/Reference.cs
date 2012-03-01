//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

//
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.239.
//
#pragma warning disable 1591

namespace NetLoc.com.microsofttranslator.api {
	using System;
	using System.Web.Services;
	using System.Diagnostics;
	using System.Web.Services.Protocols;
	using System.ComponentModel;
	using System.Xml.Serialization;


	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Web.Services.WebServiceBindingAttribute(Name="BasicHttpBinding_LanguageService", Namespace="http://api.microsofttranslator.com/v1/soap.svc")]
	public partial class Soap : System.Web.Services.Protocols.SoapHttpClientProtocol {

		private System.Threading.SendOrPostCallback GetLanguagesOperationCompleted;

		private System.Threading.SendOrPostCallback GetLanguageNamesOperationCompleted;

		private System.Threading.SendOrPostCallback DetectOperationCompleted;

		private System.Threading.SendOrPostCallback TranslateOperationCompleted;

		private bool useDefaultCredentialsSetExplicitly;

		/// <remarks/>
		public Soap() {
			this.Url = global::NetLoc.Properties.Settings.Default.NetLoc_com_microsofttranslator_api_Soap;
			if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
				this.UseDefaultCredentials = true;
				this.useDefaultCredentialsSetExplicitly = false;
			}
			else {
				this.useDefaultCredentialsSetExplicitly = true;
			}
		}

		public new string Url {
			get {
				return base.Url;
			}
			set {
				if ((((this.IsLocalFileSystemWebService(base.Url) == true)
							&& (this.useDefaultCredentialsSetExplicitly == false))
							&& (this.IsLocalFileSystemWebService(value) == false))) {
					base.UseDefaultCredentials = false;
				}
				base.Url = value;
			}
		}

		public new bool UseDefaultCredentials {
			get {
				return base.UseDefaultCredentials;
			}
			set {
				base.UseDefaultCredentials = value;
				this.useDefaultCredentialsSetExplicitly = true;
			}
		}

		/// <remarks/>
		public event GetLanguagesCompletedEventHandler GetLanguagesCompleted;

		/// <remarks/>
		public event GetLanguageNamesCompletedEventHandler GetLanguageNamesCompleted;

		/// <remarks/>
		public event DetectCompletedEventHandler DetectCompleted;

		/// <remarks/>
		public event TranslateCompletedEventHandler TranslateCompleted;

		/// <remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://api.microsofttranslator.com/v1/soap.svc/LanguageService/GetLanguages", RequestNamespace="http://api.microsofttranslator.com/v1/soap.svc", ResponseNamespace="http://api.microsofttranslator.com/v1/soap.svc", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		[return: System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
		[return: System.Xml.Serialization.XmlArrayItemAttribute(Namespace="http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
		public string[] GetLanguages([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string appId) {
			object[] results = this.Invoke("GetLanguages", new object[] {
						appId});
			return ((string[])(results[0]));
		}

		/// <remarks/>
		public void GetLanguagesAsync(string appId) {
			this.GetLanguagesAsync(appId, null);
		}

		/// <remarks/>
		public void GetLanguagesAsync(string appId, object userState) {
			if ((this.GetLanguagesOperationCompleted == null)) {
				this.GetLanguagesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLanguagesOperationCompleted);
			}
			this.InvokeAsync("GetLanguages", new object[] {
						appId}, this.GetLanguagesOperationCompleted, userState);
		}

		private void OnGetLanguagesOperationCompleted(object arg) {
			if ((this.GetLanguagesCompleted != null)) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
				this.GetLanguagesCompleted(this, new GetLanguagesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
			}
		}

		/// <remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://api.microsofttranslator.com/v1/soap.svc/LanguageService/GetLanguageNames", RequestNamespace="http://api.microsofttranslator.com/v1/soap.svc", ResponseNamespace="http://api.microsofttranslator.com/v1/soap.svc", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		[return: System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
		[return: System.Xml.Serialization.XmlArrayItemAttribute(Namespace="http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
		public string[] GetLanguageNames([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string appId, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string locale) {
			object[] results = this.Invoke("GetLanguageNames", new object[] {
						appId,
						locale});
			return ((string[])(results[0]));
		}

		/// <remarks/>
		public void GetLanguageNamesAsync(string appId, string locale) {
			this.GetLanguageNamesAsync(appId, locale, null);
		}

		/// <remarks/>
		public void GetLanguageNamesAsync(string appId, string locale, object userState) {
			if ((this.GetLanguageNamesOperationCompleted == null)) {
				this.GetLanguageNamesOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetLanguageNamesOperationCompleted);
			}
			this.InvokeAsync("GetLanguageNames", new object[] {
						appId,
						locale}, this.GetLanguageNamesOperationCompleted, userState);
		}

		private void OnGetLanguageNamesOperationCompleted(object arg) {
			if ((this.GetLanguageNamesCompleted != null)) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
				this.GetLanguageNamesCompleted(this, new GetLanguageNamesCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
			}
		}

		/// <remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://api.microsofttranslator.com/v1/soap.svc/LanguageService/Detect", RequestNamespace="http://api.microsofttranslator.com/v1/soap.svc", ResponseNamespace="http://api.microsofttranslator.com/v1/soap.svc", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		[return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
		public string Detect([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string appId, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string text) {
			object[] results = this.Invoke("Detect", new object[] {
						appId,
						text});
			return ((string)(results[0]));
		}

		/// <remarks/>
		public void DetectAsync(string appId, string text) {
			this.DetectAsync(appId, text, null);
		}

		/// <remarks/>
		public void DetectAsync(string appId, string text, object userState) {
			if ((this.DetectOperationCompleted == null)) {
				this.DetectOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDetectOperationCompleted);
			}
			this.InvokeAsync("Detect", new object[] {
						appId,
						text}, this.DetectOperationCompleted, userState);
		}

		private void OnDetectOperationCompleted(object arg) {
			if ((this.DetectCompleted != null)) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
				this.DetectCompleted(this, new DetectCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
			}
		}

		/// <remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://api.microsofttranslator.com/v1/soap.svc/LanguageService/Translate", RequestNamespace="http://api.microsofttranslator.com/v1/soap.svc", ResponseNamespace="http://api.microsofttranslator.com/v1/soap.svc", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		[return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
		public string Translate([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string appId, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string text, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string from, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string to) {
			object[] results = this.Invoke("Translate", new object[] {
						appId,
						text,
						from,
						to});
			return ((string)(results[0]));
		}

		/// <remarks/>
		public void TranslateAsync(string appId, string text, string from, string to) {
			this.TranslateAsync(appId, text, from, to, null);
		}

		/// <remarks/>
		public void TranslateAsync(string appId, string text, string from, string to, object userState) {
			if ((this.TranslateOperationCompleted == null)) {
				this.TranslateOperationCompleted = new System.Threading.SendOrPostCallback(this.OnTranslateOperationCompleted);
			}
			this.InvokeAsync("Translate", new object[] {
						appId,
						text,
						from,
						to}, this.TranslateOperationCompleted, userState);
		}

		private void OnTranslateOperationCompleted(object arg) {
			if ((this.TranslateCompleted != null)) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
				this.TranslateCompleted(this, new TranslateCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
			}
		}

		/// <remarks/>
		public new void CancelAsync(object userState) {
			base.CancelAsync(userState);
		}

		private bool IsLocalFileSystemWebService(string url) {
			if (((url == null)
						|| (url == string.Empty))) {
				return false;
			}
			System.Uri wsUri = new System.Uri(url);
			if (((wsUri.Port >= 1024)
						&& (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
				return true;
			}
			return false;
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	public delegate void GetLanguagesCompletedEventHandler(object sender, GetLanguagesCompletedEventArgs e);

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class GetLanguagesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {

		private object[] results;

		internal GetLanguagesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
				base(exception, cancelled, userState) {
			this.results = results;
		}

		/// <remarks/>
		public string[] Result {
			get {
				this.RaiseExceptionIfNecessary();
				return ((string[])(this.results[0]));
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	public delegate void GetLanguageNamesCompletedEventHandler(object sender, GetLanguageNamesCompletedEventArgs e);

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class GetLanguageNamesCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {

		private object[] results;

		internal GetLanguageNamesCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
				base(exception, cancelled, userState) {
			this.results = results;
		}

		/// <remarks/>
		public string[] Result {
			get {
				this.RaiseExceptionIfNecessary();
				return ((string[])(this.results[0]));
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	public delegate void DetectCompletedEventHandler(object sender, DetectCompletedEventArgs e);

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class DetectCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {

		private object[] results;

		internal DetectCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
				base(exception, cancelled, userState) {
			this.results = results;
		}

		/// <remarks/>
		public string Result {
			get {
				this.RaiseExceptionIfNecessary();
				return ((string)(this.results[0]));
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	public delegate void TranslateCompletedEventHandler(object sender, TranslateCompletedEventArgs e);

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.1")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class TranslateCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {

		private object[] results;

		internal TranslateCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
				base(exception, cancelled, userState) {
			this.results = results;
		}

		/// <remarks/>
		public string Result {
			get {
				this.RaiseExceptionIfNecessary();
				return ((string)(this.results[0]));
			}
		}
	}
}

#pragma warning restore 1591