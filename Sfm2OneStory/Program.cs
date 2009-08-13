using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.IO;
using OneStoryProjectEditor;

namespace OneStoryProjectEditor
{
	class Program
	{
		const string CstrReasonLable = "reason this story is in the set:";
		const string CstrStoryCrafter = "Story crafter:";
		const string CstrBackTranslator = "Backtranslator to Hindi:";
		const string CstrTestor1 = "Testing 1:";
		const string CstrTestor2 = "Testing 2:";

		const string CstrVernName = "Kangri";
		const string CstrVernCode = "xnr";
		const string CstrVernFullStop = "।";
		const string CstrNatlName = "Hindi";
		const string CstrNatlCode = "hi";
		const string CstrNatlFullStop = "।";
		const string CstrIntlName = "English";
		const string CstrIntlCode = "en";

		static string MemberGuid(TeamMembersData members, string strName, TeamMemberData.UserTypes eType)
		{
			if (String.IsNullOrEmpty(strName))
				return strName;
			if (!members.ContainsKey(strName))
				members.Add(strName, new TeamMemberData(strName, eType));
			return members[strName].MemberGuid;
		}

		static void Main(string[] args)
		{
			string[] astrBt = File.ReadAllLines(@"L:\Pahari\Storying\Kangri\KangriStoriesBT.txt");
			string[] astrRet = File.ReadAllLines(@"L:\Pahari\Storying\Kangri\KangriTestRetellings.txt");
			string[] astrCon = File.ReadAllLines(@"L:\Pahari\Storying\Kangri\ProjectConNotes.txt");
			string[] astrCoa = File.ReadAllLines(@"L:\Pahari\Storying\Kangri\CoachingNotes.txt");

			StoriesData theStories = new StoriesData();
			int nIndexBt = 0, nIndexRet = 0, nIndexCon = 0, nIndexCoa = 0;
			SkipTo(@"\t ", @"\t ", astrBt, ref nIndexBt);
			SkipTo(@"\t ", @"\t ", astrRet, ref nIndexRet);
			SkipTo(@"\t ", @"\t ", astrCon, ref nIndexCon);
			SkipTo(@"\t ", @"\t ", astrCoa, ref nIndexCoa);

			while (nIndexBt != -1)
			{
				string strStoryName = astrBt[nIndexBt].Substring(3);
				StoryData story = new StoryData(strStoryName);
				Console.WriteLine("Story: " + strStoryName);

				// first process the 'crafting info'
				string strMarker, strData, strTestorGuid = null;
				ParseLine(astrBt[++nIndexBt], out strMarker, out strData);
				while (strMarker != @"\ln")
				{
					if (strMarker == @"\co")
					{
						if (BeginsWith(strData, CstrReasonLable))
						{
							story.CraftingInfo.StoryPurpose = strData.Substring(CstrReasonLable.Length).Trim();
						}
						else if (BeginsWith(strData, CstrStoryCrafter))
						{
							string strCrafterName = strData.Substring(CstrStoryCrafter.Length).Trim();
							story.CraftingInfo.StoryCrafterMemberID = MemberGuid(theStories.TeamMembers, strCrafterName, TeamMemberData.UserTypes.eCrafter);
						}
						else if (BeginsWith(strData, CstrBackTranslator))
						{
							string strBackTranslatorName = strData.Substring(CstrBackTranslator.Length).Trim();
							story.CraftingInfo.BackTranslatorMemberID = MemberGuid(theStories.TeamMembers, strBackTranslatorName, TeamMemberData.UserTypes.eUNS);
						}
						else if (BeginsWith(strData, CstrTestor1))
						{
							string strTestor1 = strData.Substring(CstrTestor1.Length).Trim();
							strTestorGuid = MemberGuid(theStories.TeamMembers, strTestor1, TeamMemberData.UserTypes.eUNS);
							story.CraftingInfo.Testors.Add(1, strTestorGuid);
						}
						else if (BeginsWith(strData, CstrTestor2))
						{
							string strTestor2 = strData.Substring(CstrTestor2.Length).Trim();
							strTestorGuid = MemberGuid(theStories.TeamMembers, strTestor2, TeamMemberData.UserTypes.eUNS);
							story.CraftingInfo.Testors.Add(2, strTestorGuid);
						}
					}
					else if (strMarker == @"\c")
						break;
					ParseLine(astrBt[++nIndexBt], out strMarker, out strData);
				}

				// if (SkipTo(@"\ln ", @"\t ", astrBt, ref nIndexBt))
				while (strMarker == @"\ln")
				{
					string strRef = astrBt[nIndexBt].Substring(4);
					Console.WriteLine(" ln: " + strRef);
					int nTQIndex = -1;
					VerseData verse = new VerseData();
					ParseLine(astrBt[++nIndexBt], out strMarker, out strData);
					while ((strMarker != @"\ln") && (strMarker != @"\c"))
					{
						if (strMarker == @"\xnr")
							verse.VernacularText = strData;
						else if (strMarker == @"\hnd")
							verse.NationalBTText = strData;
						else if (strMarker == @"\bt")
							verse.InternationalBTText = strData;
						else if (strMarker == @"\anc")
							verse.Anchors.AddAnchorData(strData);
						else if (strMarker == @"\tsth")
						{
							verse.TestQuestions.AddTestQuestion();
							verse.TestQuestions[++nTQIndex].QuestionVernacular = strData;
						}
						else if (strMarker == @"\tst")
							verse.TestQuestions[nTQIndex].QuestionEnglish = strData;
						else if (strMarker == @"\ans")
						{
							if (nTQIndex >= 0)
							{
								System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(strTestorGuid));
								verse.TestQuestions[nTQIndex].Answers.AddNewLine(strData, strTestorGuid);
							}
						}
						else if (strMarker == @"\cn")
						{
							System.Diagnostics.Debug.Assert(verse.Anchors.Count > 0);
							verse.Anchors[0].ExegeticalHelpNotes.AddExegeticalHelpNote(strData);
						}
						else if ((strMarker == @"\ash") || (strMarker == @"\dt") || (strMarker == @"\old"))
							Console.WriteLine("Found:" + strMarker);
						else if (!String.IsNullOrEmpty(strMarker) && !String.IsNullOrEmpty(strData))
							System.Diagnostics.Debug.Assert(false, String.Format("not handling the '{0}' marker", strMarker));

						// do next line
						if (++nIndexBt < astrBt.Length)
							ParseLine(astrBt[nIndexBt], out strMarker, out strData);
						else
							break;
					}

					// now grab the retelling
					if (SkipTo(@"\ln " + strRef, @"\t ", astrRet, ref nIndexRet))
					{
						ParseLine(astrRet[++nIndexRet], out strMarker, out strData);
						while ((strMarker != @"\ln") && (strMarker != @"\c"))
						{
							if (strMarker == @"\ret")
							{
								System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(strTestorGuid));
								verse.Retellings.AddNewLine(strData, strTestorGuid);
							}
							else if (!String.IsNullOrEmpty(strMarker) && !String.IsNullOrEmpty(strData))
								System.Diagnostics.Debug.Assert(false, String.Format("not handling the '{0}' marker", strMarker));

							ParseLine(astrRet[++nIndexRet], out strMarker, out strData);
						}
					}

					// now grab the consultant notes
					if (SkipTo(@"\ln " + strRef, @"\t ", astrCon, ref nIndexCon))
					{
						ParseLine(astrCon[++nIndexCon], out strMarker, out strData);
						while ((strMarker != @"\ln") && (strMarker != @"\c"))
						{
							if (strMarker == @"\con")
							{
								ConsultNoteDataConverter con = new ConsultantNoteData();
								con.MentorComment = strData;
								con.MentorGuid = MemberGuid(theStories.TeamMembers, "Bob", TeamMemberData.UserTypes.eConsultantInTraining);
								int nRound = 1;
								try
								{
									char chRound = (strData.Substring(0, 2) == "BE") ? strData[2] : '1';
									nRound = Convert.ToInt32(chRound.ToString());
								}
								catch { }
								con.RoundNum = nRound;
								verse.ConsultantNotes.Add(con);
							}
							else if (strMarker == @"\fac")
							{
								System.Diagnostics.Debug.Assert(verse.ConsultantNotes.Count > 0);
								ConsultNoteDataConverter con = verse.ConsultantNotes[verse.ConsultantNotes.Count - 1];
								con.MenteeResponse = strData;
								con.MenteeGuid = story.CraftingInfo.StoryCrafterMemberID;
							}
							else if (!String.IsNullOrEmpty(strMarker) && !String.IsNullOrEmpty(strData))
								System.Diagnostics.Debug.Assert(false, String.Format("not handling the '{0}' marker", strMarker));

							ParseLine(astrCon[++nIndexCon], out strMarker, out strData);
						}
					}

					// now grab the coach notes
					if (SkipTo(@"\ln " + strRef, @"\t ", astrCoa, ref nIndexCoa))
					{
						ParseLine(astrCoa[++nIndexCoa], out strMarker, out strData);
						while ((strMarker != @"\ln") && (strMarker != @"\c"))
						{
							if (strMarker == @"\cch")
							{
								ConsultNoteDataConverter coa = new CoachNoteData();
								verse.CoachNotes.Add(coa);
								coa.MentorComment = strData;
								coa.MentorGuid = MemberGuid(theStories.TeamMembers, "Judy", TeamMemberData.UserTypes.eCoach);
								int nRound = 1;
								try
								{
									char chRound = (strData.Substring(0, 2) == "JP") ? strData[2] : '1';
									nRound = Convert.ToInt32(chRound);
								}
								catch { }
								coa.RoundNum = nRound;
							}
							else if ((strMarker == @"\con") || (strMarker == @"\fac"))
							{
								if (verse.CoachNotes.Count == 0)
								{
									ConsultNoteDataConverter coa2 = new CoachNoteData();
									verse.CoachNotes.Add(coa2);
								}
								System.Diagnostics.Debug.Assert(verse.CoachNotes.Count > 0);
								ConsultNoteDataConverter coa = verse.CoachNotes[verse.CoachNotes.Count - 1];
								coa.MenteeResponse = strData;
								coa.MenteeGuid = MemberGuid(theStories.TeamMembers, "Bob", TeamMemberData.UserTypes.eConsultantInTraining);
							}
							else if (strMarker == @"\dt")
								Console.WriteLine("Found:" + strMarker);
							else if (!String.IsNullOrEmpty(strMarker) && !String.IsNullOrEmpty(strData))
								System.Diagnostics.Debug.Assert(false, String.Format("not handling the '{0}' marker", strMarker));

							ParseLine(astrCoa[++nIndexCoa], out strMarker, out strData);
						}
					}

					if (verse.HasData)
						story.Verses.Add(verse);

					if (nIndexBt < astrBt.Length - 1)
						ParseLine(astrBt[nIndexBt], out strMarker, out strData);
					else
						break;
				}

