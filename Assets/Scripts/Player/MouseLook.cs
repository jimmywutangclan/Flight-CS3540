using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    Transform playerBody;
    public float sensitivity = 360;

    public UIManager uIManager;

    float pitch = 0;

    private WorldSettings worldSettings;

    // Start is called before the first frame update
    void Start()
    {
        worldSettings = FindObjectOfType<WorldSettings>();

        playerBody = transform.parent.transform;
        worldSettings = FindObjectOfType<WorldSettings>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!worldSettings.IsGameOver() && !uIManager.UIShown)
        {
            float moveX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float moveY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

            // yaw
            playerBody.Rotate(Vector3.up * moveX);

            pitch -= moveY;

            pitch = Mathf.Clamp(pitch, -85, 85);

            transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }

    public void SetDeathRotation()
    {
        transform.localRotation = Quaternion.Euler(90, 0, 0);
    }
}
