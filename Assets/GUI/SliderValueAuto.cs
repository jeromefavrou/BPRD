using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SliderValueAuto : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text Tmp_value;
    private Slider slider;
    void Start()
    {
        slider = GetComponent<Slider>();

        // Mettre à jour la valeur initiale
        UpdateValue(slider.value);

        // Ajouter un listener pour mettre à jour la valeur lorsque le slider change
        slider.onValueChanged.AddListener(UpdateValue);
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void UpdateValue(float value = 0)
    {
        // Mettre à jour le texte avec la valeur du slider
        Tmp_value.text = slider.value.ToString("F2"); // Affiche la valeur avec 2 décimales
    }
}
