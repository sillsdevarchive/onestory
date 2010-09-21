using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using SilEncConverters31;

namespace OneStoryProjectEditor
{
	public class VerseData
	{
		public string guid;
		public bool IsFirstVerse;
		public bool IsVisible = true;
		public StringTransfer VernacularText = null;
		public StringTransfer NationalBTText = null;
		public StringTransfer InternationalBTText = null;
		public AnchorsData Anchors = null;
		public TestQuestionsData TestQuestions = null;
		public RetellingsData Retellings = null;
		public ConsultantNotesData ConsultantNotes = null;
		public CoachNotesData CoachNotes = null;

		public VerseData(NewDataSet.verseRow theVerseRow, NewDataSet projFile)
		{
			guid = theVerseRow.guid;

			if (!theVerseRow.IsfirstNull())
				IsFirstVerse = theVerseRow.first;

			if (!theVerseRow.IsvisibleNull())
				IsVisible = theVerseRow.visible;

			VernacularText = new StringTransfer((!theVerseRow.IsVernacularNull()) ? theVerseRow.Vernacular : null);
			NationalBTText = new StringTransfer((!theVerseRow.IsNationalBTNull()) ? theVerseRow.NationalBT : null);
			InternationalBTText = new StringTransfer((!theVerseRow.IsInternationalBTNull()) ? theVerseRow.InternationalBT : null);

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

		public VerseData(XmlNode node)
		{
			XmlAttribute attr;
			guid = ((attr = node.Attributes[CstrAttributeGuid]) != null) ? attr.Value : null;   // can't really happen
			IsFirstVerse = ((attr = node.Attributes[CstrAttributeFirstVerse]) != null) ? (attr.Value == "true") : false;
			IsVisible = ((attr = node.Attributes[CstrAttributeVisible]) != null) ? (attr.Value == "true") : true;
			XmlNode elem;
			VernacularText = new StringTransfer(((elem = node.SelectSingleNode(CstrFieldNameVernacular)) != null) ? elem.InnerText : null);
			NationalBTText = new StringTransfer(((elem = node.SelectSingleNode(CstrFieldNameNationalBt)) != null) ? elem.InnerText : null);
			InternationalBTText = new StringTransfer(((elem = node.SelectSingleNode(CstrFieldNameInternationalBt)) != null) ? elem.InnerText : null);
			Anchors = new AnchorsData(node.SelectSingleNode(AnchorsData.CstrElementLabelAnchors));
			TestQuestions = new TestQuestionsData(node.SelectSingleNode(TestQuestionsData.CstrElementLabelTestQuestions));
			Retellings = new RetellingsData(node.SelectSingleNode(RetellingsData.CstrElementLableRetellings));
			ConsultantNotes = new ConsultantNotesData();
			CoachNotes = new CoachNotesData();
		}

		public VerseData(VerseData rhs)
		{
			// the guid shouldn't be replicated
			guid = Guid.NewGuid().ToString();   // rhs.guid;
			IsFirstVerse = rhs.IsFirstVerse;
			IsVisible = rhs.IsVisible;

			VernacularText = new StringTransfer(rhs.VernacularText.ToString());
			NationalBTText = new StringTransfer(rhs.NationalBTText.ToString());
			InternationalBTText = new StringTransfer(rhs.InternationalBTText.ToString());
			Anchors = new AnchorsData(rhs.Anchors);
			TestQuestions = new TestQuestionsData(rhs.TestQuestions);
			Retellings = new RetellingsData(rhs.Retellings);
			ConsultantNotes = new ConsultantNotesData(rhs.ConsultantNotes);
			CoachNotes = new CoachNotesData(rhs.CoachNotes);
		}

		public class SearchLookInProperties
		{
			public bool ViewLookInOptions { get; set; }
			public bool StoryLanguage { get; set; }
			public bool NationalBT { get; set; }
			public bool EnglishBT { get; set; }
			public bool ConsultantNotes { get; set; }
			public bool CoachNotes { get; set; }
			public bool Retellings { get; set; }
			public bool TestQs { get; set; }
			public bool TestAs { get; set; }
			public bool SearchAll { get; set; }
			public bool UseRegex { get; set; }
		}

		public class VerseString
		{
			public StringTransfer StringTransfer { get; set; }
			public ViewSettings.ItemToInsureOn ViewToInsureIsOn { get; set; }

			public VerseString(StringTransfer strStringTransfer,
				ViewSettings.ItemToInsureOn viewItemToInsureOn)
			{
				StringTransfer = strStringTransfer;
				ViewToInsureIsOn = viewItemToInsureOn;
			}
		}

		public class StringTransferSearchIndex : List<VerseString>
		{
			public string StoryName { get; set; }

			public StringTransferSearchIndex(string strStoryName)
			{
				StoryName = strStoryName;
			}

			public VerseString AddNewVerseString(StringTransfer strStringTransfer,
				ViewSettings.ItemToInsureOn viewItemToInsureOn)
			{
				var vs = new VerseString(strStringTransfer, viewItemToInsureOn);
				Add(vs);
				return vs;
			}
		}

		public class StorySearchIndex : List<StringTransferSearchIndex>
		{
			public StringTransferSearchIndex GetNewStorySearchIndex(string strStoryName)
			{
				var stsi = new StringTransferSearchIndex(strStoryName);
				Add(stsi);
				return stsi;
			}

			public StringTransferSearchIndex this[string strStoryName]
			{
				get
				{
					foreach (StringTransferSearchIndex stsi in this)
						if (stsi.StoryName == strStoryName)
							return stsi;

					System.Diagnostics.Debug.Assert(false);
					return null;
				}
			}
		}

		public void IndexSearch(SearchLookInProperties findProperties,
			ref StringTransferSearchIndex lstBoxesToSearch)
		{
			if (VernacularText.HasData && findProperties.StoryLanguage)
				lstBoxesToSearch.AddNewVerseString(VernacularText,
					ViewSettings.ItemToInsureOn.VernacularLangField);
			if (NationalBTText.HasData && findProperties.NationalBT)
				lstBoxesToSearch.AddNewVerseString(NationalBTText,
					ViewSettings.ItemToInsureOn.NationalBTLangField);
			if (InternationalBTText.HasData && findProperties.EnglishBT)
				lstBoxesToSearch.AddNewVerseString(InternationalBTText,
					ViewSettings.ItemToInsureOn.EnglishBTField);
			if (TestQuestions.HasData && (findProperties.TestQs || findProperties.TestAs))
				TestQuestions.IndexSearch(findProperties, ref lstBoxesToSearch);
			if (Retellings.HasData && findProperties.Retellings)
				Retellings.IndexSearch(findProperties, ref lstBoxesToSearch);
			if (ConsultantNotes.HasData && findProperties.ConsultantNotes)
				ConsultantNotes.IndexSearch(findProperties, ref lstBoxesToSearch);
			if (CoachNotes.HasData && findProperties.CoachNotes)
				CoachNotes.IndexSearch(findProperties, ref lstBoxesToSearch);
		}

		public bool HasData
		{
			get
			{
				return (VernacularText.HasData || NationalBTText.HasData || InternationalBTText.HasData
					|| Anchors.HasData || TestQuestions.HasData || Retellings.HasData
					|| ConsultantNotes.HasData || CoachNotes.HasData);
			}
		}

		public const string CstrFieldNameVernacular = "Vernacular";
		public const string CstrFieldNameNationalBt = "NationalBT";
		public const string CstrFieldNameInternationalBt = "InternationalBT";

		public const string CstrAttributeGuid = "guid";
		public const string CstrElementLabelVerse = "verse";
		public const string CstrAttributeFirstVerse = "first";
		public const string CstrAttributeVisible = "visible";

		public XElement GetXml
		{
			get
			{
				XElement elemVerse = new XElement(CstrElementLabelVerse,
					new XAttribute(CstrAttributeGuid, guid));

				// only need to write out the 'first' attribute if it's true
				if (IsFirstVerse)
					elemVerse.Add(new XAttribute(CstrAttributeFirstVerse, IsFirstVerse));

				// only need to write out the 'visible' attribute if it's false
				if (!IsVisible)
					elemVerse.Add(new XAttribute(CstrAttributeVisible, IsVisible));

				if (VernacularText.HasData)
					elemVerse.Add(new XElement(CstrFieldNameVernacular, VernacularText));
				if (NationalBTText.HasData)
					elemVerse.Add(new XElement(CstrFieldNameNationalBt, NationalBTText));
				if (InternationalBTText.HasData)
					elemVerse.Add(new XElement(CstrFieldNameInternationalBt, InternationalBTText));
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

		public void AllowConNoteButtonsOverride()
		{
			ConsultantNotes.AllowButtonsOverride();
			CoachNotes.AllowButtonsOverride();
		}

		public class ViewSettings
		{
			[Flags]
			public enum ItemToInsureOn
			{
				Undefined = 0,
				VernacularLangField = 1,
				VernacularTransliterationField = 2,
				NationalBTLangField = 4,
				NationalBTTransliterationField = 8,
				EnglishBTField = 16,
				AnchorFields = 32,
				StoryTestingQuestions = 64,
				StoryTestingQuestionAnswers = 128,
				RetellingFields = 256,
				ConsultantNoteFields = 512,
				CoachNotesFields = 1024,
				BibleViewer = 2048,
				StoryFrontMatter = 4096
			}

			public DirectableEncConverter TransliteratorVernacular { get; set; }
			public DirectableEncConverter TransliteratorNationalBT { get; set; }
			protected ItemToInsureOn _itemToInsureOn;

			public ViewSettings(ItemToInsureOn itemToInsureOn)
			{
				_itemToInsureOn = itemToInsureOn;
			}

			public ViewSettings
				(
				bool bLangVernacular,
				bool bLangNationalBT,
				bool bLangInternationalBT,
				bool bAnchors,
				bool bStoryTestingQuestions,
				bool bStoryTestingQuestionAnswers,
				bool bRetellings,
				bool bConsultantNotes,
				bool bCoachNotes,
				bool bBibleViewer,
				bool bStoryFrontMatter,
				DirectableEncConverter decTransliteratorVernacular,
				DirectableEncConverter decTransliteratorNationalBT
				)
			{
				SetItemsToInsureOn(bLangVernacular,
								   (decTransliteratorVernacular != null),
								   bLangNationalBT,
								   (decTransliteratorNationalBT != null),
								   bLangInternationalBT,
								   bAnchors,
								   bStoryTestingQuestions,
								   bStoryTestingQuestionAnswers,
								   bRetellings,
								   bConsultantNotes,
								   bCoachNotes,
								   bBibleViewer,
								   bStoryFrontMatter);
				TransliteratorVernacular = decTransliteratorVernacular;
				TransliteratorNationalBT = decTransliteratorNationalBT;
			}

			public bool IsViewItemOn(ItemToInsureOn eFlag)
			{
				return ((_itemToInsureOn & eFlag) != ItemToInsureOn.Undefined);
			}

			public void SetItemsToInsureOn
				(
				bool bLangVernacular,
				bool bLangVernacularTransliterate,
				bool bLangNationalBT,
				bool bLangNationalBTTransliterate,
				bool bLangInternationalBT,
				bool bAnchors,
				bool bStoryTestingQuestions,
				bool bStoryTestingQuestionAnswers,
				bool bRetellings,
				bool bConsultantNotes,
				bool bCoachNotes,
				bool bBibleViewer,
				bool bStoryFrontMatter
				)
			{
				_itemToInsureOn = 0;
				if (bLangVernacular)
					_itemToInsureOn |= ItemToInsureOn.VernacularLangField;
				if (bLangVernacularTransliterate)
					_itemToInsureOn |= ItemToInsureOn.VernacularTransliterationField;
				if (bLangNationalBT)
					_itemToInsureOn |= ItemToInsureOn.NationalBTLangField;
				if (bLangNationalBTTransliterate)
					_itemToInsureOn |= ItemToInsureOn.NationalBTTransliterationField;
				if (bLangInternationalBT)
					_itemToInsureOn |= ItemToInsureOn.EnglishBTField;
				if (bAnchors)
					_itemToInsureOn |= ItemToInsureOn.AnchorFields;
				if (bStoryTestingQuestions)
					_itemToInsureOn |= ItemToInsureOn.StoryTestingQuestions;
				if (bStoryTestingQuestionAnswers)
					_itemToInsureOn |= ItemToInsureOn.StoryTestingQuestionAnswers;
				if (bRetellings)
					_itemToInsureOn |= ItemToInsureOn.RetellingFields;
				if (bConsultantNotes)
					_itemToInsureOn |= ItemToInsureOn.ConsultantNoteFields;
				if (bCoachNotes)
					_itemToInsureOn |= ItemToInsureOn.CoachNotesFields;
				if (bBibleViewer)
					_itemToInsureOn |= ItemToInsureOn.BibleViewer;
				if (bStoryFrontMatter)
					_itemToInsureOn |= ItemToInsureOn.StoryFrontMatter;
			}
		}

		public static string TextareaId(int nVerseIndex, string strTextElementName)
		{
			return String.Format("ta_{0}_{1}", nVerseIndex, strTextElementName);
		}

		public static string HtmlColor(Color clrRow)
		{
			return String.Format("#{0:X2}{1:X2}{2:X2}",
								 clrRow.R, clrRow.G, clrRow.B);
		}

		public string StoryBtHtml(ProjectSettings projectSettings, TeamMembersData membersData,
			StoryStageLogic stageLogic, TeamMemberData loggedOnMember, int nVerseIndex,
			ViewSettings viewItemToInsureOn, int nNumCols)
		{
			string strRow = null;
			if (viewItemToInsureOn.IsViewItemOn(ViewSettings.ItemToInsureOn.VernacularLangField))
			{
				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameVernacular,
											   StoryData.CstrLangVernacularStyleClassName,
											   VernacularText.ToString());
			}

			if (viewItemToInsureOn.IsViewItemOn(ViewSettings.ItemToInsureOn.NationalBTLangField))
			{
				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameNationalBt,
											   StoryData.CstrLangNationalBtStyleClassName, NationalBTText.ToString());
			}

			if (viewItemToInsureOn.IsViewItemOn(ViewSettings.ItemToInsureOn.EnglishBTField))
			{
				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameInternationalBt,
											   StoryData.CstrLangInternationalBtStyleClassName, InternationalBTText.ToString());
			}

			string strStoryLineRow = String.Format(OseResources.Properties.Resources.HTML_TableRow,
												   strRow);

			if (viewItemToInsureOn.IsViewItemOn(ViewSettings.ItemToInsureOn.AnchorFields))
				strStoryLineRow += Anchors.Html(nVerseIndex, nNumCols);

			if (viewItemToInsureOn.IsViewItemOn(ViewSettings.ItemToInsureOn.RetellingFields)
				&& (Retellings.Count > 0))
				strStoryLineRow += Retellings.Html(nVerseIndex, nNumCols);

			if (viewItemToInsureOn.IsViewItemOn(ViewSettings.ItemToInsureOn.StoryTestingQuestions | ViewSettings.ItemToInsureOn.StoryTestingQuestionAnswers)
				&& (TestQuestions.Count > 0))
				strStoryLineRow += TestQuestions.Html(projectSettings, viewItemToInsureOn,
					stageLogic, loggedOnMember, nVerseIndex, nNumCols,
					membersData.HasOutsideEnglishBTer);

			return strStoryLineRow;
		}

