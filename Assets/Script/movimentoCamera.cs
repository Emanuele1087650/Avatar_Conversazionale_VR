using UnityEngine;

public class EditorCameraMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 70f;

    void Update()
    {
        // Muovi con WASD
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.position += (transform.forward * v + transform.right * h) * moveSpeed * Time.deltaTime;

        // Ruota con frecce
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
