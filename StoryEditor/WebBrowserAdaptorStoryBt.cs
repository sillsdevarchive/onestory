using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetLoc;
using Palaso.UI.WindowsForms.Keyboarding;

namespace OneStoryProjectEditor
{
	public class WebBrowserAdaptorStoryBt : WebBrowserAdaptor
	{
		#region Overrides of WebBrowserAdaptor

		public IWebBrowserDisplayStoryBt Browser
		{
			get
			{
				return (_whichBrowser == WhichBrowser.InternetExplorer)
						   ? (IWebBrowserDisplayStoryBt)IeWebBrowser
						   : (IWebBrowserDisplayStoryBt)GeckoWebBrowser;
			}
		}

		private HtmlVerseControl _myIeBrowser;
		protected override HtmlVerseControl MyIeBrowser
		{
			get
			{
				if (_myIeBrowser == null)
				{
					_myIeBrowser = new HtmlStoryBtControl
					{
						AllowWebBrowserDrop = false,
						Dock = DockStyle.Fill,
						IsWebBrowserContextMenuEnabled = false,
						Location = new Point(0, 23),
						MinimumSize = new Size(20, 20),
						Name = "htmlStoryBtControl",
						AdaptorStoryBt = this,
						Size = new Size(451, 366),
						TabIndex = 5
					};
					ResetContextMenus();
				}

				return _myIeBrowser;
			}
		}

		private GeckoDisplayControl _myGeckoBrowser;
		protected override GeckoDisplayControl MyGeckoBrowser
		{
			get
			{
				return _myGeckoBrowser ?? (_myGeckoBrowser = new GeckoStoryBtDisplayControl
				{
					DisableWmImeSetContext = false,
					Dock = DockStyle.Fill,
					Location = new Point(0, 23),
					Name = "geckoStoryBtDisplay",
					Size = new Size(451, 366),
					AdaptorStoryBt = this,
					TabIndex = 0,
					UseHttpActivityObserver = false
				});
			}
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

			BrowserDisplay.LoadDocument(strHtml);
		}

		public StringTransfer GetStringTransferEx(TextAreaIdentifier textAreaIdentifier)
		{
			var verseData = GetVerseData(textAreaIdentifier.LineIndex);

			LineData lineData;
			StringTransfer stField = null;
			switch (textAreaIdentifier.FieldType)
			{
				case StoryEditor.TextFields.StoryLine:
					lineData = verseData.StoryLine;
					break;

				case StoryEditor.TextFields.ExegeticalNote:
					stField = verseData.ExegeticalHelpNotes[textAreaIdentifier.ItemIndex];

					// since we're returning right away, we have to do this now.
					stField.HtmlElementId = textAreaIdentifier.HtmlIdentifier;
					return stField;

				case StoryEditor.TextFields.Retelling:
					lineData = verseData.Retellings[textAreaIdentifier.SubItemIndex];
					break;

				case StoryEditor.TextFields.TestQuestion:
					lineData = verseData.TestQuestions[textAreaIdentifier.ItemIndex].TestQuestionLine;
					break;

				case StoryEditor.TextFields.TestQuestionAnswer:
					// the sub-index seems to reflect the test number; not necessarily the offset into "Answers".
					lineData = TheSe.GetTqAnswerData(
						verseData.TestQuestions[textAreaIdentifier.ItemIndex].Answers,
						(textAreaIdentifier.SubItemIndex + 1).ToString());
					break;

				default:
					return null;
			}

			System.Diagnostics.Debug.Assert(lineData != null);
			var languageColumn =
				(StoryEditor.TextFields)Enum.Parse(typeof(StoryEditor.TextFields), textAreaIdentifier.LanguageColumnName);
			switch (languageColumn)
			{
				case StoryEditor.TextFields.Vernacular:
					stField = lineData.Vernacular;
					break;
				case StoryEditor.TextFields.NationalBt:
					stField = lineData.NationalBt;
					break;
				case StoryEditor.TextFields.InternationalBt:
					stField = lineData.InternationalBt;
					break;
				case StoryEditor.TextFields.FreeTranslation:
					stField = lineData.FreeTranslation;
					break;
			}

			if (stField != null)
				stField.HtmlElementId = textAreaIdentifier.HtmlIdentifier;
			return stField;
		}

		public static StoryEditor.Transliterators Transliterators { get; set; }
		public VerseData.ViewSettings ViewSettings { get; set; }
		public StoryData ParentStory { get; set; }

		#endregion

