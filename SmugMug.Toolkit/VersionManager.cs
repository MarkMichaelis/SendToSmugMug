using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

namespace SmugMug.Toolkit
{
	/// <summary>
	/// Summary description for VersionManager.
	/// </summary>
	public class VersionManager
	{
		private string url;
		private string	name;
		private string version;
		private string availableVersion;
		private bool newVersionAvailable;
		private string downloadLocation;
		private bool checkedForUpdate = false;

		public string AvailableVersion
		{
			get { return availableVersion; }
		}

		public bool NewVersionAvailable
		{
			get { return newVersionAvailable; }
		}

		public VersionManager(string url, string name, string version, bool showResult)
		{
			this.url = url;
			this.name = name;
			this.version = version;
		}

		public void DownloadNewUpdate(bool showResult)
		{
			if (checkedForUpdate == false)
			{
				this.CheckForApplicationUpdate();
			}

			if (this.newVersionAvailable)
			{
				string msgBoxMesssage = String.Format("Version {0} of {1} is available. Would you like to download this update?", 
					this.availableVersion, System.Windows.Forms.Application.ProductName);
				string msgBoxCaption = System.Windows.Forms.Application.ProductName;
				DialogResult result = MessageBox.Show(
					msgBoxMesssage,
					msgBoxCaption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information);

				if (result == DialogResult.Yes)
				{
                    Utilities.OpenUrl(this.downloadLocation);
				}
			}
			else
			{
				if (showResult)
				{
					string msgBoxMesssage = String.Format("You have the most recent version of {0}", name);
					string msgBoxCaption = System.Windows.Forms.Application.ProductName;
					MessageBox.Show(msgBoxMesssage, msgBoxCaption);
				}
			}
		}
		
		public void CheckForApplicationUpdate()
		{	
			XmlDocument xmlDoc = new XmlDocument();

			try
			{
				xmlDoc.Load(url);
				
				this.downloadLocation = xmlDoc.GetElementsByTagName("ApplicationUrl")[0].InnerText;
				this.availableVersion = xmlDoc.GetElementsByTagName("AvailableVersion")[0].InnerText;

				if (availableVersion.CompareTo(version) > 0)
				{
					this.newVersionAvailable = true;
				}
				else
				{
					
				}

				RegistrySettings.LastUpdateTimeSendToSmugMug = DateTime.Now;
				this.checkedForUpdate = true;
			}
			catch (Exception ex)
			{
				
				throw ex;
			}
		}
	}
}
