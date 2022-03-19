using System;
using System.Diagnostics;

public class Timer
{
    public float RemainingTime { get; set; }
    public bool stopped = false;
    public Timer(float duration)
    {
        RemainingTime = duration;
    }
    public event Action OnTimerEnd;
    public void Tick(float deltaTime)
    {
        if (RemainingTime == 0f)
        {
            return;
        }
        RemainingTime -= deltaTime;

        CheckForTimerEnd();
    }

    private void CheckForTimerEnd()
    {
        if (RemainingTime > 0f)
        {
            return;
        }
        RemainingTime = 0f;
        if (!stopped)
        {
            OnTimerEnd?.Invoke();
        }
        
       
    }
}
