#define UsingHtmlDisplayForConNotes

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Xsl;
using ECInterfaces;
using Palaso.UI.WindowsForms.Keyboarding;
using SilEncConverters31;
using System.Diagnostics;               // Process
using Palaso.Reporting;
using Control=System.Windows.Forms.Control;
using Timer=System.Windows.Forms.Timer;

namespace OneStoryProjectEditor
{
	// have to make this com visible, because 'this' needs to be visible to COM for the
	// call to: webBrowserNetBible.ObjectForScripting = this;
	public partial class StoryEditor : Form
	{
		internal const string CstrButtonDropTargetName = "buttonDropTarget";

		internal StoryProjectData StoryProject;
		internal StoryData theCurrentStory;
		protected string _strStoriesSet;

		// we keep a copy of this, because it ought to persist across multiple files
		internal TeamMemberData LoggedOnMember;
		internal bool Modified;
		internal Timer myFocusTimer = new Timer();
		protected Timer mySaveTimer = new Timer();

		private const int CnIntervalBetweenAutoSaveReqs = 5*1000*60;
		protected DateTime tmLastSync = DateTime.Now;
		protected TimeSpan tsBackupTime = new TimeSpan(1, 0, 0);

		public StoryEditor(string strStoriesSet)
		{
			myFocusTimer.Tick += TimeToSetFocus;
			myFocusTimer.Interval = 200;

			_strStoriesSet = strStoriesSet;

			InitializeComponent();

			panoramaToolStripMenuItem.Visible = IsInStoriesSet;
			useSameSettingsForAllStoriesToolStripMenuItem.Checked = Properties.Settings.Default.LastUseForAllStories;
			enabledToolStripMenuItem.Checked = Properties.Settings.Default.AutoSaveTimeoutEnabled;
			asSilentlyAsPossibleToolStripMenuItem.Checked = Properties.Settings.Default.DoAutoSaveSilently;

			if (enabledToolStripMenuItem.Checked)
			{
				//autosave timer goes off every 5 minutes.
				mySaveTimer.Tick += TimeToSave;
				mySaveTimer.Interval = CnIntervalBetweenAutoSaveReqs;
				mySaveTimer.Start();
			}

			try
			{
				InitializeNetBibleViewer();
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format(Properties.Resources.IDS_NeedToReboot, Environment.NewLine, ex.Message), OseResources.Properties.Resources.IDS_Caption);
			}

			try
			{
				if (String.IsNullOrEmpty(Properties.Settings.Default.LastUserType))
					NewProjectFile();
				else if ((Properties.Settings.Default.LastUserType == TeamMemberData.CstrProjectFacilitator)
						&& !String.IsNullOrEmpty(Properties.Settings.Default.LastProject))
					OpenProject(Properties.Settings.Default.LastProjectPath, Properties.Settings.Default.LastProject);
			}
			catch { }   // this was only a bene anyway, so just ignore it
		}

		private const int CnSecondsToDelyLastKeyPress = 7;
		private DateTime _tmLastKeyPressedTimeStamp;
		internal DateTime LastKeyPressedTimeStamp
		{
			get { return _tmLastKeyPressedTimeStamp; }
			set
			{
				_tmLastKeyPressedTimeStamp = value;

				// if the Bible Pane's auto hide checkbox is unchecked, then
				//  hide it when typing
				if (!netBibleViewer.checkBoxAutoHide.Checked)
					splitContainerUpDown.Minimize();
			}
		}
		protected TimeSpan tsLastKeyPressDelay = new TimeSpan(0, 0, CnSecondsToDelyLastKeyPress);

		private void TimeToSave(object sender, EventArgs e)
		{
			mySaveTimer.Stop();

			if (Modified)
			{
				// don't do it *now* if the user is typing
				if ((DateTime.Now - LastKeyPressedTimeStamp) < tsLastKeyPressDelay)
				{
					// wait at least 3 secs from the last key press
					mySaveTimer.Interval = CnSecondsToDelyLastKeyPress*1000;
				}
				else
				{
					DialogResult res = DialogResult.Yes;

					if (!asSilentlyAsPossibleToolStripMenuItem.Checked)
						res = MessageBox.Show(Properties.Resources.IDS_SaveChanges, OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);

					if (res == DialogResult.Yes)
					{
						SaveClicked();
						return;
					}
				}
			}

			mySaveTimer.Start();
		}

		internal StoriesData TheCurrentStoriesSet
		{
			get
			{
				Debug.Assert((StoryProject != null) && !String.IsNullOrEmpty(_strStoriesSet) && (StoryProject[_strStoriesSet] != null));
				return StoryProject[_strStoriesSet];
			}
		}

		internal bool IsInStoriesSet
		{
			get { return (_strStoriesSet != OseResources.Properties.Resources.IDS_ObsoleteStoriesSet); }
		}

