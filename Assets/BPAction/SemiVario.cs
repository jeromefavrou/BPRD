using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
public class SemiVario : BPAction
{
    // Start is called before the first frame update
    public Graph _graph;
    public GraphDisplay graphDisplay;
    public Pretraiter pretraite;

    public TMP_InputField h;
    public TMP_InputField dMax;

    private List<BathyPoint> preTraitData = new List<BathyPoint>();
    private List<float> distances = new List<float>();
    private List<float> semivariances = new List<float>();

    private float filter = 1;
    private int numBins = 1; // Le nombre de bins pour les distances

    void Start()
    {
        float tmp = gen_data.pp_data.min_distance * 2;
        h.text = tmp.ToString("F2");
        float max = Mathf.Sqrt((float)(gen_data.pp_data.size.x * gen_data.pp_data.size.x + gen_data.pp_data.size.y * gen_data.pp_data.size.y));
        dMax.text = Mathf.Ceil(max).ToString();
    }
    
    void OnGUI()
    {
        
        if( h.text == "" )
        {
             h.text = gen_data.pp_data.min_distance.ToString();
        }

        if( dMax.text == "" )
        {
            dMax.text = gen_data.pp_data.nemo_distance.ToString();
        }
    }

    protected override IEnumerator action()
    {
         float max = Mathf.Sqrt((float)(gen_data.pp_data.size.x * gen_data.pp_data.size.x + gen_data.pp_data.size.y * gen_data.pp_data.size.y));
        if(float.Parse(h.text) < gen_data.pp_data.min_distance)
        {
            h.text = gen_data.pp_data.min_distance.ToString();
        }
        else if(float.Parse(h.text) > max)
        {
            h.text = max.ToString();
        }
        
        if(float.Parse(dMax.text) <  gen_data.pp_data.nemo_distance)
        {
            dMax.text = gen_data.pp_data.nemo_distance.ToString();
        }
        else if(float.Parse(dMax.text) > max)
        {
            dMax.text = max.ToString();
        }


        preTraitData = pretraite.getPreTraitData();

        if( preTraitData.Count == 0)
        {
            errManager.addWarning("Aucune donnée à traiter");
            yield break;
        }

        if( gen_data.pp_data.min_distance == 0)
        {
            errManager.addWarning("Aucune donnée à traiter");
            yield break;
        }

        isProcessing = true;

        _graph.clear();

        // Récupérer toutes les paires de points et calculer leurs distances et différences de profondeur
        distances = new List<float>();
        semivariances = new List<float>();

        ThreadSegment thread = new ThreadSegment((uint)preTraitData.Count);
        
        progressBarre.setAction("Calcul de la semi-variogramme 1er partie [" + thread.get_nThreads() + " threads]");
        progressBarre.start((uint)preTraitData.Count);

        thread.Execute( th_firstPart );

        while(thread.inProcess())
        {
            if( progressBarre.validUpdate((uint)thread.totalProgress))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        //attente des thread
        thread.WaitForAllThreads();

        progressBarre.stop();

        try
        {
            filter = float.Parse(h.text);
            numBins = Mathf.CeilToInt( float.Parse(dMax.text)  / filter); // Le nombre de bins pour les distances

            if (numBins < 1)
                numBins = 1;
            

        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }

        thread = new ThreadSegment((uint)numBins);

        progressBarre.setAction("Calcul de la semi-variogramme 2eme partie [" + thread.get_nThreads() + " threads]");
        progressBarre.start((uint)numBins , 0.01f);

        thread.Execute( th_secondePart );

        while(thread.inProcess())
        {
            if( progressBarre.validUpdate((uint)thread.totalProgress))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        //attente des thread
        thread.WaitForAllThreads();

        progressBarre.stop();
        

        yield return new WaitForSeconds(0.01f);
        _graph.autoScale();
         StartCoroutine( _graph.drawGraph()) ;

        while( progressBarre.ProcessingCheck() )
        {
            yield return new WaitForSeconds(0.01f);
        }

        _graph.getLPoints().regressData = new ListPoint.RegressData();
        _graph.getLPoints().regressData.regressParametres = new ListPoint.RegressParametres();
        _graph.getLPoints().regressData.regressParametres.h = float.Parse(h.text);


        graphDisplay.saveCurrent( GraphDisplay.IndexCurve.SemiVario); 


        progressBarre.setAction("semi-variogramme calculé");
        isProcessing = false;
    }

    // Fonction exécutée par chaque thread
    // Elle calcule les distances et les différences de profondeur entre les points dans la plage donnée
    // et les ajoute à la liste des distances et des différences de profondeur
    // Utilisation de lock pour éviter les problèmes de concurrence lors de l'ajout à la liste partagée
    private void th_firstPart(uint idx_start, uint idx_end ,  ref uint totalProgress , ref bool isDone )
    {
        float dist = 0;
        float semi = 0;
        float tmp_delta =0;

        List<float> tmp_distances = new List<float>();
        List<float> tmp_semivariances = new List<float>();

        for (int i = (int)idx_start; i < idx_end; i++)
        {
            for (int j = i + 1; j < preTraitData.Count; j++)
            { 
                dist =(float)Vector2d.Distance(new Vector2d(preTraitData[i].vect.x, preTraitData[i].vect.y), new Vector2d(preTraitData[j].vect.x, preTraitData[j].vect.y));
                
                // Ajouter la distance et la différence au tableau

                tmp_delta = (float)preTraitData[i].vect.z - (float)preTraitData[j].vect.z;
                
                semi = 0.5f * tmp_delta * tmp_delta;

                tmp_distances.Add(dist);
                tmp_semivariances.Add(semi);  
            }

            totalProgress++;
        }

        lock (this)
        {
            distances.AddRange(tmp_distances);
            semivariances.AddRange(tmp_semivariances);
        }

        isDone = true;

    }

    private void th_secondePart(uint idx_start, uint idx_end ,  ref uint totalProgress , ref bool isDone )
    {
        
        float minDist = 0;  
        float maxDist = 0;

        List<Vector2d> resPoints = new List<Vector2d>();

        for (int bin = (int)idx_start; bin < idx_end; bin++)
        {
            minDist = bin  * filter;
            maxDist = (bin +1)  * filter;

            // Filtrer les distances dans cet intervalle
            List<float> semivarianceInBin = new List<float>();

            for (int i = 0; i < distances.Count; i++)
                if (distances[i] >= minDist && distances[i] < maxDist)
                    semivarianceInBin.Add(semivariances[i]);
                
            if (semivarianceInBin.Count > 0)
                resPoints.Add(new Vector2d(minDist + (maxDist - minDist) *0.5, semivarianceInBin.Average() ));
            
            totalProgress++;
        }

        // Ajout des points à la liste partagée
        // Utilisation de lock pour éviter les problèmes de concurrence lors de l'ajout à la liste partagée
        lock (this)
        {
            for (int i = 0; i < resPoints.Count; i++)
                _graph.addPoint(resPoints[i]);
        }

        isDone = true;
    }

}
