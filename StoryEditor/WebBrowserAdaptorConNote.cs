using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetLoc;

namespace OneStoryProjectEditor
{
	public abstract class WebBrowserAdaptorConNote : WebBrowserAdaptor
	{
		public new IWebBrowserDisplayConNote Browser
		{
			get
			{
				return (_whichBrowser == WhichBrowser.InternetExplorer)
						   ? (IWebBrowserDisplayConNote)IeWebBrowser
						   : (IWebBrowserDisplayConNote)GeckoWebBrowser;
			}
		}

		public override StoryData StoryData
		{
			set
			{
				Debug.Assert((value == null) || (TheSe != null));
				base.StoryData = value;
				if (value == null)
					return;

				// for ConNotes, we also have to do the 'insure extra box' thingy
				//  (there are actually one more verses than 'Count', but DataConverter(i)
				//  handles that for us)
				for (int i = 0; i <= StoryData.Verses.Count; i++)
				{
					var aCNsDC = DataConverter(i);
					foreach (var dc in aCNsDC)
						aCNsDC.InsureExtraBox(dc, TheSe.TheCurrentStory,
								TheSe.LoggedOnMember, TheSe.StoryProject.TeamMembers);
				}
			}
		}

		public bool OnAddNote(int nVerseIndex, string strReferringText, string strNote, bool bNoteToSelf)
		{
			var eNoteType = (bNoteToSelf)
								? ConsultNoteDataConverter.NoteType.NoteToSelf
								: ConsultNoteDataConverter.NoteType.RegularNote;
			return CallDoAddNote(nVerseIndex, strReferringText, strNote, eNoteType);
		}

		// this form is called from VersesData.GetHeaderRow (html)
		public bool OnAddNoteToSelf(string strButtonId)
		{
			var astrId = strButtonId.Split('_');
			System.Diagnostics.Debug.Assert(astrId.Length == 2);
			var nVerseIndex = Convert.ToInt32(astrId[1]);
			return CallDoAddNote(nVerseIndex, null, null, ConsultNoteDataConverter.NoteType.NoteToSelf);
		}

		public bool OnAddStickyNote(string strButtonId)
		{
			return CallDoAddNote(0, null, null, ConsultNoteDataConverter.NoteType.StickyNote);
		}

		private bool CallDoAddNote(int nVerseIndex, string strReferringText, string strNote, ConsultNoteDataConverter.NoteType eNoteType)
		{
			// StrIdToScrollTo = GetNextRowId;
			var aCNsDC = DataConverter(nVerseIndex);
			var aCNDC = DoAddNote(strReferringText, strNote, aCNsDC, nVerseIndex, eNoteType);

			// if we couldn't determine the top-most row, then just get the line row
			// if ((aCNDC != null) && String.IsNullOrEmpty(StrIdToScrollTo))
			if (aCNDC != null)
				StrIdToScrollTo = ConsultNoteDataConverter.ButtonRowId(nVerseIndex, aCNsDC.IndexOf(aCNDC));

			return true;
		}

		public ConsultNoteDataConverter DoAddNote(string strReferringText, string strNote,
			ConsultNotesDataConverter aCNsDC, int nVerseIndex, ConsultNoteDataConverter.NoteType eNoteType)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSE;
			if (!CheckForProperEditToken(aCNsDC, out theSE))
				return null;

			// if we're not given anything to put in the box, at least put in the logged
			//  in member's initials and re
			// (but not if we're pasting)
			if (String.IsNullOrEmpty(strNote) &&
				(theSE.LoggedOnMember != null) &&
				(StoryEditor.TextPaster == null) &&
				(eNoteType != ConsultNoteDataConverter.NoteType.StickyNote))
				strNote = StoryEditor.GetInitials(theSE.LoggedOnMember.Name) + StoryEditor.StrRegarding;

			ConsultNoteDataConverter cndc =
				aCNsDC.Add(theSE.TheCurrentStory, theSE.LoggedOnMember,
				theSE.StoryProject.TeamMembers, strReferringText, strNote, eNoteType);
			System.Diagnostics.Debug.Assert(cndc.Count == 1);

			// if there's referring text, then do it in a separate dialog so we can 'preview' the referring text
			if (!String.IsNullOrEmpty(strReferringText))
			{
				var nConversationIndex = aCNsDC.IndexOf(cndc);
				cndc.DontShowButtonsOverride = true;
				strNote = StoryData.ConNoteHtml(this, theSE.StoryProject.ProjSettings, nVerseIndex,
												nConversationIndex, theSE.LoggedOnMember,
												theSE.StoryProject.TeamMembers, cndc);
				var dlg = new AddConNoteForm(GetType(), theSE, StoryData, strNote);
				if (dlg.ShowDialog() != DialogResult.OK)
				{
					aCNsDC.Remove(cndc);
					return null;
				}
				cndc.DontShowButtonsOverride = false;
			}

