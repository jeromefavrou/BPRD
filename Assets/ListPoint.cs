using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ListPoint 
{
    private List<Vector2d>  listPoint = new List<Vector2d>();

    public class RegressParametres
    {
        public double a;
        public double b;

        public double h; // Paramètre de lissage pour la régression non paramétrique
        public double k; // Coefficient de normalisation pour la régression non paramétrique
        public double nugget;

        public RegressParametres(double a =0, double b = 0 , double h = 0 , double k = 1 , double nugget = 0)
        {
            this.a = a;
            this.b = b;
            this.h = h;
            this.k = k;
            this.nugget = nugget;
        }


    }

    public class RegressData
    {
        public double StdRMSE;
        public double XYRMSE;
        public double rSquare;
        public double rPearson;

        public RegressParametres regressParametres = null;

        public delegate double Estimateur( double x );
        public Estimateur estimateur = null;

        public RegressData(double rmse = 0,double xyrmse =0,double rSquare = 0, double rPearson = 0, RegressParametres regressParametres = null)
        {
            this.StdRMSE = rmse;
            this.XYRMSE = xyrmse;
            this.rSquare = rSquare;
            this.rPearson = rPearson;
            this.regressParametres = regressParametres;
        }

        public RegressData(RegressData data)
        {
            if( data == null)
            {
                return;
            }
            this.StdRMSE = data.StdRMSE;
            this.XYRMSE = data.XYRMSE;
            this.rSquare = data.rSquare;
            this.rPearson = data.rPearson;
            this.estimateur = data.estimateur;

            if(data.regressParametres != null)
            {
                this.regressParametres = new RegressParametres(data.regressParametres.a, data.regressParametres.b , data.regressParametres.h ,  data.regressParametres.k , data.regressParametres.nugget);
            }
        }
    }

    public RegressData regressData = null;


    public Vector2d barycentre ;

    public ListPoint()
    {
        listPoint = new List<Vector2d>();
    }

    //ocntructeur de copy
    public ListPoint(ListPoint _listPoint)
    {
        this.listPoint = new List<Vector2d>(_listPoint.getListPoint());
        this.barycentre = new Vector2d(_listPoint.barycentre.x , _listPoint.barycentre.y);
        this.regressData = new RegressData(_listPoint.regressData);
    }

    public void addPoint(Vector2d point)
    {
        listPoint.Add(point);
    }

    public void clear()
    {
        listPoint.Clear();
        regressData = null;
        barycentre = new Vector2d(0, 0);
    }

    public void clearPoints()
    {
        listPoint.Clear();
    }

    public List<Vector2d> getListPoint()
    {
        return listPoint;
    }

    public double getStandardRMSE()
    {   
        if(  regressData == null)
        {
            linearRegression();
        }
        else if(regressData.regressParametres == null)
        {
            linearRegression();
        }
        

        double sum = 0;


        foreach (Vector2d point in listPoint)
        {
            sum += Math.Pow(point.y - (regressData.regressParametres.a * point.x + regressData.regressParametres.b), 2);
        }   


        regressData.StdRMSE =Math.Sqrt(sum / listPoint.Count);

        return regressData.StdRMSE;

    }

    public double getXYRMSE()
    {
        if(  regressData == null)
        {
            regressData = new RegressData();
        }
        
        
        double sum = 0;

         foreach (Vector2d point in listPoint)
        {
            sum += Math.Pow(point.y - point.x, 2);
        }   

        regressData.XYRMSE = Math.Sqrt(sum / listPoint.Count);

        return regressData.XYRMSE;
    }
    

    public double getRSquare()
    {
        // R² = 1 - (Σ(yi - ŷi)² / Σ(yi - ȳ)²)
        if (listPoint.Count < 2) return double.NaN; // Sécurité contre la division par zéro

        calcBarycentre(); // Assure que barycentre est bien calculé avant de l'utiliser

        double sumYiYhat = 0;
        double sumYiYbar = 0;

        foreach (Vector2d point in listPoint)
        {
            sumYiYhat += Math.Pow(point.y - regressData.estimateur(point.x), 2);
            sumYiYbar += Math.Pow(point.y - barycentre.y, 2);
        }

        regressData.rSquare = 1 - (sumYiYhat / sumYiYbar);
        return regressData.rSquare;
    }

    public double getRPearson()
    {
        if (listPoint.Count < 2) return double.NaN; // Sécurité contre la division par zéro

        calcBarycentre(); // Assure que barycentre est bien calculé avant de l'utiliser
        double covXY = getCov();

        double stdX = 0;
        double stdY = 0;

        foreach (Vector2d point in listPoint)
        {
            stdX += Math.Pow(point.x - barycentre.x, 2);
            stdY += Math.Pow(point.y - barycentre.y, 2);
        }

        stdX = Math.Sqrt(stdX / (listPoint.Count - 1));
        stdY = Math.Sqrt(stdY / (listPoint.Count - 1));

        if (stdX == 0 || stdY == 0) return double.NaN; // Évite la division par zéro

        regressData.rPearson = covXY / (stdX * stdY);
        return regressData.rPearson;
    }

    public double getCov()
    {
        if (listPoint.Count < 2) return double.NaN;

        calcBarycentre(); // Assure le calcul du barycentre avant d'utiliser ses valeurs

        double sum = 0;
        foreach (Vector2d point in listPoint)
        {
            sum += (point.x - barycentre.x) * (point.y - barycentre.y);
        }

        return sum / (listPoint.Count - 1);
    }

    public Vector2d calcBarycentre()
    {
        if (listPoint.Count == 0) return new Vector2d(0, 0);

        barycentre = new Vector2d(0, 0);
        foreach (Vector2d point in listPoint)
        {
            barycentre += point;
        }
        barycentre /= listPoint.Count;
        return barycentre;
    }

    private void evaluate(ref double param , double start , double end , double step )
    {
        double maxValue = 0;
        double maxRange = 0;

        if( step > 0)
        {
            for(double e = start ; e < end ; e+=step)
            {
                param = e ;
                
                double value = this.getRSquare();

                if( value > maxValue)
                {

                    maxValue = value;
                    maxRange = e;
                }
            }
        }
        else
        {
            for(double e = start ; e > end ; e+=step)
            {
                param = e ;
                
                double value = this.getRSquare();

                if( value > maxValue)
                {

                    maxValue = value;
                    maxRange = e;
                }
            }
        }

        param = maxRange;
    } 


    public RegressParametres linearRegression()
    {
        if(regressData == null)
        {
            regressData = new RegressData();
            regressData.regressParametres = new RegressParametres();
        }

        if( regressData.regressParametres == null)
        {
            regressData.regressParametres = new RegressParametres();
        }

        calcBarycentre();

        //calcul de a
        double num = 0;
        double den = 0;

        double dx = 0;
        double dy = 0;
        
        foreach (Vector2d point in listPoint)
        {
            dx = point.x - barycentre.x;
            dy = point.y - barycentre.y;

            num += dx * dy;
            den += dx * dx;
        }
        regressData.regressParametres.a = num / den;

        //calcul de b

        regressData.regressParametres.b = barycentre.y - regressData.regressParametres.a * barycentre.x;

        regressData.estimateur = linearEstimate;

        return regressData.regressParametres;
    }

    public void regressKernelGaussien()
    {
        if(regressData == null)
        {
            regressData = new RegressData();
            regressData.regressParametres = new RegressParametres();
        }

        if( listPoint.Count == 0)
        {
            Debug.LogError("Erreur : pas de points pour la régression par noyau gaussien.");
            return;
        }



        //a calcule l'estimation pour le premier point
        double yEstim = 0;
        double yreal = 0;
        
        regressData.regressParametres.k = 1;
        regressData.regressParametres.nugget = 0;
        foreach (Vector2d point in listPoint)
        {
            yEstim += kernelGaussianEstimate(point.x);
            yreal += point.y;
        }
        
    
        regressData.regressParametres.k = yreal/ yEstim ; // Normalisation de l'estimation


        regressData.estimateur = kernelGaussianEstimate;

    }

    public void regressSpherique()
    {
        if(regressData == null)
        {
            regressData = new RegressData();
            regressData.regressParametres = new RegressParametres();
        }

        if( listPoint.Count == 0)
        {
            Debug.LogError("Erreur : pas de points pour la régression sphérique.");
            return;
        }


        var min = minBound();
        var max = maxBound();


        //si un point est proche de 0 ont admet un effet pepite 
        if( min.x < 0.05)
            regressData.regressParametres.nugget = min.y;
    

        regressData.regressParametres.b = 0.01;
        regressData.regressParametres.a = max.y;

        regressData.estimateur = spheriqueEstimate;

        //on calcule la valeur range "b" en augmentant progressivement la valeur de b jusqu'à ce que l'estimation soit au maximum
        double apStep = max.x/10.0;

        evaluate(ref regressData.regressParametres.b , 0.01 , max.x , apStep); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , max.y , 0 , -0.01); //on calcule la valeur de a

        evaluate(ref regressData.regressParametres.b , regressData.regressParametres.b - apStep , regressData.regressParametres.b+apStep , 0.001); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , regressData.regressParametres.a + 0.01 , regressData.regressParametres.a -0.01 , -0.001); //on calcule la valeur de a

    }

    public void regressExp()
    {
        if(regressData == null)
        {
            regressData = new RegressData();
            regressData.regressParametres = new RegressParametres();
        }

        if( listPoint.Count == 0)
        {
            Debug.LogError("Erreur : pas de points pour la régression exponentielle.");
            return;
        }


        var min = minBound();
        var max = maxBound();


        //si un point est proche de 0 ont admet un effet pepite 
        if( min.x < 0.05)
            regressData.regressParametres.nugget = min.y;
    

        regressData.regressParametres.b = 0.01;
        regressData.regressParametres.a = max.y;

        regressData.estimateur = expEstimate;

        //on calcule la valeur range "b" en augmentant progressivement la valeur de b jusqu'à ce que l'estimation soit au maximum
        double apStep = max.x/10.0;

        evaluate(ref regressData.regressParametres.b , 0.01 , max.x , apStep); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , max.y , 0 , -0.01); //on calcule la valeur de a

        evaluate(ref regressData.regressParametres.b , regressData.regressParametres.b - apStep , regressData.regressParametres.b+apStep , 0.001); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , regressData.regressParametres.a + 0.01 , regressData.regressParametres.a -0.01 , -0.001); //on calcule la valeur de a

    }

    public void regressGaussien()
    {
        if(regressData == null)
        {
            regressData = new RegressData();
            regressData.regressParametres = new RegressParametres();
        }

        if( listPoint.Count == 0)
        {
            Debug.LogError("Erreur : pas de points pour la régression gaussienne.");
            return;
        }


        var min = minBound();
        var max = maxBound();


        //si un point est proche de 0 ont admet un effet pepite 

        regressData.regressParametres.nugget = min.y;
    

        regressData.regressParametres.b = 0.01;
        regressData.regressParametres.a = max.y;

        regressData.estimateur = gaussianEstimate;

        //on calcule la valeur range "b" en augmentant progressivement la valeur de b jusqu'à ce que l'estimation soit au maximum
        double apStep = max.x/10.0;

        evaluate(ref regressData.regressParametres.b , 0.01 , max.x , apStep); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , max.y , 0 , -0.01); //on calcule la valeur de a

        evaluate(ref regressData.regressParametres.b , regressData.regressParametres.b - apStep , regressData.regressParametres.b+apStep , 0.001); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , regressData.regressParametres.a + 0.01 , regressData.regressParametres.a -0.01 , -0.001); //on calcule la valeur de a

    }

    public void regressSinusCardinal()
    {
        if(regressData == null)
        {
            regressData = new RegressData();
            regressData.regressParametres = new RegressParametres();
        }

        if( listPoint.Count == 0)
        {
            Debug.LogError("Erreur : pas de points pour la régression par sinus cardinal.");
            return;
        }


        var min = minBound();
        var max = maxBound();


        regressData.regressParametres.nugget = min.y;

        regressData.regressParametres.b = 0.01;
        regressData.regressParametres.a = max.y;

        regressData.estimateur = sinusCardianlEstimate;

        //on calcule la valeur range "b" en augmentant progressivement la valeur de b jusqu'à ce que l'estimation soit au maximum
        double apStep = max.x/10.0;

        evaluate(ref regressData.regressParametres.b , 0.01 , max.x , 0.01); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , max.y , 0 , -0.01); //on calcule la valeur de a

        evaluate(ref regressData.regressParametres.b , regressData.regressParametres.b - 0.01 , regressData.regressParametres.b+0.01 , 0.001); //on calcule la valeur de b
        evaluate(ref regressData.regressParametres.a , regressData.regressParametres.a + 0.01 , regressData.regressParametres.a -0.01 , -0.001); //on calcule la valeur de a

    }

    public Vector2d minBound()
    {
        Vector2d min = new Vector2d(double.MaxValue, double.MaxValue);

        foreach (Vector2d point in listPoint)
        {
            if (point.x < min.x)
            {
                min.x = point.x;
            }
            if (point.y < min.y)
            {
                min.y = point.y;
            }
        }

        return min;
    }

    public Vector2d maxBound()
    {
        Vector2d max = new Vector2d(double.MinValue, double.MinValue);

        foreach (Vector2d point in listPoint)
        {
            if (point.x > max.x)
            {
                max.x = point.x;
            }
            if (point.y > max.y)
            {
                max.y = point.y;
            }
        }

        return max;
    }

    public double linearEstimate(double x)
    {
        if( x <= 0)
            return 0;
        
        return regressData.regressParametres.a * x + regressData.regressParametres.b;
    }

    public double expEstimate(double x)
    {
        if( x <= 0 || regressData.regressParametres.b <= 0)
            return 0;

        return regressData.regressParametres.nugget + regressData.regressParametres.a * (1-Math.Exp(-x/regressData.regressParametres.b));
    }

    public double gaussianEstimate(double x)
    {
        if( x <= 0 || regressData.regressParametres.b <= 0)
            return 0;

        double k =x/regressData.regressParametres.b;

        return regressData.regressParametres.nugget + regressData.regressParametres.a * (1 - Math.Exp( -k*k ));
    }

    public double spheriqueEstimate(double x)
    {
        if( x <= 0)
            return 0;

        if( x > regressData.regressParametres.b)
            return regressData.regressParametres.nugget + regressData.regressParametres.a;

        double k = x/regressData.regressParametres.b;

        return regressData.regressParametres.nugget + regressData.regressParametres.a * ( 1.5 * k - 0.5 * Math.Pow(k, 3) );
    }

    public double sinusCardianlEstimate(double x)
    {
        if( x <= 0 || regressData.regressParametres.b <= 0)
            return 0;

       return regressData.regressParametres.nugget+regressData.regressParametres.a * ( 1 -  (regressData.regressParametres.b/x) * Math.Sin(x/regressData.regressParametres.b) ) ;
    }

