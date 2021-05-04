using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintAdjuster : MonoBehaviour
{
    public Vector3 sprintPosition;
    public Vector3 sprintRotation;

    private Vector3 normalPosition;
    private Quaternion normalRotation;
    private bool isSprinting = false;

    // Adjusts item when player is sprinting.
    public void SetSprint()
    {
        if (!isSprinting)
        {
            normalPosition = transform.localPosition;
            normalRotation = transform.localRotation;
            transform.localPosition = sprintPosition;
            transform.localRotation = Quaternion.Euler(sprintRotation);

            isSprinting = true;
        }
    }

    // Resets item to original location.
    public void SetNormal()
    {
        if (isSprinting)
        {
            transform.localPosition = normalPosition;
            transform.localRotation = normalRotation;
            isSprinting = false;
        }
        
    }
}
