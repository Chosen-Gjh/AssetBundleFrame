using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SyncColor : MonoBehaviour
{
    private Image img;

    private Material mat;
    // Start is called before the first frame update
    void Start()
    {

        img = this.transform.GetComponent<Image>();
        this.mat = this.transform.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        this.img.color = this.mat.GetColor("_Color");
    }
}
