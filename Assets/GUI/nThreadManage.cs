using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class nThreadManage : MonoBehaviour
{
    // Start is called before the first frame update

    private TMP_InputField input;
    void Start()
    {
        input = this.GetComponent<TMP_InputField>();

        if (input == null)
        {
            Debug.LogError("TMP_InputField component not found on this GameObject.");
            return;
        }

        int max = Environment.ProcessorCount - 1;
        //set la valeur
        input.text = max.ToString();
        
    }

    // Update is called once per frame
    void OnGUI()
    { 
        //si selectionné
        if (!input.isFocused)
        {
            int max = Environment.ProcessorCount - 1;
            if(input.text.Length == 0)
            {
                input.text = max.ToString();
            }
            else
            {
                try
                {
                    int value = int.Parse(input.text);
                    if (value > max)
                    {
                        input.text = max.ToString();
                    }
                    else if (value < 1)
                    {
                        input.text = "1";
                    }
                }
                catch (FormatException)
                {
                    Debug.LogError("Invalid input format. Please enter a valid number.");
                    input.text = max.ToString();
                }
                
            }  
        }

        
        
    }
    
}
