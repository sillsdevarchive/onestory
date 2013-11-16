#define AddNoteFromConNotes

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using mshtml;
using NetLoc;

namespace OneStoryProjectEditor
{
	[ComVisible(true)]
	public abstract class HtmlConNoteControl : HtmlVerseControl
	{
		internal WebBrowserAdaptorConNote AdaptorConNote;

		protected override WebBrowserAdaptor Adaptor
		{
			get { return AdaptorConNote; }
		}

		protected HtmlConNoteControl()
		{
			InitializeComponent();
#if AddNoteFromConNotes
			IsWebBrowserContextMenuEnabled = false;
#endif
			ObjectForScripting = this;
		}

		public bool OnShowHideOpenConversations(string strButtonId)
		{
			AdaptorConNote.StrIdToScrollTo = GetTopRowId;
			var astrId = strButtonId.Split('_');
			System.Diagnostics.Debug.Assert(astrId.Length ==2);
			var nVerseIndex = Convert.ToInt32(astrId[1]);

			// toggle state of 'Show All' or 'Hide Closed' button
			var aCNsDC = AdaptorConNote.DataConverter(nVerseIndex);
			aCNsDC.ShowOpenConversations = !(aCNsDC.ShowOpenConversations);

			// brute force (no need to repaint the button since the reload will do it for us
			AdaptorConNote.LoadDocument();
			// don't think we need this anymore
			// Application.DoEvents();
			// ScrollToVerse(nVerseIndex);
			return true;
		}

		public bool OnClickHide(string strId)
		{
			return AdaptorConNote.OnClickHide(strId);
		}

		public bool RemoveHtmlNodeById(string strId)
		{
			if (Document != null)
			{
				var htmldoc = (HTMLDocumentClass)Document.DomDocument;
				if (htmldoc != null)
				{
					var node = (IHTMLDOMNode)htmldoc.getElementById(strId);
					if (node != null)
					{
						node.parentNode.removeChild(node);
						return true;
					}
				}
			}
			return false;
		}

		public bool OnClickEndConversation(string strId)
		{
			int nVerseIndex, nConversationIndex;
			ConsultNotesDataConverter theCNsDC;
			ConsultNoteDataConverter theCNDC;
			if (!AdaptorConNote.GetDataConverters(strId, out nVerseIndex, out nConversationIndex,
				out theCNsDC, out theCNDC))
				return false;

			System.Diagnostics.Debug.Assert(Document != null);
			HtmlElement elemButton = Document.GetElementById(strId);
			System.Diagnostics.Debug.Assert(elemButton != null);

			if (theCNDC.IsFinished)
			{
				theCNDC.IsFinished = false;
				elemButton.InnerText = ConsultNoteDataConverter.CstrButtonLabelConversationEnd;

				// also if it were hidden, then make it unhidden
				theCNDC.Visible = true;
			}
			else
			{
				theCNDC.IsFinished = true;
				elemButton.InnerText = ConsultNoteDataConverter.CstrButtonLabelConversationReopen;
			}

			var TheSe = AdaptorConNote.TheSe;
			AdaptorConNote.StrIdToScrollTo = GetTopRowId;
			if (theCNDC.IsFinished)
			{
				if (!theCNDC.FinalComment.HasData)
					theCNDC.Remove(theCNDC.FinalComment);
			}
			else
			{
				// just in case we need to have an open box now
				theCNsDC.InsureExtraBox(theCNDC, TheSe.TheCurrentStory,
					TheSe.LoggedOnMember, TheSe.StoryProject.TeamMembers);
			}

			if (String.IsNullOrEmpty(AdaptorConNote.StrIdToScrollTo))
			{
				if (theCNDC.IsEditable(TheSe.LoggedOnMember, TheSe.StoryProject.TeamMembers,
					TheSe.TheCurrentStory))
					AdaptorConNote.StrIdToScrollTo = ConsultNoteDataConverter.TextareaId(nVerseIndex, nConversationIndex);
				else
					AdaptorConNote.StrIdToScrollTo = ConsultNoteDataConverter.TextareaReadonlyRowId(nVerseIndex, nConversationIndex,
																					 theCNDC.Count - 1);
			}

			AdaptorConNote.LoadDocument();
			return true;
		}

