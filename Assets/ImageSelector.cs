using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ImageSelector : MonoBehaviour
{
    public GeneralStatUtils gen_data;
    public TMP_Dropdown imageType;
    public GameObject buttonSave;
    public Map map;
    private Image _image;

    public Toggle traceToggle;

    public List<BathyPoint> _data ;

    void Awake()
    {
        if( gen_data == null || imageType == null)
        {
            Debug.LogError(" non assigné !");
            return;
        }

        _image = GetComponent<Image>();
        imageType.onValueChanged.AddListener(delegate {
            updateShow();
        });


        traceToggle.onValueChanged.AddListener(delegate {
            updateShow();
        });
    }

    void Start()
    {
         RectTransform rt = GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0, 0); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(0, 0); // Ancrage en haut gauche
        rt.pivot = new Vector2(0, 0);  // Le haut du GameObject est l'ancres

        //screen size
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        rt.anchoredPosition = new Vector2( 0 , 0);

        rt.sizeDelta = new Vector2( screenSize.x * 0.5f , screenSize.y * 0.5f);

        rt = traceToggle.GetComponent<RectTransform>();

        //placer au dessus
        rt.anchorMin = new Vector2(0, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(0, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(1, 1);  // Le haut du GameObject est l'ancres

        rt.anchoredPosition = new Vector2( rt.sizeDelta.x , rt.sizeDelta.y/2+20);

        float size = rt.sizeDelta.x;
    
        rt = buttonSave.GetComponent<RectTransform>();

        //placer au dessus
        rt.anchorMin = new Vector2(0, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(0, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(1, 1);  // Le haut du GameObject est l'ancres

        rt.anchoredPosition = new Vector2(  size + rt.sizeDelta.x , rt.sizeDelta.y/2 + 20);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
                //position par rapport au haut droit
        RectTransform rt = GetComponent<RectTransform>();

        //screen size
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        //ajuste la taille en fonction de la resolution de la fenetre

        rt.sizeDelta = new Vector2( screenSize.x * 0.5f , screenSize.y * 0.5f);
        
        
    }

    public void updateShow()
    {
        Texture2D selected = null;

        if(imageType.value == 0)
        {
            selected = gen_data.map2d;
        }
        else if(imageType.value == 1)
        {
            selected = gen_data.map2dGrad;
        }
        else if(imageType.value == 2)
        {
            selected = gen_data.map2dLaplace;
        }
        else if(  imageType.value == 3  )
        {
            selected = gen_data.map2dReduct;
        }
        else if( imageType.value == 4)
        {
            selected = gen_data.map2dDelta;
        }


        if(traceToggle.isOn)
        {

            bool isContour = !(gen_data.pp_data.convexeDataSegment.start == 0 && gen_data.pp_data.convexeDataSegment.end == 0);
            bool isConfirm = !(gen_data.pp_data.confirmDataSegment.start == 0 && gen_data.pp_data.confirmDataSegment.end == 0);


           for( int i = 0 ; i < _data.Count ; i++)
            {

                var relVect = gen_data.rel_vect2(_data[i].vect);


                if(  gen_data.pp_data.convexeDataSegment.set &&gen_data.pp_data.convexeDataSegment.isInside((uint)_data[i].idx))
                {
                    selected.SetPixel((int)(relVect.x*gen_data.it_data.reso), (int)(relVect.y*gen_data.it_data.reso), Color.grey); 
                }
                else if( gen_data.pp_data.confirmDataSegment.set && gen_data.pp_data.confirmDataSegment.isInside((uint)_data[i].idx))
                {
                    selected.SetPixel((int)(relVect.x*gen_data.it_data.reso), (int)(relVect.y*gen_data.it_data.reso), Color.white); 
                }
                else
                {
                    selected.SetPixel((int)(relVect.x*gen_data.it_data.reso), (int)(relVect.y*gen_data.it_data.reso), Color.black); 
                }
                
            }
        }
        else
        {
            /*for (int x = 0; x < gen_data.xSizeInterpolateData ; x++)
            {
                for (int y = (int)gen_data.limite.getLimiteYMin((uint)(x)); y < gen_data.ySizeInterpolateData ; y++)
                {  
                    selected.SetPixel(x, y, BathyGraphie2D.bathyColor(gen_data.interpolateData[x, y], gen_data.min.z, gen_data.max.z));
                }
            }*/
        }

        selected.Apply();

        // Convertir la Texture2D en Sprite
        Sprite sprite = Sprite.Create(selected, new Rect(0, 0, selected.width , selected.height), new Vector2(0.5f, 0.5f));

        _image.sprite = sprite;
        _image.preserveAspect = true; // Garde l’aspect ratio

        map.meshRenderer.sharedMaterial.mainTexture = selected;

        
    }

}
