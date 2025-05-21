using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Interpolate 
{
    
    private GeneralStatUtils gen_data;
    private List<BathyPoint> data ;

    private ProgressBarre progressBarre;

    private Error _error;

    private List<BathyPoint> tmpData = new List<BathyPoint>();

    private ListPoint.RegressData.Estimateur estimateur = null;


    public void set_utility(ref Error error , ref GeneralStatUtils _gen_data , ref List<BathyPoint> _data  , ref ProgressBarre _progressBarre , ListPoint.RegressData.Estimateur _estimateur)
    {
        estimateur = _estimateur;
        gen_data = _gen_data;
        data = _data;

        progressBarre = _progressBarre;
        _error = error;

    }


    public double nearestNeighbor(List<BathyPoint> _data , Vector2d _point , double dmax)
    {
        double dx = 0;
        double dy = 0;
        double dist = 0;
        double distanceSave = double.MaxValue;
        double result = 1;

        dmax = dmax * dmax;
        
        foreach(BathyPoint p in _data)
        {

            // Utilisation des composants x et y directement, pas besoin de créer un Vector2
            dx = p.vect.x - _point.x;
            dy = p.vect.y - _point.y;

            dist = dx * dx + dy * dy; // Distance carrée

            if( dist > dmax)
            {
                continue;
            }

            if (dist < distanceSave)
            {
                distanceSave = dist;
                result = p.vect.z;
            }
        }

        return result;
    }

    public double IDW(List<BathyPoint> _data, Vector2d _point , int pow , double dmax)
    {
        double sum1 = 0;
        double sum2 = 0;
        double weight = 0;
        double dx = 0;
        double dy = 0;
        double dist = 0;

        if( pow < 1)
        {
            pow = 1;
        }

        dmax = dmax * dmax;


        if (pow == 1)
        {
            foreach (BathyPoint p in _data)
            {

                dx = p.vect.x - _point.x;
                dy = p.vect.y - _point.y;

                dist =  dx * dx +  dy * dy;

                if (dist > dmax)
                {
                    continue;
                }

                if (dist == 0)
                {
                    return p.vect.z;
                }

                weight = 1.0f / Mathd.Sqrt(dist);
                sum1 += weight * p.vect.z;
                sum2 += weight;
            }
        }
        else if (pow == 2)
        {
            foreach (BathyPoint p in _data)
            {
                //ignorer les données de confirmation

                dx = p.vect.x - _point.x;
                dy = p.vect.y - _point.y;

                dist = dx * dx + dy * dy;

                if (dist > dmax)
                {
                    continue;
                }

                if (dist == 0)
                {
                    return p.vect.z;
                }

                weight = 1.0f / (dist);
                sum1 += weight * p.vect.z;
                sum2 += weight;
            }
        }
        else 
        {
            foreach (BathyPoint p in _data)
            {

                dx = p.vect.x - _point.x;
                dy = p.vect.y - _point.y;

                dist =dx * dx + dy * dy;

                if (dist > dmax)
                {
                    continue;
                }

                if (dist == 0)
                {
                    return p.vect.z;
                }

                dist =  Mathd.Sqrt(dist);

                weight = 1.0f / Mathd.Pow(dist , pow);
                sum1 += weight * p.vect.z;
                sum2 += weight;
            }
        }

        double res = sum1 / sum2;

        if( double.IsNaN(res))
        {
            return 1;
        }

        return res;
    }


public double OrdinaryKriging(List<BathyPoint> data, Vector2d targetPoint, int maxNeighbors = 20 )
{
    if (estimateur == null)
        return 0.0; // pas d'estimateur
    // Étape 1 : calculer toutes les distances
    List<(double distSq, BathyPoint point)> allPoints = new List<(double, BathyPoint)>();
    foreach (var p in data)
    {
        double dx = p.vect.x - targetPoint.x;
        double dy = p.vect.y - targetPoint.y;
        double distSq = dx * dx + dy * dy;
        allPoints.Add((distSq, p));
    }

    // Étape 2 : trier par distance croissante
    allPoints.Sort((a, b) => a.distSq.CompareTo(b.distSq));

    // Étape 3 : prendre les k plus proches
    int k = Mathd.Min(maxNeighbors, allPoints.Count);
    List<BathyPoint> neighbors = allPoints.Take(k).Select(p => p.point).ToList();

    if (neighbors.Count < 3)
        return 0.0; // trop peu de voisins

    // Étape 4 : déterminer range automatiquement
    double maxDist = Mathd.Sqrt(allPoints[k - 1].distSq);
    // Étape 5 : construire la matrice et le RHS
    int m = neighbors.Count;
    double[,] matrix = new double[m + 1, m + 1];
    double[] rhs = new double[m + 1];

    for (int i = 0; i < m; i++)
    {
        for (int j = 0; j < m; j++)
        {
            double dx = neighbors[i].vect.x - neighbors[j].vect.x;
            double dy = neighbors[i].vect.y - neighbors[j].vect.y;
            double dist = Mathd.Sqrt(dx * dx + dy * dy);

            matrix[i, j] = estimateur(dist); // <-- ou SemiVariogram
        }

        matrix[i, m] = 1.0;
        matrix[m, i] = 1.0;

        double dxi = neighbors[i].vect.x - targetPoint.x;
        double dyi = neighbors[i].vect.y - targetPoint.y;
        double distToPoint = Mathd.Sqrt(dxi * dxi + dyi * dyi);
        rhs[i] = estimateur(distToPoint); // <-- ou SemiVariogram
    }

    matrix[m, m] = 0.0;
    rhs[m] = 1.0;

    // Étape 6 : résoudre le système
    double[] weights = SolveLinearSystem(matrix, rhs);

    // Étape 7 : sécurité et vérification
    double sumWeights = weights.Take(m).Sum();
    if (Mathd.Abs(sumWeights - 1.0) > 0.01)
        Debug.LogWarning("Somme des poids ≠ 1 : " + sumWeights);

    // Étape 8 : appliquer les poids
    double result = 0.0;
    for (int i = 0; i < m; i++)
    {
        result += weights[i] * neighbors[i].vect.z;
    }

    return result;
}


public double[] SolveLinearSystem(double[,] A, double[] b)
{
    int n = b.Length;
    double[,] M = new double[n, n];
    double[] x = new double[n];
    double[] B = new double[n];

    // Copier A et b (pour éviter les effets de bord)
    for (int i = 0; i < n; i++)
    {
        B[i] = b[i];
        for (int j = 0; j < n; j++)
        {
            M[i, j] = A[i, j];
        }
    }

    // Élimination de Gauss avec pivot partiel
    for (int k = 0; k < n; k++)
    {
        // Trouver le pivot
        int max = k;
        for (int i = k + 1; i < n; i++)
        {
            if (Mathd.Abs(M[i, k]) > Mathd.Abs(M[max, k]))
            {
                max = i;
            }
        }

        // Échanger lignes k et max
        for (int j = 0; j < n; j++)
        {
            double temp = M[k, j];
            M[k, j] = M[max, j];
            M[max, j] = temp;
        }
        double tmpB = B[k];
        B[k] = B[max];
        B[max] = tmpB;

        // Triangularisation
        for (int i = k + 1; i < n; i++)
        {
            double factor = M[i, k] / M[k, k];
            B[i] -= factor * B[k];
            for (int j = k; j < n; j++)
            {
                M[i, j] -= factor * M[k, j];
            }
        }
    }

    // Substitution arrière
    for (int i = n - 1; i >= 0; i--)
    {
        double sum = 0.0;
        for (int j = i + 1; j < n; j++)
        {
            sum += M[i, j] * x[j];
        }
        x[i] = (B[i] - sum) / M[i, i];
    }

    return x;
}

    public IEnumerator interpolation(  )
    {
        if(gen_data.it_data.interpolationType == 4)
        {
            if(estimateur == null)
            {
                _error.addError("Estimateur non assigné pour l'interpolation");
                yield break;
            }
        }
        //gestion des cas d'erreur
        if( data == null)
        {
            _error.addError("Data non assigné");
            yield break;
        }
        if( gen_data == null)
        {
             _error.addError("GeneralStatUtils non assigné");
            yield break;
        }


        if( gen_data.it_data.interpolationType < 0 || gen_data.it_data.interpolationType > 4)
        {
             _error.addError("Type d'interpolation inconnu");
            yield break;
        }

        if( gen_data.it_data.data == null)
        {
             _error.addError("InterpolateData non assigné");
            yield break;
        }
        
        if( progressBarre.ProcessingCheck())
        {
             _error.addWarning("interpoaltion deja en cours");
            yield break;
        }



        gen_data.workingTexture = new Texture2D(gen_data.it_data.size.x, gen_data.it_data.size.y);

        if( gen_data.workingTexture == null)
        {   
            progressBarre.stop();
             _error.addError("Texture non assigné");
            yield break;
        }

        gen_data.workingTexture.filterMode = FilterMode.Point;
        gen_data.workingTexture.wrapMode = TextureWrapMode.Clamp;

        Color fillCol = Color.white;
        fillCol.a = 0;

        BathyGraphie2D.fillTexture(ref gen_data.workingTexture, fillCol);

        gen_data.limite.setReso(gen_data.it_data.reso);

        tmpData = new List<BathyPoint>( data);

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
        }

        ThreadSegment thread = new ThreadSegment((uint)gen_data.it_data.size.x);        
        
        progressBarre.setAction("Interpolation en cours[" + thread.get_nThreads() + " threads]");
        progressBarre.start((uint)gen_data.it_data.size.x , 0.01f);

        thread.Execute( th_interpolate );

        while(thread.inProcess())
        {
            if( progressBarre.validUpdate((uint)thread.totalProgress))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        //attente des thread
        thread.WaitForAllThreads();


        // Remplissage de la texture avec les données interpolées
        for (int x = 0; x < gen_data.it_data.size.x ; x++)
        {
            for (int y = (int)gen_data.limite.getLimiteYMin((uint)(x)); y < (int)gen_data.limite.getLimiteYMax((uint)(x)) ; y++)
            {  
                Color tmpColor =BathyGraphie2D.bathyColor(gen_data.it_data.data[x, y], gen_data.pp_data.min.z, gen_data.pp_data.max.z);
                tmpColor.a = 1;
                gen_data.workingTexture.SetPixel(x, y,tmpColor);
            }
        }

       progressBarre.stop();

        yield return null;
    
    }

    private void th_interpolate(uint idx_start, uint idx_end ,  ref uint totalProgress , ref bool isDone )
    {
        // Interpolation avec pause pour éviter le blocage
        //cre une copie de tmpData pour chaque thread

        for (int x = (int)idx_start; x < idx_end ; x++)
        {
            for (int y = (int)gen_data.limite.getLimiteYMin((uint)(x)) ; y < (int)gen_data.limite.getLimiteYMax((uint)(x)) ; y++)
            { 
                var vect_target_abs = gen_data.abs_vect2( new Vector2d((double)(x+0.5) / gen_data.it_data.reso, (double)(y+0.5) / gen_data.it_data.reso));

                    
                if (gen_data.it_data.interpolationType == 0)
                    gen_data.it_data.data[x, y] = nearestNeighbor(tmpData, vect_target_abs, gen_data.it_data.dMax);
                else if (gen_data.it_data.interpolationType == 1)
                    gen_data.it_data.data[x, y] = IDW(tmpData, vect_target_abs, 1, gen_data.it_data.dMax);
                else if (gen_data.it_data.interpolationType == 2)
                    gen_data.it_data.data[x, y] = IDW(tmpData, vect_target_abs, 2, gen_data.it_data.dMax);
                else if (gen_data.it_data.interpolationType == 3)
                    gen_data.it_data.data[x, y] = IDW(tmpData, vect_target_abs, 3, gen_data.it_data.dMax);
                else if (gen_data.it_data.interpolationType == 4)
                {
                    if (estimateur == null)
                        gen_data.it_data.data[x, y] = 0;
                    else
                        gen_data.it_data.data[x, y] = OrdinaryKriging(tmpData, vect_target_abs , gen_data.it_data.nNeighbors);
                }
                    
                
                
            }

            totalProgress++;
        }


        isDone = true;

    }


}
