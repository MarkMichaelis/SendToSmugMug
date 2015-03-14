using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SmugMug.Api;
using SmugMug.Toolkit;

namespace SmugMug.Api.Tests
{
    public class SmugMugAuthorize
    {
        const string authorizationUrl = "http://api.smugmug.com/services/oauth/authorize.mg";
        /// <summary>
        /// Use this method to authorize the application to SmugMug.
        /// </summary>
        public static Token AuthorizeSmugMugConsole(SmugMugApi smugmug)
        {
            Token reqTok = smugmug.GetRequestToken();

            Console.WriteLine("Press [Enter] after you authorized the application");
            Process proc = Process.Start(smugmug.GetAuthorizationURL(reqTok, AccessEnum.Full, PermissionsEnum.Modify));
            Console.ReadLine();

            Token accessTok = smugmug.GetAccessToken(reqTok);

            return accessTok;
        }
    }
}
