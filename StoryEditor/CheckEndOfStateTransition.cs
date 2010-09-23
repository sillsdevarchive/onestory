using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace OneStoryProjectEditor
{
	public class CheckEndOfStateTransition
	{
		/// <summary>
		/// Delegate definition for routines that determine whether the project stage goals are complete
		/// </summary>
		/// <param name="theSE">Needed to be able to do things like show who's logged on and add verse controls</param>
		/// <param name="theStoryProjectData">Needed for things like punctuation full stops for the various languages, etc.</param>
		/// <param name="theCurrentStory">The data of the story that we're to check</param>
		/// <param name="eProposedNextState">The state we're going to (so we can do specialized checking that everything is ok)</param>
		/// <returns></returns>
		public delegate bool CheckForValidEndOfState(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState);

		public static bool ProjFacTypeVernacular(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacTypeVernacular' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then gets rid of the Vernacular (so just allow it)
			if (!theStoryProjectData.ProjSettings.Vernacular.HasData)
				return true;

			// make sure that each verse has only one sentence
			bool bRepeatAfterMe = false;
			do
			{
				// if there are no verses, then just quit (before we get into an infinite loop)
				if (theCurrentStory.Verses.Count == 0)
				{
					ShowError(theSE, "Error: No verses in the story!");
					return false;
				}

				int nVerseNumber = 1;   // this wants to be 1, because it's dealing with the
										// VerseBT pane, which starts at 1.
				foreach (VerseData aVerseData in theCurrentStory.Verses)
				{
					if (aVerseData.IsVisible)
					{
						string strSentenceFinalPunct = theStoryProjectData.ProjSettings.Vernacular.FullStop;
						List<string> lstSentences;
						if (!GetListOfSentences(aVerseData.VernacularText, strSentenceFinalPunct, out lstSentences))
						{
							if (!aVerseData.HasData)
							{
								theCurrentStory.Verses.Remove(aVerseData);
								bRepeatAfterMe = true;
								break; // we have to exit the loop since we've modified the collection
							}
						}
						else if (lstSentences.Count > 1)
						{
							if (
								MessageBox.Show(
									String.Format(
										"Verse number '{0}' has multiple sentences. Click 'Yes' to have them separated into their own verses.",
										nVerseNumber), OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel) !=
								DialogResult.Yes)
								return false;

							int nNewVerses = lstSentences.Count;
							while (nNewVerses-- > 1)
							{
								string strSentence = lstSentences[nNewVerses];
								theCurrentStory.Verses.InsertVerse(nVerseNumber, strSentence, null, null);
							}

							aVerseData.VernacularText.SetValue(lstSentences[nNewVerses]);
							bRepeatAfterMe = true;
							break; // we have to exit the loop since we've modified the collection
						}
					}

					nVerseNumber++;
					bRepeatAfterMe = false; // if we get this far without a problem, then we haven't changed anything
				}
			} while (bRepeatAfterMe);

			// if it's a biblical story, we need to know the purpose of the story and the resources used.
			if (theCurrentStory.CraftingInfo.IsBiblicalStory &&
				(String.IsNullOrEmpty(theCurrentStory.CraftingInfo.StoryPurpose)
				|| String.IsNullOrEmpty(theCurrentStory.CraftingInfo.ResourcesUsed)))
			{
				MessageBox.Show(String.Format("In the following window, type in the purpose of the story (why you have it in your panorama) and list the resources you used to craft the story", Environment.NewLine), OseResources.Properties.Resources.IDS_Caption);
				theSE.QueryStoryPurpose();
			}

#if CheckProposedNextState
			// finally, if the user isn't doing a national bt, then we need to modify the next state
			if (!theStoryProjectData.ProjSettings.NationalBT.HasData)
			{
				// otherwise, there has to be an international bt field... unless the vern is English
				if (!theStoryProjectData.ProjSettings.InternationalBT.HasData)
				{
					System.Diagnostics.Debug.Assert(theStoryProjectData.ProjSettings.Vernacular.LangName == "English");
					eProposedNextState = (theCurrentStory.CraftingInfo.IsBiblicalStory)
						? StoryStageLogic.ProjectStages.eProjFacAddAnchors : StoryStageLogic.ProjectStages.eConsultantCheckNonBiblicalStory;
				}

				// normally, we'd go to the "Project Facilitator enters English BT" state next, but
				//  if there's a separate English BTr on the team, then we go do anchors
				//  first (and then Story Qs so that he/she can do the English BT for
				//  those afterwards as well).
				else if (theCurrentStory.CraftingInfo.IsBiblicalStory)
				{
					eProposedNextState = (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
						? StoryStageLogic.ProjectStages.eProjFacAddAnchors : StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT;
				}
				else
				{
					eProposedNextState = theStoryProjectData.TeamMembers.HasOutsideEnglishBTer
						? StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBT : StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT;
				}
			}
#endif
			return true;
		}

		public static bool ProjFacTypeNationalBT(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacTypeNationalBT' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then gets rid of the NationalBT (so just allow it)
			if (!theStoryProjectData.ProjSettings.NationalBT.HasData)
				return true;

			// if going backwards, we don't have anything to check
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert(eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeVernacular);
				return true;
			}

			// make sure that each verse has only one sentence
			bool bRepeatAfterMe = false;
			do
			{
				// if there are no verses, then just quit (before we get into an infinite loop)
				if (theCurrentStory.Verses.Count == 0)
				{
					ShowError(theSE, "Error: No verses in the story!");
					return false;
				}

				int nVerseNumber = 1;
				foreach (VerseData aVerseData in theCurrentStory.Verses)
				{
					if (aVerseData.IsVisible)
					{
						string strSentenceFinalPunct = theStoryProjectData.ProjSettings.NationalBT.FullStop;
						List<string> lstSentences;
						if (!GetListOfSentences(aVerseData.NationalBTText, strSentenceFinalPunct, out lstSentences))
						{
							// if there's nothing in this verse, then just get rid of it.
							if (!aVerseData.HasData)
							{
								theCurrentStory.Verses.Remove(aVerseData);
								bRepeatAfterMe = true;
								break; // we have to exit the loop since we've modified the collection
							}

							if (aVerseData.VernacularText.HasData)
							{
								ShowErrorFocus(theSE, aVerseData.NationalBTText.TextBox,
											   String.Format(
												   "Error: Verse {0} is missing a back-translation. Did you forget it?",
												   nVerseNumber));
								return false;
							}
						}

						else if (lstSentences.Count > 1)
						{
							MessageBox.Show(String.Format(Properties.Resources.IDS_UseStoryCollapse,
														  nVerseNumber,
														  theStoryProjectData.ProjSettings.NationalBT.LangName),
											OseResources.Properties.Resources.IDS_Caption);
							return false;
						}
					}

					nVerseNumber++;
					bRepeatAfterMe = false; // if we get this far without a problem, then we haven't changed anything
				}
			} while (bRepeatAfterMe);

			// we need to know who (which UNS) did the BT.
			QueryForUnsBackTranslator(theSE, theStoryProjectData, theCurrentStory);

#if CheckProposedNextState
			if (theCurrentStory.CraftingInfo.IsBiblicalStory)
			{
				if (theStoryProjectData.ProjSettings.InternationalBT.HasData
					&& !theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
					System.Diagnostics.Debug.Assert(eProposedNextState ==
						StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT);
				else
					eProposedNextState = StoryStageLogic.ProjectStages.eProjFacAddAnchors;
			}
			else
			{
				// normally, we'd go to doing anchors next, but if this isn't a biblical story, then
				//  no anchors and we skip right ot the English BT
				// but only if there is an English BT... if not, then we're done
				if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
					eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBT;
				else if (!theStoryProjectData.ProjSettings.InternationalBT.HasData)
					eProposedNextState = StoryStageLogic.ProjectStages.eConsultantCheckNonBiblicalStory;
				else
					System.Diagnostics.Debug.Assert(eProposedNextState ==
						StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT);
			}
#endif

			return true;
		}

		public static bool ProjFacTypeInternationalBT(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacTypeInternationalBT' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then gets rid of the EnglishBT (so just allow it)
			if (!theStoryProjectData.ProjSettings.InternationalBT.HasData)
				return true;

			// if going backwards, we don't have anything to check
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeVernacular)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeNationalBT));
				return true;
			}

			// make sure that each verse has only one sentence
			bool bRepeatAfterMe = false;
			do
			{
				// if there are no verses, then just quit (before we get into an infinite loop)
				if (theCurrentStory.Verses.Count == 0)
				{
					ShowError(theSE, "Error: No verses in the story!");
					return false;
				}

				// now go thru the English BT parts and make sure that there's only one sentence/verse.
				// make sure that each verse has only one sentence
				int nVerseNumber = 1;
				foreach (VerseData aVerseData in theCurrentStory.Verses)
				{
					if (aVerseData.IsVisible)
					{
						string strSentenceFinalPunct = theStoryProjectData.ProjSettings.InternationalBT.FullStop;
						List<string> lstSentences;
						if ((!GetListOfSentences(aVerseData.InternationalBTText, strSentenceFinalPunct, out lstSentences))
							|| (lstSentences.Count == 0))
						{
							// if there's nothing in this verse, then just get rid of it.
							if (!aVerseData.HasData)
							{
								theCurrentStory.Verses.Remove(aVerseData);
								bRepeatAfterMe = true;
								break; // we have to exit the loop since we've modified the collection
							}


							// if there's data in either the story box or the natl bt box...
							if (aVerseData.VernacularText.HasData || aVerseData.NationalBTText.HasData)
							{
								// then there ought to be some in the English BT box as well.
								// light it up and let the user know they need to do something!
								ShowErrorFocus(theSE, aVerseData.InternationalBTText.TextBox,
											   String.Format(
												   "Error: Verse {0} doesn't have any English back-translation in it. Did you forget it?",
												   nVerseNumber));
								return false;
							}
							return false;
						}

						if (lstSentences.Count > 1)
						{
							MessageBox.Show(String.Format(Properties.Resources.IDS_UseStoryCollapse,
														  nVerseNumber,
														  theStoryProjectData.ProjSettings.InternationalBT.LangName),
											OseResources.Properties.Resources.IDS_Caption);
							return false;
						}
					}

					nVerseNumber++;
					bRepeatAfterMe = false; // if we get this far without a problem, then we haven't changed anything
				}
			} while (bRepeatAfterMe);

			// if there's only an English BT, then we need to know who (which UNS) did the BT.
			if (!theStoryProjectData.ProjSettings.NationalBT.HasData)
				QueryForUnsBackTranslator(theSE, theStoryProjectData, theCurrentStory);

