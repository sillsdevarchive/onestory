using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;

namespace OneStoryProjectEditor
{
	// have to make this com visible, because 'this' needs to be visible to COM for the
	// call to: webBrowserNetBible.ObjectForScripting = this;
	public partial class StoryEditor : Form
	{
		internal const string CstrButtonDropTargetName = "buttonDropTarget";

		internal StoriesData Stories = null;
		internal StoryData theCurrentStory = null;

		// we keep a copy of this, because it ought to persist across multiple files
		internal TeamMemberData LoggedOnMember = null;

		internal bool Modified = false;

		public StoryEditor()
		{
			InitializeComponent();
			try
			{
				InitializeNetBibleViewer();
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Problem initializing Sword (the Net Bible viewer):{0}{0}{1}", Environment.NewLine, ex.Message),  StoriesData.CstrCaption);
			}

			try
			{
				if (String.IsNullOrEmpty(Properties.Settings.Default.LastUserType))
					NewProjectFile();
				else if ((Properties.Settings.Default.LastUserType == TeamMemberData.CstrCrafter)
						&& !String.IsNullOrEmpty(Properties.Settings.Default.LastProject))
					OpenProject(Properties.Settings.Default.LastProjectPath, Properties.Settings.Default.LastProject);
			}
			catch { }   // this was only a bene anyway, so just ignore it
		}

		// this is now browse for project in non-default location.
		private void browseForProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string strProjectName = null;
			try
			{
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					// for this, we have to get the name to use for this project
					//  (which should be the filename without extension)
					strProjectName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);

					// possible scenario. The user has copied a file/project from another machine
					//  and has actually put it into the default location. In this case, we don't
					//  want to query the user for the project (which has the side effect of
					//  forcing the user to overwrite the file--based on the logic it needs
					//  for other possible cases). So here, if the project file happens to be in
					//  the default location, then just go ahead and open it directly and forget
					//  about querying the user for the Project Name (i.e. don't do what's in this
					//  if statement)
					if (openFileDialog.FileName != ProjectSettings.GetDefaultProjectFileName(strProjectName))
					{
						// this means that the file is not in the default location... But before we can go ahead, we need to
						//  check to see if a project already exists with this name in the default location on the disk.
						// Here's the situation: the user has 'StoryProject' in the default location and tries to 'browse/add'
						//  a 'StoryProject' from another location. In that case, it isn't strictly true that finding the one
						//  in the default location means we will have to overwrite the existing project file (as threatened in
						//  the message box below). However, it is true, that the RecentProjects list will lose the reference to
						//  the existing one. So if the user cares anything about the existing one at all, they aren't going to
						//  want to do that... So let's be draconian and actually overwrite the file if they say 'yes'. This way,
						//  if they care, they'll say 'no' instead and give it a different name.
						string strFilename = ProjectSettings.GetDefaultProjectFileName(strProjectName);
						if (File.Exists(strFilename))
						{
							DialogResult res = MessageBox.Show(String.Format("You already have a project with the name, '{0}'. Do you want to delete the existing one?", strProjectName),  StoriesData.CstrCaption, MessageBoxButtons.YesNoCancel);
							if (res != DialogResult.Yes)
								throw StoryEditor.BackOutWithNoUI;

							// they want to delete it (so remove all traces of it, so we don't leave around a file which
							//  is no longer being referenced, which they might one day mistake for the current version)
							File.Delete(strFilename);   // TODO: probably ought to remove the folder as well and what about the .hg repository?

							// remove the existing references in the Recent lists too
							int nIndex = Properties.Settings.Default.RecentProjects.IndexOf(strProjectName);
							Properties.Settings.Default.RecentProjects.RemoveAt(nIndex);
							Properties.Settings.Default.RecentProjectPaths.RemoveAt(nIndex);
							Properties.Settings.Default.Save();
						}
					}

