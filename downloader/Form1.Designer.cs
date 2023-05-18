namespace downloader
{
    partial class main
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
            this.browser = new System.Windows.Forms.WebBrowser();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.chains = new System.Windows.Forms.ListBox();
            this.start = new System.Windows.Forms.Button();
            this.unzip = new System.Windows.Forms.Button();
            this.download = new System.Windows.Forms.Button();
            this.insert = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // browser
            // 
            this.browser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.browser.Location = new System.Drawing.Point(10, 12);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(931, 741);
            this.browser.TabIndex = 1;
            this.browser.Url = new System.Uri("", System.UriKind.Relative);
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // chains
            // 
            this.chains.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chains.FormattingEnabled = true;
            this.chains.Items.AddRange(new object[] {
            "רמי לוי",
            "שופרסל דיל",
            "יוחננוף",
            "אושר עד"});
            this.chains.Location = new System.Drawing.Point(956, 268);
            this.chains.Name = "chains";
            this.chains.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chains.Size = new System.Drawing.Size(132, 485);
            this.chains.TabIndex = 2;
            // 
            // start
            // 
            this.start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.start.Location = new System.Drawing.Point(956, 11);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(132, 54);
            this.start.TabIndex = 3;
            this.start.Text = "start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // unzip
            // 
            this.unzip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.unzip.Location = new System.Drawing.Point(956, 131);
            this.unzip.Name = "unzip";
            this.unzip.Size = new System.Drawing.Size(132, 54);
            this.unzip.TabIndex = 4;
            this.unzip.Text = "unzip";
            this.unzip.UseVisualStyleBackColor = true;
            this.unzip.Click += new System.EventHandler(this.unzip_Click);
            // 
            // download
            // 
            this.download.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.download.Location = new System.Drawing.Point(956, 71);
            this.download.Name = "download";
            this.download.Size = new System.Drawing.Size(132, 54);
            this.download.TabIndex = 6;
            this.download.Text = "download";
            this.download.UseVisualStyleBackColor = true;
            this.download.Click += new System.EventHandler(this.download_Click);
            // 
            // insert
            // 
            this.insert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.insert.Location = new System.Drawing.Point(956, 191);
            this.insert.Name = "insert";
            this.insert.Size = new System.Drawing.Size(132, 54);
            this.insert.TabIndex = 7;
            this.insert.Text = "insert";
            this.insert.UseVisualStyleBackColor = true;
            this.insert.Click += new System.EventHandler(this.insert_Click);
            // 
            // main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 765);
            this.Controls.Add(this.insert);
            this.Controls.Add(this.download);
            this.Controls.Add(this.unzip);
            this.Controls.Add(this.start);
            this.Controls.Add(this.chains);
            this.Controls.Add(this.browser);
            this.Name = "main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Chain Prices Downloader";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ListBox chains;
        private System.Windows.Forms.Button start;
        private System.Windows.Forms.Button unzip;
        private System.Windows.Forms.Button download;
        private System.Windows.Forms.Button insert;
    }
}

