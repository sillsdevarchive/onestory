using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public partial class MultiLineControl : OneStoryProjectEditor.ResizableControl
	{
		public MultiLineControl(StoryStageLogic storyStageLogic, MultipleLineDataConverter aMLDC, List<string> astrTestors)
			: base(storyStageLogic)
		{
			InitializeComponent();

			tableLayoutPanel.SuspendLayout();
			SuspendLayout();

			System.Diagnostics.Debug.Assert(tableLayoutPanel.RowCount == 1, "otherwise, fix this assumption: RetellingsControl.cs.20");
			tableLayoutPanel.RemoveRow(1);  // remove the one default one we start with

			// finally populate the buttons on that tool strip
			System.Diagnostics.Debug.Assert(aMLDC.Count > 0);
			int nNumRows = 0;
			for (int i = 0; i < aMLDC.Count; i++)
			{
				StringTransfer strRowData = aMLDC[i];
				string strUnsGui = aMLDC.MemberIDs[i];
				System.Diagnostics.Debug.Assert(astrTestors.Contains(strUnsGui));
				int nTest = astrTestors.IndexOf(strUnsGui) + 1;
				InitRow(aMLDC.LabelTextFormat, strRowData, nTest, ref nNumRows);
			}

			tableLayoutPanel.ResumeLayout(false);
			ResumeLayout(false);
		}

		protected void InitRow(string strLabelTextFormat, StringTransfer strRowData, int nTest, ref int nNumRows)
		{
			Label label = new Label
							  {
								  Anchor = AnchorStyles.Left,
								  AutoSize = true,
								  Name = strLabelTextFormat + nTest,
								  Text = String.Format(strLabelTextFormat, nTest)
							  };

			CtrlTextBox tb = new CtrlTextBox(
				strLabelTextFormat + CstrSuffixTextBox + nTest, this, strRowData);

			// add the label and tool strip as a new row to the table layout panel
			int nLayoutRow = nNumRows++;
			InsertRow(nLayoutRow);
			tableLayoutPanel.Controls.Add(label, 0, nLayoutRow);
			tableLayoutPanel.Controls.Add(tb, 1, nLayoutRow);
		}
	}
}
