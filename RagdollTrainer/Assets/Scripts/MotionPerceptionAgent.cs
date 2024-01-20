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

        [Header("Body Part Transforms")]
        [SerializeField] Transform m_HipsT;
        [SerializeField] Transform m_HeadEndT;
        [SerializeField] Transform m_EyeLeftT;
        [SerializeField] Transform m_EyeRightT;

        [Header("Focus Sphere")]
        [SerializeField] FocusSphereController m_FocusSphere;
        [SerializeField] Transform m_FocusSphereT;

        [Header("Focus Sphere Range")]
        [Range(1f, 100f)] public float m_MaxDistance = 20f;

        [Header("Focus Position")]
        Vector3 m_FocusPosition = Vector3.zero;
        [Range(0.1f, 10f)][SerializeField] float m_PositionScale = 1f;

        [Header("Acceleration")]
        [Range(0.1f, 10f)][SerializeField] float m_AccelerationScale = 1f;
        [Range(0.1f, 4f)] float m_TargetWalkingSpeed = 2f;

        float m_minWalkingSpeed = 0.1f;
        float m_maxWalkingSpeed = 4f;

        public float TargetWalkingSpeed // property
        {
            get { return m_TargetWalkingSpeed; }
            set { m_TargetWalkingSpeed = Mathf.Clamp(value, m_minWalkingSpeed, m_maxWalkingSpeed); }
        }

        [Header("Tag for Positive Reward")]
        public string m_RewardTag = "target";
        public float m_RewardTagAmount = 1f;
        public float m_MinRewardTagAmount = 0.1f;

        float _tagMemoryReward = 0.0f;
        float _VarianceReward = 0.0f;
        float[] _ContinuousActions;
        float _MeanAction = 0.0f;

        public override void Initialize()
        {
            transform.SetPositionAndRotation(m_HeadEndT.position, m_HipsT.rotation);
            m_FocusPosition = m_FocusSphere.UpdatePosition(transform.position);
            m_FocusSphere.ResetTagMemory();
        }

        public override void OnEpisodeBegin()
        {
            transform.SetPositionAndRotation(m_HeadEndT.position, m_HipsT.rotation);
            m_FocusPosition = m_FocusSphere.UpdatePosition(transform.position);
            m_FocusSphere.ResetTagMemory();
        }

        void FixedUpdate()
        {
            transform.position = m_HeadEndT.position;
            transform.rotation = m_HipsT.rotation;

            _tagMemoryReward = 0f;
            foreach (bool value in m_FocusSphere.m_TagMemory)
            {
                if (value)
                {
                    _tagMemoryReward += m_MinRewardTagAmount;
                }
            }
            if (m_FocusSphere.m_TagMemory.Count > 0 &&
                m_FocusSphere.m_TagMemory[m_FocusSphere.DetectableTags.IndexOf(m_RewardTag)])
            {
                _tagMemoryReward += m_RewardTagAmount;
            }

            _VarianceReward = 0f;
            if (_ContinuousActions != null)
            {
                _MeanAction = 0f;
                foreach (float action in _ContinuousActions)
                {
                    _MeanAction += action;
                }
                _MeanAction /= _ContinuousActions.Length;
                foreach (float action in _ContinuousActions)
                {
                    _VarianceReward += Mathf.Pow(action - _MeanAction, 2f);
                }
                _VarianceReward /= _ContinuousActions.Length;
                _VarianceReward *= -1f;
            }
            Debug.Log("$REWARD: tag: " + _tagMemoryReward + " var: " + _VarianceReward + " | spd: " + TargetWalkingSpeed + " | fcs: " + m_FocusPosition);

            AddReward(0.8f * _tagMemoryReward + 0.2f * _VarianceReward);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            m_WalkerAgent.CollectObservations(sensor);

            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(m_MaxDistance);
            sensor.AddObservation(m_FocusPosition);
            sensor.AddObservation(m_FocusSphereT.localPosition);
            sensor.AddObservation(TargetWalkingSpeed);
            sensor.AddObservation(Vector3.Distance(m_WalkerAgent.m_AvgPosition, m_FocusPosition));
            sensor.AddObservation(Vector3.Distance(m_FocusSphereT.position, m_FocusPosition));
            sensor.AddObservation(TargetWalkingSpeed - m_WalkerAgent.m_AvgVelocity.magnitude);
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
            position *= Time.deltaTime * m_PositionScale;
            m_FocusPosition += position;
            m_FocusPosition = Vector3.ClampMagnitude(m_FocusPosition, m_MaxDistance);
            m_FocusPosition = m_FocusSphere.UpdatePosition(m_FocusPosition);
            TargetWalkingSpeed = m_WalkerAgent.TargetWalkingSpeed + continuousActions[++i] * m_AccelerationScale;
        }
    }
}