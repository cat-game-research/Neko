
using System.Collections.Generic;
using UnityEngine;

public class AgentPrefabSpawner : MonoBehaviour
{
    public GameObject m_AgentsGroup;
    public GameObject m_AgentPrefab;
    public int m_AgentsToSpawn = 10;
    public LayerMask m_EnvironmentLayer;
    public bool m_IsRandomSpawn = true;
    public float m_MinDistance = 20f;
    public float m_SpawnRadius = 100;
    public float m_AgentOffsetY = 0f;

    List<GameObject> _Agents = new List<GameObject>();
    int _Checks = 0;

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

    private void Start()
    {
        if (m_AgentPrefab == null)
        {
            throw new MissingReferenceException("Agent Prefab GameObject must be defined");
        }

        for (int i = 0; i < m_AgentsToSpawn; i++)
        {
            var agent = Instantiate(m_AgentPrefab, m_AgentsGroup.transform);

            _Agents.Add(agent);

            if (m_IsRandomSpawn)
            {
                var randomPosition = GetRandomPosition(m_SpawnRadius);
                var position = agent.transform.position;

                position.x = randomPosition.x;
                position.z = randomPosition.y;

                while (m_AgentsToSpawn * m_AgentsToSpawn >= _Checks++ &&
                       IsTooClose(randomPosition, m_MinDistance, _Agents))
                {
                    randomPosition = GetRandomPosition(m_SpawnRadius);
                    position.x = randomPosition.x;
                    position.z = randomPosition.y;
                }

                if (Physics.Raycast(position, Vector3.down, out var hit, Mathf.Infinity, m_EnvironmentLayer))
                {
                    position.y = hit.point.y + m_AgentOffsetY;
                }

                agent.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
            }
        }
    }

    private Vector2Int GetRandomPosition(float radius)
    {
        return Vector2Int.RoundToInt(Random.insideUnitCircle * radius);
    }

    private bool IsTooClose(Vector2Int position, float minDistance, List<GameObject> columns)
    {
        foreach (var column in columns)
        {
            var prevPosition = new Vector2Int((int)column.transform.position.x, (int)column.transform.position.z);
            var distance = Vector2Int.Distance(position, prevPosition);
            if (distance < minDistance)
            {
                return true;
            }
        }

        return false;
    }
}
