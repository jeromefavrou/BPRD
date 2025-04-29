using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class graphImageScale : MonoBehaviour
{

    public GameObject exportbp;
    public GameObject selectCurve;

    public GameObject text1;
    public GameObject text2;
    public GameObject text3;
    public GameObject text4;
    public GameObject text5;
    public GameObject text6;
    // Start is called before the first frame update
    void Start()
    {
       this.scale();
    }
    void Update()
    {
        this.scale();
    }

    // Update is called once per frame
    void scale()
    {
         // Get the current screen size
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Calculate the scale factor based on the screen size
        float scaleFactorX = screenWidth / 1366;
        float scaleFactorY = screenHeight / 768;

        RectTransform rectTransform = GetComponent<RectTransform>();

        //scale
        rectTransform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);

        //replace les objet 
        //place le bp export en bas a droit de l'image
        Vector2 size = exportbp.GetComponent<RectTransform>().sizeDelta;

        exportbp.GetComponent<RectTransform>().anchoredPosition = new Vector2(screenWidth/2 -size.x/2 -10, -screenHeight/2 + 50);

        //place le select curve en bas a gauche de l'image
        size = selectCurve.GetComponent<RectTransform>().sizeDelta;
        selectCurve.GetComponent<RectTransform>().anchoredPosition = new Vector2(-screenWidth/2 + 50 + size.x /2, -screenHeight/2 + 50);

        //place le text1 en haut a gauche de l'image
        size = text1.GetComponent<RectTransform>().sizeDelta;
        text1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-screenWidth/2 + 50 + size.x /2, screenHeight/2 - 50 - size.y /2);
        //place le text2 en haut a gauche de l'image
        Vector2 size2 = text2.GetComponent<RectTransform>().sizeDelta;
        text2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-screenWidth/2 + 50 + size.x /2 + size2.x , screenHeight/2 - 50 - size.y /2);

        //place le text3 en haut a gauche de l'image
        Vector2 size3 = text3.GetComponent<RectTransform>().sizeDelta;
        text3.GetComponent<RectTransform>().anchoredPosition = new Vector2(-screenWidth/2 + 50 + size.x /2 + size2.x + size3.x, screenHeight/2 - 50 - size.y /2);
        //place le text4 en haut a gauche de l'image
        Vector2 size4 = text4.GetComponent<RectTransform>().sizeDelta;
        text4.GetComponent<RectTransform>().anchoredPosition = new Vector2(-screenWidth/2 + 50 + size.x /2 + size2.x + size3.x + size4.x, screenHeight/2 - 50 - size.y /2);

        //place le text5 en haut a gauche de l'image
        Vector2 size5 = text5.GetComponent<RectTransform>().sizeDelta;
        text5.GetComponent<RectTransform>().anchoredPosition = new Vector2(-screenWidth/2 + 50 + size.x /2 + size2.x + size3.x + size4.x + size5.x, screenHeight/2 - 50 - size.y /2);
        //place le text6 en haut a gauche de l'image
        Vector2 size6 = text6.GetComponent<RectTransform>().sizeDelta;
        text6.GetComponent<RectTransform>().anchoredPosition = new Vector2(-screenWidth/2 + 50 + size.x /2 + size2.x + size3.x + size4.x + size5.x + size6.x, screenHeight/2 - 50 - size.y /2);

    }
}
