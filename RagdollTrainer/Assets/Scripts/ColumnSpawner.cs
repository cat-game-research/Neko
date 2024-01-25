using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class ColumnSpawner : MonoBehaviour
{
    //TODO this should be managed automatically, with optional parent gameobject, no parent just put in root of hierarchy
    public GameObject m_ColumnsGroup;
    public GameObject m_WallColumnPrefab;
    public int m_NumberOfColumns;
    public float m_GroundRadius;
    public LayerMask m_EnvironmentLayer;
    [Tooltip("This will make the columns remain where they are after each episode")]
    public bool m_ClearColumns = true;
    [Tooltip("This will randomize the position of columns at the start of every episode")]
    public bool m_RandomizeColumns = true;
    [Tooltip("This will set the minimum distance between two columns")]
    public float m_MinDistance = 8f;

    List<GameObject> _Columns = new List<GameObject>();
    int _Checks = 0;

    private void Start()
    {
        if (m_ColumnsGroup == null)
        {
            throw new MissingReferenceException("Columns Group GameObject must be defined");
        }

        if (m_WallColumnPrefab == null)
        {
            throw new MissingReferenceException("Wall Column Prefab must be defined");
        }
    }

    public void RandomizeColumns()
    {
        RandomizeColumns(m_NumberOfColumns);
    }

    public void RandomizeColumns(int numOfColumns)
    {
        if (m_ClearColumns)
        {
            ClearColumns();
        }

        if (!m_RandomizeColumns)
        {
            return;
        }

        for (int i = 0; i < numOfColumns; i++)
        {
            var column = Instantiate(m_WallColumnPrefab, m_ColumnsGroup.transform);
            _Columns.Add(column);

            var randomPosition = GetRandomPosition(m_GroundRadius);
            var position = column.transform.position;

            position.x = randomPosition.x;
            position.z = randomPosition.y;

            while (numOfColumns * numOfColumns >= _Checks++ &&
                   IsTooClose(randomPosition, m_MinDistance, _Columns))
            {
                randomPosition = GetRandomPosition(m_GroundRadius);
                position.x = randomPosition.x;
                position.z = randomPosition.y;
            }

            if (Physics.Raycast(position, Vector3.down, out var hit, Mathf.Infinity, m_EnvironmentLayer))
            {
                position.y = hit.point.y;
            }

            column.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
        }
    }

    private void ClearColumns()
    {
        foreach (Transform child in m_ColumnsGroup.transform)
        {
            Destroy(child.gameObject);
        }

        _Columns.Clear();
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
