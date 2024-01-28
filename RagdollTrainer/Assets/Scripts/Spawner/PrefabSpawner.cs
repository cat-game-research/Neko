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
        [SerializeField] int m_XCount = 10;
        [SerializeField] int m_ZCount = 10;
        [SerializeField] float m_WidthX = 20f;
        [SerializeField] float m_WidthZ = 20f;
        [SerializeField] float m_OffsetX = 0f;
        [SerializeField] float m_OffsetZ = 0f;

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

            SpawnAllBasePrefabs();
        }

        private void SpawnAllBasePrefabs()
        {
            for (int k = 0; k < m_BasePrefab.Length; k++)
            {
                for (int i = 0; i < m_XCount; i++)
                {
                    for (int j = 0; j < m_ZCount; j++)
                    {
                        Instantiate(m_BasePrefab[k],
                                    new Vector3(i * m_WidthX + m_OffsetX, 0, j * m_WidthZ + m_OffsetZ),
                                    m_BasePrefab[k].transform.rotation,
                                    m_ParentGroup.transform);
                    }
                }
            }
        }
    }
}
