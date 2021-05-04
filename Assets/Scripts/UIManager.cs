using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool UIShown = false;
    public Text hintText;
    public Slider progressBar;

    enum HintState
    {
        Indefinite, Timed, Hidden
    }
    HintState hintState;
    float timeout = -1.0f;

    enum ProgressBarState
    {
        SlidingUp, SlidingDown, Hidden
    }
    public delegate void ProgressBarCallback();
    ProgressBarCallback OnProgressComplete;
    ProgressBarState progressBarState;
    float progressBarTotal = 0.0f;
    float progressBarTimeout = 0.0f;

    private void Start()
    {
        SetHintHidden();
        SetProgressBarHidden();
    }

    private void Update()
    {
        if (hintState == HintState.Timed)
        {
            timeout -= Time.deltaTime;
            if (timeout <= 0.0f)
            {
                SetHintHidden();
            }
        }

        if (progressBarState != ProgressBarState.Hidden)
        {
            progressBarTimeout -= Time.deltaTime;

            float sliderProgress = (progressBarState == ProgressBarState.SlidingUp ? progressBarTotal - progressBarTimeout : progressBarTimeout) / progressBarTotal;
            progressBar.value = sliderProgress;

            if (progressBarTimeout <= 0.0f)
            {
                OnProgressComplete();
                SetProgressBarHidden();
            }
        }
    }

    public void SetHintHidden()
    {
        hintText.text = "";
        hintState = HintState.Hidden;
    }

    public void SetIndefiniteHint(string hint)
    {
        hintText.text = hint;
        hintState = HintState.Indefinite;
    }
    public void SetTimedHint(string hint, float time)
    {
        hintText.text = hint;
        hintState = HintState.Timed;
        timeout = time;
    }

    public void SetProgressBarHidden()
    {
        progressBar.gameObject.SetActive(false);
        progressBarState = ProgressBarState.Hidden;
    }

    public void SetProgressBarCountUp(float time, ProgressBarCallback callback)
    {
        progressBar.gameObject.SetActive(true);

        progressBarState = ProgressBarState.SlidingUp;
        progressBarTotal = time;
        progressBarTimeout = time;
        OnProgressComplete = callback;
    }

    public void SetProgressBarCountDown(float time, ProgressBarCallback callback)
    {
        progressBar.gameObject.SetActive(true);

        progressBarState = ProgressBarState.SlidingDown;
        progressBarTotal = time;
        progressBarTimeout = time;
        OnProgressComplete = callback;
    }
}
