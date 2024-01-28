using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private LevelManager() { }

    private static LevelManager instance = null;

    public static LevelManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LevelManager();
            }
            return instance;
        }
    }

    [Header("Basic Scene Settings")]
    public bool m_IsTraining = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
