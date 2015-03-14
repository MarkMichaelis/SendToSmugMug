using log4net;
using SmugMug.Api;
using SmugMug.SendToSmugMug.Properties;
using SmugMug.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmugMug.SendToSmugMug
{
    public partial class MainWindow : Form
    {
        private long bytesUploaded;
        private int filesUploaded;
        private int uploadCount;
        private string highlightImageFilename;

        private bool closing = false;
        private bool cancelled;
        private bool uploadError = false;
        private bool isBeforeUpload = false;
        private bool skippedFiles;
        private bool uploadingAfterCreate = false;
        private bool existingAlbum = false;
        private bool loading = false;

        private UploadDuplicates uploadDuplicates;
        private ListViewItem currentItem;
        private StringCollection keywords = new StringCollection();
        private BackgroundWorker uploadImagesWorker = new BackgroundWorker();
        private TimeSpan elapsed;
        
        private List<string> supportedExtensions = new List<string>();
        private List<FileInfo> files = new List<FileInfo>();
        private Dictionary<string, Album> AlbumsToCreate = new Dictionary<string, Album>();
        private Dictionary<string, long> imageIds = new Dictionary<string, long>();
        private Dictionary<string, Album> photoAlbumDestination = new Dictionary<string, Album>();
        //private SmugMug.Api.Image[] images;
        private SmugMugApi smugMug;
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private TaskScheduler taskSchedulerMain;

        public enum UploadDuplicates
        {
            Skip = 0,
            Allow = 1,
            Replace = 2
        }

        public MainWindow(string[] args)
        {
            InitializeComponent();
            this.loading = true;
            Utils.SetDoubleBuffered(this.listViewPhotos);
            this.textBoxTitle.Text = Properties.Resources.InsertTitlePlaceholder;

            if (Properties.Settings.Default.UpgradeSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeSettings = false;
            }

            this.supportedExtensions.AddRange(Properties.Settings.Default.PhotoExtensions.Cast<string>().ToList());
            this.supportedExtensions.AddRange(Properties.Settings.Default.VideoExtensions.Cast<string>().ToList());
            this.supportedExtensions.AddRange(Properties.Settings.Default.QuickTimeExtensions.Cast<string>().ToList());

            // check for new version
            if (RegistrySettings.DisableUpdates == false)
            {
                this.CheckForNewVersion();
            }

            // load all the selected files
            if (args.Length != 0)
            {
                // check to see if we are passed in a directory
                if (Directory.Exists(args[0]))
                {
                    string path = NativeMethods.ToLongPathName(args[0]);
                    logger.InfoFormat("loading a directory of images: {0}", path);

                    //this.rootDirectory = new DirectoryInfo(path);
                    this.files.AddRange(LoadImagesFromDirectory(new DirectoryInfo(path)));
                }
                else
                {
                    foreach (string s in args)
                    {
                        if (s != null && s.Length > 0)
                        {
                            FileInfo fileInfo = new FileInfo(s);

                            if (fileInfo.Exists)
                            {
                                files.Add(fileInfo);
                            }
                        }
                    }
                }

                if (this.files == null || this.files.Count == 0)
                {
                    logger.Info("No files were present, exiting application");
                    Environment.Exit(0);
                }
            }
            else // no command line params, promt for dialog
            {
                FolderBrowserDialog browserDialog = new FolderBrowserDialog();
                browserDialog.Description = "Select a folder with Pictures.";

                DialogResult result = browserDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string path = browserDialog.SelectedPath;
                    //this.rootDirectory = new DirectoryInfo(folderName);
                    this.files.AddRange(LoadImagesFromDirectory(new DirectoryInfo(path)));
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            taskSchedulerMain = TaskScheduler.FromCurrentSynchronizationContext();
            this.Bootstrap();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.Info("Application closing");

            this.closing = true;
            if (this.uploadImagesWorker.IsBusy)
            {
                this.uploadImagesWorker.CancelAsync();
                this.uploadImagesWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(uploadImagesWorker_RunWorkerCompleted);
            }

            Properties.Settings.Default.AlbumSorting = this.comboBoxSorting.SelectedIndex;
            Properties.Settings.Default.AlbumLargestSize = this.comboBoxLargestSize.SelectedIndex;
            Properties.Settings.Default.Duplicates = this.comboBoxDuplicates.SelectedIndex;

            try
            {
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                logger.Error("Error saving config", ex);
            }
        }

        private void Bootstrap()
        {
            logger.InfoFormat("Trying to retrieve cached credentials");
            logger.InfoFormat("Cached credentials exist: {0}", Convert.ToBoolean(RegistrySettings.TokenSecret.Length));

            smugMug = new SmugMugApi(RegistrySettings.TokenSecret, App.ApiKey, App.AppSecret, Application.ProductVersion);

            if (RegistrySettings.TokenSecret.Length <= 0)
            {
                logger.InfoFormat("No cached credentials, performing OAuth login");

                Token reqTok = null;

                // need to handle the case where the sytem clock is different from SmugMug and auth will fail
                try
                {
                    reqTok = this.smugMug.GetRequestToken();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Exit();
                    return;
                }
                
                string url = smugMug.GetAuthorizationURL(reqTok, AccessEnum.Full, PermissionsEnum.Modify);

                MessageBox.Show("You will now be directed to your web browser to sign in. When you are finished, come back to Send to SmugMug and answer the next question.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(url);
                MessageBox.Show("Send to SmugMug will now check to see if you authorized it for upload.", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                try
                {
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Application.Exit();
                }
                
                logger.Info("Getting access token");
                
                try
                {
                    Token t = this.smugMug.GetAccessToken(reqTok);
 
                    if (t != null)
                    {
                        logger.InfoFormat("Saving TokenID and TokenSecret");
                        RegistrySettings.TokenID = t.id;
                        RegistrySettings.TokenSecret = t.Secret;
                    }
                    else
                    {
                        logger.InfoFormat("OAuth login failed");
                        //Application.Exit();
                        //return;
                    }

                    this.Bootstrap();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                this.SetStatusText("Logging in...");
                // start the login process
                logger.Info("Starting login process");

                Token accessToken = new Token() { id = RegistrySettings.TokenID, Secret = RegistrySettings.TokenSecret };

                logger.Info("created accessToken");

                Task taskLogin = Task.Factory.StartNew(() => this.smugMug.Login(accessToken), TaskCreationOptions.AttachedToParent);
                taskLogin.ContinueWith(ProcessFailedOAuthLogin, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, taskSchedulerMain);

                //try
                //{
                //    logger.Info("Getting access token");
                //    this.smugMug.Login(accessToken);
                //    this.SetStatusText("Loading Albums and Categories...");
                //    ArrayList list = this.smugMug.GetAlbumsAndCategories();
                //    this.TaskGetAlbumsAndCategories((Album[])list[0], (Category[])list[1]);
                //    this.TaskLoadImages();
                //}
                //catch (Exception ex)
                //{
                //    throw ex;
                //}

                taskLogin.ContinueWith(x => this.SetStatusText("Loading Albums and Categories..."), CancellationToken.None, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled, taskSchedulerMain);
                // this tasks only start when taskLogin has finished
                Task<ArrayList> taskGetAlbumsAndCategories = taskLogin.ContinueWith(y => this.smugMug.GetAlbumsAndCategories(), TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);
                taskGetAlbumsAndCategories.ContinueWith(this.TaskLoadImages, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, taskSchedulerMain);
                taskGetAlbumsAndCategories.ContinueWith(z => this.TaskGetAlbumsAndCategories((Album[])taskGetAlbumsAndCategories.Result[0], (Category[])taskGetAlbumsAndCategories.Result[1]), CancellationToken.None, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled, taskSchedulerMain);
            }
        }

        private static void SendErrorLog()
        {
            string path = App.GetLogFile();

            using (StreamReader sr = File.OpenText(path))
            {
                string errorLog = sr.ReadToEnd();
                App.SendMailWithMailTo(App.SmugMugEmail, "Send to SmugMug Error", errorLog, path);
            }
        }

        //private void TaskLoadImages()
        private void TaskLoadImages(Task result)
        {
            logger.Info("loading images into listview");

            // clear the loaded images if they exist
            if (this.listViewPhotos.Items.Count > 0)
                this.listViewPhotos.Items.Clear();

            if (this.smugMug.Account.User.SmugVault)
            {
                logger.Info("SmugVault is enabled");
                supportedExtensions.Clear();
                supportedExtensions.Add(".*");
            }

            logger.InfoFormat("Loading {0} images", this.files.Count);

            List<FileInfo> filesToUpload = new List<FileInfo>();
            // reload all the supported images
            foreach (var item in this.files)
            {
                foreach (string searchPattern in this.supportedExtensions)
                {
                    if (item.Extension.ToLower() == searchPattern)
                    {
                        filesToUpload.Add(item);
                        break;
                    }
                    else if (searchPattern == ".*")
                    {
                        filesToUpload.Add(item);
                        break;
                    }
                }
            }

            if (filesToUpload.Count > 0)
            {
                logger.InfoFormat("Loading pictures from {0}", filesToUpload[0].Directory.Name);
                this.SetStatusText(String.Format("Loading pictures from {0}...", filesToUpload[0].Directory.Name));
                this.textBoxTitle.Text = filesToUpload[0].Directory.Name;
                this.progressBar.Maximum = filesToUpload.Count;

                BackgroundWorker loadImagesWorker = new BackgroundWorker();
                loadImagesWorker.WorkerReportsProgress = true;
                loadImagesWorker.WorkerSupportsCancellation = true;
                loadImagesWorker.DoWork += new DoWorkEventHandler(loadImagesWorker_DoWork);
                loadImagesWorker.ProgressChanged += new ProgressChangedEventHandler(loadImagesWorker_ProgressChanged);
                loadImagesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadImagesWorker_RunWorkerCompleted);
                loadImagesWorker.RunWorkerAsync(filesToUpload);
            }
            else
            {
                ResetStatusBarToUpload();
            }
        }

        private void TaskGetAlbumsAndCategories(Album[] albums, Category[] categories) //Task<Album[]> taskGetAlbums, Task<Category[]> taskGetCateories)
        {
            // update Album UX
            UpdateComboBoxAlbums(albums, true);

            // update Category UX
            UpdateComboBoxCategories(categories);

            // turn on the new album settings since this is the default.
            string[] sorting = SortMethodType.GetNames(typeof(SortMethodType));
            this.comboBoxSorting.Items.AddRange(sorting);
            this.comboBoxSorting.SelectedIndex = 5;

            string[] templates = TemplateType.GetNames(typeof(TemplateType));
            this.comboBoxStyle.Items.AddRange(templates);
            this.comboBoxStyle.SelectedIndex = 0;

            this.comboBoxWatermark.SelectedIndex = 0;

            this.SetAccountTypeSettings();
            // load the state for all the controls
            this.LoadPrefs();

            UpdateComboBoxSubCategories();
            
        }

        private void TaskGetAlbumsAndCategories(Task[] tasks) //Task<Album[]> taskGetAlbums, Task<Category[]> taskGetCateories)
        {
            Task<Album[]> taskGetAlbums = (Task<Album[]>)tasks[0];
            Task<Category[]> taskGetCateories = (Task<Category[]>)tasks[1];

            if (taskGetAlbums.IsFaulted | taskGetCateories.IsFaulted)
            {
                logger.Error("Exception on Bootstrap");

                DialogResult result = MessageBox.Show("There was an error connecting to SmugMug. Please try again in about 20 seconds.",
                    "Connection Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                Application.Exit();
            }
            else if (taskGetAlbums.IsCanceled == false & taskGetCateories.IsCanceled == false)
            {
                // update Album UX
                UpdateComboBoxAlbums(taskGetAlbums.Result, true);

                // update Category UX
                UpdateComboBoxCategories(taskGetCateories.Result);

                // turn on the new album settings since this is the default.
                string[] sorting = SortMethodType.GetNames(typeof(SortMethodType));
                this.comboBoxSorting.Items.AddRange(sorting);
                this.comboBoxSorting.SelectedIndex = 5;

                string[] templates = TemplateType.GetNames(typeof(TemplateType));
                this.comboBoxStyle.Items.AddRange(templates);
                this.comboBoxStyle.SelectedIndex = 0;

                this.comboBoxWatermark.SelectedIndex = 0;

                this.SetAccountTypeSettings();
                // load the state for all the controls
                this.LoadPrefs();

                UpdateComboBoxSubCategories();
            }
        }

        private void ProcessFailedOAuthLogin(Task result)
        {
            if (result.Exception != null)
            {
                Exception ex = result.Exception.InnerException;

                if (ex is SmugMugLoginException)
                {
                    DialogResult dialog = MessageBox.Show("There was an error logging in to SmugMug and you need to re-authorize this application.",
                        "Login error",
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Exclamation);

                    if (dialog == DialogResult.Retry)
                    {
                        RegistrySettings.TokenSecret = String.Empty;
                        this.Bootstrap();
                    }
                    else
                    {
                        // TODO: need to call on the UX Thread
                        this.Close();
                    }
                }
                else if (ex is SmugMugOfflineException)
                {
                    DialogResult dialog = MessageBox.Show(
                        this,
                        "SmugMug is currently offline. Would you like to go to view their status page?",
                        "SmugMug Offline",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);

                    if (dialog == DialogResult.Yes)
                    {
                        string url = "http://api.smugmug.com/down/outage.mg";
                        Utilities.OpenUrl(url);
                    }
                    else
                    {
                        this.Close();
                    }

                }
                else
                {
                    DialogResult dialog = MessageBox.Show("There was an error: " + ex.Message,
                                                          "Login error",
                                                          MessageBoxButtons.RetryCancel,
                                                          MessageBoxIcon.Exclamation);

                    if (dialog == DialogResult.Retry)
                    {
                        this.Bootstrap();
                    }
                }
            }
        }

        internal List<FileInfo> LoadImagesFromDirectory(DirectoryInfo directory)
        {
            List<FileInfo> files = new List<FileInfo>();
            bool hasFiles = false;

            foreach (FileInfo file in directory.GetFiles("*"))
            {
                if ((file.Attributes | FileAttributes.Hidden) != file.Attributes)
                {
                    logger.InfoFormat("loading a file: {0}", file.Name);
                    files.Add(file);
                    hasFiles = true;
                }
            }

            if (hasFiles)
            {
                listViewPhotos.Groups.Add(directory.FullName, directory.Name);
                AlbumsToCreate.Add(directory.FullName, new Album() { Title = directory.Name });
            }

            if (Properties.Settings.Default.LoadImagesFromSubfolders)
            {
                foreach (DirectoryInfo dir in directory.GetDirectories())
                {
                    logger.InfoFormat("loading a sub directory: {0}", dir.FullName);
                    if ((dir.Attributes | FileAttributes.Hidden) != dir.Attributes)
                    {
                        files.AddRange(LoadImagesFromDirectory(dir));
                    }
                }
            }

            return files;
        }

        #region SmugMug CRUD Calls
        public void UpdateComboBoxAlbums(Album[] albums, bool matchToAlbum)
        {
            if (albums != null && albums.Length > 0)
            {
                this.comboBoxAdd.BeginUpdate();
                this.comboBoxAdd.DataSource = albums;
                this.comboBoxAdd.DisplayMember = "Title";
                this.comboBoxAdd.EndUpdate();
            }

            if (matchToAlbum)
            {
                this.radioButtonAddToExistingAlbum.Enabled = true;

                // if we have a match to an existing album name, use that
                foreach (Album item in this.comboBoxAdd.Items)
                {
                    if (item.Title == this.textBoxTitle.Text)
                    {
                        this.comboBoxAdd.SelectedItem = item;
                        this.radioButtonAddToExistingAlbum.Checked = true;
                        this.comboBoxAdd.Enabled = true;
                        this.existingAlbum = true;
                        break;
                    }
                }
            }
        }

        public void UpdateComboBoxCategories(Category[] categories)
        {
            this.comboBoxCategory.BeginUpdate();
            this.comboBoxCategory.Enabled = true;
            this.comboBoxCategory.DataSource = categories;
            this.comboBoxCategory.DisplayMember = "Name";
            this.comboBoxCategory.EndUpdate();

            // populate the category options menu
            foreach (Category item in categories)
            {
                MenuItem menu = new MenuItem { Text = item.Name, Tag = item.id };
                menu.Click += new EventHandler(menuItemDefaultCategory_Click);

                if (Properties.Settings.Default.DefaultCategoryID == item.id)
                {
                    menu.Checked = true;
                }

                this.menuItemDefaultCategory.MenuItems.Add(menu);
            }
        }

        private void UpdateComboBoxSubCategories()
        {
            if (this.comboBoxCategory.SelectedItem != null)
            {
                long categoryID = ((Category)this.comboBoxCategory.SelectedItem).id;
                SubCategory[] categories = this.smugMug.GetSubCategories(categoryID);
                this.comboBoxSubCategory.DataSource = categories;
                this.comboBoxSubCategory.DisplayMember = "Name";
                this.comboBoxSubCategory.SelectedIndex = -1;
            }
        }

        public void AddAlbumToShareGroup(long albumID, int shareGroupID)
        {
            this.SetStatusText("Adding album to ShareGroup...");

            TaskScheduler uiTaskScheduler = taskSchedulerMain;
            Task.Factory.StartNew(() => this.smugMug.ShareGroupsAddAlbum(shareGroupID, albumID));
        }

        public void GetShareGroups()
        {
            this.SetStatusText("Retrieving ShareGroups...");

            TaskScheduler uiTaskScheduler = taskSchedulerMain;
            Task<ShareGroup[]> task = Task.Factory.StartNew(() => this.smugMug.GetShareGroups());
            task.ContinueWith(t =>
                {
                    if (task.IsFaulted == false)
                    {
                        ShareGroup[] shareGroups = task.Result;

                        if (shareGroups.Length >= 0)
                        {
                            this.comboBoxShareGroup.BeginUpdate();
                            this.comboBoxShareGroup.Enabled = true;
                            ArrayList dataSource = new ArrayList();
                            dataSource.AddRange(shareGroups);
                            ShareGroup nullGroup = new ShareGroup();
                            nullGroup.id = 0;
                            nullGroup.Name = "";
                            dataSource.Insert(0, nullGroup);
                            this.comboBoxShareGroup.DataSource = dataSource;
                            this.comboBoxShareGroup.DisplayMember = "Name";
                            this.comboBoxShareGroup.EndUpdate();
                            this.ResetStatusBarToUpload();
                        }
                    }

                }, CancellationToken.None, TaskContinuationOptions.None, uiTaskScheduler);
        }

        public void GetWatermarks()
        {
            this.SetStatusText("Retrieving Watermarks...");

            TaskScheduler uiTaskScheduler = taskSchedulerMain;
            Task<Watermark[]> task = Task.Factory.StartNew(() => this.smugMug.GetWatermarks());
            task.ContinueWith(t =>
            {
                if (task.IsFaulted == false)
                {
                    Watermark[] watermarks = task.Result;

                    if (watermarks.Length > 0)
                    {
                        this.comboBoxWatermark.BeginUpdate();
                        this.comboBoxWatermark.Enabled = true;
                        ArrayList dataSource = new ArrayList();
                        dataSource.AddRange(watermarks);
                        Watermark nullGroup = new Watermark();
                        nullGroup.id = 0;
                        nullGroup.Name = "SmugMug";
                        dataSource.Insert(0, nullGroup);
                        this.comboBoxWatermark.DataSource = dataSource;
                        this.comboBoxWatermark.DisplayMember = "Title";
                        this.comboBoxWatermark.ValueMember = "WatermarkID";
                        this.comboBoxWatermark.SelectedIndex = 0;
                        this.comboBoxWatermark.EndUpdate();
                    }

                    this.ResetStatusBarToUpload();
                }

            }, CancellationToken.None, TaskContinuationOptions.None, uiTaskScheduler);
        }

        private Album GetNewAlbum(string title)
        {
            this.SetStatusText("Creating new Album...");

            long categoryID = 0, subCategoryID = 0;
            int templateID = 0;

            if (this.comboBoxCategory.Items.Count > 0)
            {
                categoryID = ((Category)this.comboBoxCategory.SelectedItem).id;
            }
            if (this.comboBoxSubCategory.Items.Count > 0)
            {
                if (this.comboBoxSubCategory.SelectedIndex >= 0 && this.comboBoxSubCategory.SelectedValue != null)
                    subCategoryID = ((SubCategory)this.comboBoxSubCategory.SelectedItem).id;
            }

            if (this.comboBoxStyle.Enabled) // pro
            {
                TemplateType templateType =
                    (TemplateType)Enum.Parse(typeof(TemplateType), this.comboBoxStyle.SelectedItem.ToString());
                templateID = Convert.ToInt32(templateType);
            }

            Album newAlbum = Album.Create(title, categoryID, subCategoryID, templateID);

            if (this.textBoxDescription.Text.Length > 0) newAlbum.Description = this.textBoxDescription.Text;
            if (this.textBoxKeywords.Text.Length > 0) newAlbum.Keywords = this.textBoxKeywords.Text;
            if (this.textBoxPassword.Text.Length > 0) newAlbum.Password = this.textBoxPassword.Text;
            if (this.textBoxPasswordHint.Text.Length > 0) newAlbum.PasswordHint = this.textBoxPasswordHint.Text;
            newAlbum.SortMethod = this.comboBoxSorting.SelectedItem.ToString();
            newAlbum.SortDirection = this.checkBoxSortDirection.Checked;
            newAlbum.Position = Convert.ToInt32(this.numericUpDownPosition.Value);

            newAlbum.Public = this.radioButtonPublic.Checked;
            newAlbum.Filenames = this.checkBoxShowFilenames.Checked;
            newAlbum.Comments = this.checkBoxAllowComments.Checked;
            newAlbum.External = this.checkBoxAllowExternalLinks.Checked;
            newAlbum.EXIF = this.checkBoxShowCameraInfo.Checked;
            newAlbum.Share = this.checkBoxAllowEasySharing.Checked;
            newAlbum.Printable = this.checkBoxAllowPrintOrdering.Checked;
            newAlbum.Geography = this.checkBoxGeography.Checked;
            newAlbum.WorldSearchable = this.checkBoxWorldSearchable.Checked;
            newAlbum.SmugSearchable = this.checkBoxSmugSearchable.Checked;
            newAlbum.HideOwner = this.checkBoxHideOwner.Checked;
            newAlbum.FriendEdit = this.checkBoxFriendEdit.Checked;
            newAlbum.FamilyEdit = this.checkBoxFamilyEdit.Checked;
            newAlbum.CanRank = this.checkBoxPhotoRank.Checked;
            newAlbum.SquareThumbs = this.checkBoxSquareThumbs.Checked;

            if (this.comboBoxLargestSize.SelectedItem == null)
                this.comboBoxLargestSize.SelectedIndex = 0;

            switch (this.comboBoxLargestSize.SelectedItem.ToString())
            {
                case ("X3Large"):
                    newAlbum.Originals = false;
                    newAlbum.X3Larges = true;
                    newAlbum.X2Larges = newAlbum.X3Larges;
                    newAlbum.XLarges = newAlbum.X2Larges;
                    newAlbum.Larges = newAlbum.XLarges;
                    break;
                case ("X2Large"):
                    newAlbum.Originals = false;
                    newAlbum.X3Larges = false;
                    newAlbum.X2Larges = true;
                    newAlbum.XLarges = newAlbum.X2Larges;
                    newAlbum.Larges = newAlbum.XLarges;
                    break;
                case ("Large"):
                    newAlbum.Originals = false;
                    newAlbum.X3Larges = false;
                    newAlbum.X2Larges = false;
                    newAlbum.XLarges = true;
                    newAlbum.Larges = newAlbum.XLarges;
                    break;
                default:
                    newAlbum.Originals = true;
                    newAlbum.X3Larges = newAlbum.Originals;
                    newAlbum.X2Larges = newAlbum.X3Larges;
                    newAlbum.XLarges = newAlbum.X2Larges;
                    newAlbum.Larges = newAlbum.XLarges;
                    break;
            }

            if (this.checkBoxDisplayCustomHeader.Enabled)
                newAlbum.Header = this.checkBoxDisplayCustomHeader.Checked; // power/pro
            if (this.checkBoxProtected.Enabled)
                newAlbum.Protected = this.checkBoxProtected.Checked; // pro
            if (this.checkBoxCleanStyle.Enabled)
                newAlbum.Clean = this.checkBoxCleanStyle.Checked; // pro
            if (this.checkBoxApplyWatermark.Enabled)
            {
                newAlbum.Watermarking = this.checkBoxApplyWatermark.Checked; // pro

                if (this.comboBoxWatermark.SelectedIndex > 0 && this.comboBoxWatermark.SelectedValue != null)
                {
                    try
                    {
                        newAlbum.WatermarkID = Convert.ToInt32(this.comboBoxWatermark.SelectedValue);
                    }
                    catch
                    {
                    }
                }
            }

            return newAlbum;
        }
        #endregion

        private string GetToolTip(Photo photo)
        {
            // TODO: add camera info back
            StringBuilder toolTipText = new StringBuilder();

            if (!String.IsNullOrEmpty(photo.Title))
                toolTipText.AppendLine("Caption: " + photo.Title);
            if (!String.IsNullOrEmpty(photo.Keywords))
                toolTipText.AppendLine("Keywords: " + photo.Keywords);
            if (photo.Rating != 0)
                toolTipText.AppendLine("Rating: " + photo.Rating + " stars");

            return toolTipText.ToString();
        }

        public void SetAccountTypeSettings()
        {
            AccountTypeEnum accountType = this.smugMug.Account.User.AccountType;

            switch (accountType)
            {
                case (AccountTypeEnum.Standard):
                    this.checkBoxDisplayCustomHeader.Enabled = false;
                    this.checkBoxProtected.Enabled = false;
                    this.checkBoxCleanStyle.Enabled = false;
                    this.checkBoxApplyWatermark.Enabled = false;
                    this.comboBoxStyle.Enabled = false;
                    this.comboBoxLargestSize.Items.Remove("XLarge");
                    this.comboBoxLargestSize.Items.Remove("Large");
                    break;
                case (AccountTypeEnum.Power):
                    this.checkBoxDisplayCustomHeader.Enabled = true;
                    this.checkBoxProtected.Enabled = false;
                    this.checkBoxCleanStyle.Enabled = false;
                    this.checkBoxApplyWatermark.Enabled = false;
                    this.comboBoxStyle.Enabled = true;
                    this.comboBoxLargestSize.Items.Remove("XLarge");
                    this.comboBoxLargestSize.Items.Remove("Large");
                    break;
                case (AccountTypeEnum.Pro):
                    this.checkBoxProtected.Enabled = true;
                    this.checkBoxCleanStyle.Enabled = true;
                    this.checkBoxApplyWatermark.Enabled = true;
                    this.comboBoxStyle.Enabled = true;
                    break;
            }
        }

        private void SetPreview(Photo photo)
        {
            try
            {
                
                this.previewThumbnailBox.Photo = photo.GetFramedPhoto(
                    this.previewThumbnailBox.Width,
                    1,
                    3,
                    5,
                    Color.Black,
                    Color.White,
                    Color.Gray,
                    this.previewThumbnailBox.BackColor);

                string toolTipText = GetToolTip(photo);

                this.toolTip.SetToolTip(this.listViewPhotos, toolTipText);
                this.labelPreview.Text = toolTipText;
            }
            catch
            {
            }
        }

        private void SetPreview(FileInfo file)
        {
            if (Properties.Settings.Default.VideoExtensions.Contains(file.Extension.ToLowerInvariant()) | Properties.Settings.Default.QuickTimeExtensions.Contains(file.Extension.ToLowerInvariant()))
            {
                try
                {
                    this.previewThumbnailBox.URL = file.FullName;
                }
                catch
                {
                    this.previewThumbnailBox.Stop();
                }
            }
            else
            {
                try
                {
                    using (System.Drawing.Image image = System.Drawing.Image.FromFile(file.FullName))
                    {
                        this.previewThumbnailBox.Photo = Photo.GetThumbnail(image);
                        this.previewThumbnailBox.Stop();
                    }
                }
                catch
                {

                }
            }

            this.labelPreview.Text = String.Empty;
        }

        private void SetButtonFilterText()
        {
            bool filtered = false;

            foreach (MenuItem menuItem in this.menuItemRatings.MenuItems)
            {
                if (menuItem.Checked == false && menuItem.Text != "-") filtered = true;
            }

            foreach (MenuItem menuItem in this.menuItemKeywords.MenuItems)
            {
                if (menuItem.Checked == false && menuItem.Text != "-") filtered = true;
            }

            if (filtered)
                this.buttonFilter.Text = "Selection filtered";
            else
                this.buttonFilter.Text = "Filter selection";
        }

        private void SetUploadExistingAlbum()
        {
            if (this.comboBoxAdd.SelectedValue == null)
            {
                this.comboBoxAdd.SelectedIndex = 0;
            }
            else
            {
                //this.album = (Album)this.comboBoxAdd.SelectedItem;
                this.existingAlbum = true;

                ApplyUploadStatus();
                ApplyFilterToImages();
            }
        }

        private void SetStatusText(string text)
        {
            this.statusBarPanelText.Text = text;
            //BeginInvoke((MethodInvoker)delegate
            //{
            //    this.statusBarPanelText.Text = text;
            //});
        }

        private void ApplyUploadStatus()
        {
            if (!loading)
            {
                // if we have an existing album, lets deselect images we've already uploaded.
                if (this.existingAlbum)
                {
                    // spin the cursor
                    this.Cursor = Cursors.WaitCursor;

                    try
                    {
                        // reset the uploaded status
                        Album album = (Album)this.comboBoxAdd.SelectedItem;
                        var imagesResult = this.smugMug.GetImages(album.id, album.Key, "FileName");
                        this.imageIds = new Dictionary<string, long>();

                        if (imagesResult.Length > 0)
                        {
                            //this.images = imagesResult;

                            foreach (ListViewItem lvi in this.listViewPhotos.Items)
                            {
                                foreach (var info in imagesResult)
                                {
                                    lvi.SubItems[3].Text = Properties.Resources.StatusNotUploaded;
                                    lvi.SubItems[3].ResetStyle();
                                    lvi.Checked = true;

                                    if (info.FileName == lvi.Text)
                                    {
                                        lvi.SubItems[3].Text = Properties.Resources.StatusUploaded;
                                        lvi.SubItems[3].ResetStyle();
                                        lvi.SubItems[3].ForeColor = Color.Green;

                                        // add the imageID to a hashtable
                                        imageIds.Add(info.FileName, info.id);

                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (ListViewItem lvi in this.listViewPhotos.Items)
                            {
                                lvi.SubItems[3].Text = Properties.Resources.StatusNotUploaded;
                                lvi.SubItems[3].ResetStyle();
                            }

                            this.imageIds.Clear();
                        }
                    }
                    catch
                    {
                    }

                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void ApplyFilterToImages()
        {
            foreach (ListViewItem lvi in this.listViewPhotos.Items)
            {
                bool check = true;

                if (lvi.Tag is Photo)
                {
                    Photo photo = lvi.Tag as Photo;

                    // filter on keyword
                    foreach (MenuItem menuItem in this.menuItemKeywords.MenuItems)
                    {
                        string text = menuItem.Text;
                        if (photo.Keywords != null && photo.Keywords.Length > 0 && photo.Keywords.IndexOf(text) > -1)
                        {
                            lvi.Checked = menuItem.Checked;
                        }
                    }

                    // filter on rating
                    switch (photo.Rating)
                    {
                        case (0):
                            if (menuItemRatingUnrated.Checked == false)
                            {
                                check = false;
                            }
                            break;
                        case (1):
                            if (menuItemRating1.Checked == false)
                            {
                                check = false;
                            }
                            break;
                        case (2):
                            if (menuItemRating2.Checked == false)
                            {
                                check = false;
                            }
                            break;
                        case (3):
                            if (menuItemRating3.Checked == false)
                            {
                                check = false;
                            }
                            break;
                        case (4):
                            if (menuItemRating4.Checked == false)
                            {
                                check = false;
                            }
                            break;
                        case (5):
                            if (menuItemRating5.Checked == false)
                            {
                                check = false;
                            }
                            break;
                    }

                    lvi.Checked = check;
                }
            }
        }

        private void ToggleNewAlbumSettings(bool isNewAlbum)
        {
            if (isNewAlbum)
            {
                foreach (ListViewItem lvi in this.listViewPhotos.Items)
                {
                    lvi.SubItems[3].Text = Properties.Resources.StatusNotUploaded;
                }

                this.comboBoxAdd.Enabled = false;
                this.textBoxTitle.Enabled = true;
                this.comboBoxCategory.Enabled = true;
                if (this.textBoxTitle.Text.Length == 0 | this.textBoxTitle.Text == Properties.Resources.InsertTitlePlaceholder)
                {
                    this.buttonUpload.Enabled = false;
                }

                this.comboBoxSubCategory.Enabled = true;
                this.comboBoxShareGroup.Enabled = true;
                this.textBoxDescription.Enabled = true;
                this.textBoxKeywords.Enabled = true;
                this.textBoxPassword.Enabled = true;
                this.textBoxPasswordHint.Enabled = true;
                this.comboBoxSorting.Enabled = true;
                this.numericUpDownPosition.Enabled = true;
                this.comboBoxStyle.Enabled = true;
                this.checkBoxSortDirection.Enabled = true;
                this.radioButtonPublic.Enabled = true;
                this.radioButtonUnlisted.Enabled = true;
                this.linkLabelEditGallery.Enabled = false;

                foreach (TabPage tabPage in this.tabControl1.Controls)
                {
                    foreach (Control control in tabPage.Controls)
                    {
                        if (control is CheckBox)
                        {
                            CheckBox checkBox = control as CheckBox;
                            checkBox.Enabled = true;
                        }
                    }
                }
            }
            else
            {
                this.comboBoxAdd.Enabled = true;
                this.textBoxTitle.Enabled = false;
                this.comboBoxCategory.Enabled = false;
                this.comboBoxSubCategory.Enabled = false;
                this.comboBoxShareGroup.Enabled = false;
                this.textBoxDescription.Enabled = false;
                this.textBoxKeywords.Enabled = false;
                this.textBoxPassword.Enabled = false;
                this.textBoxPasswordHint.Enabled = false;
                this.comboBoxSorting.Enabled = false;
                this.numericUpDownPosition.Enabled = false;
                this.comboBoxStyle.Enabled = false;
                this.checkBoxSortDirection.Enabled = false;
                this.radioButtonPublic.Enabled = false;
                this.radioButtonUnlisted.Enabled = false;
                this.linkLabelEditGallery.Enabled = true;

                foreach (TabPage tabPage in this.tabControl1.Controls)
                {
                    foreach (Control control in tabPage.Controls)
                    {
                        if (control is CheckBox)
                        {
                            CheckBox checkBox = control as CheckBox;
                            checkBox.Enabled = false;
                        }
                    }
                }
            }
        }

        private void ProcessAlbumCreation(Task<Album>[] t)
        {
            if (t.Any(i => i.Exception != null))
            {
                MessageBox.Show("An error occured trying to create your albums, Send to SmugMug must now exit");

                Application.Exit();
            }
            else
            {
                foreach (var task in t)
                {
                    Album album = task.Result;

                    // TODO: this isn't very efficient, but need to look for the album based on the path in the dictionary
                    // TODO: need to impliment this once we enable multiple album creation based on sub-folders
                    this.AlbumsToCreate[""] = album;

                    if (this.comboBoxShareGroup.Items.Count > 0)
                    {
                        if (this.comboBoxShareGroup.SelectedIndex >= 0 && this.comboBoxShareGroup.SelectedValue != null
                            && this.comboBoxShareGroup.SelectedValue.ToString().Length > 0)
                        {
                            try
                            {
                                int shareGroupID = Convert.ToInt32(this.comboBoxShareGroup.SelectedValue);
                                this.AddAlbumToShareGroup(album.id, shareGroupID);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                List<Album> albums = new List<Album>();
                albums.AddRange((Album[])this.comboBoxAdd.DataSource);
                albums.AddRange(this.AlbumsToCreate.Values);
                
                // TODO: when we add back multiple album support need to make sure this doesn't trigger the add to existing album code
                //UpdateComboBoxAlbums(albums.ToArray(), true);
            }
        }

        private void UploadPhotos()
        {
            // for each photo, store the album id and key for uploading
            foreach (ListViewItem lvi in this.listViewPhotos.Items)
            {
                FileInfo file = null;

                if (lvi.Tag is Photo)
                {
                    Photo photo = lvi.Tag as Photo;
                    file = new FileInfo(photo.GetFullPath());

                }
                else if (lvi.Tag is FileInfo)
                {
                    file = lvi.Tag as FileInfo;
                }

                if (this.checkBoxCreateForEach.Checked)
                {
                    // get the album based on the directory path of the photo
                    string rootDirectory = file.DirectoryName;
                    Album uploadAlbum = this.AlbumsToCreate[rootDirectory];
                    // store the album based on the full path of the photo
                    this.photoAlbumDestination.Add(file.FullName, uploadAlbum);
                }
                else
                {
                    // store the album based on the full path of the photo
                    this.photoAlbumDestination.Add(file.FullName, this.AlbumsToCreate[""]);
                }
            }

            this.statusBarPanelText.Text = String.Format("Uploading images 1 of {0}", this.listViewPhotos.CheckedItems.Count);
            logger.InfoFormat("Upload Start Time: {0}", DateTime.Now);
            DateTime dtStart = DateTime.Now;

            this.uploadCount = this.listViewPhotos.CheckedItems.Count;

            foreach (ListViewItem lvi in this.listViewPhotos.Items)
            {
                if (lvi.Checked == false)
                    continue;

                this.currentItem = lvi;

                this.listViewPhotos.SelectedItems.Clear();
                lvi.Selected = true;

                // TODO: this is causing a bug
                try
                {
                    this.listViewPhotos.TopItem = lvi;
                }
                catch
                {

                }
                
                this.progressBar.Maximum = 100;
                uploadImagesWorker = new BackgroundWorker();
                uploadImagesWorker.WorkerReportsProgress = true;
                uploadImagesWorker.WorkerSupportsCancellation = true;
                uploadImagesWorker.DoWork += new DoWorkEventHandler(uploadImagesWorker_DoWork);
                uploadImagesWorker.ProgressChanged += new ProgressChangedEventHandler(uploadImagesWorker_ProgressChanged);
                uploadImagesWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(uploadImagesWorker_RunWorkerCompleted);
                uploadImagesWorker.RunWorkerAsync(lvi.Tag);

                while (this.uploadImagesWorker.IsBusy)
                {
                    // Keep UI messages moving, so the form remains 
                    // responsive during the asynchronous operation.
                    System.Threading.Thread.Sleep(20);
                    Application.DoEvents();

                    if (this.closing)
                        return;
                }

                if (this.cancelled)
                {
                    break;
                }
            }

            this.elapsed = new TimeSpan(DateTime.Now.Ticks - dtStart.Ticks);
            DateTime dtEnd = DateTime.Now;
            logger.InfoFormat("Upload End Time: {0}", DateTime.Now);
            TimeSpan timeSpan = new TimeSpan(dtEnd.Ticks - dtStart.Ticks);
            logger.InfoFormat("Upload Elapsed Time: {0} Seconds", timeSpan.Seconds);

            decimal bytes = this.bytesUploaded / 1024m / 1024m;
            logger.InfoFormat("Images were {0} uploaded", (this.uploadError) ? "unsuccessfully" : "successfully");
            string message = String.Format("Uploaded {0} photos totaling {1} MB in {2} minutes {3} seconds. View Album?",
            this.filesUploaded,
            bytes.ToString("N"),
            this.elapsed.Minutes,
            this.elapsed.Seconds);

            if (this.elapsed.TotalMinutes > 60)
            {
                message = String.Format("Uploaded {0} photos totaling {1} MB in {2} hours {3} minutes {4} seconds. View Album?",
                    this.filesUploaded,
                    bytes.ToString("N"),
                    this.elapsed.Hours,
                    this.elapsed.Minutes,
                    this.elapsed.Seconds);
            }

            DialogResult result = MessageBox.Show(this,
                message,
                "Upload Complete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            if (result == DialogResult.Yes)
            {
                string url = String.Format("http://{0}.smugmug.com/", this.smugMug.Account.User.NickName);

                if (this.AlbumsToCreate.Count == 1)
                {
                    Album album = this.AlbumsToCreate[""];;
                    url = String.Format("http://{0}.smugmug.com/gallery/{1}_{2}", this.smugMug.Account.User.NickName, album.id, album.Key);
                }

                Utilities.OpenUrl(url);
            }

            if (this.cancelled == false & this.uploadError == false)
                Application.Exit();
            else
            {
                ResetStatusBarToUpload();
                this.textBoxTitle.Enabled = false;
                this.uploadingAfterCreate = true;
                this.radioButtonAddToExistingAlbum.Enabled = false;
            }
        }


        #region Preferences
        private void LoadPrefs()
        {
            this.comboBoxSorting.SelectedIndex = Properties.Settings.Default.AlbumSorting;

            if (Properties.Settings.Default.AlbumLargestSize < 0 | Properties.Settings.Default.AlbumLargestSize > 6)
                Properties.Settings.Default.AlbumLargestSize = 0;

            if (Properties.Settings.Default.DefaultCategoryID != 0)
            {
                foreach (Category item in this.comboBoxCategory.Items)
                {
                    if (item.id == Properties.Settings.Default.DefaultCategoryID)
                    {
                        this.comboBoxCategory.SelectedItem = item;
                        break;
                    }
                }
            }

            this.comboBoxLargestSize.SelectedIndex = Properties.Settings.Default.AlbumLargestSize;
            this.comboBoxDuplicates.SelectedIndex = Properties.Settings.Default.Duplicates;
            if (this.comboBoxDuplicates.SelectedIndex < 0)
                this.comboBoxDuplicates.SelectedIndex = 0;

            if (this.radioButtonPublic.Checked == false)
            {
                this.radioButtonUnlisted.Checked = true;
            }

            if (this.comboBoxSorting.SelectedIndex < 0)
            {
                this.comboBoxSorting.SelectedIndex = 5;
            }

            this.buttonDonate.Visible = !RegistrySettings.DonatedSendToSmugMug;

            this.SetButtonFilterText();
        }

        #endregion

        #region VersionChecker
        private void CheckForNewVersion()
        {
            logger.Info("Checking for new version...");
            VersionManager versionManager = new VersionManager(App.UpdateUrl,
                Application.ProductName, Application.ProductVersion, false);
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - RegistrySettings.LastUpdateTimeSendToSmugMug.Ticks);
            if (ts.Days >= 0)
            {
                BackgroundWorker versionChecker = new BackgroundWorker();
                versionChecker.DoWork += (sender, e) =>
                {
                    try
                    {
                        versionManager.CheckForApplicationUpdate();
                        e.Result = true;
                    }
                    catch
                    {
                        e.Result = false;
                    }
                };

                versionChecker.RunWorkerCompleted += (sender, e) =>
                {
                    bool completed = (bool)e.Result;
                    if (completed)
                    {
                        logger.InfoFormat("Current Version: {0}, Latest Version: {1}", Application.ProductVersion, versionManager.AvailableVersion);

                        bool newVersionAvailable = versionManager.NewVersionAvailable;
                        if (newVersionAvailable)
                        {
                            logger.InfoFormat("A new version is available");
                            versionManager.DownloadNewUpdate(false);
                        }
                    }
                };

                versionChecker.RunWorkerAsync();
            }
        }
        #endregion

        #region LoadImages
        

        void loadImagesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            logger.InfoFormat("Loading Start Time: {0}", DateTime.Now);
            DateTime dtStart = DateTime.Now;

            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                List<FileInfo> photosToLoad = e.Argument as List<FileInfo>;
                int photoCount = photosToLoad.Count;

                int i = 1;

                foreach (FileInfo file in photosToLoad)
                {
                    System.Threading.Thread.Sleep(100);
                    Application.DoEvents();

                    //System.Threading.Thread.Sleep(500);
                    if (file.Extension.ToLower() == ".jpg")
                    {
                        Photo photo = new Photo(file.FullName);
                        worker.ReportProgress(i++, photo);
                    }
                    else
                    {
                        worker.ReportProgress(i++, file);
                    }

                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.Message);
            }
            finally
            {
                DateTime dtEnd = DateTime.Now;
                TimeSpan timeSpan = new TimeSpan(dtEnd.Ticks - dtStart.Ticks);
                logger.InfoFormat("Loading End Time: {0}", DateTime.Now);
                logger.InfoFormat("Elapsed Time: {0} Seconds", timeSpan.Seconds);
            }
        }

        void loadImagesWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!this.progressBar.Visible)
            {
                this.progressBar.Visible = true;
            }

            this.progressBar.Value = e.ProgressPercentage;

            SetStatusText(String.Format("Loading images {0} of {1}", e.ProgressPercentage, this.progressBar.Maximum));

            ListViewItem lvi;
            bool skipItem = false;
            FileInfo file;

            if (e.UserState is Photo)
            {
                Photo photo = e.UserState as Photo;
                file = new FileInfo(photo.GetFullPath());

                logger.InfoFormat("Loading images {0} of {1}", e.ProgressPercentage, this.progressBar.Maximum);
                logger.InfoFormat("Loading image {0}", file.FullName);

                lvi = new ListViewItem(photo.GetFileName());
                if (photo.Keywords != null && photo.Keywords.Length > 0)
                {
                    string s = photo.Keywords.TrimEnd(' ', ';', ',');
                    ArrayList keywordList = new ArrayList();
                    if (s.IndexOf(';') > -1)
                    {
                        keywordList.AddRange(s.Split(';'));
                    }
                    else if (s.IndexOf(',') > -1)
                    {
                        keywordList.AddRange(s.Split(','));
                    }
                    else
                    {
                        keywordList.Add(s);
                    }

                    foreach (string keyword in keywordList)
                    {
                        if (this.keywords.Contains(keyword.Trim()) == false)
                        {
                            this.keywords.Add(keyword.Trim());
                        }
                    }
                }

                if (file.Length > this.smugMug.Account.User.FileSizeLimit)
                {
                    logger.InfoFormat("Photo exceeds file size limit: {0}", this.smugMug.Account.User.FileSizeLimit);
                    skipItem = true;
                }

                string size = (file.Length / 1024).ToString("N0") + " KB";

                lvi.SubItems.Add(size);
                lvi.SubItems.Add(photo.DateTimeOriginal.ToString());
                lvi.SubItems.Add("");
                lvi.ToolTipText = GetToolTip(photo);
                lvi.Tag = photo;
            }
            else
            {
                file = e.UserState as FileInfo;

                logger.InfoFormat("Loading images {0} of {1}", e.ProgressPercentage, this.progressBar.Maximum);
                logger.InfoFormat("Loading image {0}", file.FullName);

                lvi = new ListViewItem(file.Name);
                string size = (file.Length / 1024).ToString("N0") + " KB";

                lvi.SubItems.Add(size);
                lvi.SubItems.Add(file.CreationTime.ToString());
                lvi.SubItems.Add("");
                lvi.Tag = file;
            }

            if (skipItem == false)
            {
                lvi.UseItemStyleForSubItems = false;
                lvi.Checked = true;
                lvi.Group = listViewPhotos.Groups[file.DirectoryName];

                this.listViewPhotos.Items.Add(lvi);
                this.listViewPhotos.Update();
            }
            else
            {
                skippedFiles = true;
            }
        }

        void loadImagesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.loading = false;

            ResetStatusBarToUpload();

            // set all the options to their default states
            this.ToggleNewAlbumSettings(!this.existingAlbum);

            this.ApplyUploadStatus();

            if (this.listViewPhotos.Items.Count > 0)
            {
                this.listViewPhotos.Items[0].Selected = true;
                this.buttonUpload.Enabled = true;
                if (this.radioButtonCreateNewAlbum.Checked)
                    this.textBoxTitle.Enabled = true;
                this.radioButtonCreateNewAlbum.Enabled = true;
            }

            ArrayList.Adapter(this.keywords).Sort();

            foreach (string keyword in this.keywords)
            {
                MenuItem menuItem = new MenuItem(keyword);
                menuItem.Checked = true;
                menuItem.Click += new EventHandler(menuItemKeyword_Click);
                this.menuItemKeywords.MenuItems.Add(menuItem);
            }

            ApplyFilterToImages();

            if (this.skippedFiles)
            {
                string size = (this.smugMug.Account.User.FileSizeLimit / 1024).ToString("N0");
                MessageBox.Show(
                    String.Format(Properties.Resources.SkippedFiles, size),
                    "Some files to large",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                    );
            }

            // sort the list
            this.listViewPhotos.ListViewItemSorter = new ListViewItemDateComparer();
        }
        #endregion

        #region UploadImages
        private void uploadImagesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.cancelled = false;
            BackgroundWorker worker = sender as BackgroundWorker;

            FileInfo fileInfo;

            string title = null;
            string keyword = null;

            if (e.Argument is Photo)
            {
                Photo photo = e.Argument as Photo;

                fileInfo = new FileInfo(photo.GetFullPath());
                title = photo.Title;
                keyword = photo.Keywords.TrimEnd(' ', ';', ',');

                //if (keyword.Length > 0 && keyword.Contains(";") == false)
                //    keyword = "\"" + keyword + "\"";
            }
            else
            {
                fileInfo = e.Argument as FileInfo;
            }

            long albumId = this.photoAlbumDestination[fileInfo.FullName].id;
            string albumKey = this.photoAlbumDestination[fileInfo.FullName].Key;

            UploadContext uploadContext = null;
            
            try
            {
                long imageID;

                if (fileInfo.Exists == false)
                    throw new ArgumentException("Image does not exist: " + fileInfo.FullName);

                using (FileStream fileStream = File.OpenRead(fileInfo.FullName))
                {
                    // Initiate upload. If we are replacing duplicates, pass in imageID
                    // TODO: does this logic make sense for a new album?
                    if (uploadDuplicates == UploadDuplicates.Replace && this.imageIds.ContainsKey(fileInfo.Name))
                    {
                        imageID = this.imageIds[fileInfo.Name];
                        uploadContext = this.smugMug.PrepareUpload(fileStream, fileInfo.Name, keyword, albumId, albumKey, imageID);
                    }
                    else
                    {
                        uploadContext = this.smugMug.PrepareUpload(fileStream, fileInfo.Name, keyword, albumId, albumKey);
                    }

                    this.isBeforeUpload = true;

                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        this.cancelled = true;
                    }
                    else
                    {
                        // Main upload loop.
                        do
                        {
                            // calculate the progress out of a base "100"                    
                            double progressPercentage = ((double)uploadContext.CurrentPosition / (double)uploadContext.Request.ContentLength);
                            int percentage = (int)(progressPercentage * 100);
                            // update the progress bar                    
                            worker.ReportProgress(percentage);
                            if (worker.CancellationPending)
                            {
                                e.Cancel = true;
                                this.cancelled = true;
                                return;
                            }
                        } while (this.smugMug.UploadChunk(uploadContext));

                        // Get response after upload.
                        imageID = this.smugMug.FinishUpload(uploadContext, albumId, albumKey, fileInfo.Name);

                        // set the highlight image before we are done
                        if (imageID != 0 && this.highlightImageFilename != null && this.highlightImageFilename.Length > 0)
                        {
                            if (fileInfo.Name == this.highlightImageFilename)
                            {
                                try
                                {
                                    this.smugMug.SetAlbumHighlight(albumId, imageID);
                                    this.highlightImageFilename = null;
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Error setting highlight image", ex);
                                }
                            }
                        }

                        this.bytesUploaded += fileInfo.Length;
                        this.filesUploaded++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Exception uploading photo", ex);
                this.uploadError = true;
                // increment this value so the progress status is accurate
                this.filesUploaded++;
                throw ex;
            }
        }

        private void uploadImagesWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.isBeforeUpload)
            {
                ListViewItem lvi = currentItem;

                if (lvi.Tag is Photo)
                {
                    Photo photo = lvi.Tag as Photo;
                    SetPreview(photo);
                }
                else if (lvi.Tag is FileInfo)
                {
                    FileInfo file = lvi.Tag as FileInfo;
                    SetPreview(file);
                }

                int counter = this.filesUploaded + 1;

                lvi.SubItems[3].Text = Properties.Resources.StatusUploading;
                lvi.SubItems[3].ResetStyle();
                lvi.SubItems[3].Font = new Font(this.listViewPhotos.Font, this.listViewPhotos.Font.Style | FontStyle.Italic);

                this.statusBarPanelText.Text = String.Format("Uploading images {0} of {1}", counter, uploadCount);
                logger.InfoFormat("Uploading images {0} of {1}", counter, uploadCount);
                this.isBeforeUpload = false;
            }

            if (e.ProgressPercentage >= this.progressBar.Minimum & e.ProgressPercentage <= this.progressBar.Maximum)
            {
                this.progressBar.Value = e.ProgressPercentage;
            }
        }

        private void uploadImagesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progressBar.Value = this.progressBar.Maximum;

            ListViewItem lvi = currentItem;

            // First, handle the case where an exception was thrown.
            if (e.Error != null)
            {
                lvi.SubItems[3].Text = Properties.Resources.StatusFailed;
                lvi.SubItems[3].Font = new Font(this.listViewPhotos.Font, this.listViewPhotos.Font.Style | FontStyle.Italic);
                lvi.SubItems[3].ForeColor = Color.Red;
            }
            else if (e.Cancelled)
            {
                lvi.SubItems[3].Text = Properties.Resources.StatusFailed;
                lvi.SubItems[3].Font = new Font(this.listViewPhotos.Font, this.listViewPhotos.Font.Style | FontStyle.Italic);
                lvi.SubItems[3].ForeColor = Color.Red;
            }
            else
            {
                lvi.SubItems[3].Text = Properties.Resources.StatusUploaded;
                lvi.SubItems[3].ResetStyle();
                lvi.SubItems[3].ForeColor = Color.Green;
                lvi.Checked = false;
            }
        }

        private void ResetStatusBarToUpload()
        {
            this.progressBar.Visible = false;
            this.listViewPhotos.Enabled = true;
            this.buttonUpload.Text = "Upload";
            this.menuItemEditAccountSettings.Enabled = true;
            this.SetStatusText(
                String.Format("Connected to SmugMug as {0} ({1} tier account)", this.smugMug.Account.User.NickName, this.smugMug.Account.User.AccountType));
        }

        #endregion

        #region Controls
        private void listViewPhotos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listViewPhotos.SelectedItems.Count == 1)
            {
                ListViewItem lvi = this.listViewPhotos.SelectedItems[0];

                if (lvi.Text == this.highlightImageFilename)
                {
                    this.checkBoxHighlight.Checked = true;
                }
                else
                {
                    this.checkBoxHighlight.Checked = false;
                }

                if (lvi.Tag is Photo)
                {
                    Photo photo = lvi.Tag as Photo;
                    this.SetPreview(photo);
                }
                else if (lvi.Tag is FileInfo)
                {
                    FileInfo file = lvi.Tag as FileInfo;
                    this.SetPreview(file);
                }
            }
        }

        private void radioButtonAdd_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAddToExistingAlbum.Checked)
            {
                this.ToggleNewAlbumSettings(false);

                SetUploadExistingAlbum();
            }
        }

        private void radioButtonNew_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonCreateNewAlbum.Checked)
            {
                this.ToggleNewAlbumSettings(true);
                ApplyFilterToImages();
            }
        }

        private void textBoxTitle_Enter(object sender, EventArgs e)
        {
            if (this.textBoxTitle.Text == Properties.Resources.InsertTitlePlaceholder)
            {
                this.textBoxTitle.Text = String.Empty;
                this.buttonUpload.Enabled = false;
            }
        }

        private void textBoxTitle_Leave(object sender, EventArgs e)
        {
            if (this.textBoxTitle.Text == String.Empty)
            {
                this.textBoxTitle.Text = Properties.Resources.InsertTitlePlaceholder;
                this.buttonUpload.Enabled = false;
            }
            else 
            {
                this.buttonUpload.Enabled = true;
            }
        }

        private void textBoxTitle_TextChanged(object sender, EventArgs e)
        {
            if (!this.loading)
            {
                if (this.textBoxTitle.Text.Length == 0)
                {
                    this.buttonUpload.Enabled = false;
                }
                else
                {
                    this.buttonUpload.Enabled = true;
                }
            }
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            if (this.uploadImagesWorker.IsBusy)
            {
                this.uploadImagesWorker.CancelAsync();
            }

            Application.Exit();
        }

        private void buttonUpload_Click(object sender, EventArgs e)
        {
            // check to see if we are currently uploading something and cancel if we are
            if (this.buttonUpload.Text == "Cancel")
            {
                logger.Info("Cancelling upload");
                this.uploadImagesWorker.CancelAsync();
                this.cancelled = true;
                return;
            }

            // show progress to kick things off
            this.progressBar.Value = 0;
            this.progressBar.Visible = true;

            // disable the listbox so we can't mess with it
            this.listViewPhotos.Enabled = false;

            this.bytesUploaded = 0;
            this.filesUploaded = 0;

            int photoCount = this.listViewPhotos.CheckedItems.Count;

            this.buttonUpload.Text = "Cancel";
            this.menuItemEditAccountSettings.Enabled = false;

            this.photoAlbumDestination.Clear();

            // skip uploading previously uploaded photos if we are uploading to a new album
            if (this.radioButtonAddToExistingAlbum.Checked)
            {
                this.uploadDuplicates = (UploadDuplicates)this.comboBoxDuplicates.SelectedIndex;

                foreach (ListViewItem lvi in this.listViewPhotos.CheckedItems)
                {
                    if (this.uploadDuplicates == UploadDuplicates.Skip &&
                        lvi.SubItems[3].Text == Properties.Resources.StatusUploaded)
                    {
                        lvi.Checked = false;
                    }
                    else
                    {
                        lvi.SubItems[3].Text = Properties.Resources.StatusPending;
                        lvi.SubItems[3].ResetStyle();
                        lvi.SubItems[3].Font = new Font(this.listViewPhotos.Font, this.listViewPhotos.Font.Style | FontStyle.Italic);
                    }
                }

                this.AlbumsToCreate.Clear();
                Album album = (Album)this.comboBoxAdd.SelectedItem;
                this.AlbumsToCreate.Add("", album);
                UploadPhotos();
            }
            else if (this.radioButtonCreateNewAlbum.Checked & uploadingAfterCreate == false)
            {
                // only create a new album if we aren't re-uploading images for a failed or cancelled session
                List<Task<Album>> tasks = new List<Task<Album>>();

                // create all the albums
                if (this.checkBoxCreateForEach.Checked == false)
                {
                    this.AlbumsToCreate.Clear();
                    Album album = GetNewAlbum(this.textBoxTitle.Text);
                    this.AlbumsToCreate.Add("", album);
                }

                foreach (var item in this.AlbumsToCreate)
                {
                    Album album = GetNewAlbum(item.Value.Title);
                    // TODO: figure out why this commented code is here
                    //album.Title = item.Key;
                    Task<Album> t = Task.Factory.StartNew(() => this.smugMug.CreateAlbum(album));
                    tasks.Add(t);
                }

                Task.Factory.ContinueWhenAll(tasks.ToArray(), result =>
                {
                    ProcessAlbumCreation(result);
                    UploadPhotos();

                }, CancellationToken.None, TaskContinuationOptions.None, taskSchedulerMain);
            }
            else
            {
                UploadPhotos();
            }
        }

        private void comboBoxCategory_SelectionChangeCommitted(object sender, EventArgs e)
        {
            UpdateComboBoxSubCategories();
        }

        private void comboBoxAdd_Validated(object sender, System.EventArgs e)
        {
            SetUploadExistingAlbum();
        }

        private void comboBoxAdd_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SetUploadExistingAlbum();
        }

        private void buttonDonate_Click(object sender, EventArgs e)
        {
            DonateForm donateForm = new DonateForm(ProductName);
            DialogResult result = donateForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.buttonDonate.Visible = !RegistrySettings.DonatedSendToSmugMug;
            }
        }

        private void menuItemRating_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string text = menuItem.Text;

            if (menuItem.Checked)
            {
                menuItem.Checked = false;
            }
            else
            {
                menuItem.Checked = true;
            }

            int filterOut = 0;
            if (text == this.menuItemRating1.Text)
            {
                filterOut = 1;
                Properties.Settings.Default.MenuItemRating1 = this.menuItemRating1.Checked;
            }
            else if (text == this.menuItemRating2.Text)
            {
                filterOut = 2;
                Properties.Settings.Default.MenuItemRating2 = this.menuItemRating2.Checked;
            }
            else if (text == this.menuItemRating3.Text)
            {
                filterOut = 3;
                Properties.Settings.Default.MenuItemRating3 = this.menuItemRating3.Checked;
            }
            else if (text == this.menuItemRating4.Text)
            {
                filterOut = 4;
                Properties.Settings.Default.MenuItemRating4 = this.menuItemRating4.Checked;
            }
            else if (text == this.menuItemRating5.Text)
            {
                filterOut = 5;
                Properties.Settings.Default.MenuItemRating5 = this.menuItemRating5.Checked;
            }
            else if (text == this.menuItemRatingUnrated.Text)
            {
                filterOut = 0;
                Properties.Settings.Default.MenuItemRatingUnrated = this.menuItemRatingUnrated.Checked;
            }

            Properties.Settings.Default.Save();

            foreach (ListViewItem lvi in this.listViewPhotos.Items)
            {
                if (lvi.Tag is Photo)
                {
                    Photo photo = lvi.Tag as Photo;
                    if (photo.Rating == filterOut)
                    {
                        lvi.Checked = menuItem.Checked;
                    }
                }
            }

            this.SetButtonFilterText();
        }

        private void menuItemKeyword_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string text = menuItem.Text;

            if (menuItem.Checked)
            {
                menuItem.Checked = false;
            }
            else
            {
                menuItem.Checked = true;
            }

            foreach (ListViewItem lvi in this.listViewPhotos.Items)
            {
                if (lvi.Tag is Photo)
                {
                    Photo photo = lvi.Tag as Photo;
                    if (photo.Keywords != null && photo.Keywords.Length > 0 && photo.Keywords.IndexOf(text) > -1)
                    {
                        lvi.Checked = menuItem.Checked;
                    }
                }
            }

            this.SetButtonFilterText();
        }

        private void buttonFilter_Click(object sender, EventArgs e)
        {
            this.buttonFilter.ContextMenu.Show(this.buttonFilter, new Point(0, 0));
        }

        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            string url = "http://www.smugmug.com/help/private-albums";
            Utilities.OpenUrl(url);
        }

        private void checkBoxHighlight_CheckedChanged(object sender, System.EventArgs e)
        {
            if (this.listViewPhotos.SelectedItems.Count > 0)
            {
                try
                {
                    ListViewItem lvi = this.listViewPhotos.SelectedItems[0];

                    if (this.checkBoxHighlight.Checked)
                    {
                        // remove the bolding from any current highlighted items
                        if (this.highlightImageFilename != null && this.highlightImageFilename.Length > 0)
                        {
                            foreach (ListViewItem l in this.listViewPhotos.Items)
                            {
                                if (l.Font.Bold)
                                {
                                    l.Font = new Font(l.Font.FontFamily, l.Font.Size, FontStyle.Regular);
                                }
                            }
                        }
                        this.highlightImageFilename = lvi.SubItems[0].Text;
                        lvi.Font = new Font(lvi.Font.FontFamily, lvi.Font.Size, FontStyle.Bold);
                    }
                    else
                    {
                        lvi.Font = new Font(lvi.Font.FontFamily, lvi.Font.Size, FontStyle.Regular);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error selecting highlighted image", ex);
                }
            }
        }

        private void menuItemEditSelectAll_Click(object sender, System.EventArgs e)
        {
            if (this.textBoxTitle.Focused)
                this.textBoxTitle.SelectAll();
            else if (this.textBoxDescription.Focused)
                this.textBoxDescription.SelectAll();
            else if (this.textBoxKeywords.Focused)
                this.textBoxKeywords.SelectAll();
            else if (this.textBoxPassword.Focused)
                this.textBoxPassword.SelectAll();
            else if (this.textBoxPasswordHint.Focused)
                this.textBoxPasswordHint.SelectAll();
            else if (this.listViewPhotos.Focused)
            {
                for (int index = 0; index < this.listViewPhotos.Items.Count; index++)
                {
                    this.listViewPhotos.Items[index].Selected = true;
                }
            }
        }

        private void checkBoxCheckAll_CheckedChanged(object sender, System.EventArgs e)
        {
            if (this.checkBoxCheckAll.Checked)
            {
                for (int index = 0; index < this.listViewPhotos.Items.Count; index++)
                {
                    this.listViewPhotos.Items[index].Checked = true;
                }
            }
            else
            {
                for (int index = 0; index < this.listViewPhotos.Items.Count; index++)
                {
                    this.listViewPhotos.Items[index].Checked = false;
                }
            }
        }

        private void menuItemFileExit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void menuItemHelpAbout_Click(object sender, System.EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void comboBoxShareGroup_Enter(object sender, System.EventArgs e)
        {
            if (this.comboBoxShareGroup.Items.Count == 0)
            {
                this.GetShareGroups();
            }
        }

        private void menuItemSendError_Click(object sender, System.EventArgs e)
        {
            SendErrorLog();
        }

        private void menuItemFeedback_Click(object sender, System.EventArgs e)
        {
            Utilities.OpenUrl("http://sendtosmugmug.uservoice.com/");
        }

        private void menuItemForums_Click(object sender, System.EventArgs e)
        {
            Utilities.OpenUrl("http://www.shahine.com/garage/forums/7.aspx");
        }

        private void linkLabelManageShareGroups_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utilities.OpenUrl(String.Format("http://{0}.smugmug.com/homepage/sharegroup.mg", this.smugMug.Account.User.NickName));
        }

        private void menuItemApplyDefaultSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            this.SetAccountTypeSettings();
        }

        private void linkLabelEditGallery_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.comboBoxAdd.SelectedItem != null && this.comboBoxAdd.SelectedItem is Album)
            {
                Album selectedAlbum = (Album)this.comboBoxAdd.SelectedItem;
                string albumId = selectedAlbum.id.ToString();
                string albumKey = selectedAlbum.Key;

                string url = String.Format("http://{0}.smugmug.com/gallery/settings.mg?AlbumID={1}&AlbumKey={2}", this.smugMug.Account.User.NickName, albumId, albumKey);
                Utilities.OpenUrl(url);
            }
        }

        // TODO: make this work again
        private void menuItemEditAccountSettings_Click(object sender, EventArgs e)
        {
            RegistrySettings.TokenSecret = String.Empty;
            this.Bootstrap();
        }

        private void menuItemLoadImagesFromSubdirectories_Click(object sender, EventArgs e)
        {
            if (menuItemLoadImagesFromSubdirectories.Checked)
            {
                menuItemLoadImagesFromSubdirectories.Checked = false;
            }
            else
            {
                menuItemLoadImagesFromSubdirectories.Checked = true;
            }

            Properties.Settings.Default.LoadImagesFromSubfolders = menuItemLoadImagesFromSubdirectories.Checked;
        }

        private void listViewPhotos_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine what the last sort order was and change it.
            if (this.listViewPhotos.Sorting == SortOrder.Ascending)
                this.listViewPhotos.Sorting = SortOrder.Descending;
            else
                this.listViewPhotos.Sorting = SortOrder.Ascending;

            if (e.Column == 2)
            {
                this.listViewPhotos.ListViewItemSorter = new ListViewItemDateComparer(this.listViewPhotos.Sorting);
            }
            else
            {
                this.listViewPhotos.ListViewItemSorter = new ListViewItemStringComparer(e.Column, this.listViewPhotos.Sorting);
            }
        }

        private void menuItemDefaultCategory_Click(object sender, EventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            int categoryID = Convert.ToInt32(menu.Tag);
            Properties.Settings.Default.DefaultCategoryID = categoryID;
            Properties.Settings.Default.Save();

            foreach (MenuItem item in this.menuItemDefaultCategory.MenuItems)
            {
                if (item.Checked == true)
                    item.Checked = false;
            }

            menu.Checked = true;
        }

        private void comboBoxWatermark_Enter(object sender, EventArgs e)
        {
            if (this.comboBoxWatermark.Items.Count > 0)
            {
                var taskGetAlbums = Task.Factory.StartNew(() => GetWatermarks(), CancellationToken.None, TaskCreationOptions.None, taskSchedulerMain);
            }
        }

        private void checkBoxCreateForEach_CheckedChanged(object sender, EventArgs e)
        {
            this.textBoxTitle.Text = String.Empty;

            if (checkBoxCreateForEach.Checked)
            {
                this.textBoxTitle.Enabled = false;
                foreach (var item in this.AlbumsToCreate)
                {
                    Album album = item.Value;
                    this.textBoxTitle.Text += album.Title + ", ";
                }

                this.textBoxTitle.Text = this.textBoxTitle.Text.TrimEnd(' ');
                this.textBoxTitle.Text = this.textBoxTitle.Text.TrimEnd(',');
            }
            else
            {
                this.textBoxTitle.Enabled = true;
                this.textBoxTitle.Text = this.files[0].DirectoryName;
            }
        }
        #endregion
    }
}

