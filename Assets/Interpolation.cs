using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Interpolation : BPAction
{

    public TMP_InputField resolution;
    public TMP_InputField Dmax;
    public TMP_Dropdown interpolationType;
    public Pretraiter _preprocess;
    private Interpolate interpolate = new Interpolate();
    public ImageSelector imageSelector;
    public Map _map; 
    public Graph _graph;
    public GraphDisplay graphDisplay;

    void Start()
    {    
        if( resolution == null || Dmax == null || interpolationType == null || gen_data == null || _preprocess == null || fwdObj == null || interpolate == null || imageSelector == null || progressBarre == null || _map == null)
        {
            errManager.addError(" non assigné !");
            return;
        }
    }

void OnGUI()
{
    if (isProcessing)
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
    
    rt.anchoredPosition = new Vector2( -rt.sizeDelta.x/2 - screenSize.x * 0.15f , rt_back.anchoredPosition.y - Screen.height*0.20f );
}

protected override IEnumerator action()
{
    if (isProcessing)
    {
        errManager.addWarning("Interpolation deja en cours");
        yield break;
    }

    List<BathyPoint> data = _preprocess.getPreTraitData();

    if( data == null)
    {
        errManager.addError("Pas de données");
        yield break;
    }
    if( data.Count == 0)
    {
        errManager.addError("Pas de données");
        yield break;
    }

    isProcessing = true;

    gen_data.it_data.reset();
    float dmax = 0;
    try
    {
        gen_data.it_data.reso = float.Parse(resolution.text);
        dmax = float.Parse(Dmax.text);
    }
    catch
    {
        errManager.addError("conversion des parametres impossible");
        isProcessing = false;
        yield break;
    }


    
    int type = interpolationType.value;

    if( type > 4)
    {
        errManager.addError("Type d'interpolation inconnu");
        isProcessing = false;
        yield break;
    }


    
    gen_data.it_data.size = new Vector2Int((int)(gen_data.pp_data.size.x * gen_data.it_data.reso), (int)(gen_data.pp_data.size.y * gen_data.it_data.reso));

    gen_data.it_data.data = new double[gen_data.it_data.size.x, gen_data.it_data.size.y];


    //StartCoroutine(gen_data.limite.setLimite( gen_data));

    while( gen_data.limite.ProcessingCheck())
    {
        yield return new WaitForSeconds(0.01f);
    }

    if(gen_data.limite.failed())
    {
        gen_data.limite.setGlobal(gen_data);
    }

    gen_data.it_data.dMax = dmax;
    gen_data.it_data.interpolationType = type;


    interpolate.set_utility(ref errManager , ref gen_data ,ref  data, gen_data.it_data.interpolationType , gen_data.it_data.dMax , ref progressBarre);

    StartCoroutine(interpolate.interpolation());

    //on attant la fin de l'interpolation
    while( progressBarre.ProcessingCheck())
    {
        yield return new WaitForSeconds(0.01f);
    }
    
    gen_data.map2d =  gen_data.workingTexture;
    gen_data.workingTexture = null;
    

    gen_data.map2d.Apply();


    List<Vector2d> cross = new List<Vector2d>();

    List<BathyPoint> tmpData = new List<BathyPoint>( data);

    //si donné de test de confirmation present
    if( gen_data.pp_data.confirmDataSegment.set )
    {
        //retire les données de confirmation

        for( int i = 0 ; i < tmpData.Count ; i++)
        {
            if( gen_data.pp_data.confirmDataSegment.isInside((uint)tmpData[i].idx))
            {
                tmpData.RemoveAt(i);
                i--;
            }
        }
        

        if( gen_data.it_data.interpolationType == 1)
        {
            
            foreach( BathyPoint point in data)
            {
                if( gen_data.pp_data.confirmDataSegment.isInside((uint)point.idx))
                {
                    double predict =interpolate.IDW(tmpData, new Vector2d(point.vect.x, point.vect.y) , 1  ,  gen_data.it_data.dMax);

                    cross.Add(new Vector2d( Mathd.Abs(point.vect.z), Mathd.Abs( predict)));
                }
            }
        }
        else if( gen_data.it_data.interpolationType == 2)
        {
            foreach( BathyPoint point in data)
            {
                if( gen_data.pp_data.confirmDataSegment.isInside((uint)point.idx))
                {
                    double predict = interpolate.IDW(tmpData, new Vector2d(point.vect.x, point.vect.y) , 2  ,  gen_data.it_data.dMax);

                    cross.Add(new Vector2d( Mathd.Abs(point.vect.z), Mathd.Abs( predict)));
                }
            }
        }
        else if( gen_data.it_data.interpolationType == 3)
        {
            foreach( BathyPoint point in data)
            {
                if( gen_data.pp_data.confirmDataSegment.isInside((uint)point.idx))
                {
                    double predict =interpolate.IDW(tmpData, new Vector2d(point.vect.x, point.vect.y) , 3  ,  gen_data.it_data.dMax);

                    cross.Add(new Vector2d( Mathd.Abs(point.vect.z), Mathd.Abs( predict)));
                }
            }
        }
        else if( gen_data.it_data.interpolationType == 0)
        {
            foreach( BathyPoint point in data)
            {
                if( gen_data.pp_data.confirmDataSegment.isInside((uint)point.idx))
                {
                    double predict =interpolate.nearestNeighbor(tmpData, new Vector2d(point.vect.x, point.vect.y) , gen_data.it_data.dMax);

                    cross.Add(new Vector2d( Mathd.Abs(point.vect.z), Mathd.Abs( predict)));
                }
            }
        }
        else
        {   
            cross = new List<Vector2d>();
        }


        _graph.clear();

        foreach( Vector2d v in cross)
        {
            _graph.addPoint(v);
        }
        _graph.getLPoints().linearRegression();
        _graph.getLPoints().getRPearson();
        _graph.getLPoints().getRSquare();
        _graph.getLPoints().getStandardRMSE();
        _graph.getLPoints().getXYRMSE();


        _graph.autoScale();


        StartCoroutine( _graph.drawGraph()) ;

        while( progressBarre.ProcessingCheck() )
        {
            yield return new WaitForSeconds(0.01f);
        }
        
        _graph.drawLinearCurve();

        graphDisplay.saveCurrent( 1 );
        graphDisplay.courbeType.value = 1;
    }

    fwdObj.SetActive(true);
    imageSelector._data = data;
    imageSelector.imageType.value = 0;
    imageSelector.updateShow();



    StartCoroutine(_map.generateMesh(gen_data));

    isProcessing = false;
}
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        
    }

    
}
