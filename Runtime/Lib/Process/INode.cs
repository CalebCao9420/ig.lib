namespace IG.Runtime.Process{

    public delegate ProcessStatus ProgressHandle();
    
    public interface INode{
        ProcessStatus Execute();
    }
}