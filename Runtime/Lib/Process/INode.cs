namespace IG.Runtime.Process{
    public delegate ProcessStatus ProgressHandle();

    public interface INode{
        int    InsID{ get; }
        ProcessStatus Execute();
    }
}