		internal ApplicationException CantEditOldStoriesEx
		{
			get { return new ApplicationException("The stories are not editable in the 'Old Stories' set"); }
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
					if (openFileDialog.FileName != ProjectSettings.GetDefaultProjectFilePath(strProjectName))
					{
						// this means that the file is not in the default location... But before we can go ahead, we need to
						//  check to see if a project already exists with this name in the default location on the disk.
						// Here's the situation: the user has 'NewDataSet' in the default location and tries to 'browse/add'
						//  a 'NewDataSet' from another location. In that case, it isn't strictly true that finding the one
						//  in the default location means we will have to overwrite the existing project file (as threatened in
						//  the message box below). However, it is true, that the RecentProjects list will lose the reference to
						//  the existing one. So if the user cares anything about the existing one at all, they aren't going to
						//  want to do that... So let's be draconian and actually overwrite the file if they say 'yes'. This way,
						//  if they care, they'll say 'no' instead and give it a different name.
						string strFilename = ProjectSettings.GetDefaultProjectFilePath(strProjectName);
						if (File.Exists(strFilename))
						{
							DialogResult res = MessageBox.Show(String.Format(Properties.Resources.IDS_OverwriteProject, strProjectName), OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
							if (res != DialogResult.Yes)
								throw StoryProjectData.BackOutWithNoUI;

							NewProjectWizard.RemoveProject(strFilename, strProjectName);
						}
					}

					ProjectSettings projSettings = new ProjectSettings(Path.GetDirectoryName(openFileDialog.FileName), strProjectName);
					OpenProject(projSettings);
				}
			}
			catch (StoryProjectData.BackOutWithNoUIException)
			{
				// sub-routine has taken care of the UI, just exit without doing anything
			}
			catch (Exception ex)
			{
				string strErrorMsg = String.Format(Properties.Resources.IDS_UnableToOpenProjectFile,
					Environment.NewLine, strProjectName,
					((ex.InnerException != null) ? ex.InnerException.Message : ""), ex.Message);
				MessageBox.Show(strErrorMsg, OseResources.Properties.Resources.IDS_Caption);
				return;
			}
		}

		private void projectFromTheInternetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var dlg = new Chorus.UI.Clone.GetCloneFromInternetDialog(ProjectSettings.OneStoryProjectFolderRoot))
			{
				if (DialogResult.Cancel == dlg.ShowDialog())
					return;

				string strProjectName = Path.GetFileNameWithoutExtension(dlg.PathToNewProject);

				// we can save this information so we can use it automatically during the next restart
				string strUsername = ExtractUsernameFromUrl(dlg.ThreadSafeUrl);
				Program.SetHgParameters(dlg.PathToNewProject, strProjectName, dlg.ThreadSafeUrl, strUsername);
				ProjectSettings projSettings = new ProjectSettings(dlg.PathToNewProject, strProjectName);
				try
				{
					OpenProject(projSettings);
				}
				catch (Exception)
				{
					string strErrorMsg = String.Format(Properties.Resources.IDS_NoProjectFromInternet,
													   Environment.NewLine, dlg.ThreadSafeUrl);
					MessageBox.Show(strErrorMsg, OseResources.Properties.Resources.IDS_Caption);
				}
			}
		}

		private void projectFromASharedNetworkDriveToolStripMenu_Click(object sender, EventArgs e)
		{
			// has to be a "same-named" project open currently that we're just going to push
			//  to the network drive
			Debug.Assert((StoryProject != null) && (StoryProject.ProjSettings != null));
			openFileDialog.FileName = ProjectSettings.OneStoryFileName(StoryProject.ProjSettings.ProjectName);
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				// can't be the same as the current project!
				if (openFileDialog.FileName != StoryProject.ProjSettings.ProjectFilePath)
				{
					if (Path.GetFileNameWithoutExtension(openFileDialog.FileName) == StoryProject.ProjSettings.ProjectName)
					{
						string strNetworkDriveFolder = Path.GetDirectoryName(openFileDialog.FileName);
						Program.SetHgParametersNetworkDrive(StoryProject.ProjSettings.ProjectFolder,
															StoryProject.ProjSettings.ProjectName,
															strNetworkDriveFolder);
					}
					else
					{
						MessageBox.Show(Properties.Resources.IDS_MustBeCloneRepo,
										OseResources.Properties.Resources.IDS_Caption);
					}
				}
				else
				{
					MessageBox.Show(Properties.Resources.IDS_CantPushToTheLocalRepo,
									OseResources.Properties.Resources.IDS_Caption);
				}
			}
		}

		private static string ExtractUsernameFromUrl(string url)
		{
			// e.g. http://bobeaton:helpmepld@hg-private.languagedepot.org/
			int nIndex = url.IndexOf("//") + 2;
			if (nIndex != -1)
			{
				int nIndexEnd = url.IndexOf(':', nIndex);
				if (nIndexEnd != -1)
					return url.Substring(nIndex, nIndexEnd - nIndex);
			}
			return null;
		}

		protected void CloseProjectFile()
		{
			StoryProject = null;
			LoggedOnMember = null;
			ClearState();

			ReInitMenuVisibility();

			// restart the last sync timer whenever we switch projects
			tmLastSync = DateTime.Now;
		}

		protected void ReInitMenuVisibility()
		{
			// some that might have been made invisible need to be given a fair chance for next time
			copyStoryToolStripMenuItem.Visible =
				deleteStoryVersesToolStripMenuItem.Visible =
				deleteStoryNationalBackTranslationToolStripMenuItem.Visible =
				copyNationalBackTranslationToolStripMenuItem.Visible =
				deleteEnglishBacktranslationToolStripMenuItem.Visible =
				copyEnglishBackTranslationToolStripMenuItem.Visible =
				deleteTestToolStripMenuItem.Visible =
				/* viewVernacularLangFieldMenuItem.Visible =
				viewNationalLangFieldMenuItem.Visible =
				viewEnglishBTFieldMenuItem.Visible = */ true;
		}

		protected void ClearState()
		{
			ClearFlowControls();
			CtrlTextBox._inTextBox = null;
			theCurrentStory = null;
			StoryStageLogic.stateTransitions = null;
			comboBoxStorySelector.Items.Clear();
			comboBoxStorySelector.Text = Properties.Resources.IDS_EnterStoryName;
			textBoxStoryVerse.Text = Properties.Resources.IDS_Story;

			if (!useSameSettingsForAllStoriesToolStripMenuItem.Checked)
			{
				viewConsultantNoteFieldMenuItem.Checked =
					viewCoachNotesFieldMenuItem.Checked =
					viewNetBibleMenuItem.Checked = false;
			}
		}

		protected void NewProjectFile()
		{
			if (!CheckForSaveDirtyFile())
				return;

			CloseProjectFile();
			comboBoxStorySelector.Focus();

			// for a new project, we don't want to automatically log in (since this will be the first
			//  time editing the new project and we need to add at least the current user)
			LoggedOnMember = null;
			Debug.Assert(StoryProject == null);
			projectLoginToolStripMenuItem_Click(null, null);

			if ((StoryProject != null) && (StoryProject.ProjSettings != null))
			{
				UpdateRecentlyUsedLists(StoryProject.ProjSettings);
				UpdateUIMenusWithShortCuts();
			}
		}

		protected bool InitNewStoryProjectObject()
		{
			Debug.Assert(StoryProject == null);

			try
			{
				StoryProject = new StoryProjectData();    // null causes us to query for the project name
				Modified = StoryProject.InitializeProjectSettings(LoggedOnMember);
				CheckForLogon(StoryProject);
				Debug.Assert(LoggedOnMember != null);

				if (Modified)
					SaveClicked();

				return true;
			}
			catch (StoryProjectData.BackOutWithNoUIException)
			{
				// sub-routine has taken care of the UI, just exit without doing anything
				// why??? StoryProject = null;
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format(Properties.Resources.IDS_UnableToOpenMemberList,
					Environment.NewLine, ex.Message), OseResources.Properties.Resources.IDS_Caption);
			}

			return false;
		}

		private void projectSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert((StoryProject != null) && (StoryProject.ProjSettings != null) && (LoggedOnMember != null));

			try
			{
				Modified = StoryProject.InitializeProjectSettings(LoggedOnMember);

				if (Modified)
				{
					SaveClicked();

					if (theCurrentStory != null)
					{
						// reload the state transitions so that we can possible support a new
						//  configuration (e.g. there might now be a FPM)
						Debug.Assert(StoryProject.TeamMembers != null);
						StoryStageLogic.stateTransitions =
							new StoryStageLogic.StateTransitions(StoryProject.ProjSettings.ProjectFolder);
						ReInitMenuVisibility();
						SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage, true);
						InitAllPanes(); // just in case e.g. the font or RTL value changed
					}
				}
			}
			catch (StoryProjectData.BackOutWithNoUIException)
			{
			}
		}

		private void projectLoginToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (StoryProject == null)
				{
					InitNewStoryProjectObject();
				}
				else
				{
					// detect if the logged on member type changed, and if so, redo the Consult Notes panes
					string strMemberName = null;
					if (LoggedOnMember != null)
						strMemberName = LoggedOnMember.Name;

					LoggedOnMember = StoryProject.EditTeamMembers(strMemberName, null, ref Modified);

					if (theCurrentStory != null)
					{
						InitAllPanes(theCurrentStory.Verses);
						WarnIfNotProperMemberType();
					}
				}
			}
			catch { }   // this might throw if the user cancels, but we don't care
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

			// see if we can update from a repository first before opening.
			string strDotHgFolder = projSettings.ProjectFolder + @"\.hg";
			if (IsInStoriesSet && Directory.Exists(strDotHgFolder))
			{
				// clean up any existing open projects
				if (!CheckForSaveDirtyFile())
					return;

				CloseProjectFile();

				Program.SyncWithRepository(projSettings.ProjectFolder, true);
			}

			OpenProject(projSettings);
		}

		protected void OpenProject(ProjectSettings projSettings)
		{
			// clean up any existing open projects
			if (!CheckForSaveDirtyFile())
				return;

			CloseProjectFile();

			// next, insure that the file for the project exists (do this outside the try,
			//  so the caller is informed of no file (so, for eg., it can remove from recently
			//  used list.
			projSettings.ThrowIfProjectFileDoesntExists();

			UpdateRecentlyUsedLists(projSettings);

			try
			{

				// serialize in the file
				NewDataSet projFile = new NewDataSet();
				projFile.ReadXml(projSettings.ProjectFilePath);

				// get the data into another structure that we use internally (more flexible)
				StoryProject = GetOldStoryProjectData(projFile, projSettings);

				string strStoryToLoad = null;
				if (TheCurrentStoriesSet.Count > 0)
				{
					LoadComboBox();
					strStoryToLoad = TheCurrentStoriesSet[0].Name;    // default
				}

				// check whether we should be showing the transliteration or not
				Debug.Assert(LoggedOnMember != null);
				viewTransliterationVernacular.Checked = !String.IsNullOrEmpty(LoggedOnMember.TransliteratorVernacular);
				viewTransliterationNationalBT.Checked = !String.IsNullOrEmpty(LoggedOnMember.TransliteratorNationalBT);

				// check for project settings that might have been saved from a previous session
				if (!String.IsNullOrEmpty(Properties.Settings.Default.LastStoryWorkedOn) && comboBoxStorySelector.Items.Contains(Properties.Settings.Default.LastStoryWorkedOn))
					strStoryToLoad = Properties.Settings.Default.LastStoryWorkedOn;

				if (!String.IsNullOrEmpty(strStoryToLoad) && comboBoxStorySelector.Items.Contains(strStoryToLoad))
					comboBoxStorySelector.SelectedItem = strStoryToLoad;

				UpdateUIMenusWithShortCuts();

				if (IsInStoriesSet)
				{
					Text = String.Format(Properties.Resources.IDS_MainFrameTitle, StoryProject.ProjSettings.ProjectName);
				}
				else
				{
					Text = String.Format(Properties.Resources.IDS_MainFrameTitleOldStories, StoryProject.ProjSettings.ProjectName);
				}
			}
			catch (StoryProjectData.BackOutWithNoUIException)
			{
				// sub-routine has taken care of the UI, just exit without doing anything
			}
			catch (Exception ex)
			{
				string strErrorMsg = String.Format(Properties.Resources.IDS_UnableToOpenProjectFile,
					Environment.NewLine, projSettings.ProjectName,
					((ex.InnerException != null) ? ex.InnerException.Message : ""), ex.Message);
				MessageBox.Show(strErrorMsg, OseResources.Properties.Resources.IDS_Caption);
			}
		}

		protected void LoadComboBox()
		{
			// populate the combo boxes with all the existing story names
			foreach (StoryData aStory in TheCurrentStoriesSet)
				comboBoxStorySelector.Items.Add(aStory.Name);
		}

		protected void CheckForLogon(StoryProjectData theStoryProject)
		{
			if (LoggedOnMember == null)
				LoggedOnMember = theStoryProject.GetLogin(ref Modified);
		}

		protected StoryProjectData GetOldStoryProjectData(NewDataSet projFile, ProjectSettings projSettings)
		{
			StoryProjectData theOldStoryProject = new StoryProjectData(projFile, projSettings);
			CheckForLogon(theOldStoryProject);
			return theOldStoryProject;
		}

		private void insertNewStoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string strStoryName;
			int nIndexOfCurrentStory = -1;
			if (AddNewStoryGetIndex(ref nIndexOfCurrentStory, out strStoryName))
			{
				Debug.Assert(nIndexOfCurrentStory != -1);
				InsertNewStory(strStoryName, nIndexOfCurrentStory);
			}
		}

		protected bool CheckForProjFac()
		{
			Debug.Assert(LoggedOnMember != null);
			if ((LoggedOnMember == null) || (LoggedOnMember.MemberType != TeamMemberData.UserTypes.eProjectFacilitator))
			{
				MessageBox.Show(Properties.Resources.IDS_LogInAsProjFac, OseResources.Properties.Resources.IDS_Caption);
				return false;
			}
			return true;
		}

		protected bool AddNewStoryGetIndex(ref int nIndexForInsert, out string strStoryName)
		{
			Debug.Assert(LoggedOnMember != null);
			if (!CheckForProjFac())
			{
				strStoryName = null;
				return false;
			}

			// ask the user for what story they want to add (i.e. the name)
			strStoryName = Microsoft.VisualBasic.Interaction.InputBox(Properties.Resources.IDS_EnterStoryToAdd, OseResources.Properties.Resources.IDS_Caption, null, 300, 200);
			if (!String.IsNullOrEmpty(strStoryName))
			{
				if (TheCurrentStoriesSet.Count > 0)
				{
					foreach (StoryData aStory in TheCurrentStoriesSet)
						if (aStory.Name == strStoryName)
						{
							// if they already have a story by that name, just go there
							comboBoxStorySelector.SelectedItem = strStoryName;
							return false;
						}
						else if (aStory.Name == theCurrentStory.Name)
						{
							nIndexForInsert = TheCurrentStoriesSet.IndexOf(aStory);
							return true;
						}
				}
				else
				{
					nIndexForInsert = 0;
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
				Debug.Assert(nIndexOfCurrentStory != -1);
				nIndexOfCurrentStory = Math.Min(nIndexOfCurrentStory + 1, TheCurrentStoriesSet.Count);
				InsertNewStory(strStoryName, nIndexOfCurrentStory);
			}
		}

		TimeSpan tsFalseEnter = new TimeSpan(0, 0, 1);
		DateTime dtLastKey = DateTime.Now;
		private void comboBoxStorySelector_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)    // user just finished entering a story name to select (or add)
			{
				// ignore false double-Enters (from hitting enter in the dialog box for 'CheckForProjFac')
				if ((DateTime.Now - dtLastKey) < tsFalseEnter)
					return;

				if (StoryProject == null)
					if (!InitNewStoryProjectObject())
					{
						comboBoxStorySelector.Text = Properties.Resources.IDS_EnterStoryName;
						return;
					}

				CheckForLogon(StoryProject);

				if (!CheckForProjFac())
				{
					comboBoxStorySelector.Text = Properties.Resources.IDS_EnterStoryName;
					dtLastKey = DateTime.Now;
					return;
				}

				int nInsertIndex = 0;
				StoryData theStory = null;
				string strStoryToLoad = comboBoxStorySelector.Text;
				for (int i = 0; i < TheCurrentStoriesSet.Count; i++)
				{
					StoryData aStory = TheCurrentStoriesSet[i];
					if ((theCurrentStory != null) && (theCurrentStory == aStory))
						nInsertIndex = i + 1;
					if (aStory.Name == strStoryToLoad)
						theStory = aStory;
				}

				if (theStory == null)
				{
					if (MessageBox.Show(String.Format(Properties.Resources.IDS_UnableToFindStoryAdd, strStoryToLoad), OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
					{
						Debug.Assert(!comboBoxStorySelector.Items.Contains(strStoryToLoad));
						InsertNewStory(strStoryToLoad, nInsertIndex);
					}
					else
						comboBoxStorySelector.Text = Properties.Resources.IDS_EnterStoryName;
				}
				else
					comboBoxStorySelector.SelectedItem = theStory.Name;
			}
		}

		protected void InsertNewStory(string strStoryName, int nIndexToInsert)
		{
			if (!CheckForSaveDirtyFile())
				return;

			// query for the crafter
			MemberPicker dlg = new MemberPicker(StoryProject, TeamMemberData.UserTypes.eCrafter);
			dlg.Text = Properties.Resources.IDS_ChooseTheStoryCrafter;
			if ((dlg.ShowDialog() != DialogResult.OK) || (dlg.SelectedMember == null))
				return;

			string strCrafterGuid = dlg.SelectedMember.MemberGuid;

			DialogResult res = MessageBox.Show(Properties.Resources.IDS_IsThisStoryFromTheBible, OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
			if (res == DialogResult.Cancel)
				return;

			Debug.Assert(LoggedOnMember.MemberType == TeamMemberData.UserTypes.eProjectFacilitator);
			Debug.Assert(StoryProject.TeamMembers != null);
			StoryData theNewStory = new StoryData(strStoryName, strCrafterGuid,
				LoggedOnMember.MemberGuid,
				(res == DialogResult.Yes),
				StoryProject.ProjSettings);
			InsertNewStoryAdjustComboBox(theNewStory, nIndexToInsert);
		}

		protected void InsertNewStoryAdjustComboBox(StoryData theNewStory, int nIndexToInsert)
		{
			theCurrentStory = theNewStory;
			comboBoxStorySelector.Items.Insert(nIndexToInsert, theNewStory.Name);
			TheCurrentStoriesSet.Insert(nIndexToInsert, theCurrentStory);
			comboBoxStorySelector.SelectedItem = theNewStory.Name;
			Modified = true;
			InitAllPanes();
		}

		private bool _bCancellingChange = false;
		private void comboBoxStorySelector_SelectedIndexChanged(object sender, EventArgs e)
		{
			// do nothing if we're already on this story:
			if (_bCancellingChange
				&&  (theCurrentStory != null)
				&& (theCurrentStory.Name == (string)comboBoxStorySelector.SelectedItem))
				return;

			// save the file before moving on.
			if (!CheckForSaveDirtyFile())
			{
				// if we're backing out, try to reset the combo box with the current story
				if ((theCurrentStory != null) && (theCurrentStory.Name != (string)comboBoxStorySelector.SelectedItem))
				{
					_bCancellingChange = true;
					comboBoxStorySelector.SelectedItem = theCurrentStory.Name;
					_bCancellingChange = false;
				}
				return;
			}

			// if this happens, it means we didn't save or cleanup the document
			Debug.Assert(!Modified
						 || (flowLayoutPanelVerses.Controls.Count != 0));
#if UsingHtmlDisplayForConNotes
#else
				|| (flowLayoutPanelConsultantNotes.Controls.Count != 0)
				|| (flowLayoutPanelCoachNotes.Controls.Count != 0)); // if this happens, it means we didn't save or cleanup the document
#endif
			// we might could come thru here without having opened any file (e.g. after New)
			if (StoryProject == null)
				if (!InitNewStoryProjectObject())
					return;

			// find the story they've chosen (this shouldn't be possible to fail)
			foreach (StoryData aStory in TheCurrentStoriesSet)
				if (aStory.Name == (string)comboBoxStorySelector.SelectedItem)
				{
					theCurrentStory = aStory;
					break;
				}
			Debug.Assert(theCurrentStory != null);
			if (IsInStoriesSet)
			{
				Properties.Settings.Default.LastStoryWorkedOn = theCurrentStory.Name;
				Properties.Settings.Default.Save();

				// see if this PF is the one who's editing the story
				if (LoggedOnMember.MemberType == TeamMemberData.UserTypes.eProjectFacilitator)
					theCurrentStory.CheckForProjectFacilitator(StoryProject, LoggedOnMember);
			}

			// initialize the text box showing the storying they're editing
			textBoxStoryVerse.Text = Properties.Resources.IDS_StoryColon + theCurrentStory.Name;

			// initialize the project stage details (which might hide certain views)
			//  (do this *after* initializing the whole thing, because if we save, we'll
			//  want to save even the hidden pieces)
			// BUT to avoid the multiple repaints, temporarily disable the painting
			SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage, true);

			// forget things:
			CtrlTextBox._nLastVerse = -1;

			if (m_frmFind != null)
				// if the user switches stories, then we need to reindex the search
				m_frmFind.ResetSearchParameters();

			// finally, initialize the verse controls
			InitAllPanes();

			// get the focus off the combo box, so mouse scroll doesn't rip thru the stories!
			flowLayoutPanelVerses.Focus();
		}

		private bool _bNagOnce = true;
		private void WarnIfNotProperMemberType()
		{
			// inform the user that they won't be able to edit this if they aren't the proper member type
			Debug.Assert((theCurrentStory != null) && (LoggedOnMember != null));
			if (_bNagOnce)
				LoggedOnMember.CheckIfThisAnAcceptableEditorForThisStory(theCurrentStory);
			_bNagOnce = false;
		}

		protected void InitAllPanes(VersesData theVerses)
		{
			// the first verse (for global ConNotes) should have been initialized by now
			Debug.Assert(theVerses.FirstVerse != null);

			int nLastVerseInFocus = CtrlTextBox._nLastVerse;
			StringTransfer stLast = (CtrlTextBox._inTextBox != null)
				? CtrlTextBox._inTextBox.MyStringTransfer : null;

			ClearFlowControls();
			int nVerseIndex = 0;
			if (theVerses.Count == 0)
				theCurrentStory.Verses.InsertVerse(0, null, null, null);

			flowLayoutPanelVerses.SuspendLayout();
#if UsingHtmlDisplayForConNotes
			flowLayoutPanelVerses.LineNumberLink = linkLabelVerseBT;
			linkLabelVerseBT.Visible = true;
			htmlConsultantNotesControl.TheSE = this;
			htmlConsultantNotesControl.StoryData = theCurrentStory;
			htmlConsultantNotesControl.LineNumberLink = linkLabelConsultantNotes;
			htmlCoachNotesControl.TheSE = this;
			htmlCoachNotesControl.StoryData = theCurrentStory;
			htmlCoachNotesControl.LineNumberLink = linkLabelCoachNotes;
#else
			flowLayoutPanelConsultantNotes.SuspendLayout();
			flowLayoutPanelCoachNotes.SuspendLayout();
#endif
			SuspendLayout();

#if UsingHtmlDisplayForConNotes
#else
			// for ConNotes, there's a zeroth verse that's for global story comments
			InitConsultNotesPane(flowLayoutPanelConsultantNotes, theVerses.FirstVerse.ConsultantNotes, nVerseIndex);
			InitConsultNotesPane(flowLayoutPanelCoachNotes, theVerses.FirstVerse.CoachNotes, nVerseIndex);
#endif

			AddDropTargetToFlowLayout(nVerseIndex++);
			foreach (VerseData aVerse in theVerses)
			{
				if (aVerse.IsVisible || hiddenVersesToolStripMenuItem.Checked)
				{
					InitVerseControls(aVerse, nVerseIndex);

#if UsingHtmlDisplayForConNotes
#else
					InitConsultNotesPane(flowLayoutPanelConsultantNotes, aVerse.ConsultantNotes, nVerseIndex);

					InitConsultNotesPane(flowLayoutPanelCoachNotes, aVerse.CoachNotes, nVerseIndex);
#endif
				}

				// skip numbers, though, if we have hidden verses so that the verse nums
				//  will be the same (in case we have references to lines in the connotes)
				//  AND so it'll be a clue to the user that there are hidden verses present.
				nVerseIndex++;
			}

			flowLayoutPanelVerses.ResumeLayout(true);
#if UsingHtmlDisplayForConNotes
			// ConNotes are not done in one swell-foop via an Html control
			htmlConsultantNotesControl.LoadDocument();
			htmlCoachNotesControl.LoadDocument();
#else
			flowLayoutPanelConsultantNotes.ResumeLayout(true);
			flowLayoutPanelCoachNotes.ResumeLayout(true);
#endif
			ResumeLayout(true);

			if ((nLastVerseInFocus == -1) && (theVerses.Count > 0))
			{
				FocusOnVerse(1, false, false);
				nLastVerseInFocus = 0;
			}

			FocusOnVerse(nLastVerseInFocus, true, true);
			if ((stLast != null) && (stLast.TextBox != null))
				stLast.TextBox.Focus();
		}

		protected void InitVerseControls(VerseData aVerse, int nVerseIndex)
		{
			VerseBtControl aVerseCtrl = new VerseBtControl(this, flowLayoutPanelVerses, aVerse, nVerseIndex);
			if (!aVerse.IsVisible)
				aVerseCtrl.BackColor = System.Drawing.Color.Khaki;

			aVerseCtrl.UpdateHeight(Panel1_Width);
			flowLayoutPanelVerses.Controls.Add(aVerseCtrl);
			AddDropTargetToFlowLayout(nVerseIndex);
		}

		// this is for use by the consultant panes if we add or remove or hide a note
		internal void ReInitVerseControls()
		{
			// this sometimes gets called in bad times
			if ((theCurrentStory == null) || (theCurrentStory.Verses.Count == 0))
				return;

			int nLastVerseInFocus = CtrlTextBox._nLastVerse;
			StringTransfer stLast = (CtrlTextBox._inTextBox != null)
				? CtrlTextBox._inTextBox.MyStringTransfer : null;

			// get a new index
			int nVerseIndex = 0;
			flowLayoutPanelVerses.Controls.Clear();
			flowLayoutPanelVerses.SuspendLayout();
			SuspendLayout();

			AddDropTargetToFlowLayout(nVerseIndex++);
			foreach (VerseData aVerse in theCurrentStory.Verses)
			{
				if (aVerse.IsVisible || hiddenVersesToolStripMenuItem.Checked)
					InitVerseControls(aVerse, nVerseIndex);

				// skip numbers, though, if we have hidden verses so that the verse nums
				//  will be the same (in case we have references to lines in the connotes)
				//  AND so it'll be a clue to the user that there are hidden verses present.
				nVerseIndex++;
			}
			flowLayoutPanelVerses.ResumeLayout(true);
			ResumeLayout(true);

			FocusOnVerse(nLastVerseInFocus, true, true);
			if ((stLast != null) && (stLast.TextBox != null))
				stLast.TextBox.Focus();
		}

#if UsingHtmlDisplayForConNotes
#else
		protected void InitConsultNotesPane(ConNoteFlowLayoutPanel theFLP, ConsultNotesDataConverter aCNsDC, int nVerseIndex)
		{
			ConsultNotesControl aConsultNotesCtrl = new ConsultNotesControl(this, theFLP,
				theCurrentStory.ProjStage, aCNsDC, nVerseIndex, LoggedOnMember.MemberType);
			aConsultNotesCtrl.UpdateHeight(Panel2_Width);
			theFLP.AddCtrl(aConsultNotesCtrl);
		}

		// this is for use by the consultant panes if we add or remove or hide a single note
		internal void ReInitConsultNotesPane(ConsultNotesDataConverter aCNsD)
		{
			int nLastVerseInFocus = CtrlTextBox._nLastVerse;
			StringTransfer stLast = (CtrlTextBox._inTextBox != null)
				? CtrlTextBox._inTextBox.MyStringTransfer : null;

			int nVerseIndex = 0;
			if (flowLayoutPanelConsultantNotes.Contains(aCNsD))
			{
				flowLayoutPanelConsultantNotes.Clear();
				flowLayoutPanelConsultantNotes.SuspendLayout();
				SuspendLayout();

				// display the zeroth note (which is only for ConNotes
				InitConsultNotesPane(flowLayoutPanelConsultantNotes,
					theCurrentStory.Verses.FirstVerse.ConsultantNotes, nVerseIndex);

				foreach (VerseData aVerse in theCurrentStory.Verses)
				{
					// skip numbers, though, if we have hidden verses so that the verse nums
					//  will be the same (in case we have references to lines in the connotes)
					//  AND so it'll be a clue to the user that there are hidden verses present.
					++nVerseIndex;

					if (aVerse.IsVisible || hiddenVersesToolStripMenuItem.Checked)
						InitConsultNotesPane(flowLayoutPanelConsultantNotes,
											 aVerse.ConsultantNotes, nVerseIndex);
				}

				flowLayoutPanelConsultantNotes.ResumeLayout(true);
				ResumeLayout(true);
			}
			else
			{
				Debug.Assert(flowLayoutPanelCoachNotes.Contains(aCNsD));
				flowLayoutPanelCoachNotes.Clear();
				flowLayoutPanelCoachNotes.SuspendLayout();
				SuspendLayout();

				// display the zeroth note (which is only for ConNotes
				InitConsultNotesPane(flowLayoutPanelCoachNotes,
					theCurrentStory.Verses.FirstVerse.CoachNotes, nVerseIndex);

				foreach (VerseData aVerse in theCurrentStory.Verses)
				{
					// skip numbers, though, if we have hidden verses so that the verse nums
					//  will be the same (in case we have references to lines in the connotes)
					//  AND so it'll be a clue to the user that there are hidden verses present.
					++nVerseIndex;

					if (aVerse.IsVisible || hiddenVersesToolStripMenuItem.Checked)
						InitConsultNotesPane(flowLayoutPanelCoachNotes,
											 aVerse.CoachNotes, nVerseIndex);
				}

				flowLayoutPanelCoachNotes.ResumeLayout(true);
				ResumeLayout(true);
			}

			FocusOnVerse(nLastVerseInFocus);
			if ((stLast != null) && (stLast.TextBox != null))
				stLast.TextBox.Focus();

			// if we do this, it's because something changed
			Modified = true;
		}

		internal void HandleQueryContinueDrag(ConsultNotesControl aCNsDC, QueryContinueDragEventArgs e)
		{
			Debug.Assert(flowLayoutPanelConsultantNotes.Contains(aCNsDC._theCNsDC)
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
				Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl aCNsC = (ConsultNotesControl)ctrl;
				if (aCNsC != control)
					aCNsC.buttonDragDropHandle.Dock = DockStyle.Fill;
			}
		}

		private static void DimConsultNotesDropTargetButtons(FlowLayoutPanel theFLP, ConsultNotesControl control)
		{
			foreach (Control ctrl in theFLP.Controls)
			{
				Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl aCNsC = (ConsultNotesControl)ctrl;
				if (aCNsC != control)
					aCNsC.buttonDragDropHandle.Dock = DockStyle.Right;
			}
		}
