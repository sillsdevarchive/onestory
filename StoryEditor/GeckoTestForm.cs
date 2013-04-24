using System;
using System.Text;
using System.Windows.Forms;
using Gecko.Windows;
using System.IO;

namespace OneStoryProjectEditor
{
	public partial class GeckoTestForm : Form
	{
		private readonly StoryEditor _theSe;
		private readonly StoryData _storyData;

		public GeckoTestForm(StoryEditor theSe, StoryData storyData)
		{
			_theSe = theSe;
			_storyData = storyData;
			InitializeComponent();

			var strHtml = _storyData.PresentationHtml(_theSe.CurrentViewSettings,
													  _theSe.StoryProject.ProjSettings,
													  _theSe.StoryProject.TeamMembers,
													  null);

			var filePath = Path.Combine(Environment.CurrentDirectory, "Example6.html");
			// File.WriteAllText(filePath, strHtml.Replace("textarea", "input"), Encoding.UTF8);
			File.WriteAllText(filePath, strHtml, Encoding.UTF8);
			geckoWebBrowser.Navigate("file://" + filePath);
			// geckoWebBrowser.LoadHtml(strHtml);
		}
	}
}
