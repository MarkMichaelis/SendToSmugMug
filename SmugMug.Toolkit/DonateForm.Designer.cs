namespace SmugMug.Toolkit
{
    partial class DonateForm
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
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownDonate = new System.Windows.Forms.NumericUpDown();
            this.checkBoxDonated = new System.Windows.Forms.CheckBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDonate)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.label3.Location = new System.Drawing.Point(142, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 23);
            this.label3.TabIndex = 13;
            this.label3.Text = "US Dollars";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownDonate
            // 
            this.numericUpDownDonate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericUpDownDonate.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.numericUpDownDonate.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownDonate.Location = new System.Drawing.Point(88, 74);
            this.numericUpDownDonate.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownDonate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownDonate.Name = "numericUpDownDonate";
            this.numericUpDownDonate.Size = new System.Drawing.Size(48, 26);
            this.numericUpDownDonate.TabIndex = 12;
            this.numericUpDownDonate.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // checkBoxDonated
            // 
            this.checkBoxDonated.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxDonated.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxDonated.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.checkBoxDonated.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.checkBoxDonated.Location = new System.Drawing.Point(6, 136);
            this.checkBoxDonated.Name = "checkBoxDonated";
            this.checkBoxDonated.Size = new System.Drawing.Size(187, 23);
            this.checkBoxDonated.TabIndex = 11;
            this.checkBoxDonated.Text = "I have already donated.";
            this.checkBoxDonated.UseVisualStyleBackColor = false;
            this.checkBoxDonated.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonOk.Location = new System.Drawing.Point(199, 136);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 10;
            this.buttonOk.Text = "OK";
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.label2.Location = new System.Drawing.Point(13, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 21);
            this.label2.TabIndex = 8;
            this.label2.Text = "Amount:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 11.25F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(202)))), ((int)(((byte)(29)))));
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(331, 45);
            this.label1.TabIndex = 7;
            this.label1.Text = "My Software is always free and done on my own time. If you like it consider a don" +
    "ation.";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Location = new System.Drawing.Point(279, 136);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Cancel";
            // 
            // DonateForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SmugMug.Toolkit.Properties.Resources.Background;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(356, 162);
            this.ControlBox = false;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericUpDownDonate);
            this.Controls.Add(this.checkBoxDonated);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DonateForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Donate";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDonate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericUpDownDonate;
        private System.Windows.Forms.CheckBox checkBoxDonated;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
    }
}