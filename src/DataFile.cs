namespace Unchord
{
    public abstract class DataFile
    {
        public string DataFilePath { get; private set; }

        public DataFile(string _dataFilePath)
        {
            this.DataFilePath = _dataFilePath;
        }

        public abstract DataFile Create();
        public abstract DataFile Load();
        public abstract DataFile Save();
    }
}