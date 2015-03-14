using System;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace SmugMug.Toolkit
{
	/// <summary>
	/// Summary description for AccountSettings.
	/// </summary>
	public class RegistrySettings
	{
		private RegistrySettings()
		{
		}

		static public bool DonatedSendToSmugMug
		{
			get
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(KeyName);
				string id = "False";
				if (regKey != null)
				{
					id = regKey.GetValue("DonatedSendToSmugMug", "False").ToString();
					regKey.Close();
				}
				return Convert.ToBoolean(id);
			}
			set
			{
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(KeyName);
				regKey.SetValue("DonatedSendToSmugMug", value);
				regKey.Close();
			}
		}

        static public bool DisableUpdates
        {
            get
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(KeyName);
                string id = "False";
                if (regKey != null)
                {
                    id = regKey.GetValue("DisableUpdates", "False").ToString();
                    regKey.Close();
                }
                return Convert.ToBoolean(id);
            }
            set
            {
                RegistryKey regKey = Registry.CurrentUser.CreateSubKey(KeyName);
                regKey.SetValue("DisableUpdates", value);
                regKey.Close();
            }
        }

		static public DateTime LastUpdateTimeSendToSmugMug
		{
			get
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(KeyName);
				DateTime id = DateTime.MinValue;
				if (regKey != null)
				{
					try
					{
						id = DateTime.Parse(regKey.GetValue("LastUpdateTime", "").ToString());
					}
					catch
					{
						regKey.Close();
					}
				}

				return id;
			}
			set
			{
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(KeyName);
				regKey.SetValue("LastUpdateTime", value.ToString(CultureInfo.InvariantCulture));
				regKey.Close();
			}
		}

        static public string TokenID
        {
            get
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(KeyName);
                string id = "";
                if (regKey != null)
                {
                    id = regKey.GetValue("TokenID", "").ToString();
                    regKey.Close();
                }
                return id;
            }
            set
            {
                RegistryKey regKey = Registry.CurrentUser.CreateSubKey(KeyName);
                regKey.SetValue("TokenID", value);
                regKey.Close();
            }
        }

        static public string TokenSecret
        {
            get
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(KeyName);
                string id = "";
                if (regKey != null)
                {
                    id = regKey.GetValue("TokenSecret", "").ToString();


                    // DPAPI isn't supported on Windows 2000
                    if (Environment.OSVersion.Version.Major >= 6 || Environment.OSVersion.Version.Major >= 5 && Environment.OSVersion.Version.Minor > 0)
                    {
                        try
                        {
                            byte[] sourceBytes = Convert.FromBase64String(id);
                            byte[] decryptedBytes = ProtectedData.Unprotect(sourceBytes, null, DataProtectionScope.CurrentUser);

                            id = System.Text.Encoding.Unicode.GetString(decryptedBytes);
                        }
                        catch
                        {
                            if (id != "")
                            {
                                regKey = Registry.CurrentUser.CreateSubKey(KeyName);
                                id = regKey.GetValue("TokenSecret", "").ToString();
                                SaveEncryptedData(regKey, "TokenSecret", id);
                            }
                        }
                    }

                    regKey.Close();
                }
                return id;
            }
            set
            {
                RegistryKey regKey = Registry.CurrentUser.CreateSubKey(KeyName);

                if (Environment.OSVersion.Version.Major >= 6 || Environment.OSVersion.Version.Major >= 5 && Environment.OSVersion.Version.Minor > 0)
                {
                    try
                    {
                        SaveEncryptedData(regKey, "TokenSecret", value);
                    }
                    catch
                    {
                        regKey.SetValue("TokenSecret", value);
                    }
                }
                else
                {
                    regKey.SetValue("TokenSecret", value);
                }

                regKey.Close();
            }
        }

        private static void SaveEncryptedData(RegistryKey regKey, string name, string id)
        {
            byte[] sourceBytes = System.Text.Encoding.Unicode.GetBytes(id);
            byte[] encryptedBytes = ProtectedData.Protect(sourceBytes, null, DataProtectionScope.CurrentUser);
            regKey.SetValue(name, Convert.ToBase64String(encryptedBytes));
        }

		static protected string KeyName = "Software\\SmugMug";
	}
}