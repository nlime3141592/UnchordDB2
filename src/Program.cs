using System;

namespace Unchord
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string path = @"C:\Programming\CSharp\DataFile\output.dat";

            DataFileV001 file = new DataFileV001(path);

            // file.Create();
            // file.Load();
            // DataBlock block = file.GetBlock(0);
            DataBlock block = DataBlock.CreateEmptyBlock();

            int beg = 125;
            int cnt = 327;
/*
            // Generating
            Random prng = new Random();
            byte[] buffer = new byte[cnt];
            prng.NextBytes(buffer);
            
            // Writing
            block.WriteBytes(beg, buffer);
*/
/*
            // Reading and Printing
            byte[] buffer2 = block.ReadBytes(beg, cnt);
            file.Save();

            for(int i = 0; i < buffer2.Length; ++i)
            {
                if(i > 0 && i % 20 == 0)
                    Console.WriteLine();

                Console.Write("{0:X02} ", buffer2[i]);
            }
*/
/*
            DataBlock.Fragment frag = new DataBlock.Fragment();
            frag.idxBeg = 23;
            frag.idxEnd = 1325;
            frag.prev = frag;
            frag.next = frag;
            block.m_SplitFragmentByCache(frag, 124);
            Console.WriteLine(block.m_GetFragmentChain(frag));
*/
            // End of Program
            Console.WriteLine("\nOK");
        }
    }
}