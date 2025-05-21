using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Graph : MonoBehaviour
{
    public RawImage graphImage;
    public TMP_Text posText;
    private ListPoint _points = new ListPoint();

    private Texture2D tex ;

    private float screenRatio = 0;

    public Vector2 _barycentre = new Vector2(0, 0);

    public Vector2 ratio = new Vector2(1, 1);

    private Vector2 lastRatio = new Vector2(1, 1);

    private List<GameObject > xValues = new List<GameObject>();
    private List<GameObject > yValues = new List<GameObject>();

    public Error errManager;
    public ProgressBarre progressBarre;

    private const int sizeX = 4000;
    private int sizeY = 4000;
    // Start is called before the first frame update
    void Awake()
    {
        screenRatio = (float) graphImage.rectTransform.rect.width / (float) graphImage.rectTransform.rect.height;
    //debug 
        Debug.Log("ratio : " + screenRatio);
        tex = new Texture2D(sizeX, sizeY);
    }
    void Start()
    {

    }

    public void addPoint( Vector2d point)
    {
        _points.addPoint(point);
    }

    public void setLPoints( ListPoint points)
    {
        _points = new ListPoint(points);
    }

    public ListPoint getLPoints()
    {
        return _points;
    }

    public void clear()
    {
        _points.clear();
        _points = new ListPoint();


        tex = new Texture2D(sizeX, sizeY);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
         tex.Apply();
        graphImage.texture = tex;


        foreach (GameObject go in xValues)
        {
            Destroy(go);
        }
        foreach (GameObject go in yValues)
        {
            Destroy(go);
        }
        xValues.Clear();
        yValues.Clear();
        
    }

    public Texture2D getRawImage()
    {
        return graphImage.texture as Texture2D;
    }

    public void setRawImage( Texture2D img)
    { 
        tex = img;
        tex.Apply();
        graphImage.texture = tex;
    }

    void FillTextureWithWhite(Texture2D tex)
    {
        Color[] whitePixels = new Color[tex.width * tex.height];
        
        // Remplir le tableau avec la couleur blanche
        for (int i = 0; i < whitePixels.Length; i++)
        {
            whitePixels[i] = Color.white;
        }

        // Appliquer les pixels à la texture
        tex.SetPixels(whitePixels);
    }

    public IEnumerator drawGraph()
    {
        if(progressBarre.ProcessingCheck())
        {
            errManager.addWarning("Un dessin est déjà en cours");
            yield break;
        }

        progressBarre.setAction("Dessin du graphique en cours");
        progressBarre.start((uint)_points.getListPoint().Count);
        yield return new WaitForSeconds(0.01f);

        tex = new Texture2D(sizeX, sizeY);

        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;


        FillTextureWithWhite(tex);


        drawAxis();

        uint i = 0;

        Vector2Int dotSize = scaleSize(10); //! doit etre un multiple de 2
        Color[] dot = generateTemplateSquare(dotSize, Color.black);

        foreach (Vector2d point in _points.getListPoint())
        {
            //debug
            drawDot(point, dot , dotSize);

            i++;

            if( progressBarre.validUpdate(i))
            {
                yield return new WaitForSeconds(0.01f);
            }
        }

        tex.Apply();
        graphImage.texture = tex;

        progressBarre.stop();
    }

    private Vector2Int scaleSize(int size)
    {
        return new Vector2Int((int)(size ), (int)(size * screenRatio));
    }

    private Color[] generateTemplateSquare(Vector2Int size, Color _col)
    {
        Color[] pixels = new Color[size.x * size.y];

        // Remplir le tableau avec la couleur rouge
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = _col;
        }

        return pixels;
    }


    void drawDot(Vector2d pos , Color[] dot , Vector2Int dotSize)
    {
        //draw en utilisant SetPixels , pour optimisé le temps de calcul
        pos.x /= ratio.x;
        pos.y /= ratio.y;

        pos.y *= screenRatio;

        pos.y = (float)(pos.y + _barycentre.y) ;


        int dot2x = dotSize.x/2;
        int dot2y = (int)(float)(dotSize.y)/2;

        //on ne dessisne pas si le point est en dehors de l'image
        if( pos.x - dot2x < 0 || pos.x + dot2x >= tex.width || pos.y - dot2y < 0 || pos.y+dot2y >= tex.height)
        {
            return;
        }

        try
        {
            //draw le point
            tex.SetPixels((int)pos.x - dot2x, (int)pos.y - dot2y, dotSize.x, dotSize.y, dot);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error drawing dot: " + e.Message);
            Debug.LogError("Position: " + pos.x + ", " + pos.y);
            Debug.LogError("Dot size: " + dotSize.x + ", " + dotSize.y);
            return;
        }
    }


    void addXvalue(float xValue)
    {
        if(_points.getListPoint().Count == 0)
        {
            return;
        }
        xValues.Add( new GameObject(xValue.ToString()));
        var tmptxt  = xValues[xValues.Count - 1].AddComponent<TextMeshProUGUI>();
        xValues[xValues.Count - 1].transform.SetParent(graphImage.transform);

        tmptxt.text = xValue.ToString();
        tmptxt.fontSize = 12;
        tmptxt.color = Color.black;
        tmptxt.alignment = TextAlignmentOptions.Center;


        var posImage =   graphImage.transform.position ;
        var taille = graphImage.rectTransform.rect.size / 2;

        Vector2 refPos = new Vector2(posImage.x - taille.x , posImage.y - taille.y)   ;

        double xOnGui = (xValue-_barycentre.x) / ratio.x;
        xOnGui *= (graphImage.rectTransform.rect.width)/sizeX;

        xValues[xValues.Count - 1].transform.position = new Vector3(refPos.x + (float)xOnGui, refPos.y - 15, 0);
    }

    void addYvalue(float yValue)
    {
        if(_points.getListPoint().Count == 0)
        {
            return;
        }

        yValues.Add( new GameObject(yValue.ToString()));
        var tmptxt  = yValues[yValues.Count - 1].AddComponent<TextMeshProUGUI>();
        yValues[yValues.Count - 1].transform.SetParent(graphImage.transform);

        tmptxt.text = yValue.ToString();
        tmptxt.fontSize = 12;
        tmptxt.color = Color.black;
        tmptxt.alignment = TextAlignmentOptions.Center;


        var posImage =   graphImage.transform.position ;
        var taille = graphImage.rectTransform.rect.size / 2;

        Vector2 refPos = new Vector2(posImage.x - taille.x , posImage.y - taille.y)   ;

        double yonGui = (yValue) / ratio.y;
        yonGui *= screenRatio;
        yonGui *= (graphImage.rectTransform.rect.height )/sizeY;
        yonGui += (_barycentre.y)*(graphImage.rectTransform.rect.height )/sizeY ;


        yValues[yValues.Count - 1].transform.position = new Vector3(refPos.x - 15, refPos.y + (float)yonGui, 0);
    }
    

    void drawAxis()
    {

        //nettoye tout les gameobject dans xvales
        foreach (GameObject go in xValues)
        {
            Destroy(go);
        }

        foreach (GameObject go in yValues)
        {
            Destroy(go);
        }

        xValues.Clear();
        yValues.Clear();
        //axe principal des x = 0
        addXvalue(0);
        drawXaxe(2, 0);
        //axe principal des y = 0
        drawYaxe(2, 0);
        addYvalue(0);

        //cherche les plus grand x et y dans la list de point

        Vector2d min= _points.minBound();
        Vector2d max = _points.maxBound();


        float stepX = 1;

        if( max.x-min.x < 1)
        {
            stepX = 0.1f;
        }
        else if( max.x-min.x < 10)
        {
            stepX = 1;
        }
        else if( max.x-min.x < 100)
        {
            stepX = 10;
        }
        else
        {
            stepX = 100;
        }

        float stepY = 1;

        if( max.y-min.y < 1)
        {
            stepY = 0.1f;
        }
        else if( max.y-min.y < 10)
        {
            stepY = 1;
        }
        else if( max.y-min.y < 100)
        {
            stepY = 10;
        }
        else
        {
            stepY = 100;
        }


        for (float i = (int)min.x; i <= max.x; i +=stepX)
        {
            if (i == 0)
                continue;

            addXvalue(i);
            drawXaxe(1, i);
        }

        for (float i = (int)min.y ; i <= max.y; i +=stepY)
        {
            if (i == 0)
                continue;
            addYvalue(i);
            drawYaxe(1, i);
        }

    }

    public void drawAxesValue()
    {
        //nettoye tout les gameobject dans xvales
        foreach (GameObject go in xValues)
        {
            Destroy(go);
        }

        foreach (GameObject go in yValues)
        {
            Destroy(go);
        }

        xValues.Clear();
        yValues.Clear();

        addXvalue(0);
        addYvalue(0);

        //cherche les plus grand x et y dans la list de point

        Vector2d min= _points.minBound();
        Vector2d max = _points.maxBound();


        float stepX = 1;

        if( max.x-min.x < 1)
        {
            stepX = 0.1f;
        }
        else if( max.x-min.x < 10)
        {
            stepX = 1;
        }
        else if( max.x-min.x < 100)
        {
            stepX = 10;
        }
        else
        {
            stepX = 100;
        }

        float stepY = 1;

        if( max.y-min.y < 1)
        {
            stepY = 0.1f;
        }
        else if( max.y-min.y < 10)
        {
            stepY = 1;
        }
        else if( max.y-min.y < 100)
        {
            stepY = 10;
        }
        else
        {
            stepY = 100;
        }

        for (float i = (int)min.x; i <= max.x; i +=stepX)
        {
            if (i == 0)
                continue;

            addXvalue(i);
        }

        for (float i = (int)min.y ; i <= max.y; i +=stepY)
        {
            if (i == 0)
                continue;
            addYvalue(i);
        }
    }

    void drawXaxe(int size , float x)
    {
        var dotSize = scaleSize(size);  
        Color[] dot = generateTemplateSquare(dotSize, Color.red);

        for (int i = 0; i < tex.height; i++)
        {
            drawDot(new Vector2d(x, i * ratio.y), dot , dotSize);
        }
    }

    void drawYaxe(int size , float y)
    {
        var dotSize = scaleSize(size);
        Color[] dot = generateTemplateSquare(dotSize, Color.red);
        for (int i = 0; i < tex.width; i++)
        {
            drawDot(new Vector2d(i * ratio.x ,y), dot , dotSize);
        }
    }


    public void autoScale()
    {
        screenRatio = (float) graphImage.rectTransform.rect.width / (float) graphImage.rectTransform.rect.height;

        calculateRatio();
        Vector2d min= _points.minBound();
        
        //_barycentre = new Vector2(0, (float)(-min.y)/ratio.y*screenRatio );
        _barycentre = new Vector2(0, 0 );
    }

    public void calculateRatio()
    {
        Vector2d min= _points.minBound();
        Vector2d max = _points.maxBound();

        ratio.x = (1.1f*(float)(max.x ) / tex.width );
        ratio.y = (1.1f*(float)(max.y  ) / tex.height) * screenRatio;
    }


    private Vector2d meanPoint()
    {
        return _points.calcBarycentre();;
    }

    public void drawEstimateCurve()
    {
        if( _points.regressData.estimateur == null)
        {
            errManager.addError("Pas d'estimateur de regression linéaire");
            return;
        }

        
        var dotSize = scaleSize(2);
        Color[] dot = generateTemplateSquare(dotSize, Color.blue);

        for (double i = -tex.width; i < tex.width; i+=0.1)
        {
            double x = i * ratio.x;
            double y = _points.regressData.estimateur(x);
            //debug
            drawDot(new Vector2d(x ,y), dot , dotSize);
        }

        tex.Apply();
        graphImage.texture = tex;
    }



    // Update is called once per frame
    void OnGui()
    {
        screenRatio = (float) graphImage.rectTransform.rect.width / (float) graphImage.rectTransform.rect.height;
    }

    void Update()
    {  
        if( lastRatio != ratio)
        {
            lastRatio = ratio;
        }

        Vector2 localMousePos;
        RectTransform rectTransform = graphImage.rectTransform;

        // Convertir la position de la souris en position locale dans le rectTransform
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            Input.mousePosition,
            null, // caméra null si screen space overlay
            out localMousePos))
        {
            // Convertir en coordonnées de texture (origine en bas à gauche)
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            float x = localMousePos.x + width / 2;
            float y = localMousePos.y + height / 2;

            if (x >= 0 && x <= width && y >= 0 && y <= height)
            {
                x *= ratio.x * sizeX / width;
                y *= ratio.y * sizeY / height;
                y /= screenRatio;

                
        /*pos.y /= ratio.y;

        pos.y *= screenRatio;

        pos.y = (float)(pos.y + _barycentre.y) ;*/
                posText.text = "x: " + x.ToString("F2") + " ; y: " + y.ToString("F2");

            }

        }

    }
    
    
}
