using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private TextMeshProUGUI text;
    private bool isRunning = false;
    private float timeout;

    public double TimeElapsedSec = 0f;

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
            TimeElapsedSec += Time.deltaTime;
            var time = TimeSpan.FromSeconds(TimeElapsedSec);
            text.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        }
    }

    public void StartTimer()
    {
        TimeElapsedSec = 0;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}
