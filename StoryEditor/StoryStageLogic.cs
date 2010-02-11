using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Windows.Forms;

namespace OneStoryProjectEditor
{
	public class StoryStageLogic
	{
		// here are defines for the fixed states (i.e. those states that *have* to be in the system,
		//  because we have code that does something with them)
		public const string CstrFixedStateProjFacTypeVernacular = "ProjFacTypeVernacular";
		public const string CstrFixedStateProjFacTypeNationalBT = "ProjFacTypeNationalBT";
		public const string CstrFixedStateProjFacTypeInternationalBT = "ProjFacTypeInternationalBT";
		public const string CstrFixedStateProjFacAddAnchors = "ProjFacAddAnchors";
		public const string CstrFixedStateProjFacAddStoryQuestions = "ProjFacAddStoryQuestions";

		public const string CstrFixedStateBackTranslatorTypeInternationalBT = "BackTranslatorTypeInternationalBT";

		public const string CstrFixedStateConsultantCheckStoryQuestions = "ConsultantCheckStoryQuestions";

		public const string CstrFixedStateConsultantReviseRound1Notes = "ConsultantReviseRound1Notes";

		public const string CstrFixedStateProjFacOnlineReview1WithConsultant = "ProjFacOnlineReview1WithConsultant";
		public const string CstrFixedStateProjFacReadyForTest1 = "ProjFacReadyForTest1";
		public const string CstrFixedStateProjFacEnterAnswersToStoryQuestionsOfTest1 = "ProjFacEnterAnswersToStoryQuestionsOfTest1";
		public const string CstrFixedStateProjFacEnterRetellingOfTest1 = "ProjFacEnterRetellingOfTest1";

		public const string CstrFixedStateCoachReviewRound2Notes = "CoachReviewRound2Notes";
		public const string CstrFixedStateConsultantReviseRound2Notes = "ConsultantReviseRound2Notes";

		public const string CstrFixedStateProjFacOnlineReview2WithConsultant = "ProjFacOnlineReview2WithConsultant";
		public const string CstrFixedStateProjFacReadyForTest2 = "ProjFacReadyForTest2";
		public const string CstrFixedStateProjFacEnterAnswersToStoryQuestionsOfTest2 = "ProjFacEnterAnswersToStoryQuestionsOfTest2";

		public const string CstrFixedStateCoachReviewTest2Notes = "CoachReviewTest2Notes";

		public const string CstrFixedStateTeamComplete = "TeamComplete";

		protected string ProjectStage { get; set; }
		internal StateTransitionMap StateTransitions { get; set; }

		public StoryStageLogic()
		{
			StateTransitions = new StateTransitionMap();
			StateTransitions.InitStateTransitionsFromXml();
		}

		public StoryStageLogic(NewDataSet.ProjectStatesRow theProjectStateRow, NewDataSet projFile)
		{
			StateTransitions = SerializeStateTransitions(theProjectStateRow, projFile);
		}

