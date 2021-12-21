using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Gmcon : MonoBehaviour
{
    public bool TMP = true;

    public Stopwatch rsp;
    // Start is called before the first frame update
    void Start()
    {
        rsp = new Stopwatch();
        Application.targetFrameRate = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Onclick()
    {
        if (TMP)
        {
            rsp.Start();
        }
        else
        {
            rsp.Stop();
            Debug.Log(rsp.ElapsedMilliseconds);
            rsp.Reset();
        }
        TMP = !TMP;
    }
}