public double kernelGaussianEstimate(double x)
{
    if(x <= 0)
        return 0;

    double h_fenetre = regressData.regressParametres.h; // Largeur de la fenêtre
    const double PI = 3.1415926535897931;
    double estimation = 0.0;
    double n = (double)listPoint.Count;

    // Facteur de normalisation global pour la gaussienne
    double normalisationGaussienne = 1.0 / (Math.Sqrt(2.0 * PI) * h_fenetre);  // Normalisation de l'intégrale du noyau gaussien

    // Calcul de l'estimation en parcourant tous les points
    foreach (Vector2d point in listPoint)
    {
        // Calcul de la distance au carré entre le point x et chaque point de données
        double distance = x - point.x; // distance entre x et le point
        double kernelValue = point.y * Math.Exp(-Math.Pow(distance / h_fenetre, 2.0) / 2.0);  // Noyau gaussien sans normalisation immédiate

        // Ajout du noyau au calcul de l'estimation
        estimation += kernelValue;
    }

    // Normalisation globale
    estimation *= regressData.regressParametres.k*normalisationGaussienne / n;  // Normalisation finale par le nombre de points pour obtenir une estimation équilibrée

    return  regressData.regressParametres.nugget + estimation;
}

public double IntegrateEstimateur(double xMin, double xMax, double steps)
{
    if (regressData == null || regressData.estimateur == null)
    {
        Debug.LogError("Erreur : estimateur non défini.");
        return 0;
    }

    double sum = 0.0;
    for (double i = xMin; i < xMax; i += steps)
    {
        double x0 = i;
        double x1 = Math.Min(i + steps, xMax);
        sum += 0.5 * (regressData.estimateur(x0) + regressData.estimateur(x1)) * (x1 - x0);
    }

    return sum;
}

public double IntegrateDiscret(double xMin, double xMax)
{
    if (regressData == null || regressData.estimateur == null)
    {
        Debug.LogError("Erreur : estimateur non défini.");
        return 0;
    }
    double sum = 0.0;
    foreach (Vector2d point in listPoint)
    {
        if (point.x >= xMin || point.x <= xMax)
        {
            sum += point.y ;
        }
    }

    return sum;
}


}
