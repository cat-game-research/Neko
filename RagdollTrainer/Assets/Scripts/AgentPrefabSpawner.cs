using UnityEngine;

public class AgentPrefabSpawner : MonoBehaviour
{
    private AgentPrefabSpawner() { }

    private static AgentPrefabSpawner instance = null;

    public static AgentPrefabSpawner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AgentPrefabSpawner();
            }
            return instance;
        }
    }

    private void Awake()
    {
        Object.DontDestroyOnLoad(gameObject);
    }
}