		public bool IsDiffProcessed;

		// Html that shows the data in the StoryBt file, but in a fully read-only manner
		public string PresentationHtml(int nVerseIndex, int nNumCols, CraftingInfoData craftingInfo,
			ViewSettings viewSettings, VersesData child, VerseData theChildVerse,
			bool bPrintPreview, bool bHasOutsideEnglishBTer)
		{
			string strRow = null;
			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.VernacularLangField))
			{
				DirectableEncConverter transliterator = viewSettings.TransliteratorVernacular;
				string str = (!bPrintPreview)
					? ((child != null) && IsVisible)
						? Diff.HtmlDiff(transliterator, VernacularText, ((theChildVerse != null) && theChildVerse.IsVisible) ? theChildVerse.VernacularText : null)
						: Diff.HtmlDiff(transliterator, null, VernacularText)
					: VernacularText.GetValue(transliterator);

				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameVernacular,
											   StoryData.CstrLangVernacularStyleClassName, str);
			}

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.NationalBTLangField))
			{
				DirectableEncConverter transliterator = viewSettings.TransliteratorNationalBT;
				string str = (!bPrintPreview)
					? ((child != null) && IsVisible)
						? Diff.HtmlDiff(transliterator, NationalBTText, ((theChildVerse != null) && theChildVerse.IsVisible) ? theChildVerse.NationalBTText : null)
						: Diff.HtmlDiff(transliterator, null, NationalBTText)
					: NationalBTText.GetValue(transliterator);

				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameNationalBt,
											   StoryData.CstrLangNationalBtStyleClassName, str);
			}

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.EnglishBTField))
			{
				string str = (!bPrintPreview)
					? ((child != null) && IsVisible)
						? Diff.HtmlDiff(InternationalBTText, ((theChildVerse != null) && theChildVerse.IsVisible) ? theChildVerse.InternationalBTText : null)
						: Diff.HtmlDiff(null, InternationalBTText)
					: InternationalBTText.ToString();

				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameInternationalBt,
											   StoryData.CstrLangInternationalBtStyleClassName, str);
			}

			string strStoryLineRow = String.Format(OseResources.Properties.Resources.HTML_TableRow,
												   strRow);

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.AnchorFields))
				strStoryLineRow += Anchors.PresentationHtml(nVerseIndex, nNumCols,
					(theChildVerse != null) ? theChildVerse.Anchors : null, bPrintPreview);

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.RetellingFields))
				strStoryLineRow += Retellings.PresentationHtml(nVerseIndex, nNumCols,
					craftingInfo.Testors, (theChildVerse != null) ? theChildVerse.Retellings : null,
					bPrintPreview, false);

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.StoryTestingQuestions | ViewSettings.ItemToInsureOn.StoryTestingQuestionAnswers))
				strStoryLineRow += TestQuestions.PresentationHtml(nVerseIndex, nNumCols, viewSettings,
					craftingInfo.Testors, (theChildVerse != null) ? theChildVerse.TestQuestions : null,
					bPrintPreview, bHasOutsideEnglishBTer);

			// indicate that we've done this one
			IsDiffProcessed = true;
			return strStoryLineRow;
		}

		private static string FormatLanguageColumn(int nVerseIndex, int nNumCols,
			string strFieldName, string strLangStyleClassName, string strValue)
		{
			return String.Format(OseResources.Properties.Resources.HTML_TableCellWidthAlignTop, 100 / nNumCols,
										String.Format(OseResources.Properties.Resources.HTML_ParagraphText,
													  TextareaId(nVerseIndex, strFieldName),
													  strLangStyleClassName,
													  strValue));
		}

		// for use when the data is to be marked as an addition (i.e. yellow highlight)
		public string PresentationHtmlAsAddition(int nVerseIndex, int nNumCols,
			CraftingInfoData craftingInfo, ViewSettings viewSettings, bool bHasOutsideEnglishBTer)
		{
			string strRow = null;
			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.VernacularLangField))
			{
				DirectableEncConverter transliterator = viewSettings.TransliteratorVernacular;
				string str = Diff.HtmlDiff(transliterator, null, VernacularText);

				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameVernacular,
											   StoryData.CstrLangVernacularStyleClassName, str);
			}

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.NationalBTLangField))
			{
				DirectableEncConverter transliterator = viewSettings.TransliteratorNationalBT;
				string str = Diff.HtmlDiff(transliterator, null, NationalBTText);

				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameNationalBt,
											   StoryData.CstrLangNationalBtStyleClassName, str);
			}

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.EnglishBTField))
			{
				string str = Diff.HtmlDiff(null, InternationalBTText);

				strRow += FormatLanguageColumn(nVerseIndex, nNumCols, CstrFieldNameInternationalBt,
											   StoryData.CstrLangInternationalBtStyleClassName, str);
			}

			string strStoryLineRow = String.Format(OseResources.Properties.Resources.HTML_TableRow,
												   strRow);

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.AnchorFields))
				strStoryLineRow += Anchors.PresentationHtmlAsAddition(nVerseIndex, nNumCols);

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.RetellingFields))
				strStoryLineRow += Retellings.PresentationHtmlAsAddition(nVerseIndex, nNumCols,
					craftingInfo.Testors);

			if (viewSettings.IsViewItemOn(ViewSettings.ItemToInsureOn.StoryTestingQuestions | ViewSettings.ItemToInsureOn.StoryTestingQuestionAnswers))
				strStoryLineRow += TestQuestions.PresentationHtmlAsAddition(nVerseIndex, nNumCols, viewSettings,
					craftingInfo.Testors, bHasOutsideEnglishBTer);

			// indicate that we've done this one
			IsDiffProcessed = true;
			return strStoryLineRow;
		}
	}

	public class VersesData : List<VerseData>
	{
		internal const string CstrZerothLineName = "Story:";
		internal const string CstrHiddenVerseSuffix = " (Hidden)";

		public VerseData FirstVerse;

		public VersesData(NewDataSet.storyRow theStoryRow, NewDataSet projFile)
		{
			NewDataSet.versesRow[] theVersesRows = theStoryRow.GetversesRows();
			NewDataSet.versesRow theVersesRow;
			if (theVersesRows.Length == 0)
				theVersesRow = projFile.verses.AddversesRow(theStoryRow);
			else
				theVersesRow = theVersesRows[0];

			foreach (NewDataSet.verseRow aVerseRow in theVersesRow.GetverseRows())
				Add(new VerseData(aVerseRow, projFile));

			AdjustmentForFirstVerse();
		}

		public VersesData(XmlNode node)
		{
			if (node == null)
				return;

			XmlNodeList list = node.SelectNodes(VerseData.CstrElementLabelVerse);
			if (list == null)
				return;

			foreach (XmlNode nodeVerse in list)
				Add(new VerseData(nodeVerse));

			AdjustmentForFirstVerse();
		}

		public VersesData(VersesData rhs)
		{
			FirstVerse = new VerseData(rhs.FirstVerse);
			foreach (VerseData aVerse in rhs)
				Add(new VerseData(aVerse));
		}

		public VersesData()
		{
		}

		private void AdjustmentForFirstVerse()
		{
			// the zeroth verse is special for global connotes
			if ((Count > 0) && this[0].IsFirstVerse)
			{
				FirstVerse = this[0];
				RemoveAt(0);
			}
			else
				CreateFirstVerse();

			// sometimes the others think they are first verse too... (not sure how this
			//  happens)
			foreach (var aVerse in this)
				aVerse.IsFirstVerse = false;
		}

		public void CreateFirstVerse()
		{
			FirstVerse = new VerseData { IsFirstVerse = true };
		}

		public VerseData InsertVerse(int nIndex, string strVernacular, string strNationalBT, string strInternationalBT)
		{
			var dataVerse = new VerseData
										{
											VernacularText = new StringTransfer(strVernacular),
											NationalBTText = new StringTransfer(strNationalBT),
											InternationalBTText = new StringTransfer(strInternationalBT)
										};
			Insert(nIndex, dataVerse);
			return dataVerse;
		}

		public bool HasData
		{
			get { return (Count > 0) || ((FirstVerse != null) && (FirstVerse.HasData)); }
		}

		public XElement GetXml
		{
			get
			{
				System.Diagnostics.Debug.Assert(HasData);
				XElement elemVerses = new XElement("verses");

				// write out the zeroth verse first
				elemVerses.Add(FirstVerse.GetXml);

				// then write out the rest
				foreach (VerseData aVerseData in this)
					elemVerses.Add(aVerseData.GetXml);

				return elemVerses;
			}
		}

		public int NumOfLines
		{
			get
			{
				int nCount = 0;
				foreach (VerseData aVerse in this)
					if (aVerse.IsVisible)
						nCount++;
				return nCount;
			}
		}

		public string NumOfWords(ProjectSettings projSettings)
		{
			string strCount = null;
			if (projSettings.Vernacular.HasData)
				strCount = GetWordCountVernacular(projSettings);
			else if (projSettings.NationalBT.HasData)
				strCount = GetWordCountNationalBT(projSettings);
			else if (projSettings.InternationalBT.HasData)
				strCount = GetWordCountInternationalBT(projSettings);
			return strCount;
		}

		protected string GetWordCountVernacular(ProjectSettings projSettings)
		{
			ProjectSettings.LanguageInfo li = projSettings.Vernacular;
			int nCount = 0;
			string strCharsToIgnore = "\r\n " + li.FullStop + CheckEndOfStateTransition.QuoteCharsAsString;
			char[] achToIgnore = strCharsToIgnore.ToCharArray();

			foreach (VerseData aVerse in this)
				if (aVerse.IsVisible)
					nCount += aVerse.VernacularText.NumOfWords(achToIgnore);
			return String.Format("{0} (in {1})", nCount, li.LangName);
		}

		protected string GetWordCountNationalBT(ProjectSettings projSettings)
		{
			ProjectSettings.LanguageInfo li = projSettings.NationalBT;
			int nCount = 0;
			string strCharsToIgnore = "\r\n " + li.FullStop + CheckEndOfStateTransition.QuoteCharsAsString;
			char[] achToIgnore = strCharsToIgnore.ToCharArray();

			foreach (VerseData aVerse in this)
				if (aVerse.IsVisible)
					nCount += aVerse.NationalBTText.NumOfWords(achToIgnore);
			return String.Format("{0} (in {1})", nCount, li.LangName);
		}

		protected string GetWordCountInternationalBT(ProjectSettings projSettings)
		{
			ProjectSettings.LanguageInfo li = projSettings.InternationalBT;
			int nCount = 0;
			string strCharsToIgnore = "\r\n " + li.FullStop + CheckEndOfStateTransition.QuoteCharsAsString;
			char[] achToIgnore = strCharsToIgnore.ToCharArray();

			foreach (VerseData aVerse in this)
				if (aVerse.IsVisible)
					nCount += aVerse.InternationalBTText.NumOfWords(achToIgnore);
			return String.Format("{0} (in {1})", nCount, li.LangName);
		}

		protected int CalculateColumns(VerseData.ViewSettings viewItemToInsureOn)
		{
			int nColSpan = 0;
			if (viewItemToInsureOn.IsViewItemOn(VerseData.ViewSettings.ItemToInsureOn.VernacularLangField))
				nColSpan++;
			if (viewItemToInsureOn.IsViewItemOn(VerseData.ViewSettings.ItemToInsureOn.NationalBTLangField))
				nColSpan++;
			if (viewItemToInsureOn.IsViewItemOn(VerseData.ViewSettings.ItemToInsureOn.EnglishBTField))
				nColSpan++;
			return nColSpan;
		}

		public string StoryBtHtml(ProjectSettings projectSettings, bool bViewHidden,
			StoryStageLogic stageLogic, TeamMembersData membersData, TeamMemberData loggedOnMember,
			VerseData.ViewSettings viewItemToInsureOn)
		{
			int nColSpan = CalculateColumns(viewItemToInsureOn);

			// add a row indicating which languages are in what columns
			string strHtml = null;
			if (viewItemToInsureOn.IsViewItemOn(VerseData.ViewSettings.ItemToInsureOn.VernacularLangField))
				strHtml += String.Format(OseResources.Properties.Resources.HTML_TableCell,
										 projectSettings.Vernacular.LangName);
			if (viewItemToInsureOn.IsViewItemOn(VerseData.ViewSettings.ItemToInsureOn.NationalBTLangField))
				strHtml += String.Format(OseResources.Properties.Resources.HTML_TableCell,
										 projectSettings.NationalBT.LangName);
			if (viewItemToInsureOn.IsViewItemOn(VerseData.ViewSettings.ItemToInsureOn.EnglishBTField))
				strHtml += String.Format(OseResources.Properties.Resources.HTML_TableCell,
										 projectSettings.InternationalBT.LangName);

			strHtml = String.Format(OseResources.Properties.Resources.HTML_TableRow,
									 strHtml);

			for (int i = 1; i <= Count; i++)
			{
				VerseData aVerseData = this[i - 1];
				if (aVerseData.IsVisible || bViewHidden)
				{
					strHtml += GetHeaderRow("Ln: " + i, i, aVerseData.IsVisible, true, nColSpan);

					strHtml += aVerseData.StoryBtHtml(projectSettings, membersData,
						stageLogic, loggedOnMember, i, viewItemToInsureOn, nColSpan);
				}
			}

			return String.Format(OseResources.Properties.Resources.HTML_Table, strHtml);
		}

		protected VerseData FindChildEquivalent(VerseData theParentVerse, VersesData child)
		{
			if (child != null)
				foreach (VerseData aVerse in child)
					if (aVerse.guid == theParentVerse.guid)
					{
						aVerse.IsDiffProcessed = true;
						return aVerse;
					}
			return null;
		}

		public string PresentationHtml(CraftingInfoData craftingInfo, VersesData child, int nNumCols,
			VerseData.ViewSettings viewSettings, bool bHasOutsideEnglishBTer)
		{
			string strHtml = null;
			int nInsertCount = 0;
			int i = 1;
			while (i <= Count)
			{
				// get the parent and child verses that match
				VerseData aVerseData = this[i - 1];
				VerseData theChildVerse = FindChildEquivalent(aVerseData, child);

				// we don't want to process if it's the first verse or if both are hidden
				bool bSkip = (aVerseData.IsFirstVerse
							  || (!aVerseData.IsVisible
								  && ((theChildVerse != null) && !theChildVerse.IsVisible)));

				if (!bSkip)
				{
					// if parent was hidden and child is not, then say 'hidden', but show addition
					// if parent was visible and child is not, then say 'hidden', but show deletion
					// if both are visible, then say nothing (both can't be hidden, because we skip that
					bool bHidden = !aVerseData.IsVisible
								   || ((theChildVerse != null) && (!theChildVerse.IsVisible));
					int nLineIndex = i + nInsertCount;
					strHtml += GetHeaderRow("Ln: " + nLineIndex, nLineIndex, !bHidden, false, nNumCols);

					if (theChildVerse != null)
					{
						// see if there were any child verses that weren't processed
						bool bFoundOne = false;
						for (int j = i; j <= child.IndexOf(theChildVerse); j++)
						{
							VerseData aPassedByChild = child[j - 1];
							if (!aPassedByChild.IsDiffProcessed)
							{
								strHtml += aPassedByChild.PresentationHtmlAsAddition(nLineIndex, nNumCols,
									craftingInfo, viewSettings, bHasOutsideEnglishBTer);
								bFoundOne = true;
								nInsertCount++;
							}
						}

						if (bFoundOne)
							continue;
					}

					strHtml += aVerseData.PresentationHtml(nLineIndex, nNumCols, craftingInfo,
						viewSettings, child, theChildVerse, (child == null), bHasOutsideEnglishBTer);

					// if there is a child, but we couldn't find the equivalent verse...
					if ((child != null) && (theChildVerse == null) && (child.Count >= i))
					{
						// this means the original verse (which we just showed as deleted)
						//  was replaced by whatever is the same verse in the child collection
						theChildVerse = child[i - 1];
						if (theChildVerse.IsVisible)
							strHtml += theChildVerse.PresentationHtmlAsAddition(nLineIndex, nNumCols,
																				craftingInfo, viewSettings,
																				bHasOutsideEnglishBTer);
					}
				}

				i++;    // do this here in case we redo one (from 'continue' above)
			}

			if (child != null)
				while (i <= child.Count)
				{
					VerseData aVerseData = child[i - 1];
					if (!aVerseData.IsDiffProcessed)
					{
						int nLineIndex = i + nInsertCount;
						strHtml += GetHeaderRow("Ln: " + nLineIndex, nLineIndex, aVerseData.IsVisible, false, nNumCols);

						strHtml += aVerseData.PresentationHtmlAsAddition(nLineIndex, nNumCols,
							craftingInfo, viewSettings, bHasOutsideEnglishBTer);
					}
					i++;
				}

			return String.Format(OseResources.Properties.Resources.HTML_Table, strHtml);
		}

		public static string ButtonId(int nVerseIndex)
		{
			return String.Format("btnLn_{0}", nVerseIndex);
		}

		protected string GetHeaderRow(string strHeader, int nVerseIndex, bool bVerseVisible, bool bShowButton, int nColSpan)
		{
			string strLink = String.Format(OseResources.Properties.Resources.HTML_LinkJumpLine,
										   nVerseIndex, strHeader);
			if (!bVerseVisible)
				strLink += CstrHiddenVerseSuffix;

			string strButton = null;
			if (bShowButton)
				strButton = String.Format(OseResources.Properties.Resources.HTML_ButtonRightAlignCtxMenu,
										  nVerseIndex, // ButtonId(nVerseIndex),
										  " ");

			string strHtml = String.Format(OseResources.Properties.Resources.HTML_TableRowColor, "#AACCFF",
										   String.Format(OseResources.Properties.Resources.HTML_TableCellWidthSpanId,
														 LineId(nVerseIndex),
														 100,
														 nColSpan,
														 strLink + strButton));

			return strHtml;
		}

		public static string LineId(int nVerseIndex)
		{
			return String.Format("ln_{0}", nVerseIndex);
		}

		protected string GetHeaderRow(string strHeader, int nVerseIndex, bool bVerseVisible,
			ConsultNotesDataConverter theCNsDC, TeamMemberData LoggedOnMember)
		{
			string strHtmlAddNoteButton = null;
			if (theCNsDC.HasAddNotePrivilege(LoggedOnMember.MemberType))
				strHtmlAddNoteButton = String.Format(OseResources.Properties.Resources.HTML_TableCell,
													 String.Format(OseResources.Properties.Resources.HTML_Button,
																   nVerseIndex,
																   "return window.external.OnAddNote(this.id, null);",
																   "Add Note"));

			string strLink = String.Format(OseResources.Properties.Resources.HTML_LinkJumpLine,
										   nVerseIndex, strHeader);
			if (!bVerseVisible)
				strLink += CstrHiddenVerseSuffix;
			return String.Format(OseResources.Properties.Resources.HTML_TableRowColor, "#AACCFF",
								 String.Format("{0}{1}",
											   String.Format(OseResources.Properties.Resources.HTML_TableCellWidthId,
															 LineId(nVerseIndex),
															 100,
															 strLink),
											   strHtmlAddNoteButton));
		}

		public string ConsultantNotesHtml(object htmlConNoteCtrl,
			StoryStageLogic theStoryStage, TeamMemberData LoggedOnMember,
			bool bViewHidden, bool bShowOnlyOpenConversations)
		{
			string strHtml = null;
			strHtml += GetHeaderRow(CstrZerothLineName, 0, FirstVerse.IsVisible,
				FirstVerse.ConsultantNotes, LoggedOnMember);

			strHtml += FirstVerse.ConsultantNotes.Html(htmlConNoteCtrl, theStoryStage,
				LoggedOnMember, bViewHidden, FirstVerse.IsVisible, bShowOnlyOpenConversations, 0);

			for (int i = 1; i <= Count; i++)
			{
				VerseData aVerseData = this[i - 1];
				if (aVerseData.IsVisible || bViewHidden)
				{
					strHtml += GetHeaderRow("Ln: " + i, i, aVerseData.IsVisible,
						aVerseData.ConsultantNotes, LoggedOnMember);

					strHtml += aVerseData.ConsultantNotes.Html(htmlConNoteCtrl,
						theStoryStage, LoggedOnMember, bViewHidden, aVerseData.IsVisible, bShowOnlyOpenConversations, i);
				}
			}

			return String.Format(OseResources.Properties.Resources.HTML_Table, strHtml);
		}

		public string CoachNotesHtml(object htmlConNoteCtrl,
			StoryStageLogic theStoryStage, TeamMemberData LoggedOnMember,
			bool bViewHidden, bool bShowOnlyOpenConversations)
		{
			string strHtml = null;
			strHtml += GetHeaderRow(CstrZerothLineName, 0, FirstVerse.IsVisible,
				FirstVerse.CoachNotes, LoggedOnMember);

			strHtml += FirstVerse.CoachNotes.Html(htmlConNoteCtrl, theStoryStage,
				LoggedOnMember, bViewHidden, FirstVerse.IsVisible, bShowOnlyOpenConversations, 0);

			for (int i = 1; i <= Count; i++)
			{
				VerseData aVerseData = this[i - 1];
				if (aVerseData.IsVisible || bViewHidden)
				{
					strHtml += GetHeaderRow("Ln: " + i, i, aVerseData.IsVisible,
						aVerseData.CoachNotes, LoggedOnMember);

					strHtml += aVerseData.CoachNotes.Html(htmlConNoteCtrl, theStoryStage,
						LoggedOnMember, bViewHidden, aVerseData.IsVisible, bShowOnlyOpenConversations, i);
				}
			}

			return String.Format(OseResources.Properties.Resources.HTML_Table, strHtml);
		}

		public void IndexSearch(VerseData.SearchLookInProperties findProperties,
			ref VerseData.StringTransferSearchIndex lstBoxesToSearch)
		{
			// put the zeroth ConNotes box in the search queue
			FirstVerse.IndexSearch(findProperties, ref lstBoxesToSearch);

			for (int nVerseNum = 0; nVerseNum < Count; nVerseNum++)
			{
				VerseData aVerseData = this[nVerseNum];
				aVerseData.IndexSearch(findProperties, ref lstBoxesToSearch);
			}
		}
	}
}
