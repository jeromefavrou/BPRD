using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ScaleZ : MonoBehaviour
{
    // Start is called before the first 
    private Scrollbar scaleZ;

    public GameObject _main;

    public  TMP_Text value;
    
    void Start()
    {
        scaleZ = GetComponent<Scrollbar>();


        scaleZ.value = 0.0f;

        value.text = "1,0";


    }

    // Update is called once per frame
    void OnGUI()
    {
        float val = 1+scaleZ.value * 9;

        //arrondire au dixième
        val = Mathf.Round(val * 10) / 10;
        
         _main.transform.localScale = new Vector3(1, val, 1);

        value.text = val.ToString("0.0");

    }
}
