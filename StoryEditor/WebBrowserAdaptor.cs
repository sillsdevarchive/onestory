using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public abstract partial class WebBrowserAdaptor : UserControl
	{
		public enum WhichBrowser
		{
			Undefined = 0,
			InternetExplorer,
			GeckoFx
		}

		public const string CstrTextAreaPrefix = "ta";
		public const string CstrParagraphPrefix = "tp";
		public const string CstrButtonPrefix = "btn";
		public static readonly char[] AchDelim = new[] { '_' };

		protected WhichBrowser _whichBrowser { get; set; }

		protected GeckoDisplayControl GeckoWebBrowser { get; set; }
		protected HtmlVerseControl IeWebBrowser { get; set; }

		protected WebBrowserAdaptor()
		{
			InitializeComponent();
			// SetBrowserToUse(ShouldUseGecko);
		}

		public void SetBrowserToUse(bool shouldUseGecko)
		{
			// first remove any existing control, which is going to be replaced in any case
			Controls.Clear();

			// on linux, only Gecko works (on Windows, either Gecko or IE will work, but using IE saves us from having to redistribute too much)
			//  so on Linux, prefer Gecko, but on Windows, prefer IE.
			if (shouldUseGecko)
			{
				// try to initialize
				// if GeckoFx was successfully initialized, then use it
				if (GeckoFxInitializer.SetUpXulRunner())
				{
					_whichBrowser = WhichBrowser.GeckoFx;
					GeckoWebBrowser = MyGeckoBrowser;

					Controls.Add(GeckoWebBrowser);

					IeWebBrowser = null;    // in case it was being used
				}
				else
				{
					Controls.Add(GeckoFxInitializer.InstructionsLinkLabel);
				}
			}
			else
			{
				_whichBrowser = WhichBrowser.InternetExplorer;
				IeWebBrowser = MyIeBrowser;
				Controls.Add(IeWebBrowser);

				GeckoWebBrowser = null; // in case it was being used
			}
		}

		protected abstract HtmlVerseControl MyIeBrowser { get; }
		protected abstract GeckoDisplayControl MyGeckoBrowser { get; }

		public IWebBrowserDisplay BrowserDisplay
		{
			get
			{
				return (_whichBrowser == WhichBrowser.InternetExplorer)
						   ? (IWebBrowserDisplay)IeWebBrowser
						   : (IWebBrowserDisplay)GeckoWebBrowser;
			}
		}

		private static bool ShouldUseGecko
		{
			get
			{
				string xulRunnerPath;
				return GeckoFxInitializer.DoesXulRunnerFolderExist(out xulRunnerPath);
			}
		}

		public virtual void ScrollToVerse(int nVerseIndex)
		{
			StrIdToScrollTo = VersesData.LineId(nVerseIndex);
			if (!String.IsNullOrEmpty(StrIdToScrollTo))
				BrowserDisplay.ScrollToElement(StrIdToScrollTo, true);
		}

		public VerseData GetVerseData(int nLineIndex)
		{
			Debug.Assert(StoryData.Verses.Count > (nLineIndex - 1));
			return (nLineIndex == 0)
					   ? StoryData.Verses.FirstVerse
					   : StoryData.Verses[nLineIndex - 1];
		}

		public bool IsParagraphElement(string strHtmlId)
		{
			return (!String.IsNullOrEmpty(strHtmlId) && (strHtmlId.IndexOf(CstrParagraphPrefix) == 0));
		}

		public bool IsTextareaElement(string strHtmlId)
		{
			return (!String.IsNullOrEmpty(strHtmlId) && (strHtmlId.IndexOf(CstrTextAreaPrefix) == 0));
		}

		public bool IsButtonElement(string strHtmlId)
		{
			return (!String.IsNullOrEmpty(strHtmlId) && (strHtmlId.IndexOf(CstrButtonPrefix) == 0));
		}

		public VerseData VerseDataFromLineOptionsButtonId(string strId, out int nLineIndex)
		{
			var astr = strId.Split(AchDelim);
			if ((astr.Length == 2) && (astr[0] == VersesData.CstrButtonPrefixLineOptionsButton))
			{
				nLineIndex = Convert.ToInt32(astr[1]);
				return GetVerseData(nLineIndex);
			}
			nLineIndex = -1;
			return null;
		}

		public VerseData VerseDataFromAnchorButtonId(string strId, out int nLineIndex)
		{
			// if there is no button, this comes in as anc_3 (for line 3 anchor bar), but otherwise, it might be ???
			var astr = strId.Split(AchDelim);
			if ((astr[0] == AnchorData.CstrButtonPrefixAnchorButton) || (astr[0] == AnchorData.CstrButtonPrefixAnchorBar))
			{
				nLineIndex = Convert.ToInt32(astr[1]);
				return GetVerseData(nLineIndex);
			}

			nLineIndex = -1;
			return null;
		}

		public abstract void LoadDocument();

		public string StrIdToScrollTo;

		public StoryEditor TheSe { get; set; }
		public virtual StoryData StoryData { get; set; }

		public delegate void SetLineNumberLinkProc(string strText, int nLineIndex);
		internal SetLineNumberLinkProc SetLineNumberLink;

		public delegate void MakeLineNumberLinkVisibleProc();
		internal MakeLineNumberLinkVisibleProc MakeLineNumberLinkVisible;
	}
}
