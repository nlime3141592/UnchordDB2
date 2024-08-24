using System;
using System.IO;
using System.Text;

namespace Unchord
{
    public class DataBlock : Section
    {
#region Persistent Data
        public int DataCacheSize { get; set; }
#endregion

#region Runtime Data
        private byte[] m_dataCache;
        private int m_loadedCacheIndex;
        private int m_validByteCountOnCache;

        private Fragment m_dataChangeHistories;

        // private object m_lock;
#endregion

#region Internal Data Structures
        private class Fragment
        {
            public int idxBeg;
            public int idxEnd;
            public byte[] data;

            // NOTE: For implementing doubly linked list.
            public Fragment prev;
            public Fragment next;
        }
#endregion

        public DataBlock(int _sectionVersion, int _sectionSize, int _dataCacheSize)
        {
            base.SectionSignature = (uint)(Unchord.SectionSignature.BLOCK);
            base.SectionVersion = _sectionVersion;
            base.SectionSize = _sectionSize;

            this.DataCacheSize = _dataCacheSize;

            Fragment initialHistory = new Fragment();
            this.m_ClearFragment(initialHistory);
            m_dataChangeHistories = initialHistory;
        }

        public DataBlock(int _sectionVersion, int _sectionSize, DataSize _dataCacheSize)
        : this(_sectionVersion, _sectionSize, (int)_dataCacheSize)
        {

        }

        public DataBlock(int _sectionVersion, DataSize _sectionSize, int _dataCacheSize)
        : this(_sectionVersion, (int)_sectionSize, _dataCacheSize)
        {

        }

        public DataBlock(int _sectionVersion, DataSize _sectionSize, DataSize _dataCacheSize)
        : this(_sectionVersion, (int)_sectionSize, (int)_dataCacheSize)
        {

        }

        private DataBlock()
        : this(0, 0, 0)
        {
            
        }

        public static DataBlock CreateEmptyBlock()
        {
            return new DataBlock();
        }

#region File Operations
        public override void Save(FileStream _stream)
        {
            Fragment frag = this.m_dataChangeHistories;

            BinaryWriter wr = new BinaryWriter(_stream);
            wr.Write(base.SectionSignature);
            wr.Write(base.SectionVersion);
            wr.Write(base.SectionSize);
            wr.Write(base.ProtectedAddressByte);
            wr.Write(this.DataCacheSize);

            do
            {
                if(frag.data != null)
                {
                    _stream.Seek(base.StartPosition + frag.idxBeg, SeekOrigin.Begin);
                    _stream.Write(frag.data, 0, frag.data.Length);
                }

                frag = frag.next;
                frag.prev.next = null;
                frag.prev = null;
            }
            while(frag != m_dataChangeHistories);

            this.m_ClearFragment(this.m_dataChangeHistories);
            _stream.Seek(base.StartPosition + base.SectionSize, SeekOrigin.Begin);
        }

