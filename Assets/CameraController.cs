using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // Vitesse de mouvement avec la molette
    public float rotateSpeed = 5f; // Vitesse de rotation
    public float dragSpeed = 2f;   // Vitesse de déplacement avec le clic gauche

    private Vector3 initialPosition; // Position de départ de la caméra
    private float zoom = 10f; // Zoom de la caméra
    private float xRotation = 0f; // Rotation X pour le mouvement de la souris
    private float yRotation = 0f; // Rotation Y pour le mouvement de la souris


    public TMP_Dropdown viewType;
    public Map map;
    public GeneralStatUtils gen;

    private Camera cam;
    void Awake()
    {
        viewType.onValueChanged.AddListener(delegate { changeView();
            
        });

        cam = GetComponent<Camera>();
    }

    void Start()
    {

    }

    void Update()
    {

        // Déplacement de la caméra avec la molette
        zoom -= Input.GetAxis("Mouse ScrollWheel") * moveSpeed;
       // zoom = Mathf.Clamp(zoom, 1f, 20f);

       //deplace d'avant en arriere en focntion de la position local de la camera
       if( !cam.orthographic )
       {
        transform.Translate(0, 0, -Input.GetAxis("Mouse ScrollWheel") * moveSpeed);
       }
       else
       {
              cam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * moveSpeed;
       }
        

       //rotation du gameObject Main en fonction de la souris
        if (Input.GetMouseButton(1))
        {
            if(!cam.orthographic)
            {
                xRotation += Input.GetAxis("Mouse X") * rotateSpeed;
                yRotation -= Input.GetAxis("Mouse Y") * rotateSpeed;
                this.transform.rotation = Quaternion.Euler(yRotation, xRotation, 0);
            }
            else
            {
                yRotation -= Input.GetAxis("Mouse X") * rotateSpeed;
                map.transform.rotation = Quaternion.Euler(0, yRotation, 0);
            }

        }


        // Déplacement de la caméra avec le clic gauche de bas haut droite gauche
        if (Input.GetMouseButton(2))
        {
            transform.Translate(-Input.GetAxis("Mouse X") * dragSpeed, -Input.GetAxis("Mouse Y") * dragSpeed, 0);
        }

    }

    private void changeView()
    {
        if(viewType.value == 0)
        {
            //vue passe la projection en perspective
            cam.orthographic = false;
            map.transform.rotation = Quaternion.Euler(0, 0, 0);
            cam.nearClipPlane = 0.3f;
            
        }
        else if(viewType.value == 1)
        {
            //vue 2D passe la projection en orthogonal
            cam.orthographic = true;

            //position 
            transform.position = new Vector3(0, 0, (float)-gen.pp_data.size.y /2);
            transform.rotation = Quaternion.Euler(0, 0, 0);

            //near
            cam.nearClipPlane = (float)-gen.pp_data.size.y /2;


            transform.LookAt(map.transform);
            cam.orthographicSize = (float)gen.pp_data.size.x/4.0f;

            
        }
    }
}
