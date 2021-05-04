using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovePhysicsAfterTime : MonoBehaviour
{
    public float removalTime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("RemovePhysics", removalTime);
    }

    private void RemovePhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }
}
