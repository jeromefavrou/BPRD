using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CalculateAdjust  : BPAction
{
    // Start is called before the first frame update
    public TMP_Dropdown ajustTypeDropdown;
    public Graph _graph;
    public GraphDisplay graphDisplay;
    void Start()
    {
        
    }

    protected override IEnumerator action()
    {

        if (isProcessing)
        {
            errManager.addWarning("Un prétraitement est déjà en cours");
            yield break;
        }

        isProcessing = true;

        this.button.interactable = false;

        if( ajustTypeDropdown.value == 0)
        {
            linearSelected();
        }
        else if( ajustTypeDropdown.value == 1)
        {
            expSelected();
        }
        else if( ajustTypeDropdown.value == 2)
        {
            gaussienSelected();
        }
        else if( ajustTypeDropdown.value == 3)
        {
            spheriqueSelected();
        }
        else if( ajustTypeDropdown.value == 4)
        {
            sinusCardianlSelected();
        }

        else if( ajustTypeDropdown.value == 5)
        {
            if( _graph.getLPoints().regressData.regressParametres.h == 0)
            {
                errManager.addWarning("Ajustement non pris en charge ! fenetre h invalide :" + _graph.getLPoints().regressData.regressParametres.h.ToString() );
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

        StartCoroutine( _graph.drawGraph()) ;

        while( progressBarre.ProcessingCheck() )
        {
            yield return new WaitForSeconds(0.01f);
        }
        _graph.drawEstimateCurve();

        //ressauvegarde le graph
        graphDisplay.saveCurrent( 1 );
        graphDisplay.courbeType.value = 1;

        graphDisplay.selectChange();


        isProcessing = false;
        this.button.interactable = true;
    }


    private void linearSelected()
    {
        _graph.getLPoints().linearRegression();
        _graph.getLPoints().getRPearson();
        _graph.getLPoints().getRSquare();
        _graph.getLPoints().getStandardRMSE();
        _graph.getLPoints().getXYRMSE();
    }

    private void kernelGaussSelected()
    {
        _graph.getLPoints().regressKernelGaussien();
        _graph.getLPoints().getRSquare();
    }

    private void sinusCardianlSelected()
    {
        _graph.getLPoints().regressSinusCardinal();
        _graph.getLPoints().getRSquare();
    }

    private void spheriqueSelected()
    {
        _graph.getLPoints().regressSpherique();
        _graph.getLPoints().getRSquare();
    }

    private void gaussienSelected()
    {
        _graph.getLPoints().regressGaussien();
        _graph.getLPoints().getRSquare();
    }

    private void expSelected()
    {
        _graph.getLPoints().regressExp();
        _graph.getLPoints().getRSquare();
    }

}
