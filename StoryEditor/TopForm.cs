using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public partial class TopForm : Form
	{
		public TopForm()
		{
			InitializeComponent();
		}

		private void TopForm_Load(object sender, EventArgs e)
		{
			Top = 0;
			StoryEditor.SuspendSaveDialog++;
		}

		private void TopForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			StoryEditor.SuspendSaveDialog--;
		}
	}
}
