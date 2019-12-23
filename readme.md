# Pointer Searcher
## warning
* this tool is alpha version,and may has many bugs.please use at yourown risk  
* this tool uses huge amout of memory,maybe more than 400 MB.
* each process is very slow.  
* I'm not going to fix bug,improve memory usage and calculating speed,enhancement.  
so don't request to me.  

## how to use
### preparing
1. dump all r/w memory by using Noexs and search non static address(target address you want to make a pointer code).
1. memo main start address,main end address,heap start address, heap end address and target address.  
it's better to restart game,and dump another data and memo to narrow down search results.

### search pointer
1. launch pointer searcher  
1. fill out 1st row  
1. select `Read 1st Dump Data` button  
this process will takes few minutes  
`Reset and Search` button will be enabled when process finished  
1. After setting search option below,select `Read 1st Dump Data` button  
Max Depth : Max pointer depth  
Offset Range : Search range from target/pointer located address  
Offset Num : Search num of nearest pointed address from target/pointer located address  
For example,If Offset Num=1,you will find`[[main+B000]+10]+100` in the case below  
And if Offset Num=2,you will find`[[main+A000]+20]+200` in addition  
![num](https://user-images.githubusercontent.com/59052622/71303971-1b50bf80-2403-11ea-87f0-478df77e75c7.png)  
calculating cost will be O(N<sup>N*D</sup>) (D:Max Depth,N:Offset Num)  
1. If too many results,get another dump data and fill out 2nd and subsequent row,select `Narrow Down Result`  
If narrow down results with target address 0,tool only checks if pointer code can reaches heap region  
If narrow down results with target address not 0,tool checks if pointer code can reaches target address  

# link
* tutorial(Japanese)  
https://zit866.hatenablog.com/entry/2019/12/17/012933

# credit
* Matthew Bell : auther of great tool ,Noexes.
* ZiT866 : thanks to making tutorial.

# change log
*v0.04*  
[improve]change offset expression at least 2 digits to avoid noexs exception  
[improve]support narrow down results with target address 0 to only checks if pointer code can reaches heap region  
[improve]narrowing down results become faster  

*v0.03*  
[bugfix]actual search depth was MaxDepth+1 by mistake  
[improve]change UI to show progress  

*v0.02*  
[bugfix]can't find pointer that includes +00h offset  
[improve]1st read become faster  

*v0.01*  
first release
