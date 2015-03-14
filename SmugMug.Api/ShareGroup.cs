namespace SmugMug.Api
{
    public struct ShareGroup
    {
        public override string ToString()
        {
            return this.Name;
        }

        public int id;
        public string Tag;
        public string Name;
        public string URL; 
        public int AlbumCount;
        public string Description;
        public bool AccessPassworded;
        public string Password;
        public bool Passworded;
        public bool PasswordHint;
    }
}
