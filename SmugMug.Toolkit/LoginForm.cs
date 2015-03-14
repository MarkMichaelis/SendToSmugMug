using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using log4net;

namespace SmugMug.Toolkit
{
    public partial class LoginForm : Form
    {
        private Accounts accounts = null;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Accounts Accounts
        {
            get
            {
                return this.accounts;
            }
            set
            {
                this.accounts = value;
            }
        }

        public LoginForm()
        {
            InitializeComponent();
        }

        public LoginForm(Accounts accounts) : this()
        {
            this.accounts = accounts;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (this.accounts == null || this.accounts.Count == 0)
            {
                this.accounts = new Accounts();
                Account account = new Account();
                account.Username = RegistrySettings.Username;
                account.Password = RegistrySettings.Password;
                accounts.Add(account);
            }

            foreach (Account account in this.accounts)
            {
                this.comboBoxLogin.Items.Add(account.Username);
            }

            this.comboBoxLogin.SelectedItem = RegistrySettings.Username;

            if (RegistrySettings.Password.Length > 0)
            {
                this.checkBoxSignInAutomatically.Checked = true;
                this.textBoxPassword.Text = RegistrySettings.Password;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (this.comboBoxLogin.Text != String.Empty && this.textBoxPassword.Text != String.Empty)
            {
                RegistrySettings.Username = this.comboBoxLogin.Text;

                // persist all the accounts
                Account account = new Account(RegistrySettings.Username);

                RegistrySettings.Password = this.textBoxPassword.Text;

                if (this.checkBoxSignInAutomatically.Checked)
                {
                    RegistrySettings.RememberPassword = true;
                    account.Password = RegistrySettings.Password;
                }
                else
                {
                    RegistrySettings.RememberPassword = false;
                }

                if (this.accounts[RegistrySettings.Username] == null)
                {
                    this.accounts.Add(account);
                }
                else
                {
                    this.accounts[RegistrySettings.Username] = account;
                }

                //SaveAccounts();

                this.Close();
            }
        }

        private void linkLabelSignup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utilities.OpenUrl("http://www.smugmug.com/?referrer=hDBXAc8lccGdQ");
        }

        private void comboBoxLogin_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedAccount = this.comboBoxLogin.SelectedItem.ToString();
            Account account = this.accounts[selectedAccount];
            this.textBoxPassword.Text = account.Password;
        }
    }
}
