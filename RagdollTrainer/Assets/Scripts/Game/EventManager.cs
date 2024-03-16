using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private EventManager() { }

    private static EventManager instance = null;

    public static EventManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EventManager();
            }
            return instance;
        }
    }

    [Header("Basic Event Settings")]
    public bool m_IsDebug = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
