using UnityEngine;

namespace JKress.AITrainer
{
    public class PrefabSpawner : MonoBehaviour
    {
        [Header("Automatic Start")]
        [SerializeField] bool _RunOnAwake = true;

        [Header("Agent Prefab to Initialize")]
        [SerializeField] GameObject[] basePrefab;

        [Header("Spawning Configuration")]
        [SerializeField] int xCount = 10;
        [SerializeField] int zCount = 10;
        [SerializeField] float offsetX = 20f;
        [SerializeField] float offsetZ = 20f;

        GameObject scenePrefab;

        void Awake()
        {
            if (!_RunOnAwake) return;

            for (int k = 0; k < basePrefab.Length; k++)
            {
                //Spawn prefabs along x and z from basePrefab 
                for (int i = 0; i < xCount; i++)
                {
                    for (int j = 0; j < zCount; j++)
                    {
                        Instantiate(basePrefab[k], new Vector3(i * offsetX, 0, j * offsetZ),
                            Quaternion.identity);
                    }
                }
            }
        }
    }
}