				story.ProjStage.ProjectStage = StoryStageLogic.ProjectStages.eTeamComplete;
				theStories.Add(story);
				SkipTo(@"\t ", @"\t ", astrBt, ref nIndexBt);
			}

			theStories.ProjSettings.Vernacular.LangName = CstrVernName;
			theStories.ProjSettings.Vernacular.LangCode = CstrVernCode;
			theStories.ProjSettings.Vernacular.FullStop = CstrVernFullStop;
			theStories.ProjSettings.NationalBT.LangName = CstrNatlName;
			theStories.ProjSettings.NationalBT.LangCode = CstrNatlCode;
			theStories.ProjSettings.NationalBT.FullStop = CstrNatlFullStop;
			theStories.ProjSettings.InternationalBT.LangName = CstrIntlName;
			theStories.ProjSettings.InternationalBT.LangCode = CstrIntlCode;
			SaveXElement(theStories.GetXml, @"C:\Code\StoryEditor\StoryEditor\Kangri.onestory");
		}

		static void ParseLine(string strLine, out string strMarker, out string strData)
		{
			int nSpace = strLine.IndexOf(' ');
			if (nSpace == -1)
				strMarker = strData = null;
			else
			{
				strMarker = strLine.Substring(0, nSpace);
				strData = strLine.Substring(nSpace + 1);
			}
		}

		static bool SkipTo(string strSfmCodeFind, string strSfmCodeStopBy, string[] astr, ref int nIndex)
		{
			int nLen = astr.Length;
			for (int i = nIndex + 1; i < nLen; i++)
			{
				string strLine = astr[i];
				if (String.IsNullOrEmpty(strLine.Trim()))
					continue;
				if (BeginsWith(strLine, strSfmCodeFind))
				{
					nIndex = i;
					return true;
				}
				if (BeginsWith(strLine, strSfmCodeStopBy))
				{
					nIndex = i;
					return false;
				}
			}

			nIndex = -1;
			return false;
		}

		static bool BeginsWith(string strData, string strBeginning)
		{
			return (strData.Substring(0, Math.Min(strBeginning.Length, strData.Length)) == strBeginning);
		}

		static void SaveXElement(XElement elem, string strFilename)
		{
			// create the root portions of the XML document and tack on the fragment we've been building
			XDocument doc = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("StoryProject",
					elem));

			// save it with an extra extn.
			doc.Save(strFilename);
		}
	}
}
