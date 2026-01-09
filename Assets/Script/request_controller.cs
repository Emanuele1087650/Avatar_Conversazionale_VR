using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Meta.WitAi.TTS.Utilities;
using System.Diagnostics;
using OVRSimpleJSON;
using System.IO;

public class LlamaVisionClient : MonoBehaviour
{
    public TTSSpeaker speaker;

    [Header("Server")]
    [SerializeField] private string serverUrl = "http://127.0.0.1:8000/chat-multipart";
    [SerializeField] private string prompt; //= "Descrivi la scena";
    [SerializeField] private bool stream = false; // vedi nota su SSE più sotto

    [Header("Camera capture")]
    [SerializeField] private Camera sourceCamera;
    [SerializeField] private int width = 640;
    [SerializeField] private int height = 360;
    [SerializeField] private bool usePNG = false;  // PNG = lossless, più pesante
    [SerializeField, Range(1,100)] private int jpgQuality = 35;

    private bool _sending;
    private bool cam360 = false;
    public byte[] imgBytes;
    public string fileName = "frame.jpg";
    public string mime = "image/jpeg";


    public void SetPromptAndSend(string newPrompt, bool StatusCam)
    {
        if (_sending) {
            UnityEngine.Debug.LogWarning("Richiesta già in corso: attendi la risposta precedente.");
            return;
        }
        prompt = newPrompt;
        cam360 = StatusCam;
        StartCoroutine(CaptureAndSendFrame());
    }

    private IEnumerator CaptureAndSendFrame()
    {
        var sw = Stopwatch.StartNew();
        if (sourceCamera == null)
        {
            yield break;
        }

        _sending = true;

        if (cam360 == false)
        {
            // 1) Render della camera in un RenderTexture
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture prevRT = sourceCamera.targetTexture;
            RenderTexture prevActive = RenderTexture.active;
            sourceCamera.targetTexture = rt;
            sourceCamera.Render();
            RenderTexture.active = rt;

            // 2) Copia su Texture2D CPU e codifica
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0,0,width,height), 0, 0);
            tex.Apply(false, false);

            // cleanup GPU state subito
            sourceCamera.targetTexture = prevRT;
            RenderTexture.active = prevActive;
            rt.Release();
            Destroy(rt);

            imgBytes = usePNG ? tex.EncodeToPNG() : tex.EncodeToJPG(jpgQuality);
            Destroy(tex);
        }
        else
        {
            int imageWidth = 1024;
            bool saveAsJPEG = true;

            byte[] bytes = I360Render.Capture(imageWidth, saveAsJPEG, sourceCamera);
            if (bytes != null)
            {
                string path = Path.Combine(Application.persistentDataPath, "360render" + (saveAsJPEG ? ".jpeg" : ".png"));
                File.WriteAllBytes(path, bytes);
                imgBytes = bytes;
                UnityEngine.Debug.Log("360 render saved to " + path);
            }
        }

        // 3) Costruzione multipart/form-data
        var form = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("prompt", prompt),
            new MultipartFormFileSection("images", imgBytes, fileName, mime)
        };

        sw.Stop();
        UnityEngine.Debug.Log($"L'immagine ha impiegato {sw.Elapsed.TotalMilliseconds:F3} ms");

        var sw2 = Stopwatch.StartNew();
        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError($"Upload fallito: {www.error}\n{www.downloadHandler.text}");
            }
            else
            {
                string json = www.downloadHandler.text;
                JSONNode root = JSON.Parse(json);
                // Aggiungere ["choices"][0]
                UnityEngine.Debug.Log($"Ollama: {root["choices"][0]["message"]["content"]}");
                if (root != null && root["choices"][0]["message"]["content"] != null)
                {
                    string testo = root["choices"][0]["message"]["content"];
                    UnityEngine.Debug.Log("Testo estratto: " + testo);
                    sw2.Stop();
                    UnityEngine.Debug.Log($"Ollama ha impiegato {sw2.Elapsed.TotalMilliseconds:F3} ms");
                    speaker.Speak(testo);
                }
                else
                {
                    UnityEngine.Debug.LogError("JSON non contiene message.content");
                }
            }
        }
        _sending = false;
    }
}
