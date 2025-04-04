using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;
public class ProgressBarre : MonoBehaviour
{
    // Start is called before the first frame update

    private Slider progressBar;
    public TMP_Text progressText;

    public TMP_Text value;

    public TMP_Text timeElapsed;
    public TMP_Text timeRemaining;

    private float timeElapsedValue = 0;
    Stopwatch stopwatch = new Stopwatch();
    private bool isProcessing = false;

    uint yieldModulot =1;
    uint maxCount = 0;
    void Awake()
    {
        progressBar = GetComponent<Slider>();
    }
    void Start()
    {
        progressBar.value = 0;
    }

    public bool validUpdate(uint n)
    {   bool res = isProcessing && n % yieldModulot == 0;
        if( res)
        {
            setValue((float)n / (float)maxCount);
        }

        return res;
    }

    public bool ProcessingCheck()
    {
        return isProcessing;
    }

    public void start( uint n , float pourcent = 0.05f)
    {
        maxCount = n-1;
        yieldModulot = (uint)(float)(n*pourcent);
        if( yieldModulot == 0)
        {
            yieldModulot = 1;
        }

        progressBar.value = 0;
        timeElapsedValue = 0;

        isProcessing = true;
        stopwatch = new Stopwatch();
        stopwatch.Start();
        timeElapsed.text = "0.0s";
        timeRemaining.text = "0.0s";

    }

    public void stop()
    {
        progressBar.value = 1;

        isProcessing = false;
        stopwatch.Stop();

        timeRemaining.text = "0.0s";

    }
    public void setAction(string action)
    {
        progressText.text = action;
    }


    public void setValue(float progress)
    {
        progressBar.value = progress;
        float val = progressBar.value * 100;
        value.text = val.ToString("0.0") + "%";


        if( isProcessing)
        {
                //calculer le temps écoulé
            timeElapsedValue = (float)stopwatch.Elapsed.TotalSeconds;
            
            timeElapsed.text = timeElapsedValue.ToString("0.0") + "s";
            //calculer le temps restant
            if(progressBar.value> 0 )
            {
                    float timeRemainingValue = (1- progressBar.value) *   timeElapsedValue / progressBar.value;
                    timeRemaining.text = timeRemainingValue.ToString("0.0") + "s";
            } 
            else
            {
                timeRemaining.text = "0.0s";
            }

        }
    }

    // Update is called once per frame
    void OnGUI()
    {
        float val = progressBar.value * 100;
        value.text = val.ToString("0.0") + "%";


        //placer la barre en bas a droite
        RectTransform rt = progressBar.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(Screen.width / 2 - rt.sizeDelta.x/2 -140 , -Screen.height / 2 + 70);

        

    }
}
