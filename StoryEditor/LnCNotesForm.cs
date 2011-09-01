using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NetLoc;

namespace OneStoryProjectEditor
{
	public partial class LnCNotesForm : TopForm
	{
		private const int CnColumnInternationalBT = 0;
		private const int CnColumnVernacularBT = 1;
		private const int CnColumnNotes = 2;

		private StoryEditor _theSE;
		private int _nHeight;

		private LnCNotesForm()
		{
			InitializeComponent();
			Localizer.Ctrl(this);
		}

		public LnCNotesForm(StoryEditor theSE)
			: base(true)
		{
			InitializeComponent();
			Localizer.Ctrl(this);

			_theSE = theSE;
			ColumnGloss.DefaultCellStyle.Font = theSE.StoryProject.ProjSettings.InternationalBT.FontToUse;
			_nHeight = ColumnGloss.DefaultCellStyle.Font.Height;
			ColumnRenderings.DefaultCellStyle.Font = theSE.StoryProject.ProjSettings.Vernacular.FontToUse;
			_nHeight = Math.Max(_nHeight, ColumnRenderings.DefaultCellStyle.Font.Height);
			InitTable(theSE.StoryProject.LnCNotes);
		}

		private void InitTable(IEnumerable<LnCNote> lnCNotes)
		{
			dataGridViewLnCNotes.Rows.Clear();
			foreach (var aLnCNote in lnCNotes)
				AddToGrid(aLnCNote);
		}

		private void AddToGrid(LnCNote aLnCNote)
		{
			var aObjs = new object[]
							{
								aLnCNote.InternationalBtRendering,
								aLnCNote.VernacularRendering,
								aLnCNote.Notes
							};
			int nRow = dataGridViewLnCNotes.Rows.Add(aObjs);
			DataGridViewRow theRow = dataGridViewLnCNotes.Rows[nRow];
			theRow.Tag = aLnCNote;
			theRow.Height = _nHeight;
		}

		private void dataGridViewLnCNotes_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			// make sure we have something reasonable
			if ((e.RowIndex < 0) || (e.RowIndex >= dataGridViewLnCNotes.Rows.Count))
				return;

			var theRow = dataGridViewLnCNotes.Rows[e.RowIndex];
			EditLnCNote(theRow, Localizer.Str("Edit L & C Note"));
		}

		private void EditLnCNote(DataGridViewRow theRow, string strTitle)
		{
			var theLnCNote = theRow.Tag as LnCNote;
			var dlg = new AddLnCNoteForm(_theSE, theLnCNote) {Text = strTitle};
			if ((dlg.ShowDialog() == DialogResult.OK) && (theLnCNote != null))
			{
				theRow.Cells[CnColumnInternationalBT].Value = theLnCNote.InternationalBtRendering;
				theRow.Cells[CnColumnVernacularBT].Value = theLnCNote.VernacularRendering;
				theRow.Cells[CnColumnNotes].Value = theLnCNote.Notes;
				_theSE.Modified = true;
			}
		}

