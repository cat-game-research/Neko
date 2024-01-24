using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Serialization;

namespace Unity.MLAgentsExamples
{

    /// <summary>
    /// Utility class to allow a stable observation platform.
    /// </summary>
    public class FocusSphereController : MonoBehaviour
    {

        [Header("Tags to Focus")]
        [Tooltip("The layer mask for the focus sphere to collide with")]
        public LayerMask m_FocusSphereLayerMask;

        [Header("Layers of Focus")]
        [SerializeField, FormerlySerializedAs("detectableTags")]
        [Tooltip("List of tags in the scene to compare against.")]
        List<string> _DetectableTags;

        [HideInInspector] public List<bool> m_TagMemory;

        public List<string> DetectableTags
        {
            get { return _DetectableTags; }
            set { _DetectableTags = value; }
        }

        void Start()
        {
            ResetTagMemory();
            for (int i = 0; i < m_TagMemory.Count; i++)
            {
                m_TagMemory[i] = false;
            }
        }

        public void ResetTagMemory()
        {
            m_TagMemory = new List<bool>(DetectableTags.Count);
        }

        public Vector3 UpdatePosition(Vector3 position)
        {
            Vector3 noise = Random.insideUnitSphere;
            noise *= 0.1f;
            position += noise;
            transform.SetPositionAndRotation(position, Quaternion.identity);
            return transform.position;
        }

        void OnTriggerEnter(Collider other)
        {
            if (m_TagMemory.Count > 0 && DetectableTags.Contains(other.tag))
            {
                int index = DetectableTags.IndexOf(other.tag);
                m_TagMemory[index] = true;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (m_TagMemory.Count > 0 && DetectableTags.Contains(other.tag))
            {
                int index = DetectableTags.IndexOf(other.tag);
                m_TagMemory[index] = false;
            }
        }

    }
}
