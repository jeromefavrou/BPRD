using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Error : MonoBehaviour
{
    // Start is called before the first frame update

    private TMP_Text lastError;
    void Awake()
    {
        lastError = GetComponent<TMP_Text>();

        if( lastError == null)
        {
            Debug.LogError("Error non assigné !");
            return;
        }
    }

    void Start()
    {
        if( !guard() )
            return;
        
        lastError.text = "Error: Tout vas bien !";
        lastError.color = Color.green;
    }

    public void addLog(string log)
    {
        if( !guard() )
            return;
        //Debug.Log(log);
        lastError.text = "log : " + log;

        //met la couleur en vert
        lastError.color = Color.green;

        Debug.Log(lastError.text);
    }

    public void addError(string error)
    {
        if( !guard() )
            return;


        lastError.text = "Error : " + error;

        //met la couleur en rouge
        lastError.color = Color.red;

        Debug.LogError(lastError.text);
    }

    public void addWarning(string warning)
    {
        if( !guard() )
            return;

        lastError.text = "Warning : " + warning;

        //met la couleur en jaune  
        lastError.color = Color.yellow;

        Debug.LogWarning(lastError.text);
    }

    private bool guard()
    {
        return lastError == null ? false : true;
    }

    // Update is called once per frame
    void OnGUI()
    {
        
    }
}
