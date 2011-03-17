using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using ECInterfaces;
using SilEncConverters40;

namespace OneStoryProjectEditor
{
	public partial class HtmlDisplayForm : TopForm
	{
		HgRepository _repository;
		List<Revision> _lstRevisions;
		string _strStoryToDiff, _strLastState, _strProjectFolder;
		public DirectableEncConverter TransliteratorVernacular;
		public DirectableEncConverter TransliteratorNationalBT;

		public HtmlDisplayForm(StoryEditor theSE, StoryData storyData)
			: base(true)
		{
			InitializeComponent();

			Text = String.Format("Revision History: {0}", theSE.theCurrentStory.Name);
			_strStoryToDiff = storyData.Name;
			_strProjectFolder = theSE.StoryProject.ProjSettings.ProjectFolder;

			if (theSE.StoryProject.ProjSettings.Vernacular.HasData)
			{
				checkBoxLangVernacular.Text = String.Format(Properties.Resources.IDS_LanguageFields,
															theSE.StoryProject.ProjSettings.Vernacular.LangName);
				if (theSE.viewTransliterationVernacular.Checked
				   && !String.IsNullOrEmpty(theSE.LoggedOnMember.TransliteratorVernacular))
				{
					checkBoxLangTransliterateVernacular.Visible =
						checkBoxLangTransliterateVernacular.Checked = true;
					TransliteratorVernacular = new DirectableEncConverter(theSE.LoggedOnMember.TransliteratorVernacular,
																		  theSE.LoggedOnMember.
																			  TransliteratorDirectionForwardVernacular,
																		  NormalizeFlags.None);
				}
			}
			else
				checkBoxLangVernacular.Checked = checkBoxLangVernacular.Visible = false;

			if (theSE.StoryProject.ProjSettings.NationalBT.HasData)
			{
				checkBoxLangNationalBT.Text = String.Format(Properties.Resources.IDS_StoryLanguageField,
															theSE.StoryProject.ProjSettings.NationalBT.LangName);
				if (theSE.viewTransliterationNationalBT.Checked
				   && !String.IsNullOrEmpty(theSE.LoggedOnMember.TransliteratorNationalBT))
				{
					checkBoxLangTransliterateNationalBT.Visible =
						checkBoxLangTransliterateNationalBT.Checked = true;
					TransliteratorNationalBT = new DirectableEncConverter(theSE.LoggedOnMember.TransliteratorNationalBT,
																		  theSE.LoggedOnMember.
																			  TransliteratorDirectionForwardNationalBT,
																		  NormalizeFlags.None);
				}
			}
			else
				checkBoxLangNationalBT.Checked = checkBoxLangNationalBT.Visible = false;

			checkBoxLangInternationalBT.Checked =
				checkBoxLangInternationalBT.Visible =
				theSE.StoryProject.ProjSettings.InternationalBT.HasData;

			checkBoxLangFreeTranslation.Checked =
				checkBoxLangFreeTranslation.Visible =
				theSE.StoryProject.ProjSettings.FreeTranslation.HasData;

			checkBoxShowHidden.Checked = theSE.hiddenVersesToolStripMenuItem.Checked;

			try
			{
				NullProgress np = new NullProgress();
				_repository = HgRepository.CreateOrLocate(theSE.StoryProject.ProjSettings.ProjectFolder, np);
				_lstRevisions = _repository.GetAllRevisions();  // _revisionInRepositoryModel.GetHistoryItems();
				progressBar.Value = 0;
				progressBar.Visible = true;
				backgroundWorkerCheckRevisions.RunWorkerAsync(this);
			}
			catch (Exception ex)
			{
				string strMessage = String.Format("Error occurred:{0}{0}{1}", Environment.NewLine, ex.Message);
				if (ex.InnerException != null)
					strMessage += String.Format("{0}{1}", Environment.NewLine, ex.InnerException.Message);
				MessageBox.Show(strMessage, OseResources.Properties.Resources.IDS_Caption);
			}

			htmlStoryBtControl.TheSE = theSE;
		}

		private void tabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			System.Diagnostics.Debug.Assert((htmlStoryBtControl.TheSE != null)
				&& (htmlStoryBtControl.TheSE.StoryProject != null)
				&& (htmlStoryBtControl.TheSE.StoryProject.ProjSettings != null));

