using System;

namespace Unchord
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string path = @"C:\Programming\CSharp\DataFile\output.dat";
            
            DataBlock block = new DataBlock(DataSize.SIZE_1K);
            block.SectionVersion = 1;
            block.DataCacheSize = 127;

            Console.WriteLine(block.GetChangesHistory());
            block.WriteInt32(124, 0x004488cc);
            Console.WriteLine(block.GetChangesHistory());
            block.WriteInt32(125, 0x115599dd);
            Console.WriteLine(block.GetChangesHistory());
            block.WriteInt32(126, 0x2266aaee);
            Console.WriteLine(block.GetChangesHistory());
            block.WriteInt32(127, 0x3377bbff);
            Console.WriteLine(block.GetChangesHistory());

            Console.WriteLine("Read at {0:X08} : {1:X08}", 124, block.ReadInt32(124));
            Console.WriteLine("Read at {0:X08} : {1:X08}", 125, block.ReadInt32(125));
            Console.WriteLine("Read at {0:X08} : {1:X08}", 126, block.ReadInt32(126));
            Console.WriteLine("Read at {0:X08} : {1:X08}", 127, block.ReadInt32(127));

            block.WriteInt32(1020, 0x66cc11bb);
            Console.WriteLine(block.GetChangesHistory());
            block.WriteInt32(35, 0x44ee2266);
            Console.WriteLine(block.GetChangesHistory());
            Console.WriteLine("{0:X08}, {1}", block.ReadInt32(1020), block.ReadInt32(1020));
            
/*
            DataFileV001 file = new DataFileV001(path);
            // file.Create();
            file.Load();
            file.V0 = 0x11223344;
            file.V1 = 0x55667788;
            file.Save();
            Console.WriteLine("OK");*/
        }
    }
}