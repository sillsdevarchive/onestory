using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NetLoc;
using Palaso.UI.WindowsForms.Keyboarding;
using SilEncConverters40;

namespace OneStoryProjectEditor
{
	[ComVisible(true)]
	public partial class HtmlStoryBtControl : HtmlVerseControl, IWebBrowserDisplayStoryBt
	{
		internal WebBrowserAdaptorStoryBt AdaptorStoryBt;

		protected override WebBrowserAdaptor Adaptor
		{
			get { return AdaptorStoryBt; }
		}

		public HtmlStoryBtControl()
		{
			InitializeComponent();
			IsWebBrowserContextMenuEnabled = false;
			ObjectForScripting = this;
		}

		public void TriggerCtrlF5()
		{
			if (AdaptorStoryBt.TheSe.RealignLines())
				return;

			// done by the jscript
			// LoadDocument();
		}

		/*
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

			DocumentText = strHtml;
		}

		public void InsertNewVerseBefore(int nVerseIndex)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe))
				return;

			theSe.AddNewVerse(nVerseIndex - 1, 1, false);
		}

		public void AddNewVerseAfter(int nVerseIndex)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe))
				return;

			theSe.AddNewVerse(nVerseIndex - 1, 1, true);
		}

		public void HideVerse(int nVerseIndex)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe))
				return;

			var verseData = GetVerseData(nVerseIndex);
			// toggle
			theSe.VisiblizeVerse(verseData, !(verseData.IsVisible));
		}

		public void CopyVerse(int nVerseIndex)
		{
			try
			{
				// Copies the verse to the clipboard.
				// Clipboard.SetDataObject(_verseData);
				// make a copy so that if the user makes changes after the copy, we won't be
				//  referring to the same object.
				VerseData verseData = GetVerseData(nVerseIndex);
				_myClipboard = new VerseData(verseData);
			}
			catch   // ignore errors
			{
			}
		}

		public void PasteVerseBefore(int nVerseIndex)
		{
			PasteVerseToIndex(nVerseIndex - 1);
		}

		public void PasteVerseAfter(int nVerseIndex)
		{
			PasteVerseToIndex(nVerseIndex);
		}

		protected void PasteVerseToIndex(int nInsertionIndex)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return;

			if (_myClipboard != null)
			{
				var theNewVerse = new VerseData(_myClipboard);
				theNewVerse.AllowConNoteButtonsOverride();
				// make another copy, so that the guid is changed
				theSE.DoPasteVerse(nInsertionIndex, theNewVerse);
			}
		}

		public void OnVerseLineJump(int nVerseIndex)
		{
			AdaptorStoryBt.TheSe.FocusOnVerse(nVerseIndex, true, true);
		}
		*/

		public List<HtmlElement> GetSelectedTexts(int nLineNumber)
		{
			if (Document == null)
				return null;

			var doc = Document;

			TriggerOnBlur(doc);

			var strIdLn = VerseData.GetLineTableId(nLineNumber);
			HtmlElement elemParentLn = doc.GetElementById(strIdLn);
			if (elemParentLn == null)
				return null;

			var spans = elemParentLn.GetElementsByTagName("span");
			var list = new List<HtmlElement>(spans.Count);
			list.AddRange(spans.Cast<object>().Cast<HtmlElement>());
			return list;
		}

		public void TriggerOnBlur()
		{
			if (Document == null)
				return;

			TriggerOnBlur(Document);
		}

		private static void TriggerOnBlur(HtmlDocument doc)
		{
			// before we query for the spans, we have to trigger a 'blur'
			//  event (well, my 'fake' blur event) so the cell currently
			//  being edited will turn it's selection into a span also
			doc.InvokeScript("TriggerMyBlur");
		}

		public string GetSelectedTextByTextareaIdentifier(TextAreaIdentifier textAreaIdentifier, out StoryEditor.TextFields whichLanguage)
		{
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
		}

		public bool TriggerPreSaveEvents(string strId)
		{
			HtmlElement elem;
			if (!GetHtmlElementById(strId, out elem))
				return true;

			elem.InvokeMember("onchange");
			return false;
		}

