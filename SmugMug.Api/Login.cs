namespace SmugMug.Api
{
    public struct Login
    {
        public AccountStatusEnum AccountStatus;
        public AccountTypeEnum AccountType;
        public int FileSizeLimit;
        public string PasswordHash;
        public Session Session;
        public bool SmugVault;
        public User User;
    }
}