		private void toolStripButtonAddLnCNote_Click(object sender, EventArgs e)
		{
			var dlg = new AddLnCNoteForm(_theSE, null, null, null);
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				_theSE.StoryProject.LnCNotes.Add(dlg.TheLnCNote);
				AddToGrid(dlg.TheLnCNote);
				_theSE.Modified = true;
			}
		}

		private void toolStripButtonEditLnCNote_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(dataGridViewLnCNotes.SelectedCells.Count < 2);   // 1 or 0
			if (dataGridViewLnCNotes.SelectedCells.Count != 1)
				return;

			int nSelectedRowIndex = dataGridViewLnCNotes.SelectedCells[0].RowIndex;
			DataGridViewRow theRow = dataGridViewLnCNotes.Rows[nSelectedRowIndex];
			EditLnCNote(theRow, Localizer.Str("Add L & C Note"));
		}

		private void toolStripButtonDeleteKeyTerm_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(dataGridViewLnCNotes.SelectedCells.Count < 2);   // 1 or 0
			if (dataGridViewLnCNotes.SelectedCells.Count != 1)
				return;

			int nSelectedRowIndex = dataGridViewLnCNotes.SelectedCells[0].RowIndex;
			if (nSelectedRowIndex <= dataGridViewLnCNotes.Rows.Count - 1)
			{
				DataGridViewRow theRow = dataGridViewLnCNotes.Rows[nSelectedRowIndex];
				string strValue = (string)theRow.Cells[CnColumnInternationalBT].Value;
				// make sure the user really wants to do this
				if (MessageBox.Show(String.Format(Localizer.Str("Are you sure you want to delete the L & C Note:{0}{1}"),
												  Environment.NewLine,
												  strValue),
									StoryEditor.OseCaption,
									MessageBoxButtons.YesNoCancel)
					!= DialogResult.Yes)
					return;

				var theLnCNote = theRow.Tag as LnCNote;
				_theSE.StoryProject.LnCNotes.Remove(theLnCNote);
				InitTable(_theSE.StoryProject.LnCNotes);

				if (nSelectedRowIndex >= dataGridViewLnCNotes.Rows.Count)
					nSelectedRowIndex--;

				if ((nSelectedRowIndex >= 0) && (nSelectedRowIndex < dataGridViewLnCNotes.Rows.Count))
					dataGridViewLnCNotes.Rows[nSelectedRowIndex].Selected = true;

				_theSE.Modified = true;
			}
		}

		private void toolStripButtonSearch_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(dataGridViewLnCNotes.SelectedCells.Count < 2);   // 1 or 0
			if (dataGridViewLnCNotes.SelectedCells.Count != 1)
				return;

			int nSelectedRowIndex = dataGridViewLnCNotes.SelectedCells[0].RowIndex;
			if (nSelectedRowIndex <= dataGridViewLnCNotes.Rows.Count - 1)
			{
				DataGridViewRow theRow = dataGridViewLnCNotes.Rows[nSelectedRowIndex];
				var theLnCNote = theRow.Tag as LnCNote;
				var dlg = new ConcordanceForm(_theSE, theLnCNote.VernacularRendering,
					theLnCNote.NationalBtRendering, theLnCNote.InternationalBtRendering,
					null);
				dlg.Show();
				/* can't do this if we use 'Show' and can't not use 'Show' or we can't visit
				 * the lines found
				if ((dlg.VernacularForm != theLnCNote.VernacularRendering)
					|| (dlg.NationalForm != theLnCNote.NationalBtRendering)
					|| (dlg.InternationalForm != theLnCNote.InternationalBtRendering))
				{
					// see if the user wants to update to these values.
					theLnCNote.VernacularRendering = dlg.VernacularForm;
					theLnCNote.NationalBtRendering = dlg.NationalForm;
					theLnCNote.InternationalBtRendering = dlg.InternationalForm;
					EditLnCNote(theRow, "Would you like to modify this note with the new search pattern?");
				}
				*/
			}
		}
		/*
		private void toolStripButtonKeyTermSearch_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.Assert(dataGridViewLnCNotes.SelectedCells.Count < 2);   // 1 or 0
			if (dataGridViewLnCNotes.SelectedCells.Count != 1)
				return;

			int nSelectedRowIndex = dataGridViewLnCNotes.SelectedCells[0].RowIndex;
			if (nSelectedRowIndex <= dataGridViewLnCNotes.Rows.Count - 1)
			{
				DataGridViewRow theRow = dataGridViewLnCNotes.Rows[nSelectedRowIndex];
				var theLnCNote = theRow.Tag as LnCNote;

				var dlg = new KeyTermsSearchForm(_theSE, theLnCNote);
				dlg.Show();
			}
		}
		*/
	}
}
