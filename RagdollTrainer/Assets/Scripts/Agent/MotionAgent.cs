using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Sentis.Layers;
using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class MotionAgent : Agent
    {
        [Header("Refs")]
        public GameObject m_Awareness;
        public GameObject m_Vision;
        public GameObject m_OrientCube;
        public GameObject m_BodyCapsule;

        [Header("Positioning")]
        public float m_AwarenessOffsetY = 1f;
        public float m_VisionOffsetY = 1f;
        public float m_OrientCubeOffsetY = 1f;

        [Header("Training Settings")]
        [Tooltip("The minimum amount of cumulative reward required to continue the episode.")]
        public float m_RewardThreshold = -1000f;

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (GetCumulativeReward() < m_RewardThreshold)
            {
                EndEpisode();
            }
        }
    }
}