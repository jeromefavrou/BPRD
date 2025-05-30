using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SelectViewMange : MonoBehaviour
{
    private TMP_Dropdown viewDropdown;
    // Start is called before the first frame update
    public GameObject processView;
    public GameObject graphView;
    public GameObject parameterView;



        private const int Index_process = 0;
        private const int  Index_view3d = 1;
        private const int  Index_Graph = 2;
        private const int  Index_parameter = 3;


    void Start()
    {
        viewDropdown = GetComponent<TMP_Dropdown>();
        if (viewDropdown == null)
        {
            Debug.LogError("TMP_Dropdown component is not assigned!");
            return;
        }

        viewDropdown.onValueChanged.AddListener(delegate { change(); });

    }



    private void change()
    {
        int selectedIndex = viewDropdown.value;

        allHide();

        switch (selectedIndex)
        {
            case Index_process:
                this.processView.SetActive(true);
                // Add logic to switch to process view
                break;
            case Index_view3d:
                //just all hide();
                break;
            case Index_Graph:
                this.graphView.SetActive(true);
                // Add logic to switch to Graph view
                break;
            case Index_parameter:
                this.parameterView.SetActive(true);
                // Add logic to switch to Parameter view
                break;
            default:
                Debug.LogWarning("Unknown view selected");
                break;
        }
    }

    private void allHide()
    {
        processView.SetActive(false);
        graphView.SetActive(false);
        parameterView.SetActive(false);

    }
}