#if CheckProposedNextState
			if (!theCurrentStory.CraftingInfo.IsBiblicalStory)
				eProposedNextState = StoryStageLogic.ProjectStages.eConsultantCheckNonBiblicalStory;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eProjFacAddAnchors);
#endif

			return true;
		}

		internal static char[] achQuotes = new [] { '"', '\'', '\u2018', '\u2019', '\u201B',
			'\u201C', '\u201d', '\u201E', '\u201F' };

		internal static string QuoteCharsAsString
		{
			get
			{
				string str = null;
				foreach (char ch in achQuotes)
				{
					str += ch;
				}
				return str;
			}
		}
		public static bool GetListOfSentences(StringTransfer stParagraph, string strSentenceFinalPunct, out List<string> lstSentences)
		{
			lstSentences = new List<string>();

			string strParagraph = stParagraph.ToString();
			if (strParagraph == null)
				return false;

			// split it up based on the sentence final punctuation: (the standard list, plus the list from
			//  the project passed in)
			char[] achSplitOn = strSentenceFinalPunct.ToCharArray();
			int nStartIndex = 0, nIndex;
			while ((nStartIndex < strParagraph.Length) && (nIndex = strParagraph.IndexOfAny(achSplitOn, nStartIndex)) != -1)
			{
				if (((nIndex + 1) < strParagraph.Length) && (strParagraph.IndexOfAny(achQuotes, nIndex + 1, 1) != -1))
					nIndex++;

				string strLine = strParagraph.Substring(nStartIndex, nIndex - nStartIndex + 1).Trim();
				strLine = strLine.Replace("\r\n", " ");
				while (strLine.IndexOf("  ") != -1)
					strLine = strLine.Replace("  ", " ");
				if (!String.IsNullOrEmpty(strLine))
					lstSentences.Add(strLine);

				nStartIndex = nIndex + 1;
			}

			// if we didn't find any final punctuation...
			if (lstSentences.Count == 0)
			{
				// it may just be that the user forgot it.
				if (!String.IsNullOrEmpty(strParagraph))
				{
					// this means the user forgot the full stop, so help him/her out and add one.
					stParagraph.SetValue(strParagraph.Trim() + strSentenceFinalPunct[0]);
					lstSentences.Add(strParagraph);
				}
				else
					return false;
			}

			// otherwise, there may have been multiple sentences, but the last one didn't have
			//  a full stop
			else if (nStartIndex < strParagraph.Length)
			{
				string strLine = strParagraph.Substring(nStartIndex).Trim();
				if (!String.IsNullOrEmpty(strLine))
				{
					// this means the user forgot the full stop, so help him/her out and add one.
					lstSentences.Add(strLine + strSentenceFinalPunct[0]);
				}
			}

			return (lstSentences.Count > 0);
		}

		protected static void ShowErrorFocus(StoryEditor theSE, CtrlTextBox tb, string strStatusMessage)
		{
			if (tb != null)
			{
				tb.Focus();
				tb.SelectAll();
			}
			ShowError(theSE, strStatusMessage);
		}

		protected static void ShowErrorFocus(StoryEditor theSE,
			HtmlConNoteControl paneConNote, int nVerseNumber, string strStatusMessage)
		{
			paneConNote.ScrollToVerse(nVerseNumber);
			ShowError(theSE, strStatusMessage);
		}

		protected static void ShowError(StoryEditor theSE, string strStatusMessage)
		{
			theSE.SetStatusBar(strStatusMessage);
			Console.Beep();
			MessageBox.Show(strStatusMessage, OseResources.Properties.Resources.IDS_Caption);
		}

		public static bool ProjFacAddAnchors(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacAddAnchors' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then changes it to be a non-biblical story
			if (!theCurrentStory.CraftingInfo.IsBiblicalStory)
				return true;

			// if going backwards, we don't have anything to check
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeVernacular)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeNationalBT)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT));
				return true;
			}

			// for each verse, make sure that there is at least one anchor.
			bool bHasAnyKeyTermBeenChecked = false;
			int nVerseNumber = 1;
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				if (aVerseData.IsVisible)
				{
					if (aVerseData.Anchors.Count == 0)
					{
						ShowError(theSE,
								  String.Format("Error: Verse {0} doesn't have an anchor. Did you forget it?",
												nVerseNumber));
						theSE.FocusOnVerse(nVerseNumber, true, true);
						return false;
					}

					if (aVerseData.Anchors.IsKeyTermChecked)
						bHasAnyKeyTermBeenChecked = true;
				}
				nVerseNumber++;
			}

			if (!bHasAnyKeyTermBeenChecked)
			{
				DialogResult res = MessageBox.Show(Properties.Resources.IDS_CheckOnKeyTerms,
											   OseResources.Properties.Resources.IDS_Caption,
											   MessageBoxButtons.RetryCancel);
				if (res == DialogResult.Cancel)
					return false;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacAddStoryQuestions);
