using System;
using System.IO;
namespace Unchord
{
    // NOTE: Version #001.
    public class DataFileV001 : DataFile
    {
        private const int c_FILE_VERSION = 1;

        public int V0
        {
            get => m_mapDataBlock.ReadInt32(126);
            set => m_mapDataBlock.WriteInt32(126, value);
        }

        public int V1
        {
            get => m_playerDataBlock.ReadInt32(64);
            set => m_playerDataBlock.WriteInt32(64, value);
        }

        private DataBlock m_mapDataBlock;
        private DataBlock m_playerDataBlock;

        public DataFileV001(string _dataFilePath)
        : base(_dataFilePath)
        {

        }

        public override DataFile Create()
        {
            m_mapDataBlock = new DataBlock(DataSize.SIZE_1K);
            m_playerDataBlock = new DataBlock(DataSize.SIZE_1K);

            m_mapDataBlock.SectionVersion = DataFileV001.c_FILE_VERSION;
            m_mapDataBlock.DataCacheSize = (int)DataSize.SIZE_128B;

            m_playerDataBlock.SectionVersion = DataFileV001.c_FILE_VERSION;
            m_playerDataBlock.DataCacheSize = (int)DataSize.SIZE_256B;

            FileStream fs = new FileStream(base.DataFilePath, FileMode.Create, FileAccess.Write);

            fs.SetLength((long)DataSize.SIZE_2K);
            m_mapDataBlock.Save(fs);
            m_playerDataBlock.Save(fs);
            fs.Close();

            return this;
        }

        public override DataFile Load()
        {
            m_mapDataBlock = new DataBlock(DataSize.SIZE_1K);
            m_playerDataBlock = new DataBlock(DataSize.SIZE_1K);

            FileStream fs = new FileStream(base.DataFilePath, FileMode.Open, FileAccess.Read);
            m_mapDataBlock.Load(fs);
            m_playerDataBlock.Load(fs);
            fs.Close();

            return this;
        }

        public override DataFile Save()
        {
            FileStream fs = new FileStream(base.DataFilePath, FileMode.Open, FileAccess.Write);
            m_mapDataBlock.Save(fs);
            m_playerDataBlock.Save(fs);
            fs.Close();
            return this;
        }
    }
}