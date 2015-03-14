using System.Windows.Forms;

namespace SmugMug.Toolkit
{
    public partial class DonateForm : Form
    {
        private string productName;

        public DonateForm(string productName)
        {
            InitializeComponent();

            this.productName = productName;
        }

        private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            this.PersistDonation(checkBoxDonated.Checked);
        }

        private void buttonOk_Click(object sender, System.EventArgs e)
        {
            this.PersistDonation(checkBoxDonated.Checked);

            decimal amount = this.numericUpDownDonate.Value;

            string url = "https://www.paypal.com/xclick/business=omar%40shahine.com&no_shipping=0&no_note=1&tax=0&currency_code=USD&item_name=" + this.productName + "&amount=" + amount.ToString();
            Utilities.OpenUrl(url);
        }

        private void PersistDonation(bool donated)
        {
            if (this.productName.ToLowerInvariant() == "Send to SmugMug".ToLowerInvariant())
            {
                RegistrySettings.DonatedSendToSmugMug = true;
            }
        }
    }
}