#endif

			return true;
		}

		public static bool ProjFacAddStoryQuestions(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacAddStoryQuestions' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then changes it to be a non-biblical story
			if (!theCurrentStory.CraftingInfo.IsBiblicalStory)
				return true;

			// if going backwards, we don't have anything to check
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeVernacular)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeNationalBT)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacTypeInternationalBT)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacAddAnchors));
				return true;
			}

			// there should be at least half as many questions as there are verses.
			int nNumOfVerses = 0;
			foreach (VerseData aVerseData in theCurrentStory.Verses)
				if (aVerseData.IsVisible)
					nNumOfVerses += aVerseData.TestQuestions.Count;

			if (nNumOfVerses < (theCurrentStory.Verses.Count / 2))
			{
				int nNumLacking = (theCurrentStory.Verses.Count / 2) - nNumOfVerses;
				ShowError(theSE,
					String.Format("Error: You should have at least half as many Story Testing Questions as verses in the story. Please add at least {0} more testing question(s). (right-click on the 'verse options' button and choose 'Add a story testing question')", nNumLacking));
				return false;
			}

			// before going to the CIT, let's make sure that if the CIT had made
			//  a comment, that the PF answered it. (this only occurs if the CIT
			//  had earlier checked the story and gone backwards)
			if (!CheckThatPFRespondedToCITQuestions(theSE, theCurrentStory))
				return false;

			// for the Payne approach, they want the first two UNS checks *before* the
			//  consultant sees it, so check if we should be skipping all the way to the
			//  "after round 1 consultant checking do the UNS test" state
			DialogResult res = MessageBox.Show(Properties.Resources.IDS_CheckForSkipToUnsCheck,
										   OseResources.Properties.Resources.IDS_Caption,
										   MessageBoxButtons.YesNoCancel);
			if (res == DialogResult.Cancel)
				return false;

#if CheckProposedNextState
			if (res == DialogResult.Yes)
			{
				// theSE.AddTest();
				eProposedNextState = StoryStageLogic.ProjectStages.eProjFacReadyForTest1;
			}
			else if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
				eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBT;
			else if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorCheck1;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
												StoryStageLogic.ProjectStages.eConsultantCheckStoryInfo);
