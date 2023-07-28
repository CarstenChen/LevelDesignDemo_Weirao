using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Gamekit3D;

public class MeleeWeapon : MonoBehaviour
{
    [System.Serializable]
    public class AttackPoint
    {
        public float radius;
        public Vector3 offset;
        public Transform rootTransform;

        [System.NonSerialized] public List<Vector3> previousPos = new List<Vector3>();
    }

    public int damage = 1;
    public LayerMask targetLayer;
    public ParticleSystem hitParticle;
    public TrailEffects[] trailEffects = new TrailEffects[0];
    public AttackPoint[] attackPoints = new AttackPoint[0];

    protected GameObject weaponOwner;


    protected Vector3[] previousPos;
    protected static RaycastHit[] hitRaycastHitCache = new RaycastHit[32];

    protected bool isAttacking = false;

    const int PARTICLE_COUNT = 10;
    protected ParticleSystem[] pregeneratedParticles = new ParticleSystem[PARTICLE_COUNT];
    protected int particleIndex = 0;

    private void Awake()
    {
        if (hitParticle != null)
        {
            for (int i = 0; i< PARTICLE_COUNT; i++)
            {
                pregeneratedParticles[i] = Instantiate(hitParticle);
                pregeneratedParticles[i].Stop();
            }
        }

    }
    public void SetOwner(GameObject owner)
    {
        weaponOwner = owner;
    }

    public void BeginAttack()
    {
        isAttacking = true;
        previousPos = new Vector3[attackPoints.Length];

        for(int i = 0; i < attackPoints.Length; i++)
        {
            Vector3 pos = attackPoints[i].rootTransform.position + attackPoints[i].rootTransform.TransformVector(attackPoints[i].offset);

            previousPos[i] = pos;

            attackPoints[i].previousPos.Clear();
            attackPoints[i].previousPos.Add(previousPos[i]);
        }
    }

    public void EndAttack()
    {
        isAttacking = false;

        for (int i = 0; i < attackPoints.Length; i++)
        {
            attackPoints[i].previousPos.Clear();
        }
    }

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            for (int i = 0; i < attackPoints.Length; i++)
            {
                AttackPoint ap = attackPoints[i];

                Vector3 pos = ap.rootTransform.position + ap.rootTransform.TransformVector(ap.offset);

                Vector3 hitDir = pos - previousPos[i];

                //确保再近的范围也能有射线打出
                if (hitDir.magnitude < 0.0001f)
                {
                    hitDir = Vector3.forward * 0.0001f;
                }

                Ray hitRay = new Ray(pos, hitDir);

                int contactNum = Physics.SphereCastNonAlloc(hitRay, ap.radius, hitRaycastHitCache, hitDir.magnitude, ~0, QueryTriggerInteraction.Ignore);

                for(int j = 0; j < contactNum; j++)
                {
                    Collider collider = hitRaycastHitCache[j].collider;

                    if(collider != null)
                    {
                        CheckDamage(collider, ap);
                    }
                }

                //每一帧都记录先前的位置给这个点
                previousPos[i] = pos;
                ap.previousPos.Add(previousPos[i]);
            }
        }
    }

    void CheckDamage(Collider collider, AttackPoint point)
    {
        Damageable dd = collider.GetComponent<Damageable>();

        if (dd == null) return;

        if (dd == weaponOwner) return;

        if((targetLayer.value&(1<<collider.gameObject.layer)) == 0) return;

        Damageable.DamageMessage data = new Damageable.DamageMessage();
        data.damage = damage;
        data.damager = this;
        data.damageSource = weaponOwner.transform.position;
        data.stopCamera = false;

        dd.ApplyDamage(data);



        PlayHitParticleEffect(point.rootTransform.position);
    }

    void PlayHitParticleEffect(Vector3 position)
    {
        if (hitParticle != null)
        {
            pregeneratedParticles[particleIndex].transform.position = position;
            pregeneratedParticles[particleIndex].time = 0;
            pregeneratedParticles[particleIndex].Play();

            particleIndex = (particleIndex + 1) % PARTICLE_COUNT;

        }

    }

}
