using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using Palaso.UI.WindowsForms.Keyboarding;

namespace OneStoryProjectEditor
{
	public partial class TeamMemberForm : Form
	{
		protected const string CstrDefaultFontTooltipVernacular =
			"Click here to choose the font, size, and color of the Story language text{0}Currently, Font: {1}, Color: {2}, Keyboard: {3}, RTL: {4}";
		protected const string CstrDefaultFontTooltipNationalBT =
			"Click here to choose the font, size, and color of the National language back-translation text{0}Currently, Font: {1}, Color: {2}, Keyboard: {3}, RTL: {4}";
		protected const string CstrDefaultFontTooltipInternationalBT =
			"Click here to choose the font, size, and color of the English language back-translation text{0}Currently, Font: {1}, Color: {2}, Keyboard: {3}, RTL: {4}";
		internal const string CstrDefaultOKLabel = "&Login";

		protected ProjectSettings m_projSettings = null;
		protected TeamMembersData _dataTeamMembers = null;
		protected string m_strSelectedMember = null;

		protected bool Modified = false;

		Dictionary<string, TeamMemberData> m_mapNewMembersThisSession = new Dictionary<string, TeamMemberData>();

		public TeamMemberForm(TeamMembersData dataTeamMembers, ProjectSettings projSettings, string strOKLabel)
		{
			_dataTeamMembers = dataTeamMembers;
			m_projSettings = projSettings;

			InitializeComponent();

			foreach (TeamMemberData aMember in dataTeamMembers.Values)
			{
				listBoxTeamMembers.Items.Add(aMember.Name);
				listBoxMemberRoles.Items.Add(TeamMemberData.GetMemberTypeAsDisplayString(aMember.MemberType));
			}

			if ((listBoxTeamMembers.Items.Count > 0) && !String.IsNullOrEmpty(Properties.Settings.Default.LastMemberLogin))
				listBoxTeamMembers.SelectedItem = Properties.Settings.Default.LastMemberLogin;

			// initialize the keyboard combo list
			foreach (KeyboardController.KeyboardDescriptor keyboard in
				KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All))
			{
				comboBoxKeyboardVernacular.Items.Add(keyboard.Name);
				comboBoxKeyboardNationalBT.Items.Add(keyboard.Name);
			}

			// initialize the vernacular language controls
			textBoxVernacular.Text = ((String.IsNullOrEmpty(projSettings.Vernacular.LangName))? projSettings.ProjectName : projSettings.Vernacular.LangName);
			textBoxVernacularEthCode.Text = projSettings.Vernacular.LangCode;
			if (!String.IsNullOrEmpty(projSettings.Vernacular.Keyboard) && comboBoxKeyboardVernacular.Items.Contains(projSettings.Vernacular.Keyboard))
				comboBoxKeyboardVernacular.SelectedItem = projSettings.Vernacular.Keyboard;
			textBoxVernSentFullStop.Font = projSettings.Vernacular.LangFont;
			textBoxVernSentFullStop.ForeColor = projSettings.Vernacular.FontColor;
			textBoxVernSentFullStop.Text = projSettings.Vernacular.FullStop;
			checkBoxVernacularRTL.Checked = projSettings.Vernacular.IsRTL;

			// if there is a national language configured, then initialize those as well.
			textBoxNationalBTLanguage.Text = projSettings.NationalBT.LangName;
			textBoxNationalBTEthCode.Text = projSettings.NationalBT.LangCode;
			if (!String.IsNullOrEmpty(projSettings.NationalBT.Keyboard) && comboBoxKeyboardNationalBT.Items.Contains(projSettings.NationalBT.Keyboard))
				comboBoxKeyboardNationalBT.SelectedItem = projSettings.NationalBT.Keyboard;
			textBoxNationalBTSentFullStop.Font = projSettings.NationalBT.LangFont;
			textBoxNationalBTSentFullStop.ForeColor = projSettings.NationalBT.FontColor;
			textBoxNationalBTSentFullStop.Text = projSettings.NationalBT.FullStop;
			checkBoxNationalRTL.Checked = projSettings.NationalBT.IsRTL;
			checkBoxNationalLangBT.Checked = projSettings.NationalBT.HasData;