// Implements the manual sorting of items by columns.
class ListViewItemDateComparer : IComparer
{
    private SortOrder sortOrder;

    public ListViewItemDateComparer()
    {
        this.sortOrder = SortOrder.Ascending;
    }

    public ListViewItemDateComparer(SortOrder sortOrder)
    {
        this.sortOrder = sortOrder;
    }

    public int Compare(object x, object y)
    {
        if (this.sortOrder == SortOrder.Ascending)
            return DateTime.Compare(Convert.ToDateTime(((ListViewItem)x).SubItems[2].Text), Convert.ToDateTime(((ListViewItem)y).SubItems[2].Text));
        else
            return DateTime.Compare(Convert.ToDateTime(((ListViewItem)y).SubItems[2].Text), Convert.ToDateTime(((ListViewItem)x).SubItems[2].Text));
    }
}

// Implements the manual sorting of items by columns.
class ListViewItemStringComparer : IComparer
{
    private int col;
    private SortOrder sortOrder;

    public ListViewItemStringComparer()
    {
        col = 0;
        this.sortOrder = SortOrder.Ascending;
    }
    public ListViewItemStringComparer(int column, SortOrder sortOrder)
    {
        this.col = column;
        this.sortOrder = sortOrder;
    }
    public int Compare(object x, object y)
    {
        if (this.sortOrder == SortOrder.Ascending)
            return String.Compare(((ListViewItem)y).SubItems[col].Text, ((ListViewItem)x).SubItems[col].Text);
        else
            return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
    }
}