using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetLoc;

namespace OneStoryProjectEditor
{
	public abstract class GeckoConNotesDisplayControl : GeckoDisplayControl
	{
		internal WebBrowserAdaptorConNote AdaptorConNote;

		protected override WebBrowserAdaptor Adaptor
		{
			get { return AdaptorConNote; }
		}

		public void OnAddNote(int nVerseIndex, string strReferringText, string strNote, bool bNoteToSelf)
		{
#if ToDo
			throw new NotImplementedException();
#endif
		}

		public void ClearSelection(StringTransfer stringTransfer)
		{
			throw new NotImplementedException();
		}

		public void SetSelection(StringTransfer stringTransfer, int nFoundIndex, int nLengthToSelect)
		{
			throw new NotImplementedException();
		}

		public bool SetSelectedText(StringTransfer stringTransfer, string strNewValue, out int nNewEndPoint)
		{
			throw new NotImplementedException();
		}

		public string GetTopRowId
		{
			get { throw new NotImplementedException(); }
		}

		public bool RemoveHtmlNodeById(string strId)
		{
			throw new NotImplementedException();
		}

		public string GetSelectedText(StringTransfer stringTransfer)
		{
			throw new NotImplementedException();
		}
	}

	public class GeckoConsultantNotesControl : GeckoConNotesDisplayControl, IWebBrowserDisplay, IWebBrowserDisplayConNote
	{
		public void LoadDocument(string strHtml)
		{
			NavigateToString(strHtml, "ConsultantNotesPane.html");
		}

		public void OnVerseLineJump(int nVerseIndex)
		{
			AdaptorConNote.TheSe.FocusOnVerse(nVerseIndex, false, true);
		}
	}

	public class GeckoCoachNotesControl : GeckoConNotesDisplayControl, IWebBrowserDisplay, IWebBrowserDisplayConNote
	{
		public void LoadDocument(string strHtml)
		{
			NavigateToString(strHtml, "CoachNotesPane.html");
		}

		public void OnVerseLineJump(int nVerseIndex)
		{
			AdaptorConNote.TheSe.FocusOnVerse(nVerseIndex, true, false);
		}
	}
}