		/* doesn't seem to work... the 'value' member isn't updated until *after*
		 * keyPress is executed. I could use event.keyCode to get the latest key
		 * pressed, but that's not what I want. So have to use onKeyUp
		public bool TextareaOnKeyPress(string strId, string strText)
		{
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return false;

			int nVerseIndex, nConversationIndex;
			if (!GetIndicesFromId(strId, out nVerseIndex, out nConversationIndex))
				return false;

			ConsultNoteDataConverter theCNDC = DataConverter(nVerseIndex, nConversationIndex);
			System.Diagnostics.Debug.Assert((theCNDC != null) && (theCNDC.Count > 0));
			CommInstance aCI = theCNDC[theCNDC.Count - 1];
			System.Diagnostics.Debug.WriteLine(String.Format("Was: {0}, now: {1}",
				aCI, strText));
			aCI.SetValue(strText);

			// indicate that the document has changed
			theSE.Modified = true;
			return true;
		}
		*/

		public void CopyScriptureReference(string strId)
		{
			int nVerseIndex, nConversationIndex, nDontCare;
			if (!AdaptorConNote.GetIndicesFromId(strId, out nVerseIndex, out nConversationIndex, out nDontCare))
				return;

			ConsultNoteDataConverter theCNDC = AdaptorConNote.DataConverter(nVerseIndex, nConversationIndex);
			System.Diagnostics.Debug.Assert((theCNDC != null) && (theCNDC.Count > 0));
			CommInstance aCI = theCNDC.FinalComment;

			if (Document != null)
			{
				HtmlDocument doc = Document;
				HtmlElement elem = doc.GetElementById(strId);
				if (elem != null)
				{
					elem.InnerText += AdaptorConNote.TheSe.GetNetBibleScriptureReference;
					aCI.SetValue(elem.InnerText);
					elem.Focus();
				}
			}
		}

		public bool TextareaOnKeyUp(string strId, string strText)
		{
			return AdaptorConNote.TextareaOnKeyUp(strId, strText);
		}

		public void DoFind(string strId)
		{
			AdaptorConNote.DoFind(strId);
		}

		private const string CstrParagraphHighlightBegin = "<span style=\"background-color:Blue; color: White\">";
		private ToolStripMenuItem menuAddNote;
		private ToolStripMenuItem menuAddNoteToSelf;
		private const string CstrParagraphHighlightEnd = "</span>";

		public void SetSelection(StringTransfer stringTransfer, int nFoundIndex, int nLengthToSelect)
		{
			System.Diagnostics.Debug.Assert(stringTransfer.HasData && !String.IsNullOrEmpty(stringTransfer.HtmlElementId));
			if (Document != null)
			{
				HtmlDocument doc = Document;
				if (AdaptorConNote.IsTextareaElement(stringTransfer.HtmlElementId))
				{
					object[] oaParams = new object[] { stringTransfer.HtmlElementId, nFoundIndex, nLengthToSelect };
					doc.InvokeScript("textboxSetSelection", oaParams);
				}
				else if (AdaptorConNote.IsParagraphElement(stringTransfer.HtmlElementId))
				{
					HtmlElement elem = doc.GetElementById(stringTransfer.HtmlElementId);
					if (elem != null)
					{
						string str = stringTransfer.ToString();
						str = str.Insert(nFoundIndex + nLengthToSelect, CstrParagraphHighlightEnd);
						str = str.Insert(nFoundIndex, CstrParagraphHighlightBegin);
						System.Diagnostics.Debug.WriteLine(str);
						elem.InnerHtml = str;
					}
					else
						System.Diagnostics.Debug.Assert(false, "unexpected element id in HTML");
				}
			}
		}

		public bool OnConvertToMentoreeNote(string strId, bool bNeedsApproval)
		{
			return AdaptorConNote.SetDirectionTo(strId, bNeedsApproval, true);
		}

		public bool OnConvertToMentorNote(string strId)
		{
			return AdaptorConNote.SetDirectionTo(strId, false, false);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuAddNote = new System.Windows.Forms.ToolStripMenuItem();
			this.menuAddNoteToSelf = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenu.SuspendLayout();
			this.SuspendLayout();
			//
			// contextMenu
			//
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.menuAddNote,
			this.menuAddNoteToSelf});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(244, 48);
			//
			// menuAddNote
			//
			this.menuAddNote.Name = "menuAddNote";
			this.menuAddNote.Size = new System.Drawing.Size(243, 22);
			this.menuAddNote.Text = "Add note on selected text";
			this.menuAddNote.Click += new System.EventHandler(this.menuAddNote_Click);
			//
			// menuAddNoteToSelf
			//
			this.menuAddNoteToSelf.Name = "menuAddNoteToSelf";
			this.menuAddNoteToSelf.Size = new System.Drawing.Size(243, 22);
			this.menuAddNoteToSelf.Text = "Add note to self on selected text";
			this.menuAddNoteToSelf.Click += new System.EventHandler(this.menuAddNoteToSelf_Click);
			//
			// HtmlConNoteControl
			//
			this.ContextMenuStrip = this.contextMenu;
			this.IsWebBrowserContextMenuEnabled = false;
			this.contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private void menuAddNoteToSelf_Click(object sender, EventArgs e)
		{
			bool bNoteToSelf = true;
			ConNoteAddNote(bNoteToSelf);
		}

