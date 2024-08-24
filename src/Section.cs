using System.IO;

namespace Unchord
{
    public abstract class Section
    {
#region Persistent Data
        public uint SectionSignature
        {
            get => m_sectionSignature;
            set => m_sectionSignature = value;
        }

        public int SectionVersion
        {
            get => m_sectionVersion;
            set => m_sectionVersion = value;
        }

        public int SectionSize
        {
            get => m_sectionSize;
            set
            {
                System.Diagnostics.Debug.Assert(value > 0, "section size should be greater than 0.");
                m_sectionSize = value;
            }
        }

        public int ProtectedAddressByte
        {
            get => m_protectedAddressByte;
            set
            {
                System.Diagnostics.Debug.Assert(value > 0, "protected address byte should be greater than 0.");
                m_protectedAddressByte = value;
            }
        }

        private uint m_sectionSignature;
        private int m_sectionVersion;
        private int m_sectionSize = 1;
        private int m_protectedAddressByte = 32;
#endregion

#region Runtime Data
        public string DataFilePath { get; set; }
        public int StartPosition { get; set; }
#endregion

#region File Operations
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