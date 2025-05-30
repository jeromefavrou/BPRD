using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using TMPro;
public class BathyGraphie2D : MonoBehaviour
{
    // Start is called before the first frame update
    private Interpolate interpolate = new Interpolate();
    private Image _image;

    private Texture2D map2dNN;
    private Texture2D map2dIDW;
    private Texture2D map2d = null;


    private Texture2D map2dNN_derivate;
    private Texture2D map2dIDW_derivate;



    public double imgResolustion = 1;

    public TMP_Dropdown interpolationMethod;

    public struct ColorScale
    {
        public float a;
        public float b;

        public ColorScale(float _a = 1, float _b = 0)
        {
            a = _a;
            b = _b;
        }
    }

    public struct HSVScale
    {
        public ColorScale H;
        public ColorScale S;
        public ColorScale V;

        public HSVScale(ColorScale _H = new ColorScale(), ColorScale _S = new ColorScale(), ColorScale _V = new ColorScale())
        {
            H = _H;
            S = _S;
            V = _V;
        }
    }



    void Awake()
    {
        _image = GetComponent<Image>();

        map2dNN = new Texture2D(0, 0);
        map2dIDW = new Texture2D(0, 0);

        map2dNN_derivate = new Texture2D(0, 0);
        map2dIDW_derivate = new Texture2D(0, 0);

        interpolationMethod.onValueChanged.AddListener(delegate {
            selectTexture();
        });


        selectTexture();
    }

    void Start()
    {
        
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
    public double interpolateData(List<BathyPoint> lpoints  , Vector3d point)
    {
        if(interpolationMethod.value == 1)
            return interpolate.IDW(lpoints, new Vector2d(point.x, point.y) , 2  , 10);
        else
            return interpolate.nearestNeighbor(lpoints, new Vector2d(point.x, point.y) , 10);
    }


    public void applyTextToImage()
    {
        // Applique les changements
        map2d.Apply();

        if(interpolationMethod.value == 1)
            map2dIDW = map2d;
        else
            map2dNN = map2d;

        // Convertir la Texture2D en Sprite
        Sprite sprite = Sprite.Create(map2d, new Rect(0, 0, map2d.width , map2d.height), new Vector2(0.5f, 0.5f));

        _image.sprite = sprite;

        //garde le ration de l'image
        _image.rectTransform.sizeDelta = new Vector2(_image.rectTransform.sizeDelta.x, (((float)map2d.height )/(float)map2d.width ) * _image.rectTransform.sizeDelta.x);

    }



    public void printTrace( List<Vector3d> _data , Vector3d _min )
    {
        for( int i = 0 ; i < _data.Count ; i++)
        {
            map2d.SetPixel((int)((_data[i].x - _min.x)*imgResolustion), (int)((_data[i].y - _min.y)*imgResolustion), Color.black); 
        }

        applyTextToImage();
    }

    public void hideTrace(List<Vector3d> _data , Vector3d _min , Vector3d _max )
    {
        /*for( int i = 0 ; i < _data.Count ; i++)
        {
            map2d.SetPixel((int)((_data[i].x - _min.x)*imgResolustion), (int)((_data[i].y - _min.y)*imgResolustion), bathyColor(_data[i].z , _min.z , _max.z)); 
        }

        applyTextToImage();*/
    }


    public static Color bathyColor( HSVScale hsvScale , double _value , double _min , double _max)
    {
        
        float normalizedValue = Mathf.InverseLerp((float)_max, (float)_min, (float)_value);

        float H, S, V;

        H = hsvScale.H.a * normalizedValue + hsvScale.H.b;

        S = hsvScale.S.a * normalizedValue + hsvScale.S.b;

        V =   hsvScale.V.a * normalizedValue + hsvScale.V.b;
        


        //assure des valeurs correctes
        if (H < 0)
            H = 0;
        else if (H > 1)
            H = 1;
        if( S < 0 )
            S = 0;
        else if( S > 1 )
            S = 1;
        if( V < 0 )
            V = 0;
        else if( V > 1 )
            V = 1;


        return Color.HSVToRGB(H, S, V);
    }

    private void selectTexture()
    {
        if(interpolationMethod.value == 1)
            map2d = map2dIDW;
        else
            map2d = map2dNN;

        applyTextToImage();
    }

    public void SaveTextureToJPG(string path)
    {
        // Encoder la texture en JPG
        byte[] jpgData = ImageConversion.EncodeToJPG(map2d , 100);

        // Vérifier si l'encodage a réussi
        if (jpgData != null)
        {
            // Sauvegarder les données dans un fichier
            File.WriteAllBytes(path, jpgData);
            Debug.Log("Image sauvegardée avec succès : " + path);
        }
        else
        {
            Debug.LogError("Erreur lors de l'encodage de la texture en JPG.");
        }
    }

    public Texture2D getTexture()
    {
        return  map2d;
    }

    public static float getRawValue(Texture2D _text , int x , int y , float _min , float _max )
    {
        
        float H, S, V;
        Color.RGBToHSV(_text.GetPixel(x, y), out H, out S, out V);

        return  (_max - _min)*(((H +0.25f)/0.75f) - 0.25f)+(_max);
    }


    public static void csvSave(List<BathyPoint> data , string path , char separator = ',')
    {
        if(data == null)
            return;
        if(data.Count == 0)
            return;

        string csv = "n" + separator + "x" + separator + "y" + separator + "z" + separator + "grad(z(x,y))" + separator + "lap(z(x,y))" + "\n";
        for(int i = 0 ; i < data.Count ; i++)
        {
            csv += data[i].idx.ToString() + separator + data[i].vect.x.ToString() + separator + data[i].vect.y.ToString() + separator + data[i].vect.z.ToString() + separator+ data[i].gradiant.ToString() + separator + data[i].laplace.ToString()  +"\n";
        }

        File.WriteAllText(path, csv);
    }

    public static void fillTexture(ref Texture2D _text , Color _fill)
    {
        for(int i = 0 ; i < _text.width ; i++)
        {
            for(int j = 0 ; j < _text.height ; j++)
            {
                _text.SetPixel(i, j, _fill);
            }
        }
    }

    public static List<Vector2d>  CrossValidation(double[,] originalGrid , double[,] reducedGrid)
    {
        List<Vector2d> result = new List<Vector2d>();

        for (int i = 0; i < originalGrid.GetLength(0); i++)
        {
            for (int j = 0; j < originalGrid.GetLength(1); j++)
            {
                result.Add(new Vector2d(Mathd.Abs(reducedGrid[i, j]), Mathd.Abs(originalGrid[i, j])));
            }
        }

        return result;
    }




}
