using UnityEngine;
using Unity.MLAgents;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// This class contains logic for locomotion agents with joints which might make contact with a tagged gameObject.
    /// By attaching this as a component to those joints, their contact with the gameObject can be used as either
    /// an observation for that agent, and/or a means of reward/penalty if the agent makes contact;
    /// </summary>
    [DisallowMultipleComponent]
    public class ObjectContact : MonoBehaviour
    {
        [HideInInspector] public Agent agent;

        public float targetReward = 1;

        // Penalty amount (ex: -1)
        public float groundContactPenalty = -1;
        public float wallContactPenalty = -1;
        public int m_RewardScaleFactor = 3;

        //Contact with the gameObject for observation
        public bool touchingGround;
        public bool touchingWall;
        public bool touchingTarget;
        public bool touchingAgent;
        public bool touchingObstacle;
        public bool touchingFood;
        public bool touchingPoison;
        public bool touchingEnvironment;
        public bool touchingFocus;
        public bool touchingObjective;
        public bool touchingFriend;
        public bool touchingEnemy;

        //Check tags for the gameObject
        const string k_Ground = "ground";
        const string k_Wall = "wall";
        const string k_Target = "target";
        const string k_Agent = "agent";
        const string k_Obstacle = "obstacle";
        const string k_Food = "food";
        const string k_Poison = "poison";
        const string k_Environment = "environment";
        const string k_Focus = "focus";
        const string k_Objective = "objective";
        const string k_Friend = "friend";
        const string k_Enemy = "enemy";

        void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(k_Ground))
            {
                touchingGround = true;
                agent.AddReward(groundContactPenalty);
            }
            if (col.transform.CompareTag(k_Wall))
            {
                touchingWall = true;
                agent.AddReward(wallContactPenalty);
            }
            if (col.transform.CompareTag(k_Target))
            {
                touchingTarget = true;
                agent.AddReward(targetReward);
            }
            if (col.transform.CompareTag(k_Agent))
            {
                touchingAgent = true;
            }
            if (col.transform.CompareTag(k_Obstacle))
            {
                touchingObstacle = true;
            }
            if (col.transform.CompareTag(k_Food))
            {
                touchingFood = true;
            }
            if (col.transform.CompareTag(k_Poison))
            {
                touchingPoison = true;
            }
            if (col.transform.CompareTag(k_Environment))
            {
                touchingEnvironment = true;
            }
            if (col.transform.CompareTag(k_Focus))
            {
                touchingFocus = true;
            }
            if (col.transform.CompareTag(k_Objective))
            {
                touchingObjective = true;
            }
            if (col.transform.CompareTag(k_Friend))
            {
                touchingFriend = true;
            }
            if (col.transform.CompareTag(k_Enemy))
            {
                touchingEnemy = true;
            }
        }

        void OnCollisionStay(Collision col)
        {
            if (col.transform.CompareTag(k_Ground))
            {
                agent.AddReward(groundContactPenalty * m_RewardScaleFactor * Time.fixedDeltaTime);
            }

            if (col.transform.CompareTag(k_Wall))
            {
                agent.AddReward(wallContactPenalty * m_RewardScaleFactor * Time.fixedDeltaTime);
            }
        }

        void OnCollisionExit(Collision col)
        {
            if (col.transform.CompareTag(k_Ground))
            {
                touchingGround = false;
            }
            if (col.transform.CompareTag(k_Wall))
            {
                touchingWall = false;
            }
            if (col.transform.CompareTag(k_Target))
            {
                touchingTarget = false;
            }
            if (col.transform.CompareTag(k_Agent))
            {
                touchingAgent = false;
            }
            if (col.transform.CompareTag(k_Obstacle))
            {
                touchingObstacle = false;
            }
            if (col.transform.CompareTag(k_Food))
            {
                touchingFood = false;
            }
            if (col.transform.CompareTag(k_Poison))
            {
                touchingPoison = false;
            }
            if (col.transform.CompareTag(k_Environment))
            {
                touchingEnvironment = false;
            }
            if (col.transform.CompareTag(k_Focus))
            {
                touchingFocus = false;
            }
            if (col.transform.CompareTag(k_Objective))
            {
                touchingObjective = false;
            }
            if (col.transform.CompareTag(k_Friend))
            {
                touchingFriend = false;
            }
            if (col.transform.CompareTag(k_Enemy))
            {
                touchingEnemy = false;
            }
        }
    }
}
