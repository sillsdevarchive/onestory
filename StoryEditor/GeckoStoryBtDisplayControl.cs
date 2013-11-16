using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SilEncConverters40;

namespace OneStoryProjectEditor
{
	public class GeckoStoryBtDisplayControl : GeckoDisplayControl, IWebBrowserDisplay, IWebBrowserDisplayStoryBt
	{
		internal WebBrowserAdaptorStoryBt AdaptorStoryBt;

		protected override WebBrowserAdaptor Adaptor
		{
			get { return AdaptorStoryBt; }
		}

		public StringTransfer GetStringTransferOfLastTextAreaInFocus
		{
			get { throw new NotImplementedException(); }
		}

		public string GetTopRowId
		{
			get { throw new NotImplementedException(); }
		}

		public string GetPrevRowId
		{
			get { throw new NotImplementedException(); }
		}

		public string GetNextRowId
		{
			get { throw new NotImplementedException(); }
		}

		public string DocumentText
		{
			get { throw new NotImplementedException(); }
		}

		public void LoadDocument(string strHtml)
		{
			NavigateToString(strHtml, "StoryBtPane.html");
		}

		public void TriggerOnBlur()
		{
			throw new NotImplementedException();
		}

		public bool TriggerPreSaveEvents(string strId)
		{
			throw new NotImplementedException();
		}

		public bool DoesElementIdExist(string strId)
		{
			throw new NotImplementedException();
		}

		public void AddNote(bool bNoteToSelf)
		{
			throw new NotImplementedException();
		}

		public void ShowPrintPreviewDialog()
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

		public string GetSelectedTextByTextareaIdentifier(TextAreaIdentifier textAreaIdentifier, out StoryEditor.TextFields whichLanguage)
		{
			throw new NotImplementedException();
			/*
			WebBrowserAdaptorStoryBt.GetSiblingId pSiblingId;
			var spans = GetSelectedTexts(textAreaIdentifier.LineIndex);
			whichLanguage = textAreaIdentifier.LanguageColumn;
			switch (whichLanguage)
			{
				case StoryEditor.TextFields.Vernacular:
					pSiblingId = AdaptorStoryBt.GetMyVernacularSibling;
					break;
				case StoryEditor.TextFields.NationalBt:
					pSiblingId = AdaptorStoryBt.GetMyNationalBtSibling;
					break;
				case StoryEditor.TextFields.InternationalBt:
					pSiblingId = AdaptorStoryBt.GetMyInternationalBtSibling;
					break;
				case StoryEditor.TextFields.FreeTranslation:
					pSiblingId = AdaptorStoryBt.GetMyFreeTranslationSibling;
					break;
				default:
					System.Diagnostics.Debug.Fail("wasn't expecting this case");
					return null;
			}
			return GetSpanInnerText(spans, pSiblingId);
			*/
		}

		public void MoveSelectedTextToNewLine(VerseData verseData, VerseData verseNew, int nLineIndex)
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
