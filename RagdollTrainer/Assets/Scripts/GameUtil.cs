using System.Collections.Generic;
using UnityEngine;

public static class GameUtil
{
    public static bool IsTooClose(Vector2Int position, float minDistance, List<GameObject> columns)
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

    public static Vector2Int GetRandomPosition(float radius)
    {
        return Vector2Int.RoundToInt(Random.insideUnitCircle * radius);
    }
}
