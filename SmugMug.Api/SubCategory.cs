namespace SmugMug.Api
{
    public struct SubCategory
    {
        public override string ToString()
        {
            return this.Name;
        }

        public long id;
        public string NiceName;
        public string Name;
    }
}
