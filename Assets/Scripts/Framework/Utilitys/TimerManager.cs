using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class TimerManager : MonoSingleton<TimerManager>
{
    private class Timer
    {
        public Timer(Action callback, float interval, float delay = 0f, int loop = 0)
        {
            Callback = callback;
            Inverval = Mathf.Max(interval, 1f / 60);
            Delay = delay;
            Loop = loop;

            m_ElapseDelay = Delay;
            m_ExpiredDelay = Delay <= 0;
        }

        private Action Callback { get; set; }
        private float Inverval { get; set; }
        private float Delay { get; set; }
        private int Loop { get; set; }

        private float m_ElapseTime = 0;
        private float m_ElapseDelay = 0;
        private bool m_ExpiredDelay = false;
        private float m_ElapseLoop = 0;

        public bool OnTick(float dt)
        {
            if (Loop > 0 && m_ElapseLoop >= Loop)
            {
                return false;
            }

            if (m_ElapseDelay > 0)
            {
                m_ElapseDelay -= dt;
                return true;
            }

            if (!m_ExpiredDelay)
            {
                m_ExpiredDelay = true;
                Callback?.Invoke();
                m_ElapseLoop++;
                return true;
            }

            m_ElapseTime += dt;
            while(m_ElapseTime >= Inverval)
            {
                m_ElapseTime -= Inverval;
                Callback?.Invoke();
                m_ElapseLoop++;
            }

            return true;
        }
    }

    public static uint AddTimer(Action callback, float interval, float delay = 0f, int loop = 0)
    {
        return TimerManager.Instance.StartTimer(callback, interval, delay, loop);
    }

    public static void RemoveTimer(uint timerID)
    {
        TimerManager.Instance.StopTimer(timerID);
    }

    private Dictionary<uint, Timer> m_Timers = new Dictionary<uint, Timer>();
    private uint m_TimerID;
    public uint StartTimer(Action callback, float interval, float delay = 0f, int loop = 0)
    {
        m_TimerID++;
        m_Timers.Add(m_TimerID, new Timer(callback, interval, delay, loop));
        m_Timers.OrderBy(o => o.Key);
        return m_TimerID;
    }

    public void StopTimer(uint timerID)
    {
        if (m_Timers.ContainsKey(timerID))
        {
            m_Timers.Remove(timerID);
        }
    }

    private float m_LastTime;
    // Start is called before the first frame update
    void Start()
    {
        m_LastTime = Time.realtimeSinceStartup;
    }

    private List<uint> m_WaitRemove = new List<uint>();
    // Update is called once per frame
    void Update()
    {
        var dt = Time.realtimeSinceStartup - m_LastTime;
        m_LastTime = Time.realtimeSinceStartup;

        foreach(var item in m_Timers)
        {
            var key = item.Key;
            var timer = item.Value;
            if (!timer.OnTick(dt))
            {
                m_WaitRemove.Add(key);
            }
        }

        foreach(var key in m_WaitRemove)
        {
            m_Timers.Remove(key);
        }
        m_WaitRemove.Clear();
    }
}
