using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIPlacement : MonoBehaviour
{
    public Vector2Int offset = new Vector2Int(20, 2);
    public GameObject displayType;
    public GameObject cameraType;
    public GameObject sceneType;
    public GameObject scaleZ;

    public GameObject freeMem;

    public GameObject reductFull;
    public GameObject changeLog;


    public GameObject panel;



void Start()
{
    // Ajuster le panel
    RectTransform rt = panel.GetComponent<RectTransform>();
    rt.anchorMin = new Vector2(0, 0); // Coin inférieur gauche
    rt.anchorMax = new Vector2(1, 0.95f); // Coin supérieur droit
    rt.offsetMin = Vector2.zero;
    rt.offsetMax = Vector2.zero;

    rt = displayType.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(0, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancre

rt = cameraType.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(0, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancre

    rt = sceneType.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(0, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancre

    rt = scaleZ.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(0, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancre

    rt = freeMem.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Ancrage en haut droite
        rt.anchorMax = new Vector2(0, 1); // Ancrage en haut droite
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancre


    rt = reductFull.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1); // Ancrage en haut droite
        rt.anchorMax = new Vector2(1, 1); // Ancrage en haut droite
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancre

    rt = changeLog.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1); // Ancrage en haut droite
        rt.anchorMax = new Vector2(1, 1); // Ancrage en haut droite
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancre




    
}




    void OnGUI()
    {
        int index = 0;


        RectTransform rt = displayType.GetComponent<RectTransform>();


        index+= (int)rt.sizeDelta.x/2 + offset.x;

        rt.anchoredPosition = new Vector2(index, -offset.y);


        rt = cameraType.GetComponent<RectTransform>();



        index+= (int)rt.sizeDelta.x + offset.x;
        rt.anchoredPosition = new Vector2(index, -offset.y);

        rt = sceneType.GetComponent<RectTransform>();

        index+= (int)rt.sizeDelta.x + offset.x;
        rt.anchoredPosition = new Vector2(index, -offset.y);

        rt = scaleZ.GetComponent<RectTransform>();
        index+= (int)rt.sizeDelta.x + offset.x;
        rt.anchoredPosition = new Vector2(index, -offset.y -rt.sizeDelta.y/2 );

        rt = freeMem.GetComponent<RectTransform>();
        index+= (int)rt.sizeDelta.x + offset.x;
        rt.anchoredPosition = new Vector2(index, -offset.y);

        


        index = 0;


        rt = reductFull.GetComponent<RectTransform>();
        index += (int)rt.sizeDelta.x + offset.x;
        rt.anchoredPosition = new Vector2(-index, -offset.y);

        rt = changeLog.GetComponent<RectTransform>();
        index += (int)rt.sizeDelta.x + offset.x;
        rt.anchoredPosition = new Vector2(-index, -offset.y);


    }
}
