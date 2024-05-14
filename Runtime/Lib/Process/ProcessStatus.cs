namespace IG.Runtime.Process{
    public enum ProcessStatus{
        None       = 0,   //未启动
        Executing  = 1,   //执行中
        Successful = 200, //成功
        Failed     = 400, //失败
    }
}