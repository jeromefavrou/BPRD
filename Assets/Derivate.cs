using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Derivate : BPAction
{
    public Button exportBp;
    public TMP_Dropdown methode;
    public ImageSelector imageSelector;
    public Pretraiter _preprocess;
    private List<BathyPoint> dataAdv;


    void Start()
    {
        if( methode == null || gen_data == null)
        {
            Debug.LogError(" non assigné !");
            return;
        }


        exportBp.onClick.AddListener(delegate { BathyGraphie2D.csvSave(dataAdv , gen_data.workingPath + "/pp_d_data.csv" , gen_data.separator); });
    }

    void OnGUI()
    {
        if (gen_data.it_data == null)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }

        //position par rapport au haut droit
        RectTransform rt = GetComponent<RectTransform>();
        RectTransform rt_back = bckObj.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(1, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(1, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancres

        //screen size
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        rt.anchoredPosition = new Vector2( -rt.sizeDelta.x/2 - screenSize.x * 0.15f , rt_back.anchoredPosition.y - Screen.height*0.1f);
    }

    private double diferenceFini(double dx , double dy )
    {
        return Mathd.Sqrt(dx * dx + dy * dy) ;
    }

    private double maxDif( double dx , double dy )
    {
        return Mathd.Abs(dx > dy ? dx : dy);
    }

    private double sobel(double z00 , double z01 , double z02 , double z10  , double z12 , double z20 , double z21 , double z22)
    {
        double dx = (-z00 + z02) + (-2 * z10 + 2 * z12) + (z20 - z22);// (-z00 + z02) + (-2*z10 +2 * z12) + (z20 + z22);
        double dy = (-z00 - 2 * z01 - z02) + (z20 + 2 * z21 + z22);//( -z00 + z10) + (-2*z01 + 2 * z21) + (z02 + z12);
        return Mathd.Sqrt(dx * dx + dy * dy);
    }

    private double prewitt(double z00 , double z01 , double z02 , double z10  , double z12 , double z20 , double z21 , double z22)
    {
        double dx = (-z00 + z02) + (-z10 + z12) + (-z20 + z22);
        double dy = (-z00 - z01 - z02) + (z20 + z21 + z22);
        return Mathd.Sqrt(dx * dx + dy * dy);
    }

    private double laplace(double z11  , double z01  , double z10  , double z12  , double z21 )
    {
        return -4 * z11 + z01 + z10 + z12  + z21 ;
    }

    

    protected override IEnumerator action()
    {
        if (isProcessing)
        {
            errManager.addWarning("Derivation deja en cours");
            yield break;
        }

        isProcessing = true;

        exportBp.gameObject.SetActive(false);
        fwdObj.SetActive(false);

        gen_data.map2dGrad = new Texture2D(gen_data.it_data.size.x, gen_data.it_data.size.y);
        gen_data.map2dGrad.filterMode = FilterMode.Point;
        gen_data.map2dGrad.wrapMode = TextureWrapMode.Clamp;

        Color fillCol = Color.white;
        fillCol.a = 0;

        BathyGraphie2D.fillTexture(ref gen_data.map2dGrad, fillCol);

        gen_data.map2dLaplace = new Texture2D(gen_data.it_data.size.x, gen_data.it_data.size.y);
        gen_data.map2dLaplace.filterMode = FilterMode.Point;
        gen_data.map2dLaplace.wrapMode = TextureWrapMode.Clamp;

        BathyGraphie2D.fillTexture(ref gen_data.map2dLaplace, fillCol);

        double z11 = 0;
        double z00 = 0;
        double z01 = 0;
        double z02 = 0;
        double z10 = 0;
        double z12 = 0;
        double z20 = 0;
        double z21 = 0;
        double z22 = 0;

        double gradMax =0;
        double laplaceMax = 0;

        double value = 0 ;

        for (int i = 1; i < gen_data.it_data.size.x -1; i++)
        {
            for (int j = (int)gen_data.limite.getLimiteYMin( (uint)(i) ) ; j < (int)gen_data.limite.getLimiteYMax((uint)i)-1; j++)
            {

                z11 = gen_data.it_data.data[i,j];


                z00 =  i== 0 || j == 0 ? 0 : gen_data.it_data.data[i-1,j-1];


                z01 = j==0 ?0:gen_data.it_data.data[i,j-1];


                z02 = j==0 || i >= gen_data.it_data.size.x - 1 ?0:gen_data.it_data.data[i+1,j-1];


                z10 = i==0 ?0:gen_data.it_data.data[i-1,j];


                z12 = i >= gen_data.it_data.size.x - 1 ?0:gen_data.it_data.data[i+1,j];


                z20 = j >= gen_data.it_data.size.y - 1 || i==0?0:gen_data.it_data.data[i-1,j+1];


                z21 = j >= gen_data.it_data.size.y - 1 ?0:gen_data.it_data.data[i,j+1];


                z22 = i >= gen_data.it_data.size.x - 1 || j >= gen_data.it_data.size.y - 1 ?0:gen_data.it_data.data[i+1,j+1];
                //calcul de la derivee
                if (methode.value == 0)
                {
                    value = diferenceFini( z10 -  z12  ,  z01 -z21 ) ;
                }
                else if (methode.value == 1)
                {
                    value =sobel( z00 , z01 , z02 , z10 , z12 , z20 , z21 , z22) ;
                }
                else if(methode.value == 2)
                {
                    value =prewitt( z00 , z01 , z02 , z10 , z12 , z20 , z21 , z22)  ;
                }

                if( value > gradMax)
                {
                    gradMax = value;
                }

                value = laplace( z11 , z01 , z10 , z12 , z21 ) ;
                if( value > laplaceMax)
                {
                    laplaceMax = value;
                }


            }
        }


        //parcour de chaque point et attribut la couleur
        for (int i = 1; i < gen_data.it_data.size.x-1; i++)
        {
            for (int j = (int)gen_data.limite.getLimiteYMin((uint)i); j < (int)gen_data.limite.getLimiteYMax((uint)i)-1; j++)
            {

                z11 = gen_data.it_data.data[i,j];


                z00 =  i== 0 || j == 0 ? 0 : gen_data.it_data.data[i-1,j-1];


                z01 = j==0 ?0:gen_data.it_data.data[i,j-1];


                z02 = j==0 || i >= gen_data.it_data.size.x - 1 ?0:gen_data.it_data.data[i+1,j-1];


                z10 = i==0 ?0:gen_data.it_data.data[i-1,j];


                z12 = i >= gen_data.it_data.size.x - 1 ?0:gen_data.it_data.data[i+1,j];


                z20 = j >= gen_data.it_data.size.y - 1 || i==0?0:gen_data.it_data.data[i-1,j+1];


                z21 = j >= gen_data.it_data.size.y - 1 ?0:gen_data.it_data.data[i,j+1];


                z22 = i >= gen_data.it_data.size.x - 1 || j >= gen_data.it_data.size.y - 1 ?0:gen_data.it_data.data[i+1,j+1];
                //calcul de la derivee
                if (methode.value == 0)
                {
                    gen_data.map2dGrad.SetPixel(i, j, BathyGraphie2D.bathyColor( diferenceFini( z10 -  z12  ,  z01 -z21 ) , 0, gradMax));
                }

                else if (methode.value == 1)
                {
                    gen_data.map2dGrad.SetPixel(i, j, BathyGraphie2D.bathyColor(  sobel( z00 , z01 , z02 , z10 , z12 , z20 , z21 , z22)  , 0, gradMax));
                }
                else if(methode.value == 2)
                {
                    gen_data.map2dGrad.SetPixel(i, j, BathyGraphie2D.bathyColor(  prewitt( z00 , z01 , z02 , z10 , z12 , z20 , z21 , z22)  , 0, gradMax));
                }

                 gen_data.map2dLaplace.SetPixel(i, j, BathyGraphie2D.bathyColor(  laplace( z11 , z01 , z10 , z12 , z21 )  , 0, laplaceMax));

            }
        }

        imageSelector.imageType.value = 1;
        imageSelector.updateShow();


        List<BathyPoint> data = _preprocess.getPreTraitData();

                //complete les donnée avancé
        dataAdv = new List<BathyPoint>();

        foreach (BathyPoint p in data)
        {
            BathyPoint adv = new BathyPoint();
            adv.idx = p.idx;
            adv.vect = p.vect;

            var relVect = gen_data.rel_vect2(adv.vect);

            adv.gradiant = (double)BathyGraphie2D.getRawValue(gen_data.map2dGrad , (int) relVect.x , (int)relVect.y ,0 , (float)gradMax);
            adv.laplace = (double)BathyGraphie2D.getRawValue(gen_data.map2dLaplace , (int) relVect.x , (int)relVect.y ,0 , (float)laplaceMax);

            dataAdv.Add(adv);
        }


        fwdObj.SetActive(true);
        exportBp.gameObject.SetActive(true);

        isProcessing = false;

        yield break;
    }

    public List<BathyPoint> getDerivateData()
    {
        return dataAdv;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
