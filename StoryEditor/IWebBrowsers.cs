using System;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public interface IWebBrowserDisplay
	{
		string GetTopRowId { get; }
		void LoadDocument(string strHtml);
		void ScrollToElement(String strElemName, bool bAlignWithTop);
	}

	public interface IWebBrowserDisplayStoryBt
	{
		string GetPrevRowId { get; }
		string GetNextRowId { get; }
		string DocumentText { get; }

		void ResetDocument();
		void TriggerOnBlur();
		bool TriggerPreSaveEvents(string strId);
		bool DoesElementIdExist(string strId);
		void AddNote(bool bNoteToSelf);
		void ShowPrintPreviewDialog();

		bool SetSelectedText(StringTransfer stringTransfer, string strNewValue, out int nNewEndPoint);

		string GetSelectedTextByTextareaIdentifier(TextAreaIdentifier textAreaIdentifier,
												   out StoryEditor.TextFields whichLanguage);

		void MoveSelectedTextToNewLine(VerseData verseData, VerseData verseNew, int nLineIndex);

		void GetSelectedLanguageText(out string strVernacular, out string strNationalBt,
									 out string strInternationalBt, out string strFreeTranslation);
	}

	public interface IWebBrowserDisplayConNote
	{
		void ResetDocument();
		bool RemoveHtmlNodeById(string strId);
		string GetSelectedText(StringTransfer stringTransfer);
		void ClearSelection(StringTransfer stringTransfer);
		void SetSelection(StringTransfer stringTransfer, int nFoundIndex, int nLengthToSelect);
		bool SetSelectedText(StringTransfer stringTransfer, string strNewValue, out int nNewEndPoint);
		void OnVerseLineJump(int nVerseIndex);
	}
}