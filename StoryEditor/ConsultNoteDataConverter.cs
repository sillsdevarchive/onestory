using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Drawing;

namespace OneStoryProjectEditor
{
	public class CommInstance : StringTransfer
	{
		public ConsultNoteDataConverter.CommunicationDirections Direction;
		public string Guid;
		public DateTime TimeStamp;

		public CommInstance(string strValue,
			ConsultNoteDataConverter.CommunicationDirections direction,
			string strGuid, DateTime timeStamp)
			: base(strValue)
		{
			Direction = direction;
			Guid = strGuid ?? System.Guid.NewGuid().ToString();
			TimeStamp = timeStamp;
		}

		public CommInstance(CommInstance rhs)
			: base(rhs.ToString())
		{
			Direction = rhs.Direction;

			// the guid shouldn't be replicated
			Guid = System.Guid.NewGuid().ToString();  // rhs.Guid;
			TimeStamp = rhs.TimeStamp;
		}

		public bool IsMentor
		{
			get
			{
				return ((Direction == ConsultNoteDataConverter.CommunicationDirections.eConsultantToProjFac)
						|| (Direction == ConsultNoteDataConverter.CommunicationDirections.eCoachToConsultant));
			}
		}
	}

	public abstract class ConsultNoteDataConverter : List<CommInstance>
	{
		public int RoundNum;
		public string guid;
		public bool Visible = true;
		public bool IsFinished;

		public enum CommunicationDirections
		{
			eConsultantToProjFac,
			eProjFacToConsultant,
			eConsultantToCoach,
			eCoachToConsultant
		}

		protected ConsultNoteDataConverter(int nRoundNum)
		{
			RoundNum = nRoundNum;
			guid = Guid.NewGuid().ToString();
		}

		protected ConsultNoteDataConverter(int nRoundNum, string strGuid,
			bool bVisible, bool bIsFinished)
		{
			RoundNum = nRoundNum;
			guid = strGuid;
			Visible = bVisible;
			IsFinished = bIsFinished;
		}

		protected ConsultNoteDataConverter(ConsultNoteDataConverter rhs)
		{
			RoundNum = rhs.RoundNum;

			// the guid shouldn't be replicated
			guid = Guid.NewGuid().ToString();   // rhs.guid;
			Visible = rhs.Visible;
			IsFinished = rhs.IsFinished;

			foreach (CommInstance aCI in rhs)
				Add(new CommInstance(aCI));
		}

		protected static Dictionary<string, CommunicationDirections> CmapDirectionStringToEnumType = new Dictionary<string, CommunicationDirections>()
		{
			{ "ConsultantToProjFac", CommunicationDirections.eConsultantToProjFac },
			{ "ProjFacToConsultant", CommunicationDirections.eProjFacToConsultant },
			{ "ConsultantToCoach", CommunicationDirections.eConsultantToCoach },
			{ "CoachToConsultant", CommunicationDirections.eCoachToConsultant }
		};

		protected CommunicationDirections GetDirectionFromString(string strDirectionString)
		{
			System.Diagnostics.Debug.Assert(CmapDirectionStringToEnumType.ContainsKey(strDirectionString));
			return CmapDirectionStringToEnumType[strDirectionString];
		}

		public string GetDirectionString(CommunicationDirections eDirection)
		{
			return eDirection.ToString().Substring(1);
		}

		public void InsureExtraBox(TeamMemberData.UserTypes eLoggedOnMember,
			TeamMemberData.UserTypes eMentorType, TeamMemberData.UserTypes eMenteeType,
			string strValue)
		{
			// in case the user re-logs in, we might have extra boxes here. So remove any null ones before
			//  "insuring" the one(s) we need
			if (Count > 1)
				while (!this[Count - 1].HasData)
					RemoveAt(Count - 1);

			// don't bother, though, if the user has ended the conversation
			if (IsFinished)
				return;

			if ((eLoggedOnMember == eMentorType) && ((Count == 0) || (this[Count - 1].Direction == MenteeDirection)))
				Add(new CommInstance(strValue, MentorDirection, null, DateTime.Now));
			else if ((eLoggedOnMember == eMenteeType) && ((Count == 0) || (this[Count - 1].Direction == MentorDirection)))
				Add(new CommInstance(strValue, MenteeDirection, null, DateTime.Now));
		}

