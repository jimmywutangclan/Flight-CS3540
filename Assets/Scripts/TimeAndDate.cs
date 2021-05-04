using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeAndDate : MonoBehaviour
{
    public int dayDuration = 300;
    public bool runSkyBox = true;
    public bool alreadyChecked = true;
    public float nightAmbientIntensity = .3f;
    public float dayAmbientIntensity = .45f;
    public Color dayColor;
    public Color nightColor;
    public GameObject sun;
    public GameObject moon;

    public float numberDays;


    private float currentTime;


    private GameObject[] ambientLights;
    private float lightAmplitude;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("PersistCurrentTime", 600f, 600f);
        currentTime = 0f;
        numberDays = 0;
        sun = GameObject.FindWithTag("Sun");
        moon = GameObject.FindWithTag("Moon");

        if (sun != null)
            sun.GetComponent<Light>().color = dayColor;
        if (moon != null)
            moon.GetComponent<Light>().color = nightColor;

        ambientLights = GameObject.FindGameObjectsWithTag("AmbientLight");
        lightAmplitude = (dayAmbientIntensity - nightAmbientIntensity) / 2;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        numberDays = Mathf.Round(currentTime) / dayDuration;

        if (runSkyBox)
        {
            EditSkyBox();
        }
    }

    // Sets new day record if current day record surpasses old one
    private void PersistCurrentTime()
    {
        PlayerPrefs.SetFloat("RecordDays", Mathf.Max(numberDays, PlayerPrefs.GetFloat("RecordDays")));
    }
    
    private void EditSkyBox()
    {
        float rotateRate = (1f / dayDuration) * 360;

        sun.transform.RotateAround(Vector3.zero, Vector3.right, rotateRate * Time.deltaTime);
        sun.transform.LookAt(Vector3.zero);

        moon.transform.RotateAround(Vector3.zero, Vector3.right, rotateRate * Time.deltaTime);
        moon.transform.LookAt(Vector3.zero);

        SetAmbientIntensity(rotateRate);
    }

    private void SetAmbientIntensity(float rotation)
    {
        foreach(GameObject light in ambientLights)
        {
            // Fluctuates light via a trig function.
            light.GetComponent<Light>().intensity = (lightAmplitude * Mathf.Cos(2 * Mathf.PI * currentTime / dayDuration)) + (nightAmbientIntensity + lightAmplitude);
            light.GetComponent<Light>().color = Color.Lerp(dayColor, nightColor, Mathf.Sin((2 * Mathf.PI * currentTime / dayDuration) - (Mathf.PI / 2)) + 0.5f);

        }
    }
}
