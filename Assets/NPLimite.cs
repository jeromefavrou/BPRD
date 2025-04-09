using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPLimite
{
    private uint xSize = 0;
    public ProgressBarre _pbarre;
    public Error errManager;

    
    struct NPLimiteData
    {
        public uint ymin ;
        public uint ymax ;

        public bool minSet;
        public bool maxSet;

    }

    private NPLimiteData[] limiteData = new NPLimiteData[0];
    public List<Vector2d> convexHull = new List<Vector2d>();
    uint minSetCount = 0;
    uint maxSetCount = 0;

    private float last_resolution =1;

    private bool inError = false;

    public NPLimite()
    {
        limiteData = new NPLimiteData[0];
        minSetCount = 0;
        maxSetCount = 0;
        convexHull = new List<Vector2d>();
        last_resolution =1;
        inError = false;
    }

    public void setReso( float _res)
    {
        last_resolution = _res;
    }

    public uint getLimiteDataSize()
    {
        return (uint)limiteData.Length;
    }

    public bool failed()
    {
        return inError;
    }

    public void setGlobal( GeneralStatUtils _gen)
    {
        xSize = (uint)_gen.it_data.size.x+1;


        limiteData = new NPLimiteData[xSize];

        for( uint i = 0 ; i < xSize ; i++)
        {
            limiteData[i].ymin = 0;
            limiteData[i].ymax = (uint)_gen.it_data.size.y;
            limiteData[i].minSet = true;
            limiteData[i].maxSet = true;
        }
    }

    public bool ProcessingCheck()
    {
        return _pbarre.ProcessingCheck();
    }


    public IEnumerator calculConvexHullPointFini(List<BathyPoint> _data , GeneralStatUtils _gen , uint idStart , uint idEnd)
    {
        if( _pbarre.ProcessingCheck())
        {
            errManager.addWarning("Action deja en cours");
            yield break;
        }

        _pbarre.setAction("Calcul de l'enveloppe convexe");
        _pbarre.start((uint) _data.Count , 0.05f );


        convexHull = new List<Vector2d>();

        uint i =0;
        foreach( BathyPoint point in _data)
        {
            if( point.idx >= idStart && point.idx <= idEnd)
            {
                Vector2d real_vect = _gen.rel_vect2(point.vect);
                convexHull.Add(real_vect);
            }

            if( _pbarre.validUpdate(i))
            {
                yield return new WaitForSeconds(0.01f);
            }

            i++;
        }

        _pbarre.stop();

    }

    public IEnumerator calculConvexHullGraham(List<BathyPoint> _data , GeneralStatUtils _gen )
    {
        if(  _pbarre.ProcessingCheck())
        {
            errManager.addWarning("Action deja en cours");
            yield break;
        }

        _pbarre.setAction("Calcul de l'enveloppe convexe");


        // Caluler l'algorithme de Graham Scan
        List<Vector2d> points = new List<Vector2d>();

        // Ajoutez vos points dans la liste points. Vous devriez adapter cela à votre structure de données.
        foreach (BathyPoint point in _data)
        {
            Vector2d real_vect = _gen.rel_vect2(point.vect);
            points.Add(real_vect); // Remplacez point.x et point.y par vos vraies coordonnées.
        }

        // Appliquer l'algorithme de Graham Scan
         _pbarre.start((uint)points.Count);


        // Étape 1 : Trouver le point de départ (le plus bas et à gauche)
        Vector2d pivot = points[0];
        foreach (var point in points)
        {
            if (point.y < pivot.y || (point.y == pivot.y && point.x < pivot.x))
            {
                pivot = point;
            }
        }


            // Étape 2 : Trier les points par angle polaire par rapport au pivot
        points.Sort((a, b) => CompareAngle(pivot, a, b));

        // Étape 3 : Appliquer l'algorithme de Graham Scan pour construire l'enveloppe convexe
        Stack<Vector2d> hull = new Stack<Vector2d>();

        // Ajouter le pivot à la pile
        hull.Push(pivot);


        // Parcourir tous les points triés à partir du deuxième point
        for (int i = 1; i < points.Count; i++)
        {
            Vector2d point = points[i];

            // Tant qu'on n'a pas une rotation à gauche, on retire le dernier point de la pile
            while (hull.Count >= 2)
            {
                Vector2d top = hull.Pop();       // Le dernier point
                Vector2d nextToTop = hull.Peek(); // Le second dernier point

                //calcul de la distance entre les deux points


                double cp = CrossProduct(nextToTop, top, point);

                // Vérifier la direction de la rotation avec le produit vectoriel
                if (cp >= 0)
                {
                    // Si c'est une rotation à gauche, on garde le point dans la pile
                    hull.Push(top);
                    break;
                }
            }
            
            hull.Push(point);

            if( _pbarre.validUpdate((uint)i))
            {
                yield return new WaitForSeconds(0.01f);
            }

            // Ajouter le point courant à l'enveloppe convexe
        }

        // Retourner la liste des points de l'enveloppe convexe dans le bon ordre
        convexHull = new List<Vector2d>(hull);
        convexHull.Reverse(); // Inverser la pile pour obtenir l'ordre correct des points

        _pbarre.stop();
    }

    // Fonction de l'algorithme de Graham Scan

// Fonction de comparaison des angles polaires
private int CompareAngle(Vector2d pivot, Vector2d a, Vector2d b)
{
    double angleA = Mathd.Atan2(a.y - pivot.y, a.x - pivot.x);
    double angleB = Mathd.Atan2(b.y - pivot.y, b.x - pivot.x);

    // Compare les angles
    if (angleA < angleB) return -1;
    if (angleA > angleB) return 1;

    if( angleA == angleB)
    {
        if( a.x < b.x)
        {
            return -1;
        }
        if( a.x > b.x)
        {
            return 1;
        }
    }
    return 0; // Si les angles sont égaux
}

// Calcul du produit vectoriel pour déterminer le sens de la rotation
private double CrossProduct(Vector2d a, Vector2d b, Vector2d c)
{
    return (b.x - a.x) * (c.y - a.y) -  (c.x - a.x)*(b.y - a.y) ;
}

public IEnumerator setLimite( GeneralStatUtils _gen )
{

    if( convexHull == null)
    {
        yield break;
    }

    if( convexHull.Count == 0)
    {
        yield break;
    }


    if( _pbarre.ProcessingCheck())
    {
        errManager.addWarning("Action deja en cours");
        yield break;
    }

    _pbarre.setAction("Calcul des limites inferieur");
 

    xSize = (uint)_gen.it_data.size.x+1; 
    last_resolution = _gen.it_data.reso;
    
    _pbarre.start((uint)_gen.it_data.size.y,0.01f);


    limiteData = new NPLimiteData[xSize];

    //initialisation

    minSetCount = 0;
    maxSetCount = 0;

    for( uint x = 0 ; x < xSize ; x++)
    {
        limiteData[x].ymin = (uint)_gen.it_data.size.y;
        limiteData[x].ymax = 0;
        limiteData[x].minSet = false;
        limiteData[x].maxSet = false;
    }

yield return new WaitForSeconds(0.01f);
    //pour tout Y montant
    //calcule des limite inferieur
    for(uint y = 0 ; y < _gen.it_data.size.y-1 ; y++)
    {
        if(minSetCount >= xSize)
        {
            //tout les ymin sont set
            break;
        }


        yMinPass(y,_gen);
        
        if( _pbarre.validUpdate(y))
        {
            yield return new WaitForSeconds(0.01f);
        }

    }

    if( minSetCount != xSize )
    {
        errManager.addError("Erreur de calcul des limites inf : " + minSetCount + " / " + xSize);
        Debug.Log("Erreur de calcul des limites inf : " + minSetCount + " / " + xSize);
        inError = true;
        for( uint x = 0 ; x < xSize ; x++)
        {
            if( !limiteData[x].minSet)
            {
                limiteData[x].ymin = 0;
            }
        }
    }
    _pbarre.setAction("Calcul des limites superieur");
 
    _pbarre.start((uint)_gen.it_data.size.y,0.01f);

    //pour tout Y descendant
    //calcule des limite superieur
    for(uint y = (uint)_gen.it_data.size.y ; y > 0 ; y--)
    {
        if(maxSetCount >= xSize)
        {
            //tout les ymax sont set
            break;
        }

        yMaxPass(y,_gen);

        if( _pbarre.validUpdate((uint)_gen.it_data.size.y-y))
        {
            yield return new WaitForSeconds(0.01f);
        }
    }

    if( maxSetCount != xSize )
    {
        errManager.addError("Erreur de calcul des limites sup : " + maxSetCount + " / " + xSize);
        Debug.Log("Erreur de calcul des limites sup : " + maxSetCount + " / " + xSize);
        inError = true;
        for( uint x = 0 ; x < xSize ; x++)
        {
            if( !limiteData[x].maxSet)
            {
                limiteData[x].ymax = (uint)_gen.it_data.size.y-1;
            }
        }
    }



    _pbarre.stop();
     _pbarre.setAction("Calcul des limites terminé");
      yield return null;
}


private void yMinPass( uint y , GeneralStatUtils _gen )
{
    for( uint x = 0 ; x < xSize ; x++)
    {
        if( limiteData[x].minSet)
        {
            continue;
        }

        foreach( Vector2d p in convexHull)
        {
            if( rayCastCell((uint)(x / last_resolution) , (uint)(y / last_resolution) , p) )
            {
                limiteData[x].ymin = y;
                limiteData[x].minSet = true;
                minSetCount++;

                if( minSetCount > 1)
                {
                    interpolateLimiteMin( x );
                }

                break;
            }
        }
    }
}

private void yMaxPass( uint y , GeneralStatUtils _gen )
{
    for( uint x = 0 ; x < xSize ; x++)
    {
        if( limiteData[x].maxSet)
        {
            continue;
        }

        foreach( Vector2d p in convexHull)
        {
            if( rayCastCell( (uint)(x / last_resolution) , (uint)(y / last_resolution) , p) )
            {
                limiteData[x].ymax = y;
                limiteData[x].maxSet = true;
                maxSetCount++;

                if(maxSetCount > 1)
                {
                    interpolateLimiteMax( x );
                }

                break;
            }
        }
    }
}

private void interpolateLimiteMax(uint x)
{
    //recherche un x precedent deja set

    if( x > 0)
    {
        for( uint xPrev = x-1 ; xPrev > 0 ; xPrev--)
        {
            if( limiteData[xPrev].maxSet)
            {
                //on a trouve un x precedent adjacent
                if( xPrev == x-1)
                {
                    break;
                }

                //calcule de la pente
                float pente = (float)((int)limiteData[x].ymax - (int)limiteData[xPrev].ymax) / (float)((int)x - (int)xPrev);

                for( uint i = xPrev+1 ; i < x ; i++)
                {
                    limiteData[i].ymax = (uint)(limiteData[xPrev].ymax + pente * (i - xPrev));
                    limiteData[i].maxSet = true;
                    
                    maxSetCount++;
                }

                break;
            }
        }
    }

    //cherche un x suivant deja set
    if( x < xSize-1)
    {
        for( uint xNext = x+1 ; xNext < xSize-1 ; xNext++)
        {
            if( limiteData[xNext].maxSet)
            {
                //on a trouve un x suivant adjacent
                if( xNext == x+1)
                {
                    break;
                }

                //calcule de la pente
                float pente = (float)((int)limiteData[xNext].ymax - (int)limiteData[x].ymax) / (float)((int)xNext - (int)x);

                for( uint i = x+1 ; i < xNext ; i++)
                {
                    limiteData[i].ymax = (uint)(limiteData[x].ymax + pente * (i - x));
                    limiteData[i].maxSet = true;
                    
                    maxSetCount++;
                }

                break;
            }
        }
    }
    
}

private void interpolateLimiteMin(uint x)
{
    //recherche un x precedent deja set

    if( x > 0)
    {
        for( uint xPrev = x-1 ; xPrev > 0 ; xPrev--)
        {
            if( limiteData[xPrev].minSet)
            {
                //on a trouve un x precedent adjacent
                if( xPrev == x-1)
                {
                    break;
                }

                //calcule de la pente
                float pente = (float)((int)limiteData[x].ymin - (int)limiteData[xPrev].ymin) / (float)((int)x - (int)xPrev);

                for( uint i = xPrev +1 ; i < x ; i++)
                {
                    limiteData[i].ymin = (uint)(limiteData[xPrev].ymin + pente * ( i - xPrev));
                    limiteData[i].minSet = true;
                    
                    minSetCount++;
                }

                break;
            }
        }
    }

    //cherche
    if( x < xSize-1)
    {
        for( uint xNext = x+1 ; xNext < xSize-1 ; xNext++)
        {
            if( limiteData[xNext].minSet)
            {
                //on a trouve un x suivant adjacent
                if( xNext == x+1)
                {
                    break;
                }

                //calcule de la pente
                float pente = (float)((int)limiteData[xNext].ymin - (int)limiteData[x].ymin) / (float)((int)xNext - (int)x);

                for( uint i = x+1 ; i < xNext ; i++)
                {
                    limiteData[i].ymin = (uint)(limiteData[x].ymin + pente * (i - x));
                    limiteData[i].minSet = true;
                    
                    minSetCount++;
                }
                break;
            }
        }
    }

}

    private bool rayCastCell(uint x , uint y , Vector2d p)
    {
        return p.x >= x && p.y >= y && p.x < x+1 && p.y < y+1;
    }



    public uint getLimiteYMin( uint x)
    {
        //indexe x en fonction de la resolution arrondire tjr  a l'inferieur
        uint xIndex = (uint)(x / last_resolution);

        return (uint)(limiteData[ xIndex ].ymin*last_resolution);
    }

    public uint getLimiteYMax( uint x)
    {
        //indexe x en fonction de la resolution arrondire tjr  a l'inferieur
        uint xIndex = (uint)(x / last_resolution);


        return (uint)(limiteData[ xIndex ].ymax*last_resolution);
    }

    public float getSurface()
    {
        float surface = 0;

        for( uint x = 0 ; x < xSize ; x++)
        {
            surface += (limiteData[x].ymax - limiteData[x].ymin);
        }

        return surface;
    }
}