		// do this here, because we need to sub-class it to allow for FirstPassMentor working as well in addition to CIT
		public virtual void ThrowIfWrongEditor(TeamMemberData.UserTypes eLoggedOnMember, TeamMemberData.UserTypes eRequiredEditor)
		{
			if (IsWrongEditor(eLoggedOnMember, eRequiredEditor))
				throw new ApplicationException(String.Format("Only a '{0}' can edit this field", TeamMemberData.GetMemberTypeAsDisplayString(eRequiredEditor)));
		}

		protected virtual bool IsWrongEditor(TeamMemberData.UserTypes eLoggedOnMember, TeamMemberData.UserTypes eRequiredEditor)
		{
			return (eLoggedOnMember != eRequiredEditor);
		}

		public abstract CommunicationDirections MenteeDirection { get; }
		public abstract CommunicationDirections MentorDirection { get; }

		public abstract string MentorLabel
		{
			get;
		}

		public abstract string MenteeLabel
		{
			get;
		}

		public Color CommentColor
		{
			get { return Color.Maroon; }
		}

		public Color ResponseColor
		{
			get { return Color.Blue; }
		}

		protected abstract string InstanceElementName   // *Conversation
		{
			get;
		}

		protected abstract string SubElementName   // *Note
		{
			get;
		}

		public abstract TeamMemberData.UserTypes MentorRequiredEditor
		{
			get;
		}

		public abstract TeamMemberData.UserTypes MenteeRequiredEditor
		{
			get;
		}

		protected abstract VerseData.ViewItemToInsureOn AssociatedPane { get; }

		public bool HasData
		{
			get
			{
				return (Count > 0);
			}
		}

		public XElement GetXml
		{
			get
			{
				System.Diagnostics.Debug.Assert(Count > 0);

				XElement eleNote = new XElement(InstanceElementName,
					new XAttribute("round", RoundNum),
					new XAttribute("guid", guid));

				if (!Visible)
					eleNote.Add(new XAttribute("visible", false));

				if (IsFinished)
					eleNote.Add(new XAttribute("finished", true));

				foreach (CommInstance aCI in this)
					if (aCI.HasData)
						eleNote.Add(new XElement(SubElementName,
							new XAttribute("Direction", GetDirectionString(aCI.Direction)),
							new XAttribute("guid", aCI.Guid),
							new XAttribute("timeStamp", aCI.TimeStamp),
							aCI.ToString()));

				return eleNote;
			}
		}

		public static string TextareaId(int nVerseIndex, int nConversationIndex)
		{
			return String.Format("ta_{0}_{1}", nVerseIndex, nConversationIndex);
		}

		public static string TextareaRowId(int nVerseIndex, int nConversationIndex)
		{
			return String.Format("tarow_{0}_{1}", nVerseIndex, nConversationIndex);
		}

		public static string TextareaReadonlyRowId(int nVerseIndex, int nConversationIndex, int nCommentIndex)
		{
			return String.Format("tarorow_{0}_{1}_{2}", nVerseIndex, nConversationIndex, nCommentIndex);
		}

		public static string ButtonId(int nVerseIndex, int nConversationIndex, int nBtnIndex)
		{
			return String.Format("btn_{0}_{1}_{2}", nVerseIndex, nConversationIndex, nBtnIndex);
		}

		public const int CnBtnIndexDelete = 0;
		public const int CnBtnIndexHide = 1;
		public const int CnBtnIndexEndConversation = 2;

		public static string ButtonRowId(int nVerseIndex, int nConversationIndex)
		{
			return String.Format("btnrow_{0}_{1}", nVerseIndex, nConversationIndex);
		}

		public static string ConversationTableRowId(int nVerseIndex, int nConversationIndex)
		{
			return String.Format("convtblrow_{0}_{1}", nVerseIndex, nConversationIndex);
		}

