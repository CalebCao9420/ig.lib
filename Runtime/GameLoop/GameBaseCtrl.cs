using System;
using IG;

public class GameBaseCtrl : IGBC{
    public string GUID{ get; protected set; }

    protected GameBaseCtrl(){
        this.GUID = Guid.NewGuid().ToString();
        GameLooper.Instance.RegisterCtrl(this.GUID, this);
    }

    ~GameBaseCtrl(){
        if (GameLooper.IsValid){ GameLooper.Instance.DeregisterCtrl(this.GUID); }
    }
}