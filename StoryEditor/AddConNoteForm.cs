using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetLoc;

namespace OneStoryProjectEditor
{
	public partial class AddConNoteForm : Form
	{
		private AddConNoteForm()
		{
			InitializeComponent();
			Localizer.Ctrl(this);
		}

		public AddConNoteForm(Type typeConNotePane, string strHtmlNote)
		{
			InitializeComponent();
			Localizer.Ctrl(this);

			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			var pane = Activator.CreateInstance(typeConNotePane) as HtmlConNoteControl;
			System.Diagnostics.Debug.Assert(pane != null);
			pane.Dock = DockStyle.Fill;
			tableLayoutPanel.Controls.Add(pane, 0, 1);
			tableLayoutPanel.SetColumnSpan(pane, 2);
			textBoxConNotes.Text = pane.PaneLabel();
			pane.DocumentText = strHtmlNote;
			this.tableLayoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private void ButtonOkClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