#endif

		internal void AddNewVerse(int nInsertionIndex, int nNumberToAdd, bool bAfter)
		{
			if (bAfter)
				nInsertionIndex++;

			VersesData lstNewVerses = new VersesData();
			for (int i = 0; i < nNumberToAdd; i++)
				lstNewVerses.Add(new VerseData());

			theCurrentStory.Verses.InsertRange(nInsertionIndex, lstNewVerses);
			InitAllPanes();
			Debug.Assert(lstNewVerses.Count > 0);
			// shouldn't need to do this here (done in InitAllPanes): FocusOnVerse(nInsertionIndex);

			Modified = true;
		}

		private void TimeToSetFocus(object sender, EventArgs e)
		{
			Debug.Assert((sender != null) && (sender is Timer) && ((sender as Timer).Tag is int));
			((Timer)sender).Stop();
			int nVerseIndex = (int)((Timer)sender).Tag;
			FocusOnVerse(nVerseIndex, true, true);
		}

		/// <summary>
		/// Scroll the controls in the flow layout controls to make sure nVerseIndex line is
		/// visible.
		/// </summary>
		/// <param name="nVerseIndex">corresponds to the line number (e.g. ln: 1 == 1), but could be 0 for ConNote panes</param>
		/// <param name="bDontSyncConsultantNotePane">Don't sync the Consultant Note pane</param>
		/// <param name="bDontSyncCoachNotePane">Don't sync the Coach Note pane</param>
		public void FocusOnVerse(int nVerseIndex, bool bSyncConsultantNotePane,
			bool bSyncCoachNotePane)
		{
			// if no box was actually ever selected, then this might be -1
			if (nVerseIndex < 0)
				return;

			// light up whichever text box is visible
			// from the verses pane... (for verse controls, this is the line number, which
			//  is one more than the index we're looking for. (if this is from the zeroth
			//  line of the ConNotes, then just skip it)
			if (nVerseIndex > 0)
			{
				Control ctrl = flowLayoutPanelVerses.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is VerseBtControl);
				VerseBtControl theVerse = ctrl as VerseBtControl;

				// then scroll it into view (but not if this is the one that initiated
				//  the scrolling since it's annoying that it jumps around when greater
				//  than the height of the view).
				if ((CtrlTextBox._inTextBox == null) || (CtrlTextBox._inTextBox._ctrlVerseParent != theVerse))
					flowLayoutPanelVerses.ScrollIntoView(theVerse);
				else
					flowLayoutPanelVerses.LastControlIntoView = theVerse;
			}

			// the ConNote controls have a zeroth line, so the index is one greater
			if (viewConsultantNoteFieldMenuItem.Checked && bSyncConsultantNotePane)
			{
#if UsingHtmlDisplayForConNotes
				htmlConsultantNotesControl.ScrollToVerse(nVerseIndex);
#else
				Control ctrl = flowLayoutPanelConsultantNotes.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl theConsultantNotes = ctrl as ConsultNotesControl;
				if ((CtrlTextBox._inTextBox == null) || (CtrlTextBox._inTextBox._ctrlVerseParent != theConsultantNotes))
					flowLayoutPanelConsultantNotes.ScrollControlIntoView(theConsultantNotes);
#endif
			}

			if (viewCoachNotesFieldMenuItem.Checked && bSyncCoachNotePane)
			{
#if UsingHtmlDisplayForConNotes
				htmlCoachNotesControl.ScrollToVerse(nVerseIndex);
#else
				Control ctrl = flowLayoutPanelCoachNotes.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl theCoachNotes = ctrl as ConsultNotesControl;
				if ((CtrlTextBox._inTextBox == null) || (CtrlTextBox._inTextBox._ctrlVerseParent != theCoachNotes))
					flowLayoutPanelCoachNotes.ScrollControlIntoView(theCoachNotes);
#endif
			}
		}

		public void AddNoteAbout(VerseControl ctrlParent)
		{
			Debug.Assert(LoggedOnMember != null);
			string strNote = GetInitials(LoggedOnMember.Name) + ": Re:";
			if (ctrlParent is VerseBtControl)
			{
				VerseBtControl ctrl = ctrlParent as VerseBtControl;
				// if the control that was right-clicked on that led us here was one
				//  of the story lines, then take the selected portion of all story line
				//  controls and add it.
				if ((CtrlTextBox._inTextBox == ctrl._verseData.VernacularText.TextBox)
					|| (CtrlTextBox._inTextBox == ctrl._verseData.NationalBTText.TextBox)
					|| (CtrlTextBox._inTextBox == ctrl._verseData.InternationalBTText.TextBox))
				{
					// get selected text from all visible Story line controls
					if (viewVernacularLangFieldMenuItem.Checked)
					{
						string str = ctrl._verseData.VernacularText.TextBox.SelectedText.Trim();
						if (!String.IsNullOrEmpty(str))
							strNote += String.Format(" /{0}/", str);
					}
					if (viewNationalLangFieldMenuItem.Checked)
					{
						string str = ctrl._verseData.NationalBTText.TextBox.SelectedText.Trim();
						if (!String.IsNullOrEmpty(str))
							strNote += String.Format(" /{0}/", str);
					}
					if (viewEnglishBTFieldMenuItem.Checked)
					{
						string str = ctrl._verseData.InternationalBTText.TextBox.SelectedText.Trim();
						if (!String.IsNullOrEmpty(str))
							strNote += String.Format(" '{0}'", str);
					}
				}
				else if (CtrlTextBox._inTextBox != null)
				{
					// otherwise, it might have been a retelling or some other control
					if (!String.IsNullOrEmpty(CtrlTextBox._inTextBox._strLabel))
						strNote += CtrlTextBox._inTextBox._strLabel;

					string str = CtrlTextBox._inTextBox.SelectedText.Trim();
					if (!String.IsNullOrEmpty(str))
						strNote += String.Format(" /{0}/", str);
				}
			}
			else if (CtrlTextBox._inTextBox != null)
			// otherwise, just get the selected text out of the one box that was
			//  right-clicked in.
			{
				if (viewCoachNotesFieldMenuItem.Checked)
				{
					if (!String.IsNullOrEmpty(CtrlTextBox._inTextBox._strLabel))
						strNote += CtrlTextBox._inTextBox._strLabel;

					string str = CtrlTextBox._inTextBox.SelectedText.Trim();
					if (!String.IsNullOrEmpty(str))
						strNote += String.Format(" /{0}/", str);
				}
			}
			strNote += ". ";

			SendNoteToCorrectPane(ctrlParent.VerseNumber, strNote);
		}

		internal static string GetInitials(string name)
		{
			string[] astrNames = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string strInitials = null;
			foreach (string s in astrNames)
			{
				strInitials += s[0];
			}

			if ((strInitials != null) && (strInitials.Length == 1))
				strInitials += astrNames[0][1];
			return strInitials;
		}

		internal void SendNoteToCorrectPane(int nVerseIndex, string strNote)
		{
			if (LoggedOnMember.MemberType == TeamMemberData.UserTypes.eCoach)
			{
				if (!viewCoachNotesFieldMenuItem.Checked)
					viewCoachNotesFieldMenuItem.Checked = true;

#if UsingHtmlDisplayForConNotes
				htmlCoachNotesControl.OnAddNote(nVerseIndex, strNote);
#else
				Control ctrl = flowLayoutPanelCoachNotes.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl theCoachNotes = ctrl as ConsultNotesControl;
				StringTransfer st = theCoachNotes.DoAddNote(strNote);

				// after the note is added, the control references are no longer valid, but
				//  we want to go back to where we were... so requery the controls
				// Order: BT, then *other* connote pane and then *this* connote pane
				ctrl = flowLayoutPanelVerses.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is VerseBtControl);
				flowLayoutPanelVerses.ScrollControlIntoView(ctrl);

				if (viewConsultantNoteFieldMenuItem.Checked)
				{
					ctrl = flowLayoutPanelConsultantNotes.GetControlAtVerseIndex(nVerseIndex);
					if (ctrl == null)
						return;

					Debug.Assert(ctrl is ConsultNotesControl);
					flowLayoutPanelConsultantNotes.ScrollControlIntoView(ctrl);
				}

				ctrl = flowLayoutPanelCoachNotes.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is ConsultNotesControl);
				flowLayoutPanelCoachNotes.ScrollControlIntoView(ctrl);

				if ((st != null) && (st.TextBox != null))
					st.TextBox.Focus();
#endif
			}
			else
			{
				if (!viewConsultantNoteFieldMenuItem.Checked)
					viewConsultantNoteFieldMenuItem.Checked = true;

#if UsingHtmlDisplayForConNotes
				htmlConsultantNotesControl.OnAddNote(nVerseIndex, strNote);
#else
				Control ctrl = flowLayoutPanelConsultantNotes.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is ConsultNotesControl);
				ConsultNotesControl theConsultantNotes = ctrl as ConsultNotesControl;
				StringTransfer st = theConsultantNotes.DoAddNote(strNote);

				// after the note is added, the control references are no longer valid, but
				//  we want to go back to where we were... so requery the controls
				// Order: BT, then *other* connote pane and then *this* connote pane
				ctrl = flowLayoutPanelVerses.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is VerseBtControl);
				flowLayoutPanelVerses.ScrollControlIntoView(ctrl);

				if (viewCoachNotesFieldMenuItem.Checked)
				{
					ctrl = flowLayoutPanelCoachNotes.GetControlAtVerseIndex(nVerseIndex);
					if (ctrl == null)
						return;

					Debug.Assert(ctrl is ConsultNotesControl);
					flowLayoutPanelCoachNotes.ScrollControlIntoView(ctrl);
				}

				ctrl = flowLayoutPanelConsultantNotes.GetControlAtVerseIndex(nVerseIndex);
				if (ctrl == null)
					return;

				Debug.Assert(ctrl is ConsultNotesControl);
				flowLayoutPanelConsultantNotes.ScrollControlIntoView(ctrl);

				if ((st != null) && (st.TextBox != null))
					st.TextBox.Focus();
