using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatController
{
    private float minValue, maxValue;

    private string[] messages;
    private bool hasMessages = false;
    private float currentValue, startValue;
    private float updatePerTick;

    public string CurrentMessage { get; private set; }
    public bool MessageChanged { get; set; }

    // Sets up stat when the stat should change periodically in a predefined manner.
    public StatController(float startValue, float endValue, float updateValueFreq, 
        float maxUpdateTime, bool degens)
    {
        this.startValue = startValue;
        currentValue = startValue;
        updatePerTick = startValue / maxUpdateTime * updateValueFreq;

        if (degens)
            updatePerTick *= -1;

        if (startValue > endValue)
        {
            minValue = endValue;
            maxValue = startValue;
        }
        else
        {
            minValue = startValue;
            maxValue = endValue;
        }
    }

    // Sets up stat when the stat is not changed continuously or only under
    // certain circumstances
    public StatController(float startValue, float endValue)
    {
        currentValue = startValue;

        if (startValue > endValue)
        {
            minValue = endValue;
            maxValue = startValue;
        }
        else
        {
            minValue = startValue;
            maxValue = endValue;
        }

        updatePerTick = 0;
    }

    public void SetMessages(string[] messages)
    {
        this.messages = messages;
        CurrentMessage = messages[0];
        hasMessages = true;
    }

    // Updates the stat based on a specific game instance (ex. taking damage).
    public void UpdateStatByAmount(float amount)
    {
        currentValue = Mathf.Clamp(currentValue + amount, minValue, maxValue);

        if (hasMessages)
            HandleMessage();
    }

    // Updates stat every tick (ex. health regen over time, or becoming hungry).
    public void UpdateStatOnTick(float multiplier)
    {
        currentValue = Mathf.Clamp(currentValue + (updatePerTick * multiplier), minValue, maxValue);

        if (hasMessages)
            HandleMessage();
    }

    public float GetCurrentValue()
    {
        return currentValue;
    }

    public float GetNormalUpdatePerTick()
    {
        return updatePerTick;
    }

    public float GetMaxValue()
    {
        return maxValue;
    }

    private void HandleMessage()
    {
        float proportion = currentValue / maxValue;
        string prevMessage = CurrentMessage;
        if (startValue != 0) // If it is better for the stat to be at the max value.
        {
            if (proportion == 1)
                CurrentMessage = messages[0];
            else if (proportion > 0.75f)
                CurrentMessage = messages[1];
            else if (proportion > .25f)
                CurrentMessage = messages[2];
            else if (proportion > 0)
                CurrentMessage = messages[3];
            else
                CurrentMessage = messages[4];
        }
        else // If it is better for the stat to be at 0.
        {
            if (proportion == 0)
                CurrentMessage = messages[0];
            else if (proportion < 0.25f)
                CurrentMessage = messages[1];
            else if (proportion < .75f)
                CurrentMessage = messages[2];
            else if (proportion < 1)
                CurrentMessage = messages[3];
            else
                CurrentMessage = messages[4];
        }

        if (!prevMessage.Equals(CurrentMessage))
            MessageChanged = true;
    }
}
