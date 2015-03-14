namespace SmugMug.Api
{
    public struct User
    {
        public int id;
        public string NickName;
        public string DisplayName;
        //public string AccountType;
        public string URL;

        // OAuth
        public AccountStatusEnum AccountStatus;
        public AccountTypeEnum AccountType;
        public int FileSizeLimit;
        public bool SmugVault;
    }
}
