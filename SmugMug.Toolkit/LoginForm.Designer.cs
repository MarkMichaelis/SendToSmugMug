namespace SmugMug.Toolkit
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.comboBoxLogin = new System.Windows.Forms.ComboBox();
            this.linkLabelSignup = new System.Windows.Forms.LinkLabel();
            this.checkBoxSignInAutomatically = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelLogin = new System.Windows.Forms.Label();
            this.labelPassword = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxLogin
            // 
            this.comboBoxLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLogin.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.comboBoxLogin.Location = new System.Drawing.Point(112, 8);
            this.comboBoxLogin.Name = "comboBoxLogin";
            this.comboBoxLogin.Size = new System.Drawing.Size(233, 26);
            this.comboBoxLogin.TabIndex = 9;
            this.comboBoxLogin.SelectedIndexChanged += new System.EventHandler(this.comboBoxLogin_SelectedIndexChanged);
            // 
            // linkLabelSignup
            // 
            this.linkLabelSignup.BackColor = System.Drawing.Color.Transparent;
            this.linkLabelSignup.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.linkLabelSignup.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.linkLabelSignup.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.linkLabelSignup.Location = new System.Drawing.Point(12, 110);
            this.linkLabelSignup.Name = "linkLabelSignup";
            this.linkLabelSignup.Size = new System.Drawing.Size(167, 23);
            this.linkLabelSignup.TabIndex = 14;
            this.linkLabelSignup.TabStop = true;
            this.linkLabelSignup.Text = "Sign up for SmugMug";
            this.linkLabelSignup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkLabelSignup.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.linkLabelSignup.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSignup_LinkClicked);
            // 
            // checkBoxSignInAutomatically
            // 
            this.checkBoxSignInAutomatically.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxSignInAutomatically.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.checkBoxSignInAutomatically.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.checkBoxSignInAutomatically.Location = new System.Drawing.Point(112, 81);
            this.checkBoxSignInAutomatically.Name = "checkBoxSignInAutomatically";
            this.checkBoxSignInAutomatically.Size = new System.Drawing.Size(231, 24);
            this.checkBoxSignInAutomatically.TabIndex = 11;
            this.checkBoxSignInAutomatically.Text = "Sign-in Automatically";
            this.checkBoxSignInAutomatically.UseVisualStyleBackColor = false;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOK.Location = new System.Drawing.Point(185, 111);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 12;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.textBoxPassword.Location = new System.Drawing.Point(112, 40);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(233, 26);
            this.textBoxPassword.TabIndex = 10;
            // 
            // labelLogin
            // 
            this.labelLogin.BackColor = System.Drawing.Color.Transparent;
            this.labelLogin.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.labelLogin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.labelLogin.Location = new System.Drawing.Point(17, 10);
            this.labelLogin.Name = "labelLogin";
            this.labelLogin.Size = new System.Drawing.Size(89, 21);
            this.labelLogin.TabIndex = 15;
            this.labelLogin.Text = "E-mail:";
            this.labelLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelPassword
            // 
            this.labelPassword.BackColor = System.Drawing.Color.Transparent;
            this.labelPassword.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.labelPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.labelPassword.Location = new System.Drawing.Point(14, 42);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(92, 21);
            this.labelPassword.TabIndex = 16;
            this.labelPassword.Text = "Password:";
            this.labelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Location = new System.Drawing.Point(265, 111);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            // 
            // LoginForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SmugMug.Toolkit.Properties.Resources.Background;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(355, 142);
            this.ControlBox = false;
            this.Controls.Add(this.comboBoxLogin);
            this.Controls.Add(this.linkLabelSignup);
            this.Controls.Add(this.checkBoxSignInAutomatically);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.labelLogin);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.buttonCancel);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxLogin;
        private System.Windows.Forms.LinkLabel linkLabelSignup;
        private System.Windows.Forms.CheckBox checkBoxSignInAutomatically;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelLogin;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Button buttonCancel;

    }
}