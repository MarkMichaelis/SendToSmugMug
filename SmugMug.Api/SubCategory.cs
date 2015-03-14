namespace SmugMug.Api
{
    public struct SubCategory
    {
        public override string ToString()
        {
            return this.Name;
        }

        public int id;
        public string NiceName;
        public string Name;
    }
}
