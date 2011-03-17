using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OneStoryProjectEditor
{
	public class LnCNotesData : List<LnCNote>
	{
		public LnCNotesData()
		{
		}

		public LnCNotesData(NewDataSet projFile)
		{
			NewDataSet.StoryProjectRow theStoryProjectRow = projFile.StoryProject[0];
			NewDataSet.LnCNotesRow[] theLnCNotesRows = theStoryProjectRow.GetLnCNotesRows();
			NewDataSet.LnCNotesRow theLnCNotesRow;
			if (theLnCNotesRows.Length == 0)
				theLnCNotesRow = projFile.LnCNotes.AddLnCNotesRow(theStoryProjectRow);
			else
				theLnCNotesRow = theLnCNotesRows[0];

			foreach (NewDataSet.LnCNoteRow aLnCNoteRow in theLnCNotesRow.GetLnCNoteRows())
				Add(new LnCNote(aLnCNoteRow));
		}

		public XElement GetXml
		{
			get
			{
				var elem = new XElement("LnCNotes");

				foreach (var aLnCNote in this)
					elem.Add(aLnCNote.GetXml);

				return elem;
			}
		}

		private static TermRendering GetTermRendering(string strSearchPatterns)
		{
			return new TermRendering { Renderings = strSearchPatterns.Trim() };
		}

		private static bool StringContains(string strSearchPatterns, string strSearchString)
		{
			if (!String.IsNullOrEmpty(strSearchPatterns))
			{
				// try a simple index of without case to see if the search string is part
				//  of any of the L&C note words.
				if (strSearchPatterns.IndexOf(strSearchString, StringComparison.OrdinalIgnoreCase) != -1)
					return true;

				// also try the reverse: see if the L&C note search patterns find the
				//  search string (e.g. "God*" will find "God's")
				var termRendering = GetTermRendering(strSearchPatterns);
				var arrRegex = BiblicalTermsHTMLBuilder.GetRegexs(termRendering);
				return BiblicalTermsHTMLBuilder.SearchForHit(arrRegex, strSearchString);
			}
			return false;
		}

		public Dictionary<string,LnCNote> FindHits(string strSearchString, StoryEditor.TextFieldType eFieldType)
		{
			var map = new Dictionary<string, LnCNote>();
			strSearchString = strSearchString.Trim();

			foreach (LnCNote anLnCnote in this)
			{
				switch (eFieldType)
				{
					case StoryEditor.TextFieldType.Vernacular:
						if (StringContains(anLnCnote.VernacularRendering, strSearchString))
							map.Add(anLnCnote.VernacularRendering, anLnCnote);
						break;

					case StoryEditor.TextFieldType.NationalBt:
						if (StringContains(anLnCnote.NationalBtRendering, strSearchString))
							map.Add(anLnCnote.NationalBtRendering, anLnCnote);
						break;

					case StoryEditor.TextFieldType.InternationalBt:
						if (StringContains(anLnCnote.InternationalBtRendering, strSearchString))
							map.Add(anLnCnote.InternationalBtRendering, anLnCnote);
						break;
				}
			}
			return map;
		}
	}

	public class LnCNote
	{
		protected string guid { get; set; }
		public string VernacularRendering { get; set; }
		public string NationalBtRendering { get; set; }
		public string InternationalBtRendering { get; set; }
		public string Notes { get; set; }

		private List<string> astrKeyTermId = new List<string>();

		private string KeyTermIds
		{
			get
			{
				return string.Join(", ", astrKeyTermId.ToArray());
			}
			set
			{
				astrKeyTermId.Clear();
				string[] astr = value.Split(new [] {", "}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var s in astr)
					astrKeyTermId.Add(s);
			}
		}

		public List<Term> GetKeyTerms(BiblicalTermsList btl)
		{
			return astrKeyTermId.Select(s => btl.GetIfPresent(s)).ToList();
		}

		public string GetKeyTermsGlosses(BiblicalTermsList btl)
		{
			return String.Join(", ",
				astrKeyTermId.Select(s => btl.GetIfPresent(s))
					.Select(term => term.Gloss).ToArray());
		}

		public void SetKeyTerms(List<Term> list)
		{
			astrKeyTermId.Clear();
			foreach (Term term in list)
				astrKeyTermId.Add(term.Id);
		}

		public XElement GetXml
		{
			get
			{
				string strNote = "-";   // can't be null
				if (!String.IsNullOrEmpty(Notes))
					strNote = Notes;

				var elem = new XElement("LnCNote", strNote,
					new XAttribute("guid", guid));

				if (!String.IsNullOrEmpty(VernacularRendering))
					elem.Add(new XAttribute("VernacularRendering", VernacularRendering));

				if (!String.IsNullOrEmpty(NationalBtRendering))
					elem.Add(new XAttribute("NationalBTRendering", NationalBtRendering));

				if (!String.IsNullOrEmpty(InternationalBtRendering))
					elem.Add(new XAttribute("InternationalBTRendering", InternationalBtRendering));

				if (!String.IsNullOrEmpty(KeyTermIds))
					elem.Add(new XAttribute("KeyTermId", KeyTermIds));

				return elem;
			}
		}

		public LnCNote()
		{
			guid = Guid.NewGuid().ToString();
			Notes = String.Empty; // xml doesn't like null for this
		}

		public LnCNote(NewDataSet.LnCNoteRow theLnCNoteRow)
		{
			guid = theLnCNoteRow.guid;  // the only thing absolutely required

			Notes = theLnCNoteRow.LnCNote_text;
			if (!theLnCNoteRow.IsVernacularRenderingNull())
				VernacularRendering = theLnCNoteRow.VernacularRendering;
			if (!theLnCNoteRow.IsNationalBTRenderingNull())
				NationalBtRendering = theLnCNoteRow.NationalBTRendering;
			if (!theLnCNoteRow.IsInternationalBTRenderingNull())
				InternationalBtRendering = theLnCNoteRow.InternationalBTRendering;
			if (!theLnCNoteRow.IsKeyTermIdsNull())
				KeyTermIds = theLnCNoteRow.KeyTermIds;
		}
	}
}