			// StrIdToScrollTo = Browser.GetTopRowId;
			LoadDocument();
			theSE.Modified = true;

			CheckUpdateMentorInfo(theSE);

			// return the conversation we just created
			return cndc;
		}

		public bool GetDataConverters(string strId, out int nVerseIndex, out int nConversationIndex,
			out ConsultNotesDataConverter theCNsDC, out ConsultNoteDataConverter theCNDC)
		{
			theCNsDC = null;
			theCNDC = null;

			int nDontCare;
			if (!GetIndicesFromId(strId, out nVerseIndex, out nConversationIndex, out nDontCare))
				return false;

			theCNsDC = DataConverter(nVerseIndex);
			System.Diagnostics.Debug.Assert((theCNsDC != null) && (theCNsDC.Count > nConversationIndex));

			/* I shouldn't need this anymore, because if it's not allowed, then there shouldn't
			 * be buttons visible to get here
			StoryEditor theSE;
			if (!CheckForProperEditToken(theCNsDC, out theSE))
				return false;
			*/

			theCNDC = theCNsDC[nConversationIndex];

			// this always leads to the document being modified
			TheSe.Modified = true;

			return true;
		}

		public bool GetIndicesFromId(string strId,
			out int nVerseIndex, out int nConversationIndex, out int nCommentIndex)
		{
			nCommentIndex = 0;
			try
			{
				string[] aVerseConversationIndices = strId.Split(AchDelim);
				System.Diagnostics.Debug.Assert(((aVerseConversationIndices.Length == 3) ||
												 (aVerseConversationIndices.Length == 4))
												&&
												((aVerseConversationIndices[0] == CstrTextAreaPrefix) ||
												 (aVerseConversationIndices[0] == CstrParagraphPrefix) ||
												 (aVerseConversationIndices[0] == CstrButtonPrefix)));

				nVerseIndex = Convert.ToInt32(aVerseConversationIndices[1]);
				nConversationIndex = Convert.ToInt32(aVerseConversationIndices[2]);
				if (aVerseConversationIndices.Length == 4)
					nCommentIndex = Convert.ToInt32(aVerseConversationIndices[3]);
			}
			catch
			{
				nVerseIndex = 0;
				nConversationIndex = 0;
				return false;
			}
			return true;
		}

		public bool TextareaOnKeyUp(string strId, string strText)
		{
			int nVerseIndex, nConversationIndex, nDontCare;
			if (!GetIndicesFromId(strId, out nVerseIndex, out nConversationIndex, out nDontCare))
				return false;

			ConsultNotesDataConverter theCNsDC = DataConverter(nVerseIndex);
			StoryEditor theSE;
			if (!CheckForProperEditToken(theCNsDC, out theSE))
				return false;

			ConsultNoteDataConverter theCNDC = theCNsDC[nConversationIndex];
			System.Diagnostics.Debug.Assert((theCNDC != null) && (theCNDC.Count > 0));

			CommInstance aCI = theCNDC.FinalComment;
			aCI.SetValue(strText);

			// indicate that the document has changed
			theSE.Modified = true;
			theSE.LastKeyPressedTimeStamp = DateTime.Now;   // so we can delay the autosave while typing

			// update the status bar (in case we previously put an error there
			StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[theSE.TheCurrentStory.ProjStage.ProjectStage];
			theSE.SetDefaultStatusBar(st.StageDisplayString);

			return true;
		}

		public abstract ConsultNotesDataConverter DataConverter(int nVerseIndex);
		protected abstract void CheckUpdateMentorInfo(StoryEditor theSe);
		public abstract string PaneLabel();

		public ConsultNoteDataConverter DataConverter(int nVerseIndex, int nConversationIndex)
		{
			ConsultNotesDataConverter aCNsDC = DataConverter(nVerseIndex);
			System.Diagnostics.Debug.Assert(aCNsDC.Count > nConversationIndex);
			return aCNsDC[nConversationIndex];
		}

