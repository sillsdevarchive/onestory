using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;

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

			// set up a regex helper to pick apart the anchors
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
						{
							Dictionary<string, string> map = GetListOfAnchors(strData);
							foreach(KeyValuePair<string, string> kvp in map)
								verse.Anchors.AddAnchorData(kvp.Key, kvp.Value);
						}
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
					if (SkipTo(@"\ln " + strRef, @"\c ", astrRet, ref nIndexRet))
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
					if (SkipTo(@"\ln " + strRef, @"\c ", astrCon, ref nIndexCon))
					{
						ParseLine(astrCon[++nIndexCon], out strMarker, out strData);
						while ((strMarker != @"\ln") && (strMarker != @"\c"))
						{
							if (strMarker == @"\con")
							{
								ConsultNoteDataConverter con = new ConsultantNoteData();
								con.MentorComment = strData;
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
							}
							else if (!String.IsNullOrEmpty(strMarker) && !String.IsNullOrEmpty(strData))
								System.Diagnostics.Debug.Assert(false, String.Format("not handling the '{0}' marker", strMarker));

							ParseLine(astrCon[++nIndexCon], out strMarker, out strData);
						}
					}

					// now grab the coach notes
					if (SkipTo(@"\ln " + strRef, @"\c ", astrCoa, ref nIndexCoa))
					{
						ParseLine(astrCoa[++nIndexCoa], out strMarker, out strData);
						while ((strMarker != @"\ln") && (strMarker != @"\c"))
						{
							if (strMarker == @"\cch")
							{
								ConsultNoteDataConverter coa = new CoachNoteData();
								verse.CoachNotes.Add(coa);
								coa.MentorComment = strData;
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

				story.ProjStage.ProjectStage = StoryStageLogic.ProjectStages.eCoachReviewTest2Notes;
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
			for (int i = nIndex; i < nLen; i++)
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

		protected static Regex SearchRegEx3 = new Regex(@"([a-zA-Z1-3]{3}\.\d{2,3}\:)(\d{2}) ?- ?(\d{2})", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		protected static Regex SearchRegEx2 = new Regex(@"([a-zA-Z1-3]{3}\.\d{2,3}\:)(\d{2}), ?(\d{2})", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		protected static Regex SearchRegEx = new Regex(@"[a-zA-Z1-3]{3}\.\d{2,3}\:\d{2}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
		private static readonly char[] achAnchorDelimiters = new char[] { ';' };

		static Dictionary<string, string> GetListOfAnchors(string strParagraph)
		{
			System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(strParagraph));

			string[] aStrSentences = strParagraph.Split(achAnchorDelimiters, StringSplitOptions.RemoveEmptyEntries);

			Dictionary<string, string> mapJt2Comment = new Dictionary<string, string>();
			string strPrefix = null;
			foreach (string str in aStrSentences)
			{
				string strTrimmed = str.Trim();
				if (!String.IsNullOrEmpty(strTrimmed))
				{
					MatchCollection mc = SearchRegEx2.Matches(strTrimmed);
					if (mc.Count > 0)
					{
						System.Diagnostics.Debug.Assert(mc.Count == 1);
						// found something like, "gen.03:21, 24". Turn this into two distinct anchors
						foreach (Match match in mc)
						{
							string str1 = match.Groups[1].ToString();   // should be "gen.03:"
							string str2 = match.Groups[2].ToString();  // should be "21"
							string str3 = match.Groups[3].ToString();  // should be "24"
							mapJt2Comment.Add(str1 + str2, strTrimmed);
							mapJt2Comment.Add(str1 + str3, strTrimmed);
						}
						continue;
					}

					mc = SearchRegEx3.Matches(strTrimmed);
					if (mc.Count > 0)
					{
						System.Diagnostics.Debug.Assert(mc.Count == 1);
						// found something like, "gen.03:21 - 24". Turn this into two distinct anchors
						Match match = mc[0];
						string str1 = match.Groups[1].ToString();   // should be "gen.03:"
						string str2 = match.Groups[2].ToString();   // should be "21"
						string str3 = match.Groups[3].ToString();   // should be "24"
						int nStart = Convert.ToInt32(str2);
						int nEnd = Convert.ToInt32(str3);
						System.Diagnostics.Debug.Assert(nEnd > nStart);

						for (int i = nStart; i <= nEnd; i++)
						{
							string strJT = String.Format("{0}{1:D2}", str1, i);
							mapJt2Comment.Add(strJT, strTrimmed);
						}
						continue;
					}

					mc = SearchRegEx.Matches(strTrimmed);
					if (mc.Count > 0)
					{
						foreach (Match match in mc)
						{
							strPrefix += strTrimmed;
							mapJt2Comment.Add(match.Value, strPrefix);
							strPrefix = null;
						}
					}
					else
					{
						// this means that there wasn't an anchor here, so just attach it with
						//  the next one you find.
						strPrefix += strTrimmed + ',';
					}
				}
			}
			return mapJt2Comment;
		}

		static bool BeginsWith(string strData, string strBeginning)
		{
			return (strData.Substring(0, Math.Min(strBeginning.Length, strData.Length)) == strBeginning);
		}

		static string MemberGuid(TeamMembersData members, string strName, TeamMemberData.UserTypes eType)
		{
			if (String.IsNullOrEmpty(strName))
				return strName;
			if (!members.ContainsKey(strName))
				members.Add(strName, new TeamMemberData(strName, eType));
			return members[strName].MemberGuid;
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
