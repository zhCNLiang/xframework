using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : class, new()
{
    private static T m_Instance;
    public static T Instance
    {
        get
        {
            if (null == m_Instance)
            {
                m_Instance = new T();
                (m_Instance as Singleton<T>).Init();
            }
            return m_Instance;
        }
    }

    protected virtual void Init()
    {

    }

    protected virtual void UnInit()
    {
        
    }

    public void Destroy()
    {
        if (m_Instance != null)
        {
            (m_Instance as Singleton<T>).UnInit();
            m_Instance = null;
        }
    }
}