#endif

			return true;
		}

		public static bool ProjFacRevisesBeforeUnsTest(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacRevisesBeforeUnsTest' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then changes it to be a non-biblical story
			if (!theCurrentStory.CraftingInfo.IsBiblicalStory)
				return true;

			// before going to the CIT, let's make sure that if the CIT had made
			//  a comment, that the PF answered it. (this only occurs if the CIT
			//  had earlier checked the story and gone backwards)
			if (!CheckThatPFRespondedToCITQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
				eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBT;
			else if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorCheck1;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
												StoryStageLogic.ProjectStages.eConsultantCheckStoryInfo);
#endif

			return true;
		}

		public static bool BackTranslatorTypeInternationalBT(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'BackTranslatorTypeInternationalBT' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then gets rid of the EnglishBT (so just allow it)
			if (!theStoryProjectData.ProjSettings.InternationalBT.HasData)
				return true;

			// if going backwards, we don't have anything to check
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest));
				return true;
			}

			// if there are no verses, then just quit (before we get into an infinite loop)
			if (theCurrentStory.Verses.Count == 0)
			{
				ShowError(theSE, "Error: No verses in the story!");
				return false;
			}

			// now go thru the English BT parts and make sure that there's only one sentence/verse.
			// make sure that each verse has only one sentence
			int nVerseNumber = 1;
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				if (aVerseData.IsVisible)
				{
					string strSentenceFinalPunct = theStoryProjectData.ProjSettings.InternationalBT.FullStop;
					List<string> lstSentences;
					if ((!GetListOfSentences(aVerseData.InternationalBTText, strSentenceFinalPunct, out lstSentences))
						|| (lstSentences.Count == 0))
					{
						// light it up and let the user know they need to do something!
						ShowErrorFocus(theSE, aVerseData.InternationalBTText.TextBox,
									   String.Format(
										   "Error: Verse {0} doesn't have any English back-translation in it. Did you forget it?",
										   nVerseNumber));
						return false;
					}

					if (lstSentences.Count > 1)
					{
						// light it up and let the user know they need to do something!
						ShowErrorFocus(theSE, aVerseData.InternationalBTText.TextBox,
									   String.Format(
										   "Error: Verse {0} has multiple sentences in English, but only 1 in {1}. Adjust the English to match the {1}",
										   nVerseNumber, theStoryProjectData.ProjSettings.NationalBT.LangName));
						return false;
					}
				}

				nVerseNumber++;
			}

			// if there's only an English BT (this really can't happen... the only reason you'd
			//  have a separate English BTer is if there's a national BT), then we need to
			//  know who (which UNS) did the BT.
			if (!theStoryProjectData.ProjSettings.NationalBT.HasData)
				QueryForUnsBackTranslator(theSE, theStoryProjectData, theCurrentStory);

#if CheckProposedNextState
			if (!theCurrentStory.CraftingInfo.IsBiblicalStory)
				eProposedNextState = StoryStageLogic.ProjectStages.eConsultantCheckNonBiblicalStory;
			else if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorCheck1;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eConsultantCheckStoryInfo);
#endif

			return true;
		}

		public static bool BackTranslatorTranslateConNotesBeforeUnsTest(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'BackTranslatorTranslateConNotesBeforeUnsTest' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards, we don't have anything to check
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest));
				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eConsultantCheckStoryInfo);
#endif

			return true;
		}

		protected static void QueryForUnsBackTranslator(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory)
		{
			while (String.IsNullOrEmpty(theCurrentStory.CraftingInfo.BackTranslatorMemberID))
			{
				MemberPicker dlg = new MemberPicker(theStoryProjectData, TeamMemberData.UserTypes.eUNS);
				dlg.Text = "Choose the UNS that did this back-translation";
				if (dlg.ShowDialog() != DialogResult.OK)
					return;

				theCurrentStory.CraftingInfo.BackTranslatorMemberID = dlg.SelectedMember.MemberGuid;
			}
		}

		public static bool ConsultantCheckNonBiblicalStory(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantCheckNonBiblicalStory' work is finished: Name: {0}", theCurrentStory.Name));
			return true;
		}

		public static bool FirstPassMentorCheck1(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'FirstPassMentorCheck1' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards, we don't have anything to check
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest));
				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eConsultantCheckStoryInfo);
#endif

			return true;
		}

		public static bool ConsultantCheckStoryInfo(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantCheckStoryInfo' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eFirstPassMentorCheck1)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest));

				// if it's going to the PF, then...
				if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest))
				{
					// make sure that if the ProjectFac asked a question that the CIT responded to it.
					if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eConsultantCheckAnchors);
#endif

			return true;
		}

		public static bool ConsultantCheckAnchors(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantCheckAnchors' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckStoryInfo)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eFirstPassMentorCheck1)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest));

				if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest))
				{
					// make sure that if the ProjectFac asked a question that the CIT responded to it.
					if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eConsultantCheckStoryQuestions);
#endif

			return true;
		}

		public static bool ConsultantCheckStoryQuestions(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantCheckStoryQuestions' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckAnchors)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckStoryInfo)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eFirstPassMentorCheck1)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest));

				if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest))
				{
					// make sure that if the ProjectFac asked a question that the CIT responded to it.
					if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			if (eProposedNextState == StoryStageLogic.ProjectStages.eCoachReviewRound1Notes)
			{
				// before going to the Coach, let's make sure that if the coach had made
				//  a comment, that the CIT answered it. (this only occurs if the coach
				//  had earlier checked the story and gone backwards)
				if (!CheckThatCITRespondedToCoachQuestions(theSE, theCurrentStory))
					return false;
			}
			else if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
				|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes))
			{
				if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
					return false;
			}

