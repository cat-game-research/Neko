using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class MotionAgent : Agent
    {
        [Header("Debug")]
        [Tooltip("When this setting is true then we will use the rigidbodies and capsule collider attached to the BodyCapsule.")]
        public bool m_UseBodyCapsule = true;

        [Header("Refs")]
        public ObjectContactTrigger m_AwarenessContact;
        public GameObject m_Awareness;
        public GameObject m_Vision;
        public GameObject m_OrientCube;
        public GameObject m_BodyCapsule;

        [Header("Positioning")]
        public float m_AwarenessOffsetY = 1f;
        public float m_VisionOffsetY = 1f;
        public float m_OrientCubeOffsetY = 1f;

        [Header("Training Settings")]
        [Tooltip("Used to add variation to the training environment")]
        public bool m_RandomStartingRotation = true;
        [Tooltip("Typically set to true for training. Leaving false will make the agent retain memory between episodes.")]
        public bool m_ResetOnEpisodeBegin = true;
        [Tooltip("The minimum amount of cumulative reward required to continue the episode.")]
        public float m_RewardThreshold = -1000f;
        public float m_MinVelocity = -2.3f;
        public float m_MaxVelocity = 4.6f;

        Rigidbody _AgentRb;
        float _RunSpeed = 1.5f;
        int _ActionCount = 0;
        float _Velocity = 0f;
        float _RotateScale = 150f;
        Vector3 _Direction = Vector3.zero;
        Vector3 _RotateDirection = Vector3.zero;
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
                m_BodyCapsule.SetActive(false);
            }
            else
            {
                m_BodyCapsule.SetActive(true);
            }
        }

        private void ResetPositions()
        {
            var _position = new Vector3(m_Awareness.transform.position.x, m_AwarenessOffsetY, m_Awareness.transform.position.z);
            m_Awareness.transform.SetLocalPositionAndRotation(_position, Quaternion.identity);

            _position = new Vector3(m_Vision.transform.position.x, m_VisionOffsetY, m_Vision.transform.position.z);
            m_Vision.transform.SetLocalPositionAndRotation(_position, Quaternion.identity);

            _position = new Vector3(m_OrientCube.transform.position.x, m_OrientCubeOffsetY, m_OrientCube.transform.position.z);
            m_OrientCube.transform.SetLocalPositionAndRotation(_position, Quaternion.identity);

            _AgentRb.velocity = Vector3.zero;
            _AgentRb.angularVelocity = Vector3.zero;
            _AgentRb.position = _StartingPosition;
            _AgentRb.rotation = _StartingRotation;

            var _rotation = m_RandomStartingRotation ? Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0) : _StartingRotation;

            transform.SetLocalPositionAndRotation(_StartingPosition, _rotation);
        }

        public override void CollectObservations(VectorSensor sensor)
        {

            sensor.AddObservation(StepCount / (float)MaxStep);
            sensor.AddObservation(_ActionCount);
            sensor.AddObservation(_RunSpeed);
            sensor.AddObservation(_Velocity);
            sensor.AddObservation(_Direction);
            sensor.AddObservation(transform.localRotation);
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(transform.InverseTransformDirection(_Direction));
            sensor.AddObservation(Vector3.Distance(transform.localPosition, Vector3.zero));
            sensor.AddObservation(Quaternion.FromToRotation(transform.forward, _Direction));
            sensor.AddObservation(Quaternion.FromToRotation(transform.forward, _RotateDirection));
            sensor.AddObservation(m_Awareness.transform.InverseTransformDirection(_Direction));
            sensor.AddObservation(m_Vision.transform.InverseTransformDirection(_Direction));
            sensor.AddObservation(m_OrientCube.transform.InverseTransformDirection(_Direction));
            sensor.AddObservation(m_AwarenessContact.touchingGround);
            sensor.AddObservation(m_AwarenessContact.touchingWall);
            sensor.AddObservation(m_AwarenessContact.touchingTarget);
        }

        private void FixedUpdate()
        {
            transform.Rotate(Vector3.up, _RotateDirection.y * _RotateScale * Time.deltaTime);

            if (_AgentRb.velocity.magnitude <= m_MaxVelocity && _AgentRb.velocity.magnitude >= m_MinVelocity)
            {
                _AgentRb.MovePosition(transform.position + _Direction * _Velocity);
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            Debug.Log(GetCumulativeReward());
            if (GetCumulativeReward() < m_RewardThreshold)
            {
                EndEpisode();
            }

            ++_ActionCount;
            AddReward(-1f / MaxStep);

            var continuousActions = actionBuffers.ContinuousActions;

            var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
            var sideways = Mathf.Clamp(continuousActions[1], -1f, 1f);
            var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);

            _Velocity = Mathf.Clamp(continuousActions[3], -1f, 1f);
            _Direction = transform.TransformDirection(new Vector3(sideways, 0f, forward));
            _RotateDirection = new Vector3(0f, rotate, 0f);
        }

        public override void OnEpisodeBegin()
        {
            if (!m_ResetOnEpisodeBegin)
            {
                return;
            }

            _ActionCount = 0;
            ResetPositions();
        }
    }
}