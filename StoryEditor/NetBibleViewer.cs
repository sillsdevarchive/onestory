using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	[System.Runtime.InteropServices.ComVisible(true)]
	public partial class NetBibleViewer : UserControl
	{
		protected string m_strScriptureReference = "gen 1:1";

		#region "Sword stuff for NETfree pane"
		MarkupFilterMgr filterManager;
		SWMgr manager;
		SWModule moduleVersion = null;
		NetBibleFootnoteTooltip tooltipNBFNs = null;
		String m_strBookName, m_strChapterNumber, m_strVerseNumber;

		string verseLineBreak = "<br />";
		string preDocumentDOMScript = "<script>" +
			"function OpenHoverWindow(link)" +
			"{" +
			//"  alert(link.getAttribute(\"href\").substr(6,link.length));" +
			"  window.external.ShowHoverOver(link.getAttribute(\"href\").substr(6,link.length));" +
			"  return false;" +
			"}" +
			"" +
			"function CloseHoverWindow()" +
			"{" +
			"  window.external.HideHoverOver();" +
			"  return false;" +
			"}" +
			"" +
			"function DoScriptureRefDragSource(button)" +
			"{" +
			"  window.external.OnDoScriptureRefDragSource(button.getAttribute(\"value\"));" +
			"  return false;" +
			"}" +
			"</script>";

		string postDocumentDOMScript = "<script>" +
			"var links = document.getElementsByTagName(\"a\");" +
			"for (var i=0; i < links.length; i++)" +
			"{" +
			"  links[i].onclick = function(){return OpenHoverWindow(this);};" +
			"  links[i].onmouseover = function(){return OpenHoverWindow(this);};" +
			"  links[i].onmouseout = function(){CloseHoverWindow();};" +
			"}" +
			"var buttons = document.getElementsByTagName(\"button\");" +
			"for (var i=0; i < buttons.length; i++)" +
			"{" +
			"  buttons[i].onmousedown = function(){return DoScriptureRefDragSource(this);};" +
			"}" +
			"</script>";

		protected static char[] achAnchorDelimiters = new char[] { ' ', ':' };
		#endregion

		public NetBibleViewer()
		{
			InitializeComponent();
		}

		public void InitNetBibleViewer()
		{
			InitializeSword();
			DisplayVerses(ScriptureReference);
		}

		public string ScriptureReference
		{
			get { return m_strScriptureReference; }
			set { m_strScriptureReference = value; }
		}

		protected void InitializeSword()
		{
			// Initialize Module Variables
			filterManager = new MarkupFilterMgr((char)Sword.FMT_HTMLHREF, (char)Sword.ENC_HTML);

			/* NOTE: GC.SuppressFinalize(filterManager);
			 *  This must be placed here so the garbage collector (GC) doesn't try to clean up
			 * something that was already cleaned up.  If this is not left in an error will
			 * occur when the application closes.  This happens because when the SWMgr is
			 * cleaned by the GC it cleans its own filter and removes it from memory.  When
			 * the GC then tries to clean up the filterManager object it doesn't really
			 * exist in memory anymore and therefore it throws an exception saying some
			 * memory is probably corrupt because this object points to trash in memory.
			 * -Richard Parsons 11-21-2006
			 */
			GC.SuppressFinalize(filterManager);

			manager = new SWMgr(filterManager);
			foreach (string strPath in GetModuleLocations())
				manager.augmentModules(strPath);

			moduleVersion = manager.getModule("NETfree");
			if (moduleVersion == null)
			{
#if DEBUG
				string strModules = null;
				int numOfBibles = (int)manager.getModules().size();
				for (int i = 0; i < numOfBibles; i++)
					if (manager.getModuleAt(i).Type().Equals("Biblical Texts")) //Limit to just bibles, comment out to see all modules
						strModules += manager.getModuleAt(i).Name() + '\n';
				MessageBox.Show(String.Format("Found modules:{0}{0}{1}", Environment.NewLine, strModules), StoryEditor.cstrCaption);
#else
				throw new ApplicationException(String.Format("Can't find Sword module '{0}'. Is Sword installed?", "NETfree"));
#endif
			}

			// Setup the active module
			// Word of Christ in red
			manager.setGlobalOption("Words of Christ in Red", "On");

			//Footnotes
			manager.setGlobalOption("Footnotes", "On");

			/* NOTE: This is needed so the DOM Script I'm using for strongs numbers,
			 * morph, and footnote tags will work.  This basicly allows the webbrowser
			 * control to talk to my form control using DOM Script using the command
			 * window.external.<the public method from this form>;
			 * -Richard Parsons 01-31-2007
			 */
			webBrowserNetBible.ObjectForScripting = this;
			DisplayVerses(ScriptureReference);
		}

		protected const string cstrHtmlLineFormat = "<button type=\"button\">{0}</button>{1}<br />";

		// the anchor comes in as, for example, "gen 1:1"
		public void DisplayVerses(string strScriptureReference)
		{
			string[] aStrTokens = strScriptureReference.Split(achAnchorDelimiters);
			if (aStrTokens.Length != 3)
				throw new ApplicationException(String.Format("Ill-formed Anchor: '{0}'", strScriptureReference));

			if ((aStrTokens[0] != m_strBookName) || (aStrTokens[1] != m_strChapterNumber))
			{
				// change books or chapters. We just get a whole chapters worth
				String strReference = String.Format("{0} {1}:{2}", aStrTokens[0], aStrTokens[1], aStrTokens[2]);
				VerseKey verseKey = new VerseKey(strReference);
				int chapter = verseKey.Chapter();
				int book = verseKey.Book();

				//Display the document
				StringBuilder sb = new StringBuilder();
				while (verseKey.Chapter() == chapter && verseKey.Book() == book && verseKey.Error() == '\0')
				{
					string strVerseHtml = moduleVersion.RenderText(verseKey).Replace(verseLineBreak, null);
					string strLineHtml = String.Format(cstrHtmlLineFormat, verseKey.getShortText(), strVerseHtml);
					sb.Append(strLineHtml);
					verseKey.Verse(verseKey.Verse() + 1);
				}

				webBrowserNetBible.DocumentText = preDocumentDOMScript + sb.ToString() + postDocumentDOMScript;

				// initialize the combo boxes for this new situation
				if (aStrTokens[0] != m_strBookName)
				{
					this.toolStripComboBoxBookName.SelectedItem = verseKey.getBookAbbrev();
					m_strBookName = aStrTokens[0];

					this.toolStripComboBoxChapterNumber.Items.Clear();
					int nChapters = verseKey.chapterCount(verseKey.Testament(), book);
					for (int i = 1; i <= nChapters; i++)
						toolStripComboBoxChapterNumber.Items.Add(i.ToString());

					// if the book changes, then the chapter number changes by default
					m_strChapterNumber = null;
				}

				if (aStrTokens[1] != m_strChapterNumber)
				{
					toolStripComboBoxChapterNumber.SelectedItem = chapter.ToString();
					m_strChapterNumber = aStrTokens[1];
					this.toolStripComboBoxVerseNumber.Items.Clear();
					int nVerses = verseKey.verseCount(verseKey.Testament(), book, chapter);
					for (int i = 1; i <= nVerses; i++)
						toolStripComboBoxVerseNumber.Items.Add(i.ToString());
				}
			}

			toolStripComboBoxVerseNumber.SelectedItem = m_strVerseNumber = aStrTokens[2];
			// TODO: make the verse the user requested come forward
		}

		public void OnDoScriptureRefDragSource(string strScriptureReference)
		{
			Console.WriteLine("OnDoScriptureRefDragSource: " + strScriptureReference);
			ScriptureReference = strScriptureReference;
			webBrowserNetBible.DoDragDrop(this, DragDropEffects.Link);
		}

		public void ShowHoverOver(string s)
		{
			if (tooltipNBFNs != null)
			{
				if (tooltipNBFNs.Tag.Equals(s))
					return; //leave if we are already displaying this tool tip

				//if there is a different tooltip showing destroy it
				tooltipNBFNs.Dispose();
				tooltipNBFNs = null;
			}
			// Point ptTooltip = new Point(Cursor.Position.X - ClientRectangle.Left, Cursor.Position.Y - ClientRectangle.Top);
			Point ptTooltip = Cursor.Position;
			ptTooltip.Offset(-ClientRectangle.Location.X + 20, -ClientRectangle.Location.Y - 20);
			tooltipNBFNs = new NetBibleFootnoteTooltip(s, ptTooltip);
			tooltipNBFNs.Show();
		}

		public void HideHoverOver()
		{
			if (tooltipNBFNs != null)
			{
				//if there is a different tooltip showing destroy it
				tooltipNBFNs.Dispose();
				tooltipNBFNs = null;
			}
		}

		protected static string[] GetModuleLocations()
		{
			string strSwordProjectPath = Environment.GetEnvironmentVariable("SWORD_PATH");
			if (String.IsNullOrEmpty(strSwordProjectPath))
				strSwordProjectPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\CrossWire\The SWORD Project";
			return new string[] { strSwordProjectPath };
		}

		private void toolStripComboBoxBookName_SelectedIndexChanged(object sender, EventArgs e)
		{
			// disable the prev book button if we're on Genesis and the next book button if on Revelation
			ToolStripComboBox theSender = (ToolStripComboBox)sender;
			toolStripComboBox_SelectedIndexChanged(theSender, this.toolStripButtonPrevBook, this.toolStripButtonNextBook);
		}

		private void toolStripComboBoxChapterNumber_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToolStripComboBox theSender = (ToolStripComboBox)sender;
			toolStripComboBox_SelectedIndexChanged(theSender, this.toolStripButtonPrevChap, this.toolStripButtonNextChap);
		}

		private void toolStripComboBoxVerseNumber_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToolStripComboBox theSender = (ToolStripComboBox)sender;
			toolStripComboBox_SelectedIndexChanged(theSender, this.toolStripButtonPrevVerse, this.toolStripButtonNextVerse);
		}

		protected void toolStripPrevButton_Click(ToolStripButton theSender, ToolStripComboBox theComboBox)
		{
			if (theComboBox.SelectedIndex == 0)
				theComboBox.SelectedIndex = (theComboBox.Items.Count - 1);
			else
				theComboBox.SelectedIndex--;
		}

		protected void toolStripNextButton_Click(ToolStripButton theSender, ToolStripComboBox theComboBox)
		{
			if (theComboBox.SelectedIndex == (theComboBox.Items.Count - 1))
				theComboBox.SelectedIndex = 0;
			else
				theComboBox.SelectedIndex++;
		}

		private void toolStripButtonPrevBook_Click(object sender, EventArgs e)
		{
			toolStripPrevButton_Click((ToolStripButton)sender, toolStripComboBoxBookName);
		}

		private void toolStripButtonPrevChap_Click(object sender, EventArgs e)
		{
			toolStripPrevButton_Click((ToolStripButton)sender, toolStripComboBoxChapterNumber);
		}

		private void toolStripButtonPrevVerse_Click(object sender, EventArgs e)
		{
			toolStripPrevButton_Click((ToolStripButton)sender, toolStripComboBoxVerseNumber);
		}

		private void toolStripButtonNextBook_Click(object sender, EventArgs e)
		{
			toolStripNextButton_Click((ToolStripButton)sender, toolStripComboBoxBookName);
		}

		private void toolStripButtonNextChap_Click(object sender, EventArgs e)
		{
			toolStripNextButton_Click((ToolStripButton)sender, toolStripComboBoxChapterNumber);
		}

		private void toolStripButtonNextVerse_Click(object sender, EventArgs e)
		{
			toolStripNextButton_Click((ToolStripButton)sender, toolStripComboBoxVerseNumber);
		}

		protected void toolStripComboBox_SelectedIndexChanged(ToolStripComboBox theSender, ToolStripButton buttonPrev, ToolStripButton buttonNext)
		{
			buttonPrev.Enabled = theSender.SelectedIndex > 0;
			buttonNext.Enabled = theSender.SelectedIndex < (theSender.Items.Count - 1);
		}
	}
}
