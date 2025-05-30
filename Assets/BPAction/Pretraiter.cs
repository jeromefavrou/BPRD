using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Threading;
public class Pretraiter : BPAction
{
    // Start is called before the first frame update
    public Button exportBp;
    public Button variogrameBp;
    public TMP_InputField offset;
    
    public TMP_Dropdown limiteType;
    public TMP_Dropdown confirmType;
    public TMP_InputField startConvexIdx= null;
    public TMP_InputField endConvexIdx= null;
    public TMP_InputField startConfirmIdx = null;
    public TMP_InputField endConfirmIdx = null;

    public TMP_InputField sigma_clipping;
    public TMP_InputField sigma_clipping2;
    public TMP_Text pointTraiter;

    public TMP_InputField resolution;
    public TMP_InputField dMax;

    public ImportCSV _importCsv;

    private const string logfileName = "log.txt";
    
    private List<BathyPoint> preTraitData = new List<BathyPoint>();
    private List<BathyPoint> tmpBathyData = new List<BathyPoint>();
    private Interpolate _interpolate = new Interpolate();

    bool isProcessing2 = false;

    private Vector2d nemoPoint = new Vector2d();
    public Graph _graph;
    public GraphDisplay graphDisplay;
    void Start()
    {
        if( button == null || offset == null || pointTraiter == null || _importCsv == null || fwdObj == null || gen_data == null || sigma_clipping==null || sigma_clipping2 == null || resolution == null || errManager == null  || bckObj == null || exportBp == null || progressBarre == null || limiteType == null || startConvexIdx == null || endConvexIdx == null || variogrameBp == null)
        {
            errManager.addError("Un ou plusieurs composants ne sont pas assignés");
            return;
        }
        
        exportBp.onClick.AddListener(delegate { BathyGraphie2D.csvSave(preTraitData , gen_data.workingPath + "/pp_data.csv" , gen_data.separator); });
        limiteType.onValueChanged.AddListener(delegate { limiteChange(); });
        confirmType.onValueChanged.AddListener(delegate { confirmChange(); });

        limiteChange();
        confirmChange();
    }

    void limiteChange()
    {
        if( limiteType.value == 1)
        {
            var tmp = _importCsv.getCSVData();
            if( tmp != null)
            {
               endConvexIdx.text = "0";
            }
            else
            {
                endConvexIdx.text = "10000";
            }
            
            startConvexIdx.gameObject.SetActive(true);
            endConvexIdx.gameObject.SetActive(true);
        }
        else
        {
             startConvexIdx.gameObject.SetActive(false);
            endConvexIdx.gameObject.SetActive(false);
           
        }
    }

