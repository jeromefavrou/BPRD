using UnityEngine;
using UnityEngine.UI;
using TMPro; // Si vous utilisez TextMeshPro

public class ChangeLogManager : MonoBehaviour
{
    private Button changeLogButton;   // Référence au bouton
    public GameObject changeLogPanel; // Le panel où le changelog sera affiché
    public TextMeshProUGUI changeLogText;  // Référence au TextMeshPro pour afficher le changelog

    private bool isChangeLogVisible = false;  // Flag pour savoir si le changelog est visible
    private string changeLogFilePath = "changeLog";  // Nom du fichier dans Resources (sans extension .txt)
    private  Vector2 screenSizeOrigine = new Vector2Int(1366, 768); // Offset pour le positionnement du panel

    void Start()
    {
        changeLogButton = GetComponent<Button>(); // Récupérer le composant Button attaché à ce GameObject
        if (changeLogButton == null)
        {
            Debug.LogError("Le bouton n'est pas attaché au GameObject.");
            return;
        }
        // Ajouter une fonction au bouton pour afficher/masquer le changelog
        changeLogButton.onClick.AddListener(ToggleChangeLog);

        // Charger le changelog depuis les ressources au début
        int nLine = LoadChangeLog() +10;

        //recupere la taille du font size du changelogText
        int sizeFontText = (int)changeLogText.fontSize ;

        //definit la taille height du rect transform du changelogText
        var rect = changeLogText.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, sizeFontText *nLine * 1.1f ); // Ajuster la hauteur en fonction du contenu
        

        
    }

    void ToggleChangeLog()
    {
        // Alterner l'état de visibilité du changelog
        isChangeLogVisible = !isChangeLogVisible;
        
        // Afficher ou masquer le panel en fonction de l'état
        changeLogPanel.SetActive(isChangeLogVisible);
    }

    int LoadChangeLog()
    {
        // Charger le contenu du fichier changelog depuis Resources
        TextAsset changeLogFile = Resources.Load<TextAsset>(changeLogFilePath);
        if (changeLogFile != null)
        {
            // Afficher le contenu dans le TextMeshPro
            changeLogText.text = changeLogFile.text;

            //retourne le nombre de lignes du changelog
            int lineCount = changeLogFile.text.Split('\n').Length;
            return lineCount;
        }
        else
        {
            // Si le fichier est introuvable
            changeLogText.text = "Change log non trouvé.";

            return 0;
        }
    }

    void Update()
    {
        // Vérifier si le changelog est visible et mettre à jour la taille du panel
        if (isChangeLogVisible)
        {
            //placer le changeLogPanel au centre de l'écran
            var panelRect = changeLogPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f); // Ancrage au centre
            panelRect.anchorMax = new Vector2(0.5f, 0.5f); // Ancrage au centre
            panelRect.pivot = new Vector2(0.5f, 0.5f); // Le centre du GameObject est l'ancre


            //positionne le changelogPanel au centre de l'écran
            panelRect.anchoredPosition = new Vector2(0, 0); // Positionner au centre de l'écran

            //ajuste dela taille du changelogPanel avec le scale
           panelRect.localScale = new Vector3( (float)Screen.width/screenSizeOrigine.x , (float)Screen.height / screenSizeOrigine.y , 1f); // Ajuster l'échelle en fonction de la taille de l'écran

        }
    }
}
