using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public partial class AnchorControl : ResizableControl
	{
		protected const string CstrFieldNameAnchor = "AnchorButton";
		protected const string CstrFieldNameExegeticalHelp = "ExegeticalHelp";
		protected const string CstrFieldNameExegeticalHelpLabel = "ExegeticalHelpLabel";
		protected int m_nNumRows = 1;
		protected AnchorsData _myAnchorsData = null;
		protected Dictionary<ToolStripButton, List<TextBox>> _mapAnchorsToTextBoxes = new Dictionary<ToolStripButton, List<TextBox>>();

		public AnchorControl(AnchorsData anAnchorsData)
		{
			_myAnchorsData = anAnchorsData;
			InitializeComponent();

			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();

			this.tableLayoutPanel.Controls.Add(this.labelAnchor, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.toolStripAnchors, 1, 0);

			// add the label and tool strip as a new row to the table layout panel
			// finally populate the buttons on that tool strip
			foreach (AnchorData anAnchorData in anAnchorsData)
			{
				ToolStripButton theAnchorButton = InitAnchorButton(toolStripAnchors, anAnchorData);

				if (anAnchorData.ExegeticalHelpNotes.Count > 0)
					InitExegeticalHelpsRow(theAnchorButton, anAnchorData.ExegeticalHelpNotes, ref m_nNumRows);
			}

			this.tableLayoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		protected ToolStripButton InitAnchorButton(ToolStrip ts, AnchorData theAnchorData)
		{
			ToolStripButton aButton = new ToolStripButton();
			aButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			aButton.Tag = theAnchorData;
			aButton.Name = CstrFieldNameAnchor + theAnchorData.JumpTarget;
			aButton.AutoSize = true;
			aButton.Text = theAnchorData.JumpTarget;
			aButton.ToolTipText = theAnchorData.ToolTipText;
			aButton.Click += new EventHandler(aButton_Click);
			aButton.MouseDown += new MouseEventHandler(aButton_MouseDown);
			ts.Items.Add(aButton);
			return aButton;
		}

		protected ToolStripButton m_theLastButtonClicked = null;

		void aButton_MouseDown(object sender, MouseEventArgs e)
		{
			System.Diagnostics.Debug.Assert(sender is ToolStripButton);
			if (e.Button == MouseButtons.Right)
				m_theLastButtonClicked = (ToolStripButton)sender;
		}

		/*
		protected void InitAnchorButton(ToolStrip ts, string strJumpTarget, string strComment)
		{
			ToolStripSplitButton tssb = new ToolStripSplitButton();
			tssb.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			tssb.DropDown = dropDownAnchorButtonOptions;
			tssb.Name = CstrFieldNameAnchor + strJumpTarget;
			tssb.AutoSize = true;
			tssb.Text = strJumpTarget;
			tssb.ToolTipText = strComment;
			tssb.Click += new EventHandler(aButton_Click);
			ts.Items.Add(tssb);
		}
		*/

		void aButton_Click(object sender, EventArgs e)
		{
			Form form = this.FindForm();
			if (form is StoryEditor)
			{
				StoryEditor aSE = (StoryEditor)form;
				ToolStripButton tssb = (ToolStripButton)sender;
				aSE.SetNetBibleVerse(tssb.Text);
			}
		}

		protected void SetExegeticalHelpControls(ToolStripButton theAnchorButton, StringTransfer strQuote, ref int nNumRows)
		{
			int nLayoutRow = nNumRows++;

			Label labelExegeticalHelp = new Label();
			labelExegeticalHelp.Anchor = System.Windows.Forms.AnchorStyles.Left;
			labelExegeticalHelp.AutoSize = true;
			labelExegeticalHelp.Name = CstrFieldNameExegeticalHelpLabel + nLayoutRow.ToString();
			labelExegeticalHelp.Text = "cn:";

			TextBox tb = new TextBox();
			tb.Name = CstrFieldNameExegeticalHelp + nLayoutRow.ToString();
			tb.Multiline = true;
			tb.Dock = DockStyle.Fill;
			strQuote.SetAssociation(tb);    // tb.Text = strQuote;
			tb.TextChanged += new EventHandler(textBox_TextChanged);

			// add the label and tool strip as a new row to the table layout panel
			InsertRow(nLayoutRow);
			tableLayoutPanel.Controls.Add(labelExegeticalHelp, 0, nLayoutRow);
			tableLayoutPanel.Controls.Add(tb, 1, nLayoutRow);

			// add this row to the list of exe help lines added for this anchor button (so
			//  we can gracefully remove them if the anchor is deleted.
			List<TextBox> lstTBs;
			if (!_mapAnchorsToTextBoxes.TryGetValue(theAnchorButton, out lstTBs))
			{
				lstTBs = new List<TextBox>();
				_mapAnchorsToTextBoxes.Add(theAnchorButton, lstTBs);
			}

			lstTBs.Add(tb);
		}

		protected void InitExegeticalHelpsRow(ToolStripButton theAnchorButton, ExegeticalHelpNotesData anExHelpsNoteData, ref int nNumRows)
		{
			foreach (ExegeticalHelpNoteData anExHelpNoteData in anExHelpsNoteData)
				SetExegeticalHelpControls(theAnchorButton, anExHelpNoteData.ExegeticalHelpNote, ref nNumRows);
		}

		private void toolStripAnchors_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(NetBibleViewer)))
			{
				NetBibleViewer theNetBibleViewer = (NetBibleViewer)e.Data.GetData(typeof(NetBibleViewer));
				foreach (ToolStripButton btn in toolStripAnchors.Items)
					if (btn.Text == theNetBibleViewer.ScriptureReference)
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
				NetBibleViewer theNetBibleViewer = (NetBibleViewer)e.Data.GetData(typeof(NetBibleViewer));
				AnchorData theAnchorData = _myAnchorsData.AddAnchorData(theNetBibleViewer.ScriptureReference);
				this.InitAnchorButton(toolStripAnchors, theAnchorData);
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				if (_mapAnchorsToTextBoxes.ContainsKey(m_theLastButtonClicked))
				{
					DialogResult res = MessageBox.Show(String.Format("The anchor you are about to delete has exegetical or cultural note(s) attached to it. These will be deleted also. Click 'OK' to continue with the deletion.{0}{0}[if you would rather have kept them, say associated to another anchor, then tell bob_eaton@sall.com and he may implement that feature. For now, you can copy the note and paste it into a new note added to a new or existing anchor (right-click on the anchor and choose 'Add Exegetical/Cultural Note'). Then come back here and delete this anchor]", Environment.NewLine), StoryEditor.CstrCaption, MessageBoxButtons.OKCancel);
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

				toolStripAnchors.Items.RemoveByKey(m_theLastButtonClicked.Name);
				System.Diagnostics.Debug.Assert((m_theLastButtonClicked.Tag != null) && (m_theLastButtonClicked.Tag is AnchorData));
				AnchorData theAnchorData = (AnchorData)m_theLastButtonClicked.Tag;
				_myAnchorsData.Remove(theAnchorData);
				m_theLastButtonClicked = null;
			}
			else
				MessageBox.Show("Right-click on one of the buttons to choose which one to delete", StoryEditor.CstrCaption);
		}

		private void addCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				AnchorAddCommentForm dlg = new AnchorAddCommentForm(m_theLastButtonClicked.Text, m_theLastButtonClicked.ToolTipText);
				DialogResult res = dlg.ShowDialog();
				if ((res == DialogResult.OK) || (res == DialogResult.Yes))
				{
					System.Diagnostics.Debug.Assert((m_theLastButtonClicked.Tag != null) && (m_theLastButtonClicked.Tag is AnchorData));
					AnchorData theAnchorData = (AnchorData)m_theLastButtonClicked.Tag;
					theAnchorData.ToolTipText = m_theLastButtonClicked.ToolTipText = dlg.CommentText;
					if (res == DialogResult.Yes)
					{
						addExegeticalCulturalNoteToolStripMenuItem_Click(null, null);
					}
				}
			}
			else
				MessageBox.Show("Right-click on one of the buttons to choose which one to add the comment to", StoryEditor.CstrCaption);
		}

		private void toolStripAnchors_MouseDown(object sender, MouseEventArgs e)
		{
			m_theLastButtonClicked = null;
		}

		private void addExegeticalCulturalNoteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				System.Diagnostics.Debug.Assert(m_theLastButtonClicked.Tag is AnchorData);
				AnchorData theAnchorData = (AnchorData)m_theLastButtonClicked.Tag;
				ExegeticalHelpNoteData anEHN = theAnchorData.ExegeticalHelpNotes.AddExegeticalHelpNote("Re: " + m_theLastButtonClicked.Text);
				SetExegeticalHelpControls(m_theLastButtonClicked, anEHN.ExegeticalHelpNote, ref m_nNumRows);
				AdjustHeightWithSuspendLayout(null);
			}
			else
				MessageBox.Show("Right-click on one of the buttons to choose which one to add the exegetical or cultural note to", StoryEditor.CstrCaption);
		}
	}
}