		public static StateTransitionMap SerializeStateTransitions(NewDataSet.ProjectStatesRow theProjectStateRow, NewDataSet projFile)
		{
			StateTransitionMap sts = new StateTransitionMap();
			foreach (NewDataSet.StateTransitionRow aSTRow in theProjectStateRow.GetStateTransitionRows())
			{
				StageRequirements stageRequirements = null;
				if (aSTRow.GetStageRequirementsRows().Length == 1)
				{
					NewDataSet.StageRequirementsRow theStageRequirementRow = aSTRow.GetStageRequirementsRows()[0];
					stageRequirements = new StageRequirements
					{
						HasEndOfStateFunction = (theStageRequirementRow.IsHasEndOfStateFunctionNull()) ? false : theStageRequirementRow.HasEndOfStateFunction,
						UsingOtherEnglishBTer = (theStageRequirementRow.IsRequiresUsingOtherEnglishBTerNull()) ? false : theStageRequirementRow.RequiresUsingOtherEnglishBTer,
						RequiresFirstPassMentor = (theStageRequirementRow.IsRequiresFirstPassMentorNull()) ? false : theStageRequirementRow.RequiresFirstPassMentor,
						RequiresIndependentConsultant = (theStageRequirementRow.IsRequiresIndependentConsultantNull()) ? false : theStageRequirementRow.RequiresIndependentConsultant,
						RequiresUsingVernacular = (theStageRequirementRow.IsRequiresUsingVernacularNull()) ? false : theStageRequirementRow.RequiresUsingVernacular,
						RequiresUsingNationalBT = (theStageRequirementRow.IsRequiresUsingNationalBTNull()) ? false : theStageRequirementRow.RequiresUsingNationalBT,
						RequiresUsingEnglishBT = (theStageRequirementRow.IsRequiresUsingEnglishBTNull()) ? false : theStageRequirementRow.RequiresUsingEnglishBT,
						RequiresBiblicalStory = (theStageRequirementRow.IsRequiresBiblicalStoryNull()) ? false : theStageRequirementRow.RequiresBiblicalStory
					};
				}

				TeamMemberData.UserTypes eMemberType = (TeamMemberData.UserTypes)Enum.Parse(typeof(TeamMemberData.UserTypes), aSTRow.MemberTypeWithEditToken);

				StateTransition st = new StateTransition(aSTRow.StageName, eMemberType, aSTRow.StageDisplayString,
					aSTRow.StageInstructions, stageRequirements);

				if (aSTRow.GetViewSettingsRows().Length == 1)
				{
					NewDataSet.ViewSettingsRow theViewSettingsRow = aSTRow.GetViewSettingsRows()[0];
					st.IsAnchorVisible = theViewSettingsRow.viewAnchorField;
					st.IsCoachNotesVisible = theViewSettingsRow.viewCoachNotesField;
					st.IsConsultantNotesVisible = theViewSettingsRow.viewConsultantNoteField;
					st.IsEnglishBTVisible = theViewSettingsRow.viewEnglishBTField;
					st.IsNationalBTVisible = theViewSettingsRow.viewNationalLangField;
					st.IsNetBibleVisible = theViewSettingsRow.viewNetBible;
					st.IsRetellingVisible = theViewSettingsRow.viewRetellingField;
					st.IsStoryTestingQuestion = theViewSettingsRow.viewStoryTestingQuestionField;
					st.IsVernacularVisible = theViewSettingsRow.viewVernacularLangField;
				}

				sts.Add(aSTRow.StageName, st);
			}

			return sts;
		}


		// protected ProjectStageList _ProjectStages = new ProjectStageList();
		// internal static StateTransitions stateTransitions = new StateTransitions();

	public class ProjectStageList : List<string>
	{
		/*
		eUndefined = 0,
		eProjFacTypeVernacular,
		eProjFacTypeNationalBT,
		eProjFacTypeInternationalBT,
		eProjFacAddAnchors,
		eProjFacAddStoryQuestions,
		eBackTranslatorTypeInternationalBT,
		eConsultantCheckNonBiblicalStory,
		eFirstPassMentorCheck1,
		eConsultantCheckStoryInfo,
		eConsultantCheckAnchors,
		eConsultantCheckStoryQuestions,
		eCoachReviewRound1Notes,
		eConsultantReviseRound1Notes,
		eBackTranslatorTranslateConNotes,
		eProjFacReviseBasedOnRound1Notes,
		eProjFacOnlineReview1WithConsultant,
		eProjFacReadyForTest1,
		eProjFacEnterAnswersToStoryQuestionsOfTest1,
		eProjFacEnterRetellingOfTest1,
		eFirstPassMentorCheck2,
		eConsultantCheck2,
		eCoachReviewRound2Notes,
		eConsultantReviseRound2Notes,
		eBackTranslatorTranslateConNotes2,
		eProjFacReviseBasedOnRound2Notes,
		eProjFacOnlineReview2WithConsultant,
		eProjFacReadyForTest2,
		eProjFacEnterAnswersToStoryQuestionsOfTest2,
		eProjFacEnterRetellingOfTest2,
		eFirstPassMentorReviewTest2,
		eConsultantReviewTest2,
		eCoachReviewTest2Notes,
		eTeamComplete
		*/
	}
		/*
		public ProjectStageList ProjectStages
		{
			get { return _ProjectStages; }
			set { _ProjectStages = value; }
		}
		*/

