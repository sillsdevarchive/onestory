using System;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public partial class EditMemberForm : TopForm
	{
		private readonly TeamMemberData _theMemberData;
		private readonly ProjectSettings _theProjSettings;

		public EditMemberForm(TeamMemberData theMemberData,
			ProjectSettings theProjSettings, bool bAllowNameRoleEdits)
			: base(true)
		{
			InitializeComponent();

			_theMemberData = theMemberData;
			_theProjSettings = theProjSettings;

			if (theMemberData == null)
				return;

			textBoxName.Text = theMemberData.Name;
			MemberType = theMemberData.MemberType;
			textBoxEmail.Text = theMemberData.Email;
			textBoxPhoneNumber.Text = theMemberData.Phone;
			textBoxAltPhone.Text = theMemberData.AltPhone;
			textBoxSkypeID.Text = theMemberData.SkypeID;
			textBoxTeamViewer.Text = theMemberData.TeamViewerID;
			textBoxBioData.Text = theMemberData.BioData;
			DefaultAllowed = theMemberData.DefaultAllowed;
			DefaultRequired = theMemberData.DefaultRequired;

			if (!bAllowNameRoleEdits)
			{
				textBoxName.Enabled = groupBoxRole.Enabled = false;
				Text = "View Member Information (to edit, use 'Project', 'Login', 'Edit Member')";
			}
		}

		public DialogResult UpdateMember()
		{
			DialogResult res = ShowDialog();
			if (res == DialogResult.OK)
			{
				if (MemberName != _theMemberData.Name)
				{
					// make sure this isn't a mistake.
					res = MessageBox.Show(String.Format(Properties.Resources.IDS_ConfirmTeamMemberNameChange,
														_theMemberData.Name, MemberName),
										  OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
					if (res != DialogResult.Yes)
						return res;
				}
				_theMemberData.Name = MemberName;
				_theMemberData.MemberType = MemberType;
				_theMemberData.Email = Email;
				_theMemberData.AltPhone = AltPhone;
				_theMemberData.Phone = Phone;
				_theMemberData.BioData = BioData;
				_theMemberData.SkypeID = SkypeID;
				_theMemberData.TeamViewerID = TeamViewerID;
				_theMemberData.DefaultAllowed = DefaultAllowed;
				_theMemberData.DefaultRequired = DefaultRequired;
			}
			return res;
		}
		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(MemberName))
			{
				MessageBox.Show(
					"You have to enter at least a name and indicate your role (even if you're 'just looking')!",
					 OseResources.Properties.Resources.IDS_Caption);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		public string MemberName
		{
			get { return textBoxName.Text; }
			set { textBoxName.Text = value; }
		}

		public TeamMemberData.UserTypes MemberType
		{
			get
			{
				if (radioButtonStoryCrafter.Checked)
					return TeamMemberData.UserTypes.eCrafter;
				if (radioButtonEnglishBackTranslator.Checked)
					return TeamMemberData.UserTypes.eEnglishBacktranslator;
				if (radioButtonUNS.Checked)
					return TeamMemberData.UserTypes.eUNS;
				if (radioButtonProjectFacilitator.Checked)
					return TeamMemberData.UserTypes.eProjectFacilitator;
				if (radioButtonFirstPassMentor.Checked)
					return TeamMemberData.UserTypes.eFirstPassMentor;
				if (radioButtonConsultantInTraining.Checked)
					return TeamMemberData.UserTypes.eConsultantInTraining;
				if (radioButtonIndependentConsultant.Checked)
					return TeamMemberData.UserTypes.eIndependentConsultant;
				if (radioButtonCoach.Checked)
					return TeamMemberData.UserTypes.eCoach;
				if (radioButtonJustViewing.Checked)
					return TeamMemberData.UserTypes.eJustLooking;
				return TeamMemberData.UserTypes.eUndefined;
			}
			set
			{
				switch (value)
				{
					case TeamMemberData.UserTypes.eCrafter:
						radioButtonStoryCrafter.Checked = true;
						break;
					case TeamMemberData.UserTypes.eEnglishBacktranslator:
						radioButtonEnglishBackTranslator.Checked = true;
						break;
					case TeamMemberData.UserTypes.eUNS:
						radioButtonUNS.Checked = true;
						break;
					case TeamMemberData.UserTypes.eProjectFacilitator:
						radioButtonProjectFacilitator.Checked = true;
						break;
					case TeamMemberData.UserTypes.eFirstPassMentor:
						radioButtonFirstPassMentor.Checked = true;
						break;
					case TeamMemberData.UserTypes.eConsultantInTraining:
						radioButtonConsultantInTraining.Checked = true;
						break;
					case TeamMemberData.UserTypes.eIndependentConsultant:
						radioButtonIndependentConsultant.Checked = true;
						break;
					case TeamMemberData.UserTypes.eCoach:
						radioButtonCoach.Checked = true;
						break;
					case TeamMemberData.UserTypes.eJustLooking:
						radioButtonJustViewing.Checked = true;
						break;
					default:
						System.Diagnostics.Debug.Assert(false); // should get here.
						break;
				}
			}
		}

		public string Email
		{
			get { return (String.IsNullOrEmpty(textBoxEmail.Text) ? null : textBoxEmail.Text); }
			set { textBoxEmail.Text = value; }
		}

		public string Phone
		{
			get { return (String.IsNullOrEmpty(textBoxPhoneNumber.Text) ? null : textBoxPhoneNumber.Text); }
			set { textBoxPhoneNumber.Text = value; }
		}

		public string AltPhone
		{
			get { return (String.IsNullOrEmpty(textBoxAltPhone.Text) ? null : textBoxAltPhone.Text); }
			set { textBoxAltPhone.Text = value; }
		}

		public string SkypeID
		{
			get { return (String.IsNullOrEmpty(textBoxSkypeID.Text) ? null : textBoxSkypeID.Text); }
			set { textBoxSkypeID.Text = value; }
		}

		public string TeamViewerID
		{
			get { return (String.IsNullOrEmpty(textBoxTeamViewer.Text) ? null : textBoxTeamViewer.Text); }
			set { textBoxTeamViewer.Text = value; }
		}

		public string BioData
		{
			get { return (String.IsNullOrEmpty(textBoxBioData.Text) ? null : textBoxBioData.Text); }
			set { textBoxBioData.Text = value; }
		}

		private void radioButtonEnglishBackTranslator_Click(object sender, EventArgs e)
		{
			MessageBox.Show(Properties.Resources.IDS_AreYouSureYouWantToHaveAnEnglishBter, OseResources.Properties.Resources.IDS_Caption);
		}

		private void radioButtonProjectFacilitator_CheckedChanged(object sender, EventArgs e)
		{
			SetDefaultTasksButtonState();
		}

		private void radioButtonConsultantInTraining_CheckedChanged(object sender, EventArgs e)
		{
			SetDefaultTasksButtonState();
		}

		private void SetDefaultTasksButtonState()
		{
			buttonSetDefaultTasks.Visible = (radioButtonProjectFacilitator.Checked ||
											 radioButtonConsultantInTraining.Checked);
		}

		private void buttonSetDefaultTasks_Click(object sender, EventArgs e)
		{
			if (_theProjSettings == null)
			{
				MessageBox.Show(Properties.Resources.IDS_DoAfterOpen,
								OseResources.Properties.Resources.IDS_Caption);
				return;
			}

			// find out from the consultant what tasks they want to set in the story
			try
			{
				if (radioButtonProjectFacilitator.Checked)
					GetPfDefaultTasks();
				else if (radioButtonConsultantInTraining.Checked)
					GetCitDefaultTasks();
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, OseResources.Properties.Resources.IDS_Caption);
			}
		}

		private long _DefaultAllowed;
		public long DefaultAllowed
		{
			get
			{
				if (_DefaultAllowed == 0)
				{
					if (radioButtonProjectFacilitator.Checked)
						return (long) TasksPf.DefaultAllowed;
					if (radioButtonConsultantInTraining.Checked)
						return (long) TasksCit.DefaultAllowed;
				}
				return _DefaultAllowed;
			}
			set { _DefaultAllowed = value; }
		}

		private long _DefaultRequired;
		public long DefaultRequired
		{
			get
			{
				if (_DefaultRequired == 0)
				{
					if (radioButtonProjectFacilitator.Checked)
						return (long) TasksPf.DefaultRequired;
					if (radioButtonConsultantInTraining.Checked)
						return (long) TasksCit.DefaultRequired;
				}
				return _DefaultRequired;
			}
			set { _DefaultRequired = value; }
		}

		private void GetPfDefaultTasks()
		{
			var dlg = new SetPfTasksForm(_theProjSettings,
										 (TasksPf.TaskSettings)DefaultAllowed,
										 (TasksPf.TaskSettings)DefaultRequired, true)
										 {Text = GetDefaultTaskTitleText};

			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			DefaultAllowed = (long)dlg.TasksAllowed;
			DefaultRequired = (long)dlg.TasksRequired;
		}

		private void GetCitDefaultTasks()
		{
			var dlg = new SetCitTasksForm((TasksCit.TaskSettings) DefaultAllowed,
										  (TasksCit.TaskSettings) DefaultRequired)
										  {Text = GetDefaultTaskTitleText};

			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			DefaultAllowed = (long)dlg.TasksAllowed;
			DefaultRequired = (long)dlg.TasksRequired;
		}

		private string GetDefaultTaskTitleText
		{
			get { return String.Format("Set Default Tasks for {0}", _theMemberData.Name); }
		}
	}
}
