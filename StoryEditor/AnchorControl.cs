using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetLoc;

namespace OneStoryProjectEditor
{
	public partial class AnchorControl : ResizableControl
	{
		protected const string CstrFieldNameAnchor = "AnchorButton";
		protected int m_nNumRows = 1;
		protected VerseControl _ctrlVerse;
		protected AnchorsData _myAnchorsData;
		// protected Dictionary<ToolStripButton, List<TextBox>> _mapAnchorsToTextBoxes = new Dictionary<ToolStripButton, List<TextBox>>();

		public AnchorControl()
		{
			InitializeComponent();
			Localizer.Ctrl(this);
		}
		public AnchorControl(VerseControl ctrlVerse, StoryStageLogic storyStageLogic,
			AnchorsData anAnchorsData)
			: base(storyStageLogic)
		{
			_ctrlVerse = ctrlVerse;
			_myAnchorsData = anAnchorsData;
			InitializeComponent();
			Localizer.Ctrl(this);

			tableLayoutPanel.SuspendLayout();
			SuspendLayout();

			labelAnchor.Text = AnchorsData.AnchorLabel;
			tableLayoutPanel.Controls.Add(labelAnchor, 0, 0);
			tableLayoutPanel.Controls.Add(toolStripAnchors, 1, 0);

			// add the label and tool strip as a new row to the table layout panel
			// finally populate the buttons on that tool strip
			foreach (AnchorData anAnchorData in anAnchorsData)
				InitAnchorButton(toolStripAnchors, anAnchorData);

			tableLayoutPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		protected ToolStripButton InitAnchorButton(ToolStrip ts, AnchorData theAnchorData)
		{
			string strText = NetBibleViewer.CheckForLocalization(theAnchorData.JumpTarget);

			if (!String.IsNullOrEmpty(theAnchorData.ToolTipText)
				&& (theAnchorData.JumpTarget != theAnchorData.ToolTipText))
			{
				strText += AnchorData.CstrTooltipIndicator; // give an indication that there's a tooltip
			}

			var aButton = new ToolStripButton
							  {
								  DisplayStyle = ToolStripItemDisplayStyle.Text,
								  Tag = theAnchorData,
								  Name = CstrFieldNameAnchor + theAnchorData.JumpTarget,
								  Text = strText,
								  ToolTipText = theAnchorData.ToolTipText,
								  Margin = new Padding(0)
							  };
			aButton.Click += aButton_Click;
			aButton.MouseDown += aButton_MouseDown;
			ts.Items.Add(aButton);
			aButton.Height = 18;
			return aButton;
		}

		protected ToolStripButton m_theLastButtonClicked = null;

		void aButton_MouseDown(object sender, MouseEventArgs e)
		{
			Debug.Assert(sender is ToolStripButton);
			if (e.Button == MouseButtons.Right)
				m_theLastButtonClicked = (ToolStripButton)sender;
		}

		void aButton_Click(object sender, EventArgs e)
		{
			Form form = FindForm();
			if ((form != null) && (form is StoryEditor))
			{
				var aSE = (StoryEditor)form;
				var tssb = (ToolStripButton)sender;

				// if this is the null anchor, then do nothing
				if (tssb.Text == CstrNullAnchor)
					return;

#if !NewWay
				// the button may have the extra indicator that there's a tooltip.
				//  but the tag has the jump target
				Debug.Assert((tssb.Tag != null) && (tssb.Tag is AnchorData));
				var anchorData = tssb.Tag as AnchorData;
				var strJumpTarget = anchorData.JumpTarget;
#else
				string strJumpTarget = tssb.Text;
				int nIndLen = AnchorData.CstrTooltipIndicator.Length;
				if (strJumpTarget.Substring(strJumpTarget.Length - nIndLen, nIndLen) == AnchorData.CstrTooltipIndicator)
					strJumpTarget = strJumpTarget.Substring(0, strJumpTarget.Length - nIndLen);
#endif

				aSE.SetNetBibleVerse(strJumpTarget);
				aSE.FocusOnVerse(_ctrlVerse.VerseNumber, true, true);

				// if we aren't already in some text box, then set the focus on the
				//  parent verse form so that scroll wheel can work
				if (CtrlTextBox._inTextBox == null)
					_ctrlVerse.Focus();
			}
		}

		private void toolStripAnchors_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(NetBibleViewer)))
			{
				// the only function of the button here is to add a slot to type a con note
				StoryEditor theSE;
				if (!CheckForProperEditToken(out theSE))
					return;

				if (TeamMemberData.IsUser(theSE.LoggedOnMember.MemberType,
										  TeamMemberData.UserTypes.ProjectFacilitator)
					&& !TasksPf.IsTaskOn(theSE.TheCurrentStory.TasksAllowedPf,
										 TasksPf.TaskSettings.Anchors))
				{
					LocalizableMessageBox.Show(Localizer.Str("The consultant has not allowed you to enter Anchors at this time"),
									StoryEditor.OseCaption);
					return;
				}

				var theNetBibleViewer = (NetBibleViewer)e.Data.GetData(typeof(NetBibleViewer));
				if ((from ToolStripButton btn in toolStripAnchors.Items
					 select btn.Tag as AnchorData)
							   .Any(anchorData => anchorData.JumpTarget == theNetBibleViewer.JumpTarget))
				{
					e.Effect = DragDropEffects.None;
					return;
				}

				e.Effect = DragDropEffects.Link;
			}
		}

		private void toolStripAnchors_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(NetBibleViewer)))
			{
				// the only function of the button here is to add a slot to type a con note
				StoryEditor theSe;
				if (!CheckForProperEditToken(out theSe))
					return;

				var theNetBibleViewer = (NetBibleViewer)e.Data.GetData(typeof(NetBibleViewer));
				var theAnchorData = _myAnchorsData.AddAnchorData(theNetBibleViewer.JumpTarget, theNetBibleViewer.JumpTarget);
				InitAnchorButton(toolStripAnchors, theAnchorData);

				// indicate that we've changed something so that we don't exit without offering
				//  to save.
				theSe.Modified = true;
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				// the only function of the button here is to add a slot to type a con note
				StoryEditor theSE;
				if (!CheckForProperEditToken(out theSE))
					return;

				/*
				if (_mapAnchorsToTextBoxes.ContainsKey(m_theLastButtonClicked))
				{
					DialogResult res = LocalizableMessageBox.Show(String.Format("The anchor you are about to delete has exegetical or cultural note(s) attached to it. These will be deleted also. Click 'OK' to continue with the deletion.{0}{0}[if you would rather have kept them, say associated to another anchor, then tell bob_eaton@sall.com and he may implement that feature. For now, you can copy the note and paste it into a new note added to a new or existing anchor (right-click on the anchor and choose 'Add Exegetical/Cultural Note'). Then come back here and delete this anchor]", Environment.NewLine),  StoryEditor.OseCaption, MessageBoxButtons.OKCancel);
					if (res != DialogResult.OK)
						return;

					List<TextBox> lstTextBoxes = _mapAnchorsToTextBoxes[m_theLastButtonClicked];
					foreach (TextBox tb in lstTextBoxes)
					{
						int nRowIndex = tableLayoutPanel.GetRow(tb);
						RemoveRow(nRowIndex);
						m_nNumRows--;   // I think it's enough to just decrement these counts
					}
					_mapAnchorsToTextBoxes.Remove(m_theLastButtonClicked);  // I think these will be cleaned up now
					AdjustHeightWithSuspendLayout(null);
				}
				*/
				toolStripAnchors.Items.RemoveByKey(m_theLastButtonClicked.Name);
				Debug.Assert((m_theLastButtonClicked.Tag != null) && (m_theLastButtonClicked.Tag is AnchorData));
				var theAnchorData = (AnchorData)m_theLastButtonClicked.Tag;
				_myAnchorsData.Remove(theAnchorData);
				m_theLastButtonClicked = null;

				// indicate that we've changed something so that we don't exit without offering
				//  to save.
				theSE.Modified = true;
			}
			else
				LocalizableMessageBox.Show("Right-click on one of the buttons to choose which one to delete",  StoryEditor.OseCaption);
		}

		private void addCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				// the only function of the button here is to add a slot to type a con note
				StoryEditor theSE;
				if (!CheckForProperEditToken(out theSE))
					return;
				Debug.Assert(m_theLastButtonClicked.Tag is AnchorData);

				var dlg = new AnchorAddCommentForm(m_theLastButtonClicked.Text, m_theLastButtonClicked.ToolTipText);
				DialogResult res = dlg.ShowDialog();
				if ((res == DialogResult.OK) || (res == DialogResult.Yes))
				{
					Debug.Assert((m_theLastButtonClicked.Tag != null) && (m_theLastButtonClicked.Tag is AnchorData));
					var theAnchorData = (AnchorData)m_theLastButtonClicked.Tag;
					theAnchorData.ToolTipText = m_theLastButtonClicked.ToolTipText = dlg.CommentText;

					string strJumpTarget = m_theLastButtonClicked.Text;
					int nIndLen = AnchorData.CstrTooltipIndicator.Length;
					if (strJumpTarget.Substring(strJumpTarget.Length - nIndLen, nIndLen) != AnchorData.CstrTooltipIndicator)
						m_theLastButtonClicked.Text += AnchorData.CstrTooltipIndicator;

					theSE.Modified = true;
				}
			}
			else
				LocalizableMessageBox.Show("Right-click on one of the buttons to choose which one to add the comment to",  StoryEditor.OseCaption);
		}

		private void toolStripAnchors_MouseDown(object sender, MouseEventArgs e)
		{
			m_theLastButtonClicked = null;
		}

		private void addConsultantCoachNoteOnThisAnchorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return;

			if (m_theLastButtonClicked != null)
			{
				Debug.Assert(theSE.LoggedOnMember != null);
				string strNote = StoryEditor.GetInitials(theSE.LoggedOnMember.Name) + StoryEditor.StrRegarding + AnchorsData.AnchorLabel + " ";
				strNote += m_theLastButtonClicked.Text;

				if (m_theLastButtonClicked.ToolTipText != m_theLastButtonClicked.Text)
					strNote += String.Format(" ({0})", m_theLastButtonClicked.ToolTipText);

				strNote += ". ";

				theSE.SendNoteToCorrectPane(_ctrlVerse.VerseNumber, strNote, false);
			}
			else
				LocalizableMessageBox.Show("Right-click on one of the buttons to choose which one to add the comment to", StoryEditor.OseCaption);
		}

		private const string CstrNullAnchor = "No Anchor";

		private void insertNullAnchorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE))
				return;

			AnchorData theAnchorData = _myAnchorsData.AddAnchorData(CstrNullAnchor, CstrNullAnchor);
			InitAnchorButton(toolStripAnchors, theAnchorData);

			// indicate that we've changed something so that we don't exit without offering
			//  to save.
			theSE.Modified = true;
		}

		private void contextMenuStripAnchorOptions_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			insertNullAnchorToolStripMenuItem.Visible = (_myAnchorsData.Count == 0);

			// the only function of the button here is to add a slot to type a con note
			StoryEditor theSE;
			if (!CheckForProperEditToken(out theSE) || (theSE.LoggedOnMember == null))
				return;

			addConsultantCoachNoteOnThisAnchorToolStripMenuItem.Visible =
				TeamMemberData.IsUser(theSE.LoggedOnMember.MemberType,
									  TeamMemberData.UserTypes.AnyEditor);
		}
	}
}
