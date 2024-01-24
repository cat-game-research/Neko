# ColumnSpawner

This class is responsible for spawning a certain number of columns at random positions within a specified radius on the ground, avoiding overlaps with other objects in the environment layer.

## Properties

- `m_ColumnsGroup`: A GameObject field for grouping columns.
- `m_WallColumnPrefab`: A GameObject field for the prefab used for spawning columns.
- `m_NumberOfColumns`: An int field for the number of columns to spawn.
- `m_GroundRadius`: A float field for the radius within which columns can be spawned on the ground.
- `m_EnvironmentLayer`: A LayerMask field for the layer where environmental objects are located that should not overlap with spawned columns.
- `m_OverwriteColumns`: A bool field that determines whether the existing columns will be overwritten when new ones are spawned. It has a tooltip attribute that provides a description of what the option does.
- `m_MinDistance`: A float field that sets the minimum distance between two columns. It has a tooltip attribute that provides a description of what the option does.

## Methods

- `RandomizeColumns(int minAmount, int maxAmount)`: This method spawns a random number of columns between the minAmount and maxAmount parameters. It clears the existing columns if the m_OverwriteColumns option is true. It generates a random position for each column within the m_GroundRadius, and checks the distance between the current column and the previous columns using the IsTooClose function. If the distance is less than the m_MinDistance value, it generates a new random position. It also uses a raycast to adjust the column's position to the ground level. It sets the local position and rotation of the column using the SetLocalPositionAndRotation method.
- `ClearColumns()`: This method clears the existing columns by destroying and removing them from the _Columns list. It checks the value of the m_OverwriteColumns option before clearing the columns.
- `GetRandomPosition(float radius)`: This method returns a random Vector2Int value within the specified radius. It uses the Random.insideUnitCircle method to generate a random Vector2 value, and then rounds it to the nearest integer using the Vector2Int.RoundToInt method. This makes the columns snap to int positions on the x and z axes.
- `Start()`: This method is called when the script is initialized. It checks that the m_ColumnsGroup and the m_WallColumnPrefab fields are not null, and throws an exception if they are. It then calls the RandomizeColumns method with the m_NumberOfColumns value as both parameters.
- `IsTooClose(Vector2Int position, List<GameObject> columns, float minDistance)`: This method checks the distance between the given position and the previous columns in the list. It returns true if the distance is less than the minDistance value, and false otherwise. It uses the Vector2Int.Distance method to calculate the distance between two Vector2Int values.
