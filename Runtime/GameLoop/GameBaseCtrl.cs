using System;
using IG;

public class GameBaseCtrl : IGBC{
    public string GUID{ get; }

    protected GameBaseCtrl(){
        this.GUID = Guid.NewGuid().ToString();
        GameLooper.Instance.RegisterCtrl(this.GUID, this);
    }

    ~GameBaseCtrl(){ GameLooper.Instance.DeregisterCtrl(this.GUID); }
}