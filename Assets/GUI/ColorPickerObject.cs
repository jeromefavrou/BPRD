using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ColorPicker : MonoBehaviour
{
    public Slider sliderH;
    public Slider sliderS;
    public Slider sliderV;
    private Image colorDisplay;

    void Awake()
    {
        // Assurez-vous que les sliders et l'image sont assignés
        if (sliderH == null || sliderS == null || sliderV == null)
        {
            Debug.LogError("Un ou plusieurs sliders ne sont pas assignés !");
            return;
        }

        colorDisplay = GetComponent<Image>();
        if (colorDisplay == null)
        {
            Debug.LogError("L'image de l'affichage de la couleur n'est pas assignée !");
        }
    }

    void Start()
    {
        UpdateColor();
        // Ajoute les listeners
        sliderH.onValueChanged.AddListener(UpdateColor);
        sliderS.onValueChanged.AddListener(UpdateColor);
        sliderV.onValueChanged.AddListener(UpdateColor);
    }

    void UpdateColor(float value=0)
    {
        // Récupère les valeurs des sliders
        float h = sliderH.value;
        float s = sliderS.value;
        float v = sliderV.value;

        // Convertit HSV en RGB
        Color color = Color.HSVToRGB(h, s, v);

        // Met à jour l'affichage de la couleur
        colorDisplay.color = color;
    }


}