		public const string CstrButtonLabelHide = "Hide";
		public const string CstrButtonLabelUnhide = "Unhide";
		public const string CstrButtonLabelConversationReopen = "Reopen conversation";
		public const string CstrButtonLabelConversationEnd = "End conversation";

		public bool IsEditable(int i, TeamMemberData.UserTypes eLoggedOnMember, CommInstance aCI)
		{
			return (i == (Count - 1))
				   && (!IsFinished)
				   && ((aCI.IsMentor && !IsWrongEditor(eLoggedOnMember, MentorRequiredEditor))
					   || (!aCI.IsMentor && !IsWrongEditor(eLoggedOnMember, MenteeRequiredEditor)));
		}

		public string Html(TeamMemberData.UserTypes eLoggedOnMember,
			int nVerseIndex, int nConversationIndex)
		{
			// r1: "Round: n"; "button"
			// r2-n: "Label:"; "value in textbox"
			string strRow = String.Format(Properties.Resources.HTML_TableCellWithSpanAndWidth, 100, 2,
				String.Format("{0}{1}", ConsultNoteControl.CstrRoundLabel, RoundNum));

			strRow += String.Format(Properties.Resources.HTML_TableCell,
					 String.Format(Properties.Resources.HTML_Button,
						ButtonId(nVerseIndex, nConversationIndex, CnBtnIndexDelete),
						"return OnClickDelete(this);",
						"Delete"));

			strRow += String.Format(Properties.Resources.HTML_TableCell,
					 String.Format(Properties.Resources.HTML_Button,
						ButtonId(nVerseIndex, nConversationIndex, CnBtnIndexHide),
						"return OnClickHide(this);",
						(Visible) ? CstrButtonLabelHide : CstrButtonLabelUnhide));

			strRow += String.Format(Properties.Resources.HTML_TableCell,
					 String.Format(Properties.Resources.HTML_Button,
						ButtonId(nVerseIndex, nConversationIndex, CnBtnIndexEndConversation),
						"return OnClickEndConversation(this);",
						(IsFinished) ? CstrButtonLabelConversationReopen : CstrButtonLabelConversationEnd));

			string strHtml = String.Format(Properties.Resources.HTML_TableRowId,
				ButtonRowId(nVerseIndex, nConversationIndex),
				strRow);

			string strHtmlTable = null;
			for (int i = 0; i < Count; i++)
			{
				CommInstance aCI = this[i];

				strRow = null;
				if (aCI.IsMentor)
					strRow += String.Format(Properties.Resources.HTML_TableCell,
											MentorLabel);
				else
					strRow += String.Format(Properties.Resources.HTML_TableCell,
											MenteeLabel);

				// only the last one is editable and then only if the right person is
				//  logged in
				if (IsEditable(i, eLoggedOnMember, aCI))
				{
					strRow += String.Format(Properties.Resources.HTML_TableCellForTextArea, "#FF0000",
											String.Format(Properties.Resources.HTML_Textarea,
														  TextareaId(nVerseIndex, nConversationIndex),
														  StoryData.CstrLangInternationalBtStyleClassName,
														  aCI));

					strHtmlTable += String.Format(Properties.Resources.HTML_TableRowId,
												  TextareaRowId(nVerseIndex, nConversationIndex),
												  strRow);
				}
				else
				{
					strRow += String.Format(Properties.Resources.HTML_TableCellWidth, 100,
											String.Format(Properties.Resources.HTML_TextareaReadonly,
											StoryData.CstrLangInternationalBtStyleClassName,
											aCI));

					strHtmlTable += String.Format(Properties.Resources.HTML_TableRowId,
												  TextareaReadonlyRowId(nVerseIndex, nConversationIndex, i),
												  strRow);
				}
			}

			string strEmbeddedTable = String.Format(Properties.Resources.HTML_Table,
													strHtmlTable);

			strHtml += String.Format(Properties.Resources.HTML_TableRowIdColor,
									 ConversationTableRowId(nVerseIndex, nConversationIndex),
									 "#CCFFAA",
									 String.Format(Properties.Resources.HTML_TableCellWithSpan, 5,
												   strEmbeddedTable));

			return strHtml;
		}

