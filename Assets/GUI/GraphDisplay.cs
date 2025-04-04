using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GraphDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public Graph _graph;
    private Button button;
    public TMP_Dropdown courbeType;
    public TMP_Text npoint;
    bool stat = false;

    public TMP_Text eqRegress;
    public TMP_Text RMSE;
    public TMP_Text RMSEXY;
    public TMP_Text rSquare;
    public TMP_Text rPearson;

    public struct SaveGraph
    {
        public Texture2D rawImage;
        public ListPoint points;
    }

    private SaveGraph[] saveGraph = new SaveGraph[3];

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        courbeType.onValueChanged.AddListener(delegate {selectChange(); });

        npoint.text = "0";
        for(int i = 0; i < 3; i++)
        {
            saveGraph[i].points = new ListPoint();
            saveGraph[i].rawImage = new Texture2D(1, 1);
        }
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClick()
    {
        stat =!stat;
        _graph.gameObject.SetActive(stat);

    }

    public void selectChange()
    {
        _graph.clear();
        _graph.setLPoints(saveGraph[courbeType.value].points);
        _graph.setRawImage(saveGraph[courbeType.value].rawImage);
        _graph.drawAxesValue();

        npoint.text = saveGraph[courbeType.value].points.getListPoint().Count.ToString();

        if( saveGraph[courbeType.value].points.regressData != null)
        {
            if( saveGraph[courbeType.value].points.regressData.regressParametres != null)
            {
                
               //affichage de l'equation lineaire
                float a = (float)saveGraph[courbeType.value].points.regressData.regressParametres.a;
                float b = (float)saveGraph[courbeType.value].points.regressData.regressParametres.b;

                //arrrondire a 4 chiffre a pres la virgule
                a = Mathf.Round(a * 10000) / 10000;
                b = Mathf.Round(b * 10000) / 10000;

                eqRegress.text = "y = "+a.ToString() + "x + " + b.ToString();


                //affichage de l'erreur quadratique moyenne
                float rmse = (float)saveGraph[courbeType.value].points.regressData.StdRMSE;
                rmse = Mathf.Round(rmse * 10000) / 10000;
                RMSE.text =  rmse.ToString();

                //affichage du coefficient de determination
                float rsquare = (float)saveGraph[courbeType.value].points.regressData.rSquare;
                rsquare = Mathf.Round(rsquare * 10000) / 10000;
                rSquare.text =  rsquare.ToString();

                //affichage du coefficient de correlation de pearson
                float rpearson = (float)saveGraph[courbeType.value].points.regressData.rPearson;
                rpearson = Mathf.Round(rpearson * 10000) / 10000;
                rPearson.text =  rpearson.ToString();
            }
            else
            {
                eqRegress.text = "N/A";
                RMSE.text = "N/A";
                rSquare.text = "N/A";
                rPearson.text = "N/A";
            }
            //affiche de l'erreur quadratique moyenne en x et y
            float rmsexy = (float)saveGraph[courbeType.value].points.regressData.XYRMSE;
            rmsexy = Mathf.Round(rmsexy * 10000) / 10000;
            RMSEXY.text =  rmsexy.ToString();

        }
        else
        {
            eqRegress.text = "N/A";
            RMSE.text = "N/A";
            RMSEXY.text = "N/A";
            rSquare.text = "N/A";
            rPearson.text = "N/A";
        }
    }
   

    public void saveCurrent( uint n )
    {
        saveGraph[n].rawImage = _graph.getRawImage();
        saveGraph[n].points = new ListPoint( _graph.getLPoints() );
    }


}
