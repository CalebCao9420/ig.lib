using System;
using System.Collections;
using System.Collections.Generic;
using IG;
using UnityEngine;

public class SingletonMonoTest : SingletonMono<SingletonMonoTest>{
    // Start is called before the first frame update
    void Start(){ }

    // Update is called once per frame
    void                 Update()   { }
    public override void OnDispose(){ Debug.Log("[Print][Log]"); }
}