using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Schema;

namespace PointerSearcher
{
    class NoexsDumpIndex
    {
        public long address;
        public long pos;
        public long size;
        public NoexsDumpIndex(long addrValue, long posValue, long sizeValue)
        {
            address = addrValue;
            pos = posValue;
            size = sizeValue;
        }
    }
    class NoexsMemoryInfo
    {
        public long address;
        public long size;
        public NoexsMemoryType type;
        public Int32 perm;
        public NoexsMemoryInfo(long addrValue, long sizeValue, Int32 typeValue, Int32 permValue)
        {
            address = addrValue;
            size = sizeValue;
            type = (NoexsMemoryType)Enum.ToObject(typeof(NoexsMemoryType), typeValue);
            perm = permValue;
        }
        public bool IsReadable()
        {
            return (perm & 1) != 0;
        }

        public bool IsWriteable()
        {
            return (perm & 2) != 0;
        }

        public bool IsExecutable()
        {
            return (perm & 4) != 0;
        }

        public bool Contains(long addr)
        {
            return (address <= addr) && (address + size > addr);
        }
    }
    class NoexsDumpDataReader : IDumpDataReader
    {

        private BinaryReader fileStream;
        private long mainStartAddress;
        private long mainEndAddress;
        private long heapStartAddress;
        private long heapEndAddress;
        private long TargetAddress;
        private byte[] buffer = new byte[8];
        private byte[] buffN = null;
        private long length = 0;
        private bool firstread = true;
        private List<NoexsDumpIndex> indices;
        //private List<NoexsMemoryInfo> infos;
        private Dictionary<long, long> readData;    //key:address,value:data
        private bool m_compress = false;
        public NoexsDumpDataReader(String path, long mainStart, long mainEnd, long heapStart, long heapEnd)
        {
            fileStream = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));//System.Security.AccessControl.FileSystemRights.Read, FileShare.Read, 1024, FileOptions.SequentialScan));
            mainStartAddress = mainStart;
            mainEndAddress = mainEnd;
            heapStartAddress = heapStart;
            heapEndAddress = heapEnd;
            buffer = new byte[8];
            indices = null;
            //infos = new List<NoexsMemoryInfo>();
            readData = new Dictionary<long, long>();
        }

        ~NoexsDumpDataReader()
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }
        private void ReadData(int length)
        {
            fileStream.Read(buffer, 0, length);
        }
        private void ReverseEndian(int length)
        {
            Array.Reverse(buffer, 0, length);
        }
        private Int32 ReadBigEndianInt32()
        {
            ReadData(4);
            ReverseEndian(4);
            return BitConverter.ToInt32(buffer, 0);
        }
        private Int64 ReadBigEndianInt64()
        {
            ReadData(8);
            ReverseEndian(8);
            return BitConverter.ToInt64(buffer, 0);
        }
        private Int64 ReadLittleEndianInt64()
        {
            //ReadData(8);
            return fileStream.ReadInt64();//BitConverter.ToInt64(buffer, 0);
        }
        private Int64 ReadLittleEndianInt64(long address)
        {
            if (firstread)
            {
                long start = 134 + 8 * 5; // Edizon start of data dump
                length = (fileStream.BaseStream.Length - start) / 16;// from Address+ to address
                fileStream.BaseStream.Seek(start, SeekOrigin.Begin);
                int readSize = (int)(fileStream.BaseStream.Length - start);
                if (m_compress)
                {
                    byte[] cbuff = fileStream.ReadBytes(readSize);
                    buffN = new byte[readSize * 4];
                    length = LZ_Uncompress(cbuff, buffN, readSize) / 16;
                }
                else
                    buffN = fileStream.ReadBytes(readSize);
                firstread = false;
            }
            int Start = 0;
            int end = (int)length - 1;
            while ((end - Start) > 5)      // check whether optimal
            {
                int index = Start + (end - Start) / 2;
                long source = BitConverter.ToInt64(buffN, (index * 2) << 3);
                if (source == address)
                {
                    long target = BitConverter.ToInt64(buffN, (index * 2 + 1) << 3);
                    return target;
                }
                else if (source > address)
                {
                    end = (index - 1);
                }
                else
                {
                    Start = (index + 1);
                }
            }
            for (int index = Start; index <= end; index++)
            {
                long source = BitConverter.ToInt64(buffN, (index * 2) << 3);
                if (source == address)
                {
                    long target = BitConverter.ToInt64(buffN, (index * 2 + 1) << 3);
                    return target;
                }
            }
            return 0;
        }

        private void ReadIndicate()
        {
            if (indices != null)
            {
                //if already read indices,skip reading
                return;
            }
            indices = new List<NoexsDumpIndex>();
            fileStream.BaseStream.Seek(0, SeekOrigin.Begin);
            int magic = fileStream.ReadInt32();
            if (magic == 0x4E5A4665)
            {
                m_compress = true;
            }
            else if (magic != 0x4E5A4445) //edizon's version
            {
                fileStream.Close();
                throw new Exception("illegal file format");
            } 
            fileStream.BaseStream.Seek(134, SeekOrigin.Begin); // Edizon header size = 134
            mainStartAddress = fileStream.ReadInt64();
            mainEndAddress = fileStream.ReadInt64();
            heapStartAddress = fileStream.ReadInt64();
            heapEndAddress = fileStream.ReadInt64();
            TargetAddress = fileStream.ReadInt64();
            return;

        }
        //void IDumpDataReader.lzcompress()
        //{

        //}
        uint IDumpDataReader.LZ_Compress(byte[] inbuf, byte[] outbuf, int insize)
        {
            return LZ_Compress(inbuf, outbuf, insize);
        }
        private uint LZ_Compress(byte[] inbuf, byte[] outbuf, int insize)
        {
            uint inpos, outpos;
            uint front, back = 0;
            uint MAXRANGE = 16;
            bool match;
            if (insize < 1)
            {
                return 0;
            }
            inpos = 0;
            outpos = 0;
            do
            {
                front = 8;
                for (uint frnt = 0; frnt <= 8; frnt++)
                {
                    for (uint bak = 1; bak <= MAXRANGE; bak++)
                    {
                        if (inpos < bak * 8)
                        {
                            break;
                        }
                        match = true;
                        //uint inposbak = inpos - bak * 8;
                        for (uint i = frnt; i < 8; i++)
                            if (inbuf[inpos - bak * 8 + i] != inbuf[inpos + i]) { match = false; break; };
                        //if ((*(ulong*)(&in[inpos -back * 8]) &(0xFFFFFFFFFFFFFFFF << 8 * frnt)) == (*(ulong*)(&in[inpos]) &(0xFFFFFFFFFFFFFFFF << 8 * frnt)))
                        if (match)
                        {
                            front = frnt;
                            back = bak - 1;
                            frnt = 8;
                            break;
                        }
                    }
                }
                outbuf[outpos] = (byte)(front * 16 + back);
                outpos += 1;
                for (uint i = 0; i < front; i++)
                    outbuf[outpos + i] = inbuf[inpos + i];
                //*(unsigned long long*)(&outbuf[outpos]) = *(unsigned long long*)(&inbuf[inpos]);
                outpos += front;
                inpos += 8;
            } while (inpos < MAXRANGE * 8);
            do
            {
                front = 8;
                for (uint frnt = 0; frnt <= 8; frnt++)
                {
                    for (uint bak = 1; bak <= MAXRANGE; bak++)
                    {
                        match = true;
                        for (uint i = frnt; i < 8; i++)
                            if (inbuf[inpos - bak * 8 + i] != inbuf[inpos + i]) { match = false; break; };
                        if (match)
                        {
                            front = frnt;
                            back = bak - 1;
                            frnt = 8;
                            break;
                        }
                    }
                }
                outbuf[outpos] = (byte)(front * 16 + back);
                outpos += 1;
                for (uint i = 0; i < front; i++)
                    outbuf[outpos + i] = inbuf[inpos + i];
                outpos += front;
                inpos += 8;
            } while (inpos < insize);
            return outpos;
        }
        uint IDumpDataReader.LZ_Uncompress(byte[] inbuf, byte[] outbuf, int insize)
        {
            return LZ_Uncompress(inbuf, outbuf, insize);
        }
        private uint LZ_Uncompress(byte[] inbuf, byte[] outbuf, int insize)
        {
            uint inpos, outpos, i;
            uint front,back;
            if (insize < 1)
            {
                return 0;
            }
            inpos = 0;
            outpos = 0;
            do
            {
                front = (uint)(inbuf[inpos] / 16);
                back = (uint)(inbuf[inpos] & 0xF) * 8 + 8;
                inpos++;
                for (i = 0; i < front; i++)
                    outbuf[outpos + i] = inbuf[inpos + i];
                for (i = front; i < 8; i++)
                    outbuf[outpos + i] = outbuf[outpos - back + i];
                inpos += front;
                outpos += 8;
            } while (inpos < insize);
            return outpos;
        }
        private uint LZ_Uncompress2(byte[] inbuf, ulong[] outbuf, int insize)
        {
            ulong[] mask = { 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFF00, 0xFFFFFFFFFFFF0000, 0xFFFFFFFFFF000000, 0xFFFFFFFF00000000, 0xFFFFFF0000000000, 0xFFFF000000000000, 0xFF00000000000000, 0x0000000000000000 };
            uint inpos, outpos;
            uint front, back;
            if (insize < 1)
            {
                return 0;
            }
            inpos = 0;
            outpos = 0;
            do
            {
                front = (uint)(inbuf[inpos] / 16);
                back = (uint)(inbuf[inpos] & 0xF) + 1;
                inpos++;
                if (front == 8)
                    outbuf[outpos] =  BitConverter.ToUInt64(inbuf, (int)inpos);
                else
                    outbuf[outpos] =  BitConverter.ToUInt64(inbuf, (int)inpos) & ~mask[front] | outbuf[outpos - back] & mask[front];
                inpos += front;
                outpos += 1;
            } while (inpos < insize);
            return outpos;
        }
        PointerInfo IDumpDataReader.Read(CancellationToken token, IProgress<int> prog)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            PointerInfo pointerInfo = new PointerInfo();

            ReadIndicate();
            long start = 134 + 8 * 5; // Edizon start of data dump
            long length = (fileStream.BaseStream.Length - start) / 16;// from Address+ to address
            fileStream.BaseStream.Seek(start, SeekOrigin.Begin);
            int readSize = (int)(fileStream.BaseStream.Length - start);
            byte[] buff;
            if (m_compress)
            {
                byte[] cbuff = fileStream.ReadBytes(readSize);
                buff = new byte[readSize * 4];
                length = LZ_Uncompress(cbuff, buff, readSize) / 16;
            }
            else
                buff = fileStream.ReadBytes(readSize);
            // WIP
            //byte[] outbuff = new byte[readSize * 9 / 8];
            //byte[] outbuff2 = new byte[readSize];
            //ulong[] outbuff3 = new ulong[readSize / 8];
            //bool error = false;
            //int csize = (int)LZ_Compress(buff, outbuff, readSize);
            //int osize = (int)LZ_Uncompress(outbuff, outbuff2, csize);
            //int osize2 = (int)LZ_Uncompress2(outbuff,outbuff3, csize);
            //for (int i = 0; i < readSize; i++)
            //{
            //    if (buff[i] != outbuff2[i]) 
            //        error = true;
            //}
            //for (int i = 0; i < readSize/8; i++)
            //{
            //    if (outbuff3[i] !=(ulong) BitConverter.ToInt64(buff, i*8))
            //        error = true;
            //}
            // end test
            for (int index = 0; index < length; index++)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                long source = BitConverter.ToInt64(buff, (index * 2) << 3); //fileStream.ReadInt64();
                long target = BitConverter.ToInt64(buff, (index * 2 + 1) << 3); //fileStream.ReadInt64();
                MemoryType sourcetype = GetMemoryType(source);
                long startAddress = GetStartAddress(sourcetype);
                Address from = new Address(sourcetype, source - startAddress);
                Address to = new Address(MemoryType.HEAP, target - heapStartAddress);
                pointerInfo.AddPointer(from, to);
                if (index % 10000 == 1)
                    prog.Report((int)((long)100 * (index + 1) / length));
            }
            pointerInfo.MakeList();
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            //3.28.24
            return pointerInfo;
        }
        void IDumpDataReader.readsetup()
        {
            ReadIndicate();
        }
        long IDumpDataReader.mainStartAddress()
        {
            return mainStartAddress;
        }
        long IDumpDataReader.mainEndAddress()
        {
            return mainEndAddress;
        }
        long IDumpDataReader.heapStartAddress()
        {
            return heapStartAddress;
        }
        long IDumpDataReader.heapEndAddress()
        {
            return heapEndAddress;
        }
        long IDumpDataReader.TargetAddress()
        {
            return TargetAddress;
        }
        long IDumpDataReader.TryToParseAbs(List<IReverseOrderPath> path)
        {
            ReadIndicate();

            long address = mainStartAddress;
            for (int i = path.Count - 1; i >= 0; i--)
            {
                long data;
                if (readData.ContainsKey(address))
                {
                    data = readData[address];
                }
                else
                {
                    data = ReadLittleEndianInt64(address);
                    readData.Add(address, data);
                }
                address = path[i].ParseAddress(address, data);
                if ((address == 0) || !IsMainHeapAddress(address))
                {
                    return 0;
                }
            }
            return address;
        }
        Address IDumpDataReader.TryToParseRel(List<IReverseOrderPath> path)
        {
            long address = ((IDumpDataReader)this).TryToParseAbs(path);
            MemoryType type = GetMemoryType(address);
            return new Address(type, address - GetStartAddress(type));
        }
        bool IDumpDataReader.IsHeap(long address)
        {
            return IsHeapAddress(address);
        }
        private bool IsMainHeapAddress(long evalAddress)
        {
            if ((mainStartAddress <= evalAddress) && (evalAddress < mainEndAddress))
            {
                return true;
            }
            if ((heapStartAddress <= evalAddress) && (evalAddress < heapEndAddress))
            {
                return true;
            }
            return false;
        }
        private bool IsHeapAddress(long evalAddress)
        {
            if ((heapStartAddress <= evalAddress) && (evalAddress < heapEndAddress))
            {
                return true;
            }
            return false;
        }
        private MemoryType GetMemoryType(long address)
        {
            if ((mainStartAddress <= address) && (address < mainEndAddress))
            {
                return MemoryType.MAIN;
            }
            return MemoryType.HEAP;
        }
        private long GetStartAddress(MemoryType type)
        {
            if (type == MemoryType.MAIN)
            {
                return mainStartAddress;
            }
            return heapStartAddress;
        }
    }
}