		protected bool CheckForProperEditToken(ConsultNotesDataConverter theCNsDC,
			out StoryEditor theSE)
		{
			theSE = TheSe;  // (StoryEditor)FindForm();
			try
			{
				if (theSE == null)
					throw new ApplicationException(
						Localizer.Str("Unable to edit the file! Restart the program and if it persists, contact bob_eaton@sall.com"));

				if (!theSE.IsInStoriesSet)
					throw theSE.CantEditOldStoriesEx;

				string strPfMemberId = null;
				if (theSE.TheCurrentStory != null)
					strPfMemberId = theSE.TheCurrentStory.CraftingInfo.ProjectFacilitator.MemberId;
				if (theCNsDC.HasAddNotePrivilege(theSE.LoggedOnMember, strPfMemberId))
					return true;

				theSE.LoggedOnMember.ThrowIfEditIsntAllowed(theSE.TheCurrentStory);
			}
			catch (Exception ex)
			{
				if (theSE != null)
					theSE.SetStatusBar(String.Format(Localizer.Str("Error: {0}"), ex.Message));
				return false;
			}

			return true;
		}

		public void DoFind(string strId)
		{
			if (TheSe.IsStoryBtPaneHtml)
				return;

			int nVerseIndex, nConversationIndex, nCommentIndex;
			if (!GetIndicesFromId(strId, out nVerseIndex, out nConversationIndex, out nCommentIndex))
				return;

			ConsultNotesDataConverter theCNsDC = DataConverter(nVerseIndex);
			System.Diagnostics.Debug.Assert((theCNsDC != null) && (theCNsDC.Count > nConversationIndex));
			ConsultNoteDataConverter theCNDC = theCNsDC[nConversationIndex];
			System.Diagnostics.Debug.Assert((theCNDC != null) && (theCNDC.Count > nCommentIndex));
			SearchForm.LastStringTransferSearched = theCNDC[nCommentIndex];
			TheSe.LaunchSearchForm();
		}

		public bool OnClickHide(string strId)
		{
			int nVerseIndex, nConversationIndex;
			ConsultNotesDataConverter theCNsDC;
			ConsultNoteDataConverter theCNDC;
			if (!GetDataConverters(strId, out nVerseIndex, out nConversationIndex,
				out theCNsDC, out theCNDC))
				return false;

			// if there's only one and it's empty, then just delete it
			if (!theCNDC.HasData)
				OnClickDelete(strId);

			StrIdToScrollTo = Browser.GetTopRowId;
			theCNDC.Visible = (theCNDC.Visible) ? false : true;

			// otherwise, we have to reload the document
			LoadDocument();
			return true;
		}

		public bool OnClickDelete(string strId)
		{
			int nVerseIndex, nConversationIndex;
			ConsultNotesDataConverter theCNsDC;
			ConsultNoteDataConverter theCNDC;
			if (!GetDataConverters(strId, out nVerseIndex, out nConversationIndex,
				out theCNsDC, out theCNDC))
				return false;

			if (theCNDC.HasData)
			{
				DialogResult res = LocalizableMessageBox.Show(Localizer.Str("This conversation isn't empty! Instead of deleting it, it would be better to just hide it so it will be left around for history. Click 'Yes' to hide the conversation or click 'No' to delete it?"),
					StoryEditor.OseCaption, MessageBoxButtons.YesNoCancel);

				if (res == DialogResult.Yes)
					return OnClickHide(strId);

				if (res == DialogResult.Cancel)
					return true;
			}

			StrIdToScrollTo = Browser.GetTopRowId;
			bool bRemovedLast = (theCNsDC.IndexOf(theCNDC) == (theCNsDC.Count - 1));
			theCNsDC.Remove(theCNDC);

			// remove the HTML elements for the row of buttons and the conversation table
			//  (but only if it was the last conversation. If it wasn't, then the other
			//  conversations will have out of sequence ids, so we'll just *have* to do
			//  LoadDoc
			if (bRemovedLast
				&&
				Browser.RemoveHtmlNodeById(ConsultNoteDataConverter.ButtonRowId(nVerseIndex, nConversationIndex))
				&&
				Browser.RemoveHtmlNodeById(ConsultNoteDataConverter.ConversationTableRowId(nVerseIndex, nConversationIndex)))
			{
				return true;
			}

			LoadDocument();
			return true;
		}

