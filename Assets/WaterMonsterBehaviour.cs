using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D.Message;
using Gamekit3D;
using UnityEditor;

namespace Gamekit3D
{
    [DefaultExecutionOrder(100)]
    public class WaterMonsterBehaviour : MonoBehaviour, IMessageReceiver
    {
        public TargetScanner playerScanner;
        public RangeWeapon rangeWeapon;
        public EnemyController monsterController;
        public PlayerController player = null;
        public float hitForce;
        public float finalHitForce;
        protected Damageable damageable;

        protected Vector3 rememberedTargetPosition;


        public float attackDistance;
        protected float timerSinceLostTarget = 0.0f;
        float timeToStopPursuit;
        public TargetDistributor.TargetFollower FollowerData { get { return followerInstance; } }
        protected TargetDistributor.TargetFollower followerInstance = null;
        private void Update()
        {


        }
        private void OnEnable()
        {
            damageable = GetComponent<Damageable>();
            damageable.onDamageMessageReceivers.Add(this);
            monsterController = GetComponentInChildren<EnemyController>();
            monsterController.animator.Play("Idle", 0, Random.value);

            StateMachine<WaterMonsterBehaviour>.Initialise(monsterController.animator, this);
        }

        public void OnReceiveMessage(Message.MessageType type, object sender, object msg)
        {
            switch (type)
            {
                case Message.MessageType.DEAD:
                    Death((Damageable.DamageMessage)msg);
                    break;
                case Message.MessageType.DAMAGED:
                    ApplyDamage((Damageable.DamageMessage)msg);
                    break;
                default:
                    break;
            }
        }

        public void Death(Damageable.DamageMessage msg)
        {
            Vector3 pushForce = transform.position - msg.damageSource;

            pushForce.y = 0;

            transform.forward = -pushForce.normalized;
            monsterController.AddForce(pushForce.normalized * finalHitForce - Physics.gravity * 0.6f);

            monsterController.animator.SetTrigger("Hit");
            monsterController.animator.SetTrigger("Thrown");
            StartCoroutine(DestroySelf());
        }

        IEnumerator DestroySelf()
        {
            yield return new WaitForSeconds(3f);
            //if (waterRing.activeSelf)
            //    waterRing.GetComponent<WaterRing>().Reset();
            Destroy(this.gameObject);
        }

        public void ApplyDamage(Damageable.DamageMessage msg)
        {
            if (msg.damager.name == "FistWeapon")
                CameraShake.Shake(0.06f, 0.1f);

            float verticalDot = Vector3.Dot(Vector3.up, msg.direction);
            float horizontalDot = Vector3.Dot(transform.right, msg.direction);

            Vector3 pushForce = transform.position - msg.damageSource;

            pushForce.y = 0;

            transform.forward = -pushForce.normalized;
            monsterController.AddForce(pushForce.normalized * hitForce, false);

            monsterController.animator.SetFloat("VerticalHitDot", verticalDot);
            monsterController.animator.SetFloat("HorizontalHitDot", horizontalDot);

            monsterController.animator.SetTrigger("Hit");
        }

        public void Shoot()
        {
            rangeWeapon.Attack(rememberedTargetPosition);
        }


        public void TriggerAttack()
        {
            monsterController.animator.SetTrigger("Attack");
        }
        public void RememberTargetPosition()
        {
            if (player == null)
                return;

            rememberedTargetPosition = player.transform.position;
        }
        public void FindTarget()
        {
            //we ignore height difference if the target was already seen
            PlayerController target = playerScanner.Detect(transform, player == null);


            if (player == null)
            {
                //we just saw the player for the first time, pick an empty spot to target around them
                if (target != null)
                {
                    //monsterController.animator.SetTrigger("Spotted");
                    player = target;
                    TargetDistributor distributor = target.GetComponentInChildren<TargetDistributor>();
                    if (distributor != null)
                        followerInstance = distributor.RegisterNewFollower();
                }
            }
            else
            {
                if (target == null)
                {
                    timerSinceLostTarget += Time.deltaTime;

                    if (timerSinceLostTarget >= timeToStopPursuit)
                    {
                        Vector3 toTarget = player.transform.position - transform.position;

                        if (toTarget.sqrMagnitude > playerScanner.detectionRadius * playerScanner.detectionRadius)
                        {
                            if (followerInstance != null)
                                followerInstance.distributor.UnregisterFollower(followerInstance);

                            //the target move out of range, reset the target
                            player = null;
                        }
                    }
                }
                else
                {
                    if (target != player)
                    {
                        if (followerInstance != null)
                            followerInstance.distributor.UnregisterFollower(followerInstance);

                        player = target;

                        TargetDistributor distributor = target.GetComponentInChildren<TargetDistributor>();
                        if (distributor != null)
                            followerInstance = distributor.RegisterNewFollower();
                    }

                    timerSinceLostTarget = 0.0f;
                }

                monsterController.animator.SetBool("HaveTarget", player != null);
            }
        }

        public void Spotted()
        {

        }
        public void StopPursuit()
        {
            if (followerInstance != null)
            {
                followerInstance.requireSlot = false;
            }

            monsterController.animator.SetBool("Move", false);
        }

        public void RequestTargetPosition()
        {
            Vector3 fromTarget = transform.position - player.transform.position;
            fromTarget.y = 0;

            followerInstance.requiredPoint = player.transform.position + fromTarget.normalized * attackDistance * 0.9f;
        }

        public void StartPursuit()
        {
            if (followerInstance != null)
            {
                followerInstance.requireSlot = true;
                RequestTargetPosition();
            }

            monsterController.animator.SetBool("Move", true);
        }

        private void OnDisable()
        {
            if (followerInstance != null)
                followerInstance.distributor.UnregisterFollower(followerInstance);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WaterMonsterBehaviour))]
    public class SpitterBehaviourEditor : Editor
    {
        WaterMonsterBehaviour m_Target;

        void OnEnable()
        {
            m_Target = target as WaterMonsterBehaviour;
        }

        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();
        }
    }


#endif
}