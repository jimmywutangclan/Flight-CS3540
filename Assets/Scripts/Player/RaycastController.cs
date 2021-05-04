using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastController : MonoBehaviour
{
    private RaycastHit hitObject;
    private int layerMask = 0;
    private bool doRaycast = true;
    private Transform cameraMain;
    // Start is called before the first frame update
    void Start()
    {
        hitObject = new RaycastHit();
        cameraMain = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (doRaycast)
        {
            // Gets the raycast hitObject for the players forward direction.
            Physics.Raycast(cameraMain.position, cameraMain.forward, out hitObject,
                Mathf.Infinity, layerMask, QueryTriggerInteraction.Collide);
        }
    }

    // Bit wise or's the layer mask with the input.
    public void AddLayerMask(int layerMask)
    {
        this.layerMask |= layerMask;
    }

    // Sets whether or not the raycast should run.
    public void SetDoRaycast(bool doRaycast)
    {
        this.doRaycast = doRaycast;
    }

    // Gets whether or not the raycast runs.
    public bool GetDoRaycast()
    {
        return doRaycast;
    }

    public RaycastHit GetHitObject()
    {
        return hitObject;
    }
}
