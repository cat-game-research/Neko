using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class RampSpawner : MonoBehaviour
{
    //TODO this should be managed automatically, with optional parent gameobject, no parent just put in root of hierarchy
    public GameObject m_RampsGroup;
    public GameObject m_RampPrefab;
    public int m_NumberOfRamps;
    public float m_GroundRadius;
    public LayerMask m_EnvironmentLayer;
    [Tooltip("This will make the ramps remain where they are after each episode")]
    public bool m_ClearRamps = true;
    [Tooltip("This will randomize the position of columns at the start of every episode")]
    public bool m_RandomizeRamps = true;
    [Tooltip("This will set the minimum distance between ramps")]
    public float m_MinDistance = 8f;
    public bool m_RandomizeOnStart = false;

    List<GameObject> _Ramps = new List<GameObject>();
    int _Checks = 0;

    private void Start()
    {
        if (m_RampsGroup == null)
        {
            throw new MissingReferenceException("Columns Group GameObject must be defined");
        }

        if (m_RampPrefab == null)
        {
            throw new MissingReferenceException("Wall Column Prefab must be defined");
        }

        if (m_RandomizeOnStart)
        {
            Randomize();
        }
    }

    public void Randomize()
    {
        Randomize(m_NumberOfRamps);
    }

    public void Randomize(int numOfRamps)
    {
        if (m_ClearRamps)
        {
            Clear();
        }

        if (!m_RandomizeRamps)
        {
            return;
        }

        for (int i = 0; i < numOfRamps; i++)
        {
            var ramp = Instantiate(m_RampPrefab, m_RampsGroup.transform);
            _Ramps.Add(ramp);

            var randomPosition = GameUtil.GetRandomPosition(m_GroundRadius);
            var position = ramp.transform.position;

            position.x = randomPosition.x;
            position.z = randomPosition.y;

            while (numOfRamps * numOfRamps >= _Checks++ &&
                   GameUtil.IsTooClose(randomPosition, m_MinDistance, _Ramps))
            {
                randomPosition = GameUtil.GetRandomPosition(m_GroundRadius);
                position.x = randomPosition.x;
                position.z = randomPosition.y;
            }

            if (Physics.Raycast(position, Vector3.down, out var hit, Mathf.Infinity, m_EnvironmentLayer))
            {
                position.y = hit.point.y;
            }

            ramp.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
        }
    }

    private void Clear()
    {
        foreach (Transform child in m_RampsGroup.transform)
        {
            Destroy(child.gameObject);
        }

        _Ramps.Clear();
    }
}
