using System;
using IG;

public class GameBaseCtrl : IGBC{
    public string GUID{ get; }

    protected GameBaseCtrl(){
        this.GUID = Guid.NewGuid().ToString();
        GameLooper.Instance.RegisterCtrl(this.GUID, this);
    }

    ~GameBaseCtrl(){
        GameLooper.Instance.DeregisterCtrl(this.GUID);
    }

    public virtual bool Init(float deltaTime){
        return true;
    }

    public virtual bool FrameTick(float deltaTime){
        return true;
    }

    public virtual bool Tick(float deltaTime){
        return true;
    }

    public virtual bool FixedIntervalTick(float deltaTime){
        return true;
    }
    
    public virtual bool FixedTick(float deltaTime){
        return true;
    }

    public virtual bool LateTick(float deltaTime){
        return true;
    }

    public bool AsyncTick(float deltaTime){
        return true;
    }

    public virtual bool OnDestroy(){
        return true;
    }
}

