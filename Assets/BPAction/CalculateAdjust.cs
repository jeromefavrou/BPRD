using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CalculateAdjust : BPAction
{
    // Start is called before the first frame update
    public TMP_Dropdown ajustTypeDropdown;
    public Graph _graph;
    public GraphDisplay graphDisplay;
    public TMP_InputField parmam1;
    public TMP_InputField parmam2;
    public TMP_InputField parmam3;
    void Start()
    {
        ajustTypeDropdown.onValueChanged.AddListener(delegate
        {
            adjustChange();
        });

        //active param1 et param2
        parmam1.gameObject.SetActive(true);
        parmam2.gameObject.SetActive(true);

        parmam1.gameObject.name = "a";
        parmam2.gameObject.name = "b";

        parmam1.text = "";
        parmam2.text = "";
        parmam3.text = "";
        
    }

    protected override IEnumerator action()
    {

        if (isProcessing)
        {
            errManager.addWarning("Un ajustement est déjà en cours");
            yield break;
        }

        isProcessing = true;

        this.button.interactable = false;

        if (ajustTypeDropdown.value == 0)
        {
            linearSelected();
            
        }
        else if (ajustTypeDropdown.value == 1)
        {
            expSelected();
        }
        else if (ajustTypeDropdown.value == 2)
        {
            gaussienSelected();
        }
        else if (ajustTypeDropdown.value == 3)
        {
            spheriqueSelected();
        }
        else if (ajustTypeDropdown.value == 4)
        {
            sinusCardianlSelected();
        }

        else if (ajustTypeDropdown.value == 5)
        {
            if (_graph.getLPoints().regressData.regressParametres.h == 0)
            {
                errManager.addWarning("Ajustement non pris en charge ! fenetre h invalide :" + _graph.getLPoints().regressData.regressParametres.h.ToString());
                isProcessing = false;
                this.button.interactable = true;
                yield break;
            }
            kernelGaussSelected();
        }
        else
        {
            errManager.addWarning("Ajustement non pris en charge !");
            isProcessing = false;
            this.button.interactable = true;
            yield break;
        }



        //redessine le graph
        _graph.autoScale();
        _graph._barycentre = new Vector2(0, 0);

        StartCoroutine(_graph.drawGraph());

        while (progressBarre.ProcessingCheck())
        {
            yield return new WaitForSeconds(0.01f);
        }
        _graph.drawEstimateCurve();

        //ressauvegarde le graph
        graphDisplay.saveCurrent(GraphDisplay.IndexCurve.SemiVario);


        graphDisplay.selectChange();


        isProcessing = false;
        this.button.interactable = true;
    }


    private void linearSelected()
    {
        if (parmam1.text == "" || parmam2.text == "")
        {
            _graph.getLPoints().linearRegression();

            parmam1.text = _graph.getLPoints().regressData.regressParametres.a.ToString("F4");
            parmam2.text = _graph.getLPoints().regressData.regressParametres.b.ToString("F4");
        }
        else
        {
            if (_graph.getLPoints().regressData == null)
            {
                _graph.getLPoints().regressData = new ListPoint.RegressData();
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }
            if (_graph.getLPoints().regressData.regressParametres == null)
            {
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }

            _graph.getLPoints().regressData.regressParametres.a = float.Parse(parmam1.text);
            _graph.getLPoints().regressData.regressParametres.b = float.Parse(parmam2.text);
            _graph.getLPoints().regressData.estimateur = _graph.getLPoints().linearEstimate;

        }

        
        _graph.getLPoints().getRPearson();
        _graph.getLPoints().getRSquare();
        _graph.getLPoints().getStandardRMSE();
        _graph.getLPoints().getXYRMSE();
    }

    private void kernelGaussSelected()
    {
        if (parmam1.text == "" || parmam2.text == "" )
        {
            
            _graph.getLPoints().regressKernelGaussien();

            parmam1.text = _graph.getLPoints().regressData.regressParametres.nugget.ToString("F4");
            parmam2.text = _graph.getLPoints().regressData.regressParametres.h.ToString("F4");

        }
        else
        {
            if (_graph.getLPoints().regressData == null)
            {
                _graph.getLPoints().regressData = new ListPoint.RegressData();
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }
            if (_graph.getLPoints().regressData.regressParametres == null)
            {
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }

            _graph.getLPoints().regressData.regressParametres.nugget = float.Parse(parmam1.text);
            _graph.getLPoints().regressData.regressParametres.h = float.Parse(parmam2.text);
            _graph.getLPoints().regressData.estimateur = _graph.getLPoints().kernelGaussianEstimate;

        }

        _graph.getLPoints().getRSquare();
    }

    private void sinusCardianlSelected()
    {
        if (parmam1.text == "" || parmam2.text == "" || parmam3.text == "")
        {
            _graph.getLPoints().regressSinusCardinal();

            parmam1.text = _graph.getLPoints().regressData.regressParametres.nugget.ToString("F4");
            parmam2.text = _graph.getLPoints().regressData.regressParametres.a.ToString("F4");
            parmam3.text = _graph.getLPoints().regressData.regressParametres.b.ToString("F4");
        }
        else
        {
            if (_graph.getLPoints().regressData == null)
            {
                _graph.getLPoints().regressData = new ListPoint.RegressData();
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }
            if (_graph.getLPoints().regressData.regressParametres == null)
            {
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }

            _graph.getLPoints().regressData.regressParametres.nugget = float.Parse(parmam1.text);
            _graph.getLPoints().regressData.regressParametres.a = float.Parse(parmam2.text);
            _graph.getLPoints().regressData.regressParametres.b = float.Parse(parmam3.text);
            _graph.getLPoints().regressData.estimateur = _graph.getLPoints().sinusCardianlEstimate;

        }

        _graph.getLPoints().getRSquare();
    }

    private void spheriqueSelected()
    {
        if (parmam1.text == "" || parmam2.text == "" || parmam3.text == "")
        {
            _graph.getLPoints().regressSpherique();

            parmam1.text = _graph.getLPoints().regressData.regressParametres.nugget.ToString("F4");
            parmam2.text = _graph.getLPoints().regressData.regressParametres.a.ToString("F4");
            parmam3.text = _graph.getLPoints().regressData.regressParametres.b.ToString("F4");
        }
        else
        {
            if (_graph.getLPoints().regressData == null)
            {
                _graph.getLPoints().regressData = new ListPoint.RegressData();
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }
            if (_graph.getLPoints().regressData.regressParametres == null)
            {
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }

            _graph.getLPoints().regressData.regressParametres.nugget = float.Parse(parmam1.text);
            _graph.getLPoints().regressData.regressParametres.a = float.Parse(parmam2.text);
            _graph.getLPoints().regressData.regressParametres.b = float.Parse(parmam3.text);
            _graph.getLPoints().regressData.estimateur = _graph.getLPoints().spheriqueEstimate;

        }


        _graph.getLPoints().getRSquare();
    }

    private void gaussienSelected()
    {
        if (parmam1.text == "" || parmam2.text == "" || parmam3.text == "")
        {
            _graph.getLPoints().regressGaussien();

            parmam1.text = _graph.getLPoints().regressData.regressParametres.nugget.ToString("F4");
            parmam2.text = _graph.getLPoints().regressData.regressParametres.a.ToString("F4");
            parmam3.text = _graph.getLPoints().regressData.regressParametres.b.ToString("F4");
        }
        else
        {
            if (_graph.getLPoints().regressData == null)
            {
                _graph.getLPoints().regressData = new ListPoint.RegressData();
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }
            if (_graph.getLPoints().regressData.regressParametres == null)
            {
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }

            _graph.getLPoints().regressData.regressParametres.nugget = float.Parse(parmam1.text);
            _graph.getLPoints().regressData.regressParametres.a = float.Parse(parmam2.text);
            _graph.getLPoints().regressData.regressParametres.b = float.Parse(parmam3.text);
            _graph.getLPoints().regressData.estimateur = _graph.getLPoints().gaussianEstimate;

        }
        _graph.getLPoints().getRSquare();
    }

    private void expSelected()
    {
        if (parmam1.text == "" || parmam2.text == "" || parmam3.text == "")
        {
            _graph.getLPoints().regressExp();

            parmam1.text = _graph.getLPoints().regressData.regressParametres.nugget.ToString("F4");
            parmam2.text = _graph.getLPoints().regressData.regressParametres.a.ToString("F4");
            parmam3.text = _graph.getLPoints().regressData.regressParametres.b.ToString("F4");
        }
        else
        {
            if (_graph.getLPoints().regressData == null)
            {
                _graph.getLPoints().regressData = new ListPoint.RegressData();
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }
            if (_graph.getLPoints().regressData.regressParametres == null)
            {
                _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
            }

            _graph.getLPoints().regressData.regressParametres.nugget = float.Parse(parmam1.text);
            _graph.getLPoints().regressData.regressParametres.a = float.Parse(parmam2.text);
            _graph.getLPoints().regressData.regressParametres.b = float.Parse(parmam3.text);
            _graph.getLPoints().regressData.estimateur = _graph.getLPoints().expEstimate;

        }

        _graph.getLPoints().getRSquare();
    }

    private void adjustChange()
    {
        
        if( _graph.getLPoints().regressData == null)
        {
            _graph.getLPoints().regressData = new ListPoint.RegressData();
            _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
        }
        if (_graph.getLPoints().regressData.regressParametres == null)
        {
            _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
        }

        if (ajustTypeDropdown.value == 0)
        {
            parmam1.gameObject.SetActive(true);
            parmam2.gameObject.SetActive(true);
            parmam3.gameObject.SetActive(false);

            parmam1.gameObject.name = "a";
            parmam2.gameObject.name = "b";



        }

        else if (ajustTypeDropdown.value == 1 || ajustTypeDropdown.value == 2 || ajustTypeDropdown.value == 3 || ajustTypeDropdown.value == 4)
        {
            parmam1.gameObject.SetActive(true);
            parmam2.gameObject.SetActive(true);
            parmam3.gameObject.SetActive(true);

            parmam1.gameObject.name = "Nugget";
            parmam2.gameObject.name = "a";
            parmam3.gameObject.name = "b";



        }

        else if (ajustTypeDropdown.value == 5)
        {
            parmam1.gameObject.SetActive(true);
            parmam2.gameObject.SetActive(true);
            parmam3.gameObject.SetActive(false);

            parmam1.gameObject.name = "Nugget";
            parmam2.gameObject.name = "h";



        }

        parmam1.text = "";
        parmam2.text = "";
        parmam3.text = "";

        parmam1.GetComponent<TitleAuto>().UpdateTitle();
        parmam2.GetComponent<TitleAuto>().UpdateTitle();
        parmam3.GetComponent<TitleAuto>().UpdateTitle();
    }

}
