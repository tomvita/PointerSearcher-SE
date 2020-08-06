using System;
using System.Collections.Generic;
using System.Threading;

namespace PointerSearcher
{
    internal interface IDumpDataReader
    {
        PointerInfo Read(CancellationToken token, IProgress<int> prog);
        long TryToParseAbs(List<IReverseOrderPath> path);
        Address TryToParseRel(List<IReverseOrderPath> path);
        bool IsHeap(long address);
        long mainStartAddress();
        long mainEndAddress();
        long heapStartAddress();
        long heapEndAddress();
        long TargetAddress();
        void readsetup();
    }
}
