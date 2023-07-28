using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D;

public class RangeWeapon : MonoBehaviour
{
    public Vector3 muzzleOffset;
    public Projectile projectile;
    public Projectile Projectile
    {
        get { return projectile; }
    }

    protected Projectile loadedProjectile = null;
    protected ObjectPooler<Projectile> projectilePool;

    private void Start()
    {
        projectilePool = new ObjectPooler<Projectile>();
        projectilePool.Initialize(20, projectile);
    }

    public void Attack(Vector3 target)
    {
        AttackProjectile(target);
    }

    void AttackProjectile(Vector3 target)
    {
        if (loadedProjectile == null) LoadProjectile();

        loadedProjectile.transform.SetParent(null, true);
        loadedProjectile.Shot(target, this);
        loadedProjectile = null; //once shot, we don't own the projectile anymore, it does it's own life.
    }

    public void LoadProjectile()
    {
        if (loadedProjectile != null)
            return;

        loadedProjectile = projectilePool.GetNew();
        loadedProjectile.transform.SetParent(transform, false);
        loadedProjectile.transform.localPosition = muzzleOffset;
        loadedProjectile.transform.localRotation = Quaternion.identity;
    }
}
