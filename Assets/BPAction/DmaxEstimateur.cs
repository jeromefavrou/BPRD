using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class DmaxEstimateur : BPAction
{
    // Start is called before the first frame update
    public TMP_InputField Nneighbours;
    public TMP_InputField Dmax;
    public Pretraiter pretraite;

    private List<BathyPoint> tmpData = new List<BathyPoint>();
    private int maxNeighbors = 0;

    private List<double > distances = new List<double>();

    void Start()
    {
        
    }

    // Update is called once per frame
    protected override IEnumerator action()
    {
        //donné sans les donné de confirmation
        tmpData = gen_data.subConfirmData(pretraite.getPreTraitData());

        maxNeighbors = int.Parse(Nneighbours.text);
        if (maxNeighbors < 3)
        {
            maxNeighbors = 3;
        }
        else if (maxNeighbors > tmpData.Count-1)
        {
            maxNeighbors = tmpData.Count - 1;
        }

        Nneighbours.text = maxNeighbors.ToString();

        distances.Clear();



        ThreadSegment thread = new ThreadSegment((uint)gen_data.it_data.size.x);

        progressBarre.setAction("Estimation de la distance max [" + thread.get_nThreads() + " threads]");
        
        progressBarre.start((uint)gen_data.it_data.size.x , 0.01f);

        thread.Execute( th_estimateDmax );
        while(thread.inProcess())
        {
            if( progressBarre.validUpdate((uint)thread.totalProgress))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        //attente des thread
        thread.WaitForAllThreads();

        

        double dMax = 0;
        for (int i = 0; i < distances.Count; i++)
        {
            dMax = Mathd.Max(dMax, distances[i]);
        }

        dMax = Mathd.Sqrt(dMax);

        progressBarre.setAction("Estimation de la distance max fini : " + dMax.ToString("F2"));
        progressBarre.stop();

        //debug
        Debug.Log("Distance max : " + dMax.ToString());
        dMax *= 1.1;
        dMax = Mathd.Ceil(dMax);

        Dmax.text = dMax.ToString("F2");

        yield return new WaitForSeconds(0.1f);
    }

    private void th_estimateDmax(uint idx_start, uint idx_end ,  ref uint totalProgress , ref bool isDone )
    {
        double maxDistance = 0;
        int _maxNeighbors = maxNeighbors ;
        
        for (int x = (int)idx_start; x < idx_end ; x++)
        {
            for (int y = (int)gen_data.limite.getLimiteYMin((uint)(x)); y < (int)gen_data.limite.getLimiteYMax((uint)(x)) ; y++)
            {
                var targetPoint = gen_data.abs_vect2( new Vector2d((double)(x+0.5) / gen_data.it_data.reso, (double)(y+0.5) / gen_data.it_data.reso));

                List<(double distSq, BathyPoint point)> allPoints = new List<(double, BathyPoint)>();

                foreach (var p in tmpData)
                {
                    double dx = p.vect.x - targetPoint.x;
                    double dy = p.vect.y - targetPoint.y;
                    double distSq = dx * dx + dy * dy;
                    allPoints.Add((distSq, p));
                }

                // Étape 2 : trier par distance croissante
                allPoints.Sort((a, b) => a.distSq.CompareTo(b.distSq));

                // Étape 3 : prendre les k plus proches
                int k = Mathd.Min(_maxNeighbors, allPoints.Count);
                List<BathyPoint> neighbors = allPoints.Take(k).Select(p => p.point).ToList();

                maxDistance = Mathd.Max(maxDistance, allPoints[k - 1].distSq);
            }

            totalProgress++;
        }

        lock (this)
        {
            distances.Add(maxDistance);
        }

        isDone = true;
    }

    
}
