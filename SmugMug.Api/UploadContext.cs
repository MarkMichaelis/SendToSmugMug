using System;
using System.IO;
using System.Net;

namespace SmugMug.Api
{
    public class UploadContext
    {
        public HttpWebRequest Request;  // A HTTP request used to initiate the api call to SmugMug.
        public Stream PhotoStream;  // Used to transmit data during the POST upload request.
        public Stream RequestStream;    // A stream used to write the POST upload request.
        public long CurrentPosition; // Our current byte position in the upload process.
        public int ChunkSize;   // We chunk the upload into pieces for memory and UI reasons.

        public UploadContext(string fileName)
        {
            string uploadURL = SmugMugApi.UploadUrl + fileName;

            Request = (HttpWebRequest)WebRequest.Create(uploadURL);
            Request.ContentType = "binary/octet-stream";
            Request.Method = WebRequestMethods.Http.Put;
            //Request.KeepAlive = false;
            Request.ProtocolVersion = HttpVersion.Version10;
            Request.UserAgent = SmugMugRequest.UserAgent;

            PhotoStream = null;
            RequestStream = null;
            CurrentPosition = 0;
            ChunkSize = 0;
        }

        /// <summary>
        /// Our IPublishProgressCallback callback. Each time we upload a chunk, this function
        /// is called to update the UI on how far the download has progressed.
        /// </summary>
        public int PercentComplete
        {
            get
            {
                return Convert.ToInt32((CurrentPosition * 100) /
                    (PhotoStream.Length));
            }
        }
    }
}
