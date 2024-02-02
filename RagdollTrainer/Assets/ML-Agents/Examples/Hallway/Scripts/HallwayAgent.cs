using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class HallwayAgent : Agent
{
    public GameObject ground;
    public GameObject area;
    public GameObject symbolOGoal;
    public GameObject symbolXGoal;
    public GameObject symbolO;
    public GameObject symbolX;
    public float m_RewardAmount = 1f;
    public float m_PenalityAmount = -0.5f;
    public float m_StepPenalityAmount = -4f;
    public float m_WallPenalityAmount = -4f;
    public float m_MinRewardThreshold = -0.25f;
    Rigidbody m_AgentRb;
    int m_Selection;
    StatsRecorder m_statsRecorder;

    Vector3 _Direction = Vector3.zero;
    Vector3 _Rotation = Vector3.zero;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_statsRecorder = Academy.Instance.StatsRecorder;
        m_statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Goal/Wrong", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Goal/Total", 0, StatAggregationMethod.Sum);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(StepCount / (float)MaxStep);
        sensor.AddObservation(_Direction);
        sensor.AddObservation(_Rotation);
        sensor.AddObservation(m_AgentRb.velocity);
        sensor.AddObservation(m_AgentRb.angularVelocity);
        sensor.AddObservation(GetCumulativeReward());
        sensor.AddObservation(GetCumulativeReward() / StepCount);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        _Direction = Vector3.zero;
        _Rotation = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                _Direction = transform.forward * 1f;
                break;
            case 2:
                _Direction = transform.forward * -1f;
                break;
            case 3:
                _Rotation = transform.up * 1f;
                break;
            case 4:
                _Rotation = transform.up * -1f;
                break;
        }
        transform.Rotate(_Rotation, Time.deltaTime * 150f);
        m_AgentRb.AddForce(_Direction * 1.5f, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {
        if (GetCumulativeReward() < m_MinRewardThreshold)
        {
            EndEpisode();
        }

        AddReward(m_StepPenalityAmount / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("wall"))
        {
            AddReward(m_WallPenalityAmount / MaxStep);
        }
        if (col.gameObject.CompareTag("symbol_O_Goal") || col.gameObject.CompareTag("symbol_X_Goal"))
        {
            if ((m_Selection == 0 && col.gameObject.CompareTag("symbol_O_Goal")) ||
                (m_Selection == 1 && col.gameObject.CompareTag("symbol_X_Goal")))
            {
                SetReward(m_RewardAmount);
                m_statsRecorder.Add("Goal/Correct", 1, StatAggregationMethod.Sum);
                m_statsRecorder.Add("Goal/Total", 1, StatAggregationMethod.Sum);
            }
            else
            {
                SetReward(m_PenalityAmount);
                m_statsRecorder.Add("Goal/Wrong", 1, StatAggregationMethod.Sum);
                m_statsRecorder.Add("Goal/Total", 1, StatAggregationMethod.Sum);
            }
            EndEpisode();
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("wall"))
        {
            AddReward(m_WallPenalityAmount / MaxStep);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public override void OnEpisodeBegin()
    {
        var agentOffset = -15f;
        var blockOffset = 0f;
        m_Selection = Random.Range(0, 2);
        if (m_Selection == 0)
        {
            symbolO.transform.position =
                new Vector3(0f + Random.Range(-3f, 3f), 2f, blockOffset + Random.Range(-5f, 5f))
                + ground.transform.position;
            symbolX.transform.position =
                new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                + ground.transform.position;
        }
        else
        {
            symbolO.transform.position =
                new Vector3(0f, -1000f, blockOffset + Random.Range(-5f, 5f))
                + ground.transform.position;
            symbolX.transform.position =
                new Vector3(0f, 2f, blockOffset + Random.Range(-5f, 5f))
                + ground.transform.position;
        }

        transform.position = new Vector3(0f + Random.Range(-3f, 3f),
            1f, agentOffset + Random.Range(-5f, 5f))
            + ground.transform.position;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;

        var goalPos = Random.Range(0, 2);
        if (goalPos == 0)
        {
            symbolOGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
            symbolXGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
        }
        else
        {
            symbolXGoal.transform.position = new Vector3(7f, 0.5f, 22.29f) + area.transform.position;
            symbolOGoal.transform.position = new Vector3(-7f, 0.5f, 22.29f) + area.transform.position;
        }
        m_statsRecorder.Add("Goal/Correct", 0, StatAggregationMethod.Sum);
        m_statsRecorder.Add("Goal/Wrong", 0, StatAggregationMethod.Sum);
    }
}