#if CheckProposedNextState
			// if we have an independent consultant, then the next state is back to the PF
			if (theStoryProjectData.TeamMembers.HasIndependentConsultant
			 && theSE.LoggedOnMember.MemberType == TeamMemberData.UserTypes.eIndependentConsultant)
			{
				if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
					eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes;
				else
					eProposedNextState = StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes;
			}
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eCoachReviewRound1Notes);
#endif

			return true;
		}

		public static bool ConsultantCauseRevisionBeforeUnsTest(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantCauseRevisionBeforeUnsTest' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eFirstPassMentorCheck1)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest));

				if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotesBeforeUnsTest)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacRevisesBeforeUnsTest))
				{
					// make sure that if the ProjectFac asked a question that the CIT responded to it.
					if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			// before going to the Coach, let's make sure that if the coach had made
			//  a comment, that the CIT answered it. (this only occurs if the coach
			//  had earlier checked the story and gone backwards)
			if (!CheckThatCITRespondedToCoachQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eCoachReviewRound1Notes);
#endif

			return true;
		}

		static bool CheckThatCoachAnsweredCITsQuestions(StoryEditor theSE, StoryData theCurrentStory)
		{
			int nVerseNumber = 0;   // this wants to be 0, because ConNotes have a 0th verse
			if (theCurrentStory.Verses.FirstVerse.IsVisible)
				if (!CheckThatMentorAnsweredMenteesQuestions(theSE, theSE.htmlCoachNotesControl,
					theCurrentStory.Verses.FirstVerse.CoachNotes, ref nVerseNumber))
				{
					var viewSettings = new VerseData.ViewSettings(VerseData.ViewSettings.ItemToInsureOn.CoachNotesFields);
					theSE.NavigateTo(theCurrentStory.Name,
									 viewSettings,
									 false,
									 null);
					return false;
				}

			nVerseNumber++;
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				if (aVerseData.IsVisible)
					if (!CheckThatMentorAnsweredMenteesQuestions(theSE, theSE.htmlCoachNotesControl,
						aVerseData.CoachNotes, ref nVerseNumber))
					{
						var viewSettings = new VerseData.ViewSettings(VerseData.ViewSettings.ItemToInsureOn.CoachNotesFields);
						theSE.NavigateTo(theCurrentStory.Name,
										 viewSettings,
										 false,
										 null);
						return false;
					}
				nVerseNumber++;
			}
			return true;
		}

		static bool CheckThatCITAnsweredPFsQuestions(StoryEditor theSE, StoryData theCurrentStory)
		{
			int nVerseNumber = 0;
			if (theCurrentStory.Verses.FirstVerse.IsVisible)
				if (!CheckThatMentorAnsweredMenteesQuestions(theSE, theSE.htmlConsultantNotesControl,
					theCurrentStory.Verses.FirstVerse.ConsultantNotes, ref nVerseNumber))
				{
					var viewSettings = new VerseData.ViewSettings(VerseData.ViewSettings.ItemToInsureOn.ConsultantNoteFields);
					theSE.NavigateTo(theCurrentStory.Name,
									 viewSettings,
									 false,
									 null);
					return false;
				}

			nVerseNumber++;
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				if (aVerseData.IsVisible)
					if (!CheckThatMentorAnsweredMenteesQuestions(theSE, theSE.htmlConsultantNotesControl,
						aVerseData.ConsultantNotes, ref nVerseNumber))
					{
						var viewSettings = new VerseData.ViewSettings(VerseData.ViewSettings.ItemToInsureOn.ConsultantNoteFields);
						theSE.NavigateTo(theCurrentStory.Name,
										 viewSettings,
										 false,
										 null);
						return false;
					}
				nVerseNumber++;
			}
			return true;
		}

		static bool CheckThatCITRespondedToCoachQuestions(StoryEditor theSE, StoryData theCurrentStory)
		{
			int nVerseNumber = 0;
			if (theCurrentStory.Verses.FirstVerse.IsVisible)
				if (!CheckThatMenteeAnsweredMentorsQuestions(theSE,
						theSE.htmlCoachNotesControl, theCurrentStory.Verses.FirstVerse.CoachNotes,
						theCurrentStory.Name, VerseData.ViewSettings.ItemToInsureOn.CoachNotesFields,
						ref nVerseNumber))
					return false;

			nVerseNumber++;
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				if (aVerseData.IsVisible)
					if (!CheckThatMenteeAnsweredMentorsQuestions(theSE, theSE.htmlCoachNotesControl,
						aVerseData.CoachNotes, theCurrentStory.Name,
						VerseData.ViewSettings.ItemToInsureOn.CoachNotesFields, ref nVerseNumber))
						return false;
				nVerseNumber++;
			}
			return true;
		}

		static bool CheckThatPFRespondedToCITQuestions(StoryEditor theSE, StoryData theCurrentStory)
		{
			int nVerseNumber = 0;
			if (theCurrentStory.Verses.FirstVerse.IsVisible)
				if (!CheckThatMenteeAnsweredMentorsQuestions(theSE, theSE.htmlConsultantNotesControl,
					theCurrentStory.Verses.FirstVerse.ConsultantNotes, theCurrentStory.Name,
					VerseData.ViewSettings.ItemToInsureOn.ConsultantNoteFields, ref nVerseNumber))
					return false;

			nVerseNumber++;
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				if (aVerseData.IsVisible)
					if (!CheckThatMenteeAnsweredMentorsQuestions(theSE, theSE.htmlConsultantNotesControl,
						aVerseData.ConsultantNotes, theCurrentStory.Name,
					VerseData.ViewSettings.ItemToInsureOn.ConsultantNoteFields, ref nVerseNumber))
						return false;

				nVerseNumber++;
			}
			return true;
		}

		static bool CheckThatMentorAnsweredMenteesQuestions(StoryEditor theSE,
			HtmlConNoteControl paneConNote, ConsultNotesDataConverter aCNsDC,
			ref int nVerseNumber)
		{
			foreach (ConsultNoteDataConverter aConNote in aCNsDC)
			{
				if (aConNote.Visible)
				{
					int nIndexLast = aConNote.Count - 1;
					CommInstance theLastCI = aConNote[nIndexLast];
					if ((theLastCI.Direction == aConNote.MentorDirection)
						&& !theLastCI.HasData)
					{
						ShowErrorFocus(theSE, paneConNote, nVerseNumber,
									   String.Format(
										   "Error: in line {0}, the {1} made a comment, which you didn't respond to. Did you forget it?",
										   nVerseNumber,
										   TeamMemberData.GetMemberTypeAsDisplayString(aConNote.MenteeRequiredEditor)));
						return false;
					}
				}
			}
			return true;
		}

		static bool CheckThatMenteeAnsweredMentorsQuestions(StoryEditor theSE,
			HtmlConNoteControl paneConNote, ConsultNotesDataConverter aCNsDC,
			string strCurrentStoryName, VerseData.ViewSettings.ItemToInsureOn ePane,
			ref int nVerseNumber)
		{
			foreach (ConsultNoteDataConverter aConNote in aCNsDC)
			{
				if (aConNote.Visible)
				{
					int nIndexLast = aConNote.Count - 1;
					CommInstance theLastCI = aConNote[nIndexLast];
					if ((theLastCI.Direction == aConNote.MenteeDirection)
						&& !theLastCI.HasData)
					{
						var viewSettings = new VerseData.ViewSettings(ePane);
						theSE.NavigateTo(strCurrentStoryName,
										 viewSettings,
										 false,
										 null);
						ShowErrorFocus(theSE, paneConNote, nVerseNumber,
									   String.Format(
										   "Error: in line {0}, the {1} made a comment, which you didn't respond to. Did you forget it?",
										   nVerseNumber,
										   TeamMemberData.GetMemberTypeAsDisplayString(aConNote.MentorRequiredEditor)));
						return false;
					}
				}
			}
			return true;
		}

		public static bool CoachReviewRound1Notes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'CoachReviewRound1Notes' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				// if it's going backwards...
				System.Diagnostics.Debug.Assert(eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCauseRevisionBeforeUnsTest);
				if (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCauseRevisionBeforeUnsTest)
				{
					// make sure that if the CIT asked a question that the Coach responded to it.
					if (!CheckThatCoachAnsweredCITsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			// going forward... same thing
			// before handing it back to the consultant, let's make sure that if the consultant had initiated
			//  a conversation, that the coach answered it.
			if (!CheckThatCoachAnsweredCITsQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes);
#endif

			return true;
		}

		public static bool ConsultantReviseRound1Notes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantReviseRound1Notes' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert(eProposedNextState == StoryStageLogic.ProjectStages.eCoachReviewRound1Notes);
				if (eProposedNextState == StoryStageLogic.ProjectStages.eCoachReviewRound1Notes)
				{
					// make sure that if the CIT answered the Coach's comments.
					if (!CheckThatCITRespondedToCoachQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			// before handing it back to the Project Facilitator, let's make sure that if the coach had made
			//  a comment, that the CIT answered it.
			if (!CheckThatCITRespondedToCoachQuestions(theSE, theCurrentStory))
				return false;

			// and if the ProjectFac asked a question that the CIT responded to it.
			if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
				eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes);
#endif

			return true;
		}

		public static bool BackTranslatorTranslateConNotes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'BackTranslatorTranslateConNotes' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckStoryQuestions));

				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes);