		public void IndexSearch(SearchForm.SearchLookInProperties findProperties,
			SearchForm.StringTransferSearchIndex lstBoxesToSearch)
		{
			foreach (CommInstance aCI in this)
				lstBoxesToSearch.AddNewVerseString(aCI, AssociatedPane);
		}
	}

	public class ConsultantNoteData : ConsultNoteDataConverter
	{
		public ConsultantNoteData(NewDataSet.ConsultantConversationRow aConRow)
			: base (aConRow.round, aConRow.guid,
			(aConRow.IsvisibleNull()) ? true : aConRow.visible,
			(aConRow.IsfinishedNull()) ? false : aConRow.finished)
		{
			NewDataSet.ConsultantNoteRow[] theNoteRows = aConRow.GetConsultantNoteRows();
			foreach (NewDataSet.ConsultantNoteRow aNoteRow in theNoteRows)
				Add(new CommInstance(aNoteRow.ConsultantNote_text,
					GetDirectionFromString(aNoteRow.Direction),
					aNoteRow.guid, (aNoteRow.IstimeStampNull()) ?
						DateTime.Now : aNoteRow.timeStamp));

			// make sure that there are at least two (we can't save them if they're empty)
			System.Diagnostics.Debug.Assert(Count != 0, "It looks like you have an empty Consultant Note field that shouldn't be there. For now, you can just 'Ignore' this error (but perhaps let bob_eaton@sall.com know)");
		}

		public ConsultantNoteData(int nRound, TeamMemberData.UserTypes eLoggedOnMember,
			TeamMemberData.UserTypes eMentorType, TeamMemberData.UserTypes eMenteeType,
			string strValue)
			: base(nRound)
		{
			InsureExtraBox(eLoggedOnMember, eMentorType, eMenteeType, strValue);
		}

		public ConsultantNoteData(ConsultNoteDataConverter rhs)
			: base(rhs)
		{
		}

		public override CommunicationDirections MentorDirection
		{
			get { return CommunicationDirections.eConsultantToProjFac; }
		}

		public override CommunicationDirections MenteeDirection
		{
			get { return CommunicationDirections.eProjFacToConsultant; }
		}

		public override string MentorLabel
		{
			get { return "cons:"; }
		}

		public override string MenteeLabel
		{
			get { return "prfc:"; }
		}

		protected override string InstanceElementName
		{
			get { return "ConsultantConversation"; }
		}

		protected override string SubElementName
		{
			get { return "ConsultantNote"; }
		}

		protected override bool IsWrongEditor(TeamMemberData.UserTypes eLoggedOnMember, TeamMemberData.UserTypes eRequiredEditor)
		{
			// if it's the *mentor* that we're supposed to be checking for... (this will get called for the mentee check
			//  as well, but the special case is only for FirstPassMentor; not ProjFac)
			if (eRequiredEditor == MentorRequiredEditor)
			{
				// ... and if the logged in member isn't that mentor, nor the First Pass
				//  Mentor, nor an independent consultant...
				if ((eLoggedOnMember != eRequiredEditor)
					&& (eLoggedOnMember != TeamMemberData.UserTypes.eFirstPassMentor)
					&& (eLoggedOnMember != TeamMemberData.UserTypes.eIndependentConsultant))
				{
					return true;
				}
			}

			// otherwise, let the base class handle it
			return base.IsWrongEditor(eLoggedOnMember, eRequiredEditor);
		}