		public void OnMouseMove()
		{
			AdaptorStoryBt.TheSe.CheckBiblePaneCursorPosition();
		}

		public bool TextareaMouseUp(string strId)
		{
			AdaptorStoryBt.LastTextareaInFocusId = strId;
			return true;
		}

		public bool TextareaOnKeyUp(string strId, string strText)
		{
			return AdaptorStoryBt.TextareaOnKeyUp(strId, strText);
		}

		public bool TextareaOnChange(string strId, string strText)
		{
			return AdaptorStoryBt.TextareaOnChange(strId, strText);
		}

		private bool CheckForTaskPermission(ProjectSettings.LanguageInfo li, StoryEditor.TextFields typeField,
			StoryEditor.TextFields eType, bool isTaskOn)
		{
			if (typeField == eType)
				return (!li.HasData || isTaskOn);
			return true;
		}

		public bool TextareaOnFocus(string strId)
		{
			return AdaptorStoryBt.TextareaOnFocus(strId);
		}

		public bool TextareaOnBlur(string strId)
		{
			KeyboardController.DeactivateKeyboard();
			return false;
		}

		public bool TextareaOnSelect(string strId, int nStartIndex, int nLength)
		{
			return false;
		}

		public bool DoesElementIdExist(string strId)
		{
			if (Document == null)
				return false;

			var elem = Document.GetElementById(strId);
			return (elem != null);
		}

		public bool GetHtmlElementById(string strId, out HtmlElement elem)
		{
			if (Document == null)
			{
				elem = null;
				return false;
			}

			var doc = Document;
			elem = doc.GetElementById(strId);
			return (elem != null);
		}

		public bool GetHtmlElementById(string strId, out HtmlDocument doc, out HtmlElement elem)
		{
			if (Document == null)
			{
				doc = null;
				elem = null;
				return false;
			}

			doc = Document;
			elem = doc.GetElementById(strId);
			return (elem != null);
		}

		public bool OnAnchorButton(string strButtonId)
		{
			return AdaptorStoryBt.OnAnchorButton(strButtonId);
		}

		public void AddScriptureReference(string strId)
		{
			StoryEditor theSe;
			if (!AdaptorStoryBt.CheckForProperEditToken(out theSe))
				return;

			int nLineIndex;
			if (!GetIndicesFromId(strId, out nLineIndex))
				return;

			HtmlElement elem;
			HtmlDocument doc;
			if (!GetHtmlElementById(strId, out doc, out elem))
				return;

			var verseData = AdaptorStoryBt.GetVerseData(nLineIndex);
			var strJumpTarget = AdaptorStoryBt.TheSe.GetNetBibleScriptureReference;
			if (verseData.Anchors.Contains(strJumpTarget))
				return;

			var anchorNew = verseData.Anchors.AddAnchorData(strJumpTarget,
															strJumpTarget);

			List<string> astrDontCare = null;
			string str = anchorNew.PresentationHtml(nLineIndex, null,
													StoryData.PresentationType.Plain,
													false,
													ref astrDontCare);

			// create a new button element out of this string of html
			var elemNew = doc.CreateElement(str);
			if (elemNew == null)
				return;

			// don't know why, but you have to explicitly set the inner text
			elemNew.InnerText = NetBibleViewer.CheckForLocalization(anchorNew.JumpTarget);
			elem.AppendChild(elemNew);
			AdaptorStoryBt.TheSe.Modified = true;
		}

		public void AddNote(bool bNoteToSelf)
		{
			System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(AdaptorStoryBt.LastTextareaInFocusId));

			TextAreaIdentifier textAreaIdentifier;
			if (!WebBrowserAdaptorStoryBt.TryGetTextAreaId(AdaptorStoryBt.LastTextareaInFocusId, out textAreaIdentifier))
				return;

			var TheSe = AdaptorStoryBt.TheSe;
			var nLastSubItemIndex = -1;
			string strLastFieldReference = null,
				   strReferringText = null,
				   strNote = String.Format("{0}: ", StoryEditor.GetInitials(TheSe.LoggedOnMember.Name));

