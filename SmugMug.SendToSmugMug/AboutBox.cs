using System;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using SmugMug.Toolkit;

namespace SmugMug.SendToSmugMug
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Send to SmugMug ({0})", Assembly.GetExecutingAssembly().GetName().Version);
            sb.Append(Environment.NewLine); 
            sb.AppendFormat("Powered by SmugMug Model http://smugmugc3.codeplex.com/");
            sb.Append(Environment.NewLine);
            sb.Append("Copyright © 2013 Omar Shahine.  All rights reserved.");

            this.labelCopyright.Text = sb.ToString();
        }

        private void linkLabelWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utilities.OpenUrl("http://www.shahine.com/omar");
        }
    }
}
