namespace OneStoryProjectEditor
{
	partial class ConsultNotesControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.labelReference = new System.Windows.Forms.Label();
			this.buttonDragDropHandle = new System.Windows.Forms.Button();
			this.contextMenuStripNotesOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripNotesOptions.SuspendLayout();
			this.SuspendLayout();
			//
			// labelReference
			//
			this.labelReference.AutoSize = true;
			this.labelReference.Location = new System.Drawing.Point(3, 0);
			this.labelReference.Name = "labelReference";
			this.labelReference.Size = new System.Drawing.Size(79, 13);
			this.labelReference.TabIndex = 0;
			this.labelReference.Text = "labelReference";
			this.labelReference.ForeColor = System.Drawing.SystemColors.ActiveCaption;
			//
			// buttonDragDropHandle
			//
			this.buttonDragDropHandle.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonDragDropHandle.ContextMenuStrip = this.contextMenuStripNotesOptions;
			this.buttonDragDropHandle.Location = new System.Drawing.Point(0, 0);
			this.buttonDragDropHandle.Margin = new System.Windows.Forms.Padding(0);
			this.buttonDragDropHandle.MaximumSize = new System.Drawing.Size(15, 15);
			this.buttonDragDropHandle.Name = "buttonDragDropHandle";
			this.buttonDragDropHandle.Size = new System.Drawing.Size(15, 15);
			this.buttonDragDropHandle.TabIndex = 1;
			this.buttonDragDropHandle.UseVisualStyleBackColor = true;
			this.buttonDragDropHandle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonDragDropHandle_MouseDown);
			this.buttonDragDropHandle.AllowDrop = true;
			this.buttonDragDropHandle.DragEnter += new System.Windows.Forms.DragEventHandler(buttonDragDropHandle_DragEnter);
			this.buttonDragDropHandle.DragDrop += new System.Windows.Forms.DragEventHandler(buttonDragDropHandle_DragDrop);
			//
			// contextMenuStripNotesOptions
			//
			this.contextMenuStripNotesOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.addNoteToolStripMenuItem});
			this.contextMenuStripNotesOptions.Name = "contextMenuStripNotesOptions";
			this.contextMenuStripNotesOptions.Size = new System.Drawing.Size(153, 48);
			//
			// addNoteToolStripMenuItem
			//
			this.addNoteToolStripMenuItem.Name = "addNoteToolStripMenuItem";
			this.addNoteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.addNoteToolStripMenuItem.Text = "&Add note";
			this.addNoteToolStripMenuItem.Click += new System.EventHandler(this.addNoteToolStripMenuItem_Click);
			//
			// ConsultNotesControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Name = "ConsultNotesControl";
			this.Size = new System.Drawing.Size(669, 225);
			this.contextMenuStripNotesOptions.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelReference;
		internal System.Windows.Forms.Button buttonDragDropHandle;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripNotesOptions;
		private System.Windows.Forms.ToolStripMenuItem addNoteToolStripMenuItem;
	}
}
