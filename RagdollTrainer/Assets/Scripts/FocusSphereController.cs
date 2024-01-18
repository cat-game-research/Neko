using UnityEngine;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// Utility class to allow a stable observation platform.
    /// </summary>
    public class FocusSphereController : MonoBehaviour
    {
        public Vector3 UpdatePosition(Vector3 position)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);
            return transform.position;
        }
    }
}
