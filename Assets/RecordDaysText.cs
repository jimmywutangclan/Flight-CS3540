using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordDaysText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text =
            "You've survived " + PlayerPrefs.GetFloat("RecordDays").ToString("0.0") + " days so far";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
