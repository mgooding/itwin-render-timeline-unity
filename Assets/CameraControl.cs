using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }


    private void Update()
    {
        var mousePos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Middle mouse panning
        if (Input.GetMouseButton(2))
        {
            const float panSpeed = 50.0f;
            transform.Translate(-mousePos.x * Time.deltaTime * panSpeed, -mousePos.y * Time.deltaTime * panSpeed, 0);
            return;
        }

        // Translation zoom with scroll wheel
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mouseWheel) > 0.01f)
        {
            const float zoomSpeed = 25.0f;
            transform.Translate(0, 0, mouseWheel * zoomSpeed, Space.Self);
            return;
        }

        // Mouse look when right-click is down
        if (Input.GetMouseButton(1))
        {
            const float lookSpeed = 2.0f;

            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.y += lookSpeed * mousePos.x;
            eulerAngles.x -= lookSpeed * mousePos.y;
            transform.eulerAngles = eulerAngles;
            // No return, can be combined with WASD movement
        }

        // WASD movement
        Vector3 translation = Vector3.zero;
        bool moved = false;

        const float flySpeed = 10.0f;
        const float shiftMultiplier = 2.5f;

        float frameFlySpeed = Time.deltaTime * flySpeed;
        if (Input.GetKey(KeyCode.LeftShift)) frameFlySpeed *= shiftMultiplier;
        if (Input.GetKey(KeyCode.S)) { translation.z -= frameFlySpeed; moved = true; }
        if (Input.GetKey(KeyCode.W)) { translation.z += frameFlySpeed; moved = true; }
        if (Input.GetKey(KeyCode.A)) { translation.x -= frameFlySpeed; moved = true; }
        if (Input.GetKey(KeyCode.D)) { translation.x += frameFlySpeed; moved = true; }
        if (Input.GetKey(KeyCode.Q)) { translation.y -= frameFlySpeed; moved = true; }
        if (Input.GetKey(KeyCode.E)) { translation.y += frameFlySpeed; moved = true; }

        if (moved) transform.Translate(translation, Space.Self);
    }
}
