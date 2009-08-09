using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;

namespace OneStoryProjectEditor
{
	public class VerseData
	{
		public string guid = null;
		public StringTransfer VernacularText = null;
		public StringTransfer NationalBTText = null;
		public StringTransfer InternationalBTText = null;
		public AnchorsData Anchors = null;
		public TestQuestionsData TestQuestions = null;
		public RetellingsData Retellings = null;
		public ConsultantNotesData ConsultantNotes = null;
		public CoachNotesData CoachNotes = null;

		public VerseData(StoryProject.verseRow theVerseRow, StoryProject projFile)
		{
			guid = theVerseRow.guid;
			if (!theVerseRow.IsVernacularNull())
				VernacularText = new StringTransfer(theVerseRow.Vernacular);
			if (!theVerseRow.IsNationalBTNull())
				NationalBTText = new StringTransfer(theVerseRow.NationalBT);
			if (!theVerseRow.IsInternationalBTNull())
				InternationalBTText = new StringTransfer(theVerseRow.InternationalBT);

			Anchors = new AnchorsData(theVerseRow, projFile);
			TestQuestions = new TestQuestionsData(theVerseRow, projFile);
			Retellings = new RetellingsData(theVerseRow, projFile);
			ConsultantNotes = new ConsultantNotesData(theVerseRow, projFile);
			CoachNotes = new CoachNotesData(theVerseRow, projFile);
		}

		public VerseData()
		{
			guid = Guid.NewGuid().ToString();
			VernacularText = new StringTransfer(null);
			NationalBTText = new StringTransfer(null);
			InternationalBTText = new StringTransfer(null);
			Anchors = new AnchorsData();
			TestQuestions = new TestQuestionsData();
			Retellings = new RetellingsData();
			ConsultantNotes = new ConsultantNotesData();
			CoachNotes = new CoachNotesData();
		}

		public XElement GetXml
		{
			get
			{
				XElement elemVerse = new XElement(StoryEditor.ns + "verse", new XAttribute("guid", guid));
				if (VernacularText.HasData)
					elemVerse.Add(new XElement(StoryEditor.ns + "Vernacular", VernacularText));
				if (NationalBTText.HasData)
					elemVerse.Add(new XElement(StoryEditor.ns + "NationalBT", NationalBTText));
				if (InternationalBTText.HasData)
					elemVerse.Add(new XElement(StoryEditor.ns + "InternationalBT", InternationalBTText));
				if (Anchors.HasData)
					elemVerse.Add(Anchors.GetXml);
				if (TestQuestions.HasData)
					elemVerse.Add(TestQuestions.GetXml);
				if (Retellings.HasData)
					elemVerse.Add(Retellings.GetXml);
				if (ConsultantNotes.HasData)
					elemVerse.Add(ConsultantNotes.GetXml);
				if (CoachNotes.HasData)
					elemVerse.Add(CoachNotes.GetXml);
				return elemVerse;
			}
		}
	}

	public class VersesData : List<VerseData>
	{
		public VersesData(StoryProject.storyRow theStoryRow, StoryProject projFile)
		{
			StoryProject.versesRow[] theVersesRows = theStoryRow.GetversesRows();
			StoryProject.versesRow theVersesRow;
			if (theVersesRows.Length == 0)
				theVersesRow = projFile.verses.AddversesRow(theStoryRow);
			else
				theVersesRow = theVersesRows[0];

			foreach (StoryProject.verseRow aVerseRow in theVersesRow.GetverseRows())
				Add(new VerseData(aVerseRow, projFile));
		}

		public VersesData()
		{
		}

		public bool HasData
		{
			get { return (this.Count > 0); }
		}

		public XElement GetXml
		{
			get
			{
				System.Diagnostics.Debug.Assert(HasData);
				XElement elemVerses = new XElement(StoryEditor.ns + "verses");
				foreach (VerseData aVerseData in this)
					elemVerses.Add(aVerseData.GetXml);
				return elemVerses;
			}
		}
	}
}
