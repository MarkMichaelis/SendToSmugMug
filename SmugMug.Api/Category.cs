namespace SmugMug.Api
{
    public struct Category
    {
        public override string ToString()
        {
            return this.Name;
        }

        public long id;
        public string Name;
        public string NiceName;
        public CategoryType Type;
    }

    public enum CategoryType
    {
        SmugMug,
        User
    }
}
