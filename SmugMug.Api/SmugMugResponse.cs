using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmugMug.Api
{
    public class SmugMugResponse
    {
        public string stat { get; set; }
        public string method { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }

    // The access level for this OAuth access token
    public enum AccessEnum
    {
        Full,
        Public,
    };

    public enum PermissionsEnum
    {
        Read,
        Add,
        Modify,
    };

    /// <summary>
    /// allow secure API authentication
    /// </summary>
    public class Auth
    {
        public Token Token { get; set; }
        //public oAuthUser User { get; set; }
        public User User { get; set; }
    }

    public class GetTokenResponse : SmugMugResponse
    {
        public Auth Auth { get; set; }
    }

    public class Token
    {
        public string id { get; set; }
        public string Secret { get; set; }
        /// <summary>
        /// The access level for this OAuth access token ("Full", "Public")
        /// </summary>
        public AccessEnum? Access { get; set; }
        /// <summary>
        /// The permissions for this OAuth access token. ("Read" "Add" "Modify")
        /// </summary>
        public PermissionsEnum? Permissions { get; set; }
    }

    public class LoginResponse : SmugMugResponse
    {
        public Login Login { get; set; }
    }

    public class UserResponse : SmugMugResponse
    {
        public User User { get; set; }
    }

    public class AlbumResponse : SmugMugResponse
    {
        public Album Album { get; set; }
        public Album[] Albums { get; set; }
    }

    public class CategoryResponse : SmugMugResponse
    {
        public Category Category { get; set; }
        public Category[] Categories { get; set; }
    }

    public class SubCategoryResponse : SmugMugResponse
    {
        public SubCategory SubCategory { get; set; }
        public SubCategory[] SubCategories { get; set; }
    }

    public class ShareGroupResponse : SmugMugResponse
    {
        public ShareGroup ShareGroup { get; set; }
        public ShareGroup[] ShareGroups { get; set; }
    }

    public class ImageResponse : SmugMugResponse
    {
        public Image Image { get; set; }
        public Image[] Images { get; set; }
        public Album Album { get; set; }
    }

    public class WatermarkResponse : SmugMugResponse
    {
        public Watermark Watermark { get; set; }
        public Watermark[] Watermarks { get; set; }
    }
}
