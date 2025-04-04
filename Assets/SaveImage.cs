using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class SaveImage : MonoBehaviour
{
    // Start is called before the first frame update
    public Error _error;
    public GeneralStatUtils gen_data;
    public TMP_Dropdown imageType;
    

    private Button _button;
    void Awake()
    {
        _button = GetComponent<Button>();
    }
    void Start()
    {
        _button.onClick.AddListener(delegate {
            SaveTextureToJPG();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SaveTextureToJPG()
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

        if(selected == null)
        {
            _error.addWarning("Aucune image n'a été sélectionnée !");
            return;
        }

        byte[] jpgData = null;
        try
        {
            jpgData = ImageConversion.EncodeToJPG(selected , 100);
        }
        catch (System.Exception e)
        {
            _error.addError("Erreur lors de l'encodage de la texture en JPG : " + e.Message);
            return;
        }

        

        // Vérifier si l'encodage a réussi
        if (jpgData != null)
        {
            string path = gen_data.workingPath + "/map2dtmp.jpg";
            // Sauvegarder les données dans un fichier
            if(imageType.value == 0)
            {
                path = gen_data.workingPath + "/map2d.jpg";
            }
            else if( imageType.value == 1)
            {
                path = gen_data.workingPath + "/map2dGrad.jpg";
            }
            else if( imageType.value == 2)
            {
                path = gen_data.workingPath + "/map2dLaplace.jpg";
            }
            else if( imageType.value == 3)
            {
                path = gen_data.workingPath + "/map2dReduct.jpg";
            }
            else if( imageType.value == 4)
            {
                path = gen_data.workingPath + "/map2dDelta.jpg";
            }

            try
            {
                File.WriteAllBytes(path , jpgData); 
                _error.addLog("Image sauvegardée avec succès : " + path);
            }
            catch (System.Exception e)
            {
                _error.addError("Erreur lors de la sauvegarde de l'image : " + e.Message);
            }
            
        }
        else
        {
           _error.addError("Erreur lors de l'encodage de la texture en JPG.");
        }
    }
}
