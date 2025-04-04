using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ConnectivityCheck : MonoBehaviour
{
    public string version = "v0.3.0-alpha";
    private string url ;
    private static string release = "https://github.com/jeromefavrou/BPRD/releases/tag/";
    public Error errManager;
    private void Start()
    {
        // Lance la vérification de la version
    
        url = release + version ;
        url.Trim();
        //url = "https://github.com/jeromefavrou/BPRD/releases/tag/v0.2.4-alpha";

        
        StartCoroutine(CheckForUpdate(url));
    }

    private IEnumerator CheckForUpdate( string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        { 
            // Envoie la requête et attend la réponse
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("L'URL est accessible.");
                errManager.addLog("Version à jour : " + version);
            }
            else
            {
                // L'URL n'est pas accessible, affichage dans la console avec l'erreur
                errManager.addWarning(version + " => Une nouvelle version est peut etre disponible ou connection echoué a : " + release);
            }
        }
    }
}
