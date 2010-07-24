using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using mshtml;

namespace OneStoryProjectEditor
{
	[ComVisible(true)]
	public class HtmlStoryBtControl : HtmlVerseControl
	{
		public const string CstrFieldNameVernacular = "Vernacular";
		public const string CstrFieldNameNationalBt = "NationalBT";
		public const string CstrFieldNameInternationalBt = "InternationalBT";

		public VerseData.ViewItemToInsureOn ViewItemsToInsureOn { get; set; }
		public StoryData ParentStory { get; set; }

		public HtmlStoryBtControl()
		{
			ObjectForScripting = this;
		}

		public override void LoadDocument()
		{
			string strHtml = null;
			if (ParentStory != null)
				strHtml = ParentStory.PresentationHtml(ViewItemsToInsureOn,
					TheSE.StoryProject.ProjSettings, TheSE.StoryProject.TeamMembers, StoryData);
			else if (StoryData != null)
				strHtml = StoryData.PresentationHtml(ViewItemsToInsureOn,
					TheSE.StoryProject.ProjSettings, TheSE.StoryProject.TeamMembers, null);

			DocumentText = strHtml;
		}

		public void InsertNewVerseBefore(int nVerseIndex)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return;

			theSE.AddNewVerse(nVerseIndex - 1, 1, false);
		}

		public void AddNewVerseAfter(int nVerseIndex)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return;

			theSE.AddNewVerse(nVerseIndex - 1, 1, true);
		}

		public void HideVerse(int nVerseIndex)
		{
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return;

			VerseData verseData = Verse(nVerseIndex);
			theSE.VisiblizeVerse(verseData,
				(verseData.IsVisible) ? false : true   // toggle
				);
		}

		public void DeleteVerse(int nVerseIndex)
		{
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return;

			VerseData verseData = Verse(nVerseIndex);
			if (verseData.HasData)
			{
				DialogResult res = MessageBox.Show(
					Properties.Resources.IDS_VerseNotEmptyHideQuery,
					OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);

				if (res == DialogResult.Yes)
				{
					theSE.VisiblizeVerse(verseData, false);
					return;
				}

				if (res == DialogResult.Cancel)
					return;
			}

			if (MessageBox.Show(
				Properties.Resources.IDS_DeleteVerseQuery,
				OseResources.Properties.Resources.IDS_Caption,
				MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
			{
				theSE.DeleteVerse(verseData);
			}
		}

		protected static VerseData _myClipboard = null;
		public void CopyVerse(int nVerseIndex)
		{
			try
			{
				// Copies the verse to the clipboard.
				// Clipboard.SetDataObject(_verseData);
				// make a copy so that if the user makes changes after the copy, we won't be
				//  referring to the same object.
				VerseData verseData = Verse(nVerseIndex);
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
				VerseData theNewVerse = new VerseData(_myClipboard);
				theNewVerse.AllowConNoteButtonsOverride();
				// make another copy, so that the guid is changed
				theSE.DoPasteVerse(nInsertionIndex, theNewVerse);
			}
		}

		public void OnVerseLineJump(int nVerseIndex)
		{
			TheSE.FocusOnVerse(nVerseIndex, true, true);
		}

		public bool TextareaOnKeyUp(string strId, string strText)
		{
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return false;

			int nVerseIndex;
			string strTextElement;
			if (!GetIndicesFromId(strId, out nVerseIndex, out strTextElement))
				return false;

			if (strTextElement == CstrFieldNameVernacular)
				StoryData.Verses[nVerseIndex].VernacularText.SetValue(strText);
			else if (strTextElement == CstrFieldNameNationalBt)
				StoryData.Verses[nVerseIndex].NationalBTText.SetValue(strText);
			else if (strTextElement == CstrFieldNameInternationalBt)
				StoryData.Verses[nVerseIndex].InternationalBTText.SetValue(strText);

			// indicate that the document has changed
			theSE.Modified = true;

			// update the status bar (in case we previously put an error there
			StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[theSE.theCurrentStory.ProjStage.ProjectStage];
			theSE.SetStatusBar(String.Format("{0}  Press F1 for instructions", st.StageDisplayString));

			return true;
		}

		protected bool GetIndicesFromId(string strId,
			out int nVerseIndex, out string strTextElement)
		{
			try
			{
				string[] aVerseConversationIndices = strId.Split(_achDelim);
				System.Diagnostics.Debug.Assert(((aVerseConversationIndices.Length == 3) ||
												 (aVerseConversationIndices.Length == 4))
												&&
												((aVerseConversationIndices[0] == "ta") ||
												 (aVerseConversationIndices[0] == "btn")));

				strTextElement = aVerseConversationIndices[0];
				nVerseIndex = Convert.ToInt32(aVerseConversationIndices[1]);
			}
			catch
			{
				nVerseIndex = 0;
				strTextElement = null;
				return false;
			}
			return true;
		}

		public void SelectFoundText(string strHtmlElementId,
			int nFoundIndex, int nLengthToSelect)
		{
			if (Document != null)
			{
				HtmlDocument doc = Document;
				object[] oaParams = new object[] { strHtmlElementId, nFoundIndex, nLengthToSelect };
				// doc.InvokeScript("textboxSelect", oaParams);
				doc.InvokeScript("paragraphSelect", oaParams);
			}
		}
	}
}
