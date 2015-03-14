using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace SmugMug.Api
{
	/// <summary>
	/// Summary description for SmugMug.
	/// </summary>
	public class SmugMugApi
	{
        private static readonly string AuthorizationUrl = "http://api.smugmug.com/services/oauth/authorize.mg";
        public static readonly string UploadUrl = "http://upload.smugmug.com/";
        public static readonly string JsonUrlSecure = "https://api.smugmug.com/services/api/json/1.3.0/";
        private const string VERSION = "1.3.0";

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string tokenSecret = String.Empty;
		private string apiKey = String.Empty;
        private string appSecret = String.Empty;

		private string versionNumber = String.Empty;
		//private bool connected = false;

		private Account account;
        private Token accessToken;

        private Watermark[] watermarks;
		private ShareGroup[] sharegroups;
		private Category[] categories;
		private Album[] albums;
		
        static public IWebProxy Proxy { get; set; }

        ///// <summary>
        ///// Returns true if a successfull session was established.
        ///// </summary>
        public bool Connected
        {
            get { return !String.IsNullOrEmpty(this.accessToken.Secret); }
        }

        public Account Account
        {
            get { return this.account; }
        }

		/// <summary>
		/// The SmugMug ApiKey for the account.
		/// </summary>
		public string ApiKey
		{
			get { return this.apiKey; }
		}

        public string ApplicationSecret 
        {
            get { return appSecret; } 
        }

		/// <summary>
		/// Initlialized a new instance of the <see cref="SmugMugApi"/>.
		/// </summary>
		/// <param name="apiKey">The ApiKey for your account.</param>
		public SmugMugApi(Account account, string apiKey, string applicationVersionNumber)
		{
			this.account = account;
			this.apiKey = apiKey;
			this.versionNumber = applicationVersionNumber;

            log4net.Config.XmlConfigurator.Configure();
		}

        public SmugMugApi(string tokenSecret, string apiKey, string appSecret, string applicationVersionNumber)
        {
            this.tokenSecret = tokenSecret;
            this.apiKey = apiKey;
            this.appSecret = appSecret;
            this.versionNumber = applicationVersionNumber;

            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// Generates an authorization URL based on the requestToken , requiredAccess and requiredPermissions
        /// </summary>
        public string GetAuthorizationURL(Token requestToken, AccessEnum requiredAccess, PermissionsEnum requiredPermissions)
        {
            return string.Format("{0}?oauth_token={1}&Access={2}&Permissions={3}", AuthorizationUrl, requestToken.id, requiredAccess, requiredPermissions);
        }
        /// <summary>
        /// Check if the token is still valid.
        /// </summary>
        private Auth CheckAccessToken(ref Token accessToken)
        {
            var loginResult = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.auth.checkAccessToken", apiKey, appSecret, accessToken, true);
            var response = JsonConvert.DeserializeObject<GetTokenResponse>(loginResult);
            if (response.stat == "ok")
            {
                return response.Auth;
            }

            return null;
        }

        /// <summary>
        /// Get the request Token from SmugMug
        /// </summary>
        public Token GetRequestToken()
        {
            logger.InfoFormat("calling smugmug.auth.getRequestToken");
            var loginResult = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.auth.getRequestToken", apiKey, appSecret, null, true);
            var response = JsonConvert.DeserializeObject<GetTokenResponse>(loginResult);
            if (response.stat == "ok")
            {
                logger.InfoFormat("retrieved token: {0}", response.Auth.Token.id);
                return response.Auth.Token;
            }
            else if (response.code == 30)
            {
                logger.Error("failed to retrieve Request Token with error: ");
                logger.ErrorFormat("stat: {0}", response.stat);
                logger.ErrorFormat("code: {0}", response.code);
                logger.ErrorFormat("message: {0}", response.message);

                throw new SmugMugException("It appears your system clock is not set. Please check your System time and Time Zone and try again");
            }

            return null;
        }

        /// <summary>
        /// Get the access Token from SmugMug
        /// </summary>
        public Token GetAccessToken(Token requestToken)
        {
            logger.InfoFormat("calling smugmug.auth.getAccessToken with requestToken: {0}", requestToken.id);

            var loginResult = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.auth.getAccessToken", apiKey, appSecret, requestToken, true);
            var response = JsonConvert.DeserializeObject<GetTokenResponse>(loginResult);
            if (response.stat == "ok")
            {
                logger.InfoFormat("retrieved auth token", response.message);
                return response.Auth.Token;
            }
            else
            {
                logger.Error("failed to retrieve AccessToken with error: ");
                logger.ErrorFormat("stat: {0}", response.stat);
                logger.ErrorFormat("code: {0}", response.code);
                logger.ErrorFormat("message: {0}", response.message);
            }

            return null;
        }

        public void Login(Token accessToken)
        {
            if (accessToken == null)
                throw new ArgumentNullException("accessToken", "Cannot login with a null access token");

            // If we don't have the tokens
            if (accessToken.id == null || accessToken.Secret == null)
            {
                logger.Warn("There is no toklen id or token secret.");
                throw new SmugMugException("There is no username or password.");
            }

            var oauthUser = CheckAccessToken(ref accessToken);

            if (oauthUser == null)
            {
                throw new SmugMugLoginException("Access token is invalid");
            }
            else
            {
                this.accessToken = oauthUser.Token;
                this.account.User = oauthUser.User;
                //this.connected = true;
            }
        }

        public Watermark[] GetWatermarks()
        {
            logger.Info("Getting Watermarks");

            if (this.watermarks == null)
            {
                try
				{
                    string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.watermarks.get", apiKey, appSecret, this.accessToken, true);
                    var response = JsonConvert.DeserializeObject<WatermarkResponse>(result);

                    if (response.stat == "ok")
                    {
                        this.watermarks = response.Watermarks;
                        logger.InfoFormat("Retrieved {0}, Watermarks", this.watermarks.Length);
                    }
                    else
                    {
                        logger.Error("Could not retrieve Watermarks");
                        logger.Error(response.code);
                        logger.Error(response.message);
                        logger.Error(response.method);

                        return new Watermark[0];
                    }
				}
                catch (Exception ex)
                {
                    logger.Error("Could not retrieve Watermarks", ex);
                    throw new SmugMugException("Could not retrieve Watermarks", ex);
                }
			
            }

            return this.watermarks;
        }

		public ShareGroup[] GetShareGroups()
		{
			logger.Info("Getting ShareGroups");

			if (this.sharegroups == null)
			{
                try
                {
                    string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.sharegroups.get", apiKey, appSecret, this.accessToken, true);
                    var response = JsonConvert.DeserializeObject<ShareGroupResponse>(result);

                    if (response.stat == "ok")
                    {
                        this.sharegroups = response.ShareGroups;
                        logger.InfoFormat("Retrieved {0}, ShareGroups", this.sharegroups.Length);
                    }
                    else
                    {
                        logger.Error("Could not retrieve ShareGroups");
                        logger.Error(response.code);
                        logger.Error(response.message);
                        logger.Error(response.method);

                        return new ShareGroup[0];
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Could not retrieve ShareGroups", ex);
                    return new ShareGroup[0];
                }
			}

			return this.sharegroups;
		}

		public bool ShareGroupsAddAlbum(int shareGroupID, long albumID)
		{
			logger.Info("Adding ShareGroups");
			
			try
			{
                string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.sharegroups.albums.add", apiKey, appSecret, this.accessToken, true, "AlbumID", albumID.ToString(), "ShareGroupID", shareGroupID.ToString());
                var response = JsonConvert.DeserializeObject<SmugMugResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Added albumID:{0} to ShareGroup:{1}", albumID, shareGroupID);
                    return true;
                }
                else
                {
                    logger.Error("Could not add Album to ShareGroup");
                    throw new SmugMugException(response.code, response.message, response.method);
                }
			}
			catch (Exception ex)
			{
				logger.Error("Could not add Album to ShareGroup", ex);
				throw new SmugMugException("Could not add Album to ShareGroup", ex);
			}			
		}

		/// <summary>
		/// Gets all the Categories for the user.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="Category"/>.</returns>
		/// <remarks>Categories are cached for the existing Session.
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the Categories.</remarks>
		public Category[] GetCategories()
		{
			logger.Info("Getting Categories");

			if (this.categories == null)
			{
				try
				{
                    string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.categories.get", this.apiKey, this.appSecret, this.accessToken, true);
                
                    var response = JsonConvert.DeserializeObject<CategoryResponse>(result);

                    if (response.stat == "ok")
                    {
                        logger.InfoFormat("Retrieved {0}, Categories", response.Categories.Length);
                        this.categories = response.Categories;
                    }
                    else
                    {
                        logger.Error("Could not retrieve Categories");
                        logger.Error(response.code);
                        logger.Error(response.message);
                        logger.Error(response.method);
                        throw new SmugMugException(response.code, response.message, response.method);
                    }
				}
				catch (Exception ex)
				{
					logger.Error("Could not retrieve Categories", ex);
					throw new SmugMugException("Could not retrieve Categories", ex);
				}
			}

			return this.categories;
		}

		/// <summary>
		/// Gets all the SubCategories for a given <see cref="Category"/>.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="SubCategory"/>.
		/// If there are no SubCategories then it returns null.</returns>
		/// <remarks>SubCategories are cached for the existing Session.
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the SubCategories.</remarks>
		public SubCategory[] GetSubCategories(long categoryID)
		{
			logger.Info("Getting SubCategories for category id: " + categoryID.ToString());

            try
            {
                string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.subcategories.get", this.apiKey, this.appSecret, this.accessToken, true, "CategoryID", categoryID.ToString());
                var response = JsonConvert.DeserializeObject<SubCategoryResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Retrieved {0}, SubCategories", response.SubCategories.Length);
                    return response.SubCategories;
                }
                else
                {
                    logger.Info("No SubCategories for category id: " + categoryID);
                    return new SubCategory[0];
                }
            }
            catch (Exception ex)
            {
                logger.Error("Could not retrieve SubCategories", ex);
                throw new SmugMugException("Could not retrieve SubCategories", ex);
            }
		}

        public ArrayList GetAlbumsAndCategories()
        {
            return new ArrayList { GetAlbums(false), GetCategories()} ;
        }

        /// <summary>
		/// Gets all the Albums for the user.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="Album"/>.
		/// <remarks>Albums are cached for the existing Session. 
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the Albums.</remarks>
        public Album[] GetAlbums()
        {
            return GetAlbums(false);
        }

		/// <summary>
		/// Gets all the Albums for the user.
		/// </summary>
		/// <returns>An <see cref="Array"/> of <see cref="Album"/>.
		/// <remarks>Albums are cached for the existing Session. 
		/// Throws an <see cref="SmugMugException"/>
		/// if an error occurs trying to retrieve the Albums.</remarks>
		public Album[] GetAlbums(bool forceRefresh)
		{
			logger.Info("Getting Albums");

            if (this.albums == null | forceRefresh)
			{
				try
				{
                    string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.albums.get", this.apiKey, this.appSecret, this.accessToken, true);
                    var response = JsonConvert.DeserializeObject<AlbumResponse>(result);

                    if (response.stat == "ok")
                    {
                        logger.InfoFormat("Retrieved {0}, Albums", response.Albums.Length);
                        this.albums = response.Albums;

                        // need to decode any HTML entities in the Album titles
                        for (int i = 0; i < this.albums.Length; i++)
                        {
                            this.albums[i].Title = HttpUtility.HtmlDecode(this.albums[i].Title);
                        }
                    }
                    else
                    {
                        logger.Error("Could not retrieve Albums");
                        logger.Error(response.code);
                        logger.Error(response.message);
                        logger.Error(response.method);
                        throw new SmugMugException(response.code, response.message, response.method);
                    }

                    return this.albums;
					
				}
				catch (Exception ex)
				{
					logger.Error("Could not retrieve Albums", ex);
					throw new SmugMugException("Could not retrieve Albums", ex);
				}
			}
			else
			{
				return this.albums;
			}
		}

        public Album GetAlbumInfo(long albumID, string albumKey)
        {
            logger.Info("Getting Album info");

            try
            {
                string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.albums.getInfo", apiKey, appSecret, this.accessToken, true, "AlbumID", albumID.ToString(), "AlbumKey", albumKey);
                var response = JsonConvert.DeserializeObject<AlbumResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Retrieved Album: ", response.Album.Title);
                    return response.Album;
                }
                else
                {
                    logger.Error("Could not retrieve Album Info");
                    logger.Error(response.code);
                    logger.Error(response.message);
                    logger.Error(response.method);
                    throw new SmugMugException(response.code, response.message, response.method);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Could not retrieve Album Info", ex);
                throw new SmugMugException("Could not retrieve Album Info", ex);
            }
        }

		public Album CreateAlbum(Album album)
		{
			if (album.Title == null || album.Title.Length == 0)
			{
				logger.Error("Error creating Album, no Title");
				throw new ArgumentException("Name must not be empty or null");
			}
			try
			{
				logger.InfoFormat("Creating Album Title: {0}", album.Title);

                Dictionary<string, string> albumValues = new Dictionary<string, string>();

                albumValues.Add("Title", album.Title);
                if (album.CategoryID != 0)
                    albumValues.Add("CategoryID", album.CategoryID.ToString());
				if (album.SubCategoryID != 0)
					albumValues.Add("SubCategoryID", album.SubCategoryID.ToString());
				if (album.CommunityID != 0)
                    albumValues.Add("CommunityID", album.CommunityID.ToString());
				if (album.Description != null && album.Description.Length != 0)
					albumValues.Add("Description", album.Description);
				if (album.Keywords != null && album.Keywords.Length != 0)
					albumValues.Add("Keywords", album.Keywords);
				if (album.Password != null && album.Password.Length != 0)
					albumValues.Add("Password", album.Password);
				if (album.PasswordHint != null && album.PasswordHint.Length != 0)
					albumValues.Add("PasswordHint", album.PasswordHint);
				if (album.SortMethod != null)
					albumValues.Add("SortMethod", album.SortMethod);
                albumValues.Add("Position", album.Position.ToString());
                albumValues.Add("SortDirection", album.SortDirection.ToString());
                albumValues.Add("Public", album.Public.ToString());
                albumValues.Add("Filenames", album.Filenames.ToString());
                albumValues.Add("Comments", album.Comments.ToString());
                albumValues.Add("External", album.External.ToString());
                albumValues.Add("EXIF", album.EXIF.ToString());
                albumValues.Add("Share", album.Share.ToString());
                albumValues.Add("Printable", album.Printable.ToString());

                albumValues.Add("Geography", album.Geography.ToString());
                albumValues.Add("WorldSearchable", album.WorldSearchable.ToString());
                albumValues.Add("SmugSearchable", album.SmugSearchable.ToString());
                albumValues.Add("HideOwner", album.HideOwner.ToString());
                albumValues.Add("FriendEdit", album.FriendEdit.ToString());
                albumValues.Add("FamilyEdit", album.FamilyEdit.ToString());
                albumValues.Add("CanRank", album.CanRank.ToString());
                albumValues.Add("SquareThumbs", album.SquareThumbs.ToString());

                albumValues.Add("Larges", album.Larges.ToString());
                albumValues.Add("XLarges", album.XLarges.ToString());
                albumValues.Add("X2Larges", album.X2Larges.ToString());
                albumValues.Add("X3Larges", album.X3Larges.ToString());
                albumValues.Add("Originals", album.Originals.ToString());

                if (this.account.User.AccountType != AccountTypeEnum.Standard)
                {
                    albumValues.Add("Header", album.Header.ToString());
                }

                if (this.account.User.AccountType == AccountTypeEnum.Pro) 
                {
                    albumValues.Add("TemplateID", album.TemplateID.ToString());
                    albumValues.Add("Clean", album.Clean.ToString());
                    albumValues.Add("Protected", album.Protected.ToString());
                    albumValues.Add("Watermarking", album.Watermarking.ToString());

                    if (album.WatermarkID != 0)
                    {
                        albumValues.Add("WatermarkID", album.WatermarkID.ToString());
                    }
                }

                albumValues.Add("Extras", "Title");

                List<string> albumArgs = new List<string>();
                foreach (var item in albumValues)
	            {
                    albumArgs.Add(item.Key);
                    albumArgs.Add(item.Value);
	            }

                string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.albums.create", apiKey, appSecret, this.accessToken, true, albumArgs.ToArray());
                var response = JsonConvert.DeserializeObject<AlbumResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Created Album with album id: {0}, album name: {1}, album key: {2}", response.Album.id, response.Album.Title, response.Album.Key);
                    
                    // reload the albums
                    this.albums = GetAlbums(true);
                    return response.Album;
                }
                else
                {
                    logger.Error("Could not retrieve Albums");
                    logger.Error(response.code);
                    logger.Error(response.message);
                    logger.Error(response.method);
                    throw new SmugMugException(response.code, response.message, response.method);
                }
			}
			catch (Exception ex)
			{
				logger.Error("An error occured trying to create a new Album: " + album.Title, ex);
				throw new SmugMugException("An error occured trying to create a new Album: " + album.Title, ex);
			}
		}

		public bool SetAlbumHighlight(long albumID, long highlightID)
		{
			try
			{
                logger.InfoFormat("Changing Album highlight for album id: {0}, highlight id: {1}", albumID, highlightID);

                string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.albums.changeSettings", apiKey, appSecret, this.accessToken, true, "AlbumID", albumID.ToString(), "HighlightID", highlightID.ToString(), "Extras", "Highlight");
                var response = JsonConvert.DeserializeObject<AlbumResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Changed Album highlight for album id:{0} to:{1}", albumID, highlightID);
                    if (response.Album.Highlight.id == highlightID)
                        return true;
                    else
                    {
                        logger.Error("Could not change Album highlight");
                        logger.Error(response.code);
                        logger.Error(response.message);
                        logger.Error(response.method);
                        return false;
                    }
                }
                else
                {
                    logger.Error("Could not change Album highlight");
                    throw new SmugMugException(response.code, response.message, response.method);
                }
			}
			catch (Exception ex)
			{
				logger.Error("An error occured trying to set an album settings: " + albumID, ex);
				throw new SmugMugException("An error occured trying to set an album settings: " + albumID, ex);
			}
		}

		public bool DeleteAlbum(long albumID)
		{
			try
			{
                logger.InfoFormat("Deleting Album with album id: {0}", albumID);

                string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.albums.delete", apiKey, appSecret, this.accessToken, true, "AlbumID", albumID.ToString());
                var response = JsonConvert.DeserializeObject<SmugMugResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Deleted album id:{0}", albumID);
                    return true;
                }
                else
                {
                    logger.Error("An error occured trying to delete an Album");
                    throw new SmugMugException(response.code, response.message, response.method);
                }
			}
			catch (Exception ex)
			{
				throw new SmugMugException("An error occured trying to delete an Album: " + albumID, ex);
			}
		}

        public bool DeleteImage(long imageID)
        {
            try
            {
                logger.InfoFormat("Deleting Image with image id: {0}", imageID);

                string result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.images.delete", apiKey, appSecret, this.accessToken, true, "ImageID", imageID.ToString());
                var response = JsonConvert.DeserializeObject<SmugMugResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Deleted image id:{0}", imageID);
                    return true;
                }
                else
                {
                    logger.Error("An error occured trying to delete an Image");
                    throw new SmugMugException(response.code, response.message, response.method);
                }
            }
            catch (Exception ex)
            {
                throw new SmugMugException("An error occured trying to delete an Image: " + imageID, ex);
            }
        }

		/// <summary>
		/// This method will fetch all the imageIDs for the album specified by id. 
		/// The Album must be owned by the Session holder, or else be both non-passworded and Public, 
		/// to return results. Otherwise, an "invalid user" faultCode will result.
		/// </summary>
		/// <param name="albumID">The albumID to retreive the AlbumInfo.</param>
		/// <returns></returns>
		public int[] GetImages(long albumID, string albumKey)
		{
			return GetimageIDs(albumID, albumKey, null);
		}
		
		/// <summary>
		/// This method will fetch all the imageIDs for the album specified by id. 
		/// The Album must be owned by the Session holder, or else be both non-passworded and Public, 
		/// to return results. Otherwise, an "invalid user" faultCode will result.
		/// </summary>
		/// <param name="albumID">The albumID to retreive the AlbumInfo.</param>
		/// <param name="heavy">Retrieve all album info</param>
		/// <returns></returns>
		private int[] GetimageIDs(long albumID, string albumKey, string extras)
		{
			try
			{
                Image[] images = GetImages(albumID, albumKey, extras);
				ArrayList imagesIDs = new ArrayList();

				foreach (Image image in images)
				{
					imagesIDs.Add(image.id);
				}

				return (int[])imagesIDs.ToArray(typeof (int));
			}
			catch (Exception ex)
			{
				logger.Error("Could not get images", ex);
				throw new SmugMugException("Could not get images", ex);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="albumID"></param>
		/// <param name="heavy"></param>
		/// <returns></returns>
		public Image[] GetImages(long albumID, string albumKey, string extras)
		{
			try
			{
				logger.InfoFormat("Getting imageIDs with album id: {0}, album key: {1}, heavy: {2}", albumID, albumKey, extras);

                string result;

                if (String.IsNullOrEmpty(extras))
                {
                    result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.images.get", apiKey, appSecret, this.accessToken, true, "AlbumID", albumID.ToString(), "AlbumKey", albumKey.ToString()); 
                }
                else
                {
                    result = SmugMugRequest.ExecuteSmugMugHttpRequest("smugmug.images.get", apiKey, appSecret, this.accessToken, true, "AlbumID", albumID.ToString(), "AlbumKey", albumKey.ToString(), "Extras", extras); 
                }

                var response = JsonConvert.DeserializeObject<ImageResponse>(result);

                if (response.stat == "ok")
                {
                    logger.InfoFormat("Retreived {0}, images", response.Album.Images.Length);
                }
                else
                {
                    logger.Error("Could not retrieve Images");
                    logger.Error(response.code);
                    logger.Error(response.message);
                    logger.Error(response.method);

                    return new Image[0];
                }

                return response.Album.Images;
			}
			catch (Exception ex)
			{
				logger.Error("Could not retrieve Images", ex);
				throw new SmugMugException("Could not get images", ex);
			}
		}

        #region Upload

        public long Upload(FileInfo fileInfo, string keyword, long albumId, string albumKey)
        {
            UploadContext uploadContext = null;
            long imageID;

            if (fileInfo.Exists == false)
                throw new ArgumentException("Image does not exist: " + fileInfo.FullName);

            using (FileStream fileStream = File.OpenRead(fileInfo.FullName))
            {
                uploadContext = PrepareUpload(fileStream, fileInfo.Name, keyword, albumId, albumKey);

                do
                {
                    // calculate the progress out of a base "100"                    
                    double progressPercentage = ((double)uploadContext.CurrentPosition / (double)uploadContext.Request.ContentLength);
                    int percentage = (int)(progressPercentage * 100);
                    logger.Info("Uploading progress: " + percentage);
                } while (UploadChunk(uploadContext));

                // Get response after upload.
                imageID = FinishUpload(uploadContext, albumId, albumKey, fileInfo.Name);
            }

            return imageID;
        }
        /// <summary>
		/// Uploads the Image to SmugMug using HTTP Post.
		/// </summary>
		/// <param name="stream">The stream to the image on disk.</param>
		/// <param name="albumID">The id for the Image to be added.</param>
		/// <returns>Throws an <see cref="SmugMugUploadException"/>
		/// if an error occurs trying to upload the Image.</remarks>
        public UploadContext PrepareUpload(Stream photoStream, string fileName, string keyword, long albumID, string albumKey)
        {
            return PrepareUpload(photoStream, fileName, keyword, albumID, albumKey, 0);
        }

		/// <summary>
		/// Uploads the Image to SmugMug using HTTP Post replacing the existing image
		/// </summary>
		/// <param name="stream">The stream to the image on disk.</param>
		/// <param name="albumID">The id for the Image to be added.</param>
		/// <returns>Throws an <see cref="SmugMugUploadException"/>
		/// if an error occurs trying to upload the Image.</remarks>
		public UploadContext PrepareUpload(Stream photoStream, string fileName, string keywords, long albumID, string albumKey, long imageID)
		{
            UploadContext cookie = new UploadContext(fileName);

            logger.InfoFormat("Preparing file for upload: {0}, album id: {1}, album key: {2}",
                    fileName, albumID, albumKey);



            //int timeOut = ((int)photoStream.Length / 1024) * 1000;
            //cookie.Request.Timeout = timeOut;
            cookie.Request.Timeout = Timeout.Infinite;
            cookie.Request.ReadWriteTimeout = Timeout.Infinite;
            
            cookie.Request.ConnectionGroupName = Guid.NewGuid().ToString();

            //cookie.Request.Headers.Add("X-Smug-SessionID", this.account.Session.id);
            cookie.Request.Headers.Add("X-Smug-Version", SmugMugApi.VERSION);
            cookie.Request.Headers.Add("X-Smug-ResponseType", "JSON");
            cookie.Request.Headers.Add("X-Smug-AlbumID", albumID.ToString());
            cookie.Request.Headers.Add("X-Smug-AlbumKey", albumKey);
            
            if (imageID > 0) // we are replacing the image if the ID is passed in
                cookie.Request.Headers.Add("X-Smug-imageID", imageID.ToString());
            
            // removed this as non ascii characters we not sent
            //cookie.Request.Headers.Add("X-Smug-FileName", fileName);

            //if (String.IsNullOrEmpty(caption) == false)
            //{
            //    cookie.Request.Headers.Add("X-Smug-Caption", HttpUtility.UrlEncode(caption));
            //}

            if (String.IsNullOrEmpty(keywords) == false)
            {
                cookie.Request.Headers.Add("X-Smug-Keywords", keywords);
            }

            // Add the authorization header
            string uploadURL = SmugMugApi.UploadUrl + fileName;
            cookie.Request.Headers.Add("Authorization", OAuthUtility.GetAuthorizationHeader(this.apiKey, this.appSecret, this.accessToken, uploadURL));

            // disable HTTP/1.1 Continue
            // http://haacked.com/archive/2004/05/15/http-web-request-expect-100-continue.aspx
            ServicePointManager.Expect100Continue = false;
				
			string md5sum;
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] hash = md5.ComputeHash(photoStream);

			StringBuilder buff = new StringBuilder();
			foreach (byte hashByte in hash)
			{
				buff.Append(String.Format("{0:x2}", hashByte));
			}
			
			md5sum = buff.ToString();
            cookie.Request.Headers.Add("Content-MD5", md5sum);
            cookie.PhotoStream = photoStream;

            cookie.Request.ContentLength = cookie.PhotoStream.Length;

            // This option prevents uploads from being buffered into memory, avoiding OutOfMemory 
            // exceptions on large uploads.
            cookie.Request.AllowWriteStreamBuffering = false;

            cookie.PhotoStream.Position = 0;

            cookie.RequestStream = cookie.Request.GetRequestStream();
            cookie.ChunkSize = Math.Max((int)(cookie.PhotoStream.Length / 100), 65536);

            logger.InfoFormat("Image upload start time: {0}", DateTime.Now.ToString());

            return cookie;
        }

        /// <summary>
        /// Uploads a chunk of the image returning allowing for progress and cancellation
        /// </summary>
        /// <param name="uploadContext"></param>
        /// <returns></returns>
        public bool UploadChunk(UploadContext uploadContext)
        {
            // Upload a chunk of the photo/video, making sure that only a small piece
            // of a (possibly huge) file is read from disk into memory at a time.
            int chunk = Math.Min(uploadContext.ChunkSize, (int)(uploadContext.PhotoStream.Length - uploadContext.CurrentPosition));

            // Safely read from the data stream.
            byte[] buffer = new byte[chunk]; // Buffer to store a chunk of data.
            int offset = 0; // Offset into buffer.
            int toRead = chunk; // The amount to read from the data stream.
            while (toRead > 0)
            {
                int read = uploadContext.PhotoStream.Read(buffer, offset, toRead);
                if (read <= 0)
                {
                    throw new EndOfStreamException(
                        String.Format("End of file reached with {0} bytes left to read", toRead));
                }
                toRead -= read;
                offset += read;
            }

            // Write however much we read from the data stream.
            uploadContext.RequestStream.Write(buffer, 0, chunk);
            uploadContext.CurrentPosition += chunk;

            return uploadContext.CurrentPosition < uploadContext.PhotoStream.Length;
        }
         /// <summary>
        /// The third and final step in the photo upload process. Gets a response
        /// that includes the photoId from SmugMug after the upload has completed.
        /// </summary>
        /// <param name="uploadContext">The desired UploadContext.</param>
        /// <returns>The SmugMug photoId for the uploaded photo.</returns>
        public long FinishUpload(UploadContext uploadContext, long albumID, string albumKey, string fileName)
        {
            logger.Info("Verifying image upload");

            string result;
            DateTime now = DateTime.Now;

            try
            {
                uploadContext.RequestStream.Close();

                // set the timeout to 2 minutes
                // cookie.Request.Timeout = 120000;

                using (WebResponse response = uploadContext.Request.GetResponse())
				{
#if DEBUG
                    logger.Info("Elapsed time for GetResponse: " + TimeSpan.FromTicks(DateTime.Now.Ticks - now.Ticks).Seconds);
#endif
					using (StreamReader reader = new StreamReader(response.GetResponseStream()))
					{
						result = reader.ReadToEnd();
					}

					response.Close();
				}

                long imageID;

                var resp = JsonConvert.DeserializeObject<ImageResponse>(result);

                if (resp.stat == "ok")
                {
                    imageID = resp.Image.id;

                    if (imageID == 0)
                    {
                        logger.Error("Image upload failed, imageID = 0");
                        throw new SmugMugUploadException("Error uploading image, imageID = 0", null);
                    }

                    logger.InfoFormat("Upload successful, image id:{0} , image key:{1}", imageID, resp.Image.Key);
                    logger.InfoFormat("Image upload end time: {0}", DateTime.Now.ToString());

                }
                else
                {
                    throw new SmugMugException(resp.code, resp.message, resp.method);
                }

				return imageID;
			}
			catch (Exception ex)
			{
				if (ex is WebException)
				{
					WebException we = ex as WebException;
                    logger.InfoFormat("Image upload error time: {0}", DateTime.Now.ToString());
					logger.Error("Image uploading failed", we);
					logger.ErrorFormat("WebException status: {0}", we.Status.ToString());
					logger.ErrorFormat("WebException stack trace: {0}", we.StackTrace);

                    //// ignore receive failure errors as they seem to mean nothing and the file was uploaded
                    //if (we.Status != WebExceptionStatus.ReceiveFailure)
                    //{
                    //    // in this case we want to get the real image ID
                    //    try
                    //    {
                    //        long imageID = this.GetimageID(albumID, albumKey, fileName, now);
                    //        if (imageID != 0)
                    //        {
                    //            logger.InfoFormat("Verified image was uploaded with Image ID: " + imageID);
                    //            return imageID;
                    //        }
                    //        else
                    //        {
                    //            logger.InfoFormat("Could not verify image upload");
                    //        }
                    //    }
                    //    catch
                    //    {
                
                    //    }
                    //}

                    throw new SmugMugUploadException("Web Exception", we);
						
				}
				else
				{
                    logger.InfoFormat("Image upload Error Time: {0}", DateTime.Now.ToString());
                    logger.Error("Image uploading failed", ex);

                    throw new SmugMugUploadException("Image uploading failed", ex);
				}
			}
			finally
			{
				uploadContext.PhotoStream.Close();
			}
        }
        #endregion
    }
}
