using System;
using System.IO;
using System.Resources;
using System.Windows.Forms;
using log4net;
using SmugMug.Toolkit;
using System.Linq;

namespace SmugMug.SendToSmugMug
{
	/// <summary>
	/// Summary description for Program.
	/// </summary>
	public class App
	{
		public const string UpdateUrl = "http://www.shahine.com/software/SendToSmugMug/SendToSmugMug.xml";
        //public const string UpdateUrl = "http://www.shahine.com/software/SendToSmugMug/SendToSmugMugBeta.xml";
        public const string ApiKey = "vk5mfHN3rZuz23x2uSvtEnOnXwG9IwYG";
        public const string AppSecret = "ac2937add2b2f8ee8faad44ce710e516";
        public const string SmugMugEmail = "smugmug@shahine.com";
		private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.DoEvents();

            // clean up logs folder to have a max of 20 files
            //string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = System.IO.Path.GetTempPath();
            string logs = Path.Combine(Path.GetDirectoryName(path), "Send to SmugMug Logs");
            if (Directory.Exists(logs))
            {
                var filesToDelete = new DirectoryInfo(logs).GetFiles().Where(x => x.LastAccessTime < DateTime.Now.AddDays(-1));

                try
                {
                    filesToDelete.ToList().ForEach(f => f.Delete());
                }
                catch
                {
                }
            }

			EnableLogging();

            logger.Info("OS Version: " + Environment.OSVersion);
            logger.Info(".NET Framework Version: " + Environment.Version);
            logger.Info("Send to SmugMug Version: " + Application.ProductVersion);
			logger.InfoFormat("App Launching with {0} args", args.Length);

            foreach (var item in args)
            {
                logger.Info(item);
            }

			Application.ThreadException +=new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
			Application.Run(new MainWindow(args));
		}

		// Find a named appender already attached to a logger
		public static log4net.Appender.IAppender FindAppender(string appenderName)
		{
            return LogManager.GetRepository().GetAppenders().FirstOrDefault(appender => appender.Name == appenderName);
		}

		private static void EnableLogging()
		{
			// DEBUG < INFO < WARN < ERROR < FATAL
            log4net.Appender.ConsoleAppender consoleAppender = FindAppender("ConsoleAppender") as log4net.Appender.ConsoleAppender;
            #if DEBUG
			    consoleAppender.Threshold = log4net.Core.Level.All;
            #else
			    consoleAppender.Threshold = log4net.Core.Level.Off;
            #endif
            
            log4net.Appender.FileAppender fileAppender = FindAppender("LogFileAppender") as log4net.Appender.FileAppender;
            #if TRACE
                fileAppender.Threshold = log4net.Core.Level.All;
            #else
                fileAppender.Threshold = log4net.Core.Level.Off;
            #endif
        }

        public static string GetLogFile()
        {
            log4net.Appender.FileAppender fileAppender = App.FindAppender("LogFileAppender") as log4net.Appender.FileAppender;
            return fileAppender.File;
        }

		private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			logger.Error("Unhandled Exception", e.Exception);
			
			DialogResult result = MessageBox.Show("An unexpected error has occured. Would you like to report this error via E-mail?",
			                "Oops...",
			                MessageBoxButtons.YesNo,
			                MessageBoxIcon.Asterisk);

			if (result == DialogResult.Yes)
			{
                string path = GetLogFile();
				using (StreamReader sr = File.OpenText(path))
				{
					string errorLog = sr.ReadToEnd();
					SendMailWithMailTo(App.SmugMugEmail, "Send to SmugMug Error", errorLog, path);
				}
			}
		}

		public static void SendMailWithMailTo(
			string address,
			string subject,
			string body,
			string attach)
		{
            MapiMailMessage message = new MapiMailMessage(subject, body);
            message.Recipients.Add(address);

            if (attach != null && attach.Length > 0)
            {
                message.Files.Add(attach);
            }

            message.ShowDialog();
		}
	}
}