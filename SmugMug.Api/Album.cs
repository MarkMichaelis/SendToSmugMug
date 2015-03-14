using System.Web;
namespace SmugMug.Api
{
    public struct Album
    {
        public override string ToString()
        {
            return this.Title;
        }

        public long id;
        public string Key;
        public int CommunityID;
        public Highlight Highlight;
        // only use to set on creation
        internal int CategoryID;
        public Category Category;
        public int WatermarkID;
        public SubCategory SubCategory;
        // only use to set on creation
        internal int SubCategoryID;
        public int Position;
        public int ImageCount;
        public string Title;
        public string Description;
        public string Keywords;
        public bool Public;
        public string Password;
        public string PasswordHint;
        public bool Printable;
        public bool Filenames;
        public bool Comments;
        public bool External;
        public bool Larges;
        public bool XLarges;
        public bool X2Larges;
        public bool X3Larges;
        public bool Originals;
        public bool EXIF;
        public bool Share;
        public string SortMethod;
        public bool SortDirection;
        public string LastUpdated;
        public bool FamilyEdit;
        public bool FriendEdit;
        public bool HideOwner;
        public bool CanRank;
        public bool Clean;
        public bool Geography;
        public bool SmugSearchable;
        public bool WorldSearchable;
        public bool Protected;
        public bool Watermarking;
        public bool Header;
        internal int TemplateID;
        public Template Template;
        public bool SquareThumbs;
        public string NiceName;
        public Image[] Images;
        public string URL;

        //public static Album Create(string title)
        //{
        //    return Create(title, 0, 0);
        //}

        public static Album Create(string title, int categoryID = 0, int subcategoryID = 0, int templateID = 0)
        {
            Album album = new Album();

            album.Title = title;
            album.CategoryID = categoryID;
            album.SubCategoryID = subcategoryID;
            album.Geography = true;
            album.Clean = false;
            album.EXIF = true;
            album.Filenames = false;
            album.SquareThumbs = true;
            album.TemplateID = templateID;
            album.SortMethod = "DateTimeOriginal";
            album.SmugSearchable = true;
            album.WorldSearchable = true;
            album.External = true;
            album.Protected = false;
            album.WatermarkID = 0;
            album.Watermarking = false;
            album.Larges = true;
            album.XLarges = true;
            album.X2Larges = true;
            album.X3Larges = true;
            album.Originals = true;
            album.CanRank = true;
            album.Comments = true;
            album.Share = true;
            album.FriendEdit = false;
            album.FamilyEdit = false;
            album.Printable = true;

            return album;
        }
    }

    /// <summary>
    /// Force a display style.
    /// </summary>
    /// <remarks>Power and Pro only.</remarks>
    public enum TemplateType
    {
        ViewerChoice = 0,
        SmugMug = 3,
        Traditional = 4,
        AllThumbs = 7,
        Slideshow = 8,
        JournalOld = 9,
        SmugMugSmall = 10,
        Filmstrip = 11,
        Critique = 12,
        Journal = 16,
        Thumbnails = 17
    }

    /// <summary>
    /// The method by which to sort the photos when displaying them. The default is <see cref="Position"/>
    /// </summary>
    public enum SortMethodType
    {
        /// <summary>
        /// Sorts by user-specified position.
        /// </summary>
        Position,
        /// <summary>
        /// Sorts by the image captions.
        /// </summary>
        Caption,
        /// <summary>
        /// Sorts by the filename of each photo.
        /// </summary>
        FileName,
        /// <summary>
        /// Sorts by the date uploaded to smugmug.
        /// </summary>
        Date,
        /// <summary>
        /// Sorts by the date last modified, as told by EXIF data. Many files don't have this field correctly set.
        /// </summary>
        DateTime,
        /// <summary>
        /// Sorts by the date taken, as told by EXIF data. Many cameras don't report this properly.
        /// </summary>
        DateTimeOriginal
    }

}
