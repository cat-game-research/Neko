using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class PrefabSpawner : MonoBehaviour
    {
        [Header("Automatic Start")]
        [SerializeField] bool m_RunOnAwake = true;

        [Header("Agent Prefab to Initialize")]
        public GameObject m_ParentGroup;
        public GameObject[] m_BasePrefab;

        [Header("Spawning Configuration")]
        [SerializeField] public int m_XCount = 10;
        [SerializeField] public int m_ZCount = 10;
        [SerializeField] public float m_WidthX = 20f;
        [SerializeField] public float m_WidthZ = 20f;
        [SerializeField] public float m_OffsetX = 0f;
        [SerializeField] public float m_OffsetZ = 0f;

        void Awake()
        {
            if (m_ParentGroup == null)
            {
                m_ParentGroup = gameObject;
            }

            if (!m_RunOnAwake)
            {
                return;
            }

            SpawnAll();
        }

        private void SpawnAll()
        {
            for (int k = 0; k < m_BasePrefab.Length; k++)
            {
                for (int i = 0; i < m_XCount; i++)
                {
                    for (int j = 0; j < m_ZCount; j++)
                    {
                        InstantiatePrefab(m_BasePrefab[k],
                                          i * m_WidthX + m_OffsetX, 0, j * m_WidthZ + m_OffsetZ,
                                          m_BasePrefab[k].transform.rotation,
                                          m_ParentGroup.transform);
                    }
                }
            }
        }

        public List<string> ListPrefabs()
        {
            List<string> prefabNames = new List<string>();
            foreach (var prefab in m_BasePrefab)
            {
                if (prefab != null)
                {
                    prefabNames.Add(prefab.name);
                }
            }
            return prefabNames;
        }

        public GameObject GetBasePrefabByName(string name)
        {
            return null;
        }


        public void SpawnSingle()
        {
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WalkerAgentTrainer.prefab", typeof(GameObject));
            if (prefab != null)
            {
                InstantiatePrefab(prefab);
            }
            else
            {
                Debug.LogError($"Prefab '{name}' could not be found.");
            }
        }

        public GameObject InstantiatePrefab(GameObject prefab)
        {
            return InstantiatePrefab(prefab, 0, 0, 0, Quaternion.identity, m_ParentGroup.transform);
        }

        public GameObject InstantiatePrefab(GameObject prefab, float posX, float posY, float posZ, Quaternion rotation, Transform parent)
        {
            return Instantiate(prefab, new Vector3(posX, posY, posZ), rotation, parent);
        }
    }
}
