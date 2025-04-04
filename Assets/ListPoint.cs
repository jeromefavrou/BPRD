using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListPoint 
{
    private List<Vector2d>  listPoint = new List<Vector2d>();

    public class RegressParametres
    {
        public double a;
        public double b;

        public RegressParametres(double a =0, double b = 0)
        {
            this.a = a;
            this.b = b;
        }
    }

    public class RegressData
    {
        public double StdRMSE;
        public double XYRMSE;
        public double rSquare;
        public double rPearson;

        public RegressParametres regressParametres = null;

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

            if(data.regressParametres != null)
            {
                this.regressParametres = new RegressParametres(data.regressParametres.a, data.regressParametres.b);
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
            sum += Mathd.Pow(point.y - (regressData.regressParametres.a * point.x + regressData.regressParametres.b), 2);
        }   


        regressData.StdRMSE =Mathd.Sqrt(sum / listPoint.Count);

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
            sum += Mathd.Pow(point.y - point.x, 2);
        }   

        regressData.XYRMSE = Mathd.Sqrt(sum / listPoint.Count);

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
            sumYiYhat += Mathd.Pow(point.y - (regressData.regressParametres.a * point.x + regressData.regressParametres.b), 2);
            sumYiYbar += Mathd.Pow(point.y - barycentre.y, 2);
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
            stdX += Mathd.Pow(point.x - barycentre.x, 2);
            stdY += Mathd.Pow(point.y - barycentre.y, 2);
        }

        stdX = Mathd.Sqrt(stdX / (listPoint.Count - 1));
        stdY = Mathd.Sqrt(stdY / (listPoint.Count - 1));

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


    public RegressParametres linearRegression()
    {
        if(regressData == null)
        {
            regressData = new RegressData();
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

        return regressData.regressParametres;
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


}
