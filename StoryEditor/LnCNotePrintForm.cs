using System;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public partial class LnCNotePrintForm : Form
	{
		public LnCNotePrintForm(StoryEditor theSe)
		{
			InitializeComponent();
			printViewer.webBrowser.TheSe = theSe;
			printViewer.saveFileDialog.FileName = String.Format("{0} LnCNotes.html",
																theSe.StoryProject.ProjSettings.ProjectName);
			var strHtmlInner = theSe.StoryProject.LnCNotes.PresentationHtml;
			var strHtml = StoryData.AddHtmlHtmlDocOutside(strHtmlInner,
														  theSe.StoryProject.ProjSettings);
			printViewer.webBrowser.BrowserDisplay.LoadDocument(strHtml);
		}
	}
}