#endif

			return true;
		}

		public static bool ProjFacReviseBasedOnRound1Notes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacReviseBasedOnRound1Notes' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckStoryQuestions));

				if ((eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckStoryQuestions))
				{
					// make sure that if the PF answered the CIT 's comments.
					if (!CheckThatPFRespondedToCITQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBTTest1)
				|| (eProposedNextState == StoryStageLogic.ProjectStages.eFirstPassMentorCheck2)
				|| (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheck2))
			{
				// make sure that if the PF answered the CIT 's comments.
				if (!CheckThatPFRespondedToCITQuestions(theSE, theCurrentStory))
					return false;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacOnlineReview1WithConsultant);
#endif

			return true;
		}

		public static bool ProjFacOnlineReview1WithConsultant(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacOnlineReview1WithConsultant' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckStoryQuestions));

				if ((eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheckStoryQuestions))
				{
					// make sure that if the PF answered the CIT 's comments.
					if (!CheckThatPFRespondedToCITQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			// let's make sure that if the CIT had made a comment, that the ProjFac answered it.
			if (!CheckThatPFRespondedToCITQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacReadyForTest1);
#endif

			return true;
		}

		public static bool ProjFacReadyForTest1(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacReadyForTest1' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacOnlineReview1WithConsultant)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacAddStoryQuestions));

				return true;
			}

			// add the story question answer lines and retelling lines to the verses for test n
			DialogResult res = MessageBox.Show(Properties.Resources.IDS_AddTestQuery, OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
			if (res == DialogResult.Cancel)
				return false;

			if (res == DialogResult.Yes)
				theSE.AddTest();

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacEnterRetellingOfTest1);
#endif

			return true;
		}

		static bool CheckAnswersAnswered(StoryEditor theSE, StoryData theCurrentStory)
		{
			int nVerseNumber = 1;   // this wants to be 1, because it deals with the VerseBT pane
			foreach (VerseData aVerseData in theCurrentStory.Verses)
			{
				if (aVerseData.IsVisible)
				{
					foreach (TestQuestionData aTQ in aVerseData.TestQuestions)
						foreach (StringTransfer aST in aTQ.Answers)
							if (!aST.HasData)
							{
								ShowErrorFocus(theSE, aST.TextBox,
											   String.Format(
												   "Error: Verse {0} is missing an answer to a testing question. Did you forget it?",
												   nVerseNumber));
								return false;
							}
				}
				nVerseNumber++;
			}
			return true;
		}

		public static bool ProjFacEnterRetellingOfTest1(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacEnterRetellingOfTest1' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReadyForTest1)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacOnlineReview1WithConsultant)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes));

				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacEnterAnswersToStoryQuestionsOfTest1);
#endif

			return true;
		}

		public static bool ProjFacEnterAnswersToStoryQuestionsOfTest1(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacEnterAnswersToStoryQuestionsOfTest1' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eProjFacEnterRetellingOfTest1)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReadyForTest1)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacOnlineReview1WithConsultant)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes));

				// make sure they have some answer written into each question (if going on to the next test)
				if (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacEnterRetellingOfTest1)
				{
					if (!CheckAnswersAnswered(theSE, theCurrentStory))
						return false;
					theSE.AddTest();
				}

				return true;
			}

			// make sure they have some answer written into each question
			if (!CheckAnswersAnswered(theSE, theCurrentStory))
				return false;

			// if they've only done one test, then ask them to do another (but don't force them)
			DialogResult res =
				MessageBox.Show(
					Properties.Resources.IDS_AddAnotherTestQuery,
					OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.OKCancel);

			if (res == DialogResult.Cancel)
				return false;

			/*
			if (res == DialogResult.Yes)
			{
				theSE.AddTest();

#if CheckProposedNextState
				// this will go:
				//  ProjFacEnterRetellingOfTest1 (to get answer from UNS_1), then
				//  ProjFacEnterAnswersToStoryQuestionsOfTest1 (to get retelling from UNS_1), then (while Testor.Count < 2) back to:
				//  ProjFacEnterRetellingOfTest1, etc.
				eProposedNextState = StoryStageLogic.ProjectStages.eProjFacEnterRetellingOfTest1;
#endif
			}
#if CheckProposedNextState
			else if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
				eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBTTest1;
			else if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorCheck2;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eConsultantCheck2);
#endif
			*/

			return true;
		}

		public static bool BackTranslatorTypeInternationalBTTest1(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'BackTranslatorTypeInternationalBTTest1' work is finished: Name: {0}", theCurrentStory.Name));

			// this can happen if the person has a story in this state, but then gets rid of the EnglishBT (so just allow it)
			if (!theStoryProjectData.ProjSettings.InternationalBT.HasData)
				return true;

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert(eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes);
				return true;
			}

			// if there are no verses, then just quit (before we get into an infinite loop)
			if (theCurrentStory.Verses.Count == 0)
			{
				ShowError(theSE, "Error: No verses in the story!");
				return false;
			}

