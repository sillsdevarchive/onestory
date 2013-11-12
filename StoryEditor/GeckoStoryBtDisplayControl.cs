using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SilEncConverters40;

namespace OneStoryProjectEditor
{
	public class GeckoStoryBtDisplayControl : GeckoDisplayControl, IWebBrowserDisplayStoryBt
	{
		public static StoryEditor.Transliterators Transliterators { get; set; }
		public VerseData.ViewSettings ViewSettings { get; set; }
		public StoryData ParentStory { get; set; }

		public StringTransfer GetStringTransferOfLastTextAreaInFocus
		{
			get { throw new NotImplementedException(); }
		}

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

			NavigateToString(strHtml, "StoryBtPane.html");
		}

		public void TriggerChangeUpdate()
		{
			throw new NotImplementedException();
		}

		public bool CheckShowErrorOnFieldNotEditable(StringTransfer stringTransfer)
		{
			throw new NotImplementedException();
		}

		public bool SetSelectedText(StringTransfer stringTransfer, string strNewValue, out int nNewEndPoint)
		{
			throw new NotImplementedException();
		}

		private string _lastTextareaInFocusId;
		public string LastTextareaInFocusId
		{
			get { return _lastTextareaInFocusId; }
			set
			{
				System.Diagnostics.Debug.WriteLine("setting LastTextareaInFocusId to " + value);
				_lastTextareaInFocusId = value;
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

		public void GetSelectedLanguageText(out string strVernacular, out string strNationalBt, out string strInternationalBt, out string strFreeTranslation)
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