namespace OneStoryProjectEditor
{
    partial class AnchorControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelAnchor = new System.Windows.Forms.Label();
            this.toolStripAnchors = new System.Windows.Forms.ToolStrip();
            this.contextMenuStripAnchorOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addConsultantCoachNoteOnThisAnchorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertNullAnchorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStripAnchorOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelAnchor
            // 
            this.labelAnchor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelAnchor.AutoSize = true;
            this.labelAnchor.Location = new System.Drawing.Point(3, 0);
            this.labelAnchor.Name = "labelAnchor";
            this.labelAnchor.Size = new System.Drawing.Size(28, 13);
            this.labelAnchor.TabIndex = 8;
            this.labelAnchor.Text = "anc:";
            // 
            // toolStripAnchors
            // 
            this.toolStripAnchors.AllowDrop = true;
            this.toolStripAnchors.ContextMenuStrip = this.contextMenuStripAnchorOptions;
            this.toolStripAnchors.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStripAnchors.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripAnchors.Location = new System.Drawing.Point(34, 0);
            this.toolStripAnchors.Name = "toolStripAnchors";
            this.toolStripAnchors.Padding = new System.Windows.Forms.Padding(0);
            this.toolStripAnchors.Size = new System.Drawing.Size(672, 20);
            this.toolStripAnchors.TabIndex = 0;
            this.toolStripAnchors.Text = "toolStripAnchors";
            this.toolStripAnchors.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripAnchors_MouseDown);
            this.toolStripAnchors.DragEnter += new System.Windows.Forms.DragEventHandler(this.toolStripAnchors_DragEnter);
            this.toolStripAnchors.DragDrop += new System.Windows.Forms.DragEventHandler(this.toolStripAnchors_DragDrop);
            // 
            // contextMenuStripAnchorOptions
            // 
            this.contextMenuStripAnchorOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.addCommentToolStripMenuItem,
            this.addConsultantCoachNoteOnThisAnchorToolStripMenuItem,
            this.insertNullAnchorToolStripMenuItem});
            this.contextMenuStripAnchorOptions.Name = "contextMenuStripAnchorOptions";
            this.contextMenuStripAnchorOptions.Size = new System.Drawing.Size(307, 136);
            this.contextMenuStripAnchorOptions.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripAnchorOptions_Opening);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.deleteToolStripMenuItem.Text = "&Delete Anchor";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // addCommentToolStripMenuItem
            // 
            this.addCommentToolStripMenuItem.Name = "addCommentToolStripMenuItem";
            this.addCommentToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.addCommentToolStripMenuItem.Text = "&Add Anchor Comment (becomes a tooltip)";
            this.addCommentToolStripMenuItem.Click += new System.EventHandler(this.addCommentToolStripMenuItem_Click);
            // 
            // addConsultantCoachNoteOnThisAnchorToolStripMenuItem
            // 
            this.addConsultantCoachNoteOnThisAnchorToolStripMenuItem.Name = "addConsultantCoachNoteOnThisAnchorToolStripMenuItem";
            this.addConsultantCoachNoteOnThisAnchorToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.addConsultantCoachNoteOnThisAnchorToolStripMenuItem.Text = "Add &Consultant/Coach Note on this Anchor";
            this.addConsultantCoachNoteOnThisAnchorToolStripMenuItem.Click += new System.EventHandler(this.addConsultantCoachNoteOnThisAnchorToolStripMenuItem_Click);
            // 
            // insertNullAnchorToolStripMenuItem
            // 
            this.insertNullAnchorToolStripMenuItem.Name = "insertNullAnchorToolStripMenuItem";
            this.insertNullAnchorToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.insertNullAnchorToolStripMenuItem.Text = "Insert \"&Empty\" Anchor";
            this.insertNullAnchorToolStripMenuItem.ToolTipText = "Use this to add an empty anchor for lines of the story that don\'t really have a b" +
                "iblical anchor";
            this.insertNullAnchorToolStripMenuItem.Click += new System.EventHandler(this.insertNullAnchorToolStripMenuItem_Click);
            // 
            // AnchorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "AnchorControl";
            this.Size = new System.Drawing.Size(706, 71);
            this.contextMenuStripAnchorOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripAnchors;
        private System.Windows.Forms.Label labelAnchor;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripAnchorOptions;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCommentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addConsultantCoachNoteOnThisAnchorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertNullAnchorToolStripMenuItem;
    }
}
