using System;
using System.IO;
using ECInterfaces;                 // for IEncConverter
using SilEncConverters40;           // for AdaptItEncConverter

namespace OneStoryProjectEditor
{
	public class AdaptItGlossing
	{
		public enum GlossType
		{
			eUndefined = 0,
			eVernacularToNational,  // use Vern -> Natl project
			eVernacularToEnglish,   // use Vern -> Engl project
			eNationalToEnglish      // use Natl -> Engl project (which can be shared by other clients)
		}

		protected static string AdaptItGlossingLookupFileSpec(string strSourceLangName, string strTargetLangName)
		{
			return Path.Combine(AdaptItProjectFolder(strSourceLangName, strTargetLangName),
				"Glossing.xml");
		}

		protected static string AdaptItProjectFolderName(string strSourceLangName, string strTargetLangName)
		{
			return String.Format(@"{0} to {1} adaptations", strSourceLangName, strTargetLangName);
		}

		protected static string AdaptationFileName(string strSourceLangName, string strTargetLangName)
		{
			return String.Format(@"{0}.xml", AdaptItProjectFolderName(strSourceLangName, strTargetLangName));
		}

		protected static string AdaptItLookupFileSpec(string strSourceLangName, string strTargetLangName)
		{
			return Path.Combine(AdaptItProjectFolder(strSourceLangName, strTargetLangName),
				AdaptationFileName(strSourceLangName, strTargetLangName));
		}

		protected static string AdaptItProjectFileSpec(string strSourceLangName, string strTargetLangName)
		{
			return Path.Combine(AdaptItProjectFolder(strSourceLangName, strTargetLangName),
				"AI-ProjectConfiguration.aic");
		}

