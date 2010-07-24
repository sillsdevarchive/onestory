using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chorus.UI.Review;
using Chorus.UI.Review.RevisionsInRepository;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;

namespace OneStoryProjectEditor
{
	public partial class HtmlDisplayForm : Form
	{
		HgRepository _repository;
		List<Revision> _lstRevisions;
		// RevisionInRepositoryModel _revisionInRepositoryModel;
		// RevisionSelectedEvent _revisionSelectedEvent = new RevisionSelectedEvent();
		string _strStoryToDiff, _strLastState;

		public HtmlDisplayForm(StoryEditor theSE, StoryData storyData)
		{
			InitializeComponent();

			_strStoryToDiff = storyData.Name;
			if (theSE.StoryProject.ProjSettings.Vernacular.HasData)
			{
				checkBoxLangVernacular.Text = String.Format(Properties.Resources.IDS_LanguageFields,
															theSE.StoryProject.ProjSettings.Vernacular.LangName);
			}
			else
				checkBoxLangVernacular.Checked = checkBoxLangVernacular.Visible = false;

			if (theSE.StoryProject.ProjSettings.NationalBT.HasData)
			{
				checkBoxLangNationalBT.Text = String.Format(Properties.Resources.IDS_StoryLanguageField,
															theSE.StoryProject.ProjSettings.NationalBT.LangName);
			}
			else
				checkBoxLangNationalBT.Checked = checkBoxLangNationalBT.Visible = false;

			checkBoxLangInternationalBT.Checked =
				checkBoxLangInternationalBT.Visible =
				theSE.StoryProject.ProjSettings.InternationalBT.HasData;

			try
			{
				NullProgress np = new NullProgress();
				_repository = HgRepository.CreateOrLocate(theSE.StoryProject.ProjSettings.ProjectFolder, np);
				/*
				_revisionInRepositoryModel = new RevisionInRepositoryModel(_repository, _revisionSelectedEvent);
				_revisionInRepositoryModel.ProgressDisplay = np;
				*/
				_lstRevisions = _repository.GetAllRevisions();  // _revisionInRepositoryModel.GetHistoryItems();
				progressBar.Value = 0;
				progressBar.Visible = true;
				backgroundWorkerCheckRevisions.RunWorkerAsync(this);
				/*
				_revisionSelectedEvent.Subscribe(SetRevision);
				_revisionsInRepositoryView = new RevisionsInRepositoryView(_revisionInRepositoryModel);
				_revisionsInRepositoryView.Dock = DockStyle.Fill;
				tableLayoutPanelSettings.Controls.Add(_revisionsInRepositoryView, 0, 1);
				tableLayoutPanelSettings.SetColumnSpan(_revisionsInRepositoryView, 2);
				*/
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

		private void tabControl_Selected(object sender, TabControlEventArgs e)
		{
			if (e.TabPage == tabPageDisplayChangeReport)
			{
				if (backgroundWorkerCheckRevisions.IsBusy)
					backgroundWorkerCheckRevisions.CancelAsync();

				htmlStoryBtControl.ViewItemsToInsureOn = VerseData.SetItemsToInsureOn(
					checkBoxLangVernacular.Checked,
					checkBoxLangNationalBT.Checked,
					checkBoxLangInternationalBT.Checked,
					checkBoxAnchors.Checked,
					checkBoxStoryTestingQuestions.Checked,
					checkBoxRetellings.Checked,
					false,  // theSE.viewConsultantNoteFieldMenuItem.Checked,
					false,  // theSE.viewCoachNotesFieldMenuItem.Checked,
					false); // theSE.viewNetBibleMenuItem.Checked


				htmlStoryBtControl.ParentStory = GetStoryForPresentation(_nParentIndex);
				htmlStoryBtControl.StoryData = GetStoryForPresentation(_nChildIndex);

				htmlStoryBtControl.LoadDocument();
			}
		}

		protected StoryData GetStoryForPresentation(int nIndex)
		{
			if ((nIndex >= 0) && (nIndex < dataGridViewRevisions.Rows.Count))
			{
				RevisionInfo ri = dataGridViewRevisions.Rows[nIndex].Tag as RevisionInfo;
				if ((ri != null) && (ri.StoryProjectNode != null))
					return new StoryData(ri.StoryProjectNode);
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
						object[] ao = new object[] { false, false, rev.Number, dateString, rev.UserId, _strLastState };
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

			string strThisState = nodeStoryProject.Attributes["stage"].Value;
			System.Diagnostics.Debug.WriteLine(String.Format("In revision: {0}, story: {1}, State: {2}",
				rev.Number.LocalRevisionNumber,
				_strStoryToDiff,
				strThisState));
			if (strThisState == _strLastState)
				return false;
			_strLastState = strThisState;
			return true;
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
				|| (e.RowIndex < 0) || (e.RowIndex > dataGridViewRevisions.Rows.Count))
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
