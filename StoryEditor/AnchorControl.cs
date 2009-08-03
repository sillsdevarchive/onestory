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
		protected const string cstrFieldNameAnchor = "AnchorButton";
		protected const string cstrFieldNameExegeticalHelp = "ExegeticalHelp";
		protected const string cstrFieldNameExegeticalHelpLabel = "ExegeticalHelpLabel";
		protected int m_nNumRows = 1;

		public AnchorControl(StoryProject.anchorsRow anAnchorsRow)
		{
			InitializeComponent();

			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();

			this.tableLayoutPanel.Controls.Add(this.labelAnchor, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.toolStripAnchors, 1, 0);

			// add the label and tool strip as a new row to the table layout panel
			// finally populate the buttons on that tool strip
			foreach (StoryProject.anchorRow anAnchorRow in anAnchorsRow.GetanchorRows())
			{
				ToolStripButton theAnchorButton = InitAnchorButton(toolStripAnchors, anAnchorRow.jumpTarget, anAnchorRow.text);
				InitExegeticalHelpsRow(theAnchorButton, anAnchorRow, ref m_nNumRows);
			}

			this.tableLayoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		protected ToolStripButton InitAnchorButton(ToolStrip ts, string strJumpTarget, string strComment)
		{
			ToolStripButton aButton = new ToolStripButton();
			aButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			aButton.Name = cstrFieldNameAnchor + strJumpTarget;
			aButton.AutoSize = true;
			aButton.Text = strJumpTarget;
			aButton.ToolTipText = strComment;
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
			tssb.Name = cstrFieldNameAnchor + strJumpTarget;
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

		protected void InitExegeticalHelpRows(ToolStripButton theAnchorButton, StoryProject.exegeticalHelpsRow anEHsRow, ref int nNumRows)
		{
			StoryProject.exegeticalHelpRow[] aEHRows = anEHsRow.GetexegeticalHelpRows();
			System.Diagnostics.Debug.Assert(aEHRows != null);
			for (int i = 0; i < aEHRows.Length; i++)
			{
				StoryProject.exegeticalHelpRow aEHRow = aEHRows[i];
				SetExegeticalHelpControls(theAnchorButton, aEHRow.quote, ref nNumRows);
			}
		}

		protected void SetExegeticalHelpControls(ToolStripButton theAnchorButton, string strQuote, ref int nNumRows)
		{
			int nLayoutRow = nNumRows++;

			Label labelExegeticalHelp = new Label();
			labelExegeticalHelp.Anchor = System.Windows.Forms.AnchorStyles.Left;
			labelExegeticalHelp.AutoSize = true;
			labelExegeticalHelp.Name = cstrFieldNameExegeticalHelpLabel + nLayoutRow.ToString();
			labelExegeticalHelp.Text = "cn:";

			TextBox tb = new TextBox();
			tb.Name = cstrFieldNameExegeticalHelp + nLayoutRow.ToString();
			tb.Multiline = true;
			tb.Dock = DockStyle.Fill;
			tb.Text = strQuote;
			tb.TextChanged += new EventHandler(textBox_TextChanged);

			// add the label and tool strip as a new row to the table layout panel
			InsertRow(nLayoutRow);
			tableLayoutPanel.Controls.Add(labelExegeticalHelp, 0, nLayoutRow);
			tableLayoutPanel.Controls.Add(tb, 1, nLayoutRow);

			// add this row to the list of exe help lines added for this anchor button (so
			//  we can gracefully remove them if the anchor is deleted.
			List<TextBox> lstTBs;
			if (theAnchorButton.Tag == null)
			{
				lstTBs = new List<TextBox>();
				theAnchorButton.Tag = lstTBs;
			}
			else
				lstTBs = (List<TextBox>)theAnchorButton.Tag;

			lstTBs.Add(tb);
		}

		protected void InitExegeticalHelpsRow(ToolStripButton theAnchorButton, StoryProject.anchorRow anAnchorRow, ref int nNumRows)
		{
			StoryProject.exegeticalHelpsRow[] anEHsRow = anAnchorRow.GetexegeticalHelpsRows();
			if ((anEHsRow != null) && (anEHsRow.Length > 0))
				InitExegeticalHelpRows(theAnchorButton, anEHsRow[0], ref nNumRows);
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
				this.InitAnchorButton(toolStripAnchors, theNetBibleViewer.ScriptureReference, theNetBibleViewer.ScriptureReference);
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				if (m_theLastButtonClicked.Tag != null)
				{
					DialogResult res = MessageBox.Show(String.Format("The anchor you are about to delete has exegetical or cultural note(s) attached to it. These will be deleted also. Click 'OK' to continue with the deletion.{0}{0}[if you would rather have kept them, say associated to another anchor, then tell bob_eaton@sall.com and he may implement that feature. For now, you can copy the note and paste it into a new note added to a new or existing anchor (right-click on the anchor and choose 'Add Exegetical/Cultural Note'). Then come back here and delete this anchor]", Environment.NewLine), StoryEditor.cstrCaption, MessageBoxButtons.OKCancel);
					if (res != DialogResult.OK)
						return;

					List<TextBox> lstTextBoxes = (List<TextBox>)m_theLastButtonClicked.Tag;
					foreach (TextBox tb in lstTextBoxes)
					{
						int nRowIndex = tableLayoutPanel.GetRow(tb);
						RemoveRow(nRowIndex);
						m_nNumRows--;   // I think it's enough to just decrement these counts
					}
					m_theLastButtonClicked.Tag = null;  // I think these will be cleaned up now
					AdjustHeightWithSuspendLayout(null);
				}

				toolStripAnchors.Items.RemoveByKey(m_theLastButtonClicked.Name);
				m_theLastButtonClicked = null;
			}
			else
				MessageBox.Show("Right-click on one of the buttons to choose which one to delete", StoryEditor.cstrCaption);
		}

		private void addCommentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				AnchorAddCommentForm dlg = new AnchorAddCommentForm(m_theLastButtonClicked.Text, m_theLastButtonClicked.ToolTipText);
				DialogResult res = dlg.ShowDialog();
				if ((res == DialogResult.OK) || (res == DialogResult.Yes))
				{
					m_theLastButtonClicked.ToolTipText = dlg.CommentText;
					if (res == DialogResult.Yes)
					{
						addExegeticalCulturalNoteToolStripMenuItem_Click(null, null);
					}
				}
			}
			else
				MessageBox.Show("Right-click on one of the buttons to choose which one to add the comment to", StoryEditor.cstrCaption);
		}

		private void toolStripAnchors_MouseDown(object sender, MouseEventArgs e)
		{
			m_theLastButtonClicked = null;
		}

		private void addExegeticalCulturalNoteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_theLastButtonClicked != null)
			{
				SetExegeticalHelpControls(m_theLastButtonClicked, "Re: " + m_theLastButtonClicked.Text, ref m_nNumRows);
				AdjustHeightWithSuspendLayout(null);
			}
			else
				MessageBox.Show("Right-click on one of the buttons to choose which one to add the exegetical or cultural note to", StoryEditor.cstrCaption);
		}
	}
}