			foreach (var span in GetSelectedTexts(textAreaIdentifier.LineIndex))
			{
				var textarea = span.Parent;
				System.Diagnostics.Debug.Assert(textarea != null && textarea.TagName == "TEXTAREA");

				TextAreaIdentifier textAreaIdentifierParent;
				if (!WebBrowserAdaptorStoryBt.TryGetTextAreaId(textarea.Id, out textAreaIdentifierParent))
					return;

				// if this is a new type, then add it to the stream
				if (strLastFieldReference != textAreaIdentifierParent.FieldTypeName)
				{
					if (!String.IsNullOrEmpty(strLastFieldReference))
						strReferringText += " vs: ";

					strLastFieldReference = textAreaIdentifierParent.FieldReferenceName;
					strReferringText += strLastFieldReference;
				}

				else if (textAreaIdentifierParent.SubItemIndex != nLastSubItemIndex)
				{
					if (nLastSubItemIndex != -1)
						strReferringText += " &";
					nLastSubItemIndex = textAreaIdentifierParent.SubItemIndex;
				}
				strReferringText += " " + span.OuterHtml;
			}

			// remove the highlight class so it isn't highlighted in the connnote pane
			if (strReferringText != null)
			{
				strReferringText = strReferringText.Replace(" highlight", null);
				strReferringText = strReferringText.Replace(" readonly", null);
			}

			TheSe.SendNoteToCorrectPane(textAreaIdentifier.LineIndex, strReferringText, strNote, bNoteToSelf);
		}

		protected bool GetIndicesFromId(string strId, out int nLineIndex)
		{
			try
			{
				// for AnchorIds:
				//  anc_<lineNum>
				// where:
				//  lineNum (0-GTQ line, ln 1, etc)
				string[] aVerseConversationIndices = strId.Split(WebBrowserAdaptor.AchDelim);
				System.Diagnostics.Debug.Assert((aVerseConversationIndices[0] == AnchorData.CstrButtonPrefixAnchorBar) &&
												(aVerseConversationIndices.Length == 2));

				nLineIndex = Convert.ToInt32(aVerseConversationIndices[1]);
			}
			catch
			{
				nLineIndex = 0;
				return false;
			}
			return true;
		}

		public void SelectFoundText(string strHtmlElementId,
			int nFoundIndex, int nLengthToSelect)
		{
			if (Document == null)
				return;

			var oaParams = new object[] { strHtmlElementId, nFoundIndex, nLengthToSelect };
			Document.InvokeScript("paragraphSelect", oaParams);
		}

		public void MoveSelectedTextToNewLine(VerseData verseData, VerseData verseNew, int nLineIndex)
		{
			var spans = GetSelectedTexts(nLineIndex);

			var projSettings = AdaptorStoryBt.TheSe.StoryProject.ProjSettings;
			MoveSelectedText(spans, GetStoryLineId(nLineIndex, StoryEditor.TextFields.Vernacular.ToString()),
							 projSettings.Vernacular.HasData,
							 verseData.StoryLine.Vernacular, verseNew.StoryLine.Vernacular);
			MoveSelectedText(spans, GetStoryLineId(nLineIndex, StoryEditor.TextFields.NationalBt.ToString()),
							 projSettings.NationalBT.HasData,
							 verseData.StoryLine.NationalBt, verseNew.StoryLine.NationalBt);
			MoveSelectedText(spans, GetStoryLineId(nLineIndex, StoryEditor.TextFields.InternationalBt.ToString()),
							 projSettings.InternationalBT.HasData,
							 verseData.StoryLine.InternationalBt, verseNew.StoryLine.InternationalBt);
			MoveSelectedText(spans, GetStoryLineId(nLineIndex, StoryEditor.TextFields.FreeTranslation.ToString()),
							 projSettings.FreeTranslation.HasData,
							 verseData.StoryLine.FreeTranslation, verseNew.StoryLine.FreeTranslation);

			for (var i = 0; i < verseData.Retellings.Count; i++)
			{
				var lineDataFrom = verseData.Retellings[i];
				var lineDataTo = verseNew.Retellings[i];
				MoveSelectedText(spans, GetRetellingId(nLineIndex, i, StoryEditor.TextFields.Vernacular.ToString()),
								 projSettings.ShowRetellings.Vernacular,
								 lineDataFrom.Vernacular, lineDataTo.Vernacular);
				MoveSelectedText(spans, GetRetellingId(nLineIndex, i, StoryEditor.TextFields.NationalBt.ToString()),
								 projSettings.ShowRetellings.NationalBt,
								 lineDataFrom.NationalBt, lineDataTo.NationalBt);
				MoveSelectedText(spans, GetRetellingId(nLineIndex, i, StoryEditor.TextFields.InternationalBt.ToString()),
								 projSettings.ShowRetellings.InternationalBt,
								 lineDataFrom.InternationalBt, lineDataTo.InternationalBt);
			}
		}