		// override to allow for FirstPassMentor working as well in addition to CIT
		//  (or an independent consultant)
		public override void ThrowIfWrongEditor(TeamMemberData.UserTypes eLoggedOnMember, TeamMemberData.UserTypes eRequiredEditor)
		{
			// if it's the *mentor* that we're supposed to be checking for... (this will get called for the mentee check
			//  as well, but the special case is only for FirstPassMentor; not ProjFac)
			if (IsWrongEditor(eRequiredEditor, eRequiredEditor))
			{
				// then throw that it's the wrong editor
				throw new ApplicationException(String.Format("Only a '{0}', '{1}', or a '{2}' can edit this field",
															 TeamMemberData.GetMemberTypeAsDisplayString(
																 eRequiredEditor),
															 TeamMemberData.GetMemberTypeAsDisplayString(
																 TeamMemberData.UserTypes.eFirstPassMentor),
															 TeamMemberData.GetMemberTypeAsDisplayString(
																 TeamMemberData.UserTypes.eIndependentConsultant)));
			}
		}

		public override TeamMemberData.UserTypes MentorRequiredEditor
		{
			get { return TeamMemberData.UserTypes.eConsultantInTraining; }
		}

		public override TeamMemberData.UserTypes MenteeRequiredEditor
		{
			get { return TeamMemberData.UserTypes.eProjectFacilitator; }
		}

		protected override VerseData.ViewItemToInsureOn AssociatedPane
		{
			get { return VerseData.ViewItemToInsureOn.eConsultantNoteFields; }
		}
	}

	public class CoachNoteData : ConsultNoteDataConverter
	{
		public CoachNoteData(NewDataSet.CoachConversationRow aCoaCRow)
			: base (aCoaCRow.round, aCoaCRow.guid,
			(aCoaCRow.IsvisibleNull()) ? true : aCoaCRow.visible,
			(aCoaCRow.IsfinishedNull()) ? false : aCoaCRow.finished)
		{
			NewDataSet.CoachNoteRow[] theNoteRows = aCoaCRow.GetCoachNoteRows();
			foreach (NewDataSet.CoachNoteRow aNoteRow in theNoteRows)
				Add(new CommInstance(aNoteRow.CoachNote_text,
					GetDirectionFromString(aNoteRow.Direction),
					aNoteRow.guid,
					(aNoteRow.IstimeStampNull()) ? DateTime.Now :
					aNoteRow.timeStamp));
		}

		public CoachNoteData(int nRound, TeamMemberData.UserTypes eLoggedOnMember,
			TeamMemberData.UserTypes eMentorType, TeamMemberData.UserTypes eMenteeType,
			string strValue)
			: base (nRound)
		{
			InsureExtraBox(eLoggedOnMember, eMentorType, eMenteeType, strValue);
		}

		public CoachNoteData(ConsultNoteDataConverter rhs)
			: base(rhs)
		{
		}

		public override CommunicationDirections MentorDirection
		{
			get { return CommunicationDirections.eCoachToConsultant; }
		}

		public override CommunicationDirections MenteeDirection
		{
			get { return CommunicationDirections.eConsultantToCoach; }
		}

		public override string MentorLabel
		{
			get { return "coch:"; }
		}

		public override string MenteeLabel
		{
			get { return "cons:"; }
		}

		protected override string InstanceElementName
		{
			get { return "CoachConversation"; }
		}

		protected override string SubElementName
		{
			get { return "CoachNote"; }
		}

		public override TeamMemberData.UserTypes MentorRequiredEditor
		{
			get { return TeamMemberData.UserTypes.eCoach; }
		}

		public override TeamMemberData.UserTypes MenteeRequiredEditor
		{
			get { return TeamMemberData.UserTypes.eConsultantInTraining; }
		}

		protected override VerseData.ViewItemToInsureOn AssociatedPane
		{
			get { return VerseData.ViewItemToInsureOn.eCoachNotesFields; }
		}
	}

	public abstract class ConsultNotesDataConverter : List<ConsultNoteDataConverter>
	{
		protected string CollectionElementName = null;

		protected ConsultNotesDataConverter(string strCollectionElementName)
		{
			CollectionElementName = strCollectionElementName;
		}

		public bool HasData
		{
			get { return (this.Count > 0); }
		}

		public void InsureExtraBox(ConsultNoteDataConverter aCNDC, TeamMemberData.UserTypes eLoggedOnMemberType)
		{
			aCNDC.InsureExtraBox(eLoggedOnMemberType, MentorType, MenteeType, null);
		}

