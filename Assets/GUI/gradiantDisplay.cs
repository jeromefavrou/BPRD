using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class gradiantDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public ColorPicker startColorPicker;
    public ColorPicker endColorPicker;

    public GeneralStatUtils gen_data;
    void Start()
    {
        startColorPicker.sliderH.onValueChanged.AddListener(UpdateGradient);
        startColorPicker.sliderS.onValueChanged.AddListener(UpdateGradient);
        startColorPicker.sliderV.onValueChanged.AddListener(UpdateGradient);

        endColorPicker.sliderH.onValueChanged.AddListener(UpdateGradient);
        endColorPicker.sliderS.onValueChanged.AddListener(UpdateGradient);
        endColorPicker.sliderV.onValueChanged.AddListener(UpdateGradient);
        UpdateGradient();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateGradient(float value = 0)
    {
        // Récupère les couleurs des ColorPickers
        Color startColor = Color.HSVToRGB(startColorPicker.sliderH.value, startColorPicker.sliderS.value, startColorPicker.sliderV.value);
        Color endColor = Color.HSVToRGB(endColorPicker.sliderH.value, endColorPicker.sliderS.value, endColorPicker.sliderV.value);


        gen_data.it_data.HSV_scale.H.a = (endColorPicker.sliderH.value - startColorPicker.sliderH.value);
        gen_data.it_data.HSV_scale.H.b = startColorPicker.sliderH.value;

        gen_data.it_data.HSV_scale.S.a = (endColorPicker.sliderS.value - startColorPicker.sliderS.value);
        gen_data.it_data.HSV_scale.S.b = startColorPicker.sliderS.value;

        gen_data.it_data.HSV_scale.V.a = (endColorPicker.sliderV.value - startColorPicker.sliderV.value);
        gen_data.it_data.HSV_scale.V.b = startColorPicker.sliderV.value;


        // Met à jour le gradient de l'image
        Image image = GetComponent<Image>();

        //cree une texture2d de taille 256x1
        Texture2D texture = new Texture2D(256, 256);
        for (int i = 0; i < 256; i++)
        {
            Color color = BathyGraphie2D.bathyColor(gen_data.it_data.HSV_scale, i, 0, 256);
            for (int j = 0; j < 256; j++)
            {
                texture.SetPixel(j, i, color);
            }

        }

        texture.Apply();
        // Assigne la texture au sprite de l'image

        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.material = new Material(Shader.Find("UI/Default"));
        image.material.mainTexture = texture;
        image.color = Color.white; // Assure que la couleur de l'image est blanche pour que le gradient soit visible

    }
    

}
