using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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

            if (fileStream.ReadInt32() != 0x4E5A4445) //edizon's version
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
            byte[] buff = fileStream.ReadBytes(readSize);
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
                    prog.Report((int)(100 * (index + 1) / length));
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
