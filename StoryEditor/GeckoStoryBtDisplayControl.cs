using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SilEncConverters40;

namespace OneStoryProjectEditor
{
	public class GeckoStoryBtDisplayControl : GeckoStoryDisplayControl
	{
		public static StoryEditor.Transliterators Transliterators { get; set; }
		public VerseData.ViewSettings ViewSettings { get; set; }
		public StoryData ParentStory { get; set; }

		public override void LoadDocument()
		{
			string strHtml = null;
			if (ParentStory != null)
				strHtml = ParentStory.PresentationHtml(ViewSettings,
													   TheSe.StoryProject.ProjSettings,
													   TheSe.StoryProject.TeamMembers,
													   StoryData);
			else if (StoryData != null)
				strHtml = StoryData.PresentationHtml(ViewSettings,
													 TheSe.StoryProject.ProjSettings,
													 TheSe.StoryProject.TeamMembers,
													 null);

			var filePath = Path.Combine(Environment.CurrentDirectory, "StoryBtPane.html");
			File.WriteAllText(filePath, strHtml, Encoding.UTF8);
			Navigate("file://" + filePath);
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

		private static string _lastTextareaInFocusId;
		public static string LastTextareaInFocusId
		{
			get { return _lastTextareaInFocusId; }
			set
			{
				System.Diagnostics.Debug.WriteLine("setting LastTextareaInFocusId to " + value);
				_lastTextareaInFocusId = value;
			}
		}

		public virtual void ScrollToVerse(int nVerseIndex)
		{
			StrIdToScrollTo = VersesData.LineId(nVerseIndex);
			if (!String.IsNullOrEmpty(StrIdToScrollTo))
				ScrollToElement(StrIdToScrollTo, true);
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

		public string GetSelectedText
		{
			get
			{
				if (String.IsNullOrEmpty(LastTextareaInFocusId))
					return null;

#if ToDo
				// TODO
				TextAreaIdentifier textAreaIdentifier;
				if (!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
					return null;

				StoryEditor.TextFields whichLanguage;
				return GetSelectedTextByTextareaIdentifier(textAreaIdentifier, out whichLanguage);
#else
				return null;
#endif
			}
		}

		internal void GetSelectedLanguageText(out string strVernacular, out string strNationalBt, out string strInternationalBt, out string strFreeTranslation)
		{
			strVernacular = strNationalBt = strInternationalBt = strFreeTranslation = null;
#if ToDo
			throw new NotImplementedException();
#endif
		}

		public void ResetContextMenu()
		{
#if ToDo
			throw new NotImplementedException();
#endif
		}
	}
}
