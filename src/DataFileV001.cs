using System;
using System.Collections.Generic;
using System.IO;
namespace Unchord
{
    // NOTE: Version #001.
    public class DataFileV001 : DataFile
    {
        private const int c_FILE_VERSION = 1;

        public int V0
        {
            get => m_dataBlocks[0].ReadInt32(126);
            set => m_dataBlocks[0].WriteInt32(126, value);
        }

        public int V1
        {
            get => m_dataBlocks[1].ReadInt32(64);
            set => m_dataBlocks[1].WriteInt32(64, value);
        }

        private List<DataBlock> m_dataBlocks;

        public DataFileV001(string _dataFilePath)
        : base(_dataFilePath)
        {
            m_dataBlocks = new List<DataBlock>(4);
        }

        public override DataFile Create()
        {
            m_dataBlocks.Add(new DataBlock(DataFileV001.c_FILE_VERSION, DataSize.SIZE_1K, DataSize.SIZE_128B));
            m_dataBlocks.Add(new DataBlock(DataFileV001.c_FILE_VERSION, DataSize.SIZE_1K, DataSize.SIZE_256B));

            int fileSize = 0;

            for(int i = 0; i < m_dataBlocks.Count; ++i)
                fileSize += m_dataBlocks[i].SectionSize;

            FileStream fs = new FileStream(base.DataFilePath, FileMode.Create, FileAccess.Write);

            fs.SetLength((long)fileSize);

            for(int i = 0; i < m_dataBlocks.Count; ++i)
                m_dataBlocks[i].Save(fs);

            fs.Close();

            return this;
        }

        public override DataFile Load()
        {
            FileStream fs = new FileStream(base.DataFilePath, FileMode.Open, FileAccess.Read);
            int i = 0;

            m_dataBlocks.Clear();

            while(fs.Position < fs.Length)
            {
                m_dataBlocks.Add(DataBlock.CreateEmptyBlock());
                m_dataBlocks[i].Load(fs);
                ++i;
            }

            fs.Close();

            return this;
        }

        public override DataFile Save()
        {
            FileStream fs = new FileStream(base.DataFilePath, FileMode.Open, FileAccess.Write);
            
            for(int i = 0; i < m_dataBlocks.Count; ++i)
                m_dataBlocks[i].Save(fs);

            fs.Close();

            return this;
        }

        public DataBlock GetBlock(int _index)
        {
            return m_dataBlocks[_index];
        }
    }
}