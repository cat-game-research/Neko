using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Sentis.Layers;
using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class MotionPerceptionAgent : Agent
    {
        [Header("Root Agent")]
        [SerializeField] WalkerAgent m_WalkerAgent;

        [Header("Body Parts")]
        [SerializeField] Transform m_Hips;
        [SerializeField] Transform m_Head;
        [SerializeField] Transform m_EyeLeft;
        [SerializeField] Transform m_EyeRight;

        [Header("Focus Sphere Controller")]
        FocusSphereController m_FocusSphere;

        [Header("Focus Sphere Transform")]
        [SerializeField] Transform m_Focus;

        [Header("Focus Sphere Range")]
        [Range(1f, 100f)] public float m_MaxDistance = 20f;

        [Header("Focus Position")]
        Vector3 m_FocusPosition = Vector3.zero;
        [Range(0.1f, 10f)][SerializeField] float m_PositionScale = 1f;

        [Header("Acceleration")]
        [Range(0.1f, 10f)][SerializeField] float m_AccelerationScale = 1f;
        [Range(0.1f, 4f)] public float m_TargetWalkingSpeed = 2f;

        [Header("The Reward Tag for A Treat")]
        public string m_RewardTag = "target";

        [Header("Floor Is Lava")]
        public string m_PunishTag = "ground";

        float _tagMemoryReward = 0.0f;
        float _Variance = 0.0f;
        float[] _ContinuousActions;
        float _MeanAction = 0.0f;
        float _tagMemoryPunish = -1.0f;

        public override void Initialize()
        {
            m_FocusSphere = GetComponentInChildren<FocusSphereController>();
            transform.SetPositionAndRotation(m_Head.position, m_Hips.rotation);
            m_FocusPosition = m_FocusSphere.UpdatePosition(transform.position);
            m_WalkerAgent.UpdateTargetWalkingSpeed(m_TargetWalkingSpeed);
            m_FocusSphere.ResetTagMemory();
        }

        public override void OnEpisodeBegin()
        {
            transform.SetPositionAndRotation(m_Head.position, m_Hips.rotation);
            m_FocusPosition = m_FocusSphere.UpdatePosition(m_Head.position);
            m_WalkerAgent.UpdateTargetWalkingSpeed(m_TargetWalkingSpeed);
            m_FocusSphere.ResetTagMemory();
        }

        void FixedUpdate()
        {
            transform.position = m_Head.position;
            transform.rotation = m_Hips.rotation;

            _tagMemoryReward = 0f;
            foreach (bool value in m_FocusSphere.m_TagMemory)
            {
                if (value)
                {
                    _tagMemoryReward += 0.1f;
                }
            }
            if (m_FocusSphere.m_TagMemory[m_FocusSphere.DetectableTags.IndexOf(m_RewardTag)])
            {
                _tagMemoryReward += 1f;
            }
            else if (m_FocusSphere.m_TagMemory[m_FocusSphere.DetectableTags.IndexOf(m_PunishTag)])
            {
                _tagMemoryReward -= 1f;
            }

            _Variance = 0f;
            _MeanAction = 0f;
            foreach (float action in _ContinuousActions)
            {
                _MeanAction += action;
            }
            _MeanAction /= _ContinuousActions.Length;
            foreach (float action in _ContinuousActions)
            {
                _Variance += Mathf.Pow(action - _MeanAction, 2f);
            }
            _Variance /= _ContinuousActions.Length;
            _Variance *= -1f;

            _tagMemoryPunish = 0f;
            if (m_FocusSphere.m_TagMemory.All(x => x == false))
            {
                _tagMemoryPunish = -1f;
            }

            AddReward(0.6f * _tagMemoryReward + 0.2f * _Variance + 0.2f * _tagMemoryPunish);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            m_WalkerAgent.CollectObservations(sensor);

            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(m_MaxDistance);
            sensor.AddObservation(m_FocusPosition);
            sensor.AddObservation(m_Focus.localPosition);
            sensor.AddObservation(m_TargetWalkingSpeed);
            sensor.AddObservation(Vector3.Distance(m_WalkerAgent.m_AvgPosition, m_FocusPosition));
            sensor.AddObservation(Vector3.Distance(m_Focus.position, m_FocusPosition));
            sensor.AddObservation(m_TargetWalkingSpeed - m_WalkerAgent.m_AvgVelocity.magnitude);
            sensor.AddObservation(transform.InverseTransformDirection(m_FocusPosition));
            sensor.AddObservation(transform.InverseTransformDirection(m_WalkerAgent.m_AvgPosition));

            foreach (bool value in m_FocusSphere.m_TagMemory)
            {
                sensor.AddObservation(value);
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            _ContinuousActions = actionBuffers.ContinuousActions.Array;
            var continuousActions = actionBuffers.ContinuousActions;
            var i = -1;

            Vector3 position = new Vector3(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
            position *= m_PositionScale;
            m_FocusPosition += position;
            m_FocusPosition = Vector3.ClampMagnitude(m_FocusPosition, m_MaxDistance);
            m_FocusPosition = m_FocusSphere.UpdatePosition(m_FocusPosition);
            m_TargetWalkingSpeed = m_WalkerAgent.TargetWalkingSpeed + continuousActions[++i] * m_AccelerationScale;
            m_WalkerAgent.UpdateTargetWalkingSpeed(m_TargetWalkingSpeed);
        }
    }
}