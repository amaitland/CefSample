namespace CefSample
{
    partial class Form1
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
            this.btCreateFormHostingBrowsers = new System.Windows.Forms.Button();
            this.btCreateAnotherBrowser = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btCreateFormHostingBrowsers
            // 
            this.btCreateFormHostingBrowsers.Location = new System.Drawing.Point(50, 32);
            this.btCreateFormHostingBrowsers.Name = "btCreateFormHostingBrowsers";
            this.btCreateFormHostingBrowsers.Size = new System.Drawing.Size(181, 77);
            this.btCreateFormHostingBrowsers.TabIndex = 0;
            this.btCreateFormHostingBrowsers.Text = "Create Form For Hosting Browsers";
            this.btCreateFormHostingBrowsers.UseVisualStyleBackColor = true;
            this.btCreateFormHostingBrowsers.Click += new System.EventHandler(this.btCreateFormHostingBrowsers_Click);
            // 
            // btCreateAnotherBrowser
            // 
            this.btCreateAnotherBrowser.Location = new System.Drawing.Point(50, 124);
            this.btCreateAnotherBrowser.Name = "btCreateAnotherBrowser";
            this.btCreateAnotherBrowser.Size = new System.Drawing.Size(181, 80);
            this.btCreateAnotherBrowser.TabIndex = 1;
            this.btCreateAnotherBrowser.Text = "Create Another Browser";
            this.btCreateAnotherBrowser.UseVisualStyleBackColor = true;
            this.btCreateAnotherBrowser.Click += new System.EventHandler(this.btCreateAnotherBrowser_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 256);
            this.Controls.Add(this.btCreateAnotherBrowser);
            this.Controls.Add(this.btCreateFormHostingBrowsers);
            this.Name = "Form1";
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btCreateFormHostingBrowsers;
        private System.Windows.Forms.Button btCreateAnotherBrowser;
    }
}

