using System.Drawing;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public abstract partial class WebBrowserAdaptor : UserControl
	{
		public enum WhichBrowser
		{
			Undefined = 0,
			InternetExplorer,
			GeckoFx
		}

		protected WhichBrowser _whichBrowser { get; set; }

		protected GeckoDisplayControl GeckoWebBrowser { get; set; }
		protected HtmlVerseControl IeWebBrowser { get; set; }

		protected WebBrowserAdaptor()
		{
			InitializeComponent();
			SetBrowserToUse(ShouldUseGecko);
		}

		public void SetBrowserToUse(bool shouldUseGecko)
		{
			// on linux, only Gecko works (on Windows, either Gecko or IE will work, but using IE saves us from having to redistribute too much)
			//  so on Linux, prefer Gecko, but on Windows, prefer IE.
			if (shouldUseGecko)
			{
				// try to initialize
				// if GeckoFx was successfully initialized, then use it
				if (GeckoFxInitializer.SetUpXulRunner())
				{
					_whichBrowser = WhichBrowser.GeckoFx;
					GeckoWebBrowser = MyGeckoBrowser;

					Controls.Add(GeckoWebBrowser);

					IeWebBrowser = null;    // in case it was being used
				}
				else
				{
					Controls.Add(GeckoFxInitializer.InstructionsLinkLabel);
				}
			}
			else
			{
				_whichBrowser = WhichBrowser.InternetExplorer;
				IeWebBrowser = MyIeBrowser;
				Controls.Add(IeWebBrowser);

				GeckoWebBrowser = null; // in case it was being used
			}
		}

		protected abstract HtmlVerseControl MyIeBrowser { get; }
		protected abstract GeckoDisplayControl MyGeckoBrowser { get; }

		private static bool ShouldUseGecko
		{
			get
			{
				string xulRunnerPath;
				return GeckoFxInitializer.DoesXulRunnerFolderExist(out xulRunnerPath);
			}
		}
	}

	public class WebBrowserAdaptorStoryBt : WebBrowserAdaptor
	{
		public IWebBrowserDisplayStoryBt Browser
		{
			get
			{
				return (_whichBrowser == WhichBrowser.InternetExplorer)
						   ? (IWebBrowserDisplayStoryBt) IeWebBrowser
						   : (IWebBrowserDisplayStoryBt) GeckoWebBrowser;
			}
		}


		#region Overrides of WebBrowserAdaptor

		protected override HtmlVerseControl MyIeBrowser
		{
			get
			{
				return new HtmlStoryBtControl
				{
					AllowWebBrowserDrop = false,
					Dock = DockStyle.Fill,
					IsWebBrowserContextMenuEnabled = false,
					Location = new Point(0, 23),
					MinimumSize = new Size(20, 20),
					Name = "htmlStoryBtControl",
					ParentStory = null,
					Size = new Size(451, 366),
					StoryData = null,
					TabIndex = 5,
					TheSe = null,
					ViewSettings = null
				};
			}
		}

		protected override GeckoDisplayControl MyGeckoBrowser
		{
			get
			{
				return new GeckoStoryBtDisplayControl
						   {
							   DisableWmImeSetContext = false,
							   Dock = DockStyle.Fill,
							   Location = new Point(0, 23),
							   Name = "geckoStoryBtDisplay",
							   ParentStory = null,
							   Size = new Size(451, 366),
							   StoryData = null,
							   TabIndex = 0,
							   TheSe = null,
							   UseHttpActivityObserver = false,
							   ViewSettings = null,
						   };
			}
		}

		#endregion
	}

	public abstract class WebBrowserAdaptorConNote : WebBrowserAdaptor
	{
		public IWebBrowserDisplayConNote Browser
		{
			get
			{
				return (_whichBrowser == WhichBrowser.InternetExplorer)
						   ? (IWebBrowserDisplayConNote) IeWebBrowser
						   : (IWebBrowserDisplayConNote) GeckoWebBrowser;
			}
		}
	}

	public class WebBrowserAdaptorConsultantNotes : WebBrowserAdaptorConNote
	{
		#region Overrides of WebBrowserAdaptor

		protected override HtmlVerseControl MyIeBrowser
		{
			get
			{
				return new HtmlConsultantNotesControl
						   {
							   AllowWebBrowserDrop = false,
							   Dock = DockStyle.Fill,
							   IsWebBrowserContextMenuEnabled = false,
							   Location = new Point(0, 23),
							   MinimumSize = new Size(20, 20),
							   Name = "htmlConsultantNotesControl",
							   Size = new Size(422, 331),
							   StoryData = null,
							   TabIndex = 2,
							   TheSe = null
						   };
			}
		}

		protected override GeckoDisplayControl MyGeckoBrowser
		{
			get
			{
				return new GeckoConsultantNotesControl
						   {
							   DisableWmImeSetContext = false,
							   Dock = System.Windows.Forms.DockStyle.Fill,
							   Location = new System.Drawing.Point(0, 23),
							   Name = "geckoConsultantNotesControl",
							   Size = new System.Drawing.Size(422, 331),
							   StoryData = null,
							   TabIndex = 4,
							   TheSe = null,
							   UseHttpActivityObserver = false
						   };
			}
		}

		#endregion
	}

	public class WebBrowserAdaptorCoachNotes : WebBrowserAdaptorConNote
	{
		#region Overrides of WebBrowserAdaptor

		protected override HtmlVerseControl MyIeBrowser
		{
			get
			{
				return new HtmlCoachNotesControl
						   {
							   AllowWebBrowserDrop = false,
							   Dock = DockStyle.Fill,
							   IsWebBrowserContextMenuEnabled = false,
							   Location = new Point(0, 23),
							   MinimumSize = new Size(20, 20),
							   Name = "htmlCoachNotesControl",
							   Size = new Size(422, 228),
							   StoryData = null,
							   TabIndex = 3,
							   TheSe = null
						   };
			}
		}

		protected override GeckoDisplayControl MyGeckoBrowser
		{
			get
			{
				return new GeckoCoachNotesControl
						   {
							   DisableWmImeSetContext = false,
							   Dock = System.Windows.Forms.DockStyle.Fill,
							   Location = new System.Drawing.Point(0, 23),
							   Name = "geckoCoachNotesControl",
							   Size = new System.Drawing.Size(422, 228),
							   StoryData = null,
							   TabIndex = 5,
							   TheSe = null,
							   UseHttpActivityObserver = false
						   };
			}
		}

		#endregion
	}
}
