using UnityEngine;

public class RightStickMovement : MonoBehaviour
{
    public float speed = 2.0f;      // Velocità movimento
    public float smooth = 5.0f;     // Smorzamento movimento
    public float rotationSpeed = 2.0f;  // Velocità rotazione della telecamera

    private Vector3 targetMovement;

    void Update()
    {
        // Leggi l'analogico destro (X = orizzontale, Y = verticale)
        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        // Converti input in movimento relativo all'orientamento della camera
        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 left = new Vector3(transform.right.x, 0, transform.right.z).normalized;

        Vector3 movement = (forward * leftStick.y + left * leftStick.x) * speed;

        targetMovement = Vector3.Lerp(targetMovement, movement, Time.deltaTime * smooth);

        // Applica il movimento all’OVRCameraRig
        transform.position += targetMovement * Time.deltaTime;

        // Leggi l'input dell'analogico sinistro (X = rotazione orizzontale)
        float rightStickX = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        // Ruota l'OVRCameraRig intorno all'asse Y in base all'input dell'analogico sinistro
        transform.Rotate(0, rightStickX * rotationSpeed, 0);
    }
}
