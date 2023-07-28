using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D.Message;
namespace Gamekit3D
{
    [DefaultExecutionOrder(100)]
    public class WoodMonsterBehaviour : MonoBehaviour, IMessageReceiver
    {
        public TargetScanner playerScanner;
        public MeleeWeapon meleeWeapon;
        public EnemyController monsterController;
        public PlayerController player = null;
        public float hitForce;
        public float finalHitForce;
        protected Damageable damageable;

        protected Vector3 rememberedTargetPosition;
        public GameObject waterRing;

        public float attackDistance = 3f;
        protected float timerSinceLostTarget = 0.0f;
        float timeToStopPursuit;
        public TargetDistributor.TargetFollower FollowerData { get { return followerInstance; } }
        protected TargetDistributor.TargetFollower followerInstance = null;

        public Vector3 originalPosition { get; protected set; }

        private void Update()
        {
            monsterController.animator.SetBool("PlayerSufferWoodFrame", waterRing.activeSelf);
        }
        protected void OnEnable()
        {
            damageable = GetComponent<Damageable>();
            damageable.onDamageMessageReceivers.Add(this);

            monsterController = GetComponentInChildren<EnemyController>();

            originalPosition = transform.position;

            meleeWeapon.SetOwner(gameObject);

            monsterController.animator.Play("Idle", 0, Random.value);

            StateMachine<WoodMonsterBehaviour>.Initialise(monsterController.animator, this);
        }

        protected void OnDisable()
        {
            if (followerInstance != null)
                followerInstance.distributor.UnregisterFollower(followerInstance);

        }
        public void Spotted()
        {

        }

        private void FixedUpdate()
        {
            monsterController.animator.SetBool("Grounded", monsterController.Grounded);
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
                    monsterController.animator.SetTrigger("Spotted");
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

        public void StartPursuit()
        {
            if (followerInstance != null)
            {
                followerInstance.requireSlot = true;
                RequestTargetPosition();
            }

            monsterController.animator.SetBool("Move", true);
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

        public void TriggerAttack()
        {
            monsterController.animator.SetTrigger("Attack");
        }

        public void AttackBegin()
        {
            meleeWeapon.BeginAttack();
        }

        public void AttackEnd()
        {
            meleeWeapon.EndAttack();
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
        public void CreateWoodBlock()
        {
            waterRing.SetActive(true);
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
            if (waterRing.activeSelf)
                waterRing.GetComponent<WaterRing>().Reset();
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

    }

    

}

