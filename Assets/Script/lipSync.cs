using UnityEngine;
using Oculus.Avatar2; // namespace Avatar 2.0

[RequireComponent(typeof(OvrAvatarLipSyncContext))]
public class TTSAudioLipSync : MonoBehaviour
{
    public AudioSource ttsAudio;
    private OvrAvatarLipSyncContext lipSyncContext;
    private float[] samples = new float[1024];

    void Awake()
    {
        lipSyncContext = GetComponent<OvrAvatarLipSyncContext>();
    }

    void Update()
    {
        if (ttsAudio != null && ttsAudio.isPlaying)
        {
            ttsAudio.GetOutputData(samples, 0);
            lipSyncContext.ProcessAudioSamples(samples, samples.Length);
        }
    }
}
