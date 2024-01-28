
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

namespace Unity.MLAgentsExamples
{
    public class WalkerAgent : Agent
    {
        [Header("Training Type")]
        [Tooltip("Typically set to true for training. Leaving false will make the agent retain memory between episodes.")]
        public bool m_ResetOnEpisodeBegin = false;
        [Tooltip("When starting new models this should be set to true. Penalizes the agent for moving their torso backwards.")]
        public bool earlyTraining = false;
        [Tooltip("The minimum amount of cumulative reward required to continue the episode.")]
        public float m_RewardThreshold = -1000f;

        [Header("Target Goal")]
        [SerializeField] Transform targetT;
        [SerializeField] TargetController targetController;

        [Header("Body Parts")]
        [SerializeField] Transform hips;
        [SerializeField] Transform spine;
        [SerializeField] Transform head;
        [SerializeField] Transform thighL;
        [SerializeField] Transform shinL;
        [SerializeField] Transform footL;
        [SerializeField] Transform thighR;
        [SerializeField] Transform shinR;
        [SerializeField] Transform footR;
        [SerializeField] Transform armL;
        [SerializeField] Transform forearmL;
        [SerializeField] Transform armR;
        [SerializeField] Transform forearmR;

        [Header("Stabilizer")]
        [Range(1000, 4000)][SerializeField] float m_stabilizerTorque = 4000f;
        float m_minStabilizerTorque = 1000;
        float m_maxStabilizerTorque = 4000;
        [SerializeField] Stabilizer hipsStabilizer;
        [SerializeField] Stabilizer spineStabilizer;

        [Header("Walk Speed")]
        [Range(0.1f, 4)][SerializeField] float m_TargetWalkingSpeed = 2;
        float m_minWalkingSpeed = 0.1f;
        float m_maxWalkingSpeed = 4;

        [HideInInspector] public Vector3 m_AvgVelocity = Vector3.zero;
        [HideInInspector] public Vector3 m_AvgPosition = Vector3.zero;

        [Header("Environment Column Spawner")]
        public ColumnSpawner m_ColumnSpawner;

        public static int AGENT_ID = 0;

        public float TargetWalkingSpeed
        {
            get { return m_TargetWalkingSpeed; }
            set { m_TargetWalkingSpeed = Mathf.Clamp(value, m_minWalkingSpeed, m_maxWalkingSpeed); }
        }

        public float MStabilizerTorque
        {
            get { return m_stabilizerTorque; }
            set { m_stabilizerTorque = Mathf.Clamp(value, m_minStabilizerTorque, m_maxStabilizerTorque); }
        }

        [Tooltip("If true, walkSpeed will be randomly set between zero and m_maxWalkingSpeed in OnEpisodeBegin(). If false, the goal velocity will be walkingSpeed. Will be overwritten if external direction and speed agent is used.")]
        public bool randomizeWalkSpeed;

        [Tooltip("This will be used as a stabilized model space reference point for observations. Because ragdolls can move erratically during training, using a stabilized reference transform improves learning.")]
        [HideInInspector] public OrientationCubeController m_OrientationCube;

        [Tooltip("The indicator graphic gameobject that points towards the target.")]
        [HideInInspector] public JointDriveController m_JdController;

        public override void Initialize()
        {
            ++AGENT_ID;
            m_OrientationCube = GetComponentInChildren<OrientationCubeController>();
            m_JdController = GetComponent<JointDriveController>();

            m_JdController.SetupBodyPart(hips);
            m_JdController.SetupBodyPart(spine);
            m_JdController.SetupBodyPart(head);
            m_JdController.SetupBodyPart(thighL);
            m_JdController.SetupBodyPart(shinL);
            m_JdController.SetupBodyPart(footL);
            m_JdController.SetupBodyPart(thighR);
            m_JdController.SetupBodyPart(shinR);
            m_JdController.SetupBodyPart(footR);
            m_JdController.SetupBodyPart(armL);
            m_JdController.SetupBodyPart(forearmL);
            m_JdController.SetupBodyPart(armR);
            m_JdController.SetupBodyPart(forearmR);

            hipsStabilizer.uprightTorque = m_stabilizerTorque;
            spineStabilizer.uprightTorque = m_stabilizerTorque;

            if (targetT == null)
            {
                targetT = GetRandomTransform(-1000, 1000, -1000, 1000, 1);
            }

            if (randomizeWalkSpeed)
            {
                TargetWalkingSpeed = Random.Range(m_minWalkingSpeed, m_maxWalkingSpeed);
            }
        }

        public Transform GetRandomTransform(float minX, float maxX, float minY, float maxY, float heightY)
        {
            var waypoint = new GameObject("TargetWaypoint");
            var position = new Vector3(Random.Range(minX, maxX), heightY, Random.Range(minY, maxY));
            waypoint.transform.SetPositionAndRotation(position, Quaternion.identity);
            waypoint.transform.SetParent(transform);

            return waypoint.transform;
        }

        public override void OnEpisodeBegin()
        {
            if (!m_ResetOnEpisodeBegin)
            {
                return;
            }

            targetController.MoveTargetToRandomPosition();

            foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
            {
                bodyPart.Reset(bodyPart);
            }

            //TODO Make random hip starting rotation an option in the editor inspector
            hips.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);