#if CheckProposedNextState
			if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorCheck2;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eConsultantCheck2);
#endif

			return true;
		}

		/*
		public static bool BackTranslatorTranslateConNotesAfterUnsTest(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'BackTranslatorTranslateConNotesAfterUnsTest' work is finished: Name: {0}", theCurrentStory.Name));

#if CheckProposedNextState
			if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorCheck2;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eConsultantCheck2);
#endif

			return true;
		}
		*/

		public static bool FirstPassMentorCheck2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'FirstPassMentorCheck2' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes));

				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(theStoryProjectData.TeamMembers.IsThereAFirstPassMentor
				&& (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantCheck2));
#endif

			return true;
		}

		public static bool ConsultantCheck2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantCheck2' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eFirstPassMentorCheck2)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
					||  (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes));

				// if it's going to the PF, then...
				if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes))
				{
					// make sure that if the ProjectFac asked a question that the CIT responded to it.
					if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			// and if the ProjectFac asked a question that the CIT responded to it (so the
			//  coach can check the responses)
			if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			// if we have an independent consultant, then the next state is TeamComplete
			if (theStoryProjectData.TeamMembers.HasIndependentConsultant
			 && theSE.LoggedOnMember.MemberType == TeamMemberData.UserTypes.eIndependentConsultant)
			{
				eProposedNextState = StoryStageLogic.ProjectStages.eTeamComplete;
			}
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eCoachReviewRound2Notes);
#endif

			return true;
		}

		/*
		public static bool ConsultantCauseRevisionAfterUnsTest(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantCauseRevisionAfterUnsTest' work is finished: Name: {0}", theCurrentStory.Name));

			// and if the ProjectFac asked a question that the CIT responded to it (so the
			//  coach can check the responses)
			if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			// if we have an independent consultant, then the next state is Team Complete
			if (theStoryProjectData.TeamMembers.HasIndependentConsultant
			 && theSE.LoggedOnMember.MemberType == TeamMemberData.UserTypes.eIndependentConsultant)
			{
				eProposedNextState = StoryStageLogic.ProjectStages.eTeamComplete;
			}
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eCoachReviewRound2Notes);
#endif

			return true;
		}
		*/

		public static bool CoachReviewRound2Notes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'CoachReviewRound2Notes' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert(eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes);

				// if it's going backwards...
				if (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviseRound1Notes)
				{
					// make sure that if the CIT asked a question that the Coach responded to it.
					if (!CheckThatCoachAnsweredCITsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

			// before finalizing it, make sure that if the consultant had initiated
			//  a conversation, that the coach answered it.
			if (!CheckThatCoachAnsweredCITsQuestions(theSE, theCurrentStory))
				return false;

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eTeamComplete);
#endif

			return true;
		}

		public static bool ConsultantFinalCheck(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantFinalCheck' work is finished: Name: {0}", theCurrentStory.Name));

			// if going backwards...
			if (IsGoingBackwards(theCurrentStory, eProposedNextState))
			{
				System.Diagnostics.Debug.Assert((eProposedNextState == StoryStageLogic.ProjectStages.eFirstPassMentorCheck2)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes));

				// if it's going to the PF, then...
				if ((eProposedNextState == StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes)
					|| (eProposedNextState == StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound1Notes))
				{
					// make sure that if the ProjectFac asked a question that the CIT responded to it.
					if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
						return false;
				}

				return true;
			}

#if CheckProposedNextState
			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eTeamComplete);
