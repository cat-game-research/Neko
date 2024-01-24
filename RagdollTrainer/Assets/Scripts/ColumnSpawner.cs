using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class ColumnSpawner : MonoBehaviour
{
    public GameObject m_ColumnsGroup;
    public GameObject m_WallColumnPrefab;
    public int m_NumberOfColumns;
    public float m_GroundRadius;
    public LayerMask m_EnvironmentLayer;
    [Tooltip("This will make the columns remain where they are after each episode")]
    public bool m_OverwriteColumns = true;
    [Tooltip("This will set the minimum distance between two columns")]
    public float m_MinDistance = 1f;

    List<GameObject> _Columns = new List<GameObject>();

    public void RandomizeColumns(int minAmount, int maxAmount)
    {
        if (minAmount < 1 || maxAmount < 1)
        {
            throw new ArgumentException("The minAmount and maxAmount parameters must be positive integers");
        }

        if (maxAmount < minAmount)
        {
            throw new ArgumentException("The maxAmount parameter must be greater than or equal to the minAmount parameter");
        }

        int newNumberOfColumns = Random.Range(minAmount, maxAmount);

        ClearColumns();

        for (int i = 0; i < newNumberOfColumns; i++)
        {
            var column = Instantiate(m_WallColumnPrefab, transform);
            _Columns.Add(column);

            var randomPosition = GetRandomPosition(m_GroundRadius);
            var position = column.transform.position;

            position.x = randomPosition.x;
            position.z = randomPosition.y;

            while (IsTooClose(randomPosition, _Columns, m_MinDistance))
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
        if (m_OverwriteColumns)
        {
            foreach (Transform child in m_ColumnsGroup.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        _Columns.Clear();
    }

    private Vector2Int GetRandomPosition(float radius)
    {
        Vector2 randomPosition = Random.insideUnitCircle;
        randomPosition *= radius;
        return Vector2Int.RoundToInt(randomPosition);
    }

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

        RandomizeColumns(m_NumberOfColumns, m_NumberOfColumns);
    }

    private bool IsTooClose(Vector2Int position, List<GameObject> columns, float minDistance)
    {
        foreach (var prevColumn in columns)
        {
            var prevPosition = new Vector2Int((int)prevColumn.transform.position.x, (int)prevColumn.transform.position.z);
            var distance = Vector2Int.Distance(position, prevPosition);
            if (distance < minDistance)
            {
                return true;
            }
        }
        return false;
    }
}
