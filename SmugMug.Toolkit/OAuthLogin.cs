using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SmugMug.Toolkit
{
    public partial class OAuthLogin : Form
    {
        public event EventHandler OAuthAuthorizationCompleted;

        private string url;

        public OAuthLogin(string url)
        {
            InitializeComponent();
            this.url = url;
        }

        //public void Navigate(string url)
        //{
        //    this.webBrowser.Navigate(new Uri(url));
        //}

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.ToString().StartsWith("http://shahine.com/garage/software/send-to-smugmug/"))
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();

                if (this.OAuthAuthorizationCompleted != null)
                {
                    //this.OAuthAuthorizationCompleted(this, new EventArgs());
                    //this.Close();
                }
            }
        }

        private void OAuthLogin_Load(object sender, EventArgs e)
        {
            this.webBrowser.Navigate(new Uri(url));
        }
    }

    public class OAuthEventArgs : EventArgs
    {
    }
}
