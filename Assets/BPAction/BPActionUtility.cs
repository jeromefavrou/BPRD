using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//onbjet template 
public class BPAction : MonoBehaviour
{
        public GameObject bckObj;
        public GameObject fwdObj;
        public Error errManager;
        public ProgressBarre progressBarre;
        public GeneralStatUtils gen_data;
        protected Button button;
        protected bool isProcessing = false;

        void Awake()
        {
            button = GetComponent<Button>();

            if( progressBarre == null || gen_data == null || errManager == null || button == null)
            {
                Debug.LogError(" non assigné !");
                return;
            }
            else
            {
                button.onClick.AddListener(actionCall);
            }
        }

        void actionCall()
        {
            if(isProcessing)
            {
                errManager.addWarning("Action deja en cours");
                return;
            }

            StartCoroutine(action());
        }

        protected virtual IEnumerator action()
        {
            yield return null;
        }
    
}
