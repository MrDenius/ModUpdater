namespace ModUpdater.Jsons
{
    public struct NewData
    {
        public NewData(string type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public string type { get; set; }
        public string value { get; set; }

    }
}
