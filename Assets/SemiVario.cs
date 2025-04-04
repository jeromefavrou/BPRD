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

    void Start()
    {
        float tmp = gen_data.pp_data.min_distance * 2;
        h.text = tmp.ToString();
        dMax.text = Mathf.Ceil(gen_data.pp_data.nemo_distance).ToString(); 
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


        var preTraitData = pretraite.getPreTraitData();

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



        progressBarre.setAction("Calcul de la semi-variogramme 1er partie");
        progressBarre.start((uint)preTraitData.Count);
        _graph.clear();

            // Récupérer toutes les paires de points et calculer leurs distances et différences de profondeur
        List<float> distances = new List<float>();
        List<float> semivariances = new List<float>();

        for (int i = 0; i < preTraitData.Count; i++)
        {
            for (int j = i + 1; j < preTraitData.Count; j++)
            {
                // Ajouter la distance et la différence au tableau
                distances.Add((float)Vector2d.Distance(new Vector2d(preTraitData[i].vect.x, preTraitData[i].vect.y), new Vector2d(preTraitData[j].vect.x, preTraitData[j].vect.y)));
                semivariances.Add(0.5f * Mathf.Pow((float)Mathd.Abs(preTraitData[i].vect.z - preTraitData[j].vect.z), 2));
            }

            if( progressBarre.validUpdate((uint)i) )
            {
                yield return new WaitForSeconds(0.01f);
            }

        }

        progressBarre.stop();

        progressBarre.setAction("Calcul de la semi-variogramme 2eme partie");
        
       
        yield return new WaitForSeconds(0.01f);

        float filter = float.Parse(h.text);;
        int numBins = Mathf.CeilToInt( float.Parse(dMax.text)  / filter); // Le nombre de bins pour les distances
        
        progressBarre.start((uint)numBins , 0.01f);

        float minDist = 0;  
        float maxDist = 0;

        
        for (int bin = 0; bin < numBins; bin++)
        {
            minDist = bin * gen_data.pp_data.min_distance * filter;
            maxDist = (bin +1) * gen_data.pp_data.min_distance * filter;

            // Filtrer les distances dans cet intervalle
            List<float> semivarianceInBin = new List<float>();
            for (int i = 0; i < distances.Count; i++)
            {
                if (distances[i] >= minDist && distances[i] < maxDist)
                {
                    semivarianceInBin.Add(semivariances[i]);
                }
            }

            if (semivarianceInBin.Count > 0)
            {
                double avgSemivarianceValue = semivarianceInBin.Average();

                _graph.addPoint(new Vector2d(minDist + (maxDist - minDist) *0.5, avgSemivarianceValue));
            }
        
            if( progressBarre.validUpdate((uint)bin))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        progressBarre.stop();

        yield return new WaitForSeconds(0.01f);
        _graph.autoScale();
         StartCoroutine( _graph.drawGraph()) ;

        while( progressBarre.ProcessingCheck() )
        {
            yield return new WaitForSeconds(0.01f);
        }

        graphDisplay.saveCurrent( 0 );
        graphDisplay.courbeType.value = 0;
        graphDisplay.selectChange();

        progressBarre.setAction("semi-variogramme calculé");
        isProcessing = false;
    }
}
