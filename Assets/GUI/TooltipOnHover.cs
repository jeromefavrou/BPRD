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
            // Obtenir la position de la souris
            Vector2 mousePosition = Event.current.mousePosition;

            //screen size
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            // Créer un style de boîte pour le tooltip
            GUIStyle tooltipStyle = new GUIStyle(GUI.skin.box);
            tooltipStyle.wordWrap = true;  // Permet de découper le texte en plusieurs lignes si nécessaire

            //x automatique en fonction de la taille du texte
            //commance par séparé le texte en ligne
            string[] lines = tooltipText.Split('\n');

            //regarde la taille de la plus grande ligne
            float maxWidth = 0;
            foreach (string line in lines)
            {
                float width = tooltipStyle.CalcSize(new GUIContent(line)).x;
                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }

            Vector2 offsetBord = new Vector2(0, 0); // Décalage par rapport au bord de l'écran

            // Vérifier si le tooltip dépasse les bords de l'écran
            if( mousePosition.x + maxWidth/2 > screenSize.x -10 )
            {
                offsetBord.x = mousePosition.x + maxWidth/2 - (screenSize.x-10) ;
            }
            else if( mousePosition.x - maxWidth/2 < 0)
            {
                offsetBord.x = mousePosition.x - maxWidth/2 ;
            }

        
            //ajuste la taille de la boite en fonction de la taille du texte
            GUI.Box(new Rect(mousePosition.x - maxWidth/2 - offsetBord.x, mousePosition.y + 30, maxWidth + 20, lines.Length * 20 + 10), tooltipText, tooltipStyle);

        }
    }
}
