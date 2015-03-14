using System;

namespace SmugMug.VideoPlayer
{
	/// <summary>
	/// Summary description for VideoNotSupportedException.
	/// </summary>
	public class VideoNotSupportedException : ApplicationException 
	{
		public VideoNotSupportedException(string message) : base(message) 
		{
			
		}

		public VideoNotSupportedException(string message, Exception innerException) : base(message, innerException) 
		{
			
		}
	}
}