		public static string GetInitialStoryStage(ProjectSettings projSettings)
		{
			System.Diagnostics.Debug.Assert(projSettings.Vernacular.HasData
				|| projSettings.NationalBT.HasData
				|| projSettings.InternationalBT.HasData);

			// the first state is either eProjFacTypeVernacular or eProjFacTypeNationalBT or eProjFacTypeInternationalBT
			//  (can't have a separate EngBTr if there's no Vernacular or National BT)
			return (projSettings.Vernacular.HasData)
				? CstrFixedStateProjFacTypeVernacular
				: (projSettings.NationalBT.HasData)
					? CstrFixedStateProjFacTypeNationalBT
					: CstrFixedStateProjFacTypeInternationalBT;
		}
		/*
		public StoryStageLogic(string strProjectStage)
		{
			ProjectStage = GetProjectStageFromString(strProjectStage);
		}
		*/
		public StoryStageLogic(StoryStageLogic rhs)
		{
			ProjectStage = rhs.ProjectStage;
		}
		/*
		protected ProjectStages GetProjectStageFromString(string strProjectStageString)
		{
			System.Diagnostics.Debug.Assert(CmapStageStringToEnumType.ContainsKey(strProjectStageString));
			return CmapStageStringToEnumType[strProjectStageString];
		}
		*/
		public bool IsChangeOfStateAllowed(TeamMemberData loggedOnMember)
		{
			bool bRet = ((loggedOnMember.MemberType == TeamMemberData.UserTypes.eJustLooking)
				|| (loggedOnMember.MemberType == MemberTypeWithEditToken));

			if (!bRet)
				MessageBox.Show(
					String.Format(Properties.Resources.IDS_WhichUserEdits,
								  TeamMemberData.GetMemberTypeAsDisplayString(MemberTypeWithEditToken)),
					Properties.Resources.IDS_Caption);

			return bRet;
		}

		public bool IsEditAllowed(TeamMemberData loggedOnMember)
		{
			return (MemberTypeWithEditToken == loggedOnMember.MemberType);
		}

		public ApplicationException WrongMemberTypeEx
		{
			get {
				return
					new ApplicationException(String.Format(Properties.Resources.IDS_WhichUserEdits,
														   TeamMemberData.GetMemberTypeAsDisplayString(
															   MemberTypeWithEditToken))); }
		}

		// this isn't 100% effective. Sometimes a particular stage can have a single (but varied) editors
		//  (e.g. the Online consult could either be the ProjFac or the consultant)
		public TeamMemberData.UserTypes MemberTypeWithEditToken
		{
			get
			{
				StateTransition st = StateTransitions[ProjectStage];
				return st.MemberTypeWithEditToken;
			}
		}

