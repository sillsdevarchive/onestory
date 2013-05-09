using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetLoc;

namespace OneStoryProjectEditor
{
	public abstract class GeckoConNotesDisplayControl : GeckoDisplayControl
	{
		public override StoryData StoryData
		{
			set
			{
				System.Diagnostics.Debug.Assert((value == null) || (TheSe != null));
				base.StoryData = value;
				if (value == null)
					return;

				// for ConNotes, we also have to do the 'insure extra box' thingy
				//  (there are actually one more verses than 'Count', but DataConverter(i)
				//  handles that for us)
				for (int i = 0; i <= StoryData.Verses.Count; i++)
				{
					var aCNsDC = DataConverter(i);
					foreach (var dc in aCNsDC)
						aCNsDC.InsureExtraBox(dc, TheSe.TheCurrentStory,
								TheSe.LoggedOnMember, TheSe.StoryProject.TeamMembers);
				}
			}
		}

		public abstract string PaneLabel();
		public abstract ConsultNotesDataConverter DataConverter(int nVerseIndex);

		public void OnAddNote(int nVerseIndex, string strReferringText, string strNote, bool bNoteToSelf)
		{
#if ToDo
			throw new NotImplementedException();
#endif
		}
	}

	public class GeckoConsultantNotesControl : GeckoConNotesDisplayControl
	{
		public override void LoadDocument()
		{
			var strHtml = StoryData.ConsultantNotesHtml(this,
											TheSe.StoryProject.ProjSettings,
											TheSe.LoggedOnMember,
											TheSe.StoryProject.TeamMembers,
											TheSe.viewHiddenVersesMenu.Checked,
											TheSe.viewOnlyOpenConversationsMenu.Checked);

			NavigateToString(strHtml, "ConsultantNotesPane.html");
			LineNumberLink.Visible = true;
		}

		public override string PaneLabel()
		{
			return Localizer.Str("Consultant Notes");
		}

		public override ConsultNotesDataConverter DataConverter(int nVerseIndex)
		{
			var verse = GetVerseData(nVerseIndex);
			var aCNsDC = verse.ConsultantNotes;
			return aCNsDC;
		}

		public void OnVerseLineJump(int nVerseIndex)
		{
			TheSe.FocusOnVerse(nVerseIndex, false, true);
		}
	}

	public class GeckoCoachNotesControl : GeckoConNotesDisplayControl
	{
		public override void LoadDocument()
		{
			var strHtml = StoryData.CoachNotesHtml(this,
								TheSe.StoryProject.ProjSettings,
								TheSe.LoggedOnMember,
								TheSe.StoryProject.TeamMembers,
								TheSe.viewHiddenVersesMenu.Checked,
								TheSe.viewOnlyOpenConversationsMenu.Checked);

			NavigateToString(strHtml, "CoachNotesPane.html");
			LineNumberLink.Visible = true;
		}

		public override string PaneLabel()
		{
			return Localizer.Str("Coach Notes");
		}

		public override ConsultNotesDataConverter DataConverter(int nVerseIndex)
		{
			var verse = GetVerseData(nVerseIndex);
			var aCNsDC = verse.CoachNotes;
			return aCNsDC;
		}

		public void OnVerseLineJump(int nVerseIndex)
		{
			TheSe.FocusOnVerse(nVerseIndex, true, false);
		}
	}
}
