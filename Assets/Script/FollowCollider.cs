using UnityEngine;

public class FollowHeadCollider : MonoBehaviour
{
    public OVRCameraRig ovrRig;              // dragga qui l'OVRCameraRig (figlio)
    public BoxCollider box;   // dragga qui il CharacterController

    public float minHeight = 1f;
    public float maxHeight = 2f;

    void Reset()
    {
        box = GetComponent<BoxCollider>();
        if (!ovrRig) ovrRig = GetComponentInChildren<OVRCameraRig>();
    }

    void LateUpdate()
    {
        if (!ovrRig || !box) return;

        Transform head = ovrRig.centerEyeAnchor;

        // posizione della testa nello spazio locale del Player
        Vector3 localHead = transform.InverseTransformPoint(head.position);        

        // altezza del capsule
        float h = Mathf.Clamp(localHead.y, minHeight, maxHeight);

        // centra il collider sotto la testa
        Vector3 c = box.center;
        c.x = localHead.x;
        c.y = h * 0.5f;
        c.z = localHead.z;
        box.center = c;
    } 
}