		public bool IsTerminalTransition(string strNextStage)
		{
			StateTransition stThis = StateTransitions[ProjectStage];
			StateTransition stNext = StateTransitions[strNextStage];
			return (stThis.MemberTypeWithEditToken != stNext.MemberTypeWithEditToken);
		}
		/*
		public override string ToString()
		{
			return _ProjectStage.ToString().Substring(1);
		}

		protected static Dictionary<string, ProjectStages> CmapStageStringToEnumType = new Dictionary<string, ProjectStages>() {
			{ "ProjFacTypeVernacular", ProjectStages.eProjFacTypeVernacular },
			{ "ProjFacTypeNationalBT", ProjectStages.eProjFacTypeNationalBT },
			{ "ProjFacTypeInternationalBT", ProjectStages.eProjFacTypeInternationalBT },
			{ "ProjFacAddAnchors", ProjectStages.eProjFacAddAnchors },
			{ "ProjFacAddStoryQuestions", ProjectStages.eProjFacAddStoryQuestions },
			{ "BackTranslatorTypeInternationalBT", ProjectStages.eBackTranslatorTypeInternationalBT },
			{ "ConsultantCheckNonBiblicalStory", ProjectStages.eConsultantCheckNonBiblicalStory },
			{ "FirstPassMentorCheck1", ProjectStages.eFirstPassMentorCheck1 },
			{ "ConsultantCheckStoryInfo", ProjectStages.eConsultantCheckStoryInfo },
			{ "ConsultantCheckAnchors", ProjectStages.eConsultantCheckAnchors },
			{ "ConsultantCheckStoryQuestions", ProjectStages.eConsultantCheckStoryQuestions },
			{ "CoachReviewRound1Notes", ProjectStages.eCoachReviewRound1Notes },
			{ "ConsultantReviseRound1Notes", ProjectStages.eConsultantReviseRound1Notes },
			{ "BackTranslatorTranslateConNotes", ProjectStages.eBackTranslatorTranslateConNotes },
			{ "ProjFacReviseBasedOnRound1Notes", ProjectStages.eProjFacReviseBasedOnRound1Notes },
			{ "ProjFacOnlineReview1WithConsultant", ProjectStages.eProjFacOnlineReview1WithConsultant },
			{ "ProjFacReadyForTest1", ProjectStages.eProjFacReadyForTest1 },
			{ "ProjFacEnterAnswersToStoryQuestionsOfTest1", ProjectStages.eProjFacEnterAnswersToStoryQuestionsOfTest1 },
			{ "ProjFacEnterRetellingOfTest1", ProjectStages.eProjFacEnterRetellingOfTest1 },
			{ "FirstPassMentorCheck2", ProjectStages.eFirstPassMentorCheck2 },
			{ "ConsultantCheck2", ProjectStages.eConsultantCheck2 },
			{ "CoachReviewRound2Notes", ProjectStages.eCoachReviewRound2Notes },
			{ "ConsultantReviseRound2Notes", ProjectStages.eConsultantReviseRound2Notes },
			{ "BackTranslatorTranslateConNotes2", ProjectStages.eBackTranslatorTranslateConNotes2 },
			{ "ProjFacReviseBasedOnRound2Notes", ProjectStages.eProjFacReviseBasedOnRound2Notes },
			{ "ProjFacOnlineReview2WithConsultant", ProjectStages.eProjFacOnlineReview2WithConsultant },
			{ "ProjFacReadyForTest2", ProjectStages.eProjFacReadyForTest2 },
			{ "ProjFacEnterAnswersToStoryQuestionsOfTest2", ProjectStages.eProjFacEnterAnswersToStoryQuestionsOfTest2 },
			{ "ProjFacEnterRetellingOfTest2", ProjectStages.eProjFacEnterRetellingOfTest2 },
			{ "FirstPassMentorReviewTest2", ProjectStages.eFirstPassMentorReviewTest2 },
			{ "ConsultantReviewTest2", ProjectStages.eConsultantReviewTest2 },
			{ "CoachReviewTest2Notes", ProjectStages.eCoachReviewTest2Notes },
			{ "TeamComplete", ProjectStages.eTeamComplete }};
		*/

		public class StateTransitionMap : Dictionary<string, StateTransition>
		{
			protected const string CstrStateTransitionsXmlFilename = "StageTransitions.xml";
			public ProjectStageList ProjectStates = new ProjectStageList();

			protected string PathToXmlFile
			{
				get
				{
					// try the same folder as we're executing out of
					string strFileToCheck = Path.Combine(StoryProjectData.GetRunningFolder,
						CstrStateTransitionsXmlFilename);
#if DEBUG
					if (!File.Exists(strFileToCheck))
						// on dev machines, this file is in the "..\..\src\EC\TECkit Mapping Editor" folder
						strFileToCheck = @"C:\src\StoryEditor\StoryEditor\" + CstrStateTransitionsXmlFilename;
#endif
					System.Diagnostics.Debug.Assert(File.Exists(strFileToCheck), String.Format("Can't find: {0}! You'll need to re-install or contact bob_eaton@sall.com", strFileToCheck));

					return strFileToCheck;
				}
			}

			/* rde: this code works just fine, it's just that we don't ever write this file anymore
			public void SaveStates(string strFilename)
			{
				// create the root portions of the XML document and tack on the fragment we've been building
				XDocument doc = new XDocument(
					new XDeclaration("1.0", "utf-8", "yes"),
					GetXml);

				// save it with an extra extn.
				doc.Save(strFilename);
			}

			protected XElement GetXml
			{
				get
				{
					XElement elem = new XElement("ProjectStates");
					foreach (KeyValuePair<ProjectStages, StateTransition> kvp in this)
					{
						elem.Add(kvp.Value.GetXml);
					}
					return elem;
				}
			}
			*/