            //TODO Motion agent needs to define starting conditions for Orientation Cube
            m_OrientationCube.UpdateOrientation(hips, targetT);

            //TODO This needs to use the Event system to talk to column spawner -- no direct reference.
            m_ColumnSpawner?.RandomizeColumns();

            TargetWalkingSpeed = randomizeWalkSpeed ? Random.Range(m_minWalkingSpeed, m_maxWalkingSpeed) : TargetWalkingSpeed;
        }

        void FixedUpdate()
        {
            //TODO Motion agent needs to define targetT rather then Walker agent
            m_OrientationCube.UpdateOrientation(hips, targetT);

            var footSpacingReward = Vector3.Dot(footR.position - footL.position, footL.right);
            if (footSpacingReward > 0.1f) footSpacingReward = 0.1f;
            AddReward(footSpacingReward);

            var cubeForward = m_OrientationCube.transform.forward;
            var lookAtTargetReward = Vector3.Dot(head.forward, cubeForward) + 1;
            var matchSpeedReward = GetMatchingVelocityReward(cubeForward * TargetWalkingSpeed, GetAvgVelocity());

            //*Important* Forces movement towards target (penalize stationary swinging)
            if (earlyTraining)
            {
                matchSpeedReward = Vector3.Dot(m_AvgVelocity, cubeForward);
                if (matchSpeedReward > 0) matchSpeedReward = GetMatchingVelocityReward(cubeForward * TargetWalkingSpeed, m_AvgVelocity);
            }

            AddReward(matchSpeedReward + 0.1f * lookAtTargetReward);
        }

        public void CollectObservationBodyPart(BodyPart bp, VectorSensor sensor)
        {
            sensor.AddObservation(bp.objectContact.touchingGround);
            sensor.AddObservation(bp.objectContact.touchingWall);
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.velocity));
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(bp.rb.position - hips.position));
            sensor.AddObservation(bp.rb.transform.localRotation);

            if (bp.rb.transform != hips)
            {
                sensor.AddObservation(bp.currentStrength / m_JdController.maxJointForceLimit);
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            var cubeForward = m_OrientationCube.transform.forward;
            var velGoal = cubeForward * TargetWalkingSpeed;
            var avgVel = GetAvgVelocity();

            sensor.AddObservation(Vector3.Distance(velGoal, avgVel));
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(avgVel));
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(velGoal));
            sensor.AddObservation(Quaternion.FromToRotation(hips.forward, cubeForward));
            sensor.AddObservation(Quaternion.FromToRotation(head.forward, cubeForward));
            sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(targetT.position));

            foreach (var bodyPart in m_JdController.bodyPartsList)
            {
                CollectObservationBodyPart(bodyPart, sensor);
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var bpDict = m_JdController.bodyPartsDict;
            var continuousActions = actionBuffers.ContinuousActions;
            var i = -1;

            bpDict[spine].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
            bpDict[thighL].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[thighR].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[shinL].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[shinR].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[footR].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
            bpDict[footL].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
            bpDict[armL].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[armR].SetJointTargetRotation(continuousActions[++i], 0, continuousActions[++i]);
            bpDict[forearmL].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[forearmR].SetJointTargetRotation(continuousActions[++i], 0, 0);
            bpDict[head].SetJointTargetRotation(continuousActions[++i], continuousActions[++i], 0);

            //update joint strength settings
            bpDict[spine].SetJointStrength(continuousActions[++i]);
            bpDict[head].SetJointStrength(continuousActions[++i]);
            bpDict[thighL].SetJointStrength(continuousActions[++i]);
            bpDict[shinL].SetJointStrength(continuousActions[++i]);
            bpDict[footL].SetJointStrength(continuousActions[++i]);
            bpDict[thighR].SetJointStrength(continuousActions[++i]);
            bpDict[shinR].SetJointStrength(continuousActions[++i]);
            bpDict[footR].SetJointStrength(continuousActions[++i]);
            bpDict[armL].SetJointStrength(continuousActions[++i]);
            bpDict[forearmL].SetJointStrength(continuousActions[++i]);
            bpDict[armR].SetJointStrength(continuousActions[++i]);
            bpDict[forearmR].SetJointStrength(continuousActions[++i]);

            if (GetCumulativeReward() < m_RewardThreshold)
            {
                EndEpisode();
            }
        }

        public Vector3 GetAvgVelocity()
        {
            Vector3 velSum = Vector3.zero;

            int numOfRb = 0;
            foreach (var item in m_JdController.bodyPartsList)
            {
                numOfRb++;
                velSum += item.rb.velocity;
            }

            var avgVel = velSum / numOfRb;
            m_AvgVelocity = avgVel;

            return m_AvgVelocity;
        }

        public Vector3 GetAvgPosition()
        {
            Vector3 posSum = Vector3.zero;
            int numOfRb = 0;

            foreach (var item in m_JdController.bodyPartsList)
            {
                numOfRb++;
                posSum += item.rb.position;
            }

            var avgPos = posSum / numOfRb;
            m_AvgPosition = avgPos;

            return m_AvgPosition;
        }

        public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
        {
            var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, TargetWalkingSpeed);

            if (TargetWalkingSpeed == 0)
            {
                TargetWalkingSpeed = 0.01f;
            }

            return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / TargetWalkingSpeed, 2), 2);
        }
    }
}