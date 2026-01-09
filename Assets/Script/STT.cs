using UnityEngine;
using Oculus.Voice;
using Unity.VisualScripting;
using System.IO;

public class PromptFromSTT : MonoBehaviour
{
    public AppVoiceExperience voiceExperience;
    public LlamaVisionClient llamaClient;
    public bool StatusCam360 = true;

    public void StartListening()
    {
        if (voiceExperience && !voiceExperience.Active) voiceExperience.Activate();
    }

    public void StopListening()
    {
        if (voiceExperience && voiceExperience.Active) voiceExperience.Deactivate();
    }

    private void OnEnable()
    {
        Debug.Log($"{name}: OnEnable chiamato");
        var e = voiceExperience.VoiceEvents;
        e.OnFullTranscription.AddListener(OnFull);
        e.OnError.AddListener(OnError);
        e.OnStartListening.AddListener(() => Debug.Log("Ascolto iniziato"));
        e.OnStoppedListening.AddListener(() => Debug.Log("Ascolto terminato"));
    }

    private void OnFull(string text)
    {
        Debug.Log("Finale: " + text);
        llamaClient.SetPromptAndSend(text, StatusCam360);
    }

    private void OnError(string error, string message)
    {
        Debug.LogError($"Wit errore: {error} | {message}");
    }

    void Update()
    {
        Debug.Log("Update");

        if (OVRInput.GetDown(OVRInput.RawButton.B))
        //if (Input.GetKeyDown(KeyCode.Space))
        {
            StatusCam360 = !StatusCam360;
            Debug.Log($"Lo stato di cam 360 è cambiato in: {StatusCam360}");
            if(StatusCam360)
            {
                FindObjectOfType<PopupMessageVR>().ShowPopup($"Camera 360 attivata");
            }
            else
            {
                FindObjectOfType<PopupMessageVR>().ShowPopup($"Camera 360 disattivata");
            }
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            Debug.Log("Premuto spazio");
            StartListening();
        }

        //if (Input.GetKeyUp(KeyCode.Space))
        if (OVRInput.GetUp(OVRInput.RawButton.A))
            StopListening();
    }
}
