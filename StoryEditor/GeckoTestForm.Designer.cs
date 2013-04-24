namespace OneStoryProjectEditor
{
    partial class GeckoTestForm
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
            this.geckoWebBrowser = new Gecko.GeckoWebBrowser();
            this.SuspendLayout();
            // 
            // geckoWebBrowser
            // 
            this.geckoWebBrowser.DisableWmImeSetContext = false;
            this.geckoWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.geckoWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.geckoWebBrowser.Name = "geckoWebBrowser";
            this.geckoWebBrowser.Size = new System.Drawing.Size(842, 370);
            this.geckoWebBrowser.TabIndex = 0;
            this.geckoWebBrowser.UseHttpActivityObserver = false;
            // 
            // GeckoTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 370);
            this.Controls.Add(this.geckoWebBrowser);
            this.Name = "GeckoTestForm";
            this.Text = "GeckoTestForm";
            this.ResumeLayout(false);

        }

        #endregion

        private Gecko.GeckoWebBrowser geckoWebBrowser;
    }
}