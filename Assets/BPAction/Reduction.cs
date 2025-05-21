using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Reduction : BPAction
{
    public Button exportBp;
    public TMP_Dropdown reductionType;
    public TMP_InputField target;
    bool isProcessing2 = false;
    public GeneralStatUtils gen_data2;
    public Derivate _derivate;
    public TMP_Text pointRestant;
    public List<BathyPoint> dataAdv = new List<BathyPoint>();
    public Graph _graph;
    public Toggle shielContour;
    public Toggle shielConfirm;
    public Toggle shielGrad;
     public TMP_InputField gradPourcent;
     public ImageSelector imgSelect;

     public GraphDisplay graphDisplay;

    void Start()
    {
        if( progressBarre == null || reductionType == null || gen_data == null || _derivate == null || pointRestant == null || exportBp == null || gen_data2 == null)
        {
            Debug.LogError(" non assigné !");
            return;
        }

        exportBp.onClick.AddListener(delegate { BathyGraphie2D.csvSave(dataAdv , gen_data.workingPath + "/pp_d_r_data.csv" , gen_data.separator); });
    }

    void OnGUI()
    {
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



    protected override IEnumerator action()
    {
        if (isProcessing)
        {
            yield break;
        }

        isProcessing = true;

        exportBp.gameObject.SetActive(false);

        int type = reductionType.value;

        if(type == 0)
        {
            StartCoroutine(lessResolutionNew());

            while(isProcessing2)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        //trier les point par ordre d'index
        dataAdv.Sort((x, y) => x.idx.CompareTo(y.idx));

        imgSelect._data = dataAdv;

        //debug n point
        Debug.Log("n point restant: " + dataAdv.Count);
        pointRestant.text = "Point restant: " + dataAdv.Count;

        //calcule des donné generique
        processGenData();

        Interpolate interpolate = new Interpolate();

        
        gen_data2.it_data.size = gen_data.it_data.size;
        gen_data2.it_data.reso = gen_data.it_data.reso;
        gen_data2.limite = gen_data.limite;
        gen_data2.it_data.dMax = gen_data.it_data.dMax;
        gen_data2.it_data.nNeighbors = gen_data.it_data.nNeighbors;
        gen_data2.it_data.data = new double[gen_data.it_data.size.x, gen_data.it_data.size.y];
        gen_data2.it_data.interpolationType = gen_data.it_data.interpolationType;

        ListPoint.RegressData.Estimateur estimateurTmp = null;

        if(graphDisplay.getLPoints(1)!= null)
        {
            if(graphDisplay.getLPoints(1).regressData != null)
            {
                estimateurTmp = graphDisplay.getLPoints(1).regressData.estimateur;
            }
        }

        if(gen_data2.it_data.interpolationType == 4 && estimateurTmp == null)
        {
            errManager.addError("Pas d'estimateur caculer pour le krigeage ordinaire");
            isProcessing = false;
            yield break;
        }

        interpolate.set_utility(ref errManager ,ref gen_data2 ,ref  dataAdv , ref progressBarre , estimateurTmp);

        StartCoroutine(interpolate.interpolation());

        while( progressBarre.ProcessingCheck())
        {
            yield return new WaitForSeconds(0.01f);
        }

        gen_data.map2dReduct =  gen_data2.workingTexture;
        gen_data2.workingTexture = null;
        

        gen_data.map2dReduct.Apply();

        //soustraire les deux textures
         gen_data.map2dDelta = new Texture2D(gen_data.it_data.size.x, gen_data.it_data.size.y); 

        gen_data.map2dDelta.filterMode = FilterMode.Point;
        gen_data.map2dDelta.wrapMode = TextureWrapMode.Clamp;
        for( int x = 0 ; x < gen_data.it_data.size.x ; x++)
        {
            for( int y = 0 ; y < gen_data.it_data.size.y ; y++)
            {
                Color c = gen_data.map2d.GetPixel(x, y);
                Color c2 = gen_data.map2dReduct.GetPixel(x, y);

                Color c3 = new Color(c.r - c2.r , c.g - c2.g , c.b - c2.b , 1);

                gen_data.map2dDelta.SetPixel(x, y, c3);
            }
        }


        var tmpList = BathyGraphie2D.CrossValidation(  gen_data.it_data.data , gen_data2.it_data.data );


        //validation croisé
        if(  gen_data.it_data.data.GetLength(0) == gen_data2.it_data.data.GetLength(0) &&  gen_data.it_data.data.GetLength(1) == gen_data2.it_data.data.GetLength(1))
        {
           _graph.clear();

            foreach( Vector2d v in tmpList)
            {
                _graph.addPoint(v);
            }
            _graph.getLPoints().linearRegression();
            _graph.getLPoints().getRPearson();
            _graph.getLPoints().getRSquare();
            _graph.getLPoints().getStandardRMSE();
            _graph.getLPoints().getXYRMSE();


            _graph.autoScale();
            _graph._barycentre = new Vector2(0, 0);
             StartCoroutine( _graph.drawGraph()) ;

            while( progressBarre.ProcessingCheck() )
            {
                yield return new WaitForSeconds(0.01f);
            }
            _graph.drawEstimateCurve();

            graphDisplay.saveCurrent( GraphDisplay.IndexCurve.crossValidReduct );

        }
        else
        {
            Debug.LogError("Erreur de taille");
        }

        



        exportBp.gameObject.SetActive(true);
        isProcessing = false;

        progressBarre.setAction("Reduction Fini");

        yield return null;
    }



    IEnumerator lessResolutionNew()
    {

        List<BathyPoint> tmpData = new List<BathyPoint>(_derivate.getDerivateData());
        //creee un copy de tmpData

        if( tmpData == null)
        {
            Debug.LogError("Pas de donnée");
            yield break;
        }
        if( tmpData.Count == 0)
        {
            Debug.LogError("Pas de donnée");
             yield break;
        }

        if(isProcessing2)
        {
            yield break;
        }

        isProcessing2 = true;

        dataAdv.Clear();
        dataAdv = new List<BathyPoint>();


        double resoLess =  (gen_data.pp_data.min_distance-0.01f) / 0.5f;


        int limite = int.Parse(target.text);

        dataAdv = subLessReso(resoLess , tmpData.Count);


        progressBarre.setAction("Reduction en cours .... approche de la sous resolution optimal");
        progressBarre.start( 10 , 0.1f );

        float approache = 0;
        uint y =0;
        for(float i = 1 ;  i >0 ; i-=  0.1f)
        {
            resoLess =  (gen_data.pp_data.min_distance-0.01f) / i;
            dataAdv = subLessReso(resoLess ,  tmpData.Count);

            if( dataAdv.Count < limite )
            {
                approache = i;
                break;
            }

            if( progressBarre.validUpdate(y))
            {
                yield return new WaitForSeconds(0.01f);
            }
            y++;

        }

        y = 0;
        progressBarre.stop();
        progressBarre.setAction("Reduction en cours .... estimation de la sous resolution optimal");
        progressBarre.start( 100 );
        for(float i = approache ; i < 1  ; i+=  0.001f)
        {
            
            resoLess =  (gen_data.pp_data.min_distance-0.01f) / i;
            dataAdv = subLessReso(resoLess ,  tmpData.Count);

            if( dataAdv.Count > limite )
            {
                break;
            }


            if( progressBarre.validUpdate(y))
            {
                yield return new WaitForSeconds(0.01f);
            }

            y++;
        }

        dataAdv = subLessReso(resoLess , limite);



        progressBarre.stop();
        progressBarre.setAction("Reduction Fini");

        isProcessing2 = false;

         yield return null;

    }

    private List<BathyPoint> subLessReso( double resoLess , int limite)
    {
        List<BathyPoint> result = new List<BathyPoint>();

        List<BathyPoint> tmpData = new List<BathyPoint>(_derivate.getDerivateData());


        //sans les point de contour
        if( shielContour.isOn && gen_data.pp_data.convexeDataSegment.set)
        {
           for( int i = 0 ; i < tmpData.Count ; i++)
            {
                if( tmpData[i].idx >= gen_data.pp_data.convexeDataSegment.start && tmpData[i].idx <= gen_data.pp_data.convexeDataSegment.end)
                {
                    result.Add(tmpData[i]);
                    tmpData.RemoveAt(i);
                    i--;
                }
            }
            
        }

        //sans les point de confirlation
        if( shielConfirm.isOn && gen_data.pp_data.confirmDataSegment.set)
        {
            for( int i = 0 ; i < tmpData.Count ; i++)
            {
                if( tmpData[i].idx >= gen_data.pp_data.confirmDataSegment.start && tmpData[i].idx <= gen_data.pp_data.confirmDataSegment.end)
                {
                    result.Add(tmpData[i]);
                    tmpData.RemoveAt(i);
                    i--;
                }
            }
        }


        if( shielGrad.isOn)
        {
             //trie par odre de gradian
            tmpData.Sort((x, y) => x.gradiant.CompareTo(y.gradiant));

            //inverse
            tmpData.Reverse();

            double pourcent = double.Parse(gradPourcent.text);

            //assure d'etre compris entre 0 et 100
            if( pourcent < 0)
            {
                pourcent = 0;
                gradPourcent.text = "0";
            }
            if( pourcent > 50)
            {
                pourcent = 50;
                gradPourcent.text = "50";
            }

            //protege les 5% de point les plus fort
            int protect = (int)(tmpData.Count * double.Parse(gradPourcent.text)/100.0 );

            for( int i = 0 ; i < protect ; i++)
            {
                result.Add(tmpData[i]);
            }

            tmpData.RemoveRange(0, protect);
        }
       

        //trie aleatoirement
        
        tmpData.Sort((x, y) => Random.Range(-1, 2));


        while(tmpData.Count > 0)
        {

            //arrondir au plus proche

            Vector2d relPos = gen_data.rel_vect2(tmpData[0].vect);
            Vector2Int point2gird = new Vector2Int( (int) Mathd.Floor(relPos.x * gen_data.pp_data.min_resolution) , (int) Mathd.Floor(relPos.y * gen_data.pp_data.min_resolution) );



            List<BathyPoint> colliderPoints = new List<BathyPoint>();

            colliderPoints.Add(tmpData[0]);
            tmpData.RemoveAt(0);

            for( int i = 0 ; i < tmpData.Count ; i++)
            {
                Vector2d relPos2 = gen_data.rel_vect2(tmpData[i].vect);
                Vector2d point2gird2 = new Vector2d( (int) Mathd.Floor(relPos2.x * gen_data.pp_data.min_resolution) , (int) Mathd.Floor(relPos2.y * gen_data.pp_data.min_resolution) );

                

                if( relPos2.x >= (float)(relPos.x-resoLess)   && relPos2.x < (float)(relPos.x+resoLess)  && relPos2.y >= (float)(relPos.y-resoLess)  && relPos2.y < (float)(relPos.y+resoLess))
                {
                    colliderPoints.Add(tmpData[i]);
                    tmpData.RemoveAt(i);
                    i--;
                }
            }

            if( colliderPoints.Count == 1  )
            {
                result.Add(colliderPoints[0]);
            }
            else if( colliderPoints.Count > 1)
            {
                BathyPoint tmp = new BathyPoint();
                tmp.idx = colliderPoints[0].idx;

                int i = 0;
                foreach( BathyPoint col in colliderPoints)
                {
                    tmp.vect += col.vect;
                    tmp.gradiant += col.gradiant;
                    tmp.laplace += col.laplace;
                    i++;
                }

                tmp.vect /= i;
                tmp.gradiant /= i;
                tmp.laplace /= i;

                result.Add(tmp);
            }

            if(result.Count >= limite)
            {
                //a pour effet de suprimer le spoint restant de maniere     aleatoir
                break;
            }
        }

        return result;
    }


    private void processGenData()
    {
        gen_data2.pp_data.reset();


        foreach(BathyPoint v in dataAdv)
        {
            //calcule des extremum
            if( v.vect.x < gen_data2.pp_data.min.x)
            {
                gen_data2.pp_data.min.x = v.vect.x;
            }
            if( v.vect.y < gen_data2.pp_data.min.y)
            {
                gen_data2.pp_data.min.y = v.vect.y;
            }

            if( v.vect.z < gen_data2.pp_data.min.z )
            {
                gen_data2.pp_data.min.z = v.vect.z ;
            }

            if( v.vect.x > gen_data2.pp_data.max.x)
            {
                gen_data2.pp_data.max.x = v.vect.x;
            }
            if( v.vect.y > gen_data2.pp_data.max.y)
            {
                gen_data2.pp_data.max.y = v.vect.y;
            }
            if( v.vect.z > gen_data2.pp_data.max.z )
            {
                gen_data2.pp_data.max.z = v.vect.z ;
            }
        }

        gen_data2.pp_data.size = gen_data2.pp_data.max - gen_data2.pp_data.min;

         double a =0;
        double b = 0;
        float dist = float.MaxValue;
        float dist_min2 = float.MinValue;

        //calcule de la distance minimal entre 2 point
        for(int i = 0; i < dataAdv.Count; i++)
        {
            dist_min2 = float.MinValue;
            for(int j = i+1; j < dataAdv.Count; j++)
            {
                if( i == j)
                {
                    continue;
                }

                a = dataAdv[i].vect.x - dataAdv[j].vect.x;
                b = dataAdv[i].vect.y - dataAdv[j].vect.y;

                dist = (float)(a * a + b * b);

                if( dist < gen_data2.pp_data.min_distance)
                {
                    gen_data2.pp_data.min_distance = dist;
                    dist_min2 = dist;
                }
            }

            if( dist_min2 > gen_data2.pp_data.max_distance)
            {
                gen_data2.pp_data.max_distance = dist_min2;
            }
        }


        //calcule du point nemo , point virtuel le plus eloigné de tout point
        


        gen_data2.pp_data.min_distance = Mathf.Sqrt(gen_data2.pp_data.min_distance);
        gen_data2.pp_data.max_distance = Mathf.Sqrt(gen_data2.pp_data.max_distance);
        //clacule de la resoltion en point /metre 
        gen_data2.pp_data.min_resolution = 1.0f /  gen_data2.pp_data.min_distance;
        
        
        gen_data2.updateStat();



        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
