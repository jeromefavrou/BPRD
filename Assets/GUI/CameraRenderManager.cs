using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
public class CameraRenderMAnager : MonoBehaviour
{
    public ColorPicker backgroudColorPicker;
    // Start is called before the first frame update
    private Camera mainCamera;



    void Awake()
    {
        mainCamera = this.GetComponent<Camera>();

    }
    void Start()
    {
        backgroudColorPicker.sliderH.onValueChanged.AddListener(UpdateBackgroundColor);
        backgroudColorPicker.sliderS.onValueChanged.AddListener(UpdateBackgroundColor);
        backgroudColorPicker.sliderV.onValueChanged.AddListener(UpdateBackgroundColor);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateBackgroundColor(float value = 0)
    {

        if (backgroudColorPicker.sliderH.value == 0 && backgroudColorPicker.sliderS.value == 0 && backgroudColorPicker.sliderV.value == 0)
        {
            //mode background type Skybox

            mainCamera.clearFlags = CameraClearFlags.Skybox;

            // Si les valeurs sont toutes à zéro, on ne change pas la couleur de fond
            return;
        }
        else
        {
            //mode background type Solid Color
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        // Récupère la couleur du ColorPicker
        Color backgroundColor = Color.HSVToRGB(backgroudColorPicker.sliderH.value, backgroudColorPicker.sliderS.value, backgroudColorPicker.sliderV.value);

        // Met à jour la couleur de fond de la caméra
        mainCamera.backgroundColor = backgroundColor;
    }

}
