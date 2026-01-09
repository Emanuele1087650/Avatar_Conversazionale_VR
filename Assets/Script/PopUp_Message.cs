using UnityEngine;
using TMPro;
using System.Collections;

public class PopupMessageVR : MonoBehaviour
{
    public TextMeshProUGUI popupText;    // Il testo TMP 3D
    public GameObject popupPanel;
    public float displayTime = 2f;   // Durata in secondi
    //public KeyCode triggerKey = KeyCode.Space; // Tasto di attivazione

    private Coroutine currentCoroutine;

    void Start()
    {
        //popupText.gameObject.SetActive(false);
        if (popupPanel != null)
            popupPanel.SetActive(false);
        else
            Debug.LogError("❌ popupPanel NON assegnato!");

        if (popupText == null)
            Debug.LogError("❌ popupText NON assegnato!");
    }

    void Update()
    {
        // In simulatore puoi usare un tasto da tastiera
        /*if (Input.GetKeyDown(KeyCode.Space)) 
        {
            ShowPopup("Ciao dal simulatore Meta!");
        }*/
    }

    public void ShowPopup(string message)
    {
        if (popupPanel == null || popupText == null)
        {
            Debug.LogError("❌ popupPanel o popupText mancanti!");
            return;
        }

        popupText.text = message;
        Debug.Log("Popup comparsa");

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(PopupRoutine());
    }

    private IEnumerator PopupRoutine()
    {
        popupPanel.SetActive(true);
        //popupText.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayTime);
        popupPanel.SetActive(false);
        //popupText.gameObject.SetActive(false);
    }
}

