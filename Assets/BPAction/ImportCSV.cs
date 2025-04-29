using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
public class ImportCSV : BPAction
{
    // Start is called before the first frame update
    public TMP_Text pointImported;
    public TMP_InputField separator;
    public TMP_InputField startLine;
    public TMP_InputField XColumn;
    public TMP_InputField YColumn;
    public TMP_InputField ZColumn;
    public TMP_InputField filePath;

    private List<BathyPoint> csvData = new List<BathyPoint>();

    void Start()
    {
        //verifie que tout les composant sont bien remplis
        if( pointImported == null || separator == null || startLine == null || XColumn == null || YColumn == null || ZColumn == null || filePath == null || button == null || fwdObj == null)
        {
            errManager.addError("Un ou plusieurs composants ne sont pas assignés");
            return;
        }
    }

    void OnGUI()
    {
        if (isProcessing)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }


        //position par rapport au haut droit
        RectTransform rt = GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(1, 1); // Ancrage en haut gauche
        rt.anchorMax = new Vector2(1, 1); // Ancrage en haut gauche
        rt.pivot = new Vector2(0.5f, 1);  // Le haut du GameObject est l'ancres

        //screen size
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        rt.anchoredPosition = new Vector2( -rt.sizeDelta.x/2 - screenSize.x * 0.15f , -50);

    }


    protected override IEnumerator action()
    {
        if (progressBarre.ProcessingCheck())
        {
            yield break;
        }

        progressBarre.setAction("Importation du fichier CSV");


        csvData.Clear();

        string path = filePath.text;
        char sep = separator.text[0];
        gen_data.separator = sep;
        int start = int.Parse(startLine.text);

        int x = 0;
        int y = 0;
        int z = 0;
        try
        {
            x = int.Parse(XColumn.text);
            y = int.Parse(YColumn.text);
            z = int.Parse(ZColumn.text);
        }
        catch (System.Exception e)
        {
            errManager.addError("echec de conversion des str en int : " + e.Message);
            isProcessing = false;
            yield break;
        }



        if(!File.Exists(path))
        {
            errManager.addError("Le fichier n'existe pas");
            isProcessing = false;
            yield break;
        }

        if (start < 0)
        {
            errManager.addError("La ligne de départ doit être supérieur à 0");

            isProcessing = false;
            yield break;
        }

        if (x < 0 || y < 0 || z < 0)
        {
            errManager.addError("Les colonnes doivent être supérieur à 0");

            isProcessing = false;
            yield break;
        }

        //chemin de travail chamin sans le fichier

        gen_data.workingPath = Path.GetDirectoryName(path);
        //debug
        errManager.addLog("Chemin de travail : " + gen_data.workingPath);

        // Lire toutes les lignes du fichier CSV
        string[] lines= null;
        try
        {
            lines= File.ReadAllLines(path);
            
            if(lines.Length == 0)
            {
                errManager.addError("Le fichier est vide");
                isProcessing = false;
                yield break;
            }
        }
        catch (System.Exception e)
        {
            errManager.addError("Erreur lors de la lecture du fichier : " + e.Message);
            isProcessing = false;
            yield break;
        }
        
         progressBarre.start((uint)lines.Length);

        int i =0;

        // Parcourir les lignes du fichier CSV
        foreach (string line in lines)
        {
            //ignoer les lignes avant la ligne de départ
            if (start > 0)
            {
                start--;
                continue;
            }

            // Diviser chaque ligne par le separateur
            string[] values = line.Split(sep);

            Vector3d vec = new Vector3d();

            try
            {
                vec.x = double.Parse(values[x].Replace('.', ','));
                vec.y = double.Parse(values[y].Replace('.', ','));
                vec.z = double.Parse(values[z].Replace('.', ','));
            }
            catch (System.Exception e)
            {
                errManager.addError("echec de conversion des str en int (verifier donné ou index collone): " + e.Message);
                isProcessing = false;
                csvData.Clear();
                yield break;
            }



            // Ajouter les valeurs dans la liste
            csvData.Add(new BathyPoint(vec, (ulong)csvData.Count));

            i++;

            if (progressBarre.validUpdate((uint)i))
            {
                yield return new WaitForSeconds(0.01f);
            }
            pointImported.text = "Points importés : " + csvData.Count;

            if( csvData.Count > 0)
            {
                fwdObj.SetActive(true);
            }

            
        }

        progressBarre.stop();
        progressBarre.setAction("Importation terminée");

        isProcessing = false;
    }

    public List<BathyPoint> getCSVData()
    {
        return csvData;
    }

}
