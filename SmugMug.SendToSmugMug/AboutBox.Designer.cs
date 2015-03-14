namespace SmugMug.SendToSmugMug
{
    partial class AboutBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.linkLabelWebsite = new System.Windows.Forms.LinkLabel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // linkLabelWebsite
            // 
            this.linkLabelWebsite.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.linkLabelWebsite.BackColor = System.Drawing.Color.Transparent;
            this.linkLabelWebsite.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.linkLabelWebsite.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.linkLabelWebsite.Location = new System.Drawing.Point(12, 106);
            this.linkLabelWebsite.Name = "linkLabelWebsite";
            this.linkLabelWebsite.Size = new System.Drawing.Size(176, 23);
            this.linkLabelWebsite.TabIndex = 8;
            this.linkLabelWebsite.TabStop = true;
            this.linkLabelWebsite.Text = "www.shahine.com/omar";
            this.linkLabelWebsite.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.linkLabelWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWebsite_LinkClicked);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.buttonOk.Location = new System.Drawing.Point(263, 106);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 7;
            this.buttonOk.Text = "OK";
            // 
            // labelCopyright
            // 
            this.labelCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCopyright.BackColor = System.Drawing.Color.Transparent;
            this.labelCopyright.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.labelCopyright.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.labelCopyright.Location = new System.Drawing.Point(12, 9);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(326, 94);
            this.labelCopyright.TabIndex = 6;
            // 
            // AboutBox
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(350, 135);
            this.Controls.Add(this.linkLabelWebsite);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelCopyright);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Send to SmugMug";
            this.Load += new System.EventHandler(this.AboutBox_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel linkLabelWebsite;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label labelCopyright;

    }
}