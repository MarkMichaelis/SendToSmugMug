using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace SmugMug.SendToSmugMug
{
    class Photo
    {
        private string keywords;
        private int rating;
        private string title;
        private int orientation;
        private DateTime dateTimeOriginal;
        private FileInfo fileInfo;

        public Photo()
        {
        }

        public Photo(string filename)
        {
            this.fileInfo = new FileInfo(filename);

            // load metadata
            ReadMetadata(this.fileInfo.FullName);
                        
        }

        public void ReadMetadata(string sourceFile)
        {
            BitmapMetadata sourceMetadata = null;
            BitmapCreateOptions createOptions = BitmapCreateOptions.PreservePixelFormat
                                                                     | BitmapCreateOptions.IgnoreColorProfile;

            // Open the File
            using (Stream sourceStream = File.Open(sourceFile, FileMode.Open, FileAccess.Read))
            {
                // Decode the file and cache the content onload (BitmapCacheOption.OnLoad)
                // If you don't do this sourceMetadata won't be fully loaded 
                BitmapDecoder sourceDecoder = BitmapDecoder.Create(sourceStream,
                                                                     createOptions,
                                                                     BitmapCacheOption.None);

                // Check source has valid frames
                if (sourceDecoder.Frames[0] != null && sourceDecoder.Frames[0].Metadata != null)
                {
                    // Clone the metadata so we can throw away the reference to the underlying file
                    sourceMetadata = sourceDecoder.Frames[0].Metadata.Clone() as BitmapMetadata;

                    try
                    {
                        this.dateTimeOriginal = DateTime.Parse(sourceMetadata.DateTaken);
                    }
                    catch (Exception)
                    {
                        this.dateTimeOriginal = DateTime.MinValue;
                    }

                    this.rating = sourceMetadata.Rating;

                    if (sourceMetadata.Title != null)
                    {
                        this.title = sourceMetadata.Title.Trim();
                    }

                    if (sourceMetadata.ContainsQuery(QueryStrings.OrientationQuery))
                    {
                        this.orientation = Convert.ToInt32(sourceMetadata.GetQuery(QueryStrings.OrientationQuery));
                    }

                    List<string> keys = new List<string>();
                    // collect keywords
                    if (sourceMetadata.Keywords != null && sourceMetadata.Keywords.Count > 0)
                    {
                        keys.AddRange(sourceMetadata.Keywords);
                    }

                    // collect people tags
                    string microsoftRegions = @"/xmp/MP:RegionInfo/MPRI:Regions";
                    string microsoftPersonDisplayName = @"/MPReg:PersonDisplayName";

                    // Check there is a RegionInfo
                    if (sourceMetadata.ContainsQuery(microsoftRegions))
                    {
                        BitmapMetadata regionsMetadata = sourceMetadata.GetQuery(microsoftRegions) as BitmapMetadata;

                        // Loop through each Region
                        foreach (string regionQuery in regionsMetadata)
                        {
                            string regionFullQuery = microsoftRegions + regionQuery;

                            // Query for all the data for this region
                            BitmapMetadata regionMetadata = sourceMetadata.GetQuery(regionFullQuery) as BitmapMetadata;

                            if (regionMetadata != null)
                            {
                                if (regionMetadata.ContainsQuery(microsoftPersonDisplayName))
                                {
                                    var personDisplayNames = regionMetadata.GetQuery(microsoftPersonDisplayName).ToString();
                                    if (keys.Contains(personDisplayNames) == false)
                                        keys.Add(personDisplayNames);
                                }
                            }
                        }
                    }

                    this.keywords = String.Empty;
                    foreach (string key in keys)
                    {
                        if (key.Length > 0)
                        {
                            this.keywords = this.keywords + "," + "\"" + key + "\"";
                        }
                    }

                    this.keywords = this.keywords.TrimStart(',');
                }
                else
                {
                    throw new Exception("Unable to read Metadata from File");
                }
            }
        }

        /// <summary>
        /// Gets a 160 pixel Thumbnail of an <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to generate a Thumbnail.</param>
        /// <returns>A 160 pixel Thumbnail.</returns>
        public static Image GetThumbnail(Image image, RotateFlipType flip = RotateFlipType.RotateNoneFlipNone)
        {
            return Photo.GetThumbnail(image, 160, flip);
        }

        /// <summary>
        /// Gets a Thumbnail of an <see cref="Image"/>.
        /// </summary>
        /// <param name="image">The <see cref="Image"/> to generate a Thumbnail.</param>
        /// <param name="outputLength">The desired output length in pixels.</param>
        /// <returns>A Thumbnail.</returns>
        internal static Image GetThumbnail(Image image, int outputLength, RotateFlipType flip = RotateFlipType.RotateNoneFlipNone)
        {
            // hack for force a fu
            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            image.RotateFlip(flip);

            //int outputLength = 160;
            int adjustedHeight;
            int adjustedWidth;

            float aspectRatio = (float)image.Width / (float)image.Height;
            if (aspectRatio <= 1.0) //portrait
            {
                adjustedHeight = outputLength;
                adjustedWidth = System.Convert.ToInt32(outputLength / aspectRatio);
            }
            else
            {
                adjustedHeight = System.Convert.ToInt32(outputLength / aspectRatio);
                adjustedWidth = outputLength;
            }
            
            return resizeImage(image, new Size(adjustedWidth, adjustedHeight));
        }

        private static Image resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }

        public virtual Image GetFramedPhoto(int outputLength, int borderLength, int paddingLength, int dropShadowLength, Color borderColor, Color paddingColor, Color dropshadowColor, Color backgroundColor)
        {
            using (FileStream fs = new FileStream(this.fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                using (Image image = Image.FromStream(fs, true, false))
                {
                    RotateFlipType flip = OrientationToFlipType(this.orientation);
                    return GetThumbnail(image, flip);
                }
            }
        }

        private static RotateFlipType OrientationToFlipType(int orientation)
        {
            switch (orientation)
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                default:
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }

        public DateTime DateTimeOriginal
        {
            get 
            {
                return this.dateTimeOriginal;  
            }
        }
        
        public int Rating
        {
            get
            {
                return this.rating;
            }
        }

        public string Keywords
        {
            get
            {
                return this.keywords;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
        }

        public long Length
        {
            get
            {
                return this.fileInfo.Length;
            }
        }

        public int Orientation
        {
            get
            {
                return this.orientation;
            }
        }

        #region FileInfo methods
        /// <summary>
        /// Gets the absolute path for the <see cref="Photo"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the fully qualified location of path, 
        /// such as "C:\MyPhoto.jpg".</returns>
        public string GetFullPath()
        {
            return this.fileInfo.FullName;
        }

        /// <summary>
        /// Gets the file name and extension of the <see cref="Photo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing the fully qualified location of path, 
        /// such as "C:\MyPhoto.jpg".
        /// </returns>
        public string GetFileName()
        {
            return this.fileInfo.Name;
        }

        /// <summary>
        /// Gets the file name of the <see cref="Photo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> consisting of the characters after the last directory 
        /// character in path.
        /// </returns>
        public string GetFileNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(this.fileInfo.FullName);
        }

        /// <summary>
        /// Gets the extension of the <see cref="Photo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/>, including the period (.). For example, 
        /// for a file c:\NewPhoto.jpg, this property returns ".jpg".
        /// </returns>
        public string GetExtension()
        {
            return this.fileInfo.Extension;
        }

        public string GetDirectoryName()
        {
            return this.fileInfo.DirectoryName;
        }

        #endregion
    }

    struct QueryStrings
    {
        // Image width 
        internal const string ImageWidthQuery = "/app1/ifd/{ushort=256}"; // SHORT or LONG 1
        // Image height 
        internal const string ImageLengthQuery = "/app1/ifd/{ushort=257}"; // SHORT or LONG 1
        // Number of bits per component 
        internal const string BitsPerSampleQuery = "/app1/ifd/{ushort=258}"; // SHORT 3
        // Compression scheme 
        internal const string CompressionQuery = "/app1/ifd/{ushort=259}"; // SHORT 1
        // Pixel composition 
        internal const string PhotometricInterpretationQuery = "/app1/ifd/{ushort=262}"; // SHORT 1
        // Orientation of image 
        internal const string OrientationQuery = "/app1/ifd/{ushort=274}"; // SHORT 1
        // Number of components 
        internal const string SamplesPerPixelQuery = "/app1/ifd/{ushort=277}"; // SHORT 1
        // Image data arrangement 
        internal const string PlanarConfigurationQuery = "/app1/ifd/{ushort=284}"; // SHORT 1
        // Subsampling ratio of Y to C 
        internal const string YCbCrSubSamplingQuery = "/app1/ifd/{ushort=530}"; // SHORT 2
        // Y and C positioning 
        internal const string YCbCrPositioningQuery = "/app1/ifd/{ushort=531}"; // SHORT 1
        // Image resolution in width direction 
        internal const string XResolutionQuery = "/app1/ifd/{ushort=282}"; // RATIONAL 1
        // Image resolution in height direction 
        internal const string YResolutionQuery = "/app1/ifd/{ushort=283}"; // RATIONAL 1
        // Unit of X and Y resolution 
        internal const string ResolutionUnitQuery = "/app1/ifd/{ushort=296}"; // SHORT 1

        // B. Tags relating to recording offset
        // Image data location 
        internal const string StripOffsetsQuery = "/app1/ifd/{ushort=273}"; // SHORT or LONG *S
        // Number of rows per strip 
        internal const string RowsPerStripQuery = "/app1/ifd/{ushort=278}"; // SHORT or LONG 1
        // Bytes per compressed strip 
        internal const string StripByteCountsQuery = "/app1/ifd/{ushort=279}"; // SHORT or LONG *S
        // Offset to JPEG SOI 
        internal const string JPEGInterchangeFormatQuery = "/app1/ifd/{ushort=513}"; // LONG 1
        // Bytes of JPEG data 
        internal const string JPEGInterchangeFormatLengthQuery = "/app1/ifd/{ushort=514}"; // LONG 1

        // C. Tags relating to image data characteristics

        // Transfer function 
        internal const string TransferFunctionQuery = "/app1/ifd/{ushort=301}"; // SHORT 3 * 256
        // White point chromaticity 
        internal const string WhitePointQuery = "/app1/ifd/{ushort=318}"; // RATIONAL 2
        // Chromaticities of primaries 
        internal const string PrimaryChromaticitiesQuery = "/app1/ifd/{ushort=319}"; // RATIONAL 6
        // Color space transformation matrix coefficients 
        internal const string YCbCrCoefficientsQuery = "/app1/ifd/{ushort=529}"; // RATIONAL 3
        // Pair of black and white reference values 
        internal const string ReferenceBlackWhiteQuery = "/app1/ifd/{ushort=532}"; // RATIONAL 6

        // D. Other tags
        // File change date and time 
        internal const string DateTimeQuery = "/app1/ifd/{ushort=306}"; // ASCII 20
        // Image title 
        internal const string ImageDescriptionQuery = "/app1/ifd/{ushort=270}"; // ASCII Any
        // Image input equipment manufacturer 
        internal const string MakeQuery = "/app1/ifd/{ushort=271}"; // ASCII Any
        // Image input equipment model 
        internal const string ModelQuery = "/app1/ifd/{ushort=272}"; // ASCII Any
        // Software used 
        internal const string SoftwareQuery = "/app1/ifd/{ushort=305}"; // ASCII Any
        // Person who created the image 
        internal const string ArtistQuery = "/app1/ifd/{ushort=315}"; // ASCII Any
        // Copyright holder 
        internal const string CopyrightQuery = "/app1/ifd/{ushort=33432}"; // ASCII Any

        // F. Tags Relating to Date and Time
        // Date and time of original data generation 
        internal const string DateTimeOriginalQuery = "/app1/ifd/exif/{ushort=36867}"; // ASCII 20
        // Date and time of digital data generation 
        internal const string DateTimeDigitizedQuery = "/app1/ifd/exif/{ushort=36868}"; // ASCII 20
        // DateTime subseconds 
        internal const string SubSecTimeQuery = "/app1/ifd/exif/{ushort=37520}"; // ASCII Any
        // DateTimeOriginal subseconds 
        internal const string SubSecTimeOriginalQuery = "/app1/ifd/exif/{ushort=37521}"; // ASCII Any
        // DateTimeDigitized subseconds 
        internal const string SubSecTimeDigitizedQuery = "/app1/ifd/exif/{ushort=37522}"; // ASCII Any

        // GPS tag version 
        internal const string GPSVersionIDQuery = "/app1/ifd/gps/subifd:{ulong=0}"; // BYTE 4
        // North or South Latitude 
        internal const string GPSLatitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=1}"; // ASCII 2
        // Latitude        
        internal const string GPSLatitudeQuery = "/app1/ifd/gps/subifd:{ulong=2}"; // RATIONAL 3
        // East or West Longitude 
        internal const string GPSLongitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=3}"; // ASCII 2
        // Longitude 
        internal const string GPSLongitudeQuery = "/app1/ifd/gps/subifd:{ulong=4}"; // RATIONAL 3
        // Altitude reference 
        internal const string GPSAltitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=5}"; // BYTE 1
        // Altitude 
        internal const string GPSAltitudeQuery = "/app1/ifd/gps/subifd:{ulong=6}"; // RATIONAL 1
        // GPS time (atomic clock) 
        internal const string GPSTimeStampQuery = "/app1/ifd/gps/subifd:{ulong=7}"; // RATIONAL 3
        // GPS satellites used for measurement 
        internal const string GPSSatellitesQuery = "/app1/ifd/gps/subifd:{ulong=8}"; // ASCII Any
        // GPS receiver status 
        internal const string GPSStatusQuery = "/app1/ifd/gps/subifd:{ulong=9}"; // ASCII 2
        // GPS measurement mode 
        internal const string GPSMeasureModeQuery = "/app1/ifd/gps/subifd:{ulong=10}"; // ASCII 2
        // Measurement precision 
        internal const string GPSDOPQuery = "/app1/ifd/gps/subifd:{ulong=11}"; // RATIONAL 1
        // Speed unit 
        internal const string GPSSpeedRefQuery = "/app1/ifd/gps/subifd:{ulong=12}"; // ASCII 2
        // Speed of GPS receiver 
        internal const string GPSSpeedQuery = "/app1/ifd/gps/subifd:{ulong=13}"; // RATIONAL 1
        // Reference for direction of movement 
        internal const string GPSTrackRefQuery = "/app1/ifd/gps/subifd:{ulong=14}"; // ASCII 2
        // Direction of movement 
        internal const string GPSTrackQuery = "/app1/ifd/gps/subifd:{ulong=15}"; // RATIONAL 1
        // Reference for direction of image 
        internal const string GPSImgDirectionRefQuery = "/app1/ifd/gps/subifd:{ulong=16}"; // ASCII 2
        // Direction of image 
        internal const string GPSImgDirectionQuery = "/app1/ifd/gps/subifd:{ulong=17}"; // RATIONAL 1
        // Geodetic survey data used 
        internal const string GPSMapDatumQuery = "/app1/ifd/gps/subifd:{ulong=18}"; // ASCII Any
        // Reference for latitude of destination 
        internal const string GPSDestLatitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=19}"; // ASCII 2
        // Latitude of destination 
        internal const string GPSDestLatitudeQuery = "/app1/ifd/gps/subifd:{ulong=20}"; // RATIONAL 3
        // Reference for longitude of destination 
        internal const string GPSDestLongitudeRefQuery = "/app1/ifd/gps/subifd:{ulong=21}"; // ASCII 2
        // Longitude of destination 
        internal const string GPSDestLongitudeQuery = "/app1/ifd/gps/subifd:{ulong=22}"; // RATIONAL 3
        // Reference for bearing of destination 
        internal const string GPSDestBearingRefQuery = "/app1/ifd/gps/subifd:{ulong=23}"; // ASCII 2
        // Bearing of destination 
        internal const string GPSDestBearingQuery = "/app1/ifd/gps/subifd:{ulong=24}"; // RATIONAL 1
        // Reference for distance to destination 
        internal const string GPSDestDistanceRefQuery = "/app1/ifd/gps/subifd:{ulong=25}"; // ASCII 2
        // Distance to destination 
        internal const string GPSDestDistanceQuery = "/app1/ifd/gps/subifd:{ulong=26}"; // RATIONAL 1
        // Name of GPS processing method 
        internal const string GPSProcessingMethodQuery = "/app1/ifd/gps/subifd:{ulong=27}"; // UNDEFINED Any
        // Name of GPS area 
        internal const string GPSAreaInformationQuery = "/app1/ifd/gps/subifd:{ulong=28}"; // UNDEFINED Any
        // GPS date 
        internal const string GPSDateStampQuery = "/app1/ifd/gps/subifd:{ulong=29}"; // ASCII 11
        // GPS differential correction 
        internal const string GPSDifferentialQuery = "/app1/ifd/gps/subifd:{ulong=30}"; // SHORT 1
    }
}