		private static Regex regExReadLineNumber = new Regex(@"id=tp_(\d+?)_", RegexOptions.Compiled);

		private void menuAddNote_Click(object sender, EventArgs args)
		{
			bool bNoteToSelf = false;
			ConNoteAddNote(bNoteToSelf);
		}

		Regex regexStripTableBits = new Regex("</?(TD|TR|FONT|TEXTAREA|TBODY|TABLE|BUTTON).*?>", RegexOptions.Compiled | RegexOptions.Singleline);

		private void ConNoteAddNote(bool bNoteToSelf)
		{
			if (Document == null)
				return;

			var htmlDocument = Document.DomDocument as IHTMLDocument2;
			if (htmlDocument == null)
				return;

			var selection = htmlDocument.selection;
			var range = (IHTMLTxtRange) selection.createRange();
			if ((range == null) || String.IsNullOrEmpty(range.htmlText))
				return;

			System.Diagnostics.Debug.WriteLine(range.htmlText);
			var elem = range.parentElement();
			if (elem == null)
				return;

			while (!regExReadLineNumber.IsMatch(elem.innerHTML))
				elem = elem.parentElement;

			var strLineNumber = regExReadLineNumber.Match(elem.innerHTML).Groups[1].Value;
			var nLineNumber = Int32.Parse(strLineNumber);

			var children = elem.children as IHTMLElementCollection;
			if (children == null)
				return;

			var strReferringText = String.Format("<p><i>{0}</i></p>", Localizer.Str("Re: ConNote:"));

			// add the selection to the referring text, but strip out any bits which look like table parts
			//  (they don't add so easily)
			var strExtra = regexStripTableBits.Replace(range.htmlText, "");
			strReferringText += strExtra;
#if false
			var aMarkupService = (IMarkupServices)htmlDocument;
			IMarkupPointer aPointerBegin, aPointerEnd;
			aMarkupService.CreateMarkupPointer(out aPointerBegin);
			aMarkupService.CreateMarkupPointer(out aPointerEnd);
			aMarkupService.MovePointersToRange(range, aPointerBegin, aPointerEnd);
			int pResult;
			aPointerBegin.IsLeftOf(aPointerEnd, out pResult);
			while (pResult > 0)
			{
				IHTMLElement elemThis;
				aPointerBegin.CurrentScope(out elemThis);
				System.Diagnostics.Debug.WriteLine("{0} = {1}", elemThis.tagName, elemThis.outerHTML);
				strReferringText += elemThis.outerHTML;
				aPointerBegin.MoveAdjacentToElement(elemThis, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterEnd);
				aPointerBegin.IsLeftOf(aPointerEnd, out pResult);
			}
#endif

			// var strReferringText = String.Format("<p><i>{0}</i></p><p>{1}</p>", Localizer.Str("Re: ConNote:"), range.htmlText);
			AdaptorConNote.TheSe.SendNoteToCorrectPane(nLineNumber, strReferringText, null, bNoteToSelf);
		}

		private ContextMenuStrip contextMenu;
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	[ComVisible(true)]
	public class HtmlConsultantNotesControl : HtmlConNoteControl, IWebBrowserDisplayConNote
	{
		public void OnVerseLineJump(int nVerseIndex)
		{
			AdaptorConNote.TheSe.FocusOnVerse(nVerseIndex, false, true);
		}

		// this only applies to the Consultant Note pane
		public bool OnApproveNote(string strId)
		{
			return AdaptorConNote.SetDirectionTo(strId, false, true);
		}
	}

	[ComVisible(true)]
	public class HtmlCoachNotesControl : HtmlConNoteControl, IWebBrowserDisplayConNote
	{
		public void OnVerseLineJump(int nVerseIndex)
		{
			AdaptorConNote.TheSe.FocusOnVerse(nVerseIndex, true, false);
		}
	}
}
