using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(3, 10)]  // Ceci permet de créer un champ multi-lignes dans l'Inspector
    public string tooltipText = "Voici un commentaire\nEt voici la deuxième ligne\nEt une troisième ligne";  // Texte à entrer dans l'Inspector
    private float hoverTime = 1f;  // Temps avant d'afficher le tooltip (en secondes)
    private bool showTooltip = false;  // Si le tooltip doit être affiché ou non
    private float hoverStartTime;  // Temps de début de survol de la souris

    private Coroutine tooltipCoroutine;  // Référence à la Coroutine en cours

    void Start()
    {
        // Assurez-vous que cet objet UI a un EventTrigger attaché, ou utilisez un Button/Image avec l'EventTrigger activé


    }

    // Cette fonction est appelée lorsque la souris entre dans l'élément UI
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);  // Annuler la coroutine précédente si elle existe
        }
        tooltipCoroutine = StartCoroutine(ShowTooltipAfterDelay());  // Commencer une nouvelle coroutine
    }

    // Cette fonction est appelée lorsque la souris quitte l'élément UI
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);  // Annuler la coroutine si la souris sort de l'élément
        }
        showTooltip = false;  // Ne pas afficher le tooltip si la souris quitte l'élément
    }

    // Coroutine qui attend le délai avant d'afficher le tooltip
    private IEnumerator ShowTooltipAfterDelay()
    {
        hoverStartTime = Time.time;  // Enregistrer le temps de survol

        // Attendre 3 secondes avant d'afficher le tooltip
        yield return new WaitForSeconds(hoverTime);

        // Si la souris est toujours sur l'élément après 3 secondes, afficher le tooltip
        showTooltip = true;
    }

    void OnGUI()
    {
        if (showTooltip)
{
    Vector2 mousePosition = Event.current.mousePosition;
    Vector2 screenSize = new Vector2(Screen.width, Screen.height);

    GUIStyle tooltipStyle = new GUIStyle(GUI.skin.box);
    tooltipStyle.wordWrap = true;

    string[] lines = tooltipText.Split('\n');

    float maxWidth = 0;
    foreach (string line in lines)
    {
        float width = tooltipStyle.CalcSize(new GUIContent(line)).x;
        if (width > maxWidth)
            maxWidth = width;
    }

    // Définir les dimensions de la boîte
    float boxWidth = maxWidth + 20;
    float boxHeight = lines.Length * 20 + 10;

    // Position initiale de la boîte
    float posX = mousePosition.x - boxWidth / 2;
    float posY = mousePosition.y + 30;

    // Ajustement pour ne pas dépasser l'écran en X
    if (posX < 10) posX = 10;
    if (posX + boxWidth > screenSize.x - 10) posX = screenSize.x - boxWidth - 10;

    // Ajustement pour ne pas dépasser l'écran en Y
    if (posY + boxHeight > screenSize.y - 10)
        posY = mousePosition.y - boxHeight - 10; // Au-dessus de la souris si dépasse

    if (posY < 10)
        posY = 10;

    GUI.Box(new Rect(posX, posY, boxWidth, boxHeight), tooltipText, tooltipStyle);
}
    }
}
