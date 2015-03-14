using System;
using System.Diagnostics;

namespace SmugMug.Api
{
	public class SmugMugException : ApplicationException 
	{
        public int code { get; set; }
        public string method { get; set; }

		public SmugMugException(string message) : base(message) 
		{

		}

		public SmugMugException(string message, Exception innerException) : base(message, innerException) 
		{
			
		}

        public SmugMugException(int cod, string msg, string meth)
            : base(msg)
        {
            code = cod;
            method = meth;
        }
	}

	public sealed class SmugMugUploadException : ApplicationException 
	{
		public SmugMugUploadException(string message, Exception innerException) : base(message, innerException) 
		{
			
		}
	}

	public sealed class SmugMugLoginException : ApplicationException 
	{
		public SmugMugLoginException(string message) : base(message) 
		{
			
		}
	}

	public sealed class SmugMugOfflineException : ApplicationException 
	{
		public SmugMugOfflineException(string message) : base(message) 
		{
			
		}
	}
}
