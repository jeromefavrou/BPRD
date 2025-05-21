using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeneralStatUtils : MonoBehaviour
{
    

    

    public class PP_data
    {   public Vector3d min ;
        public Vector3d max ;
        public float min_resolution ;
        public float min_distance ;
        public float max_distance ;
        public float nemo_distance ;
        public float surface ;
        public float density ;
        
        public Vector3d size ;

        public class DataSegment
        {
            public uint start;
            public uint end;
            public bool set;

            public bool rendomiz;
            public List<uint> list;

            public DataSegment()
            {
                start = 0;
                end = 0;
                set = false;
                rendomiz = false;
                list = new List<uint>();
            }

            public bool isInside(uint idx)
            {
                if( rendomiz)
                {
                    return list.Contains(idx);
                }
                else
                {
                    return idx >= start && idx <= end;
                }
            }

            public bool isValide()
            {
                if( rendomiz)
                {
                    return list.Count > 0;
                }
                else
                {
                    return start <= end;
                }
            }
        }
        public DataSegment confirmDataSegment;
        public DataSegment convexeDataSegment;

        //constructeur
        public PP_data()
        {
            reset();
        }

        public void reset()
        {
            //debug
            Debug.Log("reset genDAta.pp_data");

            min_resolution = 1;
            min_distance = float.MaxValue;
            max_distance = float.MinValue;
            nemo_distance = float.MinValue;

            confirmDataSegment = new DataSegment();
            convexeDataSegment = new DataSegment();

            size = new Vector3d( 0, 0, 0 );

            min = new Vector3d( double.MaxValue, double.MaxValue, double.MaxValue );
            max = new Vector3d( double.MinValue, double.MinValue, double.MinValue );
        }
    }


    public class IT_data
    {
        public Vector2Int size;
        public double[,] data;
        public float reso;

        public float dMax ;
        public int nNeighbors ;
        public int interpolationType ;

        public IT_data()
        {
            reset();
        }

        public void reset()
        {
            size = new Vector2Int(0,0);
            data = new double[0,0];
            reso = 0;
        }
    }

    public IT_data it_data = new IT_data();
    
    public PP_data pp_data = new PP_data();

    public TMP_Text sizeText;
    public TMP_Text resolutionText;
    public TMP_Text minDistanceText;
    public TMP_Text MaxDistanceText;
    public TMP_Text SurfaceText;
    public TMP_Text densityText;

    public TMP_Text NemoDistanceText;

    public Texture2D workingTexture = null;
    public Texture2D map2d = null;
    public Texture2D map2dReduct = null;
    public Texture2D map2dDelta = null;
    public Texture2D map2dGrad = null;
    public Texture2D map2dLaplace = null;
    public string workingPath = "";
    public char separator = ';';

    public NPLimite limite = new NPLimite();
    public void updateStat()
    {

        //limité a 2 caractere apres la virgule chaque terme

        sizeText.text = "Size : (" + (float)pp_data.size.x + " x " + (float)pp_data.size.y + " x " + (float)pp_data.size.z+ ")";


        resolutionText.text = "Resolution : " +(float) pp_data.min_resolution;
        minDistanceText.text = "Min Distance : " + (float)pp_data.min_distance;
        MaxDistanceText.text = "Max Distance : " + (float)(pp_data.max_distance);
        NemoDistanceText.text = "Nemo Distance : " + (float)(pp_data.nemo_distance);
        SurfaceText.text = "Surface : " + (float)(pp_data.surface);
        densityText.text = "Density : " + (float)(pp_data.density);

    }

    //retourn le x relatif
    double rel_x(double x)
    {
        return x - pp_data.min.x ;
    }

    //retourn le y relatif
    double rel_y(double y)
    {
        return y - pp_data.min.y ;
    }

    //retourn le z relatif
    double rel_z(double z)
    {
        return z - pp_data.min.z ;
    }

    //le point relatif vecteur
    public Vector3d rel_vect3(Vector3d vect)
    {
        return new Vector3d(rel_x(vect.x), rel_y(vect.y), rel_z(vect.z));
    }

    //le point relatif vecteur
    public Vector2d rel_vect2(Vector2d vect)
    {
        return new Vector2d(rel_x(vect.x), rel_y(vect.y));
    }

    //retourne le x absolut
    double abs_x(double x)
    {
        return x + pp_data.min.x ;
    }

    //retourne le y absolut
    double abs_y(double y)
    {
        return y + pp_data.min.y ;
    }

    //retourne le z absolut
    double abs_z(double z)
    {
        return z + pp_data.min.z ;
    }

    //le point relatif vecteur
    public Vector3d abs_vect3(Vector3d vect)
    {
        return new Vector3d(abs_x(vect.x), abs_y(vect.y), abs_z(vect.z));
    }

    //le point relatif vecteur
    public Vector2d abs_vect2(Vector2d vect)
    {
        return new Vector2d(abs_x(vect.x), abs_y(vect.y));
    }

    public List<BathyPoint> subConfirmData( List<BathyPoint> data)
    {
        List<BathyPoint> tmpData = new List<BathyPoint>( data);

        //si donné de test de confirmation present
        if( this.pp_data.confirmDataSegment.set )
        {
            //retire les données de confirmation

            for( int i = 0 ; i < tmpData.Count ; i++)
            {
                if( this.pp_data.confirmDataSegment.isInside((uint)tmpData[i].idx))
                {
                    tmpData.RemoveAt(i);
                    i--;
                }
            }
        }

        return tmpData;
    }


    void Awake()
    {
        if( sizeText == null || resolutionText == null || minDistanceText == null || MaxDistanceText == null || NemoDistanceText == null) 
        {
            Debug.LogError("Un ou plusieurs composants ne sont pas assignés");
            return;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
