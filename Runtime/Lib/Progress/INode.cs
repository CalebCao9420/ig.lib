namespace IG.Runtime.Progress{

    public delegate ProgressState ProgressHandle();
    
    public interface INode{
        ProgressState Execute();
    }
}