using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

namespace Gamekit3D
{
//this assure it's runned before any behaviour that may use it, as the animator need to be fecthed
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        public bool interpolateTurning = false;
        public bool applyAnimationRotation = false;

        public Animator animator { get { return m_Animator; } }
        public Vector3 ExternalForce { get { return externalForce; } }
        public NavMeshAgent Agent { get { return agent; } }
        public bool FollowAgent { get { return followAgent; } }
        public bool Grounded { get { return grounded; } }

        protected NavMeshAgent agent;
        protected bool followAgent;
        protected Animator m_Animator;
        protected bool m_UnderExternalForce;
        protected bool externalForceAddGravity = true;
        protected Vector3 externalForce;
        protected bool grounded;

        protected Rigidbody m_Rigidbody;



        const float k_GroundedRayDistance = .8f;

        void OnEnable()
        {

            agent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();
            m_Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

            agent.updatePosition = false;

            m_Rigidbody = GetComponentInChildren<Rigidbody>();
            if (m_Rigidbody == null)
                m_Rigidbody = gameObject.AddComponent<Rigidbody>();

            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            followAgent = true;
        }

        private void FixedUpdate()
        {
            animator.speed = PlayerInput.Instance != null && PlayerInput.Instance.HaveControl() ? 1.0f : 0.0f;

            CheckGrounded();

            if (m_UnderExternalForce)
                ForceMovement();
        }

        void CheckGrounded()
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + Vector3.up * k_GroundedRayDistance * 0.5f, -Vector3.up);
            grounded = Physics.Raycast(ray, out hit, k_GroundedRayDistance, Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
        }

        void ForceMovement()
        {
            if(externalForceAddGravity)
                externalForce += Physics.gravity * Time.deltaTime;

            RaycastHit hit;
            Vector3 movement = externalForce * Time.deltaTime;
            if (!m_Rigidbody.SweepTest(movement.normalized, out hit, movement.sqrMagnitude))
            {
                m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
            }

            agent.Warp(m_Rigidbody.position);
        }

        private void OnAnimatorMove()
        {
            if (m_UnderExternalForce)
                return;

            if (followAgent)
            {
                agent.speed = (m_Animator.deltaPosition / Time.deltaTime).magnitude;
                transform.position = agent.nextPosition;
            }
            else
            {
                RaycastHit hit;
                if (!m_Rigidbody.SweepTest(m_Animator.deltaPosition.normalized, out hit,
                    m_Animator.deltaPosition.sqrMagnitude))
                {
                    m_Rigidbody.MovePosition(m_Rigidbody.position + m_Animator.deltaPosition);
                }
            }

            if (applyAnimationRotation)
            {
                transform.forward = m_Animator.deltaRotation * transform.forward;
            }
        }

        // used to disable position being set by the navmesh agent, for case where we want the animation to move the enemy instead (e.g. Chomper attack)
        public void SetFollowAgent(bool follow)
        {
            if (!follow && agent.enabled)
            {
                agent.ResetPath();
            }
            else if(follow && !agent.enabled)
            {
                agent.Warp(transform.position);
            }

            followAgent = follow;
            agent.enabled = follow;
        }

        public void SetAgentTarget(Vector3 position)
        {
            agent.SetDestination(position);
        }

        public void AddForce(Vector3 force, bool useGravity = true)
        {
            if (agent.enabled)
                agent.ResetPath();

            externalForce = force;
            agent.enabled = false;
            m_UnderExternalForce = true;
            externalForceAddGravity = useGravity;
        }

        public void ClearForce()
        {
            m_UnderExternalForce = false;
            agent.enabled = true;
        }

        public void SetForward(Vector3 forward)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);

            if (interpolateTurning)
            {
                targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                    agent.angularSpeed * Time.deltaTime);
            }

            transform.rotation = targetRotation;
        }

        public bool SetTarget(Vector3 position)
        {
            NavMeshHit hit;
            bool positionFound = NavMesh.SamplePosition(position, out hit, 500, NavMesh.AllAreas);
            if (positionFound)
            {

            
            }

            return agent.SetDestination(positionFound? hit.position:position);
        }

        void CheckValidPath()
        {
            if (!agent.enabled || !agent.isOnNavMesh) return;

            if (!agent.pathPending && agent.path.status == NavMeshPathStatus.PathInvalid && Time.frameCount % 2 == 0)
            {
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(PlayerController.Instance.transform.position, path);
                agent.SetPath(path);
            }
        }
        private void Update()
        {
            CheckValidPath();
        }
    }
}