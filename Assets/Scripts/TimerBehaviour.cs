using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TimerBehaviour : MonoBehaviour
{
    [SerializeField] public float duration=3f;
    [SerializeField] private UnityEvent onTimerEnd = null;
    private Timer timer;

    public void StartTimer(float timerDuration=3f)
    {
        if (timer != null)
        {
            timer.RemainingTime = duration;
        }
        else
        {
            timer = new Timer(duration);
            timer.OnTimerEnd += HandleTimerEnd;
        }
        timer.stopped = false;

    }
    private void HandleTimerEnd()
    {
        onTimerEnd.Invoke();
        Debug.Log("Timer has ended");
        duration = 3f;
        //this.enabled = false;
    }

    private void Update()
    {
        if (timer != null)
        {
            timer.Tick(Time.deltaTime);
        }
    }


    public void StopTimer()
    {
        if (timer != null)
        {
            timer.stopped = true;
            timer.OnTimerEnd -= HandleTimerEnd;
            timer = null;
            
        }
        

    }
    private void OnDisable()
    {
        if (timer != null)
        {
            timer.OnTimerEnd -= HandleTimerEnd;
        }  
        
    }
}
