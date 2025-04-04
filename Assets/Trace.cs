using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class Trace : MonoBehaviour
{
    // Start is called before the first frame update

    private Toggle traceToggle;
    void Start()
    {
        traceToggle = GetComponent<Toggle>();
        traceToggle.onValueChanged.AddListener(delegate {
            OnTriggerOnOff();
        });
    }

    //si chegemtn d'etat
    public void OnTriggerOnOff()
    {

        //debug
        Debug.Log("Trace : " + traceToggle.isOn);
        if (traceToggle.isOn)
        {
            //_main.image2d.printTrace(_main.selectData, _main.min);
        }
        else
        {
            //_main.image2d.hideTrace(_main.selectData, _main.min,  _main.max);
        }
    }
}
