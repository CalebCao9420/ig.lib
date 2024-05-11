namespace IG.Runtime.Progress{

    public delegate ProcessStatus ProgressHandle();
    
    public interface INode{
        ProcessStatus Execute();
    }
}