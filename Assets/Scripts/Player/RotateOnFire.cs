using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnFire : MonoBehaviour
{
    public float rotationAmount = -90f * Mathf.Deg2Rad;
    public float dropHeight = .3f;

    private bool doRotate = false;
    private Quaternion endRotation;

    private Transform cameraMain;

    private Vector3 start, end;

    private float time = 0f;

    void Start()
    {
        cameraMain = Camera.main.transform;
        start = cameraMain.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (doRotate)
        {
            time += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, endRotation, time / .2f);
        }
        else if (transform.localRotation != Quaternion.Euler(0, 0, 0) || cameraMain.localPosition != start)
        {
            time += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), time / .05f);
        }
    }

    public void SetDoRotate(bool doRotate)
    {
        if (!this.doRotate)
        {
            endRotation = Quaternion.Euler(0, rotationAmount, 0);
            
            end = start + Vector3.down * dropHeight;
        }
        time = 0;
        this.doRotate = doRotate;

    }
}