		public bool CheckForProperEditToken(out StoryEditor theSE)
		{
			theSE = TheSe;
			try
			{
				if (theSE == null)
					throw new ApplicationException(
						Localizer.Str("Unable to edit the file! Restart the program and if it persists, contact bob_eaton@sall.com"));

				if (!theSE.IsInStoriesSet)
					throw theSE.CantEditOldStoriesEx;

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

		private void ReloadAllWindows()
		{
			TheSe.Modified = true;
			var lastTop = BrowserDisplay.GetTopRowId;
			TheSe.InitAllPanes();
			StrIdToScrollTo = lastTop;  // because InitAllPanes will have clobbered it
		}

		public string GetSelectedText
		{
			get
			{
				if (String.IsNullOrEmpty(LastTextareaInFocusId))
					return null;

				TextAreaIdentifier textAreaIdentifier;
				if (!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
					return null;

				StoryEditor.TextFields whichLanguage;
				return Browser.GetSelectedTextByTextareaIdentifier(textAreaIdentifier, out whichLanguage);
			}
		}

		private StringTransfer GetStringTransfer(string strId)
		{
			TextAreaIdentifier textAreaIdentifier;
			if (!TryGetTextAreaId(strId, out textAreaIdentifier))
				return null;

			var stringTransfer = GetStringTransfer(textAreaIdentifier);
			return stringTransfer;
		}

		private StringTransfer GetStringTransfer(TextAreaIdentifier textAreaIdentifier)
		{
			// this might fail if the screen doesn't match what's in the internal buffer (e.g. a
			//  ExegeticalHelpNote doesn't exist in verseData.ExegeticalHelpNotes so [] throws an exception)
			// Not really sure how this can happen, but it does
			try
			{
				return GetStringTransferEx(textAreaIdentifier);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public StringTransfer GetStringTransferOfLastTextAreaInFocus
		{
			get
			{
				return String.IsNullOrEmpty(LastTextareaInFocusId) ? null : GetStringTransfer(LastTextareaInFocusId);
			}
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

		public static DialogResult QueryAboutHidingVerseInstead()
		{
			return LocalizableMessageBox.Show(
				String.Format(Localizer.Str("This line isn't empty! Instead of deleting it, it would be better to just hide it so it will be left around to know what it used to be.{0}{0}Click 'Yes' to hide the line or click 'No' to delete it?"),
							  Environment.NewLine),
				StoryEditor.OseCaption, MessageBoxButtons.YesNoCancel);
		}

		public static bool UserConfirmDeletion
		{
			get
			{
				return (LocalizableMessageBox.Show(
					Localizer.Str("Are you sure you want to delete this line (and all associated consultant notes, etc)?"),
					StoryEditor.OseCaption,
					MessageBoxButtons.YesNoCancel) == DialogResult.Yes);
			}
		}

		protected static VerseData _myClipboard = null;
		protected VerseData PasteVerseToIndex(StoryEditor theSe, int nInsertionIndex)
		{
			if (_myClipboard != null)
			{
				var theNewVerse = new VerseData(_myClipboard);
				theNewVerse.AllowConNoteButtonsOverride();
				// make another copy, so that the guid is changed
				theSe.DoPasteVerse(nInsertionIndex, theNewVerse);
				return theNewVerse;
			}
			return null;
		}

		#region ContextMenuTextarea
		private ContextMenuStrip _contextMenuTextarea;

		public void ResetContextMenus()
		{
			_contextMenuTextarea = CreateContextMenuTextarea();
			_contextMenuStripLineOptions = CreateContextMenuLineOptions();
			_contextMenuStripAnchorOptions = CreateContextMenuAnchorOptions();
		}

		public void ShowContextMenu(string strId)
		{
			if (IsButtonElement(strId))
				;
			else if (IsTextareaElement(strId))
			{
				LastTextareaInFocusId = strId;
				// done by js TriggerOnBlur(Document);
				_contextMenuTextarea.Show(MousePosition);
			}
		}

		private ContextMenuStrip CreateContextMenuTextarea()
		{
			var ctxMenu = new ContextMenuStrip();
			ctxMenu.Items.Add(StoryEditor.CstrAddNoteOnSelected, null, OnAddNewNote);
			ctxMenu.Items.Add(StoryEditor.CstrAddNoteToSelfOnSelected, null, OnAddNoteToSelf);
			ctxMenu.Items.Add(new ToolStripSeparator());
			ctxMenu.Items.Add(StoryEditor.CstrGlossTextToNational, null, OnGlossTextToNational);
			ctxMenu.Items.Add(StoryEditor.CstrGlossTextToEnglish, null, OnGlossTextToEnglish);
			ctxMenu.Items.Add(StoryEditor.CstrReorderWords, null, onReorderWords);
			ctxMenu.Items.Add(StoryEditor.CstrConcordanceSearch, null, OnConcordanceSearch);
			ctxMenu.Items.Add(StoryEditor.CstrAddLnCNote, null, OnAddLnCNote);
			ctxMenu.Items.Add(new ToolStripSeparator());
			ctxMenu.Items.Add(StoryEditor.CstrAddAnswerBox, null, onAddAnswerBox);
			ctxMenu.Items.Add(StoryEditor.CstrRemAnswerBox, null, onRemAnswerBox);
			ctxMenu.Items.Add(StoryEditor.CstrRemAnswerChangeUns, null, onChangeUns);

			ctxMenu.Items.Add(new ToolStripSeparator());
			ctxMenu.Items.Add(StoryEditor.CstrCutSelected, null, onCutSelectedText);
			ctxMenu.Items.Add(StoryEditor.CstrCopySelected, null, onCopySelectedText);
			ctxMenu.Items.Add(StoryEditor.CstrCopyOriginalSelected, null, onCopyOriginalText);
			ctxMenu.Items.Add(StoryEditor.CstrPasteSelected, null, onPasteSelectedText);
			// ctxMenu.Items.Add(StoryEditor.CstrUndo, null, onUndo);

			ctxMenu.Opening += CtxMenuOpening;
			return ctxMenu;
		}

		void CtxMenuOpening(object sender, CancelEventArgs e)
		{
			var myStringTransfer = GetStringTransferOfLastTextAreaInFocus;
			var hasStringTransfer = (myStringTransfer != null);

			// don't ask... I'm not sure why Items.ContainsKey isn't finding this...
			foreach (ToolStripItem x in _contextMenuTextarea.Items)
			{
				if (x.Text == StoryEditor.CstrCopyOriginalSelected)
				{
					x.Enabled = (hasStringTransfer && (myStringTransfer.Transliterator != null));
				}
				else if (x.Text == StoryEditor.CstrReorderWords)
				{
					x.Enabled = hasStringTransfer;
				}
				else if (x.Text == StoryEditor.CstrGlossTextToNational)
				{
					var nationalBtSiblingId = GetMyNationalBtSibling(LastTextareaInFocusId);
					x.Visible = ((nationalBtSiblingId != null) && (nationalBtSiblingId != LastTextareaInFocusId));
					if (nationalBtSiblingId != null)
					{
						x.Enabled = Browser.DoesElementIdExist(nationalBtSiblingId);
					}
				}
				else if (x.Text == StoryEditor.CstrGlossTextToEnglish)
				{
					var englishBtSibling = GetMyInternationalBtSibling(LastTextareaInFocusId);
					x.Visible = ((englishBtSibling != null) && (englishBtSibling != LastTextareaInFocusId));
					if (englishBtSibling != null)
					{
						x.Enabled = Browser.DoesElementIdExist(englishBtSibling);
					}
				}
				else if (x.Text == StoryEditor.CstrAddLnCNote)
				{
					CheckForLnCNoteLookup((ToolStripMenuItem)x);
				}
				else if (x.Text == StoryEditor.CstrConcordanceSearch)
				{
					x.Visible = (LastTextareaInFocusId.IndexOf(StoryEditor.TextFields.StoryLine.ToString(),
															   StringComparison.Ordinal) != -1);
				}
				else if (x.Text == StoryEditor.CstrAddAnswerBox)
				{
					x.Visible = ShouldTqPopupsBeVisible;
				}
				else if ((x.Text == StoryEditor.CstrRemAnswerBox) || (x.Text == StoryEditor.CstrRemAnswerChangeUns))
				{
					x.Visible = ShouldAnsPopupsBeVisible;
				}
				else if ((x.Text == StoryEditor.CstrCutSelected) ||
						 (x.Text == StoryEditor.CstrCopySelected) ||
						 (x.Text == StoryEditor.CstrCopyOriginalSelected))
				{
					x.Enabled = !String.IsNullOrEmpty(GetSelectedText);
				}
				else if (x.Text == StoryEditor.CstrPasteSelected)
				{
					x.Enabled = (hasStringTransfer && !myStringTransfer.IsFieldReadonly(ViewSettings.FieldEditibility));
				}
			}
		}

		protected bool ShouldTqPopupsBeVisible
		{
			get
			{
				TextAreaIdentifier textAreaIdentifier;
				if (String.IsNullOrEmpty(LastTextareaInFocusId) || !TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
					return false;

				return (textAreaIdentifier.FieldType == StoryEditor.TextFields.TestQuestion);
			}
		}

		protected bool ShouldAnsPopupsBeVisible
		{
			get
			{
				TextAreaIdentifier textAreaIdentifier;
				if (String.IsNullOrEmpty(LastTextareaInFocusId) || !TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
					return false;

				return (textAreaIdentifier.FieldType == StoryEditor.TextFields.TestQuestionAnswer);
			}
		}

		private void OnAddNewNote(object sender, EventArgs e)
		{
			Browser.AddNote(false);
		}

		private void OnAddNoteToSelf(object sender, EventArgs e)
		{
			Browser.AddNote(true);
		}

		private void OnGlossTextToNational(object sender, EventArgs e)
		{
			OnDoGlossing(GetMyNationalBtSibling, TheSe.StoryProject.ProjSettings.NationalBT,
						 ProjectSettings.AdaptItConfiguration.AdaptItBtDirection.VernacularToNationalBt);
		}

		private void OnGlossTextToEnglish(object sender, EventArgs e)
		{
			var st = GetStringTransferOfLastTextAreaInFocus;
			if (st == null)
				return;

			OnDoGlossing(GetMyInternationalBtSibling, TheSe.StoryProject.ProjSettings.InternationalBT,
						 ((st.WhichField & StoryEditor.TextFields.Vernacular) == StoryEditor.TextFields.Vernacular)
							 ? ProjectSettings.AdaptItConfiguration.AdaptItBtDirection.VernacularToInternationalBt
							 : ProjectSettings.AdaptItConfiguration.AdaptItBtDirection.NationalBtToInternationalBt);
		}

		public delegate string GetSiblingId(string strId);

		private void OnDoGlossing(GetSiblingId mySiblingIdGetter, ProjectSettings.LanguageInfo liSibling,
			ProjectSettings.AdaptItConfiguration.AdaptItBtDirection adaptItBtDirection)
		{
			try
			{
				var siblingId = mySiblingIdGetter(LastTextareaInFocusId);
				if (siblingId == null)
					return;

				if (!Browser.DoesElementIdExist(siblingId))
					return;

				var myStringTransfer = GetStringTransferOfLastTextAreaInFocus;
				if ((myStringTransfer == null) || !myStringTransfer.HasData)
					return;

				var dlg = new GlossingForm(TheSe.StoryProject.ProjSettings,
										   myStringTransfer.ToString(),
										   adaptItBtDirection,
										   TheSe.LoggedOnMember,
										   TheSe.advancedUseWordBreaks.Checked,
										   myStringTransfer.Transliterator);

				if (dlg.ShowDialog() != DialogResult.OK)
					return;

				StoryEditor theSe;
				if (!CheckForProperEditToken(out theSe))
					return;

				var siblingStringTransfer = GetStringTransfer(siblingId);
				if (siblingStringTransfer == null)
					return;

				if (siblingStringTransfer.ToString() != dlg.TargetSentence)
					siblingStringTransfer.SetValue(dlg.TargetSentence);

				// check whether we need to update the source as well (user may have changed it)...
				//  but only update the source data if it wasn't being transliterated
				if (myStringTransfer.Transliterator == null)
				{
					if (myStringTransfer.ToString() != dlg.SourceSentence)
						myStringTransfer.SetValue(dlg.SourceSentence);
				}

				TheSe.Modified = true;
				if (dlg.DoReorder)
				{
					var dlgReorder = new ReorderWordsForm(siblingStringTransfer,
														  liSibling.FontToUse,
														  liSibling.FullStop);
					if (dlgReorder.ShowDialog() == DialogResult.OK)
						siblingStringTransfer.SetValue(dlgReorder.ReorderedText);
				}

				StrIdToScrollTo = BrowserDisplay.GetTopRowId;
				LoadDocument();
			}
			catch (Exception ex)
			{
				LocalizableMessageBox.Show(ex.Message, StoryEditor.OseCaption);
			}
		}

		private void onReorderWords(object sender, EventArgs e)
		{
			StoryEditor theSe;
			TextAreaIdentifier textAreaIdentifier;
			if (!CheckForProperEditToken(out theSe) ||
				String.IsNullOrEmpty(LastTextareaInFocusId) ||
				!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
				return;

			var st = GetStringTransfer(textAreaIdentifier);
			if (st == null)
				return;

			var li = textAreaIdentifier.GetLanguageInfo(TheSe.StoryProject.ProjSettings);
			var dlg = new ReorderWordsForm(st, li.FontToUse, li.FullStop);
			if (dlg.ShowDialog() == DialogResult.OK)
				st.SetValue(dlg.ReorderedText);

			StrIdToScrollTo = BrowserDisplay.GetTopRowId;
			LoadDocument();
		}

		private void OnConcordanceSearch(object sender, EventArgs e)
		{
			TheSe.concordanceToolStripMenuItem_Click(null, null);
		}

		private void OnAddLnCNote(object sender, EventArgs e)
		{
			TheSe.AddLnCNote();
		}

		private void onAddAnswerBox(object sender, EventArgs e)
		{
			StoryEditor theSe;
			TextAreaIdentifier textAreaIdentifier;
			if (!CheckForProperEditToken(out theSe) ||
				String.IsNullOrEmpty(LastTextareaInFocusId) ||
				!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
				return;

			var verseData = GetVerseData(textAreaIdentifier.LineIndex);
			var testQuestionData = verseData.TestQuestions[textAreaIdentifier.ItemIndex];
			LineMemberData theNewAnswer;
			if (!theSe.AddSingleTestResult(testQuestionData, out theNewAnswer))
				return;

			StrIdToScrollTo = BrowserDisplay.GetTopRowId;
			LoadDocument();
		}

		private void onRemAnswerBox(object sender, EventArgs e)
		{
			StoryEditor theSe;
			TextAreaIdentifier textAreaIdentifier;
			if (!CheckForProperEditToken(out theSe) ||
				String.IsNullOrEmpty(LastTextareaInFocusId) ||
				!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
				return;

			var verseData = GetVerseData(textAreaIdentifier.LineIndex);

			System.Diagnostics.Debug.Assert(textAreaIdentifier.ItemIndex < verseData.TestQuestions.Count);
			var testQuestionData = verseData.TestQuestions[textAreaIdentifier.ItemIndex];
			var answers = testQuestionData.Answers;

			var answerToRemove = theSe.GetTqAnswerData(answers, (textAreaIdentifier.SubItemIndex + 1).ToString());
			answers.Remove(answerToRemove);

			theSe.Modified = true;
			StrIdToScrollTo = BrowserDisplay.GetTopRowId;
			LoadDocument();
		}

		private void onChangeUns(object sender, EventArgs e)
		{
			StoryEditor theSe;
			TextAreaIdentifier textAreaIdentifier;
			if (!CheckForProperEditToken(out theSe) ||
				String.IsNullOrEmpty(LastTextareaInFocusId) ||
				!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
				return;

			var verseData = GetVerseData(textAreaIdentifier.LineIndex);

			System.Diagnostics.Debug.Assert(textAreaIdentifier.ItemIndex < verseData.TestQuestions.Count);
			var testQuestionData = verseData.TestQuestions[textAreaIdentifier.ItemIndex];
			var answers = testQuestionData.Answers;

			var answerToChange = theSe.GetTqAnswerData(answers, (textAreaIdentifier.SubItemIndex + 1).ToString());

			theSe.ChangeAnswerBoxUns(testQuestionData, answers, answerToChange);

			StrIdToScrollTo = BrowserDisplay.GetTopRowId;
			LoadDocument();
		}

		private void onCutSelectedText(object sender, EventArgs e)
		{
			StoryEditor theSe;
			TextAreaIdentifier textAreaIdentifier;
			if (!CheckForProperEditToken(out theSe) ||
				String.IsNullOrEmpty(LastTextareaInFocusId) ||
				!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
				return;

			var st = GetStringTransfer(textAreaIdentifier);
			if ((st == null) ||
				!CheckShowErrorOnFieldNotEditable(st) ||
				!Browser.DoesElementIdExist(LastTextareaInFocusId))
				return;

			int nNewEndPoint;
			var selectedText = GetSelectedText;
			Clipboard.SetDataObject(selectedText);
			Browser.SetSelectedText(st, String.Empty, out nNewEndPoint);
			TriggerChangeUpdate();
			theSe.Modified = true;
		}

		private void onCopySelectedText(object sender, EventArgs e)
		{
			TheSe.editCopySelectionToolStripMenuItem_Click(null, null);
		}

		private void onCopyOriginalText(object sender, EventArgs e)
		{
			TextAreaIdentifier textAreaIdentifier;
			if (String.IsNullOrEmpty(LastTextareaInFocusId) ||
				!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
				return;

			var st = GetStringTransfer(textAreaIdentifier);
			if (st == null)
				return;

			Clipboard.SetText(st.ToString(), TextDataFormat.UnicodeText);
		}

		private void onPasteSelectedText(object sender, EventArgs e)
		{
			TheSe.pasteToolStripMenuItem_Click(null, null);
		}

		#endregion // ContextMenuTextarea

		#region ContextMenuLineOptions

		private ContextMenuStrip _contextMenuStripLineOptions;
		private string _lastLineOptionsButtonClicked;

		public bool OnLineOptionsButton(string strId, bool bIsRightButton)
		{
			if (bIsRightButton)
			{
				Browser.TriggerOnBlur();
				_lastLineOptionsButtonClicked = strId;
				_contextMenuStripLineOptions.Show(MousePosition);
				return false;
			}

			return true;
		}

		private static ToolStripItem AddMenuItem(ToolStrip ctxMenu, string strText, string strTooltip, EventHandler handlerClick)
		{
			var tsmi = ctxMenu.Items.Add(strText, null, handlerClick);
			tsmi.ToolTipText = strTooltip;
			return tsmi;
		}

		private ContextMenuStrip CreateContextMenuLineOptions()
		{
			var ctxMenu = new ContextMenuStrip();

			AddMenuItem(ctxMenu,
						Localizer.Str("Move &selected text to a new line"),
						Localizer.Str("This will take the selected text from the Story language, the National BT and the English BT boxes and move them into a new line following this line"),
						MoveSelectedTextToANewLineToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Move items"),
						Localizer.Str("Click this menu to move testing questions, anchors, exegetical notes, etc to another line"),
						MoveItemsToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Delete items"),
						Localizer.Str("Click this menu to delete testing questions, anchors, exegetical notes, etc"),
						DeleteItemsToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("Add a story testing &question"),
						Localizer.Str("Click to add a story testing question"),
						AddTestQuestionClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("Add &Exegetical/Cultural Note below"),
						Localizer.Str("Click this menu to add a 'cn' box to this line for entering an exegetical and/or cultural note below"),
						AddExegeticalCulturalNoteBelowToolStripMenuItemClick);

			ctxMenu.Items.Add(new ToolStripSeparator());

			_moveLineUp = AddMenuItem(ctxMenu,
									 Localizer.Str("Move Line Up"),
									 Localizer.Str("Click this menu to move the line up (i.e. switch places with the line above)"),
									 MoveLineUpClick);

			_moveLineDown = AddMenuItem(ctxMenu,
									   Localizer.Str("Move Line Down"),
									   Localizer.Str("Click this menu to move the line down (i.e. switch places with the line below)"),
									   MoveLineDownClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("Insert new &line(s) before"),
						Localizer.Str("This line will move down and the new line(s) will be inserted before"),
						AddNewLinesBeforeClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Add new line(s) after"),
						Localizer.Str("The new line(s) will be added after this line"),
						AddNewLinesAfterClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Hide line"),
						Localizer.Str("Hide this line (use \'View\', \'Show Hidden\' to see them later)"),
						HideVerseToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Delete line"),
						Localizer.Str("Delete this line"),
						DeleteTheWholeVerseToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Paste line from clipboard and insert before this line"),
						Localizer.Str("Use this to paste a previously copied line (see \'Copy line to clipboard\' command). The copied line will be inserted before this line (see \'Paste line from clipboard\' commands)"),
						PasteVerseFromClipboardToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("Paste line from clipboard and insert af&ter this line"),
						Localizer.Str("Use this to paste a previously copied line (see \'Copy line to clipboard\' command). The copied line will be inserted after this line"),
						PasteVerseFromClipboardAfterThisOneToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Copy line to clipboard"),
						Localizer.Str("Use this to copy a line to be pasted in another location"),
						CopyVerseToClipboardToolStripMenuItemClick);

			ctxMenu.Items.Add(new ToolStripSeparator());

			AddMenuItem(ctxMenu,
						Localizer.Str("Split st&ory"),
						Localizer.Str("Move this and all following lines to a new story inserted immediately after this story"),
						SplitStoryToolStripMenuItemClick);

			ctxMenu.Opening += ContextMenuStripLineOptionsOpening;

			return ctxMenu;
		}

		private ToolStripItem _moveLineUp, _moveLineDown;

		private void ContextMenuStripLineOptionsOpening(object sender, CancelEventArgs e)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);
			_moveLineUp.Enabled = (nLineIndex > 1);
			_moveLineDown.Enabled = (nLineIndex < theSe.TheCurrentStory.Verses.Count);
		}

		private void MoveSelectedTextToANewLineToolStripMenuItemClick(object sender, EventArgs e)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);
			verseData.AllowConNoteButtonsOverride();

			// make a copy and clear out the stuff that we'll have them manually move later
			var verseNew = new VerseData(verseData);
			verseNew.TestQuestions.Clear();
			verseNew.ConsultantNotes.Clear();
			verseNew.CoachNotes.Clear();

			Browser.MoveSelectedTextToNewLine(verseData, verseNew, nLineIndex);

			theSe.DoPasteVerse(nLineIndex, verseNew);

			var dlg = new CutItemPicker(verseData, verseNew, nLineIndex + 1, theSe);
			if (dlg.IsSomethingToMove)
				dlg.ShowDialog();

			ReloadAllWindows();
		}

		private void MoveItemsToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);
			var dlg = new CutItemPicker(verseData, theSe.TheCurrentStory.Verses, theSe, false);
			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			ReloadAllWindows();
		}

		private void DeleteItemsToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			var dlg = new CutItemPicker(verseData, theSe.TheCurrentStory.Verses, theSe, true)
			{
				Text = Localizer.Str("Choose the item(s) to delete and then click the Delete button")
			};

			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			ReloadAllWindows();
		}

		private void AddTestQuestionClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			var isGeneralQuestionsLine = (nLineIndex == 0);
			if (isGeneralQuestionsLine &&
				TeamMemberData.IsUser(theSe.LoggedOnMember.MemberType, TeamMemberData.UserTypes.ProjectFacilitator) &&
				!TasksPf.IsTaskOn(theSe.TheCurrentStory.TasksAllowedPf, TasksPf.TaskSettings.TestQuestions))
			{
				LocalizableMessageBox.Show(
					Localizer.Str("The consultant has not allowed you to enter testing questions at this time"),
					StoryEditor.OseCaption);
				return;
			}

			verseData.TestQuestions.AddTestQuestion();
			theSe.Modified = true;
			if (isGeneralQuestionsLine && !theSe.viewGeneralTestingsQuestionMenu.Checked)
				theSe.viewGeneralTestingsQuestionMenu.Checked = true;
			if (!isGeneralQuestionsLine && !theSe.viewStoryTestingQuestionsMenu.Checked)
				theSe.viewStoryTestingQuestionsMenu.Checked = true;
			else
			{
				StrIdToScrollTo = BrowserDisplay.GetTopRowId;
				LoadDocument();
			}
		}

		private void AddExegeticalCulturalNoteBelowToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			verseData.ExegeticalHelpNotes.AddExegeticalHelpNote("");
			theSe.Modified = true;

			if (!theSe.viewExegeticalHelps.Checked)
				theSe.viewExegeticalHelps.Checked = true;
			else
			{
				StrIdToScrollTo = BrowserDisplay.GetTopRowId;
				LoadDocument();
			}
		}

		private void MoveLineUpClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			StrIdToScrollTo = Browser.GetPrevRowId; // get the *previous* row for going up
			theSe.DoMove(nLineIndex - 2, verseData);
		}

		private void MoveLineDownClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			StrIdToScrollTo = Browser.GetNextRowId; // get the *next* row for going down
			theSe.DoMove(nLineIndex + 1, verseData);
		}

		private void AddNewLinesBeforeClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			var strNumOfLines = QueryNumberOfLinesToAdd;

			int nNumLines;
			if (!Int32.TryParse(strNumOfLines, out nNumLines))
				return;

			int nLineIndex;
			VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			theSe.AddNewVerse(nLineIndex - 1, nNumLines, false);
		}

		private void AddNewLinesAfterClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			var strNumOfLines = QueryNumberOfLinesToAdd;

			int nNumLines;
			if (!Int32.TryParse(strNumOfLines, out nNumLines))
				return;

			int nLineIndex;
			VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			theSe.AddNewVerse(nLineIndex - 1, nNumLines, true);
		}

		private static string QueryNumberOfLinesToAdd
		{
			get
			{
				return LocalizableMessageBox.InputBox(Localizer.Str("Enter the number of (blank) lines to add/insert"),
													  StoryEditor.OseCaption, "1");
			}
		}

		private void HideVerseToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			theSe.VisiblizeVerse(verseData,
				!(verseData.IsVisible)   // toggle
				);
		}

		private void DeleteTheWholeVerseToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			if (verseData.HasData)
			{
				var res = QueryAboutHidingVerseInstead();

				if (res == DialogResult.Yes)
				{
					theSe.VisiblizeVerse(verseData, false);
					return;
				}

				if (res == DialogResult.Cancel)
					return;
			}

			if (!UserConfirmDeletion)
				return;

			StrIdToScrollTo = BrowserDisplay.GetTopRowId;
			theSe.DeleteVerse(verseData);
		}

		private void PasteVerseFromClipboardToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			PasteVerseToIndex(theSe, nLineIndex - 1);
			theSe.InitAllPanes();
		}

		private void PasteVerseFromClipboardAfterThisOneToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			PasteVerseToIndex(theSe, nLineIndex);
			theSe.InitAllPanes();
		}

		private void CopyVerseToClipboardToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			_myClipboard = new VerseData(verseData);
		}

		private void SplitStoryToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || String.IsNullOrEmpty(_lastLineOptionsButtonClicked))
				return;

			int nLineIndex;
			var verseData = VerseDataFromLineOptionsButtonId(_lastLineOptionsButtonClicked, out nLineIndex);

			theSe.SplitStory(verseData);
		}

		#endregion  // ContextMenuLineOptions

		#region ContextMenuAnchorOptions

		private ContextMenuStrip _contextMenuStripAnchorOptions;
		private string _lastAnchorButtonClicked;


		public bool OnAnchorButton(string strButtonId)
		{
			_lastAnchorButtonClicked = strButtonId;
			_contextMenuStripAnchorOptions.Show(MousePosition);
			return true;
		}

		private ContextMenuStrip CreateContextMenuAnchorOptions()
		{
			var ctxMenu = new ContextMenuStrip();

			_insertNullAnchor = AddMenuItem(ctxMenu,
											Localizer.Str("Insert \"&Empty\" Anchor"),
											Localizer.Str("Use this to add an empty anchor for lines of the story that don\'t really have a biblical anchor"),
											InsertNullAnchorToolStripMenuItemClick);

			_addConsultantCoachNoteOnThisAnchor =
				AddMenuItem(ctxMenu,
							Localizer.Str("Add &Consultant/Coach Note on this Anchor"),
							Localizer.Str("Click this menu to add a consultant or coach note for this anchor"),
							AddConsultantCoachNoteOnThisAnchorToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Add Anchor Comment (becomes a tooltip)"),
						Localizer.Str("Click this menu to add a comment to the anchor (which becomes visible when you hover your cursor over the anchor button)"),
						AddCommentToolStripMenuItemClick);

			AddMenuItem(ctxMenu,
						Localizer.Str("&Delete Anchor"),
						Localizer.Str("Click this menu to delete this anchor"),
						DeleteAnchorToolStripMenuItemClick);

			ctxMenu.Opening += ContextMenuStripAnchorOptionsOpening;

			return ctxMenu;
		}

		private ToolStripItem _insertNullAnchor, _addConsultantCoachNoteOnThisAnchor;

		private void ContextMenuStripAnchorOptionsOpening(object sender, CancelEventArgs e)
		{
			int nLineIndex;
			var verseData = VerseDataFromAnchorButtonId(_lastAnchorButtonClicked, out nLineIndex);
			if (verseData == null)
				return;

			_insertNullAnchor.Visible = (verseData.Anchors.Count == 0);

			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe) || (theSe.LoggedOnMember == null))
				return;

			_addConsultantCoachNoteOnThisAnchor.Visible =
				TeamMemberData.IsUser(theSe.LoggedOnMember.MemberType,
									  TeamMemberData.UserTypes.AnyEditor);
		}

		private void DeleteAnchorToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(_lastAnchorButtonClicked))
			{
				StoryEditor theSe;
				if (!CheckForProperEditToken(out theSe))
					return;

				int nLineIndex;
				var verseData = VerseDataFromAnchorButtonId(_lastAnchorButtonClicked, out nLineIndex);
				if (verseData == null)
					return;

				AnchorData anchor;
				if (!TryGetAnchorData(verseData, nLineIndex, out anchor))
					return;

				verseData.Anchors.Remove(anchor);

				StrIdToScrollTo = BrowserDisplay.GetTopRowId;
				LoadDocument();

				_lastAnchorButtonClicked = null;

				// indicate that we've changed something so that we don't exit without offering
				//  to save.
				theSe.Modified = true;
			}
			else
				LocalizableMessageBox.Show("Right-click on one of the buttons to choose which one to delete", StoryEditor.OseCaption);
		}

		private void AddCommentToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(_lastAnchorButtonClicked))
			{
				StoryEditor theSe;
				if (!CheckForProperEditToken(out theSe))
					return;

				int nLineIndex;
				AnchorData anchor;
				if (!TryGetAnchorData(_lastAnchorButtonClicked, out nLineIndex, out anchor))
					return;

				var dlg = new AnchorAddCommentForm(NetBibleViewer.CheckForLocalization(anchor.JumpTarget), anchor.ToolTipText);
				var res = dlg.ShowDialog();
				if ((res == DialogResult.OK) || (res == DialogResult.Yes))
				{
					anchor.ToolTipText = dlg.CommentText;
					theSe.Modified = true;
					StrIdToScrollTo = BrowserDisplay.GetTopRowId;
					LoadDocument();
				}
			}
			else
				LocalizableMessageBox.Show("Right-click on one of the buttons to choose which one to add the comment to", StoryEditor.OseCaption);
		}

		private void AddConsultantCoachNoteOnThisAnchorToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(_lastAnchorButtonClicked))
			{
				StoryEditor theSe;
				if (!CheckForProperEditToken(out theSe))
					return;
				System.Diagnostics.Debug.Assert(theSe.LoggedOnMember != null);

				int nLineIndex;
				AnchorData anchor;
				if (!TryGetAnchorData(_lastAnchorButtonClicked, out nLineIndex, out anchor))
					return;

				StrIdToScrollTo = BrowserDisplay.GetTopRowId;

				var strReferringText = AnchorsData.AnchorLabel + " ";
				strReferringText += anchor.JumpTarget;

				if (anchor.JumpTarget != anchor.ToolTipText)
					strReferringText += String.Format(" ({0})", anchor.ToolTipText);

				var strNote = StoryEditor.GetInitials(theSe.LoggedOnMember.Name) + ": ";
				theSe.SendNoteToCorrectPane(nLineIndex, strReferringText, strNote, false);
			}
			else
				LocalizableMessageBox.Show("Right-click on one of the buttons to choose which one to add the comment to", StoryEditor.OseCaption);
		}

		private void InsertNullAnchorToolStripMenuItemClick(object sender, EventArgs e)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe))
				return;

			int nLineIndex;
			var verseData = VerseDataFromAnchorButtonId(_lastAnchorButtonClicked, out nLineIndex);
			if (verseData == null)
				return;

			verseData.Anchors.AddAnchorData(AnchorControl.CstrNullAnchor, AnchorControl.CstrNullAnchor);

			// indicate that we've changed something so that we don't exit without offering
			//  to save.
			theSe.Modified = true;
			StrIdToScrollTo = BrowserDisplay.GetTopRowId;
			LoadDocument();
		}

		#endregion  // ContextMenuAnchorOptions

		private static bool _bIgnoringChanges;
		public void TriggerChangeUpdate()
		{
			// we only update the StringTransfer for a textarea when the user leaves (onchange),
			//  so when the user saves, sometimes, we need to trigger that call.
			var strId = LastTextareaInFocusId;
			if (strId == null)
				return;

			// we don't want to do this if the field is read-only (e.g. so we don't cause
			//  the internal buffer to be filled with the transliterated value)
			var st = GetStringTransferOfLastTextAreaInFocus;
			if ((st == null) || st.IsFieldReadonly(ViewSettings.FieldEditibility))
				return;

			_bIgnoringChanges = true;
			Browser.TriggerPreSaveEvents(strId);
			_bIgnoringChanges = false;
		}

		public bool CheckShowErrorOnFieldNotEditable(StringTransfer stringTransfer)
		{
			// this will fail if the field is readonly which would be if the consultant hadn't allowed it or if
			//  a transliterator were turned on. Either way, this should catch it.
			if (stringTransfer.IsFieldReadonly(ViewSettings.FieldEditibility) && !_bIgnoringChanges)
			{
				LocalizableMessageBox.Show(
					String.Format(
						Localizer.Str(
							"You can't edit this field right now... either the consultant hasn't given you permission to edit the '{0}' language fields or perhaps there is a transliterator turned on"),
						stringTransfer.WhichField & StoryEditor.TextFields.Languages),
					StoryEditor.OseCaption);
				return false;
			}
			return true;
		}

		public static bool TryGetTextAreaId(string strId, out TextAreaIdentifier textAreaIdentifier)
		{
			try
			{
				// for TextAreas:
				//  ta_<lineNum>_<dataType>_<itemNum>_<subItemNum>_<stylename>
				// where:
				//  lineNum (0-GTQ line, ln 1, etc)
				//  dataType (e.g. "Retelling", "StoryLine", etc)
				//  itemNum (e.g. "TQ *1*")
				//  subItemNum (e.g. "TQ 1.Ans *3*)
				//  langName (e.g. Vernacular, etc)
				var aVerseConversationIndices = strId.Split(AchDelim);
				System.Diagnostics.Debug.Assert(((aVerseConversationIndices[0] == CstrTextAreaPrefix) &&
												 (aVerseConversationIndices.Length == 6))
												||
												((aVerseConversationIndices[0] == CstrButtonPrefix) &&
												 (aVerseConversationIndices.Length == 3)));

				textAreaIdentifier = new TextAreaIdentifier
				{
					LineIndex = Convert.ToInt32(aVerseConversationIndices[1]),
					FieldTypeName = aVerseConversationIndices[2],
					ItemIndex = Convert.ToInt32(aVerseConversationIndices[3]),
					SubItemIndex = Convert.ToInt32(aVerseConversationIndices[4]),
					LanguageColumnName = aVerseConversationIndices[5]
				};
			}
			catch
			{
				textAreaIdentifier = null;
				return false;
			}
			return true;
		}

		private void CheckForLnCNoteLookup(ToolStripMenuItem x)
		{
			x.DropDownItems.Clear();

			if (String.IsNullOrEmpty(LastTextareaInFocusId))
				return;

			TextAreaIdentifier textAreaIdentifier;
			if (!TryGetTextAreaId(LastTextareaInFocusId, out textAreaIdentifier))
				return;

			StoryEditor.TextFields whichLanguage;
			var selText = Browser.GetSelectedTextByTextareaIdentifier(textAreaIdentifier, out whichLanguage);
			if (String.IsNullOrEmpty(selText))
				return;

			var mapFoundString2LnCnote = TheSe.StoryProject.LnCNotes.FindHits(selText, whichLanguage);
			foreach (var kvp in mapFoundString2LnCnote)
			{
				var tsi = x.DropDownItems.Add(kvp.Key, null, OnLookupLnCnote);
				tsi.Tag = kvp.Value;
				tsi.Font = Font;
			}
		}

		private void OnLookupLnCnote(object sender, EventArgs e)
		{
			var tsi = sender as ToolStripItem;
			if (tsi == null)
				return;

			var note = (LnCNote)tsi.Tag;
			var dlg = new AddLnCNoteForm(TheSe, note) { Text = Localizer.Str("Edit L & C Note") };
			if ((dlg.ShowDialog() == DialogResult.OK) && (note != null))
				TheSe.Modified = true;
		}

		public string GetMyVernacularSibling(string strId)
		{
			return GetMySibling(strId, StoryEditor.TextFields.Vernacular.ToString(),
								TheSe.StoryProject.ProjSettings.Vernacular);
		}

		public string GetMyNationalBtSibling(string strId)
		{
			return GetMySibling(strId, StoryEditor.TextFields.NationalBt.ToString(),
								TheSe.StoryProject.ProjSettings.NationalBT);
		}

		public string GetMyInternationalBtSibling(string strId)
		{
			return GetMySibling(strId, StoryEditor.TextFields.InternationalBt.ToString(),
								TheSe.StoryProject.ProjSettings.InternationalBT);
		}

		public string GetMyFreeTranslationSibling(string strId)
		{
			return GetMySibling(strId, StoryEditor.TextFields.FreeTranslation.ToString(),
								TheSe.StoryProject.ProjSettings.FreeTranslation);
		}

		private static string GetMySibling(string strId, string strSiblingName, ProjectSettings.LanguageInfo languageInfo)
		{
			if (String.IsNullOrEmpty(strId) || !languageInfo.HasData)
				return null;

			TextAreaIdentifier textAreaIdentifier;
			if (!TryGetTextAreaId(strId, out textAreaIdentifier))
				return null;

			// retask for the sibling language column
			textAreaIdentifier.LanguageColumnName = strSiblingName;
			return textAreaIdentifier.HtmlIdentifier;
		}

		private bool TryGetAnchorData(string strAnchorButtonId, out int nLineIndex, out AnchorData anchor)
		{
			anchor = null;
			var verseData = VerseDataFromAnchorButtonId(strAnchorButtonId, out nLineIndex);
			return ((verseData != null) && TryGetAnchorData(verseData, nLineIndex, out anchor));
		}

		private bool TryGetAnchorData(VerseData verseData, int nLineIndex, out AnchorData anchor)
		{
			anchor = verseData.Anchors
				.FirstOrDefault(anc => AnchorData.ButtonId(nLineIndex, anc.JumpTarget) == _lastAnchorButtonClicked);
			return (anchor != null);
		}

		public bool TextareaOnKeyUp(string strId, string strText)
		{
			// we'll get the value updates during OnChange, but in order to enable
			//  the save menu, we have to set modified
			LastTextareaInFocusId = strId;
			TheSe.LastKeyPressedTimeStamp = DateTime.Now;
			return TextareaOnChange(strId, strText);
		}

		public bool TextareaOnChange(string strId, string strText)
		{
			StoryEditor theSe;
			if (!CheckForProperEditToken(out theSe))
				return false;

			var stringTransfer = GetStringTransfer(strId);
			if (stringTransfer == null)
				return false;

			if (!CheckShowErrorOnFieldNotEditable(stringTransfer))
				return false;

			// finally make sure it's supposed to be visible.
			stringTransfer.SetValue(strText);

			// indicate that the document has changed
			theSe.Modified = true;

			// update the status bar (in case we previously put an error there
			var st = StoryStageLogic.stateTransitions[theSe.TheCurrentStory.ProjStage.ProjectStage];
			theSe.SetDefaultStatusBar(st.StageDisplayString);

			return true;
		}

		public bool TextareaOnFocus(string strId)
		{
			LastTextareaInFocusId = strId;
			TextAreaIdentifier textAreaIdentifier;
			if (TryGetTextAreaId(strId, out textAreaIdentifier))
			{
				var strKeyboardName = textAreaIdentifier.GetLanguageInfo(TheSe.StoryProject.ProjSettings).Keyboard;
				if (!String.IsNullOrEmpty(strKeyboardName))
					KeyboardController.ActivateKeyboard(strKeyboardName);
			}
			return false;
		}
	}

}