		// if the coach tries to add a note in the consultant's pane, that should fail.
		// (but it's okay for a project facilitator to add one if they have a question
		//  for the consultant)
		public virtual bool CheckAddNotePrivilege(StoryEditor theSE, TeamMemberData.UserTypes eLoggedOnMember)
		{
			if ((eLoggedOnMember != MentorType) && (eLoggedOnMember != MenteeType))
			{
				theSE.SetStatusBar("Error: " + String.Format("You must be logged in as a '{0}' or a '{1}' to add a note here",
					TeamMemberData.GetMemberTypeAsDisplayString(MentorType),
					TeamMemberData.GetMemberTypeAsDisplayString(MenteeType)));
				return false;
			}
			return true;
		}

		public abstract ConsultNoteDataConverter Add(int nRound,
			TeamMemberData.UserTypes eLoggedOnMember, string strValue);
		protected abstract TeamMemberData.UserTypes MentorType { get; }
		protected abstract TeamMemberData.UserTypes MenteeType { get; }

		public XElement GetXml
		{
			get
			{
				System.Diagnostics.Debug.Assert(HasData);
				XElement elemCNDC = new XElement(CollectionElementName);
				foreach (ConsultNoteDataConverter aCNDC in this)
					if (aCNDC.HasData)
						elemCNDC.Add(aCNDC.GetXml);
				return elemCNDC;
			}
		}

		public string Html(TeamMemberData.UserTypes eLoggedOnMember, bool bViewHidden,
			int nVerseIndex)
		{
			string strHtml = null;
			for (int i = 0; i < Count; i++)
			{
				ConsultNoteDataConverter aCNDC = this[i];
				if (aCNDC.Visible || bViewHidden)
					strHtml += aCNDC.Html(eLoggedOnMember, nVerseIndex, i);
			}

			return String.Format(Properties.Resources.HTML_TableRow,
					String.Format(Properties.Resources.HTML_TableCellWithSpan, 2,
						String.Format(Properties.Resources.HTML_TableNoBorder, strHtml)));
		}

		public void IndexSearch(SearchForm.SearchLookInProperties findProperties,
			ref SearchForm.StringTransferSearchIndex lstBoxesToSearch)
		{
			foreach (ConsultNoteDataConverter aCNDC in this)
				aCNDC.IndexSearch(findProperties, lstBoxesToSearch);
		}
	}

	public class ConsultantNotesData : ConsultNotesDataConverter
	{
		protected const string CstrCollectionElementName = "ConsultantNotes";

		public ConsultantNotesData(NewDataSet.verseRow theVerseRow, NewDataSet projFile)
			: base(CstrCollectionElementName)
		{
			NewDataSet.ConsultantNotesRow[] theConsultantNotesRows = theVerseRow.GetConsultantNotesRows();
			NewDataSet.ConsultantNotesRow theConsultantNotesRow;
			if (theConsultantNotesRows.Length == 0)
				theConsultantNotesRow = projFile.ConsultantNotes.AddConsultantNotesRow(theVerseRow);
			else
				theConsultantNotesRow = theConsultantNotesRows[0];

			foreach (NewDataSet.ConsultantConversationRow aConsultantConversationRow in theConsultantNotesRow.GetConsultantConversationRows())
				Add(new ConsultantNoteData(aConsultantConversationRow));
		}

		public ConsultantNotesData(IEnumerable<ConsultNoteDataConverter> rhs)
			: base(CstrCollectionElementName)
		{
			foreach (var aCND in rhs)
				Add(new ConsultantNoteData(aCND));
		}

		public ConsultantNotesData()
			: base(CstrCollectionElementName)
		{
		}