#endif

			// before finalizing it, make sure that if the consultant had initiated
			//  a conversation, that the coach answered it.
			if (!CheckThatCITRespondedToCoachQuestions(theSE, theCurrentStory))
				return false;

			return true;
		}

		/*
		public static bool ConsultantReviseRound2Notes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantReviseRound2Notes' work is finished: Name: {0}", theCurrentStory.Name));

			// before handing it back to the Project Facilitator, let's make sure that if the coach had made
			//  a comment, that the CIT answered it.
			if (!CheckThatCITRespondedToCoachQuestions(theSE, theCurrentStory))
				return false;

			// and if the ProjectFac asked a question that the CIT responded to it.
			if (!CheckThatCITAnsweredPFsQuestions(theSE, theCurrentStory))
				return false;

			if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
				eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTranslateConNotes2;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound2Notes);

			return true;
		}

		public static bool BackTranslatorTranslateConNotes2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'BackTranslatorTranslateConNotes2' work is finished: Name: {0}", theCurrentStory.Name));

			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacReviseBasedOnRound2Notes);

			return true;
		}

		public static bool ProjFacReviseBasedOnRound2Notes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacReviseBasedOnRound2Notes' work is finished: Name: {0}", theCurrentStory.Name));

			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacOnlineReview2WithConsultant);

			return true;
		}

		public static bool ProjFacOnlineReview2WithConsultant(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacOnlineReview2WithConsultant' work is finished: Name: {0}", theCurrentStory.Name));

			// let's make sure that if the CIT had made a comment, that the ProjFac answered it.
			if (!CheckThatPFRespondedToCITQuestions(theSE, theCurrentStory))
				return false;

			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacReadyForTest2);

			return true;
		}

		public static bool ProjFacReadyForTest2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacReadyForTest2' work is finished: Name: {0}", theCurrentStory.Name));

			// add the story question answer lines and retelling lines to the verses for test n
			DialogResult res = MessageBox.Show(OseResources.Properties.Resources.IDS_AddTestQuery, OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
			if (res == DialogResult.Cancel)
				return false;

			if (res == DialogResult.Yes)
				theSE.AddTest();

			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacEnterRetellingOfTest2);

			return true;
		}

		public static bool ProjFacEnterRetellingOfTest2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacEnterRetellingOfTest2' work is finished: Name: {0}", theCurrentStory.Name));

			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eProjFacEnterAnswersToStoryQuestionsOfTest2);

			return true;
		}

		public static bool ProjFacEnterAnswersToStoryQuestionsOfTest2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ProjFacEnterAnswersToStoryQuestionsOfTest2' work is finished: Name: {0}", theCurrentStory.Name));

			// make sure they have some answer written into each question
			if (!CheckAnswersAnswered(theSE, theCurrentStory))
				return false;

			// see if they want to enter results for the next UNS test
			DialogResult res = MessageBox.Show("Click 'Yes' to create the boxes for entering the next UNS's answers to the testing questions", OseResources.Properties.Resources.IDS_Caption, MessageBoxButtons.YesNoCancel);
			if (res == DialogResult.Yes)
				theSE.AddTest();
			else if (res == DialogResult.Cancel)
				return false;
			else if (theStoryProjectData.TeamMembers.HasOutsideEnglishBTer)
				eProposedNextState = StoryStageLogic.ProjectStages.eBackTranslatorTypeInternationalBTTest2;
			else if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorReviewTest2;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eConsultantReviewTest2);

			return true;
		}

		public static bool BackTranslatorTypeInternationalBTTest2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'BackTranslatorTypeInternationalBTTest2' work is finished: Name: {0}", theCurrentStory.Name));

			// if there are no verses, then just quit (before we get into an infinite loop)
			if (theCurrentStory.Verses.Count == 0)
			{
				ShowError(theSE, "Error: No verses in the story!");
				return false;
			}

			if (theStoryProjectData.TeamMembers.HasFirstPassMentor)
				eProposedNextState = StoryStageLogic.ProjectStages.eFirstPassMentorReviewTest2;
			else
				System.Diagnostics.Debug.Assert(eProposedNextState ==
					StoryStageLogic.ProjectStages.eConsultantReviewTest2);

			return true;
		}

		public static bool FirstPassMentorReviewTest2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'FirstPassMentorReviewTest2' work is finished: Name: {0}", theCurrentStory.Name));

			System.Diagnostics.Debug.Assert(theStoryProjectData.TeamMembers.IsThereAFirstPassMentor
				&& (eProposedNextState == StoryStageLogic.ProjectStages.eConsultantReviewTest2));

			return true;
		}

		public static bool ConsultantReviewTest2(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'ConsultantReviewTest2' work is finished: Name: {0}", theCurrentStory.Name));

			System.Diagnostics.Debug.Assert(eProposedNextState == StoryStageLogic.ProjectStages.eCoachReviewTest2Notes);

			// if we have an independent consultant, then we're done
			if (theStoryProjectData.TeamMembers.HasIndependentConsultant
			 && theSE.LoggedOnMember.MemberType == TeamMemberData.UserTypes.eIndependentConsultant)
			{
				eProposedNextState = StoryStageLogic.ProjectStages.eTeamComplete;
			}

			return true;
		}

		public static bool CoachReviewTest2Notes(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'CoachReviewTest2Notes' work is finished: Name: {0}", theCurrentStory.Name));

			// before handing it back to the consultant, let's make sure that if the consultant had initiated
			//  a conversation, that the coach answered it.
			if (!CheckThatCoachAnsweredCITsQuestions(theSE, theCurrentStory))
				return false;

			System.Diagnostics.Debug.Assert(eProposedNextState ==
				StoryStageLogic.ProjectStages.eTeamComplete);

			return true;
		}
		*/
		public static bool TeamComplete(StoryEditor theSE, StoryProjectData theStoryProjectData, StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			Console.WriteLine(String.Format("Checking if stage 'TeamComplete' work is finished: Name: {0}", theCurrentStory.Name));
			return true;
		}

		/// <summary>
		/// Indicates whether the new proposed state is a backwards transition or not
		/// </summary>
		/// <param name="theCurrentStory"></param>
		/// <param name="eProposedNextState"></param>
		/// <returns></returns>
		protected static bool IsGoingBackwards(StoryData theCurrentStory, StoryStageLogic.ProjectStages eProposedNextState)
		{
			return ((int)eProposedNextState < (int)theCurrentStory.ProjStage.ProjectStage);
		}
	}
}
