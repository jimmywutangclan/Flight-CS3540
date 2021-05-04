using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAdjuster : MonoBehaviour
{
    public Vector3 sprintPosition;
    public Vector3 normalPosition;
    // Start is called before the first frame update
    void Start()
    {
        normalPosition = Camera.main.transform.localPosition;
    }

    public void SetSprintPosition()
    {
        Camera.main.transform.localPosition = sprintPosition;
    }

    public void ResetCameraPosition()
    {
        Camera.main.transform.localPosition = normalPosition;
    }
}
