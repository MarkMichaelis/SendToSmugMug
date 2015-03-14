namespace SmugMug.VideoPlayer
{
    partial class VideoControl
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

            if (disposing && (axWindowsMediaPlayer1 != null))
            {
                axWindowsMediaPlayer1.Dispose();
                axWindowsMediaPlayer1 = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoControl));
            this.axWindowsMediaPlayer1 = new AxMediaPlayer.AxMediaPlayer();
            this.pictureBoxThumbnail = new System.Windows.Forms.PictureBox();
            this.linkLabelDownloadQuicktime = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail)).BeginInit();
            this.SuspendLayout();
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(0, 0);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(160, 144);
            this.axWindowsMediaPlayer1.TabIndex = 5;
            this.axWindowsMediaPlayer1.Visible = false;
            this.axWindowsMediaPlayer1.Error += new System.EventHandler(this.axWindowsMediaPlayer1_Error);
            // 
            // pictureBoxThumbnail
            // 
            this.pictureBoxThumbnail.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxThumbnail.Image")));
            this.pictureBoxThumbnail.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxThumbnail.InitialImage")));
            this.pictureBoxThumbnail.Location = new System.Drawing.Point(40, 32);
            this.pictureBoxThumbnail.Name = "pictureBoxThumbnail";
            this.pictureBoxThumbnail.Size = new System.Drawing.Size(82, 91);
            this.pictureBoxThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxThumbnail.TabIndex = 4;
            this.pictureBoxThumbnail.TabStop = false;
            this.pictureBoxThumbnail.Visible = false;
            // 
            // linkLabelDownloadQuicktime
            // 
            this.linkLabelDownloadQuicktime.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkLabelDownloadQuicktime.Location = new System.Drawing.Point(0, 144);
            this.linkLabelDownloadQuicktime.Name = "linkLabelDownloadQuicktime";
            this.linkLabelDownloadQuicktime.Size = new System.Drawing.Size(160, 16);
            this.linkLabelDownloadQuicktime.TabIndex = 3;
            this.linkLabelDownloadQuicktime.TabStop = true;
            this.linkLabelDownloadQuicktime.Text = "Download QuickTime Preview";
            this.linkLabelDownloadQuicktime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelDownloadQuicktime.Visible = false;
            this.linkLabelDownloadQuicktime.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDownloadQuicktime_LinkClicked);
            // 
            // VideoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axWindowsMediaPlayer1);
            this.Controls.Add(this.pictureBoxThumbnail);
            this.Controls.Add(this.linkLabelDownloadQuicktime);
            this.Name = "VideoControl";
            this.Size = new System.Drawing.Size(160, 160);
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxThumbnail)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxMediaPlayer.AxMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.PictureBox pictureBoxThumbnail;
        private System.Windows.Forms.LinkLabel linkLabelDownloadQuicktime;

    }
}
