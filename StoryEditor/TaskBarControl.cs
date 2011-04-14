using System;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public partial class TaskBarControl : UserControl
	{
		private MentoreeRequirementsCheck _checker;
		private StoryEditor TheSe;
		private StoryData TheStory;

		public TaskBarControl()
		{
			InitializeComponent();
		}

		public void Initialize(StoryEditor theSe, StoryProjectData theStoryProjectData,
			StoryData theStory)
		{
			TheSe = theSe;
			TheStory = theStory;

			// if it's an empty project, then at least have the task for adding a story.
			if (TheStory == null)
			{
				if ((TheSe.LoggedOnMember != null)
					&& TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
											 TeamMemberData.UserTypes.ProjectFacilitator))
					buttonAddStory.Visible = true;
				return;
			}

			// getting rid of all the 'view' ones, since those are "tasks"
			//  buttonStoryInformation.Visible = true;

			if (TheSe.LoggedOnMember == null)
				return;

			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
									  TeamMemberData.UserTypes.ProjectFacilitator))
			{
				SetProjectFaciliatorButtons(theStory, theStoryProjectData);
			}
			else if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
										   TeamMemberData.UserTypes.EnglishBackTranslator))
			{
				SetEnglishBackTranslatorButtons();
			}
			else if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
										   TeamMemberData.UserTypes.IndependentConsultant))
			{
				SetIndependentConsultantButtons();
			}
			else if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
										   TeamMemberData.UserTypes.ConsultantInTraining))
			{
				SetConsultantInTrainingButtons();
			}
			else if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
										   TeamMemberData.UserTypes.Coach))
			{
				SetCoachButtons();
			}
		}

		private void SetEnglishBackTranslatorButtons()
		{
			buttonViewTasksPf.Visible = true;

			bool bEditAllowed = TheSe.LoggedOnMember.IsEditAllowed(TheStory);

			buttonReturnToProjectFacilitator.Visible =
				buttonSendToConsultant.Visible = bEditAllowed;

			_checker = new EnglishBterRequirementsCheck(TheSe, TheStory);
		}

		private void SetCoachButtons()
		{
			buttonViewTasksPf.Visible = true;

			// make the view CIT tasks visible only if we're in a manage with coaching
			//  situation.
			buttonViewTasksCit.Visible = !TheSe.StoryProject.TeamMembers.HasIndependentConsultant;

			if (!TheSe.LoggedOnMember.IsEditAllowed(TheStory))
				return;

			if (TheStory.ProjStage.ProjectStage == StoryStageLogic.ProjectStages.eTeamComplete)
				buttonMarkFinalApproval.Visible = true;
			else
				buttonMarkPreliminaryApproval.Visible = true;

			buttonSendToCIT.Visible = true;
		}

		private void SetConsultantInTrainingButtons()
		{
			buttonViewTasksPf.Visible = true;
			buttonViewTasksCit.Visible = true;

			if (!TheSe.LoggedOnMember.IsEditAllowed(TheStory))
				return;

			if (TheSe.StoryProject.TeamMembers.HasOutsideEnglishBTer)
				buttonSendToEnglishBter.Visible = true;
			else
				buttonReturnToProjectFacilitator.Visible = (TasksCit.IsTaskOn(TheStory.TasksAllowedCit,
																			  TasksCit.TaskSettings.
																				  SendToProjectFacilitatorForRevision));

			buttonSendToCoach.Visible = (TasksCit.IsTaskOn(TheStory.TasksAllowedCit,
														   TasksCit.TaskSettings.SendToCoachForReview));

			_checker = new ConsultantInTrainingRequirementsCheck(TheSe, TheStory);
		}

		private void SetIndependentConsultantButtons()
		{
			buttonViewTasksPf.Visible = true;

			if (!TheSe.LoggedOnMember.IsEditAllowed(TheStory))
				return;

			if (TheStory.ProjStage.ProjectStage == StoryStageLogic.ProjectStages.eTeamComplete)
				buttonMarkFinalApproval.Visible = true;
			else
				buttonMarkPreliminaryApproval.Visible = true;

			if (TheSe.StoryProject.TeamMembers.HasOutsideEnglishBTer)
				buttonSendToEnglishBter.Visible = true;
			else
				buttonReturnToProjectFacilitator.Visible = true;
		}

		private void SetButtonsAndTooltip(Button btn, bool bEditAllowed, bool bDefaultVisibility,
			TasksPf.TaskSettings taskToCheck, string strTooltipFormat, string strButtonText)
		{
			if (!bEditAllowed)
				btn.Visible = false;
				/* people didn't want to see 'View' anything as a 'task'
				toolTip.SetToolTip(btn, String.Format(Properties.Resources.IDS_PfNoEditToken,
													  strTooltipFormat,
													  TeamMemberData.GetMemberWithEditTokenAsDisplayString(
														  TheSe.StoryProject.TeamMembers,
														  TheSe.theCurrentStory.ProjStage.MemberTypeWithEditToken)));
				*/

			else if (!TasksPf.IsTaskOn(TheStory.TasksAllowedPf, taskToCheck))
				toolTip.SetToolTip(btn, String.Format(Properties.Resources.IDS_PfNotAllowedToModified,
													  strTooltipFormat));

			else
			{
				btn.Visible = bDefaultVisibility;
				if (!String.IsNullOrEmpty(strButtonText))
					btn.Text = strButtonText;
			}
		}

		private void SetProjectFaciliatorButtons(StoryData theStory,
			StoryProjectData theStoryProjectData)
		{
			bool bEditAllowed = TheSe.LoggedOnMember.IsEditAllowed(theStory);

			buttonAddStory.Visible = true;
			buttonViewTasksPf.Visible = true;

			ProjectSettings projSettings = theStoryProjectData.ProjSettings;
			buttonVernacular.Visible = bEditAllowed && projSettings.Vernacular.HasData;
			buttonNationalBt.Visible = bEditAllowed && projSettings.NationalBT.HasData;
			buttonInternationalBt.Visible = bEditAllowed && projSettings.InternationalBT.HasData;
			buttonFreeTranslation.Visible = bEditAllowed && projSettings.FreeTranslation.HasData;

			if (TheStory.CraftingInfo.IsBiblicalStory)
			{
				buttonAnchors.Visible = bEditAllowed;
				// buttonViewRetellings.Visible = bEditAllowed;
				buttonViewTestQuestions.Visible = bEditAllowed;
				// buttonViewTestQuestionAnswers.Visible = bEditAllowed;
			}

			SetButtonsAndTooltip(buttonVernacular,
								 bEditAllowed,
								 projSettings.Vernacular.HasData,
								 TasksPf.TaskSettings.VernacularLangFields,
								 "story language",
								 Properties.Resources.IDS_PfButtonLabelVernacular);

			SetButtonsAndTooltip(buttonNationalBt,
								 bEditAllowed,
								 projSettings.NationalBT.HasData,
								 TasksPf.TaskSettings.NationalBtLangFields,
								 "national language bt",
								 Properties.Resources.IDS_PfButtonLabelNationalBt);

			SetButtonsAndTooltip(buttonInternationalBt,
								 bEditAllowed,
								 projSettings.InternationalBT.HasData,
								 TasksPf.TaskSettings.InternationalBtFields,
								 "English bt",
								 Properties.Resources.IDS_PfButtonLabelInternationalBt);

			SetButtonsAndTooltip(buttonFreeTranslation,
								 bEditAllowed,
								 projSettings.FreeTranslation.HasData,
								 TasksPf.TaskSettings.FreeTranslationFields,
								 "free translation",
								 Properties.Resources.IDS_PfButtonLabelFreeTranslation);

			SetButtonsAndTooltip(buttonAnchors,
								 bEditAllowed,
								 true,
								 TasksPf.TaskSettings.Anchors,
								 "anchor",
								 Properties.Resources.IDS_PfButtonLabelAnchors);

			const string cstrRetelling = "retelling";
			SetButtonsAndTooltip(buttonAddRetellingBoxes,
								 bEditAllowed,
								 true,
								 TasksPf.TaskSettings.Retellings | TasksPf.TaskSettings.Retellings2,
								 cstrRetelling,
								 null);

			// then enable it whether there are any more tests to do
			if (TasksPf.IsTaskOn(TheStory.TasksRequiredPf, TasksPf.TaskSettings.Retellings | TasksPf.TaskSettings.Retellings2))
			{
				if (TheStory.CountRetellingsTests > 0)
					toolTip.SetToolTip(buttonAddRetellingBoxes, String.Format(Properties.Resources.IDS_PfRequiredToDoXTests,
						cstrRetelling,
						TheStory.CountRetellingsTests));
				else
				{
					toolTip.SetToolTip(buttonAddRetellingBoxes, String.Format(Properties.Resources.IDS_PfRequiredTestsDone,
						cstrRetelling));
					buttonAddRetellingBoxes.Enabled = false;
				}
			}

			SetButtonsAndTooltip(buttonViewTestQuestions,
								 bEditAllowed,
								 true,
								 TasksPf.TaskSettings.TestQuestions,
								 "story testing question",
								 Properties.Resources.IDS_PfButtonLabelTestQuestions);

			SetButtonsAndTooltip(buttonAddBoxesForAnswers,
								 bEditAllowed,
								 true,
								 TasksPf.TaskSettings.Answers | TasksPf.TaskSettings.Answers2,
								 "answer",
								 null);

			const string cstrStoryQuestionLabel = "story question";

			if (TasksPf.IsTaskOn(TheStory.TasksRequiredPf, TasksPf.TaskSettings.Answers | TasksPf.TaskSettings.Answers2))
			{
				// then enable it whether there are any more tests to do
				if (TheStory.CountTestingQuestionTests > 0)
					toolTip.SetToolTip(buttonAddBoxesForAnswers, String.Format(Properties.Resources.IDS_PfRequiredToDoXTests,
						cstrStoryQuestionLabel,
						TheStory.CountTestingQuestionTests));
				else
				{
					toolTip.SetToolTip(buttonAddBoxesForAnswers, String.Format(Properties.Resources.IDS_PfRequiredTestsDone,
						cstrStoryQuestionLabel));
					buttonAddBoxesForAnswers.Enabled = false;
				}
			}

			if (!bEditAllowed)
				return;

			if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
				buttonSendToEnglishBter.Visible = true;
			else
				buttonSendToConsultant.Visible = true;

			_checker = new ProjectFacilitatorRequirementsCheck(TheSe, theStory);
		}

		private void buttonAddStory_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
																  TeamMemberData.UserTypes.ProjectFacilitator));
			ParentForm.Close();
			TheSe.addNewStoryAfterToolStripMenuItem_Click(sender, e);
		}

		private void buttonVernacular_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacTypeVernacular, false);
			}
			else
			{
				TheSe.viewVernacularLangFieldMenuItem.Checked = true;
			}
		}

		private void buttonNationalBt_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacTypeNationalBT, false);
			}
			else
			{
				TheSe.viewNationalLangFieldMenuItem.Checked = true;
			}
		}

		private void buttonInternationalBt_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT, false);
			}
			else
			{
				TheSe.viewEnglishBTFieldMenuItem.Checked = true;
			}
		}

		private void buttonFreeTranslation_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacTypeFreeTranslation, false);
			}
			else
			{
				TheSe.viewFreeTranslationToolStripMenuItem.Checked = true;
			}
		}

		private void buttonAnchors_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacAddAnchors, false);
			}
			else
			{
				TheSe.viewAnchorFieldMenuItem.Checked =
					TheSe.viewExegeticalHelps.Checked =     // this kind of goes with anchors
					TheSe.viewNetBibleMenuItem.Checked = true;
			}
		}

		private void buttonRetellings_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacEnterRetellingOfTest1, false);
			}
			else
			{
				TheSe.viewRetellingFieldMenuItem.Checked = true;
			}
		}

		private void buttonAddRetellingBoxes_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
																  TeamMemberData.UserTypes.ProjectFacilitator));
			ParentForm.Close();
			TheSe.editAddTestResultsToolStripMenuItem_Click(sender, e);
			TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacEnterRetellingOfTest1, false);
		}

		private void buttonTestQuestions_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacAddStoryQuestions, false);
			}
			else
			{
				TheSe.viewStoryTestingQuestionMenuItem.Checked = true;
			}
		}

		private void buttonTestQuestionAnswers_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.ProjectFacilitator))
			{
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacEnterAnswersToStoryQuestionsOfTest1, false);
			}
			else
			{
				TheSe.viewStoryTestingQuestionMenuItem.Checked =
					TheSe.viewStoryTestingQuestionAnswerMenuItem.Checked = true;
			}
		}

		private void buttonAddBoxesForAnswers_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
																  TeamMemberData.UserTypes.ProjectFacilitator));
			ParentForm.Close();
			if (TheSe.AddInferenceTestBoxes())
				TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacEnterAnswersToStoryQuestionsOfTest1, false);
		}

		private void buttonStoryInformation_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			TheSe.QueryStoryPurpose();
		}

		private void buttonViewPanorama_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			TheSe.toolStripMenuItemShowPanorama_Click(sender, e);
		}

		private void buttonSendToEnglishBter_Click(object sender, EventArgs e)
		{
			// we go to the English Bter either going forwards from the PF...
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
									  TeamMemberData.UserTypes.ProjectFacilitator))
			{
				if (!CheckForPfDone())
					return;

				TheSe.SetNextStateAdvancedOverride(
					StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBTTest1, true);
			}
			// ... or coming backwards from the CIT/IC
			else if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
										   TeamMemberData.UserTypes.IndependentConsultant |
										   TeamMemberData.UserTypes.ConsultantInTraining))
			{
				if (!CheckIfReadyToReturnToPf())
					return;

				if (TheStory.CraftingInfo.IsBiblicalStory)
				{
					TheSe.SetNextStateAdvancedOverride(
						StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesAfterUnsTest, true);
				}
				else
				{
					TheSe.SetNextStateAdvancedOverride(
						StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest, true);
				}
			}
		}

		private void buttonSendToConsultant_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(
				TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.EnglishBackTranslator |
				TeamMemberData.UserTypes.ProjectFacilitator)
											&& (ParentForm != null)
											&& (_checker != null));

			// we go to the consultant forward either from the English BTer...
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
									  TeamMemberData.UserTypes.EnglishBackTranslator))
			{
				ParentForm.Close();

				if (!_checker.CheckIfRequirementsAreMet())
					return;

			}

			else if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
										   TeamMemberData.UserTypes.ProjectFacilitator))
			{
				if (!CheckForPfDone())
					return;
			}

			TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eConsultantCheck2, true);
		}

		private bool CheckForPfDone()
		{
			System.Diagnostics.Debug.Assert(_checker != null);
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
																  TeamMemberData.UserTypes.ProjectFacilitator));

			ParentForm.Close();
			if (!_checker.CheckIfRequirementsAreMet())
				return false;

			if (MessageBox.Show(Properties.Resources.IDS_TerminalTransitionMessage,
								OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
				return false;

			return true;
		}

		private void buttonReturnToProjectFacilitator_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(
				TheSe.LoggedOnMember.MemberType,
				TeamMemberData.UserTypes.EnglishBackTranslator |
				TeamMemberData.UserTypes.ConsultantInTraining |
				TeamMemberData.UserTypes.IndependentConsultant)
											&& (ParentForm != null));

			ParentForm.Close();

			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
									  TeamMemberData.UserTypes.ConsultantInTraining |
									  TeamMemberData.UserTypes.IndependentConsultant))
			{
				// if it's coming from the CIT/IC
				if (!CheckIfReadyToReturnToPf())
					return;
			}

			// this applies regardless of who it's coming from
			TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eProjFacRevisesAfterUnsTest, true);
		}

		private bool CheckIfReadyToReturnToPf()
		{
			if (!CheckEndOfStateTransition.CheckThatCITAnsweredPFsQuestions(TheSe, TheStory))
				return false;

			// if the IC is logged in and there are unapproved comments, then point that
			//  out to them.
			if (TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
									  TeamMemberData.UserTypes.IndependentConsultant))
			{
				if (TheStory.AreUnapprovedConsultantNotes)
				{
					DialogResult res = MessageBox.Show(Properties.Resources.IDS_WarnAboutUnapprovedComments,
													   OseResources.Properties.Resources.IDS_Caption,
													   MessageBoxButtons.YesNoCancel);
					if (res != DialogResult.Yes)
					{
						if (res == DialogResult.No)
							TheSe.viewConsultantNoteFieldMenuItem.Checked = true;
						return false;
					}
				}

				// if we don't have an explicit coach state, then the IC is the one who
				//  probably needs to deal with Coach Notes, so warn him/her if there
				//  are unresponded to comments.
				if (TheSe.StoryProject.TeamMembers.HasIndependentConsultant &&
					TheStory.AreUnrespondedToCoachNoteComments)
				{
					DialogResult res = MessageBox.Show(Properties.Resources.IDS_WarnAboutUnrespondedToComments,
													   OseResources.Properties.Resources.IDS_Caption,
													   MessageBoxButtons.YesNoCancel);
					if (res != DialogResult.Yes)
					{
						if (res == DialogResult.No)
						{
							TheSe.viewCoachNotesFieldMenuItem.Checked = true;
							MessageBox.Show(Properties.Resources.IDS_LoginAsCoach,
											OseResources.Properties.Resources.IDS_Caption);
						}
						return false;
					}
				}
			}

			// find out from the consultant what tasks they want to set in the story
			var dlg = new SetPfTasksForm(TheSe.StoryProject.ProjSettings,
										 TheStory.TasksAllowedPf, TheStory.TasksRequiredPf,
										 TheStory.CraftingInfo.IsBiblicalStory)
						  {
							  Text = String.Format("Set tasks for the Project Facilitator ({0}) to do on story: {1}",
												   TheSe.StoryProject.TeamMembers.GetNameFromMemberId(
													   TheStory.CraftingInfo.ProjectFacilitatorMemberID),
												   TheStory.Name)
						  };

			if (dlg.ShowDialog() != DialogResult.OK)
				return false;

			TheStory.TasksAllowedPf = dlg.TasksAllowed;
			TheStory.TasksRequiredPf = dlg.TasksRequired;

			// set the attributes that say how many tests are required
			if (TasksPf.IsTaskOn(TheStory.TasksRequiredPf, TasksPf.TaskSettings.Retellings))
				TheStory.CountRetellingsTests = 1;
			if (TasksPf.IsTaskOn(TheStory.TasksRequiredPf, TasksPf.TaskSettings.Retellings2))
				TheStory.CountRetellingsTests = 2;
			if (TasksPf.IsTaskOn(TheStory.TasksRequiredPf, TasksPf.TaskSettings.Answers))
				TheStory.CountTestingQuestionTests = 1;
			if (TasksPf.IsTaskOn(TheStory.TasksRequiredPf, TasksPf.TaskSettings.Answers2))
				TheStory.CountTestingQuestionTests = 2;

			return true;
		}

		private void buttonMarkPreliminaryApproval_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			if (!CheckEndOfStateTransition.CheckThatCITAnsweredPFsQuestions(TheSe, TheStory))
				return;

			TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eTeamComplete, true);
		}

		private void buttonMarkFinalApproval_Click(object sender, EventArgs e)
		{
			ParentForm.Close();
			TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eTeamFinalApproval, true);
		}

		private void buttonSendToCoach_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(_checker != null);
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
																  TeamMemberData.UserTypes.ConsultantInTraining));

			ParentForm.Close();
			if (!_checker.CheckIfRequirementsAreMet())
				return;

			TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eCoachReviewRound2Notes, true);
		}

		private void buttonSendToCIT_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(TeamMemberData.IsUser(TheSe.LoggedOnMember.MemberType,
																  TeamMemberData.UserTypes.Coach));

			ParentForm.Close();
			if (!CheckEndOfStateTransition.CheckThatCoachAnsweredCITsQuestions(TheSe, TheStory))
				return;

			// find out from the Coach what tasks they want to set in the story
			var dlg = new SetCitTasksForm(TheStory.TasksAllowedCit,
				TheStory.TasksRequiredCit)
						  {
							  Text = String.Format("Set tasks for the CIT to do on story: {0}",
												   TheStory.Name)
						  };

			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			TheStory.TasksAllowedCit = dlg.TasksAllowed;
			TheStory.TasksRequiredCit = dlg.TasksRequired;
			TheSe.SetNextStateAdvancedOverride(StoryStageLogic.ProjectStages.eConsultantCauseRevisionAfterUnsTest, true);
		}

		private void buttonViewTasksPf_Click(object sender, EventArgs e)
		{
			var dlg = new SetPfTasksForm(TheSe.StoryProject.ProjSettings,
										 TheStory.TasksAllowedPf,
										 TheStory.TasksRequiredPf,
										 TheStory.CraftingInfo.IsBiblicalStory)
						  {
							  Text = "Tasks for Project Facilitator",
							  Readonly = true
						  };

			dlg.ShowDialog();
		}

		private void buttonViewTasksCit_Click(object sender, EventArgs e)
		{
			var dlg = new SetCitTasksForm(TheStory.TasksAllowedCit,
										  TheStory.TasksRequiredCit)
						  {
							  Text = "Tasks for CIT",
							  Readonly = true
						  };

			dlg.ShowDialog();
		}
	}
}
