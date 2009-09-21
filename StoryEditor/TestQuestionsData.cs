using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;

namespace OneStoryProjectEditor
{
	public class TestQuestionData
	{
		public bool IsVisible = true;
		public StringTransfer QuestionVernacular = null;
		public StringTransfer QuestionNationalBT = null;
		public StringTransfer QuestionInternationalBT = null;
		public AnswersData Answers = null;

		public TestQuestionData(StoryProject.TestQuestionRow theTestQuestionRow, StoryProject projFile)
		{
			IsVisible = theTestQuestionRow.visible;

			QuestionVernacular = new StringTransfer((theTestQuestionRow.IsTQVernacularNull()) ? null : theTestQuestionRow.TQVernacular);
			QuestionNationalBT = new StringTransfer((theTestQuestionRow.IsTQNationalBTNull()) ? null : theTestQuestionRow.TQNationalBT);
			QuestionInternationalBT = new StringTransfer((theTestQuestionRow.IsTQInternationalBTNull()) ? null : theTestQuestionRow.TQInternationalBT);
			Answers = new AnswersData(theTestQuestionRow, projFile);
		}

		public TestQuestionData()
		{
			QuestionVernacular = new StringTransfer(null);
			QuestionNationalBT = new StringTransfer(null);
			QuestionInternationalBT = new StringTransfer(null);
			Answers = new AnswersData();
		}

		public bool HasData
		{
			get
			{
				return (QuestionVernacular.HasData
					|| QuestionNationalBT.HasData
					|| QuestionInternationalBT.HasData
					|| Answers.HasData);
			}
		}

		public XElement GetXml
		{
			get
			{
				System.Diagnostics.Debug.Assert(QuestionVernacular.HasData
					|| QuestionNationalBT.HasData
					|| QuestionInternationalBT.HasData
					|| Answers.HasData, "you have an empty TestQuestionData");

				XElement eleTQ = new XElement("TestQuestion", new XAttribute("visible", IsVisible));

				if (QuestionVernacular.HasData)
					eleTQ.Add(new XElement("TQVernacular", QuestionVernacular));
				if (QuestionNationalBT.HasData)
					eleTQ.Add(new XElement("TQNationalBT", QuestionNationalBT));
				if (QuestionInternationalBT.HasData)
					eleTQ.Add(new XElement("TQInternationalBT", QuestionInternationalBT));
				if (Answers.HasData)
					eleTQ.Add(Answers.GetXml);

				return eleTQ;
			}
		}
	}

	public class TestQuestionsData : List<TestQuestionData>
	{
		public TestQuestionsData(StoryProject.verseRow theVerseRow, StoryProject projFile)
		{
			StoryProject.TestQuestionsRow[] theTestQuestionsRows = theVerseRow.GetTestQuestionsRows();
			StoryProject.TestQuestionsRow theTestQuestionsRow;
			if (theTestQuestionsRows.Length == 0)
				theTestQuestionsRow = projFile.TestQuestions.AddTestQuestionsRow(theVerseRow);
			else
				theTestQuestionsRow = theTestQuestionsRows[0];

			foreach (StoryProject.TestQuestionRow aTestingQuestionRow in theTestQuestionsRow.GetTestQuestionRows())
				Add(new TestQuestionData(aTestingQuestionRow, projFile));
		}

		public TestQuestionsData()
		{
		}

		public TestQuestionData AddTestQuestion()
		{
			TestQuestionData theTQ = new TestQuestionData();
			this.Add(theTQ);
			return theTQ;
		}

		public bool HasData
		{
			get { return (this.Count > 0); }
		}

		public XElement GetXml
		{
			get
			{
				System.Diagnostics.Debug.Assert(HasData, "trying to serialize an empty TestQuestionsData");
				XElement elemTestQuestions = new XElement("TestQuestions");
				foreach (TestQuestionData aTestQuestionData in this)
					if (aTestQuestionData.HasData)
						elemTestQuestions.Add(aTestQuestionData.GetXml);
				return elemTestQuestions;
			}
		}
	}
}
