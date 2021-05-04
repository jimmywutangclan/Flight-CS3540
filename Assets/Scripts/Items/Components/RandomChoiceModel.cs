using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomChoiceModel : MonoBehaviour
{
    public GameObject[] modelChoices;

    private void Start() {
        int choice = Random.Range(0, modelChoices.Length);
        Instantiate(modelChoices[choice], transform.position, transform.rotation, transform);
    }
}
