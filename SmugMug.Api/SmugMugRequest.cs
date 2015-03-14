using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace SmugMug.Api
{
    class SmugMugRequest
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string UserAgent
        {
            get
            {
                string name = Assembly.GetExecutingAssembly().GetName().Name;
                return String.Format("{0}/{1}", name, Assembly.GetExecutingAssembly().GetName().Version);
            }
        }

        public static string ExecuteSmugMugHttpRequest(string method, string apiKey, string appSecret, Token accessToken, bool secure = false, params string[] args)
        {
            //if we don't have a method or the parameters are not in pairs of 2, bail
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("The method cannot be null or empty", "method");

            if (args.Length > 0 && args.Length % 2 != 0)
                throw new ArgumentException("The number of arguments must be even", "args");

            // we need to check the last null value if we have an "Extras" key but not a value
            if (args != null && args.Length > 0)
            {
                if (args[args.Length - 1] == null)
                    throw new ArgumentException("Passed in an Extras key without a value", "args");
            }

            // Generate the request string (with the oauth_signature)
            string message = OAuthUtility.GetMessageParameters(apiKey, appSecret, SmugMugApi.JsonUrlSecure, method, accessToken, args.ToArray());

            var myWebRequest = HttpWebRequest.Create(SmugMugApi.JsonUrlSecure + "?" + message);
            ((HttpWebRequest)myWebRequest).UserAgent = SmugMugRequest.UserAgent;
            ((HttpWebRequest)myWebRequest).AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            //do we have a proxy?
            if (SmugMugApi.Proxy != null && !SmugMugApi.Proxy.IsBypassed(new Uri(SmugMugApi.JsonUrlSecure)))
                myWebRequest.Proxy = SmugMugApi.Proxy;

            myWebRequest.Method = "GET";
            myWebRequest.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            logger.InfoFormat("HTTP request: {0}", myWebRequest.RequestUri);

            //we read the response
            string result = string.Empty;
            using (var response = myWebRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }

            logger.InfoFormat("HTTP result: {0}", result);

            if (String.IsNullOrEmpty(result))
                throw new Exception("SmugMug request returned null");

            return result;
        }
    }
}