        public override void Load(FileStream _stream)
        {
            BinaryReader rd = new BinaryReader(_stream);

            base.DataFilePath = _stream.Name;
            base.StartPosition = (int)_stream.Position;

            base.SectionSignature = rd.ReadUInt32();
            base.SectionVersion = rd.ReadInt32();
            base.SectionSize = rd.ReadInt32();
            base.ProtectedAddressByte = rd.ReadInt32();

            this.DataCacheSize = rd.ReadInt32();
            this.m_dataCache = new byte[this.DataCacheSize];

            Fragment frag = m_dataChangeHistories;

            do
            {
                frag = frag.next;
                frag.prev.next = null;
                frag.prev = null;
            }
            while(frag != m_dataChangeHistories);

            m_ClearFragment(frag);

            this.m_LoadCache(_stream, 0, this.DataCacheSize);
            _stream.Seek(base.StartPosition + base.SectionSize, SeekOrigin.Begin);
        }
#endregion

#region Writing Data
        public void WriteByte(int _byteIndex, byte _value)
        {
            byte[] bytes = { _value };
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteSByte(int _byteIndex, sbyte _value)
        {
            byte[] bytes = { (byte)_value };
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteChar(int _byteIndex, char _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteInt16(int _byteIndex, short _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteUInt16(int _byteIndex, ushort _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteInt32(int _byteIndex, int _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteUInt32(int _byteIndex, uint _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteInt64(int _byteIndex, long _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteUInt64(int _byteIndex, ulong _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteSingle(int _byteIndex, float _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteDouble(int _byteIndex, double _value)
        {
            byte[] bytes = BitConverter.GetBytes(_value);
            this.WriteBytes(_byteIndex, bytes);
        }

        public void WriteBytes(int _byteIndex, byte[] _bytes)
        {
            System.Diagnostics.Debug.Assert(_byteIndex >= base.ProtectedAddressByte, "cannot access protected address space.");

            m_WriteBytes(_byteIndex, _bytes);
        }

        private void m_WriteBytes(int _byteIndex, byte[] _bytes)
        {
            Fragment fragBeg = m_dataChangeHistories;
            Fragment fragEnd = null;
            int byteIndexEnd = _byteIndex + _bytes.Length - 1;

            System.Diagnostics.Debug.Assert(_byteIndex >= 0 && byteIndexEnd < base.SectionSize, "byte index out of range.");

            do
            {
                if(_byteIndex >= fragBeg.idxBeg && _byteIndex <= fragBeg.idxEnd)
                    break;
                else
                    fragBeg = fragBeg.next;
            }
            while(fragBeg != m_dataChangeHistories);

            fragEnd = fragBeg;

            do
            {
                if(byteIndexEnd >= fragEnd.idxBeg && byteIndexEnd <= fragEnd.idxEnd)
                    break;
                else
                    fragEnd = fragEnd.next;
            }
            while(fragEnd != m_dataChangeHistories);

            if(_byteIndex > fragBeg.idxEnd || byteIndexEnd > fragEnd.idxEnd)
            {
                System.Diagnostics.Debug.Assert(false, "you try to write on out of range.");
                return;
            }

            if(fragBeg == fragEnd)
            {
                if(fragBeg.data != null)
                {
                    Buffer.BlockCopy(_bytes, 0, fragBeg.data, _byteIndex - fragBeg.idxBeg, _bytes.Length);
                    return;
                }
                else
                {
                    Fragment f0 = new Fragment();
                    Fragment f1 = new Fragment();

                    f0.idxBeg = _byteIndex;
                    f0.idxEnd = byteIndexEnd;
                    f0.data = _bytes;
                    f0.prev = fragBeg;
                    f0.next = f1;

                    f1.idxBeg = byteIndexEnd + 1;
                    f1.idxEnd = fragBeg.idxEnd;
                    f1.prev = f0;
                    f1.next = fragBeg.next;

                    fragBeg.idxEnd = _byteIndex - 1;
                    fragBeg.next.prev = f1;
                    fragBeg.next = f0;
                }
            }
            else
            {
                Fragment f0 = new Fragment();

                f0.idxBeg = _byteIndex;
                f0.idxEnd = byteIndexEnd;
                f0.data = _bytes;

                if(f0.idxBeg == fragBeg.idxBeg)
                {
                    fragBeg.prev.next = f0;
                    f0.prev = fragBeg.prev;
                }
                else
                {
                    fragBeg.idxEnd = _byteIndex - 1;

                    if(fragBeg.data != null)
                    {
                        byte[] data = new byte[fragBeg.idxEnd - fragBeg.idxBeg + 1];

                        for(int i = 0; i < data.Length; ++i)
                            data[i] = fragBeg.data[i];

                        fragBeg.data = data;
                    }

                    fragBeg.next = f0;
                    f0.prev = fragBeg;
                }

                if(f0.idxEnd == fragEnd.idxEnd)
                {
                    fragEnd.next.prev = f0;
                    f0.next = fragEnd.next;
                }
                else
                {
                    fragEnd.idxBeg = byteIndexEnd + 1;
                    
                    if(fragEnd.data != null)
                    {
                        byte[] data = new byte[fragEnd.idxEnd - fragEnd.idxBeg + 1];

                        for(int i = 0; i < data.Length; ++i)
                            data[i] = fragEnd.data[fragEnd.idxEnd - data.Length + i + 1];

                        fragEnd.data = data;
                    }

                    fragEnd.prev = f0;
                    f0.next = fragEnd;
                }
            }
        }
#endregion

#region Reading Data
        public byte ReadByte(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 1);
            return bytes[0];
        }

        public sbyte ReadSByte(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 1);
            return (sbyte)bytes[0];
        }

        public short ReadInt16(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 2);
            return BitConverter.ToInt16(bytes, 0);
        }

        public ushort ReadUInt16(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public int ReadInt32(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public uint ReadUInt32(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public long ReadInt64(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public ulong ReadUInt64(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 8);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public float ReadSingle(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 4);
            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble(int _byteIndex)
        {
            byte[] bytes = this.ReadBytes(_byteIndex, 8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte[] ReadBytes(int _byteIndex, int _count)
        {
            System.Diagnostics.Debug.Assert(_byteIndex >= base.ProtectedAddressByte, "cannot access protected address space.");
            return m_ReadBytes(_byteIndex, _count);
        }

        private byte[] m_ReadBytes(int _byteIndex, int _count)
        {
            int byteIndexBeg = base.StartPosition + _byteIndex;
            int byteIndexEnd = byteIndexBeg + _count - 1;

            System.Diagnostics.Debug.Assert(_byteIndex >= 0 && byteIndexEnd < base.SectionSize, "byte index out of range.");

            byte[] bytes = new byte[_count];

            Fragment readingFragment = new Fragment();
            readingFragment.idxBeg = byteIndexBeg;
            readingFragment.idxEnd = byteIndexEnd;
            readingFragment.data = null;
            readingFragment.prev = readingFragment;
            readingFragment.next = readingFragment;

            Fragment frag = this.m_dataChangeHistories;
            int i = byteIndexBeg;

            while(i <= byteIndexEnd)
            {
                while(i < frag.idxEnd || i > frag.idxEnd)
                    frag = frag.next;

                if(frag.data != null)                    
                {
                    int idxBeg = Math.Max(byteIndexBeg, frag.idxBeg);
                    int idxEnd = Math.Min(byteIndexEnd, frag.idxEnd);

                    int srcOffset = idxBeg - frag.idxBeg;
                    int dstOffset = byteIndexBeg - idxBeg;

                    Buffer.BlockCopy(frag.data, srcOffset, bytes, dstOffset, idxEnd - idxBeg + 1);
                    frag = frag.next;
                }
                else
                {
                    
                }
            }

            return bytes;
        }
#endregion

#region Data Caching
        private void m_LoadCache(int _cacheIndex, int _cacheSize)
        {
            FileStream fs = new FileStream(base.DataFilePath, FileMode.Open, FileAccess.Read);
            m_LoadCache(fs, _cacheIndex, _cacheSize);
            fs.Close();
        }

        private void m_LoadCache(FileStream _stream, int _cacheIndex, int _cacheSize)
        {
            System.Diagnostics.Debug.Assert(_cacheIndex >= 0, "cache index out of range.");
            System.Diagnostics.Debug.Assert(_cacheIndex * _cacheSize < base.SectionSize, "cache index out of range.");

            int positionOrigin = base.StartPosition + _cacheIndex * _cacheSize;
            this.m_validByteCountOnCache = Math.Min(_cacheSize, base.SectionSize - positionOrigin);

            _stream.Seek(positionOrigin, SeekOrigin.Begin);
            _stream.Read(this.m_dataCache, 0, this.m_validByteCountOnCache);
        }
#endregion

#region Data Formating
        public string GetChangeHistories()
        {
            return m_GetFragmentChain(m_dataChangeHistories);
        }

        public string GetSectionInfo()
        {
            StringBuilder strBuilder = new StringBuilder((int)DataSize.SIZE_256B);

            strBuilder.AppendFormat("Block Section:\n");
            strBuilder.AppendFormat("  - {0, -20} : {1:X08}\n", "Section Signature", base.SectionSignature);
            strBuilder.AppendFormat("  - {0, -20} : {1}\n", "Section Version", base.SectionVersion);
            strBuilder.AppendFormat("  - {0, -20} : {1}\n", "Section Size", base.SectionSize);
            strBuilder.AppendFormat("  - {0, -20} : {1}\n", "Protected Address Byte", base.ProtectedAddressByte);
            strBuilder.AppendFormat("  - {0, -20} : {1}", "Data Cache Size", this.DataCacheSize);

            return strBuilder.ToString();
        }

        private string m_GetFragmentChain(Fragment _root)
        {
            StringBuilder strBuilder = new StringBuilder(this.DataCacheSize);
            Fragment frag = _root;

            strBuilder.AppendFormat("Block Changes History:\n");
            strBuilder.Append("  -   prev   <-       current       ->   next   : data\n");

            do
            {
                strBuilder.AppendFormat("  - {0:X08} <- ({1:X08}-{2:X08}) -> {3:X08} :", frag.prev.idxBeg, frag.idxBeg, frag.idxEnd, frag.next.idxBeg);

                if(frag.data == null)
                {
                    strBuilder.Append(" <no changes>");
                }
                else
                {
                    for(int i = 0; i < frag.data.Length; ++i)
                        strBuilder.AppendFormat(" {0:X02}", frag.data[i]);
                }

                if(frag.next != _root)
                    strBuilder.Append("\n");

                frag = frag.next;
            }
            while(frag != _root);

            return strBuilder.ToString();
        }
#endregion

        private void m_ClearFragment(Fragment _fragment)
        {
            _fragment.idxBeg = 0;
            _fragment.idxEnd = base.SectionSize - 1;
            _fragment.data = null;
            _fragment.prev = _fragment;
            _fragment.next = _fragment;

            m_dataChangeHistories = _fragment;
        }

        private void m_SplitFragmentByCache(Fragment _fragment, int _cacheSize)
        {
            Fragment frag = _fragment;

            while(true)
            {
                int cacheIndexBeg = frag.idxBeg / _cacheSize;
                int cacheIndexEnd = frag.idxEnd / _cacheSize;

                if(cacheIndexBeg == cacheIndexEnd)
                    break;

                int idxBeg = Math.Max(frag.idxBeg, cacheIndexBeg * _cacheSize);
                int idxEnd = Math.Min(frag.idxEnd, (cacheIndexBeg + 1) * _cacheSize - 1);

                if(idxEnd < frag.idxEnd)
                {
                    Fragment f0 = new Fragment();
                    f0.idxBeg = idxEnd + 1;
                    f0.idxEnd = frag.idxEnd;
                    frag.idxEnd = idxEnd;

                    f0.prev = frag;
                    f0.next = frag.next;
                    frag.next.prev = f0;
                    frag.next = f0;

                    if(frag.data != null)
                    {
                        byte[] data = new byte[f0.idxEnd - f0.idxBeg + 1];
                        Buffer.BlockCopy(frag.data, f0.idxBeg, data, 0, data.Length);
                        f0.data = data;
                    }

                    frag = frag.next;
                }
                else
                {
                    frag = frag.next;
                }
            }
        }
    }
}