		public override ConsultNoteDataConverter Add(int nRound,
			TeamMemberData.UserTypes eLoggedOnMember, string strValue)
		{
			TeamMemberData.UserTypes eMentorType = MentorType;
			if (eLoggedOnMember == TeamMemberData.UserTypes.eFirstPassMentor)
				eMentorType = TeamMemberData.UserTypes.eFirstPassMentor;
			else if (eLoggedOnMember == TeamMemberData.UserTypes.eIndependentConsultant)
				eMentorType = TeamMemberData.UserTypes.eIndependentConsultant;

			ConsultNoteDataConverter theNewCN = new ConsultantNoteData(
				nRound, eLoggedOnMember, eMentorType, MenteeType, strValue);
			Add(theNewCN);
			return theNewCN;
		}

		// if the coach tries to add a note in the consultant's pane, that should fail.
		// (but it's okay for a project facilitator to add one if they have a question
		//  for the consultant).
		// The base class version is overridden here, because for the Consultant Pane, the 'First Pass Mentor' is
		//  also a valid note adder
		public override bool CheckAddNotePrivilege(StoryEditor theSE, TeamMemberData.UserTypes eLoggedOnMember)
		{
			if ((eLoggedOnMember != MentorType)
				&& (eLoggedOnMember != TeamMemberData.UserTypes.eFirstPassMentor)
				&& (eLoggedOnMember != TeamMemberData.UserTypes.eIndependentConsultant)
				&& (eLoggedOnMember != MenteeType))
			{
				theSE.SetStatusBar("Error: " + String.Format("You must be logged in as a '{0}', a '{1}', a '{2}', or a '{3}' to add a note here",
					TeamMemberData.GetMemberTypeAsDisplayString(MentorType),
					TeamMemberData.GetMemberTypeAsDisplayString(TeamMemberData.UserTypes.eFirstPassMentor),
					TeamMemberData.GetMemberTypeAsDisplayString(TeamMemberData.UserTypes.eIndependentConsultant),
					TeamMemberData.GetMemberTypeAsDisplayString(MenteeType)));
				return false;
			}
			return true;
		}

		protected override TeamMemberData.UserTypes MentorType
		{
			get { return TeamMemberData.UserTypes.eConsultantInTraining; }
		}

		protected override TeamMemberData.UserTypes MenteeType
		{
			get { return TeamMemberData.UserTypes.eProjectFacilitator; }
		}
	}

	public class CoachNotesData : ConsultNotesDataConverter
	{
		protected const string CstrCollectionElementName = "CoachNotes";

		public CoachNotesData(NewDataSet.verseRow theVerseRow, NewDataSet projFile)
			: base(CstrCollectionElementName)
		{
			NewDataSet.CoachNotesRow[] theCoachNotesRows = theVerseRow.GetCoachNotesRows();
			NewDataSet.CoachNotesRow theCoachNotesRow;
			if (theCoachNotesRows.Length == 0)
				theCoachNotesRow = projFile.CoachNotes.AddCoachNotesRow(theVerseRow);
			else
				theCoachNotesRow = theCoachNotesRows[0];

			foreach (NewDataSet.CoachConversationRow aCoachConversationRow in theCoachNotesRow.GetCoachConversationRows())
				Add(new CoachNoteData(aCoachConversationRow));
		}

		public CoachNotesData(IEnumerable<ConsultNoteDataConverter> rhs)
			: base(CstrCollectionElementName)
		{
			foreach (var aCND in rhs)
				Add(new CoachNoteData(aCND));
		}

		public CoachNotesData()
			: base(CstrCollectionElementName)
		{
		}

		public override ConsultNoteDataConverter Add(int nRound,
			TeamMemberData.UserTypes eLoggedOnMember, string strValue)
		{
			// always add closest to the verse label
			ConsultNoteDataConverter theNewCN = new CoachNoteData(
				nRound, eLoggedOnMember, MentorType, MenteeType, strValue);
			Add(theNewCN);
			return theNewCN;
		}

		protected override TeamMemberData.UserTypes MentorType
		{
			get { return TeamMemberData.UserTypes.eCoach; }
		}

		protected override TeamMemberData.UserTypes MenteeType
		{
			get { return TeamMemberData.UserTypes.eConsultantInTraining; }
		}
	}
}
