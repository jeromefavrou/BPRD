using UnityEngine;
using UnityEngine.UI;

public class FullScreenToggle : MonoBehaviour
{
    // Référence au bouton
    private Button fullScreenButton;

    // Variable pour suivre l'état du mode plein écran
    private bool isFullScreen = false;
    void Awake()
    {
        // Récupérer le composant Button attaché à ce GameObject
        fullScreenButton = GetComponent<Button>();
    }
    void Start()
    {
        // Assurez-vous que le bouton appelle la fonction ToggleFullScreen quand il est cliqué
        fullScreenButton.onClick.AddListener(ToggleFullScreen);
    }

    void Update()
    {
       // Alterner entre plein écran et fenêtre avec le raccourci F11 (par exemple)
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullScreen();
        }
    }

    void ToggleFullScreen()
    {
        // Alterner entre plein écran et mode fenêtre
        isFullScreen = !isFullScreen;

        if (isFullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            // Vous pouvez définir une résolution de fenêtre spécifique ici si nécessaire
            Screen.SetResolution(1366, 768, false);
        }
    }
}