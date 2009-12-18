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
				List<AllowablePreviousStateWithConditions> lstAllowableBackwardsStages = new List<AllowablePreviousStateWithConditions>();
				if (aSTRow.GetAllowableBackwardsTransitionsRows().Length == 1)
				{
					NewDataSet.AllowableBackwardsTransitionsRow theAllowableBackwardsTransitionsRow = aSTRow.GetAllowableBackwardsTransitionsRows()[0];
					foreach (NewDataSet.AllowableBackwardsTransitionRow aABTRow in theAllowableBackwardsTransitionsRow.GetAllowableBackwardsTransitionRows())
					{
						System.Diagnostics.Debug.Assert(!sts.ProjectStates.Contains(aABTRow.AllowableBackwardsTransition_text));
						sts.ProjectStates.Add(aABTRow.AllowableBackwardsTransition_text);

						AllowablePreviousStateWithConditions aps = new AllowablePreviousStateWithConditions
						{
							RequiresUsingVernacular = (aABTRow.IsRequiresUsingVernacularNull()) ? false : aABTRow.RequiresUsingVernacular,
							RequiresUsingNationalBT = (aABTRow.IsRequiresUsingNationalBTNull()) ? false : aABTRow.RequiresUsingNationalBT,
							RequiresUsingEnglishBT = (aABTRow.IsRequiresUsingEnglishBTNull()) ? false : aABTRow.RequiresUsingEnglishBT,
							UsingOtherEnglishBTer = (aABTRow.IsRequiresUsingOtherEnglishBTerNull()) ? false : aABTRow.RequiresUsingOtherEnglishBTer,
							RequiresBiblicalStory = (aABTRow.IsRequiresBiblicalStoryNull()) ? false : aABTRow.RequiresBiblicalStory,
							RequiresFirstPassMentor = (aABTRow.IsRequiresFirstPassMentorNull()) ? false : aABTRow.RequiresFirstPassMentor
						};
						lstAllowableBackwardsStages.Add(aps);
					}
				}

				TeamMemberData.UserTypes eMemberType = (TeamMemberData.UserTypes)Enum.Parse(typeof(TeamMemberData.UserTypes), aSTRow.MemberTypeWithEditToken);

				StateTransition st = new StateTransition(aSTRow.stage, aSTRow.NextState, eMemberType, aSTRow.StageDisplayString,
					aSTRow.StageInstructions, aSTRow.FixedState, lstAllowableBackwardsStages);

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

				sts.Add(aSTRow.stage, st);
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
				MessageBox.Show(String.Format("Right now, only a '{0}' can change the state of this story. If you're a {0}, click 'Project', 'Settings' to login. You can log in as a 'Just Looking' member to be able to transition to any state, but without edit privileges.", TeamMemberData.GetMemberTypeAsDisplayString(MemberTypeWithEditToken)),  Properties.Resources.IDS_Caption);

			return bRet;
		}

		public bool IsEditAllowed(TeamMemberData loggedOnMember)
		{
			return (MemberTypeWithEditToken == loggedOnMember.MemberType);
		}

		public ApplicationException WrongMemberTypeEx
		{
			get { return new ApplicationException(String.Format("Right now, this story is in a state where only a '{0}' should be editing it. If you're a {0}, click 'Project', 'Settings' to login", TeamMemberData.GetMemberTypeAsDisplayString(MemberTypeWithEditToken))); }
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
						string strThisStage = xpStageTransition.Current.GetAttribute("stage", navigator.NamespaceURI);
						TeamMemberData.UserTypes eMemberType = (TeamMemberData.UserTypes)Enum.Parse(typeof(TeamMemberData.UserTypes), xpStageTransition.Current.GetAttribute("MemberTypeWithEditToken", navigator.NamespaceURI));

						string strNextStage = xpStageTransition.Current.GetAttribute("NextState", navigator.NamespaceURI);
						bool bIsFixedState = (xpStageTransition.Current.GetAttribute("FixedState", navigator.NamespaceURI) == "true");
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

						xpNextElement = xpStageTransition.Current.Select("AllowableBackwardsTransitions/AllowableBackwardsTransition");
						List<AllowablePreviousStateWithConditions> lstAllowableBackwardsStages = new List<AllowablePreviousStateWithConditions>();
						while (xpNextElement.MoveNext())
						{
							AllowablePreviousStateWithConditions aps = new AllowablePreviousStateWithConditions
							{
								ProjectStage = xpNextElement.Current.Value,
								RequiresUsingVernacular = (xpNextElement.Current.GetAttribute("RequiresUsingVernacular", navigator.NamespaceURI) == "true"),
								RequiresUsingNationalBT = (xpNextElement.Current.GetAttribute("RequiresUsingNationalBT", navigator.NamespaceURI) == "true"),
								RequiresUsingEnglishBT = (xpNextElement.Current.GetAttribute("RequiresUsingEnglishBT", navigator.NamespaceURI) == "true"),
								UsingOtherEnglishBTer = xpNextElement.Current.GetAttribute("RequiresUsingOtherEnglishBTer", navigator.NamespaceURI),
								RequiresBiblicalStory = (xpNextElement.Current.GetAttribute("RequiresBiblicalStory", navigator.NamespaceURI) == "true"),
								RequiresFirstPassMentor = (xpNextElement.Current.GetAttribute("RequiresFirstPassMentor", navigator.NamespaceURI) == "true")
							};
							lstAllowableBackwardsStages.Add(aps);
						}

						StateTransition st = new StateTransition(strThisStage, strNextStage, eMemberType,
							strStageDisplayString, strStageInstructions, bIsFixedState, lstAllowableBackwardsStages);

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

		public class AllowablePreviousStateWithConditions
		{
			public string ProjectStage { get; set; }
			public bool RequiresUsingVernacular { get; set; }
			public bool RequiresUsingNationalBT { get; set; }
			public bool RequiresUsingEnglishBT { get; set; }
			public bool HasUsingOtherEnglishBTer { get; set; }
			public bool RequiresUsingOtherEnglishBTer { get; set; }
			public bool RequiresBiblicalStory { get; set; }
			public bool RequiresFirstPassMentor { get; set; }
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
		}

		public class StateTransition
		{
			internal string CurrentStage;
			internal string NextState;
			internal List<AllowablePreviousStateWithConditions> AllowableBackwardsTransitions = new List<AllowablePreviousStateWithConditions>();
			internal TeamMemberData.UserTypes MemberTypeWithEditToken = TeamMemberData.UserTypes.eUndefined;
			internal string StageDisplayString;
			internal string StageInstructions;
			internal bool IsFixedState; // means that the state name can't be changed and it has a IsReadyForTransition function
#if !DataDllBuild
			public CheckEndOfStateTransition.CheckForValidEndOfState IsReadyForTransition;
#endif

			internal bool IsVernacularVisible { get; set; }
			internal bool IsNationalBTVisible { get; set; }
			internal bool IsEnglishBTVisible { get; set; }
			internal bool IsAnchorVisible { get; set; }
			internal bool IsStoryTestingQuestion { get; set; }
			internal bool IsRetellingVisible { get; set; }
			internal bool IsConsultantNotesVisible { get; set; }
			internal bool IsCoachNotesVisible { get; set; }
			internal bool IsNetBibleVisible { get; set; }

			public StateTransition
				(
					string strThisStage,
					string strNextStage,
					TeamMemberData.UserTypes eMemberTypeWithEditToken,
					string strDisplayString,
					string strInstructions,
					bool bFixedState,
					List<AllowablePreviousStateWithConditions> lstAllowableBackwardsStages
				)
			{
				CurrentStage = strThisStage;
				NextState = strNextStage;
				MemberTypeWithEditToken = eMemberTypeWithEditToken;
				StageDisplayString = strDisplayString;
				StageInstructions = strInstructions;
				AllowableBackwardsTransitions.AddRange(lstAllowableBackwardsStages);
				IsFixedState = bFixedState;
				if (IsFixedState)
				{
					string strMethodName = strThisStage.Substring(1);

#if !DataDllBuild
					IsReadyForTransition = (CheckEndOfStateTransition.CheckForValidEndOfState)Delegate.CreateDelegate(
						typeof(CheckEndOfStateTransition.CheckForValidEndOfState),
						typeof(CheckEndOfStateTransition), strMethodName);
#endif
				}
			}

			public bool IsTransitionValid(string strToStage)
			{
				foreach (AllowablePreviousStateWithConditions aps in AllowableBackwardsTransitions)
					if (aps.ProjectStage == strToStage)
						return true;
				return false;
			}

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
					XElement elemAllowableBackwardsTransition = new XElement("AllowableBackwardsTransitions");
					foreach (AllowablePreviousStateWithConditions ps in AllowableBackwardsTransitions)
					{
						XElement elemAPS = new XElement("AllowableBackwardsTransition");
						if (ps.RequiresUsingVernacular)
							elemAPS.Add(new XAttribute("RequiresUsingVernacular", true));
						if (ps.RequiresBiblicalStory)
							elemAPS.Add(new XAttribute("RequiresBiblicalStory", true));
						if (ps.RequiresUsingEnglishBT)
							elemAPS.Add(new XAttribute("RequiresUsingEnglishBT", true));
						if (ps.HasUsingOtherEnglishBTer)
							elemAPS.Add(new XAttribute("RequiresUsingOtherEnglishBTer", ps.RequiresUsingOtherEnglishBTer));
						if (ps.RequiresUsingNationalBT)
							elemAPS.Add(new XAttribute("RequiresUsingNationalBT", true));
						elemAPS.Add(ps);
						elemAllowableBackwardsTransition.Add(elemAPS);
					}

					XElement elem = new XElement("StateTransition",
						new XAttribute("stage", CurrentStage),
						new XAttribute("MemberTypeWithEditToken", MemberTypeWithEditToken),
						new XAttribute("NextState", NextState),
						new XElement("StageDisplayString", StageDisplayString),
						new XElement("StageInstructions", StageInstructions),
						elemAllowableBackwardsTransition,
						new XElement("ViewSettings",
							new XAttribute("viewVernacularLangFieldMenuItem", IsVernacularVisible),
							new XAttribute("viewNationalLangFieldMenuItem", IsNationalBTVisible),
							new XAttribute("viewEnglishBTFieldMenuItem", IsEnglishBTVisible),
							new XAttribute("viewAnchorFieldMenuItem", IsAnchorVisible),
							new XAttribute("viewStoryTestingQuestionFieldMenuItem", IsStoryTestingQuestion),
							new XAttribute("viewRetellingFieldMenuItem", IsRetellingVisible),
							new XAttribute("viewConsultantNoteFieldMenuItem", IsConsultantNotesVisible),
							new XAttribute("viewCoachNotesFieldMenuItem", IsCoachNotesVisible),
							new XAttribute("viewNetBibleMenuItem", IsNetBibleVisible)));

					return elem;
				}
			}
		}
	}
}
