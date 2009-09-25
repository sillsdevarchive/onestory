using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using NetLoc;
using Paratext;
using SILUBS.ScriptureChecks;
using System.Linq;

// Information about renderings of Biblical terms for specific projects.
// This info is project dependent and UI language independant.

namespace OneStoryProjectEditor
{
	// ----------- Project Specific, UI Language independent data ------------------

	/// <summary>
	/// All Biblical term renderings for a single text
	/// </summary>
	public class TermRenderingsList
	{
		private static Dictionary<string, TermRenderingsList> globalDictionary = new Dictionary<string, TermRenderingsList>();

		private string scrTextName;
		private CharacterCategorizer characterCategorizer;

		private List<TermRendering> renderings = new List<TermRendering>();
		private Dictionary<string, TermRendering> renderingsDict = null;

		public static event EventHandler TermRenderingsForceReload = null;

		private bool renderingsChanged = false;

		public bool RenderingsChanged
		{
			get { return renderingsChanged;  }
			set { renderingsChanged = value; }
		}

		/// <summary>
		/// True if term renderings file has been created for this project
		/// </summary>
		/// <param name="scrTextName"></param>
		/// <returns></returns>
		public static bool HasTermRenderings(string strProjectFolder, string scrTextName)
		{
			return File.Exists(FileName(strProjectFolder, scrTextName));
		}

		/// <summary>
		/// Gets renderings for specified project.
		/// </summary>
		/// <param name="scrTextName"></param>
		/// <returns></returns>
		public static TermRenderingsList GetTermRenderings(string strProjectFolder, string scrTextName)
		{
			TermRenderingsList renderings2 = GetFromCache(scrTextName);
			if (renderings2 != null)
				return renderings2;

			if (File.Exists(FileName(strProjectFolder, scrTextName)))
				renderings2 = Deserialize(FileName(strProjectFolder, scrTextName));
			else
				renderings2 = new TermRenderingsList();

			renderings2.ScrTextName = scrTextName;
			globalDictionary[scrTextName] = renderings2;

			return renderings2;
		}

		private static TermRenderingsList Deserialize(string fileName)
		{
			TermRenderingsList renderings2;

			try
			{
				using (TextReader reader = new StreamReader(fileName))
				{
					XmlSerializer xser = Utilities.Memento.CreateXmlSerializer(typeof(TermRenderingsList));
					renderings2 = (TermRenderingsList)xser.Deserialize(reader);
				}
			}
			catch (Exception ex)
			{
				// Save corrupt file
				try
				{
					File.Move(fileName, fileName + ".corrupt");
				}
				catch { }

				MessageBox.Show(string.Format(Localizer.Str("Error reading {1}. Backup {2} was made and file was reset.\r\n{0}"),
					ex.Message, fileName, Path.GetFileName(fileName + ".corrupt")));
				renderings2 = new TermRenderingsList();
			}

			renderings2.RenderingsChanged = false;

			return renderings2;
		}

		/// <summary>
		/// Get cached term renderings, if not in cache return null
		/// </summary>
		/// <param name="scrTextName"></param>
		/// <returns></returns>
		private static TermRenderingsList GetFromCache(string scrTextName)
		{
			TermRenderingsList renderings;
			if (!globalDictionary.TryGetValue(scrTextName, out renderings))
				return null;

			return renderings;
		}

		/// <summary>
		/// If renderings for this text in cache, persist them before doing Send
		/// </summary>
		/// <param name="scrTextName"></param>
		public static void PersistBeforeSend(string strProjectFolder, string scrTextName)
		{
			if (GetFromCache(scrTextName) == null)
				return;

			GetFromCache(scrTextName).PromptForSave(strProjectFolder);
		}

		/// <summary>
		/// If there is an already open version of the rendering for this ScrText, reload them.
		/// The receive operation may have updated them.
		/// </summary>
		/// <param name="scrTextName"></param>
		public static void ReloadAfterReceive(string strProjectFolder, string scrTextName)
		{
			TermRenderingsList renderings2 = GetFromCache(scrTextName);
			if (renderings2 == null)
				return;

			if (!File.Exists(FileName(strProjectFolder, scrTextName)))
				return;

			TermRenderingsList renderings3 = Deserialize(FileName(strProjectFolder, scrTextName));

			renderings2.renderings = renderings3.renderings;
			renderings2.renderingsDict = null;

			EventHandler handler = TermRenderingsForceReload;
			if (handler != null)
				handler(renderings2, null);
		}


		/// <summary>
		/// Throw away current copy of TermRenderings and any changes made to this.
		/// Force a reload the next time someone accesses this.
		/// </summary>
		/// <param name="scrTextName"></param>
		public static void DiscardTermRenderings(string scrTextName)
		{
			globalDictionary.Remove(scrTextName);
		}


		/// <summary>
		/// Return name of biblicat terms rendering file for specified project.
		/// </summary>
		/// <param name="scrTextName"></param>
		/// <returns></returns>
		public static string FileName(string strProjectFolder, string scrTextName)
		{
			string fileName = strProjectFolder + @"\BiblicalTerms" + scrTextName + ".xml";
			return fileName;
		}

