using SpaceGraphicsToolkit;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class MotionAgent : Agent
    {
        [Header("Refs")]
        public ObjectContactTrigger m_AwarenessContact;
        public GameObject m_Awareness;
        public GameObject m_OrientCube;
        public GameObject m_BodyCapsule;
        public Transform m_Target;

        [Header("Positioning")]
        public float m_AwarenessOffsetY = 1.575f;
        public float m_OrientCubeOffsetY = 1f;

        [Header("Training Settings")]
        public bool m_EarlyTraining = true;
        [Tooltip("Used to add variation to the training environment")]
        public bool m_RandomStartingRotation = true;
        [Tooltip("Typically set to true for training. Leaving false will make the agent retain memory between episodes.")]
        public bool m_ResetOnEpisodeBegin = true;
        [Tooltip("Should we penalize the agent for walking backwards, true then bring the pain.")]
        public bool m_ReversePenalty = true;
        [Tooltip("The minimum amount of cumulative reward required to continue the episode.")]
        public float m_RewardThreshold = -1000f;
        public float m_MinVelocity = -2.3f;
        public float m_MaxVelocity = 4.6f;

        [Header("Movement Settings")]
        public float m_StrafeScale = 0.2f;
        public float m_VelocityScale = 5f;
        public float m_RotateScale = 150f;

        [Header("Debug")]
        [Tooltip("When this setting is true then we will use the rigidbodies and capsule collider attached to the BodyCapsule.")]
        public bool m_UseBodyCapsule = true;

        Rigidbody _AgentRb;

        float _RunSpeed = 1.5f;
        int _ActionCount = 0;
        float _Forward = 0f;
        float _Rotate = 0f;
        float _Velocity = 0f;
        float _Strafe = 0f;

        Vector3 _Position = Vector3.zero;
        Vector3 _Direction = Vector3.zero;
        Vector3 _StartingPosition = Vector3.zero;
        Quaternion _StartingRotation = Quaternion.identity;

        public override void Initialize()
        {
            _AgentRb = m_BodyCapsule.GetComponent<Rigidbody>();
            _StartingPosition = transform.position;
            _StartingRotation = transform.rotation;
            _ActionCount = 0;

            if (m_UseBodyCapsule)
            {
                m_BodyCapsule.SetActive(true);
            }
            else
            {
                m_BodyCapsule.SetActive(false);
            }
        }

        public override void OnEpisodeBegin()
        {
            if (!m_ResetOnEpisodeBegin)
            {
                return;
            }

            ResetEpisode();
        }

        private void ResetEpisode()
        {
            _Position = new Vector3(m_Awareness.transform.position.x, m_AwarenessOffsetY, m_Awareness.transform.position.z);
            m_Awareness.transform.SetPositionAndRotation(_Position, Quaternion.identity);

            _Position = new Vector3(m_OrientCube.transform.position.x, m_OrientCubeOffsetY, m_OrientCube.transform.position.z);
            m_OrientCube.transform.SetPositionAndRotation(_Position, Quaternion.identity);

            _StartingRotation = m_RandomStartingRotation ? Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0) : _StartingRotation;
            transform.SetPositionAndRotation(_StartingPosition, _StartingRotation);

            _AgentRb.velocity = Vector3.zero;
            _AgentRb.position = _StartingPosition;
            _AgentRb.rotation = _StartingRotation;

            _ActionCount = 0;
            _Velocity = 0f;
            _Forward = 0f;
            _Rotate = 0f;
            _Strafe = 0f;
        }

        public override void CollectObservations(VectorSensor sensor)
        {

            sensor.AddObservation(StepCount / (float)MaxStep);
            sensor.AddObservation(_ActionCount);
            sensor.AddObservation(_RunSpeed);
            sensor.AddObservation(_Rotate);
            sensor.AddObservation(_Strafe);
            sensor.AddObservation(_Forward);
            sensor.AddObservation(_Velocity);
            sensor.AddObservation(_Direction);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(transform.InverseTransformDirection(_Direction));
            sensor.AddObservation(Vector3.Distance(transform.localPosition, Vector3.zero));
            sensor.AddObservation(Quaternion.FromToRotation(transform.forward, _Direction));
            sensor.AddObservation(m_Awareness.transform.InverseTransformDirection(_Direction));
            sensor.AddObservation(m_OrientCube.transform.InverseTransformDirection(_Direction));
            sensor.AddObservation(m_AwarenessContact.touchingGround);
            sensor.AddObservation(m_AwarenessContact.touchingWall);
            sensor.AddObservation(m_AwarenessContact.touchingTarget);
        }

        private void FixedUpdate()
        {
            transform.Translate(_Direction * _Velocity * m_VelocityScale * Time.deltaTime, Space.World);
            transform.Rotate(0f, _Rotate * m_RotateScale * Time.deltaTime, 0f);

            if (m_EarlyTraining && m_Target != null)
            {
                var targetDistanceReward = 1f / (Vector3.Distance(transform.position, m_Target.position) + 0.01f);
                var targetFacingReward = Mathf.InverseLerp(0, 180, Quaternion.Angle(transform.rotation, m_Target.rotation));

                Debug.Log(0.9f * targetDistanceReward + " :: " + 0.8f * targetFacingReward);
                AddReward(0.9f * targetDistanceReward + 0.8f * targetFacingReward);
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (GetCumulativeReward() < m_RewardThreshold)
            {
                EndEpisode();
            }

            ++_ActionCount;
            AddReward(-1f / MaxStep);

            if (m_ReversePenalty && _Velocity < 0)
            {
                AddReward(-1f * _Velocity / MaxStep);
            }

            var continuousActions = actionBuffers.ContinuousActions;
            _Forward = continuousActions[0];
            _Strafe = continuousActions[1];
            _Rotate = continuousActions[2];
            _Velocity = continuousActions[3];

            _Direction = transform.TransformDirection(_Strafe * Vector3.right + _Forward * Vector3.forward);
        }
    }
}