		public bool SetDirectionTo(string strId,
			bool bNeedsApproval, bool bToMentorDirection)
		{
			int nVerseIndex, nConversationIndex;
			ConsultNotesDataConverter theCNsDC;
			ConsultNoteDataConverter theCNDC;
			if (!GetDataConverters(strId, out nVerseIndex, out nConversationIndex,
								   out theCNsDC, out theCNDC))
				return false;

			theCNDC.FinalComment.Direction = (bToMentorDirection)
												 ? (bNeedsApproval)
													   ? ConsultNoteDataConverter.CommunicationDirections.
															 eConsultantToProjFacNeedsApproval
													   : theCNDC.MentorDirection
												 : theCNDC.MenteeDirection;
			StrIdToScrollTo = Browser.GetTopRowId;
			LoadDocument();
			return true;
		}
	}

	public class WebBrowserAdaptorConsultantNotes : WebBrowserAdaptorConNote
	{
		#region Overrides of WebBrowserAdaptor

		private HtmlVerseControl _myIeBrowser;
		protected override HtmlVerseControl MyIeBrowser
		{
			get
			{
				return _myIeBrowser ?? (_myIeBrowser = new HtmlConsultantNotesControl
				{
					AllowWebBrowserDrop = false,
					Dock = DockStyle.Fill,
					IsWebBrowserContextMenuEnabled = false,
					Location = new Point(0, 23),
					MinimumSize = new Size(20, 20),
					Name = "htmlConsultantNotesControl",
					Size = new Size(422, 331),
					AdaptorConNote = this,
					TabIndex = 2
				});
			}
		}

		private GeckoDisplayControl _myGeckoBrowser;
		protected override GeckoDisplayControl MyGeckoBrowser
		{
			get
			{
				return _myGeckoBrowser ?? (_myGeckoBrowser = new GeckoConsultantNotesControl
				{
					DisableWmImeSetContext = false,
					Dock = System.Windows.Forms.DockStyle.Fill,
					Location = new System.Drawing.Point(0, 23),
					Name = "geckoConsultantNotesControl",
					Size = new System.Drawing.Size(422, 331),
					TabIndex = 4,
					AdaptorConNote = this,
					UseHttpActivityObserver = false
				});
			}
		}

		public override void LoadDocument()
		{
			var strHtml = StoryData.ConsultantNotesHtml(this,
														TheSe.StoryProject.ProjSettings,
														TheSe.LoggedOnMember,
														TheSe.StoryProject.TeamMembers,
														TheSe.viewHiddenVersesMenu.Checked,
														TheSe.viewOnlyOpenConversationsMenu.Checked);
			Browser.LoadDocument(strHtml);
			LineNumberLink.Visible = true;
		}

		public override ConsultNotesDataConverter DataConverter(int nVerseIndex)
		{
			var verse = GetVerseData(nVerseIndex);
			var aCNsDC = verse.ConsultantNotes;
			return aCNsDC;
		}

		protected override void CheckUpdateMentorInfo(StoryEditor theSe)
		{
			theSe.CheckUpdateMentorInfoConsultant();
		}

		public override string PaneLabel()
		{
			return Localizer.Str("Consultant Notes");
		}

		#endregion
	}

	public class WebBrowserAdaptorCoachNotes : WebBrowserAdaptorConNote
	{
		#region Overrides of WebBrowserAdaptor

		private HtmlVerseControl _myIeBrowser;
		protected override HtmlVerseControl MyIeBrowser
		{
			get
			{
				return _myIeBrowser ?? (_myIeBrowser = new HtmlCoachNotesControl
				{
					AllowWebBrowserDrop = false,
					Dock = DockStyle.Fill,
					IsWebBrowserContextMenuEnabled = false,
					Location = new Point(0, 23),
					MinimumSize = new Size(20, 20),
					Name = "htmlCoachNotesControl",
					Size = new Size(422, 228),
					AdaptorConNote = this,
					TabIndex = 3
				});
			}
		}

		private GeckoDisplayControl _myGeckoBrowser;
		protected override GeckoDisplayControl MyGeckoBrowser
		{
			get
			{
				return _myGeckoBrowser ?? (_myGeckoBrowser = new GeckoCoachNotesControl
				{
					DisableWmImeSetContext = false,
					Dock = System.Windows.Forms.DockStyle.Fill,
					Location = new System.Drawing.Point(0, 23),
					Name = "geckoCoachNotesControl",
					Size = new System.Drawing.Size(422, 228),
					TabIndex = 5,
					AdaptorConNote = this,
					UseHttpActivityObserver = false
				});
			}
		}

		public override void LoadDocument()
		{
			var strHtml = StoryData.CoachNotesHtml(this,
												   TheSe.StoryProject.ProjSettings,
												   TheSe.LoggedOnMember,
												   TheSe.StoryProject.TeamMembers,
												   TheSe.viewHiddenVersesMenu.Checked,
												   TheSe.viewOnlyOpenConversationsMenu.Checked);

			Browser.LoadDocument(strHtml);

			LineNumberLink.Visible = true;
		}

		public override ConsultNotesDataConverter DataConverter(int nVerseIndex)
		{
			var verse = GetVerseData(nVerseIndex);
			var aCNsDC = verse.CoachNotes;
			return aCNsDC;
		}

		protected override void CheckUpdateMentorInfo(StoryEditor theSe)
		{
			theSe.CheckUpdateMentorInfoCoach();
		}

		public override string PaneLabel()
		{
			return Localizer.Str("Coach Notes");
		}

		#endregion
	}
}
