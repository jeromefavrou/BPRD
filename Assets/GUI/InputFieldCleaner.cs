using UnityEngine;
using TMPro;

public class InputFieldCleaner : MonoBehaviour
{
    // Référence à l'InputField TextMeshPro
    private TMP_InputField inputField;

    void Awake()
    {
        // Récupère le composant InputField de l'objet
        inputField = GetComponent<TMP_InputField>();

        // Vérifie si l'InputField est bien présent
        if (inputField == null)
        {
            Debug.LogError("InputFieldCleaner : l'InputField est manquant !");
        }
    }

    // Méthode appelée à chaque modification du texte dans l'InputField
    public void OnTextChanged()
    {
        // Vérifie si l'inputField n'est pas nul et a du texte
        if (inputField != null && !string.IsNullOrEmpty(inputField.text))
        {
            string text = inputField.text;

            // Supprime les guillemets du début et de la fin du texte
            if (text.StartsWith("\""))
            {
                text = text.Substring(1); // Enlève le premier caractère
            }
            if (text.EndsWith("\""))
            {
                text = text.Substring(0, text.Length - 1); // Enlève le dernier caractère
            }

            // Réaffecte le texte nettoyé à l'InputField
            inputField.text = text;
        }
    }

    // Optionnel : pour s'assurer que le texte est nettoyé quand le script commence
    void Start()
    {
        OnTextChanged();
    }

    void OnGUI()
    {
        OnTextChanged();
    }
}