			if (e.TabPage == tabPageDisplayChangeReport)
			{
				htmlStoryBtControl.ViewSettings = new VerseData.ViewSettings(
					htmlStoryBtControl.TheSE.StoryProject.ProjSettings,
					checkBoxLangVernacular.Checked,
					checkBoxLangNationalBT.Checked,
					checkBoxLangInternationalBT.Checked,
					checkBoxLangFreeTranslation.Checked,
					checkBoxAnchors.Checked,
					checkBoxStoryTestingQuestions.Checked,
					checkBoxAnswers.Checked,
					checkBoxRetellings.Checked,
					false,  // theSE.viewConsultantNoteFieldMenuItem.Checked,
					false,  // theSE.viewCoachNotesFieldMenuItem.Checked,
					false,  // theSE.viewNetBibleMenuItem.Checked
					checkBoxFrontMatter.Checked,
					checkBoxShowHidden.Checked,
					false,  // only open conversations (doesn't apply here)
					checkBoxGeneralTestingQuestions.Checked,
					(checkBoxLangTransliterateVernacular.Checked)
						? TransliteratorVernacular
						: null,
					(checkBoxLangTransliterateNationalBT.Checked)
						? TransliteratorNationalBT
						: null);

				htmlStoryBtControl.ParentStory = GetStoryForPresentation(_nParentIndex);
				htmlStoryBtControl.StoryData = GetStoryForPresentation(_nChildIndex);

				if ((htmlStoryBtControl.ParentStory == null)
					|| (htmlStoryBtControl.StoryData == null))
				{
					e.Cancel = true;
					return;
				}
#if DEBUG
				// throw the results into a file that I can use WinCmp to compare
				File.WriteAllText(@"C:\src\StoryEditor\XMLFile1.xml", htmlStoryBtControl.ParentStory.GetXml.ToString(), Encoding.UTF8);
				File.WriteAllText(@"C:\src\StoryEditor\XMLFile2.xml", htmlStoryBtControl.StoryData.GetXml.ToString(), Encoding.UTF8);
#endif

				htmlStoryBtControl.LoadDocument();
			}
		}

		protected StoryData GetStoryForPresentation(int nIndex)
		{
			if ((nIndex >= 0) && (nIndex < dataGridViewRevisions.Rows.Count))
			{
				RevisionInfo ri = dataGridViewRevisions.Rows[nIndex].Tag as RevisionInfo;
				if (ri != null)
				{
					if (ri.StoryProjectNode != null)
						return new StoryData(ri.StoryProjectNode, _strProjectFolder);

					try
					{
						XmlNode nodeStoryProject;
						string strThisState;
						ReadRevisionFile(_repository, ri.Revision, out nodeStoryProject, out strThisState);
						return new StoryData(nodeStoryProject, _strProjectFolder);
					}
					catch
					{
						// throw means that story isn't in this revision
						MessageBox.Show(String.Format(Properties.Resources.IDS_ThisStoryNotInThisRevision,
							ri.Revision.Number.LocalRevisionNumber),
										OseResources.Properties.Resources.IDS_Caption);
					}
				}
			}
			return null;
		}

		private class RevisionInfo
		{
			public Revision Revision { get; set; }
			public object[] RowInfo { get; set; }
			public XmlNode StoryProjectNode { get; set; }
		}

