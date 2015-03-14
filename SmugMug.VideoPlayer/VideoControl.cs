using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using SmugMug.Toolkit;
using WMPDXMLib;

namespace SmugMug.VideoPlayer
{
    public partial class VideoControl : UserControl
    {
        private readonly ArrayList quickTimeExtensions = new ArrayList(new string[] { ".mov", ".qt", ".mp4", ".mqv", ".mqt", ".3gp", ".3gpp", ".3gp2", ".3g2", ".3gpp2" });
        private string url;
        private FileInfo videoFile;

        public string URL
        {
            get { return this.url; }
            set
            {
                this.url = value;

                if (this.url != null && this.url.Length > 0)
                {
                    FileInfo file = new FileInfo(this.url);
                    this.OpenVideo(file);
                }
                else
                {
                    axWindowsMediaPlayer1.Visible = false;
                }
            }
        }

        private Image photo;

        public Image Photo
        {
            set
            {
                this.Height = 160;
                this.Width = 160;
                this.Location = new Point(0, 0);

                this.linkLabelDownloadQuicktime.Visible = false;
                this.Stop();
                axWindowsMediaPlayer1.Visible = false;

                this.photo = value;
                this.pictureBoxThumbnail.Image = this.photo;
                this.pictureBoxThumbnail.Dock = DockStyle.Fill;
                this.pictureBoxThumbnail.Visible = true;

            }
        }

        /// <summary>
        /// opens a video from an avi file
        /// and plays the first frame inside the panel
        /// </summary>
        void OpenVideo(FileInfo file)
        {
            videoFile = file;
            this.linkLabelDownloadQuicktime.Visible = false;
            this.pictureBoxThumbnail.Visible = false;
            this.pictureBoxThumbnail.Dock = DockStyle.None;

            try
            {
                axWindowsMediaPlayer1.Visible = true;
                axWindowsMediaPlayer1.Open(file.FullName);
                axWindowsMediaPlayer1.Mute = true;

                DateTime start = DateTime.Now;
                TimeSpan timeout = new TimeSpan (0, 0, 10); // ten seconds

                while (axWindowsMediaPlayer1.PlayState == MediaPlayer.MPPlayStateConstants.mpWaiting) 
                {
                    Application.DoEvents();

                    if (DateTime.Now - start > timeout)
                        break;
                } 

                axWindowsMediaPlayer1.Pause();
                axWindowsMediaPlayer1.Mute = false;
            }
            catch
            {
                
            }
        }

        private void DownloadQuickTimeFilter()
        {
            Utilities.OpenUrl("http://www.medialooks.com/products/directshow_filters/quicktime_filter.html");
        }

        public void Stop()
        {
            if (this.axWindowsMediaPlayer1 != null)
            {
                axWindowsMediaPlayer1.Stop();
            }
        }

        public VideoControl()
        {
            InitializeComponent();

            axWindowsMediaPlayer1.ShowControls = false;
        }

        private void linkLabelDownloadQuicktime_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.DownloadQuickTimeFilter();
        }

        private void axWindowsMediaPlayer1_Error(object sender, EventArgs e)
        {
            if (this.quickTimeExtensions.Contains(videoFile.Extension))
            {
                this.axWindowsMediaPlayer1.Visible = false;
                this.linkLabelDownloadQuicktime.Visible = true;
                this.pictureBoxThumbnail.Image = Properties.Resources.Video;
                this.pictureBoxThumbnail.Dock = DockStyle.Fill;
                this.pictureBoxThumbnail.Visible = true;
            }
            else
            {
                this.linkLabelDownloadQuicktime.Visible = false;
                this.axWindowsMediaPlayer1.Visible = false;
                throw new VideoNotSupportedException("Video format not supported + videoFile.Extension");
            }
        }
    }
}
