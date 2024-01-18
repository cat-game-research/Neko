using JKress.AITrainer;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using Unity.Sentis.Layers;
using UnityEngine;

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
    [SerializeField] Transform m_FocusT;

    [Header("Focus Sphere Range")]
    [Range(1f, 100f)] public float m_MaxDistance = 20f;

    [Header("Focus Position")]
    Vector3 m_FocusPosition = Vector3.zero;
    [Range(0.1f, 10f)][SerializeField] float m_PositionScale = 1f;

    [Header("Acceleration")]
    [Range(0.1f, 10f)][SerializeField] float m_AccelerationScale = 1f;
    [Range(0.1f, 4f)] public float m_TargetWalkingSpeed = 2f;

    /// <summary>
    /// called before anything else is run in our training excercise
    /// </summary>
    public override void Initialize()
    {
        m_FocusSphere = GetComponentInChildren<FocusSphereController>();
        transform.SetPositionAndRotation(m_Head.position, m_Hips.rotation);
        m_FocusPosition = m_FocusSphere.UpdatePosition(transform.position);
        m_WalkerAgent.UpdateTargetWalkingSpeed(m_TargetWalkingSpeed);
    }

    public override void OnEpisodeBegin()
    {
        //reset our positions and walking speeds
        transform.SetPositionAndRotation(m_Head.position, m_Hips.rotation);
        m_FocusPosition = m_FocusSphere.UpdatePosition(m_Head.position);
        m_WalkerAgent.UpdateTargetWalkingSpeed(m_TargetWalkingSpeed);
    }

    void FixedUpdate()
    {
        transform.position = m_Head.position;
        transform.rotation = m_Hips.rotation;

        //TODO handle our reward mechanisms
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Add all root agents observations to model
        m_WalkerAgent.CollectObservations(sensor);

        //Motion perception systems
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.localRotation);

        //Focus sphere observations
        sensor.AddObservation(m_MaxDistance);
        sensor.AddObservation(m_FocusPosition);
        sensor.AddObservation(m_FocusT.localPosition);
        sensor.AddObservation(m_TargetWalkingSpeed);

        //Relative position and speed of focus sphere
        sensor.AddObservation(Vector3.Distance(m_WalkerAgent.m_AvgPosition, m_FocusPosition));
        sensor.AddObservation(Vector3.Distance(m_FocusT.position, m_FocusPosition));
        sensor.AddObservation(m_TargetWalkingSpeed - m_WalkerAgent.m_AvgVelocity.magnitude);
        sensor.AddObservation(transform.InverseTransformDirection(m_FocusPosition));
        sensor.AddObservation(transform.InverseTransformDirection(m_WalkerAgent.m_AvgPosition));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        var i = -1;

        //set local position of focus sphere.
        Vector3 position = new Vector3(continuousActions[++i], continuousActions[++i], continuousActions[++i]);
        position *= m_PositionScale;
        m_FocusPosition += position;
        m_FocusPosition = Vector3.ClampMagnitude(m_FocusPosition, m_MaxDistance);
        m_FocusPosition = m_FocusSphere.UpdatePosition(m_FocusPosition);

        //set target walking speed of agent
        m_TargetWalkingSpeed = m_WalkerAgent.TargetWalkingSpeed + continuousActions[++i] * m_AccelerationScale;
        m_WalkerAgent.UpdateTargetWalkingSpeed(m_TargetWalkingSpeed);
    }
}