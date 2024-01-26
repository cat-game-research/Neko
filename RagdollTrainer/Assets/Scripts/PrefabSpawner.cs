using UnityEngine;

namespace Unity.MLAgentsExamples
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
        [SerializeField] float widthX = 20f;
        [SerializeField] float widthZ = 20f;
        [SerializeField] float offsetX = 0f;
        [SerializeField] float offsetZ = 0f;

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
                        Instantiate(basePrefab[k], new Vector3(i * widthX + offsetX, 0, j * widthZ + offsetZ),
                            basePrefab[k].transform.rotation);
                    }
                }
            }
        }
    }
}