#endif
			}
		}

		internal void AddNewVerse(int nInsertionIndex, string strVernacular, string strNationalBT, string strInternationalBT)
		{
			Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses != null));
			theCurrentStory.Verses.InsertVerse(nInsertionIndex, strVernacular, strNationalBT, strInternationalBT);
			InitAllPanes();

			Modified = true;
		}

		internal void InitAllPanes()
		{
			try
			{
				InitAllPanes(theCurrentStory.Verses);
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format(Properties.Resources.IDS_UnableToContinue, ex.Message), OseResources.Properties.Resources.IDS_Caption);
				return;
			}
		}

		internal void DeleteVerse(VerseData theVerseDataToDelete)
		{
			theCurrentStory.Verses.Remove(theVerseDataToDelete);
			InitAllPanes();
			Modified = true;
		}

		internal void VisiblizeVerse(VerseData theVerseDataToVisiblize, bool bVisible)
		{
			theVerseDataToVisiblize.IsVisible = bVisible;
			InitAllPanes();
			Modified = true;
		}

		internal void SetViewBasedOnProjectStage(StoryStageLogic.ProjectStages eStage,
			bool bDisableReInits)
		{
			Debug.Assert(StoryStageLogic.stateTransitions != null);
			StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[eStage];
			Debug.Assert(st != null);

			// see if we're supposed to use the same settings as always...
			//  but not for PFs who are in the early stages
			bool bProjFacInEarlyState = (((int)eStage <= (int)StoryStageLogic.ProjectStages.eProjFacAddStoryQuestions)
				&&  (LoggedOnMember.MemberType == TeamMemberData.UserTypes.eProjectFacilitator));

			if (!useSameSettingsForAllStoriesToolStripMenuItem.Checked
				|| bProjFacInEarlyState)
			{
				// if we are overriding the value because of a PF in the early state, then
				//  clear the flag so it doesn't continue thru the states.
				if (bProjFacInEarlyState)
					useSameSettingsForAllStoriesToolStripMenuItem.Checked = false;

				// in case the caller is just about to call InitAllPanes anyway, we don't
				//  want the screen to thrash, so have the ability to disable the thrashing.
				_bDisableReInitVerseControls = bDisableReInits;

				// if the user is pressing the control key (e.g. while changing state or
				//  selecting another story), then don't change the view settings
				if ((ModifierKeys & Keys.Control) != Keys.Control)
				{
					st.SetView(this);
				}

				_bDisableReInitVerseControls = false;
			}

			helpProvider.SetHelpString(this, st.StageInstructions);
			SetStatusBar(String.Format(Properties.Resources.IDS_PressF1ForInstructions, st.StageDisplayString));
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
				var aVerseData = (VerseData)e.Data.GetData(typeof(VerseData));
				var theDropTarget = sender as Button;
				if (theDropTarget != null)
				{
					var nInsertionIndex = (int)theDropTarget.Tag;    // (flowLayoutPanelVerses.Controls.IndexOf((Button)sender) / 2);
					DoMove(nInsertionIndex, aVerseData);
				}
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
			Modified = true;
		}

		internal void DoPasteVerse(int nInsertionIndex, VerseData theVerseToPaste)
		{
			theCurrentStory.Verses.Insert(nInsertionIndex, theVerseToPaste);
			InitAllPanes();
			Modified = true;
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
				Debug.Assert(ctrl is Button);
				if (Math.Abs(nIndex - i) > 1)
					ctrl.Visible = true;
			}
		}

		internal void DimDropTargetButtons()
		{
			var buttons = from Control ctrl in flowLayoutPanelVerses.Controls
						  where (ctrl is Button)
						  select ctrl;
			foreach (Button button in buttons)
				button.Visible = false;
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
			if (splitContainerUpDown.Panel2Collapsed)
				viewNetBibleMenuItem.Checked = true;

			netBibleViewer.DisplayVerses(strScriptureReference);
		}

		internal string GetNetBibleScriptureReference
		{
			get { return netBibleViewer.ScriptureReference; }
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

		private bool CheckForSaveDirtyFile()
		{
			// if we're in the 'old stories' window OR if it's a Just looking user, then
			//  ignore the modified flag and return
			if (!IsInStoriesSet ||
				((LoggedOnMember != null) && (LoggedOnMember.MemberType == TeamMemberData.UserTypes.eJustLooking)))
			{
				Modified = false;   // just in case
				return true;
			}

			if (Modified)
			{
				// it's annoying that the keyboard doesn't deactivate so I can just type 'y' for "Yes"
				try
				{
					KeyboardController.DeactivateKeyboard();    // ... do it manually
				}
				catch (System.IO.FileLoadException)
				{
#if !DEBUG
					throw;
#endif
				}

				DialogResult res = MessageBox.Show(Properties.Resources.IDS_SaveChanges, OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
				if (res == DialogResult.Cancel)
					return false;
				if (res == DialogResult.No)
				{
					Modified = false;
					return true;
				}

				SaveClicked();
			}

			// do cleanup, because this is always called before starting something new (new file or empty project)
			ClearFlowControls();
			textBoxStoryVerse.Text = Properties.Resources.IDS_Story;
			return true;
		}

		protected void ClearFlowControls()
		{
			flowLayoutPanelVerses.Clear();
			linkLabelVerseBT.Visible = false;
#if UsingHtmlDisplayForConNotes
			if (htmlConsultantNotesControl.Document != null)
				htmlConsultantNotesControl.Document.OpenNew(true);
			if (htmlCoachNotesControl.Document != null)
				htmlCoachNotesControl.Document.OpenNew(true);
#else
			flowLayoutPanelConsultantNotes.Clear();
			flowLayoutPanelCoachNotes.Clear();
#endif
		}

		internal void SaveClicked()
		{
			mySaveTimer.Stop();
			mySaveTimer.Interval = CnIntervalBetweenAutoSaveReqs;
			mySaveTimer.Start();

			if (!IsInStoriesSet || !Modified || (StoryProject == null) || (StoryProject.ProjSettings == null))
				return;

			string strFilename = StoryProject.ProjSettings.ProjectFilePath;

			bool bSaveThisSnapshotInRepo = (DateTime.Now - tmLastSync) > tsBackupTime;
			SaveFile(strFilename, bSaveThisSnapshotInRepo);

			if (bSaveThisSnapshotInRepo)
			{
				try
				{
					Program.BackupInRepo(StoryProject.ProjSettings.ProjectFolder);
				}
				catch (Exception ex)
				{
					MessageBox.Show(String.Format(ex.Message, strFilename), OseResources.Properties.Resources.IDS_Caption);
					return;
				}
				tmLastSync = DateTime.Now;
			}
		}

		protected void SaveXElement(XElement elem, string strFilename, bool bDoReloadTest)
		{
			// create the root portions of the XML document and tack on the fragment we've been building
			XDocument doc = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				elem);

			if (!Directory.Exists(Path.GetDirectoryName(strFilename)))
				Directory.CreateDirectory(Path.GetDirectoryName(strFilename));

			// save it with an extra extn.
			string strTempFilename = strFilename + CstrExtraExtnToAvoidClobberingFilesWithFailedSaves;
			doc.Save(strTempFilename);

			// this reload test is nice, but costly at the end of a project (where time
			//  is of an essense... so just check this when we're storing in the repo
			if (bDoReloadTest)
			{
				// now try to load the xml file. it'll throw if it's malformed
				//  (so we won't want to put it into the repo)
				var projFile = new NewDataSet();
				projFile.ReadXml(strTempFilename);
			}

			// backup the last version to appdata
			// Note: doing File.Move leaves the old file security settings rather than replacing them
			// based on the target directory. Copy, on the other hand, inherits
			// security settings from the target folder, which is what we want to do.
			if (File.Exists(strFilename))
				File.Copy(strFilename, GetBackupFilename(strFilename), true);
			File.Delete(strFilename);
			File.Copy(strTempFilename, strFilename, true);
			File.Delete(strTempFilename);
		}

		protected const string CstrExtraExtnToAvoidClobberingFilesWithFailedSaves = ".bad";

		internal void QueryStoryPurpose()
		{
			StoryFrontMatterForm dlg = new StoryFrontMatterForm(this, StoryProject, theCurrentStory);
			dlg.ShowDialog();
		}

		protected void SaveFile(string strFilename, bool bDoReloadTest)
		{
			try
			{
				// let's see if the UNS entered the purpose and resources used on this story
				if (theCurrentStory != null)
				{
					Debug.Assert(theCurrentStory.CraftingInfo != null);
					if (theCurrentStory.CraftingInfo.IsBiblicalStory
						&& (LoggedOnMember.MemberType == TeamMemberData.UserTypes.eProjectFacilitator)
						&& (((int)theCurrentStory.ProjStage.ProjectStage) > (int)StoryStageLogic.ProjectStages.eProjFacTypeVernacular)
						&& (String.IsNullOrEmpty(theCurrentStory.CraftingInfo.StoryPurpose)
						|| String.IsNullOrEmpty(theCurrentStory.CraftingInfo.ResourcesUsed)))
						QueryStoryPurpose();
				}

				SaveXElement(GetXml, strFilename, bDoReloadTest);
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show(String.Format(Properties.Resources.IDS_FileLockedMessage, strFilename), OseResources.Properties.Resources.IDS_Caption);
				return;
			}
			catch (Exception ex)
			{
				ErrorReport.ReportNonFatalException(new Exception(ex.Message));
				return;
			}

			Program.SetProjectForSyncage(StoryProject.ProjSettings.ProjectFolder);
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

		protected bool _bDisableReInitVerseControls = false;

		private void viewFieldMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			if (!_bDisableReInitVerseControls)
				ReInitVerseControls();
		}

		private void viewNetBibleMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Debug.Assert(sender is ToolStripMenuItem);
			ToolStripMenuItem tsm = (ToolStripMenuItem)sender;
			splitContainerUpDown.Panel2Collapsed = !tsm.Checked;
		}

		private void viewConsultantNoteFieldMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Debug.Assert(sender is ToolStripMenuItem);
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
			if (splitContainerLeftRight.Panel2Collapsed)   // if the whole right-half is already collapsed...
			{
				// ... first enable it.
				splitContainerLeftRight.Panel2Collapsed = false;

				// glitch, whichever half (consultant's or coach's) was collapsed last will still be active even
				//  though it's menu item will be reset. So we need to hide it if we're enabling the other one
				if (!splitContainerMentorNotes.Panel2Collapsed) // this means it's not actually hidden
					splitContainerMentorNotes.Panel2Collapsed = true;
				else
					splitContainerLeftRight_Panel2_SizeChanged(sender, e);
			}

			splitContainerMentorNotes.Panel1Collapsed = false;
		}

		private void viewCoachNotesFieldMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			Debug.Assert(sender is ToolStripMenuItem);
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
			if (splitContainerLeftRight.Panel2Collapsed)   // if the whole right-half is already collapsed...
			{
				// ... first enable it.
				splitContainerLeftRight.Panel2Collapsed = false;

				// glitch, whichever half (consultant's or coach's) was collapsed last will still be active even
				//  though it's menu item will be reset. So we need to hide it if we're enabling the other one
				if (!splitContainerMentorNotes.Panel1Collapsed) // this means it's not actually hidden
					splitContainerMentorNotes.Panel1Collapsed = true;
				else
					splitContainerLeftRight_Panel2_SizeChanged(sender, e);
			}

			splitContainerMentorNotes.Panel2Collapsed = false;
		}

		private void splitContainerLeftRight_Panel2_SizeChanged(object sender, EventArgs e)
		{
#if UsingHtmlDisplayForConNotes
#else
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
#endif
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
			if (IsInStoriesSet)
			{
				recentProjectsToolStripMenuItem.DropDownItems.Clear();
				Debug.Assert(Properties.Settings.Default.RecentProjects.Count == Properties.Settings.Default.RecentProjectPaths.Count);
				for (int i = 0; i < Properties.Settings.Default.RecentProjects.Count; i++)
				{
					string strRecentFile = Properties.Settings.Default.RecentProjects[i];
					ToolStripItem tsi = recentProjectsToolStripMenuItem.DropDownItems.Add(strRecentFile, null, recentProjectsToolStripMenuItem_Click);
					tsi.ToolTipText = String.Format(Properties.Resources.IDS_LocatedInFolder, Properties.Settings.Default.RecentProjectPaths[i]);
				}

				recentProjectsToolStripMenuItem.Enabled = (recentProjectsToolStripMenuItem.DropDownItems.Count > 0);

				toTheInternetToolStripMenuItem.Enabled =
					projectFromASharedNetworkDriveToolStripMenu.Enabled =
						((StoryProject != null) && (StoryProject.ProjSettings != null));

				saveToolStripMenuItem.Enabled = Modified;

				exportToToolboxToolStripMenuItem.Enabled =
					projectSettingsToolStripMenuItem.Enabled = ((StoryProject != null)
						&& (StoryProject.ProjSettings != null)
						&& (LoggedOnMember != null));

				projectLoginToolStripMenuItem.Enabled = (StoryProject != null);

				if ((StoryProject != null)
						&& (StoryProject.ProjSettings != null))
				{
					string strDotHgFolder = StoryProject.ProjSettings.ProjectFolder + @"\.hg";
					sendReceiveToolStripMenuItem.Enabled = Directory.Exists(strDotHgFolder);
				}
				else
					sendReceiveToolStripMenuItem.Enabled = false;
			}
			else
			{
				toTheInternetToolStripMenuItem.Enabled =
					projectFromASharedNetworkDriveToolStripMenu.Enabled =
					recentProjectsToolStripMenuItem.Enabled =
					newToolStripMenuItem.Enabled =
					saveToolStripMenuItem.Enabled =
					browseForProjectToolStripMenuItem.Enabled =
					projectSettingsToolStripMenuItem.Enabled =
					projectLoginToolStripMenuItem.Enabled =
					sendReceiveToolStripMenuItem.Enabled = false;
			}

			printPreviewToolStripMenuItem.Enabled = (StoryProject != null);
		}

		private void recentProjectsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var aRecentFile = (ToolStripDropDownItem)sender;
			string strProjectName = aRecentFile.Text;
			Debug.Assert(Properties.Settings.Default.RecentProjects.Contains(strProjectName));
			int nIndexOfPath = Properties.Settings.Default.RecentProjects.IndexOf(strProjectName);
			string strProjectPath = Properties.Settings.Default.RecentProjectPaths[nIndexOfPath];
			DoReopen(strProjectPath, strProjectName);
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
				Debug.Assert(StoryProject != null);
				return StoryProject.GetXml;
			}
		}

		internal void SetStatusBar(string strText)
		{
			statusLabel.Text = strText;
		}

		/*
		private void buttonsStoryStage_DropDownOpening(object sender, EventArgs e)
		{
			if ((StoryProject == null) || (theCurrentStory == null))
				return;

			buttonPrevState.DropDown.Items.Clear();

			// get the current StateTransition object and find all of the allowable transition states
			StoryStageLogic.StateTransition theCurrentST = StoryStageLogic.stateTransitions[theCurrentStory.ProjStage.ProjectStage];
			Debug.Assert(theCurrentST != null);

			AddListOfButtons(theCurrentST.AllowableBackwardsTransitions);
		}

		protected bool AddListOfButtons(List<StoryStageLogic.AllowableTransition> allowableTransitions)
		{
			if (allowableTransitions.Count == 0)
				return false;

			foreach (StoryStageLogic.AllowableTransition aps in allowableTransitions)
			{
				// put the allowable transitions into the DropDown list
				if ((!aps.RequiresUsingVernacular || StoryProject.ProjSettings.Vernacular.HasData)
					&& (!aps.RequiresUsingNationalBT || StoryProject.ProjSettings.NationalBT.HasData)
					&& (!aps.RequiresUsingEnglishBT || StoryProject.ProjSettings.InternationalBT.HasData)
					&& (!aps.RequiresBiblicalStory || theCurrentStory.CraftingInfo.IsBiblicalStory)
					&& (!aps.RequiresFirstPassMentor || StoryProject.TeamMembers.HasOutsideEnglishBTer)
					&& (!aps.HasUsingOtherEnglishBTer
						|| (aps.RequiresUsingOtherEnglishBTer ==
							StoryProject.TeamMembers.HasOutsideEnglishBTer))
					&& (!aps.RequiresManageWithCoaching || !StoryProject.TeamMembers.HasIndependentConsultant)
					)
				{
					StoryStageLogic.StateTransition aST = StoryStageLogic.stateTransitions[aps.ProjectStage];
					ToolStripItem tsi = buttonPrevState.DropDown.Items.Add(
						aST.StageDisplayString, null, OnSelectOtherState);
					tsi.Tag = aST;
				}
			}
			return true;
		}

		protected void OnSelectOtherState(object sender, EventArgs e)
		{
			Debug.Assert(sender is ToolStripItem);
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
					Debug.Assert(theCurrentST.IsTransitionValid(theNewST.CurrentStage));
					// if this is the last transition before they lose edit privilege, then make
					//  sure they really want to do this.
					if (theCurrentStory.ProjStage.IsTerminalTransition(theNewST.CurrentStage)
						&& (theNewST.MemberTypeWithEditToken != LoggedOnMember.MemberType))
						if (MessageBox.Show(
								String.Format(Properties.Resources.IDS_TerminalTransitionMessage,
								TeamMemberData.GetMemberTypeAsDisplayString(theNewST.MemberTypeWithEditToken),
								theNewST.StageDisplayString),
							 OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
							return;

					theCurrentStory.ProjStage.ProjectStage = theNewST.CurrentStage;
					theCurrentStory.StageTimeStamp = DateTime.Now;
					SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage, true);
					tmLastSync = DateTime.Now - tsBackupTime;   // triggers a repository story when we ask if they want to save
					Modified = true;
					break;
				}

				if (theCurrentST.CurrentStage != theNewST.CurrentStage)
					if (!DoNextState(false))
						break;
			}
			while (theCurrentST.NextState != theNewST.CurrentStage);
			InitAllPanes();
		}
		*/

		private void buttonsStoryStage_Click_1(object sender, EventArgs e)
		{
			if ((StoryProject == null) || (StoryProject.ProjSettings == null) || (theCurrentStory == null))
				return;

			StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[theCurrentStory.ProjStage.ProjectStage];
			SetNextState(st.DefaultNextState(StoryProject, theCurrentStory), true);
		}

		protected bool SetNextState(StoryStageLogic.ProjectStages stateToSet, bool bDoUpdateCtrls)
		{
			if ((StoryProject == null) || (StoryProject.ProjSettings == null) || (theCurrentStory == null))
				return false;

			if (SetNextStateIfReady(stateToSet))
			{
				SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage,
					bDoUpdateCtrls);

				if (bDoUpdateCtrls)
					InitAllPanes();    // just in case there were changes

				Modified = true;
				return true;
			}
			return false;
		}

		protected bool SetNextStateIfReady(StoryStageLogic.ProjectStages stateToSet)
		{
			try
			{
				LoggedOnMember.ThrowIfEditIsntAllowed(theCurrentStory.ProjStage.MemberTypeWithEditToken);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, OseResources.Properties.Resources.IDS_Caption);
				return false;
			}

			if ((theCurrentStory.ProjStage.ProjectStage == stateToSet)
				&& (stateToSet == StoryStageLogic.ProjectStages.eTeamComplete))
			{
				MessageBox.Show(Properties.Resources.IDS_GoBackwardsYoungMan,
								OseResources.Properties.Resources.IDS_Caption);
				return false;
			}

			StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[theCurrentStory.ProjStage.ProjectStage];
			bool bRet = st.IsReadyForTransition(this, StoryProject, theCurrentStory, stateToSet);
			if (bRet)
			{
				StoryStageLogic.ProjectStages eNextState = stateToSet;
				StoryStageLogic.StateTransition stNext = StoryStageLogic.stateTransitions[eNextState];
				bool bReqSave = false;
				if (theCurrentStory.ProjStage.IsTerminalTransition(eNextState))
				{
					if (MessageBox.Show(
							String.Format(Properties.Resources.IDS_TerminalTransitionMessage,
								TeamMemberData.GetMemberTypeAsDisplayString(stNext.MemberTypeWithEditToken),
								stNext.StageDisplayString),
							 OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
						return false;
					bReqSave = true;  // request a save if we've just done a terminal transition
				}
				// a record to our history
				theCurrentStory.TransitionHistory.Add(LoggedOnMember.MemberGuid,
					theCurrentStory.ProjStage.ProjectStage, eNextState);
				theCurrentStory.ProjStage.ProjectStage = eNextState;  // if we are ready, then go ahead and transition
				theCurrentStory.StageTimeStamp = DateTime.Now;
				tmLastSync = DateTime.Now - tsBackupTime;   // triggers a repository story when we ask if they want to save
				Modified = true;

				if (bReqSave)
					if (!CheckForSaveDirtyFile())
						return false;
			}
			return bRet;
		}

		protected void SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages stateToSet)
		{
			// a record to our history
			theCurrentStory.TransitionHistory.Add(LoggedOnMember.MemberGuid,
				theCurrentStory.ProjStage.ProjectStage, stateToSet);
			theCurrentStory.ProjStage.ProjectStage = stateToSet;  // if we are ready, then go ahead and transition
			theCurrentStory.StageTimeStamp = DateTime.Now;
			tmLastSync = DateTime.Now - tsBackupTime;   // triggers a repository story when we ask if they want to save
			Modified = true;
		}

		private void storyToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			enterTheReasonThisStoryIsInTheSetToolStripMenuItem.Enabled = ((theCurrentStory != null) &&
																		  (theCurrentStory.CraftingInfo != null));

			deleteStoryToolStripMenuItem.Enabled =
				storyCopyWithNewNameToolStripMenuItem.Enabled = (theCurrentStory != null);

			insertNewStoryToolStripMenuItem.Enabled = addNewStoryAfterToolStripMenuItem.Enabled =
				(IsInStoriesSet && (StoryProject != null) && (LoggedOnMember != null));

			// if there's a story that has more than no verses, AND if it's a bible
			//  story and before the add anchors stage or a non-biblical story and
			//  before the consultant check stage...
			if ((theCurrentStory != null)
				&& (theCurrentStory.Verses.Count > 0)
				&& (theCurrentStory.CraftingInfo != null)
				&& ((theCurrentStory.CraftingInfo.IsBiblicalStory && !WillBeLossInVerse(theCurrentStory.Verses))
					|| (!theCurrentStory.CraftingInfo.IsBiblicalStory && (theCurrentStory.ProjStage.ProjectStage < StoryStageLogic.ProjectStages.eConsultantCheckNonBiblicalStory))))
			{
				// then we can do splitting and collapsing of the story
				splitIntoLinesToolStripMenuItem.Enabled =
					realignStoryVersesToolStripMenuItem.Enabled = true;

				if (theCurrentStory.Verses.Count == 1)
					splitIntoLinesToolStripMenuItem.Text = "S&plit into Lines";
				else
					splitIntoLinesToolStripMenuItem.Text = "&Collapse into 1 line";
			}
			else
				splitIntoLinesToolStripMenuItem.Enabled =
					realignStoryVersesToolStripMenuItem.Enabled = false;

			if ((StoryProject != null) && (StoryProject.ProjSettings != null))
			{
				/*
				if (StoryProject.ProjSettings.Vernacular.HasData)
					exportStoryToolStripMenuItem.Text = String.Format(OseResources.Properties.Resources.IDS_FromStoryForDiscourseCharting, StoryProject.ProjSettings.Vernacular.LangName);
				else
					exportStoryToolStripMenuItem.Visible = false;

				// do the Hindi to English glossing (but only makes sense if we have both languages...
				if (StoryProject.ProjSettings.NationalBT.HasData && StoryProject.ProjSettings.InternationalBT.HasData)
					exportNationalBacktranslationToolStripMenuItem.Text = String.Format(OseResources.Properties.Resources.IDS_FromNatlLanguage, StoryProject.ProjSettings.NationalBT.LangName);
				else
					exportNationalBacktranslationToolStripMenuItem.Visible = false;

				if (StoryProject.ProjSettings.InternationalBT.HasData)
					exportToAdaptItToolStripMenuItem.Enabled = ((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));
				else
					exportToAdaptItToolStripMenuItem.Visible = false;
				*/
			}
		}

		private void enterTheReasonThisStoryIsInTheSetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			QueryStoryPurpose();
		}

		private void panoramaToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			Debug.Assert(IsInStoriesSet);
			toolStripMenuItemShowPanorama.Enabled = (StoryProject != null);
		}

		private void toolStripMenuItemShowPanorama_Click(object sender, EventArgs e)
		{
			if (StoryProject == null)
				return;

			// keep track of the index of the current story (in case it gets deleted)
			int nIndex = (theCurrentStory != null) ? TheCurrentStoriesSet.IndexOf(theCurrentStory) : -1;

			PanoramaView dlg = new PanoramaView(StoryProject);
			dlg.ShowDialog();

			if (dlg.Modified)
			{
				// this means that the order was probably switched, so we have to reload the combo box
				comboBoxStorySelector.Items.Clear();
				foreach (StoryData aStory in TheCurrentStoriesSet)
					comboBoxStorySelector.Items.Add(aStory.Name);

				// if the current story has been deleted, then choose another
				if (theCurrentStory != null)
				{
					int nNewIndex = TheCurrentStoriesSet.IndexOf(theCurrentStory);
					if (nNewIndex != -1)
					{
						nIndex = -1;

						// also check to see if its name has been changed
						if (theCurrentStory.Name != comboBoxStorySelector.Text)
							comboBoxStorySelector.Text = theCurrentStory.Name;
					}
				}

				if (nIndex > 0)
					nIndex--;

				// if we get here, it's because we deleted the current story
				if (TheCurrentStoriesSet.Count == 0)
				{
					// if they deleted them all, then just close the project
					ClearState();
				}
				else if ((nIndex >= 0) && (nIndex < TheCurrentStoriesSet.Count))
					comboBoxStorySelector.SelectedItem = comboBoxStorySelector.Text =
						TheCurrentStoriesSet[nIndex].Name;

				Modified = true;
			}
		}

		private void deleteStoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert(theCurrentStory != null);

			int nIndex = TheCurrentStoriesSet.IndexOf(theCurrentStory);
			Debug.Assert(nIndex != -1);
			if (nIndex == -1)
				return;

			// make sure the user really wants to do this
			if (MessageBox.Show(String.Format(Properties.Resources.IDS_ConfirmDeleteStory,
				theCurrentStory.Name), OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel)
				!= DialogResult.Yes)
				return;

			TheCurrentStoriesSet.RemoveAt(nIndex);
			Debug.Assert(comboBoxStorySelector.Items.IndexOf(theCurrentStory.Name) == nIndex);
			comboBoxStorySelector.Items.Remove(theCurrentStory.Name);

			if (nIndex > 0)
				nIndex--;
			if (nIndex < TheCurrentStoriesSet.Count)
			{
				comboBoxStorySelector.SelectedItem = comboBoxStorySelector.Text =
					TheCurrentStoriesSet[nIndex].Name;
			}
			else
				ClearState();
			Modified = true;
		}

		private void StoryEditor_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (Modified)
			{
				DialogResult res = MessageBox.Show(Properties.Resources.IDS_SaveChanges, OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
				if (res == DialogResult.Cancel)
				{
					e.Cancel = true;
					return;
				}

				if (res != DialogResult.Yes)
				{
					Modified = false;
					return;
				}

				SaveClicked();
			}

			m_frmFind = null;
		}

		private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			editFindToolStripMenuItem.Enabled =
				copyToolStripMenuItem.Enabled =
				copyNationalBackTranslationToolStripMenuItem.Enabled =
				copyEnglishBackTranslationToolStripMenuItem.Enabled =
				((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			deleteBackTranslationToolStripMenuItem.Enabled =
				deleteStoryNationalBackTranslationToolStripMenuItem.Enabled =
				deleteEnglishBacktranslationToolStripMenuItem.Enabled =
				editAddTestResultsToolStripMenuItem.Enabled =
				(IsInStoriesSet && (theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			pasteToolStripMenuItem.Enabled = (CtrlTextBox._inTextBox != null);

			editCopySelectionToolStripMenuItem.Enabled = ((CtrlTextBox._inTextBox != null) && (!String.IsNullOrEmpty(CtrlTextBox._inTextBox.SelectedText)));

			if ((StoryProject != null) && (StoryProject.ProjSettings != null) && (theCurrentStory != null) && (theCurrentStory.Verses.Count > 0))
			{
				if (StoryProject.ProjSettings.Vernacular.HasData)
					copyStoryToolStripMenuItem.Text = String.Format(Properties.Resources.IDS_StoryText, StoryProject.ProjSettings.Vernacular.LangName);
				else
				{
					copyStoryToolStripMenuItem.Visible =
						deleteStoryVersesToolStripMenuItem.Visible = false;
				}

				if (StoryProject.ProjSettings.NationalBT.HasData)
				{
					copyNationalBackTranslationToolStripMenuItem.Text =
						deleteStoryNationalBackTranslationToolStripMenuItem.Text =
						String.Format(Properties.Resources.IDS_NationalBtOfStory, StoryProject.ProjSettings.NationalBT.LangName);
				}
				else
					deleteStoryNationalBackTranslationToolStripMenuItem.Visible = copyNationalBackTranslationToolStripMenuItem.Visible = false;

				if (!StoryProject.ProjSettings.InternationalBT.HasData)
					deleteEnglishBacktranslationToolStripMenuItem.Visible =
						copyEnglishBackTranslationToolStripMenuItem.Visible = false;

				if (theCurrentStory.CraftingInfo.Testors.Count > 0)
				{
					deleteTestToolStripMenuItem.DropDownItems.Clear();
					for (int nTest = 0; nTest < theCurrentStory.CraftingInfo.Testors.Count; nTest++)
					{
						string strUnsGuid = theCurrentStory.CraftingInfo.Testors[nTest];
						AddDeleteTestSubmenu(deleteTestToolStripMenuItem,
							String.Format("Test {0} done by {1}", nTest + 1, StoryProject.GetMemberNameFromMemberGuid(strUnsGuid)),
							nTest, OnRemoveTest);
					}
				}
				else
					deleteTestToolStripMenuItem.Visible = false;
			}

			UpdateUIMenusWithShortCuts();
		}

		private void editAddTestResultsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(Properties.Resources.IDS_AreAllTestingQuestionsEnteredQuery,
								OseResources.Properties.Resources.IDS_Caption,
								MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				return;

			AddTest();
			InitAllPanes();
		}

		internal bool AddTest()
		{
			// query for the UNSs that will be doing this test
			string strUnsGuid = null;
			while (String.IsNullOrEmpty(strUnsGuid))
			{
				strUnsGuid = QueryForUnsTestor(StoryProject);
				if (String.IsNullOrEmpty(strUnsGuid))
					return false;

				foreach (string strGuid in theCurrentStory.CraftingInfo.Testors)
					if (strGuid == strUnsGuid)
					{
						DialogResult res = MessageBox.Show("You can't use the same UNS for two different tests of the same story. Please select a different UNS.", OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.OKCancel);
						if (res == DialogResult.Cancel)
							return false;
						strUnsGuid = null;
						break;
					}
			}

			theCurrentStory.CraftingInfo.Testors.Add(strUnsGuid);
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				foreach (TestQuestionData aTQ in aVerseData.TestQuestions)
					aTQ.Answers.AddNewLine(strUnsGuid).SetValue("");
				aVerseData.Retellings.AddNewLine(strUnsGuid).SetValue("");
			}

			Modified = true;
			return true;
		}

		protected string QueryForUnsTestor(StoryProjectData theStoryProjectData)
		{
			string strUnsGuid = null;
			while (String.IsNullOrEmpty(strUnsGuid))
			{
				MemberPicker dlg = new MemberPicker(theStoryProjectData, TeamMemberData.UserTypes.eUNS);
				dlg.Text = "Choose the UNS that gave answers for this test";
				DialogResult res = dlg.ShowDialog();
				if (res == DialogResult.OK)
					strUnsGuid = dlg.SelectedMember.MemberGuid;
				else if (res == DialogResult.Cancel)
					break;
			}
			return strUnsGuid;
		}

		private void OnRemoveTest(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
			if (MessageBox.Show("Are you sure you want to remove all of the results from " + tsmi.Text, OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
			{
				int nTestNum = (int)tsmi.Tag;
				Debug.Assert((nTestNum >= 0) && (nTestNum < theCurrentStory.CraftingInfo.Testors.Count));
				string strUnsGuid = theCurrentStory.CraftingInfo.Testors[nTestNum];
				foreach (VerseData aVerseData in theCurrentStory.Verses)
				{
					int nIndex;
					foreach (TestQuestionData aTQ in aVerseData.TestQuestions)
					{
						// it's possible that a question is *newer*, in which case, there may only be answers from a new UNS
						//  and not earlier ones. So delete the records based on the UnsGuid (since that is what the
						//  user will have selected off of to delete)
						nIndex = aTQ.Answers.MemberIDs.IndexOf(strUnsGuid);
						if (nIndex != -1)
						{
							aTQ.Answers.MemberIDs.RemoveAt(nIndex);
							Debug.Assert(nIndex < aTQ.Answers.Count);
							aTQ.Answers.RemoveAt(nIndex);
						}
					}

					// even the verse itself may be newer and only have a single retelling (compared
					//  with multiple retellings for verses that we're present from draft 1)
					nIndex = aVerseData.Retellings.MemberIDs.IndexOf(strUnsGuid);
					if (nIndex != -1)
					{
						aVerseData.Retellings.MemberIDs.RemoveAt(nIndex);
						Debug.Assert(nIndex < aVerseData.Retellings.Count);
						aVerseData.Retellings.RemoveAt(nIndex);
					}
				}

				theCurrentStory.CraftingInfo.Testors.RemoveAt(nTestNum);
				Modified = true;
				InitAllPanes();
			}
		}

		protected void AddDeleteTestSubmenu(ToolStripMenuItem tsm, string strText, int nTestNum, EventHandler theEH)
		{
			ToolStripMenuItem tsmSub = new ToolStripMenuItem
			{
				Name = strText,
				Text = strText,
				Tag = nTestNum,
				ToolTipText = "Delete the answers to the testing questions and the retellings associated with this testing helper (UNS). The text boxes will be deleted completely."
			};
			tsmSub.Click += theEH;
			tsm.DropDown.Items.Add(tsmSub);
		}

		private void copyStoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = GetFullStoryContentsVernacular;
			PutOnClipboard(strStory);
		}

		protected string GetFullStoryContentsVernacular
		{
			get
			{
				Debug.Assert((theCurrentStory != null)
					&& (theCurrentStory.Verses.Count > 0));

				string strText = theCurrentStory.Verses[0].VernacularText.ToString();
				for (int i = 1; i < theCurrentStory.Verses.Count; i++)
				{
					VerseData aVerse = theCurrentStory.Verses[i];
					if (aVerse.VernacularText.HasData)
						strText += ' ' + aVerse.VernacularText.ToString();
				}

				return strText;
			}
		}

		protected string GetFullStoryContentsNationalBTText
		{
			get
			{
				Debug.Assert((theCurrentStory != null)
					&& (theCurrentStory.Verses.Count > 0));

				string strText = theCurrentStory.Verses[0].NationalBTText.ToString();
				for (int i = 1; i < theCurrentStory.Verses.Count; i++)
				{
					VerseData aVerse = theCurrentStory.Verses[i];
					if (aVerse.NationalBTText.HasData)
						strText += ' ' + aVerse.NationalBTText.ToString();
				}

				return strText;
			}
		}

		protected string GetFullStoryContentsInternationalBTText
		{
			get
			{
				Debug.Assert((theCurrentStory != null)
					&& (theCurrentStory.Verses.Count > 0));

				string strText = theCurrentStory.Verses[0].InternationalBTText.ToString();
				for (int i = 1; i < theCurrentStory.Verses.Count; i++)
				{
					VerseData aVerse = theCurrentStory.Verses[i];
					if (aVerse.InternationalBTText.HasData)
						strText += ' ' + aVerse.InternationalBTText.ToString();
				}

				return strText;
			}
		}

		protected void PutOnClipboard(string strText)
		{
			try
			{
				Clipboard.SetDataObject(strText);
			}
			catch (Exception ex)
			{
				// seems to fail sometimes on Windows7. If it actually worked, then just ignore the exception
				IDataObject iData = Clipboard.GetDataObject();
				if (iData != null)
					if (iData.GetDataPresent(DataFormats.UnicodeText))
					{
						string strInput = (string)iData.GetData(DataFormats.UnicodeText);
						if (strInput == strText)
							return;
					}

				string strErrorMsg = String.Format(Properties.Resources.IDS_UnableToCopyText,
					Environment.NewLine, ex.Message,
					((ex.InnerException != null) ? ex.InnerException.Message : ""));
				MessageBox.Show(strErrorMsg, OseResources.Properties.Resources.IDS_Caption);
			}
		}

		private void copyNationalBackTranslationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = GetFullStoryContentsNationalBTText;
			PutOnClipboard(strStory);
		}

		private void copyEnglishBackTranslationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = GetFullStoryContentsInternationalBTText;
			PutOnClipboard(strStory);
		}
		/*
		private void exportStoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = theCurrentStory.Verses[0].VernacularText.ToString();
			for (int i = 1; i < theCurrentStory.Verses.Count; i++)
			{
				VerseData aVerse = theCurrentStory.Verses[i];
				strStory += ' ' + aVerse.VernacularText.ToString();
			}

			GlossInAdaptIt(strStory, AdaptItGlossing.GlossType.eVernacularToEnglish);
		}
		*/

		private void deleteStoryVersesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert((theCurrentStory != null) && (sender is ToolStripItem));

			ToolStripItem tsi = sender as ToolStripItem;
			if (tsi != null)
				if (MessageBox.Show(String.Format(Properties.Resources.IDS_ConfirmDeleteAllVerseLines,
												  tsi.Text.Replace("&", null)), OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
				{
					foreach (VerseData aVerse in theCurrentStory.Verses)
						aVerse.VernacularText.SetValue(null);
					ReInitVerseControls();
					Modified = true;
				}
		}

		private void deleteStoryNationalBackTranslationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert((theCurrentStory != null) && (sender is ToolStripItem));

			ToolStripItem tsi = sender as ToolStripItem;
			if (tsi != null)
				if (MessageBox.Show(String.Format(Properties.Resources.IDS_ConfirmDeleteAllVerseLines,
												  tsi.Text.Replace("&", null)), OseResources.Properties.Resources.IDS_Caption,
									MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
				{
					foreach (VerseData aVerse in theCurrentStory.Verses)
						aVerse.NationalBTText.SetValue(null);
					ReInitVerseControls();
					Modified = true;
				}
		}

		private void deleteEnglishBacktranslationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert((theCurrentStory != null) && (sender is ToolStripItem));

			ToolStripItem tsi = sender as ToolStripItem;
			if (tsi != null)
				if (MessageBox.Show(String.Format(Properties.Resources.IDS_ConfirmDeleteAllVerseLines,
												  tsi.Text.Replace("&", null)), OseResources.Properties.Resources.IDS_Caption,
									MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
				{
					foreach (VerseData aVerse in theCurrentStory.Verses)
						aVerse.InternationalBTText.SetValue(null);
					ReInitVerseControls();
					Modified = true;
				}
		}
		/*
		private void exportNationalBacktranslationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// iterate thru the verses and copy them to the clipboard
			Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			string strStory = theCurrentStory.Verses[0].NationalBTText.ToString();
			for (int i = 1; i < theCurrentStory.Verses.Count; i++)
			{
				VerseData aVerse = theCurrentStory.Verses[i];
				strStory += ' ' + aVerse.NationalBTText.ToString();
			}

			GlossInAdaptIt(strStory, AdaptItGlossing.GlossType.eNationalToEnglish);
		}

		protected void GlossInAdaptIt(string strStoryText, AdaptItGlossing.GlossType eGlossType)
		{
			AdaptItEncConverter theEC = AdaptItGlossing.InitLookupAdapter(StoryProject.ProjSettings, eGlossType);
			string strAdaptationFilespec = AdaptationFilespec(theEC.ConverterIdentifier, theCurrentStory.Name);
			string strProjectName = Path.GetFileNameWithoutExtension(theEC.ConverterIdentifier);
			DialogResult res = DialogResult.Yes;
			if (File.Exists(strAdaptationFilespec))
			{
				res = MessageBox.Show(String.Format(OseResources.Properties.Resources.IDS_AdaptItFileAlreadyExists,
					strProjectName, theCurrentStory.Name), OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);

				if (res == DialogResult.Cancel)
					return;

				if (res == DialogResult.Yes)
				{
					string strBackupName = strAdaptationFilespec + ".bak";
					if (File.Exists(strBackupName))
						File.Delete(strBackupName);
					File.Copy(strAdaptationFilespec, strBackupName);
					File.Delete(strAdaptationFilespec);
				}
			}

			if (res == DialogResult.Yes)
			{
				List<string> TargetWords;
				List<string> SourceWords;
				List<string> StringsInBetween;
				theEC.SplitAndConvert(strStoryText, out SourceWords, out StringsInBetween, out TargetWords);
				Debug.Assert((SourceWords.Count == TargetWords.Count)
					&& (SourceWords.Count == (StringsInBetween.Count - 1)));

				string strSourceSentFinalPunct = (eGlossType == AdaptItGlossing.GlossType.eNationalToEnglish) ?
					StoryProject.ProjSettings.NationalBT.FullStop : StoryProject.ProjSettings.Vernacular.FullStop;
				Debug.Assert(!String.IsNullOrEmpty(strSourceSentFinalPunct));

				XElement elem = new XElement("AdaptItDoc");
				for (int i = 0; i < SourceWords.Count; i++)
				{
					// the SplitAndConvert routine will actually join multi-word segments into a phrase, but for
					//  initializing the adaptation file, we really don't want that, so split them back into
					//  single word bundles.
					string strSourceWord = SourceWords[i];
					string[] astrSourcePhraseWords = strSourceWord.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					if (astrSourcePhraseWords.Length > 1)
					{
						strSourceWord = SourceWords[i] = astrSourcePhraseWords[0];
						for (int j = 1; j < astrSourcePhraseWords.Length; j++)
						{
							string str = astrSourcePhraseWords[j];
							SourceWords.Insert(i + j, str);
							StringsInBetween.Insert(i, "");
						}
					}

					// if the stuff in between is ", ", then clearly it wants to go on the end
					// but if it's like " (", then it wants to go on the next word
					string strBefore = StringsInBetween[i].Trim(), strAfter = StringsInBetween[i + 1];
					if (!String.IsNullOrEmpty(strAfter))
					{
						Debug.Assert(strAfter.Length > 0);  // should at least be a space
						int nIndexOfSpace = strAfter.IndexOf(' ');
						if (nIndexOfSpace != -1)
						{
							string strBeforeNextWord = (nIndexOfSpace < strAfter.Length) ?
								strAfter.Substring(nIndexOfSpace) : null;
							strAfter = (nIndexOfSpace > 0) ? strAfter.Substring(0, nIndexOfSpace).Trim() : null;
							StringsInBetween[i + 1] = strBeforeNextWord;
						}
					}

					string strFattr = AIBools(strSourceWord, strAfter, strSourceSentFinalPunct);

					XElement elemWord =
						new XElement("S",
							new XAttribute("s", strBefore + strSourceWord + strAfter),
							new XAttribute("k", strSourceWord),
							new XAttribute("f", strFattr),
							new XAttribute("sn", i),
							new XAttribute("w", 1),
							new XAttribute("ty", 1));

					if (!String.IsNullOrEmpty(strBefore))
						elemWord.Add(new XAttribute("pp", strBefore.Trim()));

					if (!String.IsNullOrEmpty(strAfter))
						elemWord.Add(new XAttribute("fp", strAfter.Trim()));

					elemWord.Add("");

					elem.Add(elemWord);
				}

				XDocument doc = new XDocument(
					new XDeclaration("1.0", "utf-8", "yes"),
					elem);

				doc.Save(strAdaptationFilespec);

				string strAdaptIt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
					Path.Combine("Adapt It WX Unicode", "Adapt_It_Unicode.exe"));

				// if AdaptIt is currently running, then close it first (so we can start
				//  it in "force review mode"
				foreach (Process aProcess in Process.GetProcesses())
					if (aProcess.ProcessName == "Adapt_It_Unicode")
						ReleaseProcess(aProcess);

				LaunchProgram(strAdaptIt,
					(eGlossType == AdaptItGlossing.GlossType.eNationalToEnglish) ?
					null : "/frm");

				string strMessage = String.Format(OseResources.Properties.Resources.IDS_AdaptationInstructions,
						Environment.NewLine, strProjectName, theCurrentStory.Name);
				MessageBoxButtons mbb = MessageBoxButtons.OK;

				if (eGlossType == AdaptItGlossing.GlossType.eNationalToEnglish)
				{
					strMessage += String.Format(OseResources.Properties.Resources.IDS_AdaptationInstructionsContinue,
						Environment.NewLine);
					mbb = MessageBoxButtons.YesNoCancel;
				}

				res = MessageBox.Show(strMessage, OseResources.Properties.Resources.IDS_Caption, mbb);

				if (res != DialogResult.Yes)
					return;

				// do a dummy conversion to trigger a reload of the KB now that we've surely
				//  added things.
				theEC.Convert("dummy");
			}

			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(strAdaptationFilespec);
				XPathNavigator navigator = doc.CreateNavigator();

				XPathNodeIterator xpIterator = navigator.Select("/AdaptItDoc/S");

				for (int nVerseNum = 0; nVerseNum < theCurrentStory.Verses.Count; nVerseNum++)
				{
					VerseData aVerse = theCurrentStory.Verses[nVerseNum];
					Debug.Assert((eGlossType == AdaptItGlossing.GlossType.eVernacularToEnglish)
						|| (eGlossType == AdaptItGlossing.GlossType.eNationalToEnglish));
					string strStoryVerse = (eGlossType == AdaptItGlossing.GlossType.eVernacularToEnglish)
						? aVerse.VernacularText.ToString() : aVerse.NationalBTText.ToString();
					if (String.IsNullOrEmpty(strStoryVerse))
						continue;

					List<string> TargetWords;
					List<string> SourceWords;
					List<string> StringsInBetween;
					theEC.SplitAndConvert(strStoryVerse, out SourceWords, out StringsInBetween, out TargetWords);
					Debug.Assert((SourceWords.Count == TargetWords.Count)
						&& (SourceWords.Count == (StringsInBetween.Count - 1)));

					string strEnglishBT = null;
					for (int nWordNum = 0; nWordNum < SourceWords.Count; nWordNum++)
					{
						string strSourceWord = SourceWords[nWordNum];
						string strTargetWord = TargetWords[nWordNum];

						if (xpIterator.MoveNext())
						{
							string strSourceKey = xpIterator.Current.GetAttribute("k", navigator.NamespaceURI);
							if (strSourceKey != strSourceWord)
								throw new ApplicationException(String.Format(OseResources.Properties.Resources.IDS_ErrorInAdaptation,
									Environment.NewLine, StoryProject.ProjSettings.NationalBT.LangName, nVerseNum + 1, strSourceKey, strSourceWord));

							string strTargetKey = xpIterator.Current.GetAttribute("a", navigator.NamespaceURI);
							if ((strTargetWord.IndexOf('%') == -1) && (strTargetWord != strTargetKey))
								throw new ApplicationException(String.Format(OseResources.Properties.Resources.IDS_ErrorInAdaptation,
									Environment.NewLine, StoryProject.ProjSettings.NationalBT.LangName, nVerseNum + 1, strTargetKey, strTargetWord));

							strTargetWord = xpIterator.Current.GetAttribute("t", navigator.NamespaceURI);
							strEnglishBT += strTargetWord + ' ';
						}
					}

					if (strEnglishBT != null)
						strEnglishBT = strEnglishBT.Remove(strEnglishBT.Length - 1);
					aVerse.InternationalBTText.SetValue(strEnglishBT);
				}

				Modified = true;
				if (!viewEnglishBTFieldMenuItem.Checked)
					viewEnglishBTFieldMenuItem.Checked = true;
				else
					ReInitVerseControls();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, OseResources.Properties.Resources.IDS_Caption);
			}
		}
		*/

		private static void ReleaseProcess(Process aProcess)
		{
			aProcess.CloseMainWindow();
			aProcess.Close();
			int nloop = 0;
			while (!aProcess.HasExited && (nloop++ < 5))
			{
				Thread.Sleep(2000);
			}
		}

		internal static void LaunchProgram(string strProgram, string strArguments)
		{
			try
			{
				Process myProcess = new Process
										{
											StartInfo =
												{
													FileName = strProgram,
													Arguments = strArguments,
													WindowStyle = ProcessWindowStyle.Minimized
												}
										};
				myProcess.Start();
				Thread.Sleep(2000);
			}
			catch { }    // we tried...
		}

		// from AdaptIt baseline XML.h
		const UInt32 boundaryMask = 32; // position 6
		const UInt32 paragraphMask = 2097152; // position 22

		protected string AIBools(string strSourceWord, string strAfter, string strFullStop)
		{
			UInt32 value = 0;
			if (!String.IsNullOrEmpty(strAfter) && (strFullStop.IndexOf(strAfter) != -1))
				value |= boundaryMask;
			if (strSourceWord.IndexOf(Environment.NewLine) != -1)
				value |= paragraphMask;

			string strValue = String.Format("{1}000000000000000{0}00000",
				(((value & boundaryMask) == boundaryMask) ? "1" : "0"),
				(((value & paragraphMask) == paragraphMask) ? "1" : "0"));

			return strValue;
		}

		protected string AdaptationFilespec(string strConverterFilespec, string strStoryName)
		{
			return Path.Combine(Path.GetDirectoryName(strConverterFilespec),
				Path.Combine("Adaptations", String.Format(@"{0}.xml", strStoryName)));
		}

		protected void UpdateUIMenusWithShortCuts()
		{
			refreshToolStripMenuItem.Enabled = (theCurrentStory != null);

			editFindToolStripMenuItem.Enabled =
				findNextToolStripMenuItem.Enabled =
				replaceToolStripMenuItem.Enabled =
					((StoryProject != null)
					&& (StoryProject.ProjSettings != null)
					&& (theCurrentStory != null)
					&& (theCurrentStory.Verses.Count > 0));

			if ((StoryProject != null) && (StoryProject.ProjSettings != null))
			{
				viewVernacularLangFieldMenuItem.Visible = StoryProject.ProjSettings.Vernacular.HasData;
				viewNationalLangFieldMenuItem.Visible = StoryProject.ProjSettings.NationalBT.HasData;
				viewEnglishBTFieldMenuItem.Visible = StoryProject.ProjSettings.InternationalBT.HasData;
			}
		}

		private void showHideFieldsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ViewEnableForm dlg = new ViewEnableForm(this, StoryProject.ProjSettings, theCurrentStory,
													useSameSettingsForAllStoriesToolStripMenuItem.Checked)
									 {
										 ViewSettings = new VerseData.ViewSettings
											 (
											 viewVernacularLangFieldMenuItem.Checked,
											 viewNationalLangFieldMenuItem.Checked,
											 viewEnglishBTFieldMenuItem.Checked,
											 viewAnchorFieldMenuItem.Checked,
											 viewStoryTestingQuestionMenuItem.Checked,
											 viewStoryTestingQuestionAnswerMenuItem.Checked,
											 viewRetellingFieldMenuItem.Checked,
											 viewConsultantNoteFieldMenuItem.Checked,
											 viewCoachNotesFieldMenuItem.Checked,
											 viewNetBibleMenuItem.Checked,
											 true,
											 null,
											 null
											 )
									 };

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				// have to turn this off, or these new settings won't work
				useSameSettingsForAllStoriesToolStripMenuItem.Checked = false;
				NavigateTo(theCurrentStory.Name, dlg.ViewSettings, true, CtrlTextBox._inTextBox);
				useSameSettingsForAllStoriesToolStripMenuItem.Checked = dlg.UseForAllStories;
			}
		}

		private void viewToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			UpdateUIMenusWithShortCuts();

			if ((StoryProject != null) && (StoryProject.ProjSettings != null))
			{
				showHideFieldsToolStripMenuItem.Enabled =
					historicalDifferencesToolStripMenuItem.Enabled =
					hiddenVersesToolStripMenuItem.Enabled =
					viewOnlyOpenConversationsMenu.Enabled = (theCurrentStory != null);

				if (StoryProject.ProjSettings.Vernacular.HasData)
					viewVernacularLangFieldMenuItem.Text = String.Format(Properties.Resources.IDS_LanguageFields, StoryProject.ProjSettings.Vernacular.LangName);
				else
					viewVernacularLangFieldMenuItem.Checked = viewVernacularLangFieldMenuItem.Visible = false;

				if (StoryProject.ProjSettings.NationalBT.HasData)
				{
					viewNationalLangFieldMenuItem.Text = String.Format(Properties.Resources.IDS_StoryLanguageField,
																	   StoryProject.ProjSettings.NationalBT.LangName);

					viewNationalLangFieldMenuItem.Enabled = ((theCurrentStory != null)
															 && (((int)theCurrentStory.ProjStage.ProjectStage)
																 >= (int)StoryStageLogic.ProjectStages.eProjFacTypeNationalBT));
				}
				else
					viewNationalLangFieldMenuItem.Checked = viewNationalLangFieldMenuItem.Visible = false;

				viewEnglishBTFieldMenuItem.Enabled = ((theCurrentStory != null)
															 && (((int)theCurrentStory.ProjStage.ProjectStage)
																 >= (int)StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT));

				viewAnchorFieldMenuItem.Enabled = ((theCurrentStory != null)
															 && (((int)theCurrentStory.ProjStage.ProjectStage)
																 >= (int)StoryStageLogic.ProjectStages.eProjFacAddAnchors));

				viewStoryTestingQuestionMenuItem.Enabled =
					viewStoryTestingQuestionMenuItem.Enabled = ((theCurrentStory != null)
																 && (((int)theCurrentStory.ProjStage.ProjectStage)
																	 > (int)StoryStageLogic.ProjectStages.eProjFacAddStoryQuestions));

				viewConsultantNoteFieldMenuItem.Enabled =
					viewCoachNotesFieldMenuItem.Enabled =
					stateTransitionHistoryToolStripMenuItem.Enabled = (theCurrentStory != null);

				viewTransliterationsToolStripMenuItem.Enabled = (StoryProject.ProjSettings.Vernacular.HasData || StoryProject.ProjSettings.NationalBT.HasData);

				concordanceToolStripMenuItem.Enabled = true;
			}
			else
				showHideFieldsToolStripMenuItem.Enabled =
					viewTransliterationsToolStripMenuItem.Enabled =
					stateTransitionHistoryToolStripMenuItem.Enabled =
					concordanceToolStripMenuItem.Enabled =
					historicalDifferencesToolStripMenuItem.Enabled =
					hiddenVersesToolStripMenuItem.Enabled =
					viewOnlyOpenConversationsMenu.Enabled = false;

			if (IsInStoriesSet && (StoryProject != null))
			{
				if (StoryProject[OseResources.Properties.Resources.IDS_ObsoleteStoriesSet] != null)
				{
					viewOldStoriesToolStripMenuItem.DropDownItems.Clear();
					foreach (StoryData aStory in StoryProject[OseResources.Properties.Resources.IDS_ObsoleteStoriesSet])
						viewOldStoriesToolStripMenuItem.DropDownItems.Add(aStory.Name, null, onClickViewOldStory).ToolTipText =
							"View older (obsolete) versions of the stories (that were earlier stored in the 'Old Stories' list from the 'Panorama View' window--see 'Panorama' menu, 'Show' command)";
				}
			}
			else
				viewOldStoriesToolStripMenuItem.Enabled = false;
		}

		private void onClickViewOldStory(object sender, EventArgs e)
		{
			ToolStripItem tsi = sender as ToolStripItem;
			StoryEditor theOldStoryEditor = new StoryEditor(OseResources.Properties.Resources.IDS_ObsoleteStoriesSet);
			theOldStoryEditor.StoryProject = StoryProject;
			theOldStoryEditor.LoggedOnMember = LoggedOnMember;
			theOldStoryEditor.Show();
			theOldStoryEditor.LoadComboBox();
			theOldStoryEditor.comboBoxStorySelector.SelectedItem = tsi.Text;
		}

		private void viewNetBibleMenuItem_Click(object sender, EventArgs e)
		{
			InitAllPanes();
		}

		private void editCopySelectionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CtrlTextBox._inTextBox != null)
			{
				string strText = CtrlTextBox._inTextBox.SelectedText;
				if (!String.IsNullOrEmpty(strText))
					Clipboard.SetDataObject(strText);
			}
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CtrlTextBox._inTextBox != null)
			{
				IDataObject iData = Clipboard.GetDataObject();
				if (iData != null)
					if (iData.GetDataPresent(DataFormats.UnicodeText))
					{
						string strText = (string)iData.GetData(DataFormats.UnicodeText);
						CtrlTextBox._inTextBox.SelectedText = strText;
					}
			}
		}

		protected List<string> GetSentencesVernacular(VerseData aVerseData)
		{
			string strSentenceFinalPunct = StoryProject.ProjSettings.Vernacular.FullStop;
			List<string> lstSentences;
			CheckEndOfStateTransition.GetListOfSentences(aVerseData.VernacularText, strSentenceFinalPunct, out lstSentences);
			return lstSentences;
		}

		protected List<string> GetSentencesNationalBT(VerseData aVerseData)
		{
			string strSentenceFinalPunct = StoryProject.ProjSettings.NationalBT.FullStop;
			List<string> lstSentences;
			CheckEndOfStateTransition.GetListOfSentences(aVerseData.NationalBTText, strSentenceFinalPunct, out lstSentences);
			return lstSentences;
		}

		protected List<string> GetSentencesEnglishBT(VerseData aVerseData)
		{
			string strSentenceFinalPunct = StoryProject.ProjSettings.InternationalBT.FullStop;
			List<string> lstSentences;
			CheckEndOfStateTransition.GetListOfSentences(aVerseData.InternationalBTText, strSentenceFinalPunct, out lstSentences);
			return lstSentences;
		}

		protected string GetSentence(int nIndex, List<string> lstSentences)
		{
			if ((lstSentences != null) && (lstSentences.Count > nIndex))
				return lstSentences[nIndex];
			return null;
		}

		private void realignStoryVersesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert((theCurrentStory != null) && (theCurrentStory.Verses.Count > 0));

			if (WillBeLossInVerse(theCurrentStory.Verses))
				return;

			if (!CheckForSaveDirtyFile())    // ought to do a save before this so we don't cause them to lose anything.
				return;

			// first 'collapse into 1 line'
			string strVernacular = GetFullStoryContentsVernacular;
			string strNationalBT = GetFullStoryContentsNationalBTText;
			string strEnglishBT = GetFullStoryContentsInternationalBTText;

			theCurrentStory.Verses.RemoveRange(0, theCurrentStory.Verses.Count);
			theCurrentStory.Verses.InsertVerse(0, strVernacular, strNationalBT,
				strEnglishBT);

			// then split into lines
			VerseData aVerseData = theCurrentStory.Verses[0];
			List<string> lstSentencesVernacular = GetSentencesVernacular(aVerseData);
			List<string> lstSentencesNationalBT = GetSentencesNationalBT(aVerseData);
			List<string> lstSentencesEnglishBT = GetSentencesEnglishBT(aVerseData);
			int nNumVerses = Math.Max(lstSentencesVernacular.Count,
				Math.Max(lstSentencesNationalBT.Count, lstSentencesEnglishBT.Count));

			// remove what's there so we can add the new ones from scratch
			theCurrentStory.Verses.Remove(aVerseData);

			for (int i = 0; i < nNumVerses; i++)
			{
				theCurrentStory.Verses.InsertVerse(i,
					GetSentence(i, lstSentencesVernacular),
					GetSentence(i, lstSentencesNationalBT),
					GetSentence(i, lstSentencesEnglishBT));
			}

			Modified = true;
			InitAllPanes();
		}

		private void splitIntoLinesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert((theCurrentStory != null)
				&& (theCurrentStory.Verses.Count > 0)
				&& !WillBeLossInVerse(theCurrentStory.Verses));

			if (!CheckForSaveDirtyFile())    // ought to do a save before this so we don't cause them to lose anything.
				return;

			if (theCurrentStory.Verses.Count == 1)
			{
				// means 'split into lines'
				VerseData aVerseData = theCurrentStory.Verses[0];
				List<string> lstSentencesVernacular = GetSentencesVernacular(aVerseData);
				List<string> lstSentencesNationalBT = GetSentencesNationalBT(aVerseData);
				List<string> lstSentencesEnglishBT = GetSentencesEnglishBT(aVerseData);
				int nNumVerses = Math.Max(lstSentencesVernacular.Count,
					Math.Max(lstSentencesNationalBT.Count, lstSentencesEnglishBT.Count));

				// remove what's there so we can add the new ones from scratch
				theCurrentStory.Verses.Remove(aVerseData);

				for (int i = 0; i < nNumVerses; i++)
				{
					theCurrentStory.Verses.InsertVerse(i,
						GetSentence(i, lstSentencesVernacular),
						GetSentence(i, lstSentencesNationalBT),
						GetSentence(i, lstSentencesEnglishBT));
				}
			}
			else
			{
				// means 'collapse into 1 line'
				string strVernacular = GetFullStoryContentsVernacular;
				string strNationalBT = GetFullStoryContentsNationalBTText;
				string strEnglishBT = GetFullStoryContentsInternationalBTText;

				theCurrentStory.Verses.RemoveRange(0, theCurrentStory.Verses.Count);
				theCurrentStory.Verses.InsertVerse(0, strVernacular, strNationalBT,
					strEnglishBT);
			}

			Modified = true;
			InitAllPanes();
		}

		private bool WillBeLossInVerse(VersesData theVerses)
		{
			foreach (VerseData aVerse in theVerses)
			{
				if (aVerse.Anchors.HasData
					|| aVerse.Retellings.HasData
					|| aVerse.ConsultantNotes.HasData
					|| aVerse.CoachNotes.HasData
					|| aVerse.TestQuestions.HasData)
				{
					return true;
				}
			}

			return false;
		}

		public void NavigateTo(string strStoryName, int nLineIndex, string strAnchor)
		{
			Debug.Assert(comboBoxStorySelector.Items.Contains(strStoryName));
			comboBoxStorySelector.SelectedItem = strStoryName;
			if (strStoryName != theCurrentStory.Name)
				return; // must have cancelled

			if (!String.IsNullOrEmpty(strAnchor))
				SetNetBibleVerse(strAnchor);
			Debug.Assert(theCurrentStory.Verses.Count >= nLineIndex);
			FocusOnVerse(nLineIndex, true, true);
		}

		public void NavigateTo(string strStoryName,
			VerseData.ViewSettings viewItemToInsureOn, bool bDoOffToo,
			CtrlTextBox ctbToFocus)
		{
			Debug.Assert(comboBoxStorySelector.Items.Contains(strStoryName));
			if (strStoryName != theCurrentStory.Name)
				comboBoxStorySelector.SelectedItem = strStoryName;

			bool bSomethingChanged = false;
			_bDisableReInitVerseControls = true;
			bSomethingChanged |= InsureVisible(viewVernacularLangFieldMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.VernacularLangField),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewTransliterationVernacular,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.VernacularTransliterationField),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewNationalLangFieldMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.NationalBTLangField),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewTransliterationNationalBT,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.NationalBTTransliterationField),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewEnglishBTFieldMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.EnglishBTField),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewAnchorFieldMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.AnchorFields),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewStoryTestingQuestionMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.
													   StoryTestingQuestions),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewStoryTestingQuestionAnswerMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.
													   StoryTestingQuestionAnswers),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewRetellingFieldMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.RetellingFields),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewConsultantNoteFieldMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.ConsultantNoteFields),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewCoachNotesFieldMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.CoachNotesFields),
											   bDoOffToo);
			bSomethingChanged |= InsureVisible(viewNetBibleMenuItem,
											   viewItemToInsureOn.IsViewItemOn(
												   VerseData.ViewSettings.ItemToInsureOn.BibleViewer),
											   bDoOffToo);

			_bDisableReInitVerseControls = false;

			if (bSomethingChanged)
				ReInitVerseControls();

			if (ctbToFocus != null)
				ctbToFocus.Focus();
		}

		protected bool InsureVisible(ToolStripMenuItem tsmi, bool bChecked, bool bDoOffToo)
		{
			Debug.Assert(tsmi != null);
			if (bDoOffToo)
			{
				if ((bChecked && !tsmi.Checked)
					||
					(!bChecked && tsmi.Checked))
				{
					tsmi.Checked = bChecked;
					return true;
				}
			}
			else if (bChecked && !tsmi.Checked)
			{
				tsmi.Checked = true;
				return true;
			}
			return false;
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			HtmlForm dlg = new HtmlForm
							   {
								   Text = "About... OneStory Editor",
								   ClientText = Properties.Resources.IDS_CopyrightInfo
							   };

			dlg.ShowDialog();
		}

		internal SearchForm m_frmFind = null;
		private void editFindToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// if this is called, it's very likely that CtrlTextBox._inTextBox has the first search box
			//  (if the search form is launched from the ConNotes panes, then they handle this themselves
			if (CtrlTextBox._inTextBox != null)
				SearchForm.LastStringTransferSearched = CtrlTextBox._inTextBox.MyStringTransfer;
			LaunchSearchForm();
		}

		internal void LaunchSearchForm()
		{
			if (m_frmFind == null)
				m_frmFind = new SearchForm();

			m_frmFind.Show(this, true);
		}

		internal void findNextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_frmFind == null)
			{
				m_frmFind = new SearchForm();
				m_frmFind.Show(this, true);
			}
			else
				m_frmFind.DoFindNext();
		}

		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			InitAllPanes();
			if (m_frmFind != null)
				m_frmFind.ResetSearchParameters();
		}

		private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// if this is called, it's very likely that CtrlTextBox._inTextBox has the first search box
			//  (if the search form is launched from the ConNotes panes, then they handle this themselves
			if (CtrlTextBox._inTextBox != null)
				SearchForm.LastStringTransferSearched = CtrlTextBox._inTextBox.MyStringTransfer;
			LaunchReplaceForm();
		}

		internal void LaunchReplaceForm()
		{
			if (m_frmFind == null)
				m_frmFind = new SearchForm();

			m_frmFind.Show(this, false);
		}

		private void changeProjectFolderRootToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog
						  {
							  Description =
								  String.Format("Browse to the folder where you want the program to create the '{0}' folder",
												OseResources.Properties.Resources.DefMyDocsSubfolder)
						  };
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				ProjectSettings.OneStoryProjectFolderRoot = dlg.SelectedPath;
				ProjectSettings.InsureOneStoryProjectFolderRootExists();

				string strOldProjectPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				if (Properties.Settings.Default.RecentProjectPaths.Count > 0)
				{
					foreach (string projectPath in Properties.Settings.Default.RecentProjectPaths)
					{
						if (projectPath.IndexOf(strOldProjectPath) == 0)
							strOldProjectPath = projectPath;
					}
					strOldProjectPath = Properties.Settings.Default.RecentProjectPaths[0];
				}
				else
					strOldProjectPath = Path.Combine(strOldProjectPath, "OneStory");

				// clobber any recollection we had of existing projects, since they'll
				//  now need to be "browsed" for.
				Properties.Settings.Default.RecentProjects.Clear();
				Properties.Settings.Default.RecentProjectPaths.Clear();
				Properties.Settings.Default.Save();

				string strMessage = String.Format(Properties.Resources.IDS_MoveProjectsToNewProjectFolder,
												  ProjectSettings.OneStoryProjectFolderRoot, strOldProjectPath);
				MessageBox.Show(strMessage, OseResources.Properties.Resources.IDS_Caption);
			}
		}

		private void toTheInternetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert(StoryProject.ProjSettings != null);
			Program.QueryHgRepoParameters(StoryProject.ProjSettings.ProjectFolder,
										  StoryProject.ProjSettings.ProjectName);
			Program.SyncWithRepository(StoryProject.ProjSettings.ProjectFolder, true);
		}

		private void storyCopyWithNewNameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Debug.Assert(theCurrentStory != null);

			string strStoryName;
			int nIndexOfCurrentStory = -1;
			if (AddNewStoryGetIndex(ref nIndexOfCurrentStory, out strStoryName))
			{
				Debug.Assert(nIndexOfCurrentStory != -1);
				nIndexOfCurrentStory = Math.Min(nIndexOfCurrentStory + 1, TheCurrentStoriesSet.Count);
				StoryData theNewStory = new StoryData(theCurrentStory);
				theNewStory.Name = strStoryName;
				InsertNewStoryAdjustComboBox(theNewStory, nIndexOfCurrentStory);
			}
		}

		private void hiddenVersesToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
		{
			InitAllPanes();
		}

		protected string GetTbxDestPath(string strFilename)
		{
			return String.Format(@"{0}\Toolbox\{1}",
								 StoryProject.ProjSettings.ProjectFolder, strFilename);
		}

		private void exportToToolboxToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			try
			{
				// get the xml (.onestory) file into a memory string so it can be the
				//  input to the transformer
				MemoryStream streamData = new MemoryStream(Encoding.UTF8.GetBytes(GetXml.ToString()));

#if DEBUG
				string strXslt = File.ReadAllText(@"C:\src\StoryEditor\StoryEditor\Resources\oneStory2storyingBT.xsl");
#else
				string strXslt = Properties.Resources.oneStory2storyingBT;
#endif
				// the 'document()' function in this Xslt needs the full path to the
				//  running folder
				string strPathRunningFolder = StoryProjectData.GetRunningFolder;
				strXslt = strXslt.Replace("{0}", strPathRunningFolder);
				string strTbxStoriesBTFilePath = GetTbxDestPath("StoriesBT.txt");

				// make sure the folder exists
				if (!Directory.Exists(Path.GetDirectoryName(strTbxStoriesBTFilePath)))
					Directory.CreateDirectory(Path.GetDirectoryName(strTbxStoriesBTFilePath));

				ExportToToolbox(strXslt, streamData, strTbxStoriesBTFilePath, "Stories");

				strTbxStoriesBTFilePath = GetTbxDestPath("OldStoriesBT.txt");
				ExportToToolbox(strXslt, streamData, strTbxStoriesBTFilePath, "Old Stories");

#if DEBUG
				strXslt = File.ReadAllText(@"C:\src\StoryEditor\StoryEditor\Resources\oneStory2storyingRetelling.xsl");
#else
				strXslt = Properties.Resources.oneStory2storyingRetelling;
#endif
				ExportToToolbox(strXslt, streamData,
					GetTbxDestPath("TestRetellings.txt"), null);

#if DEBUG
				strXslt = File.ReadAllText(@"C:\src\StoryEditor\StoryEditor\Resources\oneStory2ConNotes.xsl");
#else
				strXslt = Properties.Resources.oneStory2ConNotes;
#endif
				ExportToToolbox(strXslt, streamData,
					GetTbxDestPath("ProjectConNotes.txt"), null);

#if DEBUG
				strXslt = File.ReadAllText(@"C:\src\StoryEditor\StoryEditor\Resources\oneStory2CoachNotes.xsl");
#else
				strXslt = Properties.Resources.oneStory2CoachNotes;
#endif
				ExportToToolbox(strXslt, streamData,
					GetTbxDestPath("CoachingNotes.txt"), null);

				// for the key terms file, the xml is from the project key term db
				string strLnCNotesFilePath = GetTbxDestPath("L&CNotes.txt");
				string strKeyTermDb = TermRenderingsList.FileName(StoryProject.ProjSettings.ProjectFolder,
																  StoryProject.ProjSettings.Vernacular.LangCode);

				if (File.Exists(strKeyTermDb))
				{
					string strFileContents = File.ReadAllText(strKeyTermDb);
					streamData = new MemoryStream(Encoding.UTF8.GetBytes(strFileContents));

#if DEBUG
					strXslt = File.ReadAllText(@"C:\src\StoryEditor\StoryEditor\Resources\oneStory2KeyTerms.xsl");
#else
					strXslt = Properties.Resources.oneStory2KeyTerms;
#endif
					// the 'document()' function in this Xslt needs the full path to the
					//  running folder
					strXslt = strXslt.Replace("{0}", strPathRunningFolder);
					ExportToToolbox(strXslt, streamData, strLnCNotesFilePath, null);
				}

				FileInfo fiLnC = new FileInfo(strLnCNotesFilePath);
				if (!fiLnC.Exists || fiLnC.Length == 0)
					File.WriteAllText(strLnCNotesFilePath,
						Properties.Resources.IDS_TbxFile_EmptyLnC);

				CopyDefaultToolboxProjectFiles();
			}
			catch (Exception ex)
			{
				string strErrorMsg = String.Format(Properties.Resources.IDS_CantExport,
					ex.Message,
					((ex.InnerException != null) ? ex.InnerException.Message : ""));
				MessageBox.Show(strErrorMsg, OseResources.Properties.Resources.IDS_Caption);
			}

			Cursor = Cursors.Default;
		}

		protected const string CstrProjectFilename = @"\StoryingProject.prj";
		protected const string CstrStoryBtTypFilename = @"\StoryBT.typ";
		protected const string CstrLCNoteTypFilename = @"\LCNote.typ";
		protected const string CstrStoryNotesTypFilename = @"\StoryNotes.typ";

		private void CopyDefaultToolboxProjectFiles()
		{
			// copy all of the files that are in [TARGETDIR]Toolbox into the same
			//  folder (but don't overwrite) so we can have a full, modifiable project
			string strDestFolder = GetTbxDestPath("");
#if DEBUG
			string strSrcFolder = @"C:\src\StoryEditor\StoryEditor\Toolbox";
#else
			string strSrcFolder = StoryProjectData.GetRunningFolder + @"\Toolbox";
#endif
			string[] astrSrcFiles = Directory.GetFiles(strSrcFolder);
			foreach (string strSrcFile in astrSrcFiles)
			{
				string strFilename = Path.GetFileName(strSrcFile);
				string strDestFile = strDestFolder + strFilename;
				if (!File.Exists(strDestFile))
					File.Copy(strSrcFile, strDestFile);
			}

			// the files in the PathFixup folder need to have folder information put in
			strSrcFolder += @"\PathFixup";

			// create the project file
			CreateTbxFileWithPathFixup(strSrcFolder, strDestFolder, CstrProjectFilename);

			// do the StoryBT.typ file
			string strStoryBTFilename = strSrcFolder + CstrStoryBtTypFilename;
			string strStoryBT = File.ReadAllText(strStoryBTFilename);
			strStoryBT = String.Format(strStoryBT,
				(String.IsNullOrEmpty(StoryProject.ProjSettings.Vernacular.LangCode) ? "vrn" : StoryProject.ProjSettings.Vernacular.LangCode),
				(String.IsNullOrEmpty(StoryProject.ProjSettings.Vernacular.LangName) ? "Vernacular Language" : StoryProject.ProjSettings.Vernacular.LangName),
				(String.IsNullOrEmpty(StoryProject.ProjSettings.NationalBT.LangCode) ? "n" : StoryProject.ProjSettings.NationalBT.LangCode),
				(String.IsNullOrEmpty(StoryProject.ProjSettings.NationalBT.LangName) ? "National Language BT" : StoryProject.ProjSettings.NationalBT.LangName),
				StoryProject.ProjSettings.InternationalBT.LangCode,
				(String.IsNullOrEmpty(StoryProject.ProjSettings.InternationalBT.LangName) ? "English Language BT" : StoryProject.ProjSettings.InternationalBT.LangName),
				(String.IsNullOrEmpty(StoryProject.ProjSettings.Vernacular.DefaultFontName) ? "Arial Unicode MS" : StoryProject.ProjSettings.Vernacular.DefaultFontName),
				(String.IsNullOrEmpty(StoryProject.ProjSettings.InternationalBT.DefaultFontName) ? "Arial Unicode MS" : StoryProject.ProjSettings.InternationalBT.DefaultFontName),
				ProjectSettings.OneStoryProjectFolderRoot,
				StoryProject.ProjSettings.ProjectName);

			strStoryBTFilename = strDestFolder + CstrStoryBtTypFilename;
			if (!File.Exists(strStoryBTFilename))
#if DEBUG
			{
			}
			else
				File.Delete(strStoryBTFilename);
#endif
			File.WriteAllText(strStoryBTFilename, strStoryBT);

			// do the LCNote.typ file
			CreateTbxFileWithPathFixup(strSrcFolder, strDestFolder, CstrLCNoteTypFilename);

			// do the StoryNotes.typ file
			CreateTbxFileWithPathFixup(strSrcFolder, strDestFolder, CstrStoryNotesTypFilename);

			DialogResult res = MessageBox.Show(String.Format(Properties.Resources.IDS_ExportedToolboxMessage,
															 StoryProject.ProjSettings.ProjectFolder),
											   OseResources.Properties.Resources.IDS_Caption,
											   MessageBoxButtons.YesNoCancel);

			if (res == DialogResult.Yes)
				LaunchProgram(strDestFolder + CstrProjectFilename, null);
		}

		protected void CreateTbxFileWithPathFixup(string strSrcFolder,
			string strDestFolder, string strTbxFilename)
		{
			string strTbxFileFilename = strSrcFolder + strTbxFilename;
			string strTbxFileContents = File.ReadAllText(strTbxFileFilename);
			strTbxFileContents = String.Format(strTbxFileContents,
											  ProjectSettings.OneStoryProjectFolderRoot,
											  StoryProject.ProjSettings.ProjectName);

			// now get the destination address
			strTbxFileFilename = strDestFolder + strTbxFilename;
			if (!File.Exists(strTbxFileFilename))
#if DEBUG
			{
			}
			else
				File.Delete(strTbxFileFilename);
#endif
			File.WriteAllText(strTbxFileFilename, strTbxFileContents);
		}

		protected void ExportToToolbox(string strXsltFile, MemoryStream streamData,
			string strTbxFilename, string strParameter)
		{
			// write the formatted XSLT to another memory stream.
			MemoryStream streamXSLT = new MemoryStream(Encoding.UTF8.GetBytes(strXsltFile));
			TransformedXmlDataToSfm(streamXSLT, streamData, strTbxFilename, strParameter);
		}

		protected void TransformedXmlDataToSfm(Stream streamXSLT, Stream streamData,
			string strTbxFilename, string strParameter)
		{
			XslCompiledTransform myProcessor = new XslCompiledTransform();
			XmlReader xslReader = XmlReader.Create(streamXSLT, new XmlReaderSettings() { ProhibitDtd = false });
			XsltSettings xsltSettings = new XsltSettings { EnableDocumentFunction = true, EnableScript = true };
			myProcessor.Load(xslReader, xsltSettings, null);

			// rewind
			streamData.Seek(0, SeekOrigin.Begin);

			XsltArgumentList xslArg = null;
			if (!String.IsNullOrEmpty(strParameter))
			{
				xslArg = new XsltArgumentList();
				xslArg.AddParam("storySet", "", strParameter);
			}

			XmlReader reader = XmlReader.Create(streamData);
			StringBuilder strBuilder = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment };
			XmlWriter writer = XmlWriter.Create(strBuilder, settings);
			myProcessor.Transform(reader, xslArg, writer);

			System.Diagnostics.Debug.Assert(Directory.Exists(Path.GetDirectoryName(strTbxFilename)));

			// overwrite existing version
			if (File.Exists(strTbxFilename))
				File.Delete(strTbxFilename);

			File.WriteAllText(strTbxFilename, strBuilder.ToString());
		}

		private void statusLabel_Click(object sender, EventArgs e)
		{
			if ((theCurrentStory != null) && (theCurrentStory.ProjStage != null))
			{
				// update the status bar (in case we previously put an error there
				StoryStageLogic.StateTransition st =
					StoryStageLogic.stateTransitions[theCurrentStory.ProjStage.ProjectStage];
				SetStatusBar(String.Format(Properties.Resources.IDS_PressF1ForInstructions, st.StageDisplayString));
			}
		}

		private void viewTransliterationVernacular_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
			if (tsmi.Checked)
			{
				if (String.IsNullOrEmpty(LoggedOnMember.TransliteratorVernacular))
				{
					EncConverters aECs = new EncConverters();
					IEncConverter aEC = aECs.AutoSelectWithTitle(ConvType.Unicode_to_from_Unicode,
																 "Choose the transliterator for " +
																 StoryProject.ProjSettings.Vernacular.LangName);
					if (aEC != null)
					{
						LoggedOnMember.TransliteratorVernacular = aEC.Name;
						LoggedOnMember.TransliteratorDirectionForwardVernacular = aEC.DirectionForward;
						Modified = true;
					}
				}
			}

			ReInitVerseControls();
		}

		private void viewTransliterationNationalBT_Click(object sender, EventArgs e)
		{
			ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
			if (tsmi.Checked)
			{
				if (String.IsNullOrEmpty(LoggedOnMember.TransliteratorNationalBT))
				{
					EncConverters aECs = new EncConverters();
					IEncConverter aEC = aECs.AutoSelectWithTitle(ConvType.Unicode_to_from_Unicode,
																 "Choose the transliterator for " +
																 StoryProject.ProjSettings.NationalBT.LangName);
					if (aEC != null)
					{
						LoggedOnMember.TransliteratorNationalBT = aEC.Name;
						LoggedOnMember.TransliteratorDirectionForwardNationalBT = aEC.DirectionForward;
						Modified = true;
					}
				}
			}

			ReInitVerseControls();
		}

		private void viewTransliterationsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			if ((LoggedOnMember != null)
				&& (StoryProject != null)
				&& (StoryProject.ProjSettings != null))
			{
				viewTransliterationVernacular.Text = StoryProject.ProjSettings.Vernacular.LangName;
				viewTransliterationVernacular.Visible = (StoryProject.ProjSettings.Vernacular.HasData);
				viewTransliterationNationalBT.Text = StoryProject.ProjSettings.NationalBT.LangName;
				viewTransliterationNationalBT.Visible = (StoryProject.ProjSettings.NationalBT.HasData);
			}
			else
			{
				viewTransliterationVernacular.Enabled =
					viewTransliterationNationalBT.Enabled = false;
			}
		}

		private void viewTransliteratorVernacularConfigureToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LoggedOnMember.TransliteratorVernacular = null;
			viewTransliterationVernacular.Checked = true;
			viewTransliterationVernacular_Click(viewTransliterationVernacular, null);
		}

		private void viewTransliteratorNationalBTConfigureToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LoggedOnMember.TransliteratorNationalBT = null;
			viewTransliterationNationalBT.Checked = true;
			viewTransliterationNationalBT_Click(viewTransliterationNationalBT, null);
		}

		private void linkLabelConsultantNotes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var ll = sender as LinkLabel;
			if ((ll != null) && (e.Button == MouseButtons.Left))
				htmlConsultantNotesControl.OnVerseLineJump((int)ll.Tag);
		}

		private void linkLabelCoachNotes_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var ll = sender as LinkLabel;
			if ((ll != null) && (e.Button == MouseButtons.Left))
				htmlCoachNotesControl.OnVerseLineJump((int)ll.Tag);
		}

		private void linkLabelVerseBT_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var ll = sender as LinkLabel;
			if ((ll != null) && (e.Button == MouseButtons.Left))
			{
				int nVerseIndex = (int) ll.Tag;
				FocusOnVerse(nVerseIndex, true, true);
			}
		}

		private const string CstrFirstVerse = "Story (Ln: 0)";

		private void contextMenuStripVerseList_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			contextMenuStripVerseList.Items.Clear();
			if (theCurrentStory != null)
			{
				if (contextMenuStripVerseList.SourceControl != linkLabelVerseBT)
					contextMenuStripVerseList.Items.Add(CstrFirstVerse, null,
						onClickVerseNumber);

				for (int i = 0; i < theCurrentStory.Verses.Count; i++)
				{
					VerseData aVerse = theCurrentStory.Verses[i];
					string strMenuText = "Ln: " + (i + 1);
					if (!aVerse.IsVisible)
						strMenuText += OseResources.Properties.Resources.IDS_HiddenLabel;
					contextMenuStripVerseList.Items.Add(strMenuText, null,
						onClickVerseNumber);
				}
			}
		}

		private void onClickVerseNumber(object sender, EventArgs e)
		{
			string strMenuText = (sender as ToolStripMenuItem).Text;
			int nVerseNumber;
			if (strMenuText == CstrFirstVerse)
				nVerseNumber = 0;
			else
			{
				if (strMenuText.IndexOf(OseResources.Properties.Resources.IDS_HiddenLabel) > 0)
				{
					strMenuText = strMenuText.Substring(0, strMenuText.Length - OseResources.Properties.Resources.IDS_HiddenLabel.Length);
					hiddenVersesToolStripMenuItem.Checked = true;
				}
				int nIndex = strMenuText.IndexOf(' ');
				Debug.Assert(nIndex != -1);
				nVerseNumber = Convert.ToInt32(strMenuText.Substring(nIndex + 1));
			}
			FocusOnVerse(nVerseNumber, true, true);
		}

		private void historicalDifferencesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			HtmlDisplayForm dlg = new HtmlDisplayForm(this, theCurrentStory);
			dlg.Show();
		}

		private void useSameSettingsForAllStoriesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.LastUseForAllStories = useSameSettingsForAllStoriesToolStripMenuItem.Checked;
			Properties.Settings.Default.Save();
		}

		private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PrintForm dlg = new PrintForm(this);
			dlg.Show();
		}

		private void resetStoredInformationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.Reset();
			Program.InitializeLocalSettingsCollections(false);
			Properties.Settings.Default.Save();
		}

		internal void concordanceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string strVernacular = null, strNationalBT = null, strInternationalBT = null;
			if ((CtrlTextBox._inTextBox != null) && (CtrlTextBox._nLastVerse > 0))
			{
				Control ctrl = flowLayoutPanelVerses.GetControlAtVerseIndex(CtrlTextBox._nLastVerse);
				if (ctrl != null)
				{
					Debug.Assert(ctrl is VerseBtControl);
					VerseBtControl theVerse = ctrl as VerseBtControl;
					if (theVerse != null)
					{
						if (viewVernacularLangFieldMenuItem.Checked)
						{
							Debug.Assert(theVerse._verseData.VernacularText.TextBox != null);
							strVernacular = theVerse._verseData.VernacularText.TextBox.SelectedText;
						}
						if (viewNationalLangFieldMenuItem.Checked)
						{
							Debug.Assert(theVerse._verseData.NationalBTText.TextBox != null);
							strNationalBT = theVerse._verseData.NationalBTText.TextBox.SelectedText;
						}
						if (viewEnglishBTFieldMenuItem.Checked)
						{
							Debug.Assert(theVerse._verseData.InternationalBTText.TextBox != null);
							strInternationalBT = theVerse._verseData.InternationalBTText.TextBox.SelectedText;
						}
					}
				}
			}

			ConcordanceForm dlg = new ConcordanceForm(this, strVernacular, strNationalBT, strInternationalBT);
			dlg.Show();
		}

		private void toolStripMenuItemSelectState_Click(object sender, EventArgs e)
		{
			if ((StoryProject == null) || (theCurrentStory == null))
				return;

			// locate the window near the cursor...
			Point ptTooltip = Cursor.Position;
			StageEditorForm dlg = new StageEditorForm(StoryProject, theCurrentStory, ptTooltip, false);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				Debug.Assert(dlg.NextState != StoryStageLogic.ProjectStages.eUndefined);
				if (theCurrentStory.ProjStage.ProjectStage != dlg.NextState)
				{
					StoryStageLogic.StateTransition st = StoryStageLogic.stateTransitions[dlg.NextState];
					SetNextState(dlg.NextState, true);
				}
			}

			if (dlg.ViewStateChanged)
				SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage, false);
		}

		private void toolStripButtonFirst_Click(object sender, EventArgs e)
		{
			if ((TheCurrentStoriesSet != null) && (theCurrentStory != null))
				comboBoxStorySelector.SelectedIndex = 0;
		}

		private void toolStripButtonPrevious_Click(object sender, EventArgs e)
		{
			if ((TheCurrentStoriesSet == null) || (theCurrentStory == null))
				return;

			int nIndex = TheCurrentStoriesSet.IndexOf(theCurrentStory);
			if (nIndex > 0)
				comboBoxStorySelector.SelectedIndex = --nIndex;
		}

		private void toolStripButtonNext_Click(object sender, EventArgs e)
		{
			if ((TheCurrentStoriesSet == null) || (theCurrentStory == null))
				return;

			int nIndex = TheCurrentStoriesSet.IndexOf(theCurrentStory);
			if (nIndex < (TheCurrentStoriesSet.Count - 1))
				comboBoxStorySelector.SelectedIndex = ++nIndex;
		}

		private void toolStripButtonLast_Click(object sender, EventArgs e)
		{
			if ((TheCurrentStoriesSet != null) && (theCurrentStory != null))
				comboBoxStorySelector.SelectedIndex = (TheCurrentStoriesSet.Count - 1);
		}

		private void viewOnlyOpenConversationsMenu_CheckStateChanged(object sender, EventArgs e)
		{
			if (viewOnlyOpenConversationsMenu.Checked)
				theCurrentStory.Verses.ResetShowOpenConversationsFlags();

			InitAllPanes();
		}

		private void stateTransitionHistoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!theCurrentStory.TransitionHistory.HasData)
			{
				MessageBox.Show(Properties.Resources.IDS_NoTransitionHistory,
								OseResources.Properties.Resources.IDS_Caption);
				return;
			}

			var dlg = new TransitionHistoryForm(theCurrentStory.TransitionHistory, StoryProject.TeamMembers);
			dlg.ShowDialog();
		}

		private void changeStateWithoutChecksToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// locate the window near the cursor...
			Point ptTooltip = Cursor.Position;

			if (MessageBox.Show(Properties.Resources.IDS_ConfirmStateChangeOverride,
				OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
				return;

			StageEditorForm dlg = new StageEditorForm(StoryProject, theCurrentStory, ptTooltip, true);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				Debug.Assert(dlg.NextState != StoryStageLogic.ProjectStages.eUndefined);
				if (theCurrentStory.ProjStage.ProjectStage == dlg.NextState)
					return;

				SetNextStateAdvancedOverride(dlg.NextState);
				SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage, true);
				Modified = true;
			}

			// even if we don't change the state, we might
			else if (dlg.ViewStateChanged)
				SetViewBasedOnProjectStage(theCurrentStory.ProjStage.ProjectStage, false);
			InitAllPanes(); // just in case there were changes
		}

		private void advancedToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			changeStateWithoutChecksToolStripMenuItem.Enabled = ((StoryProject != null) && (theCurrentStory != null));
		}

		private void checkForProgramUpdatesNowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				// save changes before checking (so we can close rapidly if need be)
				if (!CheckForSaveDirtyFile())
					return;

				Program.CheckForProgramUpdate(true);

				// since the call to SaveDirty will have removed them all
				InitAllPanes();

				// if it returns here without throwing an exception, it means there were no updates
				MessageBox.Show(Properties.Resources.IDS_NoProgramUpdates,
					OseResources.Properties.Resources.IDS_Caption);
			}
			catch (Program.RestartException)
			{
				Close();
			}
			catch (Exception ex)
			{
				string strMessage = String.Format("Error occurred:{0}{0}{1}", Environment.NewLine, ex.Message);
				if (ex.InnerException != null)
					strMessage += String.Format("{0}{1}", Environment.NewLine, ex.InnerException.Message);
				MessageBox.Show(strMessage, OseResources.Properties.Resources.IDS_Caption);
			}
		}

		private void programUpdatesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			automaticallyCheckAtStartupToolStripMenuItem.Checked =
				Properties.Settings.Default.AutoCheckForProgramUpdatesAtStartup;
		}

		private void automaticallyCheckAtStartupToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.AutoCheckForProgramUpdatesAtStartup =
				automaticallyCheckAtStartupToolStripMenuItem.Checked;
			Properties.Settings.Default.Save();
		}

		private void enabledToolStripMenuItem_CheckStateChanged(object sender, EventArgs e)
		{
			if (enabledToolStripMenuItem.Checked)
			{
				//autosave timer goes off every 5 minutes.
				mySaveTimer.Tick += TimeToSave;
				mySaveTimer.Interval = CnIntervalBetweenAutoSaveReqs;
				mySaveTimer.Start();
			}
			else
			{
				mySaveTimer.Stop();
			}

			Properties.Settings.Default.AutoSaveTimeoutEnabled = enabledToolStripMenuItem.Checked;
			Properties.Settings.Default.Save();
		}

		private void sendReceiveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// just pretend this is the same as a reload. (it'll take care of the rest)
			string strProjectName = StoryProject.ProjSettings.ProjectName;
			string strProjectPath = StoryProject.ProjSettings.ProjectFolder;
			DoReopen(strProjectPath, strProjectName);
		}

		private void DoReopen(string strProjectPath, string strProjectName)
		{
			try
			{
				OpenProject(strProjectPath, strProjectName);
			}
			catch (ProjectSettings.ProjectFileNotFoundException ex)
			{
				// the file doesn't exist anymore, so remove it from the recent used list
				int nIndex = Properties.Settings.Default.RecentProjects.IndexOf(strProjectName);
				if (nIndex != -1)
				{
					Properties.Settings.Default.RecentProjects.RemoveAt(nIndex);
					Properties.Settings.Default.RecentProjectPaths.RemoveAt(nIndex);
					Properties.Settings.Default.Save();
					MessageBox.Show(ex.Message, OseResources.Properties.Resources.IDS_Caption);
				}
			}
			catch (Program.RestartException)
			{
				throw;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, OseResources.Properties.Resources.IDS_Caption);
			}
		}
	}
}