		private void MoveSelectedText(IEnumerable<HtmlElement> spans, string strId, bool bFieldShowing,
			StringTransfer stFrom, StringTransfer stTo)
		{
			if (!bFieldShowing)
				return;

			var strSelectedText = GetSpanInnerText(spans, strId);
			string strOriginalText;
			if (String.IsNullOrEmpty(strSelectedText) ||
				!stFrom.TryGetSourceString(strSelectedText, out strOriginalText))
				return;

			stTo.SetValue(strOriginalText);
			stFrom.RemoveSubstring(strOriginalText);
		}

		private static string GetStoryLineId(int nVerseIndex, string strFieldTypeName)
		{
			StoryEditor.LocalizedEnum<StoryEditor.TextFields> field = StoryEditor.TextFields.StoryLine;
			return StringTransfer.TextareaId(nVerseIndex,
											 field.ToString(),
											 0,
											 0,
											 strFieldTypeName);
		}

		private static string GetRetellingId(int nVerseIndex, int nSubItemIndex, string strFieldTypeName)
		{
			StoryEditor.LocalizedEnum<StoryEditor.TextFields> field = StoryEditor.TextFields.Retelling;
			return StringTransfer.TextareaId(nVerseIndex,
											 field.ToString(),
											 0,
											 nSubItemIndex,
											 strFieldTypeName);
		}

		public void ShowContextMenu(string strId)
		{
			AdaptorStoryBt.ShowContextMenu(strId);
		}

		public void GetSelectedLanguageText(out string strVernacular, out string strNationalBt,
											out string strInternationalBt, out string strFreeTranslation)
		{
			System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(AdaptorStoryBt.LastTextareaInFocusId));

			TextAreaIdentifier textAreaIdentifier;
			if (!WebBrowserAdaptorStoryBt.TryGetTextAreaId(AdaptorStoryBt.LastTextareaInFocusId, out textAreaIdentifier))
			{
				strVernacular = strNationalBt = strInternationalBt = strFreeTranslation = null;
				return;
			}

			var spans = GetSelectedTexts(textAreaIdentifier.LineIndex);
			strVernacular = GetSpanInnerText(spans, AdaptorStoryBt.GetMyVernacularSibling);
			strNationalBt = GetSpanInnerText(spans, AdaptorStoryBt.GetMyNationalBtSibling);
			strInternationalBt = GetSpanInnerText(spans, AdaptorStoryBt.GetMyInternationalBtSibling);
			strFreeTranslation = GetSpanInnerText(spans, AdaptorStoryBt.GetMyFreeTranslationSibling);
		}

		private string GetSpanInnerText(IEnumerable<HtmlElement> spans, WebBrowserAdaptorStoryBt.GetSiblingId getterSiblingId)
		{
			return GetSpanInnerText(spans, getterSiblingId(AdaptorStoryBt.LastTextareaInFocusId));
		}

		private static string GetSpanInnerText(IEnumerable<HtmlElement> spans, string strId)
		{
			return (from span in spans
					where (span != null) && (span.Parent != null) && (span.Parent.Id == strId)
					select span.InnerText).FirstOrDefault();
		}
	}
}