			toolTip.SetToolTip(buttonVernacularFont, String.Format(CstrDefaultFontTooltipVernacular,
																   Environment.NewLine, projSettings.Vernacular.LangFont,
																   projSettings.Vernacular.FontColor,
																   projSettings.Vernacular.Keyboard,
																   projSettings.Vernacular.IsRTL));
			toolTip.SetToolTip(buttonNationalBTFont, String.Format(CstrDefaultFontTooltipNationalBT,
																   Environment.NewLine, projSettings.NationalBT.LangFont,
																   projSettings.NationalBT.FontColor,
																   projSettings.NationalBT.Keyboard,
																   projSettings.NationalBT.IsRTL));
			toolTip.SetToolTip(buttonInternationalBTFont, String.Format(CstrDefaultFontTooltipInternationalBT,
																   Environment.NewLine, projSettings.InternationalBT.LangFont,
																   projSettings.InternationalBT.FontColor,
																   projSettings.InternationalBT.Keyboard,
																   projSettings.InternationalBT.IsRTL));


			textBoxProjectName.Text = projSettings.ProjectName;

			if (!String.IsNullOrEmpty(strOKLabel))
				buttonOK.Text = strOKLabel;

			// if the user hasn't configured the language information, send them there first
			if (String.IsNullOrEmpty(textBoxVernacularEthCode.Text))
				tabControlProjectMetaData.SelectedTab = tabPageLanguageInfo;

			if (!String.IsNullOrEmpty(textBoxVernacular.Text) && String.IsNullOrEmpty(textBoxVernacularEthCode.Text))
				ProposeEthnologueCode(textBoxVernacular.Text, textBoxVernacularEthCode);
		}

		public string SelectedMember
		{
			get { return m_strSelectedMember; }
			set
			{
				if (!listBoxTeamMembers.Items.Contains(value))
					throw new ApplicationException(String.Format("The project File doesn't contain a member named '{0}'", value));
				listBoxTeamMembers.SelectedItem = m_strSelectedMember = value;
			}
		}

		private bool DoAccept()
		{
			try
			{
				FinishEdit();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message,  Properties.Resources.IDS_Caption);
				return false;
			}

