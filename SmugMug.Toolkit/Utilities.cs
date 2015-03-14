using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace SmugMug.Toolkit
{
    public class Utilities
    {
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                Process.Start("iexplore.exe", url);
            }
        }
    }
}
