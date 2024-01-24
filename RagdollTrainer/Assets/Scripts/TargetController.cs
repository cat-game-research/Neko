using UnityEngine;
using Random = UnityEngine.Random;

namespace Unity.MLAgentsExamples
{
    public class TargetController : MonoBehaviour
    {
        const string k_Agent = "agent";

        [SerializeField] LayerMask m_LayerMask;
        [SerializeField] float m_RayDown = 10;

        [Header("Target Spawning RNG")]
        [Tooltip("The range of the x-coordinate for the target sphere's spawn position.")]
        public Vector2 m_SpawnX = new Vector2(-9, 9);
        [Tooltip("The range of the y-coordinate for the target sphere's spawn position. The actual y-coordinate will be adjusted based on the raycast hit point.")]
        public Vector2 m_SpawnY = new Vector2(0.5f, 1.5f);
        [Tooltip("The range of the z-coordinate for the target sphere's spawn position.")]
        public Vector2 m_SpawnZ = new Vector2(-9, 9);

        [Header("Target Respawn Options")]
        public bool m_RespawnIfTouched = true;
        public bool m_LocalRespawn = false;
        public bool m_SpawnOnlyOnGround = true;

        [Header("Target Respawn Timer")]
        [Tooltip("This is the default timer used to make the target respawn. Randomize Respawn Time will override this value. Be careful when setting this value to low or high.")]
        [Range(1f, 60f)][SerializeField] float m_RespawnTime = 30f;
        [Tooltip("This is the minimum time which the random respawn time will be.")]
        [Range(1f, 60f)][SerializeField] float m_MinRespawnTime = 30f;
        [Tooltip("This is the maximum time which the random respawn time will be.")]
        [Range(1f, 60f)][SerializeField] float m_MaxRespawnTime = 60f;

        [Tooltip("This will use the min and max respawn times to generate a new time between that.")]
        public bool m_RandomizeRespawnTime = false;

        float _Timer;

        private void Start()
        {
            _Timer = m_RandomizeRespawnTime ? Random.Range(m_MinRespawnTime, m_MaxRespawnTime) : m_RespawnTime;
        }

        private void Update()
        {
            _Timer -= Time.deltaTime;

            if (Mathf.Approximately(_Timer, 0f) || _Timer < 0f)
            {
                ResetTarget();
            }
        }

        void FixedUpdate()
        {
            if (transform.localPosition.y < -5)
            {
                Debug.Log($"{transform.name} Off Platform");
                MoveTargetToRandomPosition();
            }
        }

        private void ResetTarget()
        {
            MoveTargetToRandomPosition();
            _Timer = m_RandomizeRespawnTime ? Random.Range(m_RespawnTime / 10, m_RespawnTime) : m_RespawnTime;
        }

        public void MoveTargetToRandomPosition()
        {
            Vector3 newTargetPos;
            Collider[] hitColliders;
            bool isOnGround = false;

            do
            {
                if (m_LocalRespawn)
                {
                    newTargetPos = new Vector3(Random.Range(transform.position.x + m_SpawnX.x, transform.position.x + m_SpawnX.y), m_RayDown, Random.Range(transform.position.z + m_SpawnZ.x, transform.position.z + m_SpawnZ.y));
                }
                else
                {
                    newTargetPos = new Vector3(Random.Range(m_SpawnX.x, m_SpawnX.y), m_RayDown, Random.Range(m_SpawnZ.x, m_SpawnZ.y));
                }

                RaycastHit hit;
                if (Physics.Raycast(newTargetPos, Vector3.down, out hit, Mathf.Infinity, m_LayerMask))
                {
                    newTargetPos.y = hit.point.y + Random.Range(m_SpawnY.x, m_SpawnY.y);
                }

                if (m_SpawnOnlyOnGround)
                {
                    isOnGround = Physics.Raycast(newTargetPos, Vector3.down, out RaycastHit _, Mathf.Infinity, m_LayerMask) ||
                                 Physics.CheckSphere(newTargetPos, transform.localScale.x / 2, m_LayerMask);
                }

                hitColliders = Physics.OverlapSphere(newTargetPos, transform.localScale.x / 2);

                if (!isOnGround)
                {
                    continue;
                }

            } while (hitColliders.Length > 0);

            transform.localPosition = newTargetPos;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (col.transform.CompareTag(k_Agent))
            {
                if (m_RespawnIfTouched)
                {
                    ResetTarget();
                }
            }
        }
    }
}
