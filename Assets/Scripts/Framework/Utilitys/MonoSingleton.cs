using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool m_AppExit;
    private static T m_Instance;
    public static T Instance
    {
        get
        {
            if (null == m_Instance && !m_AppExit)
            {
                m_Instance = GameObject.FindObjectOfType<T>();
                if (null == m_Instance)
                {
                    var go = new GameObject(typeof(T).Name);
                    m_Instance = go.AddComponent<T>();
                }
                GameObject.DontDestroyOnLoad(m_Instance);
            }
            return m_Instance;
        }
    }

    void OnApplicationQuit()
    {
        m_AppExit = true;
    }
}