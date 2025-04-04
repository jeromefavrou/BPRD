using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolate 
{
    
    private double distanceSave = 0;
    private double dist = 0;
    private double result = 0;
    private double dx = 0;
    private double dy = 0;

    private GeneralStatUtils gen_data;
    private List<BathyPoint> data ;
    private int type =3 ;
    private float dmax = 1;
    private ProgressBarre progressBarre;

    private Error _error;



    public void set_utility(ref Error error , ref GeneralStatUtils _gen_data , ref List<BathyPoint> _data , int _type , float _dmax , ref ProgressBarre _progressBarre)
    {
        gen_data = _gen_data;
        data = _data;
        type = _type;
        dmax = _dmax;
        progressBarre = _progressBarre;
        _error = error;
    }

    public double nearestNeighbor(List<BathyPoint> _data , Vector2d _point , double dmax)
    {
        distanceSave = double.MaxValue;
        result = 1;

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

                dist = Mathd.Sqrt(dx * dx + dy * dy);

                if (dist > dmax)
                {
                    continue;
                }

                if (dist == 0)
                {
                    return p.vect.z;
                }

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





    public IEnumerator interpolation(  )
    {

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


        if( type < 0 || type > 4)
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

        progressBarre.setAction("Interpolation en cours");
        progressBarre.start((uint)gen_data.it_data.size.x);

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

        gen_data.limite.setReso((uint)gen_data.it_data.reso);

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
        }


        // Interpolation avec pause pour éviter le blocage
        for (int x = 0; x < gen_data.it_data.size.x ; x++)
        {
            for (int y = (int)gen_data.limite.getLimiteYMin((uint)(x)) ; y < (int)gen_data.limite.getLimiteYMax((uint)(x)) ; y++)
            { 
                var vect_target_abs = gen_data.abs_vect2( new Vector2d((double)(x+0.5) / gen_data.it_data.reso, (double)(y+0.5) / gen_data.it_data.reso));

                if (type == 0)
                    gen_data.it_data.data[x, y] = nearestNeighbor(tmpData, vect_target_abs, dmax);
                else if (type == 1)
                    gen_data.it_data.data[x, y] = IDW(tmpData, vect_target_abs, 1, dmax);
                else if (type == 2)
                {
                    try
                    {
                         gen_data.it_data.data[x, y] = IDW(tmpData, vect_target_abs, 2, dmax);
                    }
                    catch( System.Exception e)
                    {
                        _error.addError("Erreur IDW : " + e.Message);
                        //log x et y 
                        Debug.Log("x : " + x + " y : " + y);
                    }
                   
                }
                    
                else if (type == 3)
                    gen_data.it_data.data[x, y] = IDW(tmpData, vect_target_abs, 3, dmax);
                else if (type == 4)
                    gen_data.it_data.data[x, y] = 0;
            }

            // Rendre la main à Unity
            if (progressBarre.validUpdate((uint)x)) 
            {
                yield return new WaitForSeconds(0.01f);
            }

        }


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


}
