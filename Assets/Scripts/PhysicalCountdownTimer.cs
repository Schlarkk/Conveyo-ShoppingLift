using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PhysicalCountdownTimer : MonoBehaviour
{
    [Header("Set Start Time")]
    [Range(0, 59)] public int startMinutes = 0;
    [Range(0, 59)] public int startSeconds = 30;

    [Header("Digit Parents (00:00 format)")]
    public Transform digit0; // tens of minutes
    public Transform digit1; // ones of minutes
    public Transform digit2; // tens of seconds
    public Transform digit3; // ones of seconds

    [Header("Digit Colors")]
    public Color normalColor = Color.black;
    public Color warningColor = Color.red;

    [Header("Timer End Event")]
    public UnityEvent onTimerEnd;
    public UnityEvent onTimerRed;
    public UnityEvent onTimerBlack;

    public float timeRemaining;
    private bool timerRunning = true;

    void Start()
    {
        timeRemaining = startMinutes * 60 + startSeconds;
        UpdateDisplay();
    }

    void Update()
    {
        if (!timerRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            onTimerEnd.Invoke();
        }

        if (timeRemaining <= 10f)
        {
            onTimerRed.Invoke();
        }
        else if (timeRemaining > 10f)
        {
            onTimerBlack.Invoke();
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        int totalSeconds = Mathf.FloorToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        int d0 = minutes / 10;
        int d1 = minutes % 10;
        int d2 = seconds / 10;
        int d3 = seconds % 10;

        SetActiveDigit(digit0, d0);
        SetActiveDigit(digit1, d1);
        SetActiveDigit(digit2, d2);
        SetActiveDigit(digit3, d3);

        ColorDigits(totalSeconds <= 10 ? warningColor : normalColor);
    }

    void SetActiveDigit(Transform digitParent, int value)
    {
        for (int i = 0; i < digitParent.childCount; i++)
        {
            GameObject child = digitParent.GetChild(i).gameObject;
            child.SetActive(i == value);
        }
    }

    void ColorDigits(Color targetColor)
    {
        SetDigitColor(digit0, targetColor);
        SetDigitColor(digit1, targetColor);
        SetDigitColor(digit2, targetColor);
        SetDigitColor(digit3, targetColor);
    }

    void SetDigitColor(Transform digitParent, Color color)
    {
        for (int i = 0; i < digitParent.childCount; i++)
        {
            GameObject digit = digitParent.GetChild(i).gameObject;
            if (digit.activeSelf)
            {
                Image img = digit.GetComponent<Image>();
                if (img != null)
                {
                    img.color = color;
                }
            }
        }
    }

    // Optional control functions
    public void StartTimer() => timerRunning = true;
    public void StopTimer() => timerRunning = false;
    public void ResetTimer()
    {
        timeRemaining = startMinutes * 60 + startSeconds;
        timerRunning = true;
        UpdateDisplay();
    }

    public void AddTime(float seconds)
    {
        timeRemaining = Mathf.Max(0, timeRemaining + seconds);
        UpdateDisplay();
    }
}



