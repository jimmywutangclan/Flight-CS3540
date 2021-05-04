using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public Slider slider;
    public Text text;

    // Sets the max value for the stat bar, and automatically puts the current
    // value to max.
    public void SetMaxValue(float maxValue)
    {
        slider.maxValue = maxValue;
        slider.value = maxValue;
    }

    // Adds "change" to the slider value.
    public void UpdateValue(float change)
    {
        slider.value = slider.value + change;
        text.text = slider.value.ToString() + " / " + slider.maxValue;
    }

    // Sets the slider value to value.
    public void SetValue(float value)
    {
        slider.value = value;
        text.text = value.ToString() + " / " + slider.maxValue;
    }
}