			return true;
		}

		protected void FinishEdit()
		{
			// the only time you can *not* have a BT language is if the vernacular is "English"
			if ((textBoxVernacular.Text != "English") && !checkBoxEnglishBT.Checked && !checkBoxNationalLangBT.Checked)
				throw new ApplicationException("You must have a back-translation language; either a national language back-translation or English. Check one of the boxes that begins 'Project will use a *** BT?'");

			// update the language information as well (in case that was changed also)
			m_projSettings.Vernacular.LangName = textBoxVernacular.Text;
			m_projSettings.Vernacular.LangCode = textBoxVernacularEthCode.Text;
			m_projSettings.Vernacular.LangFont = textBoxVernacular.Font;
			m_projSettings.Vernacular.FontColor = textBoxVernacular.ForeColor;
			m_projSettings.Vernacular.Keyboard = (string)comboBoxKeyboardVernacular.SelectedItem;
			m_projSettings.Vernacular.FullStop = textBoxVernSentFullStop.Text;
			m_projSettings.Vernacular.IsRTL = checkBoxVernacularRTL.Checked;

			if (checkBoxNationalLangBT.Checked)
			{
				m_projSettings.NationalBT.LangName = textBoxNationalBTLanguage.Text;
				m_projSettings.NationalBT.LangCode = textBoxNationalBTEthCode.Text;
				m_projSettings.NationalBT.LangFont = textBoxNationalBTLanguage.Font;
				m_projSettings.NationalBT.FontColor = textBoxNationalBTLanguage.ForeColor;
				m_projSettings.NationalBT.Keyboard = (string)comboBoxKeyboardNationalBT.SelectedItem;
				m_projSettings.NationalBT.FullStop = textBoxNationalBTSentFullStop.Text;
				m_projSettings.NationalBT.IsRTL = checkBoxNationalRTL.Checked;
			}
			else
				m_projSettings.NationalBT.HasData = false;

			if (!checkBoxEnglishBT.Checked)
				// don't set via 'Checked' because HasData should only be set if false
				m_projSettings.InternationalBT.HasData = false;

			// English was done by the font dialog handler
			Modified = false;
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void listBoxTeamMembers_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool bOneSelected = (listBoxTeamMembers.SelectedIndex != -1);
			buttonEditMember.Enabled = buttonOK.Enabled = bOneSelected;

			// the delete button is only enabled during the current session (just to prevent them
			//  from removing a team member that has other references in the project).
			if (bOneSelected)
			{
				m_strSelectedMember = (string)listBoxTeamMembers.SelectedItem;
				buttonDeleteMember.Visible = m_mapNewMembersThisSession.ContainsKey(SelectedMember);
			}
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			// this button should only be enabled if a team member is selected
			System.Diagnostics.Debug.Assert(listBoxTeamMembers.SelectedIndex != -1);
			if (listBoxTeamMembers.SelectedIndex == -1)
				return;

			// first see if the project information has been configured
			if (String.IsNullOrEmpty(textBoxProjectName.Text)
				|| String.IsNullOrEmpty(textBoxVernacular.Text)
				|| String.IsNullOrEmpty(textBoxVernacularEthCode.Text)
				|| String.IsNullOrEmpty(textBoxVernSentFullStop.Text)
				|| (checkBoxNationalLangBT.Checked)
					&&  (  String.IsNullOrEmpty(textBoxNationalBTLanguage.Text)
						|| String.IsNullOrEmpty(textBoxNationalBTEthCode.Text)
						|| String.IsNullOrEmpty(textBoxNationalBTSentFullStop.Text)))
			{
				tabControlProjectMetaData.SelectedTab = tabPageLanguageInfo;
				MessageBox.Show("Configure the Project and Language Name information as well.",  Properties.Resources.IDS_Caption);
				return;
			}

			// if the selected user is a UNS, this is probably a mistake.
			TeamMemberData theMember = _dataTeamMembers[SelectedMember];
			if ((theMember.MemberType == TeamMemberData.UserTypes.eUNS) && (buttonOK.Text == CstrDefaultOKLabel))
			{
				MessageBox.Show("You may have added a UNS in order to identify, for example, which UNS did the back translation or a particular test. However, you as the crafter should still be logged in to enter the UNS's comments. So select your *crafter* member name and click 'Login' again",  Properties.Resources.IDS_Caption);
				return;
			}

			// when the button label is "OK", it means we're adding a UNS
			if (buttonOK.Text == CstrDefaultOKLabel)
			{
				Properties.Settings.Default.LastMemberLogin = SelectedMember;
				Properties.Settings.Default.LastUserType = _dataTeamMembers[SelectedMember].MemberTypeAsString;
				Properties.Settings.Default.Save();
			}

			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void buttonAddNewMember_Click(object sender, EventArgs e)
		{
			// unselect any member and set the target tab (see
			//  tabControlProjectMetaData_Selected for what happens)
			listBoxTeamMembers.SelectedIndex = -1;

			EditMemberForm dlg = new EditMemberForm(null);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				if (listBoxTeamMembers.Items.Contains(dlg.MemberName))
				{
					MessageBox.Show(String.Format("Oops... you already have a member with the name, '{0}'. If you meant to edit that member, then select the name in the listbox and click the 'Edit Member' button", dlg.MemberName));
					return;
				}

				TeamMemberData theNewMemberData;
				if (m_mapNewMembersThisSession.TryGetValue(dlg.MemberName, out theNewMemberData))
				{
					// I don't see how this could happen... this must have been from back when
					//  you could edit and add in a similar way. Now *Add* means *add a new one*
					//  and they can't exist in this map...
					System.Diagnostics.Debug.Assert(false);

					// must just be editing the already added member...
					System.Diagnostics.Debug.Assert(listBoxTeamMembers.Items.Contains(dlg.MemberName));

					theNewMemberData.MemberType = dlg.MemberType;
					theNewMemberData.Email = dlg.Email;
					theNewMemberData.AltPhone = dlg.AltPhone;
					theNewMemberData.Phone = dlg.Phone;
					theNewMemberData.BioData = dlg.BioData;
					theNewMemberData.SkypeID = dlg.SkypeID;
					theNewMemberData.TeamViewerID = dlg.TeamViewerID;

					// update the role listbox
					int nIndex = listBoxTeamMembers.Items.IndexOf(dlg.MemberName);
					listBoxMemberRoles.Items[nIndex] = TeamMemberData.GetMemberTypeAsDisplayString(theNewMemberData.MemberType);
				}
				else
				{
					// add this new user to the proj file
					theNewMemberData = new TeamMemberData(dlg.MemberName,
						dlg.MemberType, String.Format("mem-{0}", Guid.NewGuid()),
						dlg.Email, dlg.SkypeID, dlg.TeamViewerID, dlg.Phone, dlg.AltPhone,
						dlg.BioData);

					_dataTeamMembers.Add(dlg.MemberName, theNewMemberData);
					m_mapNewMembersThisSession.Add(dlg.MemberName, theNewMemberData);
					listBoxTeamMembers.Items.Add(dlg.MemberName);
					listBoxMemberRoles.Items.Add(TeamMemberData.GetMemberTypeAsDisplayString(theNewMemberData.MemberType));
					listBoxTeamMembers.SelectedItem = dlg.MemberName;
				}
			}
		}

		private void buttonEditMember_Click(object sender, EventArgs e)
		{
			// this button should only be enabled if a team member is selected
			System.Diagnostics.Debug.Assert(listBoxTeamMembers.SelectedIndex != -1);
			int nIndex = listBoxTeamMembers.SelectedIndex;

			m_strSelectedMember = (string)listBoxTeamMembers.SelectedItem;
			System.Diagnostics.Debug.Assert(_dataTeamMembers.ContainsKey(m_strSelectedMember));
			TeamMemberData theMemberData = _dataTeamMembers[m_strSelectedMember];
			EditMemberForm dlg = new EditMemberForm(theMemberData);
			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			theMemberData.Name = dlg.MemberName;
			theMemberData.MemberType = dlg.MemberType;
			theMemberData.Email = dlg.Email;
			theMemberData.AltPhone = dlg.AltPhone;
			theMemberData.Phone = dlg.Phone;
			theMemberData.BioData = dlg.BioData;
			theMemberData.SkypeID = dlg.SkypeID;
			theMemberData.TeamViewerID = dlg.TeamViewerID;

			// update the role listbox
			listBoxMemberRoles.Items[nIndex] = TeamMemberData.GetMemberTypeAsDisplayString(theMemberData.MemberType);
			if (theMemberData.Name != m_strSelectedMember)
			{
				_dataTeamMembers.Remove(m_strSelectedMember);
				m_strSelectedMember = theMemberData.Name;
				_dataTeamMembers.Add(m_strSelectedMember, theMemberData);
			}

			listBoxTeamMembers.Items[nIndex] = theMemberData.Name;

			// keep a hang on it so we don't try to, for example, give it a new guid
			if (!m_mapNewMembersThisSession.ContainsKey(dlg.MemberName))
				m_mapNewMembersThisSession.Add(dlg.MemberName, theMemberData);
		}

		private void buttonDeleteMember_Click(object sender, EventArgs e)
		{
			// this is only enabled if we added the member this session
			System.Diagnostics.Debug.Assert(m_mapNewMembersThisSession.ContainsKey(SelectedMember) && _dataTeamMembers.ContainsKey(SelectedMember));

			_dataTeamMembers.Remove(SelectedMember);
			m_mapNewMembersThisSession.Remove(SelectedMember);
		}

		private void tabControlProjectMetaData_Selected(object sender, TabControlEventArgs e)
		{
			if (e.TabPage == tabPageMemberList)
			{
				Console.WriteLine("tabPageMemberList Selected");

				// if the user made some changes and then is moving away from the tab,
				//  then do an implicit Accept
				if (Modified)
					if (!DoAccept())
						tabControlProjectMetaData.SelectedTab = tabPageLanguageInfo;
			}
		}

		void textBox_TextChanged(object sender, System.EventArgs e)
		{
			Modified = true;
		}

		private void buttonVernacularFont_Click(object sender, EventArgs e)
		{
			try
			{
				fontDialog.Font = textBoxVernacular.Font;
				fontDialog.Color = textBoxVernacular.ForeColor;
				if (fontDialog.ShowDialog() == DialogResult.OK)
				{
					textBoxVernacular.Font = textBoxVernacularEthCode.Font = fontDialog.Font;
					textBoxVernacular.ForeColor = textBoxVernacularEthCode.ForeColor = fontDialog.Color;
					Modified = true;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message == "Only TrueType fonts are supported. This is not a TrueType font.")
					MessageBox.Show("Since you just added this font, you have to restart the program for it to work", Properties.Resources.IDS_Caption);
			}
		}

		private void buttonNationalBTFont_Click(object sender, EventArgs e)
		{
			try
			{
				fontDialog.Font = textBoxNationalBTLanguage.Font;
				fontDialog.Color = textBoxNationalBTLanguage.ForeColor;
				if (fontDialog.ShowDialog() == DialogResult.OK)
				{
					textBoxNationalBTLanguage.Font = textBoxNationalBTEthCode.Font = fontDialog.Font;
					textBoxNationalBTLanguage.ForeColor = textBoxNationalBTEthCode.ForeColor = fontDialog.Color;
					Modified = true;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message == "Only TrueType fonts are supported. This is not a TrueType font.")
					MessageBox.Show("Since you just added this font, you have to restart the program for it to work", Properties.Resources.IDS_Caption);
			}
		}

		private void buttonInternationalBTFont_Click(object sender, EventArgs e)
		{
			try
			{
				fontDialog.Font = m_projSettings.InternationalBT.LangFont;
				fontDialog.Color = m_projSettings.InternationalBT.FontColor;
				if (fontDialog.ShowDialog() == DialogResult.OK)
				{
					m_projSettings.InternationalBT.LangFont = fontDialog.Font;
					m_projSettings.InternationalBT.FontColor = fontDialog.Color;
					Modified = true;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message == "Only TrueType fonts are supported. This is not a TrueType font.")
					MessageBox.Show("Since you just added this font, you have to restart the program for it to work", Properties.Resources.IDS_Caption);
			}
		}

		private void listBoxTeamMembers_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			buttonOK_Click(sender, e);
		}

		private void listBoxMemberRoles_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			buttonOK_Click(sender, e);
		}

		private void textBoxVernacular_Leave(object sender, EventArgs e)
		{
			ProposeEthnologueCode(textBoxVernacular.Text, textBoxVernacularEthCode);
		}

		private void textBoxNationalBTLanguage_Leave(object sender, EventArgs e)
		{
			ProposeEthnologueCode(textBoxNationalBTLanguage.Text, textBoxNationalBTEthCode);
		}

		string _strLangCodesFile = null;

		protected void ProposeEthnologueCode(string strLanguageName, TextBox tbLanguageCode)
		{
			if (!String.IsNullOrEmpty(tbLanguageCode.Text))  // only have to suggest if we don't have a value
				return;

			if (_strLangCodesFile == null)
				_strLangCodesFile = File.ReadAllText(PathToLangCodesFile);

			int nIndex = _strLangCodesFile.IndexOf(String.Format("\t{0}{1}", strLanguageName, Environment.NewLine));
			const int CnOffset = 8;
			if (nIndex >= CnOffset)
			{
				nIndex -= CnOffset;    // back up to the beginning of the line;
				int nLength = 1 /* for the tab */ + strLanguageName.Length + CnOffset;
				string strEntry = _strLangCodesFile.Substring(nIndex, nLength);

				// now, grab off just the code, which goes from the beginning of the line to the first tab.
				nIndex = strEntry.IndexOf('\t');
				string strCode = strEntry.Substring(0, nIndex);
				tbLanguageCode.Text = strCode;
			}
		}

		protected const string CstrLangCodesFilename = "LanguageCodes.tab";

		protected string PathToLangCodesFile
		{
			get
			{
				// try the same folder as we're executing out of
				string strCurrentFolder = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
				strCurrentFolder = Path.GetDirectoryName(strCurrentFolder);
				string strFileToCheck = String.Format(@"{0}\{1}", strCurrentFolder, CstrLangCodesFilename);
#if DEBUG
				if (!File.Exists(strFileToCheck))
					// on dev machines, this file is in the "..\..\src\EC\TECkit Mapping Editor" folder
					strFileToCheck = @"C:\code\StoryEditor\StoryEditor\" + CstrLangCodesFilename;
#endif
				System.Diagnostics.Debug.Assert(File.Exists(strFileToCheck), String.Format("Can't find: {0}! You'll need to re-install or contact bob_eaton@sall.com", strFileToCheck));

				return strFileToCheck;
			}
		}

		private void textBoxVernSentFullStop_Enter(object sender, EventArgs e)
		{
			SetKeyboard((string)comboBoxKeyboardVernacular.SelectedItem);
		}

		private void textBoxNationalBTSentFullStop_Enter(object sender, EventArgs e)
		{
			SetKeyboard((string)comboBoxKeyboardNationalBT.SelectedItem);
		}

		protected void SetKeyboard(string strKeybaordToSet)
		{
			if (!String.IsNullOrEmpty(strKeybaordToSet))
				KeyboardController.ActivateKeyboard(strKeybaordToSet);
		}

		private void textBoxSentFullStop_Leave(object sender, EventArgs e)
		{
			KeyboardController.DeactivateKeyboard();
		}

		private void checkBoxNationalLangBT_CheckedChanged(object sender, EventArgs e)
		{
			labelNationalBTLanguage.Enabled =
				textBoxNationalBTLanguage.Enabled =
				textBoxNationalBTEthCode.Enabled =
				comboBoxKeyboardNationalBT.Enabled =
				textBoxNationalBTSentFullStop.Enabled =
				buttonNationalBTFont.Enabled =
				checkBoxNationalRTL.Enabled = checkBoxNationalLangBT.Checked;
		}

		private void checkBoxEnglishBT_CheckedChanged(object sender, EventArgs e)
		{
			buttonInternationalBTFont.Enabled = checkBoxEnglishBT.Checked;
		}

		private void textBoxVernacular_TextChanged(object sender, EventArgs e)
		{
			Modified = true;

			// if the user retypes the language name, then clobber the eth code as well
			//  (so we'll guess again)
			textBoxVernacularEthCode.Text = null;
		}

		private void textBoxNationalBTLanguage_TextChanged(object sender, EventArgs e)
		{
			Modified = true;

			// if the user retypes the language name, then clobber the eth code as well
			//  (so we'll guess again)
			textBoxNationalBTEthCode.Text = null;
		}

		private void comboBoxKeyboard_DragDrop(object sender, DragEventArgs e)
		{
			// initialize the keyboard combo list (in case the user just added it while
			//  we were open
			System.Diagnostics.Debug.Assert(sender is ComboBox);
			ComboBox cb = (ComboBox)sender;
			cb.Items.Clear();
			foreach (KeyboardController.KeyboardDescriptor keyboard in
				KeyboardController.GetAvailableKeyboards(KeyboardController.Engines.All))
			{
				cb.Items.Add(keyboard.Name);
			}
		}
	}
}
