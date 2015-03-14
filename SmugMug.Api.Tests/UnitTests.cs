using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmugMug.Toolkit;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SmugMug.Api.Tests
{
    [TestClass]
    public class UnitTests
    {
        private const string ApiKey = "vk5mfHN3rZuz23x2uSvtEnOnXwG9IwYG";
        private const string AppSecret = "ac2937add2b2f8ee8faad44ce710e516";
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static long TestAlbumID;
        private static string TestAlbumKey;
        private static int TestShareGroupID = 9959; // Family
        private const int TestCategoryID = 120136385; // Other
        private const int TestSubCategoryID = 57349945; // San Francisco

        public static SmugMugApi SmugMugApi;
        public static Token AccessToken;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        public UnitTests()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        [ClassInitialize()]
        public static void UnitTestsInitialize(TestContext testContext) 
        {
            logger.Info("TestFixtureSetUp");

            SmugMugApi = new SmugMugApi(RegistrySettings.TokenSecret, ApiKey, AppSecret, Application.ProductVersion);
            AccessToken = new Token() { id = RegistrySettings.TokenID, Secret = RegistrySettings.TokenSecret };

            if (RegistrySettings.TokenSecret.Length <= 0)
            {
                AccessToken = SmugMugAuthorize.AuthorizeSmugMugConsole(SmugMugApi);
            }

            SmugMugApi.Login(AccessToken);

            Assert.IsTrue(SmugMugApi.Connected);

            CleanupTestAlbums();

            // create the test album
            Album album = SmugMugApi.CreateAlbum(Album.Create("TestAlbum Foo,é ", TestCategoryID, TestSubCategoryID));
            TestAlbumID = album.id;
            TestAlbumKey = album.Key;
        }

        [TestMethod]
        public void Login()
        {
            Assert.IsNotNull(SmugMugApi);
            Assert.IsTrue(SmugMugApi.Connected);
        }

        [TestMethod]
        public void IsSmugVaultEnabled()
        {
            Assert.IsFalse(SmugMugApi.Account.User.SmugVault);
        }

        [TestMethod]
        public void GetShareGroups()
        {
            ShareGroup[] sharegroups = SmugMugApi.GetShareGroups();
            Assert.IsNotNull(sharegroups);
        }

        [TestMethod]
        public void AddAlbumToShareGroup()
        {
            bool sucess = SmugMugApi.ShareGroupsAddAlbum(TestShareGroupID, TestAlbumID);
            ShareGroup[] shareGroups = SmugMugApi.GetShareGroups();
            Assert.IsTrue(sucess);
        }

        [TestMethod]
        public void GetCategories()
        {
            Category[] categories = SmugMugApi.GetCategories();
            Assert.IsNotNull(categories);
        }

        [TestMethod]
        public void GetSubCategories()
        {
            SubCategory[] categories = SmugMugApi.GetSubCategories(47);
            Assert.IsNotNull(categories);
            Assert.AreNotEqual(categories[0].Name, String.Empty);
        }

        [TestMethod]
        public void GetAlbums()
        {
            Album[] albums = SmugMugApi.GetAlbums();
            Assert.IsNotNull(albums);
        }

        [TestMethod]
        public void CreateAlbumStatic()
        {
            Album album = Album.Create("TestAlbum " + DateTime.Now.Ticks);
            album.Password = "omar";
            album.Public = false;
            album = SmugMugApi.CreateAlbum(album);
            Assert.IsTrue(album.id > 0);
            Assert.IsTrue(album.Key != null);

            bool result = SmugMugApi.DeleteAlbum(album.id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateAlbum()
        {
            Album album = Album.Create("TestAlbum " + DateTime.Now.Ticks, TestCategoryID, TestSubCategoryID);

            album = SmugMugApi.CreateAlbum(album);
            Assert.IsTrue(album.id > 0);
            Assert.IsTrue(album.Key != null);

            album = SmugMugApi.GetAlbumInfo(album.id, album.Key);

            Assert.IsTrue(album.Category.id == TestCategoryID);
            Assert.IsTrue(album.SubCategory.id == TestSubCategoryID);

            bool result = SmugMugApi.DeleteAlbum(album.id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateAlbumViewerCoice()
        {
            Album album = Album.Create("TestAlbum " + DateTime.Now.Ticks, 0, 0, Convert.ToInt32(TemplateType.Journal) );

            album = SmugMugApi.CreateAlbum(album);
            Assert.IsTrue(album.id > 0);
            Assert.IsTrue(album.Key != null);

            album = SmugMugApi.GetAlbumInfo(album.id, album.Key);

            Assert.IsTrue(album.Template.id == Convert.ToInt32(TemplateType.Journal));

            bool result = SmugMugApi.DeleteAlbum(album.id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateAlbumPhotoRankDisabled()
        {
            Album album = new Album();
            album.Title = "TestAlbum " + DateTime.Now.Ticks;
            album.CanRank = false;

            album = SmugMugApi.CreateAlbum(album);
            Assert.IsTrue(album.id > 0);
            Assert.IsTrue(album.Key != null);

            Assert.IsFalse(album.CanRank);

            bool result = SmugMugApi.DeleteAlbum(album.id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateAlbumRussian()
        {
            Album album = Album.Create("TestAlbum Павел Ефимов + Аня + Катя и Егор Левины 20.10.2007", TestCategoryID, TestSubCategoryID);

            album = SmugMugApi.CreateAlbum(album);

            album = SmugMugApi.GetAlbumInfo(album.id, album.Key);

            Assert.IsTrue(album.id > 0);
            Assert.IsTrue(album.Key != null);

            Assert.IsTrue(album.Title == "TestAlbum Павел Ефимов + Аня + Катя и Егор Левины 20.10.2007");
            Assert.IsTrue(album.Category.id == 47);
            Assert.IsTrue(album.SubCategory.id == 263302);

            bool result = SmugMugApi.DeleteAlbum(album.id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CreateAlbumOptionsTrue()
        {
            Album album = Album.Create("TestAlbum " + DateTime.Now.Ticks, TestCategoryID, TestSubCategoryID);

            album.Originals = true;
            album.Public = false;
            album.SortMethod = SortMethodType.Position.ToString();

            album.Header = true;
            album.Larges = true;
            album.Originals = true;
            album.Geography = true;
            album.WorldSearchable = true;
            album.SmugSearchable = true;
            album.HideOwner = true;
            album.FriendEdit = true;
            album.FamilyEdit = true;
            album.Password = "foo";
            album.PasswordHint = "bar";

            album = SmugMugApi.CreateAlbum(album);
            Assert.IsTrue(album.id > 0);
            Assert.IsTrue(album.Key != null);

            album = SmugMugApi.GetAlbumInfo(album.id, album.Key);

            Assert.IsTrue(album.Category.id == TestCategoryID);
            Assert.IsTrue(album.SubCategory.id == TestSubCategoryID);
            Assert.IsTrue(album.Header);
            Assert.IsTrue(album.Larges);
            Assert.IsTrue(album.Originals);
            Assert.IsTrue(album.Geography);
            Assert.IsTrue(album.WorldSearchable);
            Assert.IsTrue(album.SmugSearchable);
            Assert.IsTrue(album.HideOwner);
            Assert.IsTrue(album.FriendEdit);
            Assert.IsTrue(album.FamilyEdit);
            Assert.AreEqual(album.Password, "foo");
            Assert.AreEqual(album.PasswordHint, "bar");

            bool result = SmugMugApi.DeleteAlbum(album.id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DeleteAlbum()
        {
            Album album = SmugMugApi.CreateAlbum(Album.Create("TestAlbum " + DateTime.Now.Ticks));

            album.Originals = true;
            album.Public = false;
            album.SortMethod = SortMethodType.Position.ToString();

            album = SmugMugApi.CreateAlbum(album);
            Assert.IsTrue(album.id > 0);

            bool result = SmugMugApi.DeleteAlbum(album.id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetImages()
        {
            int[] image = SmugMugApi.GetImages(TestAlbumID, TestAlbumKey);
            Assert.IsNotNull(image);
        }

        [TestMethod]
        public void GetAlbumInfo()
        {
            Album album = SmugMugApi.GetAlbumInfo(TestAlbumID, TestAlbumKey);
            Assert.IsTrue(album.id == TestAlbumID);
        }

        [TestMethod]
        public void GetImagesWithFileNames()
        {
            FileInfo photo = GetImageToUpload(2);
            long imageID = SmugMugApi.Upload(photo, null, TestAlbumID, TestAlbumKey);
            long albumID = TestAlbumID;

            Image[] images = SmugMugApi.GetImages(TestAlbumID, TestAlbumKey, "FileName");
            Assert.IsTrue(images.Length > 0);
            Assert.IsNotNull(images[0].FileName);
            Assert.AreNotEqual(images[0].FileName, String.Empty);
        }

        [TestMethod]
        public void SetAlbumHighlight()
        {
            FileInfo photo = GetImageToUpload(0);
            long imageID = SmugMugApi.Upload(photo, null, TestAlbumID, TestAlbumKey);
            long albumID = TestAlbumID;

            bool success = SmugMugApi.SetAlbumHighlight(albumID, imageID);
            Assert.IsTrue(success);
        }

        [TestMethod]
        public void Upload()
        {
            FileInfo photo = GetImageToUpload(0);
            long imageID = SmugMugApi.Upload(photo, null, TestAlbumID, TestAlbumKey);
            Assert.IsTrue(imageID > 0);
            SmugMugApi.DeleteImage(imageID);
        }

        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void UnitTestsCleanup() 
        {
            CleanupTestAlbums();
        }

        private static FileInfo GetImageToUpload(int index)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo directory = new DirectoryInfo(Path.Combine(path, "TestImages"));
            FileInfo photo = directory.GetFiles()[index];
            return photo;
        }

        private static void CleanupTestAlbums()
        {
            // clean up any old test albums
            List<Album> albums = new List<Album>();
            albums.AddRange(SmugMugApi.GetAlbums());

            var albumsToDelete = albums.Where(a => a.Title.StartsWith("TestAlbum"));

            foreach (var item in albumsToDelete)
            {
                SmugMugApi.DeleteAlbum(item.id);
                logger.Info("Deleting test album: " + item.Title);
            }
        }

        // Find a named appender already attached to a logger
        public static log4net.Appender.IAppender FindAppender(string appenderName)
        {
            return LogManager.GetRepository().GetAppenders().FirstOrDefault(appender => appender.Name == appenderName);
        }
    }
}
