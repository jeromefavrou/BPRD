using UnityEngine;
using TMPro;

public class TitleAuto : MonoBehaviour
{
    private GameObject titlePrefab;  // Référence au prefab TextMeshPro
    private GameObject titleInstance;
    private TMP_Text Tmp_title;

    void Start()
    {        //cherhcer un prefab par son nom
        titlePrefab = Resources.Load<GameObject>("TextPrefab");

        if (titlePrefab == null)
        {
            Debug.LogError("Prefab Title non assigné !");
            return;
        }
        // Instancier le prefab au-dessus de l'objet actuel
        titleInstance = Instantiate(titlePrefab, transform);
        titleInstance.name = "Title";
        
        UpdateTitle();

    }
    
    public void UpdateTitle()
    {
        if(titleInstance == null)
        {
            Debug.LogError("L'instance du prefab Title n'est pas assignée !");
            return;
        }
        //taille de l'objet en y
        float sizeY = GetComponent<RectTransform>().sizeDelta.y;


        titleInstance.transform.localPosition = new Vector3(0, sizeY * 0.75f, 0);  // Ajuste la position

        // Récupérer le composant TMP_Text depuis le prefab
        Tmp_title = titleInstance.GetComponent<TMP_Text>();

        if (Tmp_title == null)
        {
            Debug.LogError("Le prefab ne contient pas de composant TextMeshPro !");
            return;
        }

        // Mettre à jour le texte avec le nom de l'objet parent
        Tmp_title.text = gameObject.name;
        
    }
}
