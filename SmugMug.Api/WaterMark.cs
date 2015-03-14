namespace SmugMug.Api
{
    public struct Watermark
    {
        public override string ToString()
        {
            return this.Name;
        }

        public int id;
        public string Name;

        public int WatermarkID
        {
            get { return this.id; }
        }

        public string Title
        {
            get { return this.Name; }
        }
    }

}
