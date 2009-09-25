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
	public partial class ViewSwordOptionsForm : Form
	{
		protected const string CstrSwordLink = "www.crosswire.org/sword/modules/ModDisp.jsp?modType=Bibles";

		protected List<NetBibleViewer.SwordResource> _lstBibleResources;
		protected int _nIndexOfNetBible = -1;
		protected bool _bInCtor;
		public ViewSwordOptionsForm(ref List<NetBibleViewer.SwordResource> lstBibleResources)
		{
			_lstBibleResources = lstBibleResources;

			InitializeComponent();

			_bInCtor = true;
			for (int i = 0; i < lstBibleResources.Count; i++)
			{
				NetBibleViewer.SwordResource aSR = lstBibleResources[i];
				string strName = aSR.Name;
				string strDesc = aSR.Description;
				if (aSR.Name == "NET")
					_nIndexOfNetBible = i;
				checkedListBoxSwordBibles.Items.Add(String.Format("{0}: {1}", strName, strDesc), aSR.Loaded);
			}

			linkLabelLinkToSword.Links.Add(6, 4, CstrSwordLink);
			_bInCtor = false;
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < checkedListBoxSwordBibles.Items.Count; i++)
				_lstBibleResources[i].Loaded = checkedListBoxSwordBibles.GetItemChecked(i);

			DialogResult = DialogResult.OK;
			Close();
		}

		private void linkLabelLinkToSword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string strSwordLink = (string)e.Link.LinkData;
			if (!String.IsNullOrEmpty(strSwordLink))
				System.Diagnostics.Process.Start(strSwordLink);
		}

		private void checkedListBoxSwordBibles_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if ((e.Index == _nIndexOfNetBible)
				&& (e.CurrentValue == CheckState.Unchecked)
				&& (e.NewValue == CheckState.Checked)
				&& !_bInCtor)
			{
				string strNetKey = Microsoft.VisualBasic.Interaction.InputBox("Visit 'http://bible.org/downloads' or 'http://bible.org/sites/default/resources/assets/sword/' for details on how to acquire the key for the full 'Sword Premium Edition with 60,000+ notes' version and enter the unlock key here:",
					Properties.Resources.IDS_Caption, Properties.Settings.Default.NetBibleKey, 300, 200);
				if (!String.IsNullOrEmpty(strNetKey))
				{
					Properties.Settings.Default.NetBibleKey = strNetKey;
					Properties.Settings.Default.Save();
				}
				else
					e.NewValue = e.CurrentValue;
			}
		}
	}
}
