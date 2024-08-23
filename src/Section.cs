using System.IO;

namespace Unchord
{
    public abstract class Section
    {
#region Persistent Data
        public uint SectionSignature { get; set; }
        public int SectionVersion { get; set; }
        public int SectionSize { get; set; }
        public int ProtectedAddressByte { get; set; } = 32;
#endregion

#region Runtime Data
        public string DataFilePath { get; set; }
        public int StartPosition { get; set; }
#endregion

#region Save & Load API
        public abstract void Save(FileStream _stream);
        public abstract void Load(FileStream _stream);

        public void Save()
        {
            FileStream fs = new FileStream(this.DataFilePath, FileMode.Open, FileAccess.Write);
            fs.Seek(StartPosition, SeekOrigin.Begin);
            this.Save(fs);
            fs.Close();
        }

        public void Load()
        {
            FileStream fs = new FileStream(this.DataFilePath, FileMode.Open, FileAccess.Read);
            fs.Seek(StartPosition, SeekOrigin.Begin);
            this.Load(fs);
            fs.Close();
        }
#endregion
    }
}