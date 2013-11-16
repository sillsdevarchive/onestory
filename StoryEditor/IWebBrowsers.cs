using System;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public interface IWebBrowserDisplayStoryBt
	{
		// StoryEditor TheSe { get; set; }
		// StoryData StoryData { get; set; }
		// LinkLabel LineNumberLink { get; set; }
		// VerseData.ViewSettings ViewSettings { get; set; }
		// string GetSelectedText { get; }
		// string LastTextareaInFocusId { get; set; }
		// StringTransfer GetStringTransferOfLastTextAreaInFocus { get; }

		string GetTopRowId { get; }
		string GetPrevRowId { get; }
		string GetNextRowId { get; }
		string DocumentText { get; }

		// void ResetContextMenu();
		void LoadDocument(string strHtml);
		void ScrollToElement(String strElemName, bool bAlignWithTop);
		void ResetDocument();
		void TriggerOnBlur();
		bool TriggerPreSaveEvents(string strId);
		bool DoesElementIdExist(string strId);
		void AddNote(bool bNoteToSelf);
		void ShowPrintPreviewDialog();

		// bool CheckShowErrorOnFieldNotEditable(StringTransfer stringTransfer);
		bool SetSelectedText(StringTransfer stringTransfer, string strNewValue, out int nNewEndPoint);

		string GetSelectedTextByTextareaIdentifier(TextAreaIdentifier textAreaIdentifier,
												   out StoryEditor.TextFields whichLanguage);

		void MoveSelectedTextToNewLine(VerseData verseData, VerseData verseNew, int nLineIndex);

		void GetSelectedLanguageText(out string strVernacular, out string strNationalBt,
									 out string strInternationalBt, out string strFreeTranslation);
	}

	public interface IWebBrowserDisplayConNote
	{
		// StoryEditor TheSe { get; set; }
		// StoryData StoryData { get; set; }
		// LinkLabel LineNumberLink { get; set; }

		string GetTopRowId { get; }

		void LoadDocument(string strHtml);
		void ResetDocument();
		bool RemoveHtmlNodeById(string strId);
		string GetSelectedText(StringTransfer stringTransfer);
		void ClearSelection(StringTransfer stringTransfer);
		void SetSelection(StringTransfer stringTransfer, int nFoundIndex, int nLengthToSelect);
		bool SetSelectedText(StringTransfer stringTransfer, string strNewValue, out int nNewEndPoint);
		// string PaneLabel();
		// bool OnAddNote(int nVerseIndex, string strReferringText, string strNote, bool bNoteToSelf);
		void OnVerseLineJump(int nVerseIndex);
	}
}