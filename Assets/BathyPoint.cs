using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathyPoint 
{
    public Vector3d vect;
    public ulong idx ;
    public double gradiant;
    public double laplace;

    public BathyPoint(Vector3d vect, ulong idx)
    {
        this.vect = vect;
        this.idx = idx;
    }
    public BathyPoint()
    {
        this.vect = new Vector3d();
        this.idx = 0;
    }

    public double squareDistance(BathyPoint other)
    {
        return (this.vect.x - other.vect.x) * (this.vect.x - other.vect.x) + (this.vect.y - other.vect.y) * (this.vect.y - other.vect.y) ;
    }

}