			// these are for reading the file
			protected void GetXmlDocument(out XmlDocument doc, out XPathNavigator navigator, out XmlNamespaceManager manager)
			{
				doc = new XmlDocument();
				doc.Load(PathToXmlFile);
				navigator = doc.CreateNavigator();
				manager = new XmlNamespaceManager(navigator.NameTable);
			}

			public void InitStateTransitionsFromXml()
			{
				try
				{
					XmlDocument doc;
					XPathNavigator navigator;
					XmlNamespaceManager manager;
					GetXmlDocument(out doc, out navigator, out manager);

					XPathNodeIterator xpStageTransition = navigator.Select("/ProjectStates/StateTransition", manager);
					while (xpStageTransition.MoveNext())
					{
						string strThisStage = xpStageTransition.Current.GetAttribute("StageName", navigator.NamespaceURI);
						TeamMemberData.UserTypes eMemberType = (TeamMemberData.UserTypes)Enum.Parse(typeof(TeamMemberData.UserTypes), xpStageTransition.Current.GetAttribute("MemberTypeWithEditToken", navigator.NamespaceURI));

						XPathNodeIterator xpNextElement = xpStageTransition.Current.Select("StageDisplayString");
						string strStageDisplayString = null;
						if (xpNextElement.MoveNext())
							strStageDisplayString = xpNextElement.Current.Value;

						System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(strStageDisplayString));

						xpNextElement = xpStageTransition.Current.Select("StageInstructions");
						string strStageInstructions = null;
						if (xpNextElement.MoveNext())
							strStageInstructions = xpNextElement.Current.Value;

						System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(strStageInstructions));

						xpNextElement = xpStageTransition.Current.Select("StageRequirements");
						StageRequirements StageRequirements = null;
						if (xpNextElement.MoveNext())
						{
							StageRequirements = new StageRequirements
							{
								HasEndOfStateFunction = (xpNextElement.Current.GetAttribute("HasEndOfStateFunction", navigator.NamespaceURI) == "true"),
								UsingOtherEnglishBTer = xpNextElement.Current.GetAttribute("RequiresUsingOtherEnglishBTer", navigator.NamespaceURI),
								RequiresFirstPassMentor = (xpNextElement.Current.GetAttribute("RequiresFirstPassMentor", navigator.NamespaceURI) == "true"),
								RequiresIndependentConsultant = (xpNextElement.Current.GetAttribute("RequiresIndependentConsultant", navigator.NamespaceURI) == "true"),
								RequiresUsingVernacular = (xpNextElement.Current.GetAttribute("RequiresUsingVernacular", navigator.NamespaceURI) == "true"),
								RequiresUsingNationalBT = (xpNextElement.Current.GetAttribute("RequiresUsingNationalBT", navigator.NamespaceURI) == "true"),
								RequiresUsingEnglishBT = (xpNextElement.Current.GetAttribute("RequiresUsingEnglishBT", navigator.NamespaceURI) == "true"),
								RequiresBiblicalStory = (xpNextElement.Current.GetAttribute("RequiresBiblicalStory", navigator.NamespaceURI) == "true")
							};
						}

						StateTransition st = new StateTransition(strThisStage, eMemberType,
							strStageDisplayString, strStageInstructions, StageRequirements);

						xpNextElement = xpStageTransition.Current.Select("ViewSettings");
						if (xpNextElement.MoveNext())
						{
							st.IsVernacularVisible = (xpNextElement.Current.GetAttribute("viewVernacularLangField", navigator.NamespaceURI) == "true");
							st.IsNationalBTVisible = (xpNextElement.Current.GetAttribute("viewNationalLangField", navigator.NamespaceURI) == "true");
							st.IsEnglishBTVisible = (xpNextElement.Current.GetAttribute("viewEnglishBTField", navigator.NamespaceURI) == "true");
							st.IsAnchorVisible = (xpNextElement.Current.GetAttribute("viewAnchorField", navigator.NamespaceURI) == "true");
							st.IsStoryTestingQuestion = (xpNextElement.Current.GetAttribute("viewStoryTestingQuestionField", navigator.NamespaceURI) == "true");
							st.IsRetellingVisible = (xpNextElement.Current.GetAttribute("viewRetellingField", navigator.NamespaceURI) == "true");
							st.IsConsultantNotesVisible = (xpNextElement.Current.GetAttribute("viewConsultantNoteField", navigator.NamespaceURI) == "true");
							st.IsCoachNotesVisible = (xpNextElement.Current.GetAttribute("viewCoachNotesField", navigator.NamespaceURI) == "true");
							st.IsNetBibleVisible = (xpNextElement.Current.GetAttribute("viewNetBible", navigator.NamespaceURI) == "true");
						}

						Add(strThisStage, st);
					}
				}
				catch (Exception ex)
				{
					throw new ApplicationException(String.Format("Unable to process the xml file containing the State Transitions (i.e. {0})... Reinstall.", CstrStateTransitionsXmlFilename), ex);
				}
			}
		}

		public class StageRequirements
		{
			public bool HasEndOfStateFunction { get; set; }
			public bool RequiresUsingVernacular { get; set; }
			public bool RequiresUsingNationalBT { get; set; }
			public bool RequiresUsingEnglishBT { get; set; }
			public bool HasUsingOtherEnglishBTer { get; set; }
			public bool RequiresUsingOtherEnglishBTer { get; set; }
			public bool RequiresFirstPassMentor { get; set; }
			public bool RequiresIndependentConsultant { get; set; }
			public bool RequiresBiblicalStory { get; set; }

			public object UsingOtherEnglishBTer
			{
				set
				{
					string strValue = (string)value;
					if (String.IsNullOrEmpty(strValue))
						HasUsingOtherEnglishBTer = false;
					else
					{
						HasUsingOtherEnglishBTer = true;
						RequiresUsingOtherEnglishBTer = (strValue == "true");
					}
				}
			}

			public XElement GetXml
			{
				get
				{
					XElement elem = new XElement("StageRequirements");

					if (HasEndOfStateFunction)
						elem.Add(new XAttribute("HasEndOfStateFunction", HasEndOfStateFunction));

					if (RequiresUsingVernacular)
						elem.Add(new XAttribute("RequiresUsingVernacular", RequiresUsingVernacular));

					if (RequiresUsingNationalBT)
						elem.Add(new XAttribute("RequiresUsingNationalBT", RequiresUsingNationalBT));

					if (RequiresUsingEnglishBT)
						elem.Add(new XAttribute("RequiresUsingEnglishBT", RequiresUsingEnglishBT));

					if (HasUsingOtherEnglishBTer)
						elem.Add(new XAttribute("HasEndOfStateFunction", RequiresUsingOtherEnglishBTer));

					if (RequiresFirstPassMentor)
						elem.Add(new XAttribute("RequiresFirstPassMentor", RequiresFirstPassMentor));

					if (RequiresIndependentConsultant)
						elem.Add(new XAttribute("RequiresIndependentConsultant", RequiresIndependentConsultant));

					if (RequiresBiblicalStory)
						elem.Add(new XAttribute("RequiresBiblicalStory", RequiresBiblicalStory));

					return elem;
				}
			}
		}

		public class StateTransition
		{
			public string CurrentStage { get; set; }
			public string NextState { get; set; }
			public StageRequirements StageRequirements { get; set; }
			public TeamMemberData.UserTypes MemberTypeWithEditToken = TeamMemberData.UserTypes.eUndefined;
			public string StageDisplayString { get; set; }
			public string StageInstructions { get; set; }
#if !DataDllBuild
			public CheckEndOfStateTransition.CheckForValidEndOfState IsReadyForTransition;
#endif

			public bool IsVernacularVisible { get; set; }
			public bool IsNationalBTVisible { get; set; }
			public bool IsEnglishBTVisible { get; set; }
			public bool IsAnchorVisible { get; set; }
			public bool IsStoryTestingQuestion { get; set; }
			public bool IsRetellingVisible { get; set; }
			public bool IsConsultantNotesVisible { get; set; }
			public bool IsCoachNotesVisible { get; set; }
			public bool IsNetBibleVisible { get; set; }

			public StateTransition
				(
					string strThisStage,
					TeamMemberData.UserTypes eMemberTypeWithEditToken,
					string strDisplayString,
					string strInstructions,
					StageRequirements stageRequirements
				)
			{
				CurrentStage = strThisStage;
				MemberTypeWithEditToken = eMemberTypeWithEditToken;
				StageDisplayString = strDisplayString;
				StageInstructions = strInstructions;
				StageRequirements = stageRequirements;
				if (StageRequirements.HasEndOfStateFunction)
				{
					string strMethodName = strThisStage.Substring(1);

#if !DataDllBuild
					IsReadyForTransition = (CheckEndOfStateTransition.CheckForValidEndOfState)Delegate.CreateDelegate(
						typeof(CheckEndOfStateTransition.CheckForValidEndOfState),
						typeof(CheckEndOfStateTransition), strMethodName);
#endif
				}
			}
			/*
			public bool IsTransitionValid(string strToStage)
			{
				foreach (AllowablePreviousStateWithConditions aps in AllowableBackwardsTransitions)
					if (aps.ProjectStage == strToStage)
						return true;
				return false;
			}
			*/
#if !DataDllBuild
			public void SetView(StoryEditor theSE)
			{
				// the vern should be visible if it is configured and either it's supposed to be visible (based on the
				//  state information) OR it's the only one in existance--i.e. an English story project)
				theSE.viewVernacularLangFieldMenuItem.Checked = (theSE.StoryProject.ProjSettings.Vernacular.HasData
					&& (IsVernacularVisible
						|| (!theSE.StoryProject.ProjSettings.NationalBT.HasData
						&& !theSE.StoryProject.ProjSettings.InternationalBT.HasData)));

				// the National BT should be visible if it is configured and either it's supposed to be visible (based on the
				//  state information) OR it's the only one in existance--i.e. an EnglishBT-only project)
				theSE.viewNationalLangFieldMenuItem.Checked = (theSE.StoryProject.ProjSettings.NationalBT.HasData
					&& (IsNationalBTVisible
						|| (!theSE.StoryProject.ProjSettings.Vernacular.HasData
						&& !theSE.StoryProject.ProjSettings.InternationalBT.HasData)));

				// the English BT should be visible if it is configured and either it's supposed to be visible (based on the
				//  state information) OR it's the only one in existance--i.e. an EnglishBT-only project)
				theSE.viewEnglishBTFieldMenuItem.Checked = (theSE.StoryProject.ProjSettings.InternationalBT.HasData
					&&  (IsEnglishBTVisible
						|| (!theSE.StoryProject.ProjSettings.Vernacular.HasData
						&& !theSE.StoryProject.ProjSettings.NationalBT.HasData)));

				theSE.viewAnchorFieldMenuItem.Checked = IsAnchorVisible;
				theSE.viewStoryTestingQuestionFieldMenuItem.Checked = IsStoryTestingQuestion;
				theSE.viewRetellingFieldMenuItem.Checked = IsRetellingVisible;
				theSE.viewConsultantNoteFieldMenuItem.Checked = IsConsultantNotesVisible;
				theSE.viewCoachNotesFieldMenuItem.Checked = IsCoachNotesVisible;
				theSE.viewNetBibleMenuItem.Checked = IsNetBibleVisible;
			}
#endif

			public XElement GetXml
			{
				get
				{
					XElement elem = new XElement("StateTransition",
						new XAttribute("StageName", CurrentStage),
						new XAttribute("MemberTypeWithEditToken", MemberTypeWithEditToken),
						new XElement("StageDisplayString", StageDisplayString),
						new XElement("StageInstructions", StageInstructions),
						new XElement("ViewSettings",
							new XAttribute("viewVernacularLangFieldMenuItem", IsVernacularVisible),
							new XAttribute("viewNationalLangFieldMenuItem", IsNationalBTVisible),
							new XAttribute("viewEnglishBTFieldMenuItem", IsEnglishBTVisible),
							new XAttribute("viewAnchorFieldMenuItem", IsAnchorVisible),
							new XAttribute("viewStoryTestingQuestionFieldMenuItem", IsStoryTestingQuestion),
							new XAttribute("viewRetellingFieldMenuItem", IsRetellingVisible),
							new XAttribute("viewConsultantNoteFieldMenuItem", IsConsultantNotesVisible),
							new XAttribute("viewCoachNotesFieldMenuItem", IsCoachNotesVisible),
							new XAttribute("viewNetBibleMenuItem", IsNetBibleVisible)),
						StageRequirements.GetXml);

					return elem;
				}
			}
		}
	}
}