					ProjectSettings projSettings = new ProjectSettings(Path.GetDirectoryName(openFileDialog.FileName), strProjectName);
					OpenProject(projSettings);
				}
			}
			catch (BackOutWithNoUIException)
			{
				// sub-routine has taken care of the UI, just exit without doing anything
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Unable to import the project '{1}'{0}{0}{2}{0}{0}Contact bob_eaton@sall.com for help",
					Environment.NewLine, strProjectName, ex.Message),  StoriesData.CstrCaption);
				return;
			}
		}

		protected void CloseProjectFile()
		{
			System.Diagnostics.Debug.Assert(!Modified);
			Stories = null;
			theCurrentStory = null;
			comboBoxStorySelector.Items.Clear();
			comboBoxStorySelector.Text = "<type the name of a story to create and hit Enter>";
		}

		protected void NewProjectFile()
		{
			CheckForSaveDirtyFile();
			CloseProjectFile();
			comboBoxStorySelector.Focus();

			// for a new project, we don't want to automatically log in (since this will be the first
			//  time editing the new project and we need to add at least the current user)
			LoggedOnMember = null;
			System.Diagnostics.Debug.Assert(Stories == null);
			teamMembersToolStripMenuItem_Click(null, null);

			if (Stories != null)
				UpdateRecentlyUsedLists(Stories.ProjSettings);

			buttonsStoryStage.Enabled = true;
		}

		// routines can use this exception to back out of creating a new project without UI
		//  (presumably, because they've already done so--e.g. "are you sure you want to
		//  overwrite this project + user Cancel)
		internal class BackOutWithNoUIException : ApplicationException
		{
		}

		internal static BackOutWithNoUIException BackOutWithNoUI
		{
			get { return new BackOutWithNoUIException(); }
		}

		private void teamMembersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (Stories == null)
			{
				try
				{
					Stories = GetNewStoriesData;
					buttonsStoryStage.Enabled = true;
				}
				catch (BackOutWithNoUIException)
				{
					// sub-routine has taken care of the UI, just exit without doing anything
				}
				catch (Exception ex)
				{
					MessageBox.Show(String.Format("Unable to open the member list{0}{0}{1}{0}{0}Contact bob_eaton@sall.com for help",
						Environment.NewLine, ex.Message),  StoriesData.CstrCaption);
					return;
				}
			}
			else
			{
				try
				{
					// detect if the logged on member type changed, and if so, redo the Consult Notes panes
					System.Diagnostics.Debug.Assert(LoggedOnMember != null);
					TeamMemberData.UserTypes eCurrentMemberType = LoggedOnMember.MemberType;
					LoggedOnMember = Stories.EditTeamMembers(LoggedOnMember.Name, TeamMemberForm.CstrDefaultOKLabel);
					Modified = true;
					if (theCurrentStory != null)
					{
						InitAllPanes(theCurrentStory.Verses);
						CheckForProperMemberType();
					}
				}
				catch { }   // this might throw if the user cancels, but we don't care
			}
		}

		protected void UpdateRecentlyUsedLists(ProjectSettings projSettings)
		{
			// update the recently-used-project-names list
			if (Properties.Settings.Default.RecentProjects.Contains(projSettings.ProjectName))
			{
				int nIndex = Properties.Settings.Default.RecentProjects.IndexOf(projSettings.ProjectName);
				Properties.Settings.Default.RecentProjects.RemoveAt(nIndex);
				Properties.Settings.Default.RecentProjectPaths.RemoveAt(nIndex);
			}

			Properties.Settings.Default.RecentProjects.Insert(0, projSettings.ProjectName);
			Properties.Settings.Default.RecentProjectPaths.Insert(0, projSettings.ProjectFolder);

			Properties.Settings.Default.LastProject = projSettings.ProjectName;
			Properties.Settings.Default.LastProjectPath = projSettings.ProjectFolder;
			Properties.Settings.Default.Save();
		}

		protected void OpenProject(string strProjectFolder, string strProjectName)
		{
			ProjectSettings projSettings = new ProjectSettings(strProjectFolder, strProjectName);
			OpenProject(projSettings);
		}

		protected void OpenProject(ProjectSettings projSettings)
		{
			// clean up any existing open projects
			CheckForSaveDirtyFile();
			CloseProjectFile();

			// next, insure that the file for the project exists (do this outside the try,
			//  so the caller is informed of no file (so, for eg., it can remove from recently
			//  used list.
			projSettings.ThrowIfProjectFileDoesntExists();

			UpdateRecentlyUsedLists(projSettings);

			try
			{

				// serialize in the file
				StoryProject projFile = new StoryProject();
				projFile.ReadXml(projSettings.ProjectFileName);

				// get the data into another structure that we use internally (more flexible)
				Stories = GetOldStoriesData(projFile, projSettings);

				// enable the button
				buttonsStoryStage.Enabled = true;

				string strStoryToLoad = null;
				if (Stories.Count > 0)
				{
					// populate the combo boxes with all the existing story names
					foreach (StoryData aStory in Stories)
						comboBoxStorySelector.Items.Add(aStory.Name);
					strStoryToLoad = Stories[0].Name;    // default
				}

				// check for project settings that might have been saved from a previous session
				if (!String.IsNullOrEmpty(Properties.Settings.Default.LastStoryWorkedOn) && comboBoxStorySelector.Items.Contains(Properties.Settings.Default.LastStoryWorkedOn))
					strStoryToLoad = Properties.Settings.Default.LastStoryWorkedOn;

				if (!String.IsNullOrEmpty(strStoryToLoad) && comboBoxStorySelector.Items.Contains(strStoryToLoad))
					comboBoxStorySelector.SelectedItem = strStoryToLoad;
			}
			catch (Exception ex)
			{
				string strErrorMsg = String.Format("Unable to open project '{1}'{0}{0}{2}{0}{0}{3}{0}{0}Send the project file along with the error message to bob_eaton@sall.com for help",
					Environment.NewLine, projSettings.ProjectName,
					((ex.InnerException != null) ? ex.InnerException.Message : ""), ex.Message);
				MessageBox.Show(strErrorMsg,  StoriesData.CstrCaption);
			}
		}

		protected StoriesData GetNewStoriesData
		{
			get
			{
				StoriesData ssd = new StoriesData(null);    // null causes us to query for the project name
				if (LoggedOnMember == null)
					LoggedOnMember = ssd.GetLogin();

				Modified = true;
				return ssd;
			}
		}

		protected StoriesData GetOldStoriesData(StoryProject projFile, ProjectSettings projSettings)
		{
			StoriesData theOldStories = new StoriesData(projFile, projSettings);

			if (LoggedOnMember == null)
			{
				LoggedOnMember = theOldStories.GetLogin();
				Modified = true;
			}

			return theOldStories;
		}

		private void insertNewStoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string strStoryName;
			int nIndexOfCurrentStory = -1;
			if (AddNewStoryGetIndex(ref nIndexOfCurrentStory, out strStoryName))
			{
				System.Diagnostics.Debug.Assert(nIndexOfCurrentStory != -1);
				InsertNewStory(strStoryName, nIndexOfCurrentStory);
				Modified = true;
			}
		}

		protected bool AddNewStoryGetIndex(ref int nIndexForInsert, out string strStoryName)
		{
			// ask the user for what story they want to add (i.e. the name)
			strStoryName = Microsoft.VisualBasic.Interaction.InputBox("Enter the name of the story to add", StoriesData.CstrCaption, null, 300, 200);
			if (!String.IsNullOrEmpty(strStoryName))
			{
				foreach (StoryData aStory in Stories)
					if (aStory.Name == strStoryName)
					{
						// if they already have a story by that name, just go there
						comboBoxStorySelector.SelectedItem = strStoryName;
						return false;
					}
					else if (aStory.Name == theCurrentStory.Name)
					{
						nIndexForInsert = Stories.IndexOf(aStory);
						return true;
					}
			}

			return false;
		}

		private void addNewStoryAfterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string strStoryName;
			int nIndexOfCurrentStory = -1;
			if (AddNewStoryGetIndex(ref nIndexOfCurrentStory, out strStoryName))
			{
				System.Diagnostics.Debug.Assert(nIndexOfCurrentStory != -1);
				InsertNewStory(strStoryName, nIndexOfCurrentStory + 1);
				Modified = true;
			}
		}

		private void comboBoxStorySelector_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)    // user just finished entering a story name to select (or add)
			{
				if (Stories == null)
				{
					Stories = GetNewStoriesData;
					buttonsStoryStage.Enabled = true;
				}

				int nInsertIndex = 0;
				StoryData theStory = null;
				string strStoryToLoad = comboBoxStorySelector.Text;
				for (int i = 0; i < Stories.Count; i++)
				{
					StoryData aStory = Stories[i];
					if ((theCurrentStory != null) && (theCurrentStory == aStory))
						nInsertIndex = i + 1;
					if (aStory.Name == strStoryToLoad)
						theStory = aStory;
				}

				if (theStory == null)
				{
					if (MessageBox.Show(String.Format("Unable to find the story '{0}'. Would you like to add a new one with that name?", strStoryToLoad),  StoriesData.CstrCaption, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
					{
						System.Diagnostics.Debug.Assert(!comboBoxStorySelector.Items.Contains(strStoryToLoad));
						InsertNewStory(strStoryToLoad, nInsertIndex);
					}
				}
				else
					comboBoxStorySelector.SelectedItem = theStory.Name;
			}
		}

		protected void InsertNewStory(string strStoryName, int nIndexToInsert)
		{
			CheckForSaveDirtyFile();
			comboBoxStorySelector.Items.Insert(nIndexToInsert, strStoryName);
			theCurrentStory = new StoryData(strStoryName, LoggedOnMember.MemberGuid);
			Stories.Insert(nIndexToInsert, theCurrentStory);
			comboBoxStorySelector.SelectedItem = strStoryName;
		}

		private void comboBoxStorySelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			// save the file before moving on.
			CheckForSaveDirtyFile();

			System.Diagnostics.Debug.Assert(!Modified
				|| (flowLayoutPanelVerses.Controls.Count != 0)
				|| (flowLayoutPanelConsultantNotes.Controls.Count != 0)
				|| (flowLayoutPanelCoachNotes.Controls.Count != 0)); // if this happens, it means we didn't save or cleanup the document

			// we might could come thru here without having opened any file (e.g. after New)
			if (Stories == null)
			{
				Stories = GetNewStoriesData;
				buttonsStoryStage.Enabled = true;
			}

			// find the story they've chosen (this shouldn't be possible to fail)
			foreach (StoryData aStory in Stories)
				if (aStory.Name == (string)comboBoxStorySelector.SelectedItem)
				{
					theCurrentStory = aStory;
					break;
				}
			System.Diagnostics.Debug.Assert(theCurrentStory != null);
			Properties.Settings.Default.LastStoryWorkedOn = theCurrentStory.Name;
			Properties.Settings.Default.Save();

			// initialize the text box showing the storying they're editing
			textBoxStoryVerse.Text = "Story: " + theCurrentStory.Name;
			this.Text = String.Format("OneStory Editor -- {0} Story Project", Stories.ProjSettings.ProjectName);

			// initialize the project stage details (which might hide certain views)
			//  (do this *after* initializing the whole thing, because if we save, we'll
			//  want to save even the hidden pieces)
			SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage);

			// finally, initialize the verse controls
			InitAllPanes();

			CheckForProperMemberType();

			// get the focus off the combo box, so mouse scroll doesn't rip thru the stories!
			flowLayoutPanelVerses.Focus();
		}

		private void CheckForProperMemberType()
		{
			// inform the user that they won't be able to edit this if they aren't the proper member type
			System.Diagnostics.Debug.Assert((theCurrentStory != null) && (LoggedOnMember != null));
			if (LoggedOnMember.MemberType != theCurrentStory.ProjStage.MemberTypeWithEditToken)
				try
				{
					throw theCurrentStory.ProjStage.WrongMemberTypeEx;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, StoriesData.CstrCaption);
				}
		}

		protected void InitAllPanes(VersesData theVerses)
		{
			ClearFlowControls();
			int nVerseIndex = 0;
			if (theVerses.Count == 0)
				theCurrentStory.Verses.InsertVerse(0, "<Type the Story here>",
					"<Type the UNS's back-translation here>", "<Type the English back-translation here>");

			flowLayoutPanelVerses.SuspendLayout();
			flowLayoutPanelConsultantNotes.SuspendLayout();
			flowLayoutPanelCoachNotes.SuspendLayout();
			SuspendLayout();

			AddDropTargetToFlowLayout(nVerseIndex++);
			foreach (VerseData aVerse in theVerses)
			{
				InitVerseControls(aVerse, nVerseIndex);

				InitConsultNotesPane(flowLayoutPanelConsultantNotes, aVerse.ConsultantNotes, nVerseIndex);

				InitConsultNotesPane(flowLayoutPanelCoachNotes, aVerse.CoachNotes, nVerseIndex);

				nVerseIndex++;
			}

			flowLayoutPanelVerses.ResumeLayout(true);
			flowLayoutPanelConsultantNotes.ResumeLayout(true);
			flowLayoutPanelCoachNotes.ResumeLayout(true);
			ResumeLayout(true);
		}

		protected void InitVerseControls(VerseData aVerse, int nVerseIndex)
		{
			VerseBtControl aVerseCtrl = new VerseBtControl(this, aVerse, nVerseIndex);
			aVerseCtrl.UpdateHeight(Panel1_Width);
			flowLayoutPanelVerses.Controls.Add(aVerseCtrl);
			AddDropTargetToFlowLayout(nVerseIndex);
		}

		// this is for use by the consultant panes if we add or remove or hide a note
		internal void ReInitVerseControls()
		{
			int nVerseIndex = 0;
			flowLayoutPanelVerses.Controls.Clear();
			flowLayoutPanelVerses.SuspendLayout();
			SuspendLayout();

			AddDropTargetToFlowLayout(nVerseIndex++);
			foreach (VerseData aVerse in theCurrentStory.Verses)
				InitVerseControls(aVerse, nVerseIndex++);

			flowLayoutPanelVerses.ResumeLayout(true);
			ResumeLayout(true);
		}

		protected void InitConsultNotesPane(ConNoteFlowLayoutPanel theFLP, ConsultNotesDataConverter aCNsDC, int nVerseIndex)
		{
			ConsultNotesControl aConsultNotesCtrl = new ConsultNotesControl(theCurrentStory.ProjStage, aCNsDC, nVerseIndex, LoggedOnMember.MemberType);
			aConsultNotesCtrl.UpdateHeight(Panel2_Width);
			theFLP.AddCtrl(aConsultNotesCtrl);
		}

		// this is for use by the consultant panes if we add or remove or hide a single note
		internal void ReInitConsultNotesPane(ConsultNotesDataConverter aCNsD)
		{
			int nVerseIndex = 0;
			if (flowLayoutPanelConsultantNotes.Contains(aCNsD))
			{
				flowLayoutPanelConsultantNotes.Clear();
				flowLayoutPanelConsultantNotes.SuspendLayout();
				SuspendLayout();

				foreach (VerseData aVerse in theCurrentStory.Verses)
					InitConsultNotesPane(flowLayoutPanelConsultantNotes, aVerse.ConsultantNotes, ++nVerseIndex);

				flowLayoutPanelConsultantNotes.ResumeLayout(true);
				ResumeLayout(true);
			}
			else
			{
				System.Diagnostics.Debug.Assert(flowLayoutPanelCoachNotes.Contains(aCNsD));
				flowLayoutPanelCoachNotes.Clear();
				flowLayoutPanelCoachNotes.SuspendLayout();
				SuspendLayout();

				foreach (VerseData aVerse in theCurrentStory.Verses)
					InitConsultNotesPane(flowLayoutPanelCoachNotes, aVerse.CoachNotes, ++nVerseIndex);

				flowLayoutPanelCoachNotes.ResumeLayout(true);
				ResumeLayout(true);
			}

			// if we do this, it's because something changed
			Modified = true;
		}

		internal void HandleQueryContinueDrag(ConsultNotesControl aCNsDC, QueryContinueDragEventArgs e)
		{
			System.Diagnostics.Debug.Assert(flowLayoutPanelConsultantNotes.Contains(aCNsDC._theCNsDC)
				|| flowLayoutPanelCoachNotes.Contains(aCNsDC._theCNsDC));
			FlowLayoutPanel theFLP = (flowLayoutPanelConsultantNotes.Contains(aCNsDC._theCNsDC)) ? flowLayoutPanelConsultantNotes : flowLayoutPanelCoachNotes;

			// this code causes the vertical scroll bar to move if the user is dragging the mouse beyond
			//  the boundary of the flowLayout panel that these verse controls are sitting it.
			Point pt = theFLP.PointToClient(MousePosition);
			if (theFLP.Bounds.Height < (pt.Y + 10))    // close to the bottom edge...
				theFLP.VerticalScroll.Value += 10;     // bump the scroll bar down
			else if ((pt.Y < 10) && theFLP.VerticalScroll.Value > 0)   // close to the top edge, while the scroll bar position is non-zero
				theFLP.VerticalScroll.Value -= Math.Min(10, theFLP.VerticalScroll.Value);

			if (e.Action != DragAction.Continue)
				DimConsultNotesDropTargetButtons(theFLP, aCNsDC);
			else
				LightUpConsultNotesDropTargetButtons(theFLP, aCNsDC);
		}

		private static void LightUpConsultNotesDropTargetButtons(FlowLayoutPanel theFLP, ConsultNotesControl control)
		{
			foreach (Control ctrl in theFLP.Controls)
			{
				System.Diagnostics.Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl aCNsC = (ConsultNotesControl)ctrl;
				if (aCNsC != control)
					aCNsC.buttonDragDropHandle.Dock = DockStyle.Fill;
			}
		}

		private static void DimConsultNotesDropTargetButtons(FlowLayoutPanel theFLP, ConsultNotesControl control)
		{
			foreach (Control ctrl in theFLP.Controls)
			{
				System.Diagnostics.Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl aCNsC = (ConsultNotesControl)ctrl;
				if (aCNsC != control)
					aCNsC.buttonDragDropHandle.Dock = DockStyle.Right;
			}
		}

		internal void AddNewVerse(VerseBtControl theVerse, int nNumberToAdd, bool bAfter)
		{
			int nInsertionIndex = theVerse.VerseNumber - 1;
			if (bAfter)
				nInsertionIndex++;

			VersesData lstNewVerses = new VersesData();
			for (int i = 0; i < nNumberToAdd; i++)
				lstNewVerses.Add(new VerseData());

			theCurrentStory.Verses.InsertRange(nInsertionIndex, lstNewVerses);
			InitAllPanes();
		}

		internal void AddNewVerse(int nInsertionIndex, string strVernacular, string strNationalBT, string strInternationalBT)
		{
			System.Diagnostics.Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses != null));
			theCurrentStory.Verses.InsertVerse(nInsertionIndex, strVernacular, strNationalBT, strInternationalBT);
		}

		internal void InitAllPanes()
		{
			try
			{
				InitAllPanes(theCurrentStory.Verses);
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Unable to continue! Cause: {0}", ex.Message),  StoriesData.CstrCaption);
				return;
			}
		}

		internal void DeleteVerse(VerseData theVerseDataToDelete)
		{
			theCurrentStory.Verses.Remove(theVerseDataToDelete);
			InitAllPanes();
		}

		internal void SetViewBasedOnProjectStage(StoryStageLogic.ProjectStages eStage)
		{
			StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[eStage];

			st.SetView(this);
			helpProvider.SetHelpString(this, st.StageInstructions);
			SetStatusBar(String.Format("{0}  Press F1 for instructions", st.StageDisplayString));
		}

		protected Button AddDropTargetToFlowLayout(int nVerseIndex)
		{
			Button buttonDropTarget = new Button();
			buttonDropTarget.AllowDrop = true;
			buttonDropTarget.Location = new System.Drawing.Point(3, 3);
			buttonDropTarget.Name = CstrButtonDropTargetName + nVerseIndex.ToString();
			buttonDropTarget.Size = new System.Drawing.Size(this.Panel1_Width, 10);
			buttonDropTarget.Dock = DockStyle.Fill;
			buttonDropTarget.TabIndex = nVerseIndex;
			buttonDropTarget.UseVisualStyleBackColor = true;
			buttonDropTarget.Visible = false;
			buttonDropTarget.Tag = nVerseIndex;
			buttonDropTarget.DragEnter += buttonDropTarget_DragEnter;
			buttonDropTarget.DragDrop += buttonDropTarget_DragDrop;
			flowLayoutPanelVerses.Controls.Add(buttonDropTarget);
			return buttonDropTarget;
		}

		void buttonDropTarget_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(VerseData)))
			{
				VerseData aVerseData = (VerseData)e.Data.GetData(typeof(VerseData));
				System.Diagnostics.Debug.Assert(sender is Button);
				int nInsertionIndex = (flowLayoutPanelVerses.Controls.IndexOf((Button)sender) / 2);
				DoMove(nInsertionIndex, aVerseData);
			}
		}

		void DoMove(int nInsertionIndex, VerseData theVerseToMove)
		{
			int nCurIndex = theCurrentStory.Verses.IndexOf(theVerseToMove);
			theCurrentStory.Verses.Remove(theVerseToMove);

			// if we're moving the verse to an earlier position, then remove it from its higher index,
			//  just insert it at the new lower index. However, if an earlier verse is being moved later,
			//  then once we remove it, then the insertion index will be one too many
			if (nInsertionIndex > nCurIndex)
				--nInsertionIndex;

			theCurrentStory.Verses.Insert(nInsertionIndex, theVerseToMove);
			InitAllPanes();
		}

		void buttonDropTarget_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(VerseData)))
				e.Effect = DragDropEffects.Move;
		}

		internal void LightUpDropTargetButtons(VerseBtControl aVerseCtrl)
		{
			int nIndex = flowLayoutPanelVerses.Controls.IndexOf(aVerseCtrl);
			for (int i = 0; i < flowLayoutPanelVerses.Controls.Count; i += 2)
			{
				Control ctrl = flowLayoutPanelVerses.Controls[i];
				System.Diagnostics.Debug.Assert(ctrl is Button);
				if (Math.Abs(nIndex - i) > 1)
					ctrl.Visible = true;
			}
		}

		internal void DimDropTargetButtons()
		{
			foreach (Control ctrl in flowLayoutPanelVerses.Controls)
				if (ctrl is Button)
					ctrl.Visible = false;
		}

		protected void InitializeNetBibleViewer()
		{
			netBibleViewer.InitNetBibleViewer();
			string strLastRef = "gen 1:5";
			if (!String.IsNullOrEmpty(Properties.Settings.Default.LastNetBibleReference))
				strLastRef = Properties.Settings.Default.LastNetBibleReference;
			SetNetBibleVerse(strLastRef);
		}

		internal void SetNetBibleVerse(string strScriptureReference)
		{
			if (splitContainerUpDown.Panel2Collapsed == true)
				viewNetBibleMenuItem.Checked = true;

			netBibleViewer.DisplayVerses(strScriptureReference);
		}

		protected int Panel1_Width
		{
			get
			{
				return splitContainerLeftRight.Panel1.Width - splitContainerLeftRight.Margin.Horizontal -
					SystemInformation.VerticalScrollBarWidth;
			}
		}

		protected int Panel2_Width
		{
			get
			{
				return splitContainerLeftRight.Panel2.Width - splitContainerLeftRight.Margin.Horizontal -
					SystemInformation.VerticalScrollBarWidth - 2;
			}
		}

		private void CheckForSaveDirtyFile()
		{
			if (Modified)
				SaveClicked();

			// do cleanup, because this is always called before starting something new (new file or empty project)
			ClearFlowControls();
			textBoxStoryVerse.Text = "Story";
		}

		protected void ClearFlowControls()
		{
			flowLayoutPanelVerses.Controls.Clear();
			flowLayoutPanelConsultantNotes.Clear();
			flowLayoutPanelCoachNotes.Clear();
		}

		protected void SaveClicked()
		{
			System.Diagnostics.Debug.Assert(Stories != null);
			string strFilename = Stories.ProjSettings.ProjectFileName;
			SaveFile(strFilename);
		}

		protected void SaveXElement(XElement elem, string strFilename)
		{
			// create the root portions of the XML document and tack on the fragment we've been building
			XDocument doc = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("StoryProject",
					elem));

			if (!Directory.Exists(Path.GetDirectoryName(strFilename)))
				Directory.CreateDirectory(Path.GetDirectoryName(strFilename));

			// save it with an extra extn.
			doc.Save(strFilename + CstrExtraExtnToAvoidClobberingFilesWithFailedSaves);

			// backup the last version to appdata
			// Note: doing File.Move leaves the old file security settings rather than replacing them
			// based on the target directory. Copy, on the other hand, inherits
			// security settings from the target folder, which is what we want to do.
			if (File.Exists(strFilename))
				File.Copy(strFilename, GetBackupFilename(strFilename), true);
			File.Delete(strFilename);
			File.Copy(strFilename + CstrExtraExtnToAvoidClobberingFilesWithFailedSaves, strFilename, true);
			File.Delete(strFilename + CstrExtraExtnToAvoidClobberingFilesWithFailedSaves);
		}

		protected const string CstrExtraExtnToAvoidClobberingFilesWithFailedSaves = ".out";

		internal void QueryStoryPurpose()
		{
			StoryFrontMatterForm dlg = new StoryFrontMatterForm(Stories, theCurrentStory);
			dlg.ShowDialog();
		}

		protected void SaveFile(string strFilename)
		{
			try
			{
				// let's see if the UNS entered the purpose and resources used on this story
				if (theCurrentStory != null)
				{
					System.Diagnostics.Debug.Assert(theCurrentStory.CraftingInfo != null);
					if (String.IsNullOrEmpty(theCurrentStory.CraftingInfo.StoryPurpose)
						|| String.IsNullOrEmpty(theCurrentStory.CraftingInfo.ResourcesUsed))
						QueryStoryPurpose();
				}

				SaveXElement(GetXml, strFilename);
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show(String.Format("The project file '{0}' is locked. Is it read-only? Or opened in some other program? Unlock it and try again. Or try to save it as a different name.", strFilename),  StoriesData.CstrCaption);
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Unable to save the project file '{1}'{0}{0}{2}", Environment.NewLine, strFilename, ex.Message),  StoriesData.CstrCaption);
				return;
			}

			Modified = false;
		}

		private static string GetBackupFilename(string strFilename)
		{
			return Application.UserAppDataPath + @"\Backup of " + Path.GetFileName(strFilename);
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveClicked();
		}

		protected void SetupTitleBar(string strProjectName, string strStoryName)
		{
			String str = String.Format("{0} -- {1} -- {2}",  StoriesData.CstrCaption, strProjectName, strStoryName);
		}

		/*
		protected void UpdateVersePanel()
		{
			foreach (Control ctrl in flowLayoutPanelVerses.Controls)
			{
				if (ctrl is VerseBtControl)
				{
					VerseBtControl aVerseCtrl = (VerseBtControl)ctrl;
					aVerseCtrl.UpdateView(this);
					aVerseCtrl.UpdateHeight(Panel1_Width);
				}
			}
		}
		*/

		private void viewFieldMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			ReInitVerseControls();
		}

		private void viewNetBibleMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(sender is ToolStripMenuItem);
			ToolStripMenuItem tsm = (ToolStripMenuItem)sender;
			splitContainerUpDown.Panel2Collapsed = !tsm.Checked;
		}

		private void viewConsultantNoteFieldMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(sender is ToolStripMenuItem);
			ToolStripMenuItem tsm = (ToolStripMenuItem)sender;
			bool bHidePanel1 = !tsm.Checked;
			if (bHidePanel1)
			{
				if (splitContainerMentorNotes.Panel2Collapsed)
					splitContainerLeftRight.Panel2Collapsed = true;
				else
					splitContainerMentorNotes.Panel1Collapsed = true;
				return;
			}
			// showing the Consultant's pane
			else if (splitContainerLeftRight.Panel2Collapsed)   // if the whole right-half is already collapsed...
			{
				// ... first enable it.
				splitContainerLeftRight.Panel2Collapsed = false;

				// glitch, whichever half (consultant's or coach's) was collapsed last will still be active even
				//  though it's menu item will be reset. So we need to hide it if we're enabling the other one
				if (!splitContainerMentorNotes.Panel2Collapsed) // this means it's not actually hidden
					splitContainerMentorNotes.Panel2Collapsed = true;
			}

			splitContainerMentorNotes.Panel1Collapsed = false;
		}

		private void viewCoachNotesFieldMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(sender is ToolStripMenuItem);
			ToolStripMenuItem tsm = (ToolStripMenuItem)sender;
			bool bHidePanel2 = !tsm.Checked;
			if (bHidePanel2)
			{
				if (splitContainerMentorNotes.Panel1Collapsed)
					splitContainerLeftRight.Panel2Collapsed = true;
				else
					splitContainerMentorNotes.Panel2Collapsed = true;
				return;
			}
			// showing the Coach's pane
			else if (splitContainerLeftRight.Panel2Collapsed)   // if the whole right-half is already collapsed...
			{
				// ... first enable it.
				splitContainerLeftRight.Panel2Collapsed = false;

				// glitch, whichever half (consultant's or coach's) was collapsed last will still be active even
				//  though it's menu item will be reset. So we need to hide it if we're enabling the other one
				if (!splitContainerMentorNotes.Panel1Collapsed) // this means it's not actually hidden
					splitContainerMentorNotes.Panel1Collapsed = true;
			}

			splitContainerMentorNotes.Panel2Collapsed = false;
		}

		private void splitContainerLeftRight_Panel2_SizeChanged(object sender, EventArgs e)
		{
			// if (!splitContainerMentorNotes.Panel1Collapsed)
				foreach (Control ctrl in flowLayoutPanelConsultantNotes.Controls)
				{
					if (ctrl is ConsultNotesControl)
					{
						ConsultNotesControl aConsultNoteCtrl = (ConsultNotesControl)ctrl;
						aConsultNoteCtrl.UpdateHeight(Panel2_Width);
					}
				}

			// if (!splitContainerMentorNotes.Panel2Collapsed)  these should be done even if invisible
				foreach (Control ctrl in flowLayoutPanelCoachNotes.Controls)
				{
					if (ctrl is ConsultNotesControl)
					{
						ConsultNotesControl aConsultNoteCtrl = (ConsultNotesControl)ctrl;
						aConsultNoteCtrl.UpdateHeight(Panel2_Width);
					}
				}
		}

		private void splitContainerLeftRight_Panel1_SizeChanged(object sender, EventArgs e)
		{
			foreach (Control ctrl in flowLayoutPanelVerses.Controls)
			{
				if (ctrl is VerseBtControl)
				{
					VerseBtControl aVerseCtrl = (VerseBtControl)ctrl;
					aVerseCtrl.UpdateHeight(Panel1_Width);
				}
			}
		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			NewProjectFile();
		}

		private void projectToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			recentProjectsToolStripMenuItem.DropDownItems.Clear();
			System.Diagnostics.Debug.Assert(Properties.Settings.Default.RecentProjects.Count == Properties.Settings.Default.RecentProjectPaths.Count);
			for (int i = 0; i < Properties.Settings.Default.RecentProjects.Count; i++)
			{
				string strRecentFile = Properties.Settings.Default.RecentProjects[i];
				ToolStripItem tsi = recentProjectsToolStripMenuItem.DropDownItems.Add(strRecentFile, null, recentProjectsToolStripMenuItem_Click);
				tsi.ToolTipText = String.Format("Located in folder '{0}'", Properties.Settings.Default.RecentProjectPaths[i]);
			}

			recentProjectsToolStripMenuItem.Enabled = (recentProjectsToolStripMenuItem.DropDownItems.Count > 0);

			saveToolStripMenuItem.Enabled = (Modified && (Stories != null) && (Stories.Count > 0));
		}

		private void recentProjectsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripDropDownItem aRecentFile = (ToolStripDropDownItem)sender;
			string strProjectName = aRecentFile.Text;
			System.Diagnostics.Debug.Assert(Properties.Settings.Default.RecentProjects.Contains(strProjectName));
			int nIndexOfPath = Properties.Settings.Default.RecentProjects.IndexOf(strProjectName);
			string strProjectPath = Properties.Settings.Default.RecentProjectPaths[nIndexOfPath];
			try
			{
				OpenProject(strProjectPath, strProjectName);
			}
			catch (ProjectSettings.ProjectFileNotFoundException ex)
			{
				// the file doesn't exist anymore, so remove it from the recent used list
				int nIndex = Properties.Settings.Default.RecentProjects.IndexOf(strProjectName);
				System.Diagnostics.Debug.Assert(nIndex != -1);
				Properties.Settings.Default.RecentProjects.RemoveAt(nIndex);
				Properties.Settings.Default.RecentProjectPaths.RemoveAt(nIndex);
				Properties.Settings.Default.Save();
				MessageBox.Show(ex.Message,  StoriesData.CstrCaption);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message,  StoriesData.CstrCaption);
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();    // "Closing" event will take care of checking for save
		}

		protected string StoryName
		{
			get { return (string)comboBoxStorySelector.SelectedItem; }
		}

		public XElement GetXml
		{
			get
			{
				System.Diagnostics.Debug.Assert((Stories != null) && (Stories.Count > 0));
				return Stories.GetXml;
			}
		}

		internal void SetStatusBar(string strText)
		{
			statusLabel.Text = strText;
		}

		private void buttonsStoryStage_DropDownOpening(object sender, EventArgs e)
		{
			if ((Stories == null) || (theCurrentStory == null))
				return;

			buttonsStoryStage.DropDown.Items.Clear();

			// get the current StateTransition object and find all of the allowable transition states
			StoryStageLogic.StateTransition theCurrentST = StoryStageLogic.stateTransitions[theCurrentStory.ProjStage.ProjectStage];
			System.Diagnostics.Debug.Assert(theCurrentST != null);

			if (AddListOfButtons(theCurrentST.AllowableBackwardsTransitions))
				buttonsStoryStage.DropDown.Items.Add(new ToolStripSeparator());
			AddListOfButtons(theCurrentST.AllowableForwardsTransitions);
		}

		protected bool AddListOfButtons(List<StoryStageLogic.ProjectStages> allowableTransitions)
		{
			if (allowableTransitions.Count == 0)
				return false;

			foreach (StoryStageLogic.ProjectStages eAllowableTransition in allowableTransitions)
			{
				// put the allowable transitions into the DropDown list
				StoryStageLogic.StateTransition aST = StoryStageLogic.stateTransitions[eAllowableTransition];
				ToolStripItem tsi = buttonsStoryStage.DropDown.Items.Add(
					aST.StageDisplayString, null, OnSelectOtherState);
				tsi.Tag = aST;
			}
			return true;
		}

		protected void OnSelectOtherState(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(sender is ToolStripItem);
			ToolStripItem tsi = (ToolStripItem)sender;
			StoryStageLogic.StateTransition theNewST = (StoryStageLogic.StateTransition)tsi.Tag;
			DoNextSeveral(theNewST);
		}

		protected void DoNextSeveral(StoryStageLogic.StateTransition theNewST)
		{
			if (!theCurrentStory.ProjStage.IsChangeOfStateAllowed(LoggedOnMember))
				return;

			// NOTE: the new state may actually be a previous state
			StoryStageLogic.StateTransition theCurrentST = null;
			do
			{
				theCurrentST = StoryStageLogic.stateTransitions[theCurrentStory.ProjStage.ProjectStage];

				// if we're going backwards, then just set the new state and update the view
				if ((int)theCurrentST.CurrentStage > (int)theNewST.CurrentStage)
				{
					System.Diagnostics.Debug.Assert(theCurrentST.IsTransitionValid(theNewST.CurrentStage));
					// if this is the last transition before they lose edit privilege, then make
					//  sure they really want to do this.
					if (theCurrentST.IsTerminalTransition(theNewST.CurrentStage) && (theNewST.MemberTypeWithEditToken != LoggedOnMember.MemberType))
						if (MessageBox.Show(
								String.Format(theCurrentST.TerminalTransitionMessage,
								TeamMemberData.GetMemberTypeAsDisplayString(theNewST.MemberTypeWithEditToken),
								theNewST.StageDisplayString),
							 StoriesData.CstrCaption, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
							return;

					theCurrentStory.ProjStage.ProjectStage = theNewST.CurrentStage;
					SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage);
					break;
				}
				else if (theCurrentST.CurrentStage != theNewST.CurrentStage)
					if (!DoNextState(false))
						break;
			}
			while (theCurrentST.NextState != theNewST.CurrentStage);
			InitAllPanes();
		}

		private void buttonsStoryStage_ButtonClick(object sender, EventArgs e)
		{
			DoNextState(true);
		}

		protected bool DoNextState(bool bDoUpdateCtrls)
		{
			System.Diagnostics.Debug.Assert((Stories != null) && (Stories.ProjSettings != null));
			if (SetNextStateIfReady())
			{
				SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage);
				if (bDoUpdateCtrls)
					InitAllPanes();    // just in case there were changes
				return true;
			}
			return false;
		}

		protected bool SetNextStateIfReady()
		{
			if (!theCurrentStory.ProjStage.IsChangeOfStateAllowed(LoggedOnMember))
				return false;

			StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[theCurrentStory.ProjStage.ProjectStage];
			bool bRet = st.IsReadyForTransition(this, Stories, theCurrentStory);
			if (bRet)
			{
				StoryStageLogic.StateTransition stNext = StoryStageLogic.stateTransitions[st.NextState];
				if (st.IsTerminalTransition(st.NextState))
					if (MessageBox.Show(
							String.Format(st.TerminalTransitionMessage,
								TeamMemberData.GetMemberTypeAsDisplayString(stNext.MemberTypeWithEditToken),
								stNext.StageDisplayString),
							 StoriesData.CstrCaption, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
						return false;
				theCurrentStory.ProjStage.ProjectStage = st.NextState;  // if we are ready, then go ahead and transition
			}
			return bRet;
		}

		private void storyToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			enterTheReasonThisStoryIsInTheSetToolStripMenuItem.Enabled = ((theCurrentStory != null) &&
																		  (theCurrentStory.CraftingInfo != null));
			deleteStoryToolStripMenuItem.Enabled = (theCurrentStory != null);
			showFullStorySetToolStripMenuItem.Enabled = ((Stories != null) && (Stories.Count > 0));
			addNewStoryAfterToolStripMenuItem.Enabled = (Stories != null);
		}

		private void enterTheReasonThisStoryIsInTheSetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			QueryStoryPurpose();
		}

		private void showFullStorySetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PanoramaView dlg = new PanoramaView(Stories);
			dlg.ShowDialog();

			Modified |= dlg.Modified;
		}

		private void deleteStoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(theCurrentStory != null);
			int nIndex = Stories.IndexOf(theCurrentStory);
			Stories.RemoveAt(nIndex);
			System.Diagnostics.Debug.Assert(comboBoxStorySelector.Items.IndexOf(theCurrentStory.Name) == nIndex);
			comboBoxStorySelector.Items.Remove(theCurrentStory.Name);

			if (nIndex > 0)
				nIndex--;
			if (nIndex < Stories.Count)
				comboBoxStorySelector.SelectedItem = Stories[nIndex].Name;
		}

		private void StoryEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			// the CheckForSaveDirtyFile automatically saves... so just to leave the user
			//  *some* way of backing out of changes, do it differently if they click the
			//  'X' in the upper right corner (i.e. this routine is called)
			if (Modified)
			{
				DialogResult res = MessageBox.Show("Would you like to save your changes before you exit?",  StoriesData.CstrCaption, MessageBoxButtons.YesNoCancel);
				if (res == DialogResult.Cancel)
					return;
				if (res == DialogResult.Yes)
					CheckForSaveDirtyFile();
			}
		}

		private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			copyToolStripMenuItem.Enabled =
				copyNationalBackTranslationToolStripMenuItem.Enabled =
				copyEnglishBackTranslationToolStripMenuItem.Enabled =
				((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

		}

		private void copyStoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			System.Diagnostics.Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = theCurrentStory.Verses[0].VernacularText.ToString();
			for (int i = 1; i < theCurrentStory.Verses.Count; i++)
			{
				VerseData aVerse = theCurrentStory.Verses[i];
				strStory += ' ' + aVerse.VernacularText.ToString();
			}

			Clipboard.SetText(strStory);
		}

		private void copyNationalBackTranslationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			System.Diagnostics.Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = theCurrentStory.Verses[0].NationalBTText.ToString();
			for (int i = 1; i < theCurrentStory.Verses.Count; i++)
			{
				VerseData aVerse = theCurrentStory.Verses[i];
				strStory += ' ' + aVerse.NationalBTText.ToString();
			}

			Clipboard.SetText(strStory);
		}

		private void copyEnglishBackTranslationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			System.Diagnostics.Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = theCurrentStory.Verses[0].InternationalBTText.ToString();
			for (int i = 1; i < theCurrentStory.Verses.Count; i++)
			{
				VerseData aVerse = theCurrentStory.Verses[i];
				strStory += ' ' + aVerse.InternationalBTText.ToString();
			}

			Clipboard.SetText(strStory);
		}

		/*
			Repository repo = new Repository(this, true);
			if (!repo.Exists)
				repo.Create();

			repo.SynchronizeWithRemote();
		*/
	}
}