using System;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public interface IWebBrowserDisplayStoryBt
	{
		StoryEditor TheSe { get; set; }
		StoryData StoryData { get; set; }
		LinkLabel LineNumberLink { get; set; }
		VerseData.ViewSettings ViewSettings { get; set; }
		string GetSelectedText { get; }
		string LastTextareaInFocusId { get; set; }
		StringTransfer GetStringTransferOfLastTextAreaInFocus { get; }

		void ResetContextMenu();
		void LoadDocument();
		void ScrollToElement(String strElemName, bool bAlignWithTop);
		void ScrollToVerse(int nVerseIndex);
		void ResetDocument();
		void TriggerChangeUpdate();
		bool CheckShowErrorOnFieldNotEditable(StringTransfer stringTransfer);
		bool SetSelectedText(StringTransfer stringTransfer, string strNewValue, out int nNewEndPoint);

		void GetSelectedLanguageText(out string strVernacular, out string strNationalBt,
									 out string strInternationalBt, out string strFreeTranslation);
	}

	public interface IWebBrowserDisplayConNote
	{
		StoryEditor TheSe { get; set; }
		StoryData StoryData { get; set; }
		LinkLabel LineNumberLink { get; set; }

		void LoadDocument();
		void ScrollToVerse(int nVerseIndex);
		void ResetDocument();
		string PaneLabel();
		bool OnAddNote(int nVerseIndex, string strReferringText, string strNote, bool bNoteToSelf);
		void OnVerseLineJump(int nVerseIndex);
	}
}