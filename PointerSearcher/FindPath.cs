using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PointerSearcher
{
    abstract class IReverseOrderPath
    {
        public IReverseOrderPath()
        {
        }
        public abstract String ToString(String org);
        public abstract long ParseAddress(long currentAddress, long currentData);
    }
    class ReverseOrderPathOffset : IReverseOrderPath
    {
        public ReverseOrderPathOffset(long offsetAddress)
        {
            offset = offsetAddress;
        }
        private long offset;

        public long getOffset()
        {
            return offset;
        }

        public override String ToString(String org)
        {
            return org + "+" + offset.ToString("X2");
        }
        public override long ParseAddress(long currentAddress, long currentData)
        {
            return currentAddress + offset;
        }
    }
    class ReverseOrderPathPointerJump : IReverseOrderPath
    {
        public override String ToString(String org)
        {
            return "[" + org + "]";
        }
        public override long ParseAddress(long currentAddress, long currentData)
        {
            return currentData;
        }
    }
    class FindPath
    {
        public FindPath(int offsetNum, long offsetAddress)
        {
            maxOffsetNum = offsetNum;
            maxOffsetAddress = offsetAddress;
        }
        public int maxOffsetNum { get; set; }
        public long maxOffsetAddress { get; set; }
        public void Search(CancellationToken token, IProgress<double> prog, double progAddValue, PointerInfo info, int depth, List<IReverseOrderPath> path, Address current, List<List<IReverseOrderPath>> result)
        {
            IComparable<Address> icurrent = current;
            int nearest_index = info.FindNearest(icurrent);
            int currentOffsetNum = 1;

            const double reportMin = 0.02;
            double progAddEach = progAddValue / maxOffsetNum;
            if (progAddEach < reportMin)
            {
                progAddEach = 0;
            }

            for (int i = nearest_index; i > nearest_index - maxOffsetNum; i--)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                }
                if (i < 0)
                {
                    if (progAddEach != 0) { prog.Report(progAddEach); }
                    continue;
                }
                Address nearest = info.pointedList[i].addr;
                long offset = current.OffsetFrom(nearest);
                if (IsLoop(path, nearest) || (offset >= maxOffsetAddress))
                {
                    if (progAddEach != 0) { prog.Report(progAddEach); }
                    continue;
                }
                if (offset > 0)
                {
                    ReverseOrderPathOffset add = new ReverseOrderPathOffset(offset);
                    path.Add(add);
                }

                double progAddNest = progAddEach / info.pointedList[i].pointedfrom.Count;
                if (progAddNest < reportMin)
                {
                    progAddNest = 0;
                }

                foreach (Address next in info.pointedList[i].pointedfrom)
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    if (IsLoop(path, next))
                    {
                        if (progAddNest != 0) { prog.Report(progAddNest); }

                        continue;
                    }
                    ReverseOrderPathPointerJump add = new ReverseOrderPathPointerJump();
                    path.Add(add);

                    if (next.type == MemoryType.MAIN)
                    {
                        ReverseOrderPathOffset frommain = new ReverseOrderPathOffset(next.offset);
                        path.Add(frommain);
                        result.Add(new List<IReverseOrderPath>(path));
                        path.RemoveAt(path.Count - 1);
                        if (progAddNest != 0) { prog.Report(progAddNest); }
                    }
                    else
                    {
                        if (depth > 1)
                        {
                            Search(token, prog, progAddNest, info, depth - 1, path, next, result);
                        }
                        else
                        {
                            if (progAddNest != 0) { prog.Report(progAddNest); }
                        }
                    }
                    path.RemoveAt(path.Count - 1);
                }
                if ((progAddEach != 0) && (progAddNest == 0)) { prog.Report(progAddEach); }
                if (offset > 0)
                {
                    path.RemoveAt(path.Count - 1);
                }
                currentOffsetNum++;
            }
            if ((progAddEach == 0) && (progAddValue != 0)) { prog.Report(progAddValue); }
        }
        private bool IsLoop(List<IReverseOrderPath> path, Address checkAddress)
        {
            /*IComparable<Address> iaddress = checkAddress;
            foreach (IReverseOrderPath x in path)
            {
                if (iaddress.CompareTo(x.addrMemo[0]) == 0)
                {
                    return true;
                }
            }*/
            return false;
        }

        static public async Task<List<List<IReverseOrderPath>>> NarrowDown(CancellationToken token, IProgress<int> prog, List<List<IReverseOrderPath>> list, Dictionary<IDumpDataReader, long> dumps)
        {
            int totalCount = list.Count;
            int checkedCount = 0;
            int reportMin = 5;//report by 5%
            int reportCount = (totalCount + 100 / reportMin - 1) / (100 / reportMin); //report every this count of path checked

            reportCount = reportCount < 100 ? reportCount : 100;    //at least report every 100 times

            List<List<IReverseOrderPath>> ndlist = new List<List<IReverseOrderPath>>(list);
            for (int i = 0; i < ndlist.Count; i++)
            {
                List<IReverseOrderPath> path = ndlist[i];
                foreach (IDumpDataReader dump in dumps.Keys)
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    long parseAddress = await Task.Run(() => dump.TryToParseAbs(path));
                    long targetAddress = dumps[dump];
                    bool remove = false;
                    if (targetAddress == 0)
                    {
                        //if target address is 0,only check path is valid,can reach heap region
                        if (!dump.IsHeap(parseAddress))
                        {
                            remove = true;
                        }
                    }
                    else
                    {
                        //if target address isn't 0,check if parsed address is equal to target address
                        if (parseAddress != targetAddress)
                        {
                            remove = true;
                        }
                    }
                    if (remove)
                    {
                        ndlist.Remove(path);
                        i--;
                        break;
                    }
                }
                checkedCount++;
                if ((checkedCount % reportCount) == 0)
                {
                    prog.Report(100 * checkedCount / totalCount);
                }
            }
            prog.Report(100);
            return ndlist;
        }
    }
}