		class StringOrdinalEqualityComparer : IEqualityComparer<string>
		{
			public bool Equals(string x, string y)
			{
				return String.CompareOrdinal(x, y) == 0;
			}

			public int GetHashCode(string obj)
			{
				return obj.GetHashCode();
			}
		}

		/// <summary>
		/// Get rendering info for BiblicalTerm with the specified Id.
		/// If not info exists yet, create empty entry.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public TermRendering GetRendering(string id)
		{
			// If list has not been sorted yet, do so
			if (renderingsDict == null)
			{
				//renderingsDict = new Dictionary<string, TermRendering>(new StringOrdinalEqualityComparer());
				renderingsDict = new Dictionary<string, TermRendering>();
				for (int i = 0; i < renderings.Count; )
				{
					TermRendering rend = renderings[i];

					if (!renderingsDict.ContainsKey(rend.Id))
					{
						// Normal case, add to dictionary
						rend.CharacterCategorizer = characterCategorizer;
						renderingsDict[rend.Id] = rend;
						++i;
					}
					else
					{
						// Bad case, an earlier problem has left us with multiple
						// entries for this rendering, sigh
						renderings.RemoveAt(i);
						renderingsDict[rend.Id].MergeRenderings(rend);
					}
				}

				Debug.Assert(renderings.Count == renderingsDict.Count);
			}

			TermRendering rendering;
			if (renderingsDict.TryGetValue(id, out rendering))
				return rendering;

			rendering = new TermRendering();
			rendering.Id = id;
			rendering.CharacterCategorizer = characterCategorizer;

			renderings.Add(rendering);
			renderingsDict[rendering.Id] = rendering;

			return rendering;
		}

		public bool SomeRenderingsPresent
		{
			get { return renderings.Count > 0; }
		}

		/// <summary>
		/// True if some rendering has a tag specified
		/// </summary>
		public bool SomeTagsPresent
		{
			get
			{
				foreach (TermRendering rendering in renderings)
				{
					if (rendering.Tag.Trim() != "")
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Prompt user if they would like to save these renderings.
		/// </summary>
		public void PromptForSave(string strProjectFolder)
		{
			if (!RenderingsChanged)
				return;

			DialogResult result =
				MessageBox.Show(Properties.Resources.IDS_SaveKeyTermsPrompt,
				Properties.Resources.IDS_Caption, MessageBoxButtons.YesNo);

			if (result == DialogResult.Yes)
				Save(strProjectFolder);
			else
				DiscardTermRenderings(scrTextName);
		}


		public void Save(string strProjectFolder)
		{
			renderings.Sort();

			XmlSerializer xser = Utilities.Memento.CreateXmlSerializer(typeof(TermRenderingsList));
			System.IO.TextWriter writer = new StreamWriter(FileName(strProjectFolder, scrTextName));
			using (writer)
			{
				xser.Serialize(writer, this);
			}

			RenderingsChanged = false;
		}

		public string ScrTextName
		{
			get { return scrTextName; }
			set
			{
				scrTextName = value;
				/* rde
				ScrText scrText = ScrTextCollection.Get(scrTextName);
				characterCategorizer = new ChecksDataSource(scrText).CharacterCategorizer;
				*/
			}
		}

		/// <summary>
		/// Renderings sorted by Term.Id
		/// </summary>
		public List<TermRendering> Renderings
		{
			get { return renderings; }
			set { renderings = value; }
		}

		// MERGE CODE
		/*
		/// <summary>
		/// Do three way merge of self with "parent" and "theirs"
		/// </summary>
		/// <param name="parent">common ancestor</param>
		/// <param name="theirs"></param>
		public void Merge(TermRenderingsList parent, TermRenderingsList theirs)
		{
			List<TermRendering> result = MergeUtils.MergeLists<string, TermRendering>(
				renderings, theirs.renderings, parent!=null ? parent.renderings : new List<TermRendering>(),
				rendering => rendering.Id, MergeTermRendering,
				null, true, MergeUtils.MergeOrderRules.Loose);

			renderings = result;
		}

		internal static TermRendering MergeTermRendering(TermRendering mine, TermRendering theirs, TermRendering parent)
		{
			var result = new TermRendering(mine);

			result.MergeStatus(parent, theirs);
			result.MergeGuess(parent, theirs);
			result.MergeRenderings(parent, theirs);
			result.MergeNotes(parent, theirs);
			result.MergeDenials(parent, theirs);

			return result;
		}
		*/
		public static TermRenderingsList Deserialize(byte[] data)
		{
			TermRenderingsList renderings2;

			using (TextReader reader = new StreamReader(new MemoryStream(data)))
			{
				XmlSerializer xser = Utilities.Memento.CreateXmlSerializer(typeof(TermRenderingsList));
				renderings2 = (TermRenderingsList)xser.Deserialize(reader);
			}

			return renderings2;
		}

		public byte[] Serialize()
		{
			XmlSerializer xser = Utilities.Memento.CreateXmlSerializer(typeof(TermRenderingsList));
			var ms = new MemoryStream();

			using (TextWriter writer = new StreamWriter(ms))
			{
				xser.Serialize(writer, this);
			}

			return ms.ToArray();
		}
	}
}