		private void backgroundWorkerCheckRevisions_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker bg = sender as BackgroundWorker;
			HtmlDisplayForm form = e.Argument as HtmlDisplayForm;
			try
			{
				int nCount = form._lstRevisions.Count;
				for (int i = 0; i < nCount; i++)
				{
					Revision rev = form._lstRevisions[i];
					int nPercentDone = i * 100 / nCount;
					RevisionInfo ri = null;
					XmlNode nodeStoryProject;
					if (ShowThisRevision(form._repository, rev, out nodeStoryProject))
					{
						var dateString = rev.DateString;
						DateTime when;
						if (DateTime.TryParseExact(dateString, "ddd MMM dd HH':'mm':'ss yyyy zzz",
												   null, DateTimeStyles.AssumeUniversal, out when))
						{
							when = when.ToLocalTime();
							dateString = when.ToShortDateString() + " " + when.ToShortTimeString();
						}

						// get a new one (since we're going to keep track of this one)
						object[] ao = new object[] { false, false, rev.Number.LocalRevisionNumber, dateString, rev.UserId, _strLastState };
						ri = new RevisionInfo { RowInfo = ao, Revision = rev, StoryProjectNode = nodeStoryProject };
					}

					bg.ReportProgress(nPercentDone, ri);

					if (bg.CancellationPending)
					{
						e.Cancel = true;
						break;
					}
				}
			}
			catch
			{
				// we throw an exception when we're finished checking
			}
		}

		private bool ShowThisRevision(HgRepository repository, Revision rev, out XmlNode nodeStoryProject)
		{
			nodeStoryProject = null;
			if (radioButtonShowAllRevisions.Checked)
				return true;

			string strThisState;
			ReadRevisionFile(repository, rev, out nodeStoryProject, out strThisState);

			if ((strThisState == _strLastState) && !radioButtonShowAllWithState.Checked)
				return false;
			_strLastState = strThisState;
			return true;
		}

		private void ReadRevisionFile(HgRepository repository, Revision rev,
			out XmlNode nodeStoryProject, out string strThisState)
		{
			// determine the file we want to look in
			string strFileName = Path.GetFileName(repository.PathToRepo) + ".onestory";

			// we only want to show revisions that have a state change
			string strFilePath = repository.RetrieveHistoricalVersionOfFile(strFileName, rev.Number.LocalRevisionNumber);
			string strFileContents = File.ReadAllText(strFilePath);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(strFileContents);
			string strXPathToStoryWithName = String.Format("/StoryProject/stories[@SetName = 'Stories']/story[@name = '{0}']",
				_strStoryToDiff);
			nodeStoryProject = doc.SelectSingleNode(strXPathToStoryWithName);
			if (nodeStoryProject == null)
				throw new ApplicationException("done working");

			strThisState = nodeStoryProject.Attributes["stage"].Value;
			System.Diagnostics.Debug.WriteLine(String.Format("In revision: {0}, story: {1}, State: {2}",
				rev.Number.LocalRevisionNumber,
				_strStoryToDiff,
				strThisState));
		}

		private void backgroundWorkerCheckRevisions_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			RevisionInfo ri = e.UserState as RevisionInfo;
			if ((ri != null) && (ri.RowInfo != null))
			{
				int nIndex = dataGridViewRevisions.Rows.Add(ri.RowInfo);
				DataGridViewRow theRow = dataGridViewRevisions.Rows[nIndex];
				theRow.Tag = ri;
				theRow.Cells[ColumnOldParent.Index].ToolTipText = ri.Revision.Number.LocalRevisionNumber + ": " + ri.Revision.Number.Hash;
				if (nIndex == 0)
				{
					_nChildIndex = nIndex;
					theRow.Cells[ColumnNewChild.Index].Value = true;
				}
				else if (nIndex == 1)
				{
					_nParentIndex = nIndex;
					theRow.Cells[ColumnOldParent.Index].Value = true;
				}
			}
			progressBar.Value = e.ProgressPercentage;
		}

		private void backgroundWorkerCheckRevisions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			progressBar.Visible = false;
		}

		private int _nParentIndex, _nChildIndex;
		private void dataGridViewRevisions_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			// make sure we have something reasonable
			if (((e.ColumnIndex < ColumnOldParent.Index) || (e.ColumnIndex > ColumnNewChild.Index))
				|| (e.RowIndex < 0) || (e.RowIndex >= dataGridViewRevisions.Rows.Count))
				return;

			if (e.ColumnIndex == ColumnOldParent.Index)
			{
				dataGridViewRevisions.Rows[_nParentIndex].Cells[ColumnOldParent.Index].Value = false;
				_nParentIndex = e.RowIndex;
			}
			else if (e.ColumnIndex == ColumnNewChild.Index)
			{
				dataGridViewRevisions.Rows[_nChildIndex].Cells[ColumnNewChild.Index].Value = false;
				_nChildIndex = e.RowIndex;
			}
		}

		private void radioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (backgroundWorkerCheckRevisions.IsBusy)
				backgroundWorkerCheckRevisions.CancelAsync();

			dataGridViewRevisions.Rows.Clear();

			progressBar.Value = 0;
			progressBar.Visible = true;
			while (backgroundWorkerCheckRevisions.IsBusy)
				Application.DoEvents();

			// only show the state column if we've actually looked into the file
			ColumnState.Visible = (radioButtonRevsByChangeOfState.Checked
				|| radioButtonShowAllWithState.Checked);
			backgroundWorkerCheckRevisions.RunWorkerAsync(this);
		}

		private void HtmlDisplayForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (backgroundWorkerCheckRevisions.IsBusy)
				backgroundWorkerCheckRevisions.CancelAsync();

			while (backgroundWorkerCheckRevisions.IsBusy)
				Application.DoEvents();
		}

		/*
		private void SetRevision(Revision descriptor)
		{
			Cursor.Current = Cursors.WaitCursor;
			if (descriptor != null)
			{
				FileInRevision firParent = new FileInRevision(descriptor.Number.LocalRevisionNumber,
					htmlStoryBtControl.TheSE.StoryProject.ProjSettings.ProjectFilePath, FileInRevision.Action.Unknown);
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(firParent.GetFileContents(_repository));
				string strXPath = String.Format("/StoryProject/stories[@SetName = 'Stories']/story[@name = '{0}']",
					htmlStoryBtControl.StoryData.Name);
				XmlNode parentStoryNode = doc.SelectSingleNode(strXPath);
				if (parentStoryNode != null)
				{
					StoryData parentStory = new StoryData(parentStoryNode);
					htmlStoryBtControl.ParentStory = parentStory;
				}
			}
			else
			{
				htmlStoryBtControl.ParentStory = null;
			}
			Cursor.Current = Cursors.Default;
		}
		*/
	}
}
