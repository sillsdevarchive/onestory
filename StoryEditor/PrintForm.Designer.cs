namespace OneStoryProjectEditor
{
    partial class PrintForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintForm));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxViewOptions = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.checkBoxFrontMatter = new System.Windows.Forms.CheckBox();
            this.checkBoxLangVernacular = new System.Windows.Forms.CheckBox();
            this.checkBoxLangNationalBT = new System.Windows.Forms.CheckBox();
            this.checkBoxLangInternationalBT = new System.Windows.Forms.CheckBox();
            this.checkBoxAnchors = new System.Windows.Forms.CheckBox();
            this.checkBoxStoryTestingQuestions = new System.Windows.Forms.CheckBox();
            this.checkBoxAnswers = new System.Windows.Forms.CheckBox();
            this.checkBoxRetellings = new System.Windows.Forms.CheckBox();
            this.checkedListBoxStories = new System.Windows.Forms.CheckedListBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPagePrintPreviewConfig = new System.Windows.Forms.TabPage();
            this.tabPagePrintPreview = new System.Windows.Forms.TabPage();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonPrint = new System.Windows.Forms.Button();
            this.htmlStoryBt = new OneStoryProjectEditor.HtmlStoryBtControl();
            this.tableLayoutPanel.SuspendLayout();
            this.groupBoxViewOptions.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPagePrintPreviewConfig.SuspendLayout();
            this.tabPagePrintPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Controls.Add(this.groupBoxViewOptions, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.checkedListBoxStories, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(817, 434);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // groupBoxViewOptions
            // 
            this.groupBoxViewOptions.Controls.Add(this.flowLayoutPanel1);
            this.groupBoxViewOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxViewOptions.Location = new System.Drawing.Point(411, 3);
            this.groupBoxViewOptions.Name = "groupBoxViewOptions";
            this.groupBoxViewOptions.Size = new System.Drawing.Size(403, 428);
            this.groupBoxViewOptions.TabIndex = 2;
            this.groupBoxViewOptions.TabStop = false;
            this.groupBoxViewOptions.Text = "Include in report";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.checkBoxFrontMatter);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxLangVernacular);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxLangNationalBT);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxLangInternationalBT);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxAnchors);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxStoryTestingQuestions);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxAnswers);
            this.flowLayoutPanel1.Controls.Add(this.checkBoxRetellings);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(397, 409);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // checkBoxFrontMatter
            // 
            this.checkBoxFrontMatter.AutoSize = true;
            this.checkBoxFrontMatter.Checked = true;
            this.checkBoxFrontMatter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFrontMatter.Location = new System.Drawing.Point(3, 3);
            this.checkBoxFrontMatter.Name = "checkBoxFrontMatter";
            this.checkBoxFrontMatter.Size = new System.Drawing.Size(143, 17);
            this.checkBoxFrontMatter.TabIndex = 7;
            this.checkBoxFrontMatter.Text = "Story Header Information";
            this.checkBoxFrontMatter.UseVisualStyleBackColor = true;
            // 
            // checkBoxLangVernacular
            // 
            this.checkBoxLangVernacular.AutoSize = true;
            this.checkBoxLangVernacular.Checked = true;
            this.checkBoxLangVernacular.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLangVernacular.Location = new System.Drawing.Point(3, 26);
            this.checkBoxLangVernacular.Name = "checkBoxLangVernacular";
            this.checkBoxLangVernacular.Size = new System.Drawing.Size(101, 17);
            this.checkBoxLangVernacular.TabIndex = 0;
            this.checkBoxLangVernacular.Text = "LangVernacular";
            this.checkBoxLangVernacular.UseVisualStyleBackColor = true;
            // 
            // checkBoxLangNationalBT
            // 
            this.checkBoxLangNationalBT.AutoSize = true;
            this.checkBoxLangNationalBT.Checked = true;
            this.checkBoxLangNationalBT.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLangNationalBT.Location = new System.Drawing.Point(3, 49);
            this.checkBoxLangNationalBT.Name = "checkBoxLangNationalBT";
            this.checkBoxLangNationalBT.Size = new System.Drawing.Size(103, 17);
            this.checkBoxLangNationalBT.TabIndex = 1;
            this.checkBoxLangNationalBT.Text = "LangNationalBT";
            this.checkBoxLangNationalBT.UseVisualStyleBackColor = true;
            // 
            // checkBoxLangInternationalBT
            // 
            this.checkBoxLangInternationalBT.AutoSize = true;
            this.checkBoxLangInternationalBT.Checked = true;
            this.checkBoxLangInternationalBT.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxLangInternationalBT.Location = new System.Drawing.Point(3, 72);
            this.checkBoxLangInternationalBT.Name = "checkBoxLangInternationalBT";
            this.checkBoxLangInternationalBT.Size = new System.Drawing.Size(165, 17);
            this.checkBoxLangInternationalBT.TabIndex = 2;
            this.checkBoxLangInternationalBT.Text = "&English back translation fields";
            this.checkBoxLangInternationalBT.UseVisualStyleBackColor = true;
            // 
            // checkBoxAnchors
            // 
            this.checkBoxAnchors.AutoSize = true;
            this.checkBoxAnchors.Checked = true;
            this.checkBoxAnchors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAnchors.Location = new System.Drawing.Point(3, 95);
            this.checkBoxAnchors.Name = "checkBoxAnchors";
            this.checkBoxAnchors.Size = new System.Drawing.Size(65, 17);
            this.checkBoxAnchors.TabIndex = 3;
            this.checkBoxAnchors.Text = "&Anchors";
            this.checkBoxAnchors.UseVisualStyleBackColor = true;
            // 
            // checkBoxStoryTestingQuestions
            // 
            this.checkBoxStoryTestingQuestions.AutoSize = true;
            this.checkBoxStoryTestingQuestions.Checked = true;
            this.checkBoxStoryTestingQuestions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxStoryTestingQuestions.Location = new System.Drawing.Point(3, 118);
            this.checkBoxStoryTestingQuestions.Name = "checkBoxStoryTestingQuestions";
            this.checkBoxStoryTestingQuestions.Size = new System.Drawing.Size(132, 17);
            this.checkBoxStoryTestingQuestions.TabIndex = 4;
            this.checkBoxStoryTestingQuestions.Text = "Story &testing questions";
            this.checkBoxStoryTestingQuestions.UseVisualStyleBackColor = true;
            // 
            // checkBoxAnswers
            // 
            this.checkBoxAnswers.AutoSize = true;
            this.checkBoxAnswers.Checked = true;
            this.checkBoxAnswers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAnswers.Location = new System.Drawing.Point(3, 141);
            this.checkBoxAnswers.Name = "checkBoxAnswers";
            this.checkBoxAnswers.Size = new System.Drawing.Size(155, 17);
            this.checkBoxAnswers.TabIndex = 8;
            this.checkBoxAnswers.Text = "Story test question &answers";
            this.checkBoxAnswers.UseVisualStyleBackColor = true;
            // 
            // checkBoxRetellings
            // 
            this.checkBoxRetellings.AutoSize = true;
            this.checkBoxRetellings.Checked = true;
            this.checkBoxRetellings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRetellings.Location = new System.Drawing.Point(3, 164);
            this.checkBoxRetellings.Name = "checkBoxRetellings";
            this.checkBoxRetellings.Size = new System.Drawing.Size(72, 17);
            this.checkBoxRetellings.TabIndex = 5;
            this.checkBoxRetellings.Text = "&Retellings";
            this.checkBoxRetellings.UseVisualStyleBackColor = true;
            // 
            // checkedListBoxStories
            // 
            this.checkedListBoxStories.CheckOnClick = true;
            this.checkedListBoxStories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBoxStories.FormattingEnabled = true;
            this.checkedListBoxStories.Location = new System.Drawing.Point(3, 3);
            this.checkedListBoxStories.Name = "checkedListBoxStories";
            this.checkedListBoxStories.Size = new System.Drawing.Size(402, 424);
            this.checkedListBoxStories.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPagePrintPreviewConfig);
            this.tabControl.Controls.Add(this.tabPagePrintPreview);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(831, 466);
            this.tabControl.TabIndex = 1;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_Selected);
            // 
            // tabPagePrintPreviewConfig
            // 
            this.tabPagePrintPreviewConfig.Controls.Add(this.tableLayoutPanel);
            this.tabPagePrintPreviewConfig.Location = new System.Drawing.Point(4, 22);
            this.tabPagePrintPreviewConfig.Name = "tabPagePrintPreviewConfig";
            this.tabPagePrintPreviewConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePrintPreviewConfig.Size = new System.Drawing.Size(823, 440);
            this.tabPagePrintPreviewConfig.TabIndex = 0;
            this.tabPagePrintPreviewConfig.Text = "Configure";
            this.tabPagePrintPreviewConfig.ToolTipText = "Configure what you want to see in the print report";
            this.tabPagePrintPreviewConfig.UseVisualStyleBackColor = true;
            // 
            // tabPagePrintPreview
            // 
            this.tabPagePrintPreview.Controls.Add(this.buttonClose);
            this.tabPagePrintPreview.Controls.Add(this.buttonPrint);
            this.tabPagePrintPreview.Controls.Add(this.htmlStoryBt);
            this.tabPagePrintPreview.Location = new System.Drawing.Point(4, 22);
            this.tabPagePrintPreview.Name = "tabPagePrintPreview";
            this.tabPagePrintPreview.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePrintPreview.Size = new System.Drawing.Size(823, 440);
            this.tabPagePrintPreview.TabIndex = 1;
            this.tabPagePrintPreview.Text = "Print Preview";
            this.tabPagePrintPreview.ToolTipText = "Click this tab to see the preview";
            this.tabPagePrintPreview.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(420, 408);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonPrint
            // 
            this.buttonPrint.Location = new System.Drawing.Point(328, 409);
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(75, 23);
            this.buttonPrint.TabIndex = 1;
            this.buttonPrint.Text = "&Print";
            this.buttonPrint.UseVisualStyleBackColor = true;
            this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click);
            // 
            // htmlStoryBt
            // 
            this.htmlStoryBt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.htmlStoryBt.Location = new System.Drawing.Point(3, 3);
            this.htmlStoryBt.MinimumSize = new System.Drawing.Size(20, 20);
            this.htmlStoryBt.Name = "htmlStoryBt";
            this.htmlStoryBt.ParentStory = null;
            this.htmlStoryBt.Size = new System.Drawing.Size(817, 400);
            this.htmlStoryBt.StoryData = null;
            this.htmlStoryBt.TabIndex = 0;
            this.htmlStoryBt.TheSE = null;
            this.htmlStoryBt.ViewItemsToInsureOn = OneStoryProjectEditor.VerseData.ViewItemToInsureOn.eUndefined;
            // 
            // PrintForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(831, 466);
            this.Controls.Add(this.tabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PrintForm";
            this.Text = "Print";
            this.tableLayoutPanel.ResumeLayout(false);
            this.groupBoxViewOptions.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPagePrintPreviewConfig.ResumeLayout(false);
            this.tabPagePrintPreview.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.CheckedListBox checkedListBoxStories;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPagePrintPreviewConfig;
        private System.Windows.Forms.TabPage tabPagePrintPreview;
        private System.Windows.Forms.GroupBox groupBoxViewOptions;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxLangVernacular;
        private System.Windows.Forms.CheckBox checkBoxLangNationalBT;
        private System.Windows.Forms.CheckBox checkBoxLangInternationalBT;
        private System.Windows.Forms.CheckBox checkBoxAnchors;
        private System.Windows.Forms.CheckBox checkBoxStoryTestingQuestions;
        private System.Windows.Forms.CheckBox checkBoxRetellings;
        private HtmlStoryBtControl htmlStoryBt;
        private System.Windows.Forms.CheckBox checkBoxFrontMatter;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonPrint;
        private System.Windows.Forms.CheckBox checkBoxAnswers;
    }
}