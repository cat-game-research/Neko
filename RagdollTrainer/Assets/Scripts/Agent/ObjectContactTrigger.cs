using UnityEngine;
using Unity.MLAgents;
using Unity.Sentis.Layers;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// This class contains logic for locomotion agents with joints which might make contact with a tagged gameObject.
    /// By attaching this as a component to those joints, their contact with the gameObject can be used as either
    /// an observation for that agent, and/or a means of reward/penalty if the agent makes contact;
    /// </summary>
    [DisallowMultipleComponent]
    public class ObjectContactTrigger : MonoBehaviour
    {
        [HideInInspector] public Agent agent;

        public float targetReward = 1;

        // Penalty amount (ex: -1)
        public float groundContactPenalty = -1;
        public float wallContactPenalty = -1;

        //Contact with the gameObject for observation
        public bool touchingGround;
        public bool touchingWall;
        public bool touchingTarget;

        //Check tags for the gameObject
        const string k_Ground = "ground"; // Tag of ground object.
        const string k_Wall = "wall";
        const string k_Target = "target";

        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag(k_Ground))
            {
                touchingGround = true;
            }

            if (col.CompareTag(k_Wall))
            {
                touchingWall = true;
            }

            if (col.CompareTag(k_Target))
            {
                touchingTarget = true;
                agent.AddReward(targetReward);
            }
        }


        private void OnTriggerStay(Collider col)
        {
            if (col.CompareTag(k_Ground))
            {
                agent.AddReward(groundContactPenalty);
            }

            if (col.CompareTag(k_Wall))
            {
                agent.AddReward(wallContactPenalty);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.CompareTag(k_Ground))
            {
                touchingGround = false;
            }

            if (col.CompareTag(k_Wall))
            {
                touchingWall = false;
            }

            if (col.CompareTag(k_Target))
            {
                touchingTarget = false;
            }
        }
    }
}
