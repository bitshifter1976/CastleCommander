using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private TextMeshProUGUI text;
    private bool isRunning = false;
    private double timeElapsedSec = 0f;
    private double timeToEndRoundSec;

    // Start is called before the first frame update
    private void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (isRunning)
        {
            timeElapsedSec += Time.deltaTime;
            var time = TimeSpan.FromSeconds(timeToEndRoundSec - timeElapsedSec);
            text.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        }
    }

    public void StartTimer(float timeoutSec)
    {
        timeElapsedSec = 0;
        timeToEndRoundSec = timeoutSec;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public bool IsOver()
    {
        return timeElapsedSec >= timeToEndRoundSec;
    }
}