		protected static string AdaptItWorkFolder
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					"Adapt It Unicode Work");
			}
		}

		protected static string AdaptItProjectFolder(string strSourceLangName, string strTargetLangName)
		{
			return Path.Combine(AdaptItWorkFolder, AdaptItProjectFolderName(strSourceLangName, strTargetLangName));
		}

		protected static string AdaptItProjectAdaptationsFolder(string strSourceLangName, string strTargetLangName)
		{
			return Path.Combine(AdaptItProjectFolder(strSourceLangName, strTargetLangName),
				"Adaptations");
		}

		protected static string AdaptItLookupConverterName(string strSourceLangName, string strTargetLangName)
		{
			return String.Format(@"Lookup in {0} to {1} adaptations",
				strSourceLangName, strTargetLangName);
		}

		public static AdaptItEncConverter InitLookupAdapter(ProjectSettings proj, GlossType eGlossType,
			out string strSourceLangName, out string strTargetLangName)
		{
			var aECs = new EncConverters();
			string strName, strConverterSpec;
			ProjectSettings.LanguageInfo liSource, liTarget;
			switch (eGlossType)
			{
				case GlossType.eVernacularToNational:
					strName = AdaptItLookupConverterName(proj.Vernacular.LangName, proj.NationalBT.LangName);
					strConverterSpec = AdaptItLookupFileSpec(proj.Vernacular.LangName, proj.NationalBT.LangName);
					liSource = proj.Vernacular;
					liTarget = proj.NationalBT;
					break;

				case GlossType.eVernacularToEnglish:    // the glossing KB for the Vern to Natl project
					strName = AdaptItLookupConverterName(proj.Vernacular.LangName, proj.InternationalBT.LangName);
					strConverterSpec = AdaptItLookupFileSpec(proj.Vernacular.LangName, proj.InternationalBT.LangName);
					liSource = proj.Vernacular;
					liTarget = proj.InternationalBT; // this is still the national lg project (but the glossing KB)
					break;

				case GlossType.eNationalToEnglish:
					strName = AdaptItLookupConverterName(proj.NationalBT.LangName, proj.InternationalBT.LangName);
					strConverterSpec = AdaptItLookupFileSpec(proj.NationalBT.LangName, proj.InternationalBT.LangName);
					liSource = proj.NationalBT;         // this is a whole nuther national to English project
					liTarget = proj.InternationalBT;
					break;

				default:
					System.Diagnostics.Debug.Assert(false);
					throw new ApplicationException("Wrong glossing type specified. Send to bob_eaton@sall.com for help");
			}

			strSourceLangName = liSource.LangName;
			strTargetLangName = liTarget.LangName;

			// just in case the project doesn't exist yet...
			WriteAdaptItProjectFiles(liSource, liTarget, proj.InternationalBT); // move this to AIGuesserEC project when it's mature.

			// if we don't have the converter already in the repository.
			if (!aECs.ContainsKey(strName))
			{
				aECs.AddConversionMap(strName, strConverterSpec, ConvType.Unicode_to_from_Unicode,
					EncConverters.strTypeSILadaptit, "UNICODE", "UNICODE", ProcessTypeFlags.DontKnow);
			}

			IEncConverter aEC = aECs[strName];
			System.Diagnostics.Debug.Assert((aEC != null) && (aEC is AdaptItEncConverter));
			AdaptItEncConverter theLookupAdapter = (AdaptItEncConverter)aEC;

			// in order to get the converter to load the database, do a dummy Convert
			theLookupAdapter.Convert("nothing");
			return theLookupAdapter;
		}

		protected static void WriteAdaptItProjectFiles(ProjectSettings.LanguageInfo liSource,
			ProjectSettings.LanguageInfo liTarget, ProjectSettings.LanguageInfo liNavigation)
		{
			// create folders...
			if (!Directory.Exists(AdaptItProjectAdaptationsFolder(liSource.LangName, liTarget.LangName)))
				Directory.CreateDirectory(AdaptItProjectAdaptationsFolder(liSource.LangName, liTarget.LangName));

			// create Project file
			if (!File.Exists(AdaptItProjectFileSpec(liSource.LangName, liTarget.LangName)))
			{
				string strFormat = Properties.Settings.Default.DefaultAIProjectFile;
				string strProjectFileContents = String.Format(strFormat,
					liSource.FontToUse.Name,
					liTarget.FontToUse.Name,
					liNavigation.FontToUse.Name,
					liSource.LangName,
					liTarget.LangName,
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
					AIPunctuation(liSource.FullStop),
					AIPunctuation(liTarget.FullStop),
					(liSource.DoRtl) ? "1" : "0",
					(liTarget.DoRtl) ? "1" : "0");
				File.WriteAllText(AdaptItProjectFileSpec(liSource.LangName, liTarget.LangName), strProjectFileContents);
			}

			// create main KB
			if (!File.Exists(AdaptItLookupFileSpec(liSource.LangName, liTarget.LangName)))
			{
				string strFormat = Properties.Settings.Default.DefaultAIKBFile;
				string strKBContents = String.Format(strFormat, liSource.LangName, liTarget.LangName);
				File.WriteAllText(AdaptItLookupFileSpec(liSource.LangName, liTarget.LangName), strKBContents);
			}

			if (!File.Exists(AdaptItGlossingLookupFileSpec(liSource.LangName, liTarget.LangName)))
			{
				string strFormat = Properties.Settings.Default.DefaultAIKBFile;
				string strKBContents = String.Format(strFormat, liSource.LangName, liTarget.LangName);
				File.WriteAllText(AdaptItGlossingLookupFileSpec(liSource.LangName, liTarget.LangName), strKBContents);
			}
		}

		protected static string AIPunctuation(string strSentenceFinalPuncts)
		{
			const string CstrAdaptItPunct = "?.,;:\"!()<>{}[]“”‘’";
			string strAllPunctuation = CstrAdaptItPunct;
			foreach (char ch in strSentenceFinalPuncts)
				if (strAllPunctuation.IndexOf(ch) == -1)
					strAllPunctuation += ch;
			return strAllPunctuation;
		}
	}
}