    void confirmChange()
    {
        if( confirmType.value == 1)
        {
            startConfirmIdx.gameObject.SetActive(true);
            endConfirmIdx.gameObject.SetActive(true);
        }
        else
        {
            startConfirmIdx.gameObject.SetActive(false);
            endConfirmIdx.gameObject.SetActive(false);
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
        
        rt.anchoredPosition = new Vector2( -rt.sizeDelta.x/2 - screenSize.x * 0.15f , +rt_back.anchoredPosition.y - Screen.height*0.1f);
    }

    private bool segmentDefine( List<BathyPoint> _preTraitData )
    {

        //debug _preTraitData
        if( startConvexIdx == null || endConvexIdx == null || startConfirmIdx == null || endConfirmIdx == null)
        {
            errManager.addError("Un ou plusieurs composants segment ne sont pas assignés");
            return false;
        }

        //convestiondes valeur text en uint

        try
        {

            if(limiteType.value == 1)
            {
                gen_data.pp_data.convexeDataSegment = new GeneralStatUtils.PP_data.DataSegment();
                gen_data.pp_data.convexeDataSegment.start = uint.Parse(startConvexIdx.text);
                gen_data.pp_data.convexeDataSegment.end = uint.Parse(endConvexIdx.text);
                gen_data.pp_data.convexeDataSegment.set = true;
                gen_data.pp_data.convexeDataSegment.rendomiz = false;
            }
            else
            {
                gen_data.pp_data.convexeDataSegment = new GeneralStatUtils.PP_data.DataSegment();
            }

            if(confirmType.value == 0)
            {
                //selection aleatoir parmie les point
                gen_data.pp_data.confirmDataSegment = new GeneralStatUtils.PP_data.DataSegment();
                gen_data.pp_data.confirmDataSegment.rendomiz = true;
                gen_data.pp_data.confirmDataSegment.list = new List<uint>();
                
                //selection aleatoir de 5% des point
                int nb = (int)(_preTraitData.Count * 0.1);

                for(int i = 0; i < nb; i++)
                {
                    gen_data.pp_data.confirmDataSegment.list.Add((uint)Random.Range(0, _preTraitData.Count));
                }

                gen_data.pp_data.confirmDataSegment.set = true;


            }
            else if(confirmType.value == 1)
            {
                gen_data.pp_data.confirmDataSegment = new GeneralStatUtils.PP_data.DataSegment();
                gen_data.pp_data.confirmDataSegment.start = uint.Parse(startConfirmIdx.text);
                gen_data.pp_data.confirmDataSegment.end = uint.Parse(endConfirmIdx.text);
                gen_data.pp_data.confirmDataSegment.set = true;
                gen_data.pp_data.confirmDataSegment.rendomiz = false;
            }
            else
            {
                gen_data.pp_data.confirmDataSegment = new GeneralStatUtils.PP_data.DataSegment();
            }
            

            if( !gen_data.pp_data.convexeDataSegment.isValide() || !gen_data.pp_data.confirmDataSegment.isValide())
            {
                throw new System.Exception( "start > end");
            }

            if( gen_data.pp_data.convexeDataSegment.start >= _preTraitData.Count || gen_data.pp_data.convexeDataSegment.end >= _preTraitData.Count )
            {
                throw new System.Exception( "contour > nombre de point");
            }

            if( gen_data.pp_data.confirmDataSegment.start >= _preTraitData.Count || gen_data.pp_data.confirmDataSegment.end >= _preTraitData.Count)
            {
                throw new System.Exception( "test > nombre de point");
            }


            //debug segment
            Debug.Log("Segment de donnée convexe : " + gen_data.pp_data.convexeDataSegment.start + " ; " + gen_data.pp_data.convexeDataSegment.end);
            Debug.Log("Segment de donnée confirmé : " + gen_data.pp_data.confirmDataSegment.start + " ; " + gen_data.pp_data.confirmDataSegment.end);

            return true;
        }
        catch(System.Exception e)
        {
            errManager.addError("Erreur de conversion des valeurs segment: " + e.Message);

            gen_data.pp_data.confirmDataSegment.start = 0;
            gen_data.pp_data.confirmDataSegment.end = 0;
            gen_data.pp_data.confirmDataSegment.set = false;
            
            gen_data.pp_data.convexeDataSegment.start = 0;
            gen_data.pp_data.convexeDataSegment.end = 0;
            gen_data.pp_data.convexeDataSegment.set = false;

            startConvexIdx.text = "0";
            endConvexIdx.text = "0";

            startConfirmIdx.text = "0";
            endConfirmIdx.text = "0";
            
            return false;
        }


    }

    private IEnumerator offsetAndDeadApply( )
    {
        if( isProcessing2)
        {
            errManager.addWarning("Un traitement est déjà en cours");
            yield break;
        }

        if( _importCsv.getCSVData() == null)
        {
            errManager.addWarning("Aucune donnée à traiter");
            yield break;
        }

        if( _importCsv.getCSVData().Count == 0)
        {
            errManager.addWarning("Aucune donnée à traiter");
            yield break;
        }


        isProcessing2 = true;

        progressBarre.setAction("Suppression des points > 0 et ajout offset");
        progressBarre.start((uint)_importCsv.getCSVData().Count);


        //ouvre le fichier de log et ecrit a la suite 
        
        using (StreamWriter log = File.AppendText(gen_data.workingPath + "/" + logfileName))
        {
            double off = double.Parse(offset.text);
            int i = 0;


            foreach (BathyPoint v in _importCsv.getCSVData())
            {
                if (v.vect.z < 0)
                {
                    preTraitData.Add(new BathyPoint(new Vector3d(v.vect.x, v.vect.y, v.vect.z + off), v.idx));
                }
                else
                {
                    log.WriteLine("Point supprimé car z >= 0   idx: " + v.idx);
                }

                if( progressBarre.validUpdate((uint)i))
                {
                    yield return new WaitForSeconds(0.01f);
                }

                i++;

            }
            log.WriteLine("Données > 0 supprimer : " + (_importCsv.getCSVData().Count - preTraitData.Count));
        }
        
        //debug 
        Debug.Log("Donné > 0 supprimer : " + (_importCsv.getCSVData().Count - preTraitData.Count));


        isProcessing2 = false;
    }

    private IEnumerator processDoublePoint()
    {
        if(preTraitData.Count == 0)
        {
            errManager.addWarning("Aucune donnée à traiter");
            yield break;
        }

        if(isProcessing2)
        {
            errManager.addWarning("Un traitement est déjà en cours");
            yield break;
        }


        isProcessing2 = true;

        progressBarre.setAction("Traitement des point double et supperposé");
        progressBarre.start((uint)preTraitData.Count);

        double sigma = double.Parse(sigma_clipping.text);
        double sigma2 = double.Parse(sigma_clipping2.text);

        //conteur de point traité
        int o =0;
        int g = 0;
        int l = 0;
        int u = 0;
        int f=0;


        double dist2;
        double result = double.MaxValue;
        double distanceSave = float.MaxValue;

        double d1 =0;
        double d2 =0;

        double sigma_ponderation = 0;


        using (StreamWriter log = File.AppendText(gen_data.workingPath + "/" + logfileName))
        {

            //conte les poitn en double 
            for(int i = 0; i < preTraitData.Count; i++)
            {
                for(int j = i+1; j < preTraitData.Count; j++)
                {
                    if( i == j)
                    {
                        continue;
                    }
                    if( preTraitData[i].vect.x == preTraitData[j].vect.x && preTraitData[i].vect.y == preTraitData[j].vect.y )
                    {
                        if(  preTraitData[i].vect.z == preTraitData[j].vect.z)
                        {
                            log.WriteLine("Point identique");
                            log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                            log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                            log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                            log.WriteLine("\tgarder : ["+preTraitData[i].idx+"]");
                            log.WriteLine("\tsupprimer : ["+preTraitData[j].idx+"]");

                            preTraitData.RemoveAt(j);
                            j--;
                            o++;
                            
                        }
                        else
                        {
                            f++;
                            if(Mathd.Abs(preTraitData[i].vect.z - preTraitData[j].vect.z ) > sigma )
                            {
                                result = double.MaxValue;
                                distanceSave = float.MaxValue;
                                
                                for(int h = 0; h < preTraitData.Count; h++)
                                {
                                    if( h == i || h == j)
                                    {
                                        continue;
                                    }

                                    //distance au carré
                                    dist2 = Vector3d.ToVector2d(preTraitData[i].vect - preTraitData[h].vect).SqrMagnitude();
                                    
                                    if (dist2 < distanceSave)
                                    {
                                        distanceSave = dist2;
                                        result = preTraitData[h].vect.z;
                                    }
                                }

                                d1 = Mathd.Abs(preTraitData[i].vect.z - result);
                                d2 = Mathd.Abs(preTraitData[j].vect.z - result);

                                sigma_ponderation = 0;

                                if( sigma2 != 0 && distanceSave != 0)
                                {
                                    sigma_ponderation = sigma2;
                                }

                                if( sigma_ponderation < Vector3d.kEpsilon)
                                {
                                    log.WriteLine("Point supperposé hors sigma trouvé");
                                    log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                    log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                    log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                    log.WriteLine("\tsupprimer : ["+preTraitData[i].idx+"]");
                                    log.WriteLine("\tsupprimer : ["+preTraitData[j].idx+"]");

                                    preTraitData.RemoveAt(j);
                                    preTraitData.RemoveAt(i);
                                    i--;
                                    j=preTraitData.Count;
                                    u++;

                                    continue;
                                }
                                

                                if( d1 < d2)
                                {
                                    if(d1 < sigma_ponderation )
                                    {
                                        log.WriteLine("Point supperposé hors sigma trouvé");
                                        log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                        log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                        log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                        log.WriteLine("\tgarder : ["+preTraitData[i].idx+"]");
                                        log.WriteLine("\tsupprimer : ["+preTraitData[j].idx+"]");

                                        preTraitData.RemoveAt(j);
                                        j=0;
                                        l++;
                                    }
                                    else if( d2 < sigma_ponderation )
                                    {
                                        log.WriteLine("Point supperposé hors sigma trouvé");
                                        log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                        log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                        log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                        log.WriteLine("\tsupprimer : ["+preTraitData[i].idx+"]");
                                        log.WriteLine("\tgarder : ["+preTraitData[j].idx+"]");

                                        preTraitData.RemoveAt(i);
                                        i--;
                                        j=preTraitData.Count;
                                        l++;


                                    }
                                    else
                                    {
                                        log.WriteLine("Point supperposé hors sigma trouvé");
                                        log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                        log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                        log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                        log.WriteLine("\tsupprimer : ["+preTraitData[i].idx+"]");
                                        log.WriteLine("\tsupprimer : ["+preTraitData[j].idx+"]");

                                        preTraitData.RemoveAt(j);
                                        preTraitData.RemoveAt(i);
                                        i--;
                                        j=preTraitData.Count;
                                        u++;
                                    }
                                }
                                else
                                {
                                    if(d2 <sigma_ponderation )
                                    {
                                        log.WriteLine("Point supperposé hors sigma trouvé");
                                        log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                        log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                        log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                        log.WriteLine("\tsupprimer : ["+preTraitData[i].idx+"]");
                                        log.WriteLine("\tgarder : ["+preTraitData[j].idx+"]");

                                        preTraitData.RemoveAt(i);
                                        i--;
                                        j=preTraitData.Count;
                                        l++;
                                    }
                                    else if( d1 < sigma_ponderation )
                                    {
                                        log.WriteLine("Point supperposé hors sigma trouvé");
                                        log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                        log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                        log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                        log.WriteLine("\tgarder : ["+preTraitData[i].idx+"]");
                                        log.WriteLine("\tsupprimer : ["+preTraitData[j].idx+"]");

                                        preTraitData.RemoveAt(j);
                                        j=0;
                                        l++;
                                    }
                                    else
                                    {
                                        log.WriteLine("Point supperposé hors sigma trouvé");
                                        log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                        log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                        log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                        log.WriteLine("\tsupprimer : ["+preTraitData[i].idx+"]");
                                        log.WriteLine("\tsupprimer : ["+preTraitData[j].idx+"]");

                                        preTraitData.RemoveAt(j);
                                        preTraitData.RemoveAt(i);
                                        i--;
                                        j=preTraitData.Count;
                                        u++;
                                    }
                                }

                            }
                            else
                            {
                                log.WriteLine("Point fusionné");
                                log.WriteLine("\t(x,y) en conflit: " + preTraitData[i].vect.x + " ; " + preTraitData[i].vect.y  );
                                log.WriteLine("\t["+preTraitData[i].idx+"] : " + preTraitData[i].vect.z );
                                log.WriteLine("\t["+preTraitData[j].idx+"] : " + preTraitData[j].vect.z );

                                log.WriteLine("\tgarder : ["+preTraitData[i].idx+"] et z moyenner: " + (preTraitData[i].vect.z + preTraitData[j].vect.z )*0.5);
                                log.WriteLine("\tsupprimer : ["+preTraitData[j].idx+"]");
                                //fait la moyenne des 2 point 
                                preTraitData[i].vect = new Vector3d(preTraitData[i].vect.x, preTraitData[i].vect.y,  (preTraitData[i].vect.z + preTraitData[j].vect.z )*0.5);
                                preTraitData.RemoveAt(j);
                                j=0;
                                g++;
                            }
                            
                        }
                    }
                }


                if(progressBarre.validUpdate((uint)i))
                {
                    yield return new WaitForSeconds(0.01f);
                }


            }

            log.WriteLine("point tridimentionnel identique supprimer : " + o);
            log.WriteLine("point tridimentionnel supperposé trouver: " + f);
            log.WriteLine("point tridimentionnel supperposé moyenner car dans sigma: " + g);
            log.WriteLine("point tridimentionnel supperposé 1 supprimer car hors sigma : " + l);
            log.WriteLine("point tridimentionnel supperposé tous supprimer car hors sigma: " + u);

        }

        //debug log o
        Debug.Log("point tridimentionnel identique supprimer : " + o);
        Debug.Log("point tridimentionnel supperposé trouver: " + f);
        Debug.Log("point tridimentionnel supperposé moyenner car dans sigma: " + g);
        Debug.Log("point tridimentionnel supperposé 1 supprimer car hors sigma : " + l);
        Debug.Log("point tridimentionnel supperposé tous supprimer car hors sigma: " + u);

        isProcessing2 = false;
    }

    private IEnumerator processGenData()
    {
        isProcessing2 = true;

        //save les segment de donnée
        var tmp1 = gen_data.pp_data.convexeDataSegment;
        var tmp2 = gen_data.pp_data.confirmDataSegment;
        gen_data.pp_data.reset();

        gen_data.pp_data.convexeDataSegment = tmp1;
        gen_data.pp_data.confirmDataSegment = tmp2;

        progressBarre.setAction("Calcul des données générales");
        progressBarre.start((uint)preTraitData.Count);
        yield return new WaitForSeconds(0.01f);
        
        foreach(BathyPoint v in preTraitData)
        {
            //calcule des extremum
            if( v.vect.x < gen_data.pp_data.min.x)
            {
                gen_data.pp_data.min.x = v.vect.x;
            }
            if( v.vect.y < gen_data.pp_data.min.y)
            {
                gen_data.pp_data.min.y = v.vect.y;
            }

            if( v.vect.z < gen_data.pp_data.min.z )
            {
                gen_data.pp_data.min.z = v.vect.z ;
            }

            if( v.vect.x > gen_data.pp_data.max.x)
            {
                gen_data.pp_data.max.x = v.vect.x;
            }
            if( v.vect.y > gen_data.pp_data.max.y)
            {
                gen_data.pp_data.max.y = v.vect.y;
            }
            if( v.vect.z > gen_data.pp_data.max.z )
            {
                gen_data.pp_data.max.z = v.vect.z ;
            }


        }

        gen_data.pp_data.size = gen_data.pp_data.max - gen_data.pp_data.min;

         double a =0;
        double b = 0;
        float dist = float.MaxValue;
        float dist_min2 = float.MinValue;

        //calcule de la distance minimal entre 2 point
        for(int i = 0; i < preTraitData.Count; i++)
        {
            dist_min2 = float.MinValue;
            for(int j = i+1; j < preTraitData.Count; j++)
            {
                if( i == j)
                {
                    continue;
                }

                a = preTraitData[i].vect.x - preTraitData[j].vect.x;
                b = preTraitData[i].vect.y - preTraitData[j].vect.y;

                dist = (float)(a * a + b * b);

                if( dist < gen_data.pp_data.min_distance)
                {
                    gen_data.pp_data.min_distance = dist;
                    dist_min2 = dist;
                }
            }

            if( dist_min2 > gen_data.pp_data.max_distance)
            {
                gen_data.pp_data.max_distance = dist_min2;
            }

            if( progressBarre.validUpdate((uint)i))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        gen_data.pp_data.min_distance = Mathf.Sqrt(gen_data.pp_data.min_distance);
        gen_data.pp_data.max_distance = Mathf.Sqrt(gen_data.pp_data.max_distance);
        //clacule de la resoltion en point /metre 
        gen_data.pp_data.min_resolution = 1.0f /  gen_data.pp_data.min_distance;

        progressBarre.stop();
        
        yield return new WaitForSeconds(0.01f);

        //calcule du point nemo , point virtuel le plus eloigné de tout point
        gen_data.pp_data.nemo_distance = 0.0f; //!valeur temporaire en attendant a vrai valeur
        //arrondire au superieur
        gen_data.it_data.reso =  1 ;
        gen_data.it_data.size = new Vector2Int((int)(gen_data.pp_data.size.x *gen_data.it_data.reso), (int)(gen_data.pp_data.size.y*gen_data.it_data.reso)); 

        gen_data.limite = new NPLimite();
        gen_data.limite._pbarre = progressBarre;
        gen_data.limite.errManager = errManager;


        if( limiteType.value == 0)
        {
            StartCoroutine(gen_data.limite.calculConvexHullGraham(preTraitData, gen_data)) ;
        }
        else if( limiteType.value == 1)
        {
            StartCoroutine(gen_data.limite.calculConvexHullPointFini(preTraitData, gen_data , uint.Parse(startConvexIdx.text) , uint.Parse(endConvexIdx.text)));
        }
        else
        {
            gen_data.limite.setGlobal(gen_data);
        }


        while( gen_data.limite.ProcessingCheck())
        {
            yield return new WaitForSeconds(0.01f);
        }

        if(gen_data.limite.failed())
        {
            gen_data.limite.setGlobal(gen_data);
        }
        else
        {
            StartCoroutine(gen_data.limite.setLimite( gen_data));

            while( gen_data.limite.ProcessingCheck() )
            {
                yield return new WaitForSeconds(0.01f);
            }

            if(gen_data.limite.failed())
            {
                gen_data.limite.setGlobal(gen_data);
            } 
        }

        //on retire les donné de confirmation pour le calcule du point nemo
        tmpBathyData = gen_data.subConfirmData(preTraitData);

        ThreadSegment thread = new ThreadSegment((uint)gen_data.pp_data.size.x);

        progressBarre.setAction("Caclule point Nemo [" + thread.get_nThreads() + " threads]");
        
        progressBarre.start((uint)gen_data.pp_data.size.x);

        thread.Execute( th_nemoPoint );
        while(thread.inProcess())
        {
            if( progressBarre.validUpdate((uint)thread.totalProgress))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        //attente des thread
        thread.WaitForAllThreads();

        gen_data.pp_data.nemo_distance = Mathf.Sqrt(gen_data.pp_data.nemo_distance);
        
        progressBarre.stop();
        
    

        // Affinage du point Nemo en générant des points autour de celui-ci
        int refinementIterations = 50; // Nombre d'itérations pour affiner le point
float searchRadius = 1.0f; // Rayon de recherche autour du point Nemo
float bestDist = gen_data.pp_data.nemo_distance; // Meilleure distance trouvée

for (int iter = 0; iter < refinementIterations; iter++)
{
    // Liste pour stocker les candidats
    List<Vector2d> candidates = new List<Vector2d>();

    // Générer des points autour du point Nemo
    for (float dx = -searchRadius; dx <= searchRadius; dx += 0.1f)  // Pas de 0.5 pour explorer autour
    {
        for (float dy = -searchRadius; dy <= searchRadius; dy += 0.1f)
        {
            // Calcul de la nouvelle position candidate
            double newX = nemoPoint.x + dx;
            double newY = nemoPoint.y + dy;

            // Calcul de la distance minimale pour ce nouveau point
            float dist_min = float.MaxValue;
            for (int k = 0; k < tmpBathyData.Count; k++)
            {
                a = tmpBathyData[k].vect.x - newX;
                b = tmpBathyData[k].vect.y - newY;
                dist = (float)(a * a + b * b);

                if (dist < dist_min)
                {
                    dist_min = dist;
                }
            }

            // Si cette nouvelle position est plus éloignée, on la considère
            if (dist_min > bestDist)
            {
                bestDist = dist_min;
                candidates.Add(new Vector2d(newX, newY)); // Ajouter à la liste des candidats
            }
        }
    }

    // Si on a trouvé des candidats, mettre à jour le meilleur point
    if (candidates.Count > 0)
    {
        nemoPoint = candidates
    .OrderByDescending(p => 
    {
        // Calculer la distance minimale du point candidat p aux autres points
        float dist_min = float.MaxValue;
        foreach (var point in tmpBathyData)
        {
            double a = point.vect.x - p.x;
            double b = point.vect.y - p.y;
            float dist = (float)(a * a + b * b);
            if (dist < dist_min)
            {
                dist_min = dist;
            }
        }
        return dist_min; // On trie selon la distance minimale
    })
    .First();
    }

    // Si on n'a plus de gains, on arrête l'affinage
    if (bestDist - gen_data.pp_data.nemo_distance < 0.01f)
    {
        break;
    }
}

        // Mise à jour de la distance finale
        gen_data.pp_data.nemo_distance = Mathf.Sqrt(bestDist);

        tmpBathyData.Clear();

        //debug nemo point
        double xx = nemoPoint.x-gen_data.pp_data.min.x;
        double yy = nemoPoint.y-gen_data.pp_data.min.y;

        Debug.Log("Nemo point : " + xx + " ; " + yy);

        gen_data.pp_data.surface = gen_data.limite.getSurface();

        gen_data.pp_data.density = preTraitData.Count / gen_data.pp_data.surface;

        gen_data.updateStat();


        StartCoroutine(generateRepartition());
        while( progressBarre.ProcessingCheck())
        {
            yield return new WaitForSeconds(0.01f);
        }
        

        StartCoroutine(generateDensity());
        while( progressBarre.ProcessingCheck())
        {
            yield return new WaitForSeconds(0.01f);
        }


        isProcessing2 = false;
    }
    //fonction de traitement des point nemo compatible avec le multithread
    private void th_nemoPoint(uint idx_start, uint idx_end ,  ref uint totalProgress , ref bool isDone )
    {

        for(int j = (int)idx_start; j < idx_end; j++)
        { 
            double x = (double)j ;
                //arrondire a l'inferieur 
            uint xroud = (uint)(x * gen_data.it_data.reso);
            x += gen_data.pp_data.min.x;

            for(int i = (int)gen_data.limite.getLimiteYMin( xroud ); i < gen_data.limite.getLimiteYMax( xroud ); i++)
            {

                double y =  (double)i/gen_data.it_data.reso;
                
                y += gen_data.pp_data.min.y;

                //calcule la distance entre le point et tout les autre point
                float dist_min = float.MaxValue;
                for(int k = 0; k < tmpBathyData.Count; k++)
                {
                    double a = tmpBathyData[k].vect.x - x;
                    double b = tmpBathyData[k].vect.y - y;

                    float dist = (float)(a * a + b * b);

                    if( dist < dist_min)
                    {
                        dist_min = dist;
                    }
                }

                if( dist_min > gen_data.pp_data.nemo_distance)
                {
                    gen_data.pp_data.nemo_distance = dist_min;
                    nemoPoint = new Vector2d(x, y);
                }
            }
            totalProgress++;
        }

        isDone = true;
    }

    protected override IEnumerator action()
    {

        if (isProcessing)
        {
            errManager.addWarning("Un prétraitement est déjà en cours");
            yield break;
        }

        isProcessing = true;

        exportBp.gameObject.SetActive(false);




        progressBarre.setAction("Prétraitement en cours");

        //efface les log
        File.Delete(gen_data.workingPath +"/"+   logfileName);
        preTraitData.Clear();

        bool res = segmentDefine(_importCsv.getCSVData());

        if( !res)
        {
            isProcessing = false;
            yield break;
        }
        
        StartCoroutine(offsetAndDeadApply());

        while(isProcessing2)
        {
            yield return new WaitForSeconds(0.01f);
        }


        progressBarre.setAction("Traitement des point double et supperposé");
        StartCoroutine(processDoublePoint());

        while(isProcessing2)
        {
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(this.processGenData());

        while(isProcessing2)
        {
            yield return new WaitForSeconds(0.01f);
        }

        pointTraiter.text = "Points prétraités : " + preTraitData.Count.ToString();

        progressBarre.setAction("Prétraitement terminé");

        
        if( preTraitData.Count > 0)
        {
            fwdObj.SetActive(true);
            exportBp.gameObject.SetActive(true);
            variogrameBp.gameObject.SetActive(true);

            resolution.text = Mathf.Ceil((float)gen_data.pp_data.min_resolution * gen_data.pp_data.density).ToString();
            dMax.text =  Mathf.Ceil(gen_data.pp_data.nemo_distance).ToString();
        }

        isProcessing = false;

    }



    public List<BathyPoint> getPreTraitData()
    {
        return preTraitData;
    }

    //construit un graph de densité des mesure ( courbe en cloche )
    private IEnumerator generateDensity()
    {
        //liste de point resusltat
        ListPoint listPointRes = new ListPoint();

        //fenetre de  cumul des mesure h
         

         double minZ = Mathd.Abs(gen_data.pp_data.max.z) * 0.9;
    double maxZ = Mathd.Abs(gen_data.pp_data.min.z) * 1.1;
    double h = 200 * (maxZ - minZ) / (double)preTraitData.Count;


    // Parcourt toutes les fenêtres
    for (double i = minZ + h; i < maxZ; i += h)
    {
        int n = 0;

        // Compte les mesures dans le bin [i-h, i+h)
        for (int j = 0; j < preTraitData.Count; j++)
        {
            double z = Mathd.Abs(preTraitData[j].vect.z);
            if (z >= i - h && z < i + h)
            {
                n++;
            }
        }

        // Densité : fréquence normalisée par la largeur du bin
        double density = (double)n / (preTraitData.Count * h);
        listPointRes.addPoint(new Vector2d(i, density));
    }


        _graph.clear();

        listPointRes.regressData = new ListPoint.RegressData();
        listPointRes.regressData.regressParametres = new ListPoint.RegressParametres();
        
        listPointRes.regressData.regressParametres.h = h/1.5;
        listPointRes.regressKernelGaussien();

        double intergrate = listPointRes.IntegrateEstimateur(0 , maxZ * 2 , 0.001);

        ListPoint listPointRes2 = new ListPoint();
        foreach(Vector2d v in listPointRes.getListPoint())
        {
            listPointRes2.addPoint(new Vector2d(v.x, v.y / intergrate));
        }
        //parcour la liste de point et divise tout les y par k
        listPointRes.clearPoints();
        foreach(Vector2d v in listPointRes2.getListPoint())
        {
            listPointRes.addPoint(new Vector2d(v.x, v.y));
        }
        //listPointRes.regressData.regressParametres.k = 1;
        
        _graph.setLPoints(listPointRes);
        _graph.getLPoints().getRSquare();
        _graph.autoScale();
        _graph._barycentre = new Vector2(0, 0);

        StartCoroutine( _graph.drawGraph()) ;

        while( progressBarre.ProcessingCheck() )
        {
            yield return new WaitForSeconds(0.01f);
        }

        _graph.drawEstimateCurve();

        graphDisplay.saveCurrent( GraphDisplay.IndexCurve.funcDensity );


    }

    private IEnumerator generateRepartition()
    {
        //liste de point resusltat
        ListPoint listPointRes = new ListPoint();

        //somme de tout les z 
        

        var dataCpy = new List<BathyPoint>(preTraitData);

        double sum = 0;
        foreach(BathyPoint v in dataCpy)
        {
            sum += v.vect.z;
        }

        sum = Mathd.Abs(sum);

        
        //trie par ordre croissant
        dataCpy.Sort((a, b) => a.vect.z.CompareTo(b.vect.z));
        dataCpy.Reverse();
        double cum = 0;
        
        for(int i = 0; i < dataCpy.Count; i++)
        {
            cum +=  Mathd.Abs(dataCpy[i].vect.z);
            double y = (double)i / (double)dataCpy.Count;

            listPointRes.addPoint(new Vector2d( (cum / sum) * Mathd.Abs(gen_data.pp_data.min.z) , y));
        }

        _graph.clear();
        _graph.setLPoints(listPointRes);
        //_graph.getLPoints().getRSquare();
        _graph.autoScale();
        _graph._barycentre = new Vector2(0, 0);

        StartCoroutine( _graph.drawGraph()) ;

        while( progressBarre.ProcessingCheck() )
        {
            yield return new WaitForSeconds(0.01f);
        }

        //_graph.drawEstimateCurve();

        graphDisplay.saveCurrent( GraphDisplay.IndexCurve.funcRepartition );
    }

   
}
