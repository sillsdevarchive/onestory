using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;             // for LinkLabel

namespace OneStoryProjectEditor
{
	public abstract class GeckoDisplayControl : OseGeckoWebBrowser
	{
		public const string CstrTextAreaPrefix = "ta";
		public const string CstrParagraphPrefix = "tp";
		public const string CstrButtonPrefix = "btn";

		internal LinkLabel LineNumberLink;

		internal string StrIdToScrollTo;

		public StoryEditor TheSe { get; set; }
		public virtual StoryData StoryData { get; set; }

		public abstract void LoadDocument();

		protected void NavigateToString(string strHtml, string strPaneName)
		{
			var filePath = Path.Combine(Environment.CurrentDirectory, strPaneName);
			File.WriteAllText(filePath, strHtml, Encoding.UTF8);
			Navigate("file://" + filePath);
		}

		protected VerseData GetVerseData(int nLineIndex)
		{
			Debug.Assert(StoryData.Verses.Count > (nLineIndex - 1));
			return (nLineIndex == 0)
					   ? StoryData.Verses.FirstVerse
					   : StoryData.Verses[nLineIndex - 1];
		}

		public void ScrollToElement(String strElemName, bool bAlignWithTop)
		{
			Debug.Assert(!String.IsNullOrEmpty(strElemName));
			if (Document == null)
				return;

			var elem = Document.GetElementById(strElemName) as Gecko.GeckoHtmlElement;
			if (elem == null)
				return;

			elem.ScrollIntoView(bAlignWithTop);
			if (!bAlignWithTop)
				elem.Focus();
		}

		public void ResetDocument()
		{
			//reset so we don't jump to a soon-to-be-non-existant (or wrong context) place
			// update: if you *don't* want to jump there, then clear out StrIdToScrollTo manually. This needs
			//  to be here (e.g. for DoMove) which wants to go back to the same spot
			// StrIdToScrollTo = null;
			if (Document != null)
			{
#if ToDo
				// TODO:
				Document.OpenNew(true);
#endif
			}
		}

		public virtual void ScrollToVerse(int nVerseIndex)
		{
			StrIdToScrollTo = VersesData.LineId(nVerseIndex);
			if (!String.IsNullOrEmpty(StrIdToScrollTo))
				ScrollToElement(StrIdToScrollTo, true);
		}
	}
}
