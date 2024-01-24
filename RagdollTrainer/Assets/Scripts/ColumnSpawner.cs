using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class ColumnSpawner : MonoBehaviour
{
    public GameObject m_ColumnsController;
    public GameObject m_ColumnsGroup;
    public GameObject m_WallColumnPrefab;
    public int m_NumberOfColumns;
    public float m_GroundRadius;
    public LayerMask m_EnvironmentLayer;
    [Tooltip("This will make the columns remain where they are after each episode")]
    public bool m_OverwriteColumns = true;

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
            GameObject column = Instantiate(m_WallColumnPrefab, m_ColumnsController.transform);
            _Columns.Add(column);

            Vector2 randomPosition = GetRandomPosition(m_GroundRadius);
            Vector3 position = column.transform.position;

            position.x = randomPosition.x;
            position.z = randomPosition.y;

            Collider[] overlaps = Physics.OverlapSphere(position, column.transform.localScale.x / 2, m_EnvironmentLayer);

            while (overlaps.Length > 0)
            {
                randomPosition = GetRandomPosition(m_GroundRadius);

                position.x = randomPosition.x;
                position.z = randomPosition.y;

                overlaps = Physics.OverlapSphere(position, column.transform.localScale.x / 2, m_EnvironmentLayer);
            }

            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, m_EnvironmentLayer))
            {
                position.y = hit.point.y;
            }

            column.transform.position = position;
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


    private Vector2 GetRandomPosition(float radius)
    {
        Vector2 randomPosition = Random.insideUnitCircle;
        randomPosition *= radius;
        return randomPosition;
    }

    private void Start()
    {
        if (m_ColumnsController == null)
        {
            throw new MissingReferenceException("Column Controller must be defined");
        